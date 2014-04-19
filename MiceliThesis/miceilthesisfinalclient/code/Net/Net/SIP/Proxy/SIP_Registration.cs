using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy
{
    /// <summary>
    /// Holds SIP registration info.
    /// </summary>
    public class SIP_Registration
    {        
        private DateTime                      m_RegisterTime;
        private string                        m_Address   = "";
        private string                        m_UserName  = "";
        private List<SIP_RegistrationContact> m_pContacts = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="userName">User name what owns registration.</param>
        /// <param name="address">Address what registration holds. (This is To: address of record value.)</param>
        public SIP_Registration(string userName,string address)
        {
            m_UserName = userName;
            m_Address  = address; 

            m_RegisterTime = DateTime.Now;
            m_pContacts = new List<SIP_RegistrationContact>();
        }


        #region method GetContact

        /// <summary>
        /// Gets registration contact with specified URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>Returns registration contact or null if no contact with specified URI.</returns>
        public SIP_RegistrationContact GetContact(string uri)
        {
            SIP_Uri sipUri = null;
            try{
                sipUri = SIP_Uri.Parse(uri);
            }
            catch{
            }

            // Find contact
            foreach(SIP_RegistrationContact contact in m_pContacts){
                // SIP URI
                if(sipUri != null){
                    if(contact.Contact.Address.IsSipOrSipsUri && SIP_Uri.Parse(contact.Contact.Address.Uri).Equals(sipUri)){
                        return contact;
                    }
                }
                // non sip URI, just compre it as string.
                else{
                    if(uri == contact.Contact.Address.Uri){
                        return contact;
                    }
                }
            }

            return null;
        }

        #endregion

        #region method UpdateContacts

        /// <summary>
        /// Updates registration contacts.
        /// </summary>
        /// <param name="contacts">SIP contacts to add/update.</param>
        /// <param name="expires">This is register request header field Expires: value and used only if contact doesn't have expires parameter.</param>
        /// <param name="minExpires">SIP server minimum expire time in seconds.</param>
        /// <param name="localEP">SIP proxy local end point info what accpeted this contact registration.</param>
        public void UpdateContacts(SIP_t_ContactParam[] contacts,int expires,int minExpires,SIP_EndPointInfo localEP)
        {
            lock(m_pContacts){
                foreach(SIP_t_ContactParam contact in contacts){
                    // Handle special value STAR Contact:, this means that we need to remove all contacts.
                    if(contact.IsStarContact){
                        m_pContacts.Clear();
                        return;
                    }
                                        
                    SIP_RegistrationContact currentContact = GetContact(contact.Address.Uri);
                    
                    // Get contact expire time. 
                    int contactExpires = contact.Expires;
                    if(contactExpires == -1){
                        contactExpires = expires;
                    }
                    if(contactExpires == -1){
                        contactExpires = minExpires;
                    }

                    // Remove specified contact
                    if(contactExpires == 0){
                        if(currentContact != null){
                            m_pContacts.Remove(currentContact);
                        }
                    }
                    // Add
                    else if(currentContact == null){
                        m_pContacts.Add(new SIP_RegistrationContact(contact,contactExpires,localEP));
                    }
                    // Update
                    else{
                        currentContact.UpdateContact(contact,contactExpires,localEP);
                    }
                }
            }
        }

        #endregion

        #region method RemoveExpiredContacts

        /// <summary>
        /// Removes all contacts what has been expired.
        /// </summary>
        public void RemoveExpiredContacts()
        {
            lock(m_pContacts){
                for(int i=0;i<m_pContacts.Count;i++){
                    if(m_pContacts[i].IsExpired){
                        m_pContacts.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        #endregion

        #region method RemoveAllContacts

        /// <summary>
        /// Removes all regiostration contacts.
        /// </summary>
        public void RemoveAllContacts()
        {
            lock(m_pContacts){
                m_pContacts.Clear();
            }
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets SIP address what is used for registration mapping. (This is To: address of record value.)
        /// For example: ivar@lumisoft.ee.
        /// </summary>
        public string Address
        {
            get{ return m_Address; }
        }

        /// <summary>
        /// Gets authenticated user name who made register. This available only if server required auth for registration 
        /// and user authenticated.
        /// </summary>
        public string UserName
        {
            get{ return m_UserName; }
        }

        /// <summary>
        /// Gets registration name registered contacts. NOTE: Contacts are in priority order.
        /// </summary>
        public SIP_RegistrationContact[] Contacts
        {
            get{ 
                lock(m_pContacts){
                    SIP_RegistrationContact[] retVal = m_pContacts.ToArray();
                                                            
                    // Sort by qvalue, higer qvalue means higher priority.
                    Array.Sort(retVal);
                                        
                    return retVal; 
                }
            }
        }

        /// <summary>
        /// Gets sip and sip contacts.
        /// </summary>
        public SIP_Uri[] SipContacts
        {
            get{                
                // Filter out SIP contacts, sort them by qvalue.
                List<SIP_Uri> retVal = new List<SIP_Uri>();
                foreach(SIP_RegistrationContact contact in this.Contacts){
                    if(contact.Contact.Address.IsSipUri || contact.Contact.Address.IsSecureSipUri){
                        retVal.Add(SIP_Uri.Parse(contact.Contact.Address.Uri));
                    }
                }
 
                return retVal.ToArray(); 
            }
        }

        #endregion

    }
}
