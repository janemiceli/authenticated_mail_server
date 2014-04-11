using System;
using System.IO;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Specifies filtering result.
	/// </summary>
	public enum FilterResult
	{
		/// <summary>
		/// Store messge and reply Ok to client.
		/// </summary>
		Store = 1,

		/// <summary>
		/// Don't store messge, but reply Ok to client.
		/// </summary>
		DontStore = 2,

		/// <summary>
		/// [Reserved, NOT USED at moment]Send filtering error to client.
		/// </summary>
		Error = 3,
	}

	/// <summary>
	/// SMTP server mail message filter.
	/// </summary>
	public interface ISmtpMessageFilter
	{
		/// <summary>
		/// Filters message.
		/// </summary>
		/// <param name="messageStream">Message stream which to filter.</param>
		/// <param name="filteredStream">Filtered stream.</param>
		/// <param name="sender">Senders email address.</param>
		/// <param name="recipients">Recipients email addresses.</param>
		/// <param name="api">Access to server API.</param>
		FilterResult Filter(MemoryStream messageStream,out MemoryStream filteredStream,string sender,string[] recipients,ServerAPI api);
	}
}
