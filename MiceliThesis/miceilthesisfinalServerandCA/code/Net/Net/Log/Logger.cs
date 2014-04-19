using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.Log
{
    /// <summary>
    /// Represents method that will handle <b>WriteLog</b> event.
    /// </summary>
    /// <param name="entries">Log entries.</param>
    public delegate void WriteLogHandler(LogEntry[] entries);

    /// <summary>
    /// General logging module.
    /// </summary>
    public class Logger : IDisposable
    {   
        private List<LogEntry> m_pEntries = null;
 
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Logger()
        {
            m_pEntries = new List<LogEntry>();
        }

        #region method Dispose

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Flush();
        }

        #endregion


        #region method Flush

        /// <summary>
        /// Forces to write all buffered log entries.
        /// </summary>
        public void Flush()
        {
            lock(m_pEntries){
            }
        }

        #endregion


        #region method AddRead

        /// <summary>
        /// Adds read log entry.
        /// </summary>
        /// <param name="size">Readed data size in bytes.</param>
        /// <param name="text">Log text.</param>
        public void AddRead(int size,string text)
        {
            if(this.WriteLog != null){
                this.WriteLog(new LogEntry[]{new LogEntry(LogEntryType.Read,size,text)});
            }
        }

        #endregion

        #region method AddWrite

        /// <summary>
        /// Add write log entry.
        /// </summary>
        /// <param name="size">Written data size in bytes.</param>
        /// <param name="text">Log text.</param>
        public void AddWrite(int size,string text)
        {
            if(this.WriteLog != null){
                this.WriteLog(new LogEntry[]{new LogEntry(LogEntryType.Write,size,text)});
            }
        }

        #endregion

        #region method AddDebug

        /// <summary>
        /// Adds debug entry.
        /// </summary>
        /// <param name="text"></param>
        public void AddDebug(string text)
        {
            if(this.WriteLog != null){
                this.WriteLog(new LogEntry[]{new LogEntry(LogEntryType.Debug,0,text)});
            }
        }

        #endregion


        #region Properties Implementation

        #endregion
        
        #region Events Implementation

        /// <summary>
        /// Is raised when log buffer is full and log entries must be processed,
        /// </summary>
        public event WriteLogHandler WriteLog = null;

        #region method OnWriteLog

        /// <summary>
        /// Raises WriteLog event.
        /// </summary>
        private void OnWriteLog()
        {
            if(this.WriteLog != null){
                this.WriteLog(m_pEntries.ToArray());
            }
        }

        #endregion

        #endregion

    }
}
