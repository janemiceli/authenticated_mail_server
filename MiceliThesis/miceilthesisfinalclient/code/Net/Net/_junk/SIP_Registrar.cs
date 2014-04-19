using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using LumiSoft.Net.AUTH;
using LumiSoft.Net.SIP.Header;

namespace LumiSoft.Net.SIP
{
    /// <summary>
    /// Implements SIP registrar server. Defined in rfc 3261.
    /// </summary>
    public class SIP_Registrar
    {        
        private SIP_ServerProxyCore        m_pProxy         = null;
        private SIP_RegistrationCollection m_pRegistrations = null;
        private Timer                      m_pTimer         = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="proxy">Reference to owner proxy.</param>
        public SIP_Registrar(SIP_ServerProxyCore proxy)
        {
            m_pProxy = proxy;

            m_pRegistrations = new SIP_RegistrationCollection();

            m_pTimer = new Timer(15000);
            m_pTimer.Elapsed += new ElapsedEventHandler(m_pTimer_Elapsed);
            m_pTimer.Enabled = true;
        }


        #region method m_pTimer_Elapsed

        private void m_pTimer_Elapsed(object sender,ElapsedEventArgs e)
        {
            m_pRegistrations.RemoveExpired();
        }

        #endregion


        #region method GetReferedContact

        /// <summary>
        /// Gets specified registration prefered contact. Returns null if no such registration or no contacts available.
        /// </summary>
        /// <param name="registrationName">Registration name.</param>
        /// <returns></returns>
        public SIP_RegistrationContact GetReferedContact(string registrationName)
        {
            // Try to contact recipient.
            SIP_Registration r = this[registrationName];
            if(r != null){
                return r.GetPrefferedContact();
            }

            return null;
        }

        #endregion


        #region mehtod Register

        /// <summary>
        /// Handles REGISTER method.
        /// </summary>
        /// <param name="request">SIP REGISTER request.</param>
        public void Register(SIP_Request request)
        {
            /* RFC 3261 10.3 Processing REGISTER Requests.
                1. The registrar inspects the Request-URI to determine whether it
                    has access to bindings for the domain identified in the Request-URI.
                      
                2. To guarantee that the registrar supports any necessary extensions, 
                   the registrar MUST process the Require header field.
                     
                3. A registrar SHOULD authenticate the UAC.
                     
                4. The registrar SHOULD determine if the authenticated user is
                   authorized to modify registrations for this address-of-record.
                     
                5. The registrar extracts the address-of-record from the To header
                   field of the request.  If the address-of-record is not valid
                   for the domain in the Request-URI, the registrar MUST send a
                   404 (Not Found) response and skip the remaining steps.
                     
                6. The registrar checks whether the request contains the Contact
                   header field.  If not, it skips to the last step.  If the
                   Contact header field is present, the registrar checks if there
                   is one Contact field value that contains the special value "*"
                   and an Expires field.  If the request has additional Contact
                   fields or an expiration time other than zero, the request is
                   invalid, and the server MUST return a 400 (Invalid Request).
                     
                7. The registrar now processes each contact address in the Contact
                   eader field in turn.
                     
                   If Expire paremeter specified, check that it isnt smaller than server minimum
                   allowed expire value. If smaller return error 423 (Interval Too Brief) and add 
                   header field Min-Expires. If no expire parameter, user server default value.                          
                     
                8. The registrar returns a 200 (OK) response.  The response MUST contain Contact 
                   header field values enumerating all current bindings. Each Contact value MUST 
                   feature an "expires" parameter indicating its expiration interval chosen by the
                   registrar. The response SHOULD include a Date header field.

            */

            SocketEx socket = request.Socket;

            // 3. ------------------------------------------------------------------------- 
            // User didn't supplied credentials.
            if(request.Authorization.Count == 0){
                SIP_Response notAuthenticatedResponse = request.CreateResponse(SIP_ResponseCodes.Unauthorized);
                notAuthenticatedResponse.WWWAuthenticate.Add("digest realm=\"\",qop=\"auth\",nonce=\"" + m_pProxy.Stack.DigestNonceManager.CreateNonce() + "\",opaque=\"5ccc069c403ebaf9f0171e9517f40e41\"");
                
                // Send response
                m_pProxy.Stack.TransportLayer.SendResponse(socket,notAuthenticatedResponse);
                return;
            }
                        
            // Check that client supplied supported authentication method.
            string authenticationData = "";
            // digest
            if(request.Authorization.GetFirst().ValueX.Method.ToLower() == "digest"){
                authenticationData = request.Authorization.GetFirst().ValueX.AuthData;
            }
            // Not supported authentication.
            else{
                m_pProxy.Stack.TransportLayer.SendResponse(socket,request.CreateResponse(SIP_ResponseCodes.Not_Implemented + " authentication method"));
                return;
            }
       
            Auth_HttpDigest auth = new Auth_HttpDigest(authenticationData,request.Method);
            // Check nonce validity
            if(!m_pProxy.Stack.DigestNonceManager.NonceExists(auth.Nonce)){
                SIP_Response notAuthenticatedResponse = request.CreateResponse(SIP_ResponseCodes.Unauthorized);
                notAuthenticatedResponse.WWWAuthenticate.Add("digest realm=\"\",qop=\"auth\",nonce=\"" + m_pProxy.Stack.DigestNonceManager.CreateNonce() + "\",opaque=\"5ccc069c403ebaf9f0171e9517f40e41\"");

                // Send response
                m_pProxy.Stack.TransportLayer.SendResponse(socket,notAuthenticatedResponse);
                return;
            }
            // Valid nonce, consume it so that nonce can't be used any more. 
            else{
                m_pProxy.Stack.DigestNonceManager.RemoveNonce(auth.Nonce);
            }

            SIP_ServerProxyCore.AuthenticateEventArgs eArgs = m_pProxy.OnAuthenticate(auth);
            // Authenticate failed.
            if(!eArgs.Authenticated){
                SIP_Response notAuthenticatedResponse = request.CreateResponse(SIP_ResponseCodes.Unauthorized);
                notAuthenticatedResponse.WWWAuthenticate.Add("digest realm=\"\",qop=\"auth\",nonce=\"" + m_pProxy.Stack.DigestNonceManager.CreateNonce() + "\",opaque=\"5ccc069c403ebaf9f0171e9517f40e41\"");
                
                // Send response
                m_pProxy.Stack.TransportLayer.SendResponse(socket,notAuthenticatedResponse);
                return;
            }
            //----------------------------------------------------------------------------

            // 5. Get TO:.
            string registrationName = SipUtils.ParseAddress(request.To.ToStringValue()).ToLower();

            // 4. Check if user can register specified address.
            // User is not allowed to register specified address.
            if(!m_pProxy.OnCanRegister(auth.UserName,registrationName)){
                m_pProxy.Stack.TransportLayer.SendResponse(socket,request.CreateResponse(SIP_ResponseCodes.Forbidden));
                return;
            }
                                        
            //--- 6. And 7. --------------------------------------------------------
            SIP_t_ContactParam[] contacts = request.Contact.GetAllValues();
            if(request.Header.Contains("Contact:")){                
                SIP_t_ContactParam starContact = null;
                foreach(SIP_t_ContactParam contact in contacts){
                    // We have STAR contact, store it.
                    if(starContact == null && contact.IsStarContact){
                        starContact = contact;                               
                    }

                    //--- Handle minimum expires time --------------------------------------------
                    // Get contact expires time, if not specified, get header expires time.
                    int expires = contact.Expires;
                    if(expires < 1){
                        expires = request.Expires;
                    }
                    // We don't check that for STAR contact and if contact expires parameter = 0.
                    if(!contact.IsStarContact && contact.Expires != 0 && expires < m_pProxy.Stack.MinimumExpireTime){
                        // RFC 3261 20.23 must add Min-Expires
                        SIP_Response sipExpiresResponse = request.CreateResponse(SIP_ResponseCodes.Interval_Too_Brief);

                        // The response SHOULD include a Date header field.
                        sipExpiresResponse.Header.Add("Date:",DateTime.Now.ToString("r"));
                                               
                        // Send response
                        m_pProxy.Stack.TransportLayer.SendResponse(socket,sipExpiresResponse);
                        return;
                    }
                    //---------------------------------------------------------------------------
                }

                // We have STAR contact. Check that STAR contact meets all RFC rules.
                if(starContact != null){
                    // We may have only 1 STAR Contact and expires must be 0.
                    if(contacts.Length > 1 || starContact.Expires != 0){                        
                        m_pProxy.Stack.TransportLayer.SendResponse(socket,request.CreateResponse(SIP_ResponseCodes.Bad_Request + ". Invalid STAR Contact: combination or parameter. For more info see RFC 3261 10.3.6."));
                        return;
                    }
                    // We have valid STAR Contact:.
                    //else{
                    //}
                }
            }
                    
            // Add or get SIP registration
            SIP_Registration registration = null;
            lock(m_pRegistrations){
                if(!m_pRegistrations.Contains(registrationName)){
                    // Add SIP registration.
                    registration = new SIP_Registration(auth.UserName,registrationName);
                    m_pRegistrations.Add(registration);
                }
                // Update SIP registration contacts
                else{
                    registration = m_pRegistrations[registrationName];
                }
            }

            // Update registration contacts
            registration.UpdateContacts(contacts,request.Expires);
            //--------------------------------------------------------------------

            // 8. --- Make and send SIP respone ----------------------------------
            SIP_Response sipResponse = request.CreateResponse(SIP_ResponseCodes.Ok);
            
            // The response SHOULD include a Date header field.
            sipResponse.Date = DateTime.Now;

            // List Registered Contacts
            sipResponse.Header.RemoveAll("Contact:");
            foreach(SIP_RegistrationContact contact in registration.Contacts){
                sipResponse.Header.Add("Contact:",contact.Contact.ToStringValue());
            }

            // Add Authentication-Info:, then client knows next nonce.
            sipResponse.AuthenticationInfo.Add("qop=\"auth\",nextnonce=\"" + m_pProxy.Stack.DigestNonceManager.CreateNonce() + "\"");
                                               
            // Send response
            m_pProxy.Stack.TransportLayer.SendResponse(socket,sipResponse);
            //-----------------------------------------------------------------------
        }

        #endregion


        #region Properties Implementaion

        /// <summary>
        /// Gets current SIP registrations.
        /// </summary>
        public SIP_Registration[] Registrations
        {
            get{
                lock(m_pRegistrations){
                    SIP_Registration[] retVal = new SIP_Registration[m_pRegistrations.Count];
                    m_pRegistrations.Values.CopyTo(retVal,0);

                    return retVal;
                }
            }
        }

        /// <summary>
        /// Gets SIP registration with specified registration name. Returns null if specified registration doesn't exist.
        /// </summary>
        /// <param name="registrationName">SIP registration name.</param>
        /// <returns></returns>
        public SIP_Registration this[string registrationName]
        {
            get{ return m_pRegistrations[registrationName.ToLower()]; }
        }

        #endregion

    }
}
