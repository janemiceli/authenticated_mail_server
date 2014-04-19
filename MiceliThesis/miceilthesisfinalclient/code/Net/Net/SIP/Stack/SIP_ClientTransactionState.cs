using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SIP.Stack
{
    /// <summary>
    /// This value specifies SIP client transaction state.
    /// </summary>
    public enum SIP_ClientTransactionState
    {
        /// <summary>
        /// Calling to recipient. This is used only by INVITE transaction.
        /// </summary>
        Calling,

        /// <summary>
        /// 
        /// </summary>
        Trying,

        /// <summary>
        /// 
        /// </summary>
        Proceeding,

        /// <summary>
        /// 
        /// </summary>
        Completed,

        /// <summary>
        /// Transaction has terminated.
        /// </summary>
        Terminated
    }
}
