using System;
using System.IO;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace LumiSoft.Net.POP3.Server
{
	#region Event delegates

	/// <summary>
	/// Represents the method that will handle the AuthUser event for POP3_Server.
	/// </summary>
	/// <param name="sender">The source of the event. </param>
	/// <param name="e">A AuthUser_EventArgs that contains the event data.</param>
	public delegate void AuthUserEventHandler(object sender,AuthUser_EventArgs e);

	/// <summary>
	/// Represents the method that will handle the GetMessgesList event for POP3_Server.
	/// </summary>
	/// <param name="sender">The source of the event. </param>
	/// <param name="e">A GetMessagesInfo_EventArgs that contains the event data.</param>
	public delegate void GetMessagesInfoHandler(object sender,GetMessagesInfo_EventArgs e);

	/// <summary>
	/// Represents the method that will handle the GetMessage event for POP3_Server.
	/// </summary>
	/// <param name="sender">The source of the event. </param>
	/// <param name="e">A GetMessage_EventArgs that contains the event data.</param>
	public delegate void GetMessageHandler(object sender,GetMessage_EventArgs e);

	/// <summary>
	/// Represents the method that will handle the DeleteMessage event for POP3_Server.
	/// </summary>
	/// <param name="sender">The source of the event. </param>
	/// <param name="e">A DeleteMessage_EventArgs that contains the event data.</param>
	public delegate void DeleteMessageHandler(object sender,DeleteMessage_EventArgs e);

	/// <summary>
	/// Represents the method that will handle the GetTopLines event for POP3_Server.
	/// </summary>
	/// <param name="sender">The source of the event. </param>
	/// <param name="e">A GetTopLines_Eventargs that contains the event data.</param>
	public delegate void GetTopLinesHandler(object sender,GetTopLines_Eventargs e);

	#endregion

	/// <summary>
	/// POP3 server component.
	/// </summary>
	public class POP3_Server : System.ComponentModel.Component
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private TcpListener POP3_Listener  = null;		
		private Hashtable   m_SessionTable = null;
				
		private string m_IPAddress          = "ALL";  // Holds IP Address, which to listen incoming calls.
		private int    m_port               = 110;    // Holds port number, which to listen incoming calls.
		private int    m_MaxThreads         = 10;     // Holds maximum allowed Worker Threads (Users).
		private bool   m_enabled            = false;  // If true listens incoming calls.
		private bool   m_LogCmds            = false;  // If true, writes POP3 commands to log file.
		private int    m_SessionIdleTimeOut = 80000;  // Holds session idle timeout.
		private int    m_CommandIdleTimeOut = 60000;  // Holds command ilde timeout.
		private int    m_MaxBadCommands     = 8;       // Holds maximum bad commands allowed to session.
        				
		#region Event declarations

		/// <summary>
		/// Occurs when new computer connected to POP3 server.
		/// </summary>
		public event ValidateIPHandler ValidateIPAddress = null;

		/// <summary>
		/// Occurs when connected user tryes to authenticate.
		/// </summary>
		public event AuthUserEventHandler AuthUser = null;

		/// <summary>
		/// Occurs user session ends. This is place for clean up.
		/// </summary>
		public event EventHandler SessionEnd = null;

		/// <summary>
		/// Occurs user session resetted. Messages marked for deletion are unmarked.
		/// </summary>
		public event EventHandler SessionResetted = null;

		/// <summary>
		/// Occurs when server needs to know logged in user's maibox messages.
		/// </summary>
		public event GetMessagesInfoHandler GetMessgesList = null;

		/// <summary>
		/// Occurs when user requests specified message.
		/// </summary>
		public event GetMessageHandler GetMessage = null;

		/// <summary>
		/// Occurs when user requests delete message.
		/// </summary>		
		public event DeleteMessageHandler DeleteMessage = null;

		/// <summary>
		/// Occurs when user requests specified message TOP lines.
		/// </summary>
		public event GetTopLinesHandler GetTopLines = null;

		/// <summary>
		/// Occurs when server has system error(Unknown error).
		/// </summary>
		public event ErrorEventHandler SysError = null;

		/// <summary>
		/// Occurs when POP3 session has finished and session log is available.
		/// </summary>
		public event LogEventHandler SessionLog = null;

		#endregion


		#region Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="container"></param>
		public POP3_Server(System.ComponentModel.IContainer container)
		{			
			// Required for Windows.Forms Class Composition Designer support
			container.Add(this);
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// 
		/// </summary>
		public POP3_Server()
		{			
			// Required for Windows.Forms Class Composition Designer support
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		#endregion

		#region function Dispose

		/// <summary>
		/// Clean up any resources being used and STOPs POP3 server.
		/// </summary>
		public new void Dispose()
		{
			base.Dispose();

			Stop();				
		}

		#endregion
		
		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion


		#region function Start

		/// <summary>
		/// Starts POP3 Server.
		/// </summary>
		private void Start()
		{
			try
			{
				if(!m_enabled && !this.DesignMode){
					m_SessionTable = new Hashtable();
					Thread startPOP3Server = new Thread(new ThreadStart(Run));
					startPOP3Server.Start();
				}
			}
			catch(Exception x)
			{
				OnSysError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function Stop

		/// <summary>
		/// Stops POP3 Server.
		/// </summary>
		private void Stop()
		{
			try
			{	
				if(POP3_Listener != null){
					POP3_Listener.Stop();
				}
			}
			catch(Exception x)
			{
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
					POP3_Listener = new TcpListener(IPAddress.Any,m_port);
				}
				else{
					POP3_Listener = new TcpListener(IPAddress.Parse(m_IPAddress),m_port);
				}

				// Start listening
				POP3_Listener.Start();


				while(true){
					// Check if maximum allowed thread count isn't exceeded
					if(m_SessionTable.Count < m_MaxThreads){

						// Thread is sleeping, until a client connects
						Socket clientSocket = POP3_Listener.AcceptSocket();

						string sessionID = clientSocket.GetHashCode().ToString();

						//****
						_LogWriter logWriter      = new _LogWriter(this.SessionLog);
						POP3_Session session      = new POP3_Session(clientSocket,this,sessionID,logWriter);

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
			catch(Exception x){
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
		internal void AddSession(string sessionID,POP3_Session session,_LogWriter logWriter)
		{
			m_SessionTable.Add(sessionID,session);

			if(m_LogCmds){
				logWriter.AddEntry("//----- Sys: 'Session:'" + sessionID + " added" + DateTime.Now);				
			}
		}

		#endregion

		#region function RemoveSession

		/// <summary>
		/// Removes session.
		/// </summary>
		/// <param name="session">Session which to remove.</param>
		/// <param name="logWriter">Log writer.</param>
		internal void RemoveSession(POP3_Session session,_LogWriter logWriter)
		{
			lock(m_SessionTable){
				if(!m_SessionTable.Contains(session.SessionID)){
					OnSysError(new Exception("Session '" + session.SessionID + "' doesn't exist."),new System.Diagnostics.StackTrace());
					return;
				}
				m_SessionTable.Remove(session.SessionID);
			
				// Raise session end event
				OnSessionEnd(session);
			}

			if(m_LogCmds){
				logWriter.AddEntry("//----- Sys: 'Session:'" + session.SessionID + " removed" + DateTime.Now);
			}
		}

		#endregion

		#region function IsUserLoggedIn

		/// <summary>
		/// Checks if user is logged in.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <returns></returns>
		internal bool IsUserLoggedIn(string userName)
		{			
			lock(m_SessionTable){
				foreach(POP3_Session sess in m_SessionTable.Values){
					if(sess.UserName == userName){
						return true;
					}
				}
			}
			
            return false;
		}

		#endregion

		#endregion


		#region Properties implementation

		/// <summary>
		/// Gets or sets whick IP address to listen.
		/// </summary>
		[		
		Description("IP Address to Listen POP3 requests"),
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
		Description("Port to use for POP3"),
		DefaultValue(110),
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
		DefaultValue(10),
		]
		public int Threads 
		{
			get{ return m_MaxThreads; }
			set{ m_MaxThreads = value; }
		}


		/// <summary>
		/// Runs or stops server.
		/// </summary>
		[Description("Use this property to run and stop POP3 Server"),
		DefaultValue(false),]
		public bool Enabled {
			get{ return m_enabled; }
			set{				
				if(value != m_enabled){
					if(value){
						Start();
					}
					else{
						Stop();
					}

					m_enabled = value;
				}
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
		/// Session idle timeout.
		/// </summary>
		public int SessionIdleTimeOut 
		{
			get{ return m_SessionIdleTimeOut; }

			set{ m_SessionIdleTimeOut = value; }
		}

		/// <summary>
		/// Command idle timeout.
		/// </summary>
		public int CommandIdleTimeOut 
		{
			get{ return m_CommandIdleTimeOut; }

			set{ m_CommandIdleTimeOut = value; }
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
		/// <param name="endpoint">Connected host EndPoint.</param>
		/// <returns></returns>
		internal virtual bool OnValidate_IpAddress(EndPoint endpoint) 
		{			
			ValidateIP_EventArgs oArg = new ValidateIP_EventArgs(endpoint);
			if(this.ValidateIPAddress != null){
				this.ValidateIPAddress(this, oArg);
			}

			return oArg.Validated;						
		}

		#endregion

		#region function OnAuthUser

		/// <summary>
		/// Authenticates user. // miceli
		/// </summary>
		/// <param name="session">Reference to current pop3 session.</param>
		/// <param name="userName">User name.</param>
		/// <param name="passwData"></param>
		/// <param name="data"></param>
		/// <param name="authType"></param>
		/// <returns></returns>
		internal virtual bool OnAuthUser(POP3_Session session,string userName,string passwData,string data,AuthType authType) 
		{				
			AuthUser_EventArgs oArg = new AuthUser_EventArgs(session,userName,passwData,data,authType);
			if(this.AuthUser != null){
				this.AuthUser(this,oArg);
			}
			
			return oArg.Validated;
		}

		#endregion


		#region function OnGetMessagesInfo

		/// <summary>
		/// Gest pop3 messages info.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="messages"></param>
		internal virtual void OnGetMessagesInfo(POP3_Session session,POP3_Messages messages) 
		{				
			GetMessagesInfo_EventArgs oArg = new GetMessagesInfo_EventArgs(session,messages,session.UserName);
			if(this.GetMessgesList != null){
				this.GetMessgesList(this, oArg);
			}
		}

		#endregion

		#region function OnGetMail

		/// <summary>
		/// Raises event get message.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="message">Message which to get.</param>
		/// <param name="sessionSocket">Message which to get.</param>
		/// <returns></returns>
		internal virtual byte[] OnGetMail(POP3_Session session,POP3_Message message,Socket sessionSocket) 
		{			
			GetMessage_EventArgs oArg = new GetMessage_EventArgs(session,message,sessionSocket);
			if(this.GetMessage != null){
				this.GetMessage(this,oArg);
			}
			return oArg.MessageData;
		}

		#endregion

		#region function OnDeleteMessage

		/// <summary>
		/// Raises delete message event.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="message">Message which to delete.</param>
		/// <returns></returns>
		internal virtual bool OnDeleteMessage(POP3_Session session,POP3_Message message) 
		{				
			DeleteMessage_EventArgs oArg = new DeleteMessage_EventArgs(session,message);
			if(this.DeleteMessage != null){
				this.DeleteMessage(this, oArg);
			}
			
			return true;
		}

		#endregion

		#region function OnGetTopLines

		/// <summary>
		/// Raises event GetTopLines.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="message">Message wich top lines to get.</param>
		/// <param name="nLines">Header + number of body lines to get.</param>
		/// <returns></returns>
		internal byte[] OnGetTopLines(POP3_Session session,POP3_Message message,int nLines)
		{
			GetTopLines_Eventargs oArgs = new GetTopLines_Eventargs(session,message,nLines);
			if(this.GetTopLines != null){
				this.GetTopLines(this,oArgs);
			}
			return oArgs.LinesData;
		}

		#endregion

		
		#region function OnSessionEnd

		/// <summary>
		/// Raises SessionEnd event.
		/// </summary>
		/// <param name="session">Session which is ended.</param>
		internal void OnSessionEnd(object session)
		{
			if(this.SessionEnd != null){
				this.SessionEnd(session,new EventArgs());
			}
		}

		#endregion

		#region function OnSessionResetted

		/// <summary>
		/// Raises SessionResetted event.
		/// </summary>
		/// <param name="session">Session which is resetted.</param>
		internal void OnSessionResetted(object session)
		{
			if(this.SessionResetted != null){
				this.SessionResetted(session,new EventArgs());
			}
    // miceli switch to non encrypted mode i thi
		}

		#endregion


		#region function OnSysError

		/// <summary>
		/// Raises SysError event.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="stackTrace"></param>
		internal void OnSysError(Exception x,StackTrace stackTrace)
		{if(this.SysError != null){ 	this.SysError(this,new Error_EventArgs(x,stackTrace));}}
		#endregion
		#endregion
	}
}