using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using LumiSoft.MailServer;

namespace MailServerManager.Forms
{
	/// <summary>
	/// Summary description for wfrm_Server.
	/// </summary>
	public class wfrm_Server : System.Windows.Forms.Form
	{
		private System.Windows.Forms.LinkLabel m_pBackUpServer;
		private System.Windows.Forms.LinkLabel m_pRestoreServer;
		private System.Windows.Forms.LinkLabel m_pCreateDomain;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.LinkLabel m_pCreateAlias;
		private System.Windows.Forms.LinkLabel m_pCreateUser;
		private System.Windows.Forms.LinkLabel m_pCreateRoute;
		private System.Windows.Forms.LinkLabel m_pBlockSmtp;
		private System.Windows.Forms.LinkLabel m_pAllowSmtp;
		private System.Windows.Forms.LinkLabel m_pAllowPop3;
		private System.Windows.Forms.LinkLabel m_pBlockPop3;
		private System.Windows.Forms.LinkLabel m_pAllowRelay;
		private System.Windows.Forms.LinkLabel m_pDenyRelay;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;

		private ServerAPI m_ServerAPI = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="serverAPI"></param>
		public wfrm_Server(ServerAPI serverAPI)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			m_ServerAPI = serverAPI;
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
			this.m_pBackUpServer = new System.Windows.Forms.LinkLabel();
			this.m_pRestoreServer = new System.Windows.Forms.LinkLabel();
			this.m_pCreateDomain = new System.Windows.Forms.LinkLabel();
			this.m_pCreateAlias = new System.Windows.Forms.LinkLabel();
			this.m_pCreateRoute = new System.Windows.Forms.LinkLabel();
			this.m_pBlockSmtp = new System.Windows.Forms.LinkLabel();
			this.m_pAllowSmtp = new System.Windows.Forms.LinkLabel();
			this.m_pAllowPop3 = new System.Windows.Forms.LinkLabel();
			this.m_pBlockPop3 = new System.Windows.Forms.LinkLabel();
			this.m_pAllowRelay = new System.Windows.Forms.LinkLabel();
			this.m_pDenyRelay = new System.Windows.Forms.LinkLabel();
			this.m_pCreateUser = new System.Windows.Forms.LinkLabel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_pBackUpServer
			// 
			this.m_pBackUpServer.Location = new System.Drawing.Point(24, 24);
			this.m_pBackUpServer.Name = "m_pBackUpServer";
			this.m_pBackUpServer.Size = new System.Drawing.Size(176, 16);
			this.m_pBackUpServer.TabIndex = 1;
			this.m_pBackUpServer.TabStop = true;
			this.m_pBackUpServer.Text = "Backup server";
			this.m_pBackUpServer.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pBackUpServer_LinkClicked);
			// 
			// m_pRestoreServer
			// 
			this.m_pRestoreServer.Location = new System.Drawing.Point(248, 24);
			this.m_pRestoreServer.Name = "m_pRestoreServer";
			this.m_pRestoreServer.Size = new System.Drawing.Size(176, 16);
			this.m_pRestoreServer.TabIndex = 2;
			this.m_pRestoreServer.TabStop = true;
			this.m_pRestoreServer.Text = "Restore server";
			this.m_pRestoreServer.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pRestoreServer_LinkClicked);
			// 
			// m_pCreateDomain
			// 
			this.m_pCreateDomain.Location = new System.Drawing.Point(24, 24);
			this.m_pCreateDomain.Name = "m_pCreateDomain";
			this.m_pCreateDomain.Size = new System.Drawing.Size(176, 16);
			this.m_pCreateDomain.TabIndex = 3;
			this.m_pCreateDomain.TabStop = true;
			this.m_pCreateDomain.Text = "Create new domain";
			this.m_pCreateDomain.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pCreateDomain_LinkClicked);
			// 
			// m_pCreateAlias
			// 
			this.m_pCreateAlias.Location = new System.Drawing.Point(248, 24);
			this.m_pCreateAlias.Name = "m_pCreateAlias";
			this.m_pCreateAlias.Size = new System.Drawing.Size(176, 16);
			this.m_pCreateAlias.TabIndex = 4;
			this.m_pCreateAlias.TabStop = true;
			this.m_pCreateAlias.Text = "Create new alias";
			this.m_pCreateAlias.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pCreateAlias_LinkClicked);
			// 
			// m_pCreateRoute
			// 
			this.m_pCreateRoute.Location = new System.Drawing.Point(248, 48);
			this.m_pCreateRoute.Name = "m_pCreateRoute";
			this.m_pCreateRoute.Size = new System.Drawing.Size(176, 16);
			this.m_pCreateRoute.TabIndex = 6;
			this.m_pCreateRoute.TabStop = true;
			this.m_pCreateRoute.Text = "Create new route";
			this.m_pCreateRoute.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pCreateRoute_LinkClicked);
			// 
			// m_pBlockSmtp
			// 
			this.m_pBlockSmtp.Location = new System.Drawing.Point(24, 24);
			this.m_pBlockSmtp.Name = "m_pBlockSmtp";
			this.m_pBlockSmtp.Size = new System.Drawing.Size(176, 16);
			this.m_pBlockSmtp.TabIndex = 7;
			this.m_pBlockSmtp.TabStop = true;
			this.m_pBlockSmtp.Text = "Block SMTP for IP/Range";
			this.m_pBlockSmtp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pBlockSmtp_LinkClicked);
			// 
			// m_pAllowSmtp
			// 
			this.m_pAllowSmtp.Location = new System.Drawing.Point(24, 48);
			this.m_pAllowSmtp.Name = "m_pAllowSmtp";
			this.m_pAllowSmtp.Size = new System.Drawing.Size(176, 16);
			this.m_pAllowSmtp.TabIndex = 8;
			this.m_pAllowSmtp.TabStop = true;
			this.m_pAllowSmtp.Text = "Allow SMTP for IP/Range";
			this.m_pAllowSmtp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pAllowSmtp_LinkClicked);
			// 
			// m_pAllowPop3
			// 
			this.m_pAllowPop3.Location = new System.Drawing.Point(24, 104);
			this.m_pAllowPop3.Name = "m_pAllowPop3";
			this.m_pAllowPop3.Size = new System.Drawing.Size(176, 16);
			this.m_pAllowPop3.TabIndex = 10;
			this.m_pAllowPop3.TabStop = true;
			this.m_pAllowPop3.Text = "Allow POP3 for IP/Range";
			this.m_pAllowPop3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pAllowPop3_LinkClicked);
			// 
			// m_pBlockPop3
			// 
			this.m_pBlockPop3.Location = new System.Drawing.Point(24, 80);
			this.m_pBlockPop3.Name = "m_pBlockPop3";
			this.m_pBlockPop3.Size = new System.Drawing.Size(176, 16);
			this.m_pBlockPop3.TabIndex = 9;
			this.m_pBlockPop3.TabStop = true;
			this.m_pBlockPop3.Text = "Block POP3 for IP/Range";
			this.m_pBlockPop3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pBlockPop3_LinkClicked);
			// 
			// m_pAllowRelay
			// 
			this.m_pAllowRelay.Location = new System.Drawing.Point(248, 48);
			this.m_pAllowRelay.Name = "m_pAllowRelay";
			this.m_pAllowRelay.Size = new System.Drawing.Size(176, 16);
			this.m_pAllowRelay.TabIndex = 12;
			this.m_pAllowRelay.TabStop = true;
			this.m_pAllowRelay.Text = "Allow Realy for IP/Range";
			this.m_pAllowRelay.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pAllowRelay_LinkClicked);
			// 
			// m_pDenyRelay
			// 
			this.m_pDenyRelay.Location = new System.Drawing.Point(248, 24);
			this.m_pDenyRelay.Name = "m_pDenyRelay";
			this.m_pDenyRelay.Size = new System.Drawing.Size(176, 16);
			this.m_pDenyRelay.TabIndex = 11;
			this.m_pDenyRelay.TabStop = true;
			this.m_pDenyRelay.Text = "Deny  Relay for IP/Range";
			this.m_pDenyRelay.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pDenyRelay_LinkClicked);
			// 
			// m_pCreateUser
			// 
			this.m_pCreateUser.Location = new System.Drawing.Point(24, 48);
			this.m_pCreateUser.Name = "m_pCreateUser";
			this.m_pCreateUser.Size = new System.Drawing.Size(176, 16);
			this.m_pCreateUser.TabIndex = 13;
			this.m_pCreateUser.TabStop = true;
			this.m_pCreateUser.Text = "Create new user";
			this.m_pCreateUser.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_pCreateUser_LinkClicked);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.m_pRestoreServer,
																					this.m_pBackUpServer});
			this.groupBox1.Location = new System.Drawing.Point(8, 24);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(456, 56);
			this.groupBox1.TabIndex = 14;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Server";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.m_pCreateRoute,
																					this.m_pCreateAlias,
																					this.m_pCreateDomain,
																					this.m_pCreateUser});
			this.groupBox2.Location = new System.Drawing.Point(8, 104);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(456, 80);
			this.groupBox2.TabIndex = 15;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "New";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.m_pAllowSmtp,
																					this.m_pBlockSmtp,
																					this.m_pDenyRelay,
																					this.m_pAllowRelay,
																					this.m_pBlockPop3,
																					this.m_pAllowPop3});
			this.groupBox3.Location = new System.Drawing.Point(8, 208);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(456, 136);
			this.groupBox3.TabIndex = 16;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Security";
			// 
			// wfrm_Server
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 381);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.groupBox3,
																		  this.groupBox2,
																		  this.groupBox1});
			this.Name = "wfrm_Server";
			this.Text = "wfrm_Server";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		#region function m_pBackUpServer_LinkClicked

		private void m_pBackUpServer_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				System.Windows.Forms.SaveFileDialog filedlg = new System.Windows.Forms.SaveFileDialog();
				filedlg.Filter = "(*.bcp)|*.bcp";
				filedlg.InitialDirectory = Application.StartupPath + "\\BackUP";
				filedlg.RestoreDirectory = true;
				filedlg.FileName = DateTime.Now.ToString("yyyy_MM_dd");
				
				if(filedlg.ShowDialog() == DialogResult.OK){
					m_ServerAPI.CreateBackUp(filedlg.FileName);
				}
			}			
			catch(Exception x)
			{
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}
		}

		#endregion

		#region function m_pRestoreServer_LinkClicked

		private void m_pRestoreServer_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				if(MessageBox.Show(this,"Warning: Restore overwrites current settings. BackUp Recommended!!!\nDo you want to continue?","Restore confirmation",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2) == DialogResult.Yes)
				{
					System.Windows.Forms.OpenFileDialog filedlg = new System.Windows.Forms.OpenFileDialog();
					filedlg.Filter = "(*.bcp)|*.bcp";
					filedlg.InitialDirectory = Application.StartupPath + "\\BackUP";
					filedlg.RestoreDirectory = true;
					
					if(filedlg.ShowDialog() == DialogResult.OK){
						m_ServerAPI.RestoreBackUp(filedlg.FileName);
					}
				}
			}
			catch(Exception x)
			{
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}
		}

		#endregion


		#region function m_pCreateDomain_LinkClicked

		private void m_pCreateDomain_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			wfrm_Domain frm = new wfrm_Domain(m_ServerAPI);
			if(frm.ShowDialog(this) == DialogResult.OK){

				DataRow dr = m_ServerAPI.AddDomain(frm.wp_Domain,frm.wp_Description);
				if(dr == null){
					MessageBox.Show("Error creating domain!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
					return;
				}

				m_ServerAPI.LoadDomains();
			}
		}

		#endregion

		#region function m_pCreateUser_LinkClicked

		private void m_pCreateUser_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			wfrm_User_Frame frm = new wfrm_User_Frame(m_ServerAPI,"ALL");
			if(frm.ShowDialog(this) == DialogResult.OK){

				DataRow dr = m_ServerAPI.AddUser(frm.FullName,frm.UserName,frm.Password,frm.Description,frm.Emails,frm.DomainID,frm.MailboxSize,frm.UserEnabled,frm.AllowRelay,frm.wp_RemoteAccounts);
				if(dr == null){
					MessageBox.Show("Error adding user!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
					return;
				}
			}

			m_ServerAPI.LoadUsers();
		}

		#endregion

		#region function m_pCreateAlias_LinkClicked

		private void m_pCreateAlias_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			wfrm_Alias frm = new wfrm_Alias(m_ServerAPI,"ALL");
			if(frm.ShowDialog(this) == DialogResult.OK){

				DataRow dr = m_ServerAPI.AddAlias(frm.AliasName,frm.Descriprion,frm.Members,frm.DomainID,frm.IsPublic);
				if(dr == null){
					MessageBox.Show("Error adding alias!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
					return;
				}
			}

			m_ServerAPI.LoadAliases();
		}

		#endregion

		#region function m_pCreateRoute_LinkClicked

		private void m_pCreateRoute_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			wfrm_Route frm = new wfrm_Route(m_ServerAPI);
			if(frm.ShowDialog(this) == DialogResult.OK){

				DataRow dr = m_ServerAPI.AddRoute(frm.Pattern,frm.MailBox,frm.Description,frm.DomainID);
				if(dr == null){
					MessageBox.Show("Error adding alias!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
					return;
				}
			}

			m_ServerAPI.LoadRouting();
		}

		#endregion


		#region function m_pBlockSmtp_LinkClicked

		private void m_pBlockSmtp_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			AddSecurityEntry(SecAction.Deny_SMTP);
		}

		#endregion

		#region function m_pAllowSmtp_LinkClicked

		private void m_pAllowSmtp_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			AddSecurityEntry(SecAction.Allow_SMTP);
		}

		#endregion

		#region function m_pBlockPop3_LinkClicked

		private void m_pBlockPop3_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			AddSecurityEntry(SecAction.Deny_POP3);
		}

		#endregion

		#region function m_pAllowPop3_LinkClicked

		private void m_pAllowPop3_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			AddSecurityEntry(SecAction.Allow_POP3);
		}

		#endregion

		#region function m_pDenyRelay_LinkClicked

		private void m_pDenyRelay_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			AddSecurityEntry(SecAction.Deny_Relay);
		}

		#endregion 

		#region function m_pAllowRelay_LinkClicked

		private void m_pAllowRelay_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			AddSecurityEntry(SecAction.Allow_Relay);
		}

		#endregion

		#region function AddSecurityEntry

		private void AddSecurityEntry(SecAction action)
		{
			wfrm_SecurityEntry frm = new wfrm_SecurityEntry(action);
			if(frm.ShowDialog(this) == DialogResult.OK){

				DataRow dr = m_ServerAPI.AddSecurityEntry(frm.wp_Description,frm.wp_Protocol,frm.wp_Type,frm.wp_Action,frm.wp_Content,frm.wp_StartIP,frm.wp_EndIP);
				if(dr == null){
					MessageBox.Show("Error adding security entry!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
					return;
				}
			}

			m_ServerAPI.LoadSecurity();
		}

		#endregion

		#endregion	

	}
}
