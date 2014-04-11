using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace LumiSoft.Net.FTP.Client
{
	/// <summary>
	/// Transfer mode.
	/// </summary>
	public enum TransferMode
	{
		/// <summary>
		/// ASCII transfer mode.
		/// </summary>
		Ascii = 0,
		/// <summary>
		/// Binary transfer mode. 
		/// </summary>
		Binary = 1,
	}

	/// <summary>
	/// Ftp client.
	/// </summary>
	public class FTP_Client : IDisposable
	{
		private Socket m_pClient      = null;
		private bool   m_Connected    = false;

		/// <summary>
		/// Default connection.
		/// </summary>
		public FTP_Client()
		{			
		}

		#region function Dispose

		/// <summary>
		/// Clears resources and closes connection if open.
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
		/// <param name="port">Port.</param>
		public void Connect(string host,int port)
		{
			m_pClient = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
			IPEndPoint ipdest = new IPEndPoint(System.Net.Dns.Resolve(host).AddressList[0],port);
			m_pClient.Connect(ipdest);
			
			string reply = Core.ReadLine(m_pClient);
			if(reply.StartsWith("220")){				
				m_Connected = true;
			}
			else{
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

		#region function Disconnect

		/// <summary>
		/// Disconnects from active host.
		/// </summary>
		public void Disconnect()
		{
			if(m_pClient != null){
				// Send QUIT
				Core.SendLine(m_pClient,"QUIT");
				m_pClient.Close();
				m_pClient = null;
			}

			m_Connected = false;
		}

		#endregion

		#region function Authenticate

		/// <summary>
		/// Authenticates user.
		/// </summary>
		/// <param name="userName">User name.</param>
		/// <param name="password">Password.</param>
		public void Authenticate(string userName,string password)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			Core.SendLine(m_pClient,"USER " + userName);

			string reply = Core.ReadLine(m_pClient);
			if(reply.StartsWith("331")){
				Core.SendLine(m_pClient,"PASS " + password);

				reply = Core.ReadLine(m_pClient);
				if(reply.StartsWith("230")){
			//		m_Authenticated = true;
				}
				else{
					throw new Exception(reply);
				}
			}
			else{
				throw new Exception(reply);
			}
		}

		#endregion

		
		#region function SetCurrentDir

		/// <summary>
		/// Sets current directory.
		/// </summary>
		/// <param name="dir">Directory.</param>
		public void SetCurrentDir(string dir)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			Core.SendLine(m_pClient,"CWD " + dir);

			string reply = Core.ReadLine(m_pClient);
			if(!reply.StartsWith("250")){
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

		#region function CreateDir

		/// <summary>
		/// Creates directory.
		/// </summary>
		/// <param name="dir"></param>
		public void CreateDir(string dir)
		{
			throw new Exception("Not implemented yet !");
		}

		#endregion

		#region function ReceiveFile

		/// <summary>
		/// Recieves specified file from server.
		/// </summary>
		/// <param name="fileName">File name of file which to recieve.</param>
		/// <param name="mode">Transfer mode.</param>
		/// <param name="putFileName">File path+name which to store.</param>
		public void ReceiveFile(string fileName,TransferMode mode,string putFileName)
		{
			using(FileStream fs = File.Create(putFileName)){
				ReceiveFile(fileName,mode,fs);
			}
		}

		/// <summary>
		/// Recieves specified file from server.
		/// </summary>
		/// <param name="fileName">File name of file which to recieve.</param>
		/// <param name="mode">Transfer mode.</param>
		/// <param name="storeStream">Stream where to store file.</param>
		public void ReceiveFile(string fileName,TransferMode mode,Stream storeStream)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			// Set transfer mode
			this.SetTransferMode(mode);

			TcpListener conn = new TcpListener(IPAddress.Any,this.Port());
			conn.Start();

			// Send STOR command
			Core.SendLine(m_pClient,"RETR " + fileName);

			string reply = Core.ReadLine(m_pClient);
			if(!reply.StartsWith("150")){
				throw new Exception(reply);
			}
			
			// Wait ftp server to connect
			// ToDo:May hang here if ftp server won't connect !!!
			//      must control AcceptSocket from other thread
			using(Socket clnt  = conn.AcceptSocket()){
				int count = 1;
				while(count > 0){
					byte[] data  = new Byte[4000];
					count = clnt.Receive(data,data.Length,SocketFlags.None);
					storeStream.Write(data,0,count);
				}
			}

			conn.Stop();

			// Get "226 Transfer Complete" response
			reply = Core.ReadLine(m_pClient);
			if(!reply.StartsWith("226")){
				throw new Exception(reply);
			}
		}

		#endregion

		#region function StoreFile

		/// <summary>
		/// Stores specified file to server.
		/// </summary>
		/// <param name="getFileName">File path+name which to store in server.</param>
		/// <param name="fileName">File name to store in server.</param>
		/// <param name="mode">Transfer mode.</param>
		public void StoreFile(string getFileName,string fileName,TransferMode mode)
		{
			using(FileStream fs = File.OpenRead(getFileName)){
				StoreFile(fs,fileName,mode);
			}
		}

		/// <summary>
		/// Stores specified file to server.
		/// </summary>
		/// <param name="getStream">Stream from where to gets file.</param>
		/// <param name="fileName">File name to store in server.</param>
		/// <param name="mode">Transfer mode.</param>
		public void StoreFile(Stream getStream,string fileName,TransferMode mode)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			// Set transfer mode
			this.SetTransferMode(mode);

			TcpListener conn = new TcpListener(IPAddress.Any,this.Port());
			conn.Start();

			// Send STOR command
			Core.SendLine(m_pClient,"STOR " + fileName);

			string reply = Core.ReadLine(m_pClient);
			if(!reply.StartsWith("150")){
				throw new Exception(reply);
			}
			
			// Wait ftp server to connect
			// ToDo:May hang here if ftp server won't connect !!!
			//      must control AcceptSocket from other thread
			using(Socket clnt  = conn.AcceptSocket()){
				int count = 1;
				while(count > 0){
					byte[] data  = new Byte[4000];
					count = getStream.Read(data,0,data.Length);
					clnt.Send(data,0,count,SocketFlags.None);
				}
			}

			conn.Stop();

			// Get "226 Transfer Complete" response
			reply = Core.ReadLine(m_pClient);
			if(!reply.StartsWith("226")){
				throw new Exception(reply);
			}
		}

		#endregion
		
		#region function Delete

		/// <summary>
		/// Deletes specified file or directory.
		/// </summary>
		/// <param name="file">File name.</param>
		public void DeleteFile(string file)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			throw new Exception("Not implemented yet !");
		}

		#endregion

		#region function Rename

		/// <summary>
		/// Renames specified file or directory.
		/// </summary>
		/// <param name="file">File name.</param>
		public void Rename(string file)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			throw new Exception("Not implemented yet !");
		}

		#endregion

		#region function GetsList
		
		/// <summary>
		/// 
		/// </summary>
		public void GetsList()
		{
			throw new Exception("Not implemented yet !");
		}

		#endregion


		#region function Port
        
		private int Port()
		{
			/*
			 Syntax:{PORT ipPart1,ipPart1,ipPart1,ipPart1,portPart1,portPart1<CRLF>}
			
			<host-port> ::= <host-number>,<port-number>
            <host-number> ::= <number>,<number>,<number>,<number>
            <port-number> ::= <number>,<number>
            <number> ::= any decimal integer 1 through 255
			*/
		
			
			IPHostEntry ipThis = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName());
			Random r = new Random();
			int port = 0;
			bool found = false;
			// we will try all IP addresses assigned to this machine
			// the first one that the remote machine likes will be chosen
			for(int tryCount=0;tryCount<20;tryCount++){
				for(int i=0;i<ipThis.AddressList.Length;i++){
					string ip = ipThis.AddressList[i].ToString().Replace(".",",");
					int p1 = r.Next(100);
					int p2 = r.Next(100);

					port = (p1 << 8) | p2;
			
					Core.SendLine(m_pClient,"PORT " + ip + "," + p1.ToString() + "," + p2.ToString());

					string reply = Core.ReadLine(m_pClient);
					if(reply.StartsWith("200")){
						found = true;
					break;
					}
				}
			}

			if(!found){
				throw new Exception("No suitable port found");
			}

			return port;
		}

		#endregion

		#region function SetTransferMode

		/// <summary>
		/// Sets transfer mode.
		/// </summary>
		/// <param name="mode">Transfer mode.</param>
		private void SetTransferMode(TransferMode mode)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}

			switch(mode)
			{
				case TransferMode.Ascii:
					Core.SendLine(m_pClient,"TYPE A");
					break;

				case TransferMode.Binary:
					Core.SendLine(m_pClient,"TYPE I");
					break;
			}			

			string reply = Core.ReadLine(m_pClient);
			if(!reply.StartsWith("200")){
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

	}
}
