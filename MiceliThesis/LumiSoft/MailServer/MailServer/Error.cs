using System;
using System.Diagnostics;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Error handling.
	/// </summary>
	internal class Error
	{
		private static string m_Path = "";

		#region function DumpError

		/// <summary>
		/// Writes error to error log file.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="stackTrace"></param>
		public static void DumpError(Exception x,StackTrace stackTrace)
		{
			try
			{
				string source = stackTrace.GetFrame(0).GetMethod().DeclaringType.FullName + "." + stackTrace.GetFrame(0).GetMethod().Name + "()";

				string errorText  = "";
					   errorText += "//------------- function:" + source + "  " + DateTime.Now.ToString() + "------------//\r\n";
					   errorText += x.Source + ":" + x.Message + "\r\n";
					   errorText += x.StackTrace;

				if(x is System.Data.SqlClient.SqlException){
					System.Data.SqlClient.SqlException sX = (System.Data.SqlClient.SqlException)x;
					errorText += "\r\n\r\nSql errors:\r\n";

					foreach(System.Data.SqlClient.SqlError sErr in sX.Errors){
						errorText += "\n";
						errorText += "Procedure: '" + sErr.Procedure + "'  line: " + sErr.LineNumber.ToString() + "  error: " + sErr.Number.ToString() + "\r\n";
						errorText += "Message: " + sErr.Message + "\r\n";
					}				
				}

				SCore.WriteLog(m_Path + "mailServiceError.log",errorText);
			}
			catch{
			}
		}

		#endregion


		#region Propertis Implementation

		/// <summary>
		/// Gets or sets error file path.
		/// </summary>
		public static string ErrorFilePath
		{
			get{ return m_Path; }

			set{ m_Path = value; }
		}

		#endregion

	}
}
