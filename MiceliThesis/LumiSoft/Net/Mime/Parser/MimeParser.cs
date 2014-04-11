using System;
using System.IO;
using System.Collections;

namespace LumiSoft.Net.Mime
{
	#region enum ContentDisposition

	/// <summary>
	/// Content disposition.
	/// </summary>
	public enum Disposition
	{
		/// <summary>
		/// Content is attachment.
		/// </summary>
		Attachment = 0,

		/// <summary>
		/// Content is embbed resource.
		/// </summary>
		Inline = 1,

		/// <summary>
		/// Content is unknown.
		/// </summary>
		Unknown = 40
	}

	#endregion

	/// <summary>
	/// Mime parser.
	/// </summary>
	public class MimeParser
	{		
		private string       m_Headers     = "";
		private string       m_BoundaryID  = "";
		private MemoryStream m_MsgStream   = null;
		private ArrayList    m_Entries     = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="msg">Mime message which to parse.</param>
		public MimeParser(byte[] msg)
		{
			m_MsgStream = new MemoryStream(msg);

			m_Headers    = ParseHeaders(m_MsgStream);
			m_BoundaryID = ParseBoundaryID(m_Headers);
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

			while(line != null && line.Length != 0){
				headers += line + "\r\n";

				line = r.ReadLine();
			}

			headers += "\r\n";

			return headers;
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
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers)))){
				string line = r.ReadLine();

				while(line != null){
					if(line.ToUpper().StartsWith("FROM:")){	
						LumiSoft.Net.Mime.Parser.eAddress e = new LumiSoft.Net.Mime.Parser.eAddress(line.Substring(5).Trim());
						
						return CDecode(e.Email);
					}

					line = r.ReadLine();
				}
			}

			return "";
		}

		#endregion

		#region function ParseAddress

		private string[] ParseAddress(string headers,string fieldName)
		{
			string toFieldValue = ParseHeaderField(fieldName,headers);
			
			string[] tox = toFieldValue.Split(new char[]{','});
			for(int i=0;i<tox.Length;i++){
				tox[i] = CDecode(tox[i]);
			}

			return tox;
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
			string subjectFieldValue = ParseHeaderField("SUBJECT:",headers);

			return CDecode(subjectFieldValue);
		}

		#endregion

		#region function ParseDate

		/// <summary>
		/// Parse message date.
		/// </summary>
		/// <param name="headers"></param>
		private DateTime ParseDate(string headers)
		{
			try{
				string date = ParseHeaderField("DATE:",headers);
				if(date.Length > 0){
					return ParseDateS(date);
				}
				else{
					return DateTime.Today;
				}
			}
			catch{
				return DateTime.Today;
			}
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
			return ParseHeaderField("MESSAGE-ID:",headers);
		}

		#endregion

		#region function ParseContentType

		/// <summary>
		/// Parse content type.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		private string ParseContentType(string headers)
		{
		//	return ParseHeaderField("CONTENT-TYPE:",headers);

			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers)))){
				string line = r.ReadLine();

				while(line != null){
					if(line.ToUpper().StartsWith("CONTENT-TYPE:")){	
						return line.Substring(13).Trim();
					}

					line = r.ReadLine();
				}
			}

			return "";
		}

		#endregion

		#region function ParseBoundaryID

		/// <summary>
		/// Parse boundaryID.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		internal string ParseBoundaryID(string headers)
		{
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.Default.GetBytes(headers)))){
				string line = r.ReadLine();

				while(line != null){
					int index = line.ToUpper().IndexOf("BOUNDARY=");
					if(index > -1){	
						line = line.Substring(index + 9); // Remove charset=

						// Charset may be in "" and without
						if(line.StartsWith("\"")){						
							return line.Substring(1,line.IndexOf("\"",1) - 1);
						}
						else{
							int endIndex = line.Length; 
							if(line.IndexOf(" ") > -1){
								endIndex = line.IndexOf(" ");
							}

							return line.Substring(0,endIndex);
						}						
					}

					line = r.ReadLine();

				/*	int index = line.ToUpper().IndexOf("BOUNDARY=");
					if(index > -1){	
						line = line.Substring(index + 10); // Remove BOUNDARY="
						return line.Substring(0,line.IndexOf("\""));
					}

					line = r.ReadLine();*/
				}
			}

			return "";
		}

		#endregion


		#region function ParseEntries

		/// <summary>
		/// Parses mime entries.
		/// </summary>
		/// <param name="msgStrm"></param>
		/// <param name="pos"></param>
		/// <param name="boundaryID"></param>
		internal ArrayList ParseEntries(MemoryStream msgStrm,int pos,string boundaryID)
		{
			ArrayList entries = null;
		
			// Entries are already parsed
			if(m_Entries != null){
				return m_Entries;
			}

			entries = new ArrayList();

			// If message doesn't have entries (simple text message).
			if(this.ContentType.ToLower().IndexOf("text/") > -1){
				entries.Add(new MimeEntry(msgStrm.ToArray(),this));
				m_Entries = entries;

				return m_Entries;
			}

			msgStrm.Position = pos;

			if(boundaryID.Length > 0){
				MemoryStream strmEntry = new MemoryStream();
				StreamLineReader reader = new StreamLineReader(msgStrm);
				byte[] lineData = reader.ReadLine();

				// Search first entry
				while(lineData != null){
					string line = System.Text.Encoding.Default.GetString(lineData);
					if(line.StartsWith("--" + boundaryID)){
						break;
					}
					
					lineData = reader.ReadLine();
				}

				// Start reading entries
				while(lineData != null){
					// Read entry data
					string line = System.Text.Encoding.Default.GetString(lineData);
					// Next boundary
					if(line.StartsWith("--" + boundaryID) && strmEntry.Length > 0){
						// Add Entry
						entries.Add(new MimeEntry(strmEntry.ToArray(),this));						
											
						strmEntry.SetLength(0);
					}
					else{
						strmEntry.Write(lineData,0,lineData.Length);
						strmEntry.Write(new byte[]{(byte)'\r',(byte)'\n'},0,2);
					}
						
					lineData = reader.ReadLine();
				}
			}

			return entries;
		}

		#endregion


		#region function CDecode

		private string CDecode(string data)
		{			
			if(data.IndexOf("=?") > -1){
				int index = data.IndexOf("=?");

				string[] parts = data.Substring(index+2).Split(new char[]{'?'});
				
				string encoding = parts[0];
				string type     = parts[1];
				string datax    = parts[2];

				System.Text.Encoding enc = System.Text.Encoding.GetEncoding(encoding);
				if(type.ToUpper() == "Q"){
					return Core.QDecode(enc,datax);
				}

				if(type.ToUpper() == "B"){
					return enc.GetString(Convert.FromBase64String(datax));
				}				
			}

			return data;
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

			date = date.Replace("GMT","-0000");
			date = date.Replace("EDT","-0400");
			date = date.Replace("EST","-0500");
			date = date.Replace("CDT","-0500");
			date = date.Replace("CST","-0600");
			date = date.Replace("MDT","-0600");
			date = date.Replace("MST","-0700");
			date = date.Replace("PDT","-0700");
			date = date.Replace("PST","-0800");

			string[] formats = new string[]{
				"r",
				"ddd, d MMM yyyy HH':'mm':'ss zzz",
				"ddd, dd MMM yyyy HH':'mm':'ss zzz",
				"dd'-'MMM'-'yyyy HH':'mm':'ss zzz",
				"d'-'MMM'-'yyyy HH':'mm':'ss zzz"
			};

			return DateTime.ParseExact(date.Trim(),formats,System.Globalization.DateTimeFormatInfo.InvariantInfo,System.Globalization.DateTimeStyles.None); 
		}

		#endregion

		#region static function ParseHeaderField

		/// <summary>
		/// Parse header specified header field.
		/// </summary>
		/// <param name="fieldName">Header field which to parse.</param>
		/// <param name="headers">Full headers.</param>
		public static string ParseHeaderField(string fieldName,string headers)
		{
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers)))){
				string line = r.ReadLine();

				while(line != null){
					// Find line where field begins
					if(line.ToUpper().StartsWith(fieldName.ToUpper())){
						// Remove field name and start reading value
						string fieldValue = line.Substring(fieldName.Length).Trim();

						// see if multi line value. NOTE: multi line value starts with <TAB> beginning of line.
						line = r.ReadLine();
						while(line.StartsWith("\t")){
							fieldValue += line.Trim();
							line = r.ReadLine();
						}

						return fieldValue;
					}

					line = r.ReadLine();
				}
			}

			return "";
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
			get{ return ParseFrom(m_Headers); }
		}

		/// <summary>
		/// Gets recipients.
		/// </summary>
		public string[] To
		{
			get{ return ParseAddress(m_Headers,"TO:"); }
		}

		/// <summary>
		/// Gets cc.
		/// </summary>
		public string[] Cc
		{
			get{ return ParseAddress(m_Headers,"CC:"); }
		}

		/// <summary>
		/// Gets bcc.
		/// </summary>
		public string[] Bcc
		{
			get{ return ParseAddress(m_Headers,"BCC:"); }
		}

		/// <summary>
		/// Gets subject.
		/// </summary>
		public string Subject
		{
			get{ return ParseSubject(m_Headers); }
		}

		/// <summary>
		/// Gets message body text.
		/// </summary>
		public string BodyText
		{
			get{
				m_Entries = ParseEntries(m_MsgStream,m_Headers.Length,m_BoundaryID);

				// Find first text entry
				foreach(MimeEntry ent in m_Entries){
					if(ent.MimeEntries != null){
						foreach(MimeEntry ent1 in ent.MimeEntries){
							if(ent1.MimeEntries != null){
								foreach(MimeEntry ent2 in ent1.MimeEntries){
									if(ent2.ContentType.ToUpper().IndexOf("TEXT/PLAIN") > -1){
										return ent2.DataS;
									}
								}
							}

							if(ent1.ContentType.ToUpper().IndexOf("TEXT/PLAIN") > -1){
								return ent1.DataS;
							}
						}
					}

					if(ent.ContentType.ToUpper().IndexOf("TEXT/PLAIN") > -1){
						return ent.DataS;
					}
				}
				return ""; 
			}
		}

		/// <summary>
		/// Gets messageID.
		/// </summary>
		public string MessageID
		{
			get{ return ParseMessageID(m_Headers); }
		}

		/// <summary>
		/// Gets messageID.
		/// </summary>
		public string ContentType
		{
			get{ return ParseContentType(m_Headers); }
		}

		/// <summary>
		/// Gets message date.
		/// </summary>
		public DateTime MessageDate
		{
			get{ return ParseDate(m_Headers); }
		}

		/// <summary>
		/// Gets mime entries.
		/// </summary>
		public ArrayList MimeEntries
		{
			get{ 
				m_Entries = ParseEntries(m_MsgStream,m_Headers.Length,m_BoundaryID);

				return m_Entries; 
			}
		}

		#endregion

	}
}
