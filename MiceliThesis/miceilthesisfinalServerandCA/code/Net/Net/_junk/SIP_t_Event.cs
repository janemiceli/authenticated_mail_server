using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SIP.Header
{
    /// <summary>
    /// Implements SIP "event-type *( SEMI event-param )" value. Defined in RFC 3265.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///                   event-type *( SEMI event-param )
    ///     event-param = generic-param / ( "id" EQUAL token )
    /// </code>
    /// </remarks>
    public class SIP_t_Event : SIP_t_ValueWithParams
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_Event()
        {
        }
    }
}
