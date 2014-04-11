using System;
using System.Net.Sockets;

namespace LumiSoft.Net.POP3.Server
{
	/// <summary>
	/// Provides data for the GetMailEvent event.
	/// </summary>
	public class GetMessage_EventArgs
	{
		private POP3_Session m_pSession    = null;
		private POP3_Message m_pMessage    = null;
		private Socket       m_pSocket     = null;
		private byte[]       m_MessageData = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="session">Reference to pop3 session.</param>
		/// <param name="message">Message which to get.</param>
		/// <param name="socket">Connected socket.</param>
		public GetMessage_EventArgs(POP3_Session session,POP3_Message message,Socket socket)
		{
			m_pSession = session;
			m_pMessage = message;
			m_pSocket  = socket;
		}

		#region properties Implementation

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
		/// ID of message which to retrieve.
		/// </summary>
		public string MessageID
		{
			get{ return m_pMessage.MessageID; }
		}

		/// <summary>
		/// Gets direct access to connected socket.
		/// This is meant for advanced users only.
		/// Just write message to this socket.
		/// NOTE: Message must be peiod handled and doesn't contain message terminator at end.
		/// </summary>
		public Socket ConnectedSocket
		{
			get{ return m_pSocket; }
		}

		/// <summary>
		/// Mail message which is delivered to user.
		/// </summary>
		public byte[] MessageData
		{
			get{ return m_MessageData; }

			set{ m_MessageData = value; }
		}

		/// <summary>
		/// User Name.
		/// </summary>
		public string UserName
		{
			get{ return m_pSession.UserName; }
		}

		#endregion

	}
}
