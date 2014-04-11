using System;
using System.Collections;

namespace LumiSoft.Net.NNTP.Server
{
	/// <summary>
	/// NNTP articles collection.
	/// </summary>
	public class NNTP_Articles
	{
		private Hashtable m_pArticles = null;
		
		public NNTP_Articles()
		{		
			m_pArticles = new Hashtable();
		}

		/// <summary>
		/// Adds article to articles collection,ID as key.
		/// </summary>
		/// <param name="ID">Article ID.</param>
		public void Add(string id,int number,string subject,string author,string date,string refs,string lines,string bytes)
		{
			if(!m_pArticles.ContainsKey(id))
			{
				m_pArticles.Add(id,new NNTP_Article(number,id,subject,author,date,refs,lines,bytes));
			}
		}
		/// <summary>
		/// Adds article to articles collection,number as key.
		/// </summary>
		/// <param name="ID">Article ID.</param>
		public void Add(int number,string id,string subject,string author,string date,string refs,string lines,string bytes)
		{
			if(!m_pArticles.ContainsKey(id))
			{
				m_pArticles.Add(number,new NNTP_Article(number,id,subject,author,date,refs,lines,bytes));
			}
		}
		/// <summary>
		/// Adds article to articles collection.
		/// </summary>
		/// <param name="ID">Article ID.</param>
		public void Add(NNTP_Article article)
		{
			m_pArticles.Add(article.Number,article);
		}
		

		/// <summary>
		/// Gets article by article number.
		/// </summary>
		public NNTP_Article this[int number]
		{			
			get{ return (NNTP_Article)m_pArticles[number]; }
		}

		/// <summary>
		/// Gets article by article id.
		/// </summary>
		public NNTP_Article this[string id]
		{
			get
			{
				NNTP_Article retval = null;

				foreach(NNTP_Article article in this.Articles)
				{
					if(article.ID == id)
					{
						retval = article; 
						break;
					}
				}
				return retval;
			}
		}

		/// <summary>
		/// Gets first article number.
		/// </summary>
		internal int FirstArticleNo
		{
			get{
				if(m_pArticles.Count > 0){
					return 1; 
				}
				else{
					return 0;
				}
			}
		}

		/// <summary>
		/// Gets last article number.
		/// </summary>
		internal int LastArticleNo
		{
			get{ return m_pArticles.Count; }
		}
        
		/// <summary>
		/// Gets articles count.
		/// </summary>
		public int Count
		{
			get{ return m_pArticles.Count; }
		}

		/// <summary>
		/// Gets Article list.
		/// </summary>
		public NNTP_Article[] Articles
		{
			get
			{ 
				NNTP_Article[] articles = new NNTP_Article[m_pArticles.Count];
				m_pArticles.Values.CopyTo(articles,0);

				return articles; 
			}			
		}
	}
}
