using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using LumiSoft.MailServer;

namespace WebServices
{
	/// <summary>
	/// Summary description for WConnectionCheck_Web.
	/// </summary>
	internal class RemoteAdmin : System.Web.Services.WebService
	{
		private string m_SettingsPath = "";
		/// <summary>
		/// 
		/// </summary>
		public RemoteAdmin()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();				

			m_SettingsPath = (string)Application["SettingsPath"];
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion
		

		#region DomainName related

		#region function GetDomainList

		[WebMethod()]
		public DataSet GetDomainList()
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.GetDomainList().Table.DataSet;
		}

		#endregion


		#region function AddDomain

		[WebMethod()]
		public DataSet AddDomain(string domainName,string Description)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.AddDomain(domainName,Description).Table.DataSet;
		}

		#endregion

		#region function DeleteDomain

		[WebMethod()]
		public void DeleteDomain(string domainID)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.DeleteDomain(domainID);
		}

		#endregion

		#region function DomainExists

		[WebMethod()]
		public bool DomainExists(string source)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.DomainExists(source);
		}

		#endregion

		#endregion


		#region User related

		#region function GetUserList

		[WebMethod()]
		public DataSet GetUserList(string domainID)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.GetUserList(domainID).Table.DataSet;
		}

		#endregion


		#region function AddUser

		[WebMethod()]
		public DataSet AddUser(string fullName,string userName,string password,string Description,string emails,string domainID,int mailboxSize,bool enabled,bool allowRelay,byte[] remPop3Accounts)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.AddUser(fullName,userName,password,Description,emails,domainID,mailboxSize,enabled,allowRelay,remPop3Accounts).Table.DataSet;
		}

		#endregion

		#region function DeleteUser

		[WebMethod()]
		public void DeleteUser(string userID)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.DeleteUser(userID);
		}

		#endregion

		#region function UpdateUser

		[WebMethod()]
		public void UpdateUser(string userID,string fullName,string password,string Description,string emails,string domainID,int mailboxSize,bool enabled,bool allowRelay,byte[] remPop3Accounts)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.UpdateUser(userID,fullName,password,Description,emails,domainID,mailboxSize,enabled,allowRelay,remPop3Accounts);
		}

		#endregion

		#region function MailboxExists

		[WebMethod()]
		public bool MailboxExists(string userName)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.MailboxExists(userName);
		}

		#endregion

		#region function EmailAddressExists

		[WebMethod()]
		public bool EmailAddressExists(string emailAddress)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.EmailAddressExists(emailAddress);
		}

		#endregion

		#endregion

		#region AliasName related

		#region function GetAliasesList

		[WebMethod()]
		public DataSet GetAliasesList(string DomainName)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.GetAliasesList(DomainName).Table.DataSet;
		}

		#endregion


		#region function AddAlias

		[WebMethod()]
		public DataSet AddAlias(string aliasName,string Description,string AliasMembers,string domainID,bool isPublic)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.AddAlias(aliasName,Description,AliasMembers,domainID,isPublic).Table.DataSet;
		}

		#endregion

		#region function DeleteAlias

		[WebMethod()]
		public void DeleteAlias(string aliasID)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.DeleteAlias(aliasID);
		}

		#endregion

		#region function UpdateAlias

		[WebMethod()]
		public void UpdateAlias(string aliasID,string aliasName,string Description,string AliasMembers,string domainID,bool isPublic)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.UpdateAlias(aliasID,aliasName,Description,AliasMembers,domainID,isPublic);
		}

		#endregion

		#endregion

		#region Routing related

		#region function GetRouteList

		[WebMethod()]
		public DataSet GetRouteList()
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.GetRouteList().Table.DataSet;
		}

		#endregion


		#region function AddRoute

		[WebMethod()]
		public DataSet AddRoute(string pattern,string mailbox,string Description,string domainID)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.AddRoute(pattern,mailbox,Description,domainID).Table.DataSet;
		}

		#endregion

		#region function DeleteRoute

		[WebMethod()]
		public void DeleteRoute(string routeID)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.DeleteRoute(routeID);
		}

		#endregion

		#region function UpdateRoute

		[WebMethod()]
		public void UpdateRoute(string routeID,string pattern,string mailbox,string Description,string domainID)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.UpdateRoute(routeID,pattern,mailbox,Description,domainID);
		}

		#endregion

		#endregion


		#region Security related

		#region function GetSecurityList

		[WebMethod()]
		public DataSet GetSecurityList()
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.GetSecurityList().Table.DataSet;
		}

		#endregion


		#region function AddSecurityEntry

		[WebMethod()]
		public DataSet AddSecurityEntry(string Description,string protocol,string type,string action,string content,long startIP,long endIP)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.AddSecurityEntry(Description,protocol,type,action,content,startIP,endIP).Table.DataSet;
		}

		#endregion

		#region function DeleteSecurityEntry

		[WebMethod()]
		public void DeleteSecurityEntry(string securityID)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.DeleteSecurityEntry(securityID);
		}

		#endregion

		#region function UpdateSecurityEntry

		[WebMethod()]
		public void UpdateSecurityEntry(string securityID,string Description,string protocol,string type,string action,string content,long startIP,long endIP)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.UpdateSecurityEntry(securityID,Description,protocol,type,action,content,startIP,endIP);
		}

		#endregion

		#endregion

		#region Filters related

		#region function GetFilterList

		[WebMethod()]
		public DataSet GetFilterList()
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.GetFilterList().Table.DataSet;
		}

		#endregion


		#region function AddFilter

		[WebMethod()]
		public DataSet AddFilter(string description,string assembly,string className,int cost,bool enabled)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.AddFilter(description,assembly,className,cost,enabled).Table.DataSet;
		}

		#endregion

		#region function DeleteFilter

		[WebMethod()]
		public void DeleteFilter(string filterID)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.DeleteFilter(filterID);
		}

		#endregion

		#region function UpdateFilter

		[WebMethod()]
		public void UpdateFilter(string filterID,string description,string assembly,string className,int cost,bool enabled)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.UpdateFilter(filterID,description,assembly,className,cost,enabled);
		}

		#endregion

		#endregion


		#region BackUp related

		#region function CreateBackUp

		[WebMethod()]
		public byte[] CreateBackUp()
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.CreateBackUp();
		}

		#endregion

		#region function RestoreBackUp

		[WebMethod()]
		public void RestoreBackUp(byte[] data)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.RestoreBackUp(data);
		}

		#endregion

		#endregion


		#region Settings

		#region function GetSettings

		[WebMethod()]
		public DataSet GetSettings()
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			return api.GetSettings();
		}

		#endregion

		#region function UpdateSettings

		[WebMethod()]
		public void UpdateSettings(DataSet dsSettings)
		{
			ServerAPI api = new ServerAPI(m_SettingsPath);
			api.UpdateSettings(dsSettings);
		}

		#endregion

		#endregion

	}
}
