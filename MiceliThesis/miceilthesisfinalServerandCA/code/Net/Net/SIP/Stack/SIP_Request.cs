using System;
using System.IO;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Net;

using LumiSoft.Net.SIP.Message;

namespace LumiSoft.Net.SIP.Stack
{
    /// <summary>
    /// SIP server request. Related RFC 3261.
    /// </summary>
    public class SIP_Request : SIP_Message
    {
        private string     m_Method          = "";
        private string     m_Uri             = "";
        private string     m_SipVersion      = "";
        private SocketEx   m_pSocket         = null;
        private IPEndPoint m_pRemoteEndPoint = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_Request()
        {
            m_SipVersion = "SIP/2.0";
        }


        #region method Copy

        /// <summary>
        /// Clones this request.
        /// </summary>
        /// <returns>Returns new cloned request.</returns>
        public SIP_Request Copy()
        {
            SIP_Request retVal = SIP_Request.Parse(this.ToByteData());
            retVal.Socket = m_pSocket;
            retVal.RemoteEndPoint = m_pRemoteEndPoint;

            return retVal;
        }

        #endregion


        #region method Validate

        /// <summary>
        /// Checks if SIP request has all required values as request line,header fields and their values.
        /// Throws Exception if not valid SIP request.
        /// </summary>
        public void Validate()
        {
            // Request SIP version
            // Via: + branch prameter
            // To:
            // From:
            // CallID:
            // CSeq
            // Max-Forwards RFC 3261 8.1.1.

            if(!m_SipVersion.ToUpper().StartsWith("SIP/2.0")){
                throw new SIP_ParseException("Not supported SIP version '" + m_SipVersion + "' !");
            }

            if(this.Via.GetTopMostValue() == null){
                throw new SIP_ParseException("Via: header field is missing !");
            }
            if(this.Via.GetTopMostValue().Branch == null){
                throw new SIP_ParseException("Via: header field branch parameter is missing !");
            }

            if(this.To == null){
                throw new SIP_ParseException("To: header field is missing !");
            }

            if(this.From == null){
                throw new SIP_ParseException("From: header field is missing !");
            }

            if(this.CallID == null){
                throw new SIP_ParseException("CallID: header field is missing !");
            }

            if(this.CSeq == null){
                throw new SIP_ParseException("CSeq: header field is missing !");
            }

            if(this.MaxForwards == -1){
                // We can fix it by setting it to default value 70.
                this.MaxForwards = 70;
            }


            // If INVITE, contact field is mandatory.
            if(this.Method == "INVITE" && this.Contact.GetAllValues().Length == 0){
                throw new SIP_ParseException("Contact: header field is missing !");
            }

            // TODO: get in transport made request, so check if sips and sip set as needed.
        }

        #endregion

        #region method Parse

        /// <summary>
        /// Parses SIP_Request from byte array.
        /// </summary>
        /// <param name="data">Valid SIP request data.</param>
        /// <returns>Returns parsed SIP_Request obeject.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>data</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public static SIP_Request Parse(byte[] data)
        {
            if(data == null){
                throw new ArgumentNullException("data");
            }

            return Parse(new MemoryStream(data));
        }

        /// <summary>
        /// Parses SIP_Request from stream.
        /// </summary>
        /// <param name="stream">Stream what contains valid SIP request.</param>
        /// <returns>Returns parsed SIP_Request obeject.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>stream</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public static SIP_Request Parse(Stream stream)
        {
            /* Syntax:
                SIP-Method SIP-URI SIP-Version
                SIP-Message                          
            */

            if(stream == null){
                throw new ArgumentNullException("stream");
            }

            SIP_Request retVal = new SIP_Request();

            // Parse Response-line
            StreamLineReader r = new StreamLineReader(stream);
            r.Encoding = "utf-8";
            string[] method_uri_version = r.ReadLineString().Split(' ');
            if(method_uri_version.Length != 3){
                throw new Exception("Invalid SIP request data ! Method line doesn't contain: SIP-Method SIP-URI SIP-Version.");
            }
            retVal.m_Method     = method_uri_version[0];
            retVal.m_Uri        = method_uri_version[1];
            retVal.m_SipVersion = method_uri_version[2];

            // Parse SIP message
            retVal.InternalParse(stream);

            return retVal;
        }

        #endregion

        #region method ToStream

        /// <summary>
        /// Stores SIP_Request to specified stream.
        /// </summary>
        /// <param name="stream">Stream where to store.</param>
        public void ToStream(Stream stream)
        {
            // Request-Line = Method SP Request-URI SP SIP-Version CRLF

            // Add request-line
            byte[] responseLine = Encoding.UTF8.GetBytes(m_Method + " " + m_Uri + " " + m_SipVersion + "\r\n");
            stream.Write(responseLine,0,responseLine.Length);

            // Add SIP-message
            this.InternalToStream(stream);
        }

        #endregion

        #region method ToByteData

        /// <summary>
        /// Converts this request to raw srver request data.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteData()
        {
            MemoryStream retVal = new MemoryStream();
            ToStream(retVal);

            return retVal.ToArray();
        }

        #endregion


        #region method CreateResponse

        /// <summary>
        /// Creates response to this request.
        /// </summary>
        /// <param name="statusCode_ReasonPhrase">SIP Status-Code with Reason-Phrase (Status-Code SP Reason-Phrase).</param>
        /// <returns>Returns SIP response with specified code and copy of all original headers from this request.</returns>
        public SIP_Response CreateResponse(string statusCode_ReasonPhrase)
        {
            /* RFC 3261 8.2.6.
                The From field of the response MUST equal the From header field of
                the request.  The Call-ID header field of the response MUST equal the
                Call-ID header field of the request.  The CSeq header field of the
                response MUST equal the CSeq field of the request.  The Via header
                field values in the response MUST equal the Via header field values
                in the request and MUST maintain the same ordering.

                If a request contained a To tag in the request, the To header field
                in the response MUST equal that of the request.  However, if the To
                header field in the request did not contain a tag, the URI in the To
                header field in the response MUST equal the URI in the To header
                field; additionally, the UAS MUST add a tag to the To header field in
                the response (with the exception of the 100 (Trying) response, in
                which a tag MAY be present).  This serves to identify the UAS that is
                responding, possibly resulting in a component of a dialog ID.  The
                same tag MUST be used for all responses to that request, both final
                and provisional (again excepting the 100 (Trying)).
            */

            SIP_Response response = new SIP_Response();
            response.StatusCode_ReasonPhrase = statusCode_ReasonPhrase;
            foreach(SIP_t_ViaParm via in this.Via.GetAllValues()){
                response.Via.Add(via.ToStringValue());
            }
            response.From   = this.From;
            response.To     = this.To;
            response.CallID = this.CallID;
            response.CSeq   = this.CSeq;

            return response;
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets request method(REGISTER,INVITE,...) related to this request. This is always in upper-case.
        /// </summary>
        public string Method
        {
            get{ return m_Method.ToUpper(); }

            set{
                if(string.IsNullOrEmpty(value)){
                    throw new ArgumentNullException("Property SipMethod value can't be '' or null !");
                }

                m_Method = value;
            }
        }
        
        /// <summary>
        /// Gets request URI.
        /// </summary>
        public string Uri
        {
            get{ return m_Uri; }

            set{
                m_Uri = value; 
            }
        }

        /// <summary>
        /// Gets or sets SIP request version. Normally this must be always 'SIP/2.0'.
        /// </summary>
        public string SipVersion
        {
            get{ return m_SipVersion; }

            set{
                if(string.IsNullOrEmpty(value)){
                    throw new ArgumentException("Property SipVersion can't be '' or null !");
                }

                m_SipVersion = value;
            } 
        }


        /// <summary>
        /// Gets or sets socket what received request. This socket is needed for sending response, we need
        /// to send response back to same socket. For TCP it uses open connection, for UDP this ensures
        /// that symmetric NAT works.
        /// </summary>
        internal SocketEx Socket
        {
            get{ return m_pSocket; }
            
            set{ m_pSocket = value; }
        }

        /// <summary>
        /// Gets or sets remote end point what sent this request. Returns null if this request isnt received one !.
        /// </summary>
        internal IPEndPoint RemoteEndPoint
        {
            get{ return m_pRemoteEndPoint; }

            set{ m_pRemoteEndPoint = value; }
        }

        #endregion

    }    
}
