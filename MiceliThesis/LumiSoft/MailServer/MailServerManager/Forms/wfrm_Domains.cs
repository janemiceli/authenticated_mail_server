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
	/// Summary description for wfrm_Domains.
	/// </summary>
	public class wfrm_Domains : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WDataGrid grid;
		private LumiSoft.UI.Controls.WToolBar wToolBar1;
		private System.Windows.Forms.ToolBarButton toolBarButton_Add;
		private System.Windows.Forms.ToolBarButton toolBarButton_Delete;
		private System.Windows.Forms.ImageList imgToolbar;
		private System.ComponentModel.IContainer components;

		private ServerAPI m_ServerAPI    = null;
		private DataView  m_Dv           = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="serverAPI"></param>
		/// <param name="frame"></param>
		public wfrm_Domains(ServerAPI serverAPI,WFrame frame)
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
				
				m_Dv = serverAPI.GetDomainList();
				grid.DataSource = m_Dv;

                UpdateButtons();								
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(wfrm_Domains));
			this.grid = new LumiSoft.UI.Controls.WDataGrid();
			this.wToolBar1 = new LumiSoft.UI.Controls.WToolBar();
			this.toolBarButton_Add = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton_Delete = new System.Windows.Forms.ToolBarButton();
			this.imgToolbar = new System.Windows.Forms.ImageList(this.components);
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
			this.grid.TabIndex = 3;
			// 
			// wToolBar1
			// 
			this.wToolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.wToolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						 this.toolBarButton_Add,
																						 this.toolBarButton_Delete});
			this.wToolBar1.Divider = false;
			this.wToolBar1.DropDownArrows = true;
			this.wToolBar1.ImageList = this.imgToolbar;
			this.wToolBar1.Name = "wToolBar1";
			this.wToolBar1.ShowToolTips = true;
			this.wToolBar1.Size = new System.Drawing.Size(472, 23);
			this.wToolBar1.TabIndex = 6;
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
			// imgToolbar
			// 
			this.imgToolbar.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imgToolbar.ImageSize = new System.Drawing.Size(16, 16);
			this.imgToolbar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgToolbar.ImageStream")));
			this.imgToolbar.TransparentColor = System.Drawing.Color.Empty;
			// 
			// wfrm_Domains
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 357);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.wToolBar1,
																		  this.grid});
			this.Name = "wfrm_Domains";
			this.Text = "wfrm_Domains";
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
				wfrm_Domain frm = new wfrm_Domain(m_ServerAPI);
				if(frm.ShowDialog(this) == DialogResult.OK){

					DataRow dr = m_ServerAPI.AddDomain(frm.wp_Domain,frm.wp_Description);
					if(dr == null){
						MessageBox.Show("Error creating domain!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
						return;
					}

					m_Dv.Table.ImportRow(dr);

					UpdateButtons();
				}

				return;
			}

			//--- Delete
			if(e.Button.Equals(toolBarButton_Delete)){
				if(MessageBox.Show(this,"Warning: Deleting domain, deletes all domain users and aliases!!!\nDo you want to continue?","Delete confirmation",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2) == DialogResult.Yes)
				{				
					DataRow dr = ((DataView)(grid.DataSource))[grid.CurrentRowIndex].Row;			
					if(dr != null){
						if(!m_ServerAPI.DeleteDomain(dr["DomainID"].ToString())){
							MessageBox.Show("Error deleting domain!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
							return;
						}
						dr.Delete();

						UpdateButtons();
					}				
				}
			}
		}

		#endregion

		#endregion


		#region function UpdateButtons

		private void UpdateButtons()
		{
			int rowCount = ((DataView)(grid.DataSource)).Count;
	
			if(rowCount > 0){
				toolBarButton_Delete.Enabled = true;
			}
			else{
				toolBarButton_Delete.Enabled = false;
			}
		}

		#endregion


		#region Grid Init stuff

		private void InitGrid()
		{	
			grid.ReadOnly = true;

			DataGridTableStyle ts1 = new DataGridTableStyle();
			ts1.MappingName = "Domains";			
      					
			DataGridNoActiveCellColumn TextCol = new DataGridNoActiveCellColumn();
			TextCol.MappingName = "DomainName";
			TextCol.HeaderText = "Domain Name";
			TextCol.Width = 200;
			ts1.GridColumnStyles.Add(TextCol);

			DataGridNoActiveCellColumn TextCol2 = new DataGridNoActiveCellColumn();
			TextCol2.MappingName = "Description";
			TextCol2.HeaderText = "Description";
			TextCol2.Width = 345;
			ts1.GridColumnStyles.Add(TextCol2);

			grid.TableStyles.Add(ts1);
		}

		#endregion
		
	}
}
