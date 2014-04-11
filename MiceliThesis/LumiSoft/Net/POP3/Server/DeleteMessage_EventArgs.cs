using System;

namespace LumiSoft.Net.POP3.Server
{
	/// <summary>
	/// Provides data for the DeleteMessageEvent event.
	/// </summary>
	public class DeleteMessage_EventArgs
	{
		private POP3_Session m_pSession = null;
		private POP3_Message m_pMessage = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="session">Reference to pop3 session.</param>
		/// <param name="message">Message which to delete.</param>
		public DeleteMessage_EventArgs(POP3_Session session,POP3_Message message)
		{
			m_pSession = session;
			m_pMessage = message;
		}

		#region Properties Implementation

		/// <summary>
		/// Gets reference to pop3 session.
		/// </summary>
		public POP3_Session Session
		{
			get{ return m_pSession; }
		}

		/// <summary>
		/// Gets reference to message, which to get.
		/// </summary>
		public POP3_Message Message
		{
			get{ return m_pMessage; }
		}

		/// <summary>
		/// ID of message which to delete.
		/// </summary>
		public string MessageID
		{
			get{ return m_pMessage.MessageID;}
		}

		/// <summary>
		/// User Name.
		/// </summary>
		public string UserName
		{
			get{ return m_pSession.UserName; }
		}

		/// <summary>
		/// Mailbox name.
		/// </summary>
		public string Mailbox
		{
			get{ return m_pSession.UserName; }
		}

		#endregion
	}
}
