using System;
using System.IO;
using System.Data;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Provides methods to handle messages(store,retrieve,...).
	/// </summary>
	public class MailStore
	{
		private static string m_MailStorePath = "";


		#region function GetMessageList

	/*	/// <summary>
		/// Get user mail messages.
		/// </summary>
		/// <param name="mailbox"></param>
		/// <param name="e"></param>
		public static void GetMessageList(string mailbox,LumiSoft.Net.POP3.Server.GetMessagesInfo_EventArgs e)
		{
			try
			{	
				MailServer.API.GetMessageList(mailbox,e.Messages);
			}
			catch(Exception x)
			{				
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}
*/
		#endregion


		#region function StoreMessage
/*
		/// <summary>
		/// Stores message to specifeied mailbox.
		/// </summary>
		/// <param name="mailbox">Mailbx name.</param>
		/// <param name="msgStream">Message stream.</param>
		/// <param name="to">Receptient e-adress.</param>
		/// <param name="from">Sender e-address.</param>
		/// <param name="relay">Specifies if message must be stored to relay folder.</param>
		public static void StoreMessage(string mailbox,MemoryStream msgStream,string to,string from,bool relay)
		{
			try
			{	
			//	if(relay){ // Code movet to server api
			//		// Create dummy file name
			//		string filename = Guid.NewGuid().ToString();
			//		       filename = filename.Substring(0,22);
			//	           filename = filename.Replace("-","_");
//
//					string path = m_MailStorePath + "Relay\\";
//
//					// Check if Directory exists, if not Create
//					if(!Directory.Exists(path)){
//						Directory.CreateDirectory(path);
//					}
//
//					//---- Write message data to file -------------------------------//
//					using(FileStream fStream = File.Create(path + "\\" + filename + ".eml",(int)msgStream.Length)){
//
//						// Write internal relay info line at the beginning of messsage.
//						// Note: This line is skipped when sending to destination host,
//						// actual message begins from 2 line.
//						// Header struct: 'RelayInfo:IsUndeliveredWarningSent<TAB>To<TAB>Sender<TAB>Date\r\n'
//						string internalServerHead = "RelayInfo:0\t" + to + "\t" + from + "\t" + DateTime.Now.ToString("r",System.Globalization.DateTimeFormatInfo.InvariantInfo) + "\r\n";
//						byte[] sHead = System.Text.Encoding.Default.GetBytes(internalServerHead);
//						fStream.Write(sHead,0,sHead.Length);
//
//						msgStream.WriteTo(fStream);
//					}
//					//---------------------------------------------------------------//
//				}
//				else{
					MailServer.API.StoreMessage(mailbox,msgStream,to,from,relay);
			//	}
				
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}
*/
		#endregion

		#region function GetMessage

/*		/// <summary>
		/// Gets Mail Message from specified Mailbox.
		/// </summary>
		/// <param name="mailbox">Mailbox name</param>
		/// <param name="msgID">Message MessageID</param>
		/// <returns></returns>
		public static byte[] GetMessage(string mailbox,string msgID)
		{
			byte[] retVal = null;

			try
			{
				retVal = MailServer.API.GetMessage(mailbox,msgID);
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}

			return retVal;
		}*/

		#endregion

		#region function DeleteMessage

	/*	/// <summary>
		/// Deletes Message from specified Mailbox.
		/// </summary>
		/// <param name="mailbox">Mailbox name</param>
		/// <param name="msgID">MessageID.</param>
		public static void DeleteMessage(string mailbox,string msgID)
		{
			try
			{
				MailServer.API.DeleteMessage(mailbox,msgID);
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}*/

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets or set mail store path.
		/// </summary>
		public static string MailStorePath
		{
			get{ return m_MailStorePath; }

			set{ m_MailStorePath = value; }
		}

		#endregion

	}
}
