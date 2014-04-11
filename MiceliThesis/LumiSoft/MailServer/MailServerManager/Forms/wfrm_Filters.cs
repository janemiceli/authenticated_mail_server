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
	/// Summary description for wfrm_Filters.
	/// </summary>
	public class wfrm_Filters : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ImageList imgToolbar;
		private LumiSoft.UI.Controls.WToolBar wToolBar1;
		private System.Windows.Forms.ToolBarButton toolBarButton_Add;
		private System.Windows.Forms.ToolBarButton toolBarButton_Delete;
		private System.Windows.Forms.ToolBarButton toolBarButton_Edit;
		private LumiSoft.UI.Controls.WDataGrid grid;
		private System.ComponentModel.IContainer components;

		private ServerAPI m_ServerAPI = null;
		private DataView  m_DV        = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="serverAPI"></param>
		/// <param name="frame"></param>
		public wfrm_Filters(ServerAPI serverAPI,WFrame frame)
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

			m_DV = serverAPI.GetFilterList();
			grid.DataSource = m_DV;
		}

		#region method Dispose

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(wfrm_Filters));
			this.imgToolbar = new System.Windows.Forms.ImageList(this.components);
			this.wToolBar1 = new LumiSoft.UI.Controls.WToolBar();
			this.toolBarButton_Add = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton_Delete = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton_Edit = new System.Windows.Forms.ToolBarButton();
			this.grid = new LumiSoft.UI.Controls.WDataGrid();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.SuspendLayout();
			// 
			// imgToolbar
			// 
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
			this.wToolBar1.Location = new System.Drawing.Point(0, 0);
			this.wToolBar1.Name = "wToolBar1";
			this.wToolBar1.ShowToolTips = true;
			this.wToolBar1.Size = new System.Drawing.Size(472, 26);
			this.wToolBar1.TabIndex = 22;
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
			// grid
			// 
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grid.CaptionVisible = false;
			this.grid.DataMember = "";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.Location = new System.Drawing.Point(8, 62);
			this.grid.Name = "grid";
			this.grid.Size = new System.Drawing.Size(456, 288);
			this.grid.TabIndex = 21;
			// 
			// wfrm_Filters
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 357);
			this.Controls.Add(this.grid);
			this.Controls.Add(this.wToolBar1);
			this.Name = "wfrm_Filters";
			this.Text = "Filters";
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
					
					wfrm_Filter frm = new wfrm_Filter(m_ServerAPI);
					if(frm.ShowDialog(this) == DialogResult.OK){

						DataRow dr = m_ServerAPI.AddFilter(frm.wp_Description,frm.wp_Assembly,frm.wp_Class,frm.wp_Cost,frm.wp_Enabled);
						if(dr == null){
							MessageBox.Show("Error filter alias!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
							return;
						}

						m_DV.Table.ImportRow(dr);					
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
					if(MessageBox.Show(this,"Warning: Deleting filter!!!\nDo you want to continue?","Delete confirmation",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2) == DialogResult.Yes)
					{
						DataRow dr = ((DataView)(grid.DataSource))[grid.CurrentRowIndex].Row;
						if(dr != null){
							m_ServerAPI.DeleteFilter(dr["FilterID"].ToString());
							dr.Delete();
						}
						
				//		UpdateButtons();
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
						wfrm_Filter frm = new wfrm_Filter(m_ServerAPI,dr);
						if(frm.ShowDialog(this) == DialogResult.OK){
							m_ServerAPI.UpdateFilter(dr["FilterID"].ToString(),frm.wp_Description,frm.wp_Assembly,frm.wp_Class,frm.wp_Cost,frm.wp_Enabled);

							dr["Description"] = frm.wp_Description;
							dr["Assembly"]    = frm.wp_Assembly;
							dr["ClassName"]   = frm.wp_Class;
							dr["Cost"]        = frm.wp_Cost;
							dr["Enabled"]     = frm.wp_Enabled;
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
		

		#region Grid Init stuff

		private void InitGrid()
		{			
			grid.ReadOnly = true;

			DataGridTableStyle ts1 = new DataGridTableStyle();
			ts1.MappingName = "SmtpFilters";			
      					
			DataGridNoActiveCellColumn TextCol = new DataGridNoActiveCellColumn();
			TextCol.MappingName = "Cost";
			TextCol.HeaderText = "Cost";
			TextCol.Width = 50;
			ts1.GridColumnStyles.Add(TextCol);

			DataGridNoActiveCellColumn TextCol2 = new DataGridNoActiveCellColumn();
			TextCol2.MappingName = "Description";
			TextCol2.HeaderText = "Description";
			TextCol2.Width = 150;
			ts1.GridColumnStyles.Add(TextCol2);

			DataGridNoActiveCellColumn TextCol3 = new DataGridNoActiveCellColumn();
			TextCol3.MappingName = "Assembly";
			TextCol3.HeaderText = "Assembly";
			TextCol3.Width = 150;
			ts1.GridColumnStyles.Add(TextCol3);

			DataGridNoActiveCellColumn TextCol4 = new DataGridNoActiveCellColumn();
			TextCol4.MappingName = "ClassName";
			TextCol4.HeaderText = "ClassName";
			TextCol4.Width = 225;
			ts1.GridColumnStyles.Add(TextCol4);

			ts1.AllowSorting = false;
			grid.TableStyles.Add(ts1);
		}

		#endregion
	}
}
