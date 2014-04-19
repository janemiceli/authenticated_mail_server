using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using LumiSoft.Net;

namespace LumiSoft.MailServer
{
    #region class FolderMessageInfo

    /// <summary>
    /// This class hold message info.
    /// </summary>
    internal class FolderMessageInfo
    {
        private int      m_UID  = 1;
        private int      m_Size = 0;
        private DateTime m_InternalDate;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="uid">Message UID.</param>
        /// <param name="size">Message size in bytes.</param>
        /// <param name="internalDate">Internal date.</param>
        public FolderMessageInfo(int uid,int size,DateTime internalDate)
        {
            m_UID          = uid;
            m_Size         = size;
            m_InternalDate = internalDate;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets message UID.
        /// </summary>
        public int UID
        {
            get{ return m_UID; }
        }       

        /// <summary>
        /// Gets message size in bytes.
        /// </summary>
        public int Size
        {
            get{ return m_Size; }
        }

        /// <summary>
        /// Gets message internal date.
        /// </summary>
        public DateTime InternalDate
        {
            get{ return m_InternalDate; }
        }

        #endregion
    }

    #endregion

    /// <summary>
    /// This class manages folder messages info.
    /// </summary>
    internal class FolderMessagesInfoManager
    {
        #region static method GetMessagesInfo

        /// <summary>
        /// Gets specified folder messages info.
        /// </summary>
        /// <param name="folder">Full path to folder, which messages info to get.</param>
        /// <returns></returns>
        public static List<FolderMessageInfo> GetMessagesInfo(string folder)
        {
            List<FolderMessageInfo> retVal = new List<FolderMessageInfo>();

            using(FileStream fs = GetFile(folder,-1)){
                TextReader r = new StreamReader(fs);
                string line = r.ReadLine();
                while(line != null){
                    // Skip comment lines and deleted rows
                    if(!(line.StartsWith("#") || line.StartsWith("\0"))){
                        string[] uid_size_date = line.Split(' ');                        
                        retVal.Add(new FolderMessageInfo(
                            Convert.ToInt32(uid_size_date[0]),
                            Convert.ToInt32(uid_size_date[1]),
                            DateTime.ParseExact(uid_size_date[2],"yyyyMMddHHmmss",System.Globalization.DateTimeFormatInfo.CurrentInfo))                            
                        );
                    }

                    line = r.ReadLine();
                }
            }

            return retVal;
        }

        #endregion

        #region static method Append

        /// <summary>
        /// Adds new message info to messages info database.
        /// </summary>
        /// <param name="folder">Full path to user folder.</param>
        /// <param name="uid">Message UID.</param>
        /// <param name="size">Message size in bytes.</param>
        /// <param name="internalDate">Message internaldate.</param>
        public static void Append(string folder,int uid,int size,DateTime internalDate)
        {
            using(FileStream fs = GetFile(folder,uid)){
                fs.Position = fs.Length;

                byte[] record = System.Text.Encoding.ASCII.GetBytes(uid.ToString("d10") + " " + size.ToString("d10") + " " + internalDate.ToString("yyyyMMddHHmmss") + "\r\n");
                fs.Write(record,0,record.Length);
            }
        }

        #endregion

        #region static method Delete

        /// <summary>
        /// Deletes specified message info.
        /// </summary>
        /// <param name="folder">Full path to user folder.</param>
        /// <param name="uid">Message UID.</param>
        public static void Delete(string folder,int uid)
        {
            using(FileStream fs = GetFile(folder,-1)){
                int              delRowCount = 0;
                StreamLineReader r           = new StreamLineReader(fs);
                long             pos         = fs.Position;
                string           line        = r.ReadLineString();
                while(line != null){
                    // Skip comment lines
                    if(!line.StartsWith("#")){
                        // Skip deleted row
                        if(line.StartsWith("\0")){
                            delRowCount++;
                        }
                        else{
                            string[] uid_size_date = line.Split(' ');
                            // Delete row
                            if(Convert.ToInt32(uid_size_date[0]) == uid){
                                byte[] linebytes = new byte[fs.Position - pos - 2];
                                fs.Position = pos;
                                fs.Write(linebytes,0,linebytes.Length);
                                fs.Position += 2; // CRLF
                                delRowCount++;
                                break;
                            }
                        }
                    }

                    pos  = fs.Position;
                    line = r.ReadLineString();
                }

                // There are many deleted rows, vacuum(remove deleted rows) flags database.
                if(delRowCount > 500){
                    Vacuum(fs);
                }
            }
        }

        #endregion


        #region static method GetFile

        /// <summary>
        /// Gets messages info file.
        /// </summary>
        /// <param name="folder">Folder.</param>
        /// <param name="skipUID">If cache doesn't exist and is created, skip specified message.</param>
        internal static FileStream GetFile(string folder,int skipUID)
        {
            // Try 20 seconds to open flags file, it's locked.
            DateTime start = DateTime.Now;
            string   error = "";
            
            while(start.AddSeconds(20) > DateTime.Now){
                try{
                    FileStream fs = File.Open(folder + "_messages_info.txt",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);

                    // Messages info file just created.
                    if(fs.Length == 0){                                     
                        byte[] fileCommnet = System.Text.Encoding.ASCII.GetBytes("#\r\n# This file holds messages info, don't delete this file !\r\n#\r\n");
                        fs.Write(fileCommnet,0,fileCommnet.Length);
                        
                        // Build messages info file, if there are any existing files.
                        string[] files = Directory.GetFiles(Path.GetDirectoryName(fs.Name),"*.eml");
                        foreach(string file in files){
                            string fileX = file;
                            //----------------------------------------------------------------------------------
                            // REMOVE ME: <= 0.86 version messges
                            if(file.Split('_').Length == 3){
                                long   u = Convert.ToInt64(Path.GetFileNameWithoutExtension(file).Split('_')[1]);
                                string d = Path.GetFileNameWithoutExtension(file).Split('_')[0];
                                fileX = Path.GetDirectoryName(file) + "/" + d + "_" + u.ToString("d10") + ".eml";
                                File.Move(file,fileX);
                            }
                            //---------------------------------------------------------------------------------
                            
                            // UID SIZE INTERNALDATE
                            long uid  = Convert.ToInt64(Path.GetFileNameWithoutExtension(fileX).Split('_')[1]);
                            long size = 0;
                            using(FileStream f = File.OpenRead(fileX)){
                                new _InternalHeader(f);
                                size = f.Length - f.Position;
                            }
                            string date = Path.GetFileNameWithoutExtension(fileX).Split('_')[0];

                            if(uid != skipUID){
                                byte[] record = System.Text.Encoding.ASCII.GetBytes(uid.ToString("d10") + " " + size.ToString("d10") + " " + date + "\r\n");
                                fs.Write(record,0,record.Length);
                            }
                        }
                    }
                    fs.Position = 0;

                    return fs;
                }
                catch(Exception x){
                    error = x.Message;

                    // Wait here, otherwise takes 100% CPU
                    System.Threading.Thread.Sleep(5);
                }
            }

            // If we reach here, flags file open failed.
            throw new Exception("Opening messages info file timed-out, failed with error: " + error);
        }

        #endregion

        #region static method Vacuum

        /// <summary>
        /// Vacuums flags database, deletes deleted rows empty used space from file.
        /// </summary>
        /// <param name="fs">Database file stream.</param>
        private static void Vacuum(FileStream fs)
        {
            MemoryStream buffer = new MemoryStream();
            fs.Position = 0;

            StreamLineReader r    = new StreamLineReader(fs);
            string           line = r.ReadLineString();
            while(line != null){
                // Skip deleted rows
                if(!line.StartsWith("\0")){
                    byte[] lineBytes = System.Text.Encoding.ASCII.GetBytes(line + "\r\n");
                    buffer.Write(lineBytes,0,lineBytes.Length);
                }                

                line = r.ReadLineString();
            }

            fs.SetLength(buffer.Length);
            fs.Position = 0;
            buffer.WriteTo(fs);
        }

        #endregion
    }
}
