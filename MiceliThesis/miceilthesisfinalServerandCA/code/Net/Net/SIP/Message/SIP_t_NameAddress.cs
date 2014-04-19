using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SIP.Message
{
    /// <summary>
    /// Implements SIP "name-addr" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     name-addr = [ display-name ] LAQUOT addr-spec RAQUOT
    ///     addr-spec = SIP-URI / SIPS-URI / absoluteURI
    /// </code>
    /// </remarks>
    public class SIP_t_NameAddress
    {
        private string m_DisplayName = "";
        private string m_Uri         = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_NameAddress()
        {
        }


        #region method Parse
        
        /// <summary>
        /// Parses "name-addr" or "addr-spec" from specified value.
        /// </summary>
        /// <param name="value">SIP "name-addr" or "addr-spec" value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(string value)
        {
            if(value == null){
                throw new ArgumentNullException("reader");
            }

            Parse(new StringReader(value));
        }
   
        /// <summary>
        /// Parses "name-addr" or "addr-spec" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(StringReader reader)
        {
            /* RFC 3261.
                name-addr =  [ display-name ] LAQUOT addr-spec RAQUOT
                addr-spec =  SIP-URI / SIPS-URI / absoluteURI
            */

            if(reader == null){
                throw new ArgumentNullException("reader");
            }

            reader.ReadToFirstChar();
                        
            // LAQUOT addr-spec RAQUOT
            if(reader.StartsWith("<")){
                m_Uri = reader.ReadParenthesized();
            }
            else{
                string word = reader.ReadWord();
                if(word == null){
                    throw new SIP_ParseException("Invalid 'name-addr' or 'addr-spec' value !");
                }

                reader.ReadToFirstChar();

                // name-addr
                if(reader.StartsWith("<")){
                    m_DisplayName = word;
                    m_Uri         = reader.ReadParenthesized();
                }
                // addr-spec
                else{
                    m_Uri = word;
                }
            }            
        }

        #endregion

        #region method ToStringValue

        /// <summary>
        /// Converts this to valid name-addr or addr-spec string as needed.
        /// </summary>
        /// <returns>Returns name-addr or addr-spec string.</returns>
        public string ToStringValue()
        {
            /* RFC 3261.
                name-addr =  [ display-name ] LAQUOT addr-spec RAQUOT
                addr-spec =  SIP-URI / SIPS-URI / absoluteURI
            */

            // addr-spec
            if(string.IsNullOrEmpty(m_DisplayName)){
                return "<" + m_Uri + ">";
            }
            // name-addr
            else{
                return TextUtils.QuoteString(m_DisplayName) + " <" + m_Uri + ">";
            }            
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets or sets display name.
        /// </summary>
        public string DisplayName
        {
            get{ return m_DisplayName; }

            set{
                if(value == null){
                    value = "";
                }

                m_DisplayName = value;
            }
        }

        /// <summary>
        /// Gets or sets URI. This can be SIP-URI / SIPS-URI / absoluteURI.
        /// Examples: sip:ivar@lumisoft.ee,sips:ivar@lumisoft.ee,mailto:ivar@lumisoft.ee, .... .
        /// </summary>
        public string Uri
        {
            get{ return m_Uri; }

            set{
                if(value == null){
                    value = "";
                }

                m_Uri = value;
            }
        }

        /// <summary>
        /// Gets if current URI is sip or sips URI.
        /// </summary>
        public bool IsSipOrSipsUri
        {
            get{ return IsSipUri || IsSecureSipUri; }
        }

        /// <summary>
        /// Gets if current URI is SIP uri.
        /// </summary>
        public bool IsSipUri
        {
            get{ 
                if(m_Uri.ToUpper().StartsWith("SIP") && !m_Uri.ToUpper().StartsWith("SIPS")){
                    return true; 
                }
                return false;
            }
        }

        /// <summary>
        /// Gets if current URI is SIPS uri.
        /// </summary>
        public bool IsSecureSipUri
        {
            get{ 
                if(m_Uri.ToUpper().StartsWith("SIPS")){
                    return true; 
                }
                return false;
            }
        }

        /// <summary>
        /// Gets if current URI is MAILTO uri.
        /// </summary>
        public bool IsMailToUri
        {
            get{ 
                if(m_Uri.ToUpper().StartsWith("MAILTO")){
                    return true; 
                }
                return false;
            }
        }

        #endregion

    }
}
