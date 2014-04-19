using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace LumiSoft.Net.SIP.Stack
{
    /// <summary>
    /// This class represents SIP end point info, what includes protocol and IP end point.
    /// Normally this SIP end point info is used to find right UDP socket or TCP onnection.
    /// </summary>
    public class SIP_EndPointInfo
    {
        private string     m_Transport = "UDP";
        private IPEndPoint m_pEndPoint = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="transport">Transport.</param>
        /// <param name="endPoint">IP end point.</param>
        /// <exception cref="ArgumentException">Is raised when any of arguments has invalid value.</exception>
        public SIP_EndPointInfo(string transport,IPEndPoint endPoint)
        {
            if(string.IsNullOrEmpty(transport)){
                throw new ArgumentException("Argumnet 'transport' value may not be null or empty !");
            }
            if(endPoint == null){
                throw new ArgumentException("Argument 'endPoint' value may not be null !");
            }

            m_Transport = transport;
            m_pEndPoint = endPoint;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets transport.
        /// </summary>
        public string Transport
        {
            get{ return m_Transport; }
        }

        /// <summary>
        /// Gets IP end point.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get{ return m_pEndPoint; }
        }

        #endregion

    }
}
