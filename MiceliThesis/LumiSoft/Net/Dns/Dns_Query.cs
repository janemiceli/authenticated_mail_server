using System;
using System.Text;

namespace LumiSoft.Net.Dns
{
	/// <summary>
	/// Summary description for Dns_Query.
	/// </summary>
	internal class Dns_Query
	{
		private string m_QNAME  = "";
		private QTYPE  m_QTYPE  = QTYPE.MX;
		private int    m_QCALSS = 1;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Dns_Query()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="qname"></param>
		/// <param name="qtype"></param>
		/// <param name="qclass"></param>
		public Dns_Query(string qname,QTYPE qtype,int qclass)
		{
			m_QNAME  = qname;
			m_QTYPE  = qtype;
			m_QCALSS = qclass;
		}

        
		#region function GetQuery

		/// <summary>
		/// Gets query.
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public byte[] GetQuery(Dns_Header header)
		{
			byte[] query = null;

			try
			{
				/* 	4.1.2. Question section format
					                          1  1  1  1  1  1
				0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                                               |
				/                     QNAME                     /
				/                                               /
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                     QTYPE                     |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
				|                     QCLASS                    |
				+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			
				QNAME
					a domain name represented as a sequence of labels, where
				 each label consists of a length octet followed by that
					number of octets.  The domain name terminates with the
				 zero length octet for the null label of the root.  Note
					that this field may be an odd number of octets; no
				 padding is used.
				*/

				query = new byte[521];

				// Copy dns header to query
				byte[] head = header.GetHeader();
				head.CopyTo(query,0);

				//--------- Query part ------------------------------------//
				string[] labels = m_QNAME.Split(new char[] {'.'});
				int position = 12;
					
				// Copy all domain parts(labels) to query
				// eg. lumisoft.ee = 2 labels, lumisoft and ee.
				// format = label.length + label(bytes)
				foreach(string label in labels){
					// add label lenght to query
					query[position++] = (byte)(label.Length); 

					// convert label string to byte array
					byte[] b = Encoding.ASCII.GetBytes(label.ToCharArray());
					b.CopyTo(query,position);

					// Move position by label length
					position += b.Length;
				}

				// Terminate domain (see note above)
				query[position++] = (byte) 0; 
			
				// Set QTYPE 
				query[position++] = (byte) 0;
				query[position++] = (byte)m_QTYPE;
				
				// Set QCLASS
				query[position++] = (byte) 0;
				query[position++] = (byte)m_QCALSS;
				//-------------------------------------------------------//
			}
			catch
			{
				query = null;
			}

			return query;
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// 
		/// </summary>
		public string QNAME
		{
			get{ return m_QNAME; }
		}

		/// <summary>
		/// Gets query type.
		/// </summary>
		public QTYPE QTYPE
		{
			get{ return m_QTYPE; }
		}

		/// <summary>
		/// Gets query class.
		/// </summary>
		public int QCLASS
		{
			get{ return m_QCALSS; }
		}

		#endregion

	}
}
