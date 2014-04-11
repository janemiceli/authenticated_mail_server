using System;

namespace LumiSoft.Net.Dns
{

	#region class MX_Record

	/// <summary>
	/// MX record class.
	/// </summary>
	public class MX_Record
	{
		private int    m_Preference = 0;
		private string m_Host       = "";

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="preference">MX record preference.</param>
		/// <param name="host">Mail host dns name.</param>
		public MX_Record(int preference,string host)
		{
			m_Preference = preference;
			m_Host       = host;
		}

		#region Properties Implementation

		/// <summary>
		/// Gets MX record preference.
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

	#endregion
}
