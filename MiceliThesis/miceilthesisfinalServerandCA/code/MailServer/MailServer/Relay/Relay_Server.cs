using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;

namespace LumiSoft.MailServer.Relay
{
    /// <summary>
    /// This class implements mail server relay server.
    /// </summary>
    public class Relay_Server : IDisposable
    {
        private VirtualServer       m_pVirtualServer            = null;
        private string              m_HostName                  = "";
        private int                 m_SessionIdleTimeout        = 180;
        private int                 m_MaxConnections            = 100;
        private int                 m_MaxConnsPerIP             = 10;
        private bool                m_UseSmartHost              = false;
        private string              m_SmartHost                 = "";
        private int                 m_SmartHostPort             = 25;
        private bool                m_SmartHostUseSSL           = false;
        private string              m_SmartHostUserName         = "";
        private string              m_SmartHostPassword         = "";
        private string              m_Dns1                      = "";
        private string              m_Dns2                      = "";
        private int                 m_RelayInterval             = 30;
        private int                 m_RelayRetryInterval        = 180;
        private int                 m_UndeliveredWarningAfter   = 5;
        private int                 m_UndeliveredAfter          = 60;
        private ServerReturnMessage m_UndeliveredWarningMessage = null;
        private ServerReturnMessage m_UndeliveredMessage        = null;
        private IPAddress           m_pSendingIP                = null;
        private bool                m_LogCommands               = false;
        private string              m_LogsPath                  = "";
        private bool                m_StoreUndelivered          = false;
        private bool                m_Running                   = false;
        private System.Timers.Timer m_pTimer                    = null;
        private List<Relay_Session> m_pSessions                 = null;
        private Queue<string>       m_pRelayQueue               = null;
        private Queue<string>       m_pRelayRetryQueue          = null;
        private DateTime            m_pLastRelayTime;
        private DateTime            m_pLastRelayRetryTime;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="virtualServer">Virtual server that owns this relay server.</param>
        public Relay_Server(VirtualServer virtualServer)
        {
            m_pVirtualServer = virtualServer;
        }

        #region method Dispose

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        #endregion


        #region Events Handling

        #region method m_pTimer_Elapsed

        private void m_pTimer_Elapsed(object sender,System.Timers.ElapsedEventArgs e)
        {
            CheckTimedoutSessions();
        }

        #endregion

        #endregion


        #region method Start

        /// <summary>
        /// Starts relay server.
        /// </summary>
        public void Start()
        {
            if(m_Running){
                return;
            }
            m_Running = true;

            m_pSessions = new List<Relay_Session>();

            // Create relay work queues
            m_pRelayQueue = new Queue<string>();
            m_pRelayRetryQueue = new Queue<string>();

            m_pLastRelayTime = DateTime.Now;
            m_pLastRelayRetryTime = DateTime.Now;

            // Start relay processing loop messages feeder
            Thread tr1 = new Thread(this.QueueFeeder);
            tr1.Start();
            // Start relay processing loop
            Thread tr2 = new Thread(this.ProcessQueue);
            tr2.Start();

            // Start session timeout checker timer
            m_pTimer = new System.Timers.Timer();
            m_pTimer.AutoReset = true;
            m_pTimer.Interval = 120000;
            m_pTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_pTimer_Elapsed);
            m_pTimer.Enabled = true;            
        }

        #endregion

        #region method Stop

        /// <summary>
        /// Stops relay server.
        /// </summary>
        public void Stop()
        {   
            if(m_pTimer != null){
                m_pTimer.Dispose();
                m_pTimer = null;
            }
            if(m_pRelayQueue != null){
                m_pRelayQueue = null;
            }
            if(m_pRelayRetryQueue != null){
                m_pRelayRetryQueue = null;
            }
            if(m_pSessions != null){
                m_pSessions = null;
            }
            m_Running = false;
        }

        #endregion


        #region method StoreRelayMessage

        /// <summary>
        /// Stores relay message.
        /// </summary>
        /// <param name="message">Message to store.</param>
        /// <param name="destinationHost">Destination host or IP where to send message. This value can be null, then DNS MX o A record is used to deliver message.</param>
        /// <param name="sender">Sender to report to destination server.</param>
        /// <param name="to">Destination server recipient.</param>
        public void StoreRelayMessage(Stream message,string destinationHost,string sender,string to)
        {
            // Create dummy file name
			string filename = Guid.NewGuid().ToString().Substring(0,22).Replace("-","_");
					
			string path = m_pVirtualServer.MailStorePath + "Relay";

			// Check if Directory exists, if not Create
			if(!Directory.Exists(path)){
				Directory.CreateDirectory(path);
			}

			//---- Write message data to file -------------------------------//
			using(FileStream fs = File.Create(API_Utlis.PathFix(path + "\\" + filename + ".eml"),(int)message.Length)){

				// Write internal relay info line at the beginning of messsage.
				// Note: This line is skipped when sending to destination host,
				// actual message begins from 2 line.
				// Header struct: 'RelayInfo:IsUndeliveredWarningSent<TAB>To<TAB>Sender<TAB>Date<TAB>ForwardHost\r\n'
				string internalServerHead = "RelayInfo:0\t" + to + "\t" + sender + "\t" + DateTime.Now.ToString("r",System.Globalization.DateTimeFormatInfo.InvariantInfo) + "\t" + destinationHost + "\r\n";
				byte[] sHead = System.Text.Encoding.Default.GetBytes(internalServerHead);
				fs.Write(sHead,0,sHead.Length);

                byte[] data = new byte[message.Length];
                message.Read(data,0,data.Length);
				fs.Write(data,0,data.Length);
			}
			//---------------------------------------------------------------//
        }

        #endregion

        #region method GetRelayConnections

        /// <summary>
        /// Gets how many relay connections there are for specified IP address.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <returns>Returns connection count.</returns>
        public int GetRelayConnections(IPAddress ip)
        {
            if(ip == null){
                throw new ArgumentNullException("ip");
            }

            lock(m_pSessions){
                int count = 0;
                foreach(Relay_Session session in m_pSessions){
                    if(session.RemoteEndPoint != null && session.RemoteEndPoint.Address.Equals(ip)){
                        count++;
                    }
                }
                return count;
            }
        }

        #endregion


        #region method QueueFeeder

        /// <summary>
        /// This loop processes new Relay,Retry messages and queues them up for delivery.
        /// </summary>
        private void QueueFeeder()
        {
            while(m_Running){
                try{                    
                    // Relay interval reached, add new messages to queue.
                    if(m_pLastRelayTime.AddSeconds(m_RelayInterval) < DateTime.Now){                    
                        string path = m_pVirtualServer.MailStorePath + "Relay";
                        if(Directory.Exists(path)){                            
                            lock(m_pRelayQueue){
                                string[] messages = Directory.GetFiles(path,"*.eml");                                
                                foreach(string message in messages){                                    
                                    if(!m_pRelayQueue.Contains(message) && !FileRelayedNow(message)){                                    
                                        m_pRelayQueue.Enqueue(message);
                                    }
                                }
                            }
                        }

                        m_pLastRelayTime = DateTime.Now;
                    }

                    // Relay Retry interval reached, add new messages to queue.
                    if(m_pLastRelayRetryTime.AddSeconds(m_RelayRetryInterval) < DateTime.Now){
                        string path = m_pVirtualServer.MailStorePath + "Retry";
                        if(Directory.Exists(path)){
                            lock(m_pRelayRetryQueue){
                                string[] messages = Directory.GetFiles(path,"*.eml");
                                foreach(string message in messages){
                                    if(!m_pRelayRetryQueue.Contains(message) && !FileRelayedNow(message)){
                                        m_pRelayRetryQueue.Enqueue(message);
                                    }
                                }
                            }
                        }

                        m_pLastRelayRetryTime = DateTime.Now;
                    }
                }
                catch(Exception x){
                    Error.DumpError(m_pVirtualServer.Name,x);
                }

                // Sleep, otherwise run loop takes 100% CPU.
                Thread.Sleep(10000);
            }            
        }

        #endregion

        #region method ProcessQueue

        /// <summary>
        /// This loop sends queued messages.
        /// </summary>
        private void ProcessQueue()
        {
            while(m_Running){
                try{
                    /* Get relay job from queue.
                       Relay queue is with priority 1.
                       Retry queue is with priority 2.
                      
                       This assures that retry attempt connections won't use all allowed connections.
                    */

                    // There are no relay jobs available now or maximum allowed relay connections reached.
                    // Just fall down 'while', sleep a little and try with next while loop.
                    if(!(m_pRelayQueue.Count == 0 && m_pRelayRetryQueue.Count == 0) && m_pSessions.Count < m_MaxConnections){                        
                        lock(this){
                            string file = "";
                            // Relay job available, get it, because it's with higher priority.
                            if(m_pRelayQueue.Count > 0){
                                file = m_pRelayQueue.Dequeue();
                            }
                            // Relay retry job.
                            else{
                                file = m_pRelayRetryQueue.Dequeue();
                            }

                            try{
                                bool relay_retry = true;
                                if(file.ToLower().IndexOf("retry") > -1){
                                    relay_retry = false;
                                }

							    Relay_Session session = new Relay_Session(this,file,relay_retry);
								AddSession(session);

								session.Start();							    
						    }
						    catch(IOException x){
							    // Just skip IO exceptions, they happen when incoming message storing hasn't 
							    // completed yet and relay engine tries to relay this message.
                                string dummy = x.Message;
						    }
                        }
                    }                                       
                }
                catch(Exception x){
                    Error.DumpError(m_pVirtualServer.Name,x);
                }

                // Sleep, otherwise run loop takes 100% CPU.
                Thread.Sleep(10);
            }
        }

        #endregion

        #region method AddSession

        /// <summary>
        /// Adds specified session to sessions collection.
        /// </summary>
        /// <param name="session">Session to add.</param>
        private void AddSession(Relay_Session session)
        {
            lock(m_pSessions){
                m_pSessions.Add(session);
            }
        }

        #endregion

        #region method RemoveSession

        /// <summary>
        /// Removes specified session from sessions collection.
        /// </summary>
        /// <param name="session">Session to remove.</param>
        internal void RemoveSession(Relay_Session session)
        {
            lock(m_pSessions){
                m_pSessions.Remove(session);
            }
        }

        #endregion

        #region method CheckTimedoutSessions

        /// <summary>
        /// Checks and kills all timedout sessions.
        /// </summary>
        private void CheckTimedoutSessions()
        {
            foreach(Relay_Session session in this.Sessions){
                try{
					// Close all connections what are timed out
					if(session.LastActivity.AddSeconds(m_SessionIdleTimeout) < DateTime.Now){
						session.Kill("Session idle timeout, server closing connection.");
					}
				}
				catch{
				}
            }
        }

        #endregion

                
        #region method FileRelayedNow

        /// <summary>
        /// Gets if specified file is being relayed currently.
        /// </summary>
        /// <param name="file">File to check.</param>
        /// <returns></returns>
        private bool FileRelayedNow(string file)
        {
            Relay_Session[] sessions = m_pSessions.ToArray();
            foreach(Relay_Session session in sessions){
                if(session.FileName == file){
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets this relay server owner virtual server.
        /// </summary>
        public VirtualServer VirtualServer
        {
            get{ return m_pVirtualServer; }
        }

        /// <summary>
        /// Gets active relay sessions. Note: This property is available only if Relay server is running !
        /// </summary>
        public Relay_Session[] Sessions
        {
            get{ return m_pSessions.ToArray(); }
        }

        /// <summary>
        /// Gets if server is running.
        /// </summary>
        public bool Running
        {
            get{ return m_Running; }
        }

        /// <summary>
        /// Gets or sets host name what is reported to connected SMTP servers.
        /// If "" then this local computer name is reported.
        /// </summary>
        public string HostName
        {
            get{ return m_HostName; }

            set{
                if(m_HostName != value){
                    m_HostName = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets how many seconds session may idle before it is closed automatically.
        /// </summary>
        public int SessionIdleTimeout
        {
            get{ return m_SessionIdleTimeout; }

            set{
                if(m_SessionIdleTimeout != value){
                    m_SessionIdleTimeout = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets how many conncurent relay session allowed.
        /// </summary>
        public int MaximumConnections
        {
            get{ return m_MaxConnections; }

            set{
                if(m_MaxConnections != value){
                    m_MaxConnections = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets how many conncurent relay session allowed to one destination IP(host). 
        /// Value 0 means unlimited.
        /// </summary>
        public int MaximumConnectionsPerIP
        {
            get{ return m_MaxConnsPerIP; }

            set{
                if(m_MaxConnsPerIP != value){
                    m_MaxConnsPerIP = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets if relay server uses smart host to send out messages.
        /// If false, then DNS is used to send messages out directly.
        /// </summary>
        public bool UseSmartHost
        {
            get{ return m_UseSmartHost; }

            set{
                if(m_UseSmartHost != value){
                    m_UseSmartHost = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets smart host name or IP to where send messages.
        /// </summary>
        public string SmartHost
        {
            get{ return m_SmartHost; }

            set{
                if(m_SmartHost != value){
                    m_SmartHost = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets smart host port.
        /// </summary>
        public int SmartHostPort
        {
            get{ return m_SmartHostPort; }

            set{
                if(m_SmartHostPort != value){
                    m_SmartHostPort = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets if SSL is used to connect to smart host.
        /// </summary>
        public bool SmartHostUseSSL
        {
            get{ return m_SmartHostUseSSL; }

            set{
                if(m_SmartHostUseSSL != value){
                    m_SmartHostUseSSL = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets smart host user name. Leave empty if no authentication required.
        /// </summary>
        public string SmartHostUserName
        {
            get{ return m_SmartHostUserName; }

            set{
                if(m_SmartHostUserName != value){
                    m_SmartHostUserName = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets smart host password. Leave empty if no password or authentication required.
        /// </summary>
        public string SmartHostPassword
        {
            get{ return m_SmartHostPassword; }

            set{
                if(m_SmartHostPassword != value){
                    m_SmartHostPassword = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets primary DNS server.
        /// </summary>
        public string Dns1
        {
            get{ return m_Dns1; }

            set{
                if(m_Dns1 != value){
                    m_Dns1 = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets secondary DNS server.
        /// </summary>
        public string Dns2
        {
            get{ return m_Dns2; }

            set{
                if(m_Dns2 != value){
                    m_Dns2 = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets relay interval seconds.
        /// </summary>
        public int RelayInterval
        {
            get{ return m_RelayInterval; }

            set{
                if(m_RelayInterval != value){
                    m_RelayInterval = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets relay retry interval seconds.
        /// </summary>
        public int RelayRetryInterval
        {
            get{ return m_RelayRetryInterval; }

            set{
                if(m_RelayRetryInterval != value){
                    m_RelayRetryInterval = value;
                }
            }
        }
                
        /// <summary>
        /// Gets or sets after how many minutes delayed delivery warning is sent.
        /// </summary>
        public int UndeliveredWarningAfter
        {
            get{ return m_UndeliveredWarningAfter; }

            set{
                if(m_UndeliveredWarningAfter != value){
                    m_UndeliveredWarningAfter = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets after how many minutes message is considered undelivered. 
        /// Undelivered notification is sent to sender.
        /// </summary>
        public int UndeliveredAfter
        {
            get{ return m_UndeliveredAfter; }

            set{
                if(m_UndeliveredAfter != value){
                    m_UndeliveredAfter = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets message template what is sent when message delayed delivery, immediate delivery failed.
        /// </summary>
        public ServerReturnMessage UndeliveredWarningMessage
        {
            get{ return m_UndeliveredWarningMessage; }

            set{
                if(m_UndeliveredWarningMessage != value){
                    m_UndeliveredWarningMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets message template what is sent when message delivery has failed.
        /// </summary>
        public ServerReturnMessage UndeliveredMessage
        {
            get{ return m_UndeliveredMessage; }

            set{
                if(m_UndeliveredMessage != value){
                    m_UndeliveredMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets IP address what is used to send out messages or null if not specified. 
        /// </summary>
        public IPAddress SendingIP
        {
            get{ return m_pSendingIP; }

            set{
                if(m_pSendingIP != value){
                    m_pSendingIP = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets if relay command are logged.
        /// </summary>
        public bool LogCommands
        {
            get{ return m_LogCommands; }

            set{ 
                if(m_LogCommands != value){
                    m_LogCommands = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets path where to log relay commands.
        /// </summary>
        public string LogsPath
        {
            get{ return m_LogsPath; }

            set{ 
                if(m_LogsPath != value){
                    m_LogsPath = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets if undelivered messages are stored.
        /// </summary>
        public bool StoreUndeliveredMessages
        {
            get{ return m_StoreUndelivered; }

            set{
                if(m_StoreUndelivered != value){
                    m_StoreUndelivered = value;
                }
            }
        }

        #endregion

    }
}
