using System;
using System.IO;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;
using LumiSoft.MailServer;
using LumiSoft.Net.Mime;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class lsSpamFilter : ISmtpMessageFilter
	{
		public lsSpamFilter()
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
		public FilterResult Filter(MemoryStream messageStream,out MemoryStream filteredStream,string sender,string[] recipients,ServerAPI api)
		{
			messageStream.Position = 0;
			filteredStream = messageStream; // we don't change message content, just return same stream

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
			MimeParser parser = new MimeParser(messageStream.ToArray());
			GetEntries(parser.MimeEntries,entries);

			System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
			foreach(MimeEntry ent in entries){
				if(ent.Data != null){
					string md5Hash = Convert.ToBase64String(md5.ComputeHash(ent.Data));

					foreach(DataRow dr in ds.Tables["ContentMd5"].Rows){
						// Message contains blocked content(attachment,...)
						if(dr["EntryMd5Value"].ToString() == md5Hash){
							WriteFilterLog(DateTime.Now.ToString() + " From:" + sender + " Subject:\"" + parser.Subject + "\"  Contained blocked content\r\n");
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
						//--- Send blocked note to sender
						MimeConstructor m = new MimeConstructor();
						m.Body = "Message was blocked by server and considered as SPAM !!!\n\nCaused by keywords: " + keyWords + "\n\nMaximum total cost is 100 !";
						m.From = "postmaster";
						m.To   = new string[]{sender};
						m.Attachments.Add(new Attachment("data.eml",messageStream.ToArray()));

						using(MemoryStream msg = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(m.ConstructMime()))){
							api.StoreMessage("","",msg,sender,"",true,DateTime.Now,0);
						}

						WriteFilterLog(DateTime.Now.ToString() + " From:" + sender + " Blocked KeyWords: " + keyWords + "\r\n");

						return FilterResult.DontStore;
					}
				}
			}
			//---------------------------------
			return FilterResult.Store;
		}

		private void GetEntries(ArrayList entries,ArrayList allEntries)
		{				
			if(entries != null){
				allEntries.AddRange(entries);
			}

			if(entries != null){
				foreach(MimeEntry ent in entries){
					GetEntries(ent.MimeEntries,allEntries);
				}
			}
		}

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
	}
}
