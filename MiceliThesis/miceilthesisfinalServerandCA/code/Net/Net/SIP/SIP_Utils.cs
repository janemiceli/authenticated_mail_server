using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP
{
    /// <summary>
    /// SIP helper methods.
    /// </summary>
    public class SIP_Utils
    {
        #region method ParseAddress

        /// <summary>
        /// Parses address from SIP To: header field.
        /// </summary>
        /// <param name="to">SIP header To: value.</param>
        /// <returns></returns>
        public static string ParseAddress(string to)
        {
            try{
                string retVal = to;
                if(to.IndexOf('<') > -1 && to.IndexOf('<') < to.IndexOf('>')){
                    retVal = to.Substring(to.IndexOf('<') + 1,to.IndexOf('>') - to.IndexOf('<') - 1);
                }
                // Remove sip:
                if(retVal.IndexOf(':') > -1){
                    retVal = retVal.Substring(retVal.IndexOf(':') + 1).Split(':')[0];
                }
                return retVal;
            }
            catch{
                throw new ArgumentException("Invalid SIP header To: '" + to + "' value !");
            }
        }

        #endregion

        #region method UriToRequestUri

        /// <summary>
        /// Converts URI to Request-URI by removing all not allowed Request-URI parameters from URI.
        /// </summary>
        /// <param name="uri">URI value.</param>
        /// <returns>Returns valid Request-URI value.</returns>
        public static string UriToRequestUri(string uri)
        {            
            // RFC 3261 19.1.2.(Table)
            // We need to strip off "method-param" and "header" URI parameters".
            // Currently we do it for sip or sips uri, do we need todo it for others too ?
            try{
                SIP_Uri sUri = SIP_Uri.Parse(uri);
                sUri.Parameters.Remove("method");
                sUri.Header = null;
                return sUri.ToStringValue();
            }
            catch{
                return uri;
            }            
        }

        #endregion

        #region method ToEndPointInfo

        /// <summary>
        /// Converts socket local or remote end point to SIP_EndPointInfo.
        /// </summary>
        /// <param name="socket">Socket to use.</param>
        /// <param name="local_remote">Specifies if loacl or remote end point of socket is used.</param>
        /// <returns>Returns SIP end point info.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>socket</b> is null.</exception>
        public static SIP_EndPointInfo ToEndPointInfo(SocketEx socket,bool local_remote)
        {
            if(socket == null){
                throw new ArgumentNullException("socket");
            }

            IPEndPoint ep = null;
            if(local_remote){
                ep = (IPEndPoint)socket.LocalEndPoint;
            }
            else{
                ep = (IPEndPoint)socket.RemoteEndPoint;
            }

            if(socket.RawSocket.ProtocolType == System.Net.Sockets.ProtocolType.Udp){
                return new SIP_EndPointInfo(SIP_Transport.UDP,ep);
            }
            else{
                return new SIP_EndPointInfo(SIP_Transport.TCP,ep);
            }
        }

        #endregion
    }
}
