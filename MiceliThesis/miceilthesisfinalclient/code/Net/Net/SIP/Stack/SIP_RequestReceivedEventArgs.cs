using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace LumiSoft.Net.SIP.Stack
{
    /// <summary>
    /// This class provides data for SIP_Core.OnRequestReceived method.
    /// </summary>
    public class SIP_RequestReceivedEventArgs
    {
        private SIP_Stack             m_pStack       = null;
        private SIP_Request           m_pRequest     = null;
        private SIP_ServerTransaction m_pTransaction = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stack">Reference to SIP stack.</param>
        /// <param name="request">Recieved request.</param>
        internal SIP_RequestReceivedEventArgs(SIP_Stack stack,SIP_Request request)
        {
            m_pStack   = stack;
            m_pRequest = request;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets request received by SIP stack.
        /// </summary>
        public SIP_Request Request
        {
            get{ return m_pRequest; }
        }

        /// <summary>
        /// Gets server transaction for that request. Server transaction is created when this property is 
        /// first accessed. If you don't need server transaction for that request, for example statless proxy, 
        /// just don't access this property. For ACK method, this method always return null, because ACK 
        /// doesn't create transaction !
        /// </summary>
        public SIP_ServerTransaction ServerTransaction
        {
            get{
                // ACK never creates transaction.
                if(m_pRequest.Method == SIP_Methods.ACK){
                    return null;
                }

                // Create server transaction for that request.
                if(m_pTransaction == null){
                    m_pTransaction = m_pStack.TransactionLayer.CreateServerTransaction(m_pRequest);
                }

                return m_pTransaction; 
            }
        }

        /// <summary>
        /// Gets SIP dialog where Request belongs to. Returns null if Request doesn't belong any dialog.
        /// </summary>
        public SIP_Dialog Dialog
        {
            get{ throw new NotImplementedException(); }
        }

        #endregion

    }
}
