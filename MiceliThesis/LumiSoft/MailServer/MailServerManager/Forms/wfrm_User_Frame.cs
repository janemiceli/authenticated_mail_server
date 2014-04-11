using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using LumiSoft.MailServer;
using LumiSoft.UI.Controls;

namespace MailServerManager.Forms
{
	/// <summary>
	/// Summary description for wfrm_User_Frame.
	/// </summary>
	public class wfrm_User_Frame : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WTabs.WTab wTab1;
		private LumiSoft.UI.Controls.WButton m_pCancel;
		private LumiSoft.UI.Controls.WButton m_pOk;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private ServerAPI m_ServerAPI   = null;
	//	private DataRow   drUser        = null;
		private bool      m_New         = false;

		private wfrm_User_General m_wfrm_User_General = null;
		private wfrm_User_Pop3Rem m_wfrm_User_Pop3Rem = null;

		public wfrm_User_Frame(ServerAPI serverAPI,string domainID)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			m_ServerAPI = serverAPI;

			m_wfrm_User_General = new wfrm_User_General(serverAPI,domainID);
			m_wfrm_User_Pop3Rem = new wfrm_User_Pop3Rem(serverAPI,"xxxx_afafaftwwt_sgs"); // set dummy username

			wTab1.AddTab(m_wfrm_User_General, "General");
			wTab1.AddTab(m_wfrm_User_Pop3Rem, "Pop3 remote accounts");
			wTab1.SelectFirstTab();

			m_New = true;
		}

		public wfrm_User_Frame(ServerAPI serverAPI,DataRow dr)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			m_ServerAPI = serverAPI;

			m_wfrm_User_General = new wfrm_User_General(serverAPI,dr);
			m_wfrm_User_Pop3Rem = new wfrm_User_Pop3Rem(serverAPI,dr["UserName"].ToString());

			wTab1.AddTab(m_wfrm_User_General, "General");
			wTab1.AddTab(m_wfrm_User_Pop3Rem, "Pop3 remote accounts");
			wTab1.SelectFirstTab();
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
			this.wTab1 = new LumiSoft.UI.Controls.WTabs.WTab();
			this.m_pCancel = new LumiSoft.UI.Controls.WButton();
			this.m_pOk = new LumiSoft.UI.Controls.WButton();
			this.SuspendLayout();
			// 
			// wTab1
			// 
			this.wTab1.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.wTab1.Name = "wTab1";
			this.wTab1.SelectedTab = null;
			this.wTab1.Size = new System.Drawing.Size(370, 380);
			this.wTab1.TabIndex = 0;
			// 
			// m_pCancel
			// 
			this.m_pCancel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.m_pCancel.Location = new System.Drawing.Point(286, 388);
			this.m_pCancel.Name = "m_pCancel";
			this.m_pCancel.TabIndex = 36;
			this.m_pCancel.Text = "Cancel";
			this.m_pCancel.UseStaticViewStyle = true;
			// 
			// m_pCancel.ViewStyle
			// 
			this.m_pCancel.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pCancel_ButtonPressed);
			// 
			// m_pOk
			// 
			this.m_pOk.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.m_pOk.Location = new System.Drawing.Point(190, 388);
			this.m_pOk.Name = "m_pOk";
			this.m_pOk.TabIndex = 37;
			this.m_pOk.Text = "OK";
			this.m_pOk.UseStaticViewStyle = true;
			// 
			// m_pOk.ViewStyle
			// 
			this.m_pOk.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pOk_ButtonPressed);
			// 
			// wfrm_User_Frame
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(370, 415);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_pCancel,
																		  this.m_pOk,
																		  this.wTab1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "wfrm_User_Frame";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "User properties";
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		private void m_pOk_ButtonPressed(object sender, System.EventArgs e)
		{
			wTab1.SelectFirstTab();

			// Validate user general settings
			if(!m_wfrm_User_General.ValidateValues()){
				return;
			}

			try
			{
				if(m_New && m_ServerAPI.MailboxExists(m_wfrm_User_General.UserName)){					
					MessageBox.Show("User alredy exists!!!\nPlease select another User Name.","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
			//		m_pLogOnName.FlashControl();
					return;
				}				
			}
			catch(Exception x)
			{
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void m_pCancel_ButtonPressed(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#endregion

		
		#region Properties Implementation

		/// <summary>
		/// Gets user domain ID.
		/// </summary>
		public string DomainID
		{
			get{ return m_wfrm_User_General.DomainID; }
		}

		/// <summary>
		/// Gets user full name.
		/// </summary>
		public string FullName
		{
			get{ return m_wfrm_User_General.FullName; }
		}

		/// <summary>
		/// Gets user login name.
		/// </summary>
		public string UserName
		{
			get{ return m_wfrm_User_General.UserName; }
		}

		/// <summary>
		/// Gets user password.
		/// </summary>
		public string Password
		{
			get{ return m_wfrm_User_General.Password; }
		}

		/// <summary>
		/// Gets user description.
		/// </summary>
		public string Description
		{
			get{ return m_wfrm_User_General.Description; }
		}

		/// <summary>
		/// Gets user email addresses.
		/// </summary>
		public string Emails
		{
			get{ return m_wfrm_User_General.Emails; }
		}		

		/// <summary>
		/// Gets user mailbox size. 
		/// </summary>
		public int MailboxSize
		{
			get{ return m_wfrm_User_General.MailboxSize; }
		}

		/// <summary>
		/// Gets if user is enabled.
		/// </summary>
		public bool UserEnabled
		{
			get{ return m_wfrm_User_General.UserEnabled; }
		}

		/// <summary>
		/// Gets if user may relay.
		/// </summary>
		public bool AllowRelay
		{
			get{ return m_wfrm_User_General.AllowRelay; }
		}

		/// <summary>
		/// Gets byte(DataSet) with remote accounts.
		/// </summary>
		public byte[] wp_RemoteAccounts
		{
			get{ return m_wfrm_User_Pop3Rem.wp_RemoteAccounts; }
		}

		#endregion
	}
}
