using System;
using LumiSoft.MailServer;

namespace lsMailServer
{
	/// <summary>
	/// Summary description for MainX.
	/// </summary>
	public class MainX
	{
		#region static Main

		// The main entry point for the process
		static void Main(string[] args)
		{
			if(args.Length > 0){
				if(args[0].ToLower() == "trayapp"){
					System.Windows.Forms.Application.Run(new wfrm_Tray());
				}
			}
			else{
				System.ServiceProcess.ServiceBase[] ServicesToRun;
		
				// More than one user Service may run within the same process. To add
				// another service to this process, change the following line to
				// create a second service object. For example,
				//
				//   ServicesToRun = New System.ServiceProcess.ServiceBase[] {new Service1(), new MySecondUserService()};
				//
				ServicesToRun = new System.ServiceProcess.ServiceBase[] { new MailServer() };

				System.ServiceProcess.ServiceBase.Run(ServicesToRun);
			}
		}

		#endregion
	}
}
