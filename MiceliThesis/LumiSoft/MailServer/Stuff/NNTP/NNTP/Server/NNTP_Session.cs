using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using LumiSoft.Net;
using LumiSoft.Net.NNTP;

namespace LumiSoft.Net.NNTP.Server
{	
	/// <summary>
	/// NNTP session.
	/// </summary>
	public class NNTP_Session 
	{
		private Socket          m_pClientSocket      = null;    // Referance to client Socket.
		private NNTP_Server     m_pNNTP_Server       = null;    // Referance to SMTP server.
		private string          m_SessionID          = "";      // Holds session ID.
		private string          m_UserName           = "";      // Holds loggedIn UserName.
		private string          m_ConnectedIp        = "";      // Holds connected computer's IP.
		private string          m_SelectedNewsGroup  = "";
		private NNTP_Articles   m_pArticles          = null; // !!!!
		private int             m_CurrentArticle     = -1;
		private bool            m_Authenticated      = false;   // Holds authentication flag.
		private int             m_BadCmdCount        = 0;       // Holds number of bad commands.
		private DateTime        m_SessionStartTime;
		private _LogWriter      m_pLogWriter         = null;
		private object          m_Tag                = null;
		private static          Regex _isNumber      = new Regex(@"^\d+$");


		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="clientSocket">Referance to socket.</param>
		/// <param name="server">Referance to NNTP server.</param>
		/// <param name="sessionID">Session ID which is assigned to this session.</param>
		/// <param name="logWriter">Log writer.</param>
		internal NNTP_Session(Socket clientSocket,NNTP_Server server,string sessionID,_LogWriter logWriter)
		{
			m_pClientSocket    = clientSocket;
			m_pNNTP_Server     = server;
			m_SessionID        = sessionID;
			m_pLogWriter       = logWriter;
			m_SessionStartTime = DateTime.Now;
		}



		#region function StartProcessing

		/// <summary>
		/// Starts session processing.
		/// </summary>
		public void StartProcessing()
		{								
			try
			{
				// Store client ip and host name.
				m_ConnectedIp = Core.ParseIP_from_EndPoint(m_pClientSocket.RemoteEndPoint.ToString());
				
				// Check if ip is allowed to connect this computer
				if(m_pNNTP_Server.OnValidate_IpAddress(this.RemoteEndPoint)){
 
					// Rfc 977 2.4.3
					//	200 server ready - posting allowed
					//	201 server ready - no posting allowed

					// Notify that server is ready
					SendData("200 " + System.Net.Dns.GetHostName() + " NNTP Service ready - posting allowed\r\n");
			
					//------ Create command loop --------------------------------//
					// Loop while QUIT cmd or Session TimeOut.
					long lastCmdTime = DateTime.Now.Ticks;
					string lastCmd  = "";
					while(true)
					{	
						// If there is any data available, begin command reading.			
						if(m_pClientSocket.Available > 0){
							try
							{
								lastCmd = ReadLine();
								if(SwitchCommand(lastCmd)){
									break;
								}
							}
							catch(ReadException rX){
								//---- Check that maximum bad commands count isn't exceeded ---------------//
								if(m_BadCmdCount > m_pNNTP_Server.MaxBadCommands-1){
									SendData("500 Too many bad commands, closing transmission channel\r\n");
									break;
								}
								m_BadCmdCount++;
								//-------------------------------------------------------------------------//

								switch(rX.ReadReplyCode){
									case ReadReplyCode.LengthExceeded:
										SendData("500 Line too long.\r\n");
										break;

									case ReadReplyCode.TimeOut:
										SendData("500 Command timeout.\r\n");
										break;

									case ReadReplyCode.UnKnownError:
										SendData("500 UnKnown Error.\r\n");

										m_pNNTP_Server.OnSysError(new Exception(rX.Message),new System.Diagnostics.StackTrace());										
										break;
								}
							}
							catch(Exception x){
								// Connection lost
								if(!m_pClientSocket.Connected){
									break;
								}
								
								SendData("500 Unkown temp error\r\n");
								m_pNNTP_Server.OnSysError(x,new System.Diagnostics.StackTrace());
							}

							// reset last command time
							lastCmdTime = DateTime.Now.Ticks;
						}
						else{
							//----- Session timeout stuff ------------------------------------------------//
							if(DateTime.Now.Ticks > lastCmdTime + ((long)(m_pNNTP_Server.SessionIdleTimeOut)) * 10000){						
								// Notify for closing
								SendData("500 Session timeout, closing transmission channel\r\n");
								break;
							}
						
							// Wait 100ms to save CPU, otherwise while loop may take 100% CPU. 
							Thread.Sleep(100);
							//---------------------------------------------------------------------------//
						}
					}
				}							
			}
			catch(ThreadInterruptedException e){
				string dummy = e.Message;
			}
			catch(Exception x){
				if(m_pClientSocket.Connected){
					SendData("500 Service not available, closing transmission channel\r\n");
					
					m_pNNTP_Server.OnSysError(x,new System.Diagnostics.StackTrace());
				}
				else{
					m_pLogWriter.AddEntry("Connection is aborted by client machine",this.SessionID,m_ConnectedIp,"x");
				}
			}
			finally{				
				m_pNNTP_Server.RemoveSession(this.SessionID,m_pLogWriter);
				
				// Write logs to log file, if needed
				if(m_pNNTP_Server.LogCommands){
					m_pLogWriter.Flush();
				}

				if(m_pClientSocket.Connected){
					m_pClientSocket.Close();
				}
			}
		}

		#endregion

		#region function SwitchCommand

		/// <summary>
		/// Executes NNTP command.
		/// </summary>
		/// <param name="NNTP_commandTxt">Original command text.</param>
		/// <returns>Returns true if must end session(command loop).</returns>
		private bool SwitchCommand(string NNTP_commandTxt)
		{			
			//---- Parse command --------------------------------------------------//
			string[] cmdParts = NNTP_commandTxt.TrimStart().Split(new char[]{' '});
			string command    = cmdParts[0].ToUpper().Trim();
			string argsText   = Core.GetArgsText(NNTP_commandTxt,command);
			//---------------------------------------------------------------------//

			switch(command){				
				case "ARTICLE":
					Article(argsText);
					break;
					
				case "BODY":
					Body(argsText);
					break;
						
				case "GROUP":
					Group(argsText);
					break;
						
				case "HEAD":
					Head(argsText);
					break;
						
				case "HELP":
					Help();
					break;
						
				case "IHAVE":
					IHave(argsText);
					break;
						
				case "LAST":
					Last();
					break;
						
				case "LIST":
					List();
					break;
						
				case "NEWGROUPS":
					NewGroups(argsText);
					break;
						
				case "NEWNEWS":	
					NewNews(argsText);
					break;
						
				case "NEXT":
					Next();
					break;
						
				case "POST":
					Post();
					break;
						
				case "QUIT":
					Quit();
					return true;
						
				case "STAT":
					Stat(argsText);
					break;

				case "XOVER":					
					XOVER(argsText);
					break;		
						
				default:					
					SendData("500 command unrecognized\r\n");

					//---- Check that maximum bad commands count isn't exceeded ---------------//
					if(m_BadCmdCount > m_pNNTP_Server.MaxBadCommands-1){
						SendData("500 Too many bad commands, closing transmission channel\r\n");
						return true;
					}
					m_BadCmdCount++;
					//-------------------------------------------------------------------------//
					break;				
			}
			
			return false;
		}

		#endregion

//
		#region function Body

		private void Body(string argsText)
		{
			/* Rfc 977
				BODY <message-id> or [nnn]
				Responses:
							412 no newsgroup has been selected
							420 no current article has been selected
							423 no such article number in this group
							430 no such article found	
				
				The HEAD and BODY commands are identical to the ARTICLE command
				except that they respectively return only the header lines or text
				body of the article.									

				(client wants to see the text body of the article)
				C:      BODY
				S:      222 10110 <23445@sdcsvax.ARPA> article retrieved - body
						follows (body text here)
				S:      .
			*/

			if(argsText == "")
			{
				argsText = m_CurrentArticle.ToString(); 			
			}			

			if(m_SelectedNewsGroup.Length == 0)
			{
				SendData("412 no newsgroup selected\r\n");
				return;
			}
			
			string article = m_pNNTP_Server.OnGetArticle(this,argsText);
			if(article != "")
			{
				MemoryStream msgStrm = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(article));
				string body = HeaderParser.ParseBody(msgStrm);
				SendData("222 " + argsText + " article retrieved - body follow\r\n");
				SendData(body);
				// Send end article
				SendData("\r\n.\r\n");
			}
			else
			{
				SendData("430 no such article found\r\n");
			}

		}

		#endregion

		#region function Head

		private void Head(string argsText)
		{
			/* Rfc 977
				HEAD <message-id> or [nnn]
				Responses:
							412 no newsgroup has been selected
							420 no current article has been selected
							423 no such article number in this group
							430 no such article found	

				The HEAD and BODY commands are identical to the ARTICLE command
				except that they respectively return only the header lines or text
				body of the article.													

				(client wants to see the text body of the article)
				C:      BODY
				S:      222 10110 <23445@sdcsvax.ARPA> article retrieved - body
						follows (body text here)
				S:      .
			*/

			if(argsText == "")
			{
				argsText = m_CurrentArticle.ToString(); 			
			}			

			if(m_SelectedNewsGroup.Length == 0)
			{
				SendData("412 no newsgroup selected\r\n");
				return;
			}
			
			string article = m_pNNTP_Server.OnGetArticle(this,argsText);
			if(article != "")
			{				
				string headers = System.Text.Encoding.ASCII.GetString(HeaderParser.GetHeader(System.Text.Encoding.ASCII.GetBytes(article)));
				SendData("222 " + argsText + " article retrieved - header follows\r\n");
				SendData(headers);
				// Send end article
				SendData("\r\n.\r\n");
			}
			else
			{
				SendData("430 no such article found\r\n");
			}

		}

		#endregion

		#region function Article

		private void Article(string argsText)
		{
			/* Rfc 977 3.1.2
			    ARTICLE <message-id> or [nnn]

				Responses:
							220 n <a> article retrieved - head and body follow
								(n = article number, <a> = message-id)
							221 n <a> article retrieved - head follows
							222 n <a> article retrieved - body follows
							223 n <a> article retrieved - request text separately
							412 no newsgroup has been selected
							420 no current article has been selected
							423 no such article number in this group
							430 no such article found
			
				Displays the header, a blank line, then the body (text) of the
				current or specified article.  The optional parameter nnn is the

				numeric id of an article in the current newsgroup and must be chosen
				from the range of articles provided when the newsgroup was selected.
				If it is omitted, the current article is assumed.

				The internally-maintained "current article pointer" is set by this
				command if a valid article number is specified.

				[the following applies to both forms of the article command.] A
				response indicating the current article number, a message-id string,
				and that text is to follow will be returned.

				The message-id string returned is an identification string contained
				within angle brackets ("<" and ">"), which is derived from the header
				of the article itself.  The Message-ID header line (required by
				RFC850) from the article must be used to supply this information. If
				the message-id header line is missing from the article, a single
				digit "0" (zero) should be supplied within the angle brackets.

				Since the message-id field is unique with each article, it may be
				used by a news reading program to skip duplicate displays of articles
				that have been posted more than once, or to more than one newsgroup.
			*/

			if(argsText == "")
			{
				argsText = m_CurrentArticle.ToString(); 			
			}
			else
			{
				if(IsNumeric(argsText))
				{
					m_CurrentArticle = Convert.ToInt32(argsText); 
				}
			}

			if(m_SelectedNewsGroup.Length == 0 && IsNumeric(argsText))
			{
				SendData("412 no newsgroup selected\r\n");
				return;
			}

			string article = m_pNNTP_Server.OnGetArticle(this,argsText);
			if(article != "")
			{
				SendData("220 " + argsText + " article retrieved - head and body follow\r\n");
				SendData(article);
				// Send end article
				SendData("\r\n.\r\n");
			}
			else
			{
				SendData("430 no such article found\r\n");
			}

		}

		#endregion

		#region function Stat

		private void Stat(string argsText)
		{
			/* Rfc 977 3.1.2
				STAT <message-id> or [nnn]

				Responses:
							220 n <a> article retrieved - head and body follow
								(n = article number, <a> = message-id)
							221 n <a> article retrieved - head follows
							222 n <a> article retrieved - body follows
							223 n <a> article retrieved - request text separately
							412 no newsgroup has been selected
							420 no current article has been selected
							423 no such article number in this group
							430 no such article found		

				The STAT command is similar to the ARTICLE command except that no
				text is returned.  When selecting by message number within a group,
				the STAT command serves to set the current article pointer without
				sending text. The returned acknowledgement response will contain the
				message-id, which may be of some value.  Using the STAT command to
				select by message-id is valid but of questionable value, since a
				selection by message-id does NOT alter the "current article pointer".							
				
			*/

			if(argsText == "")
			{
				argsText = m_CurrentArticle.ToString(); 			
			}
			else
			{
				if(NNTP_API.IsNumeric(argsText))
				{
					m_CurrentArticle = Convert.ToInt32(argsText); 
				}
			}

			if(m_SelectedNewsGroup.Length == 0)
			{
				SendData("412 no newsgroup selected\r\n");
				return;
			}

			string article = m_pNNTP_Server.OnGetArticle(this,argsText);
			if(article != "")
			{
				HeaderParser hp = new HeaderParser(System.Text.Encoding.ASCII.GetBytes(article));				 
				SendData("223 " + argsText + " " + hp.MessageID + " article retrieved\r\n");				
			}
			else
			{
				SendData("430 no such article found\r\n");
			}

		}

		#endregion

		#region function Group

		private void Group(string argsText)
		{
			/* Rfc 977 3.2.1
				GROUP ggg
				
				Responses:
							211 n f l s group selected
								(n = estimated number of articles in group,
								f = first article number in the group,
								l = last article number in the group,
								s = name of the group.)
							411 no such news group

				The required parameter ggg is the name of the newsgroup to be
				selected (e.g. "net.news").  A list of valid newsgroups may be
				obtained from the LIST command.

				The successful selection response will return the article numbers of
				the first and last articles in the group, and an estimate of the
				number of articles on file in the group.  It is not necessary that
				the estimate be correct, although that is helpful; it must only be
				equal to or larger than the actual number of articles on file.  (Some
				implementations will actually count the number of articles on file.
				Others will just subtract first article number from last to get an
				estimate.)

				When a valid group is selected by means of this command, the
				internally maintained "current article pointer" is set to the first
				article in the group.  If an invalid group is specified, the
				previously selected group and article remain selected.  If an empty
				newsgroup is selected, the "current article pointer" is in an
				indeterminate state and should not be used.

				Note that the name of the newsgroup is not case-dependent.  It must
				otherwise match a newsgroup obtained from the LIST command or an
				error will result. 
			*/

			NNTP_NewsGroup grp = m_pNNTP_Server.OnGetGroupInfo(this,argsText);
			   
			if(grp != null){
				m_SelectedNewsGroup = argsText;
				m_CurrentArticle    = (grp.ArticlesCount > 0) ? 1 : -1;
				//Load all messages for selected group...
				//This needs some testing with big groups (>10000 articles maybe) for performace
				NNTP_Articles_eArgs args = m_pNNTP_Server.OnGetXOVER(this,this.m_SelectedNewsGroup); 
				m_pArticles = new NNTP_Articles(); 
				m_pArticles = args.Articles;
								
				SendData("211 " + grp.ArticlesCount + " " + grp.FirstArticleNo + " " + grp.LastArticleNo + " " + argsText + " group selected\r\n");

			}
			else{
				SendData("411 no such news group\r\n");
			}
			
		}

		#endregion

		#region function Help

		private void Help()
		{
			/* Rfc 977 3.3.1
				Provides a short summary of commands that are understood by this
				implementation of the server. The help text will be presented as a
				textual response, terminated by a single period on a line by itself.
			*/

			SendData("100 Legal commands are :\r\n");
			SendData("article [MessageID|Number]\r\n");
			SendData("body [MessageID|Number]\r\n");
			SendData("group newsgroup\r\n");
			SendData("head [MessageID|Number]\r\n");
			SendData("help\r\n");
			SendData("ihave <message-id>\r\n");
			SendData("last\r\n");
			SendData("list [active|newsgroups[wildmat]|srchfields|searchable|prettynames[wildmat]]\r\n");
			SendData("newgroups yymmdd hhmmss [GMT]\r\n");
			SendData("newnews wildmat yymmdd hhmmss [GMT]\r\n");
			SendData("next\r\n");
			SendData("post\r\n");
			SendData("quit\r\n");
			SendData("stat [MessageID|number]\r\n");
			SendData("xover [range]\r\n");
			SendData(".\r\n");
		}

		#endregion
//
		#region function IHave

		private void IHave(string argsText)
		{
			/* Rfc 977 3.4.1
				IHAVE <messageid>
				
				Responses:
							235 article transferred ok
							335 send article to be transferred.  End with <CR-LF>.<CR-LF>
							435 article not wanted - do not send it
							436 transfer failed - try again later
							437 article rejected - do not try again

				The IHAVE command informs the server that the client has an article
				whose id is <messageid>.  If the server desires a copy of that
				article, it will return a response instructing the client to send the
				entire article.  If the server does not want the article (if, for
				example, the server already has a copy of it), a response indicating
				that the article is not wanted will be returned.

				If transmission of the article is requested, the client should send
				the entire article, including header and body, in the manner
				specified for text transmission from the server. A response code
				indicating success or failure of the transferral of the article will
				be returned.

				This function differs from the POST command in that it is intended
				for use in transferring already-posted articles between hosts.
				Normally it will not be used when the client is a personal
				newsreading program.  In particular, this function will invoke the
				server's news posting program with the appropriate settings (flags,
				options, etc) to indicate that the forthcoming article is being
				forwarded from another host.

				The server may, however, elect not to post or forward the article if
				after further examination of the article it deems it inappropriate to
				do so.  The 436 or 437 error codes may be returned as appropriate to
				the situation.

				Reasons for such subsequent rejection of an article may include such
				problems as inappropriate newsgroups or distributions, disk space
				limitations, article lengths, garbled headers, and the like.  These
				are typically restrictions enforced by the server host's news
				software and not necessarily the NNTP server itself.
			*/

			//must this check in all newsgroups???
			if(m_pArticles == null)
			{
				NNTP_Articles_eArgs args = m_pNNTP_Server.OnGetXOVER(this,this.m_SelectedNewsGroup); 
				m_pArticles = new NNTP_Articles(); 
				m_pArticles = args.Articles;
			}

			if(m_pArticles[argsText] == null)
			{
				SendData("335 send article to be transferred.  End with <CR-LF>.<CR-LF>\r\n");

				MemoryStream reply = null;
				ReadReplyCode replyCode = Core.ReadData(m_pClientSocket,out reply,null,m_pNNTP_Server.MaxMessageSize,m_pNNTP_Server.CommandIdleTimeOut,"\r\n.\r\n",".\r\n");
				if(replyCode == ReadReplyCode.Ok)
				{
					long recivedCount = reply.Length;
					//------- Do period handling and raise store event  --------//
					// If line starts with '.', mail client adds additional '.',
					// remove them.
					using(MemoryStream msgStream = Core.DoPeriodHandling(reply,false))
						  {
						reply.Close();
						// get list of newsgroups to post to
						string[] newsgroups = HeaderParser.ParseHeaderFields("Newsgroups:",msgStream.ToArray()).Split(Convert.ToChar(","));
						//get all the necessary headers
						HeaderParser hp = new HeaderParser(msgStream.ToArray());
						string   msgId   = m_pNNTP_Server.OnStoreArticle(this,msgStream,newsgroups);
						string   subject = hp.Subject; 
						string   from    = hp.From; 
						string   date    = hp.MessageDate.ToString("r",System.Globalization.DateTimeFormatInfo.InvariantInfo);
						string   refs    = hp.References; 
						string   lines   = hp.Lines;
						//Add msg to current article list

						if(m_pNNTP_Server.LogCommands)
						{
							m_pLogWriter.AddEntry("big binary " + recivedCount.ToString() + " bytes" + "",this.SessionID,m_ConnectedIp,"S");
						}

						SendData("240 Article posted successfully.\r\n");
					}
					//----------------------------------------------------------//									
				
				}
				else
				{
					if(replyCode == ReadReplyCode.LengthExceeded)
					{
						SendData("441 Requested action aborted: exceeded storage allocation\r\n");
					}
					else
					{
						SendData("441 Error message not terminated with '.'\r\n");						
					}
				}
			}
			else
			{
				SendData("435 article not wanted - do not send it\r\n");
			}





		}

		#endregion

		#region function Last

		private void Last()
		{
			/* Rfc 977 3.5.1
				Responses:
							223 n a article retrieved - request text separately
								(n = article number, a = unique article id)
							412 no newsgroup selected
							420 no current article has been selected
							421 no last article in this group
				
				The internally maintained "current article pointer" is set to the
				previous article in the current newsgroup.  If already positioned at
				the first article of the newsgroup, an error message is returned and
				the current article remains selected.

				The internally-maintained "current article pointer" is set by this
				command.

				A response indicating the current article number, and a message-id
				string will be returned.  No text is sent in response to this
				command.
			*/

			if(m_SelectedNewsGroup.Length == 0){
				SendData("412 no newsgroup selected\r\n");
				return;
			}
			if(m_CurrentArticle < 0){
				SendData("no current article has been selected\r\n");
				return;
			}
			if(m_CurrentArticle < 2){
				SendData("421 no last article in this group\r\n");
				return;
			}

			m_CurrentArticle--;

			NNTP_Article article = m_pArticles[m_CurrentArticle];

			SendData("223 " + article.Number + " " + article.ID + "article retrieved - request text separately\r\n");
		}

		#endregion

		#region function List

		private void List()
		{
			/* Rfc 977 3.6.1
				Responses:
							215 list of newsgroups follows
							
				Returns a list of valid newsgroups and associated information.  Each
				newsgroup is sent as a line of text in the following format:

					group last first p

				where <group> is the name of the newsgroup, <last> is the number of
				the last known article currently in that newsgroup, <first> is the
				number of the first article currently in the newsgroup, and <p> is
				either 'y' or 'n' indicating whether posting to this newsgroup is
				allowed ('y') or prohibited ('n').

				The <first> and <last> fields will always be numeric.  They may have
				leading zeros.  If the <last> field evaluates to less than the
				<first> field, there are no articles currently on file in the
				newsgroup.

				Note that posting may still be prohibited to a client even though the
				LIST command indicates that posting is permitted to a particular
				newsgroup. See the POST command for an explanation of client
				prohibitions.  The posting flag exists for each newsgroup because
				some newsgroups are moderated or are digests, and therefore cannot be
				posted to; that is, articles posted to them must be mailed to a
				moderator who will post them for the submitter.  This is independent
				of the posting permission granted to a client by the NNTP server.

				Please note that an empty list (i.e., the text body returned by this
				command consists only of the terminating period) is a possible valid
				response, and indicates that there are currently no valid newsgroups.
				
				Example:
							C: LIST
							S: 215 list of newsgroups follows
							S: net.wombats 00543 00501 y
							S: net.idiots 00100 00001 n
							S: .
			*/

			SendData("215 list of newsgroups follows\r\n");

			// Get and list newsgroups
			NNTP_NewsGroups groups = m_pNNTP_Server.OnGetNewsGroups(this);
			foreach(NNTP_NewsGroup group in groups.Newsgroups){
				SendData(group.Name + " " + group.LastArticleNo + " " + group.FirstArticleNo + " y " + "\r\n");
			}

			// Send list end
			SendData(".\r\n");
		}

		#endregion

		#region function NewGroups

		private void NewGroups(string argsText)
		{
			/* Rfc 977 3.7.1
				Responses:
							231 list of new newsgroups follows
							
				NEWGROUPS date time [GMT] [<distributions>]

				A list of newsgroups created since <date and time> will be listed in
				the same format as the LIST command.

				The date is sent as 6 digits in the format YYMMDD, where YY is the
				last two digits of the year, MM is the two digits of the month (with
				leading zero, if appropriate), and DD is the day of the month (with
				leading zero, if appropriate).  The closest century is assumed as
				part of the year (i.e., 86 specifies 1986, 30 specifies 2030, 99 is
				1999, 00 is 2000).

				Time must also be specified.  It must be as 6 digits HHMMSS with HH
				being hours on the 24-hour clock, MM minutes 00-59, and SS seconds
				00-59.  The time is assumed to be in the server's timezone unless the
				token "GMT" appears, in which case both time and date are evaluated
				at the 0 meridian.

				The optional parameter "distributions" is a list of distribution
				groups, enclosed in angle brackets.  If specified, the distribution
				portion of a new newsgroup (e.g, 'net' in 'net.wombat') will be
				examined for a match with the distribution categories listed, and
				only those new newsgroups which match will be listed.  If more than
				one distribution group is to be listed, they must be separated by
				commas within the angle brackets.

				Please note that an empty list (i.e., the text body returned by this
				command consists only of the terminating period) is a possible valid
				response, and indicates that there are currently no new newsgroups.
				
				Example:
							C: NEWGROUPS 850403 020000
							S: 231 New newsgroups since 03/04/85 02:00:00 follow
							S: net.music.gdead
							S: net.games.sources
							S: .
			*/

			
			string newsgroups = "";
			if(argsText.IndexOf("<") > -1){
				newsgroups = argsText.Substring(argsText.IndexOf("<") + 1,argsText.IndexOf(">") - argsText.IndexOf("<") - 1);
				argsText.Substring(0,argsText.IndexOf("<"));
			}

			string[] dateFormats = new string[]{"yyMMdd HHmmss","yyMMdd HHmmss 'GMT'"};
			DateTime since = DateTime.Today;
			since = DateTime.ParseExact(argsText,dateFormats,System.Globalization.DateTimeFormatInfo.InvariantInfo,System.Globalization.DateTimeStyles.AllowWhiteSpaces);
			

			SendData("231 list of new newsgroups follows\r\n");

			// Get and list newsgroups
			NNTP_NewsGroup[] groups = m_pNNTP_Server.OnGetNewsGroups(this).GetFilteredNewsgroups(since,newsgroups);
			foreach(NNTP_NewsGroup group in groups){
				SendData(group.Name + " " + group.LastArticleNo + " " + group.FirstArticleNo + "\r\n");
			}

			// Send list end
			SendData(".\r\n");
			
		}

		#endregion
//
		#region function NewNews

		private void NewNews(string argsText)
		{
			/* Rfc 977 3.8
				Responses:
							230 list of new articles by message-id follows
				
				NEWNEWS newsgroups date time [GMT] [<distribution>]

				A list of message-ids of articles posted or received to the specified
				newsgroup since "date" will be listed. The format of the listing will
				be one message-id per line, as though text were being sent.  A single
				line consisting solely of one period followed by CR-LF will terminate
				the list.

				Date and time are in the same format as the NEWGROUPS command.

				A newsgroup name containing a "*" (an asterisk) may be specified to
				broaden the article search to some or all newsgroups.  The asterisk
				will be extended to match any part of a newsgroup name (e.g.,
				net.micro* will match net.micro.wombat, net.micro.apple, etc). Thus
				if only an asterisk is given as the newsgroup name, all newsgroups
				will be searched for new news.

				(Please note that the asterisk "*" expansion is a general
				replacement; in particular, the specification of e.g., net.*.unix
				should be correctly expanded to embrace names such as net.wombat.unix
				and net.whocares.unix.)

				Conversely, if no asterisk appears in a given newsgroup name, only
				the specified newsgroup will be searched for new articles. Newsgroup
				names must be chosen from those returned in the listing of available
				groups.  Multiple newsgroup names (including a "*") may be specified
				in this command, separated by a comma.  No comma shall appear after
				the last newsgroup in the list.  [Implementors are cautioned to keep
				the 512 character command length limit in mind.]

				The exclamation point ("!") may be used to negate a match. This can
				be used to selectively omit certain newsgroups from an otherwise
				larger list.  For example, a newsgroups specification of
				"net.*,mod.*,!mod.map.*" would specify that all net.<anything> and
				all mod.<anything> EXCEPT mod.map.<anything> newsgroup names would be
				matched.  If used, the exclamation point must appear as the first
				character of the given newsgroup name or pattern.

				The optional parameter "distributions" is a list of distribution
				groups, enclosed in angle brackets.  If specified, the distribution
				portion of an article's newsgroup (e.g, 'net' in 'net.wombat') will
				be examined for a match with the distribution categories listed, and
				only those articles which have at least one newsgroup belonging to				
				the list of distributions will be listed.  If more than one
				distribution group is to be supplied, they must be separated by
				commas within the angle brackets.

				The use of the IHAVE, NEWNEWS, and NEWGROUPS commands to distribute
				news is discussed in an earlier part of this document.

				Please note that an empty list (i.e., the text body returned by this
				command consists only of the terminating period) is a possible valid
				response, and indicates that there is currently no new news.
			*/

			string newsgroups = "";
			if(argsText.IndexOf("<") > -1)
			{
				newsgroups = argsText.Substring(argsText.IndexOf("<") + 1,argsText.IndexOf(">") - argsText.IndexOf("<") - 1);
				argsText.Substring(0,argsText.IndexOf("<"));
			}

			newsgroups = argsText.Substring(0,argsText.IndexOf(" ")).Trim(); 
			argsText = argsText.Substring(argsText.IndexOf(" ")).Trim(); 
			//argsText = "20" + argsText.Substring(0,2) + "/" + argsText.Substring(2,2) + "/" +  argsText.Substring(4,2) +" " + argsText.Substring(7,2) + ":" + argsText.Substring(9,2) + ":" +  argsText.Substring(11,2);
			

			string[] dateFormats = new string[]{"yyMMdd HHmmss","yyMMdd HHmmss 'GMT'"};
			DateTime since = DateTime.Today;
			since = DateTime.ParseExact(argsText,dateFormats,System.Globalization.DateTimeFormatInfo.InvariantInfo,System.Globalization.DateTimeStyles.AllowWhiteSpaces);
	

			m_pArticles = new NNTP_Articles(); 
			m_pArticles = m_pNNTP_Server.OnGetNewNews(this,newsgroups,since).Articles; 		
			

			SendData("231 list of new articles follow\r\n");

			foreach(NNTP_Article article in m_pArticles.Articles)
			{
                SendData(article.ID + "\r\n"); 
			}
			// Send list end
			SendData(".\r\n");




		}

		#endregion

		#region function Next

		private void Next()
		{
			/* Rfc 977 3.9.1
				Responses:
							223 n a article retrieved - request text separately
								(n = article number, a = unique article id)
							412 no newsgroup selected
							420 no current article has been selected
							421 no next article in this group
			 			
				The internally maintained "current article pointer" is advanced to
				the next article in the current newsgroup.  If no more articles
				remain in the current group, an error message is returned and the
				current article remains selected.

				The internally-maintained "current article pointer" is set by this
				command.

				A response indicating the current article number, and the message-id
				string will be returned.  No text is sent in response to this
				command.
			*/

			if(m_SelectedNewsGroup.Length == 0){
				SendData("412 no newsgroup selected\r\n");
				return;
			}
			if(m_CurrentArticle < 0){
				SendData("no current article has been selected\r\n");
				return;
			}
			if(m_CurrentArticle + 1 > m_pArticles.Count){
				SendData("421 no next article in this group\r\n");
				return;
			}

			m_CurrentArticle++;
			NNTP_Article article = m_pArticles[m_CurrentArticle];

			SendData("223 " + article.Number + " " + article.ID + "article retrieved - request text separately\r\n");
		}

		#endregion
//
		#region function Post

		private void Post()
		{
			/* Rfc 977 3.10.1			
				Responses:
							240 article posted ok
							340 send article to be posted. End with <CR-LF>.<CR-LF>
							440 posting not allowed
							441 posting failed
				
				If posting is allowed, response code 340 is returned to indicate that
				the article to be posted should be sent. Response code 440 indicates
				that posting is prohibited for some installation-dependent reason.

				If posting is permitted, the article should be presented in the
				format specified by RFC850, and should include all required header
				lines. After the article's header and body have been completely sent
				by the client to the server, a further response code will be returned
				to indicate success or failure of the posting attempt.

				The text forming the header and body of the message to be posted
				should be sent by the client using the conventions for text received
				from the news server:  A single period (".") on a line indicates the
				end of the text, with lines starting with a period in the original
				text having that period doubled during transmission.

				No attempt shall be made by the server to filter characters, fold or
				limit lines, or otherwise process incoming text.  It is our intent
				that the server just pass the incoming message to be posted to the
				server installation's news posting software, which is separate from
				this specification.  See RFC850 for more details.

				Since most installations will want the client news program to allow
				the user to prepare his message using some sort of text editor, and
				transmit it to the server for posting only after it is composed, the
				client program should take note of the herald message that greeted it
				when the connection was first established. This message indicates
				whether postings from that client are permitted or not, and can be
				used to caution the user that his access is read-only if that is the
				case. This will prevent the user from wasting a good deal of time
				composing a message only to find posting of the message was denied.
				The method and determination of which clients and hosts may post is
				installation dependent and is not covered by this specification.
				
				Example:
							C: POST
							S: 340 Continue posting; Period on a line by itself to end
							C: (transmits news article in RFC850 format)
							C: .
							S: 240 Article posted successfully.
			*/

			SendData("340 Continue posting; Period on a line by itself to end\r\n");

			// Read message
			MemoryStream reply = null;
			ReadReplyCode replyCode = Core.ReadData(m_pClientSocket,out reply,null,m_pNNTP_Server.MaxMessageSize,m_pNNTP_Server.CommandIdleTimeOut,"\r\n.\r\n",".\r\n");
			if(replyCode == ReadReplyCode.Ok){
				long recivedCount = reply.Length;
				//------- Do period handling and raise store event  --------//
				// If line starts with '.', mail client adds additional '.',
				// remove them.
				using(MemoryStream msgStream = Core.DoPeriodHandling(reply,false)){
					reply.Close();
					// get list of newsgroups to post to
					string[] newsgroups = HeaderParser.ParseHeaderFields("Newsgroups:",msgStream.ToArray()).Split(Convert.ToChar(","));
					//get all the necessary headers
					HeaderParser hp = new HeaderParser(msgStream.ToArray());
					string   msgId   = m_pNNTP_Server.OnStoreArticle(this,msgStream,newsgroups);
					string   subject = hp.Subject; 
					string   from    = hp.From; 
					string   date    = hp.MessageDate.ToString("r",System.Globalization.DateTimeFormatInfo.InvariantInfo);
					string   refs    = hp.References; 
					string   lines   = hp.Lines;
					//Add msg to current article list

					if(m_pNNTP_Server.LogCommands){
						m_pLogWriter.AddEntry("big binary " + recivedCount.ToString() + " bytes" + "",this.SessionID,m_ConnectedIp,"S");
					}

					SendData("240 Article posted successfully.\r\n");
				}
				//----------------------------------------------------------//									
				
			}
			else{
				if(replyCode == ReadReplyCode.LengthExceeded){
					SendData("441 Requested action aborted: exceeded storage allocation\r\n");
				}
				else{
					SendData("441 Error message not terminated with '.'\r\n");						
				}
			}			
		}

		#endregion

		#region function Quit

		private void Quit()
		{
			/* Rfc 977 3.11.1 QUIT Command
				The server process acknowledges the QUIT command and then closes the
				connection to the client.  This is the preferred method for a client
				to indicate that it has finished all its transactions with the NNTP
				server.

				If a client simply disconnects (or the connection times out, or some
				other fault occurs), the server should gracefully cease its attempts
				to service the client.
				
				Example:
							C:  QUIT
							S:  205 goodbye.
			*/
			SendData("205 Service closing transmission channel\r\n");
		}

		#endregion


		//--- Extentions

		#region function Check

		private void Check()
		{
			/* Rfc 2980 1.1.1
			
				CHECK <message-id>
				
				Responses:
							238 no such article found, please send it to me
							400 not accepting articles
							431 try sending it again later
							438 already have it, please don't send it to me
							480 Transfer permission denied
							500 Command not understood

				CHECK is used by a peer to discover if the article with the specified
				message-id should be sent to the server using the TAKETHIS command.
				The peer does not have to wait for a response from the server before
				sending the next command.

				From using the responses to the sequence of CHECK commands, a list of
				articles to be sent can be constructed for subsequent use by the
				TAKETHIS command.

				The use of the CHECK command for streaming is optional.  Some
				implementations will directly use the TAKETHIS command and send all
				articles in the send queue on that peer for the server.

				On some implementations, the use of the CHECK command is not
				permitted when the server is in slave mode (via the SLAVE command).

				Responses that are of the form X3X must specify the message-id in the
				response.
			
			*/
/*
			if(m_pArticles == null)
			{
				NNTP_Articles_eArgs args = m_pNNTP_Server.OnGetXOVER(this,this.m_SelectedNewsGroup); 
				m_pArticles = new NNTP_Articles(); 
				m_pArticles = args.Articles;
			}

			if(m_pArticles[argsText] == null)
			{
				SendData("335 send article to be transferred.  End with <CR-LF>.<CR-LF>\r\n");

				MemoryStream reply = null;
				ReadReplyCode replyCode = Core.ReadData(m_pClientSocket,out reply,null,m_pNNTP_Server.MaxMessageSize,m_pNNTP_Server.CommandIdleTimeOut,"\r\n.\r\n",".\r\n");
				if(replyCode == ReadReplyCode.Ok)
				{
					long recivedCount = reply.Length;
					//------- Do period handling and raise store event  --------//
					// If line starts with '.', mail client adds additional '.',
					// remove them.
					using(MemoryStream msgStream = Core.DoPeriodHandling(reply,false))
					{
						reply.Close();
						// get list of newsgroups to post to
						string[] newsgroups = HeaderParser.ParseHeaderFields("Newsgroups:",msgStream.ToArray()).Split(Convert.ToChar(","));
						//get all the necessary headers
						HeaderParser hp = new HeaderParser(msgStream.ToArray());
						string   msgId   = m_pNNTP_Server.OnStoreArticle(this,msgStream,newsgroups);
						string   subject = hp.Subject; 
						string   from    = hp.From; 
						string   date    = hp.MessageDate.ToString("r",System.Globalization.DateTimeFormatInfo.InvariantInfo);
						string   refs    = hp.References; 
						string   lines   = hp.Lines;
						//Add msg to current article list

						if(m_pNNTP_Server.LogCommands)
						{
							m_pLogWriter.AddEntry("big binary " + recivedCount.ToString() + " bytes" + "",this.SessionID,m_ConnectedIp,"S");
						}

						SendData("240 Article posted successfully.\r\n");
					}
					//----------------------------------------------------------//									
				
				}
				else
				{
					if(replyCode == ReadReplyCode.LengthExceeded)
					{
						SendData("441 Requested action aborted: exceeded storage allocation\r\n");
					}
					else
					{
						SendData("441 Error message not terminated with '.'\r\n");						
					}
				}
			}
			else
			{
				SendData("435 article not wanted - do not send it\r\n");
			}
			*/
		}




		#endregion

		#region function XOVER

		private void XOVER(string argsText)
		{
			/*RFC 2980 2.8 XOVER
			
			XOVER [range]
			
			The XOVER command returns information from the overview database for
			the article(s) specified.  This command was originally suggested as
			part of the OVERVIEW work described in "The Design of a Common
			Newsgroup Overview Database for Newsreaders" by Geoff Collyer.  This
			document is distributed in the Cnews distribution.  The optional
			range argument may be any of the following:
			
						an article number
						an article number followed by a dash to indicate
							all following
						an article number followed by a dash followed by
							another article number
			
			If no argument is specified, then information from the current
			article is displayed.  Successful responses start with a 224 response
			followed by the overview information for all matched messages.  Once
			the output is complete, a period is sent on a line by itself.  If no
			argument is specified, the information for the current article is
			returned.  A news group must have been selected earlier, else a 412
			error response is returned.  If no articles are in the range
			specified, a 420 error response is returned by the server.  A 502
			response will be returned if the client only has permission to
			transfer articles.
			
			Each line of output will be formatted with the article number,
			followed by each of the headers in the overview database or the
			article itself (when the data is not available in the overview
			database) for that article separated by a tab character.  The
			sequence of fields must be in this order: subject, author, date,
			message-id, references, byte count, and line count.  Other optional
			fields may follow line count.  Other optional fields may follow line
			count.  These fields are specified by examining the response to the
			LIST OVERVIEW.FMT command.  Where no data exists, a null field must
			be provided (i.e. the output will have two tab characters adjacent to
			each other).  Servers should not output fields for articles that have
			been removed since the XOVER database was created.
			
			The LIST OVERVIEW.FMT command should be implemented if XOVER is
			implemented.  A client can use LIST OVERVIEW.FMT to determine what
			optional fields  and in which order all fields will be supplied by
			the XOVER command.  See Section 2.1.7 for more details about the LIST
			OVERVIEW.FMT command.*/

			//NNTP_Articles_eArgs args = OnGetXOVER(this,this.m_SelectedNewsGroup); 

			//NNTP_Articles artcls = args.Articles;

			try
			{
				SendData("224 data follows\r\n");

				string From = argsText.Substring(0,argsText.IndexOf("-",0));
				string cTo   = argsText.Substring(argsText.IndexOf("-",0)+1);

				int to = Convert.ToInt32(cTo);
				int from = Convert.ToInt32(From);

				if(m_pArticles == null)
				{
					NNTP_Articles_eArgs args = m_pNNTP_Server.OnGetXOVER(this,this.m_SelectedNewsGroup); 
					m_pArticles = new NNTP_Articles(); 
					m_pArticles = args.Articles;
				}
		
				for(int counter = from;counter <= to;counter++)
				{
				 
				
					SendData(m_pArticles[counter].Number  + "\t" + m_pArticles[counter].Subject + "\t" + m_pArticles[counter].Author + "\t" + m_pArticles[counter].Date 
						+ "\t" + m_pArticles[counter].ID + "\t" + m_pArticles[counter].References + "\t" + m_pArticles[counter].ByteCount + "\t" + m_pArticles[counter].Lines + "\r\n");
				}

				SendData(".\r\n");
			}
			catch(Exception x)
			{

			}
		}

		#endregion

		private void Mode_Stream()
		{
			/* Rfc 2980 1.2.1
			
				MODE STREAM
				
				Responses:
							203 Streaming is OK
							 500 Command not understood

				MODE STREAM is used by a peer to indicate to the server that it would
				like to suspend the lock step conversational nature of NNTP and send
				commands in streams.  This command should be used before TAKETHIS and
				CHECK.  See the section on the commands TAKETHIS and CHECK for more
				details.
			*/
		}

		
		#region function XReplic

		private void XReplic()
		{
			/* Rfc 2980 1.4.1
			
				XREPLIC ggg:nnn[,ggg:nnn...]
				
				Responses:
							235 article transferred ok
							335 send article to be transferred.  End with <CR-LF>.<CR-LF>
							435 article not wanted - do not send it
							436 transfer failed - try again later
							437 article rejected - do not try again

				The XREPLIC command makes is possible to exactly duplicate the news
				spool structure of one server in another server.  It first appeared
				in INN.

				This command works similarly to the IHAVE command as specified in RFC
				977.  The same response codes are used.  The command line arguments
				consist of entries separated by a single comma.  Each entry consists
				of a news group name, a colon, and an article number.  If the server
				responds with a 335 response, the article should be filed in the news
				group(s) and article number(s) specified in the XREPLIC command line.
				If the server cannot do successfully install the article once it has
				accepted it, a 436 or 437 response code can be used to indicate the
				failure.

				This command should only be used when the receiving server is being
				fed by only one other server.  It is likely that when used with
				servers that have multiple feeds that this command will frequently
				fail.
				
				XREPLIC slaving has been deprecated in INN version 1.7.2 and later.
				INN now has the ability to slave servers via transparent means,
				simply by having the article's Xref header transferred.  (In previous
				versions, this header was generated locally and stripped off on
				outgoing feeds.)

				It is likely that future versions of INN will no longer support
				XREPLIC.
			*/
		}


		#endregion

		#region function SendData
			
		/// <summary>
		/// Sends data to socket.
		/// </summary>
		/// <param name="data">String data which to send.</param>
		private void SendData(string data)
		{	
			byte[] byte_data = System.Text.Encoding.ASCII.GetBytes(data);
			
			int nCount = m_pClientSocket.Send(byte_data,byte_data.Length,0);	

			if(m_pNNTP_Server.LogCommands){
				data = data.Replace("\r\n","<CRLF>");
				m_pLogWriter.AddEntry(data,this.SessionID,m_ConnectedIp,"S");
			}

			// Remove ME:
			NNTPServer.AddDebugS("S:" + data);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="strm"></param>
		private void SendData(MemoryStream strm)
		{
			//---- split message to blocks -------------------------------//
			long totalSent = 0;
			while(strm.Position < strm.Length){
				int blockSize = 4024;
				byte[] dataBuf = new byte[blockSize];
				int nCount = strm.Read(dataBuf,0,blockSize);
				int countSended = m_pClientSocket.Send(dataBuf,nCount,SocketFlags.None);

				totalSent += countSended;

				if(countSended != nCount){
					strm.Position = totalSent;
				}
			}
			//-------------------------------------------------------------//

		//	if(m_pPOP3_Server.LogCommands){
		//		m_pLogWriter.AddEntry("big binary " + strm.Length.ToString() + " bytes",this.SessionID,this.IpStr,"S");
		//	}
		}

		#endregion

		#region function ReadLine

		/// <summary>
		/// Reads line from socket.
		/// </summary>
		/// <returns></returns>
		private string ReadLine()
		{
			string line = Core.ReadLine(m_pClientSocket,500,m_pNNTP_Server.CommandIdleTimeOut);
						
			if(m_pNNTP_Server.LogCommands){
				string lCmdTxt = line.Replace("\r\n","<CRLF>");
				m_pLogWriter.AddEntry(lCmdTxt,this.SessionID,m_ConnectedIp,"C");
			}

			// Remove ME:
			NNTPServer.AddDebugS("C:" + line);

			return line;
		}

		#endregion

		#region function ParseParams

		private string[] ParseParams(string argsText)
		{
			ArrayList p = new ArrayList();

			try
			{
				while(argsText.Length > 0){
					// Parameter is between ""
					if(argsText.StartsWith("\"")){
						p.Add(argsText.Substring(1,argsText.IndexOf("\"",1) - 1));
						// Remove parsed param
						argsText = argsText.Substring(argsText.IndexOf("\"",1) + 1).Trim();			
					}
					else{
						// Parameter is between ()
						if(argsText.StartsWith("(")){
							p.Add(argsText.Substring(1,argsText.LastIndexOf(")") - 1));
							// Remove parsed param
							argsText = argsText.Substring(argsText.LastIndexOf(")") + 1).Trim();
						}
						else{
							// Read parameter till " ", probably there is more params
							if(argsText.IndexOf(" ") > -1){
								p.Add(argsText.Substring(0,argsText.IndexOf(" ")));
								// Remove parsed param
								argsText = argsText.Substring(argsText.IndexOf(" ") + 1).Trim();
							}
							// This is last param
							else{
								p.Add(argsText);
								argsText = "";
							}
						}
					}
				}
			}
			catch{
			}

			string[] retVal = new string[p.Count];
			p.CopyTo(retVal);

			return retVal;
		}

		#endregion

		#region Properties Implementation
		
		/// <summary>
		/// Gets session ID.
		/// </summary>
		public string SessionID
		{
			get{ return m_SessionID; }
		}

		/// <summary>
		/// Gets if session authenticated.
		/// </summary>
		public bool Authenticated
		{
			get{ return m_Authenticated; }
		}

		/// <summary>
		/// Gets loggded in user name (session owner).
		/// </summary>
		public string UserName
		{
			get{ return m_UserName; }
		}

		/// <summary>
		/// Gets connected Host(client) EndPoint.
		/// </summary>
		public EndPoint RemoteEndPoint
		{
			get{ return m_pClientSocket.RemoteEndPoint; }
		}
		
		/// <summary>
		/// Gets local EndPoint which accepted client(connected host).
		/// </summary>
		public EndPoint LocalEndPoint
		{
			get{ return m_pClientSocket.LocalEndPoint; }
		}

		/// <summary>
		/// Gets session start time.
		/// </summary>
		public DateTime SessionStartTime
		{
			get{ return m_SessionStartTime; }
		}

		/// <summary>
		/// Gets or sets custom user data.
		/// </summary>
		public object Tag
		{
			get{ return m_Tag; }

			set{ m_Tag = value; }
		}
		/// <summary>
		/// Gets or Sets Selected NewsGroup.
		/// </summary>
		public string SelectedGroup
		{
			get{ return m_SelectedNewsGroup; }

			set{ m_SelectedNewsGroup = value; }
		}
		#endregion

		#region RegEx IsNumeric
		public static bool IsNumeric(string inputData)
		{
			Match m = _isNumber.Match(inputData);
			return m.Success;
		}

		#endregion


	}
}
