using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace LumiSoft.MailServer.API.UserAPI
{
    /// <summary>
    /// The BindInfo object represents services TCP listening info in LumiSoft Mail Server virtual server.
    /// </summary>
    public class BindInfo
    {
        private string    m_Protocol     = "";
        private IPAddress m_pIP          = null;
        private int       m_Port         = 0;
        private bool      m_SSL          = false;
        private byte[]    m_pCertificate = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="protocol">Network protocol.</param>
        /// <param name="ip">IP address.</param>
        /// <param name="port">Port.</param>
        /// <param name="ssl">Specifies if dedicated SSL conncetion.</param>
        /// <param name="certificate">SSL certificate.</param>
        public BindInfo(string protocol,IPAddress ip,int port,bool ssl,byte[] certificate)
        {
            m_Protocol     = protocol;
            m_pIP          = ip;
            m_Port         = port;
            m_SSL          = ssl;
            m_pCertificate = certificate;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets protocol.
        /// </summary>
        public string Protocol
        {
            get{ return m_Protocol; }
        }

        /// <summary>
        /// Gets listening IP address.
        /// </summary>
        public IPAddress IP
        {
            get{ return m_pIP; }
        }
                
        /// <summary>
        /// Gets listening port.
        /// </summary>
        public int Port
        {
            get{ return m_Port; }
        }

        /// <summary>
        /// Gets if dedicated SSL connection.
        /// </summary>
        public bool SSL
        {
            get{ return m_SSL; }
        }

        /// <summary>
        /// Gets SSL certificate.
        /// </summary>
        public byte[] Certificate
        {
            get{ return m_pCertificate; }
        }

        #endregion

    }
}
