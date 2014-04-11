using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading; 
using LumiSoft.Net.NNTP;
using LumiSoft.Net.NNTP.Server;

namespace LumiSoft.Net.NNTP.Server
{	
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class NNTPServer : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button m_pStart;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private NNTP_Server m_pServer = null;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button m_pStop;
		
		private static NNTPServer m_sForm  = null;
		private System.Windows.Forms.Button btnPull;

		public   static   NNTP_API   m_API = null;
		private  DateTime m_PullTime;
		private  DataSet  dsSettings       = null;
		internal string   m_connStr        = "";    // Sql connection string to nntp DB.
		internal string   m_NNTPStorePath  = "";
		internal string   m_StartUpPath    = "";
		internal string   m_SettingsPath   = "";
		private System.Timers.Timer timer1;
		private DB_Type   m_DB_Type        = DB_Type.XML;
		private int       m_PullInterval   = 0;
		



		public NNTPServer()
		{
			//
			// Required for Windows Form Designer support
			//
			m_pServer = new NNTP_Server();

			
			InitializeComponent();

			m_pServer.SysError += new LumiSoft.Net.ErrorEventHandler(m_pServer_SysError);
			m_pServer.ListGroups +=new ListGroupsHandler(m_pServer_ListGroups);
			m_pServer.XoverInfo +=new XoverInfoHandler(m_pServer_XoverInfo);
			m_pServer.GroupInfo +=new GroupInfoHandler(m_pServer_GroupInfo);
			m_pServer.GetArticle +=new GetArticleHandler(m_pServer_GetArticle);
			m_pServer.StoreMessage +=new StoreMessageHandler(m_pServer_StoreMessage);
			m_pServer.NewNews +=new NewNewsHandler(m_pServer_NewNews);


			m_pServer.LogCommands = true;
			//
			// TODO: Add any constructor code after InitializeComponent call
			//


			m_sForm = this;
		}

		#region method Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.m_pStart = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.m_pStop = new System.Windows.Forms.Button();
			this.btnPull = new System.Windows.Forms.Button();
			this.timer1 = new System.Timers.Timer();
			((System.ComponentModel.ISupportInitialize)(this.timer1)).BeginInit();
			this.SuspendLayout();
			// 
			// m_pStart
			// 
			this.m_pStart.Location = new System.Drawing.Point(416, 8);
			this.m_pStart.Name = "m_pStart";
			this.m_pStart.TabIndex = 0;
			this.m_pStart.Text = "Start";
			this.m_pStart.Click += new System.EventHandler(this.m_pStart_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(8, 88);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox1.Size = new System.Drawing.Size(560, 328);
			this.textBox1.TabIndex = 1;
			this.textBox1.Text = "";
			// 
			// m_pStop
			// 
			this.m_pStop.Location = new System.Drawing.Point(496, 8);
			this.m_pStop.Name = "m_pStop";
			this.m_pStop.Size = new System.Drawing.Size(72, 24);
			this.m_pStop.TabIndex = 2;
			this.m_pStop.Text = "Stop";
			this.m_pStop.Click += new System.EventHandler(this.m_pStop_Click);
			// 
			// btnPull
			// 
			this.btnPull.Location = new System.Drawing.Point(8, 56);
			this.btnPull.Name = "btnPull";
			this.btnPull.TabIndex = 3;
			this.btnPull.Text = "pull";
			this.btnPull.Click += new System.EventHandler(this.btnPull_Click);
			// 
			// timer1
			// 
			this.timer1.Interval = 10000;
			this.timer1.SynchronizingObject = this;
			this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Elapsed);
			// 
			// NNTPServer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(576, 429);
			this.Controls.Add(this.btnPull);
			this.Controls.Add(this.m_pStop);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.m_pStart);
			this.Name = "NNTPServer";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.timer1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new NNTPServer());
		}

		#region IMAP related


		#region function m_pServer_SysError
        
		public void m_pServer_SysError(object sender, LumiSoft.Net.Error_EventArgs e)
		{
			MessageBox.Show(e.Exception.Message + "\n\n" + e.Exception.StackTrace);
		}

		#endregion

		#endregion


		#region Forms events

		private void m_pStart_Click(object sender, System.EventArgs e)
		{
			string filePath     = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
			m_StartUpPath       = filePath.Substring(0,filePath.LastIndexOf('\\')) + "\\";
			m_SettingsPath      = m_StartUpPath + "Settings" + "\\";

			m_API = new NNTP_API(m_SettingsPath);
    
			LoadSettings();
		}

		private void m_pStop_Click(object sender, System.EventArgs e)
		{
			m_pServer.Enabled = false;
		}	

		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			m_pServer.Enabled = false;
		}

		#endregion

		private void m_pServer_ListGroups(object sender,LumiSoft.Net.NNTP.Server.NNTP_ListGroups_eArgs e)
		{
			m_API.GetGroups(e.Groups);    
		}

		private NNTP_NewsGroup m_pServer_GroupInfo(object sender, LumiSoft.Net.NNTP.Server.NNTP_NewsGroup grp)
		{
			int count = m_API.GetArticleCount(grp.Name);
			int first = m_API.GetMinArticle(grp.Name);
			int last = m_API.GetMaxArticle(grp.Name);
			if(count > -1)
			{								
				grp.ArticlesCount  = count;
				grp.FirstArticleNo = first;
				grp.LastArticleNo  = last;
			}
			else
			{
				grp = null;				
			}
			return grp;
			
		}

		private void m_pServer_XoverInfo(object sender,LumiSoft.Net.NNTP.Server.NNTP_Articles_eArgs e)
		{
			m_API.GetArticles(e.Newsgroup,e.Articles);			
		}

		#region function AppendDirs

		private void AppendDirs(string path,ArrayList dirsAr,string remPath)
		{
			string[] dirs = Directory.GetDirectories(path);
			foreach(string dir in dirs)
			{
				dirsAr.Add(dir.Substring(remPath.Length).Replace("\\","/"));
				AppendDirs(dir.Replace("\\","/"),dirsAr,remPath);
			}
		}

		#endregion


		#region Debug

		public void AddDebug(string line)
		{

			if(textBox1.Lines.Length > 200)
			{
				textBox1.Text = "";
			}
			textBox1.Lines = ((string)(textBox1.Text + line + "\n")).Split('\n');
		}

		public static void AddDebugS(string line)
		{
			m_sForm.AddDebug(line);
		}	
	
		#endregion

		private void Form1_Load(object sender, System.EventArgs e)
		{
		
		}

		private string m_pServer_GetArticle(NNTP_Session ses, string id, string retVal)
		{
			return retVal = m_API.GetArticle(ses.SelectedGroup,id,retVal);			
		}

		private string m_pServer_StoreMessage(NNTP_Session ses, MemoryStream msgStream, string[] newsgroups)
		{
			return m_API.StoreMessage(msgStream,newsgroups);
		}

		private void m_pServer_NewNews(object sender, NNTP_Articles_eArgs e, string newsgroups, DateTime since)
		{
			m_API.GetNewNews(newsgroups,since,e.Articles);			
		}

		private void btnPull_Click(object sender, System.EventArgs e)
		{
			if(DateTime.Now.CompareTo(m_PullTime.AddSeconds(m_PullInterval)) >= 0)
			{

				DataSet dsFeeds = m_API.GetPullFeeds();
				foreach(DataRow dr in dsFeeds.Tables["Feeds"].Rows)
				{
					NNTP_Pull pull = new NNTP_Pull(this); 					
					pull.Newsgroups = dr["Newsgroups"].ToString();
					pull.PullServer = dr["Server"].ToString();
					pull.PullFrom   = Convert.ToDateTime(dr["LastSync"].ToString());
					
					pull.PullFrom.AddHours(-2); 
					
					dr["LastSync"]  = DateTime.Now;							 

					// Create New Thread for news pull handling
					ThreadStart tStart = new ThreadStart(pull.Pull);
					Thread      tr     = new Thread(tStart);
					tr.Priority        = ThreadPriority.Lowest;
					tr.Start();

					m_PullTime = DateTime.Now;
				}

				dsFeeds.Tables["Feeds"].AcceptChanges();
				m_API.UpdatePullFeeds(dsFeeds);

			}			
	
				
		}
	 


		private void LoadSettings()
		{

			try
			{
				lock(this)
				{
					dsSettings = m_API.GetSettings();
					
					DataRow dr = dsSettings.Tables["Settings"].Rows[0];
					m_connStr             = dr["ConnectionString"].ToString();
					m_DB_Type             = (DB_Type)Enum.Parse(typeof(DB_Type),dr["DataBaseType"].ToString());
					string nntpStorePath  = dr["NNTPRoot"].ToString();
					if(!nntpStorePath.EndsWith("\\"))
					{
						nntpStorePath += "\\";
					}
					if(nntpStorePath.Length < 3)
					{
						nntpStorePath = m_StartUpPath + "NNTPStore\\";
					}
					m_NNTPStorePath = nntpStorePath;

					m_PullInterval               = Convert.ToInt32(dr["PullInterval"]);

					//------- NNTP Settings ---------------------------------------------//					
					m_pServer.IpAddress          = dr["NNTP_IPAddress"].ToString();
					m_pServer.Port               = Convert.ToInt32(dr["NNTP_Port"]);
					m_pServer.Threads            = Convert.ToInt32(dr["NNTP_Threads"]);
					m_pServer.SessionIdleTimeOut = Convert.ToInt32(dr["NNTP_SessionIdleTimeOut"]) * 1000; // Seconds to milliseconds
					m_pServer.CommandIdleTimeOut = Convert.ToInt32(dr["NNTP_CommandIdleTimeOut"]) * 1000; // Seconds to milliseconds
					m_pServer.MaxMessageSize     = Convert.ToInt32(dr["MaxMessageSize"]) * 1000000;       // Mb to byte.
					m_pServer.MaxBadCommands     = Convert.ToInt32(dr["NNTP_MaxBadCommands"]);
					m_pServer.Enabled            = Convert.ToBoolean(dr["NNTP_Enabled"]);



				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}


		}

		private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(DateTime.Now.CompareTo(m_PullTime.AddSeconds(m_PullInterval)) >= 0)
			{

				DataSet dsFeeds = m_API.GetPullFeeds();
				foreach(DataRow dr in dsFeeds.Tables["Feeds"].Rows)
				{
					NNTP_Pull pull = new NNTP_Pull(this); 					
					pull.Newsgroups = dr["Newsgroups"].ToString();
					pull.PullServer = dr["Server"].ToString();
					pull.PullFrom   = Convert.ToDateTime(dr["LastSync"].ToString());

					pull.PullFrom.AddHours(-2); 
					
					dr["LastSync"]  = DateTime.Now;							 

					// Create New Thread for news pull handling
					ThreadStart tStart = new ThreadStart(pull.Pull);
					Thread      tr     = new Thread(tStart);
					tr.Priority        = ThreadPriority.Lowest;
					tr.Start();

					m_PullTime = DateTime.Now;
				}

				dsFeeds.Tables["Feeds"].AcceptChanges();
				m_API.UpdatePullFeeds(dsFeeds);
		
			}
		}



		/// <summary>
		/// Gets api.
		/// </summary>
		public NNTP_API ServerAPI
		{
			get{ return m_API; }			
		}
	}
}
