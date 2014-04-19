using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.MailServer.API.UserAPI
{
    /// <summary>
    /// The Auth_Settings object represents Authentication settings in LumiSoft Mail Server virtual server.
    /// </summary>
    public class Auth_Settings
    {
        private System_Settings               m_pSysSettings = null;
        private ServerAuthenticationType_enum m_AuthType     = ServerAuthenticationType_enum.Integrated;
        private string                        m_WinDomain    = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sysSettings">Reference to system settings.</param>
        /// <param name="authType">Specifies authentication type.</param>
        /// <param name="winDomain">Windows domain.</param>
        internal Auth_Settings(System_Settings sysSettings,ServerAuthenticationType_enum authType,string winDomain)
        {
            m_pSysSettings = sysSettings;
            m_AuthType     = authType;
            m_WinDomain    = winDomain;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets or sets authentication type.
        /// </summary>
        public ServerAuthenticationType_enum AuthenticationType
        {
            get{ return m_AuthType; }

            set{
                if(m_AuthType != value){
                    m_AuthType = value; 

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or set windows domain to against what to do authentication. 
        /// This property is used only if ServerAuthenticationType_enum.Windows. 
        /// Value "" means that local computer is used.
        /// </summary>
        public string WinDomain
        {
            get{ return m_WinDomain; }

            set{                 
                if(m_WinDomain != value){
                    m_WinDomain = value; 

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        #endregion

    }
}
