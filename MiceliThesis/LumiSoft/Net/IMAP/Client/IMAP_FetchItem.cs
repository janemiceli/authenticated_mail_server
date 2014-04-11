using System;

namespace LumiSoft.Net.IMAP.Client
{
	/// <summary>
	/// IMAP fetch item.
	/// </summary>
	public class IMAP_FetchItem
	{
		private int    m_UID         = 0;
		private int    m_Size        = 0;		
		private byte[] m_Data        = null;
		private bool   m_HeadersOnly = true;
		private bool   m_NewMessage  = false;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="uid">Message UID.</param>
		/// <param name="size">Message size.</param>
		/// <param name="data">Message data.</param>
		/// <param name="headersOnly">Specifies if message data contains headers only or full message.</param>
		/// <param name="isNewMessage">Specifies if unseen message.</param>
		internal IMAP_FetchItem(int uid,int size,byte[] data,bool headersOnly,bool isNewMessage)
		{	
			m_UID         = uid;
			m_Size        = size;
			m_Data        = data;
			m_HeadersOnly = headersOnly;
			m_NewMessage  = isNewMessage;
		}


		#region Properties Implementation

		/// <summary>
		/// Gets message UID.
		/// </summary>
		public int UID
		{
			get{ return m_UID; }
		}

		/// <summary>
		/// Gets message size.
		/// </summary>
		public int Size
		{
			get{ return m_Size; }
		}

		/// <summary>
		/// Gets message data(headers or full message), it depends on HeadersOnly property.
		/// </summary>
		public byte[] Data
		{
			get{ return m_Data; }
		}

		/// <summary>
		/// Gets if headers or full message requested in fetch.
		/// </summary>
		public bool HeadersOnly
		{
			get{ return m_HeadersOnly; }
		}

		/// <summary>
		/// Gets if message is unseen.
		/// </summary>
		public bool IsNewMessage
		{
			get{ return m_NewMessage; }
		}

		#endregion

	}
}
