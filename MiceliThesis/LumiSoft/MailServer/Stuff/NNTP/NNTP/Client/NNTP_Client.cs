using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Security.Cryptography;

namespace LumiSoft.Net.NNTP.Server
{
	/// <summary>
	/// Summary description for NNTP_Client.
	/// </summary>
	public class NNTP_Client: IDisposable
	{
		private Socket  m_Socket        = null;
		private bool    m_Connected     = false;
		private bool    m_Authenticated = false;
		private string  m_ApopHashKey   = "";

		public NNTP_Client()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		#region function Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		public void Dispose()
		{
			Disconnect();
		}

		#endregion

		#region function Connect

		/// <summary>
		/// Connects to specified host.
		/// </summary>
		/// <param name="host">Host name.</param>
		/// <param name="port">Port number.</param>
		public void Connect(string host,int port)
		{
			if(!m_Connected)
			{
				m_Socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
				IPEndPoint ipdest = new IPEndPoint(System.Net.Dns.Resolve(host).AddressList[0],port);
				m_Socket.Connect(ipdest);

				// Set connected flag
				m_Connected     = true;
				m_Authenticated = true;
				string reply = Core.ReadLine(m_Socket);
				if(reply.StartsWith("20"))
				{
					// Try to read APOP hash key, if supports APOP
					if(reply.IndexOf("<") > -1 && reply.IndexOf(">") > -1)
					{
						m_ApopHashKey = reply.Substring(reply.LastIndexOf("<"),reply.LastIndexOf(">") - reply.LastIndexOf("<") + 1);
					}
				}				
			}
		}

		#endregion

		#region function Disconnect

		/// <summary>
		/// Closes connection to nntp server.
		/// </summary>
		public void Disconnect()
		{
			if(m_Socket != null)
			{
				// Send QUIT
				Core.SendLine(m_Socket,"QUIT");			

				m_Socket.Close();
			}

			m_Connected = false;
		}

		#endregion

		#region function NewNews

		/// <summary>
		/// Gets messages info.
		/// </summary>
		public Hashtable NewNews(string argsText)
		{
			if(!m_Connected)
			{
				throw new Exception("You must connect first !");
			}

			if(!m_Authenticated)
			{
				throw new Exception("You must authenticate first !");
			}
						
			Hashtable messageIds = new Hashtable(); 

			Core.SendLine(m_Socket,"NewNews " + argsText);

			// Read first line of reply, check if it's ok
			string line = Core.ReadLine(m_Socket);
			if(line.StartsWith("230"))
			{
				// Read lines while get only '.' on line itshelf.
				while(true)
				{
					line = Core.ReadLine(m_Socket);

					// End of data
					if(line.Trim() == ".")
					{
						break;
					}
					else
					{

						if(!messageIds.ContainsKey(line))
						{
							messageIds.Add(line,null); 
						}
					}
				}
			}
			else
			{
				throw new Exception("Server returned:" + line);
			}

			return messageIds;
		}

		#endregion

		#region function Article

		/// <summary>
		/// Gets specified Article.
		/// </summary>
		/// <param name="nr">Article number.</param>
		public byte[] Article(int nr)
		{
			if(!m_Connected)
			{
				throw new Exception("You must connect first !");
			}

			if(!m_Authenticated)
			{
				throw new Exception("You must authenticate first !");
			}

			Core.SendLine(m_Socket,"ARTICLE " + nr.ToString());

			// Read first line of reply, check if it's ok
			string line = Core.ReadLine(m_Socket);
			if(line.StartsWith("220"))
			{
				return Core.DoPeriodHandling(Core.ReadData(m_Socket,"\r\n.\r\n",".\r\n"),false).ToArray();
			}
			else
			{
				throw new Exception("Server returned:" + line);
			}
		}

		public byte[] Article(string id)
		{
			if(!m_Connected)
			{
				throw new Exception("You must connect first !");
			}

			if(!m_Authenticated)
			{
				throw new Exception("You must authenticate first !");
			}

			Core.SendLine(m_Socket,"ARTICLE " + id);

			// Read first line of reply, check if it's ok
			string line = Core.ReadLine(m_Socket);
			if(line.StartsWith("220"))
			{
				return Core.DoPeriodHandling(Core.ReadData(m_Socket,"\r\n.\r\n",".\r\n"),false).ToArray();
			}
			else
			{
				return null;
			}
		}
		#endregion



	}
}
