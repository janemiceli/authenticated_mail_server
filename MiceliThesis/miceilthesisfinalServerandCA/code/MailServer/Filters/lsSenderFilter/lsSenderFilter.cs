using System;
using System.IO;
using System.Data;

using LumiSoft.MailServer;
using LumiSoft.Net;
using LumiSoft.Net.Dns.Client;
using LumiSoft.Net.SMTP.Server;


namespace LumiSoft.MailServer
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class lsSenderFilter : ISmtpSenderFilter
	{
		/// <summary>
		/// 
		/// </summary>
		public lsSenderFilter()
		{			
		}

		/// <summary>
		/// Filters sender.
		/// </summary>
		/// <param name="from">Sender.</param>
		/// <param name="api">Reference to server API.</param>
		/// <param name="session">Reference to SMTP session.</param>
		/// <param name="errorText">Filtering error text what is returned to client. ASCII text, 100 chars maximum.</param>
		/// <returns>Returns true if sender is ok or false if rejected.</returns>
		public bool Filter(string from,IMailServerApi api,SMTP_Session session,out string errorText)
		{	
			errorText = "";
			string ip = session.RemoteEndPoint.Address.ToString();

			Dns_Client dns = new Dns_Client();
			
			bool ok = false;

			// Don't check PTR for authenticated session and LAN IP ranges
			if(session.Authenticated || ip.StartsWith("127.0.0.1") || ip.StartsWith("10.") || ip.StartsWith("192.168")){
				return true;
			}

			DnsServerResponse reponse = dns.Query(ip,QTYPE.PTR);
			if(reponse.ResponseCode == RCODE.NO_ERROR){
				foreach(PTR_Record rec in reponse.GetPTRRecords()){
					if(rec.DomainName.ToLower() == session.EhloName.ToLower()){
						ok = true;
						break;
					}
				}
			}

			if(!ok){
				errorText = "Bad EHLO/HELO name, you must have valid DNS PTR record for your EHLO name and IP.";
			}

			return ok;
		}
	}
}
