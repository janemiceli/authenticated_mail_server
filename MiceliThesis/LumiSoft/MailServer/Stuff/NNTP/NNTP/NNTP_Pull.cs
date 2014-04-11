using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using LumiSoft.Net;
using LumiSoft.Net.NNTP;


namespace LumiSoft.Net.NNTP.Server
{
	/// <summary>
	/// Summary description for NNTP_Pull.
	/// </summary>
	public class NNTP_Pull
	{

		private Socket           m_Socket             = null;
		private NNTPServer       m_pNNTP_Server       = null;    // Referance to NNTP server.
		private int       m_PullInterval       = 0;
		private int       m_PullRetryInterval  = 0;
		private string    m_Server             = "";
		private string    m_Newsgroups         = "";
		private bool      m_Pulling            = false;
		private bool      m_RetryPulling       = false;
		private DateTime  m_PullFrom;
		private Hashtable m_PullTable;	
	
		public NNTP_Pull()
		{
			//
			// TODO: Add constructor logic here
			//
		}


		internal NNTP_Pull(NNTPServer server)
		{
			m_pNNTP_Server = server;
			m_Socket       = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
		}

		#region function Pull

		/// <summary>
		/// Start pulling news
		/// </summary>
		public void Pull()
		{
			if(!m_Pulling)
			{
				m_Pulling = true;

				PullNews();

				m_Pulling = false;
			}
		}

		#endregion

		#region PullNews

		private void PullNews()
		{
			try
			{
				Hashtable newids = new Hashtable();
			
				NNTP_Client client = new NNTP_Client();
				client.Connect(m_Server,119);
				string dateFormat = "yyMMdd HHmmss";

				//m_PullFrom   = DateTime.Now.AddHours(-5);
				//m_Newsgroups = "microsoft.dotnet*";

				newids = client.NewNews(m_Newsgroups + " " + m_PullFrom.ToString(dateFormat,System.Globalization.DateTimeFormatInfo.InvariantInfo));
				foreach(string s in newids.Keys)
				{
					if(m_pNNTP_Server.ServerAPI.GetArticle("",s,"") == "")
					{  
						byte[] article = client.Article(s); 
						if(article != null)
						{
							MemoryStream ms = new MemoryStream(article);
							m_pNNTP_Server.ServerAPI.StoreMessage(ms);	
						}
					}
				}

				client.Disconnect();
			}
			catch(Exception x)
			{
				

			}
			
		}

		#endregion



		#region function AddThread

		private void AddThread(Thread tr,string data)
		{
			lock(m_PullTable)
			{				
				m_PullTable.Add(tr,data);				
			}
		}

		#endregion

		#region function RemoveThread

		private void RemoveThread(Thread t)
		{
			lock(m_PullTable)
			{				
				if(!m_PullTable.ContainsKey(t))
				{
					//Core.WriteLog(m_pServer.m_SartUpPath + "nntpServiceError.log","RemoveThread: doesn't contain");
				}
				m_PullTable.Remove(t);				
			}
		}

		#endregion

		#region function IsReplyCode

		/// <summary>
		/// Checks if reply code.
		/// </summary>
		/// <param name="replyCode">Replay code to check.</param>
		/// <param name="reply">Full repaly.</param>
		/// <returns>Retruns true if reply is as specified.</returns>
		private bool IsReplyCode(string replyCode,string reply)
		{
			if(reply.IndexOf(replyCode) > -1)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region Properties Implementation

		/// <summary>
		/// Gets or sets pull interval.
		/// </summary>
		public int PullInterval
		{
			get{ return m_PullInterval; }

			set{ m_PullInterval = value; }
		}
		/// <summary>
		/// Gets or sets pull retry interval.
		/// </summary>
		public int PullRetryInterval
		{
			get{ return m_PullRetryInterval; }

			set{ m_PullRetryInterval = value; }
		}
		/// <summary>
		/// Gets or sets server to pull from.
		/// </summary>
		public string PullServer
		{
			get{ return m_Server; }

			set{ m_Server = value; }
		}
		/// <summary>
		/// Gets or sets newsgroups to pull.
		/// </summary>
		public string Newsgroups
		{
			get{ return m_Newsgroups; }

			set{ m_Newsgroups = value; }
		}
		/// <summary>
		/// Gets or sets datetime to pull from.
		/// </summary>
		public DateTime PullFrom
		{
			get{ return m_PullFrom; }

			set{ m_PullFrom = value; }
		}
		/// <summary>
		/// Gets if Pulling messages.
		/// </summary>
		public bool IsPulling
		{
			get{ return m_Pulling; }
		}

		/// <summary>
		/// Gets if retry Pulling messages.
		/// </summary>
		public bool IsRetryPulling
		{
			get{ return m_RetryPulling; }
		}
		#endregion

	}
}
