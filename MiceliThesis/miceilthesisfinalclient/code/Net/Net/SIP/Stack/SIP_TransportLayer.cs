using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack
{
    /// <summary>
    /// Implements SIP transport layer. Defined in RFC 3261.
    /// </summary>
    internal class SIP_TransportLayer
    {
        #region class SIP_Packet

        /// <summary>
        /// This calls holds UDP SIP packet info.
        /// </summary>
        private class SIP_Packet
        {
            private Socket     m_pSocket         = null;
            private IPEndPoint m_pRemoteEndPoint = null;
            private byte[]     m_Data            = null;

            /// <summary>
            /// Default constuctor.
            /// </summary>
            /// <param name="socket">Owner socket.</param>
            /// <param name="remoteEndPoint">Remote end point from where data received.</param>
            /// <param name="data">Data received.</param>
            public SIP_Packet(Socket socket,IPEndPoint remoteEndPoint,byte[] data)
            {
                m_pSocket         = socket;
                m_pRemoteEndPoint = remoteEndPoint;
                m_Data            = data;
            }


            #region Properties Implementation

            /// <summary>
            /// Gets owner socket.
            /// </summary>
            public Socket Socket
            {
                get{ return m_pSocket; }
            }

            /// <summary>
            /// Gets remote end point from where data received.
            /// </summary>
            public IPEndPoint RemoteEndPoint
            {
                get{ return m_pRemoteEndPoint; }
            }

            /// <summary>
            /// Gets data received.
            /// </summary>
            public byte[] Data
            {
                get{ return m_Data; }
            }

            #endregion
        }

        #endregion

        #region class SipListeningPoint

        /// <summary>
        /// This class represents stack listeining point.
        /// </summary>
        private class SipListeningPoint
        {
            private BindInfoProtocol m_Protocol     = BindInfoProtocol.UDP;
            private Socket           m_pSocket      = null;
            private bool             m_Ssl          = false;
            private X509Certificate  m_pCertificate = null;
            
            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="protocol">Listening protocol.</param>
            /// <param name="socket">Socket what represents this end point.</param>
            /// <param name="ssl">Specifies if connection is SSL connection.</param>
            /// <param name="certificate">SSL certificate.</param>
            public SipListeningPoint(BindInfoProtocol protocol,Socket socket,bool ssl,X509Certificate certificate)
            {
                m_Protocol     = protocol;
                m_pSocket      = socket;
                m_Ssl          = ssl;
                m_pCertificate = certificate;
            }


            #region Proerties Implementation

            /// <summary>
            /// Gets listening protocol.
            /// </summary>
            public BindInfoProtocol Protocol
            {
                get{ return m_Protocol; }
            }

            /// <summary>
            /// Gets listening socket.
            /// </summary>
            public Socket Socket
            {
                get{ return m_pSocket; }
            }

            /// <summary>
            /// Gets listening local IP end point.
            /// </summary>
            public IPEndPoint LocalEndPoint
            {
                get{ return (IPEndPoint)m_pSocket.LocalEndPoint; }
            }

            /// <summary>
            /// Gets if connection is SSL connection. TODO: get rid of it, instead of it add protocol SSL.
            /// </summary>
            public bool SSL
            {
                get{ return m_Ssl; }
            }

            /// <summary>
            /// Gets listening point certificate. This certificate is used for SSL or TSL negoation.
            /// </summary>
            public X509Certificate Certificate
            {
                get{ return m_pCertificate; }
            }
                        
            #endregion

        }

        #endregion

        #region class SipTcpPipe

        /// <summary>
        /// Implements SIP TCP pipe. This class just waits incoming data, reads it and processes it.
        /// This is done while socket is closed by remote host or timeout reached.
        /// </summary>
        private class SipTcpPipe
        {
            private SIP_TransportLayer m_pTLayer   = null;
            private SocketEx           m_pSocket   = null;
            private bool               m_Disposed  = false;
            private MemoryStream       m_pSipData  = null;
            private string             m_Transport = "";

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="owner">Owner SIP_TransportLayer.</param>
            /// <param name="socket">Connected socket.</param>
            public SipTcpPipe(SIP_TransportLayer owner,SocketEx socket)
            {
                m_pTLayer = owner;
                m_pSocket = socket;

                if(socket.SSL){
                    m_Transport = SIP_Transport.TLS;
                }
                else{
                    m_Transport = SIP_Transport.TCP;
                }

                Start();
            }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="owner">Owner SIP_TransportLayer.</param>
            /// <param name="remoteEP">Remote end point.</param>
            /// <param name="ssl">Specifies if SSL pipe.</param>
            public SipTcpPipe(SIP_TransportLayer owner,IPEndPoint remoteEP,bool ssl)
            {
                m_pTLayer = owner;

                m_pSocket = new SocketEx();
                m_pSocket.Connect(remoteEP,true);
                if(ssl){
                    m_Transport = SIP_Transport.TLS;
                    m_pSocket.SwitchToSSL_AsClient();
                }
                else{
                    m_Transport = SIP_Transport.TLS;
                }

                // Add this to TCP pipes collection.
                m_pTLayer.m_pTcpReceiviePipes.Add(this);

                Start();
            }

            #region mehtod Dispose

            /// <summary>
            /// Cleans up any resources being used.
            /// </summary>
            public void Dispose()
            {
                if(m_Disposed){
                    return;
                }
                m_Disposed = true;

                if(m_pSocket != null){
                    try{
                        m_pSocket.Dispose();
                    }
                    catch{
                    }
                    m_pSocket = null;
                }

                // Remove from active pipes.
                if(m_pTLayer != null){
                    m_pTLayer.m_pTcpReceiviePipes.Remove(this);
                }
            }

            #endregion


            #region mehtod Start

            /// <summary>
            /// Starts reading SIP request or response.
            /// </summary>
            private void Start()
            {
                // Clear old data.
                m_pSipData.SetLength(0);

                // Start reading SIP message header.
                BeginReadHeaderLine();
            }

            #endregion

            #region method SendMessage

            /// <summary>
            /// Sends specifed SIP message to pipe.
            /// </summary>
            /// <param name="message">Raw message what to send.</param>
            public void SendMessage(byte[] message)
            {
                lock(m_pSocket){
                    m_pSocket.Write(message);
                }
            }

            #endregion


            #region method BeginReadHeaderLine

            /// <summary>
            /// Begins reading header line.
            /// </summary>
            private void BeginReadHeaderLine()
            {
                MemoryStream ms = new MemoryStream();
                m_pSocket.BeginReadLine(ms,4000,ms,new SocketCallBack(this.BeginReadHeaderLine_Completed));
            }

            #endregion

            #region mehtod BeginReadHeaderLine_Completed

            /// <summary>
            /// This method is called if header line reading has completed.
            /// </summary>
            /// <param name="result"></param>
            /// <param name="count"></param>
            /// <param name="x"></param>
            /// <param name="tag"></param>
            private void BeginReadHeaderLine_Completed(SocketCallBackResult result,long count,Exception x,object tag)
            {
                MemoryStream ms = (MemoryStream)tag;

                try{
                    if(result == SocketCallBackResult.Ok){
                        // Write readed data to buffer stream.
                        string line  = Encoding.Default.GetString(ms.ToArray());
                        byte[] lineB = Encoding.Default.GetBytes(line + "\r\n");
                        m_pSipData.Write(lineB,0,lineB.Length);
                
                        // We got whole header, see if there is any content, if so begin receive it.
                        if(ms.Length == 0){
                            m_pSipData.Position = 0;
                            string c = Mime.MimeUtils.ParseHeaderField("Content-Lenght:",m_pSipData);
                            m_pSipData.Position = m_pSipData.Length;
                            if(c != ""){
                                int contentLength = Convert.ToInt32(c);

                                // Check maximum allowed message size.
                                if(m_pTLayer.m_pSipStack.MaximumMessageSize > 0 && (m_pSipData.Length + contentLength) > m_pTLayer.m_pSipStack.MaximumMessageSize){
                                    Dispose();
                                    return;
                                }

                                // Read content data.
                                m_pSocket.BeginReadSpecifiedLength(m_pSipData,contentLength,null,this.BeginReadData_Completed);
                            }
                            else{
                                m_pTLayer.Process(m_Transport,m_pSocket,m_pSipData.ToArray(),(IPEndPoint)m_pSocket.RemoteEndPoint);

                                // Wait for new SIP message. 
                                Start();
                            }
                        }
                        else{
                            // Check maximum allowed message size.
                            if(m_pTLayer.m_pSipStack.MaximumMessageSize > 0 && m_pSipData.Length > m_pTLayer.m_pSipStack.MaximumMessageSize){
                                Dispose();
                                return;
                            }

                            // Get next header line.
                            BeginReadHeaderLine();
                        }
                    }
                    else{
                        Dispose();
                    }
                }
                catch(Exception e){
                    m_pTLayer.m_pSipStack.OnError(e);
                }
            }

            #endregion

            #region method BeginReadData_Completed

            /// <summary>
            /// This method is called if content data reading has completed.
            /// </summary>
            /// <param name="result"></param>
            /// <param name="count"></param>
            /// <param name="x"></param>
            /// <param name="tag"></param>
            private void BeginReadData_Completed(SocketCallBackResult result,long count,Exception x,object tag)
            {
                try{
                    if(result == SocketCallBackResult.Ok){
                        m_pTLayer.Process(m_Transport,m_pSocket,m_pSipData.ToArray(),(IPEndPoint)m_pSocket.RemoteEndPoint);

                        // Wait for new SIP message. 
                        Start();
                    }
                    else{
                        Dispose();
                    }
                }
                catch(Exception e){
                    m_pTLayer.m_pSipStack.OnError(e);
                }
            }

            #endregion


            #region Properties Implementation

            /// <summary>
            /// Gets SIP transport(TCP or TLS) that pipe uses.
            /// </summary>
            public string Transport
            {
                get{ return m_Transport; }
            }

            /// <summary>
            /// Gets when was last read or write activity.
            /// </summary>
            public DateTime LastActivity
            {
                get{ return m_pSocket.LastActivity; }
            }

            /// <summary>
            /// Gets pipe local IP end point.
            /// </summary>
            public IPEndPoint LocalEndPoint
            {
                get{ return (IPEndPoint)m_pSocket.LocalEndPoint; }
            }

            /// <summary>
            /// Gets pipe remote end point.
            /// </summary>
            public IPEndPoint RemoteEndPoint
            {
                get{ return (IPEndPoint)m_pSocket.RemoteEndPoint; }
            }

            #endregion

        }

        #endregion

        private SIP_Stack               m_pSipStack         = null;
        private bool                    m_IsRunning         = false;
        private List<SipListeningPoint> m_pListeningPoints  = null;
        private List<SipTcpPipe>        m_pTcpReceiviePipes = null;
        private System.Timers.Timer     m_pTcpPipesTimer    = null;
        private int                     m_pTcpConTimeout    = 600;
                
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sipStack">Reference to SIP stack.</param>
        public SIP_TransportLayer(SIP_Stack sipStack)
        {
            m_pSipStack = sipStack;

            m_pListeningPoints = new List<SipListeningPoint>();
            m_pTcpReceiviePipes = new List<SipTcpPipe>();
                        
            m_pTcpPipesTimer = new System.Timers.Timer(20000);
            m_pTcpPipesTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_pTcpPipesTimer_Elapsed);
            m_pTcpPipesTimer.Enabled = true;
        }
                

        #region method Start

        /// <summary>
        /// Starts listening incoming requests and responses.
        /// </summary>
        public void Start()
        {
            if(m_IsRunning){
                return;
            }
            // Set this flag before running thread, otherwise thead may exist before you set this flag.
            m_IsRunning = true;

            Thread tr = new Thread(new ThreadStart(this.ProcessIncomingData));
            tr.Start();            
        }

        #endregion

        #region method Stop

        /// <summary>
        /// Stops listening incoming requests and responses.
        /// </summary>
        public void Stop()
        {
            if(!m_IsRunning){
                return;
            }
            m_IsRunning = false;

            // Close all TCP connections.
            foreach(SipTcpPipe pipe in m_pTcpReceiviePipes.ToArray()){
                pipe.Dispose();
            }

            m_pListeningPoints.Clear();            
        }

        #endregion


        #region method m_pTcpPipesTimer_Elapsed

        private void m_pTcpPipesTimer_Elapsed(object sender,System.Timers.ElapsedEventArgs e)
        {
            lock(m_pTcpReceiviePipes){
                foreach(SipTcpPipe pipe in m_pTcpReceiviePipes.ToArray()){
                    // Connection timed out, remove it.
                    if(pipe.LastActivity.AddSeconds(m_pTcpConTimeout) < DateTime.Now){
                        pipe.Dispose();
                    }
                }
            }
        }

        #endregion


        #region method ProcessIncomingData

        /// <summary>
        /// Processes incoming SIP UDP messages or incoming new SIP connections.
        /// </summary>
        private void ProcessIncomingData()
        {
            try{
                // If no binds, wait some to be created.
                while(m_pSipStack.BindInfo.Length == 0 && m_IsRunning){
                    System.Threading.Thread.Sleep(100);
                }

                //--- Create listening sockets --------------------------------------------------------------                
                foreach(BindInfo bindInfo in m_pSipStack.BindInfo){
                    // TCP
                    if(bindInfo.Protocol == BindInfoProtocol.TCP){
                        Socket s = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
				        s.Bind(new IPEndPoint(bindInfo.IP,bindInfo.Port));
				        s.Listen(500);

                        m_pListeningPoints.Add(new SipListeningPoint(bindInfo.Protocol,s,bindInfo.SSL,bindInfo.SSL_Certificate));
                    }
                    // UDP
                    else if(bindInfo.Protocol == BindInfoProtocol.UDP){
                        // If any IP, replicate socket for each IP, other can't get LocalEndpoint for UDP socket.
                        if(bindInfo.IP.Equals(IPAddress.Any)){
                            IPAddress[] addresses = System.Net.Dns.GetHostAddresses("");
                            foreach(IPAddress address in addresses){
                                // IPv4
                                if(address.AddressFamily == AddressFamily.InterNetwork){
                                    Socket s = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
				                    s.Bind(new IPEndPoint(address,bindInfo.Port));

                                    m_pListeningPoints.Add(new SipListeningPoint(bindInfo.Protocol,s,false,null));
                                }
                                // IPv6
                                /*
                                else{
                                    Socket s = new Socket(AddressFamily.InterNetworkV6,SocketType.Dgram,ProtocolType.Udp);
				                    s.Bind(new IPEndPoint(address,bindInfo.Port));

                                    m_pListeningPoints.Add(new SipListeningPoint(bindInfo.Protocol,s,false,null));
                                }*/
                            }
                        }
                        else{
                            Socket s = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
				            s.Bind(new IPEndPoint(bindInfo.IP,bindInfo.Port));

                            m_pListeningPoints.Add(new SipListeningPoint(bindInfo.Protocol,s,false,null));
                        }                        
                    }
                }

                CircleCollection<SipListeningPoint> binds = new CircleCollection<SipListeningPoint>();
                binds.Add(m_pListeningPoints.ToArray());
                //-------------------------------------------------------------------------------------------

                // Accept incoming connections and datagrams.
                while(m_IsRunning){
                    try{
                        // Check that maximum allowed connections not exceeded.
                        if(m_pSipStack.MaximumConnections > 0 && m_pTcpReceiviePipes.Count > m_pSipStack.MaximumConnections){
                            // Sleep here or CPU used 100%
                            Thread.Sleep(1);

                            // Step to next while loop.
                            continue;
                        }

                        // Check next listening point and queue it up when Socket has data or connection attampt.
                        SipListeningPoint listeningPoint = binds.Next();
                        if(listeningPoint.Socket.Poll(0,SelectMode.SelectRead)){
                            Socket socket = listeningPoint.Socket;

                            // TCP, TLS
                            if(listeningPoint.Protocol == BindInfoProtocol.TCP){
                                // Accept incoming connection.
					            SocketEx clientSocket = new SocketEx(socket.Accept());

                                // SSL
                                if(listeningPoint.SSL){
                                    clientSocket.SwitchToSSL(listeningPoint.Certificate);
                                }

                                // Queue for futher processing.
                                m_pTcpReceiviePipes.Add(new SipTcpPipe(this,clientSocket));
                            }
                            // UDP
                            else if(listeningPoint.Protocol == BindInfoProtocol.UDP){                            
                                // Receive data packet.
                                byte[] dataBuffer = new byte[32000];
                                EndPoint remoteEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any,0);                                                                
                                int size = socket.ReceiveFrom(dataBuffer,ref remoteEndPoint);

                                // Copy received data to new buffer what is exactly received size.
                                byte[] data = new byte[size];
                                Array.Copy(dataBuffer,data,size);

                                // Queue UDP datagram for processing.
                                ThreadPool.QueueUserWorkItem(
                                    new WaitCallback(this.Process),
                                    new SIP_Packet(socket,(IPEndPoint)remoteEndPoint,data)
                                );
                            }
                        }

                        // Sleep here or CPU used 100%
                        Thread.Sleep(1);
                    }
                    catch(SocketException x){
                        // Just skip recieve errors.

                        /* WSAETIMEDOUT 10060 Connection timed out. 
                            A connection attempt failed because the connected party did not properly respond 
                            after a period of time, or the established connection failed because the connected 
                            host has failed to respond.
                        */
                        if(x.ErrorCode == 10060){
                            // Skip
                        }
                        /* WSAECONNRESET 10054 Connection reset by peer. 
                            An existing connection was forcibly closed by the remote host. This normally results 
                            if the peer application on the remote host is suddenly stopped, the host is rebooted,
                           the host or remote network interface is disabled, or the remote host uses a hard close. 
                        */
                        else if(x.ErrorCode == 10054){
                            // Skip
                        }
                        else{
                            m_pSipStack.OnError(x);
                        }
                    }
                    catch(Exception x){                        
                        m_pSipStack.OnError(x);
                    }
                }
            }
            catch(Exception x){
                m_pSipStack.OnError(x);
            }
        }

        #endregion

        #region method Process

        /// <summary>
        /// Processes UDP received SIP packet.
        /// </summary>
        /// <param name="state">User data.</param>
        private void Process(object state)
        {
            SIP_Packet sipPacket = (SIP_Packet)state;

            Process(SIP_Transport.UDP,new SocketEx(sipPacket.Socket),sipPacket.Data,sipPacket.RemoteEndPoint);
        }

        /// <summary>
        /// Processes SIP data (request or response).
        /// </summary>
        /// <param name="socket">Socket what accepted specified SIP message.</param>
        /// <param name="data">SIP message or may be junk data too.</param>
        /// <param name="remoteEndPoint">Remote IP end point what sent SIP message.</param>
        /// <param name="transport">SIP transport what received message.</param>
        private void Process(string transport,SocketEx socket,byte[] data,IPEndPoint remoteEndPoint)
        {
            try{
                // Log
                m_pSipStack.Logger.AddRead(data.Length,"Received (" + data.Length + " bytes): " + socket.LocalEndPoint.ToString() + " <- " + remoteEndPoint.ToString() + "\r\n" + System.Text.Encoding.UTF8.GetString(data) + "\r\n");
                
                // Bad request or response.
                if(data.Length < 10){
                    return;
                }

                // TODO: Check that advertised transport matches actual received transport.
                              
                // Dedect if sip request or reponse.
                // Response
                if(System.Text.Encoding.UTF8.GetString(data,0,10).ToUpper().StartsWith("SIP")){
                    try{                        
                        SIP_Response response = SIP_Response.Parse(data);
                        response.Validate();

                        ProcessResponse(response);
                    }
                    catch(Exception x){
                        // We have bad response, just skip it.

                        // Log
                        if(m_pSipStack.Logger != null){
                            m_pSipStack.Logger.AddDebug("Skipping message, parse error: " + x.Message);
                        }
                    }
                }
                // Request
                else{
                    SIP_Request request = null;
                    try{                    
                        request = SIP_Request.Parse(data);
                        try{
                            request.Validate();
                        }
                        catch(Exception x){
                            // Log
                            if(m_pSipStack.Logger != null){
                                m_pSipStack.Logger.AddDebug("Invalid request: " + x.Message);
                            }

                            // Bad request, send error to request maker.
                            SendResponse(socket,request.CreateResponse(SIP_ResponseCodes.x400_Bad_Request + ". " + x.Message));
                            return;
                        }
                    }
                    catch(Exception x){
                        // Log
                        if(m_pSipStack.Logger != null){
                            m_pSipStack.Logger.AddDebug("Invalid request: " + x.Message);
                        }

                        // We have bad request, try to send bad request error to request maker.
                        SIP_Response badRequestResponse = new SIP_Response();
                        badRequestResponse.StatusCode_ReasonPhrase = SIP_ResponseCodes.x400_Bad_Request;
                        socket.SendTo(badRequestResponse.ToByteData(),remoteEndPoint);
                        return;
                    }

                    SIP_ValidateRequestEventArgs eArgs = m_pSipStack.OnValidateRequest(request,remoteEndPoint);
                    // Request validated, allow transport layer to handle it.
                    if(eArgs.ResponseCode == null){
                        ProcessRequest(request,socket,remoteEndPoint);
                    }
                    // Request rejected, return response.
                    else{
                        SendResponse(socket,request.CreateResponse(eArgs.ResponseCode));
                    }
                }                 
            }
            catch(SocketException s){
                // Skip all socket errors here
                string dummy = s.Message;
            }
            catch(Exception x){
                m_pSipStack.OnError(x);
            }
        }

        #endregion

        #region method ProcessRequest

        /// <summary>
        /// Processes incoming request.
        /// </summary>
        /// <param name="request">SIP request.</param>
        /// <param name="socket">Socket which made request.</param>
        /// <param name="remoteEndPoint">Remote end point what created request.</param>
        private void ProcessRequest(SIP_Request request,SocketEx socket,IPEndPoint remoteEndPoint)
        {
            /* RFC 3581 4. Server Behavior.
                When a server compliant to this specification (which can be a proxy
                or UAS) receives a request, it examines the topmost Via header field
                value.  If this Via header field value contains an "rport" parameter
                with no value, it MUST set the value of the parameter to the source
                port of the request.  This is analogous to the way in which a server
                will insert the "received" parameter into the topmost Via header
                field value.  In fact, the server MUST insert a "received" parameter
                containing the source IP address that the request came from, even if
                it is identical to the value of the "sent-by" component.
            */

             /* RFC 3261 18.2.1 Receiving Requests.            
                 Next, the server transport attempts to match the request to a server
                 transaction.  It does so using the matching rules described in
                 Section 17.2.3.  If a matching server transaction is found, the
                 request is passed to that transaction for processing.  If no match is
                 found, the request is passed to the core, which may decide to
                 construct a new server transaction for that request.  Note that when
                 a UAS core sends a 2xx response to INVITE, the server transaction is
                 destroyed.  This means that when the ACK arrives, there will be no
                 matching server transaction, and based on this rule, the ACK is
                 passed to the UAS core, where it is processed.
            */

            // Store request socket.
            request.Socket = socket;
            request.RemoteEndPoint = remoteEndPoint;

            // Add received to via and rport if empty value specified in header.
            SIP_t_ViaParm via = request.Via.GetTopMostValue();
            // RFC 3581 4. say add always, this causes troubles with some routers, so add if needed.
            if(via.SentBy.Split(':')[0] != remoteEndPoint.Address.ToString()){
                via.Received = remoteEndPoint.Address.ToString();
            }
            // *** If via host LAN IP, always add rport. Thats not defined in RFC.
            if(via.RPort == 0/* || Core.IsPrivateIP(via.Host.Split(':')[0])*/){
                via.RPort = remoteEndPoint.Port;
            }

            /* Try to match BYE to dialog. If match found pass to daialog for processing.
               There we bend RFC, rfc says that TU must handle BYE. We just hide BYE from end user and
               process it in dialog as needed. When somebody know why we may not do so, let me know.            
            */
            if(request.Method == "BYE"){
                SIP_Dialog dialog = m_pSipStack.TransactionLayer.MatchDialog(request);
                if(dialog != null){
                    dialog.ProcessRequest(request);
                }
                // Pass request to core.
                else{
                    m_pSipStack.SipCore.OnRequestReceived(new SIP_RequestReceivedEventArgs(m_pSipStack,request));
                }
            }
            else{
                SIP_ServerTransaction transaction = m_pSipStack.TransactionLayer.MatchServerTransaction(request);
                // Pass to matched server transaction.
                if(transaction != null){
                    transaction.ProcessRequest(request);
                }
                // Pass request to core.
                else{
                    m_pSipStack.SipCore.OnRequestReceived(new SIP_RequestReceivedEventArgs(m_pSipStack,request));
                }
            }
        }

        #endregion

        #region method ProcessResponse

        /// <summary>
        /// Processes SIP recipient response.
        /// </summary>
        /// <param name="response">SIP response.</param>
        private void ProcessResponse(SIP_Response response)
        {
            /* RFC 3261 18.1.2 Receiving Responses.
                When a response is received, the client transport examines the top
                Via header field value.  If the value of the "sent-by" parameter in
                that header field value does not correspond to a value that the
                client transport is configured to insert into requests, the response
                MUST be silently discarded.
              
                Match resposne to client transaction and pass to it or the response MUST be 
                passed to the core if no matching client transaction.
            */

            //--- Check sent-by. ----------------------------------------------------------------
            // FIX ME: TODO:
            /*
            bool isValidSentBy = false;
            SIP_t_ViaParam via = response.Via.GetTopMostValue();
            if(via.SentBy == m_pSipStack.HostName){
                isValidSentBy = true;
            }
            else{
                // Check all listening points.
                foreach(SipListeningPoint linteningPoint in m_pListeningPoints){
                    if(via.SentBy == linteningPoint.LocalEndPoint.ToString()){
                        isValidSentBy = true;
                        break;
                    }
                }
              
                // For TCP, we must check active connections too if no suitable bind found.
                if(!isValidSentBy){
                    lock(m_pTcpReceiviePipes){
                        foreach(SipTcpPipe pipe in m_pTcpReceiviePipes){
                            if(via.SentBy == pipe.LocalEndPoint.ToString()){
                                isValidSentBy = true;
                                break;
                            }
                        }
                    }
                }
            }
            if(!isValidSentBy){
                m_pSipStack.Logger.AddDebug("Discarding stray repsponse, now waited sent-by value '" + via.SentBy + "' !");
                return;
            }*/
            //-----------------------------------------------------------------------------------

            SIP_ClientTransaction transaction =  m_pSipStack.TransactionLayer.MatchClientTransaction(response);
            // Allow client transaction to process response.
            if(transaction != null){
                transaction.ProcessResponse(response);
            }
            // Pass response to core.
            else{
                m_pSipStack.SipCore.OnResponseReceived(new SIP_ResponseReceivedEventArgs(m_pSipStack,null,response));
            }
        }

        #endregion


        #region method SendRequest
             
        /// <summary>
        /// Sends specified request to the host whats specified in Request-URI. NOTE: Request-URI must
        /// be sip or sips URI, otherwise exception is thrown.
        /// </summary>
        /// <param name="request">SIP request to send.</param>
        /// <exception cref="SIP_TransportException">Is raised when sending fails.</exception>
        public void SendRequest(SIP_Request request)
        {
            // Request-URI can be only sip or sips uri, otherwise we don't know where to send request.
            SIP_Uri requestURI = null;
            try{
                requestURI = SIP_Uri.Parse(request.Uri);
            }
            catch{
                throw new SIP_TransportException("Property request.URI is not SIP uri !");
            }
                        
            SendRequest(request,new SIP_Destination(requestURI));
        }

        /// <summary>
        /// Sends request to the specified destination.
        /// </summary>
        /// <param name="request">SIP request to send.</param>
        /// <param name="destination">Destination info.</param>
        /// <exception cref="SIP_TransportException">Is raised when sending fails.</exception>
        public void SendRequest(SIP_Request request,SIP_Destination destination)
        {
            try{
                SendRequest(request,destination.LocalEndPoint,new IPEndPoint(System.Net.Dns.GetHostAddresses(destination.Host)[0],destination.Port),destination.Transport);
            }
            catch(Exception x){
                throw new SIP_TransportException(x.Message);
            }
        }

        /// <summary>
        /// Sends specified request to specified SIP server.
        /// </summary>
        /// <param name="request">SIP request to send.</param>
        /// <param name="localEndPoint">Local end point to use. Value null means first available. NOTE: This is used only if transport = UDP.</param>
        /// <param name="remoteEndPoint">Destination remote endpoint where to send request.</param>
        /// <param name="transport">SIP transport to use. Supported values are defined in SIP_Transport class.</param>
        /// <exception cref="SIP_TransportException">Is raised when sending fails.</exception>
        public void SendRequest(SIP_Request request,SIP_EndPointInfo localEndPoint,IPEndPoint remoteEndPoint,string transport)
        {
            /* RFC 3561 18.1.1 Sending Requests.
                The client side of the transport layer is responsible for sending the
                request and receiving responses.  The user of the transport layer
                passes the client transport the request, an IP address, port,
                transport, and possibly TTL for multicast destinations.
             
                A client that sends a request to a multicast address MUST add the
                "maddr" parameter to its Via header field value containing the
                destination multicast address, and for IPv4, SHOULD add the "ttl"
                parameter with a value of 1.
             
                Before a request is sent, the client transport MUST insert a value of
                the "sent-by" field into the Via header field.  This field contains
                an IP address or host name, and port.  The usage of an FQDN is RECOMMENDED.
              
                For reliable transports, the response is normally sent on the
                connection on which the request was received.  Therefore, the client
                transport MUST be prepared to receive the response on the same
                connection used to send the request.
            
                For unreliable unicast transports, the client transport MUST be
                prepared to receive responses on the source IP address from which the request is sent.
              
                For multicast, the client transport MUST be prepared to receive
                responses on the same multicast group and port to which the request is sent.
            */
               
            try{
                byte[]        data = null;
                SipTcpPipe    pipe = null;
                SIP_t_ViaParm via  = request.Via.GetTopMostValue();
                
                /*
                    If UDP or TCP we always can override "transport" to reuse any existing TCP/TLS connection.                    
                    If TLS we can reuse only existing TLS connection.
                */                
                if(transport.ToUpper() == SIP_Transport.UDP || transport.ToUpper() == SIP_Transport.TCP){
                    pipe = GetTcpPipe(null,remoteEndPoint);
                    // Not existing connection.
                    if(pipe == null){
                        // Not existing connection, create it.
                        if(transport == SIP_Transport.TCP){
                            pipe = new SipTcpPipe(this,remoteEndPoint,false);
                        }
                        // Use UDP socket.
                        else{
                            foreach(SipListeningPoint listeningPoint in m_pListeningPoints){
                                if(listeningPoint.Protocol == BindInfoProtocol.UDP){
                                    if(localEndPoint == null || listeningPoint.LocalEndPoint.Equals(localEndPoint.EndPoint)){
                                        // Set sent-by value.
                                        if(!string.IsNullOrEmpty(m_pSipStack.HostName)){
                                            via.SentBy = m_pSipStack.HostName;
                                        }
                                        else if(pipe.LocalEndPoint.Address.Equals(IPAddress.Any)){
                                            // Get first IP
                                            via.SentBy = System.Net.Dns.GetHostAddresses("")[0] + ":" + pipe.LocalEndPoint.Port;
                                        }
                                        else{
                                            via.SentBy = listeningPoint.LocalEndPoint.ToString();
                                        }

                                        data = request.ToByteData();

                                        // Log
                                        m_pSipStack.Logger.AddWrite(data.Length,"Sending (" + data.Length + " bytes): " + listeningPoint.LocalEndPoint.ToString() + " -> " + remoteEndPoint.ToString() + "\r\n<begin>\r\n" + System.Text.Encoding.UTF8.GetString(data) + "<end>\r\n");

                                        listeningPoint.Socket.SendTo(data,remoteEndPoint);
                                        return;
                                    }
                                }
                            }

                            throw new SIP_TransportException("No suitable UDP socket !");
                        }
                    }
                    // We can reuse existing connection, use it.
                    // We stored pipe, we use it later.
                    //else{
                    //}
                }
                else if(transport.ToUpper() == SIP_Transport.TLS){
                    pipe = GetTcpPipe(SIP_Transport.TLS,remoteEndPoint);
                    // Not existing connection, create it.
                    if(pipe == null){
                        pipe = new SipTcpPipe(this,remoteEndPoint,true);
                    }                    
                }
                else{
                    throw new SIP_TransportException("No transport for protocol '" + transport + "' !");
                }

                // Set sent-by value.
                if(!string.IsNullOrEmpty(m_pSipStack.HostName)){
                    via.SentBy = m_pSipStack.HostName;
                }
                else if(pipe.LocalEndPoint.Address.Equals(IPAddress.Any)){
                    // Get first IP
                    via.SentBy = System.Net.Dns.GetHostAddresses("")[0] + ":" + pipe.LocalEndPoint.Port;
                }
                else{
                    via.SentBy = pipe.LocalEndPoint.ToString();
                }
                                                
                data = request.ToByteData();

                // Log
                m_pSipStack.Logger.AddWrite(data.Length,"Sending (" + data.Length + " bytes): " + pipe.LocalEndPoint.ToString() + " -> " + remoteEndPoint.ToString() + "\r\n<begin>\r\n" + System.Text.Encoding.UTF8.GetString(data) + "<end>\r\n");
           
                // Send request.
                pipe.SendMessage(data);
            }
            catch(Exception x){
                throw new SIP_TransportException(x.Message);
            }
        }

        #endregion

        #region method SendResponse

        /// <summary>
        /// Sends response to request maker.
        /// </summary>
        /// <param name="socket">Socket which to use to send response.</param>
        /// <param name="response">Response to send.</param>
        public void SendResponse(SocketEx socket,SIP_Response response)
        {
            SendResponse(socket,null,response);
        }

        /// <summary>
        /// Sends response to request maker.
        /// </summary>
        /// <param name="socket">Socket which to use to send response.</param>
        /// <param name="remoteEndPoint">Remote end point where to send response. 
        /// If this value is null, Via header is used to get remote end point.
        /// </param>
        /// <param name="response">Response to send.</param>
        public void SendResponse(SocketEx socket,IPEndPoint remoteEndPoint,SIP_Response response)
        {
            /* RFC 3581 4.  Server Behavior.
                When a server attempts to send a response, it examines the topmost
                Via header field value of that response.  If the "sent-protocol"
                component indicates an unreliable unicast transport protocol, such as
                UDP, and there is no "maddr" parameter, but there is both a
                "received" parameter and an "rport" parameter, the response MUST be
                sent to the IP address listed in the "received" parameter, and the
                port in the "rport" parameter.  The response MUST be sent from the
                same address and port that the corresponding request was received on
                in order to traverse symmetric NATs.

            */

            /* RFC 3261 18.2.2 Sending Responses.
                The server transport uses the value of the top Via header field in
                order to determine where to send a response.  It MUST follow the
                following process:

                  o  If the "sent-protocol" is a reliable transport protocol such as
                     TCP or SCTP, or TLS over those, the response MUST be sent using
                     the existing connection to the source of the original request
                     that created the transaction, if that connection is still open.
                     This requires the server transport to maintain an association
                     between server transactions and transport connections.  If that
                     connection is no longer open, the server SHOULD open a
                     connection to the IP address in the "received" parameter, if
                     present, using the port in the "sent-by" value, or the default
                     port for that transport, if no port is specified.  If that
                     connection attempt fails, the server SHOULD use the procedures
                     in [4] for servers in order to determine the IP address and
                     port to open the connection and send the response to.

                  o  Otherwise, if the Via header field value contains a "maddr"
                     parameter, the response MUST be forwarded to the address listed
                     there, using the port indicated in "sent-by", or port 5060 if
                     none is present.  If the address is a multicast address, the
                     response SHOULD be sent using the TTL indicated in the "ttl"
                     parameter, or with a TTL of 1 if that parameter is not present.

                  o  Otherwise (for unreliable unicast transports), if the top Via
                     has a "received" parameter, the response MUST be sent to the
                     address in the "received" parameter, using the port indicated
                     in the "sent-by" value, or using port 5060 if none is specified
                     explicitly.  If this fails, for example, elicits an ICMP "port
                     unreachable" response, the procedures of Section 5 of [4]
                     SHOULD be used to determine where to send the response.

                  o  Otherwise, if it is not receiver-tagged, the response MUST be
                     sent to the address indicated by the "sent-by" value, using the
                     procedures in Section 5 of [4].
            */

            // TODO: Probably we can use local endpoint instead of socket. Because then we can             
            //       Search right UDP connection or TCP/TLS connection.  
            
            try{                
                SIP_t_ViaParm via = response.Via.GetTopMostValue();

                // End point not specified, get it from Via.
                if(remoteEndPoint == null){                     
                    string host = null;
                    int    port = 5060;

                    // Use received host.
                    if(via.Received != null){
                        host = via.Received;
                    }
                    // Get sent-by host
                    else{
                        host = via.SentBy.Split(':')[0];
                    }
                            
                    // Use rport if recevived is specified too
                    if(via.Received != null && via.RPort > 0){
                        port = via.RPort;
                    }
                    // Get port from sent-by
                    else{
                        string[] host_port = via.SentBy.Split(':');
                        if(host_port.Length == 2){
                            port = Convert.ToInt32(host_port[1]);
                        }
                    }
                   
                    remoteEndPoint = new IPEndPoint(System.Net.Dns.GetHostAddresses(host)[0],port);
                }

                byte[] data = response.ToByteData();

                // Log
                m_pSipStack.Logger.AddWrite(data.Length,"Sending (" + data.Length + " bytes): " + socket.LocalEndPoint.ToString() + " -> " + remoteEndPoint.ToString() + "\r\n<begin>\r\n" + System.Text.Encoding.UTF8.GetString(data) + "<end>\r\n");

                // We don't have any more that socket what accepted request which response it is.
                // There are 2 known cases when no socket:
                //   1) Stateless proxy.
                //   2) Statefull proxy, but response didn't match any transaction.
                if(socket == null){
                    // UDP Multicast
                    if(via.ProtocolTransport.ToUpper() == SIP_Transport.UDP && via.Maddr != null){
                        throw new SIP_TransportException("UDP Multicast not implemented !");
                    }
                    // UDP
                    else if(via.ProtocolTransport.ToUpper() == SIP_Transport.UDP){          
                        foreach(SipListeningPoint listeningPoint in m_pListeningPoints){
                            if(listeningPoint.Protocol == BindInfoProtocol.UDP){
                                listeningPoint.Socket.SendTo(data,remoteEndPoint);
                                return;
                            }
                        }

                        throw new SIP_TransportException("No UDP transport available, this never should happen !");
                    }            
                    // TCP
                    else if(via.ProtocolTransport.ToUpper() == SIP_Transport.TCP){
                        SipTcpPipe pipe = GetTcpPipe(SIP_Transport.TCP,remoteEndPoint);
                        // Not existing connection, create it.
                        if(pipe == null){
                            pipe = new SipTcpPipe(this,remoteEndPoint,true);
                        }
                        pipe.SendMessage(data);
                    }
                    // TCP TLS            
                    else if(via.ProtocolTransport.ToUpper() == SIP_Transport.TLS){
                        SipTcpPipe pipe = GetTcpPipe(SIP_Transport.TLS,remoteEndPoint);
                        // Not existing connection, create it.
                        if(pipe == null){
                            pipe = new SipTcpPipe(this,remoteEndPoint,true);
                        }
                        pipe.SendMessage(data);
                    }
                }
                // We have existing socket, use it.
                else{
                    if(via.ProtocolTransport.ToUpper() == SIP_Transport.UDP){
                        socket.SendTo(data,remoteEndPoint);                        
                    }
                    else{
                        socket.Write(data);
                    }
                }
            }
            catch(Exception x){
                throw new SIP_TransportException(x.Message);
            }
        }

        #endregion


        #region method GetTcpPipe

        /// <summary>
        /// Gets specified TCP pipe. Returns null if specified pipe doesn't exist.
        /// </summary>
        /// <param name="protocol">SIP protocol or null if TCP/TLS.</param>
        /// <param name="endPoint">Remote IP end point.</param>
        /// <returns>Returns TCP pipe or null if no match.</returns>
        private SipTcpPipe GetTcpPipe(string protocol,IPEndPoint endPoint)
        {
            lock(m_pTcpReceiviePipes){
                foreach(SipTcpPipe pipe in m_pTcpReceiviePipes){
                    try{
                        if(pipe.RemoteEndPoint.Equals(endPoint)){
                            if(string.IsNullOrEmpty(protocol)){
                                return pipe;
                            }
                            else if(protocol.ToUpper() == pipe.Transport.ToUpper()){
                                return pipe;
                            }
                        }
                    }
                    catch{
                        // Skip errors here, only error case is when pipe is disposing, but we don't care about that.
                    }
                }
            }
            return null;
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets if transport layer is running.
        /// </summary>
        public bool IsRunning
        {
            get{ return m_IsRunning; }
        }

        #endregion

    }
}
