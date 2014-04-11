using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace LumiSoft.Net.Dns
{
	/// <summary>
	/// Dns reply codes.
	/// </summary>
	public enum DnsReplyCode
	{
		/// <summary>
		/// Requested records retrieved sucessfully.
		/// </summary>
		Ok = 0,

		/// <summary>
		/// No requested records found.
		/// </summary>
		NoEntries = 1,

		/// <summary>
		/// There was error retrieving records.
		/// </summary>
		TempError = 2,
	}

	/// <summary>
	/// Dns.
	/// </summary>
	public class DnsEx
	{
		private static string[] m_DnsServers  = null;
		private static bool     m_UseDnsCache = true;
		private static int      m_ID          = 100;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DnsEx()
		{
			if(!DnsCache.CacheInited){
				DnsCache.InitNewCache();
			}
		}


		#region function GetMXRecords

		/// <summary>
		/// Gets MX records.(MX records are sorted by preference, lower array element is prefered)
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="mxRecords"></param>
		/// <returns></returns>
		public DnsReplyCode GetMXRecords(string domain,out MX_Record[] mxRecords)
		{
			mxRecords = null;

			//--- Try to get MX records from cache
			if(m_UseDnsCache){
				mxRecords = DnsCache.GetMXFromCache(domain);
				if(mxRecords != null){
			//		Console.WriteLine("domain:" + domain + " from cahce.");
					return DnsReplyCode.Ok;
				}
			}
			//------------------------------------//

			Dns_Header header = new Dns_Header(DnsEx.ID,OPCODE.IQUERY);
			Dns_Query  query  = new Dns_Query(domain,QTYPE.MX,1);
						
			byte[] bQuery = query.GetQuery(header);

			byte[] reply = GetQuery(bQuery,header.ID);
			if(reply != null){				
				Dns_Answers answers = new Dns_Answers();
				if(answers.ParseAnswers(reply,header.ID)){
					mxRecords = answers.GetMxRecordsFromAnswers();

					if(mxRecords != null){
						// Add to cache
						DnsCache.AddMXToCache(domain,mxRecords);
						return DnsReplyCode.Ok;
					}
					else{
						return DnsReplyCode.NoEntries;
					}					
				}
			}
			
			return DnsReplyCode.TempError;			
		}

		#endregion


		#region function GetQuery

		private byte[] GetQuery(byte[] query,int queryID)
		{
			byte[] retVal = null;

			try
			{				
				int helper = 0;
				int count  = 0;
				while(count < 10){
					if(count > m_DnsServers.Length){
						helper = 0;
					}

					byte[] reply = QueryServer(query,m_DnsServers[helper]);
					Dns_Answers answers = new Dns_Answers();

					// If reply is ok, return it
					if(reply != null && answers.ParseAnswers(reply,queryID)){
						return reply;
					}

					count++;
					helper++;
				}
			}
			catch{
			}

			return retVal;
		}

		#endregion

		#region function QueryServer

		private byte[] QueryServer(byte[] query,string serverIP)
		{
			byte[] retVal = null;

			try
			{
				IPEndPoint ipRemoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIP),53);
				Socket udpClient = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
		
				IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Any, 0);
				EndPoint localEndPoint = (EndPoint)ipLocalEndPoint;
				udpClient.Bind(localEndPoint);	
		
				udpClient.Connect(ipRemoteEndPoint);

				//send query
				udpClient.Send(query);

				// Wait until we have a reply
				if(udpClient.Poll(5*1000000,SelectMode.SelectRead)){
					retVal = new byte[512];
					udpClient.Receive(retVal);
				}

				udpClient.Close();
			}
			catch
			{
			}

			return retVal;
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets or sets dns servers.
		/// </summary>
		public static string[] DnsServers
		{
			get{ return m_DnsServers; }

			set{ m_DnsServers = value; }
		}

		/// <summary>
		/// Gets or sets if to use dns caching.
		/// </summary>
		public static bool UseDnsCache
		{
			get{ return m_UseDnsCache; }

			set{ m_UseDnsCache = value; }
		}

		internal static int ID
		{
			get{
				if(m_ID >= 65535){
					m_ID = 100;
				}
				return m_ID++; 
			}
		}

		#endregion

	}
}
