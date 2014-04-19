using System;
using System.IO;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

using LumiSoft.MailServer;
using LumiSoft.Net.SMTP.Server;
using LumiSoft.Net.Mime;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Invalid message scanner.
	/// </summary>
	public class lsInvalidMessageScanner : ISmtpMessageFilter
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public lsInvalidMessageScanner()
		{			
		}

		/// <summary>
		/// Filters message.
		/// </summary>
		/// <param name="messageStream">Message stream which to filter.</param>
		/// <param name="filteredStream">Filtered stream.</param>
		/// <param name="sender">Senders email address.</param>
		/// <param name="recipients">Recipients email addresses.</param>
		/// <param name="api">Access to server API.</param>
		/// <param name="session">Reference to SMTP session.</param>
		/// <param name="errorText">Filtering error text what is returned to client. ASCII text, 500 chars maximum.</param>
		public FilterResult Filter(Stream messageStream,out Stream filteredStream,string sender,string[] recipients,IMailServerApi api,SMTP_Session session,out string errorText)
		{
			errorText = "";
			filteredStream = messageStream;

			try{
				messageStream.Position = 0;

		//		long pos = messageStream.Position;
				string headers = MimeUtils.ParseHeaders(messageStream).ToLower();
		//		messageStream.Position = pos;

				//--- Check required header fields ---------//
				bool headersOk = true;
				if(headers.IndexOf("from:") == -1){
					errorText = "Required From: header field is missing !";
					headersOk = false;
				}
				else if(headers.IndexOf("to:") == -1){
					errorText = "Required To: header field is missing !";
					headersOk = false;
				}
				else if(headers.IndexOf("subject:") == -1){
					errorText = "Required Subject: header field is missing !";
					headersOk = false;
				}
				//------------------------------------------//

				// Check invalid <CR> or <LF> in headers. Header may not contain <CR> without <LF>, 
				// <CRLF> must be in pairs. 
				if(headers.Replace("\r\n","").IndexOf("\r") > -1 || headers.Replace("\r\n","").IndexOf("\n") > -1){
					errorText = "Message contains invalid  <CR> or <LF> combinations !";
					headersOk = false;
				}
				//-------------------------------------------------------------------------------//

				if(!headersOk){
					return FilterResult.Error;
				}
			}
			catch{
			}
			
            // Reset stream position
            messageStream.Position = 0;

			return FilterResult.Store;
		}
	}
}
