using System;
using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net.Dns.Client
{
	/// <summary>
	/// This class represents dns server response.
	/// </summary>
	[Serializable]
	public class DnsServerResponse
	{
		private bool                m_Success             = true;
		private RCODE               m_RCODE               = RCODE.NO_ERROR;
		private List<DnsRecordBase> m_pAnswers            = null;
		private List<DnsRecordBase> m_pAuthoritiveAnswers = null;
		private List<DnsRecordBase> m_pAdditionalAnswers  = null;
		
		internal DnsServerResponse(bool connectionOk,RCODE rcode,List<DnsRecordBase> answers,List<DnsRecordBase> authoritiveAnswers,List<DnsRecordBase> additionalAnswers)
		{
			m_Success             = connectionOk;
			m_RCODE               = rcode;	
			m_pAnswers            = answers;
			m_pAuthoritiveAnswers = authoritiveAnswers;
			m_pAdditionalAnswers  = additionalAnswers;
		}


		#region method GetARecords

		/// <summary>
		/// Gets IPv4 host addess records.
		/// </summary>
		/// <returns></returns>
		public A_Record[] GetARecords()
		{
            List<A_Record> retVal = new List<A_Record>();
            foreach(DnsRecordBase record in m_pAnswers){
                if(record.RecordType == QTYPE.A){
                    retVal.Add((A_Record)record);
                }
            }

			return retVal.ToArray();
		}

		#endregion

		#region method GetNSRecords

		/// <summary>
		/// Gets name server records.
		/// </summary>
		/// <returns></returns>
		public NS_Record[] GetNSRecords()
		{
            List<NS_Record> retVal = new List<NS_Record>();
            foreach(DnsRecordBase record in m_pAnswers){
                if(record.RecordType == QTYPE.NS){
                    retVal.Add((NS_Record)record);
                }
            }

			return retVal.ToArray();
		}

		#endregion

		#region method GetCNAMERecords

		/// <summary>
		/// Gets CNAME records.
		/// </summary>
		/// <returns></returns>
		public CNAME_Record[] GetCNAMERecords()
		{
            List<CNAME_Record> retVal = new List<CNAME_Record>();
            foreach(DnsRecordBase record in m_pAnswers){
                if(record.RecordType == QTYPE.CNAME){
                    retVal.Add((CNAME_Record)record);
                }
            }

			return retVal.ToArray();
		}

		#endregion

		#region method GetSOARecords

		/// <summary>
		/// Gets SOA records.
		/// </summary>
		/// <returns></returns>
		public SOA_Record[] GetSOARecords()
		{
            List<SOA_Record> retVal = new List<SOA_Record>();
            foreach(DnsRecordBase record in m_pAnswers){
                if(record.RecordType == QTYPE.SOA){
                    retVal.Add((SOA_Record)record);
                }
            }

			return retVal.ToArray();
		}

		#endregion

		#region method GetPTRRecords

		/// <summary>
		/// Gets PTR records.
		/// </summary>
		/// <returns></returns>
		public PTR_Record[] GetPTRRecords()
		{	
            List<PTR_Record> retVal = new List<PTR_Record>();
            foreach(DnsRecordBase record in m_pAnswers){
                if(record.RecordType == QTYPE.PTR){
                    retVal.Add((PTR_Record)record);
                }
            }

            return retVal.ToArray();
		}

		#endregion

		#region method GetHINFORecords

		/// <summary>
		/// Gets HINFO records.
		/// </summary>
		/// <returns></returns>
		public HINFO_Record[] GetHINFORecords()
		{	
            List<HINFO_Record> retVal = new List<HINFO_Record>();
            foreach(DnsRecordBase record in m_pAnswers){
                if(record.RecordType == QTYPE.HINFO){
                    retVal.Add((HINFO_Record)record);
                }
            }

            return retVal.ToArray();
		}

		#endregion

		#region method GetMXRecords

		/// <summary>
		/// Gets MX records.(MX records are sorted by preference, lower array element is prefered)
		/// </summary>
		/// <returns></returns>
		public MX_Record[] GetMXRecords()
		{
            List<MX_Record> mx = new List<MX_Record>();
            foreach(DnsRecordBase record in m_pAnswers){
                if(record.RecordType == QTYPE.MX){
                    mx.Add((MX_Record)record);
                }
            }

            // Sort MX records by preference.
            MX_Record[] retVal = mx.ToArray();
            Array.Sort(retVal);

            return retVal;
		}

		#endregion

		#region method GetTXTRecords

		/// <summary>
		/// Gets text records.
		/// </summary>
		/// <returns></returns>
		public TXT_Record[] GetTXTRecords()
		{
            List<TXT_Record> retVal = new List<TXT_Record>();
            foreach(DnsRecordBase record in m_pAnswers){
                if(record.RecordType == QTYPE.TXT){
                    retVal.Add((TXT_Record)record);
                }
            }

            return retVal.ToArray();
		}

		#endregion

		#region method GetAAAARecords

		/// <summary>
		/// Gets IPv6 host addess records.
		/// </summary>
		/// <returns></returns>
		public A_Record[] GetAAAARecords()
		{
            List<A_Record> retVal = new List<A_Record>();
            foreach(DnsRecordBase record in m_pAnswers){
                if(record.RecordType == QTYPE.AAAA){
                    retVal.Add((A_Record)record);
                }
            }

            return retVal.ToArray();
		}

		#endregion


		#region method FilterRecords

		/// <summary>
		/// Filters out specified type of records from answer.
		/// </summary>
		/// <param name="answers"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private List<DnsRecordBase> FilterRecords(List<DnsRecordBase> answers,QTYPE type)
		{
            List<DnsRecordBase> retVal = new List<DnsRecordBase>();
            foreach(DnsRecordBase record in answers){
                if(record.RecordType == type){
                    retVal.Add(record);
                }
            }

            return retVal;
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets if connection to dns server was successful.
		/// </summary>
		public bool ConnectionOk
		{
			get{ return m_Success; }
		}

		/// <summary>
		/// Gets dns server response code.
		/// </summary>
		public RCODE ResponseCode
		{
			get{ return m_RCODE; }
		}

		
		/// <summary>
		/// Gets all resource records returned by server (answer records section + authority records section + additional records section). 
		/// NOTE: Before using this property ensure that ConnectionOk=true and ResponseCode=RCODE.NO_ERROR.
		/// </summary>
		public DnsRecordBase[] AllAnswers
		{
			get{
                List<DnsRecordBase> retVal = new List<DnsRecordBase>();
                retVal.AddRange(m_pAnswers.ToArray());
                retVal.AddRange(m_pAuthoritiveAnswers.ToArray());
                retVal.AddRange(m_pAdditionalAnswers.ToArray());

                return retVal.ToArray();
			}
		}

		/// <summary>
		/// Gets dns server returned answers. NOTE: Before using this property ensure that ConnectionOk=true and ResponseCode=RCODE.NO_ERROR.
		/// </summary>
		/// <code>
		/// // NOTE: DNS server may return diffrent record types even if you query MX.
		/// //       For example you query lumisoft.ee MX and server may response:	
		///	//		 1) MX - mail.lumisoft.ee
		///	//		 2) A  - lumisoft.ee
		///	// 
		///	//       Before casting to right record type, see what type record is !
		///				
		/// 
		/// foreach(DnsRecordBase record in Answers){
		///		// MX record, cast it to MX_Record
		///		if(record.RecordType == QTYPE.MX){
		///			MX_Record mx = (MX_Record)record;
		///		}
		/// }
		/// </code>
		public DnsRecordBase[] Answers
		{
			get{ return m_pAnswers.ToArray(); }
		}

		/// <summary>
		/// Gets name server resource records in the authority records section. NOTE: Before using this property ensure that ConnectionOk=true and ResponseCode=RCODE.NO_ERROR.
		/// </summary>
		public DnsRecordBase[] AuthoritiveAnswers
		{
			get{ return m_pAuthoritiveAnswers.ToArray(); }
		}

		/// <summary>
		/// Gets resource records in the additional records section. NOTE: Before using this property ensure that ConnectionOk=true and ResponseCode=RCODE.NO_ERROR.
		/// </summary>
		public DnsRecordBase[] AdditionalAnswers
		{
			get{ return m_pAdditionalAnswers.ToArray(); }
		}

		#endregion
	}
}
