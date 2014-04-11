using System;
using System.Windows.Forms;
using MailServerManager.Forms;
using System.Threading;
using LumiSoft.MailServer;

namespace MailServerManager
{
	//The Error Handler class
    internal class CustomExceptionHandler 
	{
		public CustomExceptionHandler()
		{			
		}

        //Handle the exception  event
        public void OnThreadException(object sender, ThreadExceptionEventArgs t) 
		{
			wfrm_Error frm = new wfrm_Error(t.Exception,new System.Diagnostics.StackTrace());
			frm.ShowDialog(null);		   
        }       
    }

	/// <summary>
	/// Summary description for MainX.
	/// </summary>
	public class MainX
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			CustomExceptionHandler eh    = new CustomExceptionHandler();
			Application.ThreadException += new ThreadExceptionEventHandler(eh.OnThreadException);
			Application.Run(new wfrm_Main());
		}
	}
}
