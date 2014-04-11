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
	/// Summary description for wfrm_Alias.
	/// </summary>
	public class wfrm_Alias : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WButton m_pCancel;
		private LumiSoft.UI.Controls.WLabel mt_member;
		private LumiSoft.UI.Controls.WEditBox m_pMember;
		private LumiSoft.UI.Controls.WButton m_pOk;
		private LumiSoft.UI.Controls.WButton m_pRemove;
		private LumiSoft.UI.Controls.WButton m_pAdd;
		private LumiSoft.UI.Controls.WComboBox m_pDomains;
		private LumiSoft.UI.Controls.WLabel mt_symbolAT;
		private LumiSoft.UI.Controls.WEditBox m_pName;
		private LumiSoft.UI.Controls.WEditBox m_pDescription;
		private LumiSoft.UI.Controls.WLabel mt_name;
		private LumiSoft.UI.Controls.WLabel mt_description;
		private System.Windows.Forms.ListBox m_pMembers;
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pIsPublic;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private ServerAPI m_ServerAPI   = null;
		private string    m_AliasName   = "";
		private string    m_Descriprion = "";
		private string    m_Members     = "";
		private string    m_DomainID    = "";
		private bool      m_IsPublic    = false;

		/// <summary>
		/// Add new constructor.
		/// </summary>
		/// <param name="serverAPI"></param>
		/// <param name="domainID"></param>
		public wfrm_Alias(ServerAPI serverAPI,string domainID)
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

		/// <summary>
		/// Edit constructor.
		/// </summary>
		/// <param name="serverAPI"></param>
		/// <param name="dr"></param>
		public wfrm_Alias(ServerAPI serverAPI,DataRow dr)
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

				m_pName.Text        = dr["AliasName"].ToString().Split(new char[]{'@'})[0];
				m_pDescription.Text = dr["Description"].ToString();
				m_pIsPublic.Checked = Convert.ToBoolean(dr["IsPublic"]);
			
				string[] member = dr["AliasMembers"].ToString().Split(new char[]{';'});
				foreach(string adr in member){
					m_pMembers.Items.Add(adr);
				}
			
				DataView dvDomains = m_ServerAPI.GetDomainList();
				foreach(DataRowView vDr in dvDomains){
					m_pDomains.Items.Add(vDr["DomainName"].ToString(),vDr["DomainID"].ToString());
				}

				if(m_pMembers.Items.Count > 0){
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
			this.m_pCancel = new LumiSoft.UI.Controls.WButton();
			this.mt_member = new LumiSoft.UI.Controls.WLabel();
			this.m_pMember = new LumiSoft.UI.Controls.WEditBox();
			this.m_pOk = new LumiSoft.UI.Controls.WButton();
			this.m_pRemove = new LumiSoft.UI.Controls.WButton();
			this.m_pAdd = new LumiSoft.UI.Controls.WButton();
			this.m_pDomains = new LumiSoft.UI.Controls.WComboBox();
			this.mt_symbolAT = new LumiSoft.UI.Controls.WLabel();
			this.m_pName = new LumiSoft.UI.Controls.WEditBox();
			this.m_pDescription = new LumiSoft.UI.Controls.WEditBox();
			this.mt_name = new LumiSoft.UI.Controls.WLabel();
			this.mt_description = new LumiSoft.UI.Controls.WLabel();
			this.m_pMembers = new System.Windows.Forms.ListBox();
			this.m_pIsPublic = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.SuspendLayout();
			// 
			// m_pCancel
			// 
			this.m_pCancel.DrawBorder = true;
			this.m_pCancel.Location = new System.Drawing.Point(280, 317);
			this.m_pCancel.Name = "m_pCancel";
			this.m_pCancel.TabIndex = 13;
			this.m_pCancel.Text = "Cancel";
			this.m_pCancel.UseStaticViewStyle = true;
			// 
			// m_pCancel.ViewStyle
			// 
			this.m_pCancel.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pCancel_ButtonPressed);
			// 
			// mt_member
			// 
			this.mt_member.Location = new System.Drawing.Point(8, 85);
			this.mt_member.Name = "mt_member";
			this.mt_member.Size = new System.Drawing.Size(144, 16);
			this.mt_member.TabIndex = 25;
			this.mt_member.Text = "Member email address";
			this.mt_member.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_member.ViewStyle
			// 
			// 
			// m_pMember
			// 
			this.m_pMember.DrawBorder = true;
			this.m_pMember.Location = new System.Drawing.Point(8, 101);
			this.m_pMember.Name = "m_pMember";
			this.m_pMember.Size = new System.Drawing.Size(192, 20);
			this.m_pMember.TabIndex = 17;
			this.m_pMember.UseStaticViewStyle = true;
			// 
			// m_pMember.ViewStyle
			// 
			// 
			// m_pOk
			// 
			this.m_pOk.DrawBorder = true;
			this.m_pOk.Location = new System.Drawing.Point(280, 285);
			this.m_pOk.Name = "m_pOk";
			this.m_pOk.TabIndex = 20;
			this.m_pOk.Text = "OK";
			this.m_pOk.UseStaticViewStyle = true;
			// 
			// m_pOk.ViewStyle
			// 
			this.m_pOk.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pOk_Click);
			// 
			// m_pRemove
			// 
			this.m_pRemove.DrawBorder = true;
			this.m_pRemove.Location = new System.Drawing.Point(280, 165);
			this.m_pRemove.Name = "m_pRemove";
			this.m_pRemove.TabIndex = 19;
			this.m_pRemove.Text = "Remove";
			this.m_pRemove.UseStaticViewStyle = true;
			// 
			// m_pRemove.ViewStyle
			// 
			this.m_pRemove.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pRemove_Click);
			// 
			// m_pAdd
			// 
			this.m_pAdd.DrawBorder = true;
			this.m_pAdd.Location = new System.Drawing.Point(280, 133);
			this.m_pAdd.Name = "m_pAdd";
			this.m_pAdd.TabIndex = 18;
			this.m_pAdd.Text = "Add";
			this.m_pAdd.UseStaticViewStyle = true;
			// 
			// m_pAdd.ViewStyle
			// 
			this.m_pAdd.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pAdd_Click);
			// 
			// m_pDomains
			// 
			this.m_pDomains.DrawBorder = true;
			this.m_pDomains.DropDownWidth = 144;
			this.m_pDomains.Location = new System.Drawing.Point(216, 21);
			this.m_pDomains.Name = "m_pDomains";
			this.m_pDomains.SelectedIndex = -1;
			this.m_pDomains.SelectionLength = 0;
			this.m_pDomains.SelectionStart = 0;
			this.m_pDomains.Size = new System.Drawing.Size(144, 20);
			this.m_pDomains.TabIndex = 15;
			this.m_pDomains.UseStaticViewStyle = true;
			// 
			// m_pDomains.ViewStyle
			// 
			this.m_pDomains.VisibleItems = 5;
			// 
			// mt_symbolAT
			// 
			this.mt_symbolAT.Location = new System.Drawing.Point(200, 21);
			this.mt_symbolAT.Name = "mt_symbolAT";
			this.mt_symbolAT.Size = new System.Drawing.Size(16, 24);
			this.mt_symbolAT.TabIndex = 23;
			this.mt_symbolAT.Text = "@";
			// 
			// mt_symbolAT.ViewStyle
			// 
			// 
			// m_pName
			// 
			this.m_pName.DrawBorder = true;
			this.m_pName.Location = new System.Drawing.Point(8, 21);
			this.m_pName.Name = "m_pName";
			this.m_pName.Size = new System.Drawing.Size(192, 20);
			this.m_pName.TabIndex = 14;
			this.m_pName.UseStaticViewStyle = true;
			// 
			// m_pName.ViewStyle
			// 
			// 
			// m_pDescription
			// 
			this.m_pDescription.DrawBorder = true;
			this.m_pDescription.Location = new System.Drawing.Point(8, 61);
			this.m_pDescription.Name = "m_pDescription";
			this.m_pDescription.Size = new System.Drawing.Size(352, 20);
			this.m_pDescription.TabIndex = 16;
			this.m_pDescription.UseStaticViewStyle = true;
			// 
			// m_pDescription.ViewStyle
			// 
			// 
			// mt_name
			// 
			this.mt_name.Location = new System.Drawing.Point(8, 5);
			this.mt_name.Name = "mt_name";
			this.mt_name.Size = new System.Drawing.Size(104, 16);
			this.mt_name.TabIndex = 22;
			this.mt_name.Text = "AliasName Name";
			this.mt_name.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_name.ViewStyle
			// 
			// 
			// mt_description
			// 
			this.mt_description.Location = new System.Drawing.Point(8, 45);
			this.mt_description.Name = "mt_description";
			this.mt_description.Size = new System.Drawing.Size(104, 16);
			this.mt_description.TabIndex = 24;
			this.mt_description.Text = "Description";
			this.mt_description.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_description.ViewStyle
			// 
			// 
			// m_pMembers
			// 
			this.m_pMembers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_pMembers.Location = new System.Drawing.Point(8, 133);
			this.m_pMembers.Name = "m_pMembers";
			this.m_pMembers.Size = new System.Drawing.Size(264, 210);
			this.m_pMembers.TabIndex = 21;
			// 
			// m_pIsPublic
			// 
			this.m_pIsPublic.Checked = false;
			this.m_pIsPublic.DrawBorder = true;
			this.m_pIsPublic.Location = new System.Drawing.Point(296, 88);
			this.m_pIsPublic.Name = "m_pIsPublic";
			this.m_pIsPublic.ReadOnly = false;
			this.m_pIsPublic.Size = new System.Drawing.Size(16, 16);
			this.m_pIsPublic.TabIndex = 26;
			this.m_pIsPublic.UseStaticViewStyle = true;
			// 
			// m_pIsPublic.ViewStyle
			// 
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(312, 89);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(48, 15);
			this.wLabel1.TabIndex = 27;
			this.wLabel1.Text = "Is Public";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// wfrm_Alias
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(368, 349);
			this.Controls.Add(this.wLabel1);
			this.Controls.Add(this.m_pIsPublic);
			this.Controls.Add(this.m_pCancel);
			this.Controls.Add(this.mt_member);
			this.Controls.Add(this.m_pMember);
			this.Controls.Add(this.m_pOk);
			this.Controls.Add(this.m_pRemove);
			this.Controls.Add(this.m_pAdd);
			this.Controls.Add(this.m_pDomains);
			this.Controls.Add(this.mt_symbolAT);
			this.Controls.Add(this.m_pName);
			this.Controls.Add(this.m_pDescription);
			this.Controls.Add(this.mt_name);
			this.Controls.Add(this.mt_description);
			this.Controls.Add(this.m_pMembers);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "wfrm_Alias";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Alias";
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		#region function m_pAdd_Click

		private void m_pAdd_Click(object sender, System.EventArgs e)
		{
			string member = m_pMember.Text;

			if(member.Length == 0){
				MessageBox.Show("Member name cannot be empty!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
				m_pMember.FlashControl();
				return;
			}

			if(member.IndexOf("<") == -1){
				member = "<" + member;
			}
			if(member.IndexOf(">") == -1){
				member += ">";
			}


			m_pMembers.Items.Add(member);

			m_pMember.Text = "";
			m_pMember.Focus();

			m_pDomains.Enabled = false;
		}

		#endregion

		#region function m_pRemove_Click

		private void m_pRemove_Click(object sender, System.EventArgs e)
		{
			if(m_pMembers.SelectedIndex > -1){
				m_pMembers.Items.RemoveAt(m_pMembers.SelectedIndex);
			
				if(m_pMembers.Items.Count == 0){
					m_pDomains.Enabled = true;
				}
			}
		}

		#endregion

		#region function m_pOk_Click

		private void m_pOk_Click(object sender, System.EventArgs e)
		{
			if(m_pName.Text.Length <= 0){
				MessageBox.Show("AliasName name cannot be empty!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
				m_pName.FlashControl();
				return;
			}

			if(m_pMembers.Items.Count == 0){
				MessageBox.Show("Please add at least one member!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
				return;
			}

			try
			{				
				string member = "";
				foreach(string str in m_pMembers.Items){
					member += str + ";";
				}			
				// remove ";" from end
				member = member.Substring(0,member.Length-1);
				
				m_AliasName   = m_pName.Text + "@" + m_pDomains.Text;
				m_Descriprion = m_pDescription.Text;
				m_Members     = member;
				m_DomainID    = m_pDomains.SelectedItem.Tag.ToString();
				m_IsPublic    = m_pIsPublic.Checked;
			}
			catch(Exception x)
			{
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
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

		#endregion


		#region Properties Implementation

		public string AliasName
		{
			get{ return m_AliasName; }
		}

		public string Descriprion
		{
			get{ return m_Descriprion; }
		}

		public string Members
		{
			get{ return m_Members; }
		}

		public string DomainID
		{
			get{ return m_DomainID; }
		}

		public bool IsPublic
		{
			get{ return m_IsPublic; }
		}

		#endregion

	}
}
