using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Timers;

using LumiSoft.Net.AUTH;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy
{
    #region Delegates

    /// <summary>
    /// Represents the method that will handle the SIP_ProxyCore.IsLocalUri event.
    /// </summary>
    /// <param name="uri">Request URI.</param>
    /// <returns>Returns true if server local URI, otherwise false.</returns>
    public delegate bool SIP_IsLocalUriEventHandler(string uri);

    /// <summary>
    /// Represents the method that will handle the SIP_ProxyCore.Authenticate event.
    /// </summary>
    public delegate void SIP_AuthenticateEventHandler(SIP_AuthenticateEventArgs e);

    /// <summary>
    /// Represents the method that will handle the SIP_ProxyCore.AddressExists event.
    /// </summary>
    /// <param name="address">SIP address to check.</param>
    /// <returns>Returns true if specified address exists, otherwise false.</returns>
    public delegate bool SIP_AddressExistsEventHandler(string address);

    #endregion

    /// <summary>
    /// Implements SIP registrar,statefull and stateless proxy.
    /// </summary>
    public class SIP_ProxyCore : SIP_Core
    {
        private SIP_Stack       m_pSipStack   = null;
        private SIP_ProxyMode   m_ProxyMode   = SIP_ProxyMode.Registrar | SIP_ProxyMode.Statefull;
        private SIP_ForkingMode m_ForkingMode = SIP_ForkingMode.Parallel;
        private SIP_Registrar   m_pRegistrar  = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sipStack">Reference to SIP stack.</param>
        public SIP_ProxyCore(SIP_Stack sipStack)
        {
            m_pSipStack = sipStack;

            m_pRegistrar = new SIP_Registrar(this);
        }
                                        
        
        #region method OnRequestReceived

        /// <summary>
        /// This method is called when new request is received.
        /// </summary>
        /// <param name="e">Request event arguments.</param>
        public override void OnRequestReceived(SIP_RequestReceivedEventArgs e)
        {
            /* RFC 3261 16.12. ????????? Forward does all thse steps.
                1. The proxy will inspect the Request-URI.  If it indicates a
                   resource owned by this proxy, the proxy will replace it with
                   the results of running a location service.  Otherwise, the
                   proxy will not change the Request-URI.

                2. The proxy will inspect the URI in the topmost Route header
                   field value.  If it indicates this proxy, the proxy removes it
                   from the Route header field (this route node has been reached).

                3. The proxy will forward the request to the resource indicated
                   by the URI in the topmost Route header field value or in the
                   Request-URI if no Route header field is present.  The proxy
                   determines the address, port and transport to use when
                   forwarding the request by applying the procedures in [4] to that URI.
            */

            SIP_Request request = e.Request;
            try{                

                #region Registrar

                // Registrar
                if((m_ProxyMode & SIP_ProxyMode.Registrar) != 0 && request.Method == "REGISTER"){
                    m_pRegistrar.Register(e);
                }

                #endregion

                #region Presence
/*
                // Presence
                else if((m_ProxyMode & SIP_ProxyMode.Presence) != 0 && (request.Method == "SUBSCRIBE" || request.Method == "NOTIFY")){

                }
*/
                #endregion

                #region Statefull

                // Statefull
                else if((m_ProxyMode & SIP_ProxyMode.Statefull) != 0){
                    // Statefull proxy is transaction statefull proxy only, 
                    // what don't create dialogs and keep dialog state.

                    // ACK never creates transaction, it's always passed directly to transport layer.
                    if(e.Request.Method == SIP_Methods.ACK){
                        ForwardRequest(false,e);
                    }
                    else{
                        ForwardRequest(true,e);
                    }
                }

                #endregion
                
                // NOT IMPLEMENTED
                #region CallStatefull
                /*
                // CallStatefull
                else if((m_ProxyMode & SIP_ProxyMode.CallStatefull) != 0){
                    if(request.Method == "BYE"){
                        // Statefull proxy never should get BYE, that means we don't have matching dialog.                    
                        e.ServerTransaction.SendResponse(request.CreateResponse(SIP_ResponseCodes.Call_Transaction_Does_Not_Exist));                     
                    }
                    else if(request.Method == "CANCEL"){
                        // We never should get CANCEL here, because we handle CANCEL in server transaction.
                        // (This is not RFC compilant, but why the heck we can't do so ?, if somebody knows let me know)

                        e.ServerTransaction.SendResponse(request.CreateResponse(SIP_ResponseCodes.Call_Transaction_Does_Not_Exist));
                    }
                    else if(request.Method == "ACK"){
                        // We never should get ACK here, because we handle ACK in server transaction.
                        // (This is not RFC compilant, but why the heck we can't do so ?, if somebody knows let me know)
                    }
                    else{
                        ForwardRequest(true,e);
                    }
                }*/

                #endregion

                #region Stateless

                // Stateless
                else if((m_ProxyMode & SIP_ProxyMode.Stateless) != 0){
                    // Stateless proxy don't do transaction, just forwards all.
                    ForwardRequest(false,e);
                }

                #endregion

                #region Proxy won't accept command
                
                else{
                    e.ServerTransaction.SendResponse(request.CreateResponse(SIP_ResponseCodes.x501_Not_Implemented));
                }

                #endregion

            }
            catch(Exception x){   
                m_pSipStack.TransportLayer.SendResponse(request.Socket,e.Request.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error));

                // Don't raise OnError for transport errors.
                if(!(x is SIP_TransportException)){
                    m_pSipStack.OnError(x);
                }
            }            
        }

        #endregion

        #region method OnResponseReceived

        /// <summary>
        /// This method is called when new response is received.
        /// </summary>
        /// <param name="e">Response event arguments.</param>
        public override void OnResponseReceived(SIP_ResponseReceivedEventArgs e)
        {
            /* This method is called when stateless proxy gets response or statefull proxy
               has no matching server transaction.
            */
                  
            /* RFC 3261 16.11.
                When a response arrives at a stateless
                proxy, the proxy MUST inspect the sent-by value in the first
                (topmost) Via header field value.  If that address matches the proxy,
                (it equals a value this proxy has inserted into previous requests)
                the proxy MUST remove that header field value from the response and
                forward the result to the location indicated in the next Via header
                field value.
            */
            // Just remove topmost Via:, sent-by check is done in transport layer.
            e.Response.Via.RemoveTopMostValue();

            if((m_ProxyMode & SIP_ProxyMode.Statefull) != 0){
                // We should not reach here. This happens when no matching client transaction found.
                // RFC 3161 18.1.2 orders to forward them statelessly.
                m_pSipStack.TransportLayer.SendResponse(null,e.Response);
            }
            else if((m_ProxyMode & SIP_ProxyMode.Stateless) != 0){
                m_pSipStack.TransportLayer.SendResponse(null,e.Response);
            }
        }

        #endregion


        #region method ForwardRequest

        /// <summary>
        /// Forwards specified request to destination recipient.
        /// </summary>
        /// <param name="statefull">Specifies if request is sent statefully or statelessly.</param>
        /// <param name="e">Request event arguments.</param>
        private void ForwardRequest(bool statefull,SIP_RequestReceivedEventArgs e)
        {
            /* RFC 3261 16.6. Request Forwarding
                A stateful proxy must have a mechanism to maintain the target set as
                responses are received and associate the responses to each forwarded
                request with the original request.  For the purposes of this model,
                this mechanism is a "response context" created by the proxy layer
                before forwarding the first request.
                          
                1.  Make a copy of the received request
                2.  Update the Request-URI
                3.  Update the Max-Forwards header field (-1)
                4.  Optionally add a Record-route header field value
                5.  Optionally add additional header fields
                6.  Postprocess routing information
                7.  Determine the next-hop address, port, and transport
                8.  Add a Via header field value
                9.  Add a Content-Length header field if necessary
                10. Forward the new request
                11. Set timer C
            */

            List<SIP_Destination> destinations = new List<SIP_Destination>();
                        
            // 1. Make a copy of the received request
            SIP_Request forwardRequest = e.Request.Copy();

            // 2. Update the Request-URI
            //    Is that what we do in 7. route handling ? If someone knows let me know.

            // 3. Update the Max-Forwards header field.   
            //    MaxForwards not specified, default is 70.
            if(forwardRequest.MaxForwards != -1){
                forwardRequest.MaxForwards = 70;
            }
            // MaxForwards hop limit reached.
            else if(forwardRequest.MaxForwards < 1){
                e.ServerTransaction.ProcessResponse(forwardRequest.CreateResponse(SIP_ResponseCodes.x483_Too_Many_Hops));
                return;
            }
            forwardRequest.MaxForwards = forwardRequest.MaxForwards - 1;

            // 4. Optionally add a Record-route header field value.
            //    We need to do it always, "host name" specified or positive ACK never sent through server,
            //    then all NAT types won't work.
            if(!string.IsNullOrEmpty(m_pSipStack.HostName)){
                forwardRequest.RecordRoute.Add("<sip:" + m_pSipStack.HostName + ";lr>");
            }

            // 5. Optionally add additional header fields
            //    Skip 
 
            // 6. Postprocess routing information.            
            bool hasRoute = false;
            SIP_t_AddressParam route = forwardRequest.Route.GetTopMostValue();
            if(route != null){
                // If route header is ours (what we add by Record-Route), remove it.
                // TODO: Do we need to check if ouers or it's always ouers ?                
                forwardRequest.Route.RemoveTopMostValue();

                // Get next route if any.
                route = forwardRequest.Route.GetTopMostValue();

                // We accept SIP URIs in route only. If non SIP URI, skip route.
                if(route != null && (route.Address.IsSipUri || route.Address.IsSecureSipUri)){                    
                    // Loose route.  Loose route don't change route header, just send request to topmost value.
                    if(route.Parameters["lr"] != null){
                        destinations.Add(new SIP_Destination(SIP_Uri.Parse(route.Address.Uri)));
                    }
                    /* Strict route. 
                        1) Append current Request-URI to Route: header.
                        2) Put route value to Request-URI. NOTE: We need to remove not allowed Request-URI prameters !
                        3) Remove added route from Route: header.                 
                    */
                    else{
                        forwardRequest.Route.Add(forwardRequest.Uri);
                        forwardRequest.Uri = SIP_Utils.UriToRequestUri(route.Address.Uri);
                        forwardRequest.Route.RemoveTopMostValue();
                        destinations.Add(new SIP_Destination(SIP_Uri.Parse(route.Address.Uri)));
                    }
                    hasRoute = true;
                }
                // Skip route, do we need to generate error or at least remove that route ?
                //else{                    
                //}                
            }
                                                
            /* 7. Determine the next-hop address, port, and transport.
                1) If local address, forward request to registrar contact address(es).
                2) If there is Route header field, we always need send request to specified uri.
                3) Send request to To request-URI.
            */

            /* We currently allow only SIP URIs. Do we need others or how then to handle them ?
            */            
            SIP_Uri requestUri = null;
            try{
                requestUri = SIP_Uri.Parse(e.Request.Uri);
            }
            catch{
                // Not valid SIP URI
                e.ServerTransaction.ProcessResponse(e.Request.CreateResponse(SIP_ResponseCodes.x400_Bad_Request + ": Not supported Request-URI."));
                return;
            }
                                    
            /* UDP and NAT handling:
                If local URI, we need to use local IP end point what did registration to forward request. 
                This is needed by NAT, then all NAT types supported.
            */

            /* request-URI mapping:
                1) Has route                -> forward to route URI.
                2) Is registrar AOR         -> pass request with registrar contacts to proxy context (may fork).
                3) Is registrar AOR contact -> forward request to specified URI (use contact local end point).
                4) Remote URI               -> forward request to specified URI.
            */
            
            SIP_RegistrationContact registrationContact = null;
            if(hasRoute){
                // We don't need to do nothing here, all done in 6. Route processing already.
            }
            // Registrar AOR (local address)
            else if(this.OnIsLocalUri(requestUri.Host)){        
                SIP_Registration registration = m_pRegistrar.GetRegistration(requestUri.Address);
                // No registration or no contact(s) available now.
                if(registration == null || registration.SipContacts.Length == 0){
                    // User just not available now.
                    if(this.OnAddressExists(requestUri.Address)){
                        e.ServerTransaction.ProcessResponse(e.Request.CreateResponse(SIP_ResponseCodes.x480_Temporarily_Unavailable));
                    }
                    // User not found.
                    else{
                        e.ServerTransaction.ProcessResponse(e.Request.CreateResponse(SIP_ResponseCodes.x404_Not_Found));
                    }                    
                    return;
                }
                // Add all contacts to list.
                foreach(SIP_RegistrationContact contact in registration.Contacts){
                    if(contact.Contact.Address.IsSipUri){
                        destinations.Add(new SIP_Destination(contact.LocalEndPoint,SIP_Uri.Parse(contact.Contact.Address.Uri)));
                    }
                }
            }
            // Registrar AOR contact (local address)
            else if(m_pRegistrar.GetContact(requestUri,out registrationContact)){
                destinations.Add(new SIP_Destination(registrationContact.LocalEndPoint,requestUri));
            }
            // Remote URI
            else{
                // Authenticate request. We may not require for authentication ACK !
                if(forwardRequest.Method != SIP_Methods.ACK && !AuthenticateRequest(e)){
                    return;
                }

                destinations.Add(new SIP_Destination(requestUri));
            }
                                    
            // 8. Add a Via header field value.
            //    We need to add it only for stateless proxy, for statefull client transaction adds it.
            //    We do it in 10.

            // 9. Add a Content-Length header field if necessary.
            //    Skip, our SIP_Message class is smart and do it when ever it's needed.
            
            // 10. Forward request
            if(statefull){                                
                /*  RFC 3841 9.1.
                    The Request-Disposition header field specifies caller preferences for
                    how a server should process a request.
                */
                SIP_ForkingMode forkingMode = m_ForkingMode;
                bool noCancel  = false;
                bool noRecurse = false;
                foreach(SIP_t_Directive directive in forwardRequest.RequestDisposition.GetAllValues()){
                    if(directive.Directive == SIP_t_Directive.DirectiveType.NoFork){
                        forkingMode = SIP_ForkingMode.None;
                    }
                    else if(directive.Directive == SIP_t_Directive.DirectiveType.Parallel){
                        forkingMode = SIP_ForkingMode.Parallel;
                    }
                    else if(directive.Directive == SIP_t_Directive.DirectiveType.Sequential){
                        forkingMode = SIP_ForkingMode.Sequential;
                    }                    
                    else if(directive.Directive == SIP_t_Directive.DirectiveType.NoCancel){
                        noCancel = true;
                    }                    
                    else if(directive.Directive == SIP_t_Directive.DirectiveType.NoRecurse){
                        noRecurse = true;
                    }
                }
                
                // Create proxy context that will be responsible for forwarding request.
                SIP_ProxyContext proxyContext = new SIP_ProxyContext(
                    m_pSipStack,
                    forwardRequest,
                    forkingMode,
                    noCancel,
                    noRecurse,
                    destinations.ToArray()
                );
                proxyContext.Start();
            }
            else{
                /* RFC 3261 16.11
                    However, a stateless proxy cannot simply use a random number generator to compute
                    the first component of the branch ID, as described in Section 16.6 bullet 8.
                    This is because retransmissions of a request need to have the same value, and 
                    a stateless proxy cannot tell a retransmission from the original request.
                
                    We just use: "z9hG4bK-" + md5(topmost branch)                
                */   
                             
                // ACK never may add Via: !!!
                if(forwardRequest.Method != SIP_Methods.ACK){
                    forwardRequest.Via.AddToTop("SIP/2.0/transport-tl-addign sentBy-tl-assign-it;branch=z9hG4bK-" + Core.ComputeMd5(forwardRequest.Via.GetTopMostValue().Branch));
                }

                m_pSipStack.TransportLayer.SendRequest(forwardRequest,destinations[0]);
            }
        }

        #endregion

        #region method AuthenticateRequest

        /// <summary>
        /// Authenticates SIP request. This method also sends all needed replys to request sender.
        /// </summary>
        /// <param name="e">Request event arguments.</param>
        /// <returns>Returns true if request was authenticated.</returns>
        internal bool AuthenticateRequest(SIP_RequestReceivedEventArgs e)
        {
            string userName = null;
            return AuthenticateRequest(e,out userName);
        }

        /// <summary>
        /// Authenticates SIP request. This method also sends all needed replys to request sender.
        /// </summary>
        /// <param name="e">Request event arguments.</param>
        /// <param name="userName">If authentication sucessful, then authenticated user name is stored to this variable.</param>
        /// <returns>Returns true if request was authenticated.</returns>
        internal bool AuthenticateRequest(SIP_RequestReceivedEventArgs e,out string userName)
        {
            // TODO: If multiple auth headers, what then ???
            
            userName = null;
  
            // User didn't supplied credentials. 
            if(e.Request.Authorization.Count == 0){
                SIP_Response notAuthenticatedResponse = e.Request.CreateResponse(SIP_ResponseCodes.x401_Unauthorized);
                notAuthenticatedResponse.WWWAuthenticate.Add("digest realm=\"" + m_pSipStack.HostName + "\",qop=\"auth\",nonce=\"" + m_pSipStack.DigestNonceManager.CreateNonce() + "\",opaque=\"5ccc069c403ebaf9f0171e9517f40e41\"");
                    
                e.ServerTransaction.ProcessResponse(notAuthenticatedResponse);
                return false;
            }
 
            // Check that client supplied supported authentication method.
            // digest
            if(e.Request.Authorization.GetFirst().ValueX.Method.ToLower() == "digest"){
            }
            // Not supported authentication.
            else{
                e.ServerTransaction.ProcessResponse(e.Request.CreateResponse(SIP_ResponseCodes.x501_Not_Implemented + " authentication method"));
                return false;
            }
                                     
            Auth_HttpDigest auth = new Auth_HttpDigest(e.Request.Authorization.GetFirst().ValueX.AuthData,e.Request.Method);        
            // Check nonce validity
            if(!m_pSipStack.DigestNonceManager.NonceExists(auth.Nonce)){
                SIP_Response notAuthenticatedResponse = e.Request.CreateResponse(SIP_ResponseCodes.x401_Unauthorized);
                notAuthenticatedResponse.WWWAuthenticate.Add("digest realm=\"" + m_pSipStack.HostName + "\",qop=\"auth\",nonce=\"" + m_pSipStack.DigestNonceManager.CreateNonce() + "\",opaque=\"5ccc069c403ebaf9f0171e9517f40e41\"");

                // Send response
                e.ServerTransaction.ProcessResponse(notAuthenticatedResponse);
                return false;
            }
            // Valid nonce, consume it so that nonce can't be used any more. 
            else{
                m_pSipStack.DigestNonceManager.RemoveNonce(auth.Nonce);
            }

            SIP_AuthenticateEventArgs eArgs = this.OnAuthenticate(auth);
            // Authenticate failed.
            if(!eArgs.Authenticated){
                SIP_Response notAuthenticatedResponse = e.Request.CreateResponse(SIP_ResponseCodes.x401_Unauthorized);
                notAuthenticatedResponse.WWWAuthenticate.Add("digest realm=\"" + m_pSipStack.HostName + "\",qop=\"auth\",nonce=\"" + m_pSipStack.DigestNonceManager.CreateNonce() + "\",opaque=\"5ccc069c403ebaf9f0171e9517f40e41\"");
                    
                // Send response
                e.ServerTransaction.ProcessResponse(notAuthenticatedResponse);
                return false;
            }

            userName = auth.UserName;

            return true;
        }

        #endregion
                      

        #region Properties Implementation

        /// <summary>
        /// Gets owner SIP stack.
        /// </summary>
        public SIP_Stack Stack
        {
            get{ return m_pSipStack; }
        }

        /// <summary>
        /// Gets or sets proxy mode.
        /// </summary>
        public SIP_ProxyMode ProxyMode
        {
            get{ return m_ProxyMode; }

            set{
                // Check for invalid mode ()
                if((value & SIP_ProxyMode.Statefull) != 0 && (value & SIP_ProxyMode.Stateless) != 0){
                    throw new ArgumentException("Proxy can't be at Statefull and Stateless at same time !");
                }

                m_ProxyMode = value;
            }
        }

        /// <summary>
        /// Gets or sets how proxy handle forking. This property applies for statefull proxy only.
        /// </summary>
        public SIP_ForkingMode ForkingMode
        {
            get{ return m_ForkingMode; }

            set{ m_ForkingMode = value; }
        }

        /// <summary>
        /// Gets SIP registrar server.
        /// </summary>
        public SIP_Registrar Registrar
        {
            get{ return m_pRegistrar; }
        }
                
        #endregion

        #region Events Implementation
                                
        /// <summary>
        /// This event is raised when SIP proxy needs to know if specified request URI is local URI or remote URI.
        /// </summary>
        public event SIP_IsLocalUriEventHandler IsLocalUri = null;

        #region mehtod OnIsLocalUri

        /// <summary>
        /// Raises 'IsLocalUri' event.
        /// </summary>
        /// <param name="uri">Request URI.</param>
        /// <returns>Returns true if server local URI, otherwise false.</returns>
        internal bool OnIsLocalUri(string uri)
        {
            if(this.IsLocalUri != null){
                return this.IsLocalUri(uri);
            }

            return true;
        }

        #endregion
        
        /// <summary>
        /// This event is raised when SIP proxy or registrar server needs to authenticate user.
        /// </summary>
        public event SIP_AuthenticateEventHandler Authenticate = null;
                
        #region method OnAuthenticate

        /// <summary>
        /// Is called by SIP proxy or registrar server when it needs to authenticate user.
        /// </summary>
        /// <param name="auth">Authentication context.</param>
        /// <returns></returns>
        internal SIP_AuthenticateEventArgs OnAuthenticate(Auth_HttpDigest auth)
        {
            SIP_AuthenticateEventArgs eArgs = new SIP_AuthenticateEventArgs(auth);
            if(this.Authenticate != null){
                this.Authenticate(eArgs);
            }

            return eArgs;
        }

        #endregion
               
        /// <summary>
        /// This event is raised when SIP proxy needs to know if specified local server address exists.
        /// </summary>
        public event SIP_AddressExistsEventHandler AddressExists = null;
                
        #region method OnAddressExists

        /// <summary>
        /// Is called by SIP proxy if it needs to check if specified address exists.
        /// </summary>
        /// <param name="address">SIP address to check.</param>
        /// <returns>Returns true if specified address exists, otherwise false.</returns>
        internal bool OnAddressExists(string address)
        {            
            if(this.AddressExists != null){
                return this.AddressExists(address);
            }
                        
            return false;
        }

        #endregion

        #endregion

    }
}
