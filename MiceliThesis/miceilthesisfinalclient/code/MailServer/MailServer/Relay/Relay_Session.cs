using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;

using LumiSoft.Net;
using LumiSoft.Net.SMTP.Client;
using LumiSoft.Net.Mime;

namespace LumiSoft.MailServer.Relay
{
    /// <summary>
    /// This class implements relay server session.
    /// </summary>
    public class Relay_Session
    {
        private Relay_Server     m_pRelayServer    = null;
        private string           m_MessageFile     = "";
		private bool             m_relay_retry     = false;
        private FileStream       m_pMessageStream  = null;
		private RelayInfo        m_pRelayInfo      = null;
        private string           m_SessionID       = "";
        private DateTime         m_SessionStartTime;
        private SmtpClientEx     m_pSmtpClient     = null;
        private Queue<IPAddress> m_pConnectPoints  = null;
        private IPEndPoint       m_pRemoteEndPoint = null;
        private bool             m_Ended           = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="server">Reference to owner relay server.</param>
        /// <param name="file">Message file to relay.</param>
		/// <param name="relay_retry">If true then first time relay, otherwise retry attempt.</param>
        public Relay_Session(Relay_Server server,string file,bool relay_retry)
        {
            m_pRelayServer = server;
            m_MessageFile  = file;
            m_relay_retry  = relay_retry;

            m_pMessageStream = File.Open(m_MessageFile,FileMode.Open,FileAccess.ReadWrite,FileShare.Read);
			m_pRelayInfo = new RelayInfo(m_pMessageStream,m_pRelayServer.UndeliveredAfter,m_pRelayServer.UndeliveredWarningAfter);
            m_SessionID = Guid.NewGuid().ToString();
            m_SessionStartTime = DateTime.Now;
            m_pSmtpClient = new SmtpClientEx();
			m_pSmtpClient.DnsServers = new string[]{m_pRelayServer.Dns1,m_pRelayServer.Dns2};
			if(m_pRelayServer.LogCommands){
                m_pSmtpClient.SessionLog += new LogEventHandler(OnSMTP_LogPart);
			}

            m_pConnectPoints = new Queue<IPAddress>();
        }
        
        #region method Dispose

        /// <summary>
        /// Clean up any resources beeing used.
        /// </summary>
        public void Dispose()
        {               
            if(m_pMessageStream != null){
                try{
                    m_pMessageStream.Dispose();
                }
                catch{
                }
                m_pMessageStream = null;
            }
            if(m_pSmtpClient != null){
                try{
                    m_pSmtpClient.Dispose();
                    m_pSmtpClient = null;
                }
                catch{
                }
            }
            m_pRemoteEndPoint = null;
        }

        #endregion


        #region method Start

        /// <summary>
        /// Starts relay session processing.
        /// </summary>
        public void Start()
        {
            try{               
				// Forward host is specified, use it
				if(m_pRelayInfo.ForwardHost.Length > 0){
                    string[] host_port = m_pRelayInfo.ForwardHost.Split(':');
                    int port = 25;
                    // SMTP port specified
                    if(host_port.Length == 2){
                        port = Convert.ToInt32(host_port[1]);
                    }

                    m_pSmtpClient.BeginConnect(new IPEndPoint(m_pRelayServer.SendingIP,0),host_port[0],port,false,new CommadCompleted(this.ConnectCompleted));		
				}
				// Smart host enabled, use it
				else if(m_pRelayServer.UseSmartHost){
					m_pSmtpClient.BeginConnect(new IPEndPoint(m_pRelayServer.SendingIP,0),m_pRelayServer.SmartHost,m_pRelayServer.SmartHostPort,m_pRelayServer.SmartHostUseSSL,new CommadCompleted(this.ConnectCompleted));
				}
				// Use direct delivery
				else{
                    // Get all posssible destination IPs (MX records,A).
                    foreach(IPAddress ip in m_pSmtpClient.GetDestinations(m_pRelayInfo.To)){
                        m_pConnectPoints.Enqueue(ip);
                    }

                    // We din't get any connect point, ... that never should happen.
                    if(m_pConnectPoints.Count == 0){                        
                        End(false,new Exception("Failed to get any destination IP address, invalid email domain name '" + m_pRelayInfo.To + "' or destination dns down."));
                        return;
                    }
                 
                    // Get next available destination connection point.
                    m_pRemoteEndPoint = GetConnectionPoint();

                    // All connections to specified domain to all available IP addresses exceeded.
                    if(m_pRemoteEndPoint == null){
                        End(false,new Exception("All IP addresses maximum allowed connections are exceeded for email domain '" + m_pRelayInfo.To + "'."));
                        return;
                    }

					m_pSmtpClient.BeginConnect(new IPEndPoint(m_pRelayServer.SendingIP,0),m_pRemoteEndPoint.Address.ToString(),25,new CommadCompleted(this.ConnectCompleted));
				}
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			
				End(false,x);
			}
        }

        #endregion

        #region method End

		/// <summary>
		/// Ends RelaySession, does clean up.
		/// </summary>
		/// <param name="sendOk">Specifies if message sent ok.</param>
		/// <param name="exception">If sendOk=false, then exception is filled with error info.</param>
		private void End(bool sendOk,Exception exception)
		{	
			// This method may be called multiple times, if session timed out.  Just block it.
            lock(this){
			    if(m_Ended){
				    return;
			    }
			    m_Ended = true;
            }

			try{                        
                // If exception and logging enabled, log exception.
                try{
                    if(m_pSmtpClient.SessionActiveLog != null && exception != null){
                        m_pSmtpClient.SessionActiveLog.AddTextEntry("Exception: " + exception.Message);                    
                    }
                }
                catch{
                    // FIX ME: this returns error here if smtp client not connected and exception, because
                    // no logger created yet.
                }

				if(sendOk){
					Dispose();

					// Message sended successfuly, may delete it
					File.Delete(m_MessageFile);
				}
				else if(m_relay_retry){
					Dispose();

					// Move message to Retry folder.
					string msgFileName = Path.GetFileName(m_MessageFile);                    
                    string retryFolder = API_Utlis.EnsureFolder(m_pRelayServer.VirtualServer.MailStorePath + "Retry");
					File.Move(m_MessageFile,API_Utlis.PathFix(retryFolder + "\\" + msgFileName));
				}
				else if(!m_relay_retry){
					string error     = exception.Message;
					bool   permError = error.StartsWith("5"); // SMTP permanent errors 5xx
			
					// If destination recipient is invalid or Undelivered Date Exceeded, try to return message to sender
					if(permError || m_pRelayInfo.IsUndeliveredDateExceeded){							
						MakeUndeliveredNotify(m_pRelayInfo,error,m_pMessageStream);

						Dispose();

						// Undelivery note made, may delete message
						if(m_pRelayInfo.From.Length > 0){
							File.Delete(m_MessageFile);
						}
						// There isn't return address, can't send undelivery note
						else if(m_pRelayServer.StoreUndeliveredMessages){
							// Check if Directory exists, if not Create
							if(!Directory.Exists(m_pRelayServer.VirtualServer.MailStorePath + "Undelivered\\")){
								Directory.CreateDirectory(m_pRelayServer.VirtualServer.MailStorePath + "Undelivered\\");
							}
							File.Move(m_MessageFile,m_MessageFile.Replace("Retry","Undelivered"));
						}
					}
					else if(m_pRelayInfo.MustSendWarning){
						MakeUndeliveredWarning(m_pRelayInfo,error,m_pMessageStream);

						byte[] mustSendWarningBit = System.Text.Encoding.ASCII.GetBytes("1");
						m_pMessageStream.Position = m_pRelayInfo.WarningBitPos;
						m_pMessageStream.Write(mustSendWarningBit,0,mustSendWarningBit.Length);	
						
						Dispose();
					}
				
					Dispose();
				}
			}
			catch(Exception x){	
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
			finally{
				m_pRelayServer.RemoveSession(this);                
                m_pRelayServer = null;
			}
		}

		#endregion

        #region method Kill

        /// <summary>
        /// Kills this session.
        /// </summary>
        /// <param name="text">Text to reply to connected server.</param>
        public void Kill(string text)
        {
            if(m_pSmtpClient != null && m_pSmtpClient.SessionActiveLog != null){
                m_pSmtpClient.SessionActiveLog.AddTextEntry(text);
            }

			End(false,new Exception(text));
        }

        #endregion


        #region method OnSMTP_LogPart

        private void OnSMTP_LogPart(object sender,Log_EventArgs e)
        {            
            Logger.WriteLog(m_pRelayServer.LogsPath + "relay-" + DateTime.Today.ToString("yyyyMMdd") + ".log",e.Logger);
        }

        #endregion


        #region method ConnectCompleted

		/// <summary>
		/// Is called when Connect completed.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="exception"></param>
		private void ConnectCompleted(LumiSoft.Net.SocketCallBackResult result,Exception exception)
		{
			try{
				if(LumiSoft.Net.SocketCallBackResult.Ok == result){
                    m_pRemoteEndPoint = (IPEndPoint)m_pSmtpClient.RemoteEndPoint;

					m_pSmtpClient.BeginEhlo(m_pRelayServer.HostName,new CommadCompleted(this.EhloCompleted));
				}
				else if(exception != null){
                    // Get next available destination connection point.
                    m_pRemoteEndPoint = GetConnectionPoint();

                    // We have next available connection point, try next.
                    if(m_pRemoteEndPoint != null){                        
                        m_pSmtpClient.BeginConnect(new IPEndPoint(m_pRelayServer.SendingIP,0),m_pRemoteEndPoint.Address.ToString(),25,new CommadCompleted(this.ConnectCompleted));
                    }
                    else{
					    End(false,exception);
                    }
				}
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
				
				End(false,x);
			}
		}

		#endregion

		#region method EhloCompleted

		/// <summary>
		/// Is called when EHLO is completed.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="exception"></param>
		private void EhloCompleted(LumiSoft.Net.SocketCallBackResult result,Exception exception)
		{
			try{
				if(LumiSoft.Net.SocketCallBackResult.Ok == result)
                {
					if(m_pRelayServer.UseSmartHost && m_pRelayServer.SmartHostUserName.Length > 0){
						m_pSmtpClient.BeginAuthenticate(m_pRelayServer.SmartHostUserName,m_pRelayServer.SmartHostPassword,new CommadCompleted(this.AuthenticationCompleted));
					}
					else
                    {
                        m_pSmtpClient.sendcertificate(); //miceli
						m_pSmtpClient.BeginSetSender(m_pRelayInfo.From,m_pMessageStream.Length - m_pMessageStream.Position,new CommadCompleted(this.SetSenderCompleted));
					}
				}
				else if(exception != null){
					End(false,exception);
				}
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
				
				End(false,x);
			}
		}

		#endregion

		#region method AuthenticationCompleted

		/// <summary>
		/// Is called when authentication is completed.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="exception"></param>
		private void AuthenticationCompleted(LumiSoft.Net.SocketCallBackResult result,Exception exception)
		{
			try{
				if(LumiSoft.Net.SocketCallBackResult.Ok == result){
					m_pSmtpClient.BeginSetSender(m_pRelayInfo.From,-1,new CommadCompleted(this.SetSenderCompleted));
				}
				else if(exception != null){
					End(false,exception);
				}
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
				
				End(false,exception);
			}
		}

		#endregion

		#region method SetSenderCompleted

		/// <summary>
		/// Is called SetSender completed.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="exception"></param>
		private void SetSenderCompleted(LumiSoft.Net.SocketCallBackResult result,Exception exception)
		{
			try{
				if(LumiSoft.Net.SocketCallBackResult.Ok == result){
					m_pSmtpClient.BeginAddRecipient(m_pRelayInfo.To,new CommadCompleted(this.SetRecipientCompleted));
				}
				else if(exception != null){
					End(false,exception);
				}
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
				
				End(false,x);
			}
		}

		#endregion

		#region method SetRecipientCompleted

		/// <summary>
		/// Is called when SetRecipient completed.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="exception"></param>
		private void SetRecipientCompleted(LumiSoft.Net.SocketCallBackResult result,Exception exception)
		{
			try{
				if(LumiSoft.Net.SocketCallBackResult.Ok == result){
					m_pSmtpClient.BeginSendMessage(m_pMessageStream,new CommadCompleted(this.MessageSendingCompleted));                                    
				}
				else if(exception != null){
					End(false,exception);
				}
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
				
				End(false,x);
			}
		}

		#endregion

		#region method MessageSendingCompleted

		/// <summary>
		/// Is called when MessageSending completed.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="exception"></param>
		private void MessageSendingCompleted(LumiSoft.Net.SocketCallBackResult result,Exception exception)
		{
			try{
				if(LumiSoft.Net.SocketCallBackResult.Ok == result){
					End(true,null);
				}
				else if(exception != null){
					End(false,exception);
				}
			}
			catch(Exception x){				
				End(false,x);
			}
		}

		#endregion

        
		#region method MakeUndeliveredNotify

		/// <summary>
		/// Creates undelivered notify for user and places it to relay folder.
		/// </summary>
		/// <param name="relayInfo">Relay info</param>
		/// <param name="error">SMTP returned error text.</param>
		/// <param name="file">Messsage file.</param>
		private void MakeUndeliveredNotify(RelayInfo relayInfo,string error,Stream file)
		{
			try{
				// If sender isn't specified, we can't send undelivery notify to sender.
				// Just skip undelivery notify sending.
				if(relayInfo.From.Length == 0){
					return;
				}

				file.Position = relayInfo.MessageStartPos;
                RelayVariablesManager variablesMgr = new RelayVariablesManager(this,error,file);
                file.Position = relayInfo.MessageStartPos;

                ServerReturnMessage messageTemplate = m_pRelayServer.UndeliveredMessage;
                if(messageTemplate == null){
                    string bodyRtf = "" +
                    "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fswiss\\fprq2\\fcharset0 Verdana;}{\\f1\\fswiss\\fprq2\\fcharset186 Verdana;}{\\f2\\fnil\\fcharset0 Verdana;}{\\f3\\fnil\\fcharset186 Verdana;}{\\f4\\fswiss\\fcharset0 Arial;}{\\f5\\fswiss\\fcharset186{\\*\\fname Arial;}Arial Baltic;}}\r\n" +
                    "{\\colortbl ;\\red0\\green64\\blue128;\\red255\\green0\\blue0;\\red0\\green128\\blue0;}\r\n" +
                    "\\viewkind4\\uc1\\pard\\f0\\fs20 Your message t\\lang1061\\f1 o \\cf1\\lang1033\\f2 <#relay.to>\\cf0\\f0 , dated \\b\\fs16 <#message.header[\"Date:\"]>\\b0\\fs20 , could not be delivered.\\par\r\n" +
                    "\\par\r\n" +
                    "Recipient \\i <#relay.to>'\\i0 s Server (\\cf1 <#relay.session_hostname>\\cf0 ) returned the following response: \\cf2 <#relay.error>\\cf0\\par\r\n" +
                    "\\par\r\n" +
                    "\\par\r\n" +
                    "\\b * Server will not attempt to deliver this message anymore\\b0 .\\par\r\n" +
                    "\\par\r\n" +
                    "--------\\par\r\n" +
                    "\\par\r\n" +
                    "Your original message is attached to this e-mail (\\b data.eml\\b0 )\\par\r\n" +
                    "\\par\r\n" +
                    "\\fs16 The tracking number for this message is \\cf3 <#relay.session_messageid>\\cf0\\fs20\\par\r\n" +
                    "\\lang1061\\f5\\par\r\n" +
                    "\\lang1033\\f2\\par\r\n" +
                    "}\r\n";

                    messageTemplate = new ServerReturnMessage("Undelivered notice: <#message.header[\"Subject:\"]>",bodyRtf);
                }

				// Make new message
				Mime m = new Mime();
				MimeEntity mainEntity = m.MainEntity;
				// Force to create From: header field
				mainEntity.From = new AddressList();
				mainEntity.From.Add(new MailboxAddress("postmaster"));
				// Force to create To: header field
				mainEntity.To = new AddressList();
				mainEntity.To.Add(new MailboxAddress(relayInfo.From));
				mainEntity.Subject = variablesMgr.Process(messageTemplate.Subject);
				mainEntity.ContentType = MediaType_enum.Multipart_mixed;

                string rtf = variablesMgr.Process(messageTemplate.BodyTextRtf);

                MimeEntity multipartAlternativeEntity = mainEntity.ChildEntities.Add();
                multipartAlternativeEntity.ContentType = MediaType_enum.Multipart_alternative;

				MimeEntity textEntity = multipartAlternativeEntity.ChildEntities.Add();
				textEntity.ContentType = MediaType_enum.Text_plain;
				textEntity.ContentTransferEncoding = ContentTransferEncoding_enum.QuotedPrintable;
				textEntity.DataText = SCore.RtfToText(rtf);
                                
                MimeEntity htmlEntity = multipartAlternativeEntity.ChildEntities.Add();
				htmlEntity.ContentType = MediaType_enum.Text_html;
				htmlEntity.ContentTransferEncoding = ContentTransferEncoding_enum.QuotedPrintable;
				htmlEntity.DataText = SCore.RtfToHtml(rtf);

				MimeEntity attachmentEntity = mainEntity.ChildEntities.Add();
				attachmentEntity.ContentType = MediaType_enum.Application_octet_stream;
				attachmentEntity.ContentDisposition = ContentDisposition_enum.Attachment;
				attachmentEntity.ContentTransferEncoding = ContentTransferEncoding_enum.Base64;
				attachmentEntity.ContentDisposition_FileName = "data.eml";
				attachmentEntity.DataFromStream(file);

				using(MemoryStream strm = new MemoryStream()){
					m.ToStream(strm);
					m_pRelayServer.VirtualServer.ProcessAndStoreMessage("",new string[]{relayInfo.From},strm,null);
				}
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region method MakeUndeliveredWarning

		/// <summary>
		/// Creates undelivered warning for user and places it to relay folder.
		/// </summary>
		/// <param name="relayInfo">Relay info</param>
		/// <param name="error">SMTP returned error text.</param>
		/// <param name="file">Messsage file.</param>
		private void MakeUndeliveredWarning(RelayInfo relayInfo,string error,Stream file)
		{	
			try{
				// If sender isn't specified, we can't send warning to sender.
				// Just skip warning sending.
				if(relayInfo.From.Length == 0){
					return;
				}

				file.Position = relayInfo.MessageStartPos;
                RelayVariablesManager variablesMgr = new RelayVariablesManager(this,error,file);
                file.Position = relayInfo.MessageStartPos;

                ServerReturnMessage messageTemplate = m_pRelayServer.UndeliveredMessage;
                if(messageTemplate == null){
                    string bodyRtf = "" +
                    "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Verdana;}{\\f1\\fnil\\fcharset186 Verdana;}{\\f2\\fswiss\\fcharset186{\\*\\fname Arial;}Arial Baltic;}{\\f3\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n" +
                    "{\\colortbl ;\\red0\\green64\\blue128;\\red255\\green0\\blue0;\\red0\\green128\\blue0;}\r\n" +
                    "\\viewkind4\\uc1\\pard\\f0\\fs20 This e-mail is generated by the Server(\\cf1 <#relay.hostname>\\cf0 )  to notify you, \\par\r\n" +
                    "\\lang1061\\f1 that \\lang1033\\f0 your message to \\cf1 <#relay.to>\\cf0  dated \\b\\fs16 <#message.header[\"Date:\"]>\\b0  \\fs20 could not be sent at the first attempt.\\par\r\n" +
                    "\\par\r\n" +
                    "Recipient \\i <#relay.to>'\\i0 s Server (\\cf1 <#relay.session_hostname>\\cf0 ) returned the following response: \\cf2 <#relay.error>\\cf0\\par\r\n" +
                    "\\par\r\n" +
                    "\\par\r\n" +
                    "Please note Server will attempt to deliver this message for \\b <#relay.undelivered_after>\\b0  hours.\\par\r\n" +
                    "\\par\r\n" +
                    "--------\\par\r\n" +
                    "\\par\r\n" +
                    "Your original message is attached to this e-mail (\\b data.eml\\b0 )\\par\r\n" +
                    "\\par\r\n" +
                    "\\fs16 The tracking number for this message is \\cf3 <#relay.session_messageid>\\cf0\\fs20\\par\r\n" +
                    "\\lang1061\\f2\\par\r\n" +
                    "\\pard\\lang1033\\f3\\fs17\\par\r\n" +
                    "}\r\n";

                    messageTemplate = new ServerReturnMessage("Delayed delivery notice: <#message.header[\"Subject:\"]>",bodyRtf);
                }
                              
				// Make new message
				Mime m = new Mime();
				MimeEntity mainEntity = m.MainEntity;
				// Force to create From: header field
				mainEntity.From = new AddressList();
				mainEntity.From.Add(new MailboxAddress("postmaster"));
				// Force to create To: header field
				mainEntity.To = new AddressList();
				mainEntity.To.Add(new MailboxAddress(relayInfo.From));
				mainEntity.Subject = variablesMgr.Process(messageTemplate.Subject);
				mainEntity.ContentType = MediaType_enum.Multipart_mixed;

                string rtf = variablesMgr.Process(messageTemplate.BodyTextRtf);

				MimeEntity multipartAlternativeEntity = mainEntity.ChildEntities.Add();
                multipartAlternativeEntity.ContentType = MediaType_enum.Multipart_alternative;

				MimeEntity textEntity = multipartAlternativeEntity.ChildEntities.Add();
				textEntity.ContentType = MediaType_enum.Text_plain;
				textEntity.ContentTransferEncoding = ContentTransferEncoding_enum.QuotedPrintable;
				textEntity.DataText = SCore.RtfToText(rtf);
                                
                MimeEntity htmlEntity = multipartAlternativeEntity.ChildEntities.Add();
				htmlEntity.ContentType = MediaType_enum.Text_html;
				htmlEntity.ContentTransferEncoding = ContentTransferEncoding_enum.QuotedPrintable;
				htmlEntity.DataText = SCore.RtfToHtml(rtf);

				MimeEntity attachmentEntity = mainEntity.ChildEntities.Add();
				attachmentEntity.ContentType = MediaType_enum.Application_octet_stream;
				attachmentEntity.ContentDisposition = ContentDisposition_enum.Attachment;
				attachmentEntity.ContentTransferEncoding = ContentTransferEncoding_enum.Base64;
				attachmentEntity.ContentDisposition_FileName = "data.eml";
				attachmentEntity.DataFromStream(file);

				using(MemoryStream strm = new MemoryStream()){
					m.ToStream(strm);
					m_pRelayServer.VirtualServer.ProcessAndStoreMessage("",new string[]{relayInfo.From},strm,null);
				}
			}
			catch(Exception x){
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

        #region method GetConnectionPoint

        /// <summary>
        /// Gets next connection point form available connection points queue. Returns null if none is available.
        /// Also connection points what maximum allowed connections exceeded, will be skiped.
        /// </summary>
        /// <returns>Returns connection point or null if none available.</returns>
        private IPEndPoint GetConnectionPoint()
        {
            // Try to get IP which maximum connections per IP isn't exceeded.
            while(m_pConnectPoints.Count > 0){
                IPAddress ip = m_pConnectPoints.Dequeue();
                // Check if specified IP maximum allowed connections exceeded.
                if(m_pRelayServer.MaximumConnectionsPerIP == 0 || m_pRelayServer.GetRelayConnections(ip) <= m_pRelayServer.MaximumConnectionsPerIP){
                    return new IPEndPoint(ip,0);
                }
                else if(m_pSmtpClient.SessionActiveLog != null){
                    m_pSmtpClient.SessionActiveLog.AddTextEntry("Skiping IP '" + ip.ToString() + "' maximum connections to that IP exceeded.");
                }
            }

            return null;
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets relay session ID.
        /// </summary>
        public string SessionID
        {
            get{ return m_SessionID; }
        }

        /// <summary>
        /// Gets session start time.
        /// </summary>
        public DateTime SessionStartTime
        {
            get{ return m_SessionStartTime; }
        }

        /// <summary>
		/// Gets session local endpoint.
		/// </summary>
		public EndPoint LocalEndPoint
		{
			get{ return m_pSmtpClient.LocalEndpoint; }
		}

		/// <summary>
		/// Gets session remote endpoint.
		/// </summary>
		public IPEndPoint RemoteEndPoint
		{
			get{ return (IPEndPoint)m_pRemoteEndPoint; }
		}

        /// <summary>
        /// Gets time when was session last activity.
        /// </summary>
        public DateTime LastActivity
        {
            get{ return m_pSmtpClient.LastDataTime; }
        }

        /// <summary>
        /// Gets how many seconds has left before timout is triggered.
        /// </summary>
        public int ExpectedTimeout
        {
            get{
                return (int)(m_pRelayServer.SessionIdleTimeout - ((DateTime.Now.Ticks - LastActivity.Ticks) / 10000));
            }
        }

        /// <summary>
        /// Gets log entries that are currently in log buffer.
        /// </summary>
        public SocketLogger SessionActiveLog
        {
            get{ return m_pSmtpClient.SessionActiveLog; }
        }

        /// <summary>
        /// Gets how many bytes are readed through this session.
        /// </summary>
        public long ReadedCount
        {
            get{ return m_pSmtpClient.ReadedCount; }
        }
        
        /// <summary>
        /// Gets how many bytes are written through this session.
        /// </summary>
        public long WrittenCount
        {
            get{ return m_pSmtpClient.WrittenCount; }
        }
        
        /// <summary>
        /// Gets if the connection is an SSL connection.
        /// </summary>
        public bool IsSecureConnection
        {
            get{ return m_pSmtpClient.IsSecureConnection; }
        }

        /// <summary>
        /// Gets file name which is relayed by this session.
        /// </summary>
        public string FileName
        {
            get{ return m_MessageFile; }
        }

        /// <summary>
        /// Gets relay destination recipient email address.
        /// </summary>
        public string To
        {
            get{ return m_pRelayInfo.To; }
        }

        /// <summary>
        /// Gets orginal sender email address who sent this relay message.
        /// </summary>
        public string From
        {
            get{ return m_pRelayInfo.From; }
        }

        /// <summary>
        /// Gets connected host name. If no connected host or name getting failed, returns "".
        /// </summary>
        public string ConnectedHostName
        {
            get{
                if(m_pRemoteEndPoint != null){
                    try{
                        return Dns.GetHostEntry(((IPEndPoint)m_pSmtpClient.RemoteEndPoint).Address).HostName;
                    }
                    catch{
                        return m_pRemoteEndPoint.Address.ToString();
                    }
                }
                
                return ""; 
            }
        }

        /// <summary>
        /// Gets relay message ID.
        /// </summary>
        public string MessageID
        {
            get{ return Path.GetFileNameWithoutExtension(m_MessageFile); }
        }


        /// <summary>
        /// Gets relay session owner relay server.
        /// </summary>
        internal Relay_Server OwnerServer
        {
            get{ return m_pRelayServer; }
        }

        #endregion

    }
}
