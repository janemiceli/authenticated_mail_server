using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using LumiSoft.MailServer;

namespace lsMailServer
{
	/// <summary>
	/// Summary description for wfrm_Tray.
	/// </summary>
	public class wfrm_Tray : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuItem_Start;
		private System.Windows.Forms.MenuItem menuItem_Stop;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem_Exit;

		private MailServer m_MailServer = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public wfrm_Tray()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			m_MailServer = new MailServer();
			m_MailServer.Start();
			menuItem_Start.Enabled = false;

			this.WindowState = FormWindowState.Minimized;
		}

		#region method Dsipose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			m_MailServer.Dispose();

			if(disposing){
				if(components != null){
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(wfrm_Tray));
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuItem_Start = new System.Windows.Forms.MenuItem();
			this.menuItem_Stop = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem_Exit = new System.Windows.Forms.MenuItem();
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.ContextMenu = this.contextMenu1;
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Text = "LS Mail Server";
			this.notifyIcon1.Visible = true;
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItem_Start,
																						 this.menuItem_Stop,
																						 this.menuItem1,
																						 this.menuItem_Exit});
			// 
			// menuItem_Start
			// 
			this.menuItem_Start.Index = 0;
			this.menuItem_Start.Text = "Start";
			this.menuItem_Start.Click += new System.EventHandler(this.menuItem_Start_Click);
			// 
			// menuItem_Stop
			// 
			this.menuItem_Stop.Index = 1;
			this.menuItem_Stop.Text = "Stop";
			this.menuItem_Stop.Click += new System.EventHandler(this.menuItem_Stop_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 2;
			this.menuItem1.Text = "-";
			// 
			// menuItem_Exit
			// 
			this.menuItem_Exit.Index = 3;
			this.menuItem_Exit.Text = "Exit";
			this.menuItem_Exit.Click += new System.EventHandler(this.menuItem_Exit_Click);
			// 
			// wfrm_Tray
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(208, 48);
			this.ControlBox = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.Name = "wfrm_Tray";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "LS MailServer";

		}
		#endregion


		#region Events handling

		private void menuItem_Start_Click(object sender, System.EventArgs e)
		{
			m_MailServer.Start();
			menuItem_Start.Enabled = false;
			menuItem_Stop.Enabled  = true;
		}

		private void menuItem_Stop_Click(object sender, System.EventArgs e)
		{
			m_MailServer.Stop();
			menuItem_Start.Enabled = true;
			menuItem_Stop.Enabled  = false;
		}

		private void menuItem_Exit_Click(object sender, System.EventArgs e)
		{
			m_MailServer.Stop();
			this.Dispose();
		}

		#endregion
		
	}
}
