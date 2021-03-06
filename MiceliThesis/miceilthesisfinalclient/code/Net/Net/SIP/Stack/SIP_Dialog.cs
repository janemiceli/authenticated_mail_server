using System;
using System.Collections.Generic;
using System.Text;

using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack
{
    /// <summary>
    /// Implements SIP dialog. Defined in rfc 3261 12 Dialogs.
    /// </summary>
    /// <remarks>
    /// <img src="../images/SIP_Dialog.gif" />
    /// </remarks>
    public class SIP_Dialog : IDisposable
    {
        private SIP_DialogState      m_DialogState      = SIP_DialogState.Early;
        private SIP_Stack            m_pStack           = null;
        private string               m_Method           = "";
        private string               m_CallID           = "";
        private string               m_LocalTag         = "";
        private string               m_RemoteTag        = "";
        private int                  m_LocalSequenceNo  = 0;
        private int                  m_RemoteSequenceNo = 0;
        private string               m_LocalUri         = "";
        private string               m_RemoteUri        = "";
        private string               m_RemoteTarget     = "";
        private SIP_t_AddressParam[] m_pRouteSet        = null;
        private bool                 m_IsSecure         = false;
        private bool                 m_IsServer         = false;
        private object               m_pTag             = null;

        /// <summary>
        /// Default UAS constructor.
        /// </summary>
        /// <param name="stack">Reference to SIP stack.</param>
        /// <param name="request">Server transaction request what response it is.</param>
        /// <param name="response">Response what caused that dialog creation.</param>
        internal SIP_Dialog(SIP_Stack stack,SIP_Request request,SIP_Response response)
        {
            m_pStack = stack;

            InitUAS(request,response);
        }

        /// <summary>
        /// Default UAC constructor.
        /// </summary>
        /// <param name="stack">Reference to SIP stack.</param>
        /// <param name="response">Response what caused that dialog creation.</param>
        internal SIP_Dialog(SIP_Stack stack,SIP_Response response)
        {
            m_pStack = stack;

            InitUAC(response);
        }

        #region mehtod Dispose

        /// <summary>
        /// Cleans up any resources being used. 
        /// Removes dialog from server dialogs, sets dialog state to terminated, ... .
        /// </summary>
        public void Dispose()
        {            
            m_DialogState = SIP_DialogState.Terminated;

            // Remove dialog for dialogs collection.
            m_pStack.TransactionLayer.RemoveDialog(this);

            OnTerminated();

            m_pStack.Logger.AddDebug("Dialog(id='" + this.DialogID + "') terminated.");
        }

        #endregion

        #region method InitUAS

        /// <summary>
        /// Initializes UAS SIP dialog.
        /// </summary>
        /// <param name="request">SIP request what caused dialog creation.</param>
        /// <param name="response">Response what caused that dialog creation.</param>
        private void InitUAS(SIP_Request request,SIP_Response response)
        {
            /* RFC 3261 12.1.1 UAS behavior.
                When a UAS responds to a request with a response that establishes a
                dialog (such as a 2xx to INVITE), the UAS MUST copy all Record-Route
                header field values from the request into the response (including the
                URIs, URI parameters, and any Record-Route header field parameters,
                whether they are known or unknown to the UAS) and MUST maintain the
                order of those values.  The UAS MUST add a Contact header field to
                the response.  The Contact header field contains an address where the
                UAS would like to be contacted for subsequent requests in the dialog
                (which includes the ACK for a 2xx response in the case of an INVITE).
                Generally, the host portion of this URI is the IP address or FQDN of
                the host.  The URI provided in the Contact header field MUST be a SIP
                or SIPS URI.  If the request that initiated the dialog contained a
                SIPS URI in the Request-URI or in the top Record-Route header field
                value, if there was any, or the Contact header field if there was no
                Record-Route header field, the Contact header field in the response
                MUST be a SIPS URI.  The URI SHOULD have global scope (that is, the
                same URI can be used in messages outside this dialog).  The same way,
                the scope of the URI in the Contact header field of the INVITE is not
                limited to this dialog either.  It can therefore be used in messages
                to the UAC even outside this dialog.

                If the request arrived over TLS, and the Request-URI contained a SIPS
                URI, the "secure" flag is set to TRUE.

                The route set MUST be set to the list of URIs in the Record-Route
                header field from the request, taken in order and preserving all URI
                parameters.  If no Record-Route header field is present in the
                request, the route set MUST be set to the empty set.  This route set,
                even if empty, overrides any pre-existing route set for future
                requests in this dialog.  The remote target MUST be set to the URI
                from the Contact header field of the request.

                The remote sequence number MUST be set to the value of the sequence
                number in the CSeq header field of the request.  The local sequence
                number MUST be empty.  The call identifier component of the dialog ID
                MUST be set to the value of the Call-ID in the request.  The local
                tag component of the dialog ID MUST be set to the tag in the To field
                in the response to the request (which always includes a tag), and the
                remote tag component of the dialog ID MUST be set to the tag from the
                From field in the request.  

                The remote URI MUST be set to the URI in the From field, and the
                local URI MUST be set to the URI in the To field.
            */

            m_Method           = request.Method;
            m_CallID           = request.CallID;
            m_LocalTag         = response.To.ToTag;
            m_RemoteTag        = request.From.Tag;
            m_LocalSequenceNo  = 0;
            m_RemoteSequenceNo = request.CSeq.SequenceNumber;
            m_LocalUri         = request.To.Address.Uri;
            m_RemoteUri        = request.From.Address.Uri;
            m_RemoteTarget     = request.Contact.GetTopMostValue().Address.Uri;
            m_pRouteSet        = request.RecordRoute.GetAllValues();
            m_IsSecure         = false;
            m_IsServer         = true;
            if(response.StausCodeType == SIP_StatusCodeType.Success){
                m_DialogState = SIP_DialogState.Confirmed;
            }
            else{
                m_DialogState = SIP_DialogState.Early;
            }

            m_pStack.Logger.AddDebug("Dialog(id='" + this.DialogID + "' state=" + m_DialogState + " server=" + m_IsServer + ") created.");
        }

        #endregion

        #region method InitUAC

        /// <summary>
        /// Initializes UAC SIP dialog.
        /// </summary>
        /// <param name="response">SIP response what caused dialog creation.</param>
        private void InitUAC(SIP_Response response)
        {           
            /* RFC 3261 12.1.2 UAC Behavior
                When a UAC sends a request that can establish a dialog (such as an
                INVITE) it MUST provide a SIP or SIPS URI with global scope (i.e.,
                the same SIP URI can be used in messages outside this dialog) in the
                Contact header field of the request.  If the request has a Request-
                URI or a topmost Route header field value with a SIPS URI, the
                Contact header field MUST contain a SIPS URI.
                If the request was sent over TLS, and the Request-URI contained a
                SIPS URI, the "secure" flag is set to TRUE.

                The route set MUST be set to the list of URIs in the Record-Route
                header field from the response, taken in reverse order and preserving
                all URI parameters.  If no Record-Route header field is present in
                the response, the route set MUST be set to the empty set.  This route
                set, even if empty, overrides any pre-existing route set for future
                requests in this dialog.  The remote target MUST be set to the URI
                from the Contact header field of the response.

                The local sequence number MUST be set to the value of the sequence
                number in the CSeq header field of the request.  The remote sequence
                number MUST be empty (it is established when the remote UA sends a
                request within the dialog).  The call identifier component of the
                dialog ID MUST be set to the value of the Call-ID in the request.
                The local tag component of the dialog ID MUST be set to the tag in
                the From field in the request, and the remote tag component of the
                dialog ID MUST be set to the tag in the To field of the response. 

                The remote URI MUST be set to the URI in the To field, and the local
                URI MUST be set to the URI in the From field.            
            */

            m_Method           = response.CSeq.RequestMethod.ToUpper();
            m_CallID           = response.CallID;
            m_LocalTag         = response.From.Tag;
            m_RemoteTag        = response.To.ToTag;
            m_LocalSequenceNo  = response.CSeq.SequenceNumber;
            m_RemoteSequenceNo = 0;
            m_LocalUri         = response.From.Address.Uri;
            m_RemoteUri        = response.To.Address.Uri;
            m_RemoteTarget     = response.Contact.GetTopMostValue().Address.Uri;
            m_pRouteSet        = response.RecordRoute.GetAllValues();
            m_IsSecure         = false;
            m_IsServer         = false;
            if(response.StausCodeType == SIP_StatusCodeType.Success){
                m_DialogState = SIP_DialogState.Confirmed;
            }
            else{
                m_DialogState = SIP_DialogState.Early;
            }

            m_pStack.Logger.AddDebug("Dialog(id='" + this.DialogID + "' state=" + m_DialogState + " server=" + m_IsServer + ") created.");
        }

        #endregion

// TODO BYE
        #region method CreateRequest

        /// <summary>
        /// Creates new SIP request using this dialog info.
        /// </summary>
        /// <param name="method">SIP request method.</param>
        /// <returns>Returns created SIP request.</returns>
        public SIP_Request CreateRequest(string method)
        {
            return CreateRequest(method,true);
        }

        /// <summary>
        /// Creates new SIP request using this dialog info.
        /// </summary>
        /// <param name="method">SIP request method.</param>
        /// <param name="addCSeq">Specifies if CSeq header field is increased and added to request.</param>
        /// <returns>Returns created SIP request.</returns>
        private SIP_Request CreateRequest(string method,bool addCSeq)
        {
            /* RFC 3261 12.2.1.1 Generating the Request.
                The URI in the To field of the request MUST be set to the remote URI
                from the dialog state.  The tag in the To header field of the request
                MUST be set to the remote tag of the dialog ID.  The From URI of the
                request MUST be set to the local URI from the dialog state.  The tag
                in the From header field of the request MUST be set to the local tag
                of the dialog ID.  If the value of the remote or local tags is null,
                the tag parameter MUST be omitted from the To or From header fields,
                respectively.
                The Call-ID of the request MUST be set to the Call-ID of the dialog.
                Requests within a dialog MUST contain strictly monotonically
                increasing and contiguous CSeq sequence numbers (increasing-by-one).
             
                The UAC uses the remote target and route set to build the Request-URI
                and Route header field of the request.                
            */

            if(m_DialogState == SIP_DialogState.Early || m_DialogState == SIP_DialogState.Terminated){
                throw new Exception("Invalid dialog state for CreateRequest method !");
            }

            SIP_Request request = new SIP_Request();
            request.Method = method;           
            request.To = new SIP_t_To(this.RemoteUri);
            request.To.ToTag = this.RemoteTag;
            request.From = new SIP_t_From(this.LocalUri);
            request.From.Tag = this.LocalTag;
            request.CallID = this.CallID;
            // TODO: request.Contact
            m_LocalSequenceNo++;
            request.CSeq = new SIP_t_CSeq(this.LocalSequenceNo,method);
                        
            // If the route set is empty, the UAC MUST place the remote target URI into the Request-URI.
            if(this.Routes.Length == 0){
                request.Uri = this.RemoteTarget;
            }
            else{
                /*
                    If the route set is not empty, and the first URI in the route set
                    contains the lr parameter (see Section 19.1.1), the UAC MUST place
                    the remote target URI into the Request-URI and MUST include a Route
                    header field containing the route set values in order, including all
                    parameters.
                  
                    If the route set is not empty, and its first URI does not contain the
                    lr parameter, the UAC MUST place the first URI from the route set
                    into the Request-URI, stripping any parameters that are not allowed
                    in a Request-URI.  The UAC MUST add a Route header field containing
                    the remainder of the route set values in order, including all
                    parameters.  The UAC MUST then place the remote target URI into the
                    Route header field as the last value.

                    For example, if the remote target is sip:user@remoteua and the route
                    set contains:
                        <sip:proxy1>,<sip:proxy2>,<sip:proxy3;lr>,<sip:proxy4>

                    The request will be formed with the following Request-URI and Route
                    header field:
                        METHOD sip:proxy1
                        Route: <sip:proxy2>,<sip:proxy3;lr>,<sip:proxy4>,<sip:user@remoteua>
                */

                if(this.Routes[0].Parameters["lr"] != null){
                    request.Uri = this.RemoteTarget;
                    for(int i=0;i<this.Routes.Length;i++){
                        request.Route.Add(this.Routes[i].ToStringValue());
                    }
                }
                else{
                    request.Uri = SIP_Utils.UriToRequestUri(this.Routes[0].Address.Uri);
                    for(int i=1;i<this.Routes.Length;i++){
                        request.Route.Add(this.Routes[i].ToStringValue());
                    }
                }
            }

            return request;
        }

        #endregion

        #region method CreateAck

        /// <summary>
        /// Creates ACK for active INVITE transaction.
        /// </summary>
        /// <returns>Returns created ACK request.</returns>
        internal SIP_Request CreateAck(SIP_Request request)
        {
            /* RFC 3261 13.2.2.4.
                The header fields of the ACK are constructed
                in the same way as for any request sent within a dialog (see Section
                12) with the exception of the CSeq and the header fields related to
                authentication.  The sequence number of the CSeq header field MUST be
                the same as the INVITE being acknowledged, but the CSeq method MUST
                be ACK.  The ACK MUST contain the same credentials as the INVITE.
                
                ACK must have same branch.
            */

            if(request.Method != "INVITE"){
                throw new ArgumentException("CreateAck only can create ACK for INVITE mehtod !");
            }
                        
            SIP_Request ackRequest = CreateRequest("ACK",false);
            ackRequest.Via.AddToTop(request.Via.GetTopMostValue().ToStringValue());
            ackRequest.CSeq = new SIP_t_CSeq(request.CSeq.SequenceNumber,"ACK");
            // Authorization
            foreach(SIP_HeaderField h in request.Authorization.HeaderFields){
                ackRequest.Authorization.Add(h.Value);
            }
            // Proxy-Authorization 
            foreach(SIP_HeaderField h in request.ProxyAuthorization.HeaderFields){
                ackRequest.Authorization.Add(h.Value);
            }

            return ackRequest;        
        }

        #endregion


        #region method ProcessRequest

        /// <summary>
        /// Processes this request on this dialog. Dialog will update it's state as needed.
        /// Normally this method is always called from transacrion.
        /// </summary>
        /// <param name="request">SIP request.</param>
        internal void ProcessRequest(SIP_Request request)
        {
            m_pStack.Logger.AddDebug("Dialog(id='" + this.DialogID + "') got request: " + request.Method);

            if(m_DialogState == SIP_DialogState.Early || m_DialogState == SIP_DialogState.Terminated){
                return;
            }

            // BYE terminates dialog
            if(request.Method == "BYE"){
                // TODO: UAC sent us BYE, we need to send 200 OK and not generate BYE.
                m_pStack.TransportLayer.SendResponse(request.Socket,request.CreateResponse(SIP_ResponseCodes.x200_Ok));

                Dispose();
                return;
            }
            // INVITE
            else if(request.Method == "INVITE"){
                // Do target refresh
            }            
        }

        #endregion

        #region method ProcessResponse

        /// <summary>
        /// Processes this response on this dialog. Dialog will update it's state as needed.
        /// Normally this method is always called from transaction.
        /// </summary>
        /// <param name="response">SIP response.</param>
        internal void ProcessResponse(SIP_Response response)
        {   
            m_pStack.Logger.AddDebug("Dialog(id='" + this.DialogID + "') got response: " + response.StatusCode_ReasonPhrase);
        
            // Early
            if(m_DialogState == SIP_DialogState.Early){
                /* RFC 3261 12.3 Termination of a Dialog.
                    Independent of the method, if a request outside of a dialog generates
                    a non-2xx final response, any early dialogs created through
                    provisional responses to that request are terminated.
                */
                if(response.StatusCode >= 300){
                    Dispose();
                }
                /*
                    If the dialog identifier in the 2xx response matches the dialog
                    identifier of an existing dialog, the dialog MUST be transitioned to
                    the "confirmed" state, and the route set for the dialog MUST be recomputed 
                    based on the 2xx response using the procedures of Section 12.2.1.2.
                    WE DON'T DO IT, its for SIP 1.0 backward compatibility, what we don't support.
                */
                // 2xx
                else if(response.StausCodeType == SIP_StatusCodeType.Success){
                    m_DialogState = SIP_DialogState.Confirmed;

                    m_pStack.Logger.AddDebug("Dialog(id='" + this.DialogID + "') switched to confirmed state.");
                }
            }
            // Confirmed
            else if(m_DialogState == SIP_DialogState.Confirmed){
            }
            // Terminated
            else{
            }
        }

        #endregion

        #region method IsTargetRefreshRequest

        /// <summary>
        /// Gets if specified SIP request is target refresh request.
        /// Basically that mean if dialog remote URI must be updated.
        /// </summary>
        /// <param name="request">SIP request.</param>
        /// <returns>Returns true if specified request is target refresh request.</returns>
        private bool IsTargetRefreshRequest(SIP_Request request)
        {
            /* RFC 3261 12.2.
                Target refresh requests only update the dialog's remote target URI,
                and not the route set formed from the Record-Route.
            */

            // re-INVITE
            if(request.Method == "INVITE"){
                return true;
            }

            return false;
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets SIP METHOD that caused dialog creation.
        /// </summary>
        public string Method
        {
            get{ return m_Method; }
        }

        /// <summary>
        /// Gets dialog ID. Dialog ID is composed from CallID + '-' + Local Tag + '-' + Remote Tag.
        /// </summary>
        public string DialogID
        {
            get{ return this.CallID + "-" + this.LocalTag + "-" + this.RemoteTag; }
        }

        /// <summary>
        /// Gets dialog state.
        /// </summary>
        public SIP_DialogState DialogState
        {
            get{ return m_DialogState; }
        }

        /// <summary>
        /// Gets call ID.
        /// </summary>
        public string CallID
        {
            get{ return m_CallID; }
        }

        /// <summary>
        /// Gets From: header field tag parameter value.
        /// </summary>
        public string LocalTag
        {
            get{ return m_LocalTag; }
        }

        /// <summary>
        /// Gets To: header field tag parameter value.
        /// </summary>
        public string RemoteTag
        {
            get{ return m_RemoteTag; }
        }

        /// <summary>
        /// Gets local command sequence(CSeq) number.
        /// </summary>
        public int LocalSequenceNo
        {
            get{ return m_LocalSequenceNo; }
        }

        /// <summary>
        /// Gets remote command sequence(CSeq) number.
        /// </summary>
        public int RemoteSequenceNo
        {
            get{ return m_RemoteSequenceNo; }
        }

        /// <summary>
        /// Gets local UAC From: header field URI.
        /// </summary>
        public string LocalUri
        {
            get{ return m_LocalUri; }
        }

        /// <summary>
        /// Gets remote UAC From: header field URI.
        /// </summary>
        public string RemoteUri
        {
            get{ return m_RemoteUri; }
        }

        /// <summary>
        /// Gets remote UAC Contact: header field URI.
        /// </summary>
        public string RemoteTarget
        {
            get{ return m_RemoteTarget; }
        }

        /// <summary>
        /// Gets the list of servers that need to be traversed to send a request to the peer.
        /// </summary>
        public SIP_t_AddressParam[] Routes
        {
            get{ return m_pRouteSet; }
        }

        /// <summary>
        /// Gets if request was done over TLS.
        /// </summary>
        public bool IsSecure
        {
            get{ return m_IsSecure; }
        }

        /// <summary>
        /// Gets if thid dialog is UAS or UAC dialog.
        /// </summary>
        public bool IsServer
        {
            get{ return m_IsServer; }
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

        /// <summary>
        /// Is raised if dialog is terminated.
        /// </summary>
        public event EventHandler Terminated = null;

        /// <summary>
        /// Raises Terminated event.
        /// </summary>
        protected void OnTerminated()
        {
            if(this.Terminated != null){
                this.Terminated(null,null);
            }
        }

        #endregion

    }
}
