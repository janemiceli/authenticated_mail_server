using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.MailServer.API.UserAPI
{
    /// <summary>
    /// The SMTP_Settings object represents SMTP settings in LumiSoft Mail Server virtual server.
    /// </summary>
    public class SMTP_Settings
    {
        private System_Settings m_pSysSettings       = null;
        private bool            m_Enabled            = false;
        private string          m_HostName           = "";
        private string          m_GreetingText       = "";
        private string          m_DefaultDomain      = "";
        private int             m_SessionIdleTimeOut = 0;
        private int             m_MaxConnections     = 0;
        private int             m_MaxConnsPerIP      = 0;
        private int             m_MaxBadCommnads     = 0;
        private int             m_MaxRecipientPerMsg = 0;
        private int             m_MaxMessageSize     = 0;
        private bool            m_RequireAuth        = false;
        private BindInfo[]      m_pBinds             = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sysSettings">Reference to system settings.</param>
        /// <param name="enabled">Specifies if SMTP service is enabled.</param>
        /// <param name="hostName">SMTP host name.</param>
        /// <param name="greeting">Greeting text.</param>
        /// <param name="defaultDomain">Default domain.</param>
        /// <param name="idleTimeOut">Session idle timeout seconds.</param>
        /// <param name="maxConnections">Maximum conncurent connections.</param>
        /// <param name="maxConnectionsPerIP">Maximum conncurent connections fro 1 IP address.</param>
        /// <param name="maxBadCommands">Maximum bad commands per session.</param>
        /// <param name="maxRecipients">Maximum recipients per message.</param>
        /// <param name="maxMessageSize">Maximum allowed message size.</param>
        /// <param name="requireAuth">Specifies if SMTP server is private server and requires authentication.</param>
        /// <param name="bindings">Specifies SMTP listening info.</param>
        internal SMTP_Settings(System_Settings sysSettings,bool enabled,string hostName,string greeting,string defaultDomain,int idleTimeOut,int maxConnections,int maxConnectionsPerIP,int maxBadCommands,int maxRecipients,int maxMessageSize,bool requireAuth,BindInfo[] bindings)
        {
            m_pSysSettings       = sysSettings;
            m_Enabled            = enabled;
            m_HostName           = hostName;
            m_GreetingText       = greeting;
            m_DefaultDomain      = defaultDomain;
            m_SessionIdleTimeOut = idleTimeOut;
            m_MaxConnections     = maxConnections;
            m_MaxConnsPerIP      = maxConnectionsPerIP;
            m_MaxBadCommnads     = maxBadCommands;
            m_MaxRecipientPerMsg = maxRecipients;
            m_MaxMessageSize     = maxMessageSize;
            m_RequireAuth        = requireAuth;
            m_pBinds             = bindings;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets or sets if SMTP service is enabled.
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
        /// Gets or sets SMTP host name reported to connected clients. If "", then machine NETBIOS name used.
        /// </summary>
        public string HostName
        {
            get{ return m_HostName; }

            set{
                if(m_HostName != value){
                    m_HostName = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets greeting text reported to connected clients. If "", default server greeting text is used.
        /// </summary>
        public string GreetingText
        {
            get{ return m_GreetingText; }

            set{
                if(m_GreetingText != value){
                    m_GreetingText = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets default email domain. For example if mail sent to "postmaster", then email becomes postmaster + '@' + default domain.
        /// </summary>
        public string DefaultDomain
        {
            get{ return m_DefaultDomain; }

            set{
                if(m_DefaultDomain != value){
                    m_DefaultDomain = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets how many seconds session can idle before timed out.
        /// </summary>
        public int SessionIdleTimeOut
        {
            get{ return m_SessionIdleTimeOut; }

            set{
                if(m_SessionIdleTimeOut != value){
                    m_SessionIdleTimeOut = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets maximum conncurent connections server accepts.
        /// </summary>
        public int MaximumConnections
        {
            get{ return m_MaxConnections; }

            set{
                if(m_MaxConnections != value){
                    m_MaxConnections = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets maximum conncurent connections from 1 IP address. Value 0, means unlimited connections.
        /// </summary>
        public int MaximumConnectionsPerIP
        {
            get{ return m_MaxConnsPerIP; }

            set{
                if(m_MaxConnsPerIP != value){
                    m_MaxConnsPerIP = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets maximum bad commands can happen before server terminates connection.
        /// </summary>
        public int MaximumBadCommands
        {
            get{ return m_MaxBadCommnads; }

            set{
                if(m_MaxBadCommnads != value){
                    m_MaxBadCommnads = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets how many recipients are allowed per message.
        /// </summary>
        public int MaximumRecipientsPerMessage
        {
            get{ return m_MaxRecipientPerMsg; }

            set{
                if(m_MaxRecipientPerMsg != value){
                    m_MaxRecipientPerMsg = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets maximum message size in MB.
        /// </summary>
        public int MaximumMessageSize
        {
            get{ return m_MaxMessageSize; }

            set{
                if(m_MaxMessageSize != value){
                    m_MaxMessageSize = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets if server requires authentication for all incoming connections. If this value is true,
        /// that means smtp server isn't accessible to public.
        /// </summary>
        public bool RequireAuthentication
        {
            get{ return m_RequireAuth; }

            set{
                if(m_RequireAuth != value){
                    m_RequireAuth = value;

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
                        if(!m_pBinds[i].IP.Equals(value[i].IP)){
                            changed = true;
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
