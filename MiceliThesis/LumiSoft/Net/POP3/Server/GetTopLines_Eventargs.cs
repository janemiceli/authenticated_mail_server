using System;

namespace LumiSoft.Net.POP3.Server
{
	/// <summary>
	/// Provides data for the GetTopLines event.
	/// </summary>
	public class GetTopLines_Eventargs
	{
		private POP3_Session m_pSession  = null;
		private POP3_Message m_pMessage  = null;
		private int          m_Lines     = 0;
		private byte[]       m_LinesData = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="session">Reference to pop3 session.</param>
		/// <param name="message">Message wich top line to get.</param>
		/// <param name="nLines">Number of lines to get.</param>
		public GetTopLines_Eventargs(POP3_Session session,POP3_Message message,int nLines)
		{
			m_pSession = session;
			m_pMessage = message;
			m_Lines    = nLines;
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
		/// Mailbox name.
		/// </summary>
		public string Mailbox
		{
			get{ return m_pSession.UserName; }
		}

		/// <summary>
		/// Message ID of message which TOP lines to get.
		/// </summary>
		public string MessageID
		{
			get{ return m_pMessage.MessageID; }
		}

		/// <summary>
		/// Number of lines to get.
		/// </summary>
		public int Lines
		{
			get{ return m_Lines; }
		}

		/// <summary>
		/// Gets or sets TOP lines.
		/// </summary>
		public byte[] LinesData
		{
			get{ return m_LinesData; }

			set{ m_LinesData = value; }
		}

		#endregion

	}
}
