using System;
using System.IO;
using System.Data;
using System.Threading;
using System.Collections;
using LumiSoft.Net.SMTP.Client;
using LumiSoft.Net.Mime;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Mail relayer.
	/// </summary>
	public class Relay
	{
		private static bool      m_Delivering           	= false;
		private static bool      m_DeliveringRetry      	= false;
		private static int       m_MaxThreads           	= 10;
		private static int       m_RelayInterval        	= 10;
		private static int       m_RelayRetryInterval   	= 30;
		private static int       m_RelayUndelWarning    	= 1;
		private static int       m_RelayUndelivered     	= 1;
		private static string    m_UndelWarningTemplate 	= "";
		private static string    m_UndeliveredTemplate      = "";
		private static bool      m_StoreUndeliveredMessages = true;
		private static string    m_SmartHost           		= "";    // Smart host name eg. 'mail.neti.ee'.
		private static bool      m_UseSmartHost        		= true;  // 
		private static string    m_Dns1                		= "";    // Primary dns server IP.
		private static string    m_Dns2                		= "";    // Secondary dns server IP.
		private static Hashtable m_RelayTable          		= null;

		private MailServer m_pServer = null;		
		
		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="server"></param>
		public Relay(MailServer server)
		{
			m_pServer = server;

			if(m_RelayTable == null){
				m_RelayTable = new Hashtable();
			}
		}


		#region function Deliver

		/// <summary>
		/// Sends relay mails.
		/// </summary>
		public void Deliver()
		{
			if(!m_Delivering){
				m_Delivering = true;

				RelayMails();

				m_Delivering = false;
			}
		}

		#endregion

		#region function DeliverRetry

		/// <summary>
		/// Sends retry(mails which couldn't be sent at immedeately) relay mails.
		/// </summary>
		public void DeliverRetry()
		{
			if(!m_DeliveringRetry){
				m_DeliveringRetry = true;

				SendRetryMails();

				m_DeliveringRetry = false;
			}
		}

		#endregion

		
		#region function RelayMails

		/// <summary>
		/// Relays all messages from relay directory.
		/// </summary>
		private void RelayMails()
		{
			try
			{				
				string path = m_pServer.m_MailStorePath + "Relay\\";

				// Check if Directory exists, if not Create
				if(!Directory.Exists(path)){
					Directory.CreateDirectory(path);
				}

				string[] files = Directory.GetFiles(path,"*.eml");
		
				foreach(string file in files){

					// If maximum relay threads are exceeded,
					// wait when some gets available.
					while(m_RelayTable.Count > m_MaxThreads){
						Thread.Sleep(200);
					}

					Thread tr = new Thread(new ThreadStart(this.RelayMessage));					
					AddThread(tr,file);
					tr.Start();		
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}			
		}

		#endregion

		#region function RelayMessage

		private void RelayMessage()
		{			
			try
			{				
				if(!m_RelayTable.Contains(Thread.CurrentThread)){
					SCore.WriteLog(m_pServer.m_SartUpPath + "mailServiceError.log","RelayMails: params missing");
					return;
				}

				string messageFile = m_RelayTable[Thread.CurrentThread].ToString();

				bool sendOk = false;
				using(FileStream fs = File.Open(messageFile,FileMode.Open,FileAccess.ReadWrite)){
					// Get relay info
					RelayInfo relayInf = new RelayInfo(fs,1,1);

					string from = relayInf.From;
					if(from.Length == 0){
						from = relayInf.To;
					}

					SMTP_Client smtpClnt  = new SMTP_Client();
					smtpClnt.UseSmartHost = UseSmartHost;
					smtpClnt.SmartHost    = SmartHost;
					smtpClnt.DnsServers   = new string[]{Dns1,Dns2};
					sendOk = smtpClnt.Send(new string[]{relayInf.To},from,fs);						
				}

				if(sendOk){
					// Message sended successfuly, may delete it
					File.Delete(messageFile);
				}
				// send failed
				else{
					// Move message to Retry folder
					string msgFileName = Path.GetFileName(messageFile);
					File.Move(messageFile,m_pServer.m_MailStorePath + "Retry\\" + msgFileName);
				}
			}
			catch(Exception x){
				if(!(x is IOException)){
					Error.DumpError(x,new System.Diagnostics.StackTrace());
				}
			}
			finally{				
				RemoveThread(Thread.CurrentThread);
			}
		}

		#endregion


		#region function SendRetryMails

		/// <summary>
		/// Relay retry mails from retry directory.
		/// </summary>
		private void SendRetryMails()
		{
			try
			{				
				string path = m_pServer.m_MailStorePath + "Retry\\";

				// Check if Directory exists, if not Create
				if(!Directory.Exists(path)){
					Directory.CreateDirectory(path);
				}

				string[] files = Directory.GetFiles(path,"*.eml");
		
				foreach(string file in files){

					// If maximum relay threads are exceeded,
					// wait when some gets available.
					while(m_RelayTable.Count > m_MaxThreads){
						Thread.Sleep(200);
					}

					Thread t = new Thread(new ThreadStart(this.SendRetryMail));					
					AddThread(t,file);
					t.Start();					
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function SendRetryMail

		private void SendRetryMail()
		{
			try
			{				
				if(!m_RelayTable.Contains(Thread.CurrentThread)){
					SCore.WriteLog(m_pServer.m_SartUpPath + "mailServiceError.log","SendRetryMail: params missing");
					return;
				}

				string messageFile = m_RelayTable[Thread.CurrentThread].ToString();

				using(FileStream fs = File.Open(messageFile,FileMode.Open,FileAccess.ReadWrite)){
					// Get relay info
					RelayInfo relayInf = new RelayInfo(fs,RelayUndelivered,RelayUndelWarning);

					string from = relayInf.From;
					if(from.Length == 0){
						from = relayInf.To;
					}
					
					SMTP_Client smtpClnt  = new SMTP_Client();
					smtpClnt.UseSmartHost = UseSmartHost;
					smtpClnt.SmartHost    = SmartHost;
					smtpClnt.DnsServers   = new string[]{Dns1,Dns2};
					if(smtpClnt.Send(new string[]{relayInf.To},from,fs)){
						fs.Close();
						// Message sended successfuly, may delete it
						File.Delete(messageFile);
					}
					// send failed
					else{
						string error = smtpClnt.Errors[0].ErrorText;
												
						// If destination recipient is invalid or Undelivered Date Exceeded, try to return message to sender
						if(smtpClnt.Errors[0].ErrorType != SMTP_ErrorType.UnKnown || relayInf.IsUndeliveredDateExceeded){							
							MakeUndeliveredNotify(relayInf,error,fs);
							fs.Close();

							// Undelivery note made, may delete message
							if(relayInf.From.Length > 0){
								File.Delete(messageFile);
							}
							// There isn't return address, can't send undelivery note
							else if(StoreUndeliveredMessages){
								// Check if Directory exists, if not Create
								if(!Directory.Exists(m_pServer.m_MailStorePath + "Undelivered\\")){
									Directory.CreateDirectory(m_pServer.m_MailStorePath + "Undelivered\\");
								}
								File.Move(messageFile,messageFile.Replace("Retry","Undelivered"));
							}
						}
						else if(relayInf.MustSendWarning){
							MakeUndeliveredWarning(relayInf,error,fs);

							byte[] mustSendWarningBit = System.Text.Encoding.ASCII.GetBytes("1");
							fs.Position = relayInf.WarningBitPos;
							fs.Write(mustSendWarningBit,0,mustSendWarningBit.Length);							
						}
					}
				}
			}
			catch(Exception x){
				if(!(x is IOException)){
					Error.DumpError(x,new System.Diagnostics.StackTrace());
				}
			}
			finally{				
				RemoveThread(Thread.CurrentThread);
			}
		}

		#endregion

		
		#region function MakeUndeliveredNotify

		/// <summary>
		/// Creates undelivered notify for user and places it to relay folder.
		/// </summary>
		/// <param name="relayInfo">Relay info</param>
		/// <param name="error">SMTP returned error text.</param>
		/// <param name="file">Messsage file.</param>
		private void MakeUndeliveredNotify(RelayInfo relayInfo,string error,Stream file)
		{
			try
			{
				// If sender isn't specified, we can't send undelivery notify to sender.
				// Just skip undelivery notify sending.
				if(relayInfo.From.Length == 0){
					return;
				}

				file.Position = relayInfo.MessageStartPos;

				// Make new message
				MimeConstructor mime = new MimeConstructor();
				mime.From    = "postmaster";
				mime.To      = new string[]{relayInfo.From};
				mime.Subject = "Undelivered mail warning";
				mime.Attachments.Add(new Attachment("data.eml",file));

				string bodyTxt = Relay.UndeliveredTemplate;
				bodyTxt = bodyTxt.Replace("<#RECEPTIENT>",relayInfo.To);
				bodyTxt = bodyTxt.Replace("<#ERROR>",error);

				mime.Body    = bodyTxt;
				
				byte[] data = System.Text.Encoding.Default.GetBytes(mime.ConstructMime());
				using(MemoryStream strm = new MemoryStream(data)){
					m_pServer.ProcessAndStoreMessage("",new string[]{relayInfo.From},strm);
				}
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion

		#region function MakeUndeliveredWarning

		/// <summary>
		/// Creates undelivered warning for user and places it to relay folder.
		/// </summary>
		/// <param name="relayInfo">Relay info</param>
		/// <param name="error">SMTP returned error text.</param>
		/// <param name="file">Messsage file.</param>
		private void MakeUndeliveredWarning(RelayInfo relayInfo,string error,Stream file)
		{	
			try
			{
				// If sender isn't specified, we can't send warning to sender.
				// Just skip warning sending.
				if(relayInfo.From.Length == 0){
					return;
				}

				file.Position = relayInfo.MessageStartPos;

				// Make new message
				MimeConstructor mime = new MimeConstructor();
				mime.From    = "postmaster";
				mime.To      = new string[]{relayInfo.From};
				mime.Subject = "Undelivered mail warning";
				mime.Attachments.Add(new Attachment("data.eml",file));

				string bodyTxt = Relay.UndelWarningTemplate;
					   bodyTxt = bodyTxt.Replace("<#RECEPTIENT>",relayInfo.To);
					   bodyTxt = bodyTxt.Replace("<#ERROR>",error);
					   bodyTxt = bodyTxt.Replace("<#UNDELIVERED_HOURS>",relayInfo.DeviveringForHours.ToString()); 

				mime.Body    = bodyTxt;

				byte[] data = System.Text.Encoding.Default.GetBytes(mime.ConstructMime());
				using(MemoryStream strm = new MemoryStream(data)){
					m_pServer.ProcessAndStoreMessage("",new string[]{relayInfo.From},strm);
				}
				
			//	byte[] data = System.Text.Encoding.Default.GetBytes(mime.ConstructMime());
			//	MailStore.StoreMessage("",new MemoryStream(data),relayInfo.From,"",true);
			}
			catch(Exception x)
			{
				Error.DumpError(x,new System.Diagnostics.StackTrace());
			}
		}

		#endregion


		#region function AddThread

		private void AddThread(Thread tr,string data)
		{
			lock(m_RelayTable){				
				m_RelayTable.Add(tr,data);				
			}
		}

		#endregion

		#region function RemoveThread

		private void RemoveThread(Thread t)
		{
			lock(m_RelayTable){				
				if(!m_RelayTable.ContainsKey(t)){
					SCore.WriteLog(m_pServer.m_SartUpPath + "mailServiceError.log","RemoveThread: doesn't contain");
				}
				m_RelayTable.Remove(t);				
			}
		}

		#endregion
        

		#region Properties Implementation

		/// <summary>
		/// Gets or sets maximum relay threads.
		/// </summary>
		public static int MaxRelayThreads
		{
			get{ return m_MaxThreads; }

			set{ m_MaxThreads = value; }
		}

		/// <summary>
		/// Gets or sets relay interval.
		/// </summary>
		public static int RelayInterval
		{
			get{ return m_RelayInterval; }

			set{ m_RelayInterval = value; }
		}

		/// <summary>
		/// Gets or sets relay retry(delayed relay) interval.
		/// </summary>
		public static int RelayRetryInterval
		{
			get{ return m_RelayRetryInterval; }

			set{ m_RelayRetryInterval = value; }
		}

		/// <summary>
		/// Gets or sets undelivered wanrning minutes.
		/// </summary>
		public static int RelayUndelWarning
		{
			get{ return m_RelayUndelWarning; }

			set{ m_RelayUndelWarning = value; }
		}

		/// <summary>
		/// Gets or sets undelivered wanrning reply template.
		/// </summary>
		public static string UndelWarningTemplate
		{
			get{ return m_UndelWarningTemplate; }

			set{ m_UndelWarningTemplate = value; }
		}

		/// <summary>
		/// Gets or sets undelivered hours.
		/// </summary>
		public static int RelayUndelivered
		{
			get{ return m_RelayUndelivered; }

			set{ m_RelayUndelivered = value; }
		}

		/// <summary>
		/// Gets or sets undelivered reply template.
		/// </summary>
		public static string UndeliveredTemplate
		{
			get{ return m_UndeliveredTemplate; }

			set{ m_UndeliveredTemplate = value; }
		}

		/// <summary>
		/// Gets or sets stroe undelivered messages.
		/// </summary>
		public static bool StoreUndeliveredMessages
		{
			get{ return m_StoreUndeliveredMessages; }

			set{ m_StoreUndeliveredMessages = value; }
		}

		/// <summary>
		/// Gets or sets if to use smart host.
		/// </summary>
		public static bool UseSmartHost
		{
			get{ return m_UseSmartHost; }

			set{ m_UseSmartHost = value; }
		}

		/// <summary>
		/// Gets or sets smart host.
		/// </summary>
		public static string SmartHost
		{
			get{ return m_SmartHost; }

			set{ m_SmartHost = value; }
		}

		/// <summary>
		/// Gets or sets smart host.
		/// </summary>
		public static string Dns1
		{
			get{ return m_Dns1; }

			set{ m_Dns1 = value; }
		}

		/// <summary>
		/// Gets or sets smart host.
		/// </summary>
		public static string Dns2
		{
			get{ return m_Dns2; }

			set{ m_Dns2 = value; }
		}


		/// <summary>
		/// Gets if delivering messages.
		/// </summary>
		public static bool IsDelivering
		{
			get{ return m_Delivering; }
		}

		/// <summary>
		/// Gets if delivering retry messages.
		/// </summary>
		public static bool IsDeliveringRetry
		{
			get{ return m_DeliveringRetry; }
		}

		#endregion
	
	}
}
