using System;
using System.IO;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using LumiSoft.MailServer;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class lsVirusFilter : ISmtpMessageFilter
	{
		/// <summary>
		/// 
		/// </summary>
		public lsVirusFilter()
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
			filteredStream = null;

			// Store message to tmp file
			string mailStorePath = api.GetSettings().Tables["Settings"].Rows[0]["MailRoot"].ToString();

			if(!Directory.Exists(mailStorePath + "tmpScan")){
				Directory.CreateDirectory(mailStorePath + "tmpScan");
			}

			string file = mailStorePath + "tmpScan\\" + Guid.NewGuid().ToString() + ".eml";
			using(FileStream fs = File.Create(file)){
				messageStream.WriteTo(fs);
			}

			// Execute virus program to scan tmp message
			// #FileName - place holder is replaced with file
			DataSet ds = new DataSet();
			DataTable dt = ds.Tables.Add("Settings");
			dt.Columns.Add("Program");
			dt.Columns.Add("Arguments");
			ds.ReadXml(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\lsVirusFilter_db.xml");

			string virusSoft     = ds.Tables["Settings"].Rows[0]["Program"].ToString();
			string virusSoftArgs = ds.Tables["Settings"].Rows[0]["Arguments"].ToString().Replace("#FileName",file);

			System.Diagnostics.ProcessStartInfo sInf = new System.Diagnostics.ProcessStartInfo(virusSoft,virusSoftArgs);
			sInf.CreateNoWindow  = true;
			System.Diagnostics.Process p = System.Diagnostics.Process.Start(sInf);
			if(p != null){
				p.WaitForExit(100000);
			}

			// Return scanned message and delete tmp file
			using(FileStream fs = File.OpenRead(file)){
				byte[] data = new byte[fs.Length];
				fs.Read(data,0,data.Length);

				filteredStream = new MemoryStream(data);
			}

			File.Delete(file);
			
			return FilterResult.Store;
		}
	}
}
