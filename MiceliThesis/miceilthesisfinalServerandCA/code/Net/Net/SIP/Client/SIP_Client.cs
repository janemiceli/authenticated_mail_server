using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

using LumiSoft.Net.SIP.Server;

namespace LumiSoft.Net.SIP.Client
{
    /// <summary>
    /// Implements SIP client. Defined in RFC 3261.
    /// </summary>
    public class SIP_Client
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_Client()
        {
        }


        #region method Connect

        /// <summary>
        /// Connects SIP client to specified destination host and port.
        /// </summary>
        public void Connect(string host,int port)
        {
        }

        #endregion

        #region method Disconnect

        /// <summary>
        /// Disconnects SIP client, releases all resources.
        /// </summary>
        public void Disconnect()
        {
        }

        #endregion

        /*
        public void CreateRegister(string to,SIP_Contact[] contacts)
        {
        }

        public void CreateInvite(string to,string from)
        {
        }
        
        public void SendMessage
        
         
         */
        
    }
}
