using System;
using System.Net;

namespace LumiSoft.MailServer.API.UserAPI
{
	/// <summary>
	/// Convert methods.
	/// </summary>
	internal class ConvertEx
	{
		#region method ToString

		/// <summary>
		/// Converts object to string. If value == null, returns "".
		/// </summary>
		/// <param name="value">Value to  be converted.</param>
		/// <returns></returns>
		public static string ToString(object value)
		{
			if(value == null){
				return "";
			}
			else{
				return value.ToString();
			}
		}

		#endregion

		#region method ToBoolean

		/// <summary>
		/// Convert object to bool. If value == null or object can't be converted to bool, returns false.
		/// </summary>
		/// <param name="value">Value to  be converted.</param>
		/// <returns></returns>
		public static bool ToBoolean(object value)
		{
			if(value == null){
				return false;
			}
			else{
				try{
					return Convert.ToBoolean(value);
				}
				catch{
					return false;
				}
			}
		}

        /// <summary>
		/// Convert object to bool. If value == null or object can't be converted to bool, returns false.
		/// </summary>
		/// <param name="value">Value to  be converted.</param>
        /// <param name="defaultValue">If parsing fails, this default value is used then.</param>
		/// <returns></returns>
		public static bool ToBoolean(object value,bool defaultValue)
		{
            try{
                if(value == null){
				    return false;
			    }
			    else{
				    try{
					    return Convert.ToBoolean(value);
				    }
				    catch{
					    return false;
				    }
			    }
            }
            catch{
                return defaultValue;
            }
        }

		#endregion

		#region method ToInt32

		/// <summary>
		/// Convert object to int. If value == null or object can't be converted to int, returns 0.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int ToInt32(object value)
		{
			if(value == null){
				return 0;
			}
			else{
				try{
					return Convert.ToInt32(value);
				}
				catch{
					return 0;
				}
			}
		}

		#endregion

        #region method ToIPAddress

        /// <summary>
        /// Converts specified value to IP end point. Returns default value specified if parsing fails.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="defultValue">Default value if parsing fails.</param>
        public static IPEndPoint ToIPEndPoint(string value,IPEndPoint defultValue)
        {
            if(value == null){
				return defultValue;
			}
			else{
				try{
                    // Port is missing, add 0.
                    if(value.IndexOf(':') == -1){
                        value += ":";
                    }
					return new IPEndPoint(IPAddress.Parse(value.Split(':')[0]),ConvertEx.ToInt32(value.Split(':')[1]));
				}
				catch{
					return defultValue;
				}
			}
        }

        #endregion
    }
}
