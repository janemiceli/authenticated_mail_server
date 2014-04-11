using System;
using System.IO;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using LumiSoft.Net;

namespace LumiSoft.Net.NNTP.Server
{
	#region Event Delegates

	public delegate void ListGroupsHandler(object sender,NNTP_ListGroups_eArgs e);

	public delegate NNTP_NewsGroup GroupInfoHandler(object sender,NNTP_NewsGroup grp);

	public delegate string GetArticleHandler(NNTP_Session ses, string id,string retVal);

	public delegate void XoverInfoHandler(object sender,NNTP_Articles_eArgs e);

	public delegate void NewNewsHandler(object sender,NNTP_Articles_eArgs e,string newsgroups,DateTime since);

	public delegate string StoreMessageHandler(NNTP_Session ses,MemoryStream msgStream,string[] newsgroups);

	#endregion

	/// <summary>
	/// NNTP server component.
	/// </summary>
	public class NNTP_Server : System.ComponentModel.Component 
	{ 
		private TcpListener NNTP_Listener  = null;
		private Hashtable   m_SessionTable = null;
		
		private string   m_IPAddress          = "ALL";   // Holds IP Address, which to listen incoming calls.
		private int      m_port               = 119;     // Holds port number, which to listen incoming calls.
		private int      m_MaxThreads         = 20;      // Holds maximum allowed Worker Threads.
		private bool     m_enabled            = false;   // If true listens incoming calls.
		private bool     m_LogCmds            = false;   // If true, writes POP3 commands to log file.
		private int      m_SessionIdleTimeOut = 80000;   // Holds session idle timeout.
		private int      m_CommandIdleTimeOut = 6000000; // Holds command ilde timeout.
		private int      m_MaxMessageSize     = 1000000; // Hold maximum message size.	
		private int      m_MaxBadCommands     = 8;       // Holds maximum bad commands allowed to session.



		#region Events declarations		
		/// <summary>
		/// Occurs when server has system error(Unknown error).
		/// </summary>
		public event ErrorEventHandler SysError = null;
		/// <summary>
		/// Occurs when NNTP session has finished and session log is available.
		/// </summary>
		public event LogEventHandler SessionLog = null;
		/// <summary>
		/// Occurs when LIST of groups are requested.
		/// </summary>
		public event ListGroupsHandler ListGroups = null;

		/// <summary>
		/// Occurs when a group is selected
		/// </summary>
		public event GroupInfoHandler GroupInfo = null;

		/// <summary>
		/// Occurs when a Xover cmd is given
		/// </summary>
		public event XoverInfoHandler XoverInfo = null;

		/// <summary>
		/// Occurs when a NewNews cmd is given
		/// </summary>		
		public event NewNewsHandler NewNews = null;
		/// <summary>
		/// Occurs when an article is requested
		/// </summary>
		public event GetArticleHandler GetArticle = null;
		/// <summary>
		/// Occurs when a message needs to be stored
		/// </summary>
		public event StoreMessageHandler StoreMessage = null;
		#endregion

		private void InitializeComponent()
		{

		}

	
		/// <summary>
		/// Default constructor.
		/// </summary>
		public NNTP_Server()
		{
		}


		#region function Start

		/// <summary>
		/// Starts SMTP Server.
		/// </summary>
		private void Start()
		{
			try
			{
			//	if(!m_enabled && !this.DesignMode){
					m_SessionTable = new Hashtable();

					Thread startServer = new Thread(new ThreadStart(Run));
					startServer.Start();
			//	}
			}
			catch(Exception x){
				OnSysError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function Stop

		/// <summary>
		/// Stops SMTP Server.
		/// </summary>
		private void Stop()
		{
			try
			{
				if(NNTP_Listener != null){
					NNTP_Listener.Stop();
				}
			}
			catch(Exception x){
				OnSysError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function Run

		/// <summary>
		/// Starts server message loop.
		/// </summary>
		private void Run()
		{			
			try
			{
				// check which ip's to listen (all or assigned)
				if(m_IPAddress.ToLower().IndexOf("all") > -1){
					NNTP_Listener = new TcpListener(IPAddress.Any,m_port);
				}
				else{
					NNTP_Listener = new TcpListener(IPAddress.Parse(m_IPAddress),m_port);
				}
				// Start listening
				NNTP_Listener.Start();
                

				//-------- Main Server message loop --------------------------------//
				while(true){
					// Check if maximum allowed thread count isn't exceeded
					if(m_SessionTable.Count <= m_MaxThreads){

						// Thread is sleeping, until a client connects
						Socket clientSocket = NNTP_Listener.AcceptSocket();

						string sessionID = clientSocket.GetHashCode().ToString();

						//****
						_LogWriter logWriter = new _LogWriter(this.SessionLog);
						NNTP_Session session = new NNTP_Session(clientSocket,this,sessionID,logWriter);
						
						Thread clientThread = new Thread(new ThreadStart(session.StartProcessing));
						
						// Add session to session list
						AddSession(sessionID,session,logWriter);

						// Start proccessing
						clientThread.Start();
					}
					else{
						Thread.Sleep(100);
					}
				}
			}
			catch(ThreadInterruptedException e){
				string dummy = e.Message;     // Neede for to remove compile warning
				Thread.CurrentThread.Abort();
			}
			catch(Exception x)
			{
				if(x.Message != "A blocking operation was interrupted by a call to WSACancelBlockingCall"){
					OnSysError(x,new System.Diagnostics.StackTrace());
				}
			}
		}

		#endregion

		#region Session handling stuff

		#region function AddSession

		/// <summary>
		/// Adds session.
		/// </summary>
		/// <param name="sessionID">Session ID.</param>
		/// <param name="session">Session object.</param>
		/// <param name="logWriter">Log writer.</param>
		internal void AddSession(string sessionID,NNTP_Session session,_LogWriter logWriter)
		{
			lock(m_SessionTable){
				m_SessionTable.Add(sessionID,session);

				if(m_LogCmds){
					logWriter.AddEntry("//----- Sys: 'Session:'" + sessionID + " added " + DateTime.Now);
				}
			}
		}

		#endregion

		#region function RemoveSession

		/// <summary>
		/// Removes session.
		/// </summary>
		/// <param name="sessionID">Session ID.</param>
		/// <param name="logWriter">Log writer.</param>
		internal void RemoveSession(string sessionID,_LogWriter logWriter)
		{
			lock(m_SessionTable){
				if(!m_SessionTable.Contains(sessionID)){
					OnSysError(new Exception("Session '" + sessionID + "' doesn't exist."),new System.Diagnostics.StackTrace());
					return;
				}
				m_SessionTable.Remove(sessionID);				
			}

			if(m_LogCmds){
				logWriter.AddEntry("//----- Sys: 'Session:'" + sessionID + " removed " + DateTime.Now);
			}
		}

		#endregion

		#endregion

		#region Properties Implementaion

		/// <summary>
		/// Gets or sets which IP address to listen.
		/// </summary>
		[		
		Description("IP Address to Listen NNTP requests"),
		DefaultValue("ALL"),
		]
		public string IpAddress 
		{
			get{ return m_IPAddress; }

			set{ m_IPAddress = value; }
		}


		/// <summary>
		/// Gets or sets which port to listen.
		/// </summary>
		[		
		Description("Port to use for NNTP"),
		DefaultValue(119),
		]
		public int Port 
		{
			get{ return m_port;	}

			set{ m_port = value; }
		}


		/// <summary>
		/// Gets or sets maximum session threads.
		/// </summary>
		[		
		Description("Maximum Allowed threads"),
		DefaultValue(20),
		]
		public int Threads 
		{
			get{ return m_MaxThreads; }

			set{ m_MaxThreads = value; }
		}


		/// <summary>
		/// Runs and stops server.
		/// </summary>
		[		
		Description("Use this property to run and stop NNTP Server"),
		DefaultValue(false),
		]
		public bool Enabled 
		{
			get{ return m_enabled; }

			set{				
				if(value){
					Start();
				}
				else{
					Stop();
				}

				m_enabled = value;
			}
		}
	
		/// <summary>
		/// Gets or sets if to log commands.
		/// </summary>
		public bool LogCommands
		{
			get{ return m_LogCmds; }

			set{ m_LogCmds = value; }
		}

		/// <summary>
		/// Session idle timeout in milliseconds.
		/// </summary>
		public int SessionIdleTimeOut 
		{
			get{ return m_SessionIdleTimeOut; }

			set{ m_SessionIdleTimeOut = value; }
		}

		/// <summary>
		/// Command idle timeout in milliseconds.
		/// </summary>
		public int CommandIdleTimeOut 
		{
			get{ return m_CommandIdleTimeOut; }

			set{ m_CommandIdleTimeOut = value; }
		}

		/// <summary>
		/// Maximum message size.
		/// </summary>
		public int MaxMessageSize 
		{
			get{ return m_MaxMessageSize; }

			set{ m_MaxMessageSize = value; }
		}

		/// <summary>
		/// Gets or sets maximum bad commands allowed to session.
		/// </summary>
		public int MaxBadCommands
		{
			get{ return m_MaxBadCommands; }

			set{ m_MaxBadCommands = value; }
		}
		
		#endregion

		#region Events Implementation

		#region function OnValidate_IpAddress
		
		/// <summary>
		/// Raises event ValidateIP.
		/// </summary>
		/// <param name="enpoint">Connected host EndPoint.</param>
		/// <returns>Returns true if connection allowed.</returns>
		internal bool OnValidate_IpAddress(EndPoint enpoint) 
		{			
			ValidateIP_EventArgs oArg = new ValidateIP_EventArgs(enpoint);
		//	if(this.ValidateIPAddress != null){
		//		this.ValidateIPAddress(this, oArg);
		//	}

			return oArg.Validated;						
		}

		#endregion

		internal NNTP_NewsGroups OnGetNewsGroups(NNTP_Session ses)
		{
			NNTP_NewsGroups groups = new NNTP_NewsGroups();  
			NNTP_ListGroups_eArgs oArgs = new NNTP_ListGroups_eArgs(ses,groups);
			
			if(ListGroups != null)
			{
				ListGroups(this,oArgs);
			}
			return groups;
		}

		internal string OnGetArticle(NNTP_Session ses,string articleId)
		{
			string retVal = ""; 
			if(GetArticle != null)
			{
				retVal = GetArticle(ses,articleId,retVal);
			}
			return retVal;
		}

		internal NNTP_NewsGroup OnGetGroupInfo(NNTP_Session ses,string newsgroup)
		{			 
			NNTP_NewsGroup retVal = new NNTP_NewsGroup(newsgroup,0,0,0,DateTime.Today);
			if(GroupInfo != null)
			{
				retVal = GroupInfo(this,retVal);
			}
			return retVal;			
		}

		internal NNTP_Articles_eArgs OnGetXOVER(NNTP_Session ses,string newsgroup)
		{			 
			NNTP_Articles_eArgs oArgs = new NNTP_Articles_eArgs(new NNTP_Articles(),newsgroup);
			if(XoverInfo != null)
			{
				XoverInfo(this,oArgs);
			}
			return oArgs;			
		}

		internal NNTP_Articles_eArgs OnGetNewNews(object sender,string newsgroups,DateTime since)
		{			 
			NNTP_Articles_eArgs oArgs = new NNTP_Articles_eArgs(new NNTP_Articles(),newsgroups);
			if(NewNews != null)
			{
				NewNews(this,oArgs,newsgroups,since);
			}
			return oArgs;			
		}

		internal string OnStoreArticle(NNTP_Session ses,MemoryStream msgStream,string[] newsgroups)
		{	
			string msgId = "";
			if(StoreMessage != null)
			{
				msgId = StoreMessage(ses,msgStream,newsgroups);
			}
			return msgId;
		}



		#region function OnSysError

		/// <summary>
		/// Raises SysError event.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="stackTrace"></param>
		internal void OnSysError(Exception x,StackTrace stackTrace)
		{
			if(this.SysError != null){
				this.SysError(this,new Error_EventArgs(x,stackTrace));
			}
		}

		#endregion

		#endregion
	
	

	}
}
