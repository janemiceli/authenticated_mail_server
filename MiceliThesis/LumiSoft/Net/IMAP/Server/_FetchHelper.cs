using System;
using System.IO;
using LumiSoft.Net.Mime;

namespace LumiSoft.Net.IMAP.Server
{
	/// <summary>
	/// Summary description for FetchHelper.
	/// </summary>
	internal class FetchHelper
	{
		// ToDo: functions to accept MimeParser

		#region function ParseEnvelope

		public static string ParseEnvelope(byte[] data)
		{
			/* Rfc 3501 7.4.2
				ENVELOPE
				A parenthesized list that describes the envelope structure of a
				message.  This is computed by the server by parsing the
				[RFC-2822] header into the component parts, defaulting various
				fields as necessary.

				The fields of the envelope structure are in the following
				order: date, subject, from, sender, reply-to, to, cc, bcc,
				in-reply-to, and message-id.  The date, subject, in-reply-to,
				and message-id fields are strings.  The from, sender, reply-to,
				to, cc, and bcc fields are parenthesized lists of address
				structures.

				An address structure is a parenthesized list that describes an
				electronic mail address.  The fields of an address structure
				are in the following order: personal name, [SMTP]
				at-domain-list (source route), mailbox name, and host name.

				[RFC-2822] group syntax is indicated by a special form of
				address structure in which the host name field is NIL.  If the
				mailbox name field is also NIL, this is an end of group marker
				(semi-colon in RFC 822 syntax).  If the mailbox name field is
				non-NIL, this is a start of group marker, and the mailbox name
				field holds the group name phrase.

				If the Date, Subject, In-Reply-To, and Message-ID header lines
				are absent in the [RFC-2822] header, the corresponding member
				of the envelope is NIL; if these header lines are present but
				empty the corresponding member of the envelope is the empty
				string.
			*/
			// ((sender))
			// ENVELOPE ("date" "subject" from sender reply-to to cc bcc in-reply-to "messageID")

			MimeParser p = new MimeParser(data);

			string envelope  = "ENVELOPE (";
			
			// date
			envelope += "\"" + p.MessageDate.ToString("r",System.Globalization.DateTimeFormatInfo.InvariantInfo) + "\" ";
			
			// subject
			envelope += "\"" + p.Subject + "\" ";

			// from
			// ToDo: May be multiple senders
			LumiSoft.Net.Mime.Parser.eAddress adr = new LumiSoft.Net.Mime.Parser.eAddress(p.From);
			envelope += "((\"" + adr.Name + "\" NIL \"" + adr.Mailbox + "\" \"" + adr.Domain + "\")) ";

			// sender
			// ToDo: May be multiple senders
			envelope += "((\"" + adr.Name + "\" NIL \"" + adr.Mailbox + "\" \"" + adr.Domain + "\")) ";

			// reply-to
			string replyTo = MimeParser.ParseHeaderField("reply-to:",p.Headers);
			if(replyTo.Length > 0){
				envelope += "(";
				foreach(string recipient in replyTo.Split(';')){
					LumiSoft.Net.Mime.Parser.eAddress adrTo = new LumiSoft.Net.Mime.Parser.eAddress(recipient);
					envelope += "(\"" + adrTo.Name + "\" NIL \"" + adrTo.Mailbox + "\" \"" + adrTo.Domain + "\") ";
				}
				envelope = envelope.TrimEnd();
				envelope += ") ";
			}
			else{
				envelope += "NIL ";				
			}

			// to
			string[] to = p.To;
			envelope += "(";
			foreach(string recipient in to){
				LumiSoft.Net.Mime.Parser.eAddress adrTo = new LumiSoft.Net.Mime.Parser.eAddress(recipient);
				envelope += "(\"" + adrTo.Name + "\" NIL \"" + adrTo.Mailbox + "\" \"" + adrTo.Domain + "\") ";
			}
			envelope = envelope.TrimEnd();
			envelope += ") ";

			// cc
			string cc = MimeParser.ParseHeaderField("CC:",p.Headers);
			if(cc.Length > 0){
				envelope += "(";
				foreach(string recipient in cc.Split(';')){
					LumiSoft.Net.Mime.Parser.eAddress adrTo = new LumiSoft.Net.Mime.Parser.eAddress(recipient);
					envelope += "(\"" + adrTo.Name + "\" NIL \"" + adrTo.Mailbox + "\" \"" + adrTo.Domain + "\") ";
				}
				envelope = envelope.TrimEnd();
				envelope += ") ";
			}
			else{
				envelope += "NIL ";				
			}

			// bcc
			string bcc = MimeParser.ParseHeaderField("BCC:",p.Headers);
			if(bcc.Length > 0){
				envelope += "(";
				foreach(string recipient in bcc.Split(';')){
					LumiSoft.Net.Mime.Parser.eAddress adrTo = new LumiSoft.Net.Mime.Parser.eAddress(recipient);
					envelope += "(\"" + adrTo.Name + "\" NIL \"" + adrTo.Mailbox + "\" \"" + adrTo.Domain + "\") ";
				}
				envelope = envelope.TrimEnd();
				envelope += ") ";
			}
			else{
				envelope += "NIL ";				
			}

			// in-reply-to
			string inReplyTo = MimeParser.ParseHeaderField("in-reply-to:",p.Headers);
			if(inReplyTo.Length > 0){
				envelope += "\"" + inReplyTo + "\"";
			}
			else{
				envelope += "NIL ";
			}

			// message-id
			if(p.MessageID.Length > 0){
				envelope += "\"" + p.MessageID + "\"";
			}
			else{
				envelope += "NIL";
			}

			envelope += ")";

			return envelope;
		}

		#endregion

		#region function GetHeader

		public static byte[] GetHeader(byte[] data)
		{
			using(MemoryStream strm = new MemoryStream(data)){
				TextReader reader = (TextReader)new StreamReader(strm);				
				string headerStr = "";

				string line = reader.ReadLine();
				while(line != null){				
					// End of header reached
					if(line.Length == 0){
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
            using(MemoryStream mStrm = new MemoryStream(data)){
				TextReader r = new StreamReader(mStrm);
				string line = r.ReadLine();
				
				bool fieldFound = false;
				// Loop all header lines
				while(line != null){ 
					// End of header
					if(line.Length == 0){
						break;
					}

					// Field continues
					if(fieldFound && line.StartsWith("\t")){
						retVal += line + "\r\n";
					}
					else{
						fieldFound = false;

						// Check if wanted field
						foreach(string field in fields){
							if(line.Trim().ToLower().StartsWith(field.Trim().ToLower())){
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
            using(MemoryStream mStrm = new MemoryStream(data)){
				TextReader r = new StreamReader(mStrm);
				string line = r.ReadLine();
				
				bool fieldFound = false;
				// Loop all header lines
				while(line != null){ 
					// End of header
					if(line.Length == 0){
						break;
					}

					// Filed continues
					if(fieldFound && line.StartsWith("\t")){
						retVal += line + "\r\n";
					}
					else{
						fieldFound = false;

						// Check if wanted field
						foreach(string field in fields){
							if(line.Trim().ToLower().StartsWith(field.Trim().ToLower())){								
								fieldFound = true;
							}
						}

						if(!fieldFound){
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

		#region function ParseMimeEntry

		/// <summary>
		/// Returns requested mime entry data.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="mimeEntryNo"></param>
		/// <returns>Returns requested mime entry data or NULL if requested entri doesn't exist.</returns>
		public static byte[] ParseMimeEntry(byte[] message,int mimeEntryNo)
		{
			MimeParser p = new MimeParser(message);

			if(mimeEntryNo > 0 && mimeEntryNo <= p.MimeEntries.Count){
				return ((MimeEntry)p.MimeEntries[mimeEntryNo - 1]).Data;
			}

			return null;
		}

		#endregion

	}
}
