using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MailServerManager.Forms
{
	/// <summary>
	/// Summary description for wfrm_AddRemoteServer.
	/// </summary>
	public class wfrm_AddServer : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WEditBox m_pWebServicesUrl;
		private LumiSoft.UI.Controls.WEditBox m_pWebServicesPwd;
		private LumiSoft.UI.Controls.WEditBox m_pWebServicesUser;
		private LumiSoft.UI.Controls.WEditBox m_pName;
		private LumiSoft.UI.Controls.WButton m_pOk;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		private LumiSoft.UI.Controls.WLabel wLabel2;
		private LumiSoft.UI.Controls.WLabel wLabel3;
		private LumiSoft.UI.Controls.WLabel wLabel4;
		private LumiSoft.UI.Controls.WButton m_pCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="add_connnect">Add new server or connects to new server(don't save).</param>
		public wfrm_AddServer(bool add_connnect)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.m_pWebServicesUrl = new LumiSoft.UI.Controls.WEditBox();
			this.m_pWebServicesPwd = new LumiSoft.UI.Controls.WEditBox();
			this.m_pWebServicesUser = new LumiSoft.UI.Controls.WEditBox();
			this.m_pName = new LumiSoft.UI.Controls.WEditBox();
			this.m_pOk = new LumiSoft.UI.Controls.WButton();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel2 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel3 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel4 = new LumiSoft.UI.Controls.WLabel();
			this.m_pCancel = new LumiSoft.UI.Controls.WButton();
			this.SuspendLayout();
			// 
			// m_pWebServicesUrl
			// 
			this.m_pWebServicesUrl.Location = new System.Drawing.Point(8, 64);
			this.m_pWebServicesUrl.Name = "m_pWebServicesUrl";
			this.m_pWebServicesUrl.Size = new System.Drawing.Size(240, 20);
			this.m_pWebServicesUrl.TabIndex = 2;
			this.m_pWebServicesUrl.Text = "http://server/webService/";
			this.m_pWebServicesUrl.UseStaticViewStyle = true;
			// 
			// m_pWebServicesUrl.ViewStyle
			// 
			// 
			// m_pWebServicesPwd
			// 
			this.m_pWebServicesPwd.Location = new System.Drawing.Point(8, 144);
			this.m_pWebServicesPwd.Name = "m_pWebServicesPwd";
			this.m_pWebServicesPwd.Size = new System.Drawing.Size(104, 20);
			this.m_pWebServicesPwd.TabIndex = 4;
			this.m_pWebServicesPwd.UseStaticViewStyle = true;
			// 
			// m_pWebServicesPwd.ViewStyle
			// 
			// 
			// m_pWebServicesUser
			// 
			this.m_pWebServicesUser.Location = new System.Drawing.Point(8, 104);
			this.m_pWebServicesUser.Name = "m_pWebServicesUser";
			this.m_pWebServicesUser.Size = new System.Drawing.Size(104, 20);
			this.m_pWebServicesUser.TabIndex = 3;
			this.m_pWebServicesUser.UseStaticViewStyle = true;
			// 
			// m_pWebServicesUser.ViewStyle
			// 
			// 
			// m_pName
			// 
			this.m_pName.Location = new System.Drawing.Point(8, 24);
			this.m_pName.Name = "m_pName";
			this.m_pName.Size = new System.Drawing.Size(240, 20);
			this.m_pName.TabIndex = 1;
			this.m_pName.UseStaticViewStyle = true;
			// 
			// m_pName.ViewStyle
			// 
			// 
			// m_pOk
			// 
			this.m_pOk.Location = new System.Drawing.Point(168, 104);
			this.m_pOk.Name = "m_pOk";
			this.m_pOk.Size = new System.Drawing.Size(72, 24);
			this.m_pOk.TabIndex = 5;
			this.m_pOk.Text = "OK";
			this.m_pOk.UseStaticViewStyle = true;
			// 
			// m_pOk.ViewStyle
			// 
			this.m_pOk.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pOk_ButtonPressed);
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(8, 8);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(104, 16);
			this.wLabel1.TabIndex = 6;
			this.wLabel1.Text = "Name";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// wLabel2
			// 
			this.wLabel2.Location = new System.Drawing.Point(8, 48);
			this.wLabel2.Name = "wLabel2";
			this.wLabel2.Size = new System.Drawing.Size(128, 16);
			this.wLabel2.TabIndex = 7;
			this.wLabel2.Text = "WebServices URL";
			this.wLabel2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel2.ViewStyle
			// 
			// 
			// wLabel3
			// 
			this.wLabel3.Location = new System.Drawing.Point(8, 88);
			this.wLabel3.Name = "wLabel3";
			this.wLabel3.Size = new System.Drawing.Size(104, 16);
			this.wLabel3.TabIndex = 8;
			this.wLabel3.Text = "UserName";
			this.wLabel3.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel3.ViewStyle
			// 
			// 
			// wLabel4
			// 
			this.wLabel4.Location = new System.Drawing.Point(8, 128);
			this.wLabel4.Name = "wLabel4";
			this.wLabel4.Size = new System.Drawing.Size(104, 16);
			this.wLabel4.TabIndex = 9;
			this.wLabel4.Text = "Password";
			this.wLabel4.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel4.ViewStyle
			// 
			// 
			// m_pCancel
			// 
			this.m_pCancel.Location = new System.Drawing.Point(168, 136);
			this.m_pCancel.Name = "m_pCancel";
			this.m_pCancel.Size = new System.Drawing.Size(72, 24);
			this.m_pCancel.TabIndex = 0;
			this.m_pCancel.Text = "Cancel";
			this.m_pCancel.UseStaticViewStyle = true;
			// 
			// m_pCancel.ViewStyle
			// 
			this.m_pCancel.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pCancel_ButtonPressed);
			// 
			// wfrm_AddServer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(258, 175);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_pCancel,
																		  this.wLabel4,
																		  this.wLabel3,
																		  this.wLabel2,
																		  this.wLabel1,
																		  this.m_pOk,
																		  this.m_pName,
																		  this.m_pWebServicesUser,
																		  this.m_pWebServicesPwd,
																		  this.m_pWebServicesUrl});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "wfrm_AddServer";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Add server";
			this.ResumeLayout(false);

		}
		#endregion


		#region function m_pOk_ButtonPressed

		private void m_pOk_ButtonPressed(object sender, System.EventArgs e)
		{
			if(m_pName.Text.Length == 0){
				m_pName.FlashControl();
				return;
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		#endregion

		#region function m_pCancel_ButtonPressed

		private void m_pCancel_ButtonPressed(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets server name.
		/// </summary>
		public string wp_Name
		{
			get{ return m_pName.Text; }
		}

		/// <summary>
		/// Gets server name.
		/// </summary>
		public string wp_WebServicesUrl
		{
			get{ return m_pWebServicesUrl.Text; }
		}

		/// <summary>
		/// Gets server name.
		/// </summary>
		public string wp_WebServicesUser
		{
			get{ return m_pWebServicesUser.Text; }
		}

		/// <summary>
		/// Gets server name.
		/// </summary>
		public string wp_WebServicesPwd
		{
			get{ return m_pWebServicesPwd.Text; }
		}

		#endregion

	}
}
