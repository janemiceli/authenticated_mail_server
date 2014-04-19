using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Net;

using LumiSoft.Net;
using LumiSoft.Net.AUTH;
using LumiSoft.Net.IMAP;
using LumiSoft.Net.IMAP.Server;
using LumiSoft.Net.Mime;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// XML API.
	/// </summary>
	public class xml_API : IMailServerApi
	{
		private string  m_DataPath               = "";
		private string  m_MailStorePath          = "";
		private DataSet dsSettings               = null;
		private DataSet dsUsers                  = null;
		private DataSet dsUserAddresses          = null;
		private DataSet dsUserRemoteServers      = null;
		private DataSet dsUserMessageRules       = null;
        private DataSet dsUserMessageRuleActions = null;
		private DataSet dsUserForwards           = null;
        private DataSet dsGroups                 = null;
        private DataSet dsGroupMembers           = null;
		private DataSet dsMailingLists           = null;
		private DataSet dsMailingListAddresses   = null;
        private DataSet dsMailingListACL         = null;
		private DataSet dsDomains                = null;
        private DataSet dsRules                  = null;
        private DataSet dsRuleActions            = null;
		private DataSet dsRouting                = null;
		private DataSet dsSecurity               = null;
		private DataSet dsFilters                = null;
		private DataSet dsImapACL                = null;
        private DataSet dsSharedFolderRoots      = null;
        private DataSet dsUsersDefaultFolders    = null;
        private DataSet dsRecycleBinSettings     = null;
		private DateTime m_UsersDate;              
		private DateTime m_UserAddressesDate;      
		private DateTime m_UsersRemoteServers;
		private DateTime m_UserMessageRules;
        private DateTime m_UserMessageRuleActions;
        private DateTime m_GroupsDate;
        private DateTime m_GroupMembersDate;
		private DateTime m_MailingListsDate;       
		private DateTime m_MailingListAddressesDate;
        private DateTime m_MailingListAclDate;
        private DateTime m_RulesDate;
        private DateTime m_RuleActionsDate;
		private DateTime m_RoutingDate;
		private DateTime m_DomainsDate;
		private DateTime m_SecurityDate;
		private DateTime m_FiltersDate;
		private DateTime m_ImapACLDate;
        private DateTime m_SharedFolderRootsDate;
        private DateTime m_UsersDefaultFoldersDate;
        private DateTime m_RecycleBinSettingsDate;
		private System.Timers.Timer timer1;
		private UpdateSync m_UpdSync = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="intitString"></param>
		public xml_API(string intitString)
		{
			m_UpdSync = new UpdateSync();

            if(intitString == null || intitString == ""){
                throw new Exception("Init string can't be null or \"\" !");
            }
            
			// datapath=
			// mailstorepath=
			string[] parameters = intitString.Replace("\r\n","\n").Split('\n');
			foreach(string param in parameters){
				if(param.ToLower().IndexOf("datapath=") > -1){
					m_DataPath = param.Substring(9);
                    
					timer1 = new System.Timers.Timer(15000);					
					timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Elapsed);
					timer1.Enabled = true;
				}
				else if(param.ToLower().IndexOf("mailstorepath=") > -1){
					m_MailStorePath = param.Substring(14);
				}
			}
            
			// Fix data path, if isn't ending with \
			if(m_DataPath.Length > 0 && !m_DataPath.EndsWith("\\")){
				m_DataPath += "\\"; 
			}

			// Fix mail store path, if isn't ending with \
			if(m_MailStorePath.Length > 0 && !m_MailStorePath.EndsWith("\\")){
				m_MailStorePath += "\\"; 
			}
                                                
			if(!Path.IsPathRooted(m_DataPath)){
				m_DataPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\" + m_DataPath;
			}

            if(!Path.IsPathRooted(m_MailStorePath)){
				m_MailStorePath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\" + m_MailStorePath;
			}

            // Make path directory separator to suit for current platform
            m_DataPath = API_Utlis.PathFix(m_DataPath);
            m_MailStorePath = API_Utlis.PathFix(m_MailStorePath);

            // Settings folder doesn't exist, create it
            if(!Directory.Exists(m_DataPath)){
                Directory.CreateDirectory(m_DataPath);
            }

            // Set Recycle Bin path
            RecycleBinManager.RecycleBinPath = m_MailStorePath + "RecycleBin/";
            // RecycleBin folder doesn't exist, create it
            if(!Directory.Exists(m_MailStorePath + "RecycleBin/")){
                Directory.CreateDirectory(m_MailStorePath + "RecycleBin/");
            }
                        				
			dsSettings               = new DataSet();
			dsDomains                = new DataSet();
			dsUsers                  = new DataSet();
			dsUserAddresses          = new DataSet();
			dsUserRemoteServers      = new DataSet();
			dsUserMessageRules       = new DataSet();
            dsUserMessageRuleActions = new DataSet();
			dsUserForwards           = new DataSet();
            dsGroups                 = new DataSet();
            dsGroupMembers           = new DataSet();
			dsMailingLists           = new DataSet();
			dsMailingListAddresses   = new DataSet();
            dsMailingListACL         = new DataSet();
            dsRules                  = new DataSet();
            dsRuleActions            = new DataSet();
			dsRouting                = new DataSet();
			dsSecurity               = new DataSet();
			dsFilters                = new DataSet();
			dsImapACL                = new DataSet();
            dsSharedFolderRoots      = new DataSet();
            dsUsersDefaultFolders    = new DataSet();
            dsRecycleBinSettings     = new DataSet();

			timer1_Elapsed(this,null);
		}


		#region Domain related

		#region method GetDomains

		/// <summary>
		/// Gets domain list.
		/// </summary>
		/// <returns></returns>
		public DataView GetDomains()
		{
			m_UpdSync.AddMethod();

			try{
				return new DataView(dsDomains.Copy().Tables["Domains"]);
			}
			catch(Exception x){				
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}

		#endregion


		#region metod AddDomain

		/// <summary>
		/// Adds new domain.
		/// </summary>
		/// <param name="domainID">Domain ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="domainName">Domain name. Eg. yourDomain.com .</param>
		/// <param name="description">Domain description.</param>
		/// <remarks>Throws exception if specified domain already exists.</remarks>
		public void AddDomain(string domainID,string domainName,string description)
		{
			if(domainID.Length == 0){
				throw new Exception("You must specify domainID");
			}
			ArgsValidator.ValidateDomainName(domainName);

			m_UpdSync.BeginUpdate();

			try{
				if(!DomainExists(domainName)){
					if(!ContainsID(dsDomains.Tables["Domains"],"DomainID",domainID)){
						DataRow dr = dsDomains.Tables["Domains"].NewRow();
						dr["DomainID"]    = domainID;
						dr["DomainName"]  = domainName;
						dr["Description"] = description;
									
						dsDomains.Tables["Domains"].Rows.Add(dr);
						dsDomains.WriteXml(m_DataPath + "Domains.xml",XmlWriteMode.IgnoreSchema);
					}
					else{
						throw new Exception("Domain with specified ID '" + domainID + "' already exists !");
					}
				}
				else{
					throw new Exception("Domain '" + domainName + "' already exists !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}

		#endregion

		#region method DeleteDomain

		/// <summary>
		/// Deletes specified domain.
		/// </summary>
		/// <param name="domainID">Domain ID. Use <see cref="IMailServerApi.GetDomains">GetDomains()</see> to get valid values.</param>
		/// <remarks>Deletes specified domain and all domain related data (users,mailing lists,routes).</remarks>
		public void DeleteDomain(string domainID)
		{
			m_UpdSync.BeginUpdate();

			try{
				// 1) delete specified domain users,userAddresses,userRemoteServers
				// 2) delete specified domain mailing lists
				// 4) delete specified domain

				string domainName = "";
				//--- Find domain name from domain ID
				using(DataView dv = new DataView(dsDomains.Tables["Domains"])){
					dv.RowFilter = "DomainID='" + domainID + "'";

					if(dv.Count > 0){
						domainName = dv[0]["DomainName"].ToString();
					}
					else{
						throw new Exception("Domain with specified ID '" + domainID + "' doesn't exist !");
					}
				}

                // Delete that domain email addresss.
                foreach(DataRowView drv in GetUserAddresses("")){
                    if(drv["Address"].ToString().ToLower().EndsWith("@" + domainName.ToLower())){
                        DeleteUserAddress(drv["Address"].ToString());
                    }
                }
	            
				// Delete users
				using(DataView dv = GetUsers(domainName)){
					foreach(DataRowView drV in dv){
						DeleteUser(drV["UserID"].ToString());
					}
				}

				// Delete mailing lists
				using(DataView dv = GetMailingLists(domainName)){
					foreach(DataRowView drV in dv){
						DeleteMailingList(drV["MailingListID"].ToString());
					}
				}

				// Delete domain itself
				using(DataView dv = new DataView(dsDomains.Tables["Domains"])){
					dv.RowFilter = "DomainID='" + domainID + "'";

					if(dv.Count > 0){
						dv[0].Delete();
					}

					dsDomains.WriteXml(m_DataPath + "Domains.xml",XmlWriteMode.IgnoreSchema);
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}

		#endregion

        #region method UpdateDomain

        /// <summary>
        /// Updates specified domain data.
        /// </summary>
        /// <param name="domainID">Domain ID which to update.</param>
        /// <param name="domainName">Domain name.</param>
        /// <param name="description">Domain description.</param>
        public void UpdateDomain(string domainID,string domainName,string description)
        {
            ArgsValidator.ValidateDomainName(domainName);

            m_UpdSync.BeginUpdate();

            try{                
                // Ensure that domain with specified ID does exist.
                if(!ContainsID(dsDomains.Tables["Domains"],"DomainID",domainID)){
                    throw new Exception("Invalid domainID, specified domainID '" + domainID + "' doesn't exist !");
                }
                                
                foreach(DataRow dr in dsDomains.Tables["Domains"].Rows){
                    if(dr["DomainID"].ToString().ToLower() == domainID){
                        // If group name is changed, ensure that new group name won't conflict 
                        //   any other group or user name. 
                        if(dr["DomainName"].ToString().ToLower() != domainName.ToLower()){
                            if(DomainExists(domainName)){
                                throw new Exception("Invalid domainName, specified domainName '" + domainName + "' already exists !");
                            }

                            // Rename email addresses
                            foreach(DataRow drEmail in dsUserAddresses.Tables["UserAddresses"].Rows){
                                string[] localpart_domain = drEmail["Address"].ToString().Split('@');
                                if(localpart_domain[1].ToLower() == dr["DomainName"].ToString().ToLower()){
                                    drEmail["Address"] = localpart_domain[0] + "@" + domainName;
                                }
                            }
                            dsUserAddresses.WriteXml(m_DataPath + "UserAddresses.xml",XmlWriteMode.IgnoreSchema);

                            // Rename mailing lists
                            foreach(DataRow drEmail in dsMailingLists.Tables["MailingLists"].Rows){
                                string[] localpart_domain = drEmail["MailingListName"].ToString().Split('@');
                                if(localpart_domain[1].ToLower() == dr["DomainName"].ToString().ToLower()){
                                    drEmail["MailingListName"] = localpart_domain[0] + "@" + domainName;
                                }
                            }
                            dsMailingListAddresses.WriteXml(m_DataPath + "MailingListAddresses.xml",XmlWriteMode.IgnoreSchema);
                        }                        

				        dr["DomainName"]  = domainName;
				        dr["Description"] = description;

                        dsDomains.WriteXml(m_DataPath + "Domains.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
            }
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

		#region method DomainExists

		/// <summary>
		/// Checks if specified domain exists.
		/// </summary>
		/// <param name="source">Domain name or email address.</param>
		/// <returns>Returns true if domain exists.</returns>
		public bool DomainExists(string source)
		{
			m_UpdSync.AddMethod();

			try{
				// Source is Emails
				if(source.IndexOf("@") > -1){
					source = source.Substring(source.IndexOf("@") + 1);
				}

				foreach(DataRow dr in dsDomains.Tables["Domains"].Rows){
					if(dr["DomainName"].ToString().ToLower() == source.ToLower()){
						return true;
					}
				}

				return false;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}

		#endregion

		#endregion


		#region User and Groups related

		#region method GetUsers

		/// <summary>
		/// Gets user list in specified domain.
		/// </summary>
		/// <param name="domainName">Domain which user list to retrieve.To get all use value 'ALL'.</param>
		/// <returns></returns>
		public DataView GetUsers(string domainName)
		{
			m_UpdSync.AddMethod();

			try{
				DataView dv = new DataView(dsUsers.Copy().Tables["Users"]);
				if(domainName != "ALL"){
					dv.RowFilter = "DomainName='" + domainName + "'";
				}

				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}

		#endregion


        #region method GetUserID

        /// <summary>
        /// Gets user ID from user name. Returns null if user doesn't exist.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns>Returns user ID or null if user doesn't exist.</returns>
        public string GetUserID(string userName)
        {
            foreach(DataRow dr in dsUsers.Tables["Users"].Rows){
                if(dr["UserName"].ToString().ToLower() == userName.ToLower()){
                    return dr["UserID"].ToString();
                }
            }

            return null;
        }

        #endregion

		#region method AddUser

		/// <summary>
		/// Adds new user to specified domain.
		/// </summary>
		/// <param name="userID">User ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="userName">User login name.</param>
		/// <param name="fullName">User full name.</param> 
		/// <param name="password">User login password.</param>
		/// <param name="description">User description.</param>
		/// <param name="domainName">Domain where to add user. Use <see cref="IMailServerApi.GetDomains">GetDomains()</see> to get valid values.</param>
		/// <param name="mailboxSize">Maximum mailbox size.</param>
		/// <param name="enabled">Sepcifies if user is enabled.</param>
		/// <param name="permissions">Specifies user permissions.</param>
		/// <remarks>Throws exception if specified user already exists.</remarks>
		public void AddUser(string userID,string userName,string fullName,string password,string description,string domainName,int mailboxSize,bool enabled,UserPermissions_enum permissions)
		{			
			if(userID.Length == 0){
				throw new Exception("You must specify userID");
			}
			if(userName.Length == 0){
				throw new Exception("You must specify userName");
			}

			m_UpdSync.BeginUpdate();

			try{
				if(!UserExists(userName)){
					if(!ContainsID(dsUsers.Tables["Users"],"UserID",userID)){
						DataRow dr = dsUsers.Tables["Users"].NewRow();
						dr["UserID"]       = userID;
						dr["UserName"]     = userName;
						dr["DomainName"]   = domainName;
						dr["FullName"]     = fullName;			
						dr["Password"]     = password;
						dr["Description"]  = description;
						dr["Mailbox_Size"] = mailboxSize;
						dr["Enabled"]      = enabled;
						dr["Permissions"]  = (int)permissions;
                        dr["CreationTime"] = DateTime.Now;

						dsUsers.Tables["Users"].Rows.Add(dr);
						dsUsers.WriteXml(m_DataPath + "Users.xml",XmlWriteMode.IgnoreSchema);
					}
					else{
						throw new Exception("User with specified ID '" + userID + "' already exists !");
					}
				}
				else{
					throw new Exception("User '" + userName + "' already exists !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
	
		#endregion

		#region method DeleteUser

		/// <summary>
		/// Deletes user.
		/// </summary>
		/// <param name="userID">User id of the user which to delete. Use <see cref="IMailServerApi.GetUsers">>GetUsers()</see> to get valid values.</param>
		public void DeleteUser(string userID)
		{
			m_UpdSync.BeginUpdate();

			try{
				// 1) delete user addresses
				// 2) delete user remote servers
				// 3) delete user

				string userName = "";
				//--- Find user name from user ID
				using(DataView dv = new DataView(dsUsers.Tables["Users"])){
					dv.RowFilter = "UserID='" + userID + "'";

					if(dv.Count > 0){
						userName = dv[0]["UserName"].ToString();
					}
					else{
						throw new Exception("User with specified ID '" + userID + "' doesn't exist !");
					}
				}

				// delete user addresses
				using(DataView dv = GetUserAddresses(userName)){
					foreach(DataRowView drV in dv){
						DeleteUserAddress(drV["AddressID"].ToString());
					}
				}

				// delete user remote servers
				using(DataView dv = GetUserRemoteServers(userName)){
					foreach(DataRowView drV in dv){
						DeleteUserRemoteServer(drV["ServerID"].ToString());
					}
				}

				// delete user message rules
				using(DataView dv = GetUserMessageRules(userName)){
					foreach(DataRowView drV in dv){
						DeleteUserMessageRule(userID,drV["RuleID"].ToString());
					}
				}
						
				// delete user
				using(DataView dv = new DataView(dsUsers.Tables["Users"])){
					dv.RowFilter = "UserID='" + userID + "'";

					if(dv.Count > 0){
						dsUsers.Tables["Users"].Rows.Remove(dv[0].Row);							
					}

					dsUsers.WriteXml(m_DataPath + "Users.xml",XmlWriteMode.IgnoreSchema);
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region method UpdateUser

		/// <summary>
		/// Updates new user to specified domain.
		/// </summary>
		/// <param name="userID">User id of the user which to update. Use <see cref="IMailServerApi.GetUsers">>GetUsers()</see> to get valid values.</param>
		/// <param name="userName">User login name.</param>
		/// <param name="fullName">User full name.</param>
		/// <param name="password">User login password.</param>
		/// <param name="description">User description.</param>
		/// <param name="domainName">Domain where to add user. Use <see cref="IMailServerApi.GetDomains">>GetDomains()</see> to get valid values.</param>
		/// <param name="mailboxSize">Maximum mailbox size.</param>
		/// <param name="enabled">Sepcifies if user is enabled.</param>
		/// <param name="permissions">Specifies user permissions.</param>
		public void UpdateUser(string userID,string userName,string fullName,string password,string description,string domainName,int mailboxSize,bool enabled,UserPermissions_enum permissions)
		{			
			if(userName.Length == 0){
				throw new Exception("You must specify userName");
			}

			m_UpdSync.BeginUpdate();

			try{
				if(ContainsID(dsUsers.Tables["Users"],"UserID",userID)){
					using(DataView dv = new DataView(dsUsers.Tables["Users"])){
						dv.RowFilter = "UserID='" + userID + "'";

						//--- see if user with specified user name doesn't exist by other user
						using(DataView dvX = new DataView(dsUsers.Tables["Users"])){
							dvX.RowFilter = "UserName='" + userName + "'";

							if(dvX.Count > 0){
								// see if same user updated
								if(dvX[0]["UserID"].ToString() != userID){
									throw new Exception("User '" + userName + "' already exists !");
								}
							}
						}
                                                
						if(dv.Count > 0){
                            // Rename user mailbox folder
                            if(dv[0]["UserName"].ToString().ToLower() != userName.ToLower()){                                
                                string oldMailbox = API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + dv[0]["UserName"]));
                                if(oldMailbox != null){
                                    Directory.Move(oldMailbox,API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName));
                                }
                            }

							dv[0]["UserName"]     = userName;
							dv[0]["FullName"]     = fullName;
							dv[0]["Password"]     = password;
							dv[0]["Description"]  = description;
							dv[0]["DomainName"]   = domainName;
							dv[0]["Mailbox_Size"] = mailboxSize;
							dv[0]["Enabled"]      = enabled;
							dv[0]["Permissions"]  = (int)permissions;

							dsUsers.WriteXml(m_DataPath + "Users.xml",XmlWriteMode.IgnoreSchema);
						}
					}
				}
				else{
					throw new Exception("User with specified ID '" + userID + "' doesn't exist !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region method AddUserAddress

		/// <summary>
		/// Add new email address to user.
		/// </summary>
		/// <param name="userName">User name. Use <see cref="IMailServerApi.GetUsers">>GetUsers()</see> to get valid values.</param>
		/// <param name="emailAddress">Email address to add.</param>
		/// <remarks>Throws exception if specified user email address exists.</remarks>
		public void AddUserAddress(string userName,string emailAddress)
		{			
			if(userName.Length == 0){
				throw new Exception("You must specify userName");
			}
			if(emailAddress.Length == 0){
				throw new Exception("You must specify address");
			}

			m_UpdSync.BeginUpdate();

			try{
				if(MapUser(emailAddress) == null){
					// Get user ID from name
					string userID = "";
					using(DataView dv = new DataView(dsUsers.Tables["Users"])){
						dv.RowFilter = "UserName='" + userName + "'";
					
						userID = dv[0]["UserID"].ToString();
					}

					DataRow dr = dsUserAddresses.Tables["UserAddresses"].NewRow();
					dr["UserID"]  = userID;
					dr["Address"] = emailAddress;
									
					dsUserAddresses.Tables["UserAddresses"].Rows.Add(dr);
					dsUserAddresses.WriteXml(m_DataPath + "UserAddresses.xml",XmlWriteMode.IgnoreSchema);
				}
				else{
					throw new Exception("Address '" + emailAddress + "' already exists !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}

		#endregion

		#region method DeleteUserAddress

		/// <summary>
		/// Deletes specified email address from user. 
		/// </summary>
		/// <param name="emailAddress">Email address to delete.</param>
		public void DeleteUserAddress(string emailAddress)
		{
			m_UpdSync.BeginUpdate();

			try{
                foreach(DataRow dr in dsUserAddresses.Tables["UserAddresses"].Rows){
                    if(dr["Address"].ToString().ToLower() == emailAddress.ToLower()){
                        dr.Delete();
                        dsUserAddresses.WriteXml(m_DataPath + "UserAddresses.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}

		#endregion

		#region method GetUserAddresses

		/// <summary>
		/// Gets user email addresses.
		/// </summary>
		/// <param name="userName"> Use <see cref="IMailServerApi.GetUsers">GetUsers()</see> to get valid values.</param>
		public DataView GetUserAddresses(string userName)
		{	
			m_UpdSync.AddMethod();

			try{				
				string userID = "";

				// Find user LoginName for specified UserID
				foreach(DataRow drUser in dsUsers.Tables["Users"].Rows){
					if(userName.ToLower() == drUser["UserName"].ToString().ToLower()){
						userID =  drUser["UserID"].ToString();
						break;
					}
				}

				DataView dv = new DataView(dsUserAddresses.Copy().Tables["UserAddresses"]);
                if(userID != ""){
				    dv.RowFilter = "UserID='" + userID + "'";
                }
					
				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}

		#endregion

		#region method UserExists

		/// <summary>
		/// Checks if user exists.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <returns>Returns true if user exists.</returns>
		public bool UserExists(string userName)
		{
			m_UpdSync.AddMethod();

			try{
				foreach(DataRow dr in dsUsers.Tables["Users"].Rows){
					if(dr["UserName"].ToString().ToLower() == userName.ToLower()){
						return true;
					}
				}

				return false;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}
		
		#endregion

		#region method MapUser

		/// <summary>
		/// Maps email address to user. Returns user or null if such email address won't exist.
		/// </summary>
		/// <param name="emailAddress"></param>
		/// <returns>Returns user or null if such email address won't exist.</returns>
		public string MapUser(string emailAddress)
		{
			m_UpdSync.AddMethod();

			try{
				// See if user with specified email address exists
				foreach(DataRow dr in dsUserAddresses.Tables["UserAddresses"].Rows){
					// There is such user
					if(dr["Address"].ToString().ToLower() == emailAddress.ToLower()){
						string userID = dr["UserID"].ToString();
						
						// Find user LoginName for specified UserID
						foreach(DataRow drUser in dsUsers.Tables["Users"].Rows){
							if(userID == drUser["UserID"].ToString()){
								return drUser["UserName"].ToString();
							}
						}
					}
				}
				
				return null;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}
		
		#endregion

		#region method ValidateMailboxSize

		/// <summary>
		/// Checks if specified mailbox size is exceeded.
		/// </summary>
		/// <param name="userName">User name. Use <see cref="IMailServerApi.GetUsers">GetUsers()</see> to get valid values.</param>
		/// <returns>Returns true if exceeded.</returns>
		public bool ValidateMailboxSize(string userName)
		{	
			m_UpdSync.AddMethod();

			try{
				foreach(DataRow dr in dsUsers.Tables["Users"].Rows){
					if(dr["UserName"].ToString().ToLower() == userName.ToLower()){
                        long maxAllowedSize = Convert.ToInt64(dr["Mailbox_Size"]);
                        // Size not limited
                        if(maxAllowedSize < 1){
                            return false;
                        }

						long mailboxSize = GetMailboxSize(userName);					
						if(mailboxSize > (maxAllowedSize * 1000000)){
							return true;
						}
					}
				}

				return false;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}
		
		#endregion

        #region method GetUserPermissions

        /// <summary>
        /// Gets specified user permissions.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns></returns>
        public UserPermissions_enum GetUserPermissions(string userName)
        {
            foreach(DataRow dr in dsUsers.Tables["Users"].Rows){
					if(dr["UserName"].ToString().ToLower() == userName.ToLower()){
						return (UserPermissions_enum)Convert.ToInt32(dr["Permissions"]);
					}
				}

            return UserPermissions_enum.None;
        }

        #endregion

        #region method GetUserLastLoginTime

        /// <summary>
        /// Gets user last login time.
        /// </summary>
        /// <param name="userName">User name who's last login time to get.</param>
        /// <returns>User last login time.</returns>
        public DateTime GetUserLastLoginTime(string userName)
        {
            string folderPath = API_Utlis.DirectoryExists(m_MailStorePath + "Mailboxes/" + userName);
            if(folderPath != null){
                using(FileStream fs = OpenOrCreateFile(folderPath + "/_LastLogin.txt",10000)){
                    // File just created
                    if(fs.Length == 0){
                        DateTime lastLoginTime = DateTime.Now;
                        byte[] data = System.Text.Encoding.Default.GetBytes(lastLoginTime.ToString("yyyyMMdd HH:mm:ss"));
                        fs.Write(data,0,data.Length);
                        return lastLoginTime;
                    }
                    // Load last login time from file
                    else{
                        byte[] data = new byte[fs.Length];
                        fs.Read(data,0,data.Length);
                        return DateTime.ParseExact(System.Text.Encoding.Default.GetString(data),"yyyyMMdd HH:mm:ss",System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    }            
                }
			}
            else{
                if(!UserExists(userName)){
                    throw new Exception("User '" + userName + "' doesn't exist !");
                }
                else{
                    return DateTime.MinValue;
                }
            }
        }

        #endregion

        #region method UpdateUserLastLoginTime

        /// <summary>
        /// Updates user last login time.
        /// </summary>
        /// <param name="userName">User name who's last login time to update.</param>
        public void UpdateUserLastLoginTime(string userName)
        {
            if(!UserExists(userName)){
                throw new Exception("User '" + userName + "' doesn't exist !");
            }

            string folderPath = API_Utlis.EnsureFolder(m_MailStorePath + "Mailboxes/" + userName);
            using(FileStream fs = OpenOrCreateFile(folderPath + "/_LastLogin.txt",10000)){
                // Delete file data
                fs.SetLength(0);

                byte[] data = System.Text.Encoding.Default.GetBytes(DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                fs.Write(data,0,data.Length);             
            }
        }

        #endregion


        #region method GetUserRemoteServers

		/// <summary>
		/// Gets user pop3 remote accounts.
		/// </summary>
		/// <param name="userName">User name. Use <see cref="IMailServerApi.GetUsers">GetUsers()</see> to get valid values.</param>
		/// <returns></returns>
		public DataView GetUserRemoteServers(string userName)
		{
			m_UpdSync.AddMethod();

			try{
				string userID = "";

				// Find user LoginName for specified UserID
				foreach(DataRow drUser in dsUsers.Tables["Users"].Rows){
					if(userName.ToLower() == drUser["UserName"].ToString().ToLower()){
						userID =  drUser["UserID"].ToString();
						break;
					}
				}

				DataView dv = new DataView(dsUserRemoteServers.Copy().Tables["UserRemoteServers"]);
				if(userID.Length > 0){
					dv.RowFilter = "UserID='" + userID + "'";
				}
					
				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}

		#endregion

		#region method AddUserRemoteServer

		/// <summary>
		/// Adds new remote pop3 server to user.
		/// </summary>
		/// <param name="serverID">Server ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="userName">User name. Use <see cref="IMailServerApi.GetUsers">GetUsers()</see> to get valid values.</param>
		/// <param name="description">Remote server description.</param>
		/// <param name="remoteServer">Remote server name.</param>
		/// <param name="remotePort">Remote server port.</param>
		/// <param name="remoteUser">Remote server user name.</param>
		/// <param name="remotePassword">Remote server password.</param>
		/// <param name="useSSL">Specifies if SSL must be used to connect to remote server.</param>
		/// <param name="enabled">Specifies if remote server is enabled.</param>
		/// <remarks>Throws exception if specified user remote server already exists.</remarks>
		public void AddUserRemoteServer(string serverID,string userName,string description,string remoteServer,int remotePort,string remoteUser,string remotePassword,bool useSSL,bool enabled)
		{			
			if(serverID.Length == 0){
				throw new Exception("You must specify serverID");
			}
			if(userName.Length == 0){
				throw new Exception("You must specify userName");
			}

			m_UpdSync.BeginUpdate();

			try{
				if(UserExists(userName)){
					if(!ContainsID(dsUserRemoteServers.Tables["UserRemoteServers"],"ServerID",serverID)){
						// Get user ID from name
						string userID = "";
						using(DataView dv = new DataView(dsUsers.Tables["Users"])){
							dv.RowFilter = "UserName='" + userName + "'";
					
							userID = dv[0]["UserID"].ToString();
						}

						DataRow dr = dsUserRemoteServers.Tables["UserRemoteServers"].NewRow();
						dr["ServerID"]       = serverID;
						dr["UserID"]         = userID;
                        dr["Description"]    = description;
						dr["RemoteServer"]   = remoteServer;
						dr["RemotePort"]     = remotePort;
						dr["RemoteUserName"] = remoteUser;
						dr["RemotePassword"] = remotePassword;
                        dr["UseSSL"]         = useSSL;
                        dr["Enabled"]        = enabled;
										
						dsUserRemoteServers.Tables["UserRemoteServers"].Rows.Add(dr);
						dsUserRemoteServers.WriteXml(m_DataPath + "UserRemoteServers.xml",XmlWriteMode.IgnoreSchema);
					}
					else{
						throw new Exception("Address with specified ID '" + serverID + "' already exists !");
					}
				}
				else{
					throw new Exception("User '" + userName + "' doesn't exist !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}

		#endregion

		#region method DeleteUserRemoteServer

		/// <summary>
		/// Deletes specified pop3 remote account from user.
		/// </summary>
		/// <param name="serverID">Remote server ID. Use <see cref="IMailServerApi.GetUserRemoteServers">GetUserRemoteServers()</see> to get valid values.</param>
		public void DeleteUserRemoteServer(string serverID)
		{
			m_UpdSync.BeginUpdate();

			try{			
				using(DataView dv = new DataView(dsUserRemoteServers.Tables["UserRemoteServers"])){
					dv.RowFilter = "ServerID='" + serverID + "'";

					if(dv.Count > 0){
						dv[0].Delete();
					}

					dsUserRemoteServers.WriteXml(m_DataPath + "UserRemoteServers.xml",XmlWriteMode.IgnoreSchema);
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}

		#endregion

        #region mehtod UpdateUserRemoteServer

        /// <summary>
		/// Updates user remote pop3 server.
		/// </summary>
		/// <param name="serverID">Server ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="userName">User name. Use <see cref="IMailServerApi.GetUsers">GetUsers()</see> to get valid values.</param>
		/// <param name="description">Remote server description.</param>
		/// <param name="remoteServer">Remote server name.</param>
		/// <param name="remotePort">Remote server port.</param>
		/// <param name="remoteUser">Remote server user name.</param>
		/// <param name="remotePassword">Remote server password.</param>
		/// <param name="useSSL">Specifies if SSL must be used to connect to remote server.</param>
		/// <param name="enabled">Specifies if remote server is enabled.</param>
		/// <remarks>Throws exception if specified user remote server already exists.</remarks>
		public void UpdateUserRemoteServer(string serverID,string userName,string description,string remoteServer,int remotePort,string remoteUser,string remotePassword,bool useSSL,bool enabled)
        {
            if(serverID.Length == 0){
				throw new Exception("You must specify serverID");
			}
			if(userName.Length == 0){
				throw new Exception("You must specify userName");
			}

			m_UpdSync.BeginUpdate();

			try{
				if(UserExists(userName)){
                    if(!ContainsID(dsUserRemoteServers.Tables["UserRemoteServers"],"ServerID",serverID)){
                        throw new Exception("Address with specified ID '" + serverID + "' doesn't exist !");
                    }
					
                    foreach(DataRow dr in dsUserRemoteServers.Tables["UserRemoteServers"].Rows){
                        if(dr["ServerID"].ToString().ToLower() == serverID){
					    //  dr["ServerID"]       = serverID;
					    //  dr["UserID"]         = userID;
                            dr["Description"]    = description;
						    dr["RemoteServer"]   = remoteServer;
						    dr["RemotePort"]     = remotePort;
						    dr["RemoteUserName"] = remoteUser;
						    dr["RemotePassword"] = remotePassword;
                            dr["UseSSL"]         = useSSL;
                            dr["Enabled"]        = enabled;
    										
						    dsUserRemoteServers.WriteXml(m_DataPath + "UserRemoteServers.xml",XmlWriteMode.IgnoreSchema);
                            break;
                        }
					}
				}
				else{
					throw new Exception("User '" + userName + "' doesn't exist !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion
        		

		#region method GetUserMessageRules

		/// <summary>
		/// Gets user message  rules.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <returns></returns>
		public DataView GetUserMessageRules(string userName)
		{
			m_UpdSync.AddMethod();

			try{
				string userID = "";

				// Find user LoginName for specified UserID
				foreach(DataRow drUser in dsUsers.Tables["Users"].Rows){
					if(userName.ToLower() == drUser["UserName"].ToString().ToLower()){
						userID =  drUser["UserID"].ToString();
						break;
					}
				}

				DataView dv = new DataView(dsUserMessageRules.Copy().Tables["UserMessageRules"]);
				if(userName.Length > 0){
					dv.RowFilter = "UserID='" + userID + "'";
				}
                dv.Sort = "Cost ASC";
					
				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}

		#endregion

		#region method AddUserMessageRule

		/// <summary>
        /// Adds new user message rule.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID. Guid.NewID().ToString() is suggested.</param>
        /// <param name="cost">Cost specifies in what order rules are processed. Costs with lower values are processed first.</param>
        /// <param name="enabled">Specifies if rule is enabled.</param>
        /// <param name="checkNextRule">Specifies when next rule is checked.</param>
        /// <param name="description">Rule description.</param>
        /// <param name="matchExpression">Rule match expression.</param>
        public void AddUserMessageRule(string userID,string ruleID,long cost,bool enabled,GlobalMessageRule_CheckNextRule_enum checkNextRule,string description,string matchExpression)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            m_UpdSync.BeginUpdate();

			try{
                // TODO: check match expression

                // Check that specified user exists
                if(!ContainsID(dsUsers.Tables["Users"],"UserID",userID)){
                    throw new Exception("User with specified id '" + userID + "' doesn't exist !");
                }

				if(!ContainsID(dsUserMessageRules.Tables["UserMessageRules"],"RuleID",ruleID)){
					DataRow dr = dsUserMessageRules.Tables["UserMessageRules"].NewRow();
                    dr["UserID"]          = userID;
					dr["RuleID"]          = ruleID;
					dr["Cost"]            = cost;
					dr["Enabled"]         = enabled;
					dr["CheckNextRuleIf"] = (int)checkNextRule;
					dr["Description"]     = description;
                    dr["MatchExpression"] = matchExpression;
							
					dsUserMessageRules.Tables["UserMessageRules"].Rows.Add(dr);
					dsUserMessageRules.WriteXml(m_DataPath + "UserMessageRules.xml",XmlWriteMode.IgnoreSchema);
				}
				else{
					throw new Exception("Specified ruleID '" + ruleID + "' already exists, choose another ruleID !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

		#endregion

		#region method DeleteUserMessageRule

		/// <summary>
		/// Deletes specified user message rule.
		/// </summary>
        /// <param name="userID">User who owns specified rule.</param>
		/// <param name="ruleID">Rule ID.</param>
		public void DeleteUserMessageRule(string userID,string ruleID)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            m_UpdSync.BeginUpdate();

			try{
                // Check that specified user exists
                if(!ContainsID(dsUsers.Tables["Users"],"UserID",userID)){
                    throw new Exception("User with specified id '" + userID + "' doesn't exist !");
                }

                // Check that specified rule exists
                if(!ContainsID(dsUserMessageRules.Tables["UserMessageRules"],"RuleID",ruleID)){
                    throw new Exception("Invalid ruleID '" + ruleID + "', specified ruleID doesn't exist !");
                }

                // Delete specified rule actions
                foreach(DataRowView drV in GetUserMessageRuleActions(userID,ruleID)){
                    DeleteUserMessageRuleAction(userID,ruleID,drV["ActionID"].ToString());
                }                                

                // Delete specified rule
                foreach(DataRow dr in dsUserMessageRules.Tables["UserMessageRules"].Rows){
                    if(dr["RuleID"].ToString().ToLower() == ruleID){
                        dr.Delete();
                        dsUserMessageRules.WriteXml(m_DataPath + "UserMessageRules.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

		#endregion

		#region method UpdateUserMessageRule

		/// <summary>
        /// Updates specified user message rule.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID.</param>
        /// <param name="cost">Cost specifies in what order rules are processed. Costs with lower values are processed first.</param>
        /// <param name="enabled">Specifies if rule is enabled.</param>
        /// <param name="checkNextRule">Specifies when next rule is checked.</param>
        /// <param name="description">Rule description.</param>
        /// <param name="matchExpression">Rule match expression.</param>
        public void UpdateUserMessageRule(string userID,string ruleID,long cost,bool enabled,GlobalMessageRule_CheckNextRule_enum checkNextRule,string description,string matchExpression)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            m_UpdSync.BeginUpdate();

			try{
                // TODO: check match expression

                // Check that specified user exists
                if(!ContainsID(dsUsers.Tables["Users"],"UserID",userID)){
                    throw new Exception("User with specified id '" + userID + "' doesn't exist !");
                }

                // Check that specified rule exists
                if(!ContainsID(dsUserMessageRules.Tables["UserMessageRules"],"RuleID",ruleID)){
                    throw new Exception("Invalid ruleID '" + ruleID + "', specified ruleID doesn't exist !");
                }

                foreach(DataRow dr in dsUserMessageRules.Tables["UserMessageRules"].Rows){
                    if(dr["RuleID"].ToString().ToLower() == ruleID){
                        dr["UserID"]          = userID;
                        dr["RuleID"]          = ruleID;
					    dr["Cost"]            = cost;
					    dr["Enabled"]         = enabled;
					    dr["CheckNextRuleIf"] = (int)checkNextRule;
					    dr["Description"]     = description;
                        dr["MatchExpression"] = matchExpression;

                        dsUserMessageRules.WriteXml(m_DataPath + "UserMessageRules.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

		#endregion

        #region method GetUserMessageRuleActions

        /// <summary>
        /// Gets specified user message rule actions.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID of rule which actions to get.</param>
        public DataView GetUserMessageRuleActions(string userID,string ruleID)
        {
            m_UpdSync.AddMethod();

			try{
				DataView dv = new DataView(dsUserMessageRuleActions.Copy().Tables["UserMessageRuleActions"]);
				if(userID.Length > 0){
					dv.RowFilter = "UserID='" + userID + "' AND RuleID='" + ruleID + "'";
				}
					
				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
        }

        #endregion

        #region method AddUserMessageRuleAction

        /// <summary>
        /// Adds action to specified user message rule.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID to which to add this action.</param>
        /// <param name="actionID">Action ID. Guid.NewID().ToString() is suggested.</param>
        /// <param name="description">Action description.</param>
        /// <param name="actionType">Action type.</param>
        /// <param name="actionData">Action data. Data structure depends on action type.</param>
        public void AddUserMessageRuleAction(string userID,string ruleID,string actionID,string description,GlobalMessageRuleAction_enum actionType,byte[] actionData)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }
            
            m_UpdSync.BeginUpdate();

			try{
                // Check that specified user exists
                if(!ContainsID(dsUsers.Tables["Users"],"UserID",userID)){
                    throw new Exception("User with specified id '" + userID + "' doesn't exist !");
                }

                // Check that specified rule exists
                if(!ContainsID(dsUserMessageRules.Tables["UserMessageRules"],"RuleID",ruleID)){
                    throw new Exception("Invalid ruleID '" + ruleID + "', specified ruleID doesn't exist !");
                }

				if(!ContainsID(dsUserMessageRuleActions.Tables["UserMessageRuleActions"],"ActionID",ruleID)){
					DataRow dr = dsUserMessageRuleActions.Tables["UserMessageRuleActions"].NewRow();
                    dr["UserID"]      = userID;
					dr["RuleID"]      = ruleID;
					dr["ActionID"]    = actionID;
					dr["Description"] = description;
                    dr["ActionType"]  = actionType;
                    dr["ActionData"]  = actionData;
							
					dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Rows.Add(dr);
					dsUserMessageRuleActions.WriteXml(m_DataPath + "UserMessageRuleActions.xml",XmlWriteMode.IgnoreSchema);
				}
				else{
					throw new Exception("Specified actionID '" + actionID + "' already exists, choose another actionID !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method DeleteUserMessageRuleAction

        /// <summary>
        /// Deletes specified action from specified user message rule.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID which action to delete.</param>
        /// <param name="actionID">Action ID of action which to delete.</param>
        public void DeleteUserMessageRuleAction(string userID,string ruleID,string actionID)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }
                        
            m_UpdSync.BeginUpdate();

			try{
                // Check that specified user exists
                if(!ContainsID(dsUsers.Tables["Users"],"UserID",userID)){
                    throw new Exception("User with specified id '" + userID + "' doesn't exist !");
                }

                // Check that specified rule exists
                if(!ContainsID(dsUserMessageRules.Tables["UserMessageRules"],"RuleID",ruleID)){
                    throw new Exception("Invalid ruleID '" + ruleID + "', specified ruleID doesn't exist !");
                }

                // Delete specified action
                foreach(DataRow dr in dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Rows){
                    if(dr["RuleID"].ToString().ToLower() == ruleID && dr["ActionID"].ToString().ToLower() == actionID){
                        dr.Delete();
                        dsUserMessageRuleActions.WriteXml(m_DataPath + "UserMessageRuleActions.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method UpdateUserMessageRuleAction

        /// <summary>
        /// Updates specified rule action.
        /// </summary>
        /// <param name="userID">User who owns specified rule.</param>
        /// <param name="ruleID">Rule ID which action to update.</param>
        /// <param name="actionID">Action ID of action which to update.</param>
        /// <param name="description">Action description.</param>
        /// <param name="actionType">Action type.</param>
        /// <param name="actionData">Action data. Data structure depends on action type.</param>
        public void UpdateUserMessageRuleAction(string userID,string ruleID,string actionID,string description,GlobalMessageRuleAction_enum actionType,byte[] actionData)
        {
            if(userID == null || userID == ""){
                throw new Exception("Invalid userID value, userID can't be '' or null !");
            }
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }

            m_UpdSync.BeginUpdate();

			try{
                // Check that specified user exists
                if(!ContainsID(dsUsers.Tables["Users"],"UserID",userID)){
                    throw new Exception("User with specified id '" + userID + "' doesn't exist !");
                }

                // Check that specified rule exists
                if(!ContainsID(dsUserMessageRules.Tables["UserMessageRules"],"RuleID",ruleID)){
                    throw new Exception("Invalid ruleID '" + ruleID + "', specified ruleID doesn't exist !");
                }

                bool actionExists = false;
                foreach(DataRow dr in dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Rows){
                    if(dr["RuleID"].ToString().ToLower() == ruleID && dr["ActionID"].ToString().ToLower() == actionID){
                        dr["RuleID"]      = ruleID;
					    dr["ActionID"]    = actionID;
					    dr["Description"] = description;
                        dr["ActionType"]  = actionType;
                        dr["ActionData"]  = actionData;

                        dsUserMessageRuleActions.WriteXml(m_DataPath + "UserMessageRuleActions.xml",XmlWriteMode.IgnoreSchema);
                        actionExists = true;
                        break;
                    }
                }

                if(!actionExists){
                    throw new Exception("Invalid actionID '" + actionID + "', specified actionID doesn't exist !");
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion


		#region method AuthUser

		/// <summary>
		/// Authenticates user.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <param name="passwData">Password data.</param>
		/// <param name="authData">Authentication specific data(as tag).</param>
		/// <param name="authType">Authentication type.</param>
		/// <returns></returns>
		public DataSet AuthUser(string userName,string passwData,string authData,AuthType authType)
		{
			m_UpdSync.AddMethod();

			DataSet retVal = new DataSet();
			DataTable dt = retVal.Tables.Add("Result");
			dt.Columns.Add("Result");
			dt.Columns.Add("ReturnData");
			DataRow drx = dt.NewRow();
			drx["Result"] = "false";
			drx["ReturnData"] = "";
			dt.Rows.Add(drx);

			try{
				// See if user with specified name exists
				foreach(DataRow dr in dsUsers.Tables["Users"].Rows){					
					if(Convert.ToBoolean(dr["Enabled"]) && dr["USERNAME"].ToString().ToLower() == userName.ToLower()){
						string password = dr["PASSWORD"].ToString().ToLower();

						switch(authType)
						{
							case AuthType.APOP:
								if(AuthHelper.Apop(password,authData) == passwData){
									drx["Result"] = "true";
									return retVal;
								}
								break;

							case AuthType.CRAM_MD5:	
								if(AuthHelper.Cram_Md5(password,authData) == passwData){
									drx["Result"] = "true";
									return retVal;
								}
								break;

							case AuthType.DIGEST_MD5:	
								string realm      = "";
								string nonce      = "";
								string cnonce     = "";
								string digest_uri = "";
								foreach(string clntRespParam in authData.Split(',')){
									if(clntRespParam.StartsWith("realm=")){
										realm = clntRespParam.Split(new char[]{'='},2)[1].Replace("\"","");
									}
									else if(clntRespParam.StartsWith("nonce=")){
										nonce = clntRespParam.Split(new char[]{'='},2)[1].Replace("\"","");
									}									
									else if(clntRespParam.StartsWith("cnonce=")){
										cnonce = clntRespParam.Split(new char[]{'='},2)[1].Replace("\"","");
									}									
									else if(clntRespParam.StartsWith("digest-uri=")){
										digest_uri = clntRespParam.Split(new char[]{'='},2)[1].Replace("\"","");
									}
								}

								if(passwData == AuthHelper.Digest_Md5(true,realm,userName,password,nonce,cnonce,digest_uri)){
									string returnData = AuthHelper.Digest_Md5(false,realm,userName,password,nonce,cnonce,digest_uri);
									
									drx["Result"] = "true";
									drx["ReturnData"] = returnData;
									return retVal;
								}

								break;

							case AuthType.Plain:
								if(password == passwData.ToLower()){
									drx["Result"] = "true";
									return retVal;
								}
								break;
						}

						return retVal;
					}
				}

				return retVal;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}
		
		#endregion


        #region method GroupExists

        /// <summary>
        /// Gets if specified user group exists.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <returns>Returns true, if user group exists.</returns>
        public bool GroupExists(string groupName)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Check if group exists.
            */

            //--- Validate values --------------------//
            ArgsValidator.ValidateUserName(groupName);
            //----------------------------------------//


            return GetGroupID(groupName) != null;
        }

        #endregion

        #region method GetGroups

        /// <summary>
        /// Gets user groups.
        /// </summary>
        /// <returns></returns>
        public DataView GetGroups()
        {
            /* Implementation notes:
                *) Get groups.
            */

            m_UpdSync.AddMethod();

			try{
				DataView dv = new DataView(dsGroups.Copy().Tables["Groups"]);				
				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
        }

        #endregion

        #region method AddGroup

        /// <summary>
        /// Adds new user group.
        /// </summary>
        /// <param name="groupID">Group ID. Guid.NewGuid().ToString() is suggested.</param>
        /// <param name="groupName">Group name.</param>
        /// <param name="description">Group description.</param>
        /// <param name="enabled">Specifies if group is enabled.</param>
        public void AddGroup(string groupID,string groupName,string description,bool enabled)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group ID won't exist already. Throw Exception if does.
                *) Ensure that group or user with specified name doesn't exist. Throw Exception if does.
                *) Add group.
            */

            //--- Validate values --------------------//
            if(groupID == null || groupID == ""){
                throw new Exception("Invalid groupID value, groupID can't be '' or null !");
            }
            ArgsValidator.ValidateUserName(groupName);
            ArgsValidator.ValidateNotNull(description);
            //----------------------------------------//

            m_UpdSync.BeginUpdate();

            try{                
                // Ensure that group ID won't exist already.
                if(ContainsID(dsGroups.Tables["Groups"],"GroupID",groupID)){
                    throw new Exception("Invalid group ID, specified group ID '" + groupID + "' already exists !");
                }

                // Ensure that group name won't exist already.
                if(GroupExists(groupName)){
                    throw new Exception("Invalid group name, specified group '" + groupName + "' already exists !");
                }
                // Ensure that user name with groupName doen't exist.
                else if(UserExists(groupName)){
                    throw new Exception("Invalid group name, user with specified name '" + groupName + "' already exists !");
                }

                DataRow dr = dsGroups.Tables["Groups"].NewRow();
				dr["GroupID"]     = groupID;
				dr["GroupName"]   = groupName;
				dr["Description"] = description;
				dr["Enabled"]     = enabled;
							
				dsGroups.Tables["Groups"].Rows.Add(dr);
			    dsGroups.WriteXml(m_DataPath + "Groups.xml",XmlWriteMode.IgnoreSchema);
            }
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method DeleteGroup

        /// <summary>
        /// Deletes specified user group.
        /// </summary>
        /// <param name="groupID">Group ID.</param>
        public void DeleteGroup(string groupID)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group does exist.  Throw Exception if doesn't.
                *) Delete group members.
                *) Delete group.
            */

            //--- Validate values --------------------//
            if(groupID == null || groupID == ""){
                throw new Exception("Invalid groupID value, groupID can't be '' or null !");
            }
            //----------------------------------------//

            m_UpdSync.BeginUpdate();

			try{
                // Ensure that group does exist.
                if(!ContainsID(dsGroups.Tables["Groups"],"GroupID",groupID)){
                    throw new Exception("Invalid group ID, specified group ID '" + groupID + "' doesn't exist !");
                }

                //--- Delete group members ---------------------------------------------------------//
                for(int i=0;i<dsGroupMembers.Tables["GroupMembers"].Rows.Count;i++){
                    DataRow dr = dsGroupMembers.Tables["GroupMembers"].Rows[i];
                    if(dr["GroupID"].ToString() == groupID){
                        dr.Delete();
                        i--;
                    }
                }
                dsGroupMembers.WriteXml(m_DataPath + "GroupMembers.xml",XmlWriteMode.IgnoreSchema);
                //----------------------------------------------------------------------------------//

                

                // Delete specified group
                foreach(DataRow dr in dsGroups.Tables["Groups"].Rows){
                    if(dr["GroupID"].ToString().ToLower() == groupID){
                        dr.Delete();
                        dsGroups.WriteXml(m_DataPath + "Groups.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method UpdateGroup

        /// <summary>
        /// Updates user group info.
        /// </summary>
        /// <param name="groupID">Group ID.</param>
        /// <param name="groupName">Group name.</param>
        /// <param name="description">Group description.</param>
        /// <param name="enabled">Specifies if group is enabled.</param>
        public void UpdateGroup(string groupID,string groupName,string description,bool enabled)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group with specified ID does exist.  Throw Exception if doesn't.
                *) If group name is changed, ensure that new group name won't conflict 
                   any other group or user name. Throw Exception if does. 
                *) Udpate group.
            */

            //--- Validate values --------------------//
            if(groupID == null || groupID == ""){
                throw new Exception("Invalid groupID value, groupID can't be '' or null !");
            }
            ArgsValidator.ValidateUserName(groupName);
            ArgsValidator.ValidateNotNull(description);
            //----------------------------------------//

            m_UpdSync.BeginUpdate();

            try{                
                // Ensure that group with specified ID does exist.
                if(!ContainsID(dsGroups.Tables["Groups"],"GroupID",groupID)){
                    throw new Exception("Invalid group ID, specified group ID '" + groupID + "' doesn't exist !");
                }
                                
                foreach(DataRow dr in dsGroups.Tables["Groups"].Rows){
                    if(dr["GroupID"].ToString().ToLower() == groupID){
                        // If group name is changed, ensure that new group name won't conflict 
                        //   any other group or user name. 
                        if(dr["GroupName"].ToString().ToLower() != groupName.ToLower()){
                            if(GroupExists(groupName)){
                                throw new Exception("Invalid group name, specified group '" + groupName + "' already exists !");
                            }
                            // Ensure that user name with groupName doen't exist.
                            else if(UserExists(groupName)){
                                throw new Exception("Invalid group name, user with specified name '" + groupName + "' already exists !");
                            }
                        }

                     //	dr["GroupID"]     = groupID;
				        dr["GroupName"]   = groupName;
				        dr["Description"] = description;
				        dr["Enabled"]     = enabled;

                        dsGroups.WriteXml(m_DataPath + "Groups.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
            }
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method GroupMemberExists

        /// <summary>
        /// Gets if specified group member exists in specified user group members list.
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="userOrGroup">User or group.</param>
        /// <returns></returns>
        public bool GroupMemberExists(string groupName,string userOrGroup)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group exists. Throw Exception if doesn't.
                *) Check if group member exists.
            */

            //--- Validate values --------------------//
            ArgsValidator.ValidateUserName(groupName);
            ArgsValidator.ValidateUserName(userOrGroup);
            //----------------------------------------//

            m_UpdSync.BeginUpdate();

            try{
                // Ensure that group exists. Throw Exception if doesn't.
                if(!GroupExists(groupName)){
                    throw new Exception("Invalid group name, specified group '" + groupName + "' doesn't exist !");
                }

                string groupID = GetGroupID(groupName);

                // Check if group member exists.
                foreach(DataRow dr in dsGroupMembers.Tables["GroupMembers"].Rows){
                    if(dr["GroupID"].ToString().ToLower() == groupID && dr["UserOrGroup"].ToString().ToLower() == userOrGroup.ToLower()){
                           return true;
                    }
                }
            }
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
            
            return false;
        }

        #endregion

        #region method GetGroupMembers

        /// <summary>
        /// Gets useer group members who belong to specified group.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <returns></returns>
        public string[] GetGroupMembers(string groupName)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group exists. Throw Exception if doesn't.
                *) Get members.
            */

            //--- Validate values --------------------//
            if(groupName == null || groupName == ""){
                throw new Exception("Invalid groupName value, groupName can't be '' or null !");
            }
            //----------------------------------------//

            m_UpdSync.AddMethod();

			try{
                // Ensure that group exists.
                if(!GroupExists(groupName)){
                    throw new Exception("Invalid group name, specified group name '" + groupName + "' doesn't exist !");
                }

                string groupID = GetGroupID(groupName);

				DataTable dt = dsGroupMembers.Tables["GroupMembers"];
                List<string> members = new List<string>();
                foreach(DataRow dr in dsGroupMembers.Tables["GroupMembers"].Rows){
                    if(dr["GroupID"].ToString() == groupID){
                        members.Add(dr["UserOrGroup"].ToString());
                    }
                }
				return members.ToArray();
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
        }

        #endregion

        #region method AddGroupMember

        /// <summary>
        /// Add specified user or group to specified goup members list.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <param name="userOrGroup">User or group.</param>
        public void AddGroupMember(string groupName,string userOrGroup)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group exists. Throw Exception if doesn't.
                *) Don't allow to add same group as group member.
                *) Ensure that group member doesn't exist. Throw Exception if does.
                *) Add group member.
            */

            //--- Validate values --------------------//
            ArgsValidator.ValidateUserName(groupName);
            ArgsValidator.ValidateUserName(userOrGroup);
            //----------------------------------------//

            m_UpdSync.BeginUpdate();

            try{
                // Ensure that group exists. Throw Exception if doesn't.
                if(!GroupExists(groupName)){
                    throw new Exception("Invalid group name, specified group '" + groupName + "' doesn't exist !");
                }

                // Don't allow to add same grou as group member.
                if(groupName.ToLower() == userOrGroup.ToLower()){
                    throw new Exception("Invalid group member, can't add goup itself as same group member !");
                }

                // Ensure that group member doesn't exist. Throw Exception if does.
                if(GroupMemberExists(groupName,userOrGroup)){
                    throw new Exception("Invalid group member, specified group member '" + userOrGroup + "' already exists !");
                }

                string groupID = GetGroupID(groupName);

                DataRow dr = dsGroupMembers.Tables["GroupMembers"].NewRow();
				dr["GroupID"]     = groupID;
				dr["UserOrGroup"] = userOrGroup;
							
				dsGroupMembers.Tables["GroupMembers"].Rows.Add(dr);
			    dsGroupMembers.WriteXml(m_DataPath + "GroupMembers.xml",XmlWriteMode.IgnoreSchema);
            }
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method DeleteGroupMember

        /// <summary>
        /// Deletes specified user or group from specified group members list.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <param name="userOrGroup">User or group.</param>
        public void DeleteGroupMember(string groupName,string userOrGroup)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that group exists. Throw Exception if doesn't.
                *) Ensure that group member does exist. Throw Exception if doesn't.
                *) Delete group member.
            */

            //--- Validate values --------------------//
            ArgsValidator.ValidateUserName(groupName);
            ArgsValidator.ValidateUserName(userOrGroup);
            //----------------------------------------//

            m_UpdSync.BeginUpdate();

			try{
                // Ensure that group exists. Throw Exception if doesn't.
                if(!GroupExists(groupName)){
                    throw new Exception("Invalid group name, specified group '" + groupName + "' doesn't exist !");
                }

                // Ensure that group member doesn't exist. Throw Exception if does.
                if(!GroupMemberExists(groupName,userOrGroup)){
                    throw new Exception("Invalid group member, specified group member '" + groupName + "' already exists !");
                }

                string groupID = GetGroupID(groupName);

                // Delete specified root
                foreach(DataRow dr in dsGroupMembers.Tables["GroupMembers"].Rows){
                    if(dr["GroupID"].ToString().ToLower() == groupID && dr["UserOrGroup"].ToString().ToLower() == userOrGroup.ToLower()){
                        dr.Delete();
                        dsGroupMembers.WriteXml(m_DataPath + "GroupMembers.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method GetGroupUsers

        /// <summary>
        /// Gets specified group users. All nested group members are replaced by actual users.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <returns></returns>
        public string[] GetGroupUsers(string groupName)
        {
            List<string> users = new List<string>();
            List<string> proccessedGroups = new List<string>();
            Queue<string> membersQueue = new Queue<string>();
            string[] members = GetGroupMembers(groupName);
            foreach(string member in members){
                membersQueue.Enqueue(member);
            }

            while(membersQueue.Count > 0){
                string member = membersQueue.Dequeue();
                // Nested group
                DataRow drGroup = GetGroup(member);
                if(drGroup != null){
                    // Don't proccess poroccessed groups any more, causes infinite loop
                    if(!proccessedGroups.Contains(member.ToLower())){
                        // Skip disabled groups
                        if(Convert.ToBoolean(drGroup["Enabled"])){
                            members = GetGroupMembers(member);
                            foreach(string m in members){
                                membersQueue.Enqueue(m);
                            }  
                        }

                        proccessedGroups.Add(member.ToLower());
                    }
                }
                // User
                else{
                    if(!users.Contains(member)){
                        users.Add(member);
                    }
                }
            }

            return users.ToArray();
        }

        #endregion

        #region method GetGroupID

        /// <summary>
        /// Gets specified group ID from group name. Returns null, if specified group doesn't exist.
        /// </summary>
        /// <param name="groupName">Group name which ID to get.</param>
        /// <returns>Returns null, if specified group doesn't exist.</returns>
        public string GetGroupID(string groupName)
        {
            foreach(DataRow dr in dsGroups.Tables["Groups"].Rows){
                if(dr["GroupName"].ToString().ToLower() == groupName.ToLower()){
                    return dr["GroupID"].ToString();
                }
            }                
			
            return null;
        }

        #endregion

        #endregion


        #region MailingList related

        #region function GetMailingLists

        /// <summary>
		/// Gets mailing lists.
		/// </summary>
		/// <param name="domainName">Domain name. Use <see cref="IMailServerApi.GetDomains">GetDomains()</see> to get valid values.</param>
		/// <returns></returns>
		public DataView GetMailingLists(string domainName)
		{	
			m_UpdSync.AddMethod();

			try{
				DataView dv = new DataView(dsMailingLists.Copy().Tables["MailingLists"]);
				if(domainName != "ALL"){
					dv.RowFilter = "DomainName='" + domainName + "'";
				}

				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}
		
		#endregion
		

		#region function AddMailingList

		/// <summary>
		/// Adds new mailing list.
		/// </summary>
		/// <param name="mailingListID">Mailing list ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="mailingListName">Mailing list name name. Eg. all@lumisoft.ee .</param>
		/// <param name="description">Mailing list description.</param>
		/// <param name="domainName">Domain name. Use <see cref="IMailServerApi.GetDomains">GetDomains()</see> to get valid values.</param>
		/// <param name="enabled">Specifies if mailing list is enabled.</param>
		/// <remarks>Throws exception if specified mailing list already exists.</remarks>
		public void AddMailingList(string mailingListID,string mailingListName,string description,string domainName,bool enabled)
		{
			if(mailingListID.Length == 0){
				throw new Exception("You must specify mailingListID");
			}
			if(mailingListName.Length == 0){
				throw new Exception("You must specify mailingListName");
			}

			m_UpdSync.BeginUpdate();

			try{
				if(!MailingListExists(mailingListName)){
					if(!ContainsID(dsMailingLists.Tables["MailingLists"],"MailingListID",mailingListID)){
						DataRow dr = dsMailingLists.Tables["MailingLists"].NewRow();
						dr["MailingListID"]   = mailingListID;
						dr["MailingListName"] = mailingListName;
						dr["Description"]     = description;
						dr["DomainName"]      = domainName;
						dr["Enabled"]         = enabled;

						dsMailingLists.Tables["MailingLists"].Rows.Add(dr);
						dsMailingLists.WriteXml(m_DataPath + "MailingLists.xml",XmlWriteMode.IgnoreSchema);
					}
					else{
						throw new Exception("Mailing list with specified ID '" + mailingListID + "' already exists !");
					}
				}
				else{
					throw new Exception("Mailing list '" + mailingListName + "' already exists !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region function DeleteMailingList

		/// <summary>
		/// Deletes specified mailing list.
		/// </summary>
		/// <param name="mailingListID"> Use <see cref="IMailServerApi.GetMailingLists">GetMailingLists()</see> to get valid values.</param>
		/// <returns></returns>
		public void DeleteMailingList(string mailingListID)
		{
			m_UpdSync.BeginUpdate();

			try{
				// 1) delete mailing list addresses
				// 2) delete mailing list

				string mailingListName = "";
				//--- Find mailing list name from ID
				using(DataView dv = new DataView(dsMailingLists.Tables["MailingLists"])){
					dv.RowFilter = "MailingListID='" + mailingListID + "'";

					if(dv.Count > 0){
						mailingListName = dv[0]["MailingListName"].ToString();
					}
					else{
						throw new Exception("Mailing list with specified ID '" + mailingListID + "' doesn't exist !");
					}
				}

				// delete mailing list addresses
				using(DataView dv = GetMailingListAddresses(mailingListName)){
					foreach(DataRowView drV in dv){
						DeleteMailingListAddress(drV["AddressID"].ToString());
					}
				}

				// delete mailing list
				using(DataView dv = new DataView(dsMailingLists.Tables["MailingLists"])){
					dv.RowFilter = "MailingListID='" + mailingListID + "'";

					if(dv.Count > 0){
						dv[0].Delete();						
					}

					dsMailingLists.WriteXml(m_DataPath + "MailingLists.xml",XmlWriteMode.IgnoreSchema);
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region function UpdateMailingList

		/// <summary>
		/// Updates specified mailing list.
		/// </summary>
		/// <param name="mailingListID">Mailing list ID.</param>
		/// <param name="mailingListName">Mailing list name name. Use <see cref="IMailServerApi.GetMailingLists">GetMailingLists()</see> to get valid values.</param>
		/// <param name="description">Mailing list description.</param>
		/// <param name="domainName">Domain name. Use <see cref="IMailServerApi.GetDomains">>GetUsers()</see> to get valid values.</param>
		/// <param name="enabled">Specifies if mailing list is enabled.</param>
		public void UpdateMailingList(string mailingListID,string mailingListName,string description,string domainName,bool enabled)
		{
			if(mailingListName.Length == 0){
				throw new Exception("You must specify mailingListName");
			}

			m_UpdSync.BeginUpdate();

			try{
				if(ContainsID(dsMailingLists.Tables["MailingLists"],"MailingListID",mailingListID)){
					using(DataView dv = new DataView(dsMailingLists.Tables["MailingLists"])){
						dv.RowFilter = "MailingListID='" + mailingListID + "'";

						//--- see if mailing list with specified name doesn't exist by other mailing list
						using(DataView dvX = new DataView(dsMailingLists.Tables["MailingLists"])){
							dvX.RowFilter = "MailingListName='" + mailingListName + "'";

							if(dvX.Count > 0){
								// see if same user updated
								if(dvX[0]["MailingListID"].ToString() != mailingListID){
									throw new Exception("Mailing list '" + mailingListName + "' already exists !");
								}
							}
						}

						if(dv.Count > 0){
							dv[0]["MailingListName"] = mailingListName;
							dv[0]["Description"]     = description;
							dv[0]["DomainName"]      = domainName;
							dv[0]["Enabled"]         = enabled;

							dsMailingLists.WriteXml(m_DataPath + "MailingLists.xml",XmlWriteMode.IgnoreSchema);
						}
					}
				}
				else{
					throw new Exception("Mailing list with specified ID '" + mailingListID + "' doesn't exist !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region function AddMailingListAddress

		/// <summary>
		/// Add new email address to specified mailing list.
		/// </summary>
		/// <param name="addressID">Address ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="mailingListName">Mailing list name name. Use <see cref="IMailServerApi.GetMailingLists">GetMailingLists()</see> to get valid values.</param>
		/// <param name="address">Mailing list member address.</param>
		/// <remarks>Throws exception if specified mailing list member already exists.</remarks>
		public void AddMailingListAddress(string addressID,string mailingListName,string address)
		{			
			if(addressID.Length == 0){
				throw new Exception("You must specify addressID");
			}
			if(mailingListName.Length == 0){
				throw new Exception("You must specify mailingListName");
			}
			if(address.Length == 0){
				throw new Exception("You must specify address");
			}

			m_UpdSync.BeginUpdate();

			try{
				if(MailingListExists(mailingListName)){
					if(!ContainsID(dsMailingListAddresses.Tables["MailingListAddresses"],"AddressID",addressID)){
						using(DataView dv = GetMailingListAddresses(mailingListName)){
							dv.RowFilter += " AND Address='" + address + "'";
					
							if(dv.Count > 0){
								throw new Exception("Mailing list address '" + address + "' already exists !");
							}
						}

						// Get mailing list ID from name
						string mailingListID = "";
						using(DataView dv = new DataView(dsMailingLists.Tables["MailingLists"])){
							dv.RowFilter = "MailingListName='" + mailingListName + "'";
					
							mailingListID = dv[0]["MailingListID"].ToString();
						}

						DataRow dr = dsMailingListAddresses.Tables["MailingListAddresses"].NewRow();
						dr["AddressID"]     = addressID;
						dr["MailingListID"] = mailingListID;
						dr["Address"]       = address;
									
						dsMailingListAddresses.Tables["MailingListAddresses"].Rows.Add(dr);
						dsMailingListAddresses.WriteXml(m_DataPath + "MailingListAddresses.xml",XmlWriteMode.IgnoreSchema);
					}
					else{
						throw new Exception("Address with specified ID '" + addressID + "' already exists !");
					}
				}
				else{
					throw new Exception("Mailing list doesn't '" + address + "' exist !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}

		#endregion

		#region function DeleteMailingListAddress

		/// <summary>
		/// Deletes specified email address from mailing list. 
		/// </summary>
		/// <param name="addressID">Mailing list member address ID. Use <see cref="IMailServerApi.GetMailingListAddresses">GetMailingListMembers()</see> to get valid values.</param>
		public void DeleteMailingListAddress(string addressID)
		{
			m_UpdSync.BeginUpdate();

			try{
				using(DataView dv = new DataView(dsMailingListAddresses.Tables["MailingListAddresses"])){
					dv.RowFilter = "AddressID='" + addressID + "'";

					if(dv.Count > 0){
						dv[0].Delete();
					}

					dsMailingListAddresses.WriteXml(m_DataPath + "MailingListAddresses.xml",XmlWriteMode.IgnoreSchema);
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}

		#endregion

		#region function GetMailingListAddresses

		/// <summary>
		/// Gets mailing list members.
		/// </summary>
		/// <param name="mailingListName">Mailing list name name. Use <see cref="IMailServerApi.GetMailingLists">GetMailingLists()</see> to get valid values.</param>
		public DataView GetMailingListAddresses(string mailingListName)
		{
			m_UpdSync.AddMethod();

			try{
				// Get mailing list ID from name
				string mailingListID = "";
				using(DataView dvM = new DataView(dsMailingLists.Tables["MailingLists"])){
					dvM.RowFilter = "MailingListName='" + mailingListName + "'";
						
					if(dvM.Count > 0){
						mailingListID = dvM[0]["MailingListID"].ToString();
					}
				}

				DataView dv = new DataView(dsMailingListAddresses.Copy().Tables["MailingListAddresses"]);
				dv.RowFilter = "MailingListID='" + mailingListID + "'";
					
				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}

		#endregion


        #region method GetMailingListACL

        /// <summary>
        /// Gets mailing list ACL list.
        /// </summary>
        /// <param name="mailingListName">Mailing list name.</param>
        public DataView GetMailingListACL(string mailingListName)
        {
            /* Implementation notes:
                *) Ensure that mailing list exists.
                *) Get ACL.
            */

            m_UpdSync.AddMethod();

			try{
                // Ensure that mailing list exists
                DataRow drMailingList = GetMailingList(mailingListName);
                if(drMailingList == null){
                    throw new Exception("Invalid mailing list name, specified mailing list '" + mailingListName + "' doesn't exist !");
                }

                // Get mailing list ID
                string mailingListID = drMailingList["MailingListID"].ToString();
                
				DataView dv = new DataView(dsMailingListACL.Copy().Tables["ACL"]);				
                dv.RowFilter = "MailingListID = '" + mailingListID + "'";
				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
        }

        #endregion

        #region method AddMailingListACL

        /// <summary>
        /// Adds specified user or group to mailing list ACL list (specified user can send messages to the specified mailing list).
        /// </summary>
        /// <param name="mailingListName">Mailing list name.</param>
        /// <param name="userOrGroup">User or group name.</param>
        public void AddMailingListACL(string mailingListName,string userOrGroup)
        {
            /* Implementation notes:
                *) Ensure that mailing list exists.
                *) Ensure that user or group already doesn't exist in list.
                *) Add ACL entry.
            */

            m_UpdSync.AddMethod();

            try{
                // Ensure that mailing list exists
                DataRow drMailingList = GetMailingList(mailingListName);
                if(drMailingList == null){
                    throw new Exception("Invalid mailing list name, specified mailing list '" + mailingListName + "' doesn't exist !");
                }

                // Get mailing list ID
                string mailingListID = drMailingList["MailingListID"].ToString();

                // Ensure that user or group already doesn't exist in list
                foreach(DataRow drX in dsMailingListACL.Tables["ACL"].Rows){
                    if(drX["MailingListID"].ToString() == mailingListID && drX["UserOrGroup"].ToString().ToLower() == userOrGroup.ToLower()){
                        throw new Exception("Invalid userOrGroup, specified userOrGroup '" + userOrGroup + "' already exists !");
                    }
                }

				// Add ACL entry
                DataRow dr = dsMailingListACL.Tables["ACL"].NewRow();
                dr["MailingListID"] = mailingListID;
                dr["UserOrGroup"]   = userOrGroup;

                dsMailingListACL.Tables["ACL"].Rows.Add(dr);
                dsMailingListACL.WriteXml(m_DataPath + "MailingListACL.xml",XmlWriteMode.IgnoreSchema);
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
        }

        #endregion

        #region method DeleteMailingListACL

        /// <summary>
        /// Deletes specified user or group from mailing list ACL list.
        /// </summary>
        /// <param name="mailingListName">Mailing list name.</param>
        /// <param name="userOrGroup">User or group name.</param>
        public void DeleteMailingListACL(string mailingListName,string userOrGroup)
        {
            /* Implementation notes:
                *) Ensure that mailing list exists.
                *) Delete ACL entry.
            */

            m_UpdSync.AddMethod();

            try{
                // Ensure that mailing list exists
                DataRow drMailingList = GetMailingList(mailingListName);
                if(drMailingList == null){
                    throw new Exception("Invalid mailing list name, specified mailing list '" + mailingListName + "' doesn't exist !");
                }

                // Get mailing list ID
                string mailingListID = drMailingList["MailingListID"].ToString();
              
				// Delete ACL entry
                foreach(DataRow dr in dsMailingListACL.Tables["ACL"].Rows){
                    if(dr["MailingListID"].ToString() == mailingListID && dr["UserOrGroup"].ToString().ToLower() == userOrGroup.ToLower()){
                        dr.Delete();
                        break;
                    }
                }
               
                dsMailingListACL.WriteXml(m_DataPath + "MailingListACL.xml",XmlWriteMode.IgnoreSchema);
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
        }

        #endregion

        #region method CanAccessMailingList

        /// <summary>
        /// Checks if specified user can access specified mailing list.
        /// There is one built-in user anyone, that represent all users (including anonymous).
        /// </summary>
        /// <param name="mailingListName">Mailing list name.</param>
        /// <param name="user">User name.</param>
        /// <returns></returns>
        public bool CanAccessMailingList(string mailingListName,string user)
        {
            /* Implementation notes:
                *) Ensure that mailing list exists.
                *) Check access.
            */

            m_UpdSync.AddMethod();

            try{
                // Ensure that mailing list exists
                DataRow drMailingList = GetMailingList(mailingListName);
                if(drMailingList == null){
                    throw new Exception("Invalid mailing list name, specified mailing list '" + mailingListName + "' doesn't exist !");
                }

                // Get mailing list ID
                string mailingListID = drMailingList["MailingListID"].ToString();

                foreach(DataRow dr in dsMailingListACL.Tables["ACL"].Rows){
                    if(dr["MailingListID"].ToString() == mailingListID){
                        // Built-in anyone
                        if(dr["UserOrGroup"].ToString().ToLower() == "anyone"){
                            return true;
                        }
                        // Built-in "authenticated users"
                        else if(dr["UserOrGroup"].ToString().ToLower() == "authenticated users"){
                            return UserExists(user);
                        }
                        // User or group
                        else{                            
                            if(GroupExists(dr["UserOrGroup"].ToString())){
                                return IsUserGroupMember(dr["UserOrGroup"].ToString(),user);
                            }
                            else{
                                return UserExists(user);
                            }
                        }
                    }
                }
            }
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}

            return false;
        }

        #endregion


		#region function MailingListExists

		/// <summary>
		/// Checks if user exists.
		/// </summary>
		/// <param name="mailingListName">Mailing list name.</param>
		/// <returns>Returns true if mailing list exists.</returns>
		public bool MailingListExists(string mailingListName)
		{
			m_UpdSync.AddMethod();

			try{
				foreach(DataRow dr in dsMailingLists.Tables["MailingLists"].Rows){
					if(dr["MailingListName"].ToString().ToLower() == mailingListName.ToLower()){
						return true;
					}
				}

			/*	using(DataView dv = new DataView(dsMailingLists.Tables["MailingLists"])){
					dv.RowFilter = "MailingListName='" + mailingListName + "'";

					if(dv.Count > 0){
						return true;
					}
				}*/

				return false;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}
		
		#endregion

		#endregion


        #region Rules

        #region method GetGlobalMessageRules

        /// <summary>
        /// Gets global message rules.
        /// </summary>
        /// <returns></returns>
        public DataView GetGlobalMessageRules()
        {
            m_UpdSync.AddMethod();

			try{
				DataView dv = new DataView(dsRules.Copy().Tables["GlobalMessageRules"]);
                dv.Sort = "Cost ASC";

				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
        }

        #endregion

        #region method AddGlobalMessageRule

        /// <summary>
        /// Adds new global message rule.
        /// </summary>
        /// <param name="ruleID">Rule ID. Guid.NewID().ToString() is suggested.</param>
        /// <param name="cost">Cost specifies in what order rules are processed. Costs with lower values are processed first.</param>
        /// <param name="enabled">Specifies if rule is enabled.</param>
        /// <param name="checkNextRule">Specifies when next rule is checked.</param>
        /// <param name="description">Rule description.</param>
        /// <param name="matchExpression">Rule match expression.</param>
        public void AddGlobalMessageRule(string ruleID,long cost,bool enabled,GlobalMessageRule_CheckNextRule_enum checkNextRule,string description,string matchExpression)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            m_UpdSync.BeginUpdate();

			try{
                // TODO: check match expression

				if(!GlobalMessageRuleExists(ruleID)){
					DataRow dr = dsRules.Tables["GlobalMessageRules"].NewRow();
					dr["RuleID"]          = ruleID;
					dr["Cost"]            = cost;
					dr["Enabled"]         = enabled;
					dr["CheckNextRuleIf"] = (int)checkNextRule;
					dr["Description"]     = description;
                    dr["MatchExpression"] = matchExpression;
							
					dsRules.Tables["GlobalMessageRules"].Rows.Add(dr);
					dsRules.WriteXml(m_DataPath + "GlobalMessageRules.xml",XmlWriteMode.IgnoreSchema);
				}
				else{
					throw new Exception("Specified ruleID '" + ruleID + "' already exists, choose another ruleID !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }
        
        #endregion

        #region method DeleteGlobalMessageRule

        /// <summary>
        /// Deletes specified global message rule.
        /// </summary>
        /// <param name="ruleID">Rule ID of rule which to delete.</param>
        public void DeleteGlobalMessageRule(string ruleID)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            m_UpdSync.BeginUpdate();

			try{
                // Check that specified rule exists
                if(!GlobalMessageRuleExists(ruleID)){
                    throw new Exception("Invalid ruleID '" + ruleID + "', specified ruleID doesn't exist !");
                }

                // Delete specified rule actions
                foreach(DataRowView drV in GetGlobalMessageRuleActions(ruleID)){
                    DeleteGlobalMessageRuleAction(ruleID,drV["ActionID"].ToString());
                }                                

                // Delete specified rule
                foreach(DataRow dr in dsRules.Tables["GlobalMessageRules"].Rows){
                    if(dr["RuleID"].ToString().ToLower() == ruleID){
                        dr.Delete();
                        dsRules.WriteXml(m_DataPath + "GlobalMessageRules.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }
       
        #endregion

        #region method UpdateGlobalMessageRule

        /// <summary>
        /// Updates specified global message rule.
        /// </summary>
        /// <param name="ruleID">Rule ID.</param>
        /// <param name="cost">Cost specifies in what order rules are processed. Costs with lower values are processed first.</param>
        /// <param name="enabled">Specifies if rule is enabled.</param>
        /// <param name="checkNextRule">Specifies when next rule is checked.</param>
        /// <param name="description">Rule description.</param>
        /// <param name="matchExpression">Rule match expression.</param>
        public void UpdateGlobalMessageRule(string ruleID,long cost,bool enabled,GlobalMessageRule_CheckNextRule_enum checkNextRule,string description,string matchExpression)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            m_UpdSync.BeginUpdate();

			try{
                // TODO: check match expression

                // Check that specified rule exists
                if(!GlobalMessageRuleExists(ruleID)){
                    throw new Exception("Invalid ruleID '" + ruleID + "', specified ruleID doesn't exist !");
                }

                foreach(DataRow dr in dsRules.Tables["GlobalMessageRules"].Rows){
                    if(dr["RuleID"].ToString().ToLower() == ruleID){
                        dr["RuleID"]          = ruleID;
					    dr["Cost"]            = cost;
					    dr["Enabled"]         = enabled;
					    dr["CheckNextRuleIf"] = (int)checkNextRule;
					    dr["Description"]     = description;
                        dr["MatchExpression"] = matchExpression;

                        dsRules.WriteXml(m_DataPath + "GlobalMessageRules.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }
        
        #endregion

        #region method GlobalMessageRuleExists

        /// <summary>
        /// Gets is global message rule with specified ruleID exists.
        /// </summary>
        /// <param name="ruleID">Rule ID of rule what to check.</param>
        /// <returns>Returns true if rule exists.</returns>
        public bool GlobalMessageRuleExists(string ruleID)
        {
            return ContainsID(dsRules.Tables["GlobalMessageRules"],"RuleID",ruleID);
        }

        #endregion

        #region method GetGlobalMessageRuleActions

        /// <summary>
        /// Gets specified global message rule actions.
        /// </summary>
        /// <param name="ruleID">Rule ID of rule which actions to get.</param>
        public DataView GetGlobalMessageRuleActions(string ruleID)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }

            m_UpdSync.AddMethod();

			try{
                // Check that specified rule exists
                if(!GlobalMessageRuleExists(ruleID)){
                    throw new Exception("Invalid ruleID '" + ruleID + "', specified ruleID doesn't exist !");
                }

				DataView dv = new DataView(dsRuleActions.Copy().Tables["GlobalMessageRuleActions"]);
				dv.RowFilter = "RuleID='" + ruleID + "'";

				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
        }

        #endregion

        #region method AddGlobalMessageRuleAction

        /// <summary>
        /// Adds action to specified global message rule.
        /// </summary>
        /// <param name="ruleID">Rule ID to which to add this action.</param>
        /// <param name="actionID">Action ID. Guid.NewID().ToString() is suggested.</param>
        /// <param name="description">Action description.</param>
        /// <param name="actionType">Action type.</param>
        /// <param name="actionData">Action data. Data structure depends on action type.</param>
        public void AddGlobalMessageRuleAction(string ruleID,string actionID,string description,GlobalMessageRuleAction_enum actionType,byte[] actionData)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }
            
            m_UpdSync.BeginUpdate();

			try{
                // Check that specified rule exists
                if(!GlobalMessageRuleExists(ruleID)){
                    throw new Exception("Invalid ruleID '" + ruleID + "', specified ruleID doesn't exist !");
                }

				if(!ContainsID(dsRuleActions.Tables["GlobalMessageRuleActions"],"ActionID",ruleID)){
					DataRow dr = dsRuleActions.Tables["GlobalMessageRuleActions"].NewRow();
					dr["RuleID"]      = ruleID;
					dr["ActionID"]    = actionID;
					dr["Description"] = description;
                    dr["ActionType"]  = actionType;
                    dr["ActionData"]  = actionData;
							
					dsRuleActions.Tables["GlobalMessageRuleActions"].Rows.Add(dr);
					dsRuleActions.WriteXml(m_DataPath + "GlobalMessageRuleActions.xml",XmlWriteMode.IgnoreSchema);
				}
				else{
					throw new Exception("Specified actionID '" + actionID + "' already exists, choose another actionID !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method DeleteGlobalMessageRuleAction

        /// <summary>
        /// Deletes specified action from specified global message rule.
        /// </summary>
        /// <param name="ruleID">Rule ID which action to delete.</param>
        /// <param name="actionID">Action ID of action which to delete.</param>
        public void DeleteGlobalMessageRuleAction(string ruleID,string actionID)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }
                        
            m_UpdSync.BeginUpdate();

			try{
                // Check that specified rule exists
                if(!GlobalMessageRuleExists(ruleID)){
                    throw new Exception("Invalid ruleID '" + ruleID + "', specified ruleID doesn't exist !");
                }

                // Delete specified action
                foreach(DataRow dr in dsRuleActions.Tables["GlobalMessageRuleActions"].Rows){
                    if(dr["RuleID"].ToString().ToLower() == ruleID && dr["ActionID"].ToString().ToLower() == actionID){
                        dr.Delete();
                        dsRuleActions.WriteXml(m_DataPath + "GlobalMessageRuleActions.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method UpdateGlobalMessageRuleAction

        /// <summary>
        /// Updates specified rule action.
        /// </summary>
        /// <param name="ruleID">Rule ID which action to update.</param>
        /// <param name="actionID">Action ID of action which to update.</param>
        /// <param name="description">Action description.</param>
        /// <param name="actionType">Action type.</param>
        /// <param name="actionData">Action data. Data structure depends on action type.</param>
        public void UpdateGlobalMessageRuleAction(string ruleID,string actionID,string description,GlobalMessageRuleAction_enum actionType,byte[] actionData)
        {
            if(ruleID == null || ruleID == ""){
                throw new Exception("Invalid ruleID value, ruleID can't be '' or null !");
            }
            if(actionID == null || actionID == ""){
                throw new Exception("Invalid actionID value, actionID can't be '' or null !");
            }

            m_UpdSync.BeginUpdate();

			try{
                // Check that specified rule exists
                if(!GlobalMessageRuleExists(ruleID)){
                    throw new Exception("Invalid ruleID '" + ruleID + "', specified ruleID doesn't exist !");
                }

                bool actionExists = false;
                foreach(DataRow dr in dsRuleActions.Tables["GlobalMessageRuleActions"].Rows){
                    if(dr["RuleID"].ToString().ToLower() == ruleID && dr["ActionID"].ToString().ToLower() == actionID){
                        dr["RuleID"]      = ruleID;
					    dr["ActionID"]    = actionID;
					    dr["Description"] = description;
                        dr["ActionType"]  = actionType;
                        dr["ActionData"]  = actionData;

                        dsRuleActions.WriteXml(m_DataPath + "GlobalMessageRuleActions.xml",XmlWriteMode.IgnoreSchema);
                        actionExists = true;
                        break;
                    }
                }

                if(!actionExists){
                    throw new Exception("Invalid actionID '" + actionID + "', specified actionID doesn't exist !");
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #endregion


		#region Routing related

		#region function GetRoutes

		/// <summary>
		/// Gets email address routes.
		/// </summary>
		/// <returns></returns>
		public DataView GetRoutes()
		{	
			m_UpdSync.AddMethod();

			try{
				DataView dv = new DataView(dsRouting.Copy().Tables["Routing"]);
                dv.Sort = "Cost ASC";
				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}
		
		#endregion


		#region function AddRoute

		/// <summary>
		/// Adds new route.
		/// </summary>
		/// <param name="routeID">Route ID.</param>
        /// <param name="cost">Cost specifies in what order roues are processed. Costs with lower values are processed first.</param>
		/// <param name="enabled">Specifies if route is enabled.</param>
		/// <param name="description">Route description text.</param>
		/// <param name="pattern">Match pattern. For example: *,*@domain.com,*sales@domain.com.</param>
		/// <param name="action">Specifies route action.</param>
		/// <param name="actionData">Route action data.</param>
		public void AddRoute(string routeID,long cost,bool enabled,string description,string pattern,RouteAction_enum action,byte[] actionData)
		{			
			if(routeID.Length == 0){
				throw new Exception("You must specify routeID");
			}
			if(pattern.Length == 0){
				throw new Exception("You must specify pattern");
			}

			m_UpdSync.BeginUpdate();

			try{
				if(!ContainsID(dsRouting.Tables["Routing"],"RouteID",routeID)){
					DataRow dr = dsRouting.Tables["Routing"].NewRow();
					dr["RouteID"]     = routeID;
                    dr["Cost"]        = cost;
					dr["Enabled"]     = enabled;
					dr["Description"] = description;
					dr["Pattern"]     = pattern;
					dr["Action"]      = action;
                    dr["ActionData"]  = actionData;
							
					dsRouting.Tables["Routing"].Rows.Add(dr);
					dsRouting.WriteXml(m_DataPath + "Routing.xml",XmlWriteMode.IgnoreSchema);
				}
				else{
					throw new Exception("Route with specified ID '" + routeID + "' already exists !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region function DeleteRoute

		/// <summary>
		/// Deletes route.
		/// </summary>
		/// <param name="routeID">Route ID.</param>
		public void DeleteRoute(string routeID)
		{
			m_UpdSync.BeginUpdate();

			try{
				using(DataView dv = new DataView(dsRouting.Tables["Routing"])){
					dv.RowFilter = "RouteID='" + routeID + "'";

					if(dv.Count > 0){
						dv[0].Delete();						
					}

					dsRouting.WriteXml(m_DataPath + "Routing.xml",XmlWriteMode.IgnoreSchema);
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region function UpdateRoute

		/// <summary>
		/// Updates route.
		/// </summary>
		/// <param name="routeID">Route ID.</param>
        /// <param name="cost">Cost specifies in what order roues are processed. Costs with lower values are processed first.</param>
		/// <param name="enabled">Specifies if route is enabled.</param>
		/// <param name="description">Route description text.</param>
		/// <param name="pattern">Match pattern. For example: *,*@domain.com,*sales@domain.com.</param>
		/// <param name="action">Specifies route action.</param>
		/// <param name="actionData">Route action data.</param>
		public void UpdateRoute(string routeID,long cost,bool enabled,string description,string pattern,RouteAction_enum action,byte[] actionData)
		{		
			if(pattern.Length == 0){
				throw new Exception("You must specify pattern");
			}

			m_UpdSync.BeginUpdate();

			try{
                bool contains = false;
                foreach(DataRow dr in dsRouting.Tables["Routing"].Rows){
                    if(dr["RouteID"].ToString() == routeID){
                        dr["Cost"]        = cost;
					    dr["Enabled"]     = enabled;
					    dr["Description"] = description;
					    dr["Pattern"]     = pattern;
					    dr["Action"]      = action;
                        dr["ActionData"]  = actionData;

                        dsRouting.WriteXml(m_DataPath + "Routing.xml",XmlWriteMode.IgnoreSchema);
                        contains = true;
                        break;
                    }
                }
                if(!contains){
                    throw new Exception("Route with specified ID '" + routeID + "' doesn't exist !");
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#endregion


		#region MailStore related

		#region method GetMessagesInfo

        /// <summary>
        /// Gets specified IMAP folder messages info. 
        /// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what messages info to get. For example: Inbox,Public Folders/Documnets .</param>
        /// <param name="messages">IMAP_Messages collection where to store folder messages info.</param>
        public void GetMessagesInfo(string accessingUser,string folderOwnerUser,string folder,IMAP_MessageCollection messages)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'r' permission.
                    There is builtin user system, skip ACL for it.
                *) Fill messages info.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateNotNull(messages);
            //---------------------------------------//
          			
            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'r' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.r) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }
            
            //--- Get message info from messages info db file ----------------------------------------------------//
            string userID         = GetUserID(folderOwnerUser);
            string folderFullPath = API_Utlis.PathFix(API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\" + Core.Encode_IMAP_UTF7_String(folder))) + "\\");

            List<FolderMessageInfo> messagesInfo = FolderMessagesInfoManager.GetMessagesInfo(folderFullPath);
            Dictionary<int,int>     flagsDB      = FolderMessageFlagsManager.GetFlags(userID,folderFullPath);
            foreach(FolderMessageInfo messageInfo in messagesInfo){
                IMAP_MessageFlags flags = IMAP_MessageFlags.Recent;
                if(flagsDB.ContainsKey(messageInfo.UID)){
                    flags = (IMAP_MessageFlags)flagsDB[messageInfo.UID];
                }                
                messages.Add(
                    messageInfo.InternalDate.ToString("yyyyMMddHHmmss") + "_" + messageInfo.UID.ToString("D10"),
                    messageInfo.UID,
                    messageInfo.InternalDate.ToLocalTime(),
                    messageInfo.Size,
                    flags
                );
            }
            //--------------------------------------------------------------------------------------------------//  
        }
		
		#endregion

        
		#region method StoreMessage

		/// <summary>
		/// Stores message to specified folder.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder where to store message. For example: Inbox,Public Folders/Documnets .</param>
		/// <param name="msgStream">Stream where message has stored. Stream position must be at the beginning of the message.</param>
		/// <param name="date">Recieve date.</param>
		/// <param name="flags">Message flags.</param>
		public void StoreMessage(string accessingUser,string folderOwnerUser,string folder,Stream msgStream,DateTime date,IMAP_MessageFlags flags)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'p' or 'i' permission.
                    There is builtin user system, skip ACL for it.
                *) Store message.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateNotNull(msgStream);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'p' or 'i' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.p) == 0 && (acl & IMAP_ACL_Flags.i) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Store message
            string path = API_Utlis.PathFix(API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\" + Core.Encode_IMAP_UTF7_String(folder))) + "\\");
						
            long fileSize = msgStream.Length - msgStream.Position;
            int uid = GetNextUid(folderOwnerUser,folder);
            string fileName = CreateMessageFileName(date.ToUniversalTime(),uid);
											
			//---- Write message data to file -----------------------------------------------------//
			using(FileStream fs = File.Create(path + fileName + ".eml")){
                msgStream.Position = 0;
                API_Utlis.StreamCopy(msgStream,fs);		
			}

            // Check that stored file exists, some AV programs just delete stored file, if conatins virus
            if(!File.Exists(path + fileName + ".eml")){
                return;
            }

            // Increase mailbox size
			ChangeMailboxSize(folderOwnerUser,fileSize);

			//-------------------------------------------------------------------------------------//
     
            // Store message info to messages info db.
            FolderMessagesInfoManager.Append(path,uid,(int)fileSize,date.ToUniversalTime());

            // Recent is default falgs, so we need to store message flags only if not \Recent
            if(flags != IMAP_MessageFlags.Recent){
                StoreMessageFlags(accessingUser,folderOwnerUser,folder,uid,flags);
            }
		}
		
		#endregion

		#region method StoreMessageFlags

        /// <summary>
		/// Stores IMAP message flags (\seen,\draft, ...).
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder which message flags to store. For example: Inbox,Public Folders/Documnets .</param>
		/// <param name="message">Fix ME: ???</param>
		/// <param name="msgFlags">Message flags to store.</param>
		public void StoreMessageFlags(string accessingUser,string folderOwnerUser,string folder,LumiSoft.Net.IMAP.Server.IMAP_Message message,IMAP_MessageFlags msgFlags)
		{
            StoreMessageFlags(accessingUser,folderOwnerUser,folder,(int)message.UID,msgFlags);
        }

		/// <summary>
		/// Stores IMAP message flags (\seen,\draft, ...).
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder which message flags to store. For example: Inbox,Public Folders/Documnets .</param>
		/// <param name="uid">Message UID.</param>
		/// <param name="msgFlags">Message flags to store.</param>
		private void StoreMessageFlags(string accessingUser,string folderOwnerUser,string folder,int uid,IMAP_MessageFlags msgFlags)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) Remove all message flags which permissions user doesn't have.
                *) Store message.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // Remove all message flags which permissions user doesn't have.
            if(accessingUser != "system"){
			    IMAP_ACL_Flags userACL = GetUserACL(folderOwnerUser,folder,accessingUser);					
			    if((userACL & IMAP_ACL_Flags.s) == 0){
    				msgFlags &= ~IMAP_MessageFlags.Seen;
			    }
			    else if((userACL & IMAP_ACL_Flags.d) == 0){
				    msgFlags &= ~IMAP_MessageFlags.Deleted;
			    }				
			    else if((userACL & IMAP_ACL_Flags.s) == 0){
				    msgFlags &= (~IMAP_MessageFlags.Answered | ~IMAP_MessageFlags.Draft | ~IMAP_MessageFlags.Flagged);
			    }
            }

            //--- Store flags
            FolderMessageFlagsManager.SetFlags(
                GetUserID(folderOwnerUser),
                API_Utlis.PathFix(API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\" + Core.Encode_IMAP_UTF7_String(folder))) + "\\"),
                uid,
                msgFlags
            );
		}
		
		#endregion

		#region method DeleteMessage

		/// <summary>
		/// Deletes message from mailbox.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what message to delete. For example: Inbox,Public Folders/Documnets .</param>
		/// <param name="messageID">Message ID.</param>
		/// <param name="uid">Message UID value.</param>
		public void DeleteMessage(string accessingUser,string folderOwnerUser,string folder,string messageID,int uid)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'd' permission.
                    There is builtin user system, skip ACL for it.
                *) Fill messages info.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateNotNull(messageID);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'd' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.d) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Delete message            
            string folderFullPath = API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\" + Core.Encode_IMAP_UTF7_String(folder) + "\\"));
			string msgFile        = API_Utlis.FileExists(folderFullPath + messageID + ".eml");
            string msgInfoFile    = API_Utlis.FileExists(folderFullPath + messageID + ".info");
                        			
            // Delete message from messages info db and also delete specified message flags on all users
            FolderMessagesInfoManager.Delete(folderFullPath,uid);
            FolderMessageFlagsManager.DeleteFlags(folderFullPath,uid);

            // Delete message info file
            if(msgInfoFile != null){
                File.Delete(msgInfoFile);
            }

            // Delete message file
            if(msgFile != null){
                // Message must be moved to recycle bin
                if(Convert.ToBoolean(GetRecycleBinSettings().Rows[0]["DeleteToRecycleBin"])){
                    RecycleBinManager.StoreToRecycleBin(folderOwnerUser,folder,msgFile);
                }

			    FileInfo fInfo = new FileInfo(msgFile);
			    if(fInfo.Exists){
				    long fileSize = fInfo.Length;
				    fInfo.Delete();                

				    // Decrease mailbox size
				    ChangeMailboxSize(folderOwnerUser,-fileSize);
			    }
            }
		}
		
		#endregion

        #region method GetMessageItems

        /// <summary>
        /// Gets specified message specified items.
        /// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what message to delete. For example: Inbox,Public Folders/Documnets .</param>
        /// <param name="e">MessageItems info.</param>
        public void GetMessageItems(string accessingUser,string folderOwnerUser,string folder,EmailMessageItems e)
        {
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'r' permission.
                    There is builtin user system, skip ACL for it.
                *) Store message.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateNotNull(e);
            //---------------------------------------//
            
            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'r' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.r) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Get message
            string folderPath = API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\" + Core.Encode_IMAP_UTF7_String(folder)));
			string msgFile    = API_Utlis.FileExists(API_Utlis.PathFix(folderPath + "\\" + e.MessageID + ".eml"));
                        
			// Check if file exists
			if(msgFile != null){                
                if((e.MessageItems & (IMAP_MessageItems_enum.BodyStructure | IMAP_MessageItems_enum.Envelope)) != 0){                    
                    // Using OpenOrCreateFile ensures that multiple .info files constructions won't happen at same time.
                    // This can happen if multiple sessions want same message at same time.
                    using(FileStream fs = OpenOrCreateFile(API_Utlis.PathFix(folderPath + "\\" + e.MessageID + ".info"),10000)){
                        DataSet dsMessageInfo = new DataSet("dsMessageInfo");
                        dsMessageInfo.Tables.Add("MessageInfo");
                        dsMessageInfo.Tables["MessageInfo"].Columns.Add("ImapEnvelope");
                        dsMessageInfo.Tables["MessageInfo"].Columns.Add("ImapBody");

                        // Message info file doesn't exist, create it
                        if(fs.Length == 0){
                            Mime m = null;
                            try{
                                m = Mime.Parse(msgFile);
                            }
                            // Invalid message, parsing failed
                            catch{
                                m = Mime.CreateSimple(new AddressList(),new AddressList(),"BAD Message","This is BAD message, mail server failed to parse it !","");
                            }

                            DataRow dr = dsMessageInfo.Tables["MessageInfo"].NewRow();
                            dr["ImapEnvelope"] = IMAP_Envelope.ConstructEnvelope(m.MainEntity);
                            dr["ImapBody"]     = IMAP_BODY.ConstructBodyStructure(m,false);
                            dsMessageInfo.Tables["MessageInfo"].Rows.Add(dr);
                            dsMessageInfo.WriteXml(fs);
                        }
                        // Load message info
                        else{
                            dsMessageInfo.ReadXml(fs);
                        }

                        e.Envelope = dsMessageInfo.Tables["MessageInfo"].Rows[0]["ImapEnvelope"].ToString();
                        // Remove word BODY, currently internal header adds it.
                        e.BodyStructure = dsMessageInfo.Tables["MessageInfo"].Rows[0]["ImapBody"].ToString().Substring(5);
                    }             
                }

                if((e.MessageItems & (IMAP_MessageItems_enum.Header | IMAP_MessageItems_enum.Message)) != 0){
                    bool closeStream = true;
                    // TODO: Cheage to FileAccess.Read in next versions 
                    FileStream fs = File.Open(msgFile,FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite);

                    // Just skip internal header
                    _InternalHeader internalHeader = new _InternalHeader(fs);
                    long initialPosition = fs.Position;
                    if((e.MessageItems & IMAP_MessageItems_enum.Header) != 0){
                        fs.Position = initialPosition;
                        e.Header = GetTopLines(fs,0);
                    }
                    if((e.MessageItems & IMAP_MessageItems_enum.Message) != 0){
                        fs.Position = initialPosition;                    
                        e.MessageStream = fs;
                        // Don't close stream, it will be closed by IMAP server when all done.
                        closeStream = false;
                    }

                    if(closeStream){
                        fs.Dispose();
                    }
                }                
			}
			else{
				e.MessageExists = false;
			}
        }

        #endregion


		#region method GetMessageTopLines

		/// <summary>
		/// Gets message header + number of specified lines.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what message top lines to get. For example: Inbox,Public Folders/Documnets .</param>
		/// <param name="msgID">MessageID.</param>
		/// <param name="nrLines">Number of lines to retrieve. NOTE: line counting starts at the end of header.</param>
		/// <returns>Returns message header + number of specified lines.</returns>
		public byte[] GetMessageTopLines(string accessingUser,string folderOwnerUser,string folder,string msgID,int nrLines)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists.
                *) Normalize folder. Remove '/' from folder start and end, ... .
                *) Do Shared Folders mapping.
                *) Ensure that folder exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'r' permission.
                    There is builtin user system, skip ACL for it.
                *) Get message top lines.
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateNotNull(msgID);
            //---------------------------------------//
            
            // Ensure that user exists.
            if(!UserExists(folderOwnerUser)){
                throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
            }

            // Normalize folder. Remove '/' from folder start and end.
            folder = API_Utlis.NormalizeFolder(folder);

            // Do Shared Folders mapping.
            string originalFolder = folder;
            SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
            if(mappedFolder.IsSharedFolder){
                folderOwnerUser = mappedFolder.FolderOnwer;
                folder = mappedFolder.Folder;

                if(folderOwnerUser == "" || folder == ""){
                    throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                }
            }

            // Ensure that folder exists. Throw Exception if don't.
            if(!FolderExists(folderOwnerUser + "/" + folder)){
                throw new Exception("Folder '" + folder + "' doesn't exist !");
            }

            // See if user has sufficient permissions. User requires 'r' permission.
            //  There is builtin user system, skip ACL for it.
            if(accessingUser.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                if((acl & IMAP_ACL_Flags.r) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                }
            }

            //--- Get message top lines
			string msgFile = API_Utlis.FileExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\" + Core.Encode_IMAP_UTF7_String(folder) + "\\" + msgID + ".eml"));

            if(msgFile == null){
                return null;
            }

			using(FileStream fs = File.OpenRead(msgFile)){
                // Just skip internal header
                new _InternalHeader(fs);

				return GetTopLines(fs,nrLines);
			}
		}
		
		#endregion

		#region method CopyMessage

		/// <summary>
		/// Creates copy of message to destination IMAP folder.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what contains message to copy. For example: Inbox,Public Folders/Documnets .</param>
		/// <param name="destFolderUser">Destination IMAP folder owner user name.</param>
		/// <param name="destFolder">Destination IMAP folder name.</param>
		/// <param name="message">IMAP message which to copy.</param>
		public void CopyMessage(string accessingUser,string folderOwnerUser,string folder,string destFolderUser,string destFolder,LumiSoft.Net.IMAP.Server.IMAP_Message message)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) We don't need to map shared folder, check security, it done by GetMessage and StoreMessage methods.
                *) Copy message.               
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateUserName(destFolderUser);
            ArgsValidator.ValidateFolder(destFolder);
            ArgsValidator.ValidateNotNull(message);
            //---------------------------------------//
            
            //--- Copy message
            EmailMessageItems msgItems = new EmailMessageItems(message.ID,IMAP_MessageItems_enum.Message);            
            GetMessageItems(accessingUser,folderOwnerUser,folder,msgItems);
            StoreMessage("system",destFolderUser,destFolder,msgItems.MessageStream,message.InternalDate,message.Flags);
            msgItems.MessageStream.Dispose();
		}
		
		#endregion


		#region method GetFolders
                    
		/// <summary>
		/// Gets all available IMAP folders.
		/// </summary>
		/// <param name="userName">User name who's folders to get.</param>
        /// <param name="includeSharedFolders">If true, shared folders are included.</param>
		public string[] GetFolders(string userName,bool includeSharedFolders)
		{
            /* Implementation notes:
                *) Validate values. Throw ArgumnetExcetion if invalid values.
                *) Ensure that user exists. Throw Exception if don't.
                *) See if user has sufficient permissions. User requires 'l' permission.                             
                *) Append all visible(Forlders on what user has 'r' right) user mailbox folders.
                *) Append all visible(Forlders on what user has 'r' right) public folders to folders list !
            */

            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(userName);
            //---------------------------------------//

            // Ensure that user exists.
            if(!UserExists(userName)){
                throw new Exception("User '" + userName + "' desn't exist !");
            }

            // See if user has sufficient permissions. User requires 'l' permission.
            //  There is builtin user system, skip ACL for it.
       /*     if(userName.ToLower() != "system"){
                IMAP_ACL_Flags acl = GetUserACL(folder,userName);
                if((acl & IMAP_ACL_Flags.l) == 0){
                    throw new InsufficientPermissionsException("Insufficient permissions for folder '" + folder + "' !");
                }
            }*/

			string path = API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName));
			// If user root directory doesn exist, create it.
			if(path == null){
				Directory.CreateDirectory(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName));
                path = API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName);
			}

            // Append all visible(Folders on what user has 'r' right) user mailbox folders.
            string[] folders = GetFileSytemFolders(path + "\\",false);            
            List<string> userFolders = new List<string>();
            foreach(string folder in folders){
                // Show folders what user has 'r' right
                if((GetUserACL(userName,folder,userName) & IMAP_ACL_Flags.r) != 0){
                    userFolders.Add(Core.Decode_IMAP_UTF7_String(folder).Replace('\\','/'));
                }
            }

            /* REMOVE ME: 14.09.2006 getting rid of folders.xml
			DataSet dsFolders = new DataSet();
			using(FileStream fs = OpenOrCreateFile(path + "folders.xml",10000)){
				dsFolders = GetUserMailboxFolders(userName,fs);
			}

            // Append all visible(Forlders on what user has 'r' right) user mailbox folders.
            List<string> userFolders = new List<string>();
			for(int i=0;i<dsFolders.Tables["Folders"].Rows.Count;i++){
                string folder = dsFolders.Tables["Folders"].Rows[i]["FolderPath"].ToString();
                // Show folders what user has 'r' right
                if((GetUserACL(userName,folder,userName) & IMAP_ACL_Flags.r) != 0){
                    userFolders.Add(folder);
                }
			}
             * */

            // Append all visible(Folders on what user has 'r' right) shared folders to folders list
            if(includeSharedFolders){
                SharedFolderRoot[] sharedFolderRoots = GetSharedFolderRoots();
                foreach(SharedFolderRoot sharedFolderRoot in sharedFolderRoots){
                    // Skip disabled roots
                    if(!sharedFolderRoot.Enabled){
                        continue;
                    }

                    // Root is bounded folder
                    if(sharedFolderRoot.RootType == SharedFolderRootType_enum.BoundedRootFolder){
                        // Get root child folders
                        string[] boundedFolders = GetFolders(sharedFolderRoot.BoundedUser,false);
                        foreach(string boundedFolder in boundedFolders){
                            // We want folders what will be BoundedFolder child folders, others just skip
                            if(boundedFolder.ToLower().StartsWith(sharedFolderRoot.BoundedFolder.ToLower())){
                                // Show only folder what user has 'lr' right
                                if((GetUserACL(sharedFolderRoot.BoundedUser,boundedFolder,userName) & IMAP_ACL_Flags.r) != 0){
                                    // We must cut of boundedFolder part. For example root name = 'Public'and 
                                    // boundedFolder = 'Inbox', then it must be visible as Public/InboxChildFolders,
                                    // not Public/Inbox/InboxChildFolders.
                                    userFolders.Add(sharedFolderRoot.FolderName + boundedFolder.Substring(sharedFolderRoot.BoundedFolder.Length));
                                }
                            }
                        }
                    }
                    // Root is Users Shared Folders namespace
                    else{
                        // Get shared user on which accessing user has access rights
                        List<string> sharingUsers = new List<string>();
                        foreach(DataRow dr in dsImapACL.Tables["ACL"].Rows){
                            string userOrGroup = dr["User"].ToString().ToLower();
                            string sharingUserName = dr["Folder"].ToString().Split(new char[]{'/'},2)[0];

                            // anyone access, so we have access to that folder
                            if(userOrGroup == "anyone"){
                                if(!sharingUsers.Contains(sharingUserName)){
                                    sharingUsers.Add(sharingUserName);
                                }
                            }
                            // accessing user has access to that folder
                            else if(userOrGroup == userName.ToLower()){
                                if(!sharingUsers.Contains(sharingUserName)){
                                    sharingUsers.Add(sharingUserName);
                                }
                            }
                            else{
                                // If group ACL set, see if accessing user is member of that group
                                DataRow drGroup = GetGroup(userOrGroup);
                                if(drGroup != null && IsUserGroupMember(drGroup["GroupName"].ToString(),userName)){
                                    if(!sharingUsers.Contains(sharingUserName)){
                                        sharingUsers.Add(sharingUserName);
                                    }
                                }                                
                            }
                        }

                        //--- Show folders on what user has enough ACL
                        foreach(string sharingUser in sharingUsers){
                            string[] sharingUserFolders = GetFolders(sharingUser,false);
                            foreach(string sharingUserFolder in sharingUserFolders){
                                // Show only folder what user has 'lr' right
                                if((GetUserACL(sharingUser,sharingUserFolder,userName) & IMAP_ACL_Flags.r) != 0){                                
                                    userFolders.Add(sharedFolderRoot.FolderName + "/" + sharingUser + "/" + sharingUserFolder);
                                }
                            }
                        }
                    }                   
                }
            }

			return userFolders.ToArray();
		}
		
		#endregion
//**
		#region method FolderExists

		/// <summary>
		/// Gets if specified folder exists.
		/// </summary>
		/// <param name="folderName">Folder name which to check. Eg. UserName/Inbox,UserName/Inbox/subfolder.</param>
		/// <returns>Returns true if folder exists, otherwise false.</returns>
		public bool FolderExists(string folderName)
		{	
			if(API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + Core.Encode_IMAP_UTF7_String(folderName))) != null){                
				return true;
			}
			else{
				string[] user_folder = folderName.Split(new char[]{'/'},2);
				// Inbox is missing, create it
				if(user_folder[1].ToLower() == "inbox"){
					Directory.CreateDirectory(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + user_folder[0] + "\\Inbox"));
					return true;
				}
				else{
					return false;
				}
			}
		}

		#endregion

		#region method CreateFolder

		/// <summary>
		/// Creates new IMAP folder.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what to create. For example: Inbox,Public Folders/Documnets .</param>
		public void CreateFolder(string accessingUser,string folderOwnerUser,string folder)
		{
            m_UpdSync.BeginUpdate();

            try{
                /* Implementation notes:
                    *) Validate values. Throw ArgumnetExcetion if invalid values.
                    *) Ensure that user exists.
                    *) Normalize folder. Remove '/' from folder start and end, ... .
                    *) Do Shared Folders mapping.
                    *) Ensure that folder doesn't exists. Throw Exception if don't.
                    *) See if user has sufficient permissions. User requires 'c' permission.
                        There is builtin user system, skip ACL for it.
                    *) Create folder.
                */

                //--- Validate values -------------------//
                ArgsValidator.ValidateUserName(folderOwnerUser);
                ArgsValidator.ValidateFolder(folder);
                //---------------------------------------//

                // Ensure that user exists.
                if(!UserExists(folderOwnerUser)){
                    throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
                }

                // Normalize folder. Remove '/' from folder start and end.
                folder = API_Utlis.NormalizeFolder(folder);

                // Do Shared Folders mapping.
                string originalFolder = folder;
                SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
                if(mappedFolder.IsSharedFolder){
                    folderOwnerUser = mappedFolder.FolderOnwer;
                    folder = mappedFolder.Folder;

                    if(folderOwnerUser == "" || folder == ""){
                        throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                    }
                }

                // Ensure that folder doesn't exists. Throw Exception if don't.
                if(FolderExists(folderOwnerUser + "/" + folder)){
                    throw new Exception("Folder '" + folder + "' already exist !");
                }
                
                // See if user has sufficient permissions. User requires 'c' permission.
                //  There is builtin user system, skip ACL for it.
                if(accessingUser.ToLower() != "system"){
                    IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                    if((acl & IMAP_ACL_Flags.c) == 0){
                        throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                    }
                }
               

                //--- Create folder
			    string dir = API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\" + Core.Encode_IMAP_UTF7_String(folder));
			    if(API_Utlis.DirectoryExists(dir) == null){
                    Directory.CreateDirectory(dir);

                    /* REMOVE ME: 14.09.2006 getting rid of folders.xml
				    // Syncronize folders.xml file and create folder
				    using(FileStream fs = OpenOrCreateFile(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\folders.xml",10000)){					
					    DataSet dsFolders = GetUserMailboxFolders(folderOwnerUser,fs);

					    // Get UID
					    int uid = Convert.ToInt32(dsFolders.Tables["Folders_UID_Holder"].Rows[0]["NextUID"]);
					    // Increase next UID
					    dsFolders.Tables["Folders_UID_Holder"].Rows[0]["NextUID"] = uid + 1;
                        
                        DataRow dr = dsFolders.Tables["Folders"].NewRow();
					    dr["FolderPath"]      = folder;
					    dr["Folder_IMAP_UID"] = uid;
					    dsFolders.Tables["Folders"].Rows.Add(dr);
                        
					    fs.SetLength(0);
					    dsFolders.WriteXml(fs);					
				    }*/				
			    }
			    else{
				    throw new Exception("Folder(" + folder + ") already exists");
			    }
            }
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region method DeleteFolder

		/// <summary>
		/// Deletes IMAP folder.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what to delete. For example: Inbox,Public Folders/Documnets .</param>
		public void DeleteFolder(string accessingUser,string folderOwnerUser,string folder)
		{
			m_UpdSync.BeginUpdate();

			try{
                /* Implementation notes:
                    *) Validate values. Throw ArgumnetExcetion if invalid values.
                    *) Ensure that user exists.
                    *) Normalize folder. Remove '/' from folder start and end, ... .
                    *) Don't allow to delete Users Default permanent folders.
                    *) Do Shared Folders mapping.
                    *) Don't allow to delete shared folders root folder.
                       For BoundedUser root don't allow root folder only,
                       for UsersShared root don't allow root + user name.
                    *) Ensure that folder exists. Throw Exception if don't.
                    *) See if user has sufficient permissions. User requires 'c' permission.
                        There is builtin user system, skip ACL for it.
                    *) Create folder.
                */

                //--- Validate values -------------------//
                ArgsValidator.ValidateUserName(folderOwnerUser);
                ArgsValidator.ValidateFolder(folder);
                //---------------------------------------//

                // Ensure that user exists.
                if(!UserExists(folderOwnerUser)){
                    throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
                }

                // Normalize folder. Remove '/' from folder start and end.
                folder = API_Utlis.NormalizeFolder(folder);

                // Don't allow to delete Users Default permanent folders.
                foreach(DataRow drFolder in dsUsersDefaultFolders.Tables["UsersDefaultFolders"].Rows){
                    if(drFolder["FolderName"].ToString().ToLower() == folder.ToLower() && Convert.ToBoolean(drFolder["Permanent"])){
                        throw new Exception("Can't delete permanent folder '" + folder + "' !");
                    }
                }

                // Do Shared Folders mapping.
                string originalFolder = folder;
                SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
                if(mappedFolder.IsSharedFolder){
                    folderOwnerUser = mappedFolder.FolderOnwer;
                    folder = mappedFolder.Folder;

                    /* Don't allow to delete shared folders root folder.
                       For BoundedUser root don't allow root folder only,
                       for UsersShared root don't allow root + user name.
                    */

                    // Main shared folder root.
                    if(mappedFolder.SharedRootName.ToLower() == originalFolder.ToLower()){
                        throw new ArgumentException("Can't delete shared root folder '" + originalFolder + "' !");
                    }
                    // Users shared folder: root/username -> no folder
                    if(folder == ""){
                        throw new ArgumentException("Can't delete shared root folder '" + originalFolder + "' !");
                    }

                    if(folderOwnerUser == "" || folder == ""){
                        throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                    }
                }

                // Ensure that folder doesn't exists. Throw Exception if don't.
                if(!FolderExists(folderOwnerUser + "/" + folder)){
                    throw new Exception("Folder '" + folder + "' doesn't exist !");
                }

                // See if user has sufficient permissions. User requires 'c' permission.
                //  There is builtin user system, skip ACL for it.
                if(accessingUser.ToLower() != "system"){
                    IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                    if((acl & IMAP_ACL_Flags.c) == 0){
                        throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                    }
                }

                //--- Delete folder
				string dir = API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\" + Core.Encode_IMAP_UTF7_String(folder)));
				if(dir != null){
                    // Folder Messages must be moved to recycle bin
                    if(Convert.ToBoolean(GetRecycleBinSettings().Rows[0]["DeleteToRecycleBin"])){
                        string[] messages = Directory.GetFiles(dir,"*.eml");
                        foreach(string msgFile in messages){
                            RecycleBinManager.StoreToRecycleBin(folderOwnerUser,folder,msgFile);
                        }
                    }

					Directory.Delete(dir,true);

                    /* REMOVE ME: 14.09.2006 getting rid of folders.xml
					//--- delete folder from folders.xml -----------------------------------------------------------------------//
					using(FileStream fs = OpenOrCreateFile(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\folders.xml",10000)){					
						DataSet dsFolders = GetUserMailboxFolders(folderOwnerUser,fs);
			
						for(int i=0;i<dsFolders.Tables["Folders"].Rows.Count;i++){
							// We must delete folder and it's sub folders
							if(dsFolders.Tables["Folders"].Rows[i]["FolderPath"].ToString().ToLower().StartsWith(folder.ToLower())){
								dsFolders.Tables["Folders"].Rows.Remove(dsFolders.Tables["Folders"].Rows[i]);	
								i--;
							}
						}

						fs.SetLength(0);
						dsFolders.WriteXml(fs);
					}
					//----------------------------------------------------------------------------------------------------------//
                    */

					//---- Delete specified folder ACL if any --------------------------------------//
					bool deletedAny = false;
					for(int i=0;i<dsImapACL.Tables["ACL"].Rows.Count;i++){
						DataRow dr = dsImapACL.Tables["ACL"].Rows[0];
						// We need to remove subfolders too
						if(dr["Folder"].ToString().ToLower().StartsWith(folderOwnerUser + "/" + folder.ToLower())){
							dr.Delete();
							i--;
							deletedAny = true;
						}
					}
					if(deletedAny){
						dsImapACL.WriteXml(m_DataPath + "IMAP_ACL.xml",XmlWriteMode.IgnoreSchema);
					}
					//------------------------------------------------------------------------------//
				}
				else{
					throw new Exception("Folder(" + folder + ") doesn't exist");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region method RenameFolder

		/// <summary>
		/// Renames IMAP folder.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what to delete. For example: Trash,Public Folders/Documnets .</param>
		/// <param name="newFolder">New folder name.</param>
		public void RenameFolder(string accessingUser,string folderOwnerUser,string folder,string newFolder)
		{
			m_UpdSync.BeginUpdate();

			try{
                /* Implementation notes:
                    *) Validate values. Throw ArgumnetExcetion if invalid values.
                    *) Ensure that user exists.
                    *) Normalize folders. Remove '/' from folder start and end, ... .
                    *) Do Shared Folders mapping.
                    *) Don't allow to rename shared folders root folder.
                       For BoundedUser root don't allow root folder only,
                       for UsersShared root don't allow root + user name.
                    *) Ensure that source folder exists. Throw Exception if don't.
                    *) Ensure that destinaton folder doesn't exists. Throw Exception if does.
                    *) See if user has sufficient permissions. User requires 'c' permission.
                        There is builtin user system, skip ACL for it.
                    *) Rename folder.
                */

				// Don't allow to rename Inbox
				if(folder.ToLower() == "Inbox"){
					throw new Exception("Can't rename Inbox");
				}

                //--- Validate values -------------------//
                ArgsValidator.ValidateUserName(folderOwnerUser);
                ArgsValidator.ValidateFolder(folder);
                ArgsValidator.ValidateFolder(newFolder);
                //---------------------------------------//

                // Ensure that user exists.
                if(!UserExists(folderOwnerUser)){
                    throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
                }

                // Normalize folder. Remove '/' from folder start and end.
                folder = API_Utlis.NormalizeFolder(folder);

                // Do Shared Folders mapping.
                string originalFolder = folder;
                SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
                if(mappedFolder.IsSharedFolder){
                    folderOwnerUser = mappedFolder.FolderOnwer;
                    folder = mappedFolder.Folder;
        
                    if(originalFolder.ToLower() == mappedFolder.SharedRootName.ToLower() || folderOwnerUser == "" || folder == ""){
                        throw new Exception("Can't rename shared root folder '" + originalFolder + "' !");
                    }
                }

                // Normalize folder. Remove '/' from folder start and end.
                newFolder = API_Utlis.NormalizeFolder(newFolder);

                // Do Shared Folders mapping.
                string originalNewFolder = newFolder;
                string destinationFolderOwner = folderOwnerUser;
                SharedFolderMapInfo mappedNewFolder = MapSharedFolder(originalNewFolder);
                if(mappedNewFolder.IsSharedFolder){
                    destinationFolderOwner = mappedNewFolder.FolderOnwer;
                    newFolder = mappedNewFolder.Folder;

                    if(originalNewFolder.ToLower() == mappedNewFolder.SharedRootName.ToLower() || destinationFolderOwner == "" || newFolder == ""){
                        throw new Exception("Invalid destination folder value, folder '" + originalNewFolder + "' alreay exists !");
                    }
                }

                // Don't allow shared folder to change root folder
                // For example: "Public Folder/aaa" can't be renamed to "Public Folder New/aaa"
                if(mappedFolder.SharedRootName.ToLower() != mappedNewFolder.SharedRootName.ToLower() || mappedFolder.FolderOnwer.ToLower() != mappedNewFolder.FolderOnwer.ToLower()){
                    throw new ArgumentException("Shared folder can't change root folder !");
                }
                                
                // Ensure that folder does exist. Throw Exception if don't.
                if(!FolderExists(folderOwnerUser + "/" + folder)){
                    throw new Exception("Folder '" + folder + "' doesn't exist !");
                }

                // Ensure that folder doesn't exists. Throw Exception if does.
                if(FolderExists(folderOwnerUser + "/" + newFolder)){
                    throw new Exception("Folder '" + newFolder + "' doesn't exist !");
                }

                // See if user has sufficient permissions. User requires 'c' permission.
                //  There is builtin user system, skip ACL for it.
                if(accessingUser.ToLower() != "system"){
                    IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                    if((acl & IMAP_ACL_Flags.c) == 0){
                        throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                    }
                }

                //--- Rename folder
				string dirSource = API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\" + Core.Encode_IMAP_UTF7_String(folder)));
				string dirDest   = API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\" + Core.Encode_IMAP_UTF7_String(newFolder));
				if(dirSource != null){
					if(API_Utlis.DirectoryExists(dirDest) != null){
						throw new Exception("Destination Folder(" + newFolder + ") already exists");
					}
					else{
						Directory.Move(dirSource,dirDest);

                        /* REMOVE ME: 14.09.2006 getting rid of folders.xml
						//--- rename folder in folders.xml --------------------------------------------------------------------------//
						using(FileStream fs = OpenOrCreateFile(m_MailStorePath + "Mailboxes\\" + folderOwnerUser + "\\folders.xml",10000)){					
							DataSet dsFolders = GetUserMailboxFolders(folderOwnerUser,fs);				
							for(int i=0;i<dsFolders.Tables["Folders"].Rows.Count;i++){
								// Rename folder ant it's sub folders
								if(dsFolders.Tables["Folders"].Rows[i]["FolderPath"].ToString().ToLower().StartsWith(folder.ToLower())){
									dsFolders.Tables["Folders"].Rows[i]["FolderPath"] = newFolder + dsFolders.Tables["Folders"].Rows[i]["FolderPath"].ToString().Substring(folder.Length);
								}
							}

							fs.SetLength(0);
							dsFolders.WriteXml(fs);
						}
						//----------------------------------------------------------------------------------------------------------//
                        */

						//---- Rename specified folder and it's subfolders ACL if any ------------------//
						bool renamedAny = false;
						foreach(DataRow dr in dsImapACL.Tables["ACL"].Rows){
							if(dr["Folder"].ToString().ToLower().StartsWith(folderOwnerUser + "/" + folder.ToLower())){
								string f = folderOwnerUser + "/" + folder.ToLower();

								dr["Folder"] = folderOwnerUser + "/" + newFolder + dr["Folder"].ToString().Substring(f.Length);
								renamedAny = true;
							}
						}
						if(renamedAny){
							dsImapACL.WriteXml(m_DataPath + "IMAP_ACL.xml",XmlWriteMode.IgnoreSchema);
						}
						//------------------------------------------------------------------------------//
					}
				}
				else{
					throw new Exception("Source Folder(" + folder + ") doesn't exist");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

        #region method FolderCreationTime

        /// <summary>
		/// Gets time when specified folder was created.
		/// </summary>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what creation time to get. For example: Inbox,Public Folders/Documnets .</param>
		public DateTime FolderCreationTime(string folderOwnerUser,string folder)
        {
            string folderPath = API_Utlis.DirectoryExists(m_MailStorePath + "Mailboxes/" + folderOwnerUser + "/" + Core.Encode_IMAP_UTF7_String(folder));
            if(folderPath != null){
                if(File.Exists(folderPath + "/_FolderCreationTime.txt")){
				    return DateTime.ParseExact(File.ReadAllText(folderPath + "/_FolderCreationTime.txt"),"yyyyMMdd HH:mm:ss",System.Globalization.DateTimeFormatInfo.InvariantInfo);
                }
                else{
                    DateTime creationTime = Directory.GetCreationTime(folderPath);
                    File.WriteAllText(folderPath + "/_FolderCreationTime.txt",creationTime.ToString("yyyyMMdd HH:mm:ss"));
                    return creationTime;
                }
			}
            else{
                throw new Exception("Folder '" + folderOwnerUser + "/" + folder + "' doesn't exist !");
            }
        }

		#endregion


		#region method GetSubscribedFolders

		/// <summary>
		/// Gets subscribed IMAP folders.
		/// </summary>
		/// <param name="userName"></param>
		public string[] GetSubscribedFolders(string userName)
		{
			DataSet ds = new DataSet();
			ds.Tables.Add("Subscriptions");
			ds.Tables["Subscriptions"].Columns.Add("Name");

			// Check if root directory exists, if not Create
            string path = API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName));
			if(path == null){
				Directory.CreateDirectory(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName));
                path = API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName);
			}

			if(!File.Exists(API_Utlis.PathFix(path + "\\imap.xml"))){
				DataSet dsx = ds.Copy();
				dsx.Tables["Subscriptions"].Rows.Add(new object[]{"Inbox"});
				dsx.WriteXml(API_Utlis.PathFix(path + "\\imap.xml"));
			}

			ds.ReadXml(API_Utlis.PathFix(path + "\\imap.xml"));

			ArrayList dirs = new ArrayList();
			foreach(DataRow dr in ds.Tables["Subscriptions"].Rows){
				dirs.Add(dr["Name"].ToString());
			}

			string[] retVal = new string[dirs.Count];
			dirs.CopyTo(retVal);

			return retVal;
		}
		
		#endregion

		#region method SubscribeFolder

		/// <summary>
		/// Subscribes new IMAP folder.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="folder"></param>
		public void SubscribeFolder(string userName,string folder)
		{
            string path = API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName));

			DataSet ds = new DataSet();
            ds.Tables.Add("Subscriptions");
			ds.Tables["Subscriptions"].Columns.Add("Name");
            if(File.Exists(API_Utlis.PathFix(path + "\\imap.xml"))){
			    ds.ReadXml(API_Utlis.PathFix(path + "\\imap.xml"));
            }

			// ToDo: check if already exists, raise error or just skip ??

			DataRow dr = ds.Tables["Subscriptions"].NewRow();
			dr["Name"] = folder;
			ds.Tables["Subscriptions"].Rows.Add(dr);

			ds.WriteXml(API_Utlis.PathFix(path + "\\imap.xml"));
		}
		
		#endregion

		#region method UnSubscribeFolder

		/// <summary>
		/// UnSubscribes IMAP folder.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="folder"></param>
		public void UnSubscribeFolder(string userName,string folder)
		{
			DataSet ds = new DataSet();			
			ds.ReadXml(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName + "\\imap.xml"));

            foreach(DataRow dr in ds.Tables["Subscriptions"].Rows){
                if(dr["Name"].ToString().ToLower() == folder.ToLower()){
                    dr.Delete();
                    break;
                }
            }

			ds.WriteXml(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName + "\\imap.xml"));
		}
		
		#endregion


        #region method SharedFolderRootExists

        /// <summary>
        /// Gets if specified shared root folder exists. Returns true, if root folder exists.
        /// </summary>
        /// <param name="rootFolder">Root folder name.</param>
        /// <returns>Returns true, if root folder exists.</returns>
        public bool SharedFolderRootExists(string rootFolder)
        {
            return GetSharedFolderRoot(rootFolder) != null;
        }

        #endregion

        #region method GetSharedFolderRoots

        /// <summary>
        /// Gets shared folder root folders.
        /// </summary>
        /// <returns></returns>
        public SharedFolderRoot[] GetSharedFolderRoots()
        {
            m_UpdSync.AddMethod();

			try{
                List<SharedFolderRoot> roots = new List<SharedFolderRoot>();
			    foreach(DataRow dr in dsSharedFolderRoots.Tables["SharedFoldersRoots"].Rows){
                    roots.Add(new SharedFolderRoot(
                        dr["RootID"].ToString(),
                        Convert.ToBoolean(dr["Enabled"]),
                        dr["Folder"].ToString(),
                        dr["Description"].ToString(),
                        (SharedFolderRootType_enum)Convert.ToInt32(dr["RootType"]),
                        dr["BoundedUser"].ToString(),
                        dr["BoundedFolder"].ToString()
                    ));
                }

                return roots.ToArray();				
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
        }

        #endregion

        #region method AddSharedFolderRoot

        /// <summary>
        /// Add shared folder root.
        /// </summary>
        /// <param name="rootID">Root folder ID. Guid.NewID().ToString() is suggested.</param>
        /// <param name="enabled">Specifies if root folder is enabled.</param>
        /// <param name="folder">Folder name which will be visible to public.</param>
        /// <param name="description">Description text.</param>
        /// <param name="rootType">Specifies what type root folder is.</param>
        /// <param name="boundedUser">User which to bound root folder.</param>
        /// <param name="boundedFolder">Folder which to bound to public folder.</param>
        public void AddSharedFolderRoot(string rootID,bool enabled,string folder,string description,SharedFolderRootType_enum rootType,string boundedUser,string boundedFolder)
        {
            if(rootID == null || rootID == ""){
                throw new Exception("Invalid rootID value, rootID can't be '' or null !");
            }

            //--- Validate values -------------------------------------//
            ArgsValidator.ValidateNotNull(rootID);
            ArgsValidator.ValidateSharedFolderRoot(folder);
            ArgsValidator.ValidateNotNull(description);
            if(rootType == SharedFolderRootType_enum.BoundedRootFolder){
                ArgsValidator.ValidateUserName(boundedUser);
                ArgsValidator.ValidateFolder(boundedFolder);
            }
            //---------------------------------------------------------//
                        
            m_UpdSync.BeginUpdate();

			try{
                /* Implementation notes:
                    *) Validate values. Throw ArgumnetExcetion if invalid values.
                    *) Ensure that root ID doesn't exists.
                    *) Ensure that root doesn't exists.
                    *) Add root folder.
                */

                // Ensure that root ID doesn't exists.
                if(ContainsID(dsSharedFolderRoots.Tables["SharedFoldersRoots"],"RootID",rootID)){
                    throw new Exception("Invalid root ID, specified root ID '" + rootID + "' already exists !");
                }

                // Ensure that folder doesn't exists.
                if(SharedFolderRootExists(folder)){
                    throw new ArgumentException("Invalid root folder value, root folder '" + folder + "' already exists !");
                }

			
				DataRow dr = dsSharedFolderRoots.Tables["SharedFoldersRoots"].NewRow();
				dr["RootID"]        = rootID;
				dr["Enabled"]       = enabled;
				dr["Folder"]        = folder;
                dr["Description"]   = description;
				dr["RootType"]      = (int)rootType;
				dr["BoundedUser"]   = boundedUser;
                dr["BoundedFolder"] = boundedFolder;
							
				dsSharedFolderRoots.Tables["SharedFoldersRoots"].Rows.Add(dr);
				dsSharedFolderRoots.WriteXml(m_DataPath + "SharedFoldersRoots.xml",XmlWriteMode.IgnoreSchema);
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method DeleteSharedFolderRoot

        /// <summary>
        /// Deletes shard folders root folder.
        /// </summary>
        /// <param name="rootID">Root folder ID which to delete.</param>
        public void DeleteSharedFolderRoot(string rootID)
        {
            //--- Validate values -------------------------------------//
            if(rootID == null || rootID == ""){
                throw new Exception("Invalid rootID value, rootID can't be '' or null !");
            }
            //---------------------------------------------------------//

            m_UpdSync.BeginUpdate();

			try{
                /* Implementation notes:
                    *) Validate values. Throw ArgumnetExcetion if invalid values.
                    *) Ensure that root ID exists.
                    *) Delete root folder.
                */

                // Ensure that group with specified ID does exist.
                if(!ContainsID(dsSharedFolderRoots.Tables["SharedFoldersRoots"],"RootID",rootID)){
                    throw new Exception("Invalid root ID, specified root ID '" + rootID + "' doesn't exist !");
                }

                // Delete specified root
                foreach(DataRow dr in dsSharedFolderRoots.Tables["SharedFoldersRoots"].Rows){
                    if(dr["RootID"].ToString().ToLower() == rootID){
                        dr.Delete();
                        dsSharedFolderRoots.WriteXml(m_DataPath + "SharedFoldersRoots.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method UpdateSharedFolderRoot

        /// <summary>
        /// Updates shared folder root.
        /// </summary>
        /// <param name="rootID">Root Folder which to update.</param>
        /// <param name="enabled">Specifies if root folder is enabled.</param>
        /// <param name="folder">Folder name which will be visible to public.</param>
        /// <param name="description">Description text.</param>
        /// <param name="rootType">Specifies what type root folder is.</param>
        /// <param name="boundedUser">User which to bound root folder.</param>
        /// <param name="boundedFolder">Folder which to bound to public folder.</param>
        public void UpdateSharedFolderRoot(string rootID,bool enabled,string folder,string description,SharedFolderRootType_enum rootType,string boundedUser,string boundedFolder)
        {
            if(rootID == null || rootID == ""){
                throw new Exception("Invalid rootID value, rootID can't be '' or null !");
            }

            //--- Validate values -------------------------------------//
            ArgsValidator.ValidateNotNull(rootID);
            ArgsValidator.ValidateSharedFolderRoot(folder);
            ArgsValidator.ValidateNotNull(description);
            if(rootType == SharedFolderRootType_enum.BoundedRootFolder){
                ArgsValidator.ValidateUserName(boundedUser);
                ArgsValidator.ValidateFolder(boundedFolder);
            }
            //---------------------------------------------------------//

            m_UpdSync.BeginUpdate();

			try{
                /* Implementation notes:
                    *) Validate values. Throw ArgumnetExcetion if invalid values.
                    *) Ensure that root ID exists.
                    *) If root folder name is changed, ensure that new root folder won't conflict 
                       any other root folder name. Throw Exception if does.     
                    *) Update root folder.
                */

                // Ensure that group with specified ID does exist.
                if(!ContainsID(dsSharedFolderRoots.Tables["SharedFoldersRoots"],"RootID",rootID)){
                    throw new Exception("Invalid root ID, specified root ID '" + rootID + "' doesn't exist !");
                }

                //--- Update root folder
                foreach(DataRow dr in dsSharedFolderRoots.Tables["SharedFoldersRoots"].Rows){
                    if(dr["RootID"].ToString().ToLower() == rootID){
                        // If root folder name is changed, ensure that new root folder won't conflict 
                        //   any other root folder name. Throw Exception if does.
                        if(dr["Folder"].ToString().ToLower() != folder.ToLower()){
                            if(SharedFolderRootExists(folder)){
                                throw new Exception("Invalid root folder name, specified root folder '" + folder + "' already exists !");
                            }
                        }

                     // dr["RootID"]        = rootID;
					    dr["Enabled"]       = enabled;
					    dr["Folder"]        = folder;
                        dr["Description"]   = description;
					    dr["RootType"]      = (int)rootType;
					    dr["BoundedUser"]   = boundedUser;
                        dr["BoundedFolder"] = boundedFolder;

                        dsSharedFolderRoots.WriteXml(m_DataPath + "SharedFoldersRoots.xml",XmlWriteMode.IgnoreSchema);
                        break;
                    }
                }
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion


        #region method GetFolderACL

        /// <summary>
		/// Gets specified folder ACL.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what ACL to get. For example: Inbox,Public Folders/Documnets .</param>
		/// <returns></returns>
		public DataView GetFolderACL(string accessingUser,string folderOwnerUser,string folder)
		{
			m_UpdSync.AddMethod();

			try{
                /* Implementation notes:
                    *) Validate values. Throw ArgumnetExcetion if invalid values.
                    *) Ensure that user exists.
                    *) Normalize folder. Remove '/' from folder start and end, ... .
                    *) Do Shared Folders mapping.
                    *) Ensure that folder exists. Throw Exception if don't.
                    *) See if user has sufficient permissions. User requires 'a' permission.
                        There is builtin user system, skip ACL for it.
                    *) Get folder ACL.
                */

                //--- Validate values -------------------//
                ArgsValidator.ValidateUserName(folderOwnerUser);
                ArgsValidator.ValidateFolder(folder);
                //---------------------------------------//

                // Ensure that user exists.
                if(!UserExists(folderOwnerUser)){
                    throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
                }

                // Normalize folder. Remove '/' from folder start and end.
                folder = API_Utlis.NormalizeFolder(folder);

                // Do Shared Folders mapping.
                string originalFolder = folder;
                SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
                if(mappedFolder.IsSharedFolder){
                    folderOwnerUser = mappedFolder.FolderOnwer;
                    folder = mappedFolder.Folder;

                    if(folderOwnerUser == "" || folder == ""){
                        throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                    }
                }

                // Ensure that folder doesn't exists. Throw Exception if don't.
                if(!FolderExists(folderOwnerUser + "/" + folder)){
                    throw new Exception("Folder '" + folder + "' doesn't exist !");
                }

                // See if user has sufficient permissions. User requires 'a' permission.
                //  There is builtin user system, skip ACL for it.
                if(accessingUser.ToLower() != "system"){
                    IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                    if((acl & IMAP_ACL_Flags.a) == 0){
                        throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                    }
                }

                //--- Get folder ACL
				DataView dv = new DataView(dsImapACL.Copy().Tables["ACL"]);
				if(folder != ""){
					dv.RowFilter = "Folder='" + folderOwnerUser + "/" + folder + "'";
				}

				return dv;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}

		#endregion

		#region method DeleteFolderACL

		/// <summary>
		/// Deletes specified folder ACL for specified user.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what ACL to delete. For example: Inbox,Public Folders/Documnets .</param>
        /// <param name="userOrGroup">User or user group which ACL on specified folder to delete.</param>
		public void DeleteFolderACL(string accessingUser,string folderOwnerUser,string folder,string userOrGroup)
		{
			m_UpdSync.BeginUpdate();

			try{
                /* Implementation notes:
                    *) Validate values. Throw ArgumnetExcetion if invalid values.
                    *) Ensure that folder owner user exists.
                    *) Ensure that user or user group exists.
                    *) Normalize folder. Remove '/' from folder start and end, ... .
                    *) Do Shared Folders mapping.
                    *) Ensure that folder exists. Throw Exception if don't.
                    *) See if user has sufficient permissions. User requires 'a' permission.
                       There is builtin user system, skip ACL for it.
                    *) Delete folder.
                */

                //--- Validate values -------------------//
                ArgsValidator.ValidateUserName(folderOwnerUser);
                ArgsValidator.ValidateFolder(folder);
                ArgsValidator.ValidateUserName(userOrGroup);
                //---------------------------------------//

                // Ensure that folder owner user exists.
                if(!UserExists(folderOwnerUser)){
                    throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
                }

                // Ensure that user or user group exists. Skip check for anyone.
                if(userOrGroup.ToLower() != "anyone" && !GroupExists(userOrGroup)){
                    if(!UserExists(userOrGroup)){
                        throw new Exception("Invalid userOrGroup value, there is no such user or group '" + userOrGroup + "' !");
                    }
                }

                // Normalize folder. Remove '/' from folder start and end.
                folder = API_Utlis.NormalizeFolder(folder);

                // Do Shared Folders mapping.
                string originalFolder = folder;
                SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
                if(mappedFolder.IsSharedFolder){
                    folderOwnerUser = mappedFolder.FolderOnwer;
                    folder = mappedFolder.Folder;

                    if(folderOwnerUser == "" || folder == ""){
                        throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                    }
                }

                // Ensure that folder doesn't exists. Throw Exception if don't.
                if(!FolderExists(folderOwnerUser + "/" + folder)){
                    throw new Exception("Folder '" + folder + "' doesn't exist !");
                }

                // See if user has sufficient permissions. User requires 'a' permission.
                //  There is builtin user system, skip ACL for it.
                if(accessingUser.ToLower() != "system"){
                    IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                    if((acl & IMAP_ACL_Flags.a) == 0){
                        throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                    }
                }

                //--- Delete folder ACL
				using(DataView dv = new DataView(dsImapACL.Tables["ACL"])){
					dv.RowFilter = "Folder='" + folderOwnerUser + "/" + folder + "' AND User='" + userOrGroup + "'";

					if(dv.Count > 0){
						dv[0].Delete();
					}

					dsImapACL.WriteXml(m_DataPath + "IMAP_ACL.xml",XmlWriteMode.IgnoreSchema);
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}

		#endregion

		#region method SetFolderACL

		/// <summary>
		/// Sets specified folder ACL for specified user.
		/// </summary>
        /// <param name="accessingUser">User who accesses this method. 
        /// User needs r permission to call this method or Exception is thrown. 
        /// There is special user 'system' for which permission check is skipped.</param>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
        /// <param name="folder">Folder what ACL to set. For example: Inbox,Public Folders/Documnets .</param>
        /// <param name="userOrGroup">>User or user which group ACL set to specified folder.</param>
		/// <param name="setType">Specifies how ACL flags must be stored (ADD,REMOVE,REPLACE).</param>
		/// <param name="aclFlags">ACL flags.</param>
		public void SetFolderACL(string accessingUser,string folderOwnerUser,string folder,string userOrGroup,IMAP_Flags_SetType setType,IMAP_ACL_Flags aclFlags)
		{
			m_UpdSync.BeginUpdate();

			try{
			    /* Implementation notes:
                    *) Validate values. Throw ArgumnetExcetion if invalid values.
                    *) Ensure that folder owner user exists.
                    *) Ensure that user or user group exists.
                    *) Normalize folder. Remove '/' from folder start and end, ... .
                    *) Do Shared Folders mapping.
                    *) Ensure that folder exists. Throw Exception if don't.
                    *) See if user has sufficient permissions. User requires 'a' permission.
                        There is builtin user system, skip ACL for it.
                    *) Set folder ACL folder.
                */

                //--- Validate values -------------------//
                ArgsValidator.ValidateUserName(folderOwnerUser);
                ArgsValidator.ValidateFolder(folder);
                ArgsValidator.ValidateUserName(userOrGroup);
                //---------------------------------------//

                // Ensure that folder owner user exists.
                if(!UserExists(folderOwnerUser)){
                    throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
                }

                // Ensure that user or user group exists. Skip check for anyone.
                if(userOrGroup.ToLower() != "anyone" && !GroupExists(userOrGroup)){
                    if(!UserExists(userOrGroup)){
                        throw new Exception("Invalid userOrGroup value, there is no such user or group '" + userOrGroup + "' !");
                    }
                }

                // Normalize folder. Remove '/' from folder start and end.
                folder = API_Utlis.NormalizeFolder(folder);

                // Do Shared Folders mapping.
                string originalFolder = folder;
                SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
                if(mappedFolder.IsSharedFolder){
                    folderOwnerUser = mappedFolder.FolderOnwer;
                    folder = mappedFolder.Folder;

                    if(folderOwnerUser == "" || folder == ""){
                        throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                    }
                }

                // Ensure that folder doesn't exists. Throw Exception if don't.
                if(!FolderExists(folderOwnerUser + "/" + folder)){
                    throw new Exception("Folder '" + folder + "' doesn't exist !");
                }

                // See if user has sufficient permissions. User requires 'a' permission.
                //  There is builtin user system, skip ACL for it.
                if(accessingUser.ToLower() != "system"){
                    IMAP_ACL_Flags acl = GetUserACL(folderOwnerUser,folder,accessingUser);
                    if((acl & IMAP_ACL_Flags.a) == 0){
                        throw new InsufficientPermissionsException("Insufficient permissions for folder '" + accessingUser + "/" + folder + "' !");
                    }
                }

                //--- Set folder ACL
				using(DataView dv = new DataView(dsImapACL.Tables["ACL"])){
					dv.RowFilter = "Folder='" + folderOwnerUser + "/" + folder + "' AND User='" + userOrGroup + "'";
					
					// ACL entry for specified user doesn't exists, add it
					if(dv.Count == 0){
						DataRow dr = dv.Table.NewRow();
						dr["Folder"]      = folderOwnerUser + "/" + folder;
						dr["User"]        = userOrGroup;
						dr["Permissions"] = IMAP_Utils.ACL_to_String(aclFlags);
						dv.Table.Rows.Add(dr);
					}
					else{
						IMAP_ACL_Flags currentACL_Flags = IMAP_Utils.ACL_From_String(dv[0]["Permissions"].ToString());

						if(setType == IMAP_Flags_SetType.Replace){
							dv[0]["Permissions"] = IMAP_Utils.ACL_to_String(aclFlags);
						}
						else if(setType == IMAP_Flags_SetType.Add){
							currentACL_Flags |= aclFlags;

							dv[0]["Permissions"] = IMAP_Utils.ACL_to_String(currentACL_Flags);
						}
						else if(setType == IMAP_Flags_SetType.Remove){
							currentACL_Flags &= ~aclFlags;

							dv[0]["Permissions"] = IMAP_Utils.ACL_to_String(currentACL_Flags);
						}
					}

					dsImapACL.WriteXml(m_DataPath + "IMAP_ACL.xml",XmlWriteMode.IgnoreSchema);
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}

		#endregion

		#region method GetUserACL

		/// <summary>
		/// Gets what permissions specified user has to specified folder.
		/// </summary>
        /// <param name="folderOwnerUser">User who's folder it is.</param>
		/// <param name="folder">Folder which ACL to get. For example Inbox,Public Folders.</param>
		/// <param name="user">User name which ACL to get.</param>
		/// <returns></returns>
		public IMAP_ACL_Flags GetUserACL(string folderOwnerUser,string folder,string user)
		{
            //--- Validate values -------------------//
            ArgsValidator.ValidateUserName(folderOwnerUser);
            ArgsValidator.ValidateFolder(folder);
            ArgsValidator.ValidateUserName(user);
            //---------------------------------------//

			m_UpdSync.AddMethod();
                        
			try{
                /*
                    *) Ensure that folder owner user exists.
                    *) Ensure that user exists.
                    *) Normalize folder. Remove '/' from folder start and end, ... .
                    *) Do Shared Folders mapping.
                // ??    *) Ensure that folder exists. Throw Exception if don't.
                 
                    If folder owner is user, and no permissions explicity set, then user have ALL permissions.
                    If user isn't folder owner:
                        *) Try to get User ACl. Also look user groups.
                           If doesn't exist, try to anyone ACL.                           
                           If anyone ACL doesn't exist, try to inherit ACL from parent folders.
                        
                        NOTE: Get maximum ACL user has. For example if user has explicity ACL set and
                              and has Group ACL set, then ACL = max combination of ACL falgs.
                */

                // Ensure that folder owner user exists.
                if(!UserExists(folderOwnerUser)){
                    throw new Exception("User '" + folderOwnerUser + "' doesn't exist !");
                }

                // Ensure that folder user exists.
                if(!UserExists(user)){
                    throw new Exception("User '" + user + "' doesn't exist !");
                }

                // Normalize folder. Remove '/' from folder start and end.
                folder = API_Utlis.NormalizeFolder(folder);

                // Do Shared Folders mapping.
                string originalFolder = folder;
                SharedFolderMapInfo mappedFolder = MapSharedFolder(originalFolder);
                if(mappedFolder.IsSharedFolder){
                    folderOwnerUser = mappedFolder.FolderOnwer;
                    folder = mappedFolder.Folder;

                    if(folderOwnerUser == "" || folder == ""){
                        throw new ArgumentException("Specified root folder '" + originalFolder + "' isn't accessible !");
                    }
                }

                IMAP_ACL_Flags userACL = IMAP_ACL_Flags.None;
                
                // See if ACL is set to this folder, if not show inhereted ACL
                DataView dv = null;
                try{
                    dv = GetFolderACL("system",folderOwnerUser,folder);
                }
                // Folder doesnt exist, just skip it
                catch{
                }                
                if(dv != null && dv.Count > 0){
                    bool aclSetToUser = false;
                    foreach(DataRowView drV in dv){
                        // This is group, user is member of that group
                        if(GroupExists(drV["User"].ToString()) && IsUserGroupMember(drV["User"].ToString(),user)){
                            userACL |= IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
                        } 
                        // ACL is explicity set to user
                        else if(drV["User"].ToString().ToLower() == user.ToLower()){
						    userACL = IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
                            aclSetToUser = true;
						}
                        // There is ANYONE access
						else if(drV["User"].ToString().ToLower() == "anyone"){
						    userACL = IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
						}
                    }

                    // ACL isn't explicity set to folder owner user,give all permissions to folder owner user.
                    if(!aclSetToUser && user.ToLower() == folderOwnerUser.ToLower()){
                        userACL = IMAP_ACL_Flags.All;
                    }
                }
                else{
                    // ACL isn't set and user owner, give full rights
                    if(user.ToLower() == folderOwnerUser.ToLower()){
                        userACL = IMAP_ACL_Flags.All;
                    }
                    else{
                        // Try to inherit ACL from parent folder(s)
                        // Move right to left in path.
                        while(folder.LastIndexOf('/') > -1){
                            // Move 1 level to right in path
                            folder = folder.Substring(0,folder.LastIndexOf('/'));

                            dv = GetFolderACL("system",folderOwnerUser,folder);
                            if(dv.Count > 0){                                
                                foreach(DataRowView drV in dv){
                                    string userOrGroup = drV["User"].ToString();
                                    // This is group, user is member of that group
                                    if(GroupExists(userOrGroup) && IsUserGroupMember(drV["User"].ToString(),user)){
                                        userACL |= IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
                                    }
                                    // ACL is explicity set to user
                                    else if(drV["User"].ToString().ToLower() == user.ToLower()){
							            userACL |= IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
						            }
                                    // There is ANYONE access
						            else if(drV["User"].ToString().ToLower() == "anyone"){
							            userACL |= IMAP_Utils.ACL_From_String(drV["Permissions"].ToString());
						            }
                                }

                                // We inhereted all permission, don't look other parent folders anymore
                                break;
                            }
                        }
                    }
                }

				return userACL;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}

		#endregion


        #region method CreateUserDefaultFolders

        /// <summary>
        /// Creates specified user default folders, if they don't exist already.
        /// </summary>
        /// <param name="userName">User name to who's default folders to create.</param>
        public void CreateUserDefaultFolders(string userName)
        {
            foreach(DataRowView drV in GetUsersDefaultFolders()){
                if(!FolderExists(userName + "/" + drV["FolderName"].ToString())){
                    CreateFolder("system",userName,drV["FolderName"].ToString());
                    SubscribeFolder(userName,drV["FolderName"].ToString());
                }
            }
        }

        #endregion

        #region method GetUsersDefaultFolders

        /// <summary>
        /// Gets users default folders.
        /// </summary>
        /// <returns></returns>
        public DataView GetUsersDefaultFolders()
        {   
            m_UpdSync.AddMethod();

			try{
                // Force Inbox to be created
                if(dsUsersDefaultFolders.Tables["UsersDefaultFolders"].Rows.Count == 0){
                    DataRow dr = dsUsersDefaultFolders.Tables["UsersDefaultFolders"].NewRow();
				    dr["FolderName"] = "Inbox";
				    dr["Permanent"]  = true;									
				    dsUsersDefaultFolders.Tables["UsersDefaultFolders"].Rows.Add(dr);
                }

				return new DataView(dsUsersDefaultFolders.Copy().Tables["UsersDefaultFolders"]);
			}
			catch(Exception x){				
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
        }

        #endregion

        #region method AddUsersDefaultFolder

        /// <summary>
        /// Adds users default folder.
        /// </summary>
        /// <param name="folderName">Folder name.</param>
        /// <param name="permanent">Spcifies if folder is permanent, user can't delete it.</param>
        public void AddUsersDefaultFolder(string folderName,bool permanent)
        {
            ArgsValidator.ValidateFolder(folderName);

			m_UpdSync.BeginUpdate();

			try{
                // Ensure that users default folder with specified name doesn't exist already
				foreach(DataRow drFolder in dsUsersDefaultFolders.Tables["UsersDefaultFolders"].Rows){
                    if(drFolder["FolderName"].ToString().ToLower() == folderName.ToLower()){
                        throw new Exception("Users default folder with specified name '" + folderName + "' already exists !");
                    }
                }
                                
                DataRow dr = dsUsersDefaultFolders.Tables["UsersDefaultFolders"].NewRow();
				dr["FolderName"] = folderName;
				dr["Permanent"]  = permanent;									
				dsUsersDefaultFolders.Tables["UsersDefaultFolders"].Rows.Add(dr);
				dsUsersDefaultFolders.WriteXml(m_DataPath + "UsersDefaultFolders.xml",XmlWriteMode.IgnoreSchema);
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion

        #region method DeleteUsersDefaultFolder

        /// <summary>
        /// Deletes specified users default folder.
        /// </summary>
        /// <param name="folderName">Folder name.</param>
        public void DeleteUsersDefaultFolder(string folderName)
        {
            ArgsValidator.ValidateFolder(folderName);

			m_UpdSync.BeginUpdate();

			try{                
				foreach(DataRow drFolder in dsUsersDefaultFolders.Tables["UsersDefaultFolders"].Rows){
                    if(drFolder["FolderName"].ToString().ToLower() == folderName.ToLower()){
                        drFolder.Delete();									
				        dsUsersDefaultFolders.WriteXml(m_DataPath + "UsersDefaultFolders.xml",XmlWriteMode.IgnoreSchema);
                        return;
                    }
                }

                throw new Exception("Users default folder with specified name '" + folderName + "' doesn't exists !");
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
        }

        #endregion


		#region method GetMailboxSize

		/// <summary>
		/// Gets specified user mailbox size.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <returns>Returns mailbox size.</returns>
		public long GetMailboxSize(string userName)
		{
			try{
				using(FileStream fs = File.OpenRead(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName + "\\_mailbox_size"))){
					byte[] sizeByte = new byte[fs.Length];
					fs.Read(sizeByte,0,sizeByte.Length);
					long sizeCurrent = Convert.ToInt64(System.Text.Encoding.ASCII.GetString(sizeByte).Trim());

                    if(sizeCurrent < 0){
                        sizeCurrent = 0;
                    }

					return sizeCurrent;
				}
			}
			catch(IOException x){
                // This suppresses compile warning
                string dummy = x.Message;

				// This exception happens when mailbox size file doesn't exist, just create it
				ChangeMailboxSize(userName,0);
			}

			return 0;
		}

		#endregion


        #region method GetRecycleBinSettings

        /// <summary>
        /// Gets recycle bin settings.
        /// </summary>
        /// <returns></returns>
        public DataTable GetRecycleBinSettings()
        {
            return dsRecycleBinSettings.Tables["RecycleBinSettings"];
        }

        #endregion

        #region method UpdateRecycleBinSettings

        /// <summary>
        /// Updates recycle bin settings.
        /// </summary>
        /// <param name="deleteToRecycleBin">Specifies if deleted messages are store to recycle bin.</param>
        /// <param name="deleteMessagesAfter">Specifies how old messages will be deleted.</param>
        public void UpdateRecycleBinSettings(bool deleteToRecycleBin,int deleteMessagesAfter)
        {
            dsRecycleBinSettings.Tables["RecycleBinSettings"].Rows[0]["DeleteToRecycleBin"]  = deleteToRecycleBin;
            dsRecycleBinSettings.Tables["RecycleBinSettings"].Rows[0]["DeleteMessagesAfter"] = deleteMessagesAfter;
            dsRecycleBinSettings.WriteXml(m_DataPath + "RecycleBinSettings.xml");
        }

        #endregion

        #region method GetRecycleBinMessagesInfo

        /// <summary>
        /// Gets recycle bin messages info. 
        /// </summary>
        /// <param name="user">User who's recyclebin messages to get or null if all users messages.</param>
        /// <param name="startDate">Messages from specified date. Pass DateTime.MinValue if not used.</param>
        /// <param name="endDate">Messages to specified date. Pass DateTime.MinValue if not used.</param>
        /// <returns></returns>
        public DataView GetRecycleBinMessagesInfo(string user,DateTime startDate,DateTime endDate)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add("MessagesInfo");
            ds.Tables["MessagesInfo"].Columns.Add("MessageID");
            ds.Tables["MessagesInfo"].Columns.Add("DeleteTime",typeof(DateTime));
            ds.Tables["MessagesInfo"].Columns.Add("User");
            ds.Tables["MessagesInfo"].Columns.Add("Folder");
            ds.Tables["MessagesInfo"].Columns.Add("Size");
            ds.Tables["MessagesInfo"].Columns.Add("Envelope");

            foreach(RecycleBinMessageInfo info in RecycleBinManager.GetMessagesInfo(user,startDate,endDate)){
                DataRow dr = ds.Tables["MessagesInfo"].NewRow();
                dr["MessageID"]    = info.MessageID;
                dr["DeleteTime"]   = info.DeleteTime;
                dr["User"]         = info.User;
                dr["Folder"]       = info.Folder;
                dr["Size"]         = info.Size;
                dr["Envelope"]     = info.Envelope;
                ds.Tables["MessagesInfo"].Rows.Add(dr);
            }

            return ds.Tables["MessagesInfo"].DefaultView;
        }
				
		#endregion

        #region method GetRecycleBinMessages

        /// <summary>
        /// Gets recycle bin message stream. NOTE: This method caller must take care of closing stream. 
        /// </summary>
        /// <param name="messageID">Message ID if of message what to get.</param>
        /// <returns></returns>
        public Stream GetRecycleBinMessage(string messageID)
        {
            return RecycleBinManager.GetRecycleBinMessage(messageID);
        }
				
		#endregion

        #region method DeleteRecycleBinMessage

        /// <summary>
        /// Deletes specified recycle bin message.
        /// </summary>
        /// <param name="messageID">Message ID.</param>
        public void DeleteRecycleBinMessage(string messageID)
        {
            RecycleBinManager.DeleteRecycleBinMessage(messageID);
        }

        #endregion

        #region method RestoreRecycleBinMessage

        /// <summary>
        /// Restores specified recycle bin message.
        /// </summary>
        /// <param name="messageID">Message ID.</param>
        public void RestoreRecycleBinMessage(string messageID)
        {
            RecycleBinManager.RestoreFromRecycleBin(messageID,this);
        }

        #endregion

		#endregion

	
		#region Security related

		#region method GetSecurityList

		/// <summary>
		/// Gets security entries list.
		/// </summary>
		public DataView GetSecurityList()
		{	
			m_UpdSync.AddMethod();

			try{
				return new DataView(dsSecurity.Copy().Tables["IPSecurity"]);
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}
		
		#endregion


		#region method AddSecurityEntry

		/// <summary>
		/// Adds new IP security entry.
		/// </summary>
		/// <param name="id">IP security entry ID.</param>
		/// <param name="enabled">Specifies if IP security entry is enabled.</param>
		/// <param name="description">IP security entry description text.</param>
		/// <param name="service">Specifies service for what security entry applies.</param>
		/// <param name="action">Specifies what action done if IP matches to security entry range.</param>
		/// <param name="startIP">Range start IP.</param>
		/// <param name="endIP">Range end IP.</param>
		public void AddSecurityEntry(string id,bool enabled,string description,Service_enum service,IPSecurityAction_enum action,IPAddress startIP,IPAddress endIP)
		{			
			if(id.Length == 0){
				throw new Exception("You must specify id");
			}

			m_UpdSync.BeginUpdate();
            
			try{
				if(!ContainsID(dsSecurity.Tables["IPSecurity"],"ID",id)){
					DataRow dr = dsSecurity.Tables["IPSecurity"].NewRow();
					dr["ID"]          = id;                    
					dr["Enabled"]     = enabled;
					dr["Description"] = description;
					dr["Service"]     = service;
					dr["Action"]      = action;
					dr["StartIP"]     = startIP.ToString();
					dr["EndIP"]       = endIP.ToString();

					dsSecurity.Tables["IPSecurity"].Rows.Add(dr);
					dsSecurity.WriteXml(m_DataPath + "IPSecurity.xml",XmlWriteMode.IgnoreSchema);
				}
				else{
					throw new Exception("Security entry with specified ID '" + id + "' already exists !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region method DeleteSecurityEntry

		/// <summary>
		/// Deletes security entry.
		/// </summary>
		/// <param name="id">IP security entry ID.</param>
		public void DeleteSecurityEntry(string id)
		{
			m_UpdSync.BeginUpdate();

			try{
                // Delete specified rule
                foreach(DataRow dr in dsSecurity.Tables["IPSecurity"].Rows){
                    if(dr["ID"].ToString().ToLower() == id){
                        dr.Delete();
                        dsSecurity.WriteXml(m_DataPath + "IPSecurity.xml",XmlWriteMode.IgnoreSchema);
                        return;
                    }
                }

                throw new Exception("Security entry with specified ID '" + id + "' doesn't exists !");
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region method UpdateSecurityEntry

		/// <summary>
		/// Updates IP security entry.
		/// </summary>
		/// <param name="id">IP security entry ID.</param>
		/// <param name="enabled">Specifies if IP security entry is enabled.</param>
		/// <param name="description">IP security entry description text.</param>
		/// <param name="service">Specifies service for what security entry applies.</param>
		/// <param name="action">Specifies what action done if IP matches to security entry range.</param>
		/// <param name="startIP">Range start IP.</param>
		/// <param name="endIP">Range end IP.</param>
		public void UpdateSecurityEntry(string id,bool enabled,string description,Service_enum service,IPSecurityAction_enum action,IPAddress startIP,IPAddress endIP)
		{
			m_UpdSync.BeginUpdate();

			try{                
                foreach(DataRow dr in dsSecurity.Tables["IPSecurity"].Rows){
                    if(dr["ID"].ToString().ToLower() == id){
                        dr["ID"]          = id;                    
					    dr["Enabled"]     = enabled;
					    dr["Description"] = description;
					    dr["Service"]     = service;
					    dr["Action"]      = action;
					    dr["StartIP"]     = startIP.ToString();
					    dr["EndIP"]       = endIP.ToString();
                        
                        dsSecurity.WriteXml(m_DataPath + "IPSecurity.xml",XmlWriteMode.IgnoreSchema);
                        return;
                    }
                }

                throw new Exception("Security entry with specified ID '" + id + "' doesn't exist !");
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#endregion


		#region Filters related

		#region function GetFilters

		/// <summary>
		/// Gets filter list.
		/// </summary>
		/// <returns></returns>
		public DataView GetFilters()
		{	
			m_UpdSync.AddMethod();

			try{
				return new DataView(dsFilters.Copy().Tables["SmtpFilters"]);
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
		}
		
		#endregion


		#region function AddFilter

		/// <summary>
		/// Adds new filter.
		/// </summary>
		/// <param name="filterID">Filter ID. Suggested value is Guid.NewGuid() .</param>
		/// <param name="description">Filter description</param>
		/// <param name="type">Filter type. Eg. ISmtpMessageFilter.</param>
		/// <param name="assembly">Assembly with full location. Eg. C:\MailServer\Filters\filter.dll .</param>
		/// <param name="className">Filter full class name, wih namespace. Eg. LumiSoft.MailServer.Fileters.Filter1 .</param>
		/// <param name="cost">Filters are sorted by cost and proccessed with cost value. Smallest cost is proccessed first.</param>
		/// <param name="enabled">Specifies if filter is enabled.</param>
		/// <remarks>Throws exception if specified filter entry already exists.</remarks>
		public void AddFilter(string filterID,string description,string type,string assembly,string className,long cost,bool enabled)
		{			
			if(filterID.Length == 0){
				throw new Exception("You must specify filterID");
			}

			m_UpdSync.BeginUpdate();

			try{
				if(!ContainsID(dsFilters.Tables["SmtpFilters"],"FilterID",filterID)){
					DataRow dr = dsFilters.Tables["SmtpFilters"].NewRow();
					dr["FilterID"]    = filterID;
					dr["Description"] = description;
					dr["Type"]        = type;
					dr["Assembly"]    = assembly;
					dr["ClassName"]   = className;
					dr["Cost"]        = cost;
					dr["Enabled"]     = enabled;
							
					dsFilters.Tables["SmtpFilters"].Rows.Add(dr);
					dsFilters.WriteXml(m_DataPath + "Filters.xml",XmlWriteMode.IgnoreSchema);
				}
				else{
					throw new Exception("Filter with specified ID '" + filterID + "' already exists !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region function DeleteFilter

		/// <summary>
		/// Deletes specified filter.
		/// </summary>
		/// <param name="filterID">FilterID of the filter which to delete.</param>
		public void DeleteFilter(string filterID)
		{
			m_UpdSync.BeginUpdate();

			try{
				using(DataView dv = new DataView(dsFilters.Tables["SmtpFilters"])){
					dv.RowFilter = "FilterID='" + filterID + "'";

					if(dv.Count > 0){
						dv[0].Delete();
					}

					dsFilters.WriteXml(m_DataPath + "Filters.xml",XmlWriteMode.IgnoreSchema);
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#region function UpdateFilter

		/// <summary>
		/// Updates specified filter.
		/// </summary>		
		/// <param name="filterID">FilterID which to update.</param>
		/// <param name="description">Filter description</param>
		/// <param name="type">Filter type. Eg. ISmtpMessageFilter.</param>
		/// <param name="assembly">Assembly with full location. Eg. C:\MailServer\Filters\filter.dll .</param>
		/// <param name="className">Filter full class name, wih namespace. Eg. LumiSoft.MailServer.Fileters.Filter1 .</param>
		/// <param name="cost">Filters are sorted by cost and proccessed with cost value. Smallest cost is proccessed first.</param>
		/// <param name="enabled">Specifies if filter is enabled.</param>
		/// <returns></returns>
		public void UpdateFilter(string filterID,string description,string type,string assembly,string className,long cost,bool enabled)
		{
			m_UpdSync.BeginUpdate();

			try{
				if(ContainsID(dsFilters.Tables["SmtpFilters"],"FilterID",filterID)){
					using(DataView dv = new DataView(dsFilters.Tables["SmtpFilters"])){
						dv.RowFilter = "FilterID='" + filterID + "'";

						if(dv.Count > 0){
							dv[0]["Description"] = description;
							dv[0]["Type"]        = type;
							dv[0]["Assembly"]    = assembly;
							dv[0]["ClassName"]   = className;
							dv[0]["Cost"]        = cost;
							dv[0]["Enabled"]     = enabled;
						}

						dsFilters.WriteXml(m_DataPath + "Filters.xml",XmlWriteMode.IgnoreSchema);
					}
				}
				else{
					throw new Exception("Filter with specified ID '" + filterID + "' doesn't exist !");
				}
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.EndUpdate();
			}
		}
		
		#endregion

		#endregion


		#region Settings related

		#region method GetSettings

		/// <summary>
		/// Gets server settings.
		/// </summary>
		/// <returns></returns>
		public DataRow GetSettings()
		{
			DataSet ds = new DataSet();
			API_Utlis.CreateSettingsSchema(ds);

            if(File.Exists(m_DataPath + "Settings.xml")){
			    ds.ReadXml(m_DataPath + "Settings.xml");
            }
            else if(ds.Tables["Settings"].Rows.Count == 0){
                ds.Tables["Settings"].Rows.Add(ds.Tables["Settings"].NewRow());
            }
            
			foreach(DataRow dr in ds.Tables["Settings"].Rows){
				foreach(DataColumn dc in ds.Tables["Settings"].Columns){
					if(dr.IsNull(dc.ColumnName)){
						dr[dc.ColumnName] = dc.DefaultValue;
					}
				}
			}

            if(ds.Tables["SMTP_Bindings"].Rows.Count == 0){
                ds.Tables["SMTP_Bindings"].Rows.Add(ds.Tables["SMTP_Bindings"].NewRow());
            }
            foreach(DataRow dr in ds.Tables["SMTP_Bindings"].Rows){
				foreach(DataColumn dc in ds.Tables["SMTP_Bindings"].Columns){
					if(dr.IsNull(dc.ColumnName)){
						dr[dc.ColumnName] = dc.DefaultValue;
					}
				}
			}

            if(ds.Tables["POP3_Bindings"].Rows.Count == 0){
                ds.Tables["POP3_Bindings"].Rows.Add(ds.Tables["POP3_Bindings"].NewRow());
            }
            foreach(DataRow dr in ds.Tables["POP3_Bindings"].Rows){
				foreach(DataColumn dc in ds.Tables["POP3_Bindings"].Columns){
					if(dr.IsNull(dc.ColumnName)){
						dr[dc.ColumnName] = dc.DefaultValue;
					}
				}
			}

            if(ds.Tables["IMAP_Bindings"].Rows.Count == 0){
                ds.Tables["IMAP_Bindings"].Rows.Add(ds.Tables["IMAP_Bindings"].NewRow());
            }
            foreach(DataRow dr in ds.Tables["IMAP_Bindings"].Rows){
				foreach(DataColumn dc in ds.Tables["IMAP_Bindings"].Columns){
					if(dr.IsNull(dc.ColumnName)){
						dr[dc.ColumnName] = dc.DefaultValue;
					}
				}
			}

			return ds.Tables["Settings"].Rows[0];
		}

		#endregion

		#region method UpdateSettings

		/// <summary>
		/// Updates server settings.
		/// </summary>
		public void UpdateSettings(DataRow settings)
		{
            /*
			DataRow row = GetSettings();
			foreach(DataColumn dc in row.Table.Columns){
				if(settings.Table.Columns.Contains(dc.ColumnName)){
					row[dc.ColumnName] = settings[dc.ColumnName];
				}
			}*/

			settings.Table.DataSet.WriteXml(m_DataPath + "Settings.xml",XmlWriteMode.IgnoreSchema);
		}

		#endregion

		#endregion

            
    
		#region DB_Type.Xml helpers

		#region Load stuff

        #region method LoadDomains

        /// <summary>
		///  Loads domains from xml file.
		/// </summary>
		private void LoadDomains()
		{
            DateTime dateDomains = File.GetLastWriteTime(m_DataPath + "Domains.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateDomains,m_DomainsDate) == 0){
			    return;				
			}
            m_DomainsDate = dateDomains;
			dsDomains.Clear();

            #region Create Schema

            if(!dsDomains.Tables.Contains("Domains")){
				dsDomains.Tables.Add("Domains");
			}

			if(!dsDomains.Tables["Domains"].Columns.Contains("DomainID")){
				dsDomains.Tables["Domains"].Columns.Add("DomainID",Type.GetType("System.String"));
			}
			if(!dsDomains.Tables["Domains"].Columns.Contains("DomainName")){
				dsDomains.Tables["Domains"].Columns.Add("DomainName",Type.GetType("System.String"));
			}
			if(!dsDomains.Tables["Domains"].Columns.Contains("Description")){
				dsDomains.Tables["Domains"].Columns.Add("Description",Type.GetType("System.String"));
			}

            #endregion

            if(File.Exists(m_DataPath + "Domains.xml")){
			    dsDomains.ReadXml(m_DataPath + "Domains.xml");
            }
        }

        #endregion


        #region method LoadUsers

        /// <summary>
		/// Loads users from xml file.
		/// </summary>
		private void LoadUsers()
		{
            DateTime dateUsers = File.GetLastWriteTime(m_DataPath + "Users.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateUsers,m_UsersDate) == 0){
			    return;				
			}
            m_UsersDate = dateUsers;
			dsUsers.Clear();

            #region Create Schema

            if(!dsUsers.Tables.Contains("Users")){
				dsUsers.Tables.Add("Users");
			}

			if(!dsUsers.Tables["Users"].Columns.Contains("UserID")){
				dsUsers.Tables["Users"].Columns.Add("UserID",Type.GetType("System.String"));
			}
			if(!dsUsers.Tables["Users"].Columns.Contains("FullName")){
				dsUsers.Tables["Users"].Columns.Add("FullName",Type.GetType("System.String"));
			}
			if(!dsUsers.Tables["Users"].Columns.Contains("UserName")){
				dsUsers.Tables["Users"].Columns.Add("UserName",Type.GetType("System.String"));
			}
			if(!dsUsers.Tables["Users"].Columns.Contains("Password")){
				dsUsers.Tables["Users"].Columns.Add("Password",Type.GetType("System.String"));
			}
			if(!dsUsers.Tables["Users"].Columns.Contains("Description")){
				dsUsers.Tables["Users"].Columns.Add("Description",Type.GetType("System.String"));
			}
			if(!dsUsers.Tables["Users"].Columns.Contains("DomainName")){
				dsUsers.Tables["Users"].Columns.Add("DomainName",Type.GetType("System.String"));
			}
			if(!dsUsers.Tables["Users"].Columns.Contains("Mailbox_Size")){
				dsUsers.Tables["Users"].Columns.Add("Mailbox_Size",Type.GetType("System.Int64"));
				dsUsers.Tables["Users"].Columns["Mailbox_Size"].DefaultValue = 20;
				dsUsers.Tables["Users"].Columns["Mailbox_Size"].AllowDBNull = false;
			}
			if(!dsUsers.Tables["Users"].Columns.Contains("Enabled")){
				dsUsers.Tables["Users"].Columns.Add("Enabled",Type.GetType("System.Boolean"));
				dsUsers.Tables["Users"].Columns["Enabled"].DefaultValue = true;
				dsUsers.Tables["Users"].Columns["Enabled"].AllowDBNull = false;
			}
            if(!dsUsers.Tables["Users"].Columns.Contains("Permissions")){
				dsUsers.Tables["Users"].Columns.Add("Permissions",Type.GetType("System.Int32"));
				dsUsers.Tables["Users"].Columns["Permissions"].DefaultValue = 0xFFFF;
			}
			if(!dsUsers.Tables["Users"].Columns.Contains("CreationTime")){
				dsUsers.Tables["Users"].Columns.Add("CreationTime",Type.GetType("System.DateTime"));
                dsUsers.Tables["Users"].Columns["CreationTime"].DefaultValue = DateTime.Now;
			}

            #endregion

            if(File.Exists(m_DataPath + "Users.xml")){
			    dsUsers.ReadXml(m_DataPath + "Users.xml");
            }

            LoadDataSetDefaults(dsUsers);
        }

        #endregion

        #region method LoadUserAddresses

        /// <summary>
		/// Loads users from xml file.
		/// </summary>
		private void LoadUserAddresses()
		{
            DateTime dateUserAddresses = File.GetLastWriteTime(m_DataPath + "UserAddresses.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateUserAddresses,m_UserAddressesDate) == 0){
			    return;				
			}
            m_UserAddressesDate = dateUserAddresses;
			dsUserAddresses.Clear();

            #region Create Schema

            if(!dsUserAddresses.Tables.Contains("UserAddresses")){
				dsUserAddresses.Tables.Add("UserAddresses");
			}

			if(!dsUserAddresses.Tables["UserAddresses"].Columns.Contains("AddressID")){
				dsUserAddresses.Tables["UserAddresses"].Columns.Add("AddressID",Type.GetType("System.String"));
			}
			if(!dsUserAddresses.Tables["UserAddresses"].Columns.Contains("UserID")){
				dsUserAddresses.Tables["UserAddresses"].Columns.Add("UserID",Type.GetType("System.String"));
			}
			if(!dsUserAddresses.Tables["UserAddresses"].Columns.Contains("Address")){
				dsUserAddresses.Tables["UserAddresses"].Columns.Add("Address",Type.GetType("System.String"));
			}

            #endregion

            if(File.Exists(m_DataPath + "UserAddresses.xml")){
			    dsUserAddresses.ReadXml(m_DataPath + "UserAddresses.xml");
            }
        }

        #endregion

        #region method LoadUserRemoteServers

        /// <summary>
		/// Loads users from xml file.
		/// </summary>
		private void LoadUserRemoteServers()
		{
            DateTime dateUserRemoteServers = File.GetLastWriteTime(m_DataPath + "UserRemoteServers.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateUserRemoteServers,m_UsersRemoteServers) == 0){
			    return;				
			}
            m_UsersRemoteServers = dateUserRemoteServers;
			dsUserRemoteServers.Clear();

            #region Create Schema

            if(!dsUserRemoteServers.Tables.Contains("UserRemoteServers")){
				dsUserRemoteServers.Tables.Add("UserRemoteServers");
			}

			if(!dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Contains("ServerID")){
				dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Add("ServerID",Type.GetType("System.String"));
			}
			if(!dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Contains("UserID")){
				dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Add("UserID",Type.GetType("System.String"));
			}
			if(!dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Contains("Description")){
				dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Add("Description",Type.GetType("System.String"));
			}
			if(!dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Contains("RemoteServer")){
				dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Add("RemoteServer",Type.GetType("System.String"));
			}
			if(!dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Contains("RemotePort")){
				dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Add("RemotePort",Type.GetType("System.String"));
			}
			if(!dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Contains("RemoteUserName")){
				dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Add("RemoteUserName",Type.GetType("System.String"));
			}
			if(!dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Contains("RemotePassword")){
				dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Add("RemotePassword",Type.GetType("System.String"));
			}
			if(!dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Contains("UseSSL")){
				dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Add("UseSSL",Type.GetType("System.Boolean")).DefaultValue = false;
			}
			if(!dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Contains("Enabled")){
				dsUserRemoteServers.Tables["UserRemoteServers"].Columns.Add("Enabled",Type.GetType("System.Boolean")).DefaultValue = true;
			}

            #endregion

            if(File.Exists(m_DataPath + "UserRemoteServers.xml")){
			    dsUserRemoteServers.ReadXml(m_DataPath + "UserRemoteServers.xml");
            }

            LoadDataSetDefaults(dsUserRemoteServers);
        }

        #endregion

        #region method LoadUserMessageRules

        /// <summary>
		/// Loads message ruels from xml file.
		/// </summary>
		private void LoadUserMessageRules()
		{
            DateTime dateUserMessageRules = File.GetLastWriteTime(m_DataPath + "UserMessageRules.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateUserMessageRules,m_UserMessageRules) == 0){
			    return;				
			}
            m_UserMessageRules = dateUserMessageRules;
			dsUserMessageRules.Clear();

            #region Create Schema

            if(!dsUserMessageRules.Tables.Contains("UserMessageRules")){
			    dsUserMessageRules.Tables.Add("UserMessageRules");
            }
			
            if(!dsUserMessageRules.Tables["UserMessageRules"].Columns.Contains("UserID")){
				dsUserMessageRules.Tables["UserMessageRules"].Columns.Add("UserID",Type.GetType("System.String"));
			}
			if(!dsUserMessageRules.Tables["UserMessageRules"].Columns.Contains("RuleID")){
				dsUserMessageRules.Tables["UserMessageRules"].Columns.Add("RuleID",Type.GetType("System.String"));
			}
			if(!dsUserMessageRules.Tables["UserMessageRules"].Columns.Contains("Cost")){
				dsUserMessageRules.Tables["UserMessageRules"].Columns.Add("Cost",Type.GetType("System.Int64"));
			}
			if(!dsUserMessageRules.Tables["UserMessageRules"].Columns.Contains("Enabled")){
				dsUserMessageRules.Tables["UserMessageRules"].Columns.Add("Enabled",Type.GetType("System.Boolean"));
			}
			if(!dsUserMessageRules.Tables["UserMessageRules"].Columns.Contains("CheckNextRuleIf")){
				dsUserMessageRules.Tables["UserMessageRules"].Columns.Add("CheckNextRuleIf",Type.GetType("System.Int32"));
			}
			if(!dsUserMessageRules.Tables["UserMessageRules"].Columns.Contains("Description")){
				dsUserMessageRules.Tables["UserMessageRules"].Columns.Add("Description",Type.GetType("System.String"));
			}
			if(!dsUserMessageRules.Tables["UserMessageRules"].Columns.Contains("MatchExpression")){
				dsUserMessageRules.Tables["UserMessageRules"].Columns.Add("MatchExpression",Type.GetType("System.String"));
			}

            #endregion

            if(File.Exists(m_DataPath + "UserMessageRules.xml")){
			    dsUserMessageRules.ReadXml(m_DataPath + "UserMessageRules.xml");
            }
        }

        #endregion

        #region method LoadUserMessageRuleActions

        /// <summary>
		/// Loads user message rule actions from xml file.
		/// </summary>
		private void LoadUserMessageRuleActions()
		{
            DateTime dateUserMessageRuleActions = File.GetLastWriteTime(m_DataPath + "UserMessageRuleActions.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateUserMessageRuleActions,m_UserMessageRuleActions) == 0){
			    return;				
			}
            m_UserMessageRuleActions = dateUserMessageRuleActions;
			dsUserMessageRuleActions.Clear();

            #region Create Schema

            if(!dsUserMessageRuleActions.Tables.Contains("UserMessageRuleActions")){
				dsUserMessageRuleActions.Tables.Add("UserMessageRuleActions");
			}

            if(!dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Contains("UserID")){
				dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Add("UserID",Type.GetType("System.String"));
			}
			if(!dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Contains("RuleID")){
				dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Add("RuleID",Type.GetType("System.String"));
			}
			if(!dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Contains("ActionID")){
				dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Add("ActionID",Type.GetType("System.String"));
			}
			if(!dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Contains("Description")){
				dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Add("Description",Type.GetType("System.String"));
			}
			if(!dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Contains("ActionType")){
				dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Add("ActionType",Type.GetType("System.Int32"));
			}
			if(!dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Contains("ActionData")){
				dsUserMessageRuleActions.Tables["UserMessageRuleActions"].Columns.Add("ActionData",typeof(byte[]));
			}

            #endregion

            if(File.Exists(m_DataPath + "UserMessageRuleActions.xml")){
			    dsUserMessageRuleActions.ReadXml(m_DataPath + "UserMessageRuleActions.xml");
            }
        }

        #endregion


        #region method LoadGroups

        /// <summary>
        /// Loads user groups from xml file.
        /// </summary>
        private void LoadGroups()
        {
            DateTime dateGroups = File.GetLastWriteTime(m_DataPath + "Groups.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateGroups,m_GroupsDate) == 0){
			    return;				
			}
            m_GroupsDate = dateGroups;
            dsGroups.Clear();

            #region Create Schema

            //--- Create schema --------------------------------------------//
            if(!dsGroups.Tables.Contains("Groups")){
                dsGroups.Tables.Add("Groups");
            }
            if(!dsGroups.Tables["Groups"].Columns.Contains("GroupID")){
                dsGroups.Tables["Groups"].Columns.Add("GroupID");
            }            
            if(!dsGroups.Tables["Groups"].Columns.Contains("GroupName")){
                dsGroups.Tables["Groups"].Columns.Add("GroupName");
            }
            if(!dsGroups.Tables["Groups"].Columns.Contains("Description")){
                dsGroups.Tables["Groups"].Columns.Add("Description");
            }
            if(!dsGroups.Tables["Groups"].Columns.Contains("Enabled")){
                dsGroups.Tables["Groups"].Columns.Add("Enabled");
            }
            //-------------------------------------------------------------//

            #endregion
            			
            // File doesn't exist, crete it
            if(!File.Exists(m_DataPath + "Groups.xml")){
                dsGroups.WriteXml(m_DataPath + "Groups.xml");
            }
            else{
			    dsGroups.ReadXml(m_DataPath + "Groups.xml");
            }
        }

        #endregion

        #region method LoadGroupMembers

        /// <summary>
        /// Load user goruops members from xml file.
        /// </summary>
        private void LoadGroupMembers()
        {
            DateTime dateGroupMembers = File.GetLastWriteTime(m_DataPath + "GroupMembers.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateGroupMembers,m_GroupMembersDate) == 0){
			    return;				
			}
            m_GroupMembersDate = dateGroupMembers;
            dsGroupMembers.Clear();

            #region Create Schema

            //--- Create schema --------------------------------------------//
            if(!dsGroupMembers.Tables.Contains("GroupMembers")){
                dsGroupMembers.Tables.Add("GroupMembers");
            }
            if(!dsGroupMembers.Tables["GroupMembers"].Columns.Contains("GroupID")){
                dsGroupMembers.Tables["GroupMembers"].Columns.Add("GroupID");
            }            
            if(!dsGroupMembers.Tables["GroupMembers"].Columns.Contains("UserOrGroup")){
                dsGroupMembers.Tables["GroupMembers"].Columns.Add("UserOrGroup");
            }
            //-------------------------------------------------------------//

            #endregion
            		
            // File doesn't exist, crete it
            if(!File.Exists(m_DataPath + "GroupMembers.xml")){
                dsGroupMembers.WriteXml(m_DataPath + "GroupMembers.xml");
            }
            else{
			    dsGroupMembers.ReadXml(m_DataPath + "GroupMembers.xml");
            }
        }

        #endregion


        #region method LoadMailingLists

        /// <summary>
		/// Loads mailing lists from xml file.
		/// </summary>
		private void LoadMailingLists()
		{
            DateTime dateMailingLists = File.GetLastWriteTime(m_DataPath + "MailingLists.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateMailingLists,m_MailingListsDate) == 0){
			    return;				
			}
            m_MailingListsDate = dateMailingLists;
			dsMailingLists.Clear();

            #region Create Schema

            if(!dsMailingLists.Tables.Contains("MailingLists")){
				dsMailingLists.Tables.Add("MailingLists");
			}

			if(!dsMailingLists.Tables["MailingLists"].Columns.Contains("MailingListID")){
				dsMailingLists.Tables["MailingLists"].Columns.Add("MailingListID",Type.GetType("System.String"));
			}
			if(!dsMailingLists.Tables["MailingLists"].Columns.Contains("DomainName")){
				dsMailingLists.Tables["MailingLists"].Columns.Add("DomainName",Type.GetType("System.String"));
			}
			if(!dsMailingLists.Tables["MailingLists"].Columns.Contains("MailingListName")){
				dsMailingLists.Tables["MailingLists"].Columns.Add("MailingListName",Type.GetType("System.String"));
			}
			if(!dsMailingLists.Tables["MailingLists"].Columns.Contains("Description")){
				dsMailingLists.Tables["MailingLists"].Columns.Add("Description",Type.GetType("System.String"));
			}
			if(!dsMailingLists.Tables["MailingLists"].Columns.Contains("Enabled")){
				dsMailingLists.Tables["MailingLists"].Columns.Add("Enabled",Type.GetType("System.String")).DefaultValue = true;
			}

            #endregion

            if(File.Exists(m_DataPath + "MailingLists.xml")){
			    dsMailingLists.ReadXml(m_DataPath + "MailingLists.xml");
            }

			foreach(DataRow dr in dsMailingLists.Tables["MailingLists"].Rows){
				foreach(DataColumn dc in dsMailingLists.Tables["MailingLists"].Columns){
					if(dr.IsNull(dc.ColumnName)){
						dr[dc.ColumnName] = dc.DefaultValue;
					}
				}
			}
        }

        #endregion

        #region method LoadMailingListAddresses

        /// <summary>
		/// Loads mailing lists addresses from xml file.
		/// </summary>
		private void LoadMailingListAddresses()
		{
            DateTime dateMailingListAddresses = File.GetLastWriteTime(m_DataPath + "MailingListAddresses.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateMailingListAddresses,m_MailingListAddressesDate) == 0){
			    return;				
			}
            m_MailingListAddressesDate = dateMailingListAddresses;
			dsMailingListAddresses.Clear();

            #region Create Schema

            if(!dsMailingListAddresses.Tables.Contains("MailingListAddresses")){
				dsMailingListAddresses.Tables.Add("MailingListAddresses");
			}

			if(!dsMailingListAddresses.Tables["MailingListAddresses"].Columns.Contains("AddressID")){
				dsMailingListAddresses.Tables["MailingListAddresses"].Columns.Add("AddressID",Type.GetType("System.String"));
			}
			if(!dsMailingListAddresses.Tables["MailingListAddresses"].Columns.Contains("MailingListID")){
				dsMailingListAddresses.Tables["MailingListAddresses"].Columns.Add("MailingListID",Type.GetType("System.String"));
			}
			if(!dsMailingListAddresses.Tables["MailingListAddresses"].Columns.Contains("Address")){
				dsMailingListAddresses.Tables["MailingListAddresses"].Columns.Add("Address",Type.GetType("System.String"));
			}

            #endregion

            if(File.Exists(m_DataPath + "MailingListAddresses.xml")){
			    dsMailingListAddresses.ReadXml(m_DataPath + "MailingListAddresses.xml");
            }
        }

        #endregion

        #region method LoadMailingListACL

        /// <summary>
        /// Load mailing list ACl from xml file.
        /// </summary>
        private void LoadMailingListACL()
        {
            DateTime dateMailingListACL = File.GetLastWriteTime(m_DataPath + "MailingListACL.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateMailingListACL,m_MailingListAclDate) == 0){
			    return;				
			}
            m_MailingListAclDate = dateMailingListACL;
            dsMailingListACL.Clear();

            #region Create Schema

            //--- Create schema --------------------------------------//
            if(!dsMailingListACL.Tables.Contains("ACL")){
                dsMailingListACL.Tables.Add("ACL");
            }

            if(!dsMailingListACL.Tables["ACL"].Columns.Contains("MailingListID")){
                dsMailingListACL.Tables["ACL"].Columns.Add("MailingListID");
            }
            if(!dsMailingListACL.Tables["ACL"].Columns.Contains("UserOrGroup")){
                dsMailingListACL.Tables["ACL"].Columns.Add("UserOrGroup");
            }
            //--------------------------------------------------------//

            #endregion
            
            // File doesn't exist, crete it
            if(!File.Exists(m_DataPath + "MailingListACL.xml")){
                dsMailingListACL.WriteXml(m_DataPath + "MailingListACL.xml");
            }
            else{
			    dsMailingListACL.ReadXml(m_DataPath + "MailingListACL.xml");
            }
        }

        #endregion


        #region method LoadGlobalMessageRules

        /// <summary>
		///  Loads global message rules from xml file.
		/// </summary>
		private void LoadGlobalMessageRules()
		{
            DateTime dateRules = File.GetLastWriteTime(m_DataPath + "GlobalMessageRules.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateRules,m_RulesDate) == 0){
			    return;				
			}
            m_RulesDate = dateRules;
			dsRules.Clear();

            #region Create Schema

            if(!dsRules.Tables.Contains("GlobalMessageRules")){
				dsRules.Tables.Add("GlobalMessageRules");
			}

			if(!dsRules.Tables["GlobalMessageRules"].Columns.Contains("RuleID")){
				dsRules.Tables["GlobalMessageRules"].Columns.Add("RuleID",Type.GetType("System.String"));
			}
			if(!dsRules.Tables["GlobalMessageRules"].Columns.Contains("Cost")){
				dsRules.Tables["GlobalMessageRules"].Columns.Add("Cost",Type.GetType("System.Int64"));
			}
			if(!dsRules.Tables["GlobalMessageRules"].Columns.Contains("Enabled")){
				dsRules.Tables["GlobalMessageRules"].Columns.Add("Enabled",Type.GetType("System.Boolean"));
			}
			if(!dsRules.Tables["GlobalMessageRules"].Columns.Contains("CheckNextRuleIf")){
				dsRules.Tables["GlobalMessageRules"].Columns.Add("CheckNextRuleIf",Type.GetType("System.Int32"));
			}
			if(!dsRules.Tables["GlobalMessageRules"].Columns.Contains("Description")){
				dsRules.Tables["GlobalMessageRules"].Columns.Add("Description",Type.GetType("System.String"));
			}
			if(!dsRules.Tables["GlobalMessageRules"].Columns.Contains("MatchExpression")){
				dsRules.Tables["GlobalMessageRules"].Columns.Add("MatchExpression",Type.GetType("System.String"));
			}

            #endregion

            // File doesn't exist, crete it
            if(!File.Exists(m_DataPath + "GlobalMessageRules.xml")){
                dsRules.WriteXml(m_DataPath + "GlobalMessageRules.xml");
            }
            else{
			    dsRules.ReadXml(m_DataPath + "GlobalMessageRules.xml");
            }
        }

        #endregion

        #region method LoadGlobalMessageRuleActions

        /// <summary>
		/// Loads global message rule actions from xml file.
		/// </summary>
		private void LoadGlobalMessageRuleActions()
		{
            DateTime dateRuleActions = File.GetLastWriteTime(m_DataPath + "GlobalMessageRuleActions.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateRuleActions,m_RuleActionsDate) == 0){
			    return;				
			}
            m_RuleActionsDate = dateRuleActions;
			dsRuleActions.Clear();

            #region Create Schema

            if(!dsRuleActions.Tables.Contains("GlobalMessageRuleActions")){
				dsRuleActions.Tables.Add("GlobalMessageRuleActions");
			}

			if(!dsRuleActions.Tables["GlobalMessageRuleActions"].Columns.Contains("RuleID")){
				dsRuleActions.Tables["GlobalMessageRuleActions"].Columns.Add("RuleID",Type.GetType("System.String"));
			}
			if(!dsRuleActions.Tables["GlobalMessageRuleActions"].Columns.Contains("ActionID")){
				dsRuleActions.Tables["GlobalMessageRuleActions"].Columns.Add("ActionID",Type.GetType("System.String"));
			}
			if(!dsRuleActions.Tables["GlobalMessageRuleActions"].Columns.Contains("Description")){
				dsRuleActions.Tables["GlobalMessageRuleActions"].Columns.Add("Description",Type.GetType("System.String"));
			}
			if(!dsRuleActions.Tables["GlobalMessageRuleActions"].Columns.Contains("ActionType")){
				dsRuleActions.Tables["GlobalMessageRuleActions"].Columns.Add("ActionType",Type.GetType("System.Int32"));
			}
			if(!dsRuleActions.Tables["GlobalMessageRuleActions"].Columns.Contains("ActionData")){
				dsRuleActions.Tables["GlobalMessageRuleActions"].Columns.Add("ActionData",typeof(byte[]));
			}

            #endregion

            // File doesn't exist, create it
            if(!File.Exists(m_DataPath + "GlobalMessageRuleActions.xml")){
                dsRuleActions.WriteXml(m_DataPath + "GlobalMessageRuleActions.xml");
            }
            else{
			    dsRuleActions.ReadXml(m_DataPath + "GlobalMessageRuleActions.xml");
            }
        }

        #endregion


        #region method LoadRouting

        /// <summary>
		///  Loads routing from xml file.
		/// </summary>
		private void LoadRouting()
		{
            DateTime dateRouting = File.GetLastWriteTime(m_DataPath + "Routing.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateRouting,m_RoutingDate) == 0){
			    return;				
			}
            m_RoutingDate = dateRouting;
			dsRouting.Clear();

            #region Create Schema

            if(!dsRouting.Tables.Contains("Routing")){
				dsRouting.Tables.Add("Routing");
			}

			if(!dsRouting.Tables["Routing"].Columns.Contains("RouteID")){
				dsRouting.Tables["Routing"].Columns.Add("RouteID",Type.GetType("System.String"));
			}
            if(!dsRouting.Tables["Routing"].Columns.Contains("Cost")){
				dsRouting.Tables["Routing"].Columns.Add("Cost",Type.GetType("System.Int64"));
			}
            if(!dsRouting.Tables["Routing"].Columns.Contains("Enabled")){
				dsRouting.Tables["Routing"].Columns.Add("Enabled",Type.GetType("System.Boolean"));
			}
            if(!dsRouting.Tables["Routing"].Columns.Contains("Description")){
				dsRouting.Tables["Routing"].Columns.Add("Description",Type.GetType("System.String"));
			}
			if(!dsRouting.Tables["Routing"].Columns.Contains("Pattern")){
				dsRouting.Tables["Routing"].Columns.Add("Pattern",Type.GetType("System.String"));
			}            
			if(!dsRouting.Tables["Routing"].Columns.Contains("Action")){
				dsRouting.Tables["Routing"].Columns.Add("Action",Type.GetType("System.Int32"));
			}                       
			if(!dsRouting.Tables["Routing"].Columns.Contains("ActionData")){
				dsRouting.Tables["Routing"].Columns.Add("ActionData",Type.GetType("System.Byte[]"));
			}

            #endregion

            if(File.Exists(m_DataPath + "Routing.xml")){
			    dsRouting.ReadXml(m_DataPath + "Routing.xml");
            }
        }

        #endregion
               

        #region method LoadSecurity

        /// <summary>
		///  Loads security from xml file.
		/// </summary>
		private void LoadSecurity()
		{
            DateTime dateSecurity = File.GetLastWriteTime(m_DataPath + "Security.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateSecurity,m_SecurityDate) == 0){
			    return;				
			}
            m_SecurityDate = dateSecurity;
			dsSecurity.Clear();

            #region Create Schema

            if(!dsSecurity.Tables.Contains("IPSecurity")){
				dsSecurity.Tables.Add("IPSecurity");
			}

			if(!dsSecurity.Tables["IPSecurity"].Columns.Contains("ID")){
				dsSecurity.Tables["IPSecurity"].Columns.Add("ID",Type.GetType("System.String"));
			}
			if(!dsSecurity.Tables["IPSecurity"].Columns.Contains("Enabled")){
				dsSecurity.Tables["IPSecurity"].Columns.Add("Enabled",Type.GetType("System.Boolean"));
			}
			if(!dsSecurity.Tables["IPSecurity"].Columns.Contains("Description")){
				dsSecurity.Tables["IPSecurity"].Columns.Add("Description",Type.GetType("System.String"));
			}
			if(!dsSecurity.Tables["IPSecurity"].Columns.Contains("Service")){
				dsSecurity.Tables["IPSecurity"].Columns.Add("Service",Type.GetType("System.Int32"));
			}
			if(!dsSecurity.Tables["IPSecurity"].Columns.Contains("Action")){
				dsSecurity.Tables["IPSecurity"].Columns.Add("Action",Type.GetType("System.Int32"));
			}
			if(!dsSecurity.Tables["IPSecurity"].Columns.Contains("StartIP")){
				dsSecurity.Tables["IPSecurity"].Columns.Add("StartIP",Type.GetType("System.String"));
			}
			if(!dsSecurity.Tables["IPSecurity"].Columns.Contains("EndIP")){
				dsSecurity.Tables["IPSecurity"].Columns.Add("EndIP",Type.GetType("System.String"));
			}

            #endregion

            if(File.Exists(m_DataPath + "IPSecurity.xml")){
			    dsSecurity.ReadXml(m_DataPath + "IPSecurity.xml");
            }
        }

        #endregion


        #region method LoadFilters

        /// <summary>
		///  Loads filters from xml file.
		/// </summary>
		private void LoadFilters()
		{
            DateTime dateFilters = File.GetLastWriteTime(m_DataPath + "Filters.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateFilters,m_FiltersDate) == 0){
			    return;				
			}
            m_FiltersDate = dateFilters;
			dsFilters.Clear();

            #region Create Schema

            if(!dsFilters.Tables.Contains("SmtpFilters")){
				dsFilters.Tables.Add("SmtpFilters");
			}

			if(!dsFilters.Tables["SmtpFilters"].Columns.Contains("FilterID")){
				dsFilters.Tables["SmtpFilters"].Columns.Add("FilterID",Type.GetType("System.String"));
			}
			if(!dsFilters.Tables["SmtpFilters"].Columns.Contains("Cost")){
				dsFilters.Tables["SmtpFilters"].Columns.Add("Cost",Type.GetType("System.Int64"));
			}
			if(!dsFilters.Tables["SmtpFilters"].Columns.Contains("Assembly")){
				dsFilters.Tables["SmtpFilters"].Columns.Add("Assembly",Type.GetType("System.String"));
			}
			if(!dsFilters.Tables["SmtpFilters"].Columns.Contains("ClassName")){
				dsFilters.Tables["SmtpFilters"].Columns.Add("ClassName",Type.GetType("System.String"));
			}
			if(!dsFilters.Tables["SmtpFilters"].Columns.Contains("Enabled")){
				dsFilters.Tables["SmtpFilters"].Columns.Add("Enabled",Type.GetType("System.Boolean"));
				dsFilters.Tables["SmtpFilters"].Columns["Enabled"].DefaultValue = true;
			}
			if(!dsFilters.Tables["SmtpFilters"].Columns.Contains("Description")){
				dsFilters.Tables["SmtpFilters"].Columns.Add("Description",Type.GetType("System.String"));
			}
			if(!dsFilters.Tables["SmtpFilters"].Columns.Contains("Type")){
				dsFilters.Tables["SmtpFilters"].Columns.Add("Type",Type.GetType("System.String"));
			}

            #endregion

            if(File.Exists(m_DataPath + "Filters.xml")){
			    dsFilters.ReadXml(m_DataPath + "Filters.xml");
            }
        }

        #endregion


        #region method Load_IMAP_ACL

        /// <summary>
		///  Loads IMAP ACL from xml file.
		/// </summary>
		private void Load_IMAP_ACL()
		{
            DateTime dateIMAPACL = File.GetLastWriteTime(m_DataPath + "IMAP_ACL.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateIMAPACL,m_ImapACLDate) == 0){
			    return;				
			}
            m_ImapACLDate = dateIMAPACL;
			dsImapACL.Clear();

            #region Create Schema

            if(!dsImapACL.Tables.Contains("ACL")){
				dsImapACL.Tables.Add("ACL");
			}
			
			if(!dsImapACL.Tables["ACL"].Columns.Contains("Folder")){
				dsImapACL.Tables["ACL"].Columns.Add("Folder",Type.GetType("System.String"));
			}
			if(!dsImapACL.Tables["ACL"].Columns.Contains("User")){
				dsImapACL.Tables["ACL"].Columns.Add("User",Type.GetType("System.String"));
			}
			if(!dsImapACL.Tables["ACL"].Columns.Contains("Permissions")){
				dsImapACL.Tables["ACL"].Columns.Add("Permissions",Type.GetType("System.String"));
			}

            #endregion

            if(File.Exists(m_DataPath + "IMAP_ACL.xml")){
			    dsImapACL.ReadXml(m_DataPath + "IMAP_ACL.xml");
            }
        }

        #endregion

        #region method Load_SharedFolders_Roots

        /// <summary>
        /// Loads Shared Folder Roots from xml file.
        /// </summary>
        private void Load_SharedFolders_Roots()
        {
            DateTime dateSharedFolderRoots = File.GetLastWriteTime(m_DataPath + "IMAP_ACL.xml");
            // File not changed, skip loading
            if(DateTime.Compare(dateSharedFolderRoots,m_SharedFolderRootsDate) == 0){
			    return;				
			}
            m_SharedFolderRootsDate = dateSharedFolderRoots;
            dsSharedFolderRoots.Clear();

            #region Create Schema

            if(!dsSharedFolderRoots.Tables.Contains("SharedFoldersRoots")){
                dsSharedFolderRoots.Tables.Add("SharedFoldersRoots");
            }

            if(!dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Contains("RootID")){
				dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Add("RootID",Type.GetType("System.String"));
			}
            if(!dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Contains("Enabled")){
				dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Add("Enabled",Type.GetType("System.Boolean"));
			}
            if(!dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Contains("Folder")){
				dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Add("Folder",Type.GetType("System.String"));
			}
            if(!dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Contains("Description")){
				dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Add("Description",Type.GetType("System.String"));
			}
            if(!dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Contains("RootType")){
				dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Add("RootType",Type.GetType("System.Int32"));
			}
            if(!dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Contains("BoundedUser")){
				dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Add("BoundedUser",Type.GetType("System.String"));
			}
            if(!dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Contains("BoundedFolder")){
				dsSharedFolderRoots.Tables["SharedFoldersRoots"].Columns.Add("BoundedFolder",Type.GetType("System.String"));
			}

            #endregion

            // File doesn't exist, crete it
            if(!File.Exists(m_DataPath + "SharedFoldersRoots.xml")){
                dsSharedFolderRoots.WriteXml(m_DataPath + "SharedFoldersRoots.xml");
            }
            else{
			    dsSharedFolderRoots.ReadXml(m_DataPath + "SharedFoldersRoots.xml");
            }
        }

        #endregion

        
        #region method LoadUsersDefaultFolders

        /// <summary>
        /// Loads users default folders from xml file.
        /// </summary>
        private void LoadUsersDefaultFolders()
        {
            DateTime date = File.GetLastWriteTime(m_DataPath + "UsersDefaultFolders.xml");
            // File not changed, skip loading
            if(DateTime.Compare(date,m_UsersDefaultFoldersDate) == 0){
			    return;				
			}
            m_UsersDefaultFoldersDate = date;
			dsUsersDefaultFolders.Clear();

            #region Create Schema

            if(!dsUsersDefaultFolders.Tables.Contains("UsersDefaultFolders")){
				dsUsersDefaultFolders.Tables.Add("UsersDefaultFolders");
			}
			
			if(!dsUsersDefaultFolders.Tables["UsersDefaultFolders"].Columns.Contains("FolderName")){
				dsUsersDefaultFolders.Tables["UsersDefaultFolders"].Columns.Add("FolderName",Type.GetType("System.String"));
			}
			if(!dsUsersDefaultFolders.Tables["UsersDefaultFolders"].Columns.Contains("Permanent")){
				dsUsersDefaultFolders.Tables["UsersDefaultFolders"].Columns.Add("Permanent",Type.GetType("System.Boolean"));
			}

            #endregion

            if(File.Exists(m_DataPath + "UsersDefaultFolders.xml")){
			    dsUsersDefaultFolders.ReadXml(m_DataPath + "UsersDefaultFolders.xml");
            }
        }

        #endregion


        #region method LoadRecycleBinSettings

        /// <summary>
        /// Loads recycle bin settings from xml.
        /// </summary>
        private void LoadRecycleBinSettings()
        {
            DateTime date = File.GetLastWriteTime(m_DataPath + "RecycleBinSettings.xml");
            // File not changed, skip loading
            if(DateTime.Compare(date,m_RecycleBinSettingsDate) == 0){
			    return;				
			}
            m_RecycleBinSettingsDate = date;
			dsRecycleBinSettings.Clear();

            #region Create Schema

            if(!dsRecycleBinSettings.Tables.Contains("RecycleBinSettings")){
                dsRecycleBinSettings.Tables.Add("RecycleBinSettings");
            }

            if(!dsRecycleBinSettings.Tables["RecycleBinSettings"].Columns.Contains("DeleteToRecycleBin")){
                dsRecycleBinSettings.Tables["RecycleBinSettings"].Columns.Add("DeleteToRecycleBin",typeof(bool)).DefaultValue = false;
            }
            if(!dsRecycleBinSettings.Tables["RecycleBinSettings"].Columns.Contains("DeleteMessagesAfter")){
                dsRecycleBinSettings.Tables["RecycleBinSettings"].Columns.Add("DeleteMessagesAfter",typeof(int)).DefaultValue = 1;
            }

            #endregion
                        
            if(File.Exists(m_DataPath + "RecycleBinSettings.xml")){
			    dsRecycleBinSettings.ReadXml(m_DataPath + "RecycleBinSettings.xml");
            }

            if(dsRecycleBinSettings.Tables["RecycleBinSettings"].Rows.Count == 0){
                DataRow dr = dsRecycleBinSettings.Tables["RecycleBinSettings"].NewRow();
                dr["DeleteToRecycleBin"]  = false;
                dr["DeleteMessagesAfter"] = 1;
                dsRecycleBinSettings.Tables["RecycleBinSettings"].Rows.Add(dr);
            }
        }

        #endregion

        #endregion

        #region Service Timer

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{		
			// Start update
			m_UpdSync.BeginUpdate();

			try{
				LoadDomains();									
				
				LoadUsers();		
				LoadUserAddresses();
				LoadUserRemoteServers();
				LoadUserMessageRules();
				LoadUserMessageRuleActions();

				LoadGroups();
				LoadGroupMembers();

				LoadMailingLists();
				LoadMailingListAddresses();
				LoadMailingListACL();

				LoadGlobalMessageRules();
				LoadGlobalMessageRuleActions();

				LoadRouting();	

				LoadSecurity();

				LoadFilters();

				Load_IMAP_ACL();
				Load_SharedFolders_Roots();	

                LoadUsersDefaultFolders();

                LoadRecycleBinSettings();
			}
			catch(Exception x){
				throw x;
			}
            finally{
			    // End update
			    m_UpdSync.EndUpdate();
            }
		}

		#endregion

		#endregion


        #region method GetSharedFolderRoot

        /// <summary>
        /// Gets specified shared root folder. If root folder doesn't exist, returns null.
        /// </summary>
        /// <param name="rootFolder">Root folder name.</param>
        /// <returns></returns>
        private SharedFolderRoot GetSharedFolderRoot(string rootFolder)
        {
            foreach(SharedFolderRoot root in GetSharedFolderRoots()){
                if(root.FolderName.ToLower() == rootFolder.ToLower()){
                    return root;
                }
            }

            return null;
        }

        #endregion

        #region method IsUserGroupMember

        /// <summary>
        /// Gets if specified user is specified user group member. Returns true if user is user group member.
        /// </summary>
        /// <param name="group">User group name.</param>
        /// <param name="user">User name.</param>
        /// <returns>Returns true if user is user group member.</returns>
        private bool IsUserGroupMember(string group,string user)
        {
            List<string> proccessedGroups = new List<string>();
            Queue<string> membersQueue = new Queue<string>();
            string[] members = GetGroupMembers(group);
            foreach(string member in members){
                membersQueue.Enqueue(member);
            }

            while(membersQueue.Count > 0){
                string member = membersQueue.Dequeue();
                // Nested group
                DataRow drGroup = GetGroup(member);
                if(drGroup != null){
                    // Don't proccess poroccessed groups any more, causes infinite loop
                    if(!proccessedGroups.Contains(member.ToLower())){
                        // Skip disabled groups
                        if(Convert.ToBoolean(drGroup["Enabled"])){
                            members = GetGroupMembers(member);
                            foreach(string m in members){
                                membersQueue.Enqueue(m);
                            }  
                        }

                        proccessedGroups.Add(member.ToLower());
                    }
                }
                // User
                else{
                    if(member.ToLower() == user.ToLower()){
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region method GetGroup

        /// <summary>
        /// Gets specified user group. If group doesn't exist, returns null;
        /// </summary>
        /// <param name="group">User group name.</param>
        /// <returns></returns>
        private DataRow GetGroup(string group)
        {
            m_UpdSync.AddMethod();

			try{
				foreach(DataRow dr in dsGroups.Tables["Groups"].Rows){
                    if(group.ToLower() == dr["GroupName"].ToString().ToLower()){
                        return dr;
                    }
                }
				
                return null;
			}
			catch(Exception x){
				throw x;
			}
			finally{
				m_UpdSync.RemoveMethod();
			}
        }

        #endregion

        #region method GetMailingList

        /// <summary>
        /// Gets specified mailing list or returns null if mailing list doesn't exist.
        /// </summary>
        /// <param name="mailingListName">Mailing list name.</param>
        /// <returns></returns>
        private DataRow GetMailingList(string mailingListName)
        {
            foreach(DataRow dr in dsMailingLists.Tables["MailingLists"].Rows){
                if(dr["MailingListName"].ToString().ToLower() == mailingListName.ToLower()){
                    return dr;
                }
            }

            return null;
        }

        #endregion

        #region method MapSharedFolder

        /// <summary>
        /// This call holds shaared folder mapping info.
        /// </summary>
        private class SharedFolderMapInfo
        {
            private string m_OriginalFolder = "";
            private string m_FolderOwner    = "";
            private string m_Folder         = "";
            private string m_SharedRootName = "";

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="originalFolder"></param>
            /// <param name="folderOwner"></param>
            /// <param name="folder"></param>
            /// <param name="sharedRootName"></param>
            public SharedFolderMapInfo(string originalFolder,string folderOwner,string folder,string sharedRootName)
            {
                m_OriginalFolder = originalFolder;
                m_FolderOwner    = folderOwner;
                m_Folder         = folder;
                m_SharedRootName = sharedRootName;
            }


            #region Properties Implementation

            /// <summary>
            /// Gets original folder.
            /// </summary>
            public string OriginalFolder
            {
                get{ return m_OriginalFolder; }
            }

            /// <summary>
            /// Shared folder owner. This is available only if OriginalFolder is shared folder.
            /// </summary>
            public string FolderOnwer
            {
                get{ return m_FolderOwner; }
            }

            /// <summary>
            /// Gets shared folder owner folder. This is available only if OriginalFolder is shared folder.
            /// </summary>
            public string Folder
            {
                get{ return m_Folder; }
            }

            /// <summary>
            /// Gets shared root folder name. This is available only if OriginalFolder is shared folder.
            /// </summary>
            public string SharedRootName
            {
                get{ return m_SharedRootName; }
            }

            /// <summary>
            /// Gets if OriginalFolder is shared folder.
            /// </summary>
            public bool IsSharedFolder
            {
                get{ return m_SharedRootName != ""; }
            }

            #endregion
        }

        /// <summary>
        /// If folder is Shared Folder, then maps folder to actual account.
        /// </summary>
        /// <param name="folder">Folder to map.</param>
        /// <returns></returns>
        private SharedFolderMapInfo MapSharedFolder(string folder)
        {
            string rootFolder = folder.Split(new char[]{'/'},2)[0];

            SharedFolderRoot[] roots = GetSharedFolderRoots();
            foreach(SharedFolderRoot root in roots){
                if(root.RootType == SharedFolderRootType_enum.BoundedRootFolder){
                    if(rootFolder.ToLower() == root.FolderName.ToLower()){
                        return new SharedFolderMapInfo(folder,root.BoundedUser,root.BoundedFolder,root.FolderName);
                    }
                }
                else if(root.RootType == SharedFolderRootType_enum.UsersSharedFolder){
                    if(rootFolder.ToLower() == root.FolderName.ToLower()){
                        // Cut off root folder name
                        string[] p = folder.Split(new char[]{'/'},3);
                        // root/user/folder
                        if(p.Length == 3){
                            return new SharedFolderMapInfo(folder,p[1],p[2],root.FolderName);
                        }
                        // root/user
                        else if(p.Length == 3){
                            return new SharedFolderMapInfo(folder,p[1],"",root.FolderName);
                        }
                        // root (User and folder missing)
                        else{
                            return new SharedFolderMapInfo(folder,"","",root.FolderName);
                        }
                    }
                }
            }

            return new SharedFolderMapInfo(folder,"","","");
        }

        #endregion
 
		#region method CreateMessageInfo

		/// <summary>
		/// Creates specified message cache record.
		/// </summary>
		/// <param name="file">File name of file which cache record to do.</param>
		/// <returns></returns>
		private object[] CreateMessageInfo(string file)
		{
            string[] fileParts = Path.GetFileNameWithoutExtension(file).Split('_');
            if(fileParts.Length == 3){
                // REMOVEME: **************************

			    // Size
			    int size = (int)(new FileInfo(file)).Length;

			    // Get INTERNALDATE,UID,FLAGS			
			    DateTime recieveDate = DateTime.ParseExact(fileParts[0],"yyyyMMddHHmmss",System.Globalization.DateTimeFormatInfo.InvariantInfo);
			    int      uid         = Convert.ToInt32(fileParts[1]);
			    int      flags       = Convert.ToInt32(fileParts[2]);
                        
			    return new object[]{uid,size,flags,recieveDate};
            }
            else{
                // Get INTERNALDATE,UID			
			    DateTime recieveDate = DateTime.ParseExact(fileParts[0],"yyyyMMddHHmmss",System.Globalization.DateTimeFormatInfo.InvariantInfo);
			    int      uid         = Convert.ToInt32(fileParts[1]);
                
                int size  = 0;
                int flags = 0;
                using(FileStream fs = File.OpenRead(file)){
                    _InternalHeader header = new _InternalHeader(fs);

                    size  = (int)(fs.Length - fs.Position);
                    flags = (int)header.MessageFlags;
                }

                // REMOVE ME: This never should happen
                if(size == 0){
                    throw new Exception("CreateMessageInfo if(size == 0){, this should never happen !");
                }
                
                return new object[]{uid,size,flags,recieveDate};
            }
		}

		#endregion

		#region method NormalizeFolder

		/// <summary>
		/// Normalizes folder path. For example: aaa/test/, will be aaa/test.
		/// </summary>
		/// <param name="folderPath">Folder path with folder name.</param>
		/// <returns></returns>
		private string NormalizeFolder(string folderPath)
		{
			folderPath = folderPath.Replace("\\","/");

			if(folderPath.EndsWith("/")){
				folderPath = folderPath.Substring(0,folderPath.Length - 1);
			}
                        
			if(folderPath.StartsWith("/")){
				folderPath = folderPath.Substring(1);
			}
            
			return folderPath;
		}

		#endregion

		#region method ContainsID

		private bool ContainsID(DataTable dt,string column,string idValue)
		{
			using(DataView dv = new DataView(dt)){
				dv.RowFilter = column + "='" + idValue + "'";

				if(dv.Count > 0){
					return true;
				}
			}

			return false;
		}

		#endregion

		#region method GetTopLines

		private byte[] GetTopLines(Stream strm,int nrLines)
		{
			TextReader reader = (TextReader)new StreamReader(strm);
			
			int  lCounter = 0;
			int  msgLine  = -1;
			bool msgLines = false;
			StringBuilder strBuilder = new StringBuilder();
			while(true){
				string line = reader.ReadLine();

				// Reached end of message
				if(line == null){
					break;
				}
				else{
					// End of header reached
					if(!msgLines && line.Length == 0){
						// Set flag, that message lines reading start.
						msgLines = true;
					}

					// Check that wanted message lines count isn't exceeded
					if(msgLines){
						if(msgLine > nrLines){
							break;
						}
						msgLine++;
					}

					strBuilder.Append(line + "\r\n");
				}

				lCounter++;
			}
	
			return System.Text.Encoding.ASCII.GetBytes(strBuilder.ToString());			
		}

		#endregion

		#region method CalculateMailboxSize

		/// <summary>
		/// Calculates specified user mailbox size.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <returns></returns>
		private long CalculateMailboxSize(string userName)
		{	
	        string path = API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName));
	
			long sizeTotal = 0;
			foreach(string dir in GetFileSytemFolders(path,true)){
				DirectoryInfo dirInf = new DirectoryInfo(dir);
				foreach(FileInfo file in dirInf.GetFiles()){
					sizeTotal += file.Length;
				}
			}

			return sizeTotal;
		}

		#endregion

		#region method ChangeMailboxSize

		/// <summary>
		/// Changes specified user mailbox size by specified value.
		/// </summary>
		/// <param name="userName">User name.</param> 
		/// <param name="size">If value is positive, then specified value is added to mailbox.
		/// If negative, then value is removed from mailbox size.
		/// </param>
		/// <returns>Returns new mailbox size.</returns>
		private long ChangeMailboxSize(string userName,long size)
		{
            string path     = API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName));
			string sizeFile = API_Utlis.PathFix(path + "\\_mailbox_size");

			// If file locked, wait and try to open it again
			for(int i=0;i<1000;i++){
				try{
					// If new user and folders aren't created yet, just return 0 as mailbox size.
					if(path == null){
						return 0;
					}

					long mailboxSize = 0;
					using(FileStream fs = File.Open(sizeFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.Read)){
						// Mailbox file just created,calculate mailbox size and store it to that file.
						if(fs.Length == 0){
							long sizeCurrent = CalculateMailboxSize(userName);

							byte[] sizeByte = System.Text.Encoding.ASCII.GetBytes(sizeCurrent.ToString());
							fs.Write(sizeByte,0,sizeByte.Length);

							mailboxSize = sizeCurrent;
						}
						else{
							// Read current size from file
							byte[] sizeByte = new byte[fs.Length];
							fs.Read(sizeByte,0,sizeByte.Length);
							long sizeCurrent = Convert.ToInt64(System.Text.Encoding.ASCII.GetString(sizeByte).Trim());

							// Clear current value
							fs.SetLength(0);

							// Store new value to file
							sizeByte = System.Text.Encoding.ASCII.GetBytes(Convert.ToString(sizeCurrent + size));
							fs.Write(sizeByte,0,sizeByte.Length);
				
							mailboxSize = sizeCurrent + size;
						}
					}

					return mailboxSize;
				}
				catch(IOException x){
                    // This suppresses compile warning
                    string dummy = x.Message;

					System.Threading.Thread.Sleep(10);
				}
			}

			// Never should reach there, if this happens file couldn't be opend for some reason
			throw new Exception("Error changing mailbox size, ChangeMailboxSize(long size) failed !");
		}

		#endregion

		#region method OpenOrCreateFile

		/// <summary>
		/// Opens or creates specified file. Opened file is with exclusive lock.
		/// </summary>
		/// <param name="fileName">File name with full path.</param>
		/// <param name="lockTimeOut">File unlock wait time in milliseconds. 
		/// Waits specified time for file to unlock, if times out, exception is throwown.</param>
		/// <returns></returns>
		private FileStream OpenOrCreateFile(string fileName,int lockTimeOut)
		{
			FileStream fs = null;
			DateTime lockTimeOutTime = DateTime.Now.AddMilliseconds(lockTimeOut);
			// If file locked, wait specified time to unlock
			while(lockTimeOutTime > DateTime.Now){
				try{
					fs = File.Open(API_Utlis.PathFix(fileName),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);					
					break;
				}
				catch(IOException x){                    
                    // This suppresses compile warning
                    string dummy = x.Message;

					System.Threading.Thread.Sleep(100);
				}				
			}

			if(fs == null){
				throw new Exception("Can't open file '" + fileName + "', lock wait timed out !");
			}

            return fs;			
		}

		#endregion
//***
		#region method GetUserMailboxFolders
        /*
		/// <summary>
		/// Gets specified user folders.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <param name="stream">Folders file stream.</param>
		/// <returns></returns>       
		private DataSet GetUserMailboxFolders(string userName,Stream stream)
		{
			DataSet dsFolders = new DataSet("dsFoldersInfo");
			dsFolders.Tables.Add("Folders_UID_Holder");
			dsFolders.Tables["Folders_UID_Holder"].Columns.Add("NextUID");
			dsFolders.Tables.Add("Folders");
			dsFolders.Tables["Folders"].Columns.Add("FolderPath");
			dsFolders.Tables["Folders"].Columns.Add("Folder_IMAP_UID");

			try{
				dsFolders.ReadXml(stream);					
			}
			catch(Exception x){
                string dummy = x.Message;
				// Folders file is empty or broken, create new
				stream.SetLength(0);
				dsFolders.Clear();
				
                string[] folders = GetFileSytemFolders(m_MailStorePath + "Mailboxes\\" + userName,false);
				for(int i=0;i<folders.Length;i++){
					DataRow dr = dsFolders.Tables["Folders"].NewRow();
					dr["FolderPath"]      = NormalizeFolder(folders[i].ToString());
					dr["Folder_IMAP_UID"] = i + 1;
					dsFolders.Tables["Folders"].Rows.Add(dr);
				}

				dsFolders.Tables["Folders_UID_Holder"].Rows.Add(new object[]{folders.Length + 1});

				dsFolders.WriteXml(stream);
			}

			return dsFolders;
		}*/

		#endregion

        #region method GetFileSytemFolders

        /// <summary>
        /// Gets specified path folders, all child foders are included.
        /// </summary>
        /// <param name="path">Path what folders to get.</param>
        /// <param name="fullPath">If ture than full long path folder returned (path\xxx\aaa), otherwise folders are relative to path ((no_path_\)xxx\bbb).</param>
        /// <returns></returns>
        private string[] GetFileSytemFolders(string path,bool fullPath)
        {            
            path = API_Utlis.PathFix(path);

            List<string> retVal = new List<string>();
            Queue<string> folders = new Queue<string>();
            foreach(string folder in Directory.GetDirectories(path)){
                folders.Enqueue(folder);
            }
            
            while(folders.Count > 0){
                string folder = folders.Dequeue();
                foreach(string childFolder in Directory.GetDirectories(folder)){
                    folders.Enqueue(childFolder);
                }

                if(fullPath){
                    retVal.Add(folder);
                }
                else{
                    retVal.Add(folder.Substring(path.Length));
                }
            }

            return retVal.ToArray();
        }

        #endregion

		#region method GetNextUid

		/// <summary>
		/// Gets folder next UID value.
		/// </summary>
		/// <param name="userName">User name who's folder UID to get.</param>
		/// <param name="folder">Folder which UID to get.</param>
		/// <returns></returns>
		private int GetNextUid(string userName,string folder)
		{			            
            string path    = API_Utlis.DirectoryExists(API_Utlis.PathFix(m_MailStorePath + "Mailboxes\\" + userName + "\\" + Core.Encode_IMAP_UTF7_String(folder)));
			string uidFile = API_Utlis.PathFix(path + "\\_UID_holder");

            // Try 20 seconds to open flags file, it's locked.
            DateTime start = DateTime.Now;
            string   error = "";

            while(start.AddSeconds(20) > DateTime.Now){
                try{
                    int uid = 1;
                    using(FileStream fs = File.Open(uidFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None)){
                        byte[] data = null;
					    if(fs.Length > 0){
					        //--- Read current UID -----------
					        data = new byte[fs.Length];
					        fs.Read(data,0,data.Length);

					        uid = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(data));
					        //---------------------------------
                        }
                        // File just created or UID is lost. If there are some messages on that folder, get max UID.
                        else{
                            string[] messages = Directory.GetFiles(path,"*.eml");
                            foreach(string message in messages){
                                int msgUID = Convert.ToInt32(Path.GetFileNameWithoutExtension(message).Split('_')[1]);
                                if(msgUID > uid){
                                    uid = msgUID;
                                }
                            }
                        }

					    // Increase UID
					    uid++;

					    // Write increased UID value
					    fs.Position = 0;
					    data = System.Text.Encoding.ASCII.GetBytes(uid.ToString("d10"));
					    fs.Write(data,0,data.Length);
                    }

                    return uid;
                }
                catch(Exception x){
                    error = x.Message;

                    // Wait here, otherwise takes 100% CPU
                    System.Threading.Thread.Sleep(5);
                }
            }

            // If we reach here, flags file open failed.
            throw new Exception("Getting next message UID value timed-out, failed with error: " + error);
		}

		#endregion

        #region method CreateMessageFileName

        /// <summary>
        /// Creates message file name.
        /// </summary>
        /// <param name="internalDate">Message internal date.</param>
        /// <param name="uid">Message UID value.</param>
        /// <returns></returns>
        private string CreateMessageFileName(DateTime internalDate,int uid)
        {
            return internalDate.ToString("yyyyMMddHHmmss") + "_" + uid.ToString("D10");
        }

        #endregion

        #region method LoadDataSetDefaults

        /// <summary>
        /// Replaces DbNull values with column default value in all tables.
        /// </summary>
        /// <param name="ds">DataSet where to replace null values.</param>
        private void LoadDataSetDefaults(DataSet ds)
        {
            foreach(DataTable dt in ds.Tables){
                foreach(DataRow dr in dt.Rows){
				    foreach(DataColumn dc in dt.Columns){
					    if(dr.IsNull(dc.ColumnName)){
						    dr[dc.ColumnName] = dc.DefaultValue;
					    }
				    }
			    }
            }
        }

        #endregion

    }
}
