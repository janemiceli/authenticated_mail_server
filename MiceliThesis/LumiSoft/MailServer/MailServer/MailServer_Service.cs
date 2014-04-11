using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Reflection;
using LumiSoft.Net.POP3.Client;
using LumiSoft.Net.POP3.Server;
using LumiSoft.Net.IMAP.Server;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Mail server service.
	/// </summary>
	public class MailServer : System.ServiceProcess.ServiceBase
	{
		private LumiSoft.Net.POP3.Server.POP3_Server POP3_Server;
		private LumiSoft.Net.SMTP.Server.SMTP_Server SMTP_Server;
		private LumiSoft.Net.IMAP.Server.IMAP_Server IMAP_Server;
		private System.Timers.Timer timer1;
		private System.ComponentModel.IContainer components;

		private  static  ServerAPI m_pAPI  = null;
		private DataSet  dsSettings        = null;
		internal string  m_SartUpPath      = "";	// Path where (this)service relies.	
		private  string  m_DefaultDomain   = "";
		private DB_Type  m_DB_Type         = DB_Type.XML;
		internal string  m_connStr         = "";    // Sql connection string to Mail DB.
		internal string  m_MailStorePath   = "";
		private  string  m_SMTP_LogPath    = "";
		private  string  m_POP3_LogPath    = "";
		private  string  m_IMAP_LogPath    = "";
		private  string  m_Server_LogPath  = "";
		private DateTime m_SettingsDate;            // Holds server Settings.xml date.
		private DateTime m_UsersDate;               // Holds server Users.xml date.
		private DateTime m_AliasesDate;             // Holds server Aliases.xml date.
		private DateTime m_RoutingDate;             // Holds server Routing.xml date.
		private DateTime m_DomainsDate;			    // Holds server Domains.xml date.
		private DateTime m_SecurityDate;		    // Holds server Secuirty.xml date.
		private DateTime m_FiltersDate;			    // Holds server Secuirty.xml date.
		private DateTime m_RelayTime;
		private DateTime m_RelayRetryTime;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public MailServer()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitComponent call

		}

		#region Designer Generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.POP3_Server = new LumiSoft.Net.POP3.Server.POP3_Server(this.components);
			this.IMAP_Server = new LumiSoft.Net.IMAP.Server.IMAP_Server(this.components);
			this.timer1 = new System.Timers.Timer();
			this.SMTP_Server = new LumiSoft.Net.SMTP.Server.SMTP_Server(this.components);
			((System.ComponentModel.ISupportInitialize)(this.timer1)).BeginInit();
			// 
			// POP3_Server
			// 
			this.POP3_Server.CommandIdleTimeOut = 60000;
			this.POP3_Server.LogCommands = false;
			this.POP3_Server.MaxBadCommands = 8;
			this.POP3_Server.SessionIdleTimeOut = 80000;
			this.POP3_Server.SessionResetted += new System.EventHandler(this.poP3_Server1_SessionResetted);
			this.POP3_Server.GetTopLines += new LumiSoft.Net.POP3.Server.GetTopLinesHandler(this.POP3_Server_GetTopLines);
			this.POP3_Server.GetMessage += new LumiSoft.Net.POP3.Server.GetMessageHandler(this.pop3_Server_GetMessage);
			this.POP3_Server.DeleteMessage += new LumiSoft.Net.POP3.Server.DeleteMessageHandler(this.pop3_Server_DeleteMessage);
			this.POP3_Server.SessionEnd += new System.EventHandler(this.poP3_Server1_SessionEnd);
			this.POP3_Server.SysError += new LumiSoft.Net.ErrorEventHandler(this.OnServer_SysError);
			this.POP3_Server.AuthUser += new LumiSoft.Net.POP3.Server.AuthUserEventHandler(this.Server_AuthenticateUser);
			this.POP3_Server.GetMessgesList += new LumiSoft.Net.POP3.Server.GetMessagesInfoHandler(this.pop3_Server_GetMessgesList);
			this.POP3_Server.SessionLog += new LumiSoft.Net.LogEventHandler(this.POP3_Server_SessionLog);
			this.POP3_Server.ValidateIPAddress += new LumiSoft.Net.ValidateIPHandler(this.POP3_Server_ValidateIPAddress);
			// 
			// timer1
			// 
			this.timer1.Interval = 15000;
			this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Elapsed);
			// 
			// SMTP_Server
			// 
			this.SMTP_Server.CommandIdleTimeOut = 60000;
			this.SMTP_Server.LogCommands = false;
			this.SMTP_Server.MaxBadCommands = 8;
			this.SMTP_Server.MaxMessageSize = 1000000;
			this.SMTP_Server.MaxRecipients = 100;
			this.SMTP_Server.SessionIdleTimeOut = 80000;
			this.SMTP_Server.AuthUser += new LumiSoft.Net.SMTP.Server.AuthUserEventHandler(this.Server_AuthenticateUser);
			this.SMTP_Server.SessionLog += new LumiSoft.Net.LogEventHandler(this.SMTP_Server_SessionLog);
			this.SMTP_Server.ValidateMailFrom += new LumiSoft.Net.SMTP.Server.ValidateMailFromHandler(this.smtp_Server_ValidateSender);
			this.SMTP_Server.SysError += new LumiSoft.Net.ErrorEventHandler(this.OnServer_SysError);
			this.SMTP_Server.StoreMessage += new LumiSoft.Net.SMTP.Server.NewMailEventHandler(this.smtp_Server_NewMailEvent);
			this.SMTP_Server.ValidateMailTo += new LumiSoft.Net.SMTP.Server.ValidateMailToHandler(this.smtp_Server_ValidateRecipient);
			this.SMTP_Server.ValidateMailboxSize += new LumiSoft.Net.SMTP.Server.ValidateMailboxSize(this.SMTP_Server_ValidateMailBoxSize);
			this.SMTP_Server.ValidateIPAddress += new LumiSoft.Net.ValidateIPHandler(this.SMTP_Server_ValidateIPAddress);
			//
			//
			//
			this.IMAP_Server.SysError += new LumiSoft.Net.ErrorEventHandler(OnServer_SysError);
			this.IMAP_Server.AuthUser += new LumiSoft.Net.IMAP.Server.AuthUserEventHandler(IMAP_Server_AuthUser);
			this.IMAP_Server.GetFolders += new FoldersEventHandler(IMAP_Server_GetFolders);
			this.IMAP_Server.GetSubscribedFolders += new FoldersEventHandler(IMAP_Server_GetSubscribedFolders);
			this.IMAP_Server.SubscribeFolder += new FolderEventHandler(IMAP_Server_SubscribeFolder);
			this.IMAP_Server.UnSubscribeFolder += new FolderEventHandler(IMAP_Server_UnSubscribeFolder);
			this.IMAP_Server.CreateFolder += new FolderEventHandler(IMAP_Server_CreateFolder);
			this.IMAP_Server.DeleteFolder += new FolderEventHandler(IMAP_Server_DeleteFolder);
			this.IMAP_Server.RenameFolder += new FolderEventHandler(IMAP_Server_RenameFolder);
			this.IMAP_Server.GetMessagesInfo += new MessagesEventHandler(IMAP_Server_GetMessagesInfo);
			this.IMAP_Server.DeleteMessage += new MessageEventHandler(IMAP_Server_DeleteMessage);
			this.IMAP_Server.StoreMessage += new MessageEventHandler(IMAP_Server_StoreMessage);
			this.IMAP_Server.StoreMessageFlags += new MessageEventHandler(IMAP_Server_StoreMessageFlags);
			this.IMAP_Server.CopyMessage += new MessageEventHandler(IMAP_Server_CopyMessage);
			this.IMAP_Server.GetMessage += new MessageEventHandler(IMAP_Server_GetMessage);
			this.IMAP_Server.SessionLog += new LumiSoft.Net.LogEventHandler(this.IMAP_Server_SessionLog);
			// 
			// MailServer
			// 
			this.ServiceName = "Service1";
			((System.ComponentModel.ISupportInitialize)(this.timer1)).EndInit();

		}

		#endregion

		#region function Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{			
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion


		#region function OnStart

		internal void Start()
		{
			OnStart(null);
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{						
			try
			{				
				string filePath     = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
				m_SartUpPath        = filePath.Substring(0,filePath.LastIndexOf('\\')) + "\\";
				Error.ErrorFilePath = m_SartUpPath;
								
				m_pAPI = new ServerAPI(m_SartUpPath + "Settings\\");

				LoadSettings();

				// If don't use sql, load Users and Domains from xml
				if(m_DB_Type == DB_Type.XML){
					LoadLocalDomains();
					LoadLocalUsers();
					LoadLocalAliases();
					LoadRoutes();
					LoadLocalSecurity();
					LoadLocalFilters();
				}

				timer1.Enabled = true;

				m_RelayTime      = DateTime.Today;
				m_RelayRetryTime = DateTime.Today;

				// Logging stuff
                SCore.WriteLog(m_Server_LogPath + "server.log","//---- Server started " + DateTime.Now);
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}						
		}

		#endregion

		#region function OnStop
 
		internal void Stop()
		{
			OnStop();
		}

		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{			
			SMTP_Server.Enabled = false;
			POP3_Server.Enabled = false;
			IMAP_Server.Enabled = false;

			timer1.Enabled = false;

			// Logging stuff
			SCore.WriteLog(m_Server_LogPath + "server.log","//---- Server stopped " + DateTime.Now);
		}

		#endregion


		#region SMTP Events

		//------ SMTP Events -----------------------//

		#region function SMTP_Server_ValidateIPAddress

		private void SMTP_Server_ValidateIPAddress(object sender,LumiSoft.Net.ValidateIP_EventArgs e)
		{
			try
			{		
				string IP = Ip_to_longStr(e.ConnectedIP);
				e.Validated = m_pAPI.IsSmtpAccessAllowed(IP);
			}
			catch(Exception x)
			{
				e.Validated = false;
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function Server_AuthenticateUser

		private void Server_AuthenticateUser(object sender,LumiSoft.Net.SMTP.Server.AuthUser_EventArgs e)
		{	
			try
			{
				e.Validated = m_pAPI.AuthUser(e.UserName,e.PasswData,e.AuthData,e.AuthType);
			}
			catch(Exception x)
			{
				e.Validated = false;
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function smtp_Server_ValidateSender

		private void smtp_Server_ValidateSender(object sender,LumiSoft.Net.SMTP.Server.ValidateSender_EventArgs e)
		{
			e.Validated = true;	
		}

		#endregion

		#region function smtp_Server_ValidateRecipient

		private void smtp_Server_ValidateRecipient(object sender,LumiSoft.Net.SMTP.Server.ValidateRecipient_EventArgs e)
		{
			try
			{
				e.Validated = false;

				string mailTo = e.MailTo;

				// If domain isn't specified, add default domain
				if(mailTo.IndexOf("@") == -1){
					mailTo += "@" + m_DefaultDomain;
				}

				//1) is local domain or relay needed
				//2) can map email address to mailbox
				//3) is alias
				//4) if matches any routing pattern

				// check if e-domain is local
				if(m_pAPI.DomainExists(mailTo)){
					if(m_pAPI.MapUser(mailTo) == null){
						// Check if alias.
						if(m_pAPI.GetAliasMembers(mailTo) != null){
							// Always allow authenticated users to use aliases
							if(e.Authenticated){							
								e.Validated = true;
							}
							// For not authenticated users, allow to use only public aliases
							else if(m_pAPI.IsAliasPublic(mailTo)){
								e.Validated = true;
							}
						}
						// At least check if matches any routing pattern.
						else if(m_pAPI.GetMailboxFromPattern(mailTo) != null){
							e.Validated = true;
						}
					}
					else{
						e.Validated = true;
					}
				}
				// Not this server recipient
				else{
					e.LocalRecipient = false;

					// This isn't domain what we want.
					// 1)If user Authenticated, check if relay is allowed for this user.
					// 2)Check if relay is allowed for this ip.
					if(e.Authenticated){
						if(m_pAPI.IsRelayAllowedUser(e.Session.UserName)){
							e.Validated = true;
						}
					}
					else if(m_pAPI.IsRelayAllowedIP(Ip_to_longStr(e.ConnectedIP))){
						e.Validated = true;
					}		
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function SMTP_Server_ValidateMailBoxSize

		private void SMTP_Server_ValidateMailBoxSize(object sender,LumiSoft.Net.SMTP.Server.ValidateMailboxSize_EventArgs e)
		{
			e.IsValid = false;

			try
			{
				string emailAddress = e.eAddress;

				// If domain isn't specified, add default domain
				if(emailAddress.IndexOf("@") == -1){
					emailAddress += "@" + m_DefaultDomain;
				}

				// If relay or alias address, don't check size
				if(!m_pAPI.DomainExists(emailAddress) || m_pAPI.GetAliasMembers(emailAddress) != null){
					e.IsValid = true;
					return;
				}
				//--------------------------------//

				string user = m_pAPI.MapUser(emailAddress);
				if(user == null){
					string routeMailbox = m_pAPI.GetMailboxFromPattern(emailAddress);

					// If must route message to remote address, don't check size
					if(routeMailbox != null){
						if(routeMailbox.ToUpper().StartsWith("REMOTE:")){
							e.IsValid = true;
							return;
						}
						else{
							user = routeMailbox;
						}
					}
				}

				if(user != null){
					e.IsValid = !m_pAPI.ValidateMailboxSize(user);
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion


		#region function smtp_Server_NewMailEvent

		private void smtp_Server_NewMailEvent(object sender,LumiSoft.Net.SMTP.Server.NewMail_EventArgs e)
		{	
			try
			{
				ProcessAndStoreMessage(e.MailFrom,e.MailTo,e.MessageStream);
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
				throw new Exception("Error storing message");
			}
		}

		internal void ProcessAndStoreMessage(string sender,string[] recipient,MemoryStream msgStream)
		{
			
			Hashtable forward_pathList = new Hashtable();
			//--------------------------------------------------------------//
			// Construct new forward-path list.(because of aliases, must be replaced with actual recipients)			
			foreach(string forward_path in recipient){
				string emailAddress = forward_path;

				// If domain isn't specified, add default domain
				if(emailAddress.IndexOf("@") == -1){
					emailAddress += "@" + m_DefaultDomain;
				}

				string[] aliasMembers = m_pAPI.GetAliasMembers(emailAddress);
				if(aliasMembers != null){					
					foreach(string member in aliasMembers){
						if(!forward_pathList.Contains(member)){
							forward_pathList.Add(member,member);								
						}							
					}						
				}
				else{
					if(!forward_pathList.Contains(emailAddress)){
						forward_pathList.Add(emailAddress,emailAddress);
					}
				}
			}
			//------------------------------------------------------------//

			#region Filtering suff

			//--- Filter message -----------------------------------------------//
			MemoryStream filteredMsgStream = msgStream;
			DataView dvFilters = m_pAPI.GetFilterList();
			dvFilters.Sort = "Cost";
			dvFilters.RowFilter = "Enabled=true";
			foreach(DataRowView drViewFilter in dvFilters){
				string assemblyFile = drViewFilter.Row["Assembly"].ToString();
				// File is without path probably, try to load it from filters folder
				if(!File.Exists(assemblyFile)){
					assemblyFile = m_SartUpPath + "\\Filters\\" + assemblyFile;
				}

				Assembly ass = Assembly.LoadFrom(assemblyFile);
				Type tp = ass.GetType(drViewFilter.Row["ClassName"].ToString());
				object filterInstance = Activator.CreateInstance(tp);
				ISmtpMessageFilter filter = (ISmtpMessageFilter)filterInstance;
										
				FilterResult result = filter.Filter(filteredMsgStream,out filteredMsgStream,sender,recipient,m_pAPI);
				switch(result)
				{
					case FilterResult.DontStore:
						return; // Just skip messge, act as message is stored

					case FilterResult.Error:
						// ToDO: Add implementaion here or get rid of it (use exception instead ???).
						return;
				}
			}
			//---------------------------------------------------------------//
			#endregion
							
			// Store individual msg for each Recipient
			foreach(string forward_path in forward_pathList.Values){
				string eAddress = forward_path;
				string mailbox  = m_pAPI.MapUser(eAddress);
				bool   relay    = false;
	
				// if not valid mailbox
				if(mailbox == null){
					// check e-domain, if it's local.
					if(m_pAPI.DomainExists(eAddress)){
						// Check if eAddress matches routing pattern
						string routeMailBox = m_pAPI.GetMailboxFromPattern(eAddress);

						//--- Check if mailbox is remote(destination is other mail server)
						if(routeMailBox != null){
							if(routeMailBox.ToUpper().StartsWith("REMOTE:")){
								eAddress = routeMailBox.Substring(7);
								relay = true;
							}
							else{
								mailbox = routeMailBox;
							}
						}
						//-----------------------------------------------------------------

						// If there isn't valid mailbox, some error has happened.
						// (One known reason is when user,alias,routing is deleted between RCPT and DATA)
						// Forward message to postmaster.
						if(mailbox == null){
							mailbox = "postmaster";
						}
					}
					// This isn't domain what we want, relay message.
					else{
						relay = true;
					}
				}

				MailServer.API.StoreMessage(mailbox,"Inbox",filteredMsgStream,eAddress,sender,relay,DateTime.Now,IMAP_MessageFlags.Recent);
			}
			
		}

		#endregion


		#region function SMTP_Server_SessionLog

		private void SMTP_Server_SessionLog(object sender,LumiSoft.Net.Log_EventArgs e)
		{
			DateTime today = DateTime.Today;
			string fileName = "smtpCmds_" + today.Year.ToString() + "_" + today.Month.ToString() + "_" + today.Day.ToString() + ".log";
			SCore.WriteLog(m_SMTP_LogPath + fileName,e.LogText);
		}

		#endregion

		//------ End of SMTP Events ----------------//

		#endregion		

		#region POP3 Events

		//------ POP3 Events ----------------------//

		#region function POP3_Server_ValidateIPAddress

		private void POP3_Server_ValidateIPAddress(object sender,LumiSoft.Net.ValidateIP_EventArgs e)
		{
			try
			{		
				string IP = Ip_to_longStr(e.ConnectedIP);
				e.Validated = m_pAPI.IsPop3AccessAllowed(IP);
			}
			catch(Exception x)
			{
				e.Validated = false;
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function Server_AuthenticateUser

		private void Server_AuthenticateUser(object sender,LumiSoft.Net.POP3.Server.AuthUser_EventArgs e)
		{	
			try
			{
				e.Validated = m_pAPI.AuthUser(e.UserName,e.PasswData,e.AuthData,e.AuthType);
				if(e.Validated == false){
					return;
				}

				//----- Remote pop3 servers ---------------------------------------//
				DataSet ds = m_pAPI.GetUserRemotePop3Servers(e.UserName);

				// Connect to external pop3 servers
				ArrayList extrnPop3Servers = new ArrayList();
				foreach(DataRow dr in ds.Tables["RemotePop3Servers"].Rows){
					try
					{
						string server = dr["Server"].ToString();
						int    port   = Convert.ToInt32(dr["Port"]);
						string user   = dr["UserName"].ToString();
						string passw  = dr["Password"].ToString();
					
						POP3_Client clnt = new POP3_Client();
						clnt.Connect(server,port);
						clnt.Authenticate(user,passw,false);
				          
						extrnPop3Servers.Add(clnt);
					}
					catch(Exception x){
					}
				}

				if(extrnPop3Servers.Count > 0){
					e.Session.Tag = extrnPop3Servers;
				}
				//---------------------------------------------------------------------//
			}
			catch(Exception x)
			{
				e.Validated = false;
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		
		#region function pop3_Server_GetMessgesList

		private void pop3_Server_GetMessgesList(object sender,LumiSoft.Net.POP3.Server.GetMessagesInfo_EventArgs e)
		{
			try
			{
				string userName = e.UserName;

				MailServer.API.GetMessageList(userName,e.Messages);

				//--------- Remote pop3 server -----------------------------------------------------------------------//
				// Get external pop3 server messages list, append it to local list.
				if(e.Session.Tag != null){
					ArrayList extrnPop3Servers = (ArrayList)e.Session.Tag;
					foreach(POP3_Client clnt in extrnPop3Servers){
						POP3_MessagesInfo mInf = clnt.GetMessagesInfo();				
						foreach(POP3_MessageInfo msg in mInf.Messages){
							// Message tag structure: pop3Client,messageNr in remote pop3 server.
							e.Messages.AddMessage(msg.MessegeID,(int)msg.MessageSize,new object[]{clnt,msg.MessageNr});
						}				
					}
				}
				//----------------------------------------------------------------------------------------------------//
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function pop3_Server_GetMessage

		private void pop3_Server_GetMessage(object sender,LumiSoft.Net.POP3.Server.GetMessage_EventArgs e)
		{	
			/* If there is something stored to Tag, this means that message is reomte server message.
			 * Message tag structure: pop3Client,messageNr in remote pop3 server.			  
			*/

			try
			{
				string mailbox = e.UserName;
				string msgID = e.MessageID;

				// Local message wanted
				if(e.Message.Tag == null){
					e.MessageData = MailServer.API.GetMessage(mailbox,"Inbox",msgID);
				}
				// Get remote message
				else{				
					object[] tag = (object[])e.Message.Tag;

					POP3_Client clnt = (POP3_Client)tag[0];
					e.MessageData = clnt.GetMessage(Convert.ToInt32(tag[1]));
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function pop3_Server_DeleteMessage

		private void pop3_Server_DeleteMessage(object sender,LumiSoft.Net.POP3.Server.DeleteMessage_EventArgs e)
		{
			/* If there is something stored to Tag, this means that message is reomte server message.
			 * Message tag structure: pop3Client,messageNr in remote pop3 server.			  
			*/

			try
			{
				if(e.Message.Tag == null){
					MailServer.API.DeleteMessage(e.UserName,"Inbox",e.MessageID);
				}
				// Delete remote message
				else{			
					object[] tag = (object[])e.Message.Tag;

					POP3_Client clnt = (POP3_Client)tag[0];
					clnt.DeleteMessage(Convert.ToInt32(tag[1]));
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function POP3_Server_GetTopLines

		private void POP3_Server_GetTopLines(object sender,LumiSoft.Net.POP3.Server.GetTopLines_Eventargs e)
		{		
			/* If there is something stored to Tag, this means that message is reomte server message.
			 * Message tag structure: pop3Client,messageNr in remote pop3 server.			  
			*/

			try
			{
				// Get local message top lines
				if(e.Message.Tag == null){
					e.LinesData = m_pAPI.GetMessageTopLines(e.Mailbox,"Inbox",e.MessageID,e.Lines);
				}
				// Get remote message top lines
				else{			
					object[] tag = (object[])e.Message.Tag;

					POP3_Client clnt = (POP3_Client)tag[0];
					e.LinesData = clnt.GetTopOfMessage(Convert.ToInt32(tag[1]),e.Lines);
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}			
		}

		#endregion


		#region function poP3_Server1_SessionResetted

		private void poP3_Server1_SessionResetted(object sender, System.EventArgs e)
		{
			// Reset all pop3 reomte sessions.
			POP3_Session session = (POP3_Session)sender;
			if(session.Tag != null){
				ArrayList extrnPop3Servers = (ArrayList)session.Tag;
				foreach(POP3_Client clnt in extrnPop3Servers){
					clnt.Reset();
				}
			}
		}
		
		#endregion

		#region function poP3_Server1_SessionEnd

		private void poP3_Server1_SessionEnd(object sender, System.EventArgs e)
		{
			// End all reomte pop3 server sessions.
			POP3_Session session = (POP3_Session)sender;
			if(session.Tag != null){
				ArrayList extrnPop3Servers = (ArrayList)session.Tag;
				foreach(POP3_Client clnt in extrnPop3Servers){
					clnt.Disconnect();
				}
			}
		}

		#endregion


		#region function POP3_Server_SessionLog

		private void POP3_Server_SessionLog(object sender,LumiSoft.Net.Log_EventArgs e)
		{
			DateTime today = DateTime.Today;
			string fileName = "pop3Cmds_" + today.Year.ToString() + "_" + today.Month.ToString() + "_" + today.Day.ToString() + ".log";
			SCore.WriteLog(m_POP3_LogPath + fileName,e.LogText);
		}

		#endregion

		//------ End of POP3 Events ---------------//
		
		#endregion

		#region IMAP Events

		#region function IMAP_Server_AuthUser

		private void IMAP_Server_AuthUser(object sender,LumiSoft.Net.IMAP.Server.AuthUser_EventArgs e)
		{
			try
			{
				e.Validated = m_pAPI.AuthUser(e.UserName,e.PasswData,e.AuthData,e.AuthType);
			}
			catch(Exception x){
				e.Validated = false;
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function IMAP_Server_GetFolders

		private void IMAP_Server_GetFolders(object sender, IMAP_Folders e)
		{
			IMAP_Session ses = (IMAP_Session)sender;

			string[] folders = m_pAPI.GetFolders(ses.UserName);
			foreach(string folder in folders){
				e.Add(folder);
			}
		}

		#endregion

		#region function IMAP_Server_GetSubscribedFolders

		private void IMAP_Server_GetSubscribedFolders(object sender, IMAP_Folders e)
		{
			IMAP_Session ses = (IMAP_Session)sender;

			string[] folders = m_pAPI.GetSubscribedFolders(ses.UserName);
			foreach(string folder in folders){
				e.Add(folder);
			}
		}

		#endregion

		#region function IMAP_Server_SubscribeFolder

		private void IMAP_Server_SubscribeFolder(object sender, Mailbox_EventArgs e)
		{
			IMAP_Session ses = (IMAP_Session)sender;

			m_pAPI.SubscribeFolder(ses.UserName,e.Folder);
		}

		#endregion

		#region function IMAP_Server_UnSubscribeFolder

		private void IMAP_Server_UnSubscribeFolder(object sender, Mailbox_EventArgs e)
		{
			IMAP_Session ses = (IMAP_Session)sender;

			m_pAPI.UnSubscribeFolder(ses.UserName,e.Folder);
		}

		#endregion

		#region function IMAP_Server_CreateFolder

		private void IMAP_Server_CreateFolder(object sender, Mailbox_EventArgs e)
		{
			try
			{
				IMAP_Session ses = (IMAP_Session)sender;

				m_pAPI.CreateFolder(ses.UserName,e.Folder);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region function IMAP_Server_DeleteFolder

		private void IMAP_Server_DeleteFolder(object sender, Mailbox_EventArgs e)
		{
			try
			{
				IMAP_Session ses = (IMAP_Session)sender;

				m_pAPI.DeleteFolder(ses.UserName,e.Folder);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region function IMAP_Server_RenameFolder

		private void IMAP_Server_RenameFolder(object sender, Mailbox_EventArgs e)
		{
			try
			{
				IMAP_Session ses = (IMAP_Session)sender;

				m_pAPI.RenameFolder(ses.UserName,e.Folder,e.NewFolder);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region function IMAP_Server_GetMessagesInfo

		private void IMAP_Server_GetMessagesInfo(object sender, IMAP_Messages e)
		{
			try
			{
				IMAP_Session ses = (IMAP_Session)sender;

				m_pAPI.GetMessagesInfo(ses.UserName,e.Mailbox,e);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region function IMAP_Server_GetMessage

		private void IMAP_Server_GetMessage(object sender, Message_EventArgs e)
		{
			IMAP_Session ses = (IMAP_Session)sender;

			if(!e.HeadersOnly){
				e.MessageData = m_pAPI.GetMessage(ses.UserName,ses.SelectedMailbox,e.Message.MessageID);
			}
			else{
				e.MessageData = m_pAPI.GetMessageTopLines(ses.UserName,e.Folder,e.Message.MessageID,0);
			}
		}

		#endregion

		#region function IMAP_Server_DeleteMessage

		private void IMAP_Server_DeleteMessage(object sender, Message_EventArgs e)
		{
			try
			{
				IMAP_Session ses = (IMAP_Session)sender;

				m_pAPI.DeleteMessage(ses.UserName,ses.SelectedMailbox,e.Message.MessageID);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region function IMAP_Server_CopyMessage

		private void IMAP_Server_CopyMessage(object sender, Message_EventArgs e)
		{
			try
			{
				IMAP_Session ses = (IMAP_Session)sender;

				m_pAPI.CopyMessage(ses.UserName,ses.SelectedMailbox,e.CopyLocation,e.Message);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region function IMAP_Server_StoreMessage

		private void IMAP_Server_StoreMessage(object sender, Message_EventArgs e)
		{
			try
			{
				IMAP_Session ses = (IMAP_Session)sender;

				m_pAPI.StoreMessage(ses.UserName,e.Folder,new MemoryStream(e.MessageData),"","",false,DateTime.Now,e.Message.Flags);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region function IMAP_Server_StoreMessageFlags

		private void IMAP_Server_StoreMessageFlags(object sender, Message_EventArgs e)
		{
			IMAP_Session ses = (IMAP_Session)sender;

			m_pAPI.StoreMessageFlags(ses.UserName,ses.SelectedMailbox,e.Message);
		}

		#endregion


		#region function IMAP_Server_SessionLog

		private void IMAP_Server_SessionLog(object sender,LumiSoft.Net.Log_EventArgs e)
		{
			DateTime today = DateTime.Today;
			string fileName = "imapCmds_" + today.Year.ToString() + "_" + today.Month.ToString() + "_" + today.Day.ToString() + ".log";
			SCore.WriteLog(m_IMAP_LogPath + fileName,e.LogText);
		}

		#endregion

		#endregion

		#region Common Events

		#region function OnServer_SysError

		private void OnServer_SysError(object sender,LumiSoft.Net.Error_EventArgs e)
		{
			Error.DumpError(e.Exception,e.StackTrace);
		}

		#endregion

		#endregion

				
		#region Service Timer
		
		private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{	
				//------------ local settings --------------------------------------------------------------//
				DateTime dateSettings = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Settings.xml");

				if(DateTime.Compare(dateSettings,m_SettingsDate) != 0){
					LoadSettings();						
				}

				if(m_DB_Type == DB_Type.XML){					
					DateTime dateUsers    = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Users.xml");
					DateTime dateAliases  = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Aliases.xml");
					DateTime dateDomains  = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Domains.xml");
					DateTime dateRouting  = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Routing.xml");
					DateTime dateSecurity = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Security.xml");
					DateTime dateFilters  = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Filters.xml");
					
					if(DateTime.Compare(dateDomains,m_DomainsDate) != 0){
						LoadLocalDomains();						
					}
					if(DateTime.Compare(dateUsers,m_UsersDate) != 0){
						LoadLocalUsers();						
					}
					if(DateTime.Compare(dateAliases,m_AliasesDate) != 0){	
						LoadLocalAliases();						
					}
					if(DateTime.Compare(dateRouting,m_RoutingDate) != 0){	
						LoadRoutes();						
					}
					if(DateTime.Compare(dateSecurity,m_SecurityDate) != 0){
						LoadLocalSecurity();						
					}
					if(DateTime.Compare(dateFilters,m_FiltersDate) != 0){
						LoadLocalFilters();						
					}
				}
				//------------------------------------------------------------------------------------------//

				//---- Relay stuff ----------------------------------------------------------------------------//

				if(DateTime.Now.CompareTo(m_RelayTime.AddSeconds(Relay.RelayInterval)) >= 0 && !Relay.IsDelivering){
					Relay relay = new Relay(this);

					// Create New Thread for Mail Delivery handling
					ThreadStart tStart = new ThreadStart(relay.Deliver);
					Thread      tr     = new Thread(tStart);
					tr.Priority        = ThreadPriority.Lowest;
					tr.Start();

					m_RelayTime = DateTime.Now;
				}
				
				if(DateTime.Now.CompareTo(m_RelayRetryTime.AddSeconds(Relay.RelayRetryInterval)) >= 0 && !Relay.IsDeliveringRetry){
					Relay relay = new Relay(this);

					// Create New Thread for Mail Delivery handling
					ThreadStart tStart = new ThreadStart(relay.DeliverRetry);
					Thread      tr     = new Thread(tStart);
					tr.Priority        = ThreadPriority.Lowest;
					tr.Start();

					m_RelayRetryTime = DateTime.Now;
				}
				//----------------------------------------------------------------------------------------------//
				
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion


		#region Local settings loading stuff

		#region function LoadSettings

		private void LoadSettings()
		{
			try
			{
				lock(this){
					m_pAPI.DatabaseTypeChanged();

					dsSettings = m_pAPI.GetSettings();
					m_SettingsDate = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Settings.xml");

					DataRow dr = dsSettings.Tables["Settings"].Rows[0];
					m_connStr             = dr["ConnectionString"].ToString();
					m_DB_Type             = (DB_Type)Enum.Parse(typeof(DB_Type),dr["DataBaseType"].ToString());
					string mailStorePath  = dr["MailRoot"].ToString();
					if(!mailStorePath.EndsWith("\\")){
						mailStorePath += "\\";
					}
					if(mailStorePath.Length < 3){
						mailStorePath = m_SartUpPath + "MailStore\\";
					}
					m_MailStorePath = mailStorePath;

					//------- SMTP Settings ---------------------------------------------//
					SMTP_Server.IpAddress          = dr["SMTP_IPAddress"].ToString();
					SMTP_Server.Port               = Convert.ToInt32(dr["SMTP_Port"]);
					SMTP_Server.Threads            = Convert.ToInt32(dr["SMTP_Threads"]);
					SMTP_Server.SessionIdleTimeOut = Convert.ToInt32(dr["SMTP_SessionIdleTimeOut"]) * 1000; // Seconds to milliseconds
					SMTP_Server.CommandIdleTimeOut = Convert.ToInt32(dr["SMTP_CommandIdleTimeOut"]) * 1000; // Seconds to milliseconds
					SMTP_Server.MaxMessageSize     = Convert.ToInt32(dr["MaxMessageSize"]) * 1000000;       // Mb to byte.
					SMTP_Server.MaxRecipients      = Convert.ToInt32(dr["MaxRecipients"]);
					SMTP_Server.MaxBadCommands     = Convert.ToInt32(dr["SMTP_MaxBadCommands"]);
					SMTP_Server.Enabled            = Convert.ToBoolean(dr["SMTP_Enabled"]);

					m_DefaultDomain   = dr["SMTP_DefaultDomain"].ToString();
					//-------------------------------------------------------------------//

					//------- POP3 Settings -------------------------------------//
					POP3_Server.IpAddress          = dr["POP3_IPAddress"].ToString();
					POP3_Server.Port               = Convert.ToInt32(dr["POP3_Port"]);				
					POP3_Server.Threads            = Convert.ToInt32(dr["POP3_Threads"]);
					POP3_Server.SessionIdleTimeOut = Convert.ToInt32(dr["POP3_SessionIdleTimeOut"]) * 1000; // Seconds to milliseconds
					POP3_Server.CommandIdleTimeOut = Convert.ToInt32(dr["POP3_CommandIdleTimeOut"]) * 1000; // Seconds to milliseconds
					POP3_Server.MaxBadCommands     = Convert.ToInt32(dr["POP3_MaxBadCommands"]);					
					POP3_Server.Enabled            = Convert.ToBoolean(dr["POP3_Enabled"]);
					//-----------------------------------------------------------//

					//------- IMAP Settings -------------------------------------//
					IMAP_Server.IpAddress          = dr["IMAP_IPAddress"].ToString();
					IMAP_Server.Port               = Convert.ToInt32(dr["IMAP_Port"]);				
					IMAP_Server.Threads            = Convert.ToInt32(dr["IMAP_Threads"]);
					IMAP_Server.SessionIdleTimeOut = Convert.ToInt32(dr["IMAP_SessionIdleTimeOut"]) * 1000; // Seconds to milliseconds
					IMAP_Server.CommandIdleTimeOut = Convert.ToInt32(dr["IMAP_CommandIdleTimeOut"]) * 1000; // Seconds to milliseconds
					IMAP_Server.MaxBadCommands     = Convert.ToInt32(dr["IMAP_MaxBadCommands"]);					
					IMAP_Server.Enabled            = Convert.ToBoolean(dr["IMAP_Enabled"]);
					//-----------------------------------------------------------//

				
					//------- Delivery ------------------------------------------//
					Relay.SmartHost                = dr["SmartHost"].ToString();				
					Relay.UseSmartHost             = Convert.ToBoolean(dr["UseSmartHost"]);
					Relay.Dns1                     = dr["Dns1"].ToString();
					Relay.Dns2                     = dr["Dns2"].ToString();
					Relay.RelayInterval            = Convert.ToInt32(dr["RelayInterval"]);
					Relay.RelayRetryInterval       = Convert.ToInt32(dr["RelayRetryInterval"]);
					Relay.RelayUndelWarning        = Convert.ToInt32(dr["RelayUndeliveredWarning"]);
					Relay.RelayUndelivered         = Convert.ToInt32(dr["RelayUndelivered"]);				
					Relay.MaxRelayThreads          = Convert.ToInt32(dr["MaxRelayThreads"]);
					Relay.UndeliveredTemplate      = dr["UndeliveredTemplate"].ToString();
					Relay.UndelWarningTemplate     = dr["UndeliveredWarningTemplate"].ToString();
					Relay.StoreUndeliveredMessages = Convert.ToBoolean(dr["StoreUndeliveredMessages"]);
					//-----------------------------------------------------------//

					//----- Logging settings -------------------------------------//
					SMTP_Server.LogCommands = Convert.ToBoolean(dr["LogSMTPCmds"]);
					POP3_Server.LogCommands = Convert.ToBoolean(dr["LogPOP3Cmds"]);
					IMAP_Server.LogCommands = Convert.ToBoolean(dr["LogIMAPCmds"]);

					m_SMTP_LogPath   = dr["SMTP_LogPath"].ToString()   + "\\";
					m_POP3_LogPath   = dr["POP3_LogPath"].ToString()   + "\\";
					m_IMAP_LogPath   = dr["IMAP_LogPath"].ToString()   + "\\";
					m_Server_LogPath = dr["Server_LogPath"].ToString() + "\\";
					
					//----- If no log path, use default ----------------
					if(dr["SMTP_LogPath"].ToString().Trim().Length == 0){
						m_SMTP_LogPath = m_SartUpPath + "Logs\\SMTP\\";
					}
					if(dr["POP3_LogPath"].ToString().Trim().Length == 0){
						m_POP3_LogPath = m_SartUpPath + "Logs\\POP3\\";
					}
					if(dr["IMAP_LogPath"].ToString().Trim().Length == 0){
						m_IMAP_LogPath = m_SartUpPath + "Logs\\IMAP\\";
					}
					if(dr["Server_LogPath"].ToString().Trim().Length == 0){
						m_Server_LogPath = m_SartUpPath + "Logs\\Server\\";
					}
					//------------------------------------------------------------//

					SCore.WriteLog(m_Server_LogPath + "server.log","//---- Server settings loaded " + DateTime.Now);
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function LoadLocalDomains

		private void LoadLocalDomains()
		{
			try
			{
				lock(this){
					m_pAPI.LoadDomains();
					m_DomainsDate = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Domains.xml");
					
					SCore.WriteLog(m_Server_LogPath + "server.log","//---- Local Domains loaded " + DateTime.Now);
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function LoadLocalUsers

		private void LoadLocalUsers()
		{
			try
			{
				lock(this){
					m_pAPI.LoadUsers();
					m_UsersDate = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Users.xml");

					SCore.WriteLog(m_Server_LogPath + "server.log","//---- Local Users loaded " + DateTime.Now);
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function LoadLocalAliases

		private void LoadLocalAliases()
		{
			try
			{
				lock(this){
					m_pAPI.LoadAliases();
					m_AliasesDate = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Aliases.xml");

					SCore.WriteLog(m_Server_LogPath + "server.log","//---- Local Aliases loaded " + DateTime.Now);
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function LoadRoutes

		private void LoadRoutes()
		{
			try
			{
				lock(this){
					m_pAPI.LoadRouting();
					m_RoutingDate = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Routing.xml");

					SCore.WriteLog(m_Server_LogPath + "server.log","//---- Local Routes loaded " + DateTime.Now);
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function LoadLocalSecurity

		private void LoadLocalSecurity()
		{
			try
			{
				lock(this){
					m_pAPI.LoadSecurity();
					m_SecurityDate = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Security.xml");

					SCore.WriteLog(m_Server_LogPath + "server.log","//---- Local Security loaded " + DateTime.Now);
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function LoadLocalFilters

		private void LoadLocalFilters()
		{
			try
			{
				lock(this){
					m_pAPI.LoadFilters();
					m_FiltersDate = File.GetLastWriteTime(m_SartUpPath + "\\Settings\\Filters.xml");

					SCore.WriteLog(m_Server_LogPath + "server.log","//---- Local Filters loaded " + DateTime.Now);
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#endregion
		

		#region function Ip_to_longStr

		/// <summary>
		/// Removes points from ip and fill all blocks eg.(10.0.0.1 = 10 000 000 001).
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		private string Ip_to_longStr(string ip)
		{
			try
			{
				string retVal = "";

				string[] str = ip.Split(new char[]{'.'});

				// loop through all ip blocks.
				foreach(string ipBlock in str){
					string buff = ipBlock;
					// If block size is smaller than 3, append '0' at the beginning of string.
					if(ipBlock.Length < 3){
						for(int i=0;i<3;i++){
							if(buff.Length >= 3){
								break;
							}
							buff = "0" + buff;
						}
					}

					retVal += buff;
				}

				return retVal;
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
				return "";
			}
		}

		#endregion

		
		#region Properties Implementation

		/// <summary>
		/// 
		/// </summary>
		internal static ServerAPI API
		{
			get{ return m_pAPI; }
		}

		#endregion
				
	}
}
