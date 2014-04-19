using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

using LumiSoft.Net.AUTH;
using LumiSoft.Net.Log;

// REMOVE: 
using LumiSoft.Net.SIP.Proxy;

namespace LumiSoft.Net.SIP.Stack
{
    #region Delegates

    /// <summary>
    /// Represents the method that will handle the SIP_Stack.ValidateRequest event.
    /// </summary>
    public delegate void SIP_ValidateRequestEventHandler(SIP_ValidateRequestEventArgs e);

    /// <summary>
    /// Represents the method that will handle the SIP_Stack.Error event.
    /// </summary>
    /// <param name="x">Exception happened.</param>
    public delegate void SIP_ErrorEventHandler(Exception x);

    #endregion

    /// <summary>
    /// Implements SIP stack.
    /// </summary>
    public class SIP_Stack
    {        
        private SIP_Core                     m_pCore              = null;
        private SIP_TransportLayer           m_pTransportLayer    = null;
        private SIP_TransactionLayer         m_pTransactionLayer  = null;
        private Auth_HttpDigest_NonceManager m_pNonceManager      = null;
        private string                       m_HostName           = "";
        private int                          m_MinExpireTime      = 1800;
        private int                          m_MaximumConnections = 0;
        private int                          m_MaximumMessageSize = 0;
        private BindInfo[]                   m_pBinds             = null;
        private Logger                       m_pLogger            = null;
        private bool                         m_IsRunning          = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_Stack()
        {            
            m_pCore = new SIP_ProxyCore(this);
            m_pTransportLayer = new SIP_TransportLayer(this);
            m_pTransactionLayer = new SIP_TransactionLayer(this);
            m_pNonceManager = new Auth_HttpDigest_NonceManager();
                       
            m_pBinds  = new BindInfo[]{};
            m_pLogger = new Logger();
        }

        #region method Dispose

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Stop();

            // TODO:
        }

        #endregion


        #region method Start

        /// <summary>
        /// Starts SIP stack.
        /// </summary>
        public void Start()
        {
            if(m_IsRunning){
                return;
            }
            m_IsRunning = true;

            m_pTransportLayer.Start();            
        }

        #endregion

        #region method Stop

        /// <summary>
        /// Stops SIP stack.
        /// </summary>
        public void Stop()
        {
            if(!m_IsRunning){
                return;
            }
            m_IsRunning = false;

            m_pTransportLayer.Stop();
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets or sets if SIP stack is running.
        /// </summary>
        public bool Enabled
        {
            set{
                if(value){
                    Start();
                }
                else{
                    Stop();
                }
            }
        }

        /// <summary>
        /// Gets if SIP stack has started.
        /// </summary>
        public bool IsRunning
        {
            get{ return m_IsRunning; }
        }
                
        /// <summary>
        /// Gets or ses SIP core that will handle requests and responses.
        /// </summary>
        public SIP_Core SipCore
        {
            get{ return m_pCore; }

            set{
                if(value == null){
                    throw new ArgumentNullException();
                }

                m_pCore = value; 
            }
        }

        /// <summary>
        /// Gets transport layer what is used to receive and send requests and responses.
        /// </summary>
        internal SIP_TransportLayer TransportLayer
        {
            get{ return m_pTransportLayer; }
        }

        /// <summary>
        /// Gets transaction layer.
        /// </summary>
        public SIP_TransactionLayer TransactionLayer
        {
            get{ return m_pTransactionLayer; }
        }

        /// <summary>
        /// Gets digest authentication nonce manager.
        /// </summary>
        public Auth_HttpDigest_NonceManager DigestNonceManager
        {
            get{ return m_pNonceManager; }
        }

        /// <summary>
        /// Gets or stes host name. Fully qualified domain name suggested. For example: sip.lumisoft.ee.
        /// </summary>
        public string HostName
        {
            get{ return m_HostName; }

            set{
                if(value == null){
                    throw new ArgumentNullException();
                }

                m_HostName = value;
            }
        }

        /// <summary>
        /// Gets or sets minimum expire time in seconds what server allows.
        /// </summary>
        public int MinimumExpireTime
        {
            get{ return m_MinExpireTime; }

            set{
                if(value < 10){
                    throw new ArgumentException("Property MinimumExpireTime value must be >= 10 !");
                }

                m_MinExpireTime = value;
            }
        }

        /// <summary>
        /// Gets or sets how many cunncurent connections allowed. Value 0 means not limited. This is used only for TCP based connections.
        /// </summary>
        public int MaximumConnections
        {
            get{ return m_MaximumConnections; }

            set{
                if(value < 1){
                    m_MaximumConnections = 0;
                }
                else{
                    m_MaximumConnections = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets maximum allowed SIP message size in bytes. This is used only for TCP based connections.
        /// Value null means unlimited.
        /// </summary>
        public int MaximumMessageSize
        {
            get{ return m_MaximumMessageSize; }

            set{
                if(value < 1){
                    m_MaximumMessageSize = 0;
                }
                else{
                    m_MaximumMessageSize = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets socket bind info. Use this property to specify on which protocol,IP,port server 
        /// listnes and also if connections is SSL.
        /// </summary>
        public BindInfo[] BindInfo
        {
            get{ return m_pBinds; }

            set{
                if(value == null){
                    throw new ArgumentNullException("BindInfo");
                }

                //--- See binds has changed --------------
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
                    }
                }

                if(changed){
                    // If server is currently running, stop it before applying bind info.
                    bool running = m_pTransportLayer.IsRunning;
                    if(running){
                        m_pTransportLayer.Stop();
                        System.Threading.Thread.Sleep(1000);
                    }
     
                    m_pBinds = value; 

                    // We need to restart server to take effect IP or Port change
				    if(running){				    
					    m_pTransportLayer.Start();
				    }
                }
            }
        }

        /// <summary>
        /// Gets SIP logger.
        /// </summary>
        public Logger Logger
        {
            get{ return m_pLogger; }
        }

        #endregion

        #region Events Implementation
                
        /// <summary>
        /// This event is raised when new incoming SIP request is received. You can control incoming requests
        /// with that event. For example you can require authentication or send what ever error to request maker user.
        /// </summary>
        public event SIP_ValidateRequestEventHandler ValidateRequest = null;
        
        #region mehtod OnValidateRequest

        /// <summary>
        /// Is called by Transport layer when new incoming SIP request is received.
        /// </summary>
        /// <param name="request">Incoming SIP request.</param>
        /// <param name="remoteEndPoint">Request maker IP end point.</param>
        /// <returns></returns>
        internal SIP_ValidateRequestEventArgs OnValidateRequest(SIP_Request request,IPEndPoint remoteEndPoint)
        {
            SIP_ValidateRequestEventArgs eArgs = new SIP_ValidateRequestEventArgs(request,remoteEndPoint);
            if(this.ValidateRequest != null){
                this.ValidateRequest(eArgs);
            }

            return eArgs;
        }

        #endregion


        /// <summary>
        /// This event is raised by any SIP element when unknown error happened.
        /// </summary>
        public event SIP_ErrorEventHandler Error = null;

        #region method OnError

        /// <summary>
        /// Is called when ever unknown error happens.
        /// </summary>
        /// <param name="x">Exception happened.</param>
        internal void OnError(Exception x)
        {
            if(this.Error != null){
                this.Error(x);
            }
        }

        #endregion

        #endregion

    }
}
