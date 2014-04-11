using System;
using System.IO;

namespace LumiSoft.Net.Mime
{
	/// <summary>
	/// Attachment.
	/// </summary>
	public class Attachment
	{
		private string m_FileName       = "";
		private byte[] m_FileData       = null;
		private string m_AttachmentType = "";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		public Attachment(string fileName)
		{
			m_AttachmentType = "file";
			m_FileName = Path.GetFileName(fileName);

			using(FileStream fs = File.OpenRead(fileName)){
				m_FileData = new byte[fs.Length];
				fs.Read(m_FileData,0,(int)fs.Length);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="strm"></param>
		public Attachment(string fileName,Stream strm)
		{
			m_AttachmentType = "data";
			m_FileName = fileName;

			int strmPos = (int)strm.Position;
			m_FileData = new byte[strm.Length - strmPos];
			strm.Read(m_FileData,0,(int)strm.Length - strmPos);
			strm.Position = strmPos;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="fileData"></param>
		public Attachment(string fileName,byte[] fileData)
		{
			m_AttachmentType = "data";
			m_FileName = fileName;
			m_FileData = fileData;
		}
		

		#region Properties Implementation

		/// <summary>
		/// Gets file name.
		/// </summary>
		public string FileName
		{
			get{ return m_FileName;	}
		}

		/// <summary>
		/// Gets file data.
		/// </summary>
		public byte[] FileData
		{
			get{ return m_FileData;	}
		}

		/// <summary>
		/// 
		/// </summary>
		public string AttachmentType
		{
			get{ return m_AttachmentType; }
		}

		#endregion

	}
}
