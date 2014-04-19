using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace LumiSoft.Net.NNTP.Client
{
    /// <summary>
    /// NNTP client. Definex in RFC 977.
    /// </summary>
    public class NNTP_Client : IDisposable
    {
        private SocketEx m_pSocket   = null;
        private bool     m_Connected = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NNTP_Client()
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
        /// Connects to specified NNTP server.
        /// </summary>
        /// <param name="server">NNTP server.</param>
        /// <param name="port">NNTP server port. Defualt NNTP port is 119.</param>
        public void Connect(string server,int port)
        {
            if(m_Connected){
                throw new Exception("NNTP client is already connected, Disconnect first before calling Connect !");
            }

			m_pSocket = new SocketEx();
            m_pSocket.Connect(server,port);
            /*
			if(m_LogCmds && SessionLog != null){
				m_pLogger = new SocketLogger(s,SessionLog);
				m_pLogger.SessionID = Guid.NewGuid().ToString();
				m_pSocket.Logger = m_pLogger;
			}*/

			// Set connected flag
			m_Connected = true;

			// Read server response
			string responseLine = m_pSocket.ReadLine(1000);
            if(!responseLine.StartsWith("200")){
                throw new Exception(responseLine);
            }
        }

        #endregion

        #region method Disconnect

        /// <summary>
        /// Disconnects from NNTP server, trys to send QUIT command before.
        /// </summary>
        public void Disconnect()
        {
            try{
                if(m_Connected){
                    m_pSocket.WriteLine("QUIT");
                    m_pSocket.Shutdown(SocketShutdown.Both);                
                }
            }
            catch{
            }

            m_Connected = false;
            m_pSocket = null;
        }

        #endregion


        #region method GetNewsGroups

        /// <summary>
        /// Gets NNTP newsgoups.
        /// </summary>
        /// <returns></returns>
        public string[] GetNewsGroups()
        {
            /* RFC 977 3.6.1.  LIST

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
              
                Example:
                   C: LIST
                   S: 215 list of newsgroups follows
                   S: net.wombats 00543 00501 y
                   S: net.unix-wizards 10125 10011 y
                   S: .
            */

            if(!m_Connected){
				throw new Exception("You must connect first");
			}

            // Send LIST command
            m_pSocket.WriteLine("LIST");

            // Read server response
			string responseLine = m_pSocket.ReadLine(1000);
            if(!responseLine.StartsWith("215")){
                throw new Exception(responseLine);
            }

            List<string> newsGroups = new List<string>();
            responseLine = m_pSocket.ReadLine(1000);
            while(responseLine != "."){
                newsGroups.Add(responseLine.Split(' ')[0]);

                responseLine = m_pSocket.ReadLine(1000);
            }

            return newsGroups.ToArray();
        }

        #endregion

        #region method PostMessage

        /// <summary>
        /// Posts specified message to the specified newsgroup.
        /// </summary>
        /// <param name="newsgroup">Newsgroup where to post message.</param>
        /// <param name="message">Message to post. Message is taken from stream current position.</param>
        public void PostMessage(string newsgroup,Stream message)
        {
            /* RFC 977 3.10.1.  POST

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
              
                Example:
                    C: POST
                    S: 340 Continue posting; Period on a line by itself to end
                    C: (transmits news article in RFC850 format)
                    C: .
                    S: 240 Article posted successfully.
           */

            if(!m_Connected){
				throw new Exception("You must connect first");
			}

            // Send POST command
            m_pSocket.WriteLine("POST");

            // Read server response
			string responseLine = m_pSocket.ReadLine(1000);
            if(!responseLine.StartsWith("340")){
                throw new Exception(responseLine);
            }

            // POST message
            m_pSocket.WritePeriodTerminated(message);

            // Read server response
            responseLine = m_pSocket.ReadLine(1000);
            if(!responseLine.StartsWith("240")){
                throw new Exception(responseLine);
            }

        }

        #endregion


        #region Properties Implementation

        /// <summary>
		/// Gets if NNTP client is connected.
		/// </summary>
		public bool Connected
		{
			get{ return m_Connected; }
		}

        #endregion

    }
}
