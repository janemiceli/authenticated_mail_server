using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using LumiSoft.Net;
using LumiSoft.Net.Mime;

namespace LumiSoft.MailServer.Relay
{
    /// <summary>
    /// This class replaces all variables to actual values for 
    /// undelivered notice and delayed delivery warning messages.
    /// </summary>
    public class RelayVariablesManager
    {
        private Relay_Session m_pRealySession  = null;
        private string        m_ErrorText      = "";
        private Stream        m_pMessageStream = null;
        private Mime          m_pMime          = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RelayVariablesManager(Relay_Session relaySession,string errorText,Stream messageStream)
        {
            m_pRealySession  = relaySession;
            m_ErrorText      = errorText;
            m_pMessageStream = messageStream;

            try{
                m_pMime = Mime.Parse(messageStream);
            }
            catch{
            }
        }


        #region method Process

        /// <summary>
        /// Search variables and replace them with values.
        /// </summary>
        /// <param name="text">Text to process.</param>
        /// <returns>Returns text with variables replaced with real values.</returns>
        public string Process(string text)
        {
            StringBuilder retVal = new StringBuilder();

            // Search all <# > blocks
            int currentIndex = 0;
            int startIndex   = text.IndexOf("<#");
            int endIndex     = text.IndexOf(">"); 
            while(startIndex > -1 && endIndex > startIndex){
                // Get text between last block and new variable block.
                if(currentIndex < startIndex){
                    retVal.Append(text.Substring(currentIndex,startIndex - currentIndex));
                }

                // Get variable block.
                retVal.Append(ReplaceVariable(text.Substring(startIndex,endIndex - startIndex + 1)));

                currentIndex = endIndex + 1;
                startIndex   = text.IndexOf("<#",currentIndex);
                endIndex     = text.IndexOf(">",currentIndex);                 
            }

            // Append last text after lastt variable block.
            if(currentIndex < text.Length){
                retVal.Append(text.Substring(currentIndex));
            }

            return retVal.ToString();
        }

        #endregion


        #region method ReplaceVariable

        /// <summary>
        /// Replaces specified variable with actual value.
        /// </summary>
        /// <param name="variable">Variable to replace.</param>
        private string ReplaceVariable(string variable)
        {   
            try{
                // Current date time. Syntax: <#sys.datetime("format")>
                if(variable.StartsWith("<#sys.datetime")){
                    return DateTime.Now.ToString(TextUtils.UnQuoteString(variable.Substring(variable.IndexOf("(") + 1,variable.IndexOf(")") - variable.IndexOf("(") - 1)));
                }

                // Relay server host name.
                else if(variable == "<#relay.hostname>"){
                    if(m_pRealySession.OwnerServer.HostName != null && m_pRealySession.OwnerServer.HostName != ""){
                        return m_pRealySession.OwnerServer.HostName;
                    }
                    else{
                        return System.Net.Dns.GetHostName();
                    }
                }
                // Specifies after how many hours server will try to deliver message.
                else if(variable == "<#relay.undelivered_after>"){
                    return Convert.ToString(m_pRealySession.OwnerServer.UndeliveredAfter / 60);
                }
                // Error why relay failed.
                else if(variable == "<#relay.error>"){
                    return m_ErrorText;
                }
                // Relay session ID.
                else if(variable == "<#relay.session_id>"){
                    return m_pRealySession.SessionID;
                }
                // Relay session message ID.
                else if(variable == "<#relay.session_messageid>"){
                    return m_pRealySession.MessageID;
                }
                // Relay session connected host name.
                else if(variable == "<#relay.session_hostname>"){
                    return m_pRealySession.ConnectedHostName;
                }
                /*
                // Relay session active(last) log part.
                else if(variable == "<#relay.session_activelog>"){
                    if(m_pRealySession.SessionActiveLog != null){
                        return m_pRealySession.SessionActiveLog.ToString();
                    }
                    else{
                        return "<Relay Logging disabled>";
                    }
                }*/
                // Relay message destination recipient email address.
                else if(variable == "<#relay.to>"){
                    return m_pRealySession.To;
                }
                // Original sender email address.
                else if(variable == "<#relay.from>"){
                    return m_pRealySession.From;
                }

                // Message body text.
                else if(variable == "<#message.bodytext>"){
                    string bodyText = m_pMime.BodyText;
                    if(bodyText != null){
                        return bodyText;
                    }
                }
                // Message header field value. Synatx: <#message.header["headerFieldName:"]>
                else if(variable.StartsWith("<#message.header")){
                    HeaderField headerField = m_pMime.MainEntity.Header.GetFirst(TextUtils.UnQuoteString(variable.Substring(variable.IndexOf("[") + 1,variable.IndexOf("]") - variable.IndexOf("[") - 1)));
                    if(headerField != null){
                        return headerField.Value;
                    }
                }
            }
            catch{
                // Failed to replace variable value, just invaild synatx or who knows ... .
                // Just leave variable as is.
                return variable;
            }
            
            return "";
        }

        #endregion

    }
}
