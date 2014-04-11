using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using LumiSoft.Net;
using LumiSoft.Net.IMAP.Server;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Specifies server database type.
	/// </summary>
	public enum DB_Type
	{
		/// <summary>
		/// Data will be stored to XML.
		/// </summary>
		XML   = 1,

		/// <summary>
		/// Data will be stored to MS SQL.
		/// </summary>
		MSSQL = 2,

		/// <summary>
		/// For remote administration only.
		/// </summary>
		WebServices = 3,
	}

	/// <summary>
	/// MAilserver API.
	/// </summary>
	public class ServerAPI
	{
		private string  m_DataPath        = "";
		private string  m_MailStorePath   = "";
		private string  m_ConStr          = "";
		private string  m_WebServicesUrl  = "";
		private string  m_WebServicesPwd  = "";
		private string  m_WebServicesUser = "";
		private DataSet dsUsers           = null;
		private DataSet dsAliases         = null;
		private DataSet dsDomains         = null;
		private DataSet dsRouting         = null;
		private DataSet dsSecurity        = null;
		private DataSet dsFilters         = null;

		private DB_Type m_DB_Type  = DB_Type.XML;
		
		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="dataPath"></param>
		public ServerAPI(string dataPath)
		{
			m_DataPath = dataPath;

			DatabaseTypeChanged();

		/*	DataSet dsTmp = new DataSet();
			dsTmp.ReadXml(dataPath + "Settings.xml");

			DataRow dr = dsTmp.Tables["Settings"].Rows[0];
			m_MailStorePath = dr["MailRoot"].ToString();
			m_ConStr        = dr["ConnectionString"].ToString();
			m_DB_Type       = (DB_Type)Enum.Parse(typeof(DB_Type),dr["DataBaseType"].ToString());
			
			dsUsers    = new DataSet();
			dsAliases  = new DataSet();
			dsDomains  = new DataSet();
			dsRouting  = new DataSet();
			dsSecurity = new DataSet();
			dsFilters  = new DataSet();
			
			// We need to load all stuff to memory for XML
			if(m_DB_Type == DB_Type.XML){
				LoadUsers();
				LoadAliases();
				LoadRouting();
				LoadDomains();
				LoadSecurity();				
			}

			LoadFilters();*/
		}

		/// <summary>
		/// For administration only.
		/// </summary>
		/// <param name="dataPath"></param>
		/// <param name="webServicesUrl"></param>
		/// <param name="webServicesUser"></param>
		/// <param name="webServicesPwd"></param>
		public ServerAPI(string dataPath,string webServicesUrl,string webServicesUser,string webServicesPwd)
		{
			m_WebServicesUrl  = webServicesUrl;
			m_WebServicesUser = webServicesUser;
			m_WebServicesPwd  = webServicesPwd;

			m_DB_Type         = DB_Type.WebServices;
		}


		#region DomainName related

		#region function GetDomainList

		/// <summary>
		/// Gets DomainName list.
		/// </summary>
		/// <returns></returns>
		public DataView GetDomainList()
		{
			DataView retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					retVal = new DataView(dsDomains.Tables["DOMAINS"]);
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetDomainList")){
						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Domains";

						return ds.Tables["Domains"].DefaultView;
					}

				#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.GetDomainList().Tables["Domains"].DefaultView;
					}

				#endregion
			}

			return retVal;
		}

		#endregion


		#region function AddDomain

		/// <summary>
		/// Adds new DomainName.
		/// </summary>
		/// <param name="domainName"></param>
		/// <param name="description"></param>
		/// <returns>If successful returns DomainName ID, otherwise null.</returns>
		public DataRow AddDomain(string domainName,string description)
		{
			DataRow retVal = null;

			try
			{
				switch(m_DB_Type)
				{
					#region DB_Type.XML

					case DB_Type.XML:
						
						DataSet dsDomainsCopy = dsDomains.Copy();

						DataRow dr = dsDomainsCopy.Tables["Domains"].NewRow();
						dr["DomainID"]   = Guid.NewGuid().ToString();
						dr["DomainName"]      = domainName;
						dr["Description"] = description;
						
						dsDomainsCopy.Tables["Domains"].Rows.Add(dr);
						dsDomainsCopy.WriteXml(m_DataPath + "Domains.xml",XmlWriteMode.IgnoreSchema);

						retVal = dr;

						break;

					#endregion

					#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddDomain")){
							sqlCmd.AddParameter("@DomainName" ,SqlDbType.NVarChar,domainName);
							sqlCmd.AddParameter("@Description",SqlDbType.NVarChar,description);
							
							DataSet ds = sqlCmd.Execute();
							ds.Tables[0].TableName = "Domains";

							if(ds.Tables["Domains"].Rows.Count > 0){
								return ds.Tables["Domains"].Rows[0];
							}							
						}
						break;

					#endregion

					#region DB_Type.WebServices

					case DB_Type.WebServices:
						using(RemoteAdmin eng = new RemoteAdmin()){
							_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

							return eng.AddDomain(domainName,description).Tables["Domains"].Rows[0];
						}

					#endregion
				}
			}
			catch(Exception x)
			{
				retVal = null;
			}

			return retVal;
		}

		#endregion

		#region function DeleteDomain

		/// <summary>
		/// Deletes specified DomainName.
		/// </summary>
		/// <param name="domainID"></param>
		/// <returns>Returns true if DomainName deleted successfully.</returns>
		public bool DeleteDomain(string domainID)
		{
			try
			{
				switch(m_DB_Type)
				{
					#region DB_Type.XML

					case DB_Type.XML:
						// 1) delete specified DomainName users
						// 2) delete specified DomainName aliases
						// 3) delete specified DomainName routing
						// 4) delete specified DomainName

						DataSet dsUsersCopy   = dsUsers.Copy();
						DataSet dsAliasesCopy = dsAliases.Copy();
						DataSet dsRoutingCopy = dsRouting.Copy();
						DataSet dsDomainsCopy = dsDomains.Copy();
												
						//---- Delete specified DomainName users ----------------------------//
						using(DataView dv = new DataView(dsUsersCopy.Tables["Users"])){
							dv.RowFilter = "DomainID='" + domainID + "'";
			
							if(dv.Count > 0){
								foreach(DataRowView drv in dv){
									drv.Row.Delete();
								}						
							}
						}
						//----------------------------------------------------------------//

						//---- Delete specified DomainName aliases ---------------------------//
						using(DataView dvA = new DataView(dsAliasesCopy.Tables["Aliases"])){
							dvA.RowFilter = "DomainID='" + domainID + "'";
			
							if(dvA.Count > 0){
								foreach(DataRowView drv in dvA){
									drv.Row.Delete();
								}						
							}
						}
						//----------------------------------------------------------------//

						//---- Delete specified DomainName routing ---------------------------//
						using(DataView dvR = new DataView(dsRoutingCopy.Tables["Routing"])){
							dvR.RowFilter = "DomainID='" + domainID + "'";
			
							if(dvR.Count > 0){
								foreach(DataRowView drv in dvR){
									drv.Row.Delete();
								}						
							}
						}
						//----------------------------------------------------------------//

						//---- Delete DomainName itself --------------------------------------//
						using(DataView dv = new DataView(dsDomainsCopy.Tables["Domains"])){
							dv.RowFilter = "DomainID='" + domainID + "'";

							if(dv.Count > 0){
								dsDomainsCopy.Tables["Domains"].Rows.Remove(dv[0].Row);								
							}
						}
						//----------------------------------------------------------------//

						dsDomainsCopy.WriteXml(m_DataPath + "Domains.xml",XmlWriteMode.IgnoreSchema);
						dsUsersCopy.WriteXml  (m_DataPath + "Users.xml"  ,XmlWriteMode.IgnoreSchema);
						dsAliasesCopy.WriteXml(m_DataPath + "Aliases.xml",XmlWriteMode.IgnoreSchema);
						dsRoutingCopy.WriteXml(m_DataPath + "Routing.xml",XmlWriteMode.IgnoreSchema);

						LoadUsers();
						LoadAliases();
						LoadRouting();

						return true;

					#endregion

					#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteDomain")){
							sqlCmd.AddParameter("@DomainID" ,SqlDbType.NVarChar,domainID);
							
							DataSet ds = sqlCmd.Execute();
							return true;					
						}

					#endregion

					#region DB_Type.WebServices

					case DB_Type.WebServices:
						using(RemoteAdmin eng = new RemoteAdmin()){
							_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

							eng.DeleteDomain(domainID);
							return true;
						}

					#endregion
				}
			}
			catch(Exception x)
			{
	//			System.Windows.Forms.MessageBox.Show(x.Message);
			}

			return false;
		}

		#endregion

		#region function DomainExists

		/// <summary>
		/// Checks if specified DomainName exists.
		/// </summary>
		/// <param name="source">DomainName or Emails address.</param>
		/// <returns></returns>
		public bool DomainExists(string source)
		{
			bool retVal = false;

			// Source is Emails
			if(source.IndexOf("@") > -1){
				source = source.Substring(source.IndexOf("@")+1);
			}

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					using(DataView dv = new DataView(dsDomains.Tables["Domains"])){
						dv.RowFilter = "DomainName='" + source + "'";

						if(dv.Count > 0){
							retVal = true;
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DomainExists")){
						sqlCmd.AddParameter("@DomainName",SqlDbType.NVarChar,source);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Domains";

						if(ds.Tables["Domains"].Rows.Count > 0){
							return true;
						}
						else{
							return false;
						}
					}

				#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.DomainExists(source);
					}

				#endregion
			}

			return retVal;
		}

		#endregion

		#endregion


		#region User related

		#region function GetUserList

		/// <summary>
		/// Gets user list in specified DomainName.
		/// </summary>
		/// <param name="domainID">DomainID of Domain which user list to retrieve.To get all use value 'ALL'.</param>
		/// <returns></returns>
		public DataView GetUserList(string domainID)
		{
			DataView retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					DataView dv = new DataView(dsUsers.Tables["Users"]);
					if(domainID != "ALL"){
						dv.RowFilter = "DomainID='" + domainID + "'";
					}
					retVal = dv;
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUserList")){
						if(domainID != "ALL"){
							sqlCmd.AddParameter("@DomainID",SqlDbType.UniqueIdentifier,domainID);
						}

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Users";

						return ds.Tables["Users"].DefaultView;
					}

				#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.GetUserList(domainID).Tables["Users"].DefaultView;
					}

				#endregion
			}

			return retVal;
		}

		#endregion


		#region function AddUser

		/// <summary>
		/// Adds new user to specified DomainName.
		/// </summary>
		/// <param name="fullName">User full name.</param>
		/// <param name="userName">User login name.</param>
		/// <param name="password">User login password.</param>
		/// <param name="Description">User Description.</param>
		/// <param name="emails">User Emails addresses.</param>
		/// <param name="domainID">DomainName ID of DomainName where to add user.</param>
		/// <param name="mailboxSize">Maximum mailbox size.</param>
		/// <param name="enabled">Sepcifies if user is enabled.</param>
		/// <param name="allowRelay">Specifies if user can relay.</param>
		/// <param name="remPop3Accounts">Byte DataSet Pop3RemServSchema or null.</param>
		/// <returns></returns>
		public DataRow AddUser(string fullName,string userName,string password,string Description,string emails,string domainID,int mailboxSize,bool enabled,bool allowRelay,byte[] remPop3Accounts)
		{
			DataRow retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

					case DB_Type.XML:

						DataSet dsUsersCopy = dsUsers.Copy();

						DataRow dr = dsUsersCopy.Tables["Users"].NewRow();
						dr["UserID"]       = Guid.NewGuid().ToString();
						dr["DomainID"]     = domainID;
					//	dr["DomainName"]   = domainName;
						dr["FULLNAME"]     = fullName;
						dr["USERNAME"]     = userName;
						dr["PASSWORD"]     = password;
						dr["Description"]  = Description;
						dr["Emails"]       = emails;
						dr["Mailbox_Size"] = mailboxSize;
						dr["Enabled"]      = enabled;
						dr["AllowRelay"]   = allowRelay;
						
						if(remPop3Accounts != null){
							dr["RemotePop3Servers"] = remPop3Accounts;
						}

						dsUsersCopy.Tables["Users"].Rows.Add(dr);
						dsUsersCopy.WriteXml(m_DataPath + "Users.xml",XmlWriteMode.IgnoreSchema);

						return dr;

					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddUser")){
							sqlCmd.AddParameter("@FullName"          ,SqlDbType.NVarChar,fullName);
							sqlCmd.AddParameter("@UserName"          ,SqlDbType.NVarChar,userName);
							sqlCmd.AddParameter("@Password"          ,SqlDbType.NVarChar,password);
							sqlCmd.AddParameter("@Description"       ,SqlDbType.NVarChar,Description);
							sqlCmd.AddParameter("@Emails"            ,SqlDbType.NVarChar,emails);
							sqlCmd.AddParameter("@DomainID"          ,SqlDbType.NVarChar,domainID);
							sqlCmd.AddParameter("@MailboxSize"       ,SqlDbType.NVarChar,mailboxSize);
							sqlCmd.AddParameter("@Enabled"           ,SqlDbType.Bit,enabled);
							sqlCmd.AddParameter("@AllowRelay"        ,SqlDbType.Bit,allowRelay);
							sqlCmd.AddParameter("@RemotePop3Servers" ,SqlDbType.Image,remPop3Accounts);
							
							DataSet ds = sqlCmd.Execute();
							ds.Tables[0].TableName = "Users";

							if(ds.Tables["Users"].Rows.Count > 0){
								return ds.Tables["Users"].Rows[0];
							}							
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.AddUser(fullName,userName,password,Description,emails,domainID,mailboxSize,enabled,allowRelay,remPop3Accounts).Tables["Users"].Rows[0];
					}

				#endregion
			}

			return retVal;
		}

		#endregion

		#region function DeleteUser

		/// <summary>
		/// Deletes user.
		/// </summary>
		/// <param name="userID">UserID of the user which to delete.</param>
		public void DeleteUser(string userID)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
                    DataSet dsUsersCopy = dsUsers.Copy();					
					using(DataView dv = new DataView(dsUsersCopy.Tables["Users"])){
						dv.RowFilter = "UserID='" + userID + "'";

						if(dv.Count > 0){
							dsUsersCopy.Tables["Users"].Rows.Remove(dv[0].Row);							
						}

						dsUsersCopy.WriteXml(m_DataPath + "Users.xml",XmlWriteMode.IgnoreSchema);

						// ToDo delete user folders

					}
					break;

				#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteUser")){
							sqlCmd.AddParameter("@UserID" ,SqlDbType.NVarChar,userID);
							
							DataSet ds = sqlCmd.Execute();
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						eng.DeleteUser(userID);
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function UpdateUser

		/// <summary>
		/// Updates new user to specified DomainName.
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="fullName">User full name.</param>
		/// <param name="password">User login password.</param>
		/// <param name="Description">User Description.</param>
		/// <param name="emails">User Emails addresses.</param>
		/// <param name="domainID">DomainName ID of DomainName where to add user.</param>
		/// <param name="mailboxSize">Maximum mailbox size.</param>
		/// <param name="enabled">Sepcifies if user is enabled.</param>
		/// <param name="allowRelay">Specifies if user can relay.</param>
		/// <param name="remPop3Accounts">Byte DataSet Pop3RemServSchema or null.</param>
		public void UpdateUser(string userID,string fullName,string password,string Description,string emails,string domainID,int mailboxSize,bool enabled,bool allowRelay,byte[] remPop3Accounts)
		{		
			switch(m_DB_Type)
			{
				#region DB_Type.XML

					case DB_Type.XML:
						DataSet dsUsersCopy = dsUsers.Copy();
						using(DataView dv = new DataView(dsUsersCopy.Tables["Users"])){
							dv.RowFilter = "UserID='" + userID + "'";

							if(dv.Count > 0){
								dv[0]["FULLNAME"]     = fullName;
								dv[0]["PASSWORD"]     = password;
								dv[0]["Description"]  = Description;
								dv[0]["Emails"]       = emails;
								dv[0]["DomainID"]     = domainID;
								dv[0]["Mailbox_Size"] = mailboxSize;
								dv[0]["Enabled"]      = enabled;
								dv[0]["AllowRelay"]   = allowRelay;

								if(remPop3Accounts != null){
									dv[0]["RemotePop3Servers"] = remPop3Accounts;
								}
								else{
									dv[0]["RemotePop3Servers"] = DBNull.Value;
								}
							}

							dsUsersCopy.WriteXml(m_DataPath + "Users.xml",XmlWriteMode.IgnoreSchema);
						}
						break;
					
					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateUser")){
							sqlCmd.AddParameter("@UserID"            ,SqlDbType.NVarChar,userID);
							sqlCmd.AddParameter("@FullName"          ,SqlDbType.NVarChar,fullName);
							sqlCmd.AddParameter("@Password"          ,SqlDbType.NVarChar,password);
							sqlCmd.AddParameter("@Description"       ,SqlDbType.NVarChar,Description);
							sqlCmd.AddParameter("@Emails"            ,SqlDbType.NVarChar,emails);
							sqlCmd.AddParameter("@DomainID"          ,SqlDbType.NVarChar,domainID);
							sqlCmd.AddParameter("@MailboxSize"       ,SqlDbType.NVarChar,mailboxSize);
							sqlCmd.AddParameter("@Enabled"           ,SqlDbType.Bit,enabled);
							sqlCmd.AddParameter("@AllowRelay"        ,SqlDbType.Bit,allowRelay);
							sqlCmd.AddParameter("@RemotePop3Servers" ,SqlDbType.Image,remPop3Accounts);
							
							DataSet ds = sqlCmd.Execute();
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						eng.UpdateUser(userID,fullName,password,Description,emails,domainID,mailboxSize,enabled,allowRelay,remPop3Accounts);
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function MailboxExists

		/// <summary>
		/// Checks if mailbox exists.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <returns></returns>
		public bool MailboxExists(string userName)
		{
			bool retVal = false;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					using(DataView dv = new DataView(dsUsers.Tables["Users"])){
						dv.RowFilter = "USERNAME='" + userName + "'";

						if(dv.Count > 0){
							retVal = true;
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UserExists")){
						sqlCmd.AddParameter("@UserName",SqlDbType.NVarChar,userName);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Users";

						if(ds.Tables["Users"].Rows.Count > 0){
							return true;
						}
						else{
							return false;
						}
					}

				#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.MailboxExists(userName);
					}

				#endregion
			}

			return retVal;
		}

		#endregion
		
		#region function EmailAddressExists

		/// <summary>
		/// Checks if specifeid Emails address belongs to somebody in this server.
		/// </summary>
		/// <param name="emailAddress">Emails address which to check.</param>
		/// <returns>Returns true if Emails address is found.</returns>
		public bool EmailAddressExists(string emailAddress)
		{
			bool retVal = false;

			switch(m_DB_Type)
			{
				#region DB_Type.XML
				
				case DB_Type.XML:
					emailAddress = emailAddress.Replace("*","[*]");
					emailAddress = emailAddress.Replace("%","[%]");
			
					using(DataView dv = new DataView(dsUsers.Tables["Users"])){
						dv.RowFilter = "Emails LIKE '*[<]" + emailAddress + "[>]*'";

						if(dv.Count > 0){
							retVal = true;
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_EmailAddressExists")){
						sqlCmd.AddParameter("@EmailAddress",SqlDbType.NVarChar,emailAddress);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Users";

						if(ds.Tables["Users"].Rows.Count > 0){
							return true;
						}
						else{
							return false;
						}
					}

				#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.EmailAddressExists(emailAddress);
					}

				#endregion
			}

			return retVal;
		}

		#endregion

		#region function MapUser

		/// <summary>
		/// Maps Emails address to mailbox.
		/// </summary>
		/// <param name="emailAddress"></param>
		/// <returns>Returns mailbox or null if map failed.</returns>
		public string MapUser(string emailAddress)
		{
			string retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					emailAddress = emailAddress.Replace("*","[*]");
					emailAddress = emailAddress.Replace("%","[%]");

					using(DataView dv = new DataView(dsUsers.Tables["Users"])){
						dv.RowFilter = "Emails LIKE '*[<]" + emailAddress + "[>]*'";
					
						if(dv.Count > 0){
							// UserName == MailBoxName, return MailBoxName
							retVal = dv[0].Row["USERNAME"].ToString();
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_EmailAddressExists")){
						sqlCmd.AddParameter("@EmailAddress",SqlDbType.NVarChar,emailAddress);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Users";

						if(ds.Tables["Users"].Rows.Count > 0){
							return ds.Tables["Users"].Rows[0]["USERNAME"].ToString();
						}
					}
					break;

				#endregion
			}

			return retVal;
		}

		#endregion

		#region function ValidateMailboxSize

		/// <summary>
		/// Checks if specified mailbox size is exceeded.
		/// </summary>
		/// <param name="mailbox"></param>
		/// <returns>Returns true if exceeded.</returns>
		public bool ValidateMailboxSize(string mailbox)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					using(DataView dv = new DataView(dsUsers.Tables["Users"])){
						dv.RowFilter = "USERNAME='" + mailbox + "'";

						//--- Check if user directory exists ----//
						if(!Directory.Exists(m_MailStorePath + "Mailboxes\\" + mailbox)){
							try{
								Directory.CreateDirectory(m_MailStorePath + "Mailboxes\\" + mailbox);
							}
							catch{}
						}
						//---------------------------------------//
		
						if(dv.Count > 0){
							int     maxSize   = Convert.ToInt32(dv[0]["Mailbox_Size"]);
							decimal sizeTotal = 0;

							DirectoryInfo dirInf = new DirectoryInfo(m_MailStorePath + "Mailboxes\\" + mailbox);
							foreach(FileInfo file in dirInf.GetFiles()){
								sizeTotal += file.Length;

								// If maximum size exceeded, return true
								if(sizeTotal > maxSize*1000000){
									return true;
								}
							}
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_ValidateMailboxSize")){
						sqlCmd.AddParameter("@MailBox",SqlDbType.NVarChar,mailbox);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Detail";

						if(ds.Tables["Detail"].Rows.Count > 0){
							return !Convert.ToBoolean(ds.Tables["Detail"].Rows[0]["VALIDATED"]);
						}
					}
					break;

				#endregion
			}

			return false;
		}

		#endregion

		#region function AuthUser

		/// <summary>
		/// Authenticates user.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <param name="passwData">Password data.</param>
		/// <param name="authData">Authentication specific data(as tag).</param>
		/// <param name="authType">Authentication type.</param>
		/// <returns></returns>
		public bool AuthUser(string userName,string passwData,string authData,AuthType authType)
		{
			bool retVal = false;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					using(DataView dv = new DataView(dsUsers.Tables["Users"])){
						dv.RowFilter = "USERNAME='" + userName + "' AND Enabled=true";

						// User exists, try to authenticate user.
						if(dv.Count > 0){
							string password = dv[0]["PASSWORD"].ToString().ToLower();

							switch(authType)
							{
								case AuthType.APOP:
									byte[] data = System.Text.Encoding.ASCII.GetBytes(authData + password);
			
									MD5 md5 = new MD5CryptoServiceProvider();			
									byte[] hash = md5.ComputeHash(data);

									string hexHash = BitConverter.ToString(hash).ToLower().Replace("-","");

									if(hexHash == passwData){
										retVal = true;
									}
									break;

								case AuthType.CRAM_MD5:			
									HMACMD5 kMd5 = new HMACMD5(System.Text.Encoding.ASCII.GetBytes(password));			
									byte[] hash1 = kMd5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(authData));

									string hexHash1 = BitConverter.ToString(hash1).ToLower().Replace("-","");

									if(hexHash1 == passwData){
										return true;
									}
									break;

								case AuthType.Plain:
									if(password == passwData.ToLower()){
										return true;
									}
									break;
							}							
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AuthUser")){
						sqlCmd.AddParameter("@UserName",SqlDbType.NVarChar,userName);
						
						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Users";

						if(ds.Tables["Users"].Rows.Count > 0){
							string password = ds.Tables["Users"].Rows[0]["PASSWORD"].ToString().ToLower();

							switch(authType)
							{
								case AuthType.APOP:									
									byte[] data = System.Text.Encoding.ASCII.GetBytes(authData + password);
			
									MD5 md5 = new MD5CryptoServiceProvider();			
									byte[] hash = md5.ComputeHash(data);

									string hexHash = BitConverter.ToString(hash).ToLower().Replace("-","");

									if(hexHash == passwData){
										return true;
									}

									break;

								case AuthType.CRAM_MD5:			
									HMACMD5 kMd5 = new HMACMD5(System.Text.Encoding.ASCII.GetBytes(password));			
									byte[] hash1 = kMd5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(authData));

									string hexHash1 = BitConverter.ToString(hash1).ToLower().Replace("-","");

									if(hexHash1 == passwData){
										return true;
									}
									break;

								case AuthType.Plain:
									if(password.ToLower() == passwData.ToLower()){
										return true;
									}
									break;
							}
						}
						break;
					}

				#endregion
			}

			return retVal;
		}

		#endregion


		#region function GetUserRemotePop3Servers

		/// <summary>
		/// Gets user pop3 remote server accounts.
		/// </summary>
		/// <param name="userName">User name which remote pop3 accounts to get.</param>
		/// <returns></returns>
		public DataSet GetUserRemotePop3Servers(string userName)
		{
			DataSet ds = new DataSet();
			CreatePop3RemServSchema(ds);

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					using(DataView dv = new DataView(dsUsers.Tables["Users"])){
						dv.RowFilter = "UserName='" + userName + "'";
					
						if(dv.Count > 0){							
							if(!dv[0].Row.IsNull("RemotePop3Servers")){								
								ds.ReadXml(new MemoryStream((byte[])dv[0].Row["RemotePop3Servers"]));

								return ds;
							}
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUserProperties")){
						sqlCmd.AddParameter("@UserName",SqlDbType.NVarChar,userName);

						DataSet dsX = sqlCmd.Execute();
						dsX.Tables[0].TableName = "Users";

						if(dsX.Tables["Users"].Rows.Count > 0){
							if(!dsX.Tables["Users"].Rows[0].IsNull("RemotePop3Servers")){								
								ds.ReadXml(new MemoryStream((byte[])dsX.Tables["Users"].Rows[0]["RemotePop3Servers"]));

								return ds;
							}
						}
					}
					break;

				#endregion
			}

			return ds;
		}

		#endregion

		#endregion

		#region Alias related

		#region function GetAliasesList

		/// <summary>
		/// Gets aliases.
		/// </summary>
		/// <param name="DomainName"></param>
		/// <returns></returns>
		public DataView GetAliasesList(string DomainName)
		{
			DataView retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					retVal = dsAliases.Tables["Aliases"].DefaultView;
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetAliasesList")){
				//		sqlCmd.AddParameter("@DomainName",SqlDbType.NVarChar,source);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Aliases";

						return ds.Tables["Aliases"].DefaultView;
					}

				#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.GetAliasesList(DomainName).Tables["Aliases"].DefaultView;
					}

				#endregion
			}

			return retVal;
		}

		#endregion
		

		#region function AddAlias

		/// <summary>
		/// Adds AliasName(mailing list).
		/// </summary>
		/// <param name="aliasName">AliasName name. eg. all@lumisoft.ee</param>
		/// <param name="Description">AliasName Description.</param>
		/// <param name="AliasMembers">AliasName AliasMembers.</param>
		/// <param name="domainID">DomainID where AliasName belongs.</param>
		/// <param name="isPublic">Specifies if accessible to public or only authenticated users.</param>
		/// <returns></returns>
		public DataRow AddAlias(string aliasName,string Description,string AliasMembers,string domainID,bool isPublic)
		{
			DataRow retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

					case DB_Type.XML:

						DataSet dsAliasesCopy = dsAliases.Copy();

						DataRow dr = dsAliasesCopy.Tables["Aliases"].NewRow();
						dr["AliasID"]      = Guid.NewGuid().ToString();
						dr["AliasName"]    = aliasName;
						dr["Description"]  = Description;
						dr["AliasMembers"] = AliasMembers;
						dr["DomainID"]     = domainID;
						dr["IsPublic"]     = isPublic;

						dsAliasesCopy.Tables["Aliases"].Rows.Add(dr);
						dsAliasesCopy.WriteXml(m_DataPath + "Aliases.xml",XmlWriteMode.IgnoreSchema);

						retVal = dr;

						break;

					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddAlias")){
							sqlCmd.AddParameter("@AliasName"   ,SqlDbType.NVarChar,aliasName);							
							sqlCmd.AddParameter("@Description" ,SqlDbType.NVarChar,Description);
							sqlCmd.AddParameter("@Members"     ,SqlDbType.NVarChar,AliasMembers);
							sqlCmd.AddParameter("@DomainID"    ,SqlDbType.UniqueIdentifier,domainID);
							sqlCmd.AddParameter("@IsPublic"    ,SqlDbType.Bit,isPublic);
							
							DataSet ds = sqlCmd.Execute();
							ds.Tables[0].TableName = "Aliases";

							if(ds.Tables["Aliases"].Rows.Count > 0){
								return ds.Tables["Aliases"].Rows[0];
							}							
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.AddAlias(aliasName,Description,AliasMembers,domainID,isPublic).Tables["Aliases"].Rows[0];
					}

				#endregion
			}

			return retVal;
		}

		#endregion

		#region function DeleteAlias

		/// <summary>
		/// Deletes specified AliasName.
		/// </summary>
		/// <param name="aliasID"></param>
		/// <returns></returns>
		public void DeleteAlias(string aliasID)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

					case DB_Type.XML:
						DataSet dsAliasesCopy = dsAliases.Copy();
						using(DataView dv = new DataView(dsAliasesCopy.Tables["Aliases"])){
							dv.RowFilter = "AliasID='" + aliasID + "'";

							if(dv.Count > 0){
								dsAliasesCopy.Tables["Aliases"].Rows.Remove(dv[0].Row);							
							}

							dsAliasesCopy.WriteXml(m_DataPath + "Aliases.xml",XmlWriteMode.IgnoreSchema);
						}
						break;

					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteAlias")){
							sqlCmd.AddParameter("@AliasID" ,SqlDbType.UniqueIdentifier,aliasID);
							
							DataSet ds = sqlCmd.Execute();
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						eng.DeleteAlias(aliasID);
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function UpdateAlias

		/// <summary>
		/// Updates AliasName.
		/// </summary>
		/// <param name="aliasID"></param>
		/// <param name="aliasName">AliasName name. eg. all@lumisoft.ee</param>
		/// <param name="Description">AliasName Description.</param>
		/// <param name="AliasMembers"></param>
		/// <param name="domainID">DomainID where AliasName belongs.</param>
		/// <param name="isPublic">Specifies if accessible to public or only authenticated users.</param>
		public void UpdateAlias(string aliasID,string aliasName,string Description,string AliasMembers,string domainID,bool isPublic)
		{ 
			switch(m_DB_Type)
			{
				#region DB_Type.XML

					case DB_Type.XML:
						DataSet dsAliasesCopy = dsAliases.Copy();
						using(DataView dv = new DataView(dsAliasesCopy.Tables["Aliases"])){
							dv.RowFilter = "AliasID='" + aliasID + "'";

							if(dv.Count > 0){
								dv[0]["AliasName"]    = aliasName;
								dv[0]["Description"]  = Description;
								dv[0]["AliasMembers"] = AliasMembers;
								dv[0]["DomainID"]     = domainID;
								dv[0]["IsPublic"]     = isPublic;
							}

							dsAliasesCopy.WriteXml(m_DataPath + "Aliases.xml",XmlWriteMode.IgnoreSchema);
						}
						break;

					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateAlias")){
							sqlCmd.AddParameter("@AliasID"     ,SqlDbType.UniqueIdentifier,aliasID);
							sqlCmd.AddParameter("@AliasName"   ,SqlDbType.NVarChar,aliasName);							
							sqlCmd.AddParameter("@Description" ,SqlDbType.NVarChar,Description);
							sqlCmd.AddParameter("@Members"     ,SqlDbType.NVarChar,AliasMembers);
							sqlCmd.AddParameter("@DomainID"    ,SqlDbType.UniqueIdentifier,domainID);							
							sqlCmd.AddParameter("@IsPublic"    ,SqlDbType.Bit,isPublic);
							
							DataSet ds = sqlCmd.Execute();
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						eng.UpdateAlias(aliasID,aliasName,Description,AliasMembers,domainID,isPublic);
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function GetAliasMembers

		/// <summary>
		/// Gets AliasName AliasMembers.
		/// </summary>
		/// <param name="emailAddress"></param>
		/// <returns>Return null, if AliasName not found.</returns>
		public string[] GetAliasMembers(string emailAddress)
		{
			string[] retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:                
					using(DataView dv = new DataView(dsAliases.Tables["Aliases"])){
						dv.RowFilter = "AliasName = '" + emailAddress + "'";
			
						if(dv.Count > 0){
							string AliasMembers = dv[0]["AliasMembers"].ToString();	
							retVal = AliasMembers.Split(new char[]{';'});
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetAlias")){
						sqlCmd.AddParameter("@AliasName",SqlDbType.NVarChar,emailAddress);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Aliases";

						if(ds.Tables["Aliases"].Rows.Count > 0){
							string AliasMembers = ds.Tables["Aliases"].Rows[0]["AliasMembers"].ToString();	
							return AliasMembers.Split(new char[]{';'});
						}
					}
					break;

				#endregion
			}

			return retVal;
		}

		#endregion

		#region function IsAliasPublic

		/// <summary>
		/// Checks if alias is accessible for public(non authenticated users).
		/// </summary>
		/// <param name="emailAddress"></param>
		/// <returns></returns>
		public bool IsAliasPublic(string emailAddress)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:                
					using(DataView dv = new DataView(dsAliases.Tables["Aliases"])){
						dv.RowFilter = "AliasName = '" + emailAddress + "'";
			
						if(dv.Count > 0){
							return Convert.ToBoolean(dv[0]["IsPublic"]);
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetAlias")){
						sqlCmd.AddParameter("@AliasName",SqlDbType.NVarChar,emailAddress);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Aliases";

						if(ds.Tables["Aliases"].Rows.Count > 0){
							return Convert.ToBoolean(ds.Tables["Aliases"].Rows[0]["IsPublic"]);
						}
					}
					break;

				#endregion
			}

			return false;
		}

		#endregion

		#endregion

		#region Routing related

		#region function GetRouteList

		/// <summary>
		/// Gets Emails address routes.
		/// </summary>
		/// <returns></returns>
		public DataView GetRouteList()
		{
			
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					if(!dsRouting.Tables["Routing"].Columns.Contains("Length")){
						dsRouting.Tables["Routing"].Columns.Add("Length",Type.GetType("System.Int32"),"Len(Pattern)");
					}
					DataView dv = new DataView(dsRouting.Tables["Routing"]);
					dv.Sort = "DomainID ASC,Length DESC,PATTERN DESC";
					return dv;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetRouteList")){
				//		sqlCmd.AddParameter("@DomainName",SqlDbType.NVarChar,source);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Routing";

						if(!ds.Tables["Routing"].Columns.Contains("Length")){
							ds.Tables["Routing"].Columns.Add("Length",Type.GetType("System.Int32"),"Len(Pattern)");
						}

						ds.Tables["Routing"].DefaultView.Sort = "DomainID ASC,Length DESC,PATTERN DESC";
						return ds.Tables["Routing"].DefaultView;
					}

				#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.GetRouteList().Tables["Routing"].DefaultView;
					}

				#endregion
			}

			return null;
		}

		#endregion


		#region function AddRoute

		/// <summary>
		/// Adds new Emails route.
		/// </summary>
		/// <param name="pattern">Match pattern.</param>
		/// <param name="mailbox">Mailbox to route.</param>
		/// <param name="Description">Description.</param>
		/// <param name="domainID">DomainName ID.</param>
		/// <returns></returns>
		public DataRow AddRoute(string pattern,string mailbox,string Description,string domainID)
		{
			DataRow retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					//-- Find domainName from domainID ---------------------------//
					string domainName = "";
					foreach(DataRowView drV in this.GetDomainList()){
						if(drV["DomainID"].ToString().ToUpper() == domainID.ToUpper()){
							domainName = drV["DomainName"].ToString();
						}
					}

					DataSet dsRoutingCopy = dsRouting.Copy();

					DataRow dr = dsRoutingCopy.Tables["Routing"].NewRow();
					dr["RouteID"]     = Guid.NewGuid().ToString();
					dr["Pattern"]     = pattern;
					dr["Mailbox"]     = mailbox;
					dr["Description"] = Description;
					dr["DomainID"]    = domainID;
					dr["DomainName"]  = domainName;
					
					dsRoutingCopy.Tables["Routing"].Rows.Add(dr);
					dsRoutingCopy.Tables["Routing"].Columns.Remove("Length");
					dsRoutingCopy.WriteXml(m_DataPath + "Routing.xml",XmlWriteMode.IgnoreSchema);

					if(!dsRoutingCopy.Tables["Routing"].Columns.Contains("Length")){
						dsRoutingCopy.Tables["Routing"].Columns.Add("Length",Type.GetType("System.String"),"Len(Pattern)");
					}

					retVal = dr;

					break;

				#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddRoute")){
							sqlCmd.AddParameter("@Pattern"     ,SqlDbType.NVarChar         ,pattern);							
							sqlCmd.AddParameter("@Mailbox"     ,SqlDbType.NVarChar         ,mailbox);
							sqlCmd.AddParameter("@Description" ,SqlDbType.NVarChar         ,Description);
							sqlCmd.AddParameter("@DomainID"    ,SqlDbType.UniqueIdentifier ,domainID);
							
							DataSet ds = sqlCmd.Execute();
							ds.Tables[0].TableName = "Routing";

							if(ds.Tables["Routing"].Rows.Count > 0){
								return ds.Tables["Routing"].Rows[0];
							}							
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.AddRoute(pattern,mailbox,Description,domainID).Tables["Routing"].Rows[0];
					}

				#endregion
			}

			return retVal;
		}

		#endregion

		#region function DeleteRoute

		/// <summary>
		/// Deletes route.
		/// </summary>
		/// <param name="routeID"></param>
		public void DeleteRoute(string routeID)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					DataSet dsRoutingCopy = dsRouting.Copy();
					using(DataView dv = new DataView(dsRoutingCopy.Tables["Routing"])){
						dv.RowFilter = "RouteID='" + routeID + "'";

						if(dv.Count > 0){
							dsRoutingCopy.Tables["Routing"].Rows.Remove(dv[0].Row);							
						}

						dsRoutingCopy.Tables["Routing"].Columns.Remove("Length");
						dsRoutingCopy.WriteXml(m_DataPath + "Routing.xml",XmlWriteMode.IgnoreSchema);
					}
					break;

				#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteRoute")){
							sqlCmd.AddParameter("@RouteID" ,SqlDbType.UniqueIdentifier,routeID);
							
							DataSet ds = sqlCmd.Execute();
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						eng.DeleteRoute(routeID);
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function UpdateRoute

		/// <summary>
		/// Updates Emails route.
		/// </summary>
		/// <param name="routeID"></param>
		/// <param name="pattern"></param>
		/// <param name="mailbox"></param>
		/// <param name="Description"></param>
		/// <param name="domainID"></param>
		public void UpdateRoute(string routeID,string pattern,string mailbox,string Description,string domainID)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

					case DB_Type.XML:
						//-- Find domainName from domainID ---------------------------//
						string domainName = "";
						foreach(DataRow drx in dsDomains.Tables["Domains"].Rows){
							if(drx["DomainID"].ToString() == domainID){
								domainName = drx["DomainName"].ToString();
							}
						}

						DataSet dsRoutingCopy = dsRouting.Copy();
						using(DataView dv = new DataView(dsRoutingCopy.Tables["Routing"])){
							dv.RowFilter = "RouteID='" + routeID + "'";

							if(dv.Count > 0){
								dv[0]["Pattern"]     = pattern;
								dv[0]["Mailbox"]     = mailbox;
								dv[0]["Description"] = Description;
								dv[0]["DomainID"]    = domainID;
								dv[0]["DomainName"]  = domainName;
							}

							dsRoutingCopy.Tables["Routing"].Columns.Remove("Length");
							dsRoutingCopy.WriteXml(m_DataPath + "Routing.xml",XmlWriteMode.IgnoreSchema);
						}
						break;

					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateRoute")){
							sqlCmd.AddParameter("@RouteID"     ,SqlDbType.UniqueIdentifier,routeID);
							sqlCmd.AddParameter("@Pattern"     ,SqlDbType.NVarChar,pattern);							
							sqlCmd.AddParameter("@Mailbox"     ,SqlDbType.NVarChar,mailbox);
							sqlCmd.AddParameter("@Description" ,SqlDbType.NVarChar,Description);
							sqlCmd.AddParameter("@DomainID"    ,SqlDbType.UniqueIdentifier,domainID);
							
							DataSet ds = sqlCmd.Execute();
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						eng.UpdateRoute(routeID,pattern,mailbox,Description,domainID);
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function GetMailboxFromPattern

		/// <summary>
		/// Gets mailbox from pattern.
		/// </summary>
		/// <param name="emailAddress"></param>
		/// <returns>Returns mailbox,if any match or null for no match.</returns>
		public string GetMailboxFromPattern(string emailAddress)
		{			
			string localPart = "";
		    string DomainName    = "";
			if(emailAddress.IndexOf("@") > -1){
				localPart = emailAddress.Substring(0,emailAddress.IndexOf("@"));
				DomainName    = emailAddress.Substring(emailAddress.IndexOf("@")+1);
			}
			else{
				return null; // Emails address is invalid.
			}

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:

					//-- Find domainID for domainName ---------------------------//
					string domainID = null;
					foreach(DataRow dr in dsDomains.Tables["Domains"].Rows){
						if(dr["DomainName"].ToString().ToUpper() == DomainName.ToUpper()){
							domainID = dr["DomainID"].ToString();
						}
					}
					// we didin't find any suitable domin
					if(domainID == null){
						return null;
					}
					//----------------------------------------------------------//

					// Find routing patterns for this DomainName
					using(DataView dv  = this.GetRouteList()){
						dv.RowFilter = "DomainID='" + domainID + "'";

						// Start trying patterns from  0 to ++ ...
						foreach(DataRowView vDr in dv){
							string pattern = vDr["PATTERN"].ToString();
							string mailbox = vDr["MAILBOX"].ToString();

							// Exact address routing
							if(pattern.IndexOf("*") == -1 && localPart.ToLower() == pattern.ToLower()){
								return mailbox;
							}

							// Route all messages
							if(pattern == "*"){
								return mailbox;
							}

							if(pattern.StartsWith("*")){
								if(pattern.EndsWith("*")){
									if(localPart.ToUpper().IndexOf(pattern.Replace("*","").ToUpper()) > -1){
										return mailbox;
									}
								}
								else{
									if(localPart.ToUpper().EndsWith(pattern.Replace("*","").ToUpper())){
										return mailbox;
									}
								}
							}
							else if(pattern.EndsWith("*")){
								if(localPart.ToUpper().StartsWith(pattern.Replace("*","").ToUpper())){
									return mailbox;
								}
							}
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:				
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMailboxFromPattern")){
						sqlCmd.AddParameter("@DomainName",SqlDbType.NVarChar,DomainName);
						sqlCmd.AddParameter("@LocalPart" ,SqlDbType.NVarChar,localPart);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Detail";

						if(ds.Tables["Detail"].Rows.Count > 0 && !ds.Tables["Detail"].Rows[0].IsNull("Mailbox")){
							return ds.Tables["Detail"].Rows[0]["Mailbox"].ToString();
						}
					}
					break;

				#endregion
			}

			return null;
		}

		#endregion

		#endregion


		#region MailStore related

		#region function GetMessageList

		/// <summary>
		/// Gets Inbox messages info for specified user mailbox.
		/// </summary>
		/// <param name="mailBox"></param>
		/// <param name="msgs"></param>
		public void GetMessageList(string mailBox,LumiSoft.Net.POP3.Server.POP3_Messages msgs)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					string path = m_MailStorePath + "Mailboxes\\" + mailBox + "\\Inbox\\";

					// Check if Directory exists, if not Create
					if(!Directory.Exists(path)){
						Directory.CreateDirectory(path);
					}
			
					string[] files = Directory.GetFiles(path,"*.eml");

					foreach(string file in files){
						int messageSize = 0;
						using(FileStream fStream = File.OpenRead(file)){
							messageSize = (int)fStream.Length;
						}

						msgs.AddMessage(Path.GetFileNameWithoutExtension(file),messageSize);
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMessageList")){
						sqlCmd.AddParameter("@Mailbox",SqlDbType.NVarChar,mailBox);
						sqlCmd.AddParameter("@Folder" ,SqlDbType.NVarChar,"Inbox");

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "lsMailStore";

						foreach(DataRow dr in ds.Tables["lsMailStore"].Rows){
							string messageID = dr["MessageID"].ToString();
							int    size      = Convert.ToInt32(dr["Size"]);
							msgs.AddMessage(messageID,size);
						}
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function GetMessagesInfo

		/// <summary>
		/// Gets specified IMAP folder's messges info.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="folder">IMAP folder which messages info to get. Eg. 'Inbox'.</param>
		/// <param name="messages"></param>
		public void GetMessagesInfo(string userName,string folder,LumiSoft.Net.IMAP.Server.IMAP_Messages messages)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					string path = m_MailStorePath + "Mailboxes\\" + userName + "\\" + folder;

					// Check if Directory exists, if not Create for Inbox only
					if(!Directory.Exists(path)){
						if(folder.ToLower().Trim() == "inbox"){
							Directory.CreateDirectory(path);
						}
						else{
							throw new Exception("Folder '" + folder + "' doesn't exist");
						}
					}
			
					string[] files = Directory.GetFiles(path,"*.eml");

					foreach(string file in files){
						int messageSize = 0;
						using(FileStream fStream = File.OpenRead(file)){
							messageSize = (int)fStream.Length;
						}

						// date[yyyyMMddHHmmss]_int[uid]_int[flags]
						string[] fileParts = Path.GetFileNameWithoutExtension(file).Split('_');
						DateTime recieveDate = DateTime.ParseExact(fileParts[0],"yyyyMMddHHmmss",System.Globalization.DateTimeFormatInfo.InvariantInfo);
						int      uid         = Convert.ToInt32(fileParts[1]);
						LumiSoft.Net.IMAP.Server.IMAP_MessageFlags flags = (LumiSoft.Net.IMAP.Server.IMAP_MessageFlags)Enum.Parse(typeof(LumiSoft.Net.IMAP.Server.IMAP_MessageFlags),fileParts[2]);

						messages.AddMessage(Path.GetFileNameWithoutExtension(file),uid,flags,messageSize,recieveDate);
					}

					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMessageList")){
						sqlCmd.AddParameter("@Mailbox",SqlDbType.NVarChar,userName);
						sqlCmd.AddParameter("@Folder" ,SqlDbType.NVarChar,folder);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "lsMailStore";

						foreach(DataRow dr in ds.Tables["lsMailStore"].Rows){
							string   messageID = dr["MessageID"].ToString();
							int      size      = Convert.ToInt32(dr["Size"]);
							DateTime date      = Convert.ToDateTime(dr["Date"]); 
							int      flags     = Convert.ToInt32(dr["MessageFlags"]);
							int      uid       = Convert.ToInt32(dr["UID"]);
							messages.AddMessage(messageID,uid,(IMAP_MessageFlags)flags,size,date);
						}
					}
					break;

				#endregion
			}
		}

		#endregion


		#region function StoreMessage

		/// <summary>
		/// Stores message to specified mailbox.
		/// </summary>
		/// <param name="mailbox">Mailbox name.</param>
		/// <param name="folder">Folder where to store message. Eg. 'Inbox'.</param>
		/// <param name="msgStream">Stream where message has stored.</param>
		/// <param name="to">Recipient email address.</param>
		/// <param name="from">Sendred email address.</param>
		/// <param name="isRelay">Specifies if message must be relayed.</param>
		/// <param name="date">Recieve date.</param>
		/// <param name="flags">Message flags.</param>
		public void StoreMessage(string mailbox,string folder,MemoryStream msgStream,string to,string from,bool isRelay,DateTime date,IMAP_MessageFlags flags)
		{
			// Store relay messages locally
			if(isRelay){
				// Create dummy file name
				string filename = Guid.NewGuid().ToString();
				       filename = filename.Substring(0,22);
				       filename = filename.Replace("-","_");
				
				string path = m_MailStorePath + "Relay\\";

				// Check if Directory exists, if not Create
				if(!Directory.Exists(path)){
					Directory.CreateDirectory(path);
				}

				//---- Write message data to file -------------------------------//
				using(FileStream fStream = File.Create(path + "\\" + filename + ".eml",(int)msgStream.Length)){

					// Write internal relay info line at the beginning of messsage.
					// Note: This line is skipped when sending to destination host,
					// actual message begins from 2 line.
					// Header struct: 'RelayInfo:IsUndeliveredWarningSent<TAB>To<TAB>Sender<TAB>Date\r\n'
					string internalServerHead = "RelayInfo:0\t" + to + "\t" + from + "\t" + DateTime.Now.ToString("r",System.Globalization.DateTimeFormatInfo.InvariantInfo) + "\r\n";
					byte[] sHead = System.Text.Encoding.Default.GetBytes(internalServerHead);
					fStream.Write(sHead,0,sHead.Length);

					msgStream.WriteTo(fStream);
				}
				//---------------------------------------------------------------//

				return; // !!! 
			}

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:					
					string path = m_MailStorePath + "MailBoxes\\" + mailbox + "\\" + folder + "\\";

					// Check if Directory exists, if not Create for Inbox only
					if(!Directory.Exists(path)){
						if(folder.ToLower().Trim() == "inbox"){
							Directory.CreateDirectory(path);
						}
						else{
							throw new Exception("Folder '" + folder + "' doesn't exist");
						}
					}

                    string fileName = date.ToString("yyyyMMddHHmmss") + "_" + GetNextUid(mailbox,folder) + "_" + Convert.ToString((int)flags);
											
					//---- Write message data to file -------------------------------//
					using(FileStream fStream = File.Create(path + fileName + ".eml",(int)msgStream.Length)){
						msgStream.WriteTo(fStream);
					}
					//---------------------------------------------------------------//

					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:

					byte[] topLines = null;					
					topLines = GetTopLines(msgStream,50);
					
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_StoreMessage")){
						sqlCmd.AddParameter("@MailBox"      ,SqlDbType.NVarChar ,mailbox);
						sqlCmd.AddParameter("@Folder"       ,SqlDbType.NVarChar ,folder);
						sqlCmd.AddParameter("@Data"         ,SqlDbType.Image    ,msgStream.ToArray());
						sqlCmd.AddParameter("@Size"         ,SqlDbType.BigInt   ,msgStream.Length);
						sqlCmd.AddParameter("@TopLines"     ,SqlDbType.Image    ,topLines);
						sqlCmd.AddParameter("@Date"         ,SqlDbType.DateTime ,date);
						sqlCmd.AddParameter("@MessageFlags" ,SqlDbType.Int      ,(int)flags);

						DataSet ds = sqlCmd.Execute();
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function StoreMessageFlags

		/// <summary>
		/// Stores IMAP message flags (\seen,\draft, ...).
		/// </summary>
		/// <param name="mailbox"></param>
		/// <param name="folder"></param>
		/// <param name="message"></param>
		public void StoreMessageFlags(string mailbox,string folder,LumiSoft.Net.IMAP.Server.IMAP_Message message)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					string[] fileParts  = message.MessageID.Split('_'); 
					string   msgFile    = m_MailStorePath + "Mailboxes\\" + mailbox + "\\" + folder + "\\" + message.MessageID + ".eml";
					string   msgNewFile = m_MailStorePath + "Mailboxes\\" + mailbox + "\\" + folder + "\\" + fileParts[0] + "_" + fileParts[1] + "_" + Convert.ToString((int)message.Flags) + ".eml";

					// Check if file exists
					if(File.Exists(msgFile)){
						File.Move(msgFile,msgNewFile);

						// Need to store new messageID(flags changed)
						message.MessageID = fileParts[0] + "_" + fileParts[1] + "_" + Convert.ToString((int)message.Flags);
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_StoreMessageFlags")){
						sqlCmd.AddParameter("@MessageID"    ,SqlDbType.NVarChar,message.MessageID);
						sqlCmd.AddParameter("@Mailbox"      ,SqlDbType.NVarChar,mailbox);
						sqlCmd.AddParameter("@Folder"       ,SqlDbType.NVarChar,folder);
						sqlCmd.AddParameter("@MessageFalgs" ,SqlDbType.Int     ,(int)message.Flags);

						DataSet ds = sqlCmd.Execute();
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function DeleteMessage

		/// <summary>
		/// Deletes message from mailbox.
		/// </summary>
		/// <param name="mailbox">MailBox name.</param>
		/// <param name="folder"></param>
		/// <param name="msgID">MessageID.</param>
		public void DeleteMessage(string mailbox,string folder,string msgID)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					string msgFile = m_MailStorePath + "Mailboxes\\" + mailbox + "\\" + folder + "\\" + msgID + ".eml";

					// Check if file exists
					if(File.Exists(msgFile)){
						File.Delete(msgFile);
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteMessage")){
						sqlCmd.AddParameter("@MessageID" ,SqlDbType.NVarChar,msgID);
						sqlCmd.AddParameter("@Mailbox"   ,SqlDbType.NVarChar,mailbox);
						sqlCmd.AddParameter("@Folder"    ,SqlDbType.NVarChar,folder);

						DataSet ds = sqlCmd.Execute();
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function GetMessage

		/// <summary>
		/// Gets message from mailbox.
		/// </summary>
		/// <param name="mailbox">Mailbox name.</param>
		/// <param name="folder"></param>
		/// <param name="msgID">MessageID</param>
		public byte[] GetMessage(string mailbox,string folder,string msgID)
		{
			byte[] retVal = null;
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					string msgFile = m_MailStorePath + "Mailboxes\\" + mailbox + "\\" + folder + "\\" + msgID + ".eml";

					// Check if file exists
					if(File.Exists(msgFile)){
						//---- Read message from file -----------//
						using(FileStream fStrm = File.OpenRead(msgFile)){
							retVal = new byte[fStrm.Length];
							fStrm.Read(retVal,0,(int)fStrm.Length);
						}
						//---------------------------------------//
					}
				//	else{
				//		throw new Exception("Message '" + msgID + "' doesn't exist");
				//	}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMessage")){
						sqlCmd.AddParameter("@MessageID" ,SqlDbType.NVarChar,msgID);
						sqlCmd.AddParameter("@Mailbox"   ,SqlDbType.NVarChar,mailbox);
						sqlCmd.AddParameter("@Folder"    ,SqlDbType.NVarChar,folder);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "lsMailStore";

						return (byte[])ds.Tables["lsMailStore"].Rows[0]["Data"];
					}

				#endregion
			}

			return retVal;
		}

		#endregion

		#region function GetMessageTopLines

		/// <summary>
		/// Gets message header + number of specified lines.
		/// </summary>
		/// <param name="mailbox">Mailbox.</param>
		/// <param name="folder">IMAP folder where message is.</param>
		/// <param name="msgID">MessageID.</param>
		/// <param name="nrLines">Number of lines to retrieve. NOTE: line counting starts at theend of header.</param>
		/// <returns>Returns message header + number of specified lines.</returns>
		public byte[] GetMessageTopLines(string mailbox,string folder,string msgID,int nrLines)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					string msgFile = m_MailStorePath + "Mailboxes\\" + mailbox + "\\" + folder + "\\" + msgID + ".eml";

					using(FileStream strm = File.OpenRead(msgFile)){
						return GetTopLines(strm,nrLines);
					}

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetMessageTopLines")){
						sqlCmd.AddParameter("@MessageID" ,SqlDbType.NVarChar,msgID);
						sqlCmd.AddParameter("@Mailbox"   ,SqlDbType.NVarChar,mailbox);
						sqlCmd.AddParameter("@Folder"    ,SqlDbType.NVarChar,folder);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "lsMailStore";

						return GetTopLines(new MemoryStream((byte[])ds.Tables["lsMailStore"].Rows[0]["TopLines"]),nrLines);
					}

				#endregion
			}

			return null;
		}

		#endregion

		#region function CopyMessage

		/// <summary>
		/// Creates copy of message to destination folder.
		/// </summary>
		/// <param name="mailbox">MailBox name.</param>
		/// <param name="folder"></param>
		/// <param name="destFolder"></param>
		/// <param name="message"></param>
		public void CopyMessage(string mailbox,string folder,string destFolder,LumiSoft.Net.IMAP.Server.IMAP_Message message)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					string msgFile = m_MailStorePath + "Mailboxes\\" + mailbox + "\\" + folder + "\\" + message.MessageID + ".eml";

					// Check if file exists
					if(!File.Exists(msgFile)){
						throw new Exception("Message doesn't exist");
					}

					// Check if dest folder exists
					if(!Directory.Exists(m_MailStorePath + "Mailboxes\\" + mailbox + "\\" + destFolder)){
						throw new Exception("Destination folder doesn't exist");
					}					
					
					// We need to change UID of message to maximum of dest folder UID.
					int newUID = GetNextUid(mailbox,destFolder);
					string[] fileParts = message.MessageID.Split('_');
					string msgFileDest = m_MailStorePath + "Mailboxes\\" + mailbox + "\\" + destFolder + "\\" + fileParts[0] + "_" + newUID + "_" + Convert.ToString((int)message.Flags) + ".eml";
					
					File.Copy(msgFile,msgFileDest);
					
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_CopyMessage")){
						sqlCmd.AddParameter("@MessageID"  ,SqlDbType.NVarChar,message.MessageID);
						sqlCmd.AddParameter("@Mailbox"    ,SqlDbType.NVarChar,mailbox);
						sqlCmd.AddParameter("@Folder"     ,SqlDbType.NVarChar,folder);
						sqlCmd.AddParameter("@DestFolder" ,SqlDbType.NVarChar,destFolder);

						DataSet ds = sqlCmd.Execute();
					}
					break;

				#endregion
			}
		}

		#endregion


		#region function GetFolders

		/// <summary>
		/// Gets all available IMAP folders.
		/// </summary>
		/// <param name="userName"></param>
		public string[] GetFolders(string userName)
		{
			string[] retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					ArrayList dirs = new ArrayList();
					AppendDirs(m_MailStorePath + "\\Mailboxes\\" + userName + "\\",dirs,m_MailStorePath + "\\Mailboxes\\" + userName + "\\");
					
					retVal = new string[dirs.Count];
					dirs.CopyTo(retVal);

					return retVal;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetFolders")){
						sqlCmd.AddParameter("@UserName",SqlDbType.NVarChar,userName);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Folders";

						retVal = new string[ds.Tables["Folders"].Rows.Count];
						int i = 0;
						foreach(DataRow dr in ds.Tables["Folders"].Rows){
							retVal[i] = dr["FolderName"].ToString();
							i++;
						}

						return retVal;
					}

				#endregion
			}

			return retVal;
		}

		#endregion

		#region function GetSubscribedFolders

		/// <summary>
		/// Gets subscribed IMAP folders.
		/// </summary>
		/// <param name="userName"></param>
		public string[] GetSubscribedFolders(string userName)
		{
			string[] retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					DataSet ds = new DataSet();
					ds.Tables.Add("Subscriptions");
					ds.Tables["Subscriptions"].Columns.Add("Name");

					// Check if root directory exists, if not Create
					if(!Directory.Exists(m_MailStorePath + "\\Mailboxes\\" + userName + "\\Inbox\\")){
						Directory.CreateDirectory(m_MailStorePath + "\\Mailboxes\\" + userName + "\\Inbox\\");
					}

					if(!File.Exists(m_MailStorePath + "\\Mailboxes\\" + userName + "\\imap.xml")){
						DataSet dsx = ds.Copy();
						dsx.Tables["Subscriptions"].Rows.Add(new object[]{"Inbox"});
						dsx.WriteXml(m_MailStorePath + "\\Mailboxes\\" + userName + "\\imap.xml");
					}

					ds.ReadXml(m_MailStorePath + "\\Mailboxes\\" + userName + "\\imap.xml");

					ArrayList dirs = new ArrayList();
					foreach(DataRow dr in ds.Tables["Subscriptions"].Rows){
						dirs.Add(dr["Name"].ToString());
					}

					retVal = new string[dirs.Count];
					dirs.CopyTo(retVal);

					return retVal;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetSubscribedFolders")){
						sqlCmd.AddParameter("@UserName",SqlDbType.NVarChar,userName);

						ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Folders";

						retVal = new string[ds.Tables["Folders"].Rows.Count];
						int i = 0;
						foreach(DataRow dr in ds.Tables["Folders"].Rows){
							retVal[i] = dr["FolderName"].ToString();
							i++;
						}

						return retVal;
					}

				#endregion
			}

			return null;
		}

		#endregion

		#region function SubscribeFolder

		/// <summary>
		/// Subscribes new IMAP folder.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="folder"></param>
		public void SubscribeFolder(string userName,string folder)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					DataSet ds = new DataSet();
					ds.ReadXml(m_MailStorePath + "\\Mailboxes\\" + userName + "\\imap.xml");

					// ToDo: check if already exists, raise error or just skip ??

					DataRow dr = ds.Tables["Subscriptions"].NewRow();
					dr["Name"] = folder;
					ds.Tables["Subscriptions"].Rows.Add(dr);

					ds.WriteXml(m_MailStorePath + "\\Mailboxes\\" + userName + "\\imap.xml",XmlWriteMode.WriteSchema);
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_SubscribeFolder")){
						sqlCmd.AddParameter("@UserName",SqlDbType.NVarChar,userName);
						sqlCmd.AddParameter("@Folder"  ,SqlDbType.NVarChar,folder);

						ds = sqlCmd.Execute();
						break;
					//	ds.Tables[0].TableName = "Folders";

					//	return ds.Tables["Folders"].DefaultView;
					}

				#endregion
			}
		}

		#endregion

		#region function UnSubscribeFolder

		/// <summary>
		/// UnSubscribes IMAP folder.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="folder"></param>
		public void UnSubscribeFolder(string userName,string folder)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					DataSet ds = new DataSet();			
					ds.ReadXml(m_MailStorePath + "\\Mailboxes\\" + userName + "\\imap.xml");

					DataView dv = new DataView(ds.Tables["Subscriptions"]);
					dv.RowFilter = "Name='" + folder + "'";			
					if(dv.Count > 0){				
						dv[0].Row.Delete();
					}			

					ds.WriteXml(m_MailStorePath + "\\Mailboxes\\" + userName + "\\imap.xml",XmlWriteMode.WriteSchema);
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UnSubscribeFolder")){
						sqlCmd.AddParameter("@UserName",SqlDbType.NVarChar,userName);
						sqlCmd.AddParameter("@Folder"  ,SqlDbType.NVarChar,folder);

						ds = sqlCmd.Execute();
						break;
					//	ds.Tables[0].TableName = "Folders";

					//	return ds.Tables["Folders"].DefaultView;
					}

				#endregion
			}
		}

		#endregion

		#region function CreateFolder

		/// <summary>
		/// Creates new IMAP folder.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="folder"></param>
		public void CreateFolder(string userName,string folder)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					string dir = m_MailStorePath + "\\Mailboxes\\" + userName + "\\" + folder.Replace("/","\\");
					if(!Directory.Exists(dir)){
						Directory.CreateDirectory(dir);
					}
					else{
						throw new Exception("Folder(" + folder + ") already exists");
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_CreateFolder")){
						sqlCmd.AddParameter("@UserName",SqlDbType.NVarChar,userName);
						sqlCmd.AddParameter("@Folder"  ,SqlDbType.NVarChar,folder);

						DataSet ds = sqlCmd.Execute();

						if(ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0){
							throw new Exception(ds.Tables[0].Rows[0]["ErrorText"].ToString());
						}
						break;

					//	return ds.Tables["Folders"].DefaultView;
					}

				#endregion
			}
		}

		#endregion

		#region function DeleteFolder

		/// <summary>
		/// Deletes IMAP folder.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="folder"></param>
		public void DeleteFolder(string userName,string folder)
		{
			// Don't allow to delete inbox
			if(folder.ToLower() == "inbox"){
				throw new Exception("Can't delete inbox");
			}

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:					
					string dir = m_MailStorePath + "\\Mailboxes\\" + userName + "\\" + folder.Replace("/","\\");
					if(Directory.Exists(dir)){
						Directory.Delete(dir,true);
					}
					else{
						throw new Exception("Folder(" + folder + ") doesn't exist");
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteFolder")){
						sqlCmd.AddParameter("@UserName",SqlDbType.NVarChar,userName);
						sqlCmd.AddParameter("@Folder"  ,SqlDbType.NVarChar,folder);

						DataSet ds = sqlCmd.Execute();

						if(ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0){
							throw new Exception(ds.Tables[0].Rows[0]["ErrorText"].ToString());
						}
						break;

					//	return ds.Tables["Folders"].DefaultView;
					}

				#endregion
			}
		}

		#endregion

		#region function RenameFolder

		/// <summary>
		/// Renames IMAP folder.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="folder"></param>
		/// <param name="newFolder"></param>
		public void RenameFolder(string userName,string folder,string newFolder)
		{
			// Don't allow to delete inbox
			if(folder.ToLower() == "inbox"){
				throw new Exception("Can't rename inbox");
			}

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:					
					string dirSource = m_MailStorePath + "\\Mailboxes\\" + userName + "\\" + folder.Replace("/","\\");
					string dirDest   = m_MailStorePath + "\\Mailboxes\\" + userName + "\\" + newFolder.Replace("/","\\");
					if(Directory.Exists(dirSource)){
						if(Directory.Exists(dirDest)){
							throw new Exception("Destination Folder(" + newFolder + ") already exists");
						}
						else{
							Directory.Move(dirSource,dirDest);
						}
					}
					else{
						throw new Exception("Source Folder(" + folder + ") doesn't exist");
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_RenameFolder")){
						sqlCmd.AddParameter("@UserName"  ,SqlDbType.NVarChar,userName);
						sqlCmd.AddParameter("@Folder"    ,SqlDbType.NVarChar,folder);
						sqlCmd.AddParameter("@NewFolder" ,SqlDbType.NVarChar,newFolder);

						DataSet ds = sqlCmd.Execute();

						if(ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0){
							throw new Exception(ds.Tables[0].Rows[0]["ErrorText"].ToString());
						}
						break;

					//	return ds.Tables["Folders"].DefaultView;
					}

				#endregion
			}
		}

		#endregion

		#endregion

	
		#region Security related

		#region function GetSecurityList

		/// <summary>
		/// Gets security entries list.
		/// </summary>
		public DataView GetSecurityList()
		{
			DataView retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					retVal = dsSecurity.Tables["Security_List"].DefaultView;
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetSecurityList")){
				//		sqlCmd.AddParameter("@DomainName",SqlDbType.NVarChar,source);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Security_List";

						return ds.Tables["Security_List"].DefaultView;
					}

				#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.GetSecurityList().Tables["Security_List"].DefaultView;
					}

				#endregion
			}

			return retVal;
		}

		#endregion


		#region function AddSecurityEntry

		/// <summary>
		/// Adds secuity entry.
		/// </summary>
		/// <param name="Description"></param>
		/// <param name="protocol"></param>
		/// <param name="type"></param>
		/// <param name="action"></param>
		/// <param name="content"></param>
		/// <param name="startIP"></param>
		/// <param name="endIP"></param>
		/// <returns></returns>
		public DataRow AddSecurityEntry(string Description,string protocol,string type,string action,string content,long startIP,long endIP)
		{
			DataRow retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

					case DB_Type.XML:

						DataSet dsSecurityCopy = dsSecurity.Copy();

						DataRow dr = dsSecurityCopy.Tables["Security_List"].NewRow();
						dr["SecurityID"]  = Guid.NewGuid().ToString();
						dr["Description"] = Description;
						dr["Protocol"]    = protocol;
						dr["Type"]        = type;
						dr["Content"]     = content;
						dr["Action"]      = action;
						dr["StartIP"]     = startIP;
						dr["EndIP"]       = endIP;

						dsSecurityCopy.Tables["Security_List"].Rows.Add(dr);
						dsSecurityCopy.WriteXml(m_DataPath + "Security.xml",XmlWriteMode.IgnoreSchema);

						retVal = dr;

						break;

					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddSecurityEntry")){
							sqlCmd.AddParameter("@Description" ,SqlDbType.NVarChar,Description);							
							sqlCmd.AddParameter("@Protocol"    ,SqlDbType.NVarChar,protocol);
							sqlCmd.AddParameter("@Type"        ,SqlDbType.NVarChar,type);
							sqlCmd.AddParameter("@Action"      ,SqlDbType.NVarChar,action);
							sqlCmd.AddParameter("@Content"     ,SqlDbType.NVarChar,content);
							sqlCmd.AddParameter("@StartIP"     ,SqlDbType.NVarChar,startIP);
							sqlCmd.AddParameter("@EndIP"       ,SqlDbType.NVarChar,endIP);
							
							DataSet ds = sqlCmd.Execute();
							ds.Tables[0].TableName = "Security_List";

							if(ds.Tables["Security_List"].Rows.Count > 0){
								return ds.Tables["Security_List"].Rows[0];
							}							
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.AddSecurityEntry(Description,protocol,type,action,content,startIP,endIP).Tables["Security_List"].Rows[0];
					}

				#endregion
			}

			return retVal;
		}

		#endregion

		#region function DeleteSecurityEntry

		/// <summary>
		/// Deletes security entry.
		/// </summary>
		/// <param name="securityID"></param>
		public void DeleteSecurityEntry(string securityID)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

					case DB_Type.XML:
						using(DataView dv = new DataView(dsSecurity.Tables["Security_List"])){
							dv.RowFilter = "SecurityID='" + securityID + "'";

							if(dv.Count > 0){
								dsSecurity.Tables["Security_List"].Rows.Remove(dv[0].Row);							
							}

							dsSecurity.WriteXml(m_DataPath + "Security.xml",XmlWriteMode.IgnoreSchema);	
						}
						break;

					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_DeleteSecurityEntry")){
							sqlCmd.AddParameter("@SecurityID" ,SqlDbType.UniqueIdentifier,securityID);
							
							DataSet ds = sqlCmd.Execute();
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						eng.DeleteSecurityEntry(securityID);
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function UpdateSecurityEntry

		/// <summary>
		/// Updates security entry.
		/// </summary>
		/// <param name="securityID"></param>
		/// <param name="Description"></param>
		/// <param name="protocol"></param>
		/// <param name="type"></param>
		/// <param name="action"></param>
		/// <param name="content"></param>
		/// <param name="startIP"></param>
		/// <param name="endIP"></param>
		/// <returns></returns>
		public void UpdateSecurityEntry(string securityID,string Description,string protocol,string type,string action,string content,long startIP,long endIP)
		{			
			switch(m_DB_Type)
			{
				#region DB_Type.XML

					case DB_Type.XML:

						DataSet dsSecurityCopy = dsSecurity.Copy();

						using(DataView dv = new DataView(dsSecurityCopy.Tables["Security_List"])){
							dv.RowFilter = "SecurityID='" + securityID + "'";

							if(dv.Count > 0){
								dv[0]["Description"] = Description;
								dv[0]["Protocol"]    = protocol;
								dv[0]["Type"]        = type;
								dv[0]["Action"]      = action;
								dv[0]["Content"]     = content;
								dv[0]["StartIP"]     = startIP;
								dv[0]["EndIP"]       = endIP;
							}

							dsSecurityCopy.WriteXml(m_DataPath + "Security.xml",XmlWriteMode.IgnoreSchema);
						}

						break;

					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_UpdateSecurityEntry")){
							sqlCmd.AddParameter("@SecurityID"  ,SqlDbType.UniqueIdentifier,securityID);
							sqlCmd.AddParameter("@Description" ,SqlDbType.NVarChar        ,Description);							
							sqlCmd.AddParameter("@Protocol"    ,SqlDbType.NVarChar        ,protocol);
							sqlCmd.AddParameter("@Type"        ,SqlDbType.NVarChar        ,type);
							sqlCmd.AddParameter("@Action"      ,SqlDbType.NVarChar        ,action);
							sqlCmd.AddParameter("@Content"     ,SqlDbType.NVarChar        ,content);
							sqlCmd.AddParameter("@StartIP"     ,SqlDbType.BigInt          ,startIP);
							sqlCmd.AddParameter("@EndIP"       ,SqlDbType.BigInt          ,endIP);
							
							DataSet ds = sqlCmd.Execute();
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						eng.UpdateSecurityEntry(securityID,Description,protocol,type,action,content,startIP,endIP);
					}
					break;

				#endregion
			}
		}

		#endregion


		#region function IsRelayAllowedIP

		/// <summary>
		/// Checks if relay is allowed to specified IP.
		/// </summary>
		/// <param name="ip"></param>
		/// <returns>Returns true if relay is allowed.</returns>
		public bool IsRelayAllowedIP(string ip)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					//---------------------------------------------------------//
					// First Check if ip is denied, if not check if is allowed
					using(DataView dv = new DataView(dsSecurity.Tables["Security_List"])){
						dv.RowFilter = "Protocol='SMTP' AND Action='Deny Relay' AND StartIP <= " + ip + " AND EndIP >= " + ip;
		
						if(dv.Count > 0){
							return false;
						}

						// Check if ip is allowed
						dv.RowFilter = "Protocol='SMTP' AND Action='Allow Relay' AND StartIP <= " + ip + " AND EndIP >= " + ip;

						if(dv.Count > 0){
							return true;
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_IsRelayAllowed")){
						sqlCmd.AddParameter("@IP",SqlDbType.BigInt,ip);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Detail";

						if(ds.Tables["Detail"].Rows.Count > 0){
							return Convert.ToBoolean(ds.Tables["Detail"].Rows[0]["Allowed"]);
						}
					}
					break;

				#endregion
			}

			return false;
		}

		#endregion

		#region function IsRelayAllowedUser

		/// <summary>
		/// Checks if relay is allowed to specified user.
		/// </summary>
		/// <param name="user"></param>
		/// <returns>Returns true if relay is allowed.</returns>
		public bool IsRelayAllowedUser(string user)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:					
					using(DataView dv = new DataView(dsUsers.Tables["Users"])){
						dv.RowFilter = "AllowRelay=true AND USERNAME='" + user + "'";

						if(dv.Count > 0 && Convert.ToBoolean(dv[0].Row["AllowRelay"]) == true){							
							return true;
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_GetUserProperties")){
						sqlCmd.AddParameter("@UserName",SqlDbType.NVarChar,user);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Detail";

						if(ds.Tables["Detail"].Rows.Count > 0 && Convert.ToBoolean(ds.Tables["Detail"].Rows[0]["AllowRelay"]) == true){
							return true;
						}
					}
					break;

				#endregion
			}

			return false;
		}

		#endregion

		#region function IsSmtpAccessAllowed

		/// <summary>
		/// Checks if smtp access is allowed for specified IP.
		/// </summary>
		/// <param name="ip"></param>
		/// <returns>Returns true if allowed.</returns>
		public bool IsSmtpAccessAllowed(string ip)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					using(DataView dv = new DataView(dsSecurity.Tables["Security_List"])){
						//---------------------------------------------------------//
						// First Check if ip is denied, if not check if is allowed				
						dv.RowFilter = "Protocol='SMTP' AND Action='Deny' AND StartIP <= " + ip + " AND EndIP >= " + ip;
		
						if(dv.Count > 0){
							return false;
						}

						// Check if ip is allowed
						dv.RowFilter = "Protocol='SMTP' AND Action='Allow' AND StartIP <= " + ip + " AND EndIP >= " + ip;

						if(dv.Count > 0){
							return true;
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_IsSmtpAccessAllowed")){
						sqlCmd.AddParameter("@IP",SqlDbType.BigInt,ip);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Detail";

						if(ds.Tables["Detail"].Rows.Count > 0){
							return Convert.ToBoolean(ds.Tables["Detail"].Rows[0]["Allowed"]);
						}
					}
					break;

				#endregion
			}

			return false;
		}

		#endregion

		#region function IsPop3AccessAllowed

		/// <summary>
		/// Checks if pop3 access is allowed for specified IP.
		/// </summary>
		/// <param name="ip"></param>
		/// <returns>Returns true if allowed.</returns>
		public bool IsPop3AccessAllowed(string ip)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					using(DataView dv = new DataView(dsSecurity.Tables["Security_List"])){
						//---------------------------------------------------------//
						// First Check if ip is denied, if not check if is allowed				
						dv.RowFilter = "Protocol='POP3' AND Action='Deny' AND StartIP <= " + ip + " AND EndIP >= " + ip;
		
						if(dv.Count > 0){
							return false;
						}

						// Check if ip is allowed
						dv.RowFilter = "Protocol='POP3' AND Action='Allow' AND StartIP <= " + ip + " AND EndIP >= " + ip;

						if(dv.Count > 0){
							return true;
						}
					}
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_IsPop3AccessAllowed")){
						sqlCmd.AddParameter("@IP",SqlDbType.BigInt,ip);

						DataSet ds = sqlCmd.Execute();
						ds.Tables[0].TableName = "Detail";

						if(ds.Tables["Detail"].Rows.Count > 0){
							return Convert.ToBoolean(ds.Tables["Detail"].Rows[0]["Allowed"]);
						}
					}
					break;

				#endregion
			}

			return false;
		}

		#endregion

		#endregion

		#region Filters related

		#region function GetFilterList

		/// <summary>
		/// Gets filter list.
		/// </summary>
		/// <returns></returns>
		public DataView GetFilterList()
		{
			DataView retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					retVal = new DataView(dsFilters.Tables["SmtpFilters"]);
					retVal.Sort = "Cost";
					break;

				#endregion

				#region DB_Type.MSSQL

				case DB_Type.MSSQL:
					retVal = new DataView(dsFilters.Tables["SmtpFilters"]);
					retVal.Sort = "Cost";
					break;

				#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.GetFilterList().Tables["SmtpFilters"].DefaultView;
					}

				#endregion
			}

			return retVal;
		}

		#endregion


		#region function AddFilter

		/// <summary>
		/// Adds new filter.
		/// </summary>
		/// <param name="description">Filter description</param>
		/// <param name="assembly">Assembly with full location. Eg. C:\MailServer\Filters\filter.dll .</param>
		/// <param name="className">Filter full class name, wih namespace. Eg. LumiSoft.MailServer.Fileters.Filter1 .</param>
		/// <param name="cost">Filters are sorted by cost and proccessed with cost value. Smallest cost is proccessed first.</param>
		/// <param name="enabled">Specifies if filter is enabled.</param>
		/// <returns></returns>
		public DataRow AddFilter(string description,string assembly,string className,int cost,bool enabled)
		{
			DataRow retVal = null;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

					case DB_Type.XML:

						DataSet dsFiltersCopy = dsFilters.Copy();

						DataRow dr = dsFiltersCopy.Tables["SmtpFilters"].NewRow();
						dr["FilterID"]    = Guid.NewGuid().ToString();
						dr["Description"] = description;
						dr["Assembly"]    = assembly;
						dr["ClassName"]   = className;
						dr["Cost"]        = cost;
						dr["Enabled"]     = enabled;

						dsFiltersCopy.Tables["SmtpFilters"].Rows.Add(dr);
						dsFiltersCopy.WriteXml(m_DataPath + "Filters.xml",XmlWriteMode.IgnoreSchema);

						return dr;

					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						DataSet dsFiltersCopy2 = dsFilters.Copy();

						DataRow dr2 = dsFiltersCopy2.Tables["SmtpFilters"].NewRow();
						dr2["FilterID"]    = Guid.NewGuid().ToString();
						dr2["Description"] = description;	
						dr2["Assembly"]    = assembly;
						dr2["ClassName"]   = className;
						dr2["Cost"]        = cost;
						dr2["Enabled"]     = enabled;

						dsFiltersCopy2.Tables["SmtpFilters"].Rows.Add(dr2);
						dsFiltersCopy2.WriteXml(m_DataPath + "Filters.xml",XmlWriteMode.IgnoreSchema);

						return dr2;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.AddFilter(description,assembly,className,cost,enabled).Tables["SmtpFilters"].Rows[0];
					}

				#endregion
			}

			return retVal;
		}

		#endregion

		#region function DeleteFilter

		/// <summary>
		/// Deletes specified filter.
		/// </summary>
		/// <param name="filterID">FilterID of the filter which to delete.</param>
		public void DeleteFilter(string filterID)
		{
			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
                    DataSet dsFiltersCopy = dsFilters.Copy();					
					using(DataView dv = new DataView(dsFiltersCopy.Tables["SmtpFilters"])){
						dv.RowFilter = "FilterID='" + filterID + "'";

						if(dv.Count > 0){
							dsFiltersCopy.Tables["SmtpFilters"].Rows.Remove(dv[0].Row);							
						}

						dsFiltersCopy.WriteXml(m_DataPath + "Filters.xml",XmlWriteMode.IgnoreSchema);
					}
					break;

				#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						DataSet dsFiltersCopy2 = dsFilters.Copy();					
						using(DataView dv = new DataView(dsFiltersCopy2.Tables["SmtpFilters"])){
							dv.RowFilter = "FilterID='" + filterID + "'";

							if(dv.Count > 0){
								dsFiltersCopy2.Tables["SmtpFilters"].Rows.Remove(dv[0].Row);							
							}

							dsFiltersCopy2.WriteXml(m_DataPath + "Filters.xml",XmlWriteMode.IgnoreSchema);
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						eng.DeleteFilter(filterID);
					}
					break;

				#endregion
			}
		}

		#endregion

		#region function UpdateFilter

		/// <summary>
		/// Updates specified filter.
		/// </summary>		/// 
		/// <param name="filterID">FilterID which to update.</param>
		/// <param name="description">Filter description</param>
		/// <param name="assembly">Assembly with full location. Eg. C:\MailServer\Filters\filter.dll .</param>
		/// <param name="className">Filter full class name, wih namespace. Eg. LumiSoft.MailServer.Fileters.Filter1 .</param>
		/// <param name="cost">Filters are sorted by cost and proccessed with cost value. Smallest cost is proccessed first.</param>
		/// <param name="enabled">Specifies if filter is enabled.</param>
		/// <returns></returns>
		public void UpdateFilter(string filterID,string description,string assembly,string className,int cost,bool enabled)
		{		
			switch(m_DB_Type)
			{
				#region DB_Type.XML

					case DB_Type.XML:
						DataSet dsFiltersCopy = dsFilters.Copy();
						using(DataView dv = new DataView(dsFiltersCopy.Tables["SmtpFilters"])){
							dv.RowFilter = "FilterID='" + filterID + "'";

							if(dv.Count > 0){
								DataRow dr = dv[0].Row;
								dr["Description"] = description;
								dr["Assembly"]    = assembly;
								dr["ClassName"]   = className;
								dr["Cost"]        = cost;
								dr["Enabled"]     = enabled;
							}

							dsFiltersCopy.WriteXml(m_DataPath + "Filters.xml",XmlWriteMode.IgnoreSchema);
						}
						break;
					
					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						DataSet dsFiltersCopy2 = dsFilters.Copy();
						using(DataView dv = new DataView(dsFiltersCopy2.Tables["SmtpFilters"])){
							dv.RowFilter = "FilterID='" + filterID + "'";

							if(dv.Count > 0){
								DataRow dr = dv[0].Row;
								dr["Description"] = description;
								dr["Assembly"]    = assembly;
								dr["ClassName"]   = className;
								dr["Cost"]        = cost;
								dr["Enabled"]     = enabled;
							}

							dsFiltersCopy2.WriteXml(m_DataPath + "Filters.xml",XmlWriteMode.IgnoreSchema);
						}
						break;

					#endregion

				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						eng.UpdateFilter(filterID,description,assembly,className,cost,enabled);
					}
					break;

				#endregion
			}
		}

		#endregion

		#endregion


		#region BackUp related

		#region function CreateBackUp

		/// <summary>
		/// Backups all server.(settings,users,...).
		/// </summary>
		/// <param name="fileName">File name to which store backup.</param>
		public void CreateBackUp(string fileName)
		{
			byte[] data = CreateBackUp();
			using(FileStream fs = File.Create(fileName)){				
				fs.Write(data,0,data.Length);
			}
		}

		/// <summary>
		///  Backups all server.(settings,users,...).
		/// </summary>
		/// <returns></returns>
		public byte[] CreateBackUp()
		{
			DataSet dsAll = new DataSet();

			switch(m_DB_Type)
			{					
				#region DB_Type.XML

					case DB_Type.XML:						
						dsAll.Merge(dsDomains);				
						dsAll.Merge(dsUsers);
						dsAll.Merge(dsAliases);
						dsAll.Merge(dsRouting);
						dsAll.Merge(this.GetSettings());
						dsAll.Merge(dsSecurity);						
						break;

					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:
						dsAll.Merge(this.GetDomainList().Table);				
						dsAll.Merge(this.GetUserList("").Table);
						dsAll.Merge(this.GetAliasesList("").Table);
						dsAll.Merge(this.GetRouteList().Table);
						dsAll.Merge(this.GetSettings());
						dsAll.Merge(this.GetSecurityList().Table);
						break;

					#endregion

				#region DB_Type.WebServices

					case DB_Type.WebServices:
						using(RemoteAdmin eng = new RemoteAdmin()){
							_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

							return eng.CreateBackUp();
						}

				#endregion
			}

			using(MemoryStream mStrm = new MemoryStream()){
				dsAll.WriteXml(mStrm,XmlWriteMode.IgnoreSchema);
				return mStrm.ToArray();
			}
		}

		#endregion

		#region function RestoreBackUp

		/// <summary>
		/// Restores server from backup.(settings,users,...).
		/// </summary>
		/// <param name="fileName">File name from which to restore settings.</param>
		public void RestoreBackUp(string fileName)
		{
			using(FileStream fs = File.OpenRead(fileName)){
				byte[] data = new byte[fs.Length];
				fs.Read(data,0,data.Length);

				RestoreBackUp(data);
			}
		}

		/// <summary>
		/// Restores server from backup.(settings,users,...).
		/// </summary>
		/// <param name="data"></param>
		public void RestoreBackUp(byte[] data)
		{
			DataSet dsAll = new DataSet();
			dsAll.ReadXml(new MemoryStream(data));

			DB_Type dbType = (DB_Type)Enum.Parse(typeof(DB_Type),dsAll.Tables["Settings"].Rows[0]["DataBaseType"].ToString());

			switch(dbType)
			{
				#region DB_Type.XML

					case DB_Type.XML:
													
						if(dsAll.Tables.Contains("Domains")){
							DataSet dsX = new DataSet();
							dsX.Merge(dsAll.Tables["Domains"]);
							dsX.WriteXml(m_DataPath + "Domains.xml",XmlWriteMode.IgnoreSchema);
						}

						if(dsAll.Tables.Contains("Users")){
							DataSet dsX = new DataSet();
							dsX.Merge(dsAll.Tables["Users"]);
							dsX.WriteXml(m_DataPath + "Users.xml",XmlWriteMode.IgnoreSchema);
						}

						if(dsAll.Tables.Contains("Aliases")){
							DataSet dsX = new DataSet();
							dsX.Merge(dsAll.Tables["Aliases"]);
							dsX.WriteXml(m_DataPath + "Aliases.xml",XmlWriteMode.IgnoreSchema);
						}

						if(dsAll.Tables.Contains("Routing")){
							DataSet dsX = new DataSet();
							dsX.Merge(dsAll.Tables["Routing"]);
							dsX.WriteXml(m_DataPath + "Routing.xml",XmlWriteMode.IgnoreSchema);
						}

						if(dsAll.Tables.Contains("Settings")){
							DataSet dsX = new DataSet();
							dsX.Merge(dsAll.Tables["Settings"]);
							dsX.WriteXml(m_DataPath + "Settings.xml",XmlWriteMode.IgnoreSchema);
						}
							
						if(dsAll.Tables.Contains("Security_List")){
							DataSet dsX = new DataSet();
							dsX.Merge(dsAll.Tables["Security_List"]);
							dsX.WriteXml(m_DataPath + "Security.xml",XmlWriteMode.IgnoreSchema);
						}
						break;

					#endregion

				#region DB_Type.MSSQL

					case DB_Type.MSSQL:

						#region Clear old settings

						using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_TruncateSettings")){
							DataSet ds = sqlCmd.Execute();
						}

						#endregion


						#region Restore domains

						if(dsAll.Tables.Contains("Domains"))
						{
							foreach(DataRow dr in dsAll.Tables["Domains"].Rows){
								using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddDomain")){
									if(dr.Table.Columns.Contains("DomainName")){
										sqlCmd.AddParameter("@DomainName" ,SqlDbType.NVarChar,dr["DomainName"].ToString());
									}
									if(dr.Table.Columns.Contains("Description")){
										sqlCmd.AddParameter("@Description",SqlDbType.NVarChar,dr["Description"].ToString());
									}
									if(dr.Table.Columns.Contains("DomainID")){
										sqlCmd.AddParameter("@DomainID"   ,SqlDbType.NVarChar,dr["DomainID"].ToString());
									}
							
									DataSet ds = sqlCmd.Execute();
								}
							}
						}

						#endregion

						#region Restore users

						if(dsAll.Tables.Contains("Users")){
							foreach(DataRow dr in dsAll.Tables["Users"].Rows){								
								using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddUser")){
									if(dr.Table.Columns.Contains("FULLNAME")){
										sqlCmd.AddParameter("@FullName"    ,SqlDbType.NVarChar,dr["FULLNAME"].ToString());
									}
									if(dr.Table.Columns.Contains("USERNAME")){
										sqlCmd.AddParameter("@UserName"    ,SqlDbType.NVarChar,dr["USERNAME"].ToString());
									}
									if(dr.Table.Columns.Contains("PASSWORD")){
										sqlCmd.AddParameter("@Password"    ,SqlDbType.NVarChar,dr["PASSWORD"].ToString());
									}
									if(dr.Table.Columns.Contains("Description")){
										sqlCmd.AddParameter("@Description" ,SqlDbType.NVarChar,dr["Description"].ToString());
									}
									if(dr.Table.Columns.Contains("Emails")){
										sqlCmd.AddParameter("@Emails"      ,SqlDbType.NVarChar,dr["Emails"].ToString());
									}
									if(dr.Table.Columns.Contains("Mailbox_Size")){
										sqlCmd.AddParameter("@MailboxSize" ,SqlDbType.NVarChar,dr["Mailbox_Size"].ToString());
									}
									if(dr.Table.Columns.Contains("DomainID")){
										sqlCmd.AddParameter("@DomainID"    ,SqlDbType.NVarChar,dr["DomainID"].ToString());
									}									
									if(dr.Table.Columns.Contains("UserID")){
										sqlCmd.AddParameter("@UserID"      ,SqlDbType.NVarChar,dr["UserID"].ToString());
									}
									if(dr.Table.Columns.Contains("RemotePop3Servers")){
										sqlCmd.AddParameter("@RemotePop3Servers" ,SqlDbType.Image,(byte[])dr["RemotePop3Servers"]);
									}
									
									DataSet ds = sqlCmd.Execute();
								}
							}
						}

						#endregion

						#region Restore aliases

						if(dsAll.Tables.Contains("Aliases")){
							foreach(DataRow dr in dsAll.Tables["Aliases"].Rows){
								using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddAlias")){
									if(dr.Table.Columns.Contains("AliasName")){
										sqlCmd.AddParameter("@AliasName"   ,SqlDbType.NVarChar,dr["AliasName"].ToString());
									}
									if(dr.Table.Columns.Contains("Description")){
										sqlCmd.AddParameter("@Description" ,SqlDbType.NVarChar,dr["Description"].ToString());
									}
									if(dr.Table.Columns.Contains("AliasMembers")){
										sqlCmd.AddParameter("@Members"     ,SqlDbType.NVarChar,dr["AliasMembers"].ToString());
									}
									if(dr.Table.Columns.Contains("DomainID")){
										sqlCmd.AddParameter("@DomainID"    ,SqlDbType.UniqueIdentifier,dr["DomainID"].ToString());
									}
									if(dr.Table.Columns.Contains("AliasID")){
										sqlCmd.AddParameter("@AliasID"    ,SqlDbType.UniqueIdentifier,dr["AliasID"].ToString());
									}
									
									DataSet ds = sqlCmd.Execute();
								}
							}
						}

						#endregion

						#region Restore routing

						if(dsAll.Tables.Contains("Routing")){
							foreach(DataRow dr in dsAll.Tables["Routing"].Rows){
								using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddRoute")){
									if(dr.Table.Columns.Contains("Pattern")){
										sqlCmd.AddParameter("@Pattern"   ,SqlDbType.NVarChar,dr["Pattern"].ToString());
									}
									if(dr.Table.Columns.Contains("Mailbox")){
										sqlCmd.AddParameter("@Mailbox" ,SqlDbType.NVarChar,dr["Mailbox"].ToString());
									}
									if(dr.Table.Columns.Contains("Description")){
										sqlCmd.AddParameter("@Description"     ,SqlDbType.NVarChar,dr["Description"].ToString());
									}
									if(dr.Table.Columns.Contains("DomainID")){
										sqlCmd.AddParameter("@DomainID"    ,SqlDbType.UniqueIdentifier,dr["DomainID"].ToString());
									}
									if(dr.Table.Columns.Contains("AliasID")){
										sqlCmd.AddParameter("@RouteID"    ,SqlDbType.UniqueIdentifier,dr["RouteID"].ToString());
									}
									
									DataSet ds = sqlCmd.Execute();
								}
							}
						}

						#endregion

						#region Restore settings

						if(dsAll.Tables.Contains("Settings")){
							DataSet dsX = new DataSet();
							dsX.Merge(dsAll.Tables["Settings"]);
							dsX.WriteXml(m_DataPath + "Settings.xml",XmlWriteMode.IgnoreSchema);
						}

						#endregion

						#region Restore security
							
						if(dsAll.Tables.Contains("Security_List"))
						{
							foreach(DataRow dr in dsAll.Tables["Security_List"].Rows){
								using(WSqlCommand sqlCmd = new WSqlCommand(m_ConStr,"lspr_AddSecurityEntry")){
									if(dr.Table.Columns.Contains("Description")){
										sqlCmd.AddParameter("@Description" ,SqlDbType.NVarChar,dr["Description"].ToString());
									}
									if(dr.Table.Columns.Contains("Protocol")){
										sqlCmd.AddParameter("@Protocol"    ,SqlDbType.NVarChar,dr["Protocol"].ToString());
									}
									if(dr.Table.Columns.Contains("Type")){
										sqlCmd.AddParameter("@Type"        ,SqlDbType.NVarChar,dr["Type"].ToString());
									}
									if(dr.Table.Columns.Contains("Action")){
										sqlCmd.AddParameter("@Action"      ,SqlDbType.NVarChar,dr["Action"].ToString());
									}
									if(dr.Table.Columns.Contains("Content")){
										sqlCmd.AddParameter("@Content"     ,SqlDbType.NVarChar,dr["Content"].ToString());
									}
									if(dr.Table.Columns.Contains("StartIP")){
										sqlCmd.AddParameter("@StartIP"     ,SqlDbType.NVarChar,dr["StartIP"].ToString());
									}
									if(dr.Table.Columns.Contains("EndIP")){
										sqlCmd.AddParameter("@EndIP"       ,SqlDbType.NVarChar,dr["EndIP"].ToString());
									}
									if(dr.Table.Columns.Contains("SecurityID")){
										sqlCmd.AddParameter("@SecurityID"  ,SqlDbType.NVarChar,dr["SecurityID"].ToString());
									}
								
									DataSet ds = sqlCmd.Execute();
								}
							}
						}

						#endregion

						break;

					#endregion

				#region DB_Type.WebServices

					case DB_Type.WebServices:
						using(RemoteAdmin eng = new RemoteAdmin()){
							_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

							eng.RestoreBackUp(data);
						}
						break;

				#endregion
			}
		}

		#endregion

		#endregion


		#region Settings

		#region function GetSettings

		/// <summary>
		/// Gets mailserver core settings (ports,database type, ...).
		/// </summary>
		/// <returns></returns>
		public DataSet GetSettings()
		{
			switch(m_DB_Type)
			{	
				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						return eng.GetSettings();
					}

				#endregion

				default:
					DataSet ds = new DataSet();
					CreateSettingsSchema(ds);
					ds.ReadXml(m_DataPath + "Settings.xml");

					foreach(DataRow dr in ds.Tables["Settings"].Rows){
						foreach(DataColumn dc in ds.Tables["Settings"].Columns){
							if(dr.IsNull(dc.ColumnName)){
								dr[dc.ColumnName] = dc.DefaultValue;
							}
						}
					}

					return ds;
			}			
		}

		#endregion

		#region function UpdateSettings

		/// <summary>
		/// Updates mailserver core settings (ports,database type, ...).
		/// </summary>
		/// <param name="dsSettings"></param>
		/// <returns></returns>
		public void UpdateSettings(DataSet dsSettings)
		{
			switch(m_DB_Type)
			{	
				#region DB_Type.WebServices

				case DB_Type.WebServices:
					using(RemoteAdmin eng = new RemoteAdmin()){
						_Core.InitWebService(m_WebServicesUrl,m_WebServicesPwd,m_WebServicesUser,eng);

						eng.UpdateSettings(dsSettings);
					}
					break;

				#endregion

				default:
					if(dsSettings != null && dsSettings.Tables.Contains("Settings")){
						dsSettings.WriteXml(m_DataPath + "Settings.xml",XmlWriteMode.IgnoreSchema);

						// If DB_Type changed to XMl, we need to reload Users, ...
						DB_Type dbT = (DB_Type)Enum.Parse(typeof(DB_Type),dsSettings.Tables["Settings"].Rows[0]["DataBaseType"].ToString());
						if(dbT != m_DB_Type && dbT == DB_Type.XML){
							LoadUsers();
							LoadAliases();
							LoadRouting();
							LoadDomains();
							LoadSecurity();
						}
					}
					break;
			}			
		}

		#endregion

		#endregion


		#region DB_Type.Xml helpers

		#region Load stuff

		/// <summary>
		/// Loads users from xml file.
		/// </summary>
		public void LoadUsers()
		{
			if(m_DB_Type == DB_Type.XML){
				lock(dsUsers){
					dsUsers.Clear();
					CreateUsersSchema(dsUsers);
					dsUsers.ReadXml(m_DataPath + "Users.xml");
				}
			}
		}

		/// <summary>
		///  Loads aliases from xml file.
		/// </summary>
		public void LoadAliases()
		{
			if(m_DB_Type == DB_Type.XML){
				lock(dsAliases){
					dsAliases.Clear();
					CreateAliasesSchema(dsAliases);
					dsAliases.ReadXml(m_DataPath + "Aliases.xml");

					foreach(DataRow dr in dsAliases.Tables["Aliases"].Rows){
						foreach(DataColumn dc in dsAliases.Tables["Aliases"].Columns){
							if(dr.IsNull(dc.ColumnName)){
								dr[dc.ColumnName] = dc.DefaultValue;
							}
						}
					}
				}
			}
		}

		/// <summary>
		///  Loads routing from xml file.
		/// </summary>
		public void LoadRouting()
		{
			if(m_DB_Type == DB_Type.XML){
				lock(dsRouting){
					dsRouting.Clear();
					CreateRoutingsSchema(dsRouting);
					dsRouting.ReadXml(m_DataPath + "Routing.xml");
				}
			}
		}

		/// <summary>
		///  Loads domains from xml file.
		/// </summary>
		public void LoadDomains()
		{
			if(m_DB_Type == DB_Type.XML){
				lock(dsDomains){
					dsDomains.Clear();
					CreateDomainsSchema(dsDomains);
					dsDomains.ReadXml(m_DataPath + "Domains.xml");
				}
			}
		}

		/// <summary>
		///  Loads security from xml file.
		/// </summary>
		public void LoadSecurity()
		{
			if(m_DB_Type == DB_Type.XML){
				lock(dsSecurity){
					dsSecurity.Clear();
					CreateSecuritySchema(dsSecurity);
					dsSecurity.ReadXml(m_DataPath + "Security.xml");
				}
			}
		}

		/// <summary>
		///  Loads filters from xml file.
		/// </summary>
		public void LoadFilters()
		{
		//	if(m_DB_Type == DB_Type.XML){
				lock(dsFilters){
					dsFilters.Clear();
					CreateFiltersSchema(dsFilters);
					dsFilters.ReadXml(m_DataPath + "Filters.xml");
				}
		//	}
		}

		#endregion
	
		#region Schema stuff

		#region function CreateDomainsSchema

		private void CreateDomainsSchema(DataSet ds)
		{
			// If table is missing, add it
			if(!ds.Tables.Contains("Domains")){
				ds.Tables.Add("Domains");
			}

			// If DomainName column is missing, add it
			if(!ds.Tables["Domains"].Columns.Contains("DomainID")){
				ds.Tables["Domains"].Columns.Add("DomainID",Type.GetType("System.String"));
			}

			// If DomainName column is missing, add it
			if(!ds.Tables["Domains"].Columns.Contains("DomainName")){
				ds.Tables["Domains"].Columns.Add("DomainName",Type.GetType("System.String"));
			}

			// If Description column is missing, add it
			if(!ds.Tables["Domains"].Columns.Contains("Description")){
				ds.Tables["Domains"].Columns.Add("Description",Type.GetType("System.String"));
			}
		}

		#endregion


		#region function CreateUsersSchema

		private void CreateUsersSchema(DataSet ds)
		{
			// If table is missing, add it
			if(!ds.Tables.Contains("Users")){
				ds.Tables.Add("Users");
			}

			// If UserID column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("UserID")){
				ds.Tables["Users"].Columns.Add("UserID",Type.GetType("System.String"));
			}

			// If DomainID column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("DomainID")){
				ds.Tables["Users"].Columns.Add("DomainID",Type.GetType("System.String"));
			}

			// If FullName column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("FullName")){
				ds.Tables["Users"].Columns.Add("FullName",Type.GetType("System.String"));
			}

			// If USERNAME column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("UserName")){
				ds.Tables["Users"].Columns.Add("UserName",Type.GetType("System.String"));
			}

			// If Description column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("Password")){
				ds.Tables["Users"].Columns.Add("Password",Type.GetType("System.String"));
			}

			// If Description column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("Description")){
				ds.Tables["Users"].Columns.Add("Description",Type.GetType("System.String"));
			}

			// If Emails column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("Emails")){
				ds.Tables["Users"].Columns.Add("Emails",Type.GetType("System.String"));
			}
			
			// If DomainName column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("DomainName")){
				ds.Tables["Users"].Columns.Add("DomainName",Type.GetType("System.String"));
			}

			// If DomainName column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("Mailbox_Size")){
				ds.Tables["Users"].Columns.Add("Mailbox_Size",Type.GetType("System.Int32"));
				ds.Tables["Users"].Columns["Mailbox_Size"].DefaultValue = 20;
				ds.Tables["Users"].Columns["Mailbox_Size"].AllowDBNull = false;
			}


			// If Enabled column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("Enabled")){
				ds.Tables["Users"].Columns.Add("Enabled",Type.GetType("System.Boolean"));
				ds.Tables["Users"].Columns["Enabled"].DefaultValue = true;
				ds.Tables["Users"].Columns["Enabled"].AllowDBNull = false;
			}

			// If AllowRealy column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("AllowRelay")){
				ds.Tables["Users"].Columns.Add("AllowRelay",Type.GetType("System.Boolean"));
				ds.Tables["Users"].Columns["AllowRelay"].DefaultValue = true;
				ds.Tables["Users"].Columns["AllowRelay"].AllowDBNull = false;
			}

			// If RemotePop3Servers column is missing, add it
			if(!ds.Tables["Users"].Columns.Contains("RemotePop3Servers")){
				ds.Tables["Users"].Columns.Add("RemotePop3Servers",Type.GetType("System.Byte[]"));
		//		ds.Tables["Users"].Columns["RemotePop3Servers"].DefaultValue = "";
		//		ds.Tables["Users"].Columns["RemotePop3Servers"].AllowDBNull = false;
			}

		}

		#endregion

		#region function CreateAliasesSchema

		private void CreateAliasesSchema(DataSet ds)
		{
			// If table is missing, add it
			if(!ds.Tables.Contains("Aliases")){
				ds.Tables.Add("Aliases");
			}

			// If AliasID column is missing, add it
			if(!ds.Tables["Aliases"].Columns.Contains("AliasID")){
				ds.Tables["Aliases"].Columns.Add("AliasID",Type.GetType("System.String"));
			}

			// If DomainID column is missing, add it
			if(!ds.Tables["Aliases"].Columns.Contains("DomainID")){
				ds.Tables["Aliases"].Columns.Add("DomainID",Type.GetType("System.String"));
			}

			// If AliasName column is missing, add it
			if(!ds.Tables["Aliases"].Columns.Contains("AliasName")){
				ds.Tables["Aliases"].Columns.Add("AliasName",Type.GetType("System.String"));
			}

			// If Description column is missing, add it
			if(!ds.Tables["Aliases"].Columns.Contains("Description")){
				ds.Tables["Aliases"].Columns.Add("Description",Type.GetType("System.String"));
			}

			// If AliasMembers column is missing, add it
			if(!ds.Tables["Aliases"].Columns.Contains("AliasMembers")){
				ds.Tables["Aliases"].Columns.Add("AliasMembers",Type.GetType("System.String"));
			}

			// If AliasMembers column is missing, add it
			if(!ds.Tables["Aliases"].Columns.Contains("IsPublic")){
				ds.Tables["Aliases"].Columns.Add("IsPublic",Type.GetType("System.String")).DefaultValue = false;
			}
		}

		#endregion

		#region function CreateRoutingsSchema

		private void CreateRoutingsSchema(DataSet ds)
		{
			// If table is missing, add it
			if(!ds.Tables.Contains("Routing")){
				ds.Tables.Add("Routing");
			}

			// If DomainID column is missing, add it
			if(!ds.Tables["Routing"].Columns.Contains("DomainID")){
				ds.Tables["Routing"].Columns.Add("DomainID",Type.GetType("System.String"));
			}

			// If DomainID column is missing, add it
			if(!ds.Tables["Routing"].Columns.Contains("DomainName")){
				ds.Tables["Routing"].Columns.Add("DomainName",Type.GetType("System.String"));
			}

			// If RouteID column is missing, add it
			if(!ds.Tables["Routing"].Columns.Contains("RouteID")){
				ds.Tables["Routing"].Columns.Add("RouteID",Type.GetType("System.String"));
			}

			// If Pattern column is missing, add it
			if(!ds.Tables["Routing"].Columns.Contains("Pattern")){
				ds.Tables["Routing"].Columns.Add("Pattern",Type.GetType("System.String"));
			}

			// If Mailbox column is missing, add it
			if(!ds.Tables["Routing"].Columns.Contains("Mailbox")){
				ds.Tables["Routing"].Columns.Add("Mailbox",Type.GetType("System.String"));
			}

			// If Description column is missing, add it
			if(!ds.Tables["Routing"].Columns.Contains("Description")){
				ds.Tables["Routing"].Columns.Add("Description",Type.GetType("System.String"));
			}

			// If Length column is missing, add it
			if(!ds.Tables["Routing"].Columns.Contains("Length")){
				ds.Tables["Routing"].Columns.Add("Length",Type.GetType("System.String"),"Len(Pattern)");
			}
		}

		#endregion


		#region function CreateSecuritySchema

		private void CreateSecuritySchema(DataSet ds)
		{
			// If table is missing, add it
			if(!ds.Tables.Contains("Security_List")){
				ds.Tables.Add("Security_List");
			}

			// If SecurityID column is missing, add it
			if(!ds.Tables["Security_List"].Columns.Contains("SecurityID")){
				ds.Tables["Security_List"].Columns.Add("SecurityID",Type.GetType("System.String"));
			}

			// If Description column is missing, add it
			if(!ds.Tables["Security_List"].Columns.Contains("Description")){
				ds.Tables["Security_List"].Columns.Add("Description",Type.GetType("System.String"));
			}

			// If Protocol column is missing, add it
			if(!ds.Tables["Security_List"].Columns.Contains("Protocol")){
				ds.Tables["Security_List"].Columns.Add("Protocol",Type.GetType("System.String"));
			}

			// If Type column is missing, add it
			if(!ds.Tables["Security_List"].Columns.Contains("Type")){
				ds.Tables["Security_List"].Columns.Add("Type",Type.GetType("System.String"));
			}

			// If Action column is missing, add it
			if(!ds.Tables["Security_List"].Columns.Contains("Action")){
				ds.Tables["Security_List"].Columns.Add("Action",Type.GetType("System.String"));
			}

			// If Content column is missing, add it
			if(!ds.Tables["Security_List"].Columns.Contains("Content")){
				ds.Tables["Security_List"].Columns.Add("Content",Type.GetType("System.String"));
			}

			// If StartIP column is missing, add it
			if(!ds.Tables["Security_List"].Columns.Contains("StartIP")){
				ds.Tables["Security_List"].Columns.Add("StartIP",Type.GetType("System.String"));
			}
			
			// If EndIP column is missing, add it
			if(!ds.Tables["Security_List"].Columns.Contains("EndIP")){
				ds.Tables["Security_List"].Columns.Add("EndIP",Type.GetType("System.String"));
			}
		}

		#endregion

		#region function CreateFiltersSchema

		private void CreateFiltersSchema(DataSet ds)
		{
			// If table is missing, add it
			if(!ds.Tables.Contains("SmtpFilters")){
				ds.Tables.Add("SmtpFilters");
			}

			// If column is missing, add it
			if(!ds.Tables["SmtpFilters"].Columns.Contains("FilterID")){
				ds.Tables["SmtpFilters"].Columns.Add("FilterID",Type.GetType("System.String"));
			}

			// If column is missing, add it
			if(!ds.Tables["SmtpFilters"].Columns.Contains("Cost")){
				ds.Tables["SmtpFilters"].Columns.Add("Cost",Type.GetType("System.Int32"));
			}

			// If column is missing, add it
			if(!ds.Tables["SmtpFilters"].Columns.Contains("Assembly")){
				ds.Tables["SmtpFilters"].Columns.Add("Assembly",Type.GetType("System.String"));
			}

			// If column is missing, add it
			if(!ds.Tables["SmtpFilters"].Columns.Contains("ClassName")){
				ds.Tables["SmtpFilters"].Columns.Add("ClassName",Type.GetType("System.String"));
			}

			// If column is missing, add it
			if(!ds.Tables["SmtpFilters"].Columns.Contains("Enabled")){
				ds.Tables["SmtpFilters"].Columns.Add("Enabled",Type.GetType("System.Boolean"));
				ds.Tables["SmtpFilters"].Columns["Enabled"].DefaultValue = true;
			}

			// If column is missing, add it
			if(!ds.Tables["SmtpFilters"].Columns.Contains("Description")){
				ds.Tables["SmtpFilters"].Columns.Add("Description",Type.GetType("System.String"));
			}
		}

		#endregion


		#region function CreatePop3RemServSchema

		/// <summary>
		/// Creates pop3 remote servers schema.
		/// </summary>
		/// <param name="ds"></param>
		public void CreatePop3RemServSchema(DataSet ds)
		{			
			if(!ds.Tables.Contains("RemotePop3Servers")){
				ds.Tables.Add("RemotePop3Servers");
			}

			if(!ds.Tables["RemotePop3Servers"].Columns.Contains("Server")){
				ds.Tables["RemotePop3Servers"].Columns.Add("Server");
			}

			if(!ds.Tables["RemotePop3Servers"].Columns.Contains("Port")){
				ds.Tables["RemotePop3Servers"].Columns.Add("Port");
			}

			if(!ds.Tables["RemotePop3Servers"].Columns.Contains("UserName")){
				ds.Tables["RemotePop3Servers"].Columns.Add("UserName");
			}

			if(!ds.Tables["RemotePop3Servers"].Columns.Contains("Password")){
				ds.Tables["RemotePop3Servers"].Columns.Add("Password");
			}
		}

		#endregion


		#region function CreateSettingsSchema

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ds"></param>
		public void CreateSettingsSchema(DataSet ds)
		{
			if(!ds.Tables.Contains("Settings")){
				ds.Tables.Add("Settings");
			}

			if(!ds.Tables["Settings"].Columns.Contains("MailRoot")){
				ds.Tables["Settings"].Columns.Add("MailRoot");
			}

			if(!ds.Tables["Settings"].Columns.Contains("ConnectionString")){
				ds.Tables["Settings"].Columns.Add("ConnectionString");
			}

			if(!ds.Tables["Settings"].Columns.Contains("DataBaseType")){
				ds.Tables["Settings"].Columns.Add("DataBaseType");
			}

			if(!ds.Tables["Settings"].Columns.Contains("ErrorFile")){
				ds.Tables["Settings"].Columns.Add("ErrorFile");
			}

			if(!ds.Tables["Settings"].Columns.Contains("SMTP_IPAddress")){
				ds.Tables["Settings"].Columns.Add("SMTP_IPAddress");
			}

			if(!ds.Tables["Settings"].Columns.Contains("SMTP_Port")){
				ds.Tables["Settings"].Columns.Add("SMTP_Port");
			}

			if(!ds.Tables["Settings"].Columns.Contains("SMTP_Threads")){
				ds.Tables["Settings"].Columns.Add("SMTP_Threads");
			}

			if(!ds.Tables["Settings"].Columns.Contains("POP3_IPAddress")){
				ds.Tables["Settings"].Columns.Add("POP3_IPAddress");
			}

			if(!ds.Tables["Settings"].Columns.Contains("POP3_Port")){
				ds.Tables["Settings"].Columns.Add("POP3_Port");
			}

			if(!ds.Tables["Settings"].Columns.Contains("POP3_Threads")){
				ds.Tables["Settings"].Columns.Add("POP3_Threads");
			}

			if(!ds.Tables["Settings"].Columns.Contains("SmartHost")){
				ds.Tables["Settings"].Columns.Add("SmartHost");
			}

			if(!ds.Tables["Settings"].Columns.Contains("UseSmartHost")){
				ds.Tables["Settings"].Columns.Add("UseSmartHost");
			}

			if(!ds.Tables["Settings"].Columns.Contains("Dns1")){
				ds.Tables["Settings"].Columns.Add("Dns1");
			}

			if(!ds.Tables["Settings"].Columns.Contains("Dns2")){
				ds.Tables["Settings"].Columns.Add("Dns2");
			}

			if(!ds.Tables["Settings"].Columns.Contains("LogServer")){
				ds.Tables["Settings"].Columns.Add("LogServer");
			}

			if(!ds.Tables["Settings"].Columns.Contains("LogSMTPCmds")){
				ds.Tables["Settings"].Columns.Add("LogSMTPCmds");
			}

			if(!ds.Tables["Settings"].Columns.Contains("LogPOP3Cmds")){
				ds.Tables["Settings"].Columns.Add("LogPOP3Cmds");
			}

			if(!ds.Tables["Settings"].Columns.Contains("SMTP_SessionIdleTimeOut")){
				ds.Tables["Settings"].Columns.Add("SMTP_SessionIdleTimeOut");
			}

			if(!ds.Tables["Settings"].Columns.Contains("SMTP_CommandIdleTimeOut")){
				ds.Tables["Settings"].Columns.Add("SMTP_CommandIdleTimeOut");
			}

			if(!ds.Tables["Settings"].Columns.Contains("SMTP_MaxBadCommands")){
				ds.Tables["Settings"].Columns.Add("SMTP_MaxBadCommands");
			}

			if(!ds.Tables["Settings"].Columns.Contains("POP3_SessionIdleTimeOut")){
				ds.Tables["Settings"].Columns.Add("POP3_SessionIdleTimeOut");
			}

			if(!ds.Tables["Settings"].Columns.Contains("POP3_CommandIdleTimeOut")){
				ds.Tables["Settings"].Columns.Add("POP3_CommandIdleTimeOut");
			}

			if(!ds.Tables["Settings"].Columns.Contains("POP3_MaxBadCommands")){
				ds.Tables["Settings"].Columns.Add("POP3_MaxBadCommands");
			}

			if(!ds.Tables["Settings"].Columns.Contains("MaxMessageSize")){
				ds.Tables["Settings"].Columns.Add("MaxMessageSize");
			}

			if(!ds.Tables["Settings"].Columns.Contains("MaxRecipients")){
				ds.Tables["Settings"].Columns.Add("MaxRecipients");
			}

			if(!ds.Tables["Settings"].Columns.Contains("MaxRelayThreads")){
				ds.Tables["Settings"].Columns.Add("MaxRelayThreads");
			}

			if(!ds.Tables["Settings"].Columns.Contains("RelayInterval")){
				ds.Tables["Settings"].Columns.Add("RelayInterval");
			}

			if(!ds.Tables["Settings"].Columns.Contains("RelayRetryInterval")){
				ds.Tables["Settings"].Columns.Add("RelayRetryInterval");
			}

			if(!ds.Tables["Settings"].Columns.Contains("RelayUndeliveredWarning")){
				ds.Tables["Settings"].Columns.Add("RelayUndeliveredWarning");
			}

			if(!ds.Tables["Settings"].Columns.Contains("RelayUndelivered")){
				ds.Tables["Settings"].Columns.Add("RelayUndelivered");
			}

			if(!ds.Tables["Settings"].Columns.Contains("UndeliveredWarningTemplate")){
				ds.Tables["Settings"].Columns.Add("UndeliveredWarningTemplate");
			}

			if(!ds.Tables["Settings"].Columns.Contains("UndeliveredTemplate")){
				ds.Tables["Settings"].Columns.Add("UndeliveredTemplate");
			}

			if(!ds.Tables["Settings"].Columns.Contains("StoreUndeliveredMessages")){
				ds.Tables["Settings"].Columns.Add("StoreUndeliveredMessages");
			}

			if(!ds.Tables["Settings"].Columns.Contains("SMTP_LogPath")){
				ds.Tables["Settings"].Columns.Add("SMTP_LogPath");
			}

			if(!ds.Tables["Settings"].Columns.Contains("POP3_LogPath")){
				ds.Tables["Settings"].Columns.Add("POP3_LogPath");
			}

			if(!ds.Tables["Settings"].Columns.Contains("Server_LogPath")){
				ds.Tables["Settings"].Columns.Add("Server_LogPath");
			}

			if(!ds.Tables["Settings"].Columns.Contains("IMAP_LogPath")){
				ds.Tables["Settings"].Columns.Add("IMAP_LogPath").DefaultValue = "";
			}

			if(!ds.Tables["Settings"].Columns.Contains("IMAP_SessionIdleTimeOut")){
				ds.Tables["Settings"].Columns.Add("IMAP_SessionIdleTimeOut").DefaultValue = 800;
			}

			if(!ds.Tables["Settings"].Columns.Contains("IMAP_CommandIdleTimeOut")){
				ds.Tables["Settings"].Columns.Add("IMAP_CommandIdleTimeOut").DefaultValue = 60;
			}

			if(!ds.Tables["Settings"].Columns.Contains("IMAP_MaxBadCommands")){
				ds.Tables["Settings"].Columns.Add("IMAP_MaxBadCommands").DefaultValue = 10;
			}

			if(!ds.Tables["Settings"].Columns.Contains("LogIMAPCmds")){
				ds.Tables["Settings"].Columns.Add("LogIMAPCmds").DefaultValue = "false";
			}

			if(!ds.Tables["Settings"].Columns.Contains("IMAP_Port")){
				ds.Tables["Settings"].Columns.Add("IMAP_Port").DefaultValue = 143;
			}

			if(!ds.Tables["Settings"].Columns.Contains("IMAP_Threads")){
				ds.Tables["Settings"].Columns.Add("IMAP_Threads").DefaultValue = 100;
			}

			if(!ds.Tables["Settings"].Columns.Contains("IMAP_IPAddress")){
				ds.Tables["Settings"].Columns.Add("IMAP_IPAddress").DefaultValue = "ALL";
			}

			if(!ds.Tables["Settings"].Columns.Contains("SMTP_Enabled")){
				ds.Tables["Settings"].Columns.Add("SMTP_Enabled").DefaultValue = "true";
			}

			if(!ds.Tables["Settings"].Columns.Contains("POP3_Enabled")){
				ds.Tables["Settings"].Columns.Add("POP3_Enabled").DefaultValue = "true";
			}

			if(!ds.Tables["Settings"].Columns.Contains("IMAP_Enabled")){
				ds.Tables["Settings"].Columns.Add("IMAP_Enabled").DefaultValue = "true";
			}

			if(!ds.Tables["Settings"].Columns.Contains("SMTP_DefaultDomain")){
				ds.Tables["Settings"].Columns.Add("SMTP_DefaultDomain").DefaultValue = "";
			}

		}

		#endregion

		#endregion

		#endregion

		#region function DatabaseTypeChanged

		/// <summary>
		/// Sets Settings.xml specified Database type.
		/// </summary>
		public void DatabaseTypeChanged()
		{
			DataSet dsTmp = new DataSet();
			dsTmp.ReadXml(m_DataPath + "Settings.xml");

			DataRow dr = dsTmp.Tables["Settings"].Rows[0];
			m_MailStorePath = dr["MailRoot"].ToString();
			m_ConStr        = dr["ConnectionString"].ToString();
			m_DB_Type       = (DB_Type)Enum.Parse(typeof(DB_Type),dr["DataBaseType"].ToString());
			
			dsUsers    = new DataSet();
			dsAliases  = new DataSet();
			dsDomains  = new DataSet();
			dsRouting  = new DataSet();
			dsSecurity = new DataSet();
			dsFilters  = new DataSet();
			
			// We need to load all stuff to memory for XML
			if(m_DB_Type == DB_Type.XML){
				LoadUsers();
				LoadAliases();
				LoadRouting();
				LoadDomains();
				LoadSecurity();				
			}

			LoadFilters();
		}

		#endregion

		#region Helpers

		#region function GetTopLines

		private byte[] GetTopLines(Stream strm,int nrLines)
		{
			TextReader reader = (TextReader)new StreamReader(strm);
			
			strm.Position = 0;

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

				// Don't allow read more than 150 lines
				if(lCounter > 150){
					break;
				}

				lCounter++;
			}
	
			return System.Text.Encoding.ASCII.GetBytes(strBuilder.ToString());			
		}

		#endregion

		#endregion


		#region function AppendDirs

		private void AppendDirs(string path,ArrayList dirsAr,string remPath)
		{
			string[] dirs = Directory.GetDirectories(path);
			foreach(string dir in dirs){
				dirsAr.Add(dir.Substring(remPath.Length).Replace("\\","/"));
				AppendDirs(dir.Replace("\\","/"),dirsAr,remPath);
			}
		}

		#endregion

		#region function GetNextUid

		/// <summary>
		/// Gets,stores and returns free UID.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="mailbox"></param>
		/// <returns></returns>
		private int GetNextUid(string userName,string mailbox)
		{
			int uid = 1;

			switch(m_DB_Type)
			{
				#region DB_Type.XML

				case DB_Type.XML:
					// Try while get new id stored
					for(int i=0;i<50;i++){
						try
						{
							if(!File.Exists(m_MailStorePath + "\\Mailboxes\\" + userName + "\\" + mailbox.Replace("/","\\") + "\\_UID_holder")){
								using(TextWriter wr = File.CreateText(m_MailStorePath + "\\Mailboxes\\" + userName + "\\" + mailbox.Replace("/","\\") + "\\_UID_holder")){
									wr.Write("0");
								}
							}
							
							using(FileStream fs = File.Open(m_MailStorePath + "\\Mailboxes\\" + userName + "\\" + mailbox.Replace("/","\\") + "\\_UID_holder",FileMode.Open,FileAccess.ReadWrite,FileShare.None)){
								byte[] data = null;
								//--- Read current UID -----------
								data = new byte[fs.Length];
								fs.Read(data,0,data.Length);

								uid = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(data)) + 1;
								//---------------------------------

								// Write increased UID value
								fs.SetLength(0);
								data = System.Text.Encoding.ASCII.GetBytes(uid.ToString());
								fs.Write(data,0,data.Length);
							}

							// UID increased successfully
							break;
						}
						catch{ // Just try again
							System.Threading.Thread.Sleep(30);
						}
					}
					
				/*	string[] files = Directory.GetFiles(m_MailStorePath + "\\Mailboxes\\" + userName + "\\" + mailbox.Replace("/","\\"),"*.eml");
					ArrayList list = new ArrayList();
					foreach(string file in files){
						// date[yyyyMMddHHmmss]_int[uid]_int[flags]
						string[] fileParts = Path.GetFileNameWithoutExtension(file).Split('_');
						list.Add(Convert.ToInt32(fileParts[1]));
					}
			
					list.Sort();
					
					if(list.Count > 0){
						uid = (int)list[list.Count - 1] + 1;
					}*/
					break;

				#endregion
			}

			return uid;
		}

		#endregion


		#region static function IsConnection

		/// <summary>
		/// Checks if database connection is ok.
		/// </summary>
		/// <param name="dataPath"></param>
		/// <param name="conStr"></param>
		/// <param name="dbType"></param>
		/// <returns></returns>
		public static bool IsConnection(string dataPath,string conStr,DB_Type dbType)
		{
			try
			{
				switch(dbType)
				{
					case DB_Type.XML:
						return Directory.Exists(dataPath);

					case DB_Type.MSSQL:
						using(WSqlCommand sqlCmd = new WSqlCommand(conStr,"lspr_CheckConnection")){
							sqlCmd.Execute();
						}
						return true;
				}				
			}
			catch{				
			}

			return false;
		}

		#endregion
	
	}
}
