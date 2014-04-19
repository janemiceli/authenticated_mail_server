using System;
using System.Windows.Forms;

namespace lsSpamFilter
{
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
			Application.Run(new wfrm_Configure());
		}
	}
}
