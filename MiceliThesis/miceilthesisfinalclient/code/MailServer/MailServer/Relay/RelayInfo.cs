using System;
using System.IO;
using LumiSoft.Net;

namespace LumiSoft.MailServer.Relay
{
	/// <summary>
	/// This class parses relay info from message.
	/// </summary>
	internal class RelayInfo
	{
		private string   m_To                 = "";
		private string   m_From               = "";
		private bool     m_IsWSent            = false;
		private DateTime m_MsgDate;
		private int      m_Undelivered        = 1;
		private int      m_UndeliveredWarning = 1;
		private int      m_MsgStartPos        = 0;
		private string   m_ForwardHost        = "";
		
		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="relayMsgStrm">Message stream from to read relay info.</param>
		/// <param name="undelivered">Specifies minutes when message is considered to be undelivered.</param>
		/// <param name="undeliveredWarning">Specifies minutes when delayed delivery warning is sent.</param>
		public RelayInfo(Stream relayMsgStrm,int undelivered,int undeliveredWarning)
		{
			ReadRelayInfo(relayMsgStrm);

			m_Undelivered = undelivered / 60;
			m_UndeliveredWarning = undeliveredWarning;
		}


		#region function ReadRelayInfo

		/// <summary>
		/// Parses relay info from stream.
		/// </summary>
		/// <param name="relayMsgStrm"></param>
		private void ReadRelayInfo(Stream relayMsgStrm)
		{
			StreamLineReader reader = new StreamLineReader(relayMsgStrm);
			string relayHead = System.Text.Encoding.ASCII.GetString(reader.ReadLine());
			if(relayHead != null && relayHead.StartsWith("RelayInfo:")){
				relayHead = relayHead.Replace("RelayInfo:","");

				string[] param = relayHead.Split(new char[]{'\t'});
				if(param.Length == 5){
					m_IsWSent     = Convert.ToBoolean(Convert.ToInt32(param[0]));
					m_To          = param[1];
					m_From        = param[2];
					m_MsgDate     = DateTime.ParseExact(param[3],"r",System.Globalization.DateTimeFormatInfo.InvariantInfo);
					m_ForwardHost = param[4];
				}

				m_MsgStartPos = (int)relayMsgStrm.Position;
			}
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets recipient.
		/// </summary>
		public string To
		{
			get{ return m_To; }
		}

		/// <summary>
		/// Gets sender.
		/// </summary>
		public string From
		{
			get{ return m_From; }
		}

		/// <summary>
		/// Gets message store date.
		/// </summary>
		public DateTime MessageDate
		{
			get{ return m_MsgDate; }
		}

		/// <summary>
		/// Gets if undelivered warning is sent.
		/// </summary>
		public bool IsUndeliveredWarningSent
		{
			get{ return m_IsWSent; }
		}

		/// <summary>
		/// Gets if undelivered date is exceeded.
		/// </summary>
		public bool IsUndeliveredDateExceeded
		{
			get{				
				if(DateTime.Now.CompareTo(m_MsgDate.AddHours(m_Undelivered)) >= 0){
					return true;
				}
				else{
					return false;
				}
			}
		}

		/// <summary>
		/// Gets if must send undelivered warning.
		/// </summary>
		public bool MustSendWarning
		{
			get{ 
				if(!m_IsWSent && DateTime.Now.CompareTo(m_MsgDate.AddMinutes(m_UndeliveredWarning)) >= 0){
					return true;
				}
				else{
					return false;
				}
			}
		}

		/// <summary>
		/// Gets undelivered bit position in stream.
		/// </summary>
		public int WarningBitPos
		{
			get{ return 10; }
		}

		/// <summary>
		/// Gets message start position in stream.
		/// </summary>
		public int MessageStartPos
		{
			get{ return m_MsgStartPos; }
		}

		/// <summary>
		/// Gets how many hours server will try to send message.
		/// </summary>
		public int DeviveringForHours
		{
			get{ return m_Undelivered-(m_UndeliveredWarning/60); }
		}

		/// <summary>
		/// Gets or sets host where message must be forwarded. This can be host name or IP address.
		/// </summary>
		public string ForwardHost
		{
			get{ return m_ForwardHost; }

			set{ m_ForwardHost = value; }
		}

		#endregion

	}
}
