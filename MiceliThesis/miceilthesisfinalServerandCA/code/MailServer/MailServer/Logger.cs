using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using LumiSoft.Net;

namespace LumiSoft.MailServer
{
    /// <summary>
    /// Provides logging related methods.
    /// </summary>
    internal class Logger
    {
        #region method WriteLog

        /// <summary>
        /// Writes socket log to the specified log file.
        /// </summary>
        /// <param name="file">Log file.</param>
        /// <param name="logger">Socket logger.</param>
        public static void WriteLog(string file,SocketLogger logger)
        {
            try{
                using(TextDb db = new TextDb('\t')){
                    db.OpenOrCreate(file);

                    db.AppendComment("Fields: SessionID SessionStartTime RemoteEndPoint AuthenticatedUser LogType LogText");
                    foreach(SocketLogEntry logEntry in logger.LogEntries){
                        string logText = logEntry.Text.Replace("\r","");
                        if(logText.EndsWith("\n")){
                            logText = logText.Substring(0,logText.Length - 1);
                        }

                        string logType = "";
                        if(logEntry.Type == SocketLogEntryType.FreeText){
                            logType = "xxx";
                        }
                        else if(logEntry.Type == SocketLogEntryType.ReadFromRemoteEP){
                            logType = "<<<";
                        }
                        else if(logEntry.Type == SocketLogEntryType.SendToRemoteEP){
                            logType = ">>>";
                        }

                        foreach(string logLine in logText.Split('\n')){
                            db.Append(new string[]{
                                logger.SessionID,
                                DateTime.Now.ToString(),
                                ConvertEx.ToString(logger.RemoteEndPoint),
                                logger.UserName,
                                logType,
                                logLine
                            });
                        }
                    }
                }
            }
            catch(Exception x){
                Error.DumpError(x,new System.Diagnostics.StackTrace());
            }
        }

        #endregion

        #region method WriteLog

        /// <summary>
		/// Writes specified text to log file.
		/// </summary>
		/// <param name="fileName">Log file name.</param>
		/// <param name="text">Log text.</param>
		public static void WriteLog(string fileName,string text)
		{
			try{
                fileName = SCore.PathFix(fileName);
                                
				// If there isn't such directory, create it.
                if(!Directory.Exists(Path.GetDirectoryName(fileName))){
				    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
				}
				
				using(FileStream fs = new FileStream(fileName,FileMode.OpenOrCreate,FileAccess.Write)){
					StreamWriter w = new StreamWriter(fs);     // create a Char writer 
					w.BaseStream.Seek(0, SeekOrigin.End);      // set the file pointer to the end
					w.Write(text + "\r\n");
					w.Flush();  // update underlying file
				}
			}
			catch{
			}
        }

        #endregion
    }
}
