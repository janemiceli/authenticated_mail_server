using System;
using System.IO;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;

using LumiSoft.MailServer;
using LumiSoft.Net.SMTP.Server;
using LumiSoft.Net.Mime;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class lsSpamFilter : ISmtpMessageFilter
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public lsSpamFilter()
		{			
		}

		#region method Filter

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

			messageStream.Position = 0;
			filteredStream = messageStream; // we don't change message content, just return same stream

			try{
				//--- Load data -----------------------
				DataSet ds = new DataSet();
				DataTable dt = ds.Tables.Add("KewWords");
				dt.Columns.Add("Cost",typeof(int));
				dt.Columns.Add("KeyWord");
				
				dt = ds.Tables.Add("ContentMd5");
				dt.Columns.Add("Description");
				dt.Columns.Add("EntryMd5Value");
				ds.ReadXml(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\lsSpam_db.xml");

				//--- Do mime parts data md5 hash compare ----------------
				ArrayList entries = new ArrayList();
				Mime parser = Mime.Parse(messageStream);

				System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
				foreach(MimeEntity ent in parser.MimeEntities){
					if(ent.Data != null){
						string md5Hash = Convert.ToBase64String(md5.ComputeHash(ent.Data));

						foreach(DataRow dr in ds.Tables["ContentMd5"].Rows){
							// Message contains blocked content(attachment,...)
							if(dr["EntryMd5Value"].ToString() == md5Hash){
								WriteFilterLog(DateTime.Now.ToString() + " From:" + sender + " Subject:\"" + parser.MainEntity.Subject + "\"  Contained blocked content:hash=" + md5Hash + "\r\n");
								return FilterResult.DontStore;
							}
						}
					}
				}
				
				byte[] topLines = new byte[2000];
				if(messageStream.Length < 2000){
					topLines = new byte[messageStream.Length];
				}
				messageStream.Read(topLines,0,topLines.Length);

				string lines = System.Text.Encoding.ASCII.GetString(topLines).ToLower();

				//--- Try spam keywords -----------			
				int totalCost = 0;
				string keyWords = "";
				DataView dv = ds.Tables["KewWords"].DefaultView;
				dv.Sort = "Cost DESC";
				foreach(DataRowView drV in dv){
					if(lines.IndexOf(drV.Row["KeyWord"].ToString().ToLower()) > -1){
						totalCost += Convert.ToInt32(drV.Row["Cost"]);

						keyWords += drV.Row["KeyWord"].ToString() + " cost:" + drV.Row["Cost"].ToString() + " ";

						// Check that total cost isn't exceeded
						if(totalCost > 99){
							errorText = "Message was blocked by server and considered as SPAM !";

							WriteFilterLog(DateTime.Now.ToString() + " From:" + sender + " Blocked KeyWords: " + keyWords + "\r\n");

							return FilterResult.Error;
						}
					}
				}
				//---------------------------------

                // Reset stream position
                messageStream.Position = 0;

				return FilterResult.Store;
			}
			catch(Exception x){
				return FilterResult.DontStore;
			}
		}

		#endregion

		#region method WriteFilterLog

		private void WriteFilterLog(string text)
		{
			try{
				using(FileStream fs = new FileStream(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\lsSpam_block.log",FileMode.OpenOrCreate)){
					fs.Seek(0,SeekOrigin.End);
					byte[] data = System.Text.Encoding.ASCII.GetBytes(text);
					fs.Write(data,0,data.Length);					
				}
			}
			catch{
			}
		}

		#endregion
	}
}
