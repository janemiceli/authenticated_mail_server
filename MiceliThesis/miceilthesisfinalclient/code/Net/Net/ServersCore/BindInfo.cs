using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace LumiSoft.Net
{
    /// <summary>
    /// Holds socket bind info.
    /// </summary>
    public class BindInfo
    {
        private BindInfoProtocol m_Protocol     = BindInfoProtocol.TCP;
        private IPAddress        m_pIP          = null;
        private int              m_Port         = 0;
        private bool             m_SSL          = false;
        private X509Certificate  m_pCertificate = null;
        private object           m_Tag          = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="protocol">Bind protocol.</param>
        /// <param name="ip">IP address to listen.</param>
        /// <param name="port">Port to listen.</param>
        public BindInfo(BindInfoProtocol protocol,IPAddress ip,int port)
        {
            m_Protocol = protocol;
            m_pIP      = ip;
            m_Port     = port;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ip">IP address to listen.</param>
        /// <param name="port">Port to listen.</param>
        /// <param name="ssl">Specifies if dedicated SSL connection. If true, SSL connection negotiation 
        /// is done at once after connection is accepted.</param>
        /// <param name="sslCertificate">Certificate to use for SSL connections.</param>
        public BindInfo(IPAddress ip,int port,bool ssl,X509Certificate sslCertificate)
        {
            m_pIP          = ip;
            m_Port         = port;
            m_SSL          = ssl;
            m_pCertificate = sslCertificate;
        }


        #region override method Equals

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>Returns true if two objects are equal.</returns>
        public override bool Equals(object obj)
        {
            if(obj == null){
                return false;
            }
            if(!(obj is BindInfo)){
                return false;
            }

            BindInfo bInfo = (BindInfo)obj;
            if(bInfo.Protocol != m_Protocol){
                return false;
            }
            if(!bInfo.IP.Equals(m_pIP)){
                return false;
            }
            if(bInfo.Port != m_Port){
                return false;
            }
            if(bInfo.SSL != m_SSL){
                return false;
            }
            if(!X509Certificate.Equals(bInfo.SSL_Certificate,m_pCertificate)){
                return false;
            }

            return true;
        }

        #endregion

        #region override method GetHashCode

        /// <summary>
        /// Returns the hash code.
        /// </summary>
        /// <returns>Returns the hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets protocol.
        /// </summary>
        public BindInfoProtocol Protocol
        {
            get{ return m_Protocol; }
        }

        /// <summary>
        /// Gets IP address.
        /// </summary>
        public IPAddress IP
        {
            get{ return m_pIP; }
        }

        /// <summary>
        /// Gets port.
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
        public X509Certificate SSL_Certificate
        {
            get{ return m_pCertificate; }
        }

        /// <summary>
        /// Gets or sets user data. This is used internally don't use it !!!.
        /// </summary>
        public object Tag
        {
            get{ return m_Tag; }

            set{ m_Tag = value; }
        }

        #endregion

    }
}
