using System;
using System.Data; 
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace LumiSoft.Net.NNTP.Server
{

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
	/// Summary description for NNTP_API.
	/// </summary>
	public class NNTP_API
	{
		private static  Regex _isNumber   = new Regex(@"^\d+$");
		private string  m_DataPath        = "";
		private string  m_NNTPStorePath   = "";
		private string  m_ConStr          = "";
		private DB_Type m_DB_Type         = DB_Type.XML;


		public NNTP_API(string dataPath)
		{
			m_DataPath      = dataPath;

			DataSet dsTmp = new DataSet();
			dsTmp.ReadXml(dataPath + "Settings.xml");

			DataRow dr = dsTmp.Tables["Settings"].Rows[0];
			m_NNTPStorePath = dr["NNTPRoot"].ToString();
			m_ConStr        = dr["ConnectionString"].ToString();
			m_DB_Type       = (DB_Type)Enum.Parse(typeof(DB_Type),dr["DataBaseType"].ToString());


		}

		#region GetGroups
		/// <summary>
		/// Returns all the newsgroups with article count.
		/// </summary>
		public void GetGroups(NNTP_NewsGroups groups)
		{
			DataSet ds = new DataSet();
			ds.ReadXml(m_DataPath + "NewsGroups.xml");
			foreach(DataRow dr in ds.Tables[0].Rows)
			{
				groups.Add(dr["Name"].ToString(),GetArticleCount(dr["Name"].ToString()),GetMinArticle(dr["Name"].ToString()),GetMaxArticle((string)dr["Name"]),Convert.ToDateTime(dr["Date"].ToString()));
			}
		}
		#endregion

		#region AddGroup
		/// <summary>
		/// Add a newsgroup.
		/// </summary>
		public void AddGroup(string group)
		{
			DataSet ds = new DataSet();
			ds.ReadXml(m_DataPath + "NewsGroups.xml");
					
			DataRow dr = ds.Tables[0].NewRow();
			dr["Name"] = group.ToLower().Trim();
			dr["Date"] = DateTime.Now;
			ds.Tables[0].Rows.Add(dr);
			ds.WriteXml(m_DataPath + "NewsGroups.xml");

		}
		#endregion

		#region GetArticleCount
		/// <summary>
		/// Gets the article count for a newsgroups.
		/// </summary>
		public int GetArticleCount(string group)
		{
			group = group.Replace(".","\\"); 
			string path = m_NNTPStorePath + "groups\\" + group;
			// Check if Directory exists, if not ret -1 to indicate group doesn't exists
			if(!Directory.Exists(path)){return -1;}
			return Directory.GetFiles(path).Length;   
		}
		#endregion

		#region GetMinArticle
		/// <summary>
		/// Gets the article count for a newsgroups.
		/// </summary>
		public int GetMinArticle(string group)
		{
			group = group.Replace(".","\\"); 
			string path = m_NNTPStorePath + "groups\\" + group;
			// Check if Directory exists, if not ret -1 to indicate group doesn't exists
			if(!Directory.Exists(path)){return -1;}
			string[] files = Directory.GetFiles(path,"*.txt");
			if(files.Length > 0)
			{
				string filename = Path.GetFileNameWithoutExtension(files[0]);
				return Convert.ToInt32(filename.Substring(0,filename.IndexOf("_"))); 
			}
			else
			{
                return 0;
			}

		}
		#endregion

		#region GetMaxArticle
		/// <summary>
		/// Gets the article count for a newsgroups.
		/// </summary>
		public int GetMaxArticle(string group)
		{
			group = group.Replace(".","\\"); 
			string path = m_NNTPStorePath + "groups\\" + group;
			// Check if Directory exists, if not ret -1 to indicate group doesn't exists
			if(!Directory.Exists(path)){return -1;}
			string[] files = Directory.GetFiles(path,"*.txt");
			if(files.Length > 0)
			{
				string filename = Path.GetFileNameWithoutExtension(files[files.Length-1]);
				return Convert.ToInt32(filename.Substring(0,filename.IndexOf("_"))); 
			}
			else
			{
				return 0;
			}
		}

		#endregion

		#region GetArticles
		/// <summary>
		/// Gets article info for a newsgroup.
		/// </summary>
		public void GetArticles(string group,NNTP_Articles articles)
		{		 
			byte[] fileData       = null;
			int    msgNo          = 1;
			Hashtable files       = new Hashtable();
			group = group.Replace(".","\\"); 
			string path = m_NNTPStorePath + "groups\\" + group;
			if(!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
				AddGroup(group);
			}
			string[] articls = Directory.GetFiles(path,"*.txt");
			
			foreach(string articl in articls)
			{
				using(FileStream fs = File.OpenRead(articl))
				{
					fileData = new byte[fs.Length];
					fs.Read(fileData,0,(int)fs.Length);
				}
				HeaderParser hp = new HeaderParser(fileData);
				msgNo = Convert.ToInt32(Path.GetFileNameWithoutExtension(articl).Substring(0,Path.GetFileNameWithoutExtension(articl).IndexOf("_"))); 
				string date = hp.MessageDate.ToString();
				articles.Add(msgNo,hp.MessageID,hp.Subject,hp.From,date,hp.References,hp.Lines,fileData.Length.ToString());	               
				

			}
		}

		#endregion

		#region GetNewNews
		/// <summary>
		/// Gets new article info for newsgroups.
		/// </summary>
		public void GetNewNews(string groups,DateTime since,NNTP_Articles articles)
		{		 
			byte[] fileData       = null;
			int    msgNo          = 1;
			Hashtable files       = new Hashtable();

			NNTP_NewsGroups newsgroups = ParseNewsGroups(groups); 

			foreach(NNTP_NewsGroup group in newsgroups.Newsgroups)
			{

				string groupPath = group.Name.Replace(".","\\"); 
				string path = m_NNTPStorePath + "groups\\" + groupPath;
				if(!Directory.Exists(path)){
					Directory.CreateDirectory(path);
					AddGroup(group.Name);
				}
				string[] articls = Directory.GetFiles(path,"*.txt");
			
				foreach(string articl in articls)
				{
					using(FileStream fs = File.OpenRead(articl))
					{
						fileData = new byte[fs.Length];
						fs.Read(fileData,0,(int)fs.Length);
					}
					HeaderParser hp = new HeaderParser(fileData);
					msgNo = Convert.ToInt32(Path.GetFileNameWithoutExtension(articl).Substring(0,Path.GetFileNameWithoutExtension(articl).IndexOf("_"))); 
					string date = hp.MessageDate.ToString();
					
					if(Convert.ToDateTime(date).CompareTo(since) > 0)
					{
						articles.Add(hp.MessageID,msgNo,hp.Subject,hp.From,date,hp.References,hp.Lines,fileData.Length.ToString());	               
					}
				}
			}
		}

		#endregion

		#region ParseNewsgroups
		public NNTP_NewsGroups ParseNewsGroups(string wildcard)
		{
			NNTP_NewsGroups allGroups = new NNTP_NewsGroups();  
			GetGroups(allGroups);

			NNTP_NewsGroups selGroups = new NNTP_NewsGroups();  

			string[] groupParams = wildcard.Split(Convert.ToChar(","));
			
			foreach(string param in groupParams)
			{
				if(param == "*")
				{
					return allGroups;
				}        
				if(param.IndexOf("*") == -1)
				{
					if(allGroups.Contains(param))
					{
						selGroups.Add(param,0,0,0,DateTime.Today);                     
					}
				}
				if(param.IndexOf("*") >= 0)
				{
					foreach(NNTP_NewsGroup ng in allGroups.Newsgroups)
					{
						if(ng.Name.CompareTo(param) == param.Length -1)
						{
							selGroups.Add(param,0,0,0,DateTime.Today);                     							
						}
					}
				}

			}
			return selGroups;

		}
		#endregion

		#region FindArticle
		/// <summary>
		/// Finds the newsgroup(s) that contains specified articleId
		/// </summary>
		/// <param name="articleId">article Id to search.</param>
		public string[] FindArticle(string articleId)
		{
			DataSet ds = new DataSet();
			ds.ReadXml(m_DataPath + "NewsGroups.xml");
			string newsgroups = "";
			foreach(DataRow dr in ds.Tables[0].Rows)
			{
				string group = dr["Name"].ToString().Replace(".","\\");
				string path = m_NNTPStorePath + "groups\\" + group;
				string fileName = path + "\\" + articleId + ".txt";
				if(!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
					AddGroup(group);
				}
				if(Directory.GetFiles(path,"*_" + articleId + ".txt").Length != 0) 
				{
                    newsgroups += group + ",";               
				}
			}            
			if(newsgroups.Length > 0)
			{
				newsgroups = newsgroups.Substring(0,newsgroups.Length -1); 
			}
			return newsgroups.Split(Convert.ToChar(","));
		}

		#endregion

		#region GetArticleFileName

		public string GetArticleFileName(string newsgroup,string articleId)
		{
			string path = m_NNTPStorePath + "groups\\" + newsgroup;
			string fileName = path + "\\" + articleId + ".txt";
			string[] match = Directory.GetFiles(path,"*_" + articleId + ".txt");

			if(match.Length != 0) 
			{
				return match[0];       
			}
			else
			{
				return "";
			}


		}

		#endregion

		#region GetArticle
		/// <summary>
		/// Gets an article.
		/// </summary>
		public string GetArticle(string group,string article,string retVal)
		{		 
			byte[] fileData       = null;
			string fileName       = "";
			string path           = "";

			article = RemInvalidChars(article);
			group = group.Replace(".","\\"); 
			path = m_NNTPStorePath + "groups\\" + group;

			if(IsNumeric(article.Trim()))
			{
				int no = Convert.ToInt32(article.Trim());
				article = no.ToString("00000");
				string[] articls = Directory.GetFiles(path,article + "_*");					 
				if(articls.Length > 0)
				{
					fileName = articls[0];
				}    
			}
			else
			{
				group = group.Replace(".","\\"); 
				path = m_NNTPStorePath + "groups\\" + group;
				if(!Directory.Exists(path)){
					Directory.CreateDirectory(path);
					AddGroup( group.Replace("\\","."));
				}
				article = article.Replace("<","");article = article.Replace(">","");
				fileName = path + "\\" + article + ".txt";
				if(!File.Exists(fileName))
				{
					string[] newsgroups = FindArticle(article);
					if(newsgroups[0] != "")
					{
						group = newsgroups[0].Replace(".","\\"); 
						path = m_NNTPStorePath + "groups\\" + group;
						fileName = path + "\\" + article + ".txt";
						fileName = GetArticleFileName(newsgroups[0],article);
						if(!File.Exists(fileName)){}
					}
					else
					{
						return "";
					}
				}			
			}

			using(FileStream fs = File.OpenRead(fileName))
			{
				fileData = new byte[fs.Length];
				fs.Read(fileData,0,(int)fs.Length);
			}

			retVal = System.Text.Encoding.ASCII.GetString(fileData);					
			return retVal; 
            
		}

		#endregion

		#region StoreMessage
		
		public string StoreMessage(MemoryStream msgStream,string[] newsgroups)
		{
			string from       = HeaderParser.ParseHeaderFields("FROM:",msgStream.ToArray()).Replace("FROM:","").Trim();  
			string fromDomain = from.Substring(from.IndexOf("@")).Replace(">","");
			string id         = Guid.NewGuid().ToString();
			int    onGroup    = 0;
			string fileName   = "";

			id = id.Substring(0,30);
			id = id.Replace("-","");			
			id += fromDomain;

			foreach(string group in newsgroups)
			{
				string newsgroup = group.ToUpper().Replace("\r\n","").Replace("NEWSGROUPS:","").Trim();
				newsgroup = newsgroup.ToLower().Replace(".","\\"); 
				string path = m_NNTPStorePath + "groups\\" + newsgroup;
				
				// Check if Directory exists, if not Create
				if(!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
					AddGroup(group);
				}
				

				if(onGroup == 0) //Only add Message-Id once when posting to multiple groups
				{
					fileName = GetNextUid(newsgroup) + "_" + id;
					msgStream = HeaderParser.AddHeader(msgStream,"Message-ID:","<" + fileName + ">"); 															
				}				

				string filePath = path + "\\" + fileName + ".txt";

									
				//---- Write message data to file -------------------------------//
				using(FileStream fStream = File.Create(filePath,(int)msgStream.Length))
					  {
					msgStream.WriteTo(fStream);
				}
				//---------------------------------------------------------------//						
				onGroup ++;
			}
			return "<" + Path.GetFileNameWithoutExtension(fileName) + ">";
		}


		public void StoreMessage(MemoryStream msgStream)
		{
			string[] newsgroups = HeaderParser.ParseHeaderFields("NEWSGROUPS:",msgStream.ToArray()).Replace("NEWSGROUPS:","").Trim().Split(Convert.ToChar(","));  
			string   from       = HeaderParser.ParseHeaderFields("FROM:",msgStream.ToArray()).Replace("FROM:","").Trim();  			
			string   id         = HeaderParser.ParseHeaderFields("MESSAGE-ID:",msgStream.ToArray()).ToUpper().Replace("MESSAGE-ID:","").ToLower().Trim();  
			int      onGroup    = 0;
			string   fileName   = "";

			id = id.Replace("<","").Replace(">","");
			id = RemInvalidChars(id);

			foreach(string group in newsgroups)
			{
				string newsgroup = group.ToUpper().Replace("\r\n","").Replace("NEWSGROUPS:","").Trim();
				newsgroup = newsgroup.ToLower().Replace(".","\\"); 
				string path = m_NNTPStorePath + "groups\\" + newsgroup;

				if(!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
					AddGroup(group.ToUpper().Replace("NEWSGROUPS:",""));
				}
			}

			foreach(string group in newsgroups)
			{
				string newsgroup = group.ToUpper().Replace("\r\n","").Replace("NEWSGROUPS:","").Trim();
				newsgroup = newsgroup.ToLower().Replace(".","\\"); 
				string path = m_NNTPStorePath + "groups\\" + newsgroup;
			
				fileName = GetNextUid(newsgroup) + "_" + id;

				string filePath = path + "\\" + fileName + ".txt";


								
				//---- Write message data to file -------------------------------//
				using(FileStream fStream = File.Create(filePath,(int)msgStream.Length))
				{
					msgStream.WriteTo(fStream);
				}
				//---------------------------------------------------------------//						
				onGroup ++;
			}
			//return "<" + Path.GetFileNameWithoutExtension(fileName) + ">";
			
		}


		#endregion

		#region RegEx IsNumeric
		public static bool IsNumeric(string inputData)
		{
			Match m = _isNumber.Match(inputData);
			return m.Success;
		}

		#endregion

		#region RemoveInvalidChars
		/// <summary>
		/// Removed illigal chars from a filename (/\<>*:|)
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public string RemInvalidChars(string fileName)
		{
			return fileName.Replace("/","").Replace("<","").Replace("\\","").Replace(">","").Replace(":","").Replace("*","").Replace("?","").Replace("|","");
		}
		#endregion

		#region NextUid
		/// <summary>
		/// Gets,stores and returns free UID.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="mailbox"></param>
		/// <returns></returns>
		private string GetNextUid(string newsGroup)
		{
			int uid = 1;

			if(!Directory.Exists(newsGroup))
			{
				Directory.CreateDirectory(newsGroup);
			}

					
			for(int i=0;i<50;i++)
			{
				try
				{
					if(!File.Exists(m_NNTPStorePath + "groups\\" + newsGroup + "\\_UID_holder"))
					{
						using(TextWriter wr = File.CreateText(m_NNTPStorePath + "groups\\" + newsGroup + "\\_UID_holder"))
						{
							wr.Write("0");
						}
					}
							
					using(FileStream fs = File.Open(m_NNTPStorePath + "groups\\" + newsGroup + "\\_UID_holder",FileMode.Open,FileAccess.ReadWrite,FileShare.None))
					{
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
				catch
				{ // Just try again
					System.Threading.Thread.Sleep(30);
				}
			}

			string strUid = uid.ToString("00000");

			return strUid;
		}

		#endregion

		#region Settings Related

		#region GetSettings
		public DataSet GetSettings()
		{
			DataSet ds = new DataSet();
			CreateSettingsSchema(ds);
			ds.ReadXml(m_DataPath + "Settings.xml");

			foreach(DataRow dr in ds.Tables["Settings"].Rows)
			{
				foreach(DataColumn dc in ds.Tables["Settings"].Columns)
				{
					if(dr.IsNull(dc.ColumnName))
					{
						dr[dc.ColumnName] = dc.DefaultValue;
					}
				}
			}

			return ds;

		}

		#endregion

		#region Get Pull Feeds
		public DataSet GetPullFeeds()
		{
			DataSet ds = new DataSet();
			CreatePullFeedsSchema(ds);
			ds.ReadXml(m_DataPath + "PullFeeds.xml");

			foreach(DataRow dr in ds.Tables["Feeds"].Rows)
			{
				foreach(DataColumn dc in ds.Tables["Feeds"].Columns)
				{
					if(dr.IsNull(dc.ColumnName))
					{
						dr[dc.ColumnName] = dc.DefaultValue;
					}
				}
			}

			return ds;

		}

		#endregion

		#region function UpdateSettings

		/// <summary>
		/// Updates nntp core settings (ports,database type, ...).
		/// </summary>
		/// <param name="dsSettings"></param>
		/// <returns></returns>
		public void UpdateSettings(DataSet dsSettings)
		{
			if(dsSettings != null && dsSettings.Tables.Contains("Settings"))
			{
				dsSettings.WriteXml(m_DataPath + "Settings.xml",XmlWriteMode.IgnoreSchema);
			}


		}

		#endregion

		#region function UpdateSettings

		public void UpdatePullFeeds(DataSet dsFeeds)
		{
			if(dsFeeds != null && dsFeeds.Tables.Contains("Feeds"))
			{
				dsFeeds.WriteXml(m_DataPath + "PullFeeds.xml",XmlWriteMode.IgnoreSchema);
			}
		}

		#endregion


		#endregion

		#region function CreateSettingsSchema

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ds"></param>
		public void CreateSettingsSchema(DataSet ds)
		{
			if(!ds.Tables.Contains("Settings"))
			{
				ds.Tables.Add("Settings");
			}

			if(!ds.Tables["Settings"].Columns.Contains("NNTPRoot"))
			{
				ds.Tables["Settings"].Columns.Add("NNTPRoot");
			}

			if(!ds.Tables["Settings"].Columns.Contains("ConnectionString"))
			{
				ds.Tables["Settings"].Columns.Add("ConnectionString");
			}

			if(!ds.Tables["Settings"].Columns.Contains("DataBaseType"))
			{
				ds.Tables["Settings"].Columns.Add("DataBaseType");
			}

			if(!ds.Tables["Settings"].Columns.Contains("ErrorFile"))
			{
				ds.Tables["Settings"].Columns.Add("ErrorFile");
			}

			if(!ds.Tables["Settings"].Columns.Contains("NNTP_IPAddress"))
			{
				ds.Tables["Settings"].Columns.Add("NNTP_IPAddress");
			}

			if(!ds.Tables["Settings"].Columns.Contains("NNTP_Port"))
			{
				ds.Tables["Settings"].Columns.Add("NNTP_Port");
			}

			if(!ds.Tables["Settings"].Columns.Contains("NNTP_Threads"))
			{
				ds.Tables["Settings"].Columns.Add("NNTP_Threads");
			}	

			if(!ds.Tables["Settings"].Columns.Contains("LogServer"))
			{
				ds.Tables["Settings"].Columns.Add("LogServer");
			}

			if(!ds.Tables["Settings"].Columns.Contains("LogNNTPCmds"))
			{
				ds.Tables["Settings"].Columns.Add("LogNNTPCmds");
			}

			if(!ds.Tables["Settings"].Columns.Contains("NNTP_SessionIdleTimeOut"))
			{
				ds.Tables["Settings"].Columns.Add("NNTP_SessionIdleTimeOut");
			}

			if(!ds.Tables["Settings"].Columns.Contains("NNTP_CommandIdleTimeOut"))
			{
				ds.Tables["Settings"].Columns.Add("NNTP_CommandIdleTimeOut");
			}

			if(!ds.Tables["Settings"].Columns.Contains("NNTP_MaxBadCommands"))
			{
				ds.Tables["Settings"].Columns.Add("NNTP_MaxBadCommands");
			}		

			if(!ds.Tables["Settings"].Columns.Contains("MaxMessageSize"))
			{
				ds.Tables["Settings"].Columns.Add("MaxMessageSize");
			}
			
			if(!ds.Tables["Settings"].Columns.Contains("MaxPullThreads"))
			{
				ds.Tables["Settings"].Columns.Add("MaxPullThreads");
			}

			if(!ds.Tables["Settings"].Columns.Contains("PullInterval"))
			{
				ds.Tables["Settings"].Columns.Add("PullInterval");
			}
			
			if(!ds.Tables["Settings"].Columns.Contains("NNTP_LogPath"))
			{
				ds.Tables["Settings"].Columns.Add("NNTP_LogPath");
			}	
		
			if(!ds.Tables["Settings"].Columns.Contains("NNTP_Enabled"))
			{
				ds.Tables["Settings"].Columns.Add("NNTP_Enabled");
			}	

		}


		#endregion

		#region function CreatePullFeedsSchema

		public void CreatePullFeedsSchema(DataSet ds)
		{
			if(!ds.Tables.Contains("Feeds"))
			{
				ds.Tables.Add("Feeds");
			}

			if(!ds.Tables["Feeds"].Columns.Contains("Server"))
			{
				ds.Tables["Feeds"].Columns.Add("Server");
			}

			if(!ds.Tables["Feeds"].Columns.Contains("Newsgroups"))
			{
				ds.Tables["Feeds"].Columns.Add("Newsgroups");
			}

			if(!ds.Tables["Feeds"].Columns.Contains("LastSync"))
			{
				ds.Tables["Feeds"].Columns.Add("LastSync");
			}

			if(!ds.Tables["Feeds"].Columns.Contains("Enabled"))
			{
				ds.Tables["Feeds"].Columns.Add("Enabled");
			}

		}
	

		#endregion
		
	}
}
