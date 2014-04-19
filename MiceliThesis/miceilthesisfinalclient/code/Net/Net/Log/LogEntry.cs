using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.Log
{
    /// <summary>
    /// Implements log entry.
    /// </summary>
    public class LogEntry
    {
        private LogEntryType m_Type = LogEntryType.Debug;
        private int          m_Size = 0;
        private string       m_Text = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="type">Log entry type.</param>
        /// <param name="size">Specified how much data was readed or written.</param>
        /// <param name="text">Description text.</param>
        public LogEntry(LogEntryType type,int size,string text)
        {
            m_Type = type;
            m_Size = size;
            m_Text = text;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets log entry type.
        /// </summary>
        public LogEntryType EntryType
        {
            get{ return m_Type; }
        }

        /// <summary>
        /// Gets how much data was readed or written, depends on <b>LogEntryType</b>.
        /// </summary>
        public int Size
        {
            get{ return m_Size; }
        }

        /// <summary>
        /// Gets describing text.
        /// </summary>
        public string Text
        {
            get{ return m_Text; }
        }

        #endregion

    }
}
