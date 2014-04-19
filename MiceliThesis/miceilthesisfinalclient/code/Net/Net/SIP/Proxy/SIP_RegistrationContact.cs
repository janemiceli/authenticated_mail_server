using System;
using System.Collections.Generic;
using System.Text;

using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;

namespace LumiSoft.Net.SIP.Proxy
{
    /// <summary>
    /// Implements SIP registration contact.
    /// </summary>
    public class SIP_RegistrationContact : IComparable
    {
        private SIP_t_ContactParam m_pContact        = null;
        private int                m_Expire          = 0;
        private DateTime           m_LastUpdate;
        private SIP_EndPointInfo   m_pLocalEndPoint  = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="contact">SIP contact.</param>
        /// <param name="expires">Time in seconds when contact expires.</param>
        /// <param name="localEP">SIP proxy local end point info what accpeted this contact registration.</param>
        public SIP_RegistrationContact(SIP_t_ContactParam contact,int expires,SIP_EndPointInfo localEP)
        {
             UpdateContact(contact,expires,localEP);
        }


        #region method UpdateContact

        /// <summary>
        /// Updates contact info.
        /// </summary>
        /// <param name="contact">SIP contact.</param>
        /// <param name="expires">Time in seconds when contact expires.</param>
        /// <param name="localEP">SIP proxy local end point info what accpeted this contact registration.</param>
        public void UpdateContact(SIP_t_ContactParam contact,int expires,SIP_EndPointInfo localEP)
        {
            m_pContact       = contact;
            m_Expire         = expires;
            m_pLocalEndPoint = localEP;
            m_LastUpdate     = DateTime.Now;
        }

        #endregion

        #region method ToStringValue

        /// <summary>
        /// Returns this.Contact as string value, but expires parameter is replaced with remaining time value.
        /// </summary>
        /// <returns></returns>
        public string ToStringValue()
        {
            SIP_t_ContactParam retVal = new SIP_t_ContactParam();
            retVal.Parse(new StringReader(m_pContact.ToStringValue()));
            retVal.Expires = this.Expires;

            return retVal.ToStringValue();
        }

        #endregion


        #region IComparable interface Implementation

        /// <summary>
        /// Compares the current instance with another object of the same type. 
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>Returns 0 if two objects equal, -1 if this instance is less or 1 this instance is greater.</returns>
        public int CompareTo(object obj)
        {
            if(obj == null){
                return -1;
            }
            if(!(obj is SIP_RegistrationContact)){
                return -1;
            }

            // We must reverse values, because greater value mean higer priority.

            SIP_RegistrationContact compareValue = (SIP_RegistrationContact)obj;
            if(compareValue.QValue == this.QValue){
                return 0;
            }
            else if(compareValue.QValue > this.QValue){
                return 1;
            }            
            else if(compareValue.QValue < this.QValue){
                return -1;
            }

            return -1;
        }

        #endregion

        #region Properties Implementation

        /// <summary>
        /// Gets SIP contact.
        /// </summary>
        public SIP_t_ContactParam Contact
        {
            get{ return m_pContact; }
        }

        /// <summary>
        /// Gets if this contact is expired.
        /// </summary>
        public bool IsExpired
        {
            get{ return this.Expires == 0; }
        }

        /// <summary>
        /// Gets after how many seconds this contact expires. This is live calulated value, so it decreses every second.
        /// </summary>
        public int Expires
        {
            get{ 
                if(DateTime.Now > m_LastUpdate.AddSeconds(m_Expire)){
                    return 0;
                }
                else{
                    return (int)((TimeSpan)(m_LastUpdate.AddSeconds(m_Expire) - DateTime.Now)).TotalSeconds;
                }
            }
        }

        /// <summary>
        /// Gets qvalue. QValue is used for contacts sorting. Higer value means higher priority.
        /// </summary>
        public double QValue
        {
            get{
                // No qvalue value = 1.0.  Not checked from RFC, but found some where in net. 
                // If some one know exact RFC topic, let me know.
                if(m_pContact.QValue == -1){
                    return 1.0;
                }
                return m_pContact.QValue; 
            }
        }

        /// <summary>
        /// Gets local IP end point what accepted this contact registration.
        /// This end point is(must be) used to connect "this.Contact" address, otherwise if SIP stack
        /// has server IPs, UA is behind NAT, then UA only accpets data from this.LocalEndPoint.
        /// NOTE: This property can be null, if registration is added programatically and not trough network !
        /// </summary>
        public SIP_EndPointInfo LocalEndPoint
        {
            get{ return m_pLocalEndPoint; }
        }

        #endregion

    }
}
