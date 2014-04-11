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

namespace LumiSoft.Net.IMAP.Server
{
	#region Event delegates

	/// <summary>
	/// Represents the method that will handle the AuthUser event for SMTP_Server.
	/// </summary>
	/// <param name="sender">The source of the event. </param>
	/// <param name="e">A AuthUser_EventArgs that contains the event data.</param>
	public delegate void AuthUserEventHandler(object sender,AuthUser_EventArgs e);

	/// <summary>
	/// 
	/// </summary>
	public delegate void FolderEventHandler(object sender,Mailbox_EventArgs e);

	/// <summary>
	/// 
	/// </summary>
	public delegate void FoldersEventHandler(object sender,IMAP_Folders e);

	/// <summary>
	/// 
	/// </summary>
	public delegate void MessagesEventHandler(object sender,IMAP_Messages e);

	/// <summary>
	/// 
	/// </summary>
	public delegate void MessageEventHandler(object sender,Message_EventArgs e);

	#endregion

	/// <summary>
	/// IMAP server componet.
	/// </summary>
	public class IMAP_Server : System.ComponentModel.Component
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private TcpListener IMAP_Listener  = null;
		private Hashtable   m_SessionTable = null;
		
		private string m_IPAddress          = "ALL";   // Holds IP Address, which to listen incoming calls.
		private int    m_port               = 143;     // Holds port number, which to listen incoming calls.
		private int    m_MaxThreads         = 20;      // Holds maximum allowed Worker Threads.
		private bool   m_enabled            = false;   // If true listens incoming calls.
		private bool   m_LogCmds            = false;   // If true, writes commands to log file.
		private int    m_SessionIdleTimeOut = 80000;   // Holds session idle timeout.
		private int    m_CommandIdleTimeOut = 6000000; // Holds command ilde timeout.
		private int    m_MaxMessageSize     = 1000000; // Hold maximum message size.
		private int    m_MaxBadCommands     = 8;       // Holds maximum bad commands allowed to session.

		#region Events declarations

		/// <summary>
		/// Occurs when new computer connected to IMAP server.
		/// </summary>
		public event ValidateIPHandler ValidateIPAddress = null;

		/// <summary>
		/// Occurs when connected user tryes to authenticate.
		/// </summary>
		public event AuthUserEventHandler AuthUser = null;

		/// <summary>
		/// Occurs when server requests to subscribe folder.
		/// </summary>
		public event FolderEventHandler SubscribeFolder = null;

		/// <summary>
		/// Occurs when server requests to unsubscribe folder.
		/// </summary>
		public event FolderEventHandler UnSubscribeFolder = null;

		/// <summary>
		/// Occurs when server requests all available folders.
		/// </summary>
		public event FoldersEventHandler GetFolders = null;

		/// <summary>
		/// Occurs when server requests subscribed folders.
		/// </summary>
		public event FoldersEventHandler GetSubscribedFolders = null;

		/// <summary>
		/// Occurs when server requests to create folder.
		/// </summary>
		public event FolderEventHandler CreateFolder = null;

		/// <summary>
		/// Occurs when server requests to delete folder.
		/// </summary>
		public event FolderEventHandler DeleteFolder = null;

		/// <summary>
		/// Occurs when server requests to rename folder.
		/// </summary>
		public event FolderEventHandler RenameFolder = null;

		/// <summary>
		/// Occurs when server requests to folder messages info.
		/// </summary>
		public event MessagesEventHandler GetMessagesInfo = null;

		/// <summary>
		/// Occurs when server requests to delete message.
		/// </summary>
		public event MessageEventHandler DeleteMessage = null;

		/// <summary>
		/// Occurs when server requests to store message.
		/// </summary>
		public event MessageEventHandler StoreMessage = null;

		/// <summary>
		/// Occurs when server requests to store message flags.
		/// </summary>
		public event MessageEventHandler StoreMessageFlags = null;

		/// <summary>
		/// Occurs when server requests to copy message to new location.
		/// </summary>
		public event MessageEventHandler CopyMessage = null;

		/// <summary>
		/// Occurs when server requests to get message.
		/// </summary>
		public event MessageEventHandler GetMessage = null;
		
		/// <summary>
		/// Occurs when server has system error(Unknown error).
		/// </summary>
		public event ErrorEventHandler SysError = null;

		/// <summary>
		/// Occurs when IMAP session has finished and session log is available.
		/// </summary>
		public event LogEventHandler SessionLog = null;

		#endregion

		
		#region Constructors

		/// <summary>
		/// 
		/// </summary>
		/// <param name="container"></param>
		public IMAP_Server(System.ComponentModel.IContainer container)
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
		public IMAP_Server()
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
		/// Clean up any resources being used and stops IMAP server.
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
				if(IMAP_Listener != null){
					IMAP_Listener.Stop();
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
					IMAP_Listener = new TcpListener(IPAddress.Any,m_port);
				}
				else{
					IMAP_Listener = new TcpListener(IPAddress.Parse(m_IPAddress),m_port);
				}
				// Start listening
				IMAP_Listener.Start();
                

				//-------- Main Server message loop --------------------------------//
				while(true){
					// Check if maximum allowed thread count isn't exceeded
					if(m_SessionTable.Count <= m_MaxThreads){

						// Thread is sleeping, until a client connects
						Socket clientSocket = IMAP_Listener.AcceptSocket();

						string sessionID = clientSocket.GetHashCode().ToString();

						//****
						_LogWriter logWriter = new _LogWriter(this.SessionLog);
						IMAP_Session session = new IMAP_Session(clientSocket,this,sessionID,logWriter);
						
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
		internal void AddSession(string sessionID,IMAP_Session session,_LogWriter logWriter)
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
		Description("IP Address to Listen IMAP requests"),
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
		Description("Port to use for IMAP"),
		DefaultValue(143),
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
		DefaultValue(50),
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
		Description("Use this property to run and stop SMTP Server"),
		DefaultValue(false),
		]
		public bool Enabled 
		{
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
			if(this.ValidateIPAddress != null){
				this.ValidateIPAddress(this, oArg);
			}

			return oArg.Validated;						
		}

		#endregion

		#region function OnAuthUser

		/// <summary>
		/// Raises event AuthUser.
		/// </summary>
		/// <param name="session">Reference to current IMAP session.</param>
		/// <param name="userName">User name.</param>
		/// <param name="passwordData">Password compare data,it depends of authentication type.</param>
		/// <param name="data">For md5 eg. md5 calculation hash.It depends of authentication type.</param>
		/// <param name="authType">Authentication type.</param>
		/// <returns>Returns true if user is authenticated ok.</returns>
		internal bool OnAuthUser(IMAP_Session session,string userName,string passwordData,string data,AuthType authType)
		{
			AuthUser_EventArgs oArgs = new AuthUser_EventArgs(session,userName,passwordData,data,authType);
			if(this.AuthUser != null){
				this.AuthUser(this,oArgs);
			}

			return oArgs.Validated;
		}

		#endregion

		#region function OnSubscribeMailbox

		/// <summary>
		/// Raises event 'SubscribeMailbox'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="mailbox">Mailbox which to subscribe.</param>
		/// <returns></returns>
		internal string OnSubscribeMailbox(IMAP_Session session,string mailbox)
		{
			if(this.SubscribeFolder != null){
				Mailbox_EventArgs eArgs = new Mailbox_EventArgs(mailbox);
				this.SubscribeFolder(session,eArgs);

				return eArgs.ErrorText;
			}

			return null;
		}

		#endregion

		#region function OnUnSubscribeMailbox

		/// <summary>
		/// Raises event 'UnSubscribeMailbox'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="mailbox">Mailbox which to unsubscribe.</param>
		/// <returns></returns>
		internal string OnUnSubscribeMailbox(IMAP_Session session,string mailbox)
		{
			if(this.UnSubscribeFolder != null){
				Mailbox_EventArgs eArgs = new Mailbox_EventArgs(mailbox);
				this.UnSubscribeFolder(session,eArgs);

				return eArgs.ErrorText;
			}

			return null;
		}

		#endregion

		#region function OnGetSubscribedMailboxes

		/// <summary>
		/// Raises event 'GetSubscribedMailboxes'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="referenceName">Mailbox reference.</param>
		/// <param name="mailBox">Mailbox search pattern or mailbox.</param>
		/// <returns></returns>
		internal IMAP_Folders OnGetSubscribedMailboxes(IMAP_Session session,string referenceName,string mailBox)
		{
			IMAP_Folders retVal = new IMAP_Folders(referenceName,mailBox);
			if(this.GetSubscribedFolders != null){
				this.GetSubscribedFolders(session,retVal);
			}

			return retVal;
		}

		#endregion

		#region function OnGetMailboxes

		/// <summary>
		/// Raises event 'GetMailboxes'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="referenceName">Mailbox reference.</param>
		/// <param name="mailBox">Mailbox search pattern or mailbox.</param>
		/// <returns></returns>
		internal IMAP_Folders OnGetMailboxes(IMAP_Session session,string referenceName,string mailBox)
		{
			IMAP_Folders retVal = new IMAP_Folders(referenceName,mailBox);
			if(this.GetFolders != null){
				this.GetFolders(session,retVal);
			}

			return retVal;
		}

		#endregion

		#region function OnCreateMailbox

		/// <summary>
		/// Raises event 'CreateMailbox'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="mailbox">Mailbox to create.</param>
		/// <returns></returns>
		internal string OnCreateMailbox(IMAP_Session session,string mailbox)
		{
			if(this.CreateFolder != null){
				Mailbox_EventArgs eArgs = new Mailbox_EventArgs(mailbox);
				this.CreateFolder(session,eArgs);

				return eArgs.ErrorText;
			}

			return null;
		}

		#endregion

		#region function OnDeleteMailbox

		/// <summary>
		/// Raises event 'DeleteMailbox'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="mailbox">Mailbox which to delete.</param>
		/// <returns></returns>
		internal string OnDeleteMailbox(IMAP_Session session,string mailbox)
		{
			if(this.DeleteFolder != null){
				Mailbox_EventArgs eArgs = new Mailbox_EventArgs(mailbox);
				this.DeleteFolder(session,eArgs);

				return eArgs.ErrorText;
			}

			return null;
		}

		#endregion

		#region function OnRenameMailbox

		/// <summary>
		/// Raises event 'RenameMailbox'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="mailbox">Mailbox which to rename.</param>
		/// <param name="newMailboxName">New mailbox name.</param>
		/// <returns></returns>
		internal string OnRenameMailbox(IMAP_Session session,string mailbox,string newMailboxName)
		{
			if(this.RenameFolder != null){
				Mailbox_EventArgs eArgs = new Mailbox_EventArgs(mailbox,newMailboxName);
				this.RenameFolder(session,eArgs);

				return eArgs.ErrorText;
			}

			return null;
		}

		#endregion

		#region function OnGetMessagesInfo

		/// <summary>
		/// Raises event 'GetMessagesInfo'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="mailbox">Mailbox which messages info to get.</param>
		/// <returns></returns>
		internal IMAP_Messages OnGetMessagesInfo(IMAP_Session session,string mailbox)
		{
			IMAP_Messages messages = new IMAP_Messages(mailbox);
			if(this.GetMessagesInfo != null){
				this.GetMessagesInfo(session,messages);
			}

			return messages;
		}

		#endregion

		#region function OnGetMessage

		/// <summary>
		/// Raises event 'GetMessage'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="msg">Message which to get.</param>
		/// <param name="headersOnly">Specifies if message header or full message is wanted.</param>
		/// <returns></returns>
		internal Message_EventArgs OnGetMessage(IMAP_Session session,IMAP_Message msg,bool headersOnly)
		{
			Message_EventArgs eArgs = new Message_EventArgs(session.SelectedMailbox,msg,headersOnly);
			if(this.GetMessage != null){
				this.GetMessage(session,eArgs);
			}

			return eArgs;
		}

		#endregion

		#region function OnDeleteMessage

		/// <summary>
		/// Raises event 'DeleteMessage'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="message">Message which to delete.</param>
		/// <returns></returns>
		internal string OnDeleteMessage(IMAP_Session session,IMAP_Message message)
		{
			Message_EventArgs eArgs = new Message_EventArgs(session.SelectedMailbox,message);
			if(this.DeleteMessage != null){
				this.DeleteMessage(session,eArgs);
			}

			return eArgs.ErrorText;
		}

		#endregion

		#region function OnCopyMessage

		/// <summary>
		/// Raises event 'CopyMessage'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="msg">Message which to copy.</param>
		/// <param name="location">New message location.</param>
		/// <returns></returns>
		internal string OnCopyMessage(IMAP_Session session,IMAP_Message msg,string location)
		{
			Message_EventArgs eArgs = new Message_EventArgs(session.SelectedMailbox,msg,location);
			if(this.CopyMessage != null){
				this.CopyMessage(session,eArgs);
			}

			return eArgs.ErrorText;
		}

		#endregion

		#region function OnStoreMessage

		/// <summary>
		/// Raises event 'StoreMessage'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="folder">Folder where to store.</param>
		/// <param name="msg">Message which to store.</param>
		/// <param name="messageData">Message data which to store.</param>
		/// <returns></returns>
		internal string OnStoreMessage(IMAP_Session session,string folder,IMAP_Message msg,byte[] messageData)
		{
			Message_EventArgs eArgs = new Message_EventArgs(folder,msg);
			eArgs.MessageData = messageData;
			if(this.StoreMessage != null){
				this.StoreMessage(session,eArgs);
			}

			return eArgs.ErrorText;
		}

		#endregion

		#region function OnStoreMessageFlags

		/// <summary>
		/// Raises event 'StoreMessageFlags'.
		/// </summary>
		/// <param name="session">Reference to IMAP session.</param>
		/// <param name="msg">Message which flags to store.</param>
		/// <returns></returns>
		internal string OnStoreMessageFlags(IMAP_Session session,IMAP_Message msg)
		{
			Message_EventArgs eArgs = new Message_EventArgs(session.SelectedMailbox,msg);
			if(this.StoreMessageFlags != null){
				this.StoreMessageFlags(session,eArgs);
			}

			return eArgs.ErrorText;
		}

		#endregion


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
