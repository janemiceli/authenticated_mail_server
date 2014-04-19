using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
//using System.IO.Compression;

using ICSharpCode.SharpZipLib.GZip;

using LumiSoft.Net;
using LumiSoft.Net.SMTP.Server;
using LumiSoft.Net.POP3.Server;
using LumiSoft.Net.IMAP;
using LumiSoft.Net.IMAP.Server;
using LumiSoft.MailServer.Relay;
using LumiSoft.Net.SIP;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;
using LumiSoft.Net.SIP.Proxy;

namespace LumiSoft.MailServer.Monitoring
{
	/// <summary>
	/// Monitoring server session.
	/// </summary>
	public class MonitoringServerSession : SocketServerSession
	{
		private MonitoringServer m_pServer      = null;
		private int              m_BadCmdCount  = 0;

		/// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sessionID">Session ID.</param>
        /// <param name="socket">Server connected socket.</param>
        /// <param name="bindInfo">BindInfo what accepted socket.</param>
        /// <param name="server">Reference to server.</param>
        internal MonitoringServerSession(string sessionID,SocketEx socket,BindInfo bindInfo,MonitoringServer server) : base(sessionID,socket,bindInfo,server)
        {
            m_pServer = server;

			// Start session proccessing
			StartSession();
		}


		#region method StartSession

		/// <summary>
		/// Starts session.
		/// </summary>
		private void StartSession()
		{			
			try{
                //--- Validate connecting IP --------------------------------------------------------------//
                DataSet dsSettings = new DataSet();
                dsSettings.Tables.Add("IP_Access");
                dsSettings.Tables["IP_Access"].Columns.Add("StartIP");
                dsSettings.Tables["IP_Access"].Columns.Add("EndIP");

                if(File.Exists(SCore.PathFix(m_pServer.MailServer.StartupPath + "Settings\\AdminAccess.xml"))){
                    dsSettings.ReadXml(SCore.PathFix(m_pServer.MailServer.StartupPath + "Settings\\AdminAccess.xml"));
                }
                else{
                    // There is no access conf file, add default values.

                    DataRow dr = dsSettings.Tables["IP_Access"].NewRow();
                    dr["StartIP"] = "127.0.0.1";
                    dr["EndIP"] = "127.0.0.1";
                    dsSettings.Tables["IP_Access"].Rows.Add(dr);
                }

                bool canAcces = false;
                IPAddress ip = ((IPEndPoint)this.Socket.RemoteEndPoint).Address;
                foreach(DataRow dr in dsSettings.Tables["IP_Access"].Rows){
                    // See if IP matches range
                    if(Core.CompareIP(IPAddress.Parse(dr["StartIP"].ToString()),ip) >= 0 && Core.CompareIP(IPAddress.Parse(dr["EndIP"].ToString()),ip) <= 0){
                        canAcces = true;
                        break;
                    }                    
                }
                if(!canAcces){
                    this.Socket.Disconnect();
                    return;
                }
                //------------------------------------------------------------------------------------------//

                // Add session to session list
			    m_pServer.AddSessionX(this);

				this.Socket.WriteLine("+OK " + m_pServer.HostName + " Monitoring Server ready");

				BeginRecieveCmd();
			}
			catch(Exception x){
				OnError(x);
			}
		}

		#endregion

		#region method EndSession

		/// <summary>
		/// Ends session, closes socket.
		/// </summary>
		private void EndSession()
		{
            try{
				if(this.Socket != null){
					this.Socket.Shutdown(SocketShutdown.Both);
					this.Socket.Disconnect();
					//this.Socket = null;
				}
			}
			catch{ // We don't need to check errors here, because they only may be Socket closing errors.
			}
			finally{
				m_pServer.RemoveSessionX(this);
			}
		}

		#endregion


		#region method OnSessionTimeout

		/// <summary>
		/// Is called by server when session has timed out.
		/// </summary>
		protected override void OnSessionTimeout()
		{           
			try{
				this.Socket.WriteLine("-ERR Session timeout, closing transmission channel");
			}
			catch{
			}

			EndSession();
		}

		#endregion

		#region method OnError

		/// <summary>
		/// Is called when error occures.
		/// </summary>
		/// <param name="x"></param>
		private void OnError(Exception x)
		{
			EndSession();
		}

		#endregion


		#region method BeginRecieveCmd
		
		/// <summary>
		/// Starts recieveing command.
		/// </summary>
		private void BeginRecieveCmd()
		{
			MemoryStream strm = new MemoryStream();
			this.Socket.BeginReadLine(strm,64000,strm,new SocketCallBack(this.EndRecieveCmd));
		}

		#endregion

		#region method EndRecieveCmd

		/// <summary>
		/// Is called if command is recieved.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="count"></param>
		/// <param name="exception"></param>
		/// <param name="tag"></param>
		private void EndRecieveCmd(SocketCallBackResult result,long count,Exception exception,object tag)
		{
			try{
				switch(result)
				{
					case SocketCallBackResult.Ok:
						MemoryStream strm = (MemoryStream)tag;

						string cmdLine = this.Socket.Encoding.GetString(strm.ToArray());

						// Exceute command
						if(SwitchCommand(cmdLine)){
							// Session end, close session
							EndSession();
						}
						break;

					case SocketCallBackResult.LengthExceeded:
						this.Socket.WriteLine("-ERR Line too long.");

						BeginRecieveCmd();
						break;

					case SocketCallBackResult.SocketClosed:
						EndSession();
						break;

					case SocketCallBackResult.Exception:
						OnError(exception);
						break;
				}
			}
			catch(Exception x){
				 OnError(x);
			}
		}

		#endregion


		#region method SwitchCommand

		/// <summary>
		/// Executes command.
		/// </summary>
		/// <param name="commandText">Original command text.</param>
		/// <returns>Returns true if must end session(command loop).</returns>
		private bool SwitchCommand(string commandText)
		{
			//---- Parse command --------------------------------------------------//
			string[] cmdParts = commandText.TrimStart().Split(new char[]{' '});
			string command = cmdParts[0].ToUpper().Trim();
			string argsText = Core.GetArgsText(commandText,command);
			//---------------------------------------------------------------------//

			bool getNextCmd = true;

			switch(command)
			{	
                case "NOOP":
				    Noop();
			    break;

                case "LOGIN":
				    Login(argsText);
					break;


                case "GETSERVERINFO":
				    GetServerInfo();
					break;

                case "GETIPADDRESSES":
				    GetIPAddresses();
					break;

                case "KILLSESSION":
				    KillSession(argsText);
					break;

                case "GETSESSIONS":
				    GetSessions();
					break;

                case "GETEVENTS":
				    GetEvents();
					break;

                case "GETSIPREGISTRATIONS":
				    GetSipRegistrations(argsText);
					break;

                case "GETSIPREGISTRATION":
				    GetSipRegistration(argsText);
					break;

                case "SETSIPREGISTRATION":
				    SetSipRegistration(argsText);
					break;

                case "DELETESIPREGISTRATION":
				    DeleteSipRegistration(argsText);
					break;

                case "CLEAREVENTS":
				    ClearEvents();
					break;

                case "GETLOGSESSIONS":
				    GetLogSessions(argsText);
					break;

                 case "GETSESSIONLOG":
				    GetSessionLog(argsText);
					break;

                case "GETVIRTUALSERVERS":
					GetVirtualServers();
					break;

                case "GETVIRTUALSERVERAPIS":
					GetVirtualServerAPIs();
					break;

                case "ADDVIRTUALSERVER":
					AddVirtualServer(argsText);
					break;

                case "UPDATEVIRTUALSERVER":
					UpdateVirtualServer(argsText);
					break;

                case "DELETEVIRTUALSERVER":
					DeleteVirtualServers(argsText);
					break;

                case "GETSETTINGS":
					GetSettings(argsText);
					break;

                case "UPDATESETTINGS":
					UpdateSettings(argsText);
					break;

                case "GETDOMAINS":
					GetDomains(argsText);
					break;


                case "ADDDOMAIN":
					AddDomain(argsText);
					break;

                case "UPDATEDOMAIN":
					UpdateDomain(argsText);
					break;

                case "DELETEDOMAIN":
					DeleteDomain(argsText);
					break;


                case "GETUSERS":
					GetUsers(argsText);
					break;

                case "ADDUSER":
					AddUser(argsText);
					break;

                case "UPDATEUSER":
					UpdateUser(argsText);
					break;

                case "DELETEUSER":
					DeleteUser(argsText);
					break;

                case "GETUSEREMAILADDRESSES":
					GetUserEmailAddresses(argsText);
					break;

                case "ADDUSEREMAILADDRESS":
					AddUserEmailAddress(argsText);
					break;

                case "DELETEUSEREMAILADDRESS":
					DeleteUserEmailAddress(argsText);
					break;

                case "GETUSERMESSAGERULES":
					GetUserMessageRules(argsText);
					break;

                case "ADDUSERMESSAGERULE":
					AddUserMessageRule(argsText);
					break;

                case "UPDATEUSERMESSAGERULE":
					UpdateUserMessageRule(argsText);
					break;

                case "DELETEUSERMESSAGERULE":
					DeleteUserMessageRule(argsText);
					break;

                case "GETUSERMESSAGERULEACTIONS":
					GetUserMessageRuleActions(argsText);
					break;

                case "ADDUSERMESSAGERULEACTION":
					AddUserMessageRuleAction(argsText);
					break;

                case "UPDATEUSERMESSAGERULEACTION":
					UpdateUserMessageRuleAction(argsText);
					break;

                case "DELETEUSERMESSAGERULEACTION":
					DeleteUserMessageRuleAction(argsText);
					break;


                case "GETUSERMAILBOXSIZE":
					GetUserMailboxSize(argsText);
					break;

                case "GETUSERLASTLOGINTIME":
					GetUserLastLoginTime(argsText);
					break;

                case "GETUSERFOLDERS":
					GetUserFolders(argsText);
					break;

                case "ADDUSERFOLDER":
					AddUserFolder(argsText);
					break;

                case "DELETEUSERFOLDER":
					DeleteUserFolder(argsText);
					break;

                case "RENAMEUSERFOLDER":
					RenameUserFolder(argsText);
					break;

                case "GETUSERFOLDERINFO":
					GetUserFolderInfo(argsText);
					break;

                case "GETUSERFOLDERMESSAGESINFO":
					GetUserFolderMessagesInfo(argsText);
					break;

                case "GETUSERFOLDERMESSAGE":
					GetUserFolderMessage(argsText);
					break;

                case "STOREUSERFOLDERMESSAGE":
					StoreUserFolderMessage(argsText);
					break;

                case "DELETEUSERFOLDERMESSAGE":
					DeleteUserFolderMessage(argsText);
					break;

                case "GETUSERREMOTESERVERS":
					GetUserRemoteServers(argsText);
					break;

                case "ADDUSERREMOTESERVER":
					AddUserRemoteServer(argsText);
					break;

                case "UPDATEUSERREMOTESERVER":
					UpdateUserRemoteServer(argsText);
					break;

                case "DELETEUSERREMOTESERVER":
					DeleteUserRemoteServer(argsText);
					break;

                case "GETUSERFOLDERACL":
					GetUserFolderAcl(argsText);
					break;

                case "SETUSERFOLDERACL":
					SetUserFolderAcl(argsText);
					break;

                case "DELETEUSERFOLDERACL":
					DeleteUserFolderAcl(argsText);
					break;


                case "GETUSERSDEFAULTFOLDERS":
					GetUsersDefaultFolders(argsText);
					break;

                case "ADDUSERSDEFAULTFOLDER":
					AddUsersDefaultFolder(argsText);
					break;

                case "DELETEUSERSDEFAULTFOLDER":
					DeleteUsersDefaultFolder(argsText);
					break;


                case "GETGROUPS":
					GetGroups(argsText);
					break;

                case "GETGROUPMEMBERS":
					GetGroupMembers(argsText);
					break;

                case "ADDGROUP":
					AddGroup(argsText);
					break;

                case "UPDATEGROUP":
					UpdateGroup(argsText);
					break;

                case "DELETEGROUP":
					DeleteGroup(argsText);
					break;

                case "ADDGROUPMEMBER":
					AddGroupMember(argsText);
					break;

                case "DELETEGROUPMEMBER":
					DeleteGroupMember(argsText);
					break;

                case "GETMAILINGLISTS":
					GetMailingLists(argsText);
					break;

                case "ADDMAILINGLIST":
					AddMailingList(argsText);
					break;
                                    
                case "UPDATEMAILINGLIST":
					UpdateMailingList(argsText);
					break;

                case "DELETEMAILINGLIST":
					DeleteMailingList(argsText);
					break;

                case "GETMAILINGLISTMEMBERS":
					GetMailingListMembers(argsText);
					break;

                case "ADDMAILINGLISTMEMBER":
					AddMailingListMember(argsText);
					break;

                case "DELETEMAILINGLISTMEMBER":
					DeleteMailingListMember(argsText);
					break;

                case "GETMAILINGLISTACL":
					GetMailingListAcl(argsText);
					break;

                case "ADDMAILINGLISTACL":
					AddMailingListAcl(argsText);
					break;

                case "DELETEMAILINGLISTACL":
					DeleteMailingListAcl(argsText);
					break;

                case "GETGLOBALMESSAGERULES":
					GetGlobalMessageRules(argsText);
					break;

                case "ADDGLOBALMESSAGERULE":
					AddGlobalMessageRule(argsText);
					break;

                case "UPDATEGLOBALMESSAGERULE":
					UpdateGlobalMessageRule(argsText);
					break;

                case "DELETEGLOBALMESSAGERULE":
					DeleteGlobalMessageRule(argsText);
					break;

                case "GETGLOBALMESSAGERULEACTIONS":
					GetGlobalMessageRuleActions(argsText);
					break;

                case "ADDGLOBALMESSAGERULEACTION":
					AddGlobalMessageRuleAction(argsText);
					break;

                case "UPDATEGLOBALMESSAGERULEACTION":
					UpdateGlobalMessageRuleAction(argsText);
					break;

                case "DELETEGLOBALMESSAGERULEACTION":
					DeleteGlobalMessageRuleAction(argsText);
					break;

                case "GETROUTES":
					GetRoutes(argsText);
					break;

                case "ADDROUTE":
					AddRoute(argsText);
					break;

                case "UPDATEROUTE":
					UpdateRoute(argsText);
					break;

                case "DELETEROUTE":
					DeleteRoute(argsText);
					break;

                case "GETSHAREDROOTFOLDERS":
					GetSharedRootFolders(argsText);
					break;

                case "ADDSHAREDROOTFOLDER":
					AddSharedRootFolder(argsText);
					break;

                case "UPDATESHAREDROOTFOLDER":
					UpdateSharedRootFolder(argsText);
					break;

                case "DELETESHAREDROOTFOLDER":
					DeleteSharedRootFolder(argsText);
					break;

                case "GETFILTERTYPES":
					GetFilterTypes(argsText);
					break;

                case "GETFILTERS":
					GetFilters(argsText);
					break;

                case "ADDFILTER":
					AddFilter(argsText);
					break;

                case "UPDATEFILTER":
					UpdateFilter(argsText);
					break;

                case "DELETEFILTER":
					DeleteFilter(argsText);
					break;

                case "GETQUEUE":
					GetQueue(argsText);
					break;

                case "GETIPSECURITY":
					GetIPSecurity(argsText);
					break;

                case "ADDIPSECURITYENTRY":
					AddIPSecurityEntry(argsText);
					break;

                case "UPDATEIPSECURITYENTRY":
					UpdateIPSecurityEntry(argsText);
					break;

                case "DELETEIPSECURITYENTRY":
					DeleteIPSecurityEntry(argsText);
					break;


                case "GETRECYCLEBINSETTINGS":
					GetRecycleBinSettings(argsText);
					break;

                case "UPDATERECYCLEBINSETTINGS":
					UpdateRecycleBinSettings(argsText);
					break;

                case "GETRECYCLEBINMESSAGESINFO":
					GetRecycleBinMessagesInfo(argsText);
					break;

                case "GETRECYCLEBINMESSAGE":
					GetRecycleBinMessage(argsText);
					break;

                case "RESTORERECYCLEBINMESSAGE":
					RestoreRecycleBinMessage(argsText);
					break;
                                    
			
				case "QUIT":
					QUIT();
					getNextCmd = false;
					return true;
										
				default:					
					this.Socket.WriteLine("-ERR command '" + command + "' unrecognized");

					//---- Check that maximum bad commands count isn't exceeded ---------------//
					if(m_BadCmdCount > m_pServer.MaxBadCommands-1){
						this.Socket.WriteLine("-ERR Too many bad commands, closing transmission channel");
						return true;
					}
					m_BadCmdCount++;
					//-------------------------------------------------------------------------//

					break;				
			}

			if(getNextCmd){
				BeginRecieveCmd();
			}
			
			return false;
		}

		#endregion



        #region method Noop

        private void Noop()
        {
            this.Socket.WriteLine("+OK");
        }

        #endregion


        #region method Login

        private void Login(string argsText)
        {
            /* Login "<UserName>" "<password>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: Login \"<UserName>\" \"<password>\"");
                    return;
                }

                DataSet dsSettings = new DataSet();
                dsSettings.Tables.Add("Users");
                dsSettings.Tables["Users"].Columns.Add("UserName");
                dsSettings.Tables["Users"].Columns.Add("Password");

                if(File.Exists(SCore.PathFix(m_pServer.MailServer.StartupPath + "Settings\\AdminAccess.xml"))){
                    dsSettings.ReadXml(SCore.PathFix(m_pServer.MailServer.StartupPath + "Settings\\AdminAccess.xml"));
                }
                else{
                    // There is no access conf file, add default values.

                    DataRow dr = dsSettings.Tables["Users"].NewRow();
                    dr["UserName"] = "Administrator";
                    dr["Password"] = "";
                    dsSettings.Tables["Users"].Rows.Add(dr);
                }

                foreach(DataRow dr in dsSettings.Tables["Users"].Rows){
                    if(dr["UserName"].ToString() == TextUtils.UnQuoteString(args[0]) && dr["Password"].ToString() == TextUtils.UnQuoteString(args[1])){
                        // Store session authenticated user name.
                        this.SetUserName(dr["UserName"].ToString());

                        this.Socket.WriteLine("+OK");                        
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR LOGIN failed, invalid user name or password !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetServerInfo

        private void GetServerInfo()
        {
            /* GetServerInfo
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                DataSet ds = new DataSet("dsServerInfo");
                ds.Tables.Add("ServerInfo");
                ds.Tables["ServerInfo"].Columns.Add("OS");
                ds.Tables["ServerInfo"].Columns.Add("MailServerVersion");
                ds.Tables["ServerInfo"].Columns.Add("MemoryUsage");
                ds.Tables["ServerInfo"].Columns.Add("CpuUsage");
                ds.Tables["ServerInfo"].Columns.Add("ServerStartTime",typeof(DateTime));
                ds.Tables["ServerInfo"].Columns.Add("Read_KB_Sec");
                ds.Tables["ServerInfo"].Columns.Add("Write_KB_Sec");
                ds.Tables["ServerInfo"].Columns.Add("SmtpSessions");
                ds.Tables["ServerInfo"].Columns.Add("Pop3Sessions");
                ds.Tables["ServerInfo"].Columns.Add("ImapSessions");
                ds.Tables["ServerInfo"].Columns.Add("RelaySessions");

                
                // Calculate CPU usage
                TimeSpan start = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
                System.Threading.Thread.Sleep(100);
                TimeSpan end = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
                            

                // Calculate read/write KB/Sec and get total sessions count
                ArrayList allSessions = new ArrayList(m_pServer.MailServer.Sessions);
                int  smtpSessions  = 0;
                int  pop3Sessions  = 0;
                int  imapSessions  = 0;
                int  relaySessions = 0;                
                long startReads    = 0;
                long startWrites   = 0;
                foreach(object session in allSessions){
                    try{
                        if(session is SMTP_Session){
                            startReads  += ((SMTP_Session)session).ReadedCount;
                            startWrites += ((SMTP_Session)session).WrittenCount;
                            smtpSessions++;
                        }
                        else if(session is POP3_Session){
                            startReads  += ((POP3_Session)session).ReadedCount;
                            startWrites += ((POP3_Session)session).WrittenCount;
                            pop3Sessions++;
                        }
                        else if(session is IMAP_Session){
                            startReads  += ((IMAP_Session)session).ReadedCount;
                            startWrites += ((IMAP_Session)session).WrittenCount;
                            imapSessions++;
                        }
                        else if(session is Relay_Session){
                            startReads  += ((Relay_Session)session).ReadedCount;
                            startWrites += ((Relay_Session)session).WrittenCount;
                            relaySessions++;
                        }
                    }
                    catch{
                    }
                }
                System.Threading.Thread.Sleep(200);
                long endReads  = 0;
                long endWrites = 0;
                foreach(object session in allSessions){
                    try{
                        if(session is SMTP_Session){
                            endReads  += ((SMTP_Session)session).ReadedCount;
                            endWrites += ((SMTP_Session)session).WrittenCount;
                        }
                        else if(session is POP3_Session){
                            endReads  += ((POP3_Session)session).ReadedCount;
                            endWrites += ((POP3_Session)session).WrittenCount;
                        }
                        else if(session is IMAP_Session){
                            endReads  += ((IMAP_Session)session).ReadedCount;
                            endWrites += ((IMAP_Session)session).WrittenCount;
                        }
                        else if(session is Relay_Session){
                            endReads  += ((Relay_Session)session).ReadedCount;
                            endWrites += ((Relay_Session)session).WrittenCount;
                        }
                    }
                    catch{
                    }
                }


                DataRow dr = ds.Tables["ServerInfo"].NewRow();
                dr["OS"]                = Environment.OSVersion.Platform.ToString();
                dr["MailServerVersion"] = "0.92";
                dr["MemoryUsage"]       = (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1000000);
                dr["CpuUsage"]          = Math.Min((int)(((decimal)(end - start).Milliseconds / 100) * 100),100);
                dr["ServerStartTime"]   = m_pServer.MailServer.StartTime;
                dr["Read_KB_Sec"]       = Math.Max((endReads - startReads) * 5,0) / 1000;
                dr["Write_KB_Sec"]      = Math.Max((endWrites - startWrites) * 5,0) / 1000;
                dr["SmtpSessions"]      = smtpSessions;
                dr["Pop3Sessions"]      = pop3Sessions;
                dr["ImapSessions"]      = imapSessions;
                dr["RelaySessions"]     = relaySessions;
                ds.Tables["ServerInfo"].Rows.Add(dr);
                
                // Compress data
                byte[] dsZipped = CompressDataSet(ds);

                this.Socket.WriteLine("+OK " + dsZipped.Length);
                this.Socket.Write(dsZipped);
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetIPAddresses

        private void GetIPAddresses()
        {
            /* GetIPAddresses
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                DataSet ds = new DataSet("dsIPs");
                ds.Tables.Add("dsIPs");
                ds.Tables["dsIPs"].Columns.Add("IP");

                // Add IPv4 Any
                DataRow dr = ds.Tables["dsIPs"].NewRow();
                dr["IP"] = IPAddress.Any.ToString();
                ds.Tables["dsIPs"].Rows.Add(dr);
                // Add IPv4 localhost
                dr = ds.Tables["dsIPs"].NewRow();
                dr["IP"] = IPAddress.Loopback.ToString();
                ds.Tables["dsIPs"].Rows.Add(dr);
                // Add IPv6 entries if OS support IPv6
                // FIX ME: mono won't support this property yet
                //if(System.Net.Sockets.Socket.OSSupportsIPv6){
                    // Add IPv6 Any
                    dr = ds.Tables["dsIPs"].NewRow();
                    dr["IP"] = IPAddress.IPv6Any.ToString();
                    ds.Tables["dsIPs"].Rows.Add(dr);
                    // Add IPv6 localhost
                    dr = ds.Tables["dsIPs"].NewRow();
                    dr["IP"] = IPAddress.IPv6Loopback.ToString();
                    ds.Tables["dsIPs"].Rows.Add(dr);
                //}
                
                IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());			
		        foreach(IPAddress ip in hostInfo.AddressList){
                    dr = ds.Tables["dsIPs"].NewRow();
                    dr["IP"] = ip.ToString();
                    ds.Tables["dsIPs"].Rows.Add(dr);
			    }

                // Compress data
                byte[] dsZipped = CompressDataSet(ds);

                this.Socket.WriteLine("+OK " + dsZipped.Length);
                this.Socket.Write(dsZipped);
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method KillSession

        private void KillSession(string argsText)
        {
            /* KillSession "<sessionID>"
                  Responses:
                    +OK <sizeOfData>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 1){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: KillSession \"<sessionID>\"");
                    return;
                }

                foreach(object session in m_pServer.MailServer.Sessions){
                    if(session is SMTP_Session){
                        SMTP_Session s = (SMTP_Session)session;
                        if(s.SessionID == args[0]){
                            s.Kill();                            
                            this.Socket.WriteLine("+OK");
                            return;
                        }
                    }
                    else if(session is POP3_Session){
                        POP3_Session s = (POP3_Session)session;
                        if(s.SessionID == args[0]){
                            s.Kill();                            
                            this.Socket.WriteLine("+OK");
                            return;
                        }
                    }
                    else if(session is IMAP_Session){
                        IMAP_Session s = (IMAP_Session)session;
                        if(s.SessionID == args[0]){
                            s.Kill();                            
                            this.Socket.WriteLine("+OK");
                            return;
                        }
                    }
                    else if(session is Relay_Session){
                        Relay_Session s = (Relay_Session)session;
                        if(s.SessionID == args[0]){
                            s.Kill("Session killed by administrator !");                            
                            this.Socket.WriteLine("+OK");
                            return;
                        }
                    }
                    else if(session is MonitoringServerSession){
                        MonitoringServerSession s = (MonitoringServerSession)session;
                        if(s.SessionID == args[0]){
                            s.Kill();                            
                            this.Socket.WriteLine("+OK");
                            return;
                        }
                    }
                }

                this.Socket.WriteLine("+OK");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetSessions

        private void GetSessions()
        {
            /* GetSessions
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                DataSet ds = new DataSet();
			    DataTable dt = ds.Tables.Add("Sessions");
            //  dt.Columns.Add("VirtualServer");
			    dt.Columns.Add("SessionType");
			    dt.Columns.Add("SessionID");
			    dt.Columns.Add("UserName");
			    dt.Columns.Add("LocalEndPoint");
			    dt.Columns.Add("RemoteEndPoint");
			    dt.Columns.Add("SessionStartTime",typeof(DateTime));
                dt.Columns.Add("ExpectedTimeout").DefaultValue = 0;
                dt.Columns.Add("SessionLog");
                dt.Columns.Add("ReadedCount").DefaultValue = 0;
                dt.Columns.Add("WrittenCount").DefaultValue = 0;
                dt.Columns.Add("ReadTransferRate").DefaultValue = 0;
                dt.Columns.Add("WriteTransferRate").DefaultValue = 0;
                dt.Columns.Add("IsSecureConnection").DefaultValue = 0;

                ArrayList allSessions = new ArrayList(m_pServer.MailServer.Sessions);

                // Store inital Readed Written count. That is needed for transfer ratre calculation.
                Hashtable transferRatesHolder = new Hashtable();
                foreach(object session in allSessions){
                    if(session is SMTP_Session){
                        transferRatesHolder.Add(session,new object[]{((SMTP_Session)session).ReadedCount,((SMTP_Session)session).WrittenCount});
                    }
                    else if(session is POP3_Session){
                        transferRatesHolder.Add(session,new object[]{((POP3_Session)session).ReadedCount,((POP3_Session)session).WrittenCount});
                    }
                    else if(session is IMAP_Session){
                        transferRatesHolder.Add(session,new object[]{((IMAP_Session)session).ReadedCount,((IMAP_Session)session).WrittenCount});
                    }
                    else if(session is Relay_Session){
                        transferRatesHolder.Add(session,new object[]{((Relay_Session)session).ReadedCount,((Relay_Session)session).WrittenCount});
                    }
                    else if(session is MonitoringServerSession){
                        transferRatesHolder.Add(session,new object[]{((MonitoringServerSession)session).ReadedCount,((MonitoringServerSession)session).WrittenCount});
                    }
                }
                // Sleep 1 sec
                System.Threading.Thread.Sleep(1000);
                
                foreach(object session in allSessions){
                    try{
                        if(session is SMTP_Session){
                            SMTP_Session smtpSession = (SMTP_Session)session;

                            DataRow dr = dt.NewRow();
					        dr["SessionType"]        = "SMTP";
					        dr["SessionID"]          = smtpSession.SessionID;
					        dr["UserName"]           = smtpSession.UserName;
					        dr["LocalEndPoint"]      = ObjectToString(smtpSession.LocalEndPoint);
					        dr["RemoteEndPoint"]     = ObjectToString(smtpSession.RemoteEndPoint);
					        dr["SessionStartTime"]   = smtpSession.SessionStartTime;
                            dr["ExpectedTimeout"]    = smtpSession.ExpectedTimeout;
                            if(smtpSession.SessionActiveLog != null){
                                dr["SessionLog"]     = SocketLogger.LogEntriesToString(smtpSession.SessionActiveLog,true,true);
                            }
                            else{
                                dr["SessionLog"]     = "<Logging disabled>";
                            }
                            dr["ReadedCount"]        = smtpSession.ReadedCount;
                            dr["WrittenCount"]       = smtpSession.WrittenCount;                    
                            dr["ReadTransferRate"]   = smtpSession.ReadedCount - ((long)((object[])transferRatesHolder[session])[0]);
                            dr["WriteTransferRate"]  = smtpSession.WrittenCount - ((long)((object[])transferRatesHolder[session])[1]);
                            dr["IsSecureConnection"] = smtpSession.IsSecureConnection;  
                            dt.Rows.Add(dr);
                        }
                        else if(session is POP3_Session){
                            POP3_Session pop3Session = (POP3_Session)session;

                            DataRow dr = dt.NewRow();
					        dr["SessionType"]        = "POP3";
					        dr["SessionID"]          = pop3Session.SessionID;
					        dr["UserName"]           = pop3Session.UserName;
					        dr["LocalEndPoint"]      = ObjectToString(pop3Session.LocalEndPoint);
					        dr["RemoteEndPoint"]     = ObjectToString(pop3Session.RemoteEndPoint);
					        dr["SessionStartTime"]   = pop3Session.SessionStartTime;
                            dr["ExpectedTimeout"]    = pop3Session.ExpectedTimeout;
                            if(pop3Session.SessionActiveLog != null){
                                dr["SessionLog"]     = SocketLogger.LogEntriesToString(pop3Session.SessionActiveLog,true,true);
                            }
                            else{
                                dr["SessionLog"]     = "<Logging disabled>";
                            }
                            dr["ReadedCount"]        = pop3Session.ReadedCount;
                            dr["WrittenCount"]       = pop3Session.WrittenCount;                    
                            dr["ReadTransferRate"]   = pop3Session.ReadedCount - ((long)((object[])transferRatesHolder[session])[0]);
                            dr["WriteTransferRate"]  = pop3Session.WrittenCount - ((long)((object[])transferRatesHolder[session])[1]);
                            dr["IsSecureConnection"] = pop3Session.IsSecureConnection;
					        dt.Rows.Add(dr);
                        }
                        else if(session is IMAP_Session){
                            IMAP_Session imapSession = (IMAP_Session)session;

                            DataRow dr = dt.NewRow();
					        dr["SessionType"]        = "IMAP";
					        dr["SessionID"]          = imapSession.SessionID;
					        dr["UserName"]           = imapSession.UserName;
					        dr["LocalEndPoint"]      = ObjectToString(imapSession.LocalEndPoint);
					        dr["RemoteEndPoint"]     = ObjectToString(imapSession.RemoteEndPoint);
					        dr["SessionStartTime"]   = imapSession.SessionStartTime;
                            dr["ExpectedTimeout"]    = imapSession.ExpectedTimeout;
                            if(imapSession.SessionActiveLog != null){
                                dr["SessionLog"]     = SocketLogger.LogEntriesToString(imapSession.SessionActiveLog,true,true);
                            }
                            else{
                                dr["SessionLog"]     = "<Logging disabled>";
                            }
                            dr["ReadedCount"]        = imapSession.ReadedCount;
                            dr["WrittenCount"]       = imapSession.WrittenCount;                    
                            dr["ReadTransferRate"]   = imapSession.ReadedCount - ((long)((object[])transferRatesHolder[session])[0]);
                            dr["WriteTransferRate"]  = imapSession.WrittenCount - ((long)((object[])transferRatesHolder[session])[1]);
                            dr["IsSecureConnection"] = imapSession.IsSecureConnection;
					        dt.Rows.Add(dr);
                        }
                        else if(session is Relay_Session){
                            Relay_Session relaySession = (Relay_Session)session;

                            DataRow dr = dt.NewRow();
					        dr["SessionType"]        = "RELAY";
					        dr["SessionID"]          = relaySession.SessionID;
					        dr["UserName"]           = "";
					        dr["LocalEndPoint"]      = ObjectToString(relaySession.LocalEndPoint);
					        dr["RemoteEndPoint"]     = ObjectToString(relaySession.RemoteEndPoint);
					        dr["SessionStartTime"]   = relaySession.SessionStartTime;
                            dr["ExpectedTimeout"]    = relaySession.ExpectedTimeout;
                            if(relaySession.SessionActiveLog != null){
                                dr["SessionLog"]         = SocketLogger.LogEntriesToString(relaySession.SessionActiveLog,true,true);
                            }
                            else{
                                dr["SessionLog"]     = "<Logging disabled>";
                            }
                            dr["ReadedCount"]        = relaySession.ReadedCount;
                            dr["WrittenCount"]       = relaySession.WrittenCount;                    
                            dr["ReadTransferRate"]   = relaySession.ReadedCount - ((long)((object[])transferRatesHolder[session])[0]);
                            dr["WriteTransferRate"]  = relaySession.WrittenCount - ((long)((object[])transferRatesHolder[session])[1]);
                            dr["IsSecureConnection"] = relaySession.IsSecureConnection;
					        dt.Rows.Add(dr);
                        }
                        else if(session is MonitoringServerSession){
                            MonitoringServerSession mangementSession = (MonitoringServerSession)session;

                            DataRow dr = dt.NewRow();
					        dr["SessionType"]        = "ADMIN";
					        dr["SessionID"]          = mangementSession.SessionID;
					        dr["UserName"]           = mangementSession.UserName;
					        dr["LocalEndPoint"]      = ObjectToString(mangementSession.LocalEndPoint);
					        dr["RemoteEndPoint"]     = ObjectToString(mangementSession.RemoteEndPoint);
					        dr["SessionStartTime"]   = mangementSession.SessionStartTime;
                            dr["ExpectedTimeout"]    = mangementSession.ExpectedTimeout;
                            if(mangementSession.SessionActiveLog != null){
                                dr["SessionLog"]     = SocketLogger.LogEntriesToString(mangementSession.SessionActiveLog,true,true);
                            }
                            else{
                                dr["SessionLog"]     = "<Logging disabled>";
                            }
                            dr["ReadedCount"]        = mangementSession.ReadedCount;
                            dr["WrittenCount"]       = mangementSession.WrittenCount;                    
                            dr["ReadTransferRate"]   = mangementSession.ReadedCount - ((long)((object[])transferRatesHolder[session])[0]);
                            dr["WriteTransferRate"]  = mangementSession.WrittenCount - ((long)((object[])transferRatesHolder[session])[1]);
                            dr["IsSecureConnection"] = mangementSession.IsSecureConnection;
					        dt.Rows.Add(dr);
                        }
                    }
                    // Just skip failed session, session probably terminating.
                    catch{
                    }
                }

                // Compress data
                byte[] dsZipped = CompressDataSet(ds);

                this.Socket.WriteLine("+OK " + dsZipped.Length);
                this.Socket.Write(dsZipped);
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetSipRegistrations

        private void GetSipRegistrations(string argsText)
        {
            /* GetSipRegistrations "<virtualServerID>"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 1){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetSipRegistrations \"<virtualServerID>\"");
                    return;
                }
                                                
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        DataSet ds = new DataSet("dsSipRegistrations");
                        ds.Tables.Add("SipRegistrations");
                        ds.Tables["SipRegistrations"].Columns.Add("UserName");
                        ds.Tables["SipRegistrations"].Columns.Add("AddressOfRecord");
                        ds.Tables["SipRegistrations"].Columns.Add("Contacts");

                        foreach(SIP_Registration registration in ((SIP_ProxyCore)virtualServer.SipServer.SipCore).Registrar.Registrations){
                            DataRow dr = ds.Tables["SipRegistrations"].NewRow();
                            dr["UserName"]        = registration.UserName;
                            dr["AddressOfRecord"] = registration.Address;
                            string contacts = "";
                            foreach(SIP_RegistrationContact contact in registration.Contacts){
                                contacts += contact.ToStringValue() + "\t";
                            }
                            dr["Contacts"] = contacts;
                            ds.Tables["SipRegistrations"].Rows.Add(dr);
                        }

                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");               
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetSipRegistration

        private void GetSipRegistration(string argsText)
        {
            /* GetSipRegistration "<virtualServerID>" "<addressOfRecord>"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetSipRegistration \"<virtualServerID>\" \"<addressOfRecord>\"");
                    return;
                }
                                                
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        DataSet ds = new DataSet("dsSipRegistration");
                        ds.Tables.Add("Contacts");
                        ds.Tables["Contacts"].Columns.Add("Value");

                        foreach(SIP_Registration registration in ((SIP_ProxyCore)virtualServer.SipServer.SipCore).Registrar.Registrations){
                            if(args[1].ToLower() == registration.Address.ToLower()){                        
                                foreach(SIP_RegistrationContact contact in registration.Contacts){
                                    DataRow dr = ds.Tables["Contacts"].NewRow();
                                    dr["Value"] = contact.ToStringValue();
                                    ds.Tables["Contacts"].Rows.Add(dr);
                                }
                                break;
                            }                   
                        }

                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");               
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method SetSipRegistration

        private void SetSipRegistration(string argsText)
        {
            /* SetSipRegistration "<virtualServerID>" "<addressOfRecord>" "<contacts>"
                  Responses:
                    +OK                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: SetSipRegistration \"<virtualServerID>\" \"<addressOfRecord>\" \"<contacts>\"");
                    return;
                }
                
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        List<SIP_t_ContactParam> contacts = new List<SIP_t_ContactParam>();
                        foreach(string contact in args[2].Split('\t')){
                            if(contact.Length > 0){
                                SIP_t_ContactParam c = new SIP_t_ContactParam();
                                c.Parse(new LumiSoft.Net.StringReader(contact));
                                contacts.Add(c);
                            }
                        }
                        
                        ((SIP_ProxyCore)virtualServer.SipServer.SipCore).Registrar.SetRegistration(args[1],contacts.ToArray());
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }                    
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");                
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteSipRegistration

        private void DeleteSipRegistration(string argsText)
        {
            /* DeleteSipRegistration "<virtualServerID>" "<addressOfRecord>"
                  Responses:
                    +OK                                        
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteSipRegistration \"<virtualServerID>\" \"<addressOfRecord>\"");
                    return;
                }
                
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        foreach(SIP_Registration registration in ((SIP_ProxyCore)virtualServer.SipServer.SipCore).Registrar.Registrations){
                            if(args[0].ToLower() == registration.Address.ToLower()){
                                ((SIP_ProxyCore)virtualServer.SipServer.SipCore).Registrar.DeleteRegistration(registration.Address);
                                break;
                            }
                        }

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");                
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetEvents

        private void GetEvents()
        {
            /* GetEvents
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                DataSet ds = new DataSet("dsEvents");
                ds.Tables.Add("Events");
                ds.Tables["Events"].Columns.Add("ID");
                ds.Tables["Events"].Columns.Add("VirtualServer");
                ds.Tables["Events"].Columns.Add("CreateDate",typeof(DateTime));
                ds.Tables["Events"].Columns.Add("Type");
                ds.Tables["Events"].Columns.Add("Text");

                if(File.Exists(SCore.PathFix(m_pServer.MailServer.StartupPath + "\\Settings\\Events.xml"))){
                    ds.ReadXml(SCore.PathFix(m_pServer.MailServer.StartupPath + "\\Settings\\Events.xml"));
                }

                // Compress data
                byte[] dsZipped = CompressDataSet(ds);

                this.Socket.WriteLine("+OK " + dsZipped.Length);
                this.Socket.Write(dsZipped);
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method ClearEvents

        private void ClearEvents()
        {
            /* ClearEvents
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                if(File.Exists(SCore.PathFix(m_pServer.MailServer.StartupPath + "\\Settings\\Events.xml"))){
                    File.Delete(SCore.PathFix(m_pServer.MailServer.StartupPath + "\\Settings\\Events.xml"));
                }

                this.Socket.WriteLine("+OK");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetLogSessions

        private void GetLogSessions(string argsText)
        {
            /* GetLogSessions <virtualServerID> <service> <limit> "<startTime>" "<endTime>" "containsText"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 6){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetLogSessions <virtualServerID> <service> <limit> \"<startTime>\" \"<endTime>\" \"containsText\"");
                    return;
                }

                VirtualServer virtualServer = null;
                foreach(VirtualServer vServer in m_pServer.MailServer.VirtualServers){
                    if(vServer.ID.ToLower() == args[0].ToLower()){                        
                        virtualServer = vServer;
                        break;
                    }
                }
                if(virtualServer == null){
                    this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
                }

                DataSet ds = new DataSet("dsLogsSessions");
                ds.Tables.Add("LogSessions");
                ds.Tables["LogSessions"].Columns.Add("SessionID");
                ds.Tables["LogSessions"].Columns.Add("StartTime",typeof(DateTime));
                ds.Tables["LogSessions"].Columns.Add("RemoteEndPoint");
                ds.Tables["LogSessions"].Columns.Add("UserName");
                
                int      limit        = ConvertEx.ToInt32(args[2]);
                DateTime startTime    = Convert.ToDateTime(TextUtils.UnQuoteString(args[3]));
                DateTime endTime      = Convert.ToDateTime(TextUtils.UnQuoteString(args[4]));
                string   containsText = TextUtils.UnQuoteString(args[5]);
                
                string fileName = "";
                if(args[1] == "SMTP"){
                    fileName = virtualServer.SMTP_LogsPath + "smtp";
                }
                else if(args[1] == "POP3"){
                    fileName = virtualServer.POP3_LogsPath + "pop3";
                }
                else if(args[1] == "IMAP"){
                    fileName = virtualServer.IMAP_LogsPath + "imap";
                }
                else if(args[1] == "RELAY"){
                    fileName = virtualServer.RELAY_LogsPath + "relay";
                }
                else if(args[1] == "FETCH"){
                    fileName = virtualServer.FETCH_LogsPath + "fetch";
                }
                else{
                    throw new Exception("Invalid <service value> !");
                }
                fileName += "-" + startTime.ToString("yyyyMMdd") + ".log";
                
         
                if(File.Exists(fileName)){
                    using(TextDb db = new TextDb('\t')){
                        db.OpenRead(fileName);

                        Dictionary<string,string> processedSessions = new Dictionary<string,string>();
                        while(db.MoveNext()){
                            string[] row = db.CurrentRow;
                            if(row.Length == 6){
                                string   sessionID        = row[0];
                                DateTime sessionStartTime = Convert.ToDateTime(row[1]);
                                string   remoteEndPoint   = row[2];
                                string   userName         = row[3];
                                string   logText          = row[5];

                                if(!processedSessions.ContainsKey(sessionID)){
                                    //--- Apply fileting criteria ---------------------------------------------//
                                    bool add = true;
                                    if(startTime > sessionStartTime || endTime < sessionStartTime){
                                        add = false;
                                    }
                                    if(containsText.Length > 0 && logText.ToLower().IndexOf(containsText.ToLower()) == -1){
                                        add = false;
                                    }
                                    if(processedSessions.Count > limit){                                        
                                        break;
                                    }
                                    //------------------------------------------------------------------------//

                                    if(add){
                                        DataRow dr = ds.Tables["LogSessions"].NewRow();
                                        dr["SessionID"]      = sessionID;
                                        dr["StartTime"]      = sessionStartTime;
                                        dr["RemoteEndPoint"] = remoteEndPoint;
                                        dr["UserName"]       = userName;
                                        ds.Tables["LogSessions"].Rows.Add(dr);

                                        processedSessions.Add(sessionID,"");
                                    }
                                }
                            }
                        }
                    }
                }

                // Compress data
                byte[] dsZipped = CompressDataSet(ds);

                this.Socket.WriteLine("+OK " + dsZipped.Length);
                this.Socket.Write(dsZipped);
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetSessionLog

        private void GetSessionLog(string argsText)
        {
            /* GetSessionLog <virtualServerID> <service> "<sessionID>" "<sessionStartDate>"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 4){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetSessionLog <virtualServerID> <service> \"<sessionID>\" \"<sessionStartDate>\"");
                    return;
                }

                VirtualServer virtualServer = null;
                foreach(VirtualServer vServer in m_pServer.MailServer.VirtualServers){
                    if(vServer.ID.ToLower() == args[0].ToLower()){                        
                        virtualServer = vServer;
                        break;
                    }
                }
                if(virtualServer == null){
                    this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
                }
                                
                DateTime startTime = Convert.ToDateTime(TextUtils.UnQuoteString(args[3]));

                DataSet ds = new DataSet("dsSessionLog");
                ds.Tables.Add("SessionLog");
                ds.Tables["SessionLog"].Columns.Add("LogText");
                
                string fileName = "";
                if(args[1] == "SMTP"){
                    fileName += virtualServer.SMTP_LogsPath + "smtp";
                }
                else if(args[1] == "POP3"){
                    fileName += virtualServer.POP3_LogsPath + "pop3";
                }
                else if(args[1] == "IMAP"){
                    fileName += virtualServer.IMAP_LogsPath + "imap";
                }
                else if(args[1] == "RELAY"){
                    fileName += virtualServer.RELAY_LogsPath + "relay";
                }
                else if(args[1] == "FETCH"){
                    fileName += virtualServer.FETCH_LogsPath + "fetch";
                }
                else{
                    throw new Exception("Invalid <service value> !");
                }
                fileName += "-" + startTime.ToString("yyyyMMdd") + ".log";
                
         
                if(File.Exists(fileName)){                    
                    StringBuilder retVal = new StringBuilder();
                    using(TextDb db = new TextDb('\t')){
                        db.OpenRead(fileName);
                                                
                        while(db.MoveNext()){
                            string[] row = db.CurrentRow;
                            if(row.Length == 6){
                                string sessionID = row[0];
                                string logText   = row[5];

                                if(sessionID.ToLower() == args[2].ToLower()){
                                    retVal.Append(db.CurrentRowString + "\r\n");
                                }
                            }
                        }
                    }

                    DataRow dr = ds.Tables["SessionLog"].NewRow();
                    dr["LogText"] = retVal.ToString();
                    ds.Tables["SessionLog"].Rows.Add(dr);
                }

                // Compress data
                byte[] dsZipped = CompressDataSet(ds);

                this.Socket.WriteLine("+OK " + dsZipped.Length);
                this.Socket.Write(dsZipped);
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetVirtualServerAPIs

        private void GetVirtualServerAPIs()
        {
            /* GetVirtualServerAPIs
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                DataSet ds = new DataSet();
                ds.Tables.Add("API");
                ds.Tables["API"].Columns.Add("AssemblyName");
                ds.Tables["API"].Columns.Add("TypeName");
                string[] files = Directory.GetFiles(SCore.PathFix(m_pServer.MailServer.StartupPath),"*.dll");
                foreach(string file in files){
                    try{
                        Assembly assembly = Assembly.LoadFile(file);
                        Type[] types = assembly.GetTypes();
                        foreach(Type type in types){
                            if(type.GetInterface(typeof(IMailServerApi).ToString()) != null){
                                DataRow dr = ds.Tables["API"].NewRow();
                                dr["AssemblyName"] = Path.GetFileName(file);
                                dr["TypeName"]     = type.ToString();
                                ds.Tables["API"].Rows.Add(dr);
                            }
                        }
                    }
                    catch{
                    }
                }
                
                // Compress data
                byte[] dsZipped = CompressDataSet(ds);

                this.Socket.WriteLine("+OK " + dsZipped.Length);
                this.Socket.Write(dsZipped);
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }            
        }

        #endregion

        #region method GetVirtualServers

        private void GetVirtualServers()
        {
            try{
                DataSet ds = new DataSet();
                ds.Tables.Add("Servers");
                ds.Tables["Servers"].Columns.Add("ID");
                ds.Tables["Servers"].Columns.Add("Enabled").DefaultValue = true;
                ds.Tables["Servers"].Columns.Add("Name");
                ds.Tables["Servers"].Columns.Add("API_assembly");
                ds.Tables["Servers"].Columns.Add("API_class");
                ds.Tables["Servers"].Columns.Add("API_initstring");
			    ds.ReadXml(SCore.PathFix(m_pServer.MailServer.StartupPath + "Settings\\localServers.xml"));
                foreach(DataRow dr in ds.Tables["Servers"].Rows){
                    foreach(DataColumn dc in ds.Tables["Servers"].Columns){
                        if(dr.IsNull(dc)){
                            dr[dc] = dc.DefaultValue;
                        }
                    }
                }

                // Compress data
                byte[] dsZipped = CompressDataSet(ds);

                this.Socket.WriteLine("+OK " + dsZipped.Length);
                this.Socket.Write(dsZipped);
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddVirtualServer

        private void AddVirtualServer(string argsText)
        {
            /* AddVirtualServer <ID> <enabled> "<name>" "<assembly>" "<type>" "<initString>:base64"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 6){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddVirtualServer <ID> <enabled> \"<name>\" \"<assembly>\" \"<type>\" \"<initString>:base64\"");
                    return;
                }
                                                
                // Try to load Virtual server
                m_pServer.MailServer.LoadApi(TextUtils.UnQuoteString(args[3]),TextUtils.UnQuoteString(args[4]),System.Text.Encoding.Default.GetString(Convert.FromBase64String(TextUtils.UnQuoteString(args[5]))));

                DataSet ds = new DataSet();
                ds.Tables.Add("Servers");
                ds.Tables["Servers"].Columns.Add("ID");
                ds.Tables["Servers"].Columns.Add("Enabled");
                ds.Tables["Servers"].Columns.Add("Name");
                ds.Tables["Servers"].Columns.Add("API_assembly");
                ds.Tables["Servers"].Columns.Add("API_class");
                ds.Tables["Servers"].Columns.Add("API_initstring");
			    ds.ReadXml(SCore.PathFix(m_pServer.MailServer.StartupPath + "Settings\\localServers.xml"));

                DataRow dr = ds.Tables["Servers"].NewRow();
                dr["ID"]             = args[0];
                dr["Enabled"]        = args[1];
                dr["Name"]           = TextUtils.UnQuoteString(args[2]);
                dr["API_assembly"]   = TextUtils.UnQuoteString(args[3]);
                dr["API_class"]      = TextUtils.UnQuoteString(args[4]);
                dr["API_initstring"] = System.Text.Encoding.Default.GetString(Convert.FromBase64String(TextUtils.UnQuoteString(args[5])));
                ds.Tables["Servers"].Rows.Add(dr);

                ds.WriteXml(SCore.PathFix(m_pServer.MailServer.StartupPath + "Settings\\localServers.xml"));

                // Force to refresh virtual servers
                m_pServer.MailServer.LoadVirtualServers();

                this.Socket.WriteLine("+OK");
            }
            catch(System.Reflection.TargetInvocationException x){
                this.Socket.WriteLine("-ERR " + x.InnerException.Message);
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateVirtualServer

        private void UpdateVirtualServer(string argsText)
        {
            /* UpdateVirtualServer <ID> <enabled> "<name>" "<initString>:base64"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */
            
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 4){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateVirtualServer <ID> <enabled> \"<name>\" \"<initString>:base64\"");
                    return;
                }
                                    
                DataSet ds = new DataSet();
                ds.Tables.Add("Servers");
                ds.Tables["Servers"].Columns.Add("ID");
                ds.Tables["Servers"].Columns.Add("Enabled");
                ds.Tables["Servers"].Columns.Add("Name");
                ds.Tables["Servers"].Columns.Add("API_assembly");
                ds.Tables["Servers"].Columns.Add("API_class");
                ds.Tables["Servers"].Columns.Add("API_initstring");
			    ds.ReadXml(SCore.PathFix(m_pServer.MailServer.StartupPath + "Settings\\localServers.xml"));
                if(ds.Tables.Contains("Servers")){
                    foreach(DataRow dr in ds.Tables["Servers"].Rows){
                        if(dr["ID"].ToString() == TextUtils.UnQuoteString(args[0])){
                            dr["Enabled"]        = args[1];
                            dr["Name"]           = TextUtils.UnQuoteString(args[2]);
                            dr["API_initstring"] = System.Text.Encoding.Default.GetString(Convert.FromBase64String(TextUtils.UnQuoteString(args[3])));
                            ds.WriteXml(SCore.PathFix(m_pServer.MailServer.StartupPath + "Settings\\localServers.xml"));

                            this.Socket.WriteLine("+OK");
                            return;
                        }
                    }

                    this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
                }
            }
            catch(System.Reflection.TargetInvocationException x){
                this.Socket.WriteLine("-ERR " + x.InnerException.Message);
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteVirtualServers

        private void DeleteVirtualServers(string argsText)
        {
            /* DeleteVirtualServer <virtualServerID>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 1){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteVirtualServer <virtualServerID>");
                    return;
                }

                DataSet ds = new DataSet();
			    ds.ReadXml(SCore.PathFix(m_pServer.MailServer.StartupPath + "Settings\\localServers.xml"));
                if(ds.Tables.Contains("Servers")){
                    foreach(DataRow dr in ds.Tables["Servers"].Rows){
                        if(dr["ID"].ToString() == TextUtils.UnQuoteString(args[0])){
                            dr.Delete();
                            ds.WriteXml(SCore.PathFix(m_pServer.MailServer.StartupPath + "Settings\\localServers.xml"));

                            // Force to refresh virtual servers
                            m_pServer.MailServer.LoadVirtualServers();

                            this.Socket.WriteLine("+OK");
                            return;
                        }
                    }

                    this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
                }
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetSettings

        private void GetSettings(string argsText)
        {
            /* GetSettings <virtualServerID>
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == argsText.ToLower()){
                        MemoryStream ms = new MemoryStream();
                        virtualServer.API.GetSettings().Table.DataSet.WriteXml(ms);
                        ms.Position = 0;

                        this.Socket.WriteLine("+OK " + ms.Length);
                        this.Socket.Write(ms);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateSettings

        private void UpdateSettings(string argsText)
        {
            /* UpdateSettings <virtualServerID> <dataLength><CRLF>
                <data>  -> data length of dataLength
             
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateSettings <virtualServerID> <dataLength><CRLF><data>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        MemoryStream ms = new MemoryStream();
                        this.Socket.ReadSpecifiedLength(Convert.ToInt32(args[1]),ms);
                        ms.Position = 0;
                        DataSet ds = new DataSet();
                        ds.ReadXml(ms);
                        virtualServer.API.UpdateSettings(ds.Tables["Settings"].Rows[0]);
                                       
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetDomains

        private void GetDomains(string argsText)
        {
            try{
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == argsText.ToLower()){
                        MemoryStream ms = new MemoryStream();
                        DataSet ds = virtualServer.API.GetDomains().Table.DataSet;
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddDomain

        private void AddDomain(string argsText)
        {
            /* AddDomain <virtualServerID> "<domainID>" "<domainName>" "<description>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 4){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddDomain <virtualServerID> \"<domainID>\" \"<domainName>\" \"<description>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddDomain(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3])
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateDomain

        private void UpdateDomain(string argsText)
        {
            /* UpdateDomain <virtualServerID> "<domainID>" "<domainName>" "<description>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 4){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateDomain <virtualServerID> \"<domainID>\" \"<domainName>\" \"<description>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateDomain(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3])
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteDomain

        private void DeleteDomain(string argsText)
        {
            /* DeleteDomain <virtualServerID> "<domainID>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteDomain <virtualServerID> \"<domainID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteDomain(TextUtils.UnQuoteString(args[1]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetUsers

        private void GetUsers(string argsText)
        {
            try{
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == argsText.ToLower()){ 
                        DataSet ds = virtualServer.API.GetUsers("ALL").Table.DataSet;
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddUser

        private void AddUser(string argsText)
        {
            /* AddUser <virtualServerID> "<userID>" "<userName>" "<fullName>" "<password>" "<description>" <mailboxSize> <enabled> <permissions>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 9){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddUser <virtualServerID> \"<userID>\" \"<userName>\" \"<fullName>\" \"<password>\" \"<description>\" <mailboxSize> <enabled> <allowRelay>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddUser(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            TextUtils.UnQuoteString(args[4]),
                            TextUtils.UnQuoteString(args[5]),
                            "",
                            Convert.ToInt32(args[6]),
                            Convert.ToBoolean(args[7]),
                            (UserPermissions_enum)Convert.ToInt32(args[8])
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateUser

        private void UpdateUser(string argsText)
        {
            /* UpdateUser <virtualServerID> "<userID>" "<userName>" "<fullName>" "<password>" "<description>" <mailboxSize> <enabled> <permissions>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 9){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateUser <virtualServerID> \"<userID>\" \"<userName>\" \"<fullName>\" \"<password>\" \"<description>\" <mailboxSize> <enabled> <allowRelay>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateUser(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            TextUtils.UnQuoteString(args[4]),
                            TextUtils.UnQuoteString(args[5]),
                            "",
                            Convert.ToInt32(args[6]),
                            Convert.ToBoolean(args[7]),
                            (UserPermissions_enum)Convert.ToInt32(args[8])
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteUser

        private void DeleteUser(string argsText)
        {
            /* DeleteUser <virtualServerID> "<userID>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteUser <virtualServerID> \"<userID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteUser(TextUtils.UnQuoteString(args[1]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetUserEmailAddresses

        private void GetUserEmailAddresses(string argsText)
        {
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserEmailAddresses <virtualServerID> <userID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        DataSet ds = DataView_To_DataSet(virtualServer.API.GetUserAddresses(UserName_From_UserID(virtualServer.API.GetUsers("ALL"),args[1])));
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddUserEmailAddress

        private void AddUserEmailAddress(string argsText)
        {
            /* AddUserEmailAddress <virtualServerID> "<userID>" "<emailAddress>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddUserEmailAddress <virtualServerID> \"<userID>\" \"<emailAddress>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        MemoryStream ms = new MemoryStream();
                        virtualServer.API.AddUserAddress(UserName_From_UserID(virtualServer.API.GetUsers("ALL"),TextUtils.UnQuoteString(args[1])),TextUtils.UnQuoteString(args[2]));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteUserEmailAddress

        private void DeleteUserEmailAddress(string argsText)
        {
            /* DeleteUserEmailAddress <virtualServerID> "<userID>" "<emailAddress>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteUserEmailAddress <virtualServerID> \"<userID>\" \"<emailAddress>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        MemoryStream ms = new MemoryStream();
                        virtualServer.API.DeleteUserAddress(TextUtils.UnQuoteString(args[2]));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetUserMessageRules

        private void GetUserMessageRules(string argsText)
        {
            /* GetUserMessageRules <virtualServerID> "<userID>"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserMessageRules <virtualServerID> \"<userID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        DataSet ds = DataView_To_DataSet(virtualServer.API.GetUserMessageRules(UserName_From_UserID(virtualServer.API.GetUsers("ALL"),TextUtils.UnQuoteString(args[1]))));
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddUserMessageRule

        private void AddUserMessageRule(string argsText)
        {
            /* AddUserMessageRule <virtualServerID> "<userID>" "<ruleID>" <cost> <enabled> "<description>" "<matchExpression>" <checkNext>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 8){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddGlobalMessageRule <virtualServerID> \"<userID>\" \"<ruleID>\" <cost> <enabled> \"<description>\" \"<matchExpression>\" <checkNext>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddUserMessageRule(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            Convert.ToInt64(args[3]),
                            Convert.ToBoolean(args[4]),
                            (GlobalMessageRule_CheckNextRule_enum)Convert.ToInt32(args[7]),
                            TextUtils.UnQuoteString(args[5]),
                            TextUtils.UnQuoteString(args[6])
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateUserMessageRule

        private void UpdateUserMessageRule(string argsText)
        {
            /* UpdateUserMessageRule <virtualServerID> "<userID>" "<ruleID>" <cost> <enabled> "<description>" "<matchExpression>" <checkNext>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 8){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateUserMessageRule <virtualServerID> \"<userID>\" \"<ruleID>\" <cost> <enabled> \"<description>\" \"<matchExpression>\" <checkNext>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateUserMessageRule(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            Convert.ToInt64(args[3]),
                            Convert.ToBoolean(args[4]),
                            (GlobalMessageRule_CheckNextRule_enum)Convert.ToInt32(args[7]),
                            TextUtils.UnQuoteString(args[5]),
                            TextUtils.UnQuoteString(args[6])
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteUserMessageRule

        private void DeleteUserMessageRule(string argsText)
        {
            /* DeleteUserMessageRule <virtualServerID> "<userID>" "<ruleID>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteUserMessageRule <virtualServerID> \"<userID>\" \"<ruleID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteUserMessageRule(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2])
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetUserMessageRuleActions

        private void GetUserMessageRuleActions(string argsText)
        {
            /* GetUserMessageRuleActions <virtualServerID> "<userID>" "<messageRuleID>"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserMessageRuleActions <virtualServerID> \"<userID>\" \"<messageRuleID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        DataSet ds = DataView_To_DataSet(virtualServer.API.GetUserMessageRuleActions(TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2])));
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddUserMessageRuleAction

        private void AddUserMessageRuleAction(string argsText)
        {
            /* AddUserMessageRuleAction <virtualServerID> "<userID>" "<messageRuleID>" "<messageRuleActionID>" "<description>" <actionType> "<actionData>:base64"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 7){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddUserMessageRuleAction <virtualServerID> \"<userID>\" \"<messageRuleID>\" \"<messageRuleActionID>\" \"<description>\" <actionType> \"<actionData>:base64\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddUserMessageRuleAction(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            TextUtils.UnQuoteString(args[4]),
                            (GlobalMessageRuleAction_enum)Convert.ToInt32(args[5]),
                            Convert.FromBase64String(TextUtils.UnQuoteString(args[6]))
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateUserMessageRuleAction

        private void UpdateUserMessageRuleAction(string argsText)
        {
            /* UpdateUserMessageRuleAction <virtualServerID> "<userID>" "<messageRuleID>" "<messageRuleActionID>" "<description>" <actionType> "<actionData>:base64"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 7){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateUserMessageRuleAction <virtualServerID> \"<messageRuleID>\" \"<messageRuleActionID>\" \"<description>\" <actionType> \"<actionData>:base64\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateUserMessageRuleAction(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            TextUtils.UnQuoteString(args[4]),
                            (GlobalMessageRuleAction_enum)Convert.ToInt32(args[5]),
                            Convert.FromBase64String(TextUtils.UnQuoteString(args[6]))
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteUserMessageRuleAction

        private void DeleteUserMessageRuleAction(string argsText)
        {
            /* DeleteUserMessageRuleAction <virtualServerID> "<userID>" "<messageRuleID>" "<messageRuleActionID>"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length !=4){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteUserMessageRuleAction <virtualServerID> \"<userID>\" \"<messageRuleID>\" \"<messageRuleActionID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteUserMessageRuleAction(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3])
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetUserMailboxSize

        private void GetUserMailboxSize(string argsText)
        {
            /* GetUserMailboxSize <virtualServerID> "<userID>"
                      Responses:
                        +OK                     
                        -ERR <errorText>
                */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserMailboxSize <virtualServerID> <userID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        this.Socket.WriteLine("+OK " + virtualServer.API.GetMailboxSize(UserName_From_UserID(virtualServer.API.GetUsers("ALL"),TextUtils.UnQuoteString(args[1]))).ToString());
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetUserLastLoginTime

        private void GetUserLastLoginTime(string argsText)
        {
            /* GetUserLastLoginTime <virtualServerID> "<userID>"
                      Responses:
                        +OK <lastLoginTime>                   
                        -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserLastLoginTime <virtualServerID> <userID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        this.Socket.WriteLine("+OK " + virtualServer.API.GetUserLastLoginTime(UserName_From_UserID(virtualServer.API.GetUsers("ALL"),TextUtils.UnQuoteString(args[1]))).ToString("u"));
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetUserFolders

        private void GetUserFolders(string argsText)
        {
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserFolders <virtualServerID> <userID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        virtualServer.API.CreateUserDefaultFolders(UserName_From_UserID(virtualServer.API.GetUsers("ALL"),args[1]));
                        string[] folders = virtualServer.API.GetFolders(UserName_From_UserID(virtualServer.API.GetUsers("ALL"),args[1]),false);
                        DataSet ds = new DataSet();
                        DataTable dt = ds.Tables.Add("Folders");
                        dt.Columns.Add("Folder");
                        foreach(string folder in folders){
                            DataRow dr = dt.NewRow();
                            dr["Folder"] = folder;
                            dt.Rows.Add(dr);
                        }
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddUserFolder

        private void AddUserFolder(string argsText)
        {
            /* AddUserFolder <virtualServerID> "<folderOwnerUser>" "<folder>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddUserFolder <virtualServerID> \"<folderOwnerUser>\" \"<folder>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        virtualServer.API.CreateFolder("system",TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2]));
                                            
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteUserFolder

        private void DeleteUserFolder(string argsText)
        {
            /* DeleteUserFolder <virtualServerID> "<folderOwnerUser>" "<folder>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteUserFolder <virtualServerID> \"<folderOwnerUser>\" \"<folder>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        virtualServer.API.DeleteFolder("system",TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2]));
                                            
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method RenameUserFolder

        private void RenameUserFolder(string argsText)
        {
            /* RenameUserFolder <virtualServerID> "<folderOwnerUser>" "<folder>" "<newFolder>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 4){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: RenameUserFolder <virtualServerID> \"<folderOwnerUser>\" \"<folder>\" \"<newFolder>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        virtualServer.API.RenameFolder("system",TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2]),TextUtils.UnQuoteString(args[3]));
                                            
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetUserFolderInfo

        private void GetUserFolderInfo(string argsText)
        {
            /* GetUserFolderInfo <virtualServerID> "<folderOwnerUser>" "<folder>"
                  Responses:
                    +OK "<creationDate>" <numberOfMessages> <sizeUsed>                  
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserFolderInfo <virtualServerID> \"<folderOwnerUser>\" \"<folder>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        IMAP_MessageCollection messages = new IMAP_MessageCollection();
                        virtualServer.API.GetMessagesInfo(
                            "system",
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            messages
                        );

                        // Calculate size used
                        long sizeUsed = 0;
                        foreach(IMAP_Message message in messages){
                            sizeUsed += message.Size;
                        }

                        DateTime creationTime = virtualServer.API.FolderCreationTime(TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2]));
                                            
                        this.Socket.WriteLine("+OK \"" + creationTime.ToString("yyyyMMdd HH:mm:ss") + "\" " + messages.Count + " " + sizeUsed + "");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetUserFolderMessagesInfo

        private void GetUserFolderMessagesInfo(string argsText)
        {
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserFolderMessagesInfo <virtualServerID> \"<user>\" \"<folder>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user
                        // TODO: handle no existent folder

                        DataSet ds = new DataSet();
                        ds.Tables.Add("MessagesInfo");
                        ds.Tables["MessagesInfo"].Columns.Add("ID");
                        ds.Tables["MessagesInfo"].Columns.Add("UID");  // REMOVE ME: Now needed for delete message.
                        ds.Tables["MessagesInfo"].Columns.Add("Size",typeof(long));
                        ds.Tables["MessagesInfo"].Columns.Add("Flags",typeof(long));
                        ds.Tables["MessagesInfo"].Columns.Add("Envelope");

                        IMAP_MessageCollection messages = new IMAP_MessageCollection();
                        virtualServer.API.GetMessagesInfo("system",args[1],args[2],messages);
                        foreach(IMAP_Message message in messages){
                            try{
                                DataRow dr = ds.Tables["MessagesInfo"].NewRow();
                                dr["ID"]    = message.ID;
                                dr["UID"]   = message.UID;
                                dr["Size"]  = message.Size;
                                dr["Flags"] = message.Flags;

                                EmailMessageItems msgItems = new EmailMessageItems(message.ID,IMAP_MessageItems_enum.Envelope);
                                virtualServer.API.GetMessageItems("system",args[1],args[2],msgItems);
                                dr["Envelope"] = msgItems.Envelope;
                                ds.Tables["MessagesInfo"].Rows.Add(dr);
                            }
                            catch{
                            }
                        }

                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetUserFolderMessage

        private void GetUserFolderMessage(string argsText)
        {
            /* C: GetUserFolderMessage <virtualServerID> "<folderOwnerUser>" "<folder>" "<messageID>"
               S: +OK <sizeInBytes>
               S: <messageData>
                            
                  Responses:
                    +OK               
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 4){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserFolderMessage <virtualServerID> \"<folderOwnerUser>\" \"<folder>\" \"<messageID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user
                        // TODO: handle no existent folder

                        EmailMessageItems msgItems = new EmailMessageItems(args[3],IMAP_MessageItems_enum.Message);
                        virtualServer.API.GetMessageItems(
                            "system",
                            args[1],
                            args[2],
                            msgItems
                        );

                        if(msgItems.MessageExists){
                            try{
                                this.Socket.WriteLine("+OK " + ConvertEx.ToString(msgItems.MessageStream.Length - msgItems.MessageStream.Position));
                                this.Socket.Write(msgItems.MessageStream);
                            }
                            finally{
                                msgItems.MessageStream.Close();
                            }
                        }
                        else{
                            this.Socket.WriteLine("-ERR Specified message doesn't exist !");
                        }
                                                
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method StoreUserFolderMessage

        private void StoreUserFolderMessage(string argsText)
        {
            /* C: StoreUserFolderMessage <virtualServerID> "<folderOwnerUser>" "<folder>" <sizeInBytes>
               S: +OK Send message data
               C: <messageData>
               S: +OK
             
                  Responses:
                    +OK               
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 4){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: StoreUserFolderMessage <virtualServerID> \"<folderOwnerUser>\" \"<folder>\" <sizeInBytes>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user
                        // TODO: handle no existent folder

                        this.Socket.WriteLine("+OK");
                        using(FileStream messageStream = File.Open(Path.GetTempFileName(),FileMode.Open)){
                            this.Socket.ReadSpecifiedLength(ConvertEx.ToInt32(args[3]),messageStream);
                            messageStream.Position = 0;
                            virtualServer.API.StoreMessage(
                                "system",
                                args[1],
                                args[2],
                                messageStream,
                                DateTime.Now,
                                IMAP_MessageFlags.Recent
                            );
                        }
                                                
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteUserFolderMessage

        private void DeleteUserFolderMessage(string argsText)
        {
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 5){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteUserFolderMessage <virtualServerID> \"<folderOwnerUser>\" \"<folder>\" \"<ID>\" \"<UID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user
                        // TODO: handle no existent folder

                        virtualServer.API.DeleteMessage(
                            "system",
                            args[1],
                            args[2],
                            args[3],
                            ConvertEx.ToInt32(args[4])
                        );
                                                
                        this.Socket.WriteLine("+OK ");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion
        
        #region method GetUserRemoteServers

        private void GetUserRemoteServers(string argsText)
        {
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserRemoteServers <virtualServerID> <userID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user

                        DataSet ds = DataView_To_DataSet(virtualServer.API.GetUserRemoteServers(UserName_From_UserID(virtualServer.API.GetUsers("ALL"),args[1])));
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddUserRemoteServer

        private void AddUserRemoteServer(string argsText)
        {
            /* AddUserRemoteServer <virtualServerID> "<remoteServerID>" "<userName>" "<description>" "<remoteHost>" <remoteHostPort> "<remoteHostUserName>" "<remoteHostPassword>" <ssl> <enabled>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 10){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddUserRemoteServer <virtualServerID> \"<remoteServerID>\" \"<userName>\" \"<description>\" \"<remoteHost>\" <remoteHostPort> \"<remoteHostUserName>\" \"<remoteHostPassword>\" <ssl> <enabled>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user
                        
                        virtualServer.API.AddUserRemoteServer(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            TextUtils.UnQuoteString(args[4]),
                            Convert.ToInt32(args[5]),
                            TextUtils.UnQuoteString(args[6]),
                            TextUtils.UnQuoteString(args[7]),
                            Convert.ToBoolean(args[8]),
                            Convert.ToBoolean(args[9]
                        ));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteUserRemoteServer

        private void DeleteUserRemoteServer(string argsText)
        {
            /* DeleteUserRemoteServer <virtualServerID> "<remoteServerID>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteUserRemoteServer <virtualServerID> \"<remoteServerID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user
                        
                        virtualServer.API.DeleteUserRemoteServer(TextUtils.UnQuoteString(args[1]));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateUserRemoteServer

        private void UpdateUserRemoteServer(string argsText)
        {
            /* UpdateUserRemoteServer <virtualServerID> "<remoteServerID>" "<userName>" "<description>" "<remoteHost>" <remoteHostPort> "<remoteHostUserName>" "<remoteHostPassword>" <ssl> <enabled>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 10){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateUserRemoteServer <virtualServerID> <userID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user
                        
                        virtualServer.API.UpdateUserRemoteServer(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            TextUtils.UnQuoteString(args[4]),
                            Convert.ToInt32(args[5]),
                            TextUtils.UnQuoteString(args[6]),
                            TextUtils.UnQuoteString(args[7]),
                            Convert.ToBoolean(args[8]),
                            Convert.ToBoolean(args[9]
                        ));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetUserFolderAcl

        private void GetUserFolderAcl(string argsText)
        {
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserFolders <virtualServerID> <userID> \"<FolderName>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user
                        // TODO: handle no existent folder

                        DataSet ds = DataView_To_DataSet(virtualServer.API.GetFolderACL("system",UserName_From_UserID(virtualServer.API.GetUsers("ALL"),args[1]),TextUtils.UnQuoteString(args[2])));
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method SetUserFolderAcl

        private void SetUserFolderAcl(string argsText)
        {
            /* SetUserFolderAcl <virtualServerID> "<folderOwnerUser>" "<folder>" "<userOrGroup>" <flags:int32>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 5){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUserFolders <virtualServerID> \"<folderOwnerUser>\" \"<folder>\" \"<userOrGroup>\" <flags:int32>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user
                        // TODO: handle no existent folder

                        MemoryStream ms = new MemoryStream();
                        virtualServer.API.SetFolderACL("system",TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2]),TextUtils.UnQuoteString(args[3]),IMAP_Flags_SetType.Replace,(IMAP_ACL_Flags)Convert.ToInt32(args[4]));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteUserFolderAcl

        private void DeleteUserFolderAcl(string argsText)
        {
            /* DeleteUserFolderAcl <virtualServerID> "<folderOwnerUser>" "<folder>" "<userOrGroup>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 4){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteUserFolderAcl <virtualServerID> \"<folderOwnerUser>\" \"<folder>\" \"<userOrGroup>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user
                        // TODO: handle no existent folder
                                               
                        virtualServer.API.DeleteFolderACL("system",TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2]),TextUtils.UnQuoteString(args[3]));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

       

        #region method GetUsersDefaultFolders

        private void GetUsersDefaultFolders(string argsText)
        {
            /* GetUsersDefaultFolders <virtualServerID>
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    + ERR <errorText>
            */
           
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 1){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetUsersDefaultFolders <virtualServerID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        DataSet ds = DataView_To_DataSet(virtualServer.API.GetUsersDefaultFolders());
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddUsersDefaultFolder

        private void AddUsersDefaultFolder(string argsText)
        {
            /* AddUsersDefaultFolder <virtualServerID> "<folderName>" <permanent>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */
            
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddUsersDefaultFolder <virtualServerID> \"<folderName>\" <permanent>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){                        
                        virtualServer.API.AddUsersDefaultFolder(
                            TextUtils.UnQuoteString(args[1]),
                            ConvertEx.ToBoolean(args[2])
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteUsersDefaultFolder

        private void DeleteUsersDefaultFolder(string argsText)
        {
            /* DeleteUsersDefaultFolder <virtualServerID> "<folderName>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */
            
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteUsersDefaultFolder <virtualServerID> \"<<folderName>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        virtualServer.API.DeleteUsersDefaultFolder(TextUtils.UnQuoteString(args[1]));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetGroups

        private void GetGroups(string argsText)
        {
            try{
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == argsText.ToLower()){
                        DataSet ds = virtualServer.API.GetGroups().Table.DataSet;
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddGroup

        private void AddGroup(string argsText)
        {
            /* AddGroup <virtualServerID> "<groupID>" "<groupName>" "<description>" <enabled>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 5){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateGroup <virtualServerID> \"<groupID>\" \"<groupName>\" \"<description>\" <enabled>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddGroup(TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2]),TextUtils.UnQuoteString(args[3]),Convert.ToBoolean(args[4]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateGroup

        private void UpdateGroup(string argsText)
        {
            /* UpdateGroup <virtualServerID> "<groupID>" "<groupName>" "<description>" <enabled>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 5){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateGroup <virtualServerID> \"<groupID>\" \"<groupName>\" \"<description>\" <enabled>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateGroup(TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2]),TextUtils.UnQuoteString(args[3]),Convert.ToBoolean(args[4]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteGroup

        private void DeleteGroup(string argsText)
        {
            /* DeleteGroup <virtualServerID> "<groupID>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteGroup <virtualServerID> \"<groupID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteGroup(TextUtils.UnQuoteString(args[1]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetGroupMembers

        private void GetGroupMembers(string argsText)
        {
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetGroupMembers <virtualServerID> <groupID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent group

                        string[] members = virtualServer.API.GetGroupMembers(GroupName_From_GroupID(virtualServer.API.GetGroups(),args[1]));
                        DataSet ds = new DataSet();
                        DataTable dt = ds.Tables.Add("Members");
                        dt.Columns.Add("Member");
                        foreach(string member in members){
                            DataRow dr = dt.NewRow();
                            dr["Member"] = member;
                            dt.Rows.Add(dr);
                        }
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddGroupMember

        private void AddGroupMember(string argsText)
        {
            /* AddGroupMember <virtualServerID> "<groupID>" "<member>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddGroupMember <virtualServerID> \"<groupID>\" \"<member>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddGroupMember(GroupName_From_GroupID(virtualServer.API.GetGroups(),TextUtils.UnQuoteString(args[1])),TextUtils.UnQuoteString(args[2]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteGroupMember

        private void DeleteGroupMember(string argsText)
        {
            /* DeletGroupMember <virtualServerID> "<groupID>" "<member>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeletGroupMember <virtualServerID> \"<groupID>\" \"<member>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteGroupMember(GroupName_From_GroupID(virtualServer.API.GetGroups(),TextUtils.UnQuoteString(args[1])),TextUtils.UnQuoteString(args[2]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetMailingLists

        private void GetMailingLists(string argsText)
        {
            try{
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == argsText.ToLower()){
                        DataSet ds = virtualServer.API.GetMailingLists("ALL").Table.DataSet;
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddMailingList

        private void AddMailingList(string argsText)
        {
            /* AddMailingList <virtualServerID> "<mailingListID>" "<mailingListName>" "<description>" <enabled>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 5){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddMailingList <virtualServerID> \"<mailingListID>\" \"<mailingListName>\" \"<description>\" <enabled>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddMailingList(TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2]),TextUtils.UnQuoteString(args[3]),"",Convert.ToBoolean(args[4]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateMailingList

        private void UpdateMailingList(string argsText)
        {
            /* UpdateMailingList <virtualServerID> "<mailingListID>" "<mailingListName>" "<description>" <enabled>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 5){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateGroup <virtualServerID> \"<mailingListID>\" \"<mailingListName>\" \"<description>\" <enabled>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateMailingList(TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2]),TextUtils.UnQuoteString(args[3]),"",Convert.ToBoolean(args[4]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteMailingList

        private void DeleteMailingList(string argsText)
        {
            /* DeleteMailingList <virtualServerID> "<mailingListID>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteMailingList <virtualServerID> \"<mailingListID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteMailingList(TextUtils.UnQuoteString(args[1]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetMailingListMembers

        private void GetMailingListMembers(string argsText)
        {
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetMailingListMembers <virtualServerID> <mailignListID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent mailing list

                        DataSet ds = DataView_To_DataSet(virtualServer.API.GetMailingListAddresses(MailingListName_From_ID(virtualServer.API.GetMailingLists("ALL"),args[1])));
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddMailingListMember

        private void AddMailingListMember(string argsText)
        {
            /* AddMailingListMember <virtualServerID> "<mailingListID>" "<member>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddMailingListMember <virtualServerID> \"<mailingListID>\" \"<member>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddMailingListAddress(Guid.NewGuid().ToString(),MailingListName_From_ID(virtualServer.API.GetMailingLists("ALL"),TextUtils.UnQuoteString(args[1])),TextUtils.UnQuoteString(args[2]));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteMailingListMember

        private void DeleteMailingListMember(string argsText)
        {
            /* DeleteMailingListMember <virtualServerID> "<mailingListID>" "<member>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteMailingListMember <virtualServerID> \"<mailingListID>\" \"<member>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){                        
                        virtualServer.API.DeleteMailingListAddress(AddressID_From_MailingListMember(virtualServer.API.GetMailingListAddresses(MailingListName_From_ID(virtualServer.API.GetMailingLists("ALL"),TextUtils.UnQuoteString(args[1]))),TextUtils.UnQuoteString(args[2])));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetMailingListAcl

        private void GetMailingListAcl(string argsText)
        {
            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetMailingListMembers <virtualServerID> <mailignListID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent mailing list

                        DataSet ds = DataView_To_DataSet(virtualServer.API.GetMailingListACL(MailingListName_From_ID(virtualServer.API.GetMailingLists("ALL"),args[1])));
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddMailingListAcl

        private void AddMailingListAcl(string argsText)
        {
            /* AddMailingListAcl <virtualServerID> "<mailingListID>" "<userOrGroup>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddMailingListAcl <virtualServerID> \"<mailingListID>\" \"<userOrGroup>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddMailingListACL(MailingListName_From_ID(virtualServer.API.GetMailingLists("ALL"),TextUtils.UnQuoteString(args[1])),TextUtils.UnQuoteString(args[2]));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteMailingListAcl

        private void DeleteMailingListAcl(string argsText)
        {
            /* DeleteMailingListAcl <virtualServerID> "<mailingListID>" "<userOrGroup>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteMailingListAcl <virtualServerID> \"<mailingListID>\" \"<userOrGroup>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteMailingListACL(MailingListName_From_ID(virtualServer.API.GetMailingLists("ALL"),TextUtils.UnQuoteString(args[1])),TextUtils.UnQuoteString(args[2]));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion



        #region method GetGlobalMessageRules

        private void GetGlobalMessageRules(string argsText)
        {
            /* GetGlobalMessageRules <virtualServerID>
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == argsText.ToLower()){
                        DataSet ds = DataView_To_DataSet(virtualServer.API.GetGlobalMessageRules());
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddGlobalMessageRule

        private void AddGlobalMessageRule(string argsText)
        {
            /* AddGlobalMessageRule <virtualServerID> "<ruleID>" <cost> <enabled> "<description>" "<matchExpression>" <checkNext>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 7){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddGlobalMessageRule <virtualServerID> \"<ruleID>\" <cost> <enabled> \"<description>\" \"<matchExpression>\" <checkNext>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddGlobalMessageRule(
                            TextUtils.UnQuoteString(args[1]),
                            Convert.ToInt64(args[2]),
                            Convert.ToBoolean(args[3]),
                            (GlobalMessageRule_CheckNextRule_enum)Convert.ToInt32(args[6]),
                            TextUtils.UnQuoteString(args[4]),
                            TextUtils.UnQuoteString(args[5])
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateGlobalMessageRule

        private void UpdateGlobalMessageRule(string argsText)
        {
            /* UpdateGlopbalMessageRule <virtualServerID> "<ruleID>" <cost> <enabled> "<description>" "<matchExpression>" <checkNext>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 7){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateGlopbalMessageRule <virtualServerID> \"<ruleID>\" <cost> <enabled> \"<description>\" \"<matchExpression>\" <checkNext>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateGlobalMessageRule(
                            TextUtils.UnQuoteString(args[1]),
                            Convert.ToInt64(args[2]),
                            Convert.ToBoolean(args[3]),
                            (GlobalMessageRule_CheckNextRule_enum)Convert.ToInt32(args[6]),
                            TextUtils.UnQuoteString(args[4]),
                            TextUtils.UnQuoteString(args[5])
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteGlobalMessageRule

        private void DeleteGlobalMessageRule(string argsText)
        {
            /* DeleteGlobalMessageRule <virtualServerID> "<ruleID>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteGlobalMessageRule <virtualServerID> \"<ruleID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteGlobalMessageRule(
                            TextUtils.UnQuoteString(args[1])
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetGlobalMessageRuleActions

        private void GetGlobalMessageRuleActions(string argsText)
        {
            /* GetGlobalMessageRuleActions <virtualServerID> "<messageRuleID>"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetGlobalMessageRuleActions <virtualServerID> \"<messageRuleID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        DataSet ds = DataView_To_DataSet(virtualServer.API.GetGlobalMessageRuleActions(TextUtils.UnQuoteString(args[1])));
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddGlobalMessageRuleAction

        private void AddGlobalMessageRuleAction(string argsText)
        {
            /* AddGlobalMessageRuleAction <virtualServerID> "<messageRuleID>" "<messageRuleActionID>" "<description>" <actionType> "<actionData>:base64"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 6){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddGlobalMessageRuleAction <virtualServerID> \"<messageRuleID>\" \"<messageRuleActionID>\" \"<description>\" <actionType> \"<actionData>:base64\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddGlobalMessageRuleAction(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            (GlobalMessageRuleAction_enum)Convert.ToInt32(args[4]),
                            Convert.FromBase64String(TextUtils.UnQuoteString(args[5]))
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateGlobalMessageRuleAction

        private void UpdateGlobalMessageRuleAction(string argsText)
        {
            /* UpdateGlobalMessageRuleAction <virtualServerID> "<messageRuleID>" "<messageRuleActionID>" "<description>" <actionType> "<actionData>:base64"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 6){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateGlobalMessageRuleAction <virtualServerID> \"<messageRuleID>\" \"<messageRuleActionID>\" \"<description>\" <actionType> \"<actionData>:base64\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateGlobalMessageRuleAction(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            (GlobalMessageRuleAction_enum)Convert.ToInt32(args[4]),
                            Convert.FromBase64String(TextUtils.UnQuoteString(args[5]))
                        );
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteGlobalMessageRuleAction

        private void DeleteGlobalMessageRuleAction(string argsText)
        {
            /* DeleteGlobalMessageRuleAction <virtualServerID> "<messageRuleID>" "<messageRuleActionID>"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteGlobalMessageRuleAction <virtualServerID> \"<messageRuleID>\" \"<messageRuleActionID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteGlobalMessageRuleAction(TextUtils.UnQuoteString(args[1]),TextUtils.UnQuoteString(args[2]));
                        
                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetRoutes

        private void GetRoutes(string argsText)
        {
            /* GetRoutes <virtualServerID>
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == argsText.ToLower()){
                        DataSet ds = virtualServer.API.GetRoutes().Table.DataSet;
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddRoute

        private void AddRoute(string argsText)
        {
            /* AddRoute <virtualServerID> "<routeID>" <cost> "<description>" "<pattern>" <enabled> <actionType> "<actionData>:base64"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 8){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddRoute <virtualServerID> \"<routeID>\" <cost> \"<description>\" \"<pattern>\" <enabled> <actionType> \"<actionData>:base64\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddRoute(
                            TextUtils.UnQuoteString(args[1]),
                            Convert.ToInt64(args[2]),
                            Convert.ToBoolean(args[5]),
                            TextUtils.UnQuoteString(args[3]),
                            TextUtils.UnQuoteString(args[4]),
                            (RouteAction_enum)Convert.ToInt32(args[6]),
                            Convert.FromBase64String(TextUtils.UnQuoteString(args[7]))
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateRoute

        private void UpdateRoute(string argsText)
        {
            /* UpdateRoute <virtualServerID> "<routeID>" <cost> "<description>" "<pattern>" <enabled> <actionType> "<actionData>:base64"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 8){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateRoute <virtualServerID> \"<routeID>\" <cost> \"<description>\" \"<pattern>\" <enabled> <actionType> \"<actionData>:base64\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateRoute(
                            TextUtils.UnQuoteString(args[1]),
                            Convert.ToInt64(args[2]),
                            Convert.ToBoolean(args[5]),
                            TextUtils.UnQuoteString(args[3]),
                            TextUtils.UnQuoteString(args[4]),
                            (RouteAction_enum)Convert.ToInt32(args[6]),
                            Convert.FromBase64String(TextUtils.UnQuoteString(args[7]))
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion
        
        #region method DeleteRoute

        private void DeleteRoute(string argsText)
        {
            /* DeleteRoute <virtualServerID> "<routeID>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteRoute <virtualServerID> \"<routeID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteRoute(TextUtils.UnQuoteString(args[1]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetSharedRootFolders

        private void GetSharedRootFolders(string argsText)
        {
            /* GetSharedRootFolders <virtualServerID>
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == argsText.ToLower()){
                        DataSet ds = new DataSet();
                        ds.Tables.Add("SharedFoldersRoots");
                        ds.Tables["SharedFoldersRoots"].Columns.Add("RootID");
                        ds.Tables["SharedFoldersRoots"].Columns.Add("Enabled");
                        ds.Tables["SharedFoldersRoots"].Columns.Add("Folder");
                        ds.Tables["SharedFoldersRoots"].Columns.Add("Description");
                        ds.Tables["SharedFoldersRoots"].Columns.Add("RootType");
                        ds.Tables["SharedFoldersRoots"].Columns.Add("BoundedUser");
                        ds.Tables["SharedFoldersRoots"].Columns.Add("BoundedFolder");
                        foreach(SharedFolderRoot root in virtualServer.API.GetSharedFolderRoots()){
                            DataRow dr = ds.Tables["SharedFoldersRoots"].NewRow();
                            dr["RootID"]        = root.RootID;
                            dr["Enabled"]       = root.Enabled;
                            dr["Folder"]        = root.FolderName;
                            dr["Description"]   = root.Description;
                            dr["RootType"]      = (int)root.RootType;
                            dr["BoundedUser"]   = root.BoundedUser;
                            dr["BoundedFolder"] = root.BoundedFolder;
                            ds.Tables["SharedFoldersRoots"].Rows.Add(dr);
                        }
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddSharedRootFolder

        private void AddSharedRootFolder(string argsText)
        {
            /* AddSharedRootFolder <virtualServerID> "<rootFolderID>" "<rootFolderName>" "<description>" <type> "<boundedUser>" "boundedFolder" <enabled>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 8){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddSharedRootFolder <virtualServerID> \"<rootFolderID>\" \"<rootFolderName>\" \"<description>\" <type> \"<boundedUser>\" \"boundedFolder\" <enabled>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddSharedFolderRoot(
                            TextUtils.UnQuoteString(args[1]),
                            Convert.ToBoolean(args[7]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            (SharedFolderRootType_enum)Convert.ToInt32(args[4]),
                            TextUtils.UnQuoteString(args[5]),
                            TextUtils.UnQuoteString(args[6])
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateSharedRootFolder

        private void UpdateSharedRootFolder(string argsText)
        {
            /* UpdateSharedRootFolder <virtualServerID> "<rootFolderID>" "<rootFolderName>" "<description>" <type> "<boundedUser>" "boundedFolder" <enabled>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 8){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateSharedRootFolder <virtualServerID> \"<rootFolderID>\" \"<rootFolderName>\" \"<description>\" <type> \"<boundedUser>\" \"boundedFolder\" <enabled>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateSharedFolderRoot(
                            TextUtils.UnQuoteString(args[1]),
                            Convert.ToBoolean(args[7]),
                            TextUtils.UnQuoteString(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            (SharedFolderRootType_enum)Convert.ToInt32(args[4]),
                            TextUtils.UnQuoteString(args[5]),
                            TextUtils.UnQuoteString(args[6])
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteSharedRootFolder

        private void DeleteSharedRootFolder(string argsText)
        {
            /* DeleteSharedRootFolder <virtualServerID> "<rootFolderID>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteSharedRootFolder <virtualServerID> \"<rootFolderID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteSharedFolderRoot(TextUtils.UnQuoteString(args[1]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetFilterTypes

        private void GetFilterTypes(string argsText)
        {
            /* GetFilterTypes <virtualServerID>
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 1){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetFilterTypes <virtualServerID>");
                    return;
                }

                DataSet ds = new DataSet();
                ds.Tables.Add("Filters");
                ds.Tables["Filters"].Columns.Add("AssemblyName");
                ds.Tables["Filters"].Columns.Add("TypeName");
                string[] files = Directory.GetFiles(m_pServer.MailServer.StartupPath + "Filters");
                foreach(string file in files){
                    try{
                        if(Path.GetExtension(file) == ".exe" || Path.GetExtension(file) == ".dll"){
                            Assembly assembly = Assembly.LoadFile(file);
                            Type[] types = assembly.GetTypes();
                            foreach(Type type in types){
                                if(type.GetInterface(typeof(ISmtpMessageFilter).ToString()) != null || type.GetInterface(typeof(ISmtpSenderFilter).ToString()) != null || type.GetInterface(typeof(ISmtpUserMessageFilter).ToString()) != null){
                                    DataRow dr = ds.Tables["Filters"].NewRow();
                                    dr["AssemblyName"] = Path.GetFileName(file);
                                    dr["TypeName"]     = type.ToString();
                                    ds.Tables["Filters"].Rows.Add(dr);
                                }
                            }
                        }
                    }
                    catch{
                    }
                }
                
                // Compress data
                byte[] dsZipped = CompressDataSet(ds);

                this.Socket.WriteLine("+OK " + dsZipped.Length);
                this.Socket.Write(dsZipped);
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }            
        }

        #endregion

        #region method GetFilters

        private void GetFilters(string argsText)
        {
            /* GetFilters <virtualServerID>
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == argsText.ToLower()){
                        DataSet ds = virtualServer.API.GetFilters().Table.DataSet;
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddFilter

        private void AddFilter(string argsText)
        {
            /* AddFilter <virtualServerID> "<filterID>" <cost> "<description>" <"assembly>" "<filterClass>" <enabled>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 7){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddFilter <virtualServerID> \"<filterID>\" <cost> \"<description>\" \"<assembly>\" \"<filterClass>\" <enabled>");
                    return;
                }
                
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        //--- Get filter type ---------------------------------------------------//
                        string type = "";
                        try{
                            string assemblyFile = TextUtils.UnQuoteString(args[4]);
                            if(assemblyFile.IndexOf(":") == -1){
                                assemblyFile = m_pServer.MailServer.StartupPath + "\\Filters\\" + assemblyFile;
                            }
                            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(assemblyFile);
                            Type t = assembly.GetType(TextUtils.UnQuoteString(args[5]));
                            if(t == null){
                                this.Socket.WriteLine("-ERR filterClass contains invalid value !");
                                return;
                            }

                            if(t.GetInterface("LumiSoft.MailServer.ISmtpMessageFilter") != null){
                                type = "ISmtpMessageFilter";
                            }
                            else if(t.GetInterface("LumiSoft.MailServer.ISmtpSenderFilter") != null){
                                type = "ISmtpSenderFilter";
                            }
                            else if(t.GetInterface("LumiSoft.MailServer.ISmtpUserMessageFilter") != null){
                                type = "ISmtpUserMessageFilter";
                            }
                        }
                        catch(Exception x){                            
                            this.Socket.WriteLine("-ERR " + x.Message);
                            return;
                        }
                        //----------------------------------------------------------------------//

                        virtualServer.API.AddFilter(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[3]),
                            type,
                            TextUtils.UnQuoteString(args[4]),
                            TextUtils.UnQuoteString(args[5]),
                            Convert.ToInt64(args[2]),
                            Convert.ToBoolean(TextUtils.UnQuoteString(args[6]))
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateFilter

        private void UpdateFilter(string argsText)
        {
            /* UpdateFilter <virtualServerID> "<filterID>" <cost> "<description>" <"assembly>" "<filterClass>" <enabled>
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 7){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddFilter <virtualServerID> \"<filterID>\" <cost> \"<description>\" \"<assembly>\" \"<filterClass>\" <enabled>");
                    return;
                }
                
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        //--- Get filter type ---------------------------------------------------//
                        string type = "";
                        try{
                            string assemblyFile = TextUtils.UnQuoteString(args[4]);
                            if(assemblyFile.IndexOf(":") == -1){
                                assemblyFile = m_pServer.MailServer.StartupPath + "\\Filters\\" + assemblyFile;
                            }
                            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(assemblyFile);
                            Type t = assembly.GetType(TextUtils.UnQuoteString(args[5]));
                            if(t == null){
                                this.Socket.WriteLine("-ERR filterClass contains invalid value !");
                                return;
                            }

                            if(t.GetInterface("LumiSoft.MailServer.ISmtpMessageFilter") != null){
                                type = "ISmtpMessageFilter";
                            }
                            else if(t.GetInterface("LumiSoft.MailServer.ISmtpSenderFilter") != null){
                                type = "ISmtpSenderFilter";
                            }
                            else if(t.GetInterface("LumiSoft.MailServer.ISmtpUserMessageFilter") != null){
                                type = "ISmtpUserMessageFilter";
                            }
                        }
                        catch(Exception x){                            
                            this.Socket.WriteLine("-ERR " + x.Message);
                            return;
                        }
                        //----------------------------------------------------------------------//

                        virtualServer.API.UpdateFilter(
                            TextUtils.UnQuoteString(args[1]),
                            TextUtils.UnQuoteString(args[3]),
                            type,
                            TextUtils.UnQuoteString(args[4]),
                            TextUtils.UnQuoteString(args[5]),
                            Convert.ToInt64(args[2]),
                            Convert.ToBoolean(TextUtils.UnQuoteString(args[6]))
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteFilter

        private void DeleteFilter(string argsText)
        {
            /* DeleteFilter <virtualServerID> "<filterID>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteFilter <virtualServerID> \"<filterID>\"");
                    return;
                }
                
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteFilter(TextUtils.UnQuoteString(args[1]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetIPSecurity

        private void GetIPSecurity(string argsText)
        {
            /* GetIPSecurity <virtualServerID>
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == argsText.ToLower()){
                        DataSet ds = virtualServer.API.GetSecurityList().Table.DataSet;
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method AddIPSecurityEntry

        private void AddIPSecurityEntry(string argsText)
        {
            /* AddIPSecurityEntry <virtualServerID> "<securityEntryID>" enabled "<description>" <service> <action> "<startIP>" "<endIP>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 8){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: AddIPSecurityEntry <virtualServerID> \"<securityEntryID>\" enabled \"<description>\" <service> <action> \"<startIP>\" \"<endIP>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.AddSecurityEntry(
                            TextUtils.UnQuoteString(args[1]),
                            Convert.ToBoolean(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            (Service_enum)Convert.ToInt32(args[4]),
                            (IPSecurityAction_enum)Convert.ToInt32(args[5]),
                            IPAddress.Parse(TextUtils.UnQuoteString(args[6])),
                            IPAddress.Parse(TextUtils.UnQuoteString(args[7]))
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateIPSecurityEntry

        private void UpdateIPSecurityEntry(string argsText)
        {
            /* UpdateIPSecurityEntry <virtualServerID> "<securityEntryID>" enabled "<description>" <service> <action> "<startIP>" "<endIP>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 8){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateIPSecurityEntry <virtualServerID> \"<securityEntryID>\" enabled \"<description>\" <service> <action> \"<startIP>\" \"<endIP>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateSecurityEntry(
                            TextUtils.UnQuoteString(args[1]),
                            Convert.ToBoolean(args[2]),
                            TextUtils.UnQuoteString(args[3]),
                            (Service_enum)Convert.ToInt32(args[4]),
                            (IPSecurityAction_enum)Convert.ToInt32(args[5]),
                            IPAddress.Parse(TextUtils.UnQuoteString(args[6])),
                            IPAddress.Parse(TextUtils.UnQuoteString(args[7]))
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method DeleteIPSecurityEntry

        private void DeleteIPSecurityEntry(string argsText)
        {
            /* DeleteIPSecurityEntry <virtualServerID> "<securityEntryID>"
                  Responses:
                    +OK                     
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: DeleteIPSecurityEntry <virtualServerID> \"<securityEntryID>\"");
                    return;
                }
                
                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.DeleteSecurityEntry(TextUtils.UnQuoteString(args[1]));

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetQueue

        private void GetQueue(string argsText)
        {
            /* GetQueue <virtualServerID> <queueType>
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetQueue <virtualServerID> <queueType>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        DataSet ds = new DataSet();
                        ds.Tables.Add("Queue");
                        ds.Tables["Queue"].Columns.Add("CreateTime",typeof(DateTime));
                        ds.Tables["Queue"].Columns.Add("Header");

                        string folder = "";
                        // SMTP queue
                        if(Convert.ToInt32(args[1]) == 1){
                            folder = virtualServer.MailStorePath + "IncomingSMTP";
                        }
                        // Relay queue
                        else{
                            folder = virtualServer.MailStorePath + "Retry";
                        }
                        string[] files = Directory.GetFiles(folder,"*.eml");
                        foreach(string file in files){
                            try{                                
                                string header = "";
                                using(FileStream fs = new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.Read)){                
                                    header = LumiSoft.Net.Mime.MimeUtils.ParseHeaders(fs);
                                }
                                if(header.Length > 2000){
                                    header = header.Substring(0,2000);
                                }

                                DataRow dr = ds.Tables["Queue"].NewRow();
                                dr["CreateTime"] = File.GetCreationTime(file);
                                dr["Header"]     = header;
                                ds.Tables["Queue"].Rows.Add(dr);
                            }
                            catch{
                            }
                        }

                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion


        #region method GetRecycleBinSettings

        private void GetRecycleBinSettings(string argsText)
        {
            /* GetRecycleBinSettings <virtualServerID>
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 1){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetRecycleBinSettings <virtualServerID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        MemoryStream ms = new MemoryStream();
                        virtualServer.API.GetRecycleBinSettings().DataSet.WriteXml(ms);
                        ms.Position = 0;

                        this.Socket.WriteLine("+OK " + ms.Length);
                        this.Socket.Write(ms);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method UpdateRecycleBinSettings

        private void UpdateRecycleBinSettings(string argsText)
        {
            /* UpdateRecycleBinSettings <virtualServerID> <deleteToRecycleBin> <deleteMessagesAfter>
                  Responses:
                    +OK <sizeOfData>                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 3){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: UpdateRecycleBinSettings <virtualServerID> <deleteToRecycleBin> <deleteMessagesAfter>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.UpdateRecycleBinSettings(
                            Convert.ToBoolean(args[1]),
                            Convert.ToInt32(args[2])
                        );

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion
        
        #region method GetRecycleBinMessagesInfo

        private void GetRecycleBinMessagesInfo(string argsText)
        {
            /* GetRecycleBinMessagesInfo <virtualServerID> "<user>" "<startDate>" "<endDate>"
                  Responses:
                    +OK <sizeOfData>
                    <data>
                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 4){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetRecycleBinMessagesInfo <virtualServerID> \"<user>\" \"<startDate>\" \"<endDate>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        DataSet ds = virtualServer.API.GetRecycleBinMessagesInfo(args[1],Convert.ToDateTime(args[2]),Convert.ToDateTime(args[3])).Table.DataSet;
                        
                        // Compress data
                        byte[] dsZipped = CompressDataSet(ds);

                        this.Socket.WriteLine("+OK " + dsZipped.Length);
                        this.Socket.Write(dsZipped);
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method GetRecycleBinMessage

        private void GetRecycleBinMessage(string argsText)
        {
            /* C: GetRecycleBinMessage <virtualServerID> "<messageID>"
               S: +OK <sizeInBytes>
               S: <messageData>
                            
                  Responses:
                    +OK               
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ',true);
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: GetRecycleBinMessage <virtualServerID> \"<messageID>\"");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){ 
                        // TODO: handle no existent user
                        // TODO: handle no existent folder

                        
                        Stream messageStream = virtualServer.API.GetRecycleBinMessage(args[1]);
                        try{
                            this.Socket.WriteLine("+OK " + ConvertEx.ToString(messageStream.Length - messageStream.Position));
                            this.Socket.Write(messageStream);
                        }
                        finally{
                            messageStream.Close();
                        }
                                                
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + args[0] + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        #region method RestoreRecycleBinMessage

        private void RestoreRecycleBinMessage(string argsText)
        {
            /* RestoreFromRecycleBin <virtualServerID> <messageID>
                  Responses:
                    +OK <sizeOfData>                    
                    -ERR <errorText>
            */

            try{
                string[] args = TextUtils.SplitQuotedString(argsText,' ');
                if(args.Length != 2){
                    this.Socket.WriteLine("-ERR Invalid arguments. Syntax: RestoreRecycleBinMessage <virtualServerID> <messageID>");
                    return;
                }

                foreach(VirtualServer virtualServer in m_pServer.MailServer.VirtualServers){
                    if(virtualServer.ID.ToLower() == args[0].ToLower()){
                        virtualServer.API.RestoreRecycleBinMessage(args[1]);

                        this.Socket.WriteLine("+OK");
                        return;
                    }
                }

                this.Socket.WriteLine("-ERR Specified virtual server with ID '" + argsText + "' doesn't exist !");
            }
            catch(Exception x){
                this.Socket.WriteLine("-ERR " + x.Message);
            }
        }

        #endregion

        

        

		#region method QUIT

		/// <summary>
		/// QUIT command.
		/// </summary>
		private void QUIT()
		{
			this.Socket.WriteLine("+OK Service closing transmission channel");
		}

		#endregion


		#region method ObjectToString

		private string ObjectToString(Object val)
		{
			if(val != null){
				return val.ToString();
			}

			return "";
		}

		#endregion

        #region merhod CompressDataSet

        /// <summary>
        /// Compresses specified dataset with GZIP.
        /// </summary>
        /// <param name="ds">DataSet to compress.</param>
        /// <returns>Returns gzipped dataset xml data.</returns>
        private byte[] CompressDataSet(DataSet ds)
        {
            /* Mono won't support it
            MemoryStream ms = new MemoryStream();
            ds.WriteXml(ms);
            
            MemoryStream retVal = new MemoryStream();
            GZipStream gzip = new GZipStream(retVal,CompressionMode.Compress);

            byte[] dsBytes = ms.ToArray();
            gzip.Write(dsBytes,0,dsBytes.Length);
            gzip.Flush();
            gzip.Dispose();

            return retVal.ToArray();
            */

            MemoryStream ms = new MemoryStream();
            ds.WriteXml(ms);
            
            MemoryStream retVal = new MemoryStream();
            GZipOutputStream gzip = new GZipOutputStream(retVal);

            byte[] dsBytes = ms.ToArray();
            gzip.Write(dsBytes,0,dsBytes.Length);
            gzip.Flush();
            gzip.Dispose();

            return retVal.ToArray();
        }

        #endregion

        // REMOVE ME:
        private string UserName_From_UserID(DataView users,string userID)
        {
            foreach(DataRowView drV in users){
                if(drV["UserID"].ToString() == userID){
                    return drV["UserName"].ToString();
                }
            }

            throw new Exception("Specified userID '" + userID + "' doesn't exist !");
        }

        // REMOVE ME:
        private string GroupName_From_GroupID(DataView groups,string groupID)
        {
            foreach(DataRowView drV in groups){
                if(drV["GroupID"].ToString() == groupID){
                    return drV["GroupName"].ToString();
                }
            }

            throw new Exception("Specified groupID '" + groupID + "' doesn't exist !");
        }

        // REMOVE ME:
        private string MailingListName_From_ID(DataView mailingLists,string mailingListID)
        {
            foreach(DataRowView drV in mailingLists){
                if(drV["MailingListID"].ToString() == mailingListID){
                    return drV["MailingListName"].ToString();
                }
            }

            throw new Exception("Specified mailingList ID '" + mailingListID + "' doesn't exist !");
        }

        // REMOVE ME:
        private string AddressID_From_MailingListMember(DataView mailingListMembers,string member)
        {
            foreach(DataRowView drV in mailingListMembers){
                if(drV["Address"].ToString() == member.ToLower()){
                    return drV["AddressID"].ToString();
                }
            }

            throw new Exception("Specified mailing list member '" + member + "' doesn't exist !");
        }

        // REMOVE ME:
        private string AddressID_From_UserEmail(DataView userEmails,string emailAddress)
        {
            foreach(DataRowView drV in userEmails){
                if(drV["Address"].ToString() == emailAddress.ToLower()){
                    return drV["AddressID"].ToString();
                }
            }

            throw new Exception("Specified emailaddress '" + emailAddress + "' doesn't exist !");
        }

        #region method DataView_To_DataSet

        /// <summary>
        /// Copies DataView.Table stucture and it's data to new data set.
        /// </summary>
        /// <param name="dv"></param>
        /// <returns></returns>
        private DataSet DataView_To_DataSet(DataView dv)
        {
            DataSet ds = new DataSet();
            DataTable dt = dv.Table.Clone();
            ds.Tables.Add(dt);

            foreach(DataRowView drV in dv){
                dt.ImportRow(drV.Row);
            }

            return ds;
        }

        #endregion

    }
}
