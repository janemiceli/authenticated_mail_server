using System;
using System.Net;

namespace LumiSoft.Net
{
	/// <summary>
	/// Common interface for socket server sessions.
	/// </summary>
	public interface ISocketServerSession
    {
        #region method Kill

        /// <summary>
        /// Kills session.
        /// </summary>
        void Kill();

        #endregion

        #region method OnSessionTimeout

        /// <summary>
		/// For internal use only !
		/// </summary>
		void OnSessionTimeout();

        #endregion


        #region Properties Implemtation

        /// <summary>
		/// Gets session ID.
		/// </summary>
		string SessionID
		{
			get;
		}

        /// <summary>
		/// Gets session start time.
		/// </summary>
		DateTime SessionStartTime
		{
			get;
		}

        /// <summary>
        /// Gets how many seconds has left before timout is triggered.
        /// </summary>
        int ExpectedTimeout
        {
            get;
        }

        /// <summary>
		/// Gets last data activity time.
		/// </summary>
		DateTime SessionLastDataTime
		{
			get;
        }

        /// <summary>
		/// Gets loggded in user name (session owner).
		/// </summary>
		string UserName
		{
			get;
		}

        /// <summary>
		/// Gets EndPoint which accepted conection.
		/// </summary>
		IPEndPoint LocalEndPoint
		{
			get;
		}

		/// <summary>
		/// Gets connected Host(client) EndPoint.
		/// </summary>
		IPEndPoint RemoteEndPoint
		{
			get;
		}
		
		/// <summary>
		/// Gets or sets custom user data.
		/// </summary>
		object Tag
		{
			get;

			set;
		}

        /// <summary>
        /// Gets log entries that are currently in log buffer.
        /// </summary>
        SocketLogger SessionActiveLog
        {
            get;
        }

        /// <summary>
        /// Gets how many bytes are readed through this session.
        /// </summary>
        long ReadedCount
        {
            get;
        }
        
        /// <summary>
        /// Gets how many bytes are written through this session.
        /// </summary>
        long WrittenCount
        {
            get;
        }
        
        /// <summary>
        /// Gets if the connection is an SSL connection.
        /// </summary>
        bool IsSecureConnection
        {
            get;
        }

        #endregion
    }
}
