using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.Threading;

using LumiSoft.Net;
using LumiSoft.Net.Mime;
using LumiSoft.Net.SMTP.Server;
using LumiSoft.Net.POP3.Client;
using LumiSoft.Net.POP3.Server;
using LumiSoft.Net.IMAP;
using LumiSoft.Net.IMAP.Server;
using LumiSoft.MailServer.Relay;
using LumiSoft.Net.SIP;
using LumiSoft.Net.SIP.Stack;
using LumiSoft.Net.SIP.Proxy;

namespace LumiSoft.MailServer
{
    /// <summary>
    /// Implements mail server virtual server.
    /// </summary>
    public class VirtualServer
    {
        private Server              m_pOwnerServer       = null;
        private string              m_ID                 = "";
        private string              m_Name               = "";
        private string              m_ApiInitString      = "";
        private IMailServerApi      m_pApi               = null;
        private bool                m_Running            = false;
        private SMTP_Server         m_pSmtpServer        = null;
        private POP3_Server         m_pPop3Server        = null;
        private IMAP_Server         m_pImapServer        = null;
        private Relay_Server        m_pRelayServer       = null;
        private SIP_Stack           m_pSipServer         = null;
        private FetchPop3           m_pFetchServer       = null;
        private RecycleBinManager   m_pRecycleBinManager = null;
        private BadLoginManager     m_pBadLoginManager   = null;
        private System.Timers.Timer m_pTimer             = null;
        // Settings
        private DateTime                     m_SettingsDate;
        private string                       m_MailStorePath      = "";
        private MailServerAuthType_enum      m_AuthType           = MailServerAuthType_enum.Integrated;
        private string                       m_Auth_Win_Domain    = "";
        private bool                         m_SMTP_RequireAuth   = false;
        private string                       m_SMTP_DefaultDomain = "";        
        private string                       m_Server_LogPath     = "";
		private string                       m_SMTP_LogPath       = "";
		private string                       m_POP3_LogPath       = "";
		private string                       m_IMAP_LogPath       = "";
        private string                       m_Relay_LogPath      = "";
		private string                       m_Fetch_LogPath      = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="server">Server what owns this virtual server.</param>
        /// <param name="id">Virtual server ID.</param>
        /// <param name="name">Virtual server name.</param>
        /// <param name="apiInitString">Virtual server api initi string.</param>
        /// <param name="api">Virtual server API.</param>
        public VirtualServer(Server server,string id,string name,string apiInitString,IMailServerApi api)
        {
            m_pOwnerServer  = server;
            m_ID            = id;
            m_Name          = name;
            m_ApiInitString = apiInitString;
            m_pApi          = api;
        }


        #region Events Handling

        #region SMTP Events

		//------ SMTP Events -----------------------//

		#region method SMTP_Server_ValidateIPAddress

		private void SMTP_Server_ValidateIPAddress(object sender,LumiSoft.Net.ValidateIP_EventArgs e)
		{
			try{		
				e.Validated = IsAccessAllowed(Service_enum.SMTP,e.RemoteEndPoint.Address);
			}
			catch(Exception x){
				e.Validated = false;
				Error.DumpError(this.Name,x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region method Server_AuthenticateUser

		private void Server_AuthenticateUser(object sender,LumiSoft.Net.SMTP.Server.AuthUser_EventArgs e)
		{	
			try{
				string returnData = "";
				e.Validated = OnAuthenticate(e.Session.RemoteEndPoint.Address,e.UserName,e.PasswData,e.AuthData,e.AuthType,out returnData);
				e.ReturnData = returnData;
			}
			catch(Exception x){
				e.Validated = false;
				Error.DumpError(this.Name,x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region method smtp_Server_ValidateSender

		private void smtp_Server_ValidateSender(object sender,LumiSoft.Net.SMTP.Server.ValidateSender_EventArgs e)
		{
			if(m_SMTP_RequireAuth && !e.Session.Authenticated){
				e.Validated = false;
				e.ErrorText = "Authentication required !";
				return;
			}

			try{
				//--- Filter sender -----------------------------------------------------//					
				DataView dvFilters = m_pApi.GetFilters();
				dvFilters.RowFilter = "Enabled=true AND Type='ISmtpSenderFilter'";
				dvFilters.Sort = "Cost";			
				foreach(DataRowView drViewFilter in dvFilters){
					string assemblyFile = drViewFilter.Row["Assembly"].ToString();
					// File is without path probably, try to load it from filters folder
					if(!File.Exists(assemblyFile)){
						assemblyFile = m_pOwnerServer.StartupPath + "\\Filters\\" + assemblyFile;
					}

					Assembly ass = Assembly.LoadFrom(assemblyFile);
					Type tp = ass.GetType(drViewFilter.Row["ClassName"].ToString());
					object filterInstance = Activator.CreateInstance(tp);
					ISmtpSenderFilter filter = (ISmtpSenderFilter)filterInstance;
								
					string error = null;
					if(!filter.Filter(e.MailFrom,m_pApi,e.Session,out error)){
						e.Validated = false;
						if(error != null){
							e.ErrorText = error;
						}

						return;
					}
				}
				//----------------------------------------------------------------------//

				e.Validated = true;
			}
			catch(Exception x){
				e.Validated = false;
				e.ErrorText = "Internal server error";
				Error.DumpError(this.Name,x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region method smtp_Server_ValidateRecipient

		private void smtp_Server_ValidateRecipient(object sender,LumiSoft.Net.SMTP.Server.ValidateRecipient_EventArgs e)
		{
			try{
				e.Validated = false;

				string mailTo = e.MailTo;

				// If domain isn't specified, add default domain
				if(mailTo.IndexOf("@") == -1){
					mailTo += "@" + m_SMTP_DefaultDomain;
				}

				//1) is local domain or relay needed
				//2) can map email address to mailbox
				//3) is alias
				//4) if matches any routing pattern

				// check if e-domain is local
				if(m_pApi.DomainExists(mailTo)){
					if(m_pApi.MapUser(mailTo) == null){
						// Check if alias.
						if(m_pApi.MailingListExists(mailTo)){
                            // Not authenticated, see is anyone access allowed
                            if(!e.Authenticated){
                                e.Validated = m_pApi.CanAccessMailingList(mailTo,"anyone");
                            }
                            // Authenticated, see if user has access granted
                            else{
                                e.Validated = m_pApi.CanAccessMailingList(mailTo,e.Session.UserName);
                            }
						}
                        // At last check if matches any routing pattern.
                        else{
                            DataView dv = m_pApi.GetRoutes();
                            foreach(DataRowView drV in dv){
                                // We have matching route
                                if(Convert.ToBoolean(drV["Enabled"]) && SCore.IsAstericMatch(drV["Pattern"].ToString(),mailTo)){
                                    e.Validated = true;
                                    break;
                                }
                            }
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
						if(IsRelayAllowed(e.Session.UserName,e.Session.RemoteEndPoint.Address)){
							e.Validated = true;
						}
					}
					else if(IsRelayAllowed("",e.Session.RemoteEndPoint.Address)){
						e.Validated = true;
					}		
				}
			}
			catch(Exception x){
				Error.DumpError(this.Name,x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region method SMTP_Server_ValidateMailBoxSize

		private void SMTP_Server_ValidateMailBoxSize(object sender,LumiSoft.Net.SMTP.Server.ValidateMailboxSize_EventArgs e)
		{
			e.IsValid = false;

			try{
				string emailAddress = e.eAddress;

				// If domain isn't specified, add default domain
				if(emailAddress.IndexOf("@") == -1){
					emailAddress += "@" + m_SMTP_DefaultDomain;
				}

				// If relay or mailing list address, don't check size
				if(!m_pApi.DomainExists(emailAddress) || m_pApi.MailingListExists(emailAddress)){
					e.IsValid = true;
					return;
				}
				//--------------------------------//

				string user = m_pApi.MapUser(emailAddress);
                // Don't check mailbox size for routed address.
				if(user == null){
                    e.IsValid = true;
				}
				else{
					e.IsValid = !m_pApi.ValidateMailboxSize(user);
				}
			}
			catch(Exception x){
				Error.DumpError(this.Name,x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion


        #region method SMTP_Server_GetMessageStoreStream

        /// <summary>
        /// Is called by SMTP server if server needs to get stream where to store incoming message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event data.</param>
        private void SMTP_Server_GetMessageStoreStream(object sender,GetMessageStoreStream_eArgs e)
        {
            if(!Directory.Exists(m_MailStorePath + "IncomingSMTP")){
                Directory.CreateDirectory(m_MailStorePath + "IncomingSMTP");
            }

            e.StoreStream = new FileStream(API_Utlis.PathFix(m_MailStorePath + "IncomingSMTP\\" + Guid.NewGuid().ToString().Replace("-","") + ".eml"),FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite); 
        }

        #endregion

        #region method SMTP_Server_MessageStoringCompleted

        /// <summary>
        /// Is called by SMTP server if server has completed incoming message storing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event data.</param>
        private void SMTP_Server_MessageStoringCompleted(object sender,MessageStoringCompleted_eArgs e)
        {
            string fileName = ((FileStream)e.MessageStream).Name;

            try{
                // Message stored successfully to file, process it.
                if(e.ErrorText == null){                    
                    e.MessageStream.Position = 0;

				    ProcessAndStoreMessage(e.Session.MailFrom,e.Session.MailTo,e.MessageStream,e);
                }
			}
			catch(Exception x){
				Error.DumpError(this.Name,x,new System.Diagnostics.StackTrace());
                e.ServerReply.ErrorReply = true;
                e.ServerReply.ReplyText = "Error storing message";
			}
            finally{
                // Close file and delete it.
                ((FileStream)e.MessageStream).Dispose();
                File.Delete(fileName);
            }
        }

        #endregion

        #region method ProcessAndStoreMessage

        /// <summary>
        /// Processes and stores message.
        /// </summary>
        /// <param name="sender">Mail from.</param>
        /// <param name="recipient">Recipient to.</param>
        /// <param name="msgStream">Message stream. Stream position must be there where message begins.</param>
        /// <param name="e">Event data.</param>
		public void ProcessAndStoreMessage(string sender,string[] recipient,Stream msgStream,MessageStoringCompleted_eArgs e)
		{	
            
			#region Global Filtering stuff
            
			//--- Filter message -----------------------------------------------//
			Stream filteredMsgStream = msgStream;
			DataView dvFilters = m_pApi.GetFilters();
			dvFilters.RowFilter = "Enabled=true AND Type='ISmtpMessageFilter'";
			dvFilters.Sort = "Cost";			
			foreach(DataRowView drViewFilter in dvFilters){
                try{
				    filteredMsgStream.Position = 0;

				    string assemblyFile = API_Utlis.PathFix(drViewFilter.Row["Assembly"].ToString());
				    // File is without path probably, try to load it from filters folder
				    if(!File.Exists(assemblyFile)){
					    assemblyFile = API_Utlis.PathFix(m_pOwnerServer.StartupPath + "\\Filters\\" + assemblyFile);
				    }

				    Assembly ass = Assembly.LoadFrom(assemblyFile);
				    Type tp = ass.GetType(drViewFilter.Row["ClassName"].ToString());
				    object filterInstance = Activator.CreateInstance(tp);
				    ISmtpMessageFilter filter = (ISmtpMessageFilter)filterInstance;
    						
				    string errorText = "";
                    SMTP_Session session = null;
                    if(e != null){
                        session = e.Session;
                    }
				    FilterResult result = filter.Filter(filteredMsgStream,out filteredMsgStream,sender,recipient,m_pApi,session,out errorText);
                    if(result == FilterResult.DontStore){
                        // Just skip messge, act as message is stored
                        e.ServerReply.ReplyText = "Message just discarded by filter !";
						return; 
                    }
                    else if(result == FilterResult.Error){
                        if(e != null){
						    e.ServerReply.ErrorReply = true;
						    e.ServerReply.ReplyText = errorText;
                        }
                        else{
                            // NOTE: 26.01.2006 - e maybe null if that method is called server internally and no smtp session.                            
                        }
						return;
                    }
                    
                    // Filter didn't return message stream
                    if(filteredMsgStream == null){
                        e.ServerReply.ErrorReply = true;
				        e.ServerReply.ReplyText = "Message rejected by filter";
                        return;
                    }
                }
                catch(Exception x){
                    // Filtering failed, log error and allow message through.
                    OnError(x);
                }
			}
			//---------------------------------------------------------------//
			#endregion

            #region Global Message Rules

            filteredMsgStream.Position = 0;
            Mime mime = null;
            try{
                mime = Mime.Parse(filteredMsgStream);
            }
            // Invalid message syntax, block such message.
            catch{
                e.ServerReply.ErrorReply = true;
                e.ServerReply.ReplyText = "Invalid message structure/syntax, message blocked !";
                return;
            }
            string[] to = recipient;

            //--- Check Global Message Rules --------------------------------------------------------------//
            bool   deleteMessage = false;
            string storeFolder   = "Inbox";
            string smtpErrorText = null;   

            // Loop rules
            foreach(DataRowView drV_Rule in m_pApi.GetGlobalMessageRules()){
                // Reset stream position
                filteredMsgStream.Position = 0;

                if(Convert.ToBoolean(drV_Rule["Enabled"])){
                    string ruleID = drV_Rule["RuleID"].ToString();
                    GlobalMessageRule_CheckNextRule_enum checkNextIf = (GlobalMessageRule_CheckNextRule_enum)(int)drV_Rule["CheckNextRuleIf"];
                    string matchExpression = drV_Rule["MatchExpression"].ToString();

                    // e may be null if server internal method call and no actual session !
                    SMTP_Session session = null;
                    if(e != null){
                        session = e.Session;
                    }
                    GlobalMessageRuleProcessor ruleEngine = new GlobalMessageRuleProcessor();
                    bool matches = ruleEngine.Match(matchExpression,sender,to,session,mime,(int)filteredMsgStream.Length);
                    if(matches){                        
                        // Do actions
                        GlobalMessageRuleActionResult result = ruleEngine.DoActions(
                            m_pApi.GetGlobalMessageRuleActions(ruleID),
                            this,
                            filteredMsgStream,
                            sender,
                            to
                        );

                        if(result.DeleteMessage){
                            deleteMessage = true;
                        }
                        if(result.StoreFolder != null){                            
                            storeFolder = result.StoreFolder;                           
                        }
                        if(result.ErrorText != null){
                            smtpErrorText = result.ErrorText;
                        }
                    }

                    //--- See if we must check next rule -------------------------------------------------//
                    if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.Always){
                        // Do nothing
                    }
                    else if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.IfMatches && !matches){
                        break;
                    }
                    else if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.IfNotMatches && matches){
                        break;
                    }
                    //------------------------------------------------------------------------------------//
                }
            }

            // Return error to connected client
            if(smtpErrorText != null){
                e.ServerReply.ErrorReply = true;
                e.ServerReply.ReplyText = smtpErrorText;
                return;
            }

            // Just don't store message
            if(deleteMessage){
                return;
            }
            
            // Reset stream position
            filteredMsgStream.Position = 0;
            //--- End of Global Rules -------------------------------------------------------------------//

            #endregion


            #region Construct final recipients

            // Construct final recipients, replace mailing list members with actual addresses.			
		    List<string> finalRecipients = new List<string>();            
            foreach(string r in recipient){
                string emailAddress = r;

				// If domain isn't specified, add default domain
				if(emailAddress.IndexOf("@") == -1){
					emailAddress += "@" + m_SMTP_DefaultDomain;
				}

                // This is mailing list address, get member email addresses
                if(m_pApi.MailingListExists(emailAddress)){
                    List<string> processedMailignLists = new List<string>();
                    Queue<string> processQueue = new Queue<string>();
                    processQueue.Enqueue(emailAddress);

                    // Loop while there are mailing lists or nested mailing list available
                    while(processQueue.Count > 0){
                        string mailingList = processQueue.Dequeue();                        
                        processedMailignLists.Add(mailingList);
                          
                        // Process mailing list members
					    foreach(DataRowView drV in m_pApi.GetMailingListAddresses(mailingList)){
                            string member = drV["Address"].ToString();

                            // Member is asteric pattern matching server emails
                            if(member.IndexOf('*') > -1){
                                DataView dvServerAddresses = m_pApi.GetUserAddresses("");
                                foreach(DataRowView drvServerAddress in dvServerAddresses){
                                    string serverAddress = drvServerAddress["Address"].ToString();
                                    if(SCore.IsAstericMatch(member,serverAddress)){
                                        if(!finalRecipients.Contains(serverAddress)){
                                            finalRecipients.Add(serverAddress);
                                        }
                                    }
                                }
                            }
                            // Member is user or group, not email address
                            else if(member.IndexOf('@') == -1){                            
                                // Member is group, replace with actual users
                                if(m_pApi.GroupExists(member)){
                                    foreach(string user in m_pApi.GetGroupUsers(member)){
                                        if(!finalRecipients.Contains(user)){
                                            finalRecipients.Add(user);
                                        }
                                    }
                                }
                                // Member is user
                                else if(m_pApi.UserExists(member)){
                                    if(!finalRecipients.Contains(member)){
                                        finalRecipients.Add(member);
                                    }
                                }
                                // Unknown member, do nothing
                                else{
                                }
                            }
                            // Member is nested mailing list
                            else if(m_pApi.MailingListExists(member)){
                                // Don't proccess poroccessed mailing lists any more, causes infinite loop
                                if(!processedMailignLists.Contains(member)){
                                    processQueue.Enqueue(member);
                                }
                            }
                            // Member is normal email address
                            else{
                                if(!finalRecipients.Contains(member)){
                                    finalRecipients.Add(member);
                                }
                            }					
					    }
                    }
				}
                // Normal email address
                else{
                    if(!finalRecipients.Contains(emailAddress)){
                        finalRecipients.Add(emailAddress);
                    }
                }
            }

            // TODO: ???
            // Map routing
            // Map local server email address to user.

            #endregion
          
            // Store individual message to each recipient
            foreach(string r in finalRecipients){
                ProcessRecipientMsg(sender,r,storeFolder,filteredMsgStream,e);
            }
        }

        #region method ProcessRecipientMsg

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recipient"></param>
        /// <param name="storeFolder">Message folder where message will be stored. For example 'Inbox'.</param>
        /// <param name="msgStream"></param>
        /// <param name="e">Event data.</param>
		private void ProcessRecipientMsg(string sender,string recipient,string storeFolder,Stream msgStream,MessageStoringCompleted_eArgs e)
		{
            msgStream.Position = 0;

			string eAddress    = recipient;
			string mailbox     = "";
			bool   relay       = false;
			string forwardHost = null;

            // We have local user instead of emailaddress
            if(recipient.IndexOf('@') == -1){
                mailbox = recipient;
            }
            else{
                mailbox = m_pApi.MapUser(eAddress);
            }

			// If not valid local mailbox or relay message.
			if(mailbox == null){

                #region Check Routing

                bool routingDone = false;
                DataView dv = m_pApi.GetRoutes();
                foreach(DataRowView drV in dv){
                    // We have matching route
                    if(Convert.ToBoolean(drV["Enabled"]) && SCore.IsAstericMatch(drV["Pattern"].ToString(),eAddress)){
                        RouteAction_enum action     = (RouteAction_enum)Convert.ToInt32(drV["Action"]);
                        byte[]           actionData = (byte[])drV["ActionData"];

                        #region RouteToEmail

                        if(action == RouteAction_enum.RouteToEmail){
                            XmlTable table = new XmlTable("ActionData");
                            table.Parse(actionData);
                            eAddress = table.GetValue("EmailAddress");
                            relay = true;
                        }

                        #endregion

                        #region RouteToHost

                        else if(action == RouteAction_enum.RouteToHost){
                            XmlTable table = new XmlTable("ActionData");
                            table.Parse(actionData);
                            forwardHost = table.GetValue("Host") + ":" + table.GetValue("Port");
                            relay = true;
                        }

                        #endregion

                        #region RouteToMailbox

                        else if(action == RouteAction_enum.RouteToMailbox){
                            XmlTable table = new XmlTable("ActionData");
                            table.Parse(actionData);
                            mailbox = table.GetValue("Mailbox");
                        }

                        #endregion

                        routingDone = true;
                        break;
                    }
                }

                // Routing not done, handle message normally.
                if(!routingDone){
                    // Local message, but won't match to any mailbox. We never should reach there,
                    // this may happen if routing deleted between session, just skip that message.
                    if(m_pApi.DomainExists(eAddress)){
                        return;
                    }
                    else{
                        relay = true;
                    }
                }

                #endregion                 
			}

			if(relay){                
				this.RelayServer.StoreRelayMessage(msgStream,forwardHost,sender,eAddress);
			}
			else{
				ProcessUserMsg(sender,eAddress,mailbox,storeFolder,msgStream,e);
			}
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recipient"></param>
        /// <param name="userName"></param>
        /// <param name="storeFolder">Message folder where message will be stored. For example 'Inbox'.</param>
        /// <param name="msgStream"></param>
        /// <param name="e">Event data.</param>
		internal void ProcessUserMsg(string sender,string recipient,string userName,string storeFolder,Stream msgStream,MessageStoringCompleted_eArgs e)
        {
            string userID = m_pApi.GetUserID(userName);
            // This value can be null only if user deleted during this session, so just skip next actions.
            if(userID == null){
                return;
            }

            #region User Message rules
	
            Stream filteredMsgStream = msgStream;
			filteredMsgStream.Position = 0;

            Mime mime = null;
            try{
                mime = Mime.Parse(filteredMsgStream);
            }
            // Invalid message syntax, block such message.
            catch{
                e.ServerReply.ErrorReply = true;
                e.ServerReply.ReplyText = "Invalid message structure/syntax, message blocked !";
                return;
            }

            string[] to = new string[]{recipient};

            //--- Check User Message Rules --------------------------------------------------------------//
            bool   deleteMessage = false;
            string smtpErrorText = null;   

            // Loop rules
            foreach(DataRowView drV_Rule in m_pApi.GetUserMessageRules(userName)){
                // Reset stream position
                filteredMsgStream.Position = 0;

                if(Convert.ToBoolean(drV_Rule["Enabled"])){
                    string ruleID = drV_Rule["RuleID"].ToString();
                    GlobalMessageRule_CheckNextRule_enum checkNextIf = (GlobalMessageRule_CheckNextRule_enum)(int)drV_Rule["CheckNextRuleIf"];
                    string matchExpression = drV_Rule["MatchExpression"].ToString();

                    // e may be null if server internal method call and no actual session !
                    SMTP_Session session = null;
                    if(e != null){
                        session = e.Session;
                    }
                    GlobalMessageRuleProcessor ruleEngine = new GlobalMessageRuleProcessor();
                    bool matches = ruleEngine.Match(matchExpression,sender,to,session,mime,(int)filteredMsgStream.Length);
                    if(matches){                        
                        // Do actions
                        GlobalMessageRuleActionResult result = ruleEngine.DoActions(
                            m_pApi.GetUserMessageRuleActions(userID,ruleID),
                            this,
                            filteredMsgStream,
                            sender,
                            to
                        );

                        if(result.DeleteMessage){
                            deleteMessage = true;
                        }
                        if(result.StoreFolder != null){                            
                            storeFolder = result.StoreFolder;                           
                        }
                        if(result.ErrorText != null){
                            smtpErrorText = result.ErrorText;
                        }
                    }

                    //--- See if we must check next rule -------------------------------------------------//
                    if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.Always){
                        // Do nothing
                    }
                    else if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.IfMatches && !matches){
                        break;
                    }
                    else if(checkNextIf == GlobalMessageRule_CheckNextRule_enum.IfNotMatches && matches){
                        break;
                    }
                    //------------------------------------------------------------------------------------//
                }
            }

            // Return error to connected client
            if(smtpErrorText != null){
                e.ServerReply.ErrorReply = true;
                e.ServerReply.ReplyText = smtpErrorText;
                return;
            }

            // Just don't store message
            if(deleteMessage){
                return;
            }
            
            // Reset stream position
            filteredMsgStream.Position = 0;
            //--- End of Global Rules -------------------------------------------------------------------//
            
            #endregion

            // ToDo: User message filtering
			#region User message filtering

				//--- Filter message -----------------------------------------------//
		/*		MemoryStream filteredMsgStream = msgStream;
				DataView dvFilters = m_pApi.GetFilters();
				dvFilters.RowFilter = "Enabled=true AND Type=ISmtpUserMessageFilter";
				dvFilters.Sort = "Cost";			
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
												
					FilterResult result = filter.Filter(filteredMsgStream,out filteredMsgStream,sender,recipient,m_pApi);
					switch(result)
					{
						case FilterResult.DontStore:
							return; // Just skip messge, act as message is stored

						case FilterResult.Error:
							// ToDO: Add implementaion here or get rid of it (use exception instead ???).
							return;
					}
				}*/
				//---------------------------------------------------------------//

				#endregion

			try{
				m_pApi.StoreMessage("system",userName,storeFolder,filteredMsgStream,DateTime.Now,IMAP_MessageFlags.Recent);
			}
			catch{
                // Storing probably failed because there isn't such folder, just store to user inbox.
				m_pApi.StoreMessage("system",userName,"Inbox",filteredMsgStream,DateTime.Now,IMAP_MessageFlags.Recent);
			}
		}

		#endregion


		#region method SMTP_Server_SessionLog

		private void SMTP_Server_SessionLog(object sender,LumiSoft.Net.Log_EventArgs e)
		{
            Logger.WriteLog(m_SMTP_LogPath + "smtp-" + DateTime.Today.ToString("yyyyMMdd") + ".log",e.Logger);
		}

		#endregion

		//------ End of SMTP Events ----------------//

		#endregion		

		#region POP3 Events

		//------ POP3 Events ----------------------//

		#region method POP3_Server_ValidateIPAddress

		private void POP3_Server_ValidateIPAddress(object sender,LumiSoft.Net.ValidateIP_EventArgs e)
		{
			try{		
				e.Validated = IsAccessAllowed(Service_enum.POP3,e.RemoteEndPoint.Address);
			}
			catch(Exception x){
				e.Validated = false;
				Error.DumpError(this.Name,x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region method Server_AuthenticateUser

		private void Server_AuthenticateUser(object sender,LumiSoft.Net.POP3.Server.AuthUser_EventArgs e)
		{	
			try{
				string returnData = "";
				e.Validated = OnAuthenticate(e.Session.RemoteEndPoint.Address,e.UserName,e.PasswData,e.AuthData,e.AuthType,out returnData);
				e.ReturnData = returnData;

                // Check that user is allowed to access this service
                if(e.Validated && (m_pApi.GetUserPermissions(e.UserName) & UserPermissions_enum.POP3) == 0){
                    e.ErrorText = "You don't have right to access this service !";
                }
			}
			catch(Exception x){
				e.Validated = false;
				Error.DumpError(this.Name,x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		
		#region method pop3_Server_GetMessgesList

		private void pop3_Server_GetMessgesList(object sender,LumiSoft.Net.POP3.Server.GetMessagesInfo_EventArgs e)
		{
			try{
				string userName = e.UserName;

				IMAP_MessageCollection messages = new IMAP_MessageCollection();
				m_pApi.GetMessagesInfo(userName,userName,"Inbox",messages);
				for(int i=0;i<messages.Count;i++){
					IMAP_Message msg = messages[i]; 
					e.Messages.Add(msg.ID,msg.UID.ToString(),msg.Size);
				}
			}
			catch(Exception x){
				Error.DumpError(this.Name,x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

        #region method POP3_Server_GetMessageStream

        private void POP3_Server_GetMessageStream(object sender,POP3_eArgs_GetMessageStream e)
        {
            try{ 
                EmailMessageItems eArgs = new EmailMessageItems(e.MessageInfo.ID,IMAP_MessageItems_enum.Message);
                m_pApi.GetMessageItems(e.Session.UserName,e.Session.UserName,"Inbox",eArgs);

                e.MessageExists = eArgs.MessageExists;
                if(eArgs.MessageStream != null){
                    e.MessageStream = eArgs.MessageStream;
                }
			}
			catch(Exception x){
				Error.DumpError(this.Name,x,new System.Diagnostics.StackTrace());
			}
        }

        #endregion

		#region method pop3_Server_DeleteMessage

		private void pop3_Server_DeleteMessage(object sender,LumiSoft.Net.POP3.Server.POP3_Message_EventArgs e)
		{
			try{
				m_pApi.DeleteMessage(e.UserName,e.UserName,"Inbox",e.MessageID,Convert.ToInt32(e.MessageUID));
			}
			catch(Exception x){
				Error.DumpError(this.Name,x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region method POP3_Server_GetTopLines

		private void POP3_Server_GetTopLines(object sender,LumiSoft.Net.POP3.Server.POP3_Message_EventArgs e)
		{	
			try{				
				e.MessageData = m_pApi.GetMessageTopLines(e.UserName,e.UserName,"Inbox",e.MessageID,e.Lines);				
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}			
		}

		#endregion


		#region method POP3_Server_SessionLog

		private void POP3_Server_SessionLog(object sender,LumiSoft.Net.Log_EventArgs e)
		{
            Logger.WriteLog(m_POP3_LogPath + "pop3-" + DateTime.Today.ToString("yyyyMMdd") + ".log",e.Logger);
		}

		#endregion

		//------ End of POP3 Events ---------------//
		
		#endregion

		#region IMAP Events

		#region method IMAP_Server_ValidateIPAddress

        /// <summary>
        /// Is called by IMAP server if connected client IP address must be validated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event data.</param>
		private void IMAP_Server_ValidateIPAddress(object sender,LumiSoft.Net.ValidateIP_EventArgs e)
		{
			try{		
				e.Validated = IsAccessAllowed(Service_enum.IMAP,e.RemoteEndPoint.Address);
			}
			catch(Exception x){
				e.Validated = false;
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region method IMAP_Server_AuthUser

        /// <summary>
        /// Is called by IMAP server if connected client authentication data must be validated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event data.</param>
		private void IMAP_Server_AuthUser(object sender,LumiSoft.Net.IMAP.Server.AuthUser_EventArgs e)
		{
			try{
				string returnData = "";
				e.Validated = OnAuthenticate(e.Session.RemoteEndPoint.Address,e.UserName,e.PasswData,e.AuthData,e.AuthType,out returnData);
				e.ReturnData = returnData;
               
                // Check that user is allowed to access this service
                if(e.Validated && (m_pApi.GetUserPermissions(e.UserName) & UserPermissions_enum.IMAP) == 0){
                    e.ErrorText = "You don't have right to access this service !";
                }

                // Force to create user default folders.
                if(e.Validated){
                    m_pApi.CreateUserDefaultFolders(e.UserName);
                }
			}
			catch(Exception x){
				e.Validated = false;
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion


		#region method IMAP_Server_GetSharedRootFolders

		private void IMAP_Server_GetSharedRootFolders(object sender, SharedRootFolders_EventArgs e)
		{
            SharedFolderRoot[] rootFolders =  m_pApi.GetSharedFolderRoots();
            List<string> publicFolders = new List<string>();
            List<string> usersFolders = new List<string>();
            foreach(SharedFolderRoot rootFolder in rootFolders){
                if(rootFolder.Enabled){
                    if(rootFolder.RootType == SharedFolderRootType_enum.BoundedRootFolder){
                        publicFolders.Add(rootFolder.FolderName);
                    }
                    else{
                        usersFolders.Add(rootFolder.FolderName);
                    }
                }
            }
            
			e.PublicRootFolders = publicFolders.ToArray();
			e.SharedRootFolders = usersFolders.ToArray();
		}

		#endregion

		#region method IMAP_Server_GetFolders

		private void IMAP_Server_GetFolders(object sender, IMAP_Folders e)
		{
			IMAP_Session ses = (IMAP_Session)sender;

			string[] folders = m_pApi.GetFolders(ses.UserName,true);
			foreach(string folder in folders){
				e.Add(folder,true);
			}
		}

		#endregion

		#region method IMAP_Server_GetSubscribedFolders

		private void IMAP_Server_GetSubscribedFolders(object sender, IMAP_Folders e)
		{
			IMAP_Session ses = (IMAP_Session)sender;

			string[] folders = m_pApi.GetSubscribedFolders(ses.UserName);
			foreach(string folder in folders){
				e.Add(folder,true);
			}
		}

		#endregion

		#region method IMAP_Server_SubscribeFolder

		private void IMAP_Server_SubscribeFolder(object sender, Mailbox_EventArgs e)
		{
			IMAP_Session ses = (IMAP_Session)sender;

			m_pApi.SubscribeFolder(ses.UserName,e.Folder);
		}

		#endregion

		#region method IMAP_Server_UnSubscribeFolder

		private void IMAP_Server_UnSubscribeFolder(object sender, Mailbox_EventArgs e)
		{
			IMAP_Session ses = (IMAP_Session)sender;

			m_pApi.UnSubscribeFolder(ses.UserName,e.Folder);
		}

		#endregion

		#region method IMAP_Server_CreateFolder

		private void IMAP_Server_CreateFolder(object sender, Mailbox_EventArgs e)
		{
			try{
				IMAP_Session ses = (IMAP_Session)sender;

				m_pApi.CreateFolder(ses.UserName,ses.UserName,e.Folder);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region method IMAP_Server_DeleteFolder

		private void IMAP_Server_DeleteFolder(object sender, Mailbox_EventArgs e)
		{
			try{
				IMAP_Session ses = (IMAP_Session)sender;

				m_pApi.DeleteFolder(ses.UserName,ses.UserName,e.Folder);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region method IMAP_Server_RenameFolder

		private void IMAP_Server_RenameFolder(object sender, Mailbox_EventArgs e)
		{
			try{
				IMAP_Session ses = (IMAP_Session)sender;

				m_pApi.RenameFolder(ses.UserName,ses.UserName,e.Folder,e.NewFolder);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion


		#region method IMAP_Server_GetMessagesInfo

		private void IMAP_Server_GetMessagesInfo(object sender,IMAP_eArgs_GetMessagesInfo e)
		{
			try{
				string userName   = e.Session.UserName;
				string folder     = e.FolderInfo.Folder;

                // Set dummy folder UID, FIX ME: 
                e.FolderInfo.FolderUID = 124221;
				
				m_pApi.GetMessagesInfo(userName,userName,folder,e.FolderInfo.Messages);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

        #region method IMAP_Server_GetMessageItems

        private void IMAP_Server_GetMessageItems(object sender,IMAP_eArgs_MessageItems e)
        {
			string userName   = e.Session.UserName;
			string folder     = e.Session.SelectedMailbox;
                        
            EmailMessageItems eArgs = new EmailMessageItems(e.MessageInfo.ID,e.MessageItems);				
			m_pApi.GetMessageItems(userName,userName,folder,eArgs);

            eArgs.CopyTo(e);
        }

        #endregion

		#region method IMAP_Server_DeleteMessage

		private void IMAP_Server_DeleteMessage(object sender, Message_EventArgs e)
		{
			try{
				IMAP_Session ses = (IMAP_Session)sender;

				string userName = ses.UserName;
				string folder   = e.Folder;
				
				m_pApi.DeleteMessage(userName,userName,folder,e.Message.ID,(int)e.Message.UID);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region method IMAP_Server_CopyMessage

		private void IMAP_Server_CopyMessage(object sender,Message_EventArgs e)
		{
			try{
				IMAP_Session ses = (IMAP_Session)sender;

				string userName = ses.UserName;
				string folder  = e.Folder;	
				string mailboxDest = e.CopyLocation;								
				string destFolderUserName = ses.UserName;		
				
				m_pApi.CopyMessage(userName,userName,folder,destFolderUserName,mailboxDest,e.Message);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region method IMAP_Server_StoreMessage

		private void IMAP_Server_StoreMessage(object sender, Message_EventArgs e)
		{
			try{
				IMAP_Session ses = (IMAP_Session)sender;

				string userName   = ses.UserName;
				string folder     = e.Folder;
											
				m_pApi.StoreMessage(userName,userName,folder,new MemoryStream(e.MessageData),e.Message.InternalDate,e.Message.Flags);
			}
			catch(Exception x){
				e.ErrorText = x.Message;
			}
		}

		#endregion

		#region method IMAP_Server_StoreMessageFlags

		private void IMAP_Server_StoreMessageFlags(object sender, Message_EventArgs e)
		{
			IMAP_Session ses = (IMAP_Session)sender;

			string userName   = ses.UserName;
			string folder     = e.Folder;
			
			m_pApi.StoreMessageFlags(userName,userName,folder,e.Message,e.Message.Flags);
		}

		#endregion


		#region method IMAP_Server_GetFolderACL

		private void IMAP_Server_GetFolderACL(object sender,IMAP_GETACL_eArgs e)
		{      
            try{
			    DataView dv = m_pApi.GetFolderACL(e.Session.UserName,e.Session.UserName,e.Folder);
			    foreach(DataRowView drV in dv){
				    e.ACL.Add(drV["User"].ToString(),IMAP_Utils.ACL_From_String(drV["Permissions"].ToString()));
			    }
		    }
            catch(Exception x){
                e.ErrorText = x.Message;
            }
		}

		#endregion

		#region method IMAP_Server_SetFolderACL

		private void IMAP_Server_SetFolderACL(object sender, IMAP_SETACL_eArgs e)
		{
            try{
			    m_pApi.SetFolderACL(e.Session.UserName,e.Session.UserName,e.Folder,e.UserName,e.FlagsSetType,e.ACL);
            }
            catch(Exception x){
                e.ErrorText = x.Message;
            }
		}

		#endregion

		#region method IMAP_Server_DeleteFolderACL

		private void IMAP_Server_DeleteFolderACL(object sender,IMAP_DELETEACL_eArgs e)
		{
	        try{
			    m_pApi.DeleteFolderACL(e.Session.UserName,e.Session.UserName,e.Folder,e.UserName);
            }
            catch(Exception x){
                e.ErrorText = x.Message;
            }
		}

		#endregion

		#region method IMAP_Server_GetUserACL

		private void IMAP_Server_GetUserACL(object sender, IMAP_GetUserACL_eArgs e)
		{            
            e.ACL = m_pApi.GetUserACL(e.Session.UserName,e.Folder,e.Session.UserName);
		}

		#endregion


        #region method IMAP_Server_GetUserQuota

        private void IMAP_Server_GetUserQuota(object sender,IMAP_eArgs_GetQuota e)
        {
            e.MailboxSize = m_pApi.GetMailboxSize(e.UserName);
            foreach(DataRowView drv in m_pApi.GetUsers("ALL")){
                if(drv["UserName"].ToString().ToLower() == e.UserName.ToLower()){
                    e.MaxMailboxSize = ConvertEx.ToInt32(drv["Mailbox_Size"]) * 1000 * 1000;
                    break;
                }
            }            
        }

        #endregion


        #region method IMAP_Server_SessionLog

        private void IMAP_Server_SessionLog(object sender,LumiSoft.Net.Log_EventArgs e)
		{
            Logger.WriteLog(m_IMAP_LogPath + "imap-" + DateTime.Today.ToString("yyyyMMdd") + ".log",e.Logger);
		}

		#endregion

		#endregion

        #region SIP Events

        #region method m_pSipServer_Authenticate

        private void m_pSipServer_Authenticate(SIP_AuthenticateEventArgs e)
        {
            // TODO: change api to return 1 user
        
            foreach(DataRowView drV in m_pApi.GetUsers("ALL")){
                if(e.AuthContext.UserName.ToLower() == drV["UserName"].ToString().ToLower()){                    
                    e.Authenticated = e.AuthContext.Authenticate(drV["UserName"].ToString(),drV["Password"].ToString());
                    return;
                }
            }

            e.Authenticated = false;
        }

        #endregion

        #region method m_pSipServer_IsLocalUri

        private bool m_pSipServer_IsLocalUri(string uri)
        {
            // TODO: get domain

            return m_pApi.DomainExists(uri);
        }

        #endregion

        #region method m_pSipServer_AddressExists

        private bool m_pSipServer_AddressExists(string address)
        {
            return m_pApi.MapUser(address) != null;
        }

        #endregion

        #region method m_pSipServer_CanRegister

        private bool m_pSipServer_CanRegister(string userName,string address)
        {
            foreach(DataRowView drV in m_pApi.GetUserAddresses(userName)){
                if(drV["Address"].ToString().ToLower() == address.ToLower()){
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region method m_pSipServer_Error

        private void m_pSipServer_Error(Exception x)
        {
            OnError(x);
        }

        #endregion

        #endregion

        #region Common Events

        #region method OnAuthenticate

        private bool OnAuthenticate(IPAddress ip,string userName,string passwData,string authData,LumiSoft.Net.AuthType authType,out string returnData)
		{
            returnData = null;

            // See if too many bad logins for specified IP and user if so block auth.
            if(m_pBadLoginManager.IsExceeded(ip.ToString(),userName)){
                return false;
            }

            bool validated = false;            
            // Integrated auth
            if(m_AuthType == MailServerAuthType_enum.Integrated){
                DataSet dsResult = m_pApi.AuthUser(userName,passwData,authData,authType);
			    validated = Convert.ToBoolean(dsResult.Tables["Result"].Rows[0]["Result"]);
			    returnData = dsResult.Tables["Result"].Rows[0]["ReturnData"].ToString();
            }
            // Windows auth
            else if(m_AuthType == MailServerAuthType_enum.Windows){           
                if(m_pApi.UserExists(userName)){
                    validated = WinLogon.Logon(m_Auth_Win_Domain,userName,passwData);
                }
            }
                                               						
			// Increase bad login info
			if(!validated){
                m_pBadLoginManager.Put(ip.ToString(),userName);
			}

            // Update user last login
            if(validated){
                m_pApi.UpdateUserLastLoginTime(userName);
            }

			return validated;
		}

		#endregion

		#region method OnServer_SysError

		private void OnServer_SysError(object sender,LumiSoft.Net.Error_EventArgs e)
		{
			OnError(e.Exception);
		}

		#endregion

		#endregion



        #region method m_pTimer_Elapsed

        private void m_pTimer_Elapsed(object sender,System.Timers.ElapsedEventArgs e)
        {
            try{
				// Load settings after every 1 minutes.
				if(DateTime.Now > m_SettingsDate.AddMinutes(1)){
					LoadSettings();
				}
			}
			catch(Exception x){
                OnError(x);
			}
        }

        #endregion

        #endregion


        #region method Start

        /// <summary>
        /// Starts this virtual server.
        /// </summary>
        public void Start()
        {
            if(m_Running){
                return;
            }
            m_Running = true;

            m_pSmtpServer = new SMTP_Server();
			m_pSmtpServer.AuthUser += new LumiSoft.Net.SMTP.Server.AuthUserEventHandler(this.Server_AuthenticateUser);
			m_pSmtpServer.SessionLog += new LumiSoft.Net.LogEventHandler(this.SMTP_Server_SessionLog);
			m_pSmtpServer.ValidateMailFrom += new LumiSoft.Net.SMTP.Server.ValidateMailFromHandler(this.smtp_Server_ValidateSender);
			m_pSmtpServer.SysError += new LumiSoft.Net.ErrorEventHandler(this.OnServer_SysError);
            m_pSmtpServer.GetMessageStoreStream += new GetMessageStoreStreamHandler(SMTP_Server_GetMessageStoreStream);
            m_pSmtpServer.MessageStoringCompleted += new MessageStoringCompletedHandler(SMTP_Server_MessageStoringCompleted);
			m_pSmtpServer.ValidateMailTo += new LumiSoft.Net.SMTP.Server.ValidateMailToHandler(this.smtp_Server_ValidateRecipient);
			m_pSmtpServer.ValidateMailboxSize += new LumiSoft.Net.SMTP.Server.ValidateMailboxSize(this.SMTP_Server_ValidateMailBoxSize);
			m_pSmtpServer.ValidateIPAddress += new LumiSoft.Net.ValidateIPHandler(this.SMTP_Server_ValidateIPAddress);

            m_pPop3Server = new POP3_Server();
            m_pPop3Server.GetTopLines += new LumiSoft.Net.POP3.Server.MessageHandler(this.POP3_Server_GetTopLines);
            m_pPop3Server.GetMessageStream += new GetMessageStreamHandler(POP3_Server_GetMessageStream);
			m_pPop3Server.DeleteMessage += new LumiSoft.Net.POP3.Server.MessageHandler(this.pop3_Server_DeleteMessage);
			m_pPop3Server.SysError += new LumiSoft.Net.ErrorEventHandler(this.OnServer_SysError);
			m_pPop3Server.AuthUser += new LumiSoft.Net.POP3.Server.AuthUserEventHandler(this.Server_AuthenticateUser);
			m_pPop3Server.GetMessgesList += new LumiSoft.Net.POP3.Server.GetMessagesInfoHandler(this.pop3_Server_GetMessgesList);
			m_pPop3Server.SessionLog += new LumiSoft.Net.LogEventHandler(this.POP3_Server_SessionLog);
			m_pPop3Server.ValidateIPAddress += new LumiSoft.Net.ValidateIPHandler(this.POP3_Server_ValidateIPAddress);

            m_pImapServer = new IMAP_Server();
            m_pImapServer.ValidateIPAddress += new LumiSoft.Net.ValidateIPHandler(this.IMAP_Server_ValidateIPAddress);
			m_pImapServer.SysError += new LumiSoft.Net.ErrorEventHandler(OnServer_SysError);
			m_pImapServer.AuthUser += new LumiSoft.Net.IMAP.Server.AuthUserEventHandler(IMAP_Server_AuthUser);
			m_pImapServer.GetSharedRootFolders += new SharedRootFoldersEventHandler(IMAP_Server_GetSharedRootFolders);
			m_pImapServer.GetFolders += new FoldersEventHandler(IMAP_Server_GetFolders);
			m_pImapServer.GetSubscribedFolders += new FoldersEventHandler(IMAP_Server_GetSubscribedFolders);
			m_pImapServer.SubscribeFolder += new FolderEventHandler(IMAP_Server_SubscribeFolder);
			m_pImapServer.UnSubscribeFolder += new FolderEventHandler(IMAP_Server_UnSubscribeFolder);
			m_pImapServer.CreateFolder += new FolderEventHandler(IMAP_Server_CreateFolder);
			m_pImapServer.DeleteFolder += new FolderEventHandler(IMAP_Server_DeleteFolder);
			m_pImapServer.RenameFolder += new FolderEventHandler(IMAP_Server_RenameFolder);
			m_pImapServer.GetMessagesInfo += new MessagesEventHandler(IMAP_Server_GetMessagesInfo);
			m_pImapServer.DeleteMessage += new MessageEventHandler(IMAP_Server_DeleteMessage);
			m_pImapServer.StoreMessage += new MessageEventHandler(IMAP_Server_StoreMessage);
			m_pImapServer.StoreMessageFlags += new MessageEventHandler(IMAP_Server_StoreMessageFlags);
			m_pImapServer.CopyMessage += new MessageEventHandler(IMAP_Server_CopyMessage);
            m_pImapServer.GetMessageItems += new MessagesItemsEventHandler(IMAP_Server_GetMessageItems);
			m_pImapServer.GetFolderACL += new GetFolderACLEventHandler(IMAP_Server_GetFolderACL);
			m_pImapServer.SetFolderACL += new SetFolderACLEventHandler(IMAP_Server_SetFolderACL);
			m_pImapServer.DeleteFolderACL += new DeleteFolderACLEventHandler(IMAP_Server_DeleteFolderACL);
			m_pImapServer.GetUserACL += new GetUserACLEventHandler(IMAP_Server_GetUserACL);
            m_pImapServer.GetUserQuota += new GetUserQuotaHandler(IMAP_Server_GetUserQuota);
			m_pImapServer.SessionLog += new LumiSoft.Net.LogEventHandler(this.IMAP_Server_SessionLog);

            m_pRelayServer = new Relay_Server(this);

            m_pFetchServer = new FetchPop3(this,m_pApi);

            m_pSipServer = new SIP_Stack();
            ((SIP_ProxyCore)m_pSipServer.SipCore).Authenticate += new SIP_AuthenticateEventHandler(m_pSipServer_Authenticate);
            ((SIP_ProxyCore)m_pSipServer.SipCore).IsLocalUri += new SIP_IsLocalUriEventHandler(m_pSipServer_IsLocalUri);
            ((SIP_ProxyCore)m_pSipServer.SipCore).AddressExists += new SIP_AddressExistsEventHandler(m_pSipServer_AddressExists);
            ((SIP_ProxyCore)m_pSipServer.SipCore).Registrar.CanRegister += new SIP_CanRegisterEventHandler(m_pSipServer_CanRegister);
            m_pSipServer.Error += new SIP_ErrorEventHandler(m_pSipServer_Error);

            m_pRecycleBinManager = new RecycleBinManager(m_pApi);
 
            m_pBadLoginManager = new BadLoginManager();

            m_pTimer = new System.Timers.Timer();            
            m_pTimer.Interval = 15000;
            m_pTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_pTimer_Elapsed);
            m_pTimer.Enabled = true;

            LoadSettings();
        }
                                                                                        
        #endregion

        #region method Stop

        /// <summary>
        /// Stops this virtual server.
        /// </summary>
        public void Stop()
        {
            m_Running = false;

            if(m_pSmtpServer != null){
                try{
                    m_pSmtpServer.Dispose();
                }
                catch{
                }
                m_pSmtpServer = null;
            }
            if(m_pPop3Server != null){
                try{
                    m_pPop3Server.Dispose();
                }
                catch{
                }
                m_pPop3Server = null;
            }
            if(m_pImapServer != null){
                try{
                    m_pImapServer.Dispose();
                }
                catch{
                }
                m_pImapServer = null;
            }
            if(m_pRelayServer != null){
                try{
                    m_pRelayServer.Dispose();
                }
                catch{
                }
                m_pRelayServer = null;
            }
            if(m_pFetchServer != null){
                try{
                    m_pFetchServer.Dispose();                    
                }
                catch{
                }
                m_pFetchServer = null;
            } 
            if(m_pSipServer != null){
                try{
                    m_pSipServer.Stop();                    
                }
                catch{
                }
                m_pSipServer = null;
            } 
            if(m_pTimer != null){
                try{
                    m_pTimer.Dispose();
                }
                catch{
                }
                m_pTimer = null;
            }
            if(m_pRecycleBinManager != null){
                try{
                    m_pRecycleBinManager.Dispose();
                }
                catch{
                }
                m_pRecycleBinManager = null;
            }
            if(m_pBadLoginManager != null){
                try{
                    m_pBadLoginManager.Dispose();
                }
                catch{
                }
                m_pBadLoginManager = null;
            }
        }

        #endregion


        #region method LoadSettings

		private void LoadSettings()
		{
			try{
				// ??? ToDo: calculate settings md5 hash here, see if changed at all

				lock(this){
					m_SettingsDate = DateTime.Now;

					DataRow dr = m_pApi.GetSettings();

                    //--- Try to get mailstore path from API init string ----------------------------------//
                    m_MailStorePath = "Settings\\MailStore";
                    // mailstorepath=
			        string[] parameters = m_ApiInitString.Replace("\r\n","\n").Split('\n');
			        foreach(string param in parameters){
                        if(param.ToLower().IndexOf("mailstorepath=") > -1){
					        m_MailStorePath = param.Substring(14);
                        }
			        }
                    // Fix mail store path, if isn't ending with \
			        if(m_MailStorePath.Length > 0 && !m_MailStorePath.EndsWith("\\")){
				        m_MailStorePath += "\\"; 
			        }
                    if(!Path.IsPathRooted(m_MailStorePath)){
				        m_MailStorePath = m_pOwnerServer.StartupPath + m_MailStorePath;
			        }
                    // Make path directory separator to suit for current platform
                    m_MailStorePath = API_Utlis.PathFix(m_MailStorePath);                    
                    //------------------------------------------------------------------------------------//

                    //--- System settings -----------------------------------------------------------------//
                    m_AuthType        = (MailServerAuthType_enum)ConvertEx.ToInt32(dr["ServerAuthenticationType"]);
                    m_Auth_Win_Domain = ConvertEx.ToString(dr["ServerAuthWinDomain"]);
                    //-------------------------------------------------------------------------------------//


                    #region SMTP

                    //------- SMTP Settings ---------------------------------------------//
                    try{
                        List<BindInfo> smtpIpBinds = new List<BindInfo>();
                        foreach(DataRow dr_Bind in dr.Table.DataSet.Tables["SMTP_Bindings"].Rows){
                            X509Certificate cert = null;
                            if(!dr_Bind.IsNull("SSL_Certificate") && ((byte[])dr_Bind["SSL_Certificate"]).Length > 0){
                                cert = new X509Certificate((byte[])dr_Bind["SSL_Certificate"]);
                            }

                            smtpIpBinds.Add(new BindInfo(
                                IPAddress.Parse(ConvertEx.ToString(dr_Bind["IP"])),
                                ConvertEx.ToInt32(dr_Bind["Port"]),
                                ConvertEx.ToBoolean(dr_Bind["SSL"]),
                                cert
                            ));
                        }
                        m_pSmtpServer.BindInfo = smtpIpBinds.ToArray();
					    m_pSmtpServer.MaxConnections      = ConvertEx.ToInt32(dr["SMTP_Threads"]);
                        m_pSmtpServer.MaxConnectionsPerIP = ConvertEx.ToInt32(dr["SMTP_MaxConnectionsPerIP"]);
					    m_pSmtpServer.SessionIdleTimeOut  = ConvertEx.ToInt32(dr["SMTP_SessionIdleTimeOut"]) * 1000; // Seconds to milliseconds
					    m_pSmtpServer.MaxMessageSize      = ConvertEx.ToInt32(dr["MaxMessageSize"]) * 1000000;       // Mb to byte.
					    m_pSmtpServer.MaxRecipients       = ConvertEx.ToInt32(dr["MaxRecipients"]);
					    m_pSmtpServer.MaxBadCommands      = ConvertEx.ToInt32(dr["SMTP_MaxBadCommands"]);
					    m_pSmtpServer.HostName            = ConvertEx.ToString(dr["SMTP_HostName"]);
                        m_pSmtpServer.GreetingText        = ConvertEx.ToString(dr["SMTP_GreetingText"]);
                        if(m_AuthType == MailServerAuthType_enum.Windows){
                            // For windows auth, we can allow only plain text login, because otherwise 
                            // we can't do auth against windows (it requires user name and password).
                            m_pSmtpServer.SupportedAuthentications = LumiSoft.Net.AUTH.SaslAuthTypes.Login | LumiSoft.Net.AUTH.SaslAuthTypes.Plain;
                        }
                        else{
                            m_pSmtpServer.SupportedAuthentications = LumiSoft.Net.AUTH.SaslAuthTypes.All;
                        }
                        m_SMTP_RequireAuth    = ConvertEx.ToBoolean(dr["SMTP_RequireAuth"]);					
					    m_SMTP_DefaultDomain  = ConvertEx.ToString(dr["SMTP_DefaultDomain"]);
                        m_pSmtpServer.Enabled = ConvertEx.ToBoolean(dr["SMTP_Enabled"]);
                    }
                    catch(Exception x){
                        OnError(x);
                    }
                    //-------------------------------------------------------------------//

                    #endregion

                    #region POP3

                    //------- POP3 Settings -------------------------------------//
                    try{
					    List<BindInfo> pop3IpBinds = new List<BindInfo>();
                        foreach(DataRow dr_Bind in dr.Table.DataSet.Tables["POP3_Bindings"].Rows){
                            X509Certificate cert = null;
                            if(!dr_Bind.IsNull("SSL_Certificate") && ((byte[])dr_Bind["SSL_Certificate"]).Length > 0){
                                cert = new X509Certificate((byte[])dr_Bind["SSL_Certificate"]);
                            }

                            pop3IpBinds.Add(new BindInfo(
                                IPAddress.Parse(ConvertEx.ToString(dr_Bind["IP"])),
                                ConvertEx.ToInt32(dr_Bind["Port"]),
                                ConvertEx.ToBoolean(dr_Bind["SSL"]),
                                cert
                            ));
                        }
                        m_pPop3Server.BindInfo = pop3IpBinds.ToArray();
					    m_pPop3Server.MaxConnections      = ConvertEx.ToInt32(dr["POP3_Threads"]);
                        m_pPop3Server.MaxConnectionsPerIP = ConvertEx.ToInt32(dr["POP3_MaxConnectionsPerIP"]);
					    m_pPop3Server.SessionIdleTimeOut  = ConvertEx.ToInt32(dr["POP3_SessionIdleTimeOut"]) * 1000; // Seconds to milliseconds
					    m_pPop3Server.MaxBadCommands      = ConvertEx.ToInt32(dr["POP3_MaxBadCommands"]);					
					    m_pPop3Server.HostName            = ConvertEx.ToString(dr["POP3_HostName"]);
                        m_pPop3Server.GreetingText        = ConvertEx.ToString(dr["POP3_GreetingText"]);
                        if(m_AuthType == MailServerAuthType_enum.Windows){
                            // For windows auth, we can allow only plain text login, because otherwise 
                            // we can't do auth against windows (it requires user name and password).
                            m_pPop3Server.SupportedAuthentications = LumiSoft.Net.AUTH.SaslAuthTypes.Login | LumiSoft.Net.AUTH.SaslAuthTypes.Plain;
                        }
                        else{
                            m_pPop3Server.SupportedAuthentications = LumiSoft.Net.AUTH.SaslAuthTypes.All;
                        }
					    m_pPop3Server.Enabled = ConvertEx.ToBoolean(dr["POP3_Enabled"]);
                    }
                    catch(Exception x){
                        OnError(x);
                    }
                    //-----------------------------------------------------------//

                    #endregion

                    #region IMAP

                    //------- IMAP Settings -------------------------------------//
                    try{
					    List<BindInfo> imapIpBinds = new List<BindInfo>();
                        foreach(DataRow dr_Bind in dr.Table.DataSet.Tables["IMAP_Bindings"].Rows){
                            X509Certificate cert = null;
                            if(!dr_Bind.IsNull("SSL_Certificate") && ((byte[])dr_Bind["SSL_Certificate"]).Length > 0){
                                cert = new X509Certificate((byte[])dr_Bind["SSL_Certificate"]);
                            }

                            imapIpBinds.Add(new BindInfo(
                                IPAddress.Parse(ConvertEx.ToString(dr_Bind["IP"])),
                                ConvertEx.ToInt32(dr_Bind["Port"]),
                                ConvertEx.ToBoolean(dr_Bind["SSL"]),
                                cert
                            ));
                        }
                        m_pImapServer.BindInfo = imapIpBinds.ToArray();
					    m_pImapServer.MaxConnections      = ConvertEx.ToInt32(dr["IMAP_Threads"]);
                        m_pImapServer.MaxConnectionsPerIP = ConvertEx.ToInt32(dr["IMAP_Threads"]);
					    m_pImapServer.SessionIdleTimeOut  = ConvertEx.ToInt32(dr["IMAP_SessionIdleTimeOut"]) * 1000; // Seconds to milliseconds
					    m_pImapServer.MaxBadCommands      = ConvertEx.ToInt32(dr["IMAP_MaxBadCommands"]);					
					    m_pImapServer.HostName            = ConvertEx.ToString(dr["IMAP_HostName"]);
                        if(m_AuthType == MailServerAuthType_enum.Windows){
                            // For windows auth, we can allow only plain text login, because otherwise 
                            // we can't do auth against windows (it requires user name and password).
                            m_pImapServer.SupportedAuthentications = LumiSoft.Net.AUTH.SaslAuthTypes.Login | LumiSoft.Net.AUTH.SaslAuthTypes.Plain;
                        }
                        else{
                            m_pImapServer.SupportedAuthentications = LumiSoft.Net.AUTH.SaslAuthTypes.All;
                        }
                        m_pImapServer.GreetingText = ConvertEx.ToString(dr["IMAP_GreetingText"]);
					    m_pImapServer.Enabled      = ConvertEx.ToBoolean(dr["IMAP_Enabled"]);
                    }
                    catch(Exception x){
                        OnError(x);
                    }
                    //-----------------------------------------------------------//

                    #endregion

                    #region Relay

                    //------- Relay ----------------------------------------------
                    try{
                        m_pRelayServer.HostName = ConvertEx.ToString(dr["Relay_HostName"]);
                        m_pRelayServer.SessionIdleTimeout = ConvertEx.ToInt32(dr["Relay_SessionIdleTimeOut"]);
                        m_pRelayServer.MaximumConnections = ConvertEx.ToInt32(dr["MaxRelayThreads"]);
                        m_pRelayServer.MaximumConnectionsPerIP = ConvertEx.ToInt32(dr["Relay_MaxConnectionsPerIP"]);
                        m_pRelayServer.UseSmartHost = ConvertEx.ToBoolean(dr["UseSmartHost"]);
                        m_pRelayServer.SmartHost = ConvertEx.ToString(dr["SmartHost"]);
                        m_pRelayServer.SmartHostPort = ConvertEx.ToInt32(dr["SmartHostPort"]);
                        m_pRelayServer.SmartHostUseSSL = ConvertEx.ToBoolean(dr["SmartHostUseSSL"]);
                        m_pRelayServer.SmartHostUserName = ConvertEx.ToString(dr["SmartHostUserName"]);
                        m_pRelayServer.SmartHostPassword = ConvertEx.ToString(dr["SmartHostPassword"]);
                        m_pRelayServer.Dns1 = ConvertEx.ToString(dr["Dns1"]);
                        m_pRelayServer.Dns2 = ConvertEx.ToString(dr["Dns2"]);
                        m_pRelayServer.RelayInterval = ConvertEx.ToInt32(dr["RelayInterval"]);
                        m_pRelayServer.RelayRetryInterval = ConvertEx.ToInt32(dr["RelayRetryInterval"]);
                        m_pRelayServer.UndeliveredWarningAfter = ConvertEx.ToInt32(dr["RelayUndeliveredWarning"]);
                        m_pRelayServer.UndeliveredAfter = ConvertEx.ToInt32(dr["RelayUndelivered"]) * 60;
                        m_pRelayServer.UndeliveredWarningMessage = null;
                        m_pRelayServer.UndeliveredMessage = null;
                        foreach(DataRow drReturnMessage in dr.Table.DataSet.Tables["ServerReturnMessages"].Rows){
                            if(drReturnMessage["MessageType"].ToString() == "delayed_delivery_warning"){
                                m_pRelayServer.UndeliveredWarningMessage = new ServerReturnMessage(drReturnMessage["Subject"].ToString(),drReturnMessage["BodyTextRtf"].ToString());
                            }
                            else if(drReturnMessage["MessageType"].ToString() == "undelivered"){
                                m_pRelayServer.UndeliveredMessage = new ServerReturnMessage(drReturnMessage["Subject"].ToString(),drReturnMessage["BodyTextRtf"].ToString());
                            }                            
                        }
                        if(dr["RelayLocalIP"].ToString().Length > 0){
                            m_pRelayServer.SendingIP = IPAddress.Parse(dr["RelayLocalIP"].ToString());
                        }
                        else{
                            m_pRelayServer.SendingIP = null;
                        }
                        m_pRelayServer.LogCommands = ConvertEx.ToBoolean(dr["LogRelayCmds"]);                
                        if(dr["Relay_LogPath"].ToString().Length == 0){
						    m_pRelayServer.LogsPath = m_pOwnerServer.StartupPath + "Logs\\Relay\\";
					    }
					    else{
						    m_pRelayServer.LogsPath = dr["Relay_LogPath"].ToString() + "\\";
					    }
                        m_Relay_LogPath = m_pRelayServer.LogsPath;
                        m_pRelayServer.StoreUndeliveredMessages = ConvertEx.ToBoolean(dr["StoreUndeliveredMessages"]);
                        if(!m_pRelayServer.Running){
                            m_pRelayServer.Start();
                        }
                    }
                    catch(Exception x){
                        OnError(x);
                    }
                    //------------------------------------------------------------

                    #endregion

                    #region FETCH

                    //----- Fetch POP3 settings ----------------------------------//
                    try{
                        m_pFetchServer.Enabled       = ConvertEx.ToBoolean(dr["FetchPop3_Enabled"]);
                        m_pFetchServer.FetchInterval = ConvertEx.ToInt32(dr["FetchPOP3_Interval"]);
                    }                    
                    catch(Exception x){
                        OnError(x);
                    }
                    //------------------------------------------------------------//

                    #endregion

                    #region SIP

                    List<BindInfo> sipIpBinds = new List<BindInfo>();
                    foreach(DataRow dr_Bind in dr.Table.DataSet.Tables["SIP_Bindings"].Rows){
                        X509Certificate cert = null;
                        if(!dr_Bind.IsNull("SSL_Certificate") && ((byte[])dr_Bind["SSL_Certificate"]).Length > 0){
                            cert = new X509Certificate((byte[])dr_Bind["SSL_Certificate"]);
                        }

                        if(dr_Bind["Protocol"].ToString().ToUpper() == "TCP"){
                            sipIpBinds.Add(new BindInfo(
                                IPAddress.Parse(ConvertEx.ToString(dr_Bind["IP"])),
                                ConvertEx.ToInt32(dr_Bind["Port"]),
                                ConvertEx.ToBoolean(dr_Bind["SSL"]),
                                cert
                            ));
                        }
                        else{
                            sipIpBinds.Add(new BindInfo(
                                BindInfoProtocol.UDP,
                                IPAddress.Parse(ConvertEx.ToString(dr_Bind["IP"])),
                                ConvertEx.ToInt32(dr_Bind["Port"])
                            ));
                        }
                    }
                    m_pSipServer.BindInfo = sipIpBinds.ToArray();

                    m_pSipServer.Enabled           = ConvertEx.ToBoolean(dr["SIP_Enabled"]);
                    m_pSipServer.HostName          = dr["SIP_HostName"].ToString();
                    m_pSipServer.MinimumExpireTime = ConvertEx.ToInt32(dr["SIP_MinExpires"]);

                    #endregion

                    #region LOGGING

                    //----- Logging settings -------------------------------------//
                    try{
					    m_pSmtpServer.LogCommands  = ConvertEx.ToBoolean(dr["LogSMTPCmds"],false);
					    m_pPop3Server.LogCommands  = ConvertEx.ToBoolean(dr["LogPOP3Cmds"],false);
					    m_pImapServer.LogCommands  = ConvertEx.ToBoolean(dr["LogIMAPCmds"],false);
					    m_pFetchServer.LogCommands = ConvertEx.ToBoolean(dr["LogFetchPOP3Cmds"],false);
    					
					    m_SMTP_LogPath   = ConvertEx.ToString(dr["SMTP_LogPath"])   + "\\";
					    m_POP3_LogPath   = ConvertEx.ToString(dr["POP3_LogPath"])   + "\\";
					    m_IMAP_LogPath   = ConvertEx.ToString(dr["IMAP_LogPath"])   + "\\";
					    m_Server_LogPath = ConvertEx.ToString(dr["Server_LogPath"]) + "\\";
					    m_Fetch_LogPath  = ConvertEx.ToString(dr["FetchPOP3_LogPath"]) + "\\";
    					
					    //----- If no log path, use default ----------------
					    if(dr["SMTP_LogPath"].ToString().Trim().Length == 0){
						    m_SMTP_LogPath = m_pOwnerServer.StartupPath + "Logs\\SMTP\\";
					    }
					    if(dr["POP3_LogPath"].ToString().Trim().Length == 0){
						    m_POP3_LogPath = m_pOwnerServer.StartupPath + "Logs\\POP3\\";
					    }
					    if(dr["IMAP_LogPath"].ToString().Trim().Length == 0){
						    m_IMAP_LogPath = m_pOwnerServer.StartupPath + "Logs\\IMAP\\";
					    }
					    if(dr["Server_LogPath"].ToString().Trim().Length == 0){
						    m_Server_LogPath = m_pOwnerServer.StartupPath + "Logs\\Server\\";
					    }					
					    if(dr["FetchPOP3_LogPath"].ToString().Trim().Length == 0){
						    m_Fetch_LogPath = m_pOwnerServer.StartupPath + "Logs\\FetchPOP3\\";
					    }
					    m_pFetchServer.LogPath = m_Fetch_LogPath;
				    }
                    catch(Exception x){
                        OnError(x);
                    }
					//------------------------------------------------------------//

                    #endregion

			//		SCore.WriteLog(m_Server_LogPath + "server.log","//---- Server settings loaded " + DateTime.Now);
				}
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

        #region method IsRelayAllowed

		/// <summary>
		/// Checks if relay is allowed to specified User/IP.
        /// First user 'Allow Relay' checked, if not allowed, then checked if relay denied for that IP,
        /// at last checks if relay is allowed for that IP.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="ip"></param>
		/// <returns>Returns true if relay is allowed.</returns>
		private bool IsRelayAllowed(string userName,IPAddress ip)
		{
			if(userName != null && userName.Length > 0){
                if((m_pApi.GetUserPermissions(userName) & UserPermissions_enum.Relay) != 0){
                    return true;
                }
            }
            			
            using(DataView dv = m_pApi.GetSecurityList()){
                // Check if ip is denied
                foreach(DataRowView drV in dv){
                    if(Convert.ToBoolean(drV["Enabled"]) && Convert.ToInt32(drV["Service"]) == (int)Service_enum.Relay && Convert.ToInt32(drV["Action"]) == (int)IPSecurityAction_enum.Deny){
                        // See if IP matches range
                        if(Core.CompareIP(IPAddress.Parse(drV["StartIP"].ToString()),ip) >= 0 && Core.CompareIP(IPAddress.Parse(drV["EndIP"].ToString()),ip) <= 0){
                            return false;
                        }
                    }
                }

                // Check if ip is allowed
                foreach(DataRowView drV in dv){
                    if(Convert.ToBoolean(drV["Enabled"]) && Convert.ToInt32(drV["Service"]) == (int)Service_enum.Relay && Convert.ToInt32(drV["Action"]) == (int)IPSecurityAction_enum.Allow){
                        // See if IP matches range
                        if(Core.CompareIP(IPAddress.Parse(drV["StartIP"].ToString()),ip) >= 0 && Core.CompareIP(IPAddress.Parse(drV["EndIP"].ToString()),ip) <= 0){
                            return true;
                        }
                    }
                }
            }

			return false;
		}
		
		#endregion

        #region method IsAccessAllowed

		/// <summary>
		/// Checks if specified service access is allowed for specified IP.
		/// </summary>
		/// <param name="service">SMTP or POP3 or IMAP.</param>
		/// <param name="ip"></param>
		/// <returns>Returns true if allowed.</returns>
		public bool IsAccessAllowed(Service_enum service,IPAddress ip)
		{
			using(DataView dv = m_pApi.GetSecurityList()){
                // Check if ip is denied
                foreach(DataRowView drV in dv){
                    if(Convert.ToBoolean(drV["Enabled"]) && Convert.ToInt32(drV["Service"]) == (int)service && Convert.ToInt32(drV["Action"]) == (int)IPSecurityAction_enum.Deny){
                        // See if IP matches range
                        if(Core.CompareIP(IPAddress.Parse(drV["StartIP"].ToString()),ip) >= 0 && Core.CompareIP(IPAddress.Parse(drV["EndIP"].ToString()),ip) <= 0){
                            return false;
                        }
                    }
                }

                // Check if ip is allowed
                foreach(DataRowView drV in dv){
                    if(Convert.ToBoolean(drV["Enabled"]) && Convert.ToInt32(drV["Service"]) == (int)service && Convert.ToInt32(drV["Action"]) == (int)IPSecurityAction_enum.Allow){
                        // See if IP matches range
                        if(Core.CompareIP(IPAddress.Parse(drV["StartIP"].ToString()),ip) >= 0 && Core.CompareIP(IPAddress.Parse(drV["EndIP"].ToString()),ip) <= 0){
                            return true;
                        }
                    }
                }
            }
            
            return false;
		}
		
		#endregion


        #region method OnError

        /// <summary>
        /// Is called when error happens.
        /// </summary>
        /// <param name="x"></param>
        private void OnError(Exception x)
        {
            Error.DumpError(this.Name,x);
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets virtual server ID.
        /// </summary>
        public string ID
        {
            get{ return m_ID; }
        }

        /// <summary>
        /// Starts or stops server.
        /// </summary>
        public bool Enabled
        {
            get{ return m_Running; }

            set{
                if(value){
                    Start();
                }
                else{
                    Stop();
                }
            }
        }

        /// <summary>
        /// Gets virtual server name
        /// </summary>
        public string Name
        {
            get{ return m_Name; }
        }

        /// <summary>
        /// Gets this virtual server API.
        /// </summary>
        public IMailServerApi API
        {
            get{ return m_pApi; }
        }

        /// <summary>
        /// Gets mailstore path.
        /// </summary>
        public string MailStorePath
        {
            get{ return m_MailStorePath; }
        }

        /// <summary>
        /// Gets this virtual server SMTP server. Returns null if server is stopped.
        /// </summary>
        public SMTP_Server SmtpServer
        {
            get{ return m_pSmtpServer; }
        }

        /// <summary>
        /// Gets this virtual server POP3 server. Returns null if server is stopped.
        /// </summary>
        public POP3_Server Pop3Server
        {
            get{ return m_pPop3Server; }
        }

        /// <summary>
        /// Gets this virtual server IMAP server. Returns null if server is stopped.
        /// </summary>
        public IMAP_Server ImapServer
        {
            get{ return m_pImapServer; }
        }

        /// <summary>
        /// Gets this virtual server Relay server. Returns null if server is stopped.
        /// </summary>
        public Relay_Server RelayServer
        {
            get{ return m_pRelayServer; }
        }

        /// <summary>
        /// Gets this virtual server SIP server. Returns null if server is stopped.
        /// </summary>
        public SIP_Stack SipServer
        {
            get{ return m_pSipServer; }
        }


        //---- ?? Used by management server log viewer.

        internal string SMTP_LogsPath
        {
            get{ return m_SMTP_LogPath; }
        }

        internal string POP3_LogsPath
        {
            get{ return m_POP3_LogPath; }
        }

        internal string IMAP_LogsPath
        {
            get{ return m_IMAP_LogPath; }
        }

        internal string RELAY_LogsPath
        {
            get{ return m_Relay_LogPath; }
        }

        internal string FETCH_LogsPath
        {
            get{ return m_Fetch_LogPath; }
        }
                
        #endregion

    }
}
