using System;

namespace LumiSoft.Net.NNTP.Server
{
	/// <summary>
	/// Summary description for NNTP_Articles_eArgs.
	/// </summary>
	public class NNTP_Articles_eArgs
	{
		private NNTP_Articles m_pArticels = null;
		private string        m_Newsgroup = "";
		private bool          m_Exists    = true;

		public NNTP_Articles_eArgs(NNTP_Articles articles,string newsgroup)
		{		
			m_pArticels = articles;
			m_Newsgroup = newsgroup;
		}

		/// <summary>
		/// Gets NNTP articles collection.
		/// </summary>
		public NNTP_Articles Articles
		{
			get{ return m_pArticels; }
		}

		/// <summary>
		/// Gets newgroup which articles.
		/// </summary>
		public string Newsgroup
		{
			get{ return m_Newsgroup; }
		}

		/// <summary>
		/// Gets or sets if specified newsgroup exists(is valid).
		/// </summary>
		public bool NewsgroupExists
		{
			get{ return m_Exists; }

			set{ m_Exists = value; }
		}
	}
}
