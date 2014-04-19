using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace LumiSoft.Net
{
    /// <summary>
    /// Socket for IP based protocols like TCP or UDP.
    /// </summary>
    public class IPSocket
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public IPSocket()
        {
        }

        public void Bind(IPEndPoint localEndPoint)
        {
        }

        public void Listen()
        {
        }

        public void Connect(bool ssl)
        {
        }

        public void Accept()
        {
        }

        public void Accept(string certificate)
        {
        }


        #region Properties Implementation

        public IPEndPoint LocalEndPoint
        {
            get{ return null; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get{ return null; }
        }

        public Stream Stream
        {
            get{ return null; }
        }

        #endregion

    }
}
