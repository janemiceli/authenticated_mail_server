using System;

namespace LumiSoft.Net.NNTP.Server{
	/// <summary>
	/// Summary description for NNTP_ListGroups_eArgs.
	/// </summary>
	public class NNTP_ListGroups_eArgs
	{
		private NNTP_Session    m_pSession  = null;
		private NNTP_NewsGroups m_pGroups   = null;

		public NNTP_ListGroups_eArgs(NNTP_Session session,NNTP_NewsGroups groups)
		{
			m_pSession = session;
			m_pGroups  = groups;			
		}
		
		/// <summary>
		/// Gets NNTP group list.
		/// </summary>
		public NNTP_NewsGroups Groups
		{
			get{ return m_pGroups; }
		}
	}
}
