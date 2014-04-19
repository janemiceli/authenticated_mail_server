using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace LumiSoft.Net.SIP
{
    /// <summary>
    /// FIX ME: temporary class, make some global logging functionality.
    /// </summary>
    public class SipLogger
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SipLogger()
        {
        }

        #region method AddDebugg

        /// <summary>
        /// Logs specified debug text.
        /// </summary>
        /// <param name="text"></param>
        public void AddDebug(string text)
        {
            try{
                //System.IO.File.AppendAllText("c:\\sipX.txt",text + "\r\n");
            }
            catch{
            }
        }

        #endregion

        #region method AddSend

        /// <summary>
        /// Logs specified send.
        /// </summary>
        /// <param name="data">Data what was sent.</param>
        /// <param name="localEndPoint">Local IP end point.</param>
        /// <param name="remoteEndPoint">Remote IP end point.</param>
        public void AddSend(byte[] data,IPEndPoint localEndPoint,IPEndPoint remoteEndPoint)
        {
            try{
                string text = "Sending (" + data.Length + " bytes): " + localEndPoint.ToString() + " -> " + remoteEndPoint.ToString() + "\r\n<begin>\r\n" + System.Text.Encoding.UTF8.GetString(data) + "<end>\r\n";
                //System.IO.File.AppendAllText("c:\\sipx.txt",text);
            }
            catch{
            }
        }

        #endregion

        #region method AddReceive

        /// <summary>
        /// Logs specified receive.
        /// </summary>
        /// <param name="data">Data what was received.</param>
        /// <param name="localEndPoint">Local IP end point.</param>
        /// <param name="remoteEndPoint">Remote IP end point.</param>
        public void AddReceive(byte[] data,IPEndPoint localEndPoint,IPEndPoint remoteEndPoint)
        {
            try{
                //System.IO.File.AppendAllText("c:\\sipx.txt","Received (" + data.Length + " bytes): " + localEndPoint.ToString() + " <- " + remoteEndPoint.ToString() + "\r\n" + System.Text.Encoding.UTF8.GetString(data) + "\r\n");
            }
            catch{
            }
        }

        #endregion
    }
}
