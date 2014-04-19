using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.MailServer.API.UserAPI
{
    /// <summary>
    /// The SIP_Settings object represents SIP settings in LumiSoft Mail Server virtual server.
    /// </summary>
    public class SIP_Settings
    {
        private System_Settings m_pSysSettings = null;
        private bool            m_Enabled      = false;
        private string          m_HostName     = "";
        private int             m_MinExpires   = 60;
        private BindInfo[]      m_pBinds       = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sysSettings">Reference to system settings.</param>
        /// <param name="enabled">Specifies if IMAP service is enabled.</param>
        /// <param name="hostName">SIP host name.</param>
        /// <param name="minExpires">SIP minimum content expire time in seconds.</param>
        /// <param name="bindings">Specifies SIP listening info.</param>
        internal SIP_Settings(System_Settings sysSettings,bool enabled,string hostName,int minExpires,BindInfo[] bindings)
        {
            m_pSysSettings = sysSettings;
            m_Enabled      = enabled;
            m_HostName     = hostName;
            m_MinExpires   = minExpires;
            m_pBinds       = bindings;
        }

        #region Properties Implementation

        /// <summary>
        /// Gets or sets if IMAP server is enabled.
        /// </summary>
        public bool Enabled
        {
            get{ return m_Enabled; }

            set{
                if(m_Enabled != value){
                    m_Enabled = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets SIP hostname. For example sip.lumisoft.ee.
        /// </summary>
        public string HostName
        {
            get{ return m_HostName; }

            set{
                if(value == null){
                    throw new ArgumentNullException();
                }

                if(m_HostName != value){
                    m_HostName = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or set SIP minimum allowed content expire time in seconds.
        /// </summary>
        public int MinimumExpires
        {
            get{ return m_MinExpires; }

            set{
                if(value < 60){
                    throw new ArgumentException("Argument MinimumExpires value must be >= 60 !");
                }

                if(m_MinExpires != value){
                    m_MinExpires = value;

                    m_pSysSettings.SetValuesChanged();
                }                
            }
        }

        /// <summary>
        /// Gets IP bindings.
        /// </summary>
        public BindInfo[] Binds
        {
            get{ return m_pBinds; }

            set{
                if(value == null){
                    throw new ArgumentNullException("Binds");
                }

                //--- See if bindinfo has changed ----------------------------------------------------------------//
                bool changed = false;
                if(m_pBinds.Length != value.Length){
                    changed = true;
                }
                else{
                    for(int i=0;i<m_pBinds.Length;i++){
                        if(!m_pBinds[i].Equals(value[i])){
                            changed = true;
                            break;
                        }
                        else if(!m_pBinds[i].Port.Equals(value[i].Port)){
                            changed = true;
                        }
                        else if(!m_pBinds[i].SSL.Equals(value[i].SSL)){
                            changed = true;
                        }
                        else{
                            if(m_pBinds[i].Certificate == null && value[i].Certificate != null){
                                 changed = true;
                            }
                            else if(value[i].Certificate == null && m_pBinds[i].Certificate != null){
                                changed = true;
                            }
                            else if(m_pBinds[i].Certificate == null && value[i].Certificate == null){
                            }
                            else{
                                if(!m_pBinds[i].Certificate.Equals(value[i].Certificate)){
                                    changed = true;
                                }
                            }
                        }
                    }
                }
                //-----------------------------------------------------------------------------------------------//

                if(changed){
                    m_pBinds = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        #endregion

    }
}
