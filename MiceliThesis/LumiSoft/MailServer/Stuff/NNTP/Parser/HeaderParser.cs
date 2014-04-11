using System;
using System.IO;
using LumiSoft.Net.Mime;

namespace LumiSoft.Net.NNTP.Server
{
	/// <summary>
	/// Parses headers for a news article.
	/// </summary>
	public class HeaderParser
	{
		private string    m_Headers     = "";
		private string    m_From        = "";
		private string[]  m_Newsgroups  = null;
		private string    m_Subject     = "";
		private string    m_MsgID       = "";
		private string    m_References  = "";
		private string    m_Xref        = "";
		private string    m_Lines       = "";	
		private string    m_Body        = "";
		private DateTime  m_MsgDate;

		public HeaderParser(byte[] msg)
		{				
			MemoryStream msgStrm = new MemoryStream(msg);

			m_Headers = ParseHeaders(msgStrm);

			m_From        = ParseFrom(m_Headers);
			m_Newsgroups  = ParseNewsgroups(m_Headers);
			m_Subject     = ParseSubject(m_Headers);
			m_MsgDate     = ParseDate(m_Headers);
			m_MsgID       = ParseMessageID(m_Headers);
			m_References  = ParseReferences(m_Headers);
			m_Xref		  = ParseXRef(m_Headers);
			m_Lines       = ParseLines(m_Headers);	
								
		}

		#region function ParseHeaders

		/// <summary>
		/// Parses mime headers from message.
		/// </summary>
		/// <param name="msgStrm"></param>
		/// <returns></returns>
		private string ParseHeaders(MemoryStream msgStrm)
		{
			/*3.1.  GENERAL DESCRIPTION
			A message consists of header fields and, optionally, a body.
			The  body  is simply a sequence of lines containing ASCII charac-
			ters.  It is separated from the headers by a null line  (i.e.,  a
			line with nothing preceding the CRLF).
			*/

			string headers = "";

			TextReader r = new StreamReader(msgStrm);
			string line = r.ReadLine();

			while(line != null && line.Length != 0)
			{
				headers += line + "\r\n";

				line = r.ReadLine();
			}

			headers += "\r\n";

			return headers;
		}

		#endregion

		#region function ParseHeaders

		/// <summary>
		/// Parses body from message.
		/// </summary>
		/// <param name="msgStrm"></param>
		/// <returns></returns>
		public static string ParseBody(MemoryStream msgStrm)
		{
			string body    = "";
			TextReader r = new StreamReader(msgStrm);
			string line = r.ReadLine();

			while(line != null && line.Length != 0)
			{				
				line = r.ReadLine();
			}
			line = r.ReadLine();
			while(line != null && line.Length != 0)
			{
				body += line + "\r\n";
				line = r.ReadLine();
			}

			body += "\r\n";
			return body;
		}

		#endregion

		#region function ParseFrom

		/// <summary>
		/// Parse sender from message.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		private string ParseFrom(string headers)
		{
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers))))
				  {
				string line = r.ReadLine();

				while(line != null)
				{
					if(line.ToUpper().StartsWith("FROM:"))
					{						
						return CDecode(line.Substring(5).Trim());
					}

					line = r.ReadLine();
				}
			}

			return "";
		}

		#endregion

		#region function ParseNewsgroups

		private string[] ParseNewsgroups(string headers)
		{
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers))))
				  {
				string line = r.ReadLine();

				while(line != null)
				{
					if(line.ToUpper().StartsWith("TO:"))
					{
						line = line.Substring(3);

						string to = line;
						if(line.EndsWith(","))
						{
							to = "";
							while(line.EndsWith(","))
							{
								to += line;
								line = r.ReadLine();
							}
						}

						string[] tox = to.Split(new char[]{','});
						for(int i=0;i<tox.Length;i++)
						{
							tox[i] = CDecode(tox[i]);
						}

						return tox;
					}

					line = r.ReadLine();
				}
			}

			return null;
		}

		#endregion

		#region function ParseSubject

		/// <summary>
		/// Parses subject from message.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		private string ParseSubject(string headers)
		{
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers))))
				  {
				string line = r.ReadLine();

				while(line != null)
				{
					if(line.ToUpper().StartsWith("SUBJECT:"))
					{						
						return CDecode(line.Substring(8).Trim());
					}

					line = r.ReadLine();
				}
			}

			return "";
		}

		#endregion

		#region function ParseDate

		/// <summary>
		/// Parse message date.
		/// </summary>
		/// <param name="headers"></param>
		private DateTime ParseDate(string headers)
		{
			DateTime date = DateTime.Today;
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers))))
				  {
				string line = r.ReadLine();

				while(line != null)
				{
					if(line.ToUpper().StartsWith("DATE:"))
					{						
						date = ParseDateS(line.Substring(5).Trim()); 
						break;
					}
					line = r.ReadLine();					
				}
			}
			return date;
		}

		#endregion

		#region function ParseMessageID

		/// <summary>
		/// Parse message ID.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		private string ParseMessageID(string headers)
		{
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers))))
				  {
				string line = r.ReadLine();

				while(line != null)
				{
					if(line.ToUpper().StartsWith("MESSAGE-ID:"))
					{						
						return line.Substring(11).Trim();
					}

					line = r.ReadLine();
				}
			}

			return "";
		}

		#endregion

		#region function ParseReferences

		/// <summary>
		/// Parse References.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		private string ParseReferences(string headers)
		{
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers))))
			{
				string line = r.ReadLine();

				while(line != null)
				{
					if(line.ToUpper().StartsWith("REFERENCES:"))
					{						
						return line.Substring(11).Trim();
					}

					line = r.ReadLine();
				}
			}

			return "";
		}

		#endregion

		#region function ParseXRef

		/// <summary>
		/// Parse XRef.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		private string ParseXRef(string headers)
		{
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers))))
			{
				string line = r.ReadLine();

				while(line != null)
				{
					if(line.ToUpper().StartsWith("XREF:"))
					{						
						return line.Substring(5).Trim();
					}

					line = r.ReadLine();
				}
			}

			return "";
		}

		#endregion

		#region function ParseLines

		/// <summary>
		/// Parse Lines.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		private string ParseLines(string headers)
		{
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers))))
			{
				string line = r.ReadLine();

				while(line != null)
				{
					if(line.ToUpper().StartsWith("LINES:"))
					{						
						return line.Substring(6).Trim();
					}

					line = r.ReadLine();
				}
			}

			return "";
		}

		#endregion

		#region function GetHeader

		public static byte[] GetHeader(byte[] data)
		{
			using(MemoryStream strm = new MemoryStream(data))
				  {
				TextReader reader = (TextReader)new StreamReader(strm);				
				string headerStr = "";

				string line = reader.ReadLine();
				while(line != null)
				{				
					// End of header reached
					if(line.Length == 0)
					{
						break;
					}

					headerStr += line + "\r\n";

					line = reader.ReadLine();
				}
		
				return System.Text.Encoding.ASCII.GetBytes(headerStr);
			}
		}

		#endregion
        
		#region function ParseHeaderFields

		/// <summary>
		/// Returns requested header fields lines.
		/// </summary>
		/// <param name="fieldsStr">Header fields to get.</param>
		/// <param name="data">Message data.</param>
		/// <returns></returns>
		public static string ParseHeaderFields(string fieldsStr,byte[] data)
		{
			string retVal = "";

			string[] fields = fieldsStr.Split(' ');
			using(MemoryStream mStrm = new MemoryStream(data))
				  {
				TextReader r = new StreamReader(mStrm);
				string line = r.ReadLine();
				
				bool fieldFound = false;
				// Loop all header lines
				while(line != null)
				{ 
					// End of header
					if(line.Length == 0)
					{
						break;
					}

					// Filed continues
					if(fieldFound && line.StartsWith("\t"))
					{
						retVal += line + "\r\n";
					}
					else
					{
						fieldFound = false;

						// Check if wanted field
						foreach(string field in fields)
						{
							if(line.Trim().ToLower().StartsWith(field.Trim().ToLower()))
							{
								retVal += line + "\r\n";
								fieldFound = true;
							}
						}
					}

					line = r.ReadLine();
				}
			}

			return retVal;
		}

		#endregion

		#region function ParseHeaderFieldsNot

		/// <summary>
		/// Returns header fields lines except requested.
		/// </summary>
		/// <param name="fieldsStr">Header fields to skip.</param>
		/// <param name="data">Message data.</param>
		/// <returns></returns>
		public static string ParseHeaderFieldsNot(string fieldsStr,byte[] data)
		{
			string retVal = "";

			string[] fields = fieldsStr.Split(' ');
			using(MemoryStream mStrm = new MemoryStream(data))
				  {
				TextReader r = new StreamReader(mStrm);
				string line = r.ReadLine();
				
				bool fieldFound = false;
				// Loop all header lines
				while(line != null)
				{ 
					// End of header
					if(line.Length == 0)
					{
						break;
					}

					// Filed continues
					if(fieldFound && line.StartsWith("\t"))
					{
						retVal += line + "\r\n";
					}
					else
					{
						fieldFound = false;

						// Check if wanted field
						foreach(string field in fields)
						{
							if(line.Trim().ToLower().StartsWith(field.Trim().ToLower()))
							{								
								fieldFound = true;
							}
						}

						if(!fieldFound)
						{
							retVal += line + "\r\n";
						}
					}

					line = r.ReadLine();
				}
			}

			return retVal;
		}

		#endregion

		#region function ParseText

		/// <summary>
		/// Parses body text from message
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string ParseText(byte[] data)
		{
			MimeParser p = new MimeParser(data);

			return p.BodyText;
		}

		#endregion

		#region static function ParseDate

		/// <summary>
		/// Parses rfc2822 datetime.
		/// </summary>
		/// <param name="date">Date string</param>
		/// <returns></returns>
		public static DateTime ParseDateS(string date)
		{
			/*
			GMT  -0000
			EDT  -0400
			EST  -0500
			CDT  -0500
			CST  -0600
			MDT  -0600
			MST  -0700
			PDT  -0700
			PST  -0800
			*/

			try
			{

				date = date.Replace("GMT","-0000");
				date = date.Replace("EDT","-0400");
				date = date.Replace("EST","-0500");
				date = date.Replace("CDT","-0500");
				date = date.Replace("CST","-0600");
				date = date.Replace("MDT","-0600");
				date = date.Replace("MST","-0700");
				date = date.Replace("PDT","-0700");
				date = date.Replace("PST","-0800");
				date = date.Replace("UTC","");
				date = date.Replace("(UTC)","").Trim();
				date = date.Replace("(C-0500)","").Trim();
				date = date.Replace("(-0000)","").Trim();
				date = date.Replace("(-0400)","").Trim();
				date = date.Replace("(-0500)","").Trim();
				date = date.Replace("(-0600)","").Trim();
				date = date.Replace("(-0700)","").Trim();
				date = date.Replace("(-0800)","").Trim();
				date = date.Replace("()","").Trim();


				string[] formats = new string[]{
												   "r",
												   "ddd, d MMM yyyy HH':'mm':'ss zzz",
												   "ddd, dd MMM yyyy HH':'mm':'ss zzz",
												   "ddd, dd MMM yy HH':'mm':'ss zzz",
												   "dd MMM yyyy HH':'mm':'ss zzz",
												   "dd MMM yyyy HH':'mm':'ss",
												   "dd MMM yy HH':'mm':'ss zzz",
												   "ddd, dd MMM yyyy H':'mm':'ss zzz",
												   "dd'-'MMM'-'yyyy HH':'mm':'ss zzz"   
											   };

				return DateTime.ParseExact(date,formats,System.Globalization.DateTimeFormatInfo.InvariantInfo,System.Globalization.DateTimeStyles.None); 
			}
			catch(Exception x)
			{

				return DateTime.Today;
				

			}
		}

		#endregion

		#region function AddHeader

		/// <summary>
		/// Adds a header to a message.
		/// </summary>
		/// <param name="msgdata">Message data.</param>
		/// <param name="header">Name of header</param>
		/// <param name="data">value</param>		
		/// <returns></returns>

		public static MemoryStream AddHeader(MemoryStream msgdata,string header,string data)
		{			
			using(MemoryStream mStrm = new MemoryStream(msgdata.ToArray()))
			{
				string headers = "";
				bool   exists  = false;
				TextReader r = new StreamReader(mStrm);
				string line = r.ReadLine();
								
				// Loop all header lines
				while(line != null)
				{ 					
					// End of header
					if(line.Length == 0)
					{
						break;
					}
					
					headers += line + "\r\n";

					if(line.ToUpper().IndexOf(header.ToUpper()) > -1) //check if header not exists
					{
						exists = true;
					}
					line = r.ReadLine();
				}
				
				if(!exists)
				{
					headers += header + " " + data; //Add header
				}
				headers += "\r\n";				
				headers += "\r\n" +"\0" + "\r\n"+ r.ReadToEnd(); //Add null line,then rest of msg
				
				MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(headers));
				return ms;
			}

			
		}

		#endregion

		#region function CDecode

		private string CDecode(string data)
		{			
			if(data.IndexOf("=?") > -1)
			{
				int index = data.IndexOf("=?");

				string[] parts = data.Substring(index+2).Split(new char[]{'?'});
				
				string encoding = parts[0];
				string type     = parts[1];
				string datax    = parts[2];

				System.Text.Encoding enc = System.Text.Encoding.GetEncoding(encoding);
				if(type.ToUpper() == "Q")
				{
					return Core.QDecode(enc,datax);
				}

				if(type.ToUpper() == "B")
				{
					return enc.GetString(Convert.FromBase64String(datax));
				}				
			}

			return data;
		}

		#endregion

		#region Properties Implementation

		/// <summary>
		/// Gets message headers.
		/// </summary>
		public string Headers
		{
			get{ return m_Headers; }
		}

		/// <summary>
		/// Gets sender.
		/// </summary>
		public string From
		{
			get{ return m_From; }
		}

		/// <summary>
		/// Gets newsgroups.
		/// </summary>
		public string[] Newsgroups
		{
			get{ return m_Newsgroups; }
		}
		/// <summary>
		/// Gets msg body.
		/// </summary>
		public string Body
		{
			get{ return m_Body; }
		}

		/// <summary>
		/// Gets subject.
		/// </summary>
		public string Subject
		{
			get{ return m_Subject; }
		}
		/// <summary>
		/// Gets messageID.
		/// </summary>
		public string MessageID
		{
			get{ return m_MsgID; }
		}
		/// <summary>
		/// Gets References.
		/// </summary>
		public string References
		{
			get{ return m_References; }
		}
		/// <summary>
		/// Gets XReferences.
		/// </summary>
		public string XRef
		{
			get{ return m_Xref; }
		}

		/// <summary>
		/// Gets Lines.
		/// </summary>
		public string Lines
		{
			get{ return m_Lines; }
		}

		/// <summary>
		/// Gets message date.
		/// </summary>
		public DateTime MessageDate
		{
			get{ return m_MsgDate; }
		}

		#endregion
	}
}
