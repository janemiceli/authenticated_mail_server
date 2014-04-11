using System;
using System.Web;
using System.Net;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Summary description for _Core.
	/// </summary>
	internal class _Core
	{
	//	public _Core()
	//	{
	//	}

		#region function InitWebService

		/// <summary>
		/// Sets WebService propeties (url,UserName, ...).
		/// </summary>
		/// <param name="url"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <param name="webService"></param>
		public static void InitWebService(string url,string userName,string password,System.Web.Services.Protocols.SoapHttpClientProtocol webService)
		{
			//---- Init WebService settings
			webService.Url = url;
				
			//--- Set Web Application security settings ----------------------------------------------------------//
			NetworkCredential myCred = new NetworkCredential(userName,password);
			CredentialCache myCache  = new CredentialCache();
			myCache.Add(new Uri(url),"basic",myCred);
			webService.Credentials = myCache;
			//----------------------------------------------------------------------------------------------------//

			//----- Set Proxy settings ---------------------------------------------------------------------------//				
			WebProxy proxy    = WebProxy.GetDefaultProxy();
			proxy.Credentials = CredentialCache.DefaultCredentials;
				
			webService.Proxy  = proxy;
			//-------------------------------------------------------------//
		}

		#endregion
	}
}
