using System;

using LumiSoft.MailServer;
using LumiSoft.MailServer.UI;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Application main class.
	/// </summary>
	public class MainX
	{
		#region static method Main

		/// <summary>
		/// Application main entry point.
		/// </summary>
		/// <param name="args">Command line argumnets.</param>
		public static void Main(string[] args)
		{
            // Add app domain unhandled exception handler.
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                        
			if(args.Length > 0){
				if(args[0].ToLower() == "/?" || args[0].ToLower() == "/h"){                    
					string text = "";
					text += "Possible keys:\r\n";
					text += "\r\n";
					text += "\t -trayapp, runs server as Windows tray application.\r\n";
					text += "\t -winform, runs server in Windows Forms window.\r\n";
					                    					
                    System.Windows.Forms.MessageBox.Show(null,text,"Info:",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Information);
				} 
				else if(args[0].ToLower() == "-trayapp"){                    
                    System.Windows.Forms.Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                    System.Windows.Forms.Application.EnableVisualStyles();
					System.Windows.Forms.Application.Run(new wfrm_Tray());
				}
                else if(args[0].ToLower() == "-winform"){                    
                    System.Windows.Forms.Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                    System.Windows.Forms.Application.EnableVisualStyles();
					System.Windows.Forms.Application.Run(new wfrm_WinForm());
				}
                else{
                    System.Windows.Forms.MessageBox.Show("Invalid command line argument was specified !","Error:",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Error);
                }
			}
			else{
                System.Windows.Forms.Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.Run(new wfrm_Install());
			}
        }
                
        #endregion

        #region method CurrentDomain_UnhandledException

        /// <summary>
        /// This is called when unhandled exception happened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandledException(object sender,UnhandledExceptionEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Error: " + ((Exception)e.ExceptionObject).ToString(),"Error:",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Error);
        }

        #endregion

        #region method Application_ThreadException

        /// <summary>
        /// This method is called when unhandled excpetion happened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender,System.Threading.ThreadExceptionEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Error: " + e.Exception.ToString(),"Error:",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Error);
        }

        #endregion

    }
}
