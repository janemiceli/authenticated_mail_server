using System;
using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net.POP3.Client
{
	/// <summary>
	/// Holds POP3 messages info.
	/// </summary>
	public class POP3_MessagesInfo : IEnumerable
	{
		private List<POP3_MessageInfo> m_pMessages = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public POP3_MessagesInfo()
		{	
			m_pMessages = new List<POP3_MessageInfo>();
		}


		#region method Add

		/// <summary>
		/// Add message to pop3 messages collection.
		/// </summary>
		/// <param name="messageID">Message unique ID.</param>
		/// <param name="messageNo">Message number.</param>
		/// <param name="messageSize">Message size in bytes.</param>
		internal void Add(string messageID,int messageNo,long messageSize)
		{
			m_pMessages.Add(new POP3_MessageInfo(messageID,messageNo,messageSize));
		}

		#endregion


		#region method Contains

		/// <summary>
		/// Gets if collection contains message with specified number.
		/// </summary>
		/// <param name="messageNo">Message number.</param>
		/// <returns></returns>
		public bool Contains(int messageNo)
		{
			foreach(POP3_MessageInfo msgInfo in this){
				if(msgInfo.MessageNumber == messageNo){
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets if collection contains message with specified UID.
		/// </summary>
		/// <param name="messageUID">Message unique ID.</param>
		/// <returns></returns>
		public bool Contains(string messageUID)
		{
			foreach(POP3_MessageInfo msgInfo in this){
				if(msgInfo.MessageUID == messageUID){
					return true;
				}
			}

			return false;
		}

		#endregion


		#region method GetByNo

		/// <summary>
		/// Gets message by MessageNumber. Returns null if sepcified message info doesn't exist.
		/// </summary>
		/// <param name="messageNo">Message number.</param>
		/// <returns></returns>
		public POP3_MessageInfo GetByNo(int messageNo)
		{
			foreach(POP3_MessageInfo msgInfo in this){
				if(msgInfo.MessageNumber == messageNo){
					return msgInfo;
				}
			}

			return null;
		}

		#endregion

		#region method GetByUID

		/// <summary>
		/// Gets message by MessageUID. Returns null if sepcified message info doesn't exist.
		/// </summary>
		/// <param name="messageUID">Message UID.</param>
		/// <returns></returns>
		public POP3_MessageInfo GetByUID(string messageUID)
		{
			foreach(POP3_MessageInfo msgInfo in this){
				if(msgInfo.MessageUID == messageUID){
					return msgInfo;
				}
			}

			return null;
		}

		#endregion



		#region interface IEnumerator

		/// <summary>
		/// Gets enumerator.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return m_pMessages.GetEnumerator();
		}

		#endregion

		#region Properties Implementation

		/// <summary>
		/// Gets specified pop3 message info at specified index from collection.
		/// </summary>
		public POP3_MessageInfo this[int index]
		{
			get{
				if(index > -1 && index < m_pMessages.Count){
					return m_pMessages[index];
				}
				else{
					throw new Exception("No such message !");
				}
			}
		}

		/// <summary>
		/// Gets total size of messages in bytes.
		/// </summary>
		public long TotalSize
		{
			get{ 
				long sizeTotal = 0;
				foreach(POP3_MessageInfo msg in this.Messages){
					sizeTotal += msg.MessageSize;
				}
				return sizeTotal; 
			}
		}

		/// <summary>
		/// Gets messages count.
		/// </summary>
		public int Count
		{
			get{ return m_pMessages.Count; }
		}

		/// <summary>
		/// Gets pop3 messages info as array.
		/// </summary>
		public POP3_MessageInfo[] Messages
		{
			get{ return m_pMessages.ToArray(); }
		}

		#endregion

	}
}
