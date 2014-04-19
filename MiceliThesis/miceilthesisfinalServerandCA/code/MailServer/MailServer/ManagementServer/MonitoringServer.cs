using System;
using System.Net;
using System.Net.Sockets;
using LumiSoft.Net;

namespace LumiSoft.MailServer.Monitoring
{
	/// <summary>
	/// LumiSoft mailserver monitoring server.
	/// </summary>
	public class MonitoringServer : SocketServer
	{
		private Server m_pServer = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public MonitoringServer(Server server) : base()
		{
			m_pServer = server;

			this.BindInfo = new BindInfo[]{new BindInfo(IPAddress.Any,5252,false,null)};
            this.SessionIdleTimeOut = 30 * 60 * 1000; // Session idle timeout after 30 min
        }


        #region method InitNewSession

        /// <summary>
		/// Initialize and start new session here. Session isn't added to session list automatically, 
		/// session must add itself to server session list by calling AddSession().
		/// </summary>
		/// <param name="socket">Connected client socket.</param>
        /// <param name="bindInfo">BindInfo what accepted socket.</param>
		protected override void InitNewSession(Socket socket,BindInfo bindInfo)
		{	
            string   sessionID = Guid.NewGuid().ToString();
            SocketEx socketEx  = new SocketEx(socket);
            /*if(LogCommands){
                socketEx.Logger = new SocketLogger(socket,this.SessionLog);
				socketEx.Logger.SessionID = sessionID;
            }*/
			MonitoringServerSession session = new MonitoringServerSession(sessionID,socketEx,bindInfo,this);
        }

        #endregion

        internal void AddSessionX(SocketServerSession session)
		{
			AddSession(session);
		}

		internal void RemoveSessionX(SocketServerSession session)
		{
			RemoveSession(session);
        }


        #region Properties Implementation

        /// <summary>
		/// Gets reference to MailServer.
		/// </summary>
		public Server MailServer
		{
			get{ return m_pServer; }
        }

        #endregion

    }
}
