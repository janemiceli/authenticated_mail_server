using System;
using System.Text;
using System.Collections;

namespace LumiSoft.Net.Dns
{
	/// <summary>
	/// This class holds Dns answers returned by server.
	/// </summary>
	internal class Dns_Answers
	{
		private Dns_Answer[] m_Answers = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Dns_Answers()
		{			
		}


		#region function ParseAnswers

		/// <summary>
		/// Parses answer.
		/// </summary>
		/// <param name="reply"></param>
		/// <param name="queryID"></param>
		/// <returns>Returns true if answer parsed successfully.</returns>
		internal bool ParseAnswers(byte[] reply,int queryID)
		{			
			try
			{
				/*
			     							   1  1  1  1  1  1
				 0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                                               |
				/                                               /
				/                      NAME                     /
				|                                               |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                      TYPE                     |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                     CLASS                     |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                      TTL                      |
				|                                               |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                   RDLENGTH                    |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--|
				/                     RDATA                     /
				/                                               /
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				*/

				//------- Parse result -----------------------------------//

				Dns_Header replyHeader = new Dns_Header();
				if(!replyHeader.ParseHeader(reply)){
					return false;
				}

				// Check that it's query what we want
				if(queryID != replyHeader.ID){
					return false;
				}
				
				int pos = 12;

				//----- Parse question part ------------//
				for(int q=0;q<replyHeader.QDCOUNT;q++){
					string dummy = "";
					GetQName(reply,ref pos,ref dummy);
					//qtype + qclass
					pos += 4;
				}
				//--------------------------------------//
				
				ArrayList answers = new ArrayList();

				//---- Start parsing aswers ------------------------------------------------------------------//
				for(int i=0;i<replyHeader.ANCOUNT;i++){
					string name = "";
					if(!GetQName(reply,ref pos,ref name)){
						return false;
					}

					int type     = reply[pos++] << 8 | reply[pos++];
					int rdClass  = reply[pos++] << 8 | reply[pos++];
					int ttl      = reply[pos++] << 24 | reply[pos++] << 16 | reply[pos++] << 8  | reply[pos++];
					int rdLength = reply[pos++] << 8 | reply[pos++];

					object answerObj = null;
					switch((QTYPE)type)
					{
						case QTYPE.MX:
							answerObj = ParseMxRecord(reply,ref pos);
							break;

						default:
							answerObj = "sgas"; // Dummy place holder for now
							pos += rdLength;
							break;
					}
			
					// Add answer to answer list
					if(answerObj != null){
						answers.Add(new Dns_Answer(name,(QTYPE)type,rdClass,ttl,rdLength,answerObj));
					}
					else{
						return false; // Parse failed
					}
				}
				//-------------------------------------------------------------------------------------------//

				if(answers.Count > 0){
					m_Answers = new Dns_Answer[answers.Count];
					answers.CopyTo(m_Answers);
				}

				return true;
			}
			catch{
				return false;
			}
		}

		#endregion
		
		#region function GetQName

		private bool GetQName(byte[] reply,ref int offset,ref string name)
		{	
			try
			{
				// Do while not terminator
				while(reply[offset] != 0){
					
					// Check if it's pointer(In pointer first two bits always 1)
					bool isPointer = ((reply[offset] & 0xC0) == 0xC0);
					
					// If pointer
					if(isPointer){
						int pStart = ((reply[offset] & 0x3F) << 8) | (reply[++offset]);
						offset++;						
						return GetQName(reply,ref pStart,ref name);
					}
					else{
						// label lenght (length = 8Bit and first 2 bits always 0)
						int labelLenght = (reply[offset] & 0x3F);
						offset++;
						
						// Copy label into name 
						name += Encoding.ASCII.GetString(reply,offset,labelLenght);
						offset += labelLenght;
					}
									
					// If the next char isn't terminator,
					// label continues - add dot between two labels
					if (reply[offset] != 0){
						name += ".";
					}
				}

				// Move offset by terminator lenght
				offset++;

				return true;
			}
			catch//(Exception x)
			{
		//		System.Windows.Forms.MessageBox.Show(x.Message);
				return false;
			}
		}

		#endregion


		#region function ParseMxRecord

		/// <summary>
		/// Parses MX record.
		/// </summary>
		/// <param name="reply"></param>
		/// <param name="offset"></param>
		/// <returns>Returns null, if failed.</returns>
		private object ParseMxRecord(byte[] reply,ref int offset)
		{
			/* RFC 1035	3.3.9. MX RDATA format

			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                  PREFERENCE                   |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			/                   EXCHANGE                    /
			/                                               /
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

			where:

			PREFERENCE      
				A 16 bit integer which specifies the preference given to
				this RR among others at the same owner.  Lower values
                are preferred.

			EXCHANGE 
			    A <domain-name> which specifies a host willing to act as
                a mail exchange for the owner name. 
			*/

			try
			{
				int pref = reply[offset++] << 8 | reply[offset++];
		
				string name = "";				
				if(GetQName(reply,ref offset,ref name)){
					return new MX_Record(pref,name);
				}
			}
			catch{
			}

			return null;
		}

		#endregion

		#region function GetMxRecordsFromAnswers

		/// <summary>
		/// Gets MX records from answer collection and ORDERS them by preference.
		/// NOTE: Duplicate preference records are appended to end. 
		/// </summary>
		/// <returns></returns>
		internal MX_Record[] GetMxRecordsFromAnswers()
		{
			MX_Record[] retVal = null;

			try
			{		
				SortedList mx            = new SortedList();
				ArrayList  duplicateList = new ArrayList();
				foreach(Dns_Answer answer in m_Answers){
					if(answer.QTYPE == QTYPE.MX){
						MX_Record mxRec = (MX_Record)answer.RecordObj;

						if(!mx.Contains(mxRec.Preference)){
							mx.Add(mxRec.Preference,mxRec);
						}
						else{
							duplicateList.Add(mxRec);
						}
					}
				}

				MX_Record[] mxBuff = new MX_Record[mx.Count + duplicateList.Count];
				mx.Values.CopyTo(mxBuff,0);
				duplicateList.CopyTo(mxBuff,mx.Count);
				retVal = mxBuff;
			}
			catch{				
			}

			return retVal;
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets answers.
		/// </summary>
		public Dns_Answer[] Answers
		{
			get{ return m_Answers; }
		}

		#endregion

	}
}
