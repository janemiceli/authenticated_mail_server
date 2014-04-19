using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SIP.Proxy
{
    /// <summary>
    /// This enum specifies SIP proxy server 'forking' mode.
    /// </summary>
    public enum SIP_ForkingMode
    {
        /// <summary>
        /// No forking. The contact with highest q value is used.
        /// </summary>
        None,

        /// <summary>
        /// All contacts are processed parallel at same time.
        /// </summary>
        Parallel,

        /// <summary>
        /// Contacts are processed from highest q value to lower.
        /// </summary>
        Sequential,
    }
}
