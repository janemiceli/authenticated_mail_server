using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace LumiSoft.Net
{
	#region enum AuthType

	/// <summary>
	/// Authentication type.
	/// </summary>
	public enum AuthType
	{
		/// <summary>
		/// Plain username/password authentication.
		/// </summary>
		Plain = 0,

		/// <summary>
		/// APOP
		/// </summary>
		APOP  = 1,

		/// <summary>
		/// Not implemented.
		/// </summary>
		LOGIN = 2,	
	
		/// <summary>
		/// Cram-md5 authentication.
		/// </summary>
		CRAM_MD5 = 3,	
	}

	#endregion

	/// <summary>
	/// Provides net core utility methods.
	/// </summary>
	public class Core
	{

		#region function DoPeriodHandling

		/// <summary>
		/// Does period handling.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="add_Remove">If true add periods, else removes periods.</param>
		/// <returns></returns>
		public static MemoryStream DoPeriodHandling(byte[] data,bool add_Remove)
		{
			using(MemoryStream strm = new MemoryStream(data)){
				return DoPeriodHandling(strm,add_Remove);
			}
		}

		/// <summary>
		/// Does period handling.
		/// </summary>
		/// <param name="strm">Input stream.</param>
		/// <param name="add_Remove">If true add periods, else removes periods.</param>
		/// <returns></returns>
		public static MemoryStream DoPeriodHandling(Stream strm,bool add_Remove)
		{
			return DoPeriodHandling(strm,add_Remove,true);
		}

		/// <summary>
		/// Does period handling.
		/// </summary>
		/// <param name="strm">Input stream.</param>
		/// <param name="add_Remove">If true add periods, else removes periods.</param>
		/// <param name="setStrmPosTo0">If true sets stream position to 0.</param>
		/// <returns></returns>
		public static MemoryStream DoPeriodHandling(Stream strm,bool add_Remove,bool setStrmPosTo0)
		{			
			MemoryStream replyData = new MemoryStream();

			byte[] crlf = new byte[]{(byte)'\r',(byte)'\n'};

			if(setStrmPosTo0){
				strm.Position = 0;
			}

			StreamLineReader r = new StreamLineReader(strm);
			byte[] line = r.ReadLine();

			// Loop through all lines
			while(line != null){
				if(line.Length > 0){
					if(line[0] == (byte)'.'){
						/* Add period Rfc 281 4.5.2
						   -  Before sending a line of mail text, the SMTP client checks the
						   first character of the line.  If it is a period, one additional
						   period is inserted at the beginning of the line.
						*/
						if(add_Remove){
							replyData.WriteByte((byte)'.');
							replyData.Write(line,0,line.Length);
						}
						/* Remove period Rfc 281 4.5.2
						 If the first character is a period , the first characteris deleted.							
						*/
						else{
							replyData.Write(line,1,line.Length-1);
						}
					}
					else{
						replyData.Write(line,0,line.Length);
					}
				}					
                // write enc stuff here miceli
				replyData.Write(crlf,0,crlf.Length);

				// Read next line
				line = r.ReadLine();
			}

			replyData.Position = 0;

			return replyData;
		}

		#endregion


		#region function ReadLine

		/// <summary>
		/// Reads line of data from Socket.
		/// </summary>
		/// <param name="socket"></param>
		/// <returns></returns>
		public static string ReadLine(Socket socket)
		{
			return ReadLine(socket,500,60000);
		}

		/// <summary>
		/// Reads line of data from Socket.
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="maxLen"></param>
		/// <param name="idleTimeOut"></param>
		/// <returns></returns>
		public static string ReadLine(Socket socket,int maxLen,int idleTimeOut)
		{
			try
			{
				long lastDataTime   = DateTime.Now.Ticks;
				ArrayList lineBuf   = new ArrayList();
				byte      prevByte  = 0;
				
				while(true){
					if(socket.Available > 0){
						// Read next byte
						byte[] currByte = new byte[1];
						int countRecieved = socket.Receive(currByte,1,SocketFlags.None);
						// Count must be equal. Eg. some computers won't give byte at first read attempt
						if(countRecieved == 1){
							lineBuf.Add(currByte[0]);

							// Line found
							if((prevByte == (byte)'\r' && currByte[0] == (byte)'\n')){
								byte[] retVal = new byte[lineBuf.Count-2];    // Remove <CRLF> 
								lineBuf.CopyTo(0,retVal,0,lineBuf.Count-2);
							
								return System.Text.Encoding.Default.GetString(retVal).Trim();
							}
						
							// Store byte
							prevByte = currByte[0];

							// Check if maximum length is exceeded
							if(lineBuf.Count > maxLen){
								throw new ReadException(ReadReplyCode.LengthExceeded,"Maximum line length exceeded");
							}

							// reset last data time
							lastDataTime = DateTime.Now.Ticks;
						}						
					}
					else{
						//---- Time out stuff -----------------------//
						if(DateTime.Now.Ticks > lastDataTime + ((long)(idleTimeOut)) * 10000){
							throw new ReadException(ReadReplyCode.TimeOut,"Read timeout");
						}					
						System.Threading.Thread.Sleep(100);									
						//------------------------------------------//
					}
				}
			}
			catch(Exception x){
				if(x is ReadException){
					throw x;
				}
				throw new ReadException(ReadReplyCode.UnKnownError,x.Message);
			}
		}

		#endregion

		#region function SendLine
		
		/// <summary>
		/// Sends line to Socket.
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="lineData"></param>
		public static void SendLine(Socket socket,string lineData)
		{
			byte[] byte_data = System.Text.Encoding.Default.GetBytes(lineData + "\r\n");
			int countSended = socket.Send(byte_data);
			if(countSended != byte_data.Length){
				throw new Exception("Send error, didn't send all bytes !");
			}
		}

		#endregion


		#region function ReadData

		/// <summary>
		/// Reads byte data from Socket while gets terminator or timeout.
		/// </summary>
		/// <param name="socket">Socket from to read data.</param>
		/// <param name="terminator">Terminator which terminates data.</param>
		/// <param name="removeFromEnd">Chars which will be removed from end of data.</param>
		/// <returns></returns>
		public static byte[] ReadData(Socket socket,string terminator,string removeFromEnd)
		{
			MemoryStream storeStream = new MemoryStream();
			ReadReplyCode code = ReadData(socket,out storeStream,null,10000000,300000,terminator,removeFromEnd);

			if(code != ReadReplyCode.Ok){
				throw new Exception("Error:" + code.ToString());
			}

			return storeStream.ToArray();
		}

		#endregion
		
		#region function ReadData

		/// <summary>
		/// Reads specified count of data from Socket.
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="count">Number of bytes to read.</param>
		/// <param name="storeStrm"></param>
		/// <param name="storeToStream">If true stores readed data to stream, otherwise just junks data.</param>
		/// <param name="cmdIdleTimeOut"></param>
		/// <returns></returns>
		public static ReadReplyCode ReadData(Socket socket,long count,Stream storeStrm,bool storeToStream,int cmdIdleTimeOut)
		{
			ReadReplyCode replyCode = ReadReplyCode.Ok;

			try
			{
				long lastDataTime = DateTime.Now.Ticks;
				long readedCount  = 0;
				while(readedCount < count){
					int countAvailable = socket.Available;
					if(countAvailable > 0){
						byte[] b = null;						
						if((readedCount + countAvailable) <= count){
							b = new byte[countAvailable];								
						}
						// There are more data in socket than we need, just read as much we need
						else{
							b = new byte[count - readedCount];
						}

						int countRecieved = socket.Receive(b,0,b.Length,SocketFlags.None);
						readedCount += countRecieved;

						if(storeToStream && countRecieved > 0){
							storeStrm.Write(b,0,countRecieved);
						}

						// reset last data time
						lastDataTime = DateTime.Now.Ticks;
					}
					else{					
						//---- Idle and time out stuff ----------------------------------------//
						if(DateTime.Now.Ticks > lastDataTime + ((long)(cmdIdleTimeOut)) * (long)10000){
							replyCode = ReadReplyCode.TimeOut;
							break;
						}
						System.Threading.Thread.Sleep(50);
						//---------------------------------------------------------------------//
					}
				}
			}
			catch{
				replyCode = ReadReplyCode.UnKnownError;
			}

			return replyCode;
		}

		#endregion

		#region function ReadData

		/// <summary>
		/// Reads reply from socket.
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="replyData">Data that has been readen from socket.</param>
		/// <param name="addData">Data that has will be written at the beginning of read data. This param may be null.</param>
		/// <param name="maxLength">Maximum Length of data which may read.</param>
		/// <param name="cmdIdleTimeOut">Command idle time out in milliseconds.</param>
		/// <param name="terminator">Terminator string which terminates reading. eg '\r\n'.</param>
		/// <param name="removeFromEnd">Removes following string from reply.NOTE: removes only if ReadReplyCode is Ok.</param>		
		/// <returns>Return reply code.</returns>
		public static ReadReplyCode ReadData(Socket socket,out MemoryStream replyData,byte[] addData,int maxLength,int cmdIdleTimeOut,string terminator,string removeFromEnd)
		{
			ReadReplyCode replyCode = ReadReplyCode.Ok;	
			replyData = null;

			try
			{
				replyData = new MemoryStream();
				_FixedStack stack = new _FixedStack(terminator);
				int nextReadWriteLen = 1;

				long lastDataTime = DateTime.Now.Ticks;
				while(nextReadWriteLen > 0){
					if(socket.Available >= nextReadWriteLen){
						//Read byte(s)
						byte[] b = new byte[nextReadWriteLen];
						int countRecieved = socket.Receive(b);

						// Write byte(s) to buffer, if length isn't exceeded.
						if(replyCode != ReadReplyCode.LengthExceeded){							
							replyData.Write(b,0,countRecieved);
						}

						// Write to stack(terminator checker)
						nextReadWriteLen = stack.Push(b,countRecieved);

						//---- Check if maximum length is exceeded ---------------------------------//
						if(replyCode != ReadReplyCode.LengthExceeded && replyData.Length > maxLength){
							replyCode = ReadReplyCode.LengthExceeded;
						}
						//--------------------------------------------------------------------------//

						// reset last data time
						lastDataTime = DateTime.Now.Ticks;
					}
					else{					
						//---- Idle and time out stuff ----------------------------------------//
						if(DateTime.Now.Ticks > lastDataTime + ((long)(cmdIdleTimeOut)) * 10000){
							replyCode = ReadReplyCode.TimeOut;
							break;
						}
						System.Threading.Thread.Sleep(50);
						//---------------------------------------------------------------------//
					}
				}

				// If reply is ok then remove chars if any specified by 'removeFromEnd'.
				if(replyCode == ReadReplyCode.Ok && removeFromEnd.Length > 0){					
					replyData.SetLength(replyData.Length - removeFromEnd.Length);				
				}
			}
			catch{
				replyCode = ReadReplyCode.UnKnownError;				
			}

			return replyCode;
		}

		#endregion

		
		#region function ParseIP_from_EndPoint

		/// <summary>
		/// 
		/// </summary>
		/// <param name="endpoint"></param>
		/// <returns></returns>
		public static string ParseIP_from_EndPoint(string endpoint)
		{
			string retVal = endpoint;

			int index = endpoint.IndexOf(":");
			if(index > 1){
				retVal = endpoint.Substring(0,index);
			}

			return retVal;
		}

		#endregion

		#region function GetArgsText

		/// <summary>
		/// Gets argument part of command text.
		/// </summary>
		/// <param name="input">Input srting from where to remove value.</param>
		/// <param name="cmdTxtToRemove">Command text which to remove.</param>
		/// <returns></returns>
		public static string GetArgsText(string input,string cmdTxtToRemove)
		{
			string buff = input.Trim();
			if(buff.Length >= cmdTxtToRemove.Length){
				buff = buff.Substring(cmdTxtToRemove.Length);
			}
			buff = buff.Trim();

			return buff;
		}

		#endregion

		
		#region function GetHostName

		/// <summary>
		/// 
		/// </summary>
		/// <param name="IP"></param>
		/// <returns></returns>
		public static string GetHostName(string IP)
		{
			try
			{
				return System.Net.Dns.GetHostByAddress(IP).HostName;
			}
			catch{
				return "UnkownHost";
			}
		}

		#endregion


		#region function IsNumber

		/// <summary>
		/// Checks if specified string is number(long).
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static bool IsNumber(string str)
		{
			try
			{
				Convert.ToInt64(str);
				return true;
			}
			catch{
				return false;
			}
		}

		#endregion


		#region function QDecode

		/// <summary>
		/// quoted-printable decoder.
		/// </summary>
		/// <param name="encoding">Input string encoding.</param>
		/// <param name="data">String which to encode.</param>
		/// <returns></returns>
		public static string QDecode(System.Text.Encoding encoding,string data)
		{
			MemoryStream strm = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(data));
			int b = strm.ReadByte();

			MemoryStream dStrm = new MemoryStream();

			while(b > -1){
				// Hex eg. =E4
				if(b == '='){
					byte[] buf = new byte[2];
					strm.Read(buf,0,2);

					// <CRLF> followed by =, it's splitted line
					if(!(buf[0] == '\r' && buf[1] == '\n')){
						try{
							int val = int.Parse(System.Text.Encoding.ASCII.GetString(buf),System.Globalization.NumberStyles.HexNumber);
							string encodedChar = encoding.GetString(new byte[]{(byte)val});
							byte[] d = System.Text.Encoding.Unicode.GetBytes(encodedChar);
							dStrm.Write(d,0,d.Length);
						}
						catch{ // If worng hex value, just skip this chars
						}
					}
				}
				else{
					string encodedChar = encoding.GetString(new byte[]{(byte)b});
					byte[] d = System.Text.Encoding.Unicode.GetBytes(encodedChar);
					dStrm.Write(d,0,d.Length);
				}

				b = strm.ReadByte();
			}

			return System.Text.Encoding.Unicode.GetString(dStrm.ToArray());
		}

		#endregion

		#region function IsAscii

		/// <summary>
		/// Checks if specified string data is acii data.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool IsAscii(string data)
		{			
			foreach(char c in data){
				if((int)c > 127){ 
					return false;
				}
			}

			return true;
		}

		#endregion

	}
}
