using System;
using System.Collections;

namespace LumiSoft.Net.NNTP.Server
{
	/// <summary>
	/// NNTP newsgroups collection.
	/// </summary>
	public class NNTP_NewsGroups
	{
		private Hashtable m_pNewsgroups = null;

		/// <summary>
		/// Default constructor
		/// </summary>
		public NNTP_NewsGroups()
		{			
			m_pNewsgroups = new Hashtable();
		}


		/// <summary>
		/// Gets newsgroups which are created after specified date.
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="newsgroups"></param>
		/// <returns></returns>
		public NNTP_NewsGroup[] GetFilteredNewsgroups(DateTime startDate,string newsgroups)
		{
			ArrayList retVal = new ArrayList();
			foreach(NNTP_NewsGroup group in this.Newsgroups){
				if(newsgroups.Length == 0){
					if(group.CreateDate.CompareTo(startDate) > 0){
						retVal.Add(group);
					}
				}
				else{
					throw new Exception("To do: NNTP_NewsGroups.GetFilteredNewsgroups");
				}
			}

			NNTP_NewsGroup[] retGroups = new NNTP_NewsGroup[retVal.Count];
			retVal.CopyTo(retGroups);
			
			return retGroups;
		}


		/// <summary>
		/// Adds new newsgroup to newsgroups list.
		/// </summary>
		/// <param name="newsgroupName">Newsgroup name.</param>
		/// <param name="articlesCount">Count of articles in newsgroup.</param>
		/// <param name="creDate">Newsgroup creation time.</param>
		public void Add(string newsgroupName,int articlesCount,int firstArticle,int lastArticle,DateTime creDate)
		{
			m_pNewsgroups.Add(newsgroupName,new NNTP_NewsGroup(newsgroupName,articlesCount,firstArticle,lastArticle,creDate));			

		}
		/// <summary>
		/// Removes a newsgroup from the list
		/// </summary>
		/// <param name="newsgroupName">Newsgroup name.</param>
		public void Remove(string newsgroupName)
		{
			m_pNewsgroups.Remove(newsgroupName);
		}

		/// <summary>
		/// Determines whether contains this newsgroup.
		/// </summary>
		/// <param name="newsgroupName"></param>
		/// <returns></returns>
		public bool Contains(string newsgroupName)
		{
			return m_pNewsgroups.ContainsKey(newsgroupName);
		}


		/// <summary>
		/// Gets specified newsgroup.
		/// </summary>
		public NNTP_NewsGroup this[string newsgroupName]
		{
			get{ return (NNTP_NewsGroup)m_pNewsgroups[newsgroupName]; }
		}

		/// <summary>
		/// Gets newsgroup list.
		/// </summary>
		public NNTP_NewsGroup[] Newsgroups
		{
			get{ 
				NNTP_NewsGroup[] groups = new NNTP_NewsGroup[m_pNewsgroups.Count];
				m_pNewsgroups.Values.CopyTo(groups,0);

				return groups; 
			}
		}
	}
}
