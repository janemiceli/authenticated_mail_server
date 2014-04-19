using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Security.Cryptography;

namespace LumiSoft.Net.POP3.Client
{
	/// <summary>
	/// POP3 Client.
	/// </summary>
	/// <example>
	/// <code>
	/// 
	/// /*
	///  To make this code to work, you need to import following namespaces:
	///  using LumiSoft.Net.Mime;
	///  using LumiSoft.Net.POP3.Client; 
	///  */
	/// 
	/// using(POP3_Client c = new POP3_Client()){
	///		c.Connect("ivx",110);
	///		c.Authenticate("test","test",true);
	///		
	///		POP3_MessagesInfo mInf = c.GetMessagesInfo();
	///		
	///		// Get first message if there is any
	///		if(mInf.Count > 0){
	///			byte[] messageData = c.GetMessage(mInf[0]);
	///		
	///			// Do your suff
	///			
	///			// Parse message
	///			Mime m = Mime.Parse(messageData);
	///			string from = m.MainEntity.From;
	///			string subject = m.MainEntity.Subject;			
	///			// ... 
	///		}		
	///	}
	/// </code>
	/// </example>
	public class POP3_Client : IDisposable
	{
		private SocketEx     m_pSocket       = null;
		private SocketLogger m_pLogger       = null;
		private bool         m_Connected     = false;
		private bool         m_Authenticated = false;
		private string       m_ApopHashKey   = "";
		private bool         m_LogCmds       = false;

		/// <summary>
		/// Occurs when POP3 session has finished and session log is available.
		/// </summary>
		public event LogEventHandler SessionLog = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public POP3_Client()
		{				
		}

		#region method Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		public void Dispose()
		{
			try{
				Disconnect();
			}
			catch{
			}
		}

		#endregion


		#region method Connect

        /// <summary>
		/// Connects to specified host.
		/// </summary>
		/// <param name="host">Host name.</param>
		/// <param name="port">Port number. Default POP3 port is 110.</param>
		public void Connect(string host,int port)
		{
            Connect(host,port,false);
        }

		/// <summary>
		/// Connects to specified host.
		/// </summary>
		/// <param name="host">Host name.</param>
		/// <param name="port">Port number. Default POP3 port is 110 and SSL port is 995.</param>
        /// <param name="ssl">Specifies if to connected via SSL.</param>
		public void Connect(string host,int port,bool ssl)
		{
			if(!m_Connected){
                SocketEx s = new SocketEx();
                s.Connect(host,port,ssl);
                m_pSocket = s;

				if(m_LogCmds && SessionLog != null){
					m_pLogger = new SocketLogger(s.RawSocket,SessionLog);
					m_pLogger.SessionID = Guid.NewGuid().ToString();
					m_pSocket.Logger = m_pLogger;
				}

				// Set connected flag
				m_Connected = true;

				string reply = m_pSocket.ReadLine();
				if(reply.StartsWith("+OK")){
					// Try to read APOP hash key, if supports APOP
					if(reply.IndexOf("<") > -1 && reply.IndexOf(">") > -1){
						m_ApopHashKey = reply.Substring(reply.LastIndexOf("<"),reply.LastIndexOf(">") - reply.LastIndexOf("<") + 1);
					}
				}
			}
		}

		#endregion

		#region method Disconnect

		/// <summary>
		/// Closes connection to POP3 server.
		/// </summary>
		public void Disconnect()
		{
			try{
				if(m_pSocket != null){
					// Send QUIT
					m_pSocket.WriteLine("QUIT");			

					m_pSocket.Shutdown(SocketShutdown.Both);					
				}
			}
			catch{
			}

			if(m_pLogger != null){
				m_pLogger.Flush();
			}
			m_pLogger = null;

			m_pSocket       = null;
			m_Connected     = false;			
			m_Authenticated = false;
		}

		#endregion

        #region method StartTLS

        /// <summary>
        /// Switches POP3 connection to SSL.
        /// </summary>
        public void StartTLS()
        {
            /* RFC 2595 4. POP3 STARTTLS extension.
                Arguments: none

                Restrictions:
                    Only permitted in AUTHORIZATION state.
             
                Possible Responses:
                     +OK -ERR

                 Examples:
                     C: STLS
                     S: +OK Begin TLS negotiation
                     <TLS negotiation, further commands are under TLS layer>
                       ...
                     C: STLS
                     S: -ERR Command not permitted when TLS active
            */

            if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(m_Authenticated){
				throw new Exception("The STLS command is only valid in non-authenticated state !");
			}
            if(m_pSocket.SSL){
                throw new Exception("Connection is already secure !");
            }

            m_pSocket.WriteLine("STLS");

            string reply = m_pSocket.ReadLine();
			if(!reply.ToUpper().StartsWith("+OK")){
				throw new Exception("Server returned:" + reply);
			}

            m_pSocket.SwitchToSSL_AsClient();
        }

        #endregion

		#region method Authenticate

		/// <summary>
		/// Authenticates user.
		/// </summary>
		/// <param name="userName">User login name.</param>
		/// <param name="password">Password.</param>
		/// <param name="tryApop"> If true and POP3 server supports APOP, then APOP is used, otherwise normal login used.</param>
		public void Authenticate(string userName,string password,bool tryApop)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			if(m_Authenticated){
				throw new Exception("You are already authenticated !");
			}

			// Supports APOP, use it
			if(tryApop && m_ApopHashKey.Length > 0){
				//--- Compute md5 hash -----------------------------------------------//
				byte[] data = System.Text.Encoding.ASCII.GetBytes(m_ApopHashKey + password);
			
				MD5 md5 = new MD5CryptoServiceProvider();			
				byte[] hash = md5.ComputeHash(data);

				string hexHash = BitConverter.ToString(hash).ToLower().Replace("-","");
				//---------------------------------------------------------------------//

				m_pSocket.WriteLine("APOP " + userName + " " + hexHash);

				string reply = m_pSocket.ReadLine();
				if(reply.StartsWith("+OK")){
					m_Authenticated = true;
				}
				else{
					throw new Exception("Server returned:" + reply);
				}
			}
			else{ // Use normal LOGIN, don't support APOP 
				m_pSocket.WriteLine("USER " + userName);

				string reply = m_pSocket.ReadLine();
				if(reply.StartsWith("+OK")){
					m_pSocket.WriteLine("PASS " + password);

					reply = m_pSocket.ReadLine();
					if(reply.StartsWith("+OK")){
						m_Authenticated = true;
					}
					else{
						throw new Exception("Server returned:" + reply);
					}
				}
				else{
					throw new Exception("Server returned:" + reply);
				}				
			}

            if(m_Authenticated && m_pSocket.Logger != null){
                m_pSocket.Logger.UserName = userName;
            }
		}

		#endregion


		#region function GetMessagesInfo

		/// <summary>
		/// Gets messages info.
		/// </summary>
		public POP3_MessagesInfo GetMessagesInfo()
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			POP3_MessagesInfo messagesInfo = new POP3_MessagesInfo();

			// Before getting list get UIDL list, then we can make full message info (UID,No,Size).
			Hashtable uidlList = GetUidlList();

			m_pSocket.WriteLine("LIST");

			/* NOTE: If reply is +OK, this is multiline respone and is terminated with '.'.
			Examples:
				C: LIST
				S: +OK 2 messages (320 octets)
				S: 1 120				
				S: 2 200
				S: .
				...
				C: LIST 3
				S: -ERR no such message, only 2 messages in maildrop
			*/

			// Read first line of reply, check if it's ok
			string line = m_pSocket.ReadLine();
			if(line.StartsWith("+OK")){
				// Read lines while get only '.' on line itshelf.
				while(true){
					line = m_pSocket.ReadLine();

					// End of data
					if(line.Trim() == "."){
						break;
					}
					else{
						string[] param = line.Trim().Split(new char[]{' '});
						int  no   = Convert.ToInt32(param[0]);
						long size = Convert.ToInt64(param[1]);

						messagesInfo.Add(uidlList[no].ToString(),no,size);
					}
				}
			}
			else{
				throw new Exception("Server returned:" + line);
			}

			return messagesInfo;
		}

		#endregion

		#region function GetUidlList

		/// <summary>
		/// Gets uid listing.
		/// </summary>
		/// <returns>Returns Hashtable containing uidl listing. Key column contains message NR and value contains message UID.</returns>
		public Hashtable GetUidlList()
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			Hashtable retVal = new Hashtable();

			m_pSocket.WriteLine("UIDL");

			/* NOTE: If reply is +OK, this is multiline respone and is terminated with '.'.
			Examples:
				C: UIDL
				S: +OK
				S: 1 whqtswO00WBw418f9t5JxYwZ
				S: 2 QhdPYR:00WBw1Ph7x7
				S: .
				...
				C: UIDL 3
				S: -ERR no such message
			*/

			// Read first line of reply, check if it's ok
			string line = m_pSocket.ReadLine();
			if(line.StartsWith("+OK")){
				// Read lines while get only '.' on line itshelf.				
				while(true){
					line = m_pSocket.ReadLine();

					// End of data
					if(line.Trim() == "."){
						break;
					}
					else{
						string[] param = line.Trim().Split(new char[]{' '});
						int    nr  = Convert.ToInt32(param[0]);
						string uid = param[1];

						retVal.Add(nr,uid);
					}
				}
			}
			else{
				throw new Exception("Server returned:" + line);
			}

			return retVal;
		}

		#endregion

		#region method GetMessage

		/// <summary>
		/// Gets specified message.
		/// </summary>
		/// <param name="msgInfo">Pop3 message info of message what to get.</param>
		/// <returns></returns>
		public byte[] GetMessage(POP3_MessageInfo msgInfo)
		{
			return GetMessage(msgInfo.MessageNumber);
		}

		/// <summary>
		/// Gets specified message.
		/// </summary>
		/// <param name="messageNo">Message number.</param>
		public byte[] GetMessage(int messageNo)
		{
            MemoryStream stream = new MemoryStream();
			GetMessage(messageNo,stream);
            return stream.ToArray();
		}

        /// <summary>
        /// Gets specified message and stores it to specified stream.
        /// </summary>
        /// <param name="messageNo">Message number.</param>
        /// <param name="stream">Stream where to store message.</param>
        public void GetMessage(int messageNo,Stream stream)
		{
            if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			m_pSocket.WriteLine("RETR " + messageNo.ToString());

			// Read first line of reply, check if it's ok
			string line = m_pSocket.ReadLine();
			if(line.StartsWith("+OK")){    
                m_pSocket.ReadPeriodTerminated(stream,100000000);
			}
			else{
				throw new Exception("Server returned:" + line);
			}
        }

		#endregion

		#region method DeleteMessage

		/// <summary>
		/// Deletes specified message
		/// </summary>
		/// <param name="messageNr">Message number.</param>
		public void DeleteMessage(int messageNr)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			m_pSocket.WriteLine("DELE " + messageNr.ToString());

			// Read first line of reply, check if it's ok
			string line = m_pSocket.ReadLine();
			if(!line.StartsWith("+OK")){
				throw new Exception("Server returned:" + line);
			}
		}

		#endregion

		#region method GetTopOfMessage

		/// <summary>
		/// Gets top lines of message.
		/// </summary>
		/// <param name="nr">Message number which top lines to get.</param>
		/// <param name="nLines">Number of lines to get.</param>
		public byte[] GetTopOfMessage(int nr,int nLines)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}
			

			m_pSocket.WriteLine("TOP " + nr.ToString() + " " + nLines.ToString());

			// Read first line of reply, check if it's ok
			string line = m_pSocket.ReadLine();
			if(line.StartsWith("+OK")){
				MemoryStream strm = new MemoryStream();
                m_pSocket.ReadPeriodTerminated(strm,100000000);

                return strm.ToArray();
			}
			else{
				throw new Exception("Server returned:" + line);
			}
		}

		#endregion

		#region method Reset

		/// <summary>
		/// Resets session.
		/// </summary>
		public void Reset()
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			m_pSocket.WriteLine("RSET");

			// Read first line of reply, check if it's ok
			string line = m_pSocket.ReadLine();
			if(!line.StartsWith("+OK")){
				throw new Exception("Server returned:" + line);
			}
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets if pop3 client is connected.
		/// </summary>
		public bool Connected
		{
			get{ return m_Connected; }
		}

		/// <summary>
		/// Gets if pop3 client is authenticated.
		/// </summary>
		public bool Authenticated
		{
			get{ return m_Authenticated; }
		}

		/// <summary>
		/// Gets or sets if to log commands.
		/// </summary>
		public bool LogCommands
		{
			get{ return m_LogCmds;	}

			set{ m_LogCmds = value; }
		}
        
        /// <summary>
        /// Gets if the connection is an SSL connection.
        /// </summary>
        public bool IsSecureConnection
        {
            get{ 
                if(!m_Connected){
				    throw new Exception("You must connect first");
			    }

                return m_pSocket.SSL; 
            }
        }

		#endregion

	}
}
