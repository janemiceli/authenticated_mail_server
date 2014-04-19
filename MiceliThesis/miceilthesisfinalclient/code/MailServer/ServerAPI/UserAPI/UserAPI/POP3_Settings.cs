using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.MailServer.API.UserAPI
{
    /// <summary>
    /// The POP3_Settings object represents POP3 settings in LumiSoft Mail Server virtual server.
    /// </summary>
    public class POP3_Settings
    {
        private System_Settings m_pSysSettings   = null;
        private bool            m_Enabled        = false;
        private string          m_HostName       = "";
        private string          m_GreetingText   = "";
        private int             m_IdleTimeout    = 0;
        private int             m_MaxConnections = 0;
        private int             m_MaxConnsPerIP  = 0;
        private int             m_MaxBadCommands = 0;        
        private BindInfo[]      m_pBinds         = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sysSettings">Reference to system settings.</param>
        /// <param name="enabled">Specifies if POP3 service is enabled.</param>
        /// <param name="hostName">Host name.</param>
        /// <param name="greeting">Greeting text.</param>
        /// <param name="idleTimeout">Session idle timeout seconds.</param>
        /// <param name="maxConnections">Maximum conncurent connections.</param>
        /// <param name="maxConnectionsPerIP">Maximum conncurent connections fro 1 IP address.</param>
        /// <param name="maxBadCommands">Maximum bad commands per session.</param>
        /// <param name="bindings">Specifies POP3 listening info.</param>
        internal POP3_Settings(System_Settings sysSettings,bool enabled,string hostName,string greeting,int idleTimeout,int maxConnections,int maxConnectionsPerIP,int maxBadCommands,BindInfo[] bindings)
        {
            m_pSysSettings   = sysSettings;
            m_Enabled        = enabled;
            m_HostName       = hostName;
            m_GreetingText   = greeting;
            m_IdleTimeout    = idleTimeout;
            m_MaxConnections = maxConnections;
            m_MaxConnsPerIP  = maxConnectionsPerIP;
            m_MaxBadCommands = maxBadCommands;
            m_pBinds         = bindings;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets or sets if POP3 server is enabled.
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
        /// Gets or sets POP3 host name reported to connected clients. If "", then machine NETBIOS name used.
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
        /// Gets or sets how many seconds session can idle before timed out.
        /// </summary>
        public int SessionIdleTimeOut
        {
            get{ return m_IdleTimeout; }

            set{
                if(m_IdleTimeout != value){
                    m_IdleTimeout = value;

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
            get{ return m_MaxBadCommands; }

            set{
                if(m_MaxBadCommands != value){
                    m_MaxBadCommands = value;

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
