using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace LumiSoft.MailServer.API.UserAPI
{
    /// <summary>
    /// The Relay_Settings object represents Relay settings in LumiSoft Mail Server virtual server.
    /// </summary>
    public class Relay_Settings
    {
        private System_Settings m_pSysSettings         = null;
        private bool            m_UseSmartHost         = false;
        private string          m_SmartHost            = "";
        private int             m_SmartHostPort        = 25;
        private bool            m_SmartHostSSL         = false;
        private string          m_SmartHostUserName    = "";
        private string          m_SmartHostPassword    = "";
        private string          m_Dns1                 = "";
        private string          m_Dns2                 = "";
        private string          m_HostName             = "";
        private int             m_IdleTimeout          = 0;
        private int             m_MaxConnections       = 0;
        private int             m_MaxConnectionsPerIP  = 0;
        private int             m_RelayInterval        = 0;
        private int             m_RelayRetryInterval   = 0;
        private int             m_SendUndelWaringAfter = 0;
        private int             m_SendUndeliveredAfter = 0;
        private bool            m_StoreUndeliveredMsgs = false;
        private IPAddress       m_pBindIP              = IPAddress.Any;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sysSettings">Reference to system settings.</param>
        /// <param name="useSmartHost">Specifies if smart host is used for relaying.</param>
        /// <param name="smartHost">Smart host name or ip.</param>
        /// <param name="smartHostPort">Smart host port.</param>
        /// <param name="smrtHostSSL">Specifies if smart host is connected via SSL.</param>
        /// <param name="smartHostUserName">Smart host user name.</param>
        /// <param name="dns1">DNS server.</param>
        /// <param name="dns2">DNS server.</param>
        /// <param name="smartHostPassword">Smart host password.</param>
        /// <param name="hostName">Host name reported to connected server.</param>
        /// <param name="idleTimeout">Session idle timeout seconds.</param>
        /// <param name="maxConnections">Maximum conncurent conncetions.</param>
        /// <param name="maxConnectionsPerIP">Maximum conncurent conncetions to one IP address.</param>
        /// <param name="relayInterval">Relay messages interval seconds.</param>
        /// <param name="relayRetryInterval">Relay retry messages interval seconds.</param>
        /// <param name="undeliveredWarning">Specifies after how many minutes delayed delivery message is sent.</param>
        /// <param name="undelivered">Specifies after how many hours undelivered notification message is sent.</param>
        /// <param name="storeUndelivered">Specifies if undelivered messages are stored to "Undelivered" folder in mail store.</param>
        /// <param name="bindIP">IP addresss what is used to send out relay messages.</param>
        internal Relay_Settings(System_Settings sysSettings,bool useSmartHost,string smartHost,int smartHostPort,bool smrtHostSSL,string smartHostUserName,string smartHostPassword,string dns1,string dns2,string hostName,int idleTimeout,int maxConnections,int maxConnectionsPerIP,int relayInterval,int relayRetryInterval,int undeliveredWarning,int undelivered,bool storeUndelivered,string bindIP)
        {
            m_pSysSettings         = sysSettings;
            m_UseSmartHost         = useSmartHost;
            m_SmartHost            = smartHost;
            m_SmartHostPort        = smartHostPort;
            m_SmartHostSSL         = smrtHostSSL;
            m_SmartHostUserName    = smartHostUserName;
            m_SmartHostPassword    = smartHostPassword;
            m_Dns1                 = dns1;
            m_Dns2                 = dns2;
            m_HostName             = hostName;
            m_IdleTimeout          = idleTimeout;
            m_MaxConnections       = maxConnections;
            m_MaxConnectionsPerIP  = maxConnectionsPerIP;
            m_RelayInterval        = relayInterval;
            m_RelayRetryInterval   = relayRetryInterval;
            m_SendUndelWaringAfter = undeliveredWarning;
            m_SendUndeliveredAfter = undelivered;
            m_StoreUndeliveredMsgs = storeUndelivered;
            if(bindIP == ""){
                m_pBindIP = IPAddress.Any;
            }
            else{
                m_pBindIP = IPAddress.Parse(bindIP);
            }
        }


        #region Properties Implementation
                
        /// <summary>
        /// Gets or sets if relay send emails thorugh smart host or directly using dns.
        /// </summary>
        public bool UseSmartHost
        {
            get{ return m_UseSmartHost; }

            set{
                if(m_UseSmartHost != value){
                    m_UseSmartHost = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets smart host to use for sending relay messages.
        /// </summary>
        public string SmartHost
        {
            get{ return m_SmartHost; }

            set{
                if(m_SmartHost != value){
                    m_SmartHost = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets smart host server port.
        /// </summary>
        public int SmartHostPort
        {
            get{ return m_SmartHostPort; }

            set{
                if(m_SmartHostPort != value){
                    m_SmartHostPort = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets if smart host connected via SSL.
        /// </summary>
        public bool SmartHostSsl
        {
            get{ return m_SmartHostSSL; }

            set{
                if(m_SmartHostSSL != value){
                    m_SmartHostSSL = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets smart host server user name.
        /// </summary>
        public string SmartHostUserName
        {
            get{ return m_SmartHostUserName; }

            set{
                if(m_SmartHostUserName != value){
                    m_SmartHostUserName = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets smart host server password.
        /// </summary>
        public string SmartHostPassword
        {
            get{ return m_SmartHostPassword; }

            set{
                if(m_SmartHostPassword != value){
                    m_SmartHostPassword = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets DNS server.
        /// </summary>
        public string Dns1
        {
            get{ return m_Dns1; }

            set{
                if(m_Dns1 != value){
                    m_Dns1 = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets DNS server.
        /// </summary>
        public string Dns2
        {
            get{ return m_Dns2; }

            set{
                if(m_Dns2 != value){
                    m_Dns2 = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets EHLO/HELO host name reported to connected server. If "", then machine NETBIOS name used.
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
        /// Gets or sets how many seconds session can idle before times out.
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
        /// Gets or sets maximum concurent connections allowed to send out messages.
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
        /// Gets or sets maximum concurent connections allowed to one destination IP.
        /// </summary>
        public int MaximumConnectionsPerIP
        {
            get{ return m_MaxConnectionsPerIP; }

            set{
                if(m_MaxConnectionsPerIP != value){
                    m_MaxConnectionsPerIP = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets interval seconds.
        /// </summary>
        public int RelayInterval
        {
            get{ return m_RelayInterval; }

            set{
                if(m_RelayInterval != value){
                    m_RelayInterval = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets relay retry messages interval seconds.
        /// </summary>
        public int RelayRetryInterval
        {
            get{ return m_RelayRetryInterval; }

            set{
                if(m_RelayRetryInterval != value){
                    m_RelayRetryInterval = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets after how many minutes delayed delivery message is sent.
        /// </summary>
        public int SendUndeliveredWarningAfter
        {
            get{ return m_SendUndelWaringAfter; }

            set{
                if(m_SendUndelWaringAfter != value){
                    m_SendUndelWaringAfter = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets after how many hours undelivered notification is sent.
        /// </summary>
        public int SendUndeliveredAfter
        {
            get{ return m_SendUndeliveredAfter; }

            set{
                if(m_SendUndeliveredAfter != value){
                    m_SendUndeliveredAfter = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets if undelivered messages are stored to "Undelivered" folder in mail store.
        /// </summary>
        public bool StoreUndeliveredMessages
        {
            get{ return m_StoreUndeliveredMsgs; }

            set{
                if(m_StoreUndeliveredMsgs != value){
                    m_StoreUndeliveredMsgs = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets IP addresss what is used to send out relay messages.
        /// </summary>
        public IPAddress BindIP
        {
            get{ return m_pBindIP; }

            set{
                if(value == null){
                    throw new ArgumentNullException("BindIP");
                }

                if(!m_pBindIP.Equals(value)){
                    m_pBindIP = value;

                    m_pSysSettings.SetValuesChanged();
                }
            }
        }

        #endregion

    }
}
