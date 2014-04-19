using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

using LumiSoft.Net.IMAP.Server;

namespace LumiSoft.Net.IMAP.Client
{
	/// <summary>
	/// IMAP client.
	/// </summary>
	/// <example>
	/// <code>
	/// using(IMAP_Client c = new IMAP_Client()){
	///		c.Connect("ivx",143);
	///		c.Authenticate("test","test");
	///				
	///		c.SelectFolder("Inbox");
	///				
	///		IMAP_SequenceSet sequence_set = new IMAP_SequenceSet();
	///		// First message
	///		sequence_set.Parse("1");
	///		// All messages
	///	//  sequence_set.Parse("1:*");
	///		// Messages 1,3,6 and 100 to last
	///	//  sequence_set.Parse("1,3,6,100:*");
	///	
	///		// Get messages flags and header
	///		IMAP_FetchItem msgsInfo = c.FetchMessages(sequence_set,IMAP_FetchItem_Flags.MessageFlags | IMAP_FetchItem_Flags.Header,true,false);
	///		
	///		// Do your suff
	///	}
	/// </code>
	/// </example>
	public class IMAP_Client : IDisposable
	{
		private SocketEx m_pSocket        = null;
		private bool     m_Connected      = false;
		private bool     m_Authenticated  = false;
        private char     m_PathSeparator  = '\0';
		private string   m_SelectedFolder = "";
		private int      m_MsgCount       = 0;
		private int      m_NewMsgCount    = 0;
        private long     m_UIDNext        = 0;
        private long     m_UIDValidity    = 0;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public IMAP_Client()
		{			
		}

		#region method Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		public void Dispose()
		{
			Disconnect();
		}

		#endregion


		#region method Connect

        /// <summary>
		/// Connects to IMAP server.
		/// </summary>		
		/// <param name="host">Host name.</param>
		/// <param name="port">Port number. Default IMAP port is 143.</param>
        public void Connect(string host,int port)
		{
            Connect(host,port,false);
        }

		/// <summary>
		/// Connects to IMAP server.
		/// </summary>		
		/// <param name="host">Host name.</param>
		/// <param name="port">Port number. Default IMAP port is 143 and SSL port is 993.</param>
        /// <param name="ssl">Specifies if to connected via SSL.</param>
		public void Connect(string host,int port,bool ssl)
		{
			if(!m_Connected){
				m_pSocket = new SocketEx();
                m_pSocket.Connect(host,port,ssl);

				string reply = m_pSocket.ReadLine();
				reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

				if(!reply.ToUpper().StartsWith("OK")){
					m_pSocket.Disconnect();
					m_pSocket = null;
					throw new Exception("Server returned:" + reply);
				}

				m_Connected = true;
                // Clear path separator, so next access will get it.
                m_PathSeparator  = '\0';
			}
		}

		#endregion

		#region method Disconnect

		/// <summary>
		/// Disconnects from IMAP server.
		/// </summary>
		public void Disconnect()
		{
			if(m_pSocket != null && m_pSocket.Connected){
				// Send QUIT
				m_pSocket.WriteLine("a1 LOGOUT");

				m_pSocket = null;
			}

			m_Connected     = false;
			m_Authenticated = false;
		}

		#endregion

        #region method StartTLS

        /// <summary>
        /// Switches IMAP connection to SSL.
        /// </summary>
        public void StartTLS()
        {
            /* RFC 2595 3. IMAP STARTTLS extension.
             
                Example:    C: a001 CAPABILITY
                            S: * CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED
                            S: a001 OK CAPABILITY completed
                            C: a002 STARTTLS
                            S: a002 OK Begin TLS negotiation now
                            <TLS negotiation, further commands are under TLS layer>
            */

            if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(m_Authenticated){
				throw new Exception("The STARTTLS command is only valid in non-authenticated state !");
			}
            if(m_pSocket.SSL){
                throw new Exception("Connection is already secure !");
            }

            m_pSocket.WriteLine("a1 STARTTLS");

            string reply = m_pSocket.ReadLine();
            reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}

            m_pSocket.SwitchToSSL_AsClient();
        }

        #endregion

        #region method Authenticate

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
			if(m_Authenticated){
				throw new Exception("You are already authenticated !");
			}

			m_pSocket.WriteLine("a1 LOGIN \"" + userName +  "\" \"" + password + "\"");

			string reply = m_pSocket.ReadLine();
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(reply.ToUpper().StartsWith("OK")){
				m_Authenticated = true;
			}
			else{
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion


		#region method CreateFolder

		/// <summary>
		/// Creates specified folder.
		/// </summary>
		/// <param name="folderName">Folder name. Eg. test, Inbox/SomeSubFolder. NOTE: use GetFolderSeparator() to get right folder separator.</param>
		public void CreateFolder(string folderName)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\',this.PathSeparator).Replace('/',this.PathSeparator);

			m_pSocket.WriteLine("a1 CREATE \"" + Core.Encode_IMAP_UTF7_String(folderName) + "\"");

			string reply = m_pSocket.ReadLine();
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

		#region method DeleteFolder

		/// <summary>
		/// Deletes specified folder.
		/// </summary>
		/// <param name="folderName">Folder name.</param>
		public void DeleteFolder(string folderName)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			m_pSocket.WriteLine("a1 DELETE \"" + Core.Encode_IMAP_UTF7_String(folderName) + "\"");

			string reply = m_pSocket.ReadLine();
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

		#region method RenameFolder

		/// <summary>
		/// Renames specified folder.
		/// </summary>
		/// <param name="sourceFolderName">Source folder name.</param>
		/// <param name="destinationFolderName">Destination folder name.</param>
		public void RenameFolder(string sourceFolderName,string destinationFolderName)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			m_pSocket.WriteLine("a1 RENAME \"" + Core.Encode_IMAP_UTF7_String(sourceFolderName) + "\" \"" + Core.Encode_IMAP_UTF7_String(destinationFolderName) + "\"");

			string reply = m_pSocket.ReadLine();
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

		#region method GetFolders

		/// <summary>
		///  Gets all available folders.
		/// </summary>
		/// <returns></returns>
		public string[] GetFolders()
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			ArrayList list = new ArrayList();

			m_pSocket.WriteLine("a1 LIST \"\" \"*\"");

			// Must get lines with * and cmdTag + OK or cmdTag BAD/NO
			string reply = m_pSocket.ReadLine();			
			if(reply.StartsWith("*")){
				// Read multiline response
				while(reply.StartsWith("*")){
					// don't show not selectable folders
					if(reply.ToLower().IndexOf("\\noselect") == -1){
						reply = reply.Substring(reply.IndexOf(")") + 1).Trim(); // Remove * LIST(..)
						reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Folder separator

						// Folder name between ""
						if(reply.IndexOf("\"") > -1){
							list.Add(Core.Decode_IMAP_UTF7_String(reply.Substring(reply.IndexOf("\"") + 1,reply.Length - reply.IndexOf("\"") - 2)));
						}
						else{
							list.Add(Core.Decode_IMAP_UTF7_String(reply.Trim()));
						}
					}

					reply = m_pSocket.ReadLine();
				}
			}
			
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}

			string[] retVal = new string[list.Count];
			list.CopyTo(retVal);

            return retVal;
		}

		#endregion

		#region method GetSubscribedFolders

		/// <summary>
		/// Gets all subscribed folders.
		/// </summary>
		/// <returns></returns>
		public string[] GetSubscribedFolders()
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

            ArrayList list = new ArrayList();

			m_pSocket.WriteLine("a1 LSUB \"\" \"*\"");

			// Must get lines with * and cmdTag + OK or cmdTag BAD/NO
			string reply = m_pSocket.ReadLine();			
			if(reply.StartsWith("*")){
				// Read multiline response
				while(reply.StartsWith("*")){
					// don't show not selectable folders
					if(reply.ToLower().IndexOf("\\noselect") == -1){
						reply = reply.Substring(reply.IndexOf(")") + 1).Trim(); // Remove * LSUB(..)
						reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Folder separator

						// Folder name between ""
						if(reply.IndexOf("\"") > -1){
							list.Add(Core.Decode_IMAP_UTF7_String(reply.Substring(reply.IndexOf("\"") + 1,reply.Length - reply.IndexOf("\"") - 2)));
						}
						else{
							list.Add(Core.Decode_IMAP_UTF7_String(reply.Trim()));
						}
					}

					reply = m_pSocket.ReadLine();
				}
			}
			
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}

			string[] retVal = new string[list.Count];
			list.CopyTo(retVal);

            return retVal;

            // REMOVE ME:
	/*		ArrayList list = new ArrayList();

			m_pSocket.WriteLine("a1 LSUB \"\" \"*\"");

			// Must get lines with * and cmdTag + OK or cmdTag BAD/NO
			string reply = m_pSocket.ReadLine();			
			if(reply.StartsWith("*")){
				// Read multiline response
				while(reply.StartsWith("*")){
					//
					string folder = reply.Substring(reply.LastIndexOf(" ")).Trim().Replace("\"","");
					list.Add(Core.Decode_IMAP_UTF7_String(folder));

					reply = m_pSocket.ReadLine();
				}
			}
			
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}

			string[] retVal = new string[list.Count];
			list.CopyTo(retVal);

            return retVal;*/
		}

		#endregion

		#region method SubscribeFolder

		/// <summary>
		/// Subscribes specified folder.
		/// </summary>
		/// <param name="folderName">Folder name.</param>
		public void SubscribeFolder(string folderName)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			m_pSocket.WriteLine("a1 SUBSCRIBE \"" + Core.Encode_IMAP_UTF7_String(folderName) + "\"");

			string reply = m_pSocket.ReadLine();
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

		#region method UnSubscribeFolder

		/// <summary>
		/// UnSubscribes specified folder.
		/// </summary>
		/// <param name="folderName">Folder name,</param>
		public void UnSubscribeFolder(string folderName)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			m_pSocket.WriteLine("a1 UNSUBSCRIBE \"" + Core.Encode_IMAP_UTF7_String(folderName) + "\"");

			string reply = m_pSocket.ReadLine();
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

		#region method SelectFolder

		/// <summary>
		/// Selects specified folder.
		/// </summary>
		/// <param name="folderName">Folder name.</param>
		public void SelectFolder(string folderName)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			m_pSocket.WriteLine("a1 SELECT \"" + Core.Encode_IMAP_UTF7_String(folderName) + "\"");

			// Must get lines with * and cmdTag + OK or cmdTag BAD/NO
			string reply = m_pSocket.ReadLine();		
			if(reply.StartsWith("*")){
				// Read multiline response
				while(reply.StartsWith("*")){
					// Get rid of *
					reply = reply.Substring(1).Trim();

					if(reply.ToUpper().IndexOf("EXISTS") > -1 && reply.ToUpper().IndexOf("FLAGS") == -1){
						m_MsgCount = Convert.ToInt32(reply.Substring(0,reply.IndexOf(" ")).Trim());
					}
					else if(reply.ToUpper().IndexOf("RECENT") > -1 && reply.ToUpper().IndexOf("FLAGS") == -1){
						m_NewMsgCount = Convert.ToInt32(reply.Substring(0,reply.IndexOf(" ")).Trim());
					}
                    else if(reply.ToUpper().IndexOf("UIDNEXT") > -1){
                        m_UIDNext = Convert.ToInt64(reply.Substring(reply.ToUpper().IndexOf("UIDNEXT") + 8,reply.IndexOf(']') - reply.ToUpper().IndexOf("UIDNEXT") - 8));
                    }
                    else if(reply.ToUpper().IndexOf("UIDVALIDITY") > -1){
                        m_UIDValidity = Convert.ToInt64(reply.Substring(reply.ToUpper().IndexOf("UIDVALIDITY") + 12,reply.IndexOf(']') - reply.ToUpper().IndexOf("UIDVALIDITY") - 12));
                    }
					
					reply = m_pSocket.ReadLine();
				}
			}
			
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}

			m_SelectedFolder = folderName;
		}

		#endregion


        #region method GetNamespacesInfo

        /// <summary>
        /// Gets IMAP server namespaces info.
        /// </summary>
        /// <returns></returns>
        public IMAP_NamespacesInfo GetNamespacesInfo()
        {
            /* RFC 2342 5. NAMESPACE Command.
               Arguments: none

               Response:  an untagged NAMESPACE response that contains the prefix
                          and hierarchy delimiter to the server's Personal
                          Namespace(s), Other Users' Namespace(s), and Shared
                          Namespace(s) that the server wishes to expose. The
                          response will contain a NIL for any namespace class
                          that is not available. Namespace_Response_Extensions
                          MAY be included in the response.
                          Namespace_Response_Extensions which are not on the IETF
                          standards track, MUST be prefixed with an "X-".

               Result:    OK - Command completed
                          NO - Error: Can't complete command
                          BAD - argument invalid
             
             
                Example:
                    < A server that supports a single personal namespace.  No leading
                    prefix is used on personal mailboxes and "/" is the hierarchy
                    delimiter.>

                    C: A001 NAMESPACE
                    S: * NAMESPACE (("" "/")) NIL NIL
                    S: A001 OK NAMESPACE command completed             
            */

            if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

            m_pSocket.WriteLine("a1 NAMESPACE");

            IMAP_NamespacesInfo namespacesInfo = new IMAP_NamespacesInfo(null,null,null);
            string reply = m_pSocket.ReadLine();
            while(reply.StartsWith("*")){
                reply = RemoveCmdTag(reply);

                if(reply.ToUpper().StartsWith("NAMESPACE")){
                    namespacesInfo = IMAP_NamespacesInfo.Parse(reply);
                }

                reply = m_pSocket.ReadLine();
            }
                     
            reply = RemoveCmdTag(reply);
			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}

            return namespacesInfo;
        }

        #endregion


        #region method GetFolderACL

        /// <summary>
		/// Gets specified folder ACL entries.
		/// </summary>
		/// <param name="folderName">Folder which ACL entries to get.</param>
        /// <returns></returns>
		public IMAP_Acl[] GetFolderACL(string folderName)
		{
            /* RFC 2086 4.3. GETACL
				Arguments:  mailbox name

				Data:       untagged responses: ACL

				Result:     OK - getacl completed
							NO - getacl failure: can't get acl
							BAD - command unknown or arguments invalid

					The GETACL command returns the access control list for mailbox in
					an untagged ACL reply.

				Example:    C: A002 GETACL INBOX
							S: * ACL INBOX Fred rwipslda
							S: A002 OK Getacl complete
							
							.... Multiple users
							S: * ACL INBOX Fred rwipslda test rwipslda
							
							.... No acl flags for Fred
							S: * ACL INBOX Fred "" test rwipslda
									
			*/
            			
            if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

            m_pSocket.WriteLine("a1 GETACL " + TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName)));

            List<IMAP_Acl> retVal = new List<IMAP_Acl>();
            string reply = m_pSocket.ReadLine();
            while(reply.StartsWith("*")){
                reply = RemoveCmdTag(reply);

                if(reply.ToUpper().StartsWith("ACL")){
                    retVal.Add(IMAP_Acl.Parse(reply));
                }

                reply = m_pSocket.ReadLine();
            }
                     
            reply = RemoveCmdTag(reply);
			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}

            return retVal.ToArray();
        }

        #endregion

        #region method SetFolderACL

        /// <summary>
		/// Sets specified user ACL permissions for specified folder.
		/// </summary>
		/// <param name="folderName">Folder name which ACL to set.</param>
		/// <param name="userName">User name who's ACL to set.</param>
		/// <param name="acl">ACL permissions to set.</param>
		public void SetFolderACL(string folderName,string userName,IMAP_ACL_Flags acl)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			m_pSocket.WriteLine("a1 SETACL \"" + Core.Encode_IMAP_UTF7_String(folderName) + "\" \"" + userName + "\" " + IMAP_Utils.ACL_to_String(acl));

			string reply = m_pSocket.ReadLine();
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

		#region method DeleteFolderACL

		/// <summary>
		/// Deletes specified user access to specified folder.
		/// </summary>
		/// <param name="folderName">Folder which ACL to remove.</param>
		/// <param name="userName">User name who's ACL to remove.</param>
		public void DeleteFolderACL(string folderName,string userName)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			m_pSocket.WriteLine("a1 DELETEACL \"" + Core.Encode_IMAP_UTF7_String(folderName) + "\" \"" + userName + "\"");

			string reply = m_pSocket.ReadLine();
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

		#region method GetFolderMyrights

		/// <summary>
		/// Gets myrights to specified folder.
		/// </summary>
		/// <param name="folderName"></param>
		/// <returns></returns>
		public IMAP_ACL_Flags GetFolderMyrights(string folderName)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			IMAP_ACL_Flags aclFlags = IMAP_ACL_Flags.None;

			m_pSocket.WriteLine("a1 MYRIGHTS \"" + Core.Encode_IMAP_UTF7_String(folderName) + "\"");

			// Must get lines with * and cmdTag + OK or cmdTag BAD/NO
			string reply = m_pSocket.ReadLine();		
			if(reply.StartsWith("*")){
				// Read multiline response
				while(reply.StartsWith("*")){
					// Get rid of *
					reply = reply.Substring(1).Trim();

					if(reply.ToUpper().IndexOf("MYRIGHTS") > -1){
						aclFlags = IMAP_Utils.ACL_From_String(reply.Substring(0,reply.IndexOf(" ")).Trim());
					}
					
					reply = m_pSocket.ReadLine();
				}
			}
			
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}

			return aclFlags;
		}

		#endregion

		
		#region method CopyMessages
        		
		/// <summary>
		/// Copies specified messages to specified folder.
		/// </summary>
		/// <param name="sequence_set">IMAP sequence-set.</param>
		/// <param name="destFolder">Destination folder name.</param>
		/// <param name="uidCopy">Specifies if UID COPY or COPY. 
		/// For UID COPY all sequence_set numers must be message UID values and for normal COPY message numbers.</param>
		public void CopyMessages(IMAP_SequenceSet sequence_set,string destFolder,bool uidCopy)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}
			if(m_SelectedFolder.Length == 0){
				throw new Exception("You must select folder first !");
			}
			

			if(uidCopy){
				m_pSocket.WriteLine("a1 UID COPY " + sequence_set.ToSequenceSetString() + " \"" + Core.Encode_IMAP_UTF7_String(destFolder) + "\"");
			}
			else{
				m_pSocket.WriteLine("a1 COPY " + sequence_set.ToSequenceSetString() + " \"" + Core.Encode_IMAP_UTF7_String(destFolder) + "\"");
			}

			// Read server resposnse
			string reply = m_pSocket.ReadLine();

			// There is STATUS reponse, read and process it
			while(reply.StartsWith("*")){
				ProcessStatusResponse(reply);

				reply = m_pSocket.ReadLine();
			}

			// We must get OK or otherwise there is error
			if(!RemoveCmdTag(reply).ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}
		}
		
		#endregion

		#region method MoveMessages
        				
		/// <summary>
		/// Moves specified messages to specified folder.
		/// </summary>
		/// <param name="sequence_set">IMAP sequence-set.</param>
		/// <param name="destFolder">Folder where to copy messages.</param>
		/// <param name="uidMove">Specifies if sequence-set contains message UIDs or message numbers.</param>
		public void MoveMessages(IMAP_SequenceSet sequence_set,string destFolder,bool uidMove)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}
			if(m_SelectedFolder.Length == 0){
				throw new Exception("You must select folder first !");
			}

			CopyMessages(sequence_set,destFolder,uidMove);
			DeleteMessages(sequence_set,uidMove);
		}

		#endregion

		#region method DeleteMessages
        		
		/// <summary>
		/// Deletes specified messages.
		/// </summary>
		/// <param name="sequence_set">IMAP sequence-set.</param>
		/// <param name="uidDelete">Specifies if sequence-set contains message UIDs or message numbers.</param>
		public void DeleteMessages(IMAP_SequenceSet sequence_set,bool uidDelete)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}
			if(m_SelectedFolder.Length == 0){
				throw new Exception("You must select folder first !");
			}

			// 1) Set deleted flag
			// 2) Delete messages with EXPUNGE command

			if(uidDelete){
				m_pSocket.WriteLine("a1 UID STORE " + sequence_set.ToSequenceSetString()  + " +FLAGS.SILENT (\\Deleted)");
			}
			else{
				m_pSocket.WriteLine("a1 STORE " + sequence_set.ToSequenceSetString()  + " +FLAGS.SILENT (\\Deleted)");
			}

			// Read server resposnse
			string reply = m_pSocket.ReadLine();

			// There is STATUS reponse, read and process it
			while(reply.StartsWith("*")){
				ProcessStatusResponse(reply);

				reply = m_pSocket.ReadLine();
			}

			// We must get OK or otherwise there is error
			if(!RemoveCmdTag(reply).ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}
			
			m_pSocket.WriteLine("a1 EXPUNGE");

			// Read server resposnse
			reply = m_pSocket.ReadLine();

			// There is STATUS reponse, read and process it
			while(reply.StartsWith("*")){
				ProcessStatusResponse(reply);

				reply = m_pSocket.ReadLine();
			}

			// We must get OK or otherwise there is error
			if(!RemoveCmdTag(reply).ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

		#region method StoreMessage

		/// <summary>
		/// Stores message to specified folder.
		/// </summary>
		/// <param name="folderName">Folder where to store message.</param>
		/// <param name="data">Message data which to store.</param>		
		public void StoreMessage(string folderName,byte[] data)
		{
			StoreMessage(folderName,IMAP_MessageFlags.Seen,DateTime.Now,data);
		}

		/// <summary>
		/// Stores message to specified folder.
		/// </summary>
		/// <param name="folderName">Folder where to store message.</param>
		/// <param name="messageFlags">Message flags what are stored for message.</param>
		/// <param name="inernalDate">Internal date value what are stored for message.</param>
		/// <param name="data">Message data which to store.</param>
		public void StoreMessage(string folderName,IMAP_MessageFlags messageFlags,DateTime inernalDate,byte[] data)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\',this.PathSeparator).Replace('/',this.PathSeparator);

			m_pSocket.WriteLine("a1 APPEND \"" + Core.Encode_IMAP_UTF7_String(folderName) + "\" (" + IMAP_Utils.MessageFlagsToString(messageFlags) + ") \"" + IMAP_Utils.DateTimeToString(inernalDate) + "\" {" + data.Length + "}");

			// Read server resposnse
			string reply = m_pSocket.ReadLine();

			// There is STATUS reponse, read and process it
			while(reply.StartsWith("*")){
				ProcessStatusResponse(reply);

				reply = m_pSocket.ReadLine();
			}

			// We must get reply with starting +			
			if(reply.StartsWith("+")){
				// Send message
                m_pSocket.Write(data);

				// Send CRLF, ends splitted command line
				m_pSocket.Write(new byte[]{(byte)'\r',(byte)'\n'});

				// Read server resposnse
				reply = m_pSocket.ReadLine();

				// There is STATUS reponse, read and process it
				while(reply.StartsWith("*")){
					ProcessStatusResponse(reply);

					reply = m_pSocket.ReadLine();
				}

				// We must get OK or otherwise there is error
				if(!RemoveCmdTag(reply).ToUpper().StartsWith("OK")){
					throw new Exception("Server returned:" + reply);
				}
			}
			else{
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion

		#region method Search
        
		private int[] Search()
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}
			if(m_SelectedFolder.Length == 0){
				throw new Exception("You must select folder first !");
			}

			// TODO:

			return null;
		}

		#endregion

		#region method FetchMessages
        		
		/// <summary>
		/// Fetches specifes messages specified fetch items.
		/// </summary>
		/// <param name="sequence_set">IMAP sequence-set.</param>
		/// <param name="fetchFlags">Specifies what data to fetch from IMAP server.</param>
		/// <param name="setSeenFlag">If true message seen flag is setted.</param>
		/// <param name="uidFetch">Specifies if sequence-set contains message UIDs or message numbers.</param>
		/// <returns></returns>
		public IMAP_FetchItem[] FetchMessages(IMAP_SequenceSet sequence_set,IMAP_FetchItem_Flags fetchFlags,bool setSeenFlag,bool uidFetch)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}
			if(m_SelectedFolder.Length == 0){
				throw new Exception("You must select folder first !");
			}

			List<IMAP_FetchItem> fetchItems = new List<IMAP_FetchItem>();

			//--- Construct FETCH command line -----------------------------------------------------------------------//
			string fetchCmdLine = "a1";
			if(uidFetch){
				fetchCmdLine += " UID";
			}
			fetchCmdLine += " FETCH " + sequence_set.ToSequenceSetString() + " (UID";
			
			// FLAGS
			if((fetchFlags & IMAP_FetchItem_Flags.MessageFlags) != 0){
				fetchCmdLine += " FLAGS";
			}
			// RFC822.SIZE
			if((fetchFlags & IMAP_FetchItem_Flags.Size) != 0){
				fetchCmdLine += " RFC822.SIZE";
			}
			// INTERNALDATE
			if((fetchFlags & IMAP_FetchItem_Flags.InternalDate) != 0){
				fetchCmdLine += " INTERNALDATE";
			}
			// ENVELOPE
			if((fetchFlags & IMAP_FetchItem_Flags.Envelope) != 0){
				fetchCmdLine += " ENVELOPE";
			}
            // BODYSTRUCTURE
			if((fetchFlags & IMAP_FetchItem_Flags.BodyStructure) != 0){
				fetchCmdLine += " BODYSTRUCTURE";
			}
			// BODY[] or BODY.PEEK[]
			if((fetchFlags & IMAP_FetchItem_Flags.Message) != 0){
				if(setSeenFlag){
					fetchCmdLine += " BODY[]";
				}
				else{
					fetchCmdLine += " BODY.PEEK[]";
				}
			}
			// BODY[HEADER] or BODY.PEEK[HEADER] ---> This needed only if full message isn't requested.
			if((fetchFlags & IMAP_FetchItem_Flags.Message) == 0 && (fetchFlags & IMAP_FetchItem_Flags.Header) != 0){
				if(setSeenFlag){
					fetchCmdLine += " BODY[HEADER]";
				}
				else{
					fetchCmdLine += " BODY.PEEK[HEADER]";
				}
			}
			//--------------------------------------------------------------------------------------------------------//
            
			fetchCmdLine += ")";

			// Send fetch command line to server
			m_pSocket.WriteLine(fetchCmdLine);
	
			// Must get lines with * and cmdTag + OK or cmdTag BAD/NO
			string reply = m_pSocket.ReadLine(50000);
			// Read multiline response
			while(reply.StartsWith("*")){
				// Fetch may return status response there, skip them					
				if(IsStatusResponse(reply)){
					// Read next fetch item or server response
					reply = m_pSocket.ReadLine(50000);
					continue;
				}
				
				int               no            = 0;
				int               uid           = 0;
				int               size          = 0;
				byte[]            data          = null;
				IMAP_MessageFlags flags         = IMAP_MessageFlags.Recent;
				string            envelope      = "";
                string            bodystructure = "";
				string            internalDate  = "";

				// Remove *
				reply = reply.Substring(1).TrimStart();
				
				// Get message number
				no = Convert.ToInt32(reply.Substring(0,reply.IndexOf(" ")));

				// Get rid of FETCH  and parse params. Reply:* 1 FETCH (UID 12 BODY[] ...)
				reply = reply.Substring(reply.IndexOf("FETCH (") + 7);

				StringReader r = new StringReader(reply);
				// Loop fetch result fields
				while(r.Available > 0){
					r.ReadToFirstChar();

					// Fetch command closing ) parenthesis
					if(r.SourceString == ")"){
						break;
					}

					#region UID <value>

					// UID <value>
					else if(r.StartsWith("UID",false)){
						// Remove UID word from reply
						r.ReadSpecifiedLength("UID".Length);
						r.ReadToFirstChar();

						// Read <value>
						string word = r.ReadWord();
						if(word == null){
							throw new Exception("IMAP server didn't return UID <value> !");
						}
						else{
							uid = Convert.ToInt32(word);
						}
					}

					#endregion

					#region RFC822.SIZE <value>

					// RFC822.SIZE <value>
					else if(r.StartsWith("RFC822.SIZE",false)){
						// Remove RFC822.SIZE word from reply
						r.ReadSpecifiedLength("RFC822.SIZE".Length);
						r.ReadToFirstChar();

						// Read <value>
						string word = r.ReadWord();
						if(word == null){
							throw new Exception("IMAP server didn't return RFC822.SIZE <value> !");
						}
						else{
							try{
								size = Convert.ToInt32(word);
							}
							catch{
								throw new Exception("IMAP server returned invalid RFC822.SIZE <value> '" + word + "' !");
							}
						}
					}

					#endregion

					#region INTERNALDATE <value>

					// INTERNALDATE <value>
					else if(r.StartsWith("INTERNALDATE",false)){
						// Remove INTERNALDATE word from reply
						r.ReadSpecifiedLength("INTERNALDATE".Length);
						r.ReadToFirstChar();

						// Read <value>
						string word = r.ReadWord();
						if(word == null){
							throw new Exception("IMAP server didn't return INTERNALDATE <value> !");
						}
						else{
							internalDate = word;
						}
					}

					#endregion

					#region ENVELOPE (<envelope-string>)

					else if(r.StartsWith("ENVELOPE",false)){
						// Remove ENVELOPE word from reply
						r.ReadSpecifiedLength("ENVELOPE".Length);
						r.ReadToFirstChar();
                        
                        /* 
                            Handle string literals {count-to-read}<CRLF>data(length = count-to-read).
                            (string can be quoted string or literal)
                            Loop while get envelope,invalid response or timeout.
                        */

                        while(true){
                            try{
						        envelope = r.ReadParenthesized();
                                break;
                            }
                            catch(Exception x){
                                string s = r.ReadToEnd();

                                /* partial_envelope {count-to-read}
                                   Example: ENVELOPE ("Mon, 03 Apr 2006 10:10:10 GMT" {35}
                                */
                                if(s.EndsWith("}")){                                
                                    // Get partial envelope and append it back to reader
                                    r.AppenString(s.Substring(0,s.LastIndexOf('{')));

                                    // Read remaining envelope and append it to reader
                                    int countToRead = Convert.ToInt32(s.Substring(s.LastIndexOf('{') + 1,s.LastIndexOf('}') - s.LastIndexOf('{') - 1));
                                    MemoryStream strm = new MemoryStream();
                                    m_pSocket.ReadSpecifiedLength(countToRead,strm);
                                    r.AppenString(TextUtils.QuoteString(System.Text.Encoding.Default.GetString(strm.ToArray())));

                                    // Read fetch continuing line
						            r.AppenString(m_pSocket.ReadLine(50000));
                                }
                                // Unexpected response
                                else{
                                    throw x;
                                }
                            }
                        }
					}

					#endregion

                    #region BODYSTRUCTURE (<bodystructure-string>)

                    else if(r.StartsWith("BODYSTRUCTURE",false)){
                        // Remove BODYSTRUCTURE word from reply
						r.ReadSpecifiedLength("BODYSTRUCTURE".Length);
						r.ReadToFirstChar();

						bodystructure = r.ReadParenthesized();
                    }

                    #endregion

                    #region BODY[] or BODY[HEADER]

                    // BODY[] or BODY[HEADER]
					else if(r.StartsWith("BODY",false)){
						if(r.StartsWith("BODY[]",false)){
							// Remove BODY[]
							r.ReadSpecifiedLength("BODY[]".Length);
						}
						else if(r.StartsWith("BODY[HEADER]",false)){
							// Remove BODY[HEADER]
							r.ReadSpecifiedLength("BODY[HEADER]".Length);
						}
						else{
							throw new Exception("Invalid FETCH response: " + r.SourceString);
						}
						r.ReadToFirstChar();

						// We must now have {<size-to-read>}, or there is error
						if(!r.StartsWith("{")){
							throw new Exception("Invalid FETCH BODY[] or BODY[HEADER] response: " + r.SourceString);
						}
						// Read <size-to-read>
						int dataLength = Convert.ToInt32(r.ReadParenthesized());

						// Read data
						MemoryStream storeStrm = new MemoryStream(dataLength);
						m_pSocket.ReadSpecifiedLength(dataLength,storeStrm);
						data = storeStrm.ToArray();
				
						// Read fetch continuing line
						r.AppenString(m_pSocket.ReadLine(50000).Trim());
					}

					#endregion

					#region FLAGS (<flags-list>)

					// FLAGS (<flags-list>)
					else if(r.StartsWith("FLAGS",false)){
						// Remove FLAGS word from reply
						r.ReadSpecifiedLength("FLAGS".Length);
						r.ReadToFirstChar();

						// Read (<flags-list>)
						string flagsList = r.ReadParenthesized();
						if(flagsList == null){
							throw new Exception("IMAP server didn't return FLAGS (<flags-list>) !");
						}
						else{
							flags = IMAP_Utils.ParseMessageFlags(flagsList);
						}
					}

					#endregion

					else{
						throw new Exception("Not supported fetch reply: " + r.SourceString);
					}
				}

				fetchItems.Add(new IMAP_FetchItem(no,uid,size,data,flags,internalDate,envelope,bodystructure,fetchFlags));

				// Read next fetch item or server response
				reply = m_pSocket.ReadLine(50000);
			}
			
			// We must get OK or otherwise there is error
			if(!RemoveCmdTag(reply).ToUpper().StartsWith("OK")){
				if(!reply.ToUpper().StartsWith("NO")){
					throw new Exception("Server returned:" + reply);
				}
			}

			return fetchItems.ToArray();
		}

		#endregion

        #region method FetchMessage

        /// <summary>
        /// Gets specified message from server and stores to specified stream.
        /// </summary>
        /// <param name="uid">Message UID which to get.</param>
        /// <param name="storeStream">Stream where to store message.</param>
        public void FetchMessage(int uid,Stream storeStream)
        {
            if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}
			if(m_SelectedFolder.Length == 0){
				throw new Exception("You must select folder first !");
			}
                        
            m_pSocket.WriteLine("a1 UID FETCH " + uid + " BODY[]");

            string reply = m_pSocket.ReadLine(50000);
			// Read multiline response
			while(reply.StartsWith("*")){
                // Fetch may return status response there, skip them					
				if(IsStatusResponse(reply)){
					// Read next server response
					reply = m_pSocket.ReadLine(50000);
					continue;
				}

                reply = RemoveCmdTag(reply);

                // We must get here: BODY[] {sizeOfData}
                if(reply.ToUpper().ToString().IndexOf("BODY[") > - 1){
                    if(reply.IndexOf('{') > -1){
                        StringReader r = new StringReader(reply);
                        while(r.Available > 0 && !r.StartsWith("{")){
                            r.ReadSpecifiedLength(1);
                        }
                        int sizeOfData = Convert.ToInt32(r.ReadParenthesized());
                        m_pSocket.ReadSpecifiedLength(sizeOfData,storeStream);                        
                        m_pSocket.ReadLine();
                    }
                }

                // Read next server response
				reply = m_pSocket.ReadLine(50000);
            }

            // We must get OK or otherwise there is error
			if(!RemoveCmdTag(reply).ToUpper().StartsWith("OK")){
				if(!reply.ToUpper().StartsWith("NO")){
					throw new Exception("Server returned:" + reply);
				}
			}
        }

        #endregion

        #region method StoreMessageFlags

        /// <summary>
		/// Stores specified message flags to specified messages.
		/// </summary>
		/// <param name="sequence_set">IMAP sequence-set.</param>
		/// <param name="msgFlags">Message flags.</param>
		/// <param name="uidStore">Specifies if UID STORE or STORE. 
		/// For UID STORE all sequence_set numers must be message UID values and for normal STORE message numbers.</param>
		public void StoreMessageFlags(IMAP_SequenceSet sequence_set,IMAP_MessageFlags msgFlags,bool uidStore)
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}
			if(m_SelectedFolder.Length == 0){
				throw new Exception("You must select folder first !");
			}

			if(uidStore){
				m_pSocket.WriteLine("a1 UID STORE " + sequence_set.ToSequenceSetString() + " FLAGS (" + IMAP_Utils.MessageFlagsToString(msgFlags) + ")");
			}
			else{
				m_pSocket.WriteLine("a1 STORE " + sequence_set.ToSequenceSetString() + " FLAGS (" + IMAP_Utils.MessageFlagsToString(msgFlags) + ")");
			}

			// Read server resposnse
			string reply = m_pSocket.ReadLine();

			// There is STATUS reponse, read and process it
			while(reply.StartsWith("*")){
				ProcessStatusResponse(reply);

				reply = m_pSocket.ReadLine();
			}

			// We must get OK or otherwise there is error
			if(!RemoveCmdTag(reply).ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}
		}

		#endregion


        #region method GetFolderQuota

        /// <summary>
        /// Gets specified folder quota info. Throws Exception if server doesn't support QUOTA.
        /// </summary>
        /// <param name="folder">Folder name.</param>
        /// <returns></returns>
        public IMAP_Quota GetFolderQuota(string folder)
        {
            /* RFC 2087 4.3. GETQUOTAROOT Command

                    Arguments:  mailbox name

                    Data:       untagged responses: QUOTAROOT, QUOTA

                    Result:     OK - getquota completed
                                NO - getquota error: no such mailbox, permission denied
                                BAD - command unknown or arguments invalid

               The GETQUOTAROOT command takes the name of a mailbox and returns the
               list of quota roots for the mailbox in an untagged QUOTAROOT
               response.  For each listed quota root, it also returns the quota
               root's resource usage and limits in an untagged QUOTA response.

                   Example:    C: A003 GETQUOTAROOT INBOX
                               S: * QUOTAROOT INBOX ""
                               S: * QUOTA "" (STORAGE 10 512)
                               S: A003 OK Getquota completed
            */

            if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

            IMAP_Quota retVal = null;

            m_pSocket.WriteLine("a1 GETQUOTAROOT \"" + Core.Encode_IMAP_UTF7_String(folder) + "\"");

            // Must get lines with * and cmdTag + OK or cmdTag BAD/NO
			string reply = m_pSocket.ReadLine();		
			if(reply.StartsWith("*")){
				// Read multiline response
				while(reply.StartsWith("*")){
					// Get rid of *
					reply = reply.Substring(1).Trim();

					if(reply.ToUpper().StartsWith("QUOTAROOT")){
						// Skip QUOTAROOT
					}
					else if(reply.ToUpper().StartsWith("QUOTA")){
						StringReader r = new StringReader(reply);
                        // Skip QUOTA word
                        r.ReadWord();

                        string qoutaRootName = r.ReadWord();
                        long   storage       = -1;
                        long   maxStorage    = -1;
                        long   messages      = -1;
                        long   maxMessages   = -1;

                        string limits = r.ReadParenthesized();
                        r = new StringReader(limits);
                        while(r.Available > 0){
                            string limitName = r.ReadWord();
                            // STORAGE usedBytes maximumAllowedBytes
                            if(limitName.ToUpper() == "STORAGE"){
                                storage    = Convert.ToInt64(r.ReadWord());
                                maxStorage = Convert.ToInt64(r.ReadWord());
                            }
                            // STORAGE messagesCount maximumAllowedMessages
                            else if(limitName.ToUpper() == "MESSAGE"){
                                messages    = Convert.ToInt64(r.ReadWord());
                                maxMessages = Convert.ToInt64(r.ReadWord());
                            }
                        }

                        retVal = new IMAP_Quota(qoutaRootName,messages,maxMessages,storage,maxStorage);
					}
					
					reply = m_pSocket.ReadLine();
				}
			}
			
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				throw new Exception("Server returned:" + reply);
			}

            return retVal;
        }

        #endregion


        #region method GetMessagesTotalSize

        /// <summary>
		/// Gets messages total size in selected folder.
		/// </summary>
		/// <returns></returns>
		public int GetMessagesTotalSize()
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}
			if(m_SelectedFolder.Length == 0){
				throw new Exception("You must select folder first !");
			}

			int totalSize = 0;
			
			m_pSocket.WriteLine("a1 FETCH 1:* (RFC822.SIZE)");

			// Must get lines with * and cmdTag + OK or cmdTag BAD/NO
			string reply = m_pSocket.ReadLine();			
			if(reply.StartsWith("*")){
				// Read multiline response
				while(reply.StartsWith("*")){
					// Get rid of * 1 FETCH  and parse params. Reply:* 1 FETCH (UID 12 BODY[] ...)
					reply = reply.Substring(reply.IndexOf("FETCH (") + 7);
					
					// RFC822.SIZE field
					if(reply.ToUpper().StartsWith("RFC822.SIZE")){
						reply = reply.Substring(11).Trim(); // Remove RFC822.SIZE word from reply
						
						totalSize += Convert.ToInt32(reply.Substring(0,reply.Length - 1).Trim()); // Remove ending ')'						
					}					

					reply = m_pSocket.ReadLine();
				}
			}
			
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				if(!reply.ToUpper().StartsWith("NO")){
					throw new Exception("Server returned:" + reply);
				}
			}

			return totalSize;
		}

		#endregion

		#region method GetUnseenMessagesCount

		/// <summary>
		/// Gets unseen messages count in selected folder.
		/// </summary>
		/// <returns></returns>
		public int GetUnseenMessagesCount()
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}
			if(m_SelectedFolder.Length == 0){
				throw new Exception("You must select folder first !");
			}

			int count = 0;
			
			m_pSocket.WriteLine("a1 FETCH 1:* (FLAGS)");

			// Must get lines with * and cmdTag + OK or cmdTag BAD/NO
			string reply = m_pSocket.ReadLine();			
			if(reply.StartsWith("*")){
				// Read multiline response
				while(reply.StartsWith("*")){
					// Get rid of * 1 FETCH  and parse params. Reply:* 1 FETCH (UID 12 BODY[] ...)
					reply = reply.Substring(reply.IndexOf("FETCH (") + 7);
					
					if(reply.ToUpper().IndexOf("\\SEEN") == -1){
						count++;
					}

					reply = m_pSocket.ReadLine();
				}
			}
			
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				if(!reply.ToUpper().StartsWith("NO")){
					throw new Exception("Server returned:" + reply);
				}
			}

			return count;
		}

		#endregion

		#region method GetFolderSeparator

		/// <summary>
		/// Gets IMAP server folder separator char.
		/// </summary>
		/// <returns></returns>
		public string GetFolderSeparator()
		{
			if(!m_Connected){
				throw new Exception("You must connect first !");
			}
			if(!m_Authenticated){
				throw new Exception("You must authenticate first !");
			}

			string folderSeparator = "";

			m_pSocket.WriteLine("a1 LIST \"\" \"\"");

			// Must get lines with * and cmdTag + OK or cmdTag BAD/NO
			string reply = m_pSocket.ReadLine();			
			if(reply.StartsWith("*")){
				// Read multiline response
				while(reply.StartsWith("*")){
					reply = reply.Substring(reply.IndexOf(")") + 1).Trim(); // Remove * LIST(..)

					// get folder separator
					folderSeparator = reply.Substring(0,reply.IndexOf(" ")).Trim();

					reply = m_pSocket.ReadLine();
				}
			}
			
			reply = reply.Substring(reply.IndexOf(" ")).Trim(); // Remove Cmd tag

			if(!reply.ToUpper().StartsWith("OK")){
				if(!reply.ToUpper().StartsWith("NO")){
					throw new Exception("Server returned:" + reply);
				}
			}

			reply = reply.Substring(reply.IndexOf(")") + 1).Trim(); // Remove * LIST(..)


			return folderSeparator.Replace("\"","");
		}

		#endregion


		#region method RemoveCmdTag

		/// <summary>
		/// Removes command tag from response line.
		/// </summary>
		/// <param name="responseLine">Response line with command tag.</param>
		/// <returns></returns>
		private string RemoveCmdTag(string responseLine)
		{
			return responseLine.Substring(responseLine.IndexOf(" ")).Trim();
		}

		#endregion

		#region method ProcessStatusResponse

		/// <summary>
		/// Processes IMAP STATUS response and updates this class status info.
		/// </summary>
		/// <param name="statusResponse">IMAP STATUS response line.</param>
		private void ProcessStatusResponse(string statusResponse)
		{
			/* RFC 3501 7.3.1.  EXISTS Response
				Example:    S: * 23 EXISTS
			*/

			/* RFC 3501 7.3.2.  RECENT Response
				Example:    S: * 5 RECENT
			*/

			statusResponse = statusResponse.ToUpper();

			// Get rid of *
			statusResponse = statusResponse.Substring(1).Trim();

			if(statusResponse.IndexOf("EXISTS") > -1 && statusResponse.IndexOf("FLAGS") == -1){
				m_MsgCount = Convert.ToInt32(statusResponse.Substring(0,statusResponse.IndexOf(" ")).Trim());
			}
			else if(statusResponse.IndexOf("RECENT") > -1 && statusResponse.IndexOf("FLAGS") == -1){
				m_NewMsgCount = Convert.ToInt32(statusResponse.Substring(0,statusResponse.IndexOf(" ")).Trim());
			}
		}

		#endregion

		#region method IsStatusResponse

		/// <summary>
		/// Gets if specified line is STATUS response.
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private bool IsStatusResponse(string line)
		{
			if(!line.StartsWith("*")){
				return false;
			}

			// Remove *
			line = line.Substring(1).TrimStart();

			// RFC 3951 7.1.2.  NO Response (untagged)
			if(line.ToUpper().StartsWith("NO")){
				return true;
			}
			// RFC 3951 7.1.3.  BAD Response (untagged)
			else if(line.ToUpper().StartsWith("BAD")){
				return true;
			}

			// * 1 EXISTS
			// * 1 RECENT
            // * 1 EXPUNGE
			if(line.ToLower().IndexOf("exists") > -1){
				return true;
			}
			if(line.ToLower().IndexOf("recent") > -1 && line.ToLower().IndexOf("flags") == -1){
				return true;
			}
            else if(line.ToLower().IndexOf("expunge") > -1){
                return true;
            }

			return false;
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
        /// Gets IMAP server path separator char.
        /// </summary>
        public char PathSeparator
        {
            get{
                // Get separator
                if(m_PathSeparator == '\0'){
                    m_PathSeparator = GetFolderSeparator()[0];
                }

                return m_PathSeparator; 
            }
        }

		/// <summary>
		/// Gets selected folder.
		/// </summary>
		public string SelectedFolder
		{
			get{ return m_SelectedFolder; }
		}

        /// <summary>
        /// Gets folder UID.
        /// </summary>
        public long UIDValidity
        {
            get{ return m_UIDValidity; }
        }

        /// <summary>
        /// Gets next predicted message UID.
        /// </summary>
        public long UIDNext
        {
            get{ return m_UIDNext; }
        }

		/// <summary>
		/// Gets numbers of recent(not accessed messages) in selected folder.
		/// </summary>
		public int RecentMessagesCount
		{
			get{ return m_NewMsgCount; }
		}

		/// <summary>
		/// Gets numbers of messages in selected folder.
		/// </summary>
		public int MessagesCount
		{
			get{ return m_MsgCount; }
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
