using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy
{
    #region Delegates

    /// <summary>
    /// Represents the method that will handle the SIP_Registrar.CanRegister event.
    /// </summary>
    /// <param name="userName">Authenticated user name.</param>
    /// <param name="address">Address to be registered.</param>
    /// <returns>Returns true if specified user can register specified address, otherwise false.</returns>
    public delegate bool SIP_CanRegisterEventHandler(string userName,string address);

    #endregion

    /// <summary>
    /// This class implements SIP registrar server. Defined in RFC 3261.
    /// </summary>
    public class SIP_Registrar
    {
        private SIP_ProxyCore              m_pProxy         = null;
        private SIP_Stack                  m_pSipStack      = null;
        private SIP_RegistrationCollection m_pRegistrations = null;
        private Timer                      m_pTimer         = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="proxy">Owner proxy.</param>
        internal SIP_Registrar(SIP_ProxyCore proxy)
        {
            m_pProxy    = proxy;
            m_pSipStack = m_pProxy.Stack;

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


        #region method GetRegistration

        /// <summary>
        /// Gets specified registration. Returns null if no such registration.
        /// </summary>
        /// <param name="addressOfRecord">Address of record of registration which to get.</param>
        /// <returns>Returns SIP registration or null if no match.</returns>
        public SIP_Registration GetRegistration(string addressOfRecord)
        {
            return m_pRegistrations[addressOfRecord];
        }

        #endregion

        #region method SetRegistration

        /// <summary>
        /// Add or updates specified SIP registration info.
        /// </summary>
        /// <param name="addressOfRecord">Registration address of record.</param>
        /// <param name="contacts">Registration address of record contacts to update.</param>
        public void SetRegistration(string addressOfRecord,SIP_t_ContactParam[] contacts)
        {
            SetRegistration(addressOfRecord,contacts,null);
        }

        /// <summary>
        /// Add or updates specified SIP registration info.
        /// </summary>
        /// <param name="addressOfRecord">Registration address of record.</param>
        /// <param name="contacts">Registration address of record contacts to update.</param>
        /// <param name="localEP">SIP proxy local end point info what accpeted this contact registration.</param>
        public void SetRegistration(string addressOfRecord,SIP_t_ContactParam[] contacts,SIP_EndPointInfo localEP)
        {
            lock(m_pRegistrations){
                SIP_Registration registration = m_pRegistrations[addressOfRecord];
                if(registration == null){
                    registration = new SIP_Registration("system",addressOfRecord);
                    m_pRegistrations.Add(registration);
                }

                registration.UpdateContacts(contacts,180,m_pSipStack.MinimumExpireTime,localEP);
            }
        }

        #endregion

        #region method DeleteRegistration

        /// <summary>
        /// Deletes specified registration and all it's contacts.
        /// </summary>
        /// <param name="addressOfRecord">Registration address of record what to remove.</param>
        public void DeleteRegistration(string addressOfRecord)
        {
            m_pRegistrations.Remove(addressOfRecord);
        }

        #endregion

        #region method GetContact

        /// <summary>
        /// Gets registration contact.
        /// </summary>
        /// <param name="uri">SIP URI to get.</param>
        /// <param name="retVal">If specified SIP URI is registration contact, then that contect will be stored here.</param>
        /// <returns>Returns true is specified SIP URI is registration contact.</returns>
        internal bool GetContact(SIP_Uri uri,out SIP_RegistrationContact retVal)
        {
            retVal = null;

            lock(m_pRegistrations){
                foreach(SIP_Registration registration in m_pRegistrations){
                    foreach(SIP_RegistrationContact c in registration.Contacts){
                        if(c.Contact.Address.IsSipUri && SIP_Uri.Parse(c.Contact.Address.Uri).Equals(uri)){
                            retVal = c;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion


        #region method Register

        /// <summary>
        /// Handles REGISTER method.
        /// </summary>
        /// <param name="e">Request event arguments.</param>
        internal void Register(SIP_RequestReceivedEventArgs e)
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

            SIP_Request request = e.Request;

            // 3. Authenticate request
            string userName = null;
            if(!m_pProxy.AuthenticateRequest(e,out userName)){
                return;
            }
                        
            // 5. Get To:.
            // We have not SIP URI, To: may be SIP URI only.
            if(!(request.To.Address.IsSipUri || request.To.Address.IsSecureSipUri)){
                e.ServerTransaction.SendResponse(request.CreateResponse(SIP_ResponseCodes.x400_Bad_Request + ": To: value must be SIP URI."));
                return;
            }
            SIP_Uri to = SIP_Uri.Parse(request.To.Address.Uri);

            // 4. Check if user can register specified address.
            // User is not allowed to register specified address.
            if(!OnCanRegister(userName,to.Address)){
                e.ServerTransaction.SendResponse(request.CreateResponse(SIP_ResponseCodes.x403_Forbidden));
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

                    /* RFC 3261 10.2.2.
                        Expire value 0 in Expire header or contact expire parameter, means that specified
                        contact must be removed. 
                        If STAR contact then expires must be always 0.
                    */

                    //--- Handle minimum expires time --------------------------------------------
                    // Get contact expires time, if not specified, get header expires time, if not specified,
                    // use server minimum value.
                    int expires = contact.Expires;
                    if(expires == -1){
                        expires = request.Expires;
                    }
                    if(expires == -1){
                        expires = m_pSipStack.MinimumExpireTime;
                    }
                    // We don't check that for STAR contact and if contact expires parameter = 0.
                    if(!contact.IsStarContact && expires != 0 && expires < m_pSipStack.MinimumExpireTime){                        
                        SIP_Response sipExpiresResponse = request.CreateResponse(SIP_ResponseCodes.x423_Interval_Too_Brief);

                        // RFC 3261 20.23 must add Min-Expires for "Interval Too Brief".
                        sipExpiresResponse.MinExpires = m_pSipStack.MinimumExpireTime;

                        // The response SHOULD include a Date header field.
                        sipExpiresResponse.Date = DateTime.Now;
                                               
                        // Send response
                        e.ServerTransaction.SendResponse(sipExpiresResponse);
                        return;
                    }
                    //---------------------------------------------------------------------------
                }

                // We have STAR contact. Check that STAR contact meets all RFC rules.
                if(starContact != null){
                    // RFC 3261 10.3-6. We may have only 1 STAR Contact and Expires: header field must be 0.
                    if(contacts.Length > 1 || request.Expires != 0){                        
                        e.ServerTransaction.SendResponse(request.CreateResponse(SIP_ResponseCodes.x400_Bad_Request + ". Invalid STAR Contact: combination or parameter. For more info see RFC 3261 10.3.6."));
                        return;
                    }
                    // We have valid STAR Contact:.
                    //else{
                    //}
                }
            }
                    
            // Add or update SIP registration
            SIP_Registration registration = null;
            lock(m_pRegistrations){
                if(!m_pRegistrations.Contains(to.Address)){
                    // Add SIP registration.
                    registration = new SIP_Registration(userName,to.Address);
                    m_pRegistrations.Add(registration);
                }
                // Update SIP registration contacts
                else{
                    registration = m_pRegistrations[to.Address];
                }
            }
                        
            // Update registration contacts.
            registration.UpdateContacts(contacts,request.Expires,m_pSipStack.MinimumExpireTime,SIP_Utils.ToEndPointInfo(e.Request.Socket,true));
            //--------------------------------------------------------------------

            // 8. --- Make and send SIP respone ----------------------------------
            SIP_Response sipResponse = request.CreateResponse(SIP_ResponseCodes.x200_Ok);
            
            // The response SHOULD include a Date header field.
            sipResponse.Date = DateTime.Now;

            // List Registered Contacts. We also need to return expires parameter with remaining time.
            sipResponse.Header.RemoveAll("Contact:");
            foreach(SIP_RegistrationContact contact in registration.Contacts){
                // Don't list expired contacts what wait to be disposed.
                if(contact.Expires > 1){
                    sipResponse.Header.Add("Contact:",contact.ToStringValue());
                }
            }

            // Add Authentication-Info:, then client knows next nonce.
            sipResponse.AuthenticationInfo.Add("qop=\"auth\",nextnonce=\"" + m_pSipStack.DigestNonceManager.CreateNonce() + "\"");
                                               
            // Send response
            e.ServerTransaction.SendResponse(sipResponse);
            //-----------------------------------------------------------------------
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets owner proxy core.
        /// </summary>
        public SIP_ProxyCore Proxy
        {
            get{ return m_pProxy; }
        }

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

        #endregion

        #region Events Implementation

        /// <summary>
        /// This event is raised when SIP registrar need to check if specified user can register specified address.
        /// </summary>
        public event SIP_CanRegisterEventHandler CanRegister = null;

        #region method OnCanRegister

        /// <summary>
        /// Is called by SIP registrar if it needs to check if specified user can register specified address.
        /// </summary>
        /// <param name="userName">Authenticated user name.</param>
        /// <param name="address">Address to be registered.</param>
        /// <returns>Returns true if specified user can register specified address, otherwise false.</returns>
        internal bool OnCanRegister(string userName,string address)
        {
            if(this.CanRegister != null){
                return this.CanRegister(userName,address);
            }

            return false;
        }

        #endregion

        #endregion

    }
}
