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
	/// Summary description for wfrm_Users.
	/// </summary>
	public class wfrm_Users : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WComboBox m_pDomains;
		private LumiSoft.UI.Controls.WLabel mt_domain;
		private LumiSoft.UI.Controls.WDataGrid grid;
		private LumiSoft.UI.Controls.WToolBar wToolBar1;
		private System.Windows.Forms.ToolBarButton toolBarButton_Add;
		private System.Windows.Forms.ToolBarButton toolBarButton_Delete;
		private System.Windows.Forms.ToolBarButton toolBarButton_Edit;
		private System.Windows.Forms.ImageList imgToolbar;
		private System.ComponentModel.IContainer components;

		private ServerAPI m_ServerAPI = null;
		private DataView  m_DvUsers   = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="serverAPI"></param>
		/// <param name="frame"></param>
		public wfrm_Users(ServerAPI serverAPI,WFrame frame)
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

				//---- Toolbar stuff
				frame.Frame_ToolBar = wToolBar1;

				InitGrid();
				RefreshForm();
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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(wfrm_Users));
			this.m_pDomains = new LumiSoft.UI.Controls.WComboBox();
			this.mt_domain = new LumiSoft.UI.Controls.WLabel();
			this.grid = new LumiSoft.UI.Controls.WDataGrid();
			this.wToolBar1 = new LumiSoft.UI.Controls.WToolBar();
			this.toolBarButton_Add = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton_Delete = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton_Edit = new System.Windows.Forms.ToolBarButton();
			this.imgToolbar = new System.Windows.Forms.ImageList(this.components);
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.SuspendLayout();
			// 
			// m_pDomains
			// 
			this.m_pDomains.DropDownWidth = 168;
			this.m_pDomains.Location = new System.Drawing.Point(96, 24);
			this.m_pDomains.Name = "m_pDomains";
			this.m_pDomains.EditStyle = LumiSoft.UI.Controls.EditStyle.Selectable;
			this.m_pDomains.SelectedIndex = -1;
			this.m_pDomains.Size = new System.Drawing.Size(168, 20);
			this.m_pDomains.TabIndex = 7;
			this.m_pDomains.UseStaticViewStyle = true;
			// 
			// m_pDomains.ViewStyle
			// 
			this.m_pDomains.VisibleItems = 5;
			this.m_pDomains.SelectedIndexChanged += new System.EventHandler(this.m_pDomains_SelectedIndexChanged);
			// 
			// mt_domain
			// 
			this.mt_domain.Location = new System.Drawing.Point(16, 24);
			this.mt_domain.Name = "mt_domain";
			this.mt_domain.Size = new System.Drawing.Size(80, 24);
			this.mt_domain.TabIndex = 11;
			this.mt_domain.Text = "Domain";
			this.mt_domain.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_domain.ViewStyle
			// 
			// 
			// grid
			// 
			this.grid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.grid.CaptionVisible = false;
			this.grid.DataMember = "";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.Location = new System.Drawing.Point(8, 56);
			this.grid.Name = "grid";
			this.grid.Size = new System.Drawing.Size(456, 288);
			this.grid.TabIndex = 6;
			// 
			// wToolBar1
			// 
			this.wToolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.wToolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						 this.toolBarButton_Add,
																						 this.toolBarButton_Delete,
																						 this.toolBarButton_Edit});
			this.wToolBar1.Divider = false;
			this.wToolBar1.DropDownArrows = true;
			this.wToolBar1.ImageList = this.imgToolbar;
			this.wToolBar1.Name = "wToolBar1";
			this.wToolBar1.ShowToolTips = true;
			this.wToolBar1.Size = new System.Drawing.Size(472, 23);
			this.wToolBar1.TabIndex = 12;
			this.wToolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.wToolBar1_ButtonClick);
			// 
			// toolBarButton_Add
			// 
			this.toolBarButton_Add.ImageIndex = 0;
			// 
			// toolBarButton_Delete
			// 
			this.toolBarButton_Delete.ImageIndex = 2;
			// 
			// toolBarButton_Edit
			// 
			this.toolBarButton_Edit.ImageIndex = 1;
			// 
			// imgToolbar
			// 
			this.imgToolbar.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imgToolbar.ImageSize = new System.Drawing.Size(16, 16);
			this.imgToolbar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgToolbar.ImageStream")));
			this.imgToolbar.TransparentColor = System.Drawing.Color.Empty;
			// 
			// wfrm_Users
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 357);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.wToolBar1,
																		  this.m_pDomains,
																		  this.mt_domain,
																		  this.grid});
			this.Name = "wfrm_Users";
			this.Text = "wfrm_Users";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling
		
		#region function m_pDomains_SelectedIndexChanged

		private void m_pDomains_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(m_DvUsers != null){
				if(m_pDomains.SelectedItem.ToString() == "ALL"){				
					m_DvUsers.RowFilter = "";
				}
				else{
					m_DvUsers.RowFilter = "DomainID='" + m_pDomains.SelectedItem.Tag.ToString() + "'";
				}
			}
		}

		#endregion
		
		#region function wToolBar1_ButtonClick

		private void wToolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			//--- Add new
			if(e.Button.Equals(toolBarButton_Add)){
				if(m_pDomains.Items.Count > 1){
					wfrm_User_Frame frm = new wfrm_User_Frame(m_ServerAPI,m_pDomains.SelectedItem.Tag.ToString());
					if(frm.ShowDialog(this) == DialogResult.OK){

						DataRow dr = m_ServerAPI.AddUser(frm.FullName,frm.UserName,frm.Password,frm.Description,frm.Emails,frm.DomainID,frm.MailboxSize,frm.UserEnabled,frm.AllowRelay,frm.wp_RemoteAccounts);
						if(dr == null){
							MessageBox.Show("Error adding user!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
							return;
						}

						m_DvUsers.Table.ImportRow(dr);

						UpdateButtons();
					}
				}
				else{
					MessageBox.Show("Please open Emails domain before!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
				}

				return;
			}

			//--- Delete
			if(e.Button.Equals(toolBarButton_Delete)){
				try
				{
					if(MessageBox.Show(this,"Warning: Deleting user!!!\nDo you want to continue?","Delete confirmation",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2) == DialogResult.Yes)
					{
						DataRow dr = ((DataView)(grid.DataSource))[grid.CurrentRowIndex].Row;
						if(dr != null){
							m_ServerAPI.DeleteUser(dr["UserID"].ToString());
							dr.Delete();
						}
						
						UpdateButtons();
					}
				}
				catch(Exception x)
				{
					wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
					frm.ShowDialog(this);
				}

				return;
			}

			//--- Edit
			if(e.Button.Equals(toolBarButton_Edit)){
				try
				{
					DataRow dr = ((DataView)(grid.DataSource))[grid.CurrentRowIndex].Row;

					if(dr != null){
						wfrm_User_Frame frm = new wfrm_User_Frame(m_ServerAPI,dr);
						if(frm.ShowDialog(this) == DialogResult.OK){
							
							m_ServerAPI.UpdateUser(dr["UserID"].ToString(),frm.FullName,frm.Password,frm.Description,frm.Emails,frm.DomainID,frm.MailboxSize,frm.UserEnabled,frm.AllowRelay,frm.wp_RemoteAccounts);

							dr["FULLNAME"]     = frm.FullName;
							dr["USERNAME"]     = frm.UserName;
							dr["PASSWORD"]     = frm.Password;
							dr["Description"]  = frm.Description;
							dr["Emails"]       = frm.Emails;
							dr["DomainID"]     = frm.DomainID;
							dr["Mailbox_Size"] = frm.MailboxSize;
							dr["Enabled"]      = frm.UserEnabled;
							dr["AllowRelay"]   = frm.AllowRelay;

							if(frm.wp_RemoteAccounts != null){
								dr["RemotePop3Servers"] = frm.wp_RemoteAccounts;
							}
							else{
								dr["RemotePop3Servers"] = System.DBNull.Value;
							}
						}
					}
				}
				catch(Exception x)
				{
					wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
					frm.ShowDialog(this);
				}

				return;
			}
		}

		#endregion

		#endregion


		#region function Fill_Combo

		private void Fill_Combo()
		{
			m_pDomains.Items.Clear();

			m_pDomains.Items.Add("ALL","ALL");

			DataView dvDomains = m_ServerAPI.GetDomainList();
			foreach(DataRowView vDr in dvDomains){
				m_pDomains.Items.Add(vDr["DomainName"].ToString(),vDr["DomainID"].ToString());
			}
			
			if(m_pDomains.Items.Count > 0){
				m_pDomains.SelectedIndex = 0;
			}
		}

		#endregion


		#region function UpdateButtons

		private void UpdateButtons()
		{
			int rowCount = ((DataView)(grid.DataSource)).Count;

			if(rowCount > 0){
				toolBarButton_Edit.Enabled   = true;
				toolBarButton_Delete.Enabled = true;
			}
			else{
				toolBarButton_Edit.Enabled   = false;
				toolBarButton_Delete.Enabled = false;
			}
		}

		#endregion

		#region function RefreshForm

		public void RefreshForm()
		{
			Fill_Combo();

			m_DvUsers = m_ServerAPI.GetUserList("ALL");
			grid.DataSource = m_DvUsers;

			UpdateButtons();
		}

		#endregion

		#region Grid Init stuff

		private void InitGrid()
		{							
			grid.ReadOnly = true;

			DataGridTableStyle ts1 = new DataGridTableStyle();
			ts1.MappingName = "Users";			

			DataGridNoActiveCellColumn TextCol2 = new DataGridNoActiveCellColumn();
			TextCol2.MappingName = "UserName";
			TextCol2.HeaderText = "Mailbox";
			TextCol2.Width = 145;
			ts1.GridColumnStyles.Add(TextCol2);
      					
			DataGridNoActiveCellColumn TextCol = new DataGridNoActiveCellColumn();
			TextCol.MappingName = "Emails";
			TextCol.HeaderText = "E-mail Address";
			TextCol.Width = 400;
			ts1.GridColumnStyles.Add(TextCol);
			
			DataGridNoActiveCellColumn TextCol3 = new DataGridNoActiveCellColumn();
			TextCol3.MappingName = "Description";
			TextCol3.HeaderText = "Description";
			TextCol3.Width = 250;
			ts1.GridColumnStyles.Add(TextCol3);

			grid.TableStyles.Add(ts1);

		}

		#endregion
		
	}
}
