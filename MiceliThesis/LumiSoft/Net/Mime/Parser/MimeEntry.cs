using System;
using System.IO;
using System.Collections;
using System.Text;

namespace LumiSoft.Net.Mime
{
	/// <summary>
	/// Mime entry.
	/// </summary>
	public class MimeEntry
	{
		private string      m_Headers         = "";
		private string      m_ConentType      = "";
		private string      m_CharSet         = "";
		private string      m_ContentEncoding = "";
		private Disposition m_Disposition     = Disposition.Unknown;
		private string      m_FileName        = "";
		private byte[]      m_Data            = null;
		private ArrayList   m_Entries         = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="mimeEntry"></param>
		/// <param name="mime"></param>
		public MimeEntry(byte[] mimeEntry,MimeParser mime)
		{
			m_Headers         = ParseHeaders(mimeEntry);
			m_ConentType      = ParseContentType(m_Headers);

			// != multipart content (must be nested)
			if(m_ConentType.ToLower().IndexOf("multipart") == -1){
				m_CharSet         = ParseCharSet(m_Headers);
				m_ContentEncoding = ParseEncoding(m_Headers);
				m_FileName        = ParseFileName(m_Headers);
				m_Disposition     = ParseContentDisposition(m_Headers);
				
				m_Data = ParseData(System.Text.Encoding.Default.GetString(mimeEntry).Substring(m_Headers.Length + 2)); // 2-<CRLF>
			}
			else{ // Get nested entries
				string boundaryID = mime.ParseBoundaryID(m_Headers);
				m_Entries = mime.ParseEntries(new MemoryStream(mimeEntry),m_Headers.Length,boundaryID);
			}
		}

		#region function ParseHeaders

		/// <summary>
		/// Parses mime entry headers.
		/// </summary>
		/// <param name="mimeEntry"></param>
		/// <returns></returns>
		private string ParseHeaders(byte[] mimeEntry)
		{
			string headers = "";

			using(TextReader r = new StreamReader(new MemoryStream(mimeEntry))){
				string line = r.ReadLine();

				while(line != null && line.Length != 0){
					headers += line + "\r\n";

					line = r.ReadLine();
				}
			}

			return headers;
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

		#region function ParseCharSet

		/// <summary>
		/// Parse charset.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		private string ParseCharSet(string headers)
		{			
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers)))){
				string line = r.ReadLine();

				while(line != null){
					int index = line.ToUpper().IndexOf("CHARSET=");
					if(index > -1){	
						line = line.Substring(index + 8); // Remove charset=

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
				}
			}

			// If content type text and no encoding, consider it as ascii
			if(m_ConentType.ToLower().IndexOf("text/") > -1){
				return "ascii";
			}
			else{
				return "";
			}
		}

		#endregion

		#region function ParseEncoding

		/// <summary>
		/// Parse encoding.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		private string ParseEncoding(string headers)
		{
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers)))){
				string line = r.ReadLine();

				while(line != null){
					if(line.ToUpper().StartsWith("CONTENT-TRANSFER-ENCODING:")){	
						return line.Substring(26).Trim();
					}

					line = r.ReadLine();
				}
			}

			// If  no encoding, consider it as ascii
			return "7bit";
		}

		#endregion

		#region function ParseFileName

		/// <summary>
		/// Parse file name.
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		private string ParseFileName(string headers)
		{			
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers)))){
				string line = r.ReadLine();

				while(line != null){
					int index = line.ToUpper().IndexOf("FILENAME=");
					if(index > -1){	
						string retVal = line.Substring(index + 9);						
						return retVal.Substring(1,retVal.Length - 2);
					}

					line = r.ReadLine();
				}
			}

			return "";
		}

		#endregion

		#region function ParseContentDisposition

		private Disposition ParseContentDisposition(string headers)
		{
			using(TextReader r = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(headers)))){
				string line = r.ReadLine();

				while(line != null){
					if(line.ToUpper().StartsWith("CONTENT-DISPOSITION:")){	
						string dispos = line.Substring(20).Trim();

						if(dispos.ToUpper().IndexOf("ATTACHMENT") > -1){
							return Disposition.Attachment;
						}

						if(dispos.ToUpper().IndexOf("INLINE") > -1){
							return Disposition.Inline;
						}
					}

					line = r.ReadLine();
				}
			}

			return Disposition.Unknown;
		}

		#endregion


		#region function ParseData

		/// <summary>
		/// Parse entry data.
		/// </summary>
		/// <param name="mimeDataEntry"></param>
		/// <returns></returns>
		private byte[] ParseData(string mimeDataEntry)
		{
			switch(m_ContentEncoding.ToLower())
			{
				case "quoted-printable":
					return Encoding.GetEncoding(m_CharSet).GetBytes(Core.QDecode(Encoding.GetEncoding(m_CharSet),mimeDataEntry));

				case "7bit":
					return Encoding.ASCII.GetBytes(mimeDataEntry);

				case "8bit":
					return Encoding.GetEncoding(m_CharSet).GetBytes(mimeDataEntry);

				case "base64":	
					if(mimeDataEntry.Trim().Length > 0){
						return Convert.FromBase64String(mimeDataEntry);
					}
					else{
						return new byte[]{};
					}

				default:
					throw new Exception("Not supported content-encoding " + m_ContentEncoding + " !");
			}
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets content type.
		/// </summary>
		public string ContentType
		{
			get{ return m_ConentType; }
		}

		/// <summary>
		/// Gets content disposition type.
		/// </summary>
		public Disposition ContentDisposition
		{ 
			get{ return m_Disposition; }
		}

		/// <summary>
		/// Gets file name. NOTE: available only if ContentDisposition.Attachment.
		/// </summary>
		public string FileName
		{
			get{ return m_FileName; }
		}

		/// <summary>
		/// Gets mime entry data.
		/// </summary>
		public byte[] Data
		{
			get{ return m_Data; }
		}

		/// <summary>
		/// Gets string data. NOTE: available only if content-type=text/xxx.
		/// </summary>
		public string DataS
		{
			get{ return Encoding.GetEncoding(m_CharSet).GetString(m_Data); }
		}

		/// <summary>
		/// Gets nested mime entries.
		/// </summary>
		public ArrayList MimeEntries
		{
			get{ return m_Entries; }
		}

		#endregion

	}
}
