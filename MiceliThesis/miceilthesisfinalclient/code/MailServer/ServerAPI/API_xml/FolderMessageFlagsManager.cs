using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using LumiSoft.Net;
using LumiSoft.Net.IMAP;

namespace LumiSoft.MailServer
{
    /// <summary>
    /// This class manages folder message flags for folder users.
    /// </summary>
    internal class FolderMessageFlagsManager
    {
        #region static method GetFlags

        /// <summary>
        /// Gets specified user message flags.
        /// </summary>
        /// <param name="userID">User ID whos flags to get.</param>
        /// <param name="folder">Folder.</param>
        public static Dictionary<int,int> GetFlags(string userID,string folder)
        {
            Dictionary<int,int> retVal = new Dictionary<int,int>();

            using(FileStream fs = GetFlagsFile(userID,folder)){
                TextReader r = new StreamReader(fs);
                string line = r.ReadLine();
                while(line != null){
                    // Skip comment lines and deleted rows
                    if(!(line.StartsWith("#") || line.StartsWith("\0"))){
                        string[] userID_uid_flags = line.Split(' ');
                        if(userID_uid_flags[0] == userID){
                            if(retVal.ContainsKey(Convert.ToInt32(userID_uid_flags[1]))){
                                // This should never happen. TODO: ?
                            }
                            else{
                                retVal.Add(Convert.ToInt32(userID_uid_flags[1]),Convert.ToInt32(userID_uid_flags[2]));
                            }
                        }
                    }

                    line = r.ReadLine();
                }
            }

            return retVal;
        }

        #endregion

        #region static method SetFlags

        /// <summary>
        /// Sets specified message flags for specified user.
        /// </summary>
        /// <param name="userID">User ID.</param>
        /// <param name="folder">Folder.</param>
        /// <param name="uid">Message UID.</param>
        /// <param name="flags">Message flags.</param>
        public static void SetFlags(string userID,string folder,int uid,IMAP_MessageFlags flags)
        {
            using(FileStream fs = GetFlagsFile(userID,folder)){
                StreamLineReader r    = new StreamLineReader(fs);
                long             pos  = fs.Position;
                string           line = r.ReadLineString();
                while(line != null){
                    // Skip comment lines
                    if(!line.StartsWith("#")){
                        string[] userID_uid_flags = line.Split(' ');
                        // Update user message flags
                        if(userID_uid_flags[0] == userID && Convert.ToInt32(userID_uid_flags[1]) == uid){
                            fs.Position = pos;
                            byte[] record1 = System.Text.Encoding.ASCII.GetBytes(userID + " " + uid.ToString("d10") + " " + ((int)flags).ToString("d4") + "\r\n");
                            fs.Write(record1,0,record1.Length);
                            return;
                        }
                    }

                    pos  = fs.Position;
                    line = r.ReadLineString();
                }

                // If we reach here, then specified user has no flags for specified message, add new record.
                byte[] record = System.Text.Encoding.ASCII.GetBytes(userID + " " + uid.ToString("d10") + " " + ((int)flags).ToString("d4") + "\r\n");
                fs.Write(record,0,record.Length);
            }
        }

        #endregion

        #region static method DeleteFlags

        /// <summary>
        /// Deletes specified message all users flags.
        /// </summary>
        /// <param name="folder">Folder.</param>
        /// <param name="uid">Message UID.</param>
        public static void DeleteFlags(string folder,int uid)
        {
            using(FileStream fs = GetFlagsFile(null,folder)){
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
                            string[] userID_uid_flags = line.Split(' ');
                            // Delete row
                            if(Convert.ToInt32(userID_uid_flags[1]) == uid){
                                byte[] linebytes = new byte[fs.Position - pos - 2];
                                fs.Position = pos;
                                fs.Write(linebytes,0,linebytes.Length);
                                fs.Position += 2; // CRLF
                                delRowCount++;
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

        /// <summary>
        /// Deletes specified user message flags on specified message.
        /// </summary>
        /// <param name="userID">User ID.</param>
        /// <param name="folder">Folder.</param>
        /// <param name="uid">Message UID.</param>
        public static void DeleteFlags(string userID,string folder,int uid)
        {            
            using(FileStream fs = GetFlagsFile(userID,folder)){
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
                            string[] userID_uid_flags = line.Split(' ');
                            // Delete row
                            if(userID_uid_flags[1] == userID && Convert.ToInt32(userID_uid_flags[1]) == uid){
                                byte[] linebytes = new byte[fs.Position - pos - 2];
                                fs.Position = pos;
                                fs.Write(linebytes,0,linebytes.Length);
                                fs.Position += 2; // CRLF
                                delRowCount++;
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


        #region static method GetFlagsFile

        /// <summary>
        /// Gets flags file.
        /// </summary>
        /// <param name="userID">User how owns flags file folder.</param>
        /// <param name="folder">Folder.</param>
        internal static FileStream GetFlagsFile(string userID,string folder)
        {
            // Try 20 seconds to open flags file, it's locked.
            DateTime start = DateTime.Now;
            string   error = "";

            while(start.AddSeconds(20) > DateTime.Now){
                try{
                    FileStream fs = File.Open(folder + "_flags.txt",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);

                    // Flags file just created.
                    if(fs.Length == 0){                                     
                        byte[] fileCommnet = System.Text.Encoding.ASCII.GetBytes("#\r\n# This file holds message flags info for folder user(s), don't delete this file !\r\n#\r\n");
                        fs.Write(fileCommnet,0,fileCommnet.Length);
                        
                        if(userID != null){
                            // CHANGE ME: Get rid of message internal header or at least don't store flags here.
                            // REMOVE ME: because change me:.                        
                            string[] files = Directory.GetFiles(Path.GetDirectoryName(fs.Name),"*.eml");
                            foreach(string file in files){
                                long uid   = Convert.ToInt64(Path.GetFileNameWithoutExtension(file).Split('_')[1]);
                                int  flags = 0;
                                using(FileStream f = File.Open(file,FileMode.Open)){
                                    flags = (int)(new _InternalHeader(f).MessageFlags);
                                }

                                if(flags != 0){
                                    byte[] record = System.Text.Encoding.ASCII.GetBytes(userID + " " + uid.ToString("d10") + " " + flags.ToString("d4") + "\r\n");
                                    fs.Write(record,0,record.Length);
                                }
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
            throw new Exception("Opening flags file timed-out, failed with error: " + error);
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
