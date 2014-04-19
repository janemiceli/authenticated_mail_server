using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SIP.Stack
{
    /// <summary>
    /// SIP server transaction state.
    /// </summary>
    public enum SIP_ServerTransactionState
    {
        /// <summary>
        /// This is transaction initial state. Used only in INVITE transaction.
        /// </summary>
        Proceeding,

        /// <summary>
        /// This is transaction initial state. Used only in Non-INVITE transaction.
        /// </summary>
        Trying,

        /// <summary>
        /// Transaction has got final response.
        /// </summary>
        Completed,

        /// <summary>
        /// Transation has got ACK from request maker. This is used only in INVITE.
        /// </summary>
        Confirmed,

        /// <summary>
        /// Transaction has terminated.
        /// </summary>
        Terminated,
    }
}
