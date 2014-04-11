using System;

namespace LumiSoft.Net.NNTP.Server
{
	/// <summary>
	/// NNTP newsgroup.
	/// </summary>
	public class NNTP_NewsGroup
	{
		private string   m_Name          = "";
		private int      m_ArticlesCount = 0;
		private int      m_FirstArticle = 0;
		private int      m_LastArticle = 0;
		private DateTime m_Date;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="newsGroupName">Newsgroup name.</param>
		/// <param name="articlesCount">Count of articles in newsgroup.</param>
		/// <param name="creDate">Newsgroup creation time.</param>
		public NNTP_NewsGroup(string newsgroupName,int articlesCount,int firstArticle,int lastArticle,DateTime creDate)
		{			
			m_Name          = newsgroupName;
			m_ArticlesCount = articlesCount;
			m_FirstArticle  = firstArticle;
			m_LastArticle   = lastArticle;
			m_Date          = creDate;
		}


		#region Properties Implementation

		/// <summary>
		/// Gets newsgroup name.
		/// </summary>
		public string Name
		{
			get{ return m_Name; }
		}

		/// <summary>
		/// Gets articles count.
		/// </summary>
		public int ArticlesCount
		{
			get{ return m_ArticlesCount; }
			set{ m_ArticlesCount = value;}
		}

		/// <summary>
		/// Gets newsgroup creation date.
		/// </summary>
		public DateTime CreateDate
		{
			get{ return m_Date; }
		}

		/// <summary>
		/// Gets first article number.
		/// </summary>
		internal int FirstArticleNo
		{
			get{ return m_FirstArticle; }	
			set{ m_FirstArticle = value;}

		}

		/// <summary>
		/// Gets last article number.
		/// </summary>
		internal int LastArticleNo
		{
			get{ return m_LastArticle; }
			set{ m_LastArticle = value;}

		}

		#endregion

	}
}
