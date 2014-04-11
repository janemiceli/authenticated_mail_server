using System;

namespace LumiSoft.Net.NNTP.Server
{
	/// <summary>
	/// NNTP article.
	/// </summary>
	public class NNTP_Article
	{
		private int    m_number;
		private string m_subject;
		private string m_author;
		private string m_msgid;
		private string m_references;
		private string m_lines;
		private string m_date;
		private string m_bytes;

		public NNTP_Article()
		{		
		
		}

		public NNTP_Article(int number,string id,string subject,string author,string date,string refs,string lines,string bytes)
		{		
			m_number  = number;
			m_subject = subject;
			m_author  = author;
			m_msgid   = id;
			m_lines   = lines;
			m_references = refs;
			m_date = date;
			m_bytes = bytes;
		}


		/// <summary>
		/// Gets article number.
		/// </summary>
		public int Number
		{
			get{ return m_number; }
		}
		/// <summary>
		/// Gets article ID
		/// </summary>
		public string ID
		{
			get{ return m_msgid; }
		}
		/// <summary>
		/// Gets article subject
		/// </summary>
		public string Subject
		{
			get{ return m_subject; }
		}
		/// <summary>
		/// Gets article Author
		/// </summary>
		public string Author
		{
			get{ return m_author; }
		}
		/// <summary>
		/// Gets article date
		/// </summary>
		public string Date
		{
			get{ return m_date; }
		}
		/// <summary>
		/// Gets article References
		/// </summary>
		public string References
		{
			get{ return m_references; }
		}
		/// <summary>
		/// Gets article Lines
		/// </summary>
		public string Lines
		{
			get{ return m_lines; }
		}
		/// <summary>
		/// Gets article ByteCount
		/// </summary>
		public string ByteCount
		{
			get{ return m_bytes; }
		}
	}
}
