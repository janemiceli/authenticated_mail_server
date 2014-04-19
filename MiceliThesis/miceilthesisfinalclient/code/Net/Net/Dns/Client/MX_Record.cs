using System;

namespace LumiSoft.Net.Dns.Client
{
	/// <summary>
	/// MX record class.
	/// </summary>
	[Serializable]
	public class MX_Record : DnsRecordBase,IComparable
	{
		private int    m_Preference = 0;
		private string m_Host       = "";

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="preference">MX record preference.</param>
		/// <param name="host">Mail host dns name.</param>
		/// <param name="ttl">TTL value.</param>
		public MX_Record(int preference,string host,int ttl) : base(QTYPE.MX,ttl)
		{
			m_Preference = preference;
			m_Host       = host;
        }


        #region IComparable Implementation

        /// <summary>
        /// Compares the current instance with another object of the same type. 
        /// </summary>
        /// <param name="obj">An object to compare with this instance. </param>
        /// <returns>Returns 0 if two objects are equal, returns negative value if this object is less,
        /// returns positive value if this object is grater.</returns>
        public int CompareTo(object obj)
        {
            if(obj == null){
                throw new ArgumentNullException("obj");
            }
            if(!(obj is MX_Record)){
                throw new ArgumentException("Argument obj is not MX_Record !");
            }

            MX_Record mx = (MX_Record)obj;
            if(this.Preference > mx.Preference){
                return 1;
            }
            else if(this.Preference < mx.Preference){
                return -1;
            }
            else{
                return 0;
            }
        }

        #endregion


        #region Properties Implementation

        /// <summary>
		/// Gets MX record preference. The lower number is the higher priority server.
		/// </summary>
		public int Preference
		{
			get{ return m_Preference; }
		}

		/// <summary>
		/// Gets mail host dns name.
		/// </summary>
		public string Host
		{
			get{ return m_Host; }
		}

		#endregion

	}
}
