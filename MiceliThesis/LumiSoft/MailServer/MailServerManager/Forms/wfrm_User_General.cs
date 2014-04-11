using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using LumiSoft.MailServer;
using LumiSoft.UI.Controls;

namespace MailServerManager.Forms
{
	/// <summary>
	/// Summary description for wfrm_User.
	/// </summary>
	public class wfrm_User_General : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pEnabled;
		private LumiSoft.UI.Controls.WSpinEdit m_pMailboxSize;
		private LumiSoft.UI.Controls.WLabel wLabel3;
		private LumiSoft.UI.Controls.WLabel mt_userenabled;
		private LumiSoft.UI.Controls.WLabel mt_mailboxsize;
		private LumiSoft.UI.Controls.WButton m_pRemove;
		private LumiSoft.UI.Controls.WButton m_pAdd;
		private LumiSoft.UI.Controls.WButton m_pGenerate;
		private LumiSoft.UI.Controls.WComboBox m_pDomains;
		private LumiSoft.UI.Controls.WLabel mt_symbolAT;
		private LumiSoft.UI.Controls.WEditBox m_pAddress;
		private LumiSoft.UI.Controls.WEditBox m_pDescription;
		private LumiSoft.UI.Controls.WEditBox m_pPassword;
		private LumiSoft.UI.Controls.WEditBox m_pLogOnName;
		private LumiSoft.UI.Controls.WEditBox m_pFullName;
		private LumiSoft.UI.Controls.WLabel mt_Emails;
		private LumiSoft.UI.Controls.WLabel mt_description;
		private LumiSoft.UI.Controls.WLabel mt_password;
		private LumiSoft.UI.Controls.WLabel mt_username;
		private LumiSoft.UI.Controls.WLabel mt_fullname;
		private System.Windows.Forms.ListBox m_pAddresses;
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pAllowRelay;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private ServerAPI m_ServerAPI = null;		
		private DataRow drUser        = null;

		public wfrm_User_General(ServerAPI serverAPI,string domainID)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			try
			{
				m_ServerAPI = serverAPI;
					 

				DataView dvDomains = m_ServerAPI.GetDomainList();
				foreach(DataRowView vDr in dvDomains){
					m_pDomains.Items.Add(vDr["DomainName"].ToString(),vDr["DomainID"].ToString());
				}
		
				if(m_pDomains.Items.Count > 0){
					m_pDomains.SelectedIndex = 0;
				}
				if(domainID != "ALL"){
					m_pDomains.SelectItemByTag(domainID);
				}
			}
			catch(Exception x)
			{
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}
		}		

		public wfrm_User_General(ServerAPI serverAPI,DataRow dr)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			
			try
			{
				m_ServerAPI = serverAPI;
				drUser      = dr;

				m_pFullName.Text      = dr["FullName"].ToString();
				m_pLogOnName.Text     = dr["UserName"].ToString();
				m_pPassword.Text      = dr["Password"].ToString();
				m_pDescription.Text   = dr["Description"].ToString();
				m_pMailboxSize.Text   = dr["Mailbox_Size"].ToString();
				m_pEnabled.Checked    = Convert.ToBoolean(dr["Enabled"]);
				m_pAllowRelay.Checked = Convert.ToBoolean(dr["AllowRelay"]);
											
				string[] addresses = dr["Emails"].ToString().Split(new char[]{';'});
				foreach(string adr in addresses){
					m_pAddresses.Items.Add(adr);
				}

				DataView dvDomains = m_ServerAPI.GetDomainList();
				foreach(DataRowView vDr in dvDomains){
					m_pDomains.Items.Add(vDr["DomainName"].ToString(),vDr["DomainID"].ToString());
				}

				if(m_pAddresses.Items.Count > 0){
					m_pDomains.SelectItemByTag(dr["DomainID"].ToString());
					m_pDomains.Enabled = false;
				}
			}
			catch(Exception x)
			{
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}
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
			this.m_pEnabled = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.m_pMailboxSize = new LumiSoft.UI.Controls.WSpinEdit();
			this.wLabel3 = new LumiSoft.UI.Controls.WLabel();
			this.mt_userenabled = new LumiSoft.UI.Controls.WLabel();
			this.mt_mailboxsize = new LumiSoft.UI.Controls.WLabel();
			this.m_pRemove = new LumiSoft.UI.Controls.WButton();
			this.m_pAdd = new LumiSoft.UI.Controls.WButton();
			this.m_pGenerate = new LumiSoft.UI.Controls.WButton();
			this.m_pDomains = new LumiSoft.UI.Controls.WComboBox();
			this.mt_symbolAT = new LumiSoft.UI.Controls.WLabel();
			this.m_pAddress = new LumiSoft.UI.Controls.WEditBox();
			this.m_pDescription = new LumiSoft.UI.Controls.WEditBox();
			this.m_pPassword = new LumiSoft.UI.Controls.WEditBox();
			this.m_pLogOnName = new LumiSoft.UI.Controls.WEditBox();
			this.m_pFullName = new LumiSoft.UI.Controls.WEditBox();
			this.mt_Emails = new LumiSoft.UI.Controls.WLabel();
			this.mt_description = new LumiSoft.UI.Controls.WLabel();
			this.mt_password = new LumiSoft.UI.Controls.WLabel();
			this.mt_username = new LumiSoft.UI.Controls.WLabel();
			this.mt_fullname = new LumiSoft.UI.Controls.WLabel();
			this.m_pAddresses = new System.Windows.Forms.ListBox();
			this.m_pAllowRelay = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.SuspendLayout();
			// 
			// m_pEnabled
			// 
			this.m_pEnabled.Checked = true;
			this.m_pEnabled.Location = new System.Drawing.Point(328, 134);
			this.m_pEnabled.Name = "m_pEnabled";
			this.m_pEnabled.ReadOnly = false;
			this.m_pEnabled.Size = new System.Drawing.Size(30, 22);
			this.m_pEnabled.TabIndex = 30;
			this.m_pEnabled.UseStaticViewStyle = true;
			// 
			// m_pEnabled.ViewStyle
			// 
			// 
			// m_pMailboxSize
			// 
			this.m_pMailboxSize.BackColor = System.Drawing.Color.White;
			this.m_pMailboxSize.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Left;
			this.m_pMailboxSize.DecimalPlaces = 0;
			this.m_pMailboxSize.DecMaxValue = new System.Decimal(new int[] {
																			   999999999,
																			   0,
																			   0,
																			   0});
			this.m_pMailboxSize.DecMinValue = new System.Decimal(new int[] {
																			   999999999,
																			   0,
																			   0,
																			   -2147483648});
			this.m_pMailboxSize.DecValue = new System.Decimal(new int[] {
																			20,
																			0,
																			0,
																			0});
			this.m_pMailboxSize.Location = new System.Drawing.Point(120, 134);
			this.m_pMailboxSize.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pMailboxSize.MaxLength = 32767;
			this.m_pMailboxSize.Name = "m_pMailboxSize";
			this.m_pMailboxSize.ReadOnly = false;
			this.m_pMailboxSize.Size = new System.Drawing.Size(56, 20);
			this.m_pMailboxSize.TabIndex = 29;
			this.m_pMailboxSize.Text = "20";
			this.m_pMailboxSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.m_pMailboxSize.UseStaticViewStyle = true;
			// 
			// m_pMailboxSize.ViewStyle
			// 
			// 
			// wLabel3
			// 
			this.wLabel3.Location = new System.Drawing.Point(176, 134);
			this.wLabel3.Name = "wLabel3";
			this.wLabel3.Size = new System.Drawing.Size(40, 24);
			this.wLabel3.TabIndex = 43;
			this.wLabel3.Text = "MB";
			this.wLabel3.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel3.ViewStyle
			// 
			// 
			// mt_userenabled
			// 
			this.mt_userenabled.Location = new System.Drawing.Point(224, 134);
			this.mt_userenabled.Name = "mt_userenabled";
			this.mt_userenabled.Size = new System.Drawing.Size(104, 24);
			this.mt_userenabled.TabIndex = 44;
			this.mt_userenabled.Text = "User Enabled";
			this.mt_userenabled.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_userenabled.ViewStyle
			// 
			// 
			// mt_mailboxsize
			// 
			this.mt_mailboxsize.Location = new System.Drawing.Point(8, 134);
			this.mt_mailboxsize.Name = "mt_mailboxsize";
			this.mt_mailboxsize.Size = new System.Drawing.Size(112, 24);
			this.mt_mailboxsize.TabIndex = 41;
			this.mt_mailboxsize.Text = "Max. Mailbox Size";
			this.mt_mailboxsize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_mailboxsize.ViewStyle
			// 
			// 
			// m_pRemove
			// 
			this.m_pRemove.Location = new System.Drawing.Point(280, 246);
			this.m_pRemove.Name = "m_pRemove";
			this.m_pRemove.TabIndex = 34;
			this.m_pRemove.Text = "Remove";
			this.m_pRemove.UseStaticViewStyle = true;
			// 
			// m_pRemove.ViewStyle
			// 
			this.m_pRemove.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pRemove_Click);
			// 
			// m_pAdd
			// 
			this.m_pAdd.Location = new System.Drawing.Point(280, 214);
			this.m_pAdd.Name = "m_pAdd";
			this.m_pAdd.TabIndex = 33;
			this.m_pAdd.Text = "Add";
			this.m_pAdd.UseStaticViewStyle = true;
			// 
			// m_pAdd.ViewStyle
			// 
			this.m_pAdd.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pAdd_Click);
			// 
			// m_pGenerate
			// 
			this.m_pGenerate.Location = new System.Drawing.Point(232, 62);
			this.m_pGenerate.Name = "m_pGenerate";
			this.m_pGenerate.Size = new System.Drawing.Size(128, 24);
			this.m_pGenerate.TabIndex = 27;
			this.m_pGenerate.Text = "Generate Password";
			this.m_pGenerate.UseStaticViewStyle = true;
			// 
			// m_pGenerate.ViewStyle
			// 
			this.m_pGenerate.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pGenerate_Click);
			// 
			// m_pDomains
			// 
			this.m_pDomains.DropDownWidth = 144;
			this.m_pDomains.Location = new System.Drawing.Point(216, 182);
			this.m_pDomains.Name = "m_pDomains";
			this.m_pDomains.EditStyle = LumiSoft.UI.Controls.EditStyle.Selectable;
			this.m_pDomains.SelectedIndex = -1;
			this.m_pDomains.Size = new System.Drawing.Size(144, 20);
			this.m_pDomains.TabIndex = 32;
			this.m_pDomains.UseStaticViewStyle = true;
			// 
			// m_pDomains.ViewStyle
			// 
			this.m_pDomains.VisibleItems = 5;
			// 
			// mt_symbolAT
			// 
			this.mt_symbolAT.Location = new System.Drawing.Point(200, 182);
			this.mt_symbolAT.Name = "mt_symbolAT";
			this.mt_symbolAT.Size = new System.Drawing.Size(16, 24);
			this.mt_symbolAT.TabIndex = 45;
			this.mt_symbolAT.Text = "@";
			// 
			// mt_symbolAT.ViewStyle
			// 
			// 
			// m_pAddress
			// 
			this.m_pAddress.Location = new System.Drawing.Point(8, 182);
			this.m_pAddress.Name = "m_pAddress";
			this.m_pAddress.Size = new System.Drawing.Size(192, 20);
			this.m_pAddress.TabIndex = 31;
			this.m_pAddress.UseStaticViewStyle = true;
			// 
			// m_pAddress.ViewStyle
			// 
			// 
			// m_pDescription
			// 
			this.m_pDescription.Location = new System.Drawing.Point(8, 102);
			this.m_pDescription.Name = "m_pDescription";
			this.m_pDescription.Size = new System.Drawing.Size(352, 20);
			this.m_pDescription.TabIndex = 28;
			this.m_pDescription.UseStaticViewStyle = true;
			// 
			// m_pDescription.ViewStyle
			// 
			// 
			// m_pPassword
			// 
			this.m_pPassword.Location = new System.Drawing.Point(120, 62);
			this.m_pPassword.Name = "m_pPassword";
			this.m_pPassword.Size = new System.Drawing.Size(104, 20);
			this.m_pPassword.TabIndex = 26;
			this.m_pPassword.UseStaticViewStyle = true;
			// 
			// m_pPassword.ViewStyle
			// 
			// 
			// m_pLogOnName
			// 
			this.m_pLogOnName.Location = new System.Drawing.Point(8, 62);
			this.m_pLogOnName.Name = "m_pLogOnName";
			this.m_pLogOnName.Size = new System.Drawing.Size(104, 20);
			this.m_pLogOnName.TabIndex = 25;
			this.m_pLogOnName.UseStaticViewStyle = true;
			// 
			// m_pLogOnName.ViewStyle
			// 
			// 
			// m_pFullName
			// 
			this.m_pFullName.Location = new System.Drawing.Point(8, 22);
			this.m_pFullName.Name = "m_pFullName";
			this.m_pFullName.Size = new System.Drawing.Size(216, 20);
			this.m_pFullName.TabIndex = 24;
			this.m_pFullName.UseStaticViewStyle = true;
			// 
			// m_pFullName.ViewStyle
			// 
			// 
			// mt_Emails
			// 
			this.mt_Emails.Location = new System.Drawing.Point(8, 166);
			this.mt_Emails.Name = "mt_Emails";
			this.mt_Emails.Size = new System.Drawing.Size(104, 16);
			this.mt_Emails.TabIndex = 42;
			this.mt_Emails.Text = "E-mail Address";
			this.mt_Emails.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_Emails.ViewStyle
			// 
			// 
			// mt_description
			// 
			this.mt_description.Location = new System.Drawing.Point(8, 86);
			this.mt_description.Name = "mt_description";
			this.mt_description.Size = new System.Drawing.Size(104, 16);
			this.mt_description.TabIndex = 40;
			this.mt_description.Text = "Description";
			this.mt_description.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_description.ViewStyle
			// 
			// 
			// mt_password
			// 
			this.mt_password.Location = new System.Drawing.Point(120, 46);
			this.mt_password.Name = "mt_password";
			this.mt_password.Size = new System.Drawing.Size(104, 16);
			this.mt_password.TabIndex = 39;
			this.mt_password.Text = "Password";
			this.mt_password.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_password.ViewStyle
			// 
			// 
			// mt_username
			// 
			this.mt_username.Location = new System.Drawing.Point(8, 46);
			this.mt_username.Name = "mt_username";
			this.mt_username.Size = new System.Drawing.Size(104, 16);
			this.mt_username.TabIndex = 38;
			this.mt_username.Text = "User Name";
			this.mt_username.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_username.ViewStyle
			// 
			// 
			// mt_fullname
			// 
			this.mt_fullname.Location = new System.Drawing.Point(8, 6);
			this.mt_fullname.Name = "mt_fullname";
			this.mt_fullname.Size = new System.Drawing.Size(104, 16);
			this.mt_fullname.TabIndex = 37;
			this.mt_fullname.Text = "Full Name";
			this.mt_fullname.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_fullname.ViewStyle
			// 
			// 
			// m_pAddresses
			// 
			this.m_pAddresses.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_pAddresses.Location = new System.Drawing.Point(8, 214);
			this.m_pAddresses.Name = "m_pAddresses";
			this.m_pAddresses.Size = new System.Drawing.Size(264, 132);
			this.m_pAddresses.TabIndex = 36;
			// 
			// m_pAllowRelay
			// 
			this.m_pAllowRelay.Checked = true;
			this.m_pAllowRelay.Location = new System.Drawing.Point(328, 152);
			this.m_pAllowRelay.Name = "m_pAllowRelay";
			this.m_pAllowRelay.ReadOnly = false;
			this.m_pAllowRelay.Size = new System.Drawing.Size(30, 22);
			this.m_pAllowRelay.TabIndex = 46;
			this.m_pAllowRelay.UseStaticViewStyle = true;
			// 
			// m_pAllowRelay.ViewStyle
			// 
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(224, 152);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(104, 24);
			this.wLabel1.TabIndex = 47;
			this.wLabel1.Text = "Allow Relay";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// wfrm_User_General
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(368, 351);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_pAllowRelay,
																		  this.wLabel1,
																		  this.m_pEnabled,
																		  this.m_pMailboxSize,
																		  this.wLabel3,
																		  this.mt_userenabled,
																		  this.mt_mailboxsize,
																		  this.m_pRemove,
																		  this.m_pAdd,
																		  this.m_pGenerate,
																		  this.m_pDomains,
																		  this.mt_symbolAT,
																		  this.m_pAddress,
																		  this.m_pDescription,
																		  this.m_pPassword,
																		  this.m_pLogOnName,
																		  this.m_pFullName,
																		  this.mt_Emails,
																		  this.mt_description,
																		  this.mt_password,
																		  this.mt_username,
																		  this.mt_fullname,
																		  this.m_pAddresses});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "wfrm_User_General";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "User";
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		#region function m_pGenerate_Click

		private void m_pGenerate_Click(object sender, System.EventArgs e)
		{
			m_pPassword.Text = Guid.NewGuid().ToString().Substring(0,8);
		}

		#endregion

		#region function m_pAdd_Click

		private void m_pAdd_Click(object sender, System.EventArgs e)
		{
			string address = m_pAddress.Text + "@" + m_pDomains.Text;

			#region Validation

			if(m_pAddress.Text.Length == 0){
				MessageBox.Show("Emails address can't be empty!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
				m_pAddress.FlashControl();
				return;
			}

			if(m_ServerAPI.EmailAddressExists(address)){
				MessageBox.Show("Emails address already exists!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
				return;
			}

			#endregion
			
			if(address.IndexOf("<") == -1){
				address = "<" + address;
			}
			if(address.IndexOf(">") == -1){
				address += ">";
			}

			m_pAddresses.Items.Add(address);
			m_pAddress.Text = "";
			m_pAddress.Focus();

			m_pDomains.Enabled = false;
		}

		#endregion

		#region function m_pRemove_Click

		private void m_pRemove_Click(object sender, System.EventArgs e)
		{
			if(m_pAddresses.SelectedIndex > -1){
				m_pAddresses.Items.RemoveAt(m_pAddresses.SelectedIndex);
			
				if(m_pAddresses.Items.Count == 0){
					m_pDomains.Enabled = true;
				}
			}
		}

		#endregion

		#endregion

	
		#region function ValidateValues

		/// <summary>
		/// Validate form values.
		/// </summary>
		/// <returns></returns>
		public bool ValidateValues()
		{
			if(m_pLogOnName.Text.Length <= 0){
				MessageBox.Show("User name cannot be empty!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
				m_pLogOnName.FlashControl();
				return false;
			}

		//	if(m_pPassword.Text.Length <= 0){
		//		MessageBox.Show("Password cannot be empty!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
		//		m_pPassword.FlashControl();
		//		return false;
		//	}

			if(m_pAddresses.Items.Count == 0){
				MessageBox.Show("Please add at least one Emails address!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
			//	m_pAddresses.F
				return false;
			}

			return true;
		}

		#endregion

		
		#region Properties Implementation

		public string FullName
		{
			get{ return  m_pFullName.Text; }
		}

		public string UserName
		{
			get{ return m_pLogOnName.Text; }
		}

		public string Password
		{
			get{ return m_pPassword.Text; }
		}

		public string Description
		{
			get{ return m_pDescription.Text; }
		}

		public string Emails
		{
			get{
				string address = "";
				foreach(string str in m_pAddresses.Items){
					address += str + ";";
				}
			
				// remove ";" from end
				address = address.Substring(0,address.Length-1);

				return address; 
			}
		}

		public bool UserEnabled
		{
			get{ return m_pEnabled.Checked; }
		}

		public bool AllowRelay
		{
			get{ return m_pAllowRelay.Checked; }
		}

		public string DomainID
		{
			get{ return m_pDomains.SelectedItem.Tag.ToString(); }
		}

		public int MailboxSize
		{
			get{ return (int)m_pMailboxSize.DecValue; }
		}

		#endregion
	}
}
