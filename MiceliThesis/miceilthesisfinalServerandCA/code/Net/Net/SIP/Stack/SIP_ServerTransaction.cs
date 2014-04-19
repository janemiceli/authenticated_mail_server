using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Net;

using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack
{
    /// <summary>
    /// Implements SIP server transaction. Defined in RFC 3261 17.2 Server Transaction.
    /// A transaction is a sequence of SIP messages exchanged between SIP network elements. 
    /// Server transaction is created when remote UA (client transaction) request is received by local UA.
    /// Server transaction provides way to send responses back to remote UA client transaction.
    /// </summary>
    /// <remarks>
    /// <img src="../images/SIP_ServerTransaction.gif" />
    /// </remarks>
    public class SIP_ServerTransaction : SIP_Transaction
    {
        private SIP_Stack                   m_pSipStack                = null;
        private string                      m_ID                       = "";
        private SIP_Request                 m_pRequest                 = null;
        private SIP_ServerTransactionState  m_TransactionState         = SIP_ServerTransactionState.Proceeding;
        private Timer                       m_pTransactionTimeoutTimer = null;  
        private Timer                       m_pTimerG                  = null;
        private Timer                       m_pTimerH                  = null;
        private Timer                       m_pTimerI                  = null;
        private Timer                       m_pTimerJ                  = null;
        private int                         m_T1                       = 500;
        private int                         m_T2                       = 4000;
        private int                         m_T4                       = 5000;
        private List<SIP_Response>          m_pResponses               = null;
        private SIP_Dialog                  m_pDialog                  = null;
        private object                      m_pTag                     = null;
        private bool                        m_Disposed                 = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sipStack">Reference to SIP stack.</param>
        /// <param name="request">SIP request what caused to create server transaction.</param>
        internal SIP_ServerTransaction(SIP_Stack sipStack,SIP_Request request)
        {
            m_pSipStack = sipStack;
            m_pRequest  = request;

            m_ID         = request.Via.GetTopMostValue().Branch;
            m_pResponses = new List<SIP_Response>();

            Begin();
        }

        #region method Dispose

        /// <summary>
        /// Disposes transaction and cleans up all resources.
        /// </summary>
        public override void Dispose()
        {
            if(m_Disposed){
                return;
            }

            try{
                m_TransactionState = SIP_ServerTransactionState.Terminated;

                base.Dispose();

                if(m_pTransactionTimeoutTimer != null){
                    m_pTransactionTimeoutTimer.Dispose();
                    m_pTransactionTimeoutTimer = null;
                }
                            
                if(m_pTimerG != null){
                    m_pTimerG.Dispose();
                    m_pTimerG = null;
                }

                if(m_pTimerH != null){
                    m_pTimerH.Dispose();
                    m_pTimerH = null;
                }

                if(m_pTimerI != null){
                    m_pTimerI.Dispose();
                    m_pTimerI = null;
                }

                if(m_pTimerJ != null){
                    m_pTimerJ.Dispose();
                    m_pTimerJ = null;
                }
                                                                
                // Log
                m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) disposed.");
            }
            finally{
                // Remove from transactions collection.
                m_pSipStack.TransactionLayer.RemoveServerTransaction(this);

                OnTerminated();
            }

            m_Disposed = true;
        }

        #endregion


        #region method Begin

        /// <summary>
        /// Starts processing server transaction.
        /// </summary>
        private void Begin()
        {
            // Log
            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) created.");

            // INVITE server transaction
            if(m_pRequest.Method == "INVITE"){
                /*  When a server transaction is constructed for a request, it enters the
                    "Proceeding" state. The server transaction MUST generate a 100 (Trying) response.
                */

                m_TransactionState = SIP_ServerTransactionState.Proceeding;                

                SendTrying();
            }
            // Non-INVITE server transaction
            else{
                /* When a server transaction is constructed for a request, it enters the "Trying" state.
                */

                m_TransactionState = SIP_ServerTransactionState.Trying;
            }

            // This is just timout timer and ensures that transaction ends or will terminated.
            m_pTransactionTimeoutTimer = new Timer(120 * m_T1);
            m_pTransactionTimeoutTimer.Elapsed += new ElapsedEventHandler(m_pTransactionTimeoutTimer_Elapsed);
            m_pTransactionTimeoutTimer.Enabled = true;
            // Log
            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Transaction timeout Timer started, will triger after " + m_pTransactionTimeoutTimer.Interval + ".");
        }

        #endregion

        #region method Cancel

        /// <summary>
        /// Cancels transaction. NOTE: Only INVITE transaction can be canceled, for other methods cancel is skipped.
        /// If If final response is already sent, then cancel has no effect and will be skipped.
        /// </summary>
        public override void Cancel()
        {   
            /* RFC 3261 9.2.
                If we have sent final response, skip cancel, because it has no effect. If the UAS has not 
                issued a final response for the original request, its behavior depends on the method of the 
                original request. If the original request was an INVITE, the UAS SHOULD immediately respond to 
                the INVITE with a 487 (Request Terminated). A CANCEL request has no impact on the processing of 
                transactions with any other method defined in this specification.
            */

            lock(this){
                // If final response sent, skip cancel.
                if(GetFinalResponse() != null){
                    return;
                }
                else{
                    // Only INVITE can be canceled.
                    if(m_pRequest.Method == SIP_Methods.INVITE){
                        SendResponse(m_pRequest.CreateResponse(SIP_ResponseCodes.x487_Request_Terminated));
                        // We no normally must get ACK now, if not timeout timers will 
                        // take care of disposing this transaction.
                                                   
                        OnCanceled();

                        // Log
                        m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) canceled.");
                    }
                }
            }
        }

        #endregion

        #region method SendResponse
        
        /// <summary>
        /// Sends a SIP response message to the client transaction whose request initiated this server transaction.
        /// The response value must be SIP_ServerTransaction.Request.CreateResponse value.
        /// </summary>
        /// <param name="response">SIP response to send.</param>
        public void SendResponse(SIP_Response response)
        {
            ProcessResponse(response);
        }

        #endregion
                        
        
        #region method m_pTimerG_Elapsed

        /// <summary>
        /// Is called when INVITE response retransission must be done.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_pTimerG_Elapsed(object sender,ElapsedEventArgs e)
        { 
            /* RFC 3261 17.2.1.
                When timer G fires, the server transaction MUST retransmit the response by passing 
                it to the transport layer, and MUST reset the timer with a value of MIN(2*T1,T2).
                For the default values of T1 and T2, this results in intervals of 500 ms, 1 s, 2 s, 4 s, 4 s, etc.
            */

            // Log.
            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Timer G(INVITE response retransmission interval) triggered,next transmit after " + m_pTimerG.Interval + " ms.");
    
            // Retransmits response.
            m_pSipStack.TransportLayer.SendResponse(m_pRequest.Socket,GetFinalResponse());

            // Update interval.
            m_pTimerG.Interval = Math.Min(m_pTimerG.Interval * 2,m_T2);
        }

        #endregion

        #region method m_pTimerH_Elapsed

        /// <summary>
        /// Is called when INVITE completed state ACK wait time reached.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_pTimerH_Elapsed(object sender,ElapsedEventArgs e)
        {
            /* RFC 3261 17.2.1.
                If timer H fires while in the "Completed" state, it implies that the
                ACK was never received.  In this case, the server transaction MUST
                transition to the "Terminated" state, and MUST indicate to the TU
                that a transaction failure has occurred.
            */

            // Log.
            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Timer H(Wait time for ACK receipt) triggered.");

            if(m_TransactionState == SIP_ServerTransactionState.Completed){
                Dispose();

                // Notify TU
                OnTransactionError("ACK never received !");
            }
        }

        #endregion

        #region merhod m_pTimerI_Elapsed

        /// <summary>
        /// Is called when INVITE Confirmed linger time completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_pTimerI_Elapsed(object sender,ElapsedEventArgs e)
        {
            // Log.
            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Timer I(Wait time for ACK retransmissions) triggered.");

            Dispose();
        }

        #endregion

        #region mehtod m_pTimerJ_Elapsed

        /// <summary>
        /// Is called when non-INVITE Confirmed linger time completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_pTimerJ_Elapsed(object sender,ElapsedEventArgs e)
        {
            /* RFC 3261 17.2.2
                Timer J fires, at which point it MUST transition to the "Terminated" state.
            */

            // Log.
            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Timer J(Wait time for retransmissions of non-INVITE requests) triggered.");

            Dispose();
        }

        #endregion

        #region method m_pTransactionTimeoutTimer_Elapsed

        /// <summary>
        /// Is called when transaction has timed out.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_pTransactionTimeoutTimer_Elapsed(object sender,ElapsedEventArgs e)
        { 
            // Log.
            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Transaction timeout timer triggered.");

            OnTimedOut();           
            Dispose();
        }

        #endregion

                       
        #region method ProcessRequest

        /// <summary>
        /// Processes transaction request.
        /// </summary>
        /// <param name="request">SIP request.</param>
        internal void ProcessRequest(SIP_Request request)
        {   
            // TODO: We MAY accept only ACK,CANCEL and request retransmission.

            // Log
            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) got request '" + request.Method +  "'.");

            lock(this){
            
                #region ACK

                if(request.Method == SIP_Methods.ACK){
                    /* RFC 3261 17.2.1.
                        If an ACK is received while the server transaction is in the
                        "Completed" state, the server transaction MUST transition to the
                        "Confirmed" state.  As Timer G is ignored in this state, any
                        retransmissions of the response will cease.
                
                        Also kill timer H(ACK wait timeout timer) because we got ACK.
                    */
                    if(m_TransactionState == SIP_ServerTransactionState.Completed){
                        m_TransactionState = SIP_ServerTransactionState.Confirmed;

                        // If there is timer G (response retransmit timer), stop it,because we got ACK.
                        if(m_pTimerG != null){
                            m_pTimerG.Dispose();
                            m_pTimerG = null;

                            // Log
                            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Timer G (response retransmit timer) stopped.");
                        }
                        // If there is timer H (ACK wait timeout timer), stop it because we got ACK.
                        if(m_pTimerH != null){
                            m_pTimerH.Dispose();
                            m_pTimerH = null;

                            // Log
                            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Timer H (ACK wait timeout timer) stopped.");
                        }

                        /* RFC 3261 17.2.1.
                            The purpose of the "Confirmed" state is to absorb any additional ACK
                            messages that arrive, triggered from retransmissions of the final response.
                            When confirmed state is entered, timer I is set to fire in T4
                            seconds for unreliable transports, and zero seconds for reliable transports.
                        */
                        if(request.Via.GetTopMostValue().ProtocolTransport.ToUpper() == "UDP"){
                            m_pTimerI = new Timer(m_T4);
                            m_pTimerI.AutoReset = false;
                            m_pTimerI.Elapsed += new ElapsedEventHandler(m_pTimerI_Elapsed);

                            // Log
                            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Timer I (additional ACK absorb) started, will triger after " + m_pTimerI.Interval + ".");
                        }
                        else{
                            Dispose();
                        }
                    }
                }

                #endregion

                #region CANCEL

                else if(request.Method == SIP_Methods.CANCEL){
                    // Do new server transaction for cancel.
                    // We bind RFC here, rfc says UAS needs to respond canel, we hide cancel stuff from user. 
                    // If somebody knows why we can't do so, let me know.
                    m_pSipStack.TransactionLayer.CreateServerTransaction(request).SendResponse(request.CreateResponse(SIP_ResponseCodes.x200_Ok));

                    Cancel();
                }

                #endregion

                #region INVITE

                // INVITE server transaction
                else if(request.Method == SIP_Methods.INVITE){
                    /*
                        If a retransmission of the request is received while in the "Proceeding" state, the most
                        recently sent provisional response MUST be passed to the transport layer for retransmission. 
                  
                        In the "Completed" state, the server transaction MUST pass the final response to the transport
                        layer for retransmission whenever a retransmission of the request is received.  
                    */

                    if(m_TransactionState == SIP_ServerTransactionState.Proceeding){
                        SIP_Response lastProvisionalResponse = GetLastProvisionalResponse();
                        if(lastProvisionalResponse != null){
                            m_pSipStack.TransportLayer.SendResponse(request.Socket,lastProvisionalResponse);
                        }
                    }
                    else if(m_TransactionState == SIP_ServerTransactionState.Completed){
                        m_pSipStack.TransportLayer.SendResponse(request.Socket,GetFinalResponse());
                    }
                }

                #endregion

                #region Non-INVITE

                // Non-INVITE server transaction
                else{
                    /*
                        In the "Trying" state, any further request retransmissions are discarded. 
                  
                        If a retransmission of the request is received while in the "Proceeding" state, the most
                        recently sent provisional response MUST be passed to the transport layer for retransmission. 
                  
                        In the "Completed" state, the server transaction MUST pass the final response to the transport
                        layer for retransmission whenever a retransmission of the request is received.               
                    */

                    if(m_TransactionState == SIP_ServerTransactionState.Trying){
                        // Do nothing
                    }
                    else if(m_TransactionState == SIP_ServerTransactionState.Proceeding){
                        m_pSipStack.TransportLayer.SendResponse(request.Socket,GetLastProvisionalResponse());
                    }
                    else if(m_TransactionState == SIP_ServerTransactionState.Completed){
                        m_pSipStack.TransportLayer.SendResponse(request.Socket,GetFinalResponse());
                    }
                }

                #endregion

            }
        }
                
        #endregion

        #region method ProcessResponse

        /// <summary>
        /// Processes specified SIP response through this transaction.
        /// </summary>
        /// <param name="response">SIP response to process.</param>
        internal void ProcessResponse(SIP_Response response)
        {     
            #region INVITE

            // INVITE server transaction
            if(m_pRequest.Method == "INVITE"){

                #region Proceeding

                // Proceeding state
                if(m_TransactionState == SIP_ServerTransactionState.Proceeding){
                    // 1xx response
                    if(response.StausCodeType == SIP_StatusCodeType.Provisional){
                        // We don't forward 100 Trying, skip it
                        if(response.StatusCode == 100){
                            return;
                        }

                        m_pResponses.Add(response);

                        EnsureDialog(response);
                        PassResponseToDialog(response);

                        // Send unreliably (Pass to tranport layer)
                        m_pSipStack.TransportLayer.SendResponse(m_pRequest.Socket,m_pRequest.RemoteEndPoint,response);
                    }
                    // 2xx response
                    else if(response.StausCodeType == SIP_StatusCodeType.Success){
                        EnsureDialog(response);
                        PassResponseToDialog(response);

                        // Send unreliably (Pass to tranport layer)
                        m_pSipStack.TransportLayer.SendResponse(m_pRequest.Socket,m_pRequest.RemoteEndPoint,response);

                        m_TransactionState = SIP_ServerTransactionState.Terminated;
                        Dispose();
                    }
                    // 300-699 response
                    else{
                        m_TransactionState = SIP_ServerTransactionState.Completed;
                        m_pResponses.Add(response);

                        // Send unreliably (Pass to tranport layer)
                        m_pSipStack.TransportLayer.SendResponse(m_pRequest.Socket,m_pRequest.RemoteEndPoint,response);
                        
                        /* RFC 3261 17.2.1.
                            When the "Completed" state is entered, timer H MUST be set to fire in
                            64*T1 seconds for all transports.  Timer H determines when the server
                            transaction abandons retransmitting the response.
                        */
                        m_pTimerH = new Timer(64 * m_T1);
                        m_pTimerH.AutoReset = false;
                        m_pTimerH.Elapsed += new ElapsedEventHandler(m_pTimerH_Elapsed);
                        // Log
                        m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Timer H(response retransmit timeout timer) started, will triger after " + m_pTimerH.Interval + ".");
  
                        /* RFC 3261 17.2.1.
                            For unreliable transports, timer G is set to fire in T1 seconds, 
                            and is not set to fire for reliable transports.
                            (INVITE final response retransmit timer)
                        */
                        if(response.Via.GetTopMostValue().ProtocolTransport.ToUpper() == "UDP"){
                            m_pTimerG = new Timer(m_T1);
                            m_pTimerG.Elapsed += new ElapsedEventHandler(m_pTimerG_Elapsed);
                            m_pTimerG.Enabled = true;

                            // Log
                            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Timer G(INVITE final response retransmit timer) started, will triger after " + m_pTimerG.Interval + ".");
                        }
                    }
                }

                #endregion

                #region Completed

                // Completed state
                else if(m_TransactionState == SIP_ServerTransactionState.Completed){
                    // We do nothing here, we just wait ACK to arrive.
                }

                #endregion

                #region Confirmed

                else if(m_TransactionState == SIP_ServerTransactionState.Confirmed){
                    // We do nothing, just wait ACK retransmissions while linger time.
                }

                #endregion

                #region Terminated

                else if(m_TransactionState == SIP_ServerTransactionState.Terminated){
                   // We should never rreach here, but if so, skip it.
                }

                #endregion
            }

            #endregion

            #region Non-INVITE

            // Non-INVITE server transaction
            else{

                #region Trying

                // Trying state
                if(m_TransactionState == SIP_ServerTransactionState.Trying){
                    // 1xx response
                    if(response.StausCodeType == SIP_StatusCodeType.Provisional){
                        m_TransactionState = SIP_ServerTransactionState.Proceeding;
                        m_pResponses.Add(response);

                        // Send unreliably (Pass to tranport layer)
                        m_pSipStack.TransportLayer.SendResponse(m_pRequest.Socket,m_pRequest.RemoteEndPoint,response);                        
                    }
                    // 200-699 response
                    else{
                        m_TransactionState = SIP_ServerTransactionState.Completed;
                        m_pResponses.Add(response);

                        // Send unreliably (Pass to tranport layer)
                        m_pSipStack.TransportLayer.SendResponse(m_pRequest.Socket,m_pRequest.RemoteEndPoint,response);

                        /* RFC 3261 17.2.2.
                            When the server transaction enters the "Completed" state, it MUST set
                            Timer J to fire in 64*T1 seconds for unreliable transports.
                            (wait time for retransmissions of non-INVITE requests)
                        */
                        if(response.Via.GetTopMostValue().ProtocolTransport.ToUpper() == "UDP"){
                            m_pTimerJ = new Timer(64 * m_T1);
                            m_pTimerJ.AutoReset = false;
                            m_pTimerJ.Elapsed += new ElapsedEventHandler(m_pTimerJ_Elapsed);
                            m_pTimerJ.Enabled = true;

                            // Log
                            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Timer J(wait time for retransmissions of non-INVITE requests) started, will triger after " + m_pTimerJ.Interval + ".");
                        }
                        else{
                            Dispose();
                        }
                    }
                }

                #endregion

                #region Proceeding

                // Proceeding state
                else if(m_TransactionState == SIP_ServerTransactionState.Proceeding){
                    // 1xx response
                    if(response.StausCodeType == SIP_StatusCodeType.Provisional){
                        m_pResponses.Add(response);

                        // Send unreliably (Pass to tranport layer)
                        m_pSipStack.TransportLayer.SendResponse(m_pRequest.Socket,m_pRequest.RemoteEndPoint,response);
                    }
                    // 200-699 response
                    else{
                        m_TransactionState = SIP_ServerTransactionState.Completed;
                        m_pResponses.Add(response);

                        // Send unreliably (Pass to tranport layer)
                        m_pSipStack.TransportLayer.SendResponse(m_pRequest.Socket,m_pRequest.RemoteEndPoint,response);

                        /* RFC 3261 17.2.2.
                            When the server transaction enters the "Completed" state, it MUST set
                            Timer J to fire in 64*T1 seconds for unreliable transports.
                            (wait time for retransmissions of non-INVITE requests)
                        */
                        if(response.Via.GetTopMostValue().ProtocolTransport.ToUpper() == "UDP"){
                            m_pTimerJ = new Timer(64 * m_T1);
                            m_pTimerJ.AutoReset = false;
                            m_pTimerJ.Elapsed += new ElapsedEventHandler(m_pTimerJ_Elapsed);
                            m_pTimerJ.Enabled = true;

                            // Log
                            m_pSipStack.Logger.AddDebug("Transaction(id='" + m_ID + "' method=" + m_pRequest.Method + " server=true) Timer J(wait time for retransmissions of non-INVITE requests) started, will triger after " + m_pTimerJ.Interval + ".");
                        }
                    }
                }

                #endregion

                #region Completed

                // Completed state
                else if(m_TransactionState == SIP_ServerTransactionState.Completed){
                    // Just resend last response.
                    m_pSipStack.TransportLayer.SendResponse(m_pRequest.Socket,m_pRequest.RemoteEndPoint,GetFinalResponse());
                }

                #endregion

                #region Terminated

                // Terminated state
                else if(m_TransactionState == SIP_ServerTransactionState.Terminated){
                    // We should never rreach here, but if so, skip it.
                }

                #endregion
            }

            #endregion
        }
                                               
        #endregion


        #region method SendTrying

        /// <summary>
        /// Sends trying response to request maker.
        /// </summary>
        private void SendTrying()
        {
            /* RFC 3261 8.2.6.1 Sending a Provisional Response.
                When a 100 (Trying) response is generated, any Timestamp header field
                present in the request MUST be copied into this 100 (Trying)
                response.  If there is a delay in generating the response, the UAS
                SHOULD add a delay value into the Timestamp value in the response.
            */

            SIP_Response sipTryingResponse = new SIP_Response();                        
            sipTryingResponse.StatusCode_ReasonPhrase = SIP_ResponseCodes.x100_Trying;
            sipTryingResponse.Via.AddToTop(m_pRequest.Via.GetTopMostValue().ToStringValue());
            sipTryingResponse.To = m_pRequest.To;
            sipTryingResponse.From = m_pRequest.From;
            sipTryingResponse.CallID = m_pRequest.CallID;
            sipTryingResponse.CSeq = m_pRequest.CSeq;

            // Add time stamp, because we send Trying at once, we don't have delay.
            if(m_pRequest.Timestamp != null){
                sipTryingResponse.Timestamp = new SIP_t_Timestamp(m_pRequest.Timestamp.Time,0);
            }

            // Send response
            m_pSipStack.TransportLayer.SendResponse(m_pRequest.Socket,m_pRequest.RemoteEndPoint,sipTryingResponse);
        }

        #endregion

        #region method PassResponseToDialog

        /// <summary>
        /// Passes specified response to SIP dialog. If dialog doesn't exist, nothing is done.
        /// </summary>
        /// <param name="response">SIP response.</param>
        private void PassResponseToDialog(SIP_Response response)
        {
            lock(this){
                if(this.Dialog != null){
                    this.Dialog.ProcessResponse(response);
                }
            }
        }

        #endregion

        #region method EnsureDialog

        /// <summary>
        /// Ensures that SIP dialog exists. If not, creates SIP dialog, if exists, updates dialog state.
        /// </summary>
        /// <param name="response">SIP response what causes dialog creation.</param>
        private void EnsureDialog(SIP_Response response)
        {
            lock(this){
                // Create dialog
                if(this.CanCreateDialog && this.Dialog == null){
                    m_pDialog = m_pSipStack.TransactionLayer.CreateDialog(this,m_pRequest,response);
                }
            }
        }

        #endregion
                

        #region Properties Implementation

        /// <summary>
        /// Gets transaction ID (Via: branch parameter value).
        /// </summary>
        public override string ID
        {
            get{ return m_ID; }
        }

        /// <summary>
        /// Gets current transaction state.
        /// </summary>
        public SIP_ServerTransactionState TransactionState
        {
            get{ return m_TransactionState; }
        }

        /// <summary>
        /// Gets request what created this server transaction.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public override SIP_Request Request
        {
            get{ 
                if(m_Disposed){
                    throw new ObjectDisposedException("SIP_ServerTransaction");
                }

                return m_pRequest; 
            }
        }

        /// <summary>
        /// Gets transaction related responses.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public override SIP_Response[] Responses
        {
            get{ 
                if(m_Disposed){
                    throw new ObjectDisposedException("SIP_ServerTransaction");
                }

                return m_pResponses.ToArray(); 
            }
        }

        /// <summary>
        /// Gets transaction dialog. Returns null if no dialog available.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public override SIP_Dialog Dialog
        {
            get{ 
                if(m_Disposed){
                    throw new ObjectDisposedException("SIP_ServerTransaction");
                }

                return m_pDialog; 
            }
        }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag
        {
            get{ return m_pTag; }

            set{ m_pTag = value; }
        }

        #endregion

        #region Events Implementation
                
        #endregion

    }
}
