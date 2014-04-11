using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MailServerManager.Forms
{
	/// <summary>
	/// Summary description for wfrm_Servers.
	/// </summary>
	public class wfrm_Servers : System.Windows.Forms.Form
	{
		private System.Windows.Forms.LinkLabel m_pAddRemoteServer;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.LinkLabel m_pConnectToServer;
		private System.Windows.Forms.LinkLabel m_pAddLocalServer;
		private System.Windows.Forms.GroupBox groupBox1;

		private wfrm_Main m_p_wfrm_Main = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="form"></param>
		public wfrm_Servers(wfrm_Main form)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			m_p_wfrm_Main = form;
		}

		#region function Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
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
			this.m_pAddRemoteServer = new System.Windows.Forms.LinkLabel();
			this.m_pConnectToServer = new System.Windows.Forms.LinkLabel();
			this.m_pAddLocalServer = new System.Windows.Forms.LinkLabel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_pAddRemoteServer
			// 
			this.m_pAddRemoteServer.Enabled = false;
			this.m_pAddRemoteServer.Location = new System.Drawing.Point(24, 48);
			this.m_pAddRemoteServer.Name = "m_pAddRemoteServer";
			this.m_pAddRemoteServer.Size = new System.Drawing.Size(100, 16);
			this.m_pAddRemoteServer.TabIndex = 3;
			this.m_pAddRemoteServer.TabStop = true;
			this.m_pAddRemoteServer.Text = "Add remote server";
			this.m_pAddRemoteServer.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pAddRemoteServer_LinkClicked);
			// 
			// m_pConnectToServer
			// 
			this.m_pConnectToServer.Location = new System.Drawing.Point(24, 96);
			this.m_pConnectToServer.Name = "m_pConnectToServer";
			this.m_pConnectToServer.Size = new System.Drawing.Size(144, 16);
			this.m_pConnectToServer.TabIndex = 4;
			this.m_pConnectToServer.TabStop = true;
			this.m_pConnectToServer.Text = "Connect to remote server";
			this.m_pConnectToServer.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pConnectToServer_LinkClicked);
			// 
			// m_pAddLocalServer
			// 
			this.m_pAddLocalServer.Enabled = false;
			this.m_pAddLocalServer.Location = new System.Drawing.Point(24, 24);
			this.m_pAddLocalServer.Name = "m_pAddLocalServer";
			this.m_pAddLocalServer.Size = new System.Drawing.Size(100, 16);
			this.m_pAddLocalServer.TabIndex = 5;
			this.m_pAddLocalServer.TabStop = true;
			this.m_pAddLocalServer.Text = "Add local server";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.m_pAddLocalServer,
																					this.m_pAddRemoteServer,
																					this.m_pConnectToServer});
			this.groupBox1.Location = new System.Drawing.Point(8, 24);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(456, 128);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Servers";
			// 
			// wfrm_Servers
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 381);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.groupBox1});
			this.Name = "wfrm_Servers";
			this.Text = "wfrm_Servers";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		#region function m_pAddRemoteServer_LinkClicked

		private void m_pAddRemoteServer_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			wfrm_AddServer frm = new wfrm_AddServer(true);
			if(frm.ShowDialog() == DialogResult.OK){
				m_p_wfrm_Main.AddMailServer(frm.wp_Name,frm.wp_WebServicesUrl,frm.wp_WebServicesUser,frm.wp_WebServicesPwd);

				// ToDo: save to xml
			}
		}

		#endregion

		#region function m_pConnectToServer_LinkClicked

		private void m_pConnectToServer_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			wfrm_AddServer frm = new wfrm_AddServer(true);
			if(frm.ShowDialog() == DialogResult.OK){
				m_p_wfrm_Main.AddMailServer(frm.wp_Name,frm.wp_WebServicesUrl,frm.wp_WebServicesUser,frm.wp_WebServicesPwd);
			}
		}

		#endregion

		#endregion
		
	}
}
