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
	/// Summary description for wfrm_Security.
	/// </summary>
	public class wfrm_Security : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WDataGrid grid;
		private System.Windows.Forms.ImageList imgToolbar;
		private LumiSoft.UI.Controls.WToolBar wToolBar1;
		private System.Windows.Forms.ToolBarButton toolBarButton_Add;
		private System.Windows.Forms.ToolBarButton toolBarButton_Delete;
		private System.Windows.Forms.ToolBarButton toolBarButton_Edit;
		private System.ComponentModel.IContainer components;

		private ServerAPI m_ServerAPI = null;
		private DataView  m_DvSec     = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="serverAPI"></param>
		public wfrm_Security(ServerAPI serverAPI,WFrame frame)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			m_ServerAPI = serverAPI;

			//---- Toolbar stuff
			frame.Frame_ToolBar = wToolBar1;

			InitGrid();

			m_DvSec = serverAPI.GetSecurityList();
			grid.DataSource = m_DvSec;

			UpdateButtons();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(wfrm_Security));
			this.grid = new LumiSoft.UI.Controls.WDataGrid();
			this.imgToolbar = new System.Windows.Forms.ImageList(this.components);
			this.wToolBar1 = new LumiSoft.UI.Controls.WToolBar();
			this.toolBarButton_Add = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton_Delete = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton_Edit = new System.Windows.Forms.ToolBarButton();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.grid.CaptionVisible = false;
			this.grid.DataMember = "";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.Location = new System.Drawing.Point(8, 24);
			this.grid.Name = "grid";
			this.grid.Size = new System.Drawing.Size(456, 320);
			this.grid.TabIndex = 4;
			// 
			// imgToolbar
			// 
			this.imgToolbar.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imgToolbar.ImageSize = new System.Drawing.Size(16, 16);
			this.imgToolbar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgToolbar.ImageStream")));
			this.imgToolbar.TransparentColor = System.Drawing.Color.Empty;
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
			this.wToolBar1.TabIndex = 13;
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
			// wfrm_Security
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 357);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.wToolBar1,
																		  this.grid});
			this.Name = "wfrm_Security";
			this.Text = "wfrm_Security";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling
		
		#region function wToolBar1_ButtonClick

		private void wToolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			//--- Add new
			if(e.Button.Equals(toolBarButton_Add)){
				try
				{
					wfrm_SecurityEntry frm = new wfrm_SecurityEntry();
					if(frm.ShowDialog(this) == DialogResult.OK){

						DataRow dr = m_ServerAPI.AddSecurityEntry(frm.wp_Description,frm.wp_Protocol,frm.wp_Type,frm.wp_Action,frm.wp_Content,frm.wp_StartIP,frm.wp_EndIP);
						if(dr == null){
							MessageBox.Show("Error updating alias!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
							return;
						}

						m_DvSec.Table.ImportRow(dr);
						
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

			//--- Delete
			if(e.Button.Equals(toolBarButton_Delete)){
				try
				{
					if(MessageBox.Show(this,"Warning: Deleting security entry!!!\nDo you want to continue?","Delete confirmation",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2) == DialogResult.Yes)
					{
						DataRow dr = ((DataView)(grid.DataSource))[grid.CurrentRowIndex].Row;

						if(dr != null){
							m_ServerAPI.DeleteSecurityEntry(dr["SecurityID"].ToString());
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
						wfrm_SecurityEntry frm = new wfrm_SecurityEntry(dr);
						if(frm.ShowDialog(this) == DialogResult.OK){
							m_ServerAPI.UpdateSecurityEntry(dr["SecurityID"].ToString(),frm.wp_Description,frm.wp_Protocol,frm.wp_Type,frm.wp_Action,frm.wp_Content,frm.wp_StartIP,frm.wp_EndIP);
												
							dr["Description"] = frm.wp_Description;
							dr["Protocol"]    = frm.wp_Protocol;
							dr["Type"]        = frm.wp_Type;
							dr["Action"]      = frm.wp_Action;
							dr["Content"]     = frm.wp_Content;
							dr["StartIP"]     = frm.wp_StartIP;
							dr["EndIP"]       = frm.wp_EndIP;
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


		#region Grid Init stuff

		private void InitGrid()
		{
			grid.ReadOnly = true;

			DataGridTableStyle ts1 = new DataGridTableStyle();
			ts1.MappingName = "Security_List";			
      					
			DataGridNoActiveCellColumn TextCol = new DataGridNoActiveCellColumn();
			TextCol.MappingName = "Description";
			TextCol.HeaderText = "Description";
			TextCol.Width = 180;
			ts1.GridColumnStyles.Add(TextCol);

			DataGridNoActiveCellColumn TextCol2 = new DataGridNoActiveCellColumn();
			TextCol2.MappingName = "Protocol";
			TextCol2.HeaderText = "Protocol";
			TextCol2.Width = 55;
			ts1.GridColumnStyles.Add(TextCol2);

			DataGridNoActiveCellColumn TextCol3 = new DataGridNoActiveCellColumn();
			TextCol3.MappingName = "Type";
			TextCol3.HeaderText = "Type";
			TextCol3.Width = 65;
			ts1.GridColumnStyles.Add(TextCol3);

			DataGridNoActiveCellColumn TextCol4 = new DataGridNoActiveCellColumn();
			TextCol4.MappingName = "Action";
			TextCol4.HeaderText = "Action";
			TextCol4.Width = 65;
			ts1.GridColumnStyles.Add(TextCol4);

			DataGridNoActiveCellColumn TextCol5 = new DataGridNoActiveCellColumn();
			TextCol5.MappingName = "Content";
			TextCol5.HeaderText = "Content";
			TextCol5.Width = 180;
			ts1.GridColumnStyles.Add(TextCol5);

			grid.TableStyles.Add(ts1);

		}

		#endregion

	}
}
