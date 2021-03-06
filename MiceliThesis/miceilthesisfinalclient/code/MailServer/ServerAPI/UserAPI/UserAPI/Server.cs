using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Data;
using System.Timers;

using LumiSoft.Net;

namespace LumiSoft.MailServer.API.UserAPI
{
    /// <summary>
    /// The Server object represents an LumiSoft Mail Server computer.
    /// </summary>
    public class Server : IDisposable
    {
        private bool                    m_Connected         = false;
        private string                  m_Host              = "";
        private string                  m_UserName          = "";
        private SocketEx                m_pSocket           = null;
        private SessionCollection       m_pSessions         = null;
        private EventCollection         m_pEvents           = null;
        private VirtualServerCollection m_pVirtualServers   = null;
        private object                  m_pLockSynchronizer = null;
        private Timer                   m_pTimerNoop        = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Server()
        {
            m_pSocket           = new SocketEx();
            m_pLockSynchronizer = m_pSocket;
        }

        #region mehtod Dispose

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }

        #endregion


        #region mehtod Connect

        /// <summary>
        /// Connects to the specified LumiSoft mail server.
        /// </summary>
        /// <param name="host">Host name or IP of server to connect.</param>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        public void Connect(string host,string userName,string password)
        {   
            if(m_Connected){
                return;
            }
    
            m_pSocket.Encoding = System.Text.Encoding.UTF8;
            m_pSocket.Connect(host,5252);

            // Read greeting text
            m_pSocket.ReadLine();
            
            // Auth user
            m_pSocket.WriteLine("LOGIN " + TextUtils.QuoteString(userName) + " " + TextUtils.QuoteString(password));

            string response = m_pSocket.ReadLine();
            if(!response.StartsWith("+OK")){
                m_pSocket.Disconnect();

                throw new Exception(response);
            }

            m_Connected = true;
            m_Host      = host;
            m_UserName  = userName;

            m_pTimerNoop = new Timer(30000);
            m_pTimerNoop.AutoReset = true;
            m_pTimerNoop.Elapsed += new ElapsedEventHandler(m_pTimerNoop_Elapsed);
            m_pTimerNoop.Enabled = true;
        }
                
        #endregion

        #region method Disconnect

        /// <summary>
        /// Disconnects from connected server.
        /// </summary>
        public void Disconnect()
        {
            m_pSocket.Disconnect();
            m_Connected = false;

            m_pTimerNoop.Dispose();
            m_pTimerNoop = null;
        }

        #endregion


        #region method m_pTimerNoop_Elapsed

        private void m_pTimerNoop_Elapsed(object sender,ElapsedEventArgs e)
        {
            try{
                /* Noop
                  Responses:
                    +OK 
                */

                lock(this.LockSynchronizer){
                    m_pSocket.WriteLine("NOOP");
                    m_pSocket.ReadLine();
                }
            }
            catch{
            }
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets if API is connected to server.
        /// </summary>
        public bool Connected
        {
            get{ return m_Connected; }
        }

        /// <summary>
        /// Gets connected host name or IP. NOTE: You must be connected before accessing this property.
        /// </summary>
        public string Host
        {
            get{
                if(!m_Connected){
                    throw new Exception("You must connect first, before accessing this property !");
                }

                return m_Host;
            }
        }

        /// <summary>
        /// Gets user name which was used to connect to mail server. NOTE: You must be connected before accessing this property.
        /// </summary>
        public string UserName
        {
            get{
                if(!m_Connected){
                    throw new Exception("You must connect first, before accessing this property !");
                }

                return m_UserName;
            }
        }

        /// <summary>
        /// Gets mail server IP addresses. NOTE: You must be connected before accessing this property.
        /// </summary>
        public IPAddress[] IPAddresses
        {
            get{
                if(!m_Connected){
                    throw new Exception("You must connect first, before accessing this property !");
                }

                /* GetIPAddresses
                      Responses:
                        +OK <sizeOfData>
                        <data>
                        
                        -ERR <errorText>
                */

                lock(this.LockSynchronizer){
                    // Call TCP GetIPAddresses
                    this.Socket.WriteLine("GetIPAddresses");

                    string response = this.Socket.ReadLine();
                    if(!response.ToUpper().StartsWith("+OK")){
                        throw new Exception(response);
                    }

                    int sizeOfData = Convert.ToInt32(response.Split(new char[]{' '},2)[1]);
                    MemoryStream ms = new MemoryStream();
                    this.Socket.ReadSpecifiedLength(sizeOfData,ms);
                    
                    // Decompress dataset
                    DataSet ds = Utils.DecompressDataSet(ms);

                    List<IPAddress> retVal = new List<IPAddress>();
                    if(ds.Tables.Contains("dsIPs")){
                        foreach(DataRow dr in ds.Tables["dsIPs"].Rows){
                            retVal.Add(IPAddress.Parse(dr["IP"].ToString()));
                        }
                    }

                    return retVal.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets mail server sessions. NOTE: You must be connected before accessing this property.
        /// </summary>
        public ServerInfo ServerInfo
        {
            get{
                if(!m_Connected){
                    throw new Exception("You must connect first, before accessing this property !");
                }

                /* GetServerInfo
                      Responses:
                        +OK <sizeOfData>
                        <data>
                        
                        -ERR <errorText>
                */
                
                lock(this.LockSynchronizer){
                    // Call TCP GetServerInfo
                    this.Socket.WriteLine("GetServerInfo");

                    string response = this.Socket.ReadLine();
                    if(!response.ToUpper().StartsWith("+OK")){
                        throw new Exception(response);
                    }

                    int sizeOfData = Convert.ToInt32(response.Split(new char[]{' '},2)[1]);
                    MemoryStream ms = new MemoryStream();
                    this.Socket.ReadSpecifiedLength(sizeOfData,ms);
                    
                    // Decompress dataset
                    DataSet ds = Utils.DecompressDataSet(ms);

                    return new ServerInfo(
                        ds.Tables["ServerInfo"].Rows[0]["OS"].ToString(),
                        ds.Tables["ServerInfo"].Rows[0]["MailServerVersion"].ToString(),
                        Convert.ToInt32(ds.Tables["ServerInfo"].Rows[0]["MemoryUsage"]),
                        Convert.ToInt32(ds.Tables["ServerInfo"].Rows[0]["CpuUsage"]),
                        Convert.ToDateTime(ds.Tables["ServerStartTime"]),
                        Convert.ToInt32(ds.Tables["ServerInfo"].Rows[0]["Read_KB_Sec"]),
                        Convert.ToInt32(ds.Tables["ServerInfo"].Rows[0]["Write_KB_Sec"]),
                        Convert.ToInt32(ds.Tables["ServerInfo"].Rows[0]["SmtpSessions"]),
                        Convert.ToInt32(ds.Tables["ServerInfo"].Rows[0]["Pop3Sessions"]),
                        Convert.ToInt32(ds.Tables["ServerInfo"].Rows[0]["ImapSessions"]),
                        Convert.ToInt32(ds.Tables["ServerInfo"].Rows[0]["RelaySessions"])
                    );
                }
            } 
        }

        /// <summary>
        /// Gets mail server sessions. NOTE: You must be connected before accessing this property.
        /// </summary>
        public SessionCollection Sessions
        {
            get{
                if(!m_Connected){
                    throw new Exception("You must connect first, before accessing this property !");
                }

                // Not created yet, create it. We use late binding here, 
                // create objects and bind them when we need them.
                if(m_pSessions == null){
                    m_pSessions = new SessionCollection(this);
                }

                return m_pSessions; 
            } 
        }

        /// <summary>
        /// Gets mail server events. NOTE: You must be connected before accessing this property.
        /// </summary>
        public EventCollection Events
        {
            get{
                if(!m_Connected){
                    throw new Exception("You must connect first, before accessing this property !");
                }

                // Not created yet, create it. We use late binding here, 
                // create objects and bind them when we need them.
                if(m_pEvents == null){
                    m_pEvents = new EventCollection(this);
                }

                return m_pEvents; 
            } 
        }

        /// <summary>
        /// Gets mail server virtual servers. NOTE: You must be connected before accessing this property.
        /// </summary>
        public VirtualServerCollection VirtualServers
        {
            get{
                if(!m_Connected){
                    throw new Exception("You must connect first, before accessing this property !");
                }

                // Not created yet, create it. We use late binding here, 
                // create objects and bind them when we need them.
                if(m_pVirtualServers == null){
                    m_pVirtualServers = new VirtualServerCollection(this);
                }

                return m_pVirtualServers; 
            } 
        }


        /// <summary>
        /// Gets connected socket.
        /// </summary>
        internal SocketEx Socket
        {
            get{ return m_pSocket; }
        }

        /// <summary>
        /// Gets lobal lock() object what can be used to lock whole User API.
        /// </summary>
        internal object LockSynchronizer
        {
            get{ return m_pLockSynchronizer; }
        }
    
        #endregion

    }    
}
