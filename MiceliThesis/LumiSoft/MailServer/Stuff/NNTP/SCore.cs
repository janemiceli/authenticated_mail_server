using System;
using System.IO;

namespace LumiSoft.Net.NNTP.Server
{
	/// <summary>
	/// Server utility functions.
	/// </summary>
	public class SCore
	{
		//	public SCore()
		//	{			
		//	}

		#region function WriteLog

		#region function WriteLog(fileName,text)

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="text"></param>
		public static void WriteLog(string fileName,string text)
		{
			try
			{
				// Try to parse directory path
				if(fileName.IndexOf("\\") > -1)
				{
					string dirPath = fileName.Substring(0,fileName.LastIndexOf("\\"));
					// If there isn't such directory, create it.
					if(!Directory.Exists(dirPath))
					{
						Directory.CreateDirectory(dirPath);
					}
				}

				using(FileStream fs = new FileStream(fileName,FileMode.OpenOrCreate, FileAccess.Write))
					  {
					StreamWriter w = new StreamWriter(fs);      // create a Char writer 
					w.BaseStream.Seek(0, SeekOrigin.End);      // set the file pointer to the end
					w.Write(text + "\r\n");
					w.Flush();  // update underlying file
				}
			}
			catch
			{
			}
		}

		#endregion

		#endregion

	}
}
