using System;
using System.IO;
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
	/// Summary description for wfrm_User_Pop3Rem.
	/// </summary>
	public class wfrm_User_Pop3Rem : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WLabel wLabel3;
		private LumiSoft.UI.Controls.WLabel wLabel2;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		private System.Windows.Forms.DataGrid grid;
		private LumiSoft.UI.Controls.WButton m_pAdd;
		private LumiSoft.UI.Controls.WEditBox n_pPassword;
		private LumiSoft.UI.Controls.WEditBox m_pUserName;
		private LumiSoft.UI.Controls.WEditBox m_pServer;
		private LumiSoft.UI.Controls.WSpinEdit m_pPort;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private LumiSoft.UI.Controls.WButton m_pDelete;

		private DataSet ds = null;

		public wfrm_User_Pop3Rem(ServerAPI serverAPI,string userName)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			InitGrid();

			ds = serverAPI.GetUserRemotePop3Servers(userName);
			grid.DataSource = ds.Tables["RemotePop3Servers"].DefaultView;
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
			this.wLabel3 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel2 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.m_pAdd = new LumiSoft.UI.Controls.WButton();
			this.grid = new System.Windows.Forms.DataGrid();
			this.n_pPassword = new LumiSoft.UI.Controls.WEditBox();
			this.m_pUserName = new LumiSoft.UI.Controls.WEditBox();
			this.m_pPort = new LumiSoft.UI.Controls.WSpinEdit();
			this.m_pServer = new LumiSoft.UI.Controls.WEditBox();
			this.m_pDelete = new LumiSoft.UI.Controls.WButton();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.SuspendLayout();
			// 
			// wLabel3
			// 
			this.wLabel3.Location = new System.Drawing.Point(10, 72);
			this.wLabel3.Name = "wLabel3";
			this.wLabel3.Size = new System.Drawing.Size(72, 24);
			this.wLabel3.TabIndex = 19;
			this.wLabel3.Text = "Password";
			this.wLabel3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel3.ViewStyle
			// 
			// 
			// wLabel2
			// 
			this.wLabel2.Location = new System.Drawing.Point(10, 48);
			this.wLabel2.Name = "wLabel2";
			this.wLabel2.Size = new System.Drawing.Size(72, 24);
			this.wLabel2.TabIndex = 18;
			this.wLabel2.Text = "Username";
			this.wLabel2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel2.ViewStyle
			// 
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(10, 24);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(72, 24);
			this.wLabel1.TabIndex = 17;
			this.wLabel1.Text = "Server";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// m_pAdd
			// 
			this.m_pAdd.Location = new System.Drawing.Point(218, 72);
			this.m_pAdd.Name = "m_pAdd";
			this.m_pAdd.Size = new System.Drawing.Size(56, 24);
			this.m_pAdd.TabIndex = 15;
			this.m_pAdd.Text = "Add";
			// 
			// m_pAdd.ViewStyle
			// 
			this.m_pAdd.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pAdd_ButtonPressed);
			// 
			// grid
			// 
			this.grid.DataMember = "";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.Location = new System.Drawing.Point(8, 112);
			this.grid.Name = "grid";
			this.grid.Size = new System.Drawing.Size(352, 184);
			this.grid.TabIndex = 14;
			// 
			// n_pPassword
			// 
			this.n_pPassword.Location = new System.Drawing.Point(82, 72);
			this.n_pPassword.Name = "n_pPassword";
			this.n_pPassword.Size = new System.Drawing.Size(120, 20);
			this.n_pPassword.TabIndex = 13;
			// 
			// wEditBox3.ViewStyle
			// 
			// 
			// m_pUserName
			// 
			this.m_pUserName.Location = new System.Drawing.Point(82, 48);
			this.m_pUserName.Name = "m_pUserName";
			this.m_pUserName.Size = new System.Drawing.Size(120, 20);
			this.m_pUserName.TabIndex = 12;
			// 
			// wEditBox2.ViewStyle
			// 
			// 
			// m_pPort
			// 
			this.m_pPort.BackColor = System.Drawing.Color.White;
			this.m_pPort.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pPort.DecimalPlaces = 0;
			this.m_pPort.DecMaxValue = new System.Decimal(new int[] {
																		999999999,
																		0,
																		0,
																		0});
			this.m_pPort.DecMinValue = new System.Decimal(new int[] {
																		1,
																		0,
																		0,
																		0});
			this.m_pPort.DecValue = new System.Decimal(new int[] {
																	 110,
																	 0,
																	 0,
																	 0});
			this.m_pPort.Location = new System.Drawing.Point(210, 24);
			this.m_pPort.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pPort.MaxLength = 32767;
			this.m_pPort.Name = "m_pPort";
			this.m_pPort.ReadOnly = false;
			this.m_pPort.Size = new System.Drawing.Size(48, 20);
			this.m_pPort.TabIndex = 11;
			this.m_pPort.Text = "110";
			this.m_pPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pPort.UseStaticViewStyle = false;
			// 
			// wSpinEdit1.ViewStyle
			// 
			// 
			// m_pServer
			// 
			this.m_pServer.Location = new System.Drawing.Point(82, 24);
			this.m_pServer.Name = "m_pServer";
			this.m_pServer.Size = new System.Drawing.Size(120, 20);
			this.m_pServer.TabIndex = 10;
			// 
			// wEditBox1.ViewStyle
			// 
			// 
			// m_pDelete
			// 
			this.m_pDelete.Location = new System.Drawing.Point(288, 72);
			this.m_pDelete.Name = "m_pDelete";
			this.m_pDelete.Size = new System.Drawing.Size(72, 24);
			this.m_pDelete.TabIndex = 20;
			this.m_pDelete.Text = "Delete";
			// 
			// m_pDelete.ViewStyle
			// 
			this.m_pDelete.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pDelete_ButtonPressed);
			// 
			// wfrm_User_Pop3Rem
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(376, 309);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_pDelete,
																		  this.wLabel3,
																		  this.wLabel2,
																		  this.wLabel1,
																		  this.m_pAdd,
																		  this.grid,
																		  this.n_pPassword,
																		  this.m_pUserName,
																		  this.m_pPort,
																		  this.m_pServer});
			this.Name = "wfrm_User_Pop3Rem";
			this.Text = "wfrm_User_Pop3Rem";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		private void m_pAdd_ButtonPressed(object sender, System.EventArgs e)
		{
			if(m_pServer.Text.Length < 1){
				m_pServer.FlashControl();
				return;
			}

			if(m_pUserName.Text.Length < 1){
				m_pUserName.FlashControl();
				return;
			}

			DataRow dr = ds.Tables["RemotePop3Servers"].NewRow();
			dr["Server"]   = m_pServer.Text;
			dr["Port"]     = (int)m_pPort.DecValue;
			dr["UserName"] = m_pUserName.Text;
			dr["Password"] = n_pPassword.Text;

			ds.Tables["RemotePop3Servers"].Rows.Add(dr);			
		}

		private void m_pDelete_ButtonPressed(object sender, System.EventArgs e)
		{
			DataRow dr = ((DataView)(grid.DataSource))[grid.CurrentRowIndex].Row;
			if(MessageBox.Show(this,"Warning: Deleting remote account '" + dr["Server"].ToString() + "/" + dr["UserName"].ToString() + "' !!!\nDo you want to continue?","Delete confirmation",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2) == DialogResult.Yes){
				dr.Delete();
			}
		}

		#endregion


		#region Grid Init stuff

		private void InitGrid()
		{							
			grid.ReadOnly = true;

			DataGridTableStyle ts1 = new DataGridTableStyle();
			ts1.MappingName = "RemotePop3Servers";			

			DataGridNoActiveCellColumn TextCol = new DataGridNoActiveCellColumn();
			TextCol.MappingName = "Server";
			TextCol.HeaderText = "Pop3 Server";
			TextCol.Width = 160;
			ts1.GridColumnStyles.Add(TextCol);

			DataGridNoActiveCellColumn TextCol2 = new DataGridNoActiveCellColumn();
			TextCol2.MappingName = "Port";
			TextCol2.HeaderText = "Port";
			TextCol2.Width = 40;
			ts1.GridColumnStyles.Add(TextCol2);

			DataGridNoActiveCellColumn TextCol3 = new DataGridNoActiveCellColumn();
			TextCol3.MappingName = "UserName";
			TextCol3.HeaderText = "UserName";
			TextCol3.Width = 80;
			ts1.GridColumnStyles.Add(TextCol3);      				
						
			DataGridNoActiveCellColumn TextCol4 = new DataGridNoActiveCellColumn();
			TextCol4.MappingName = "Password";
			TextCol4.HeaderText = "Password";
			TextCol4.Width = 80;
			ts1.GridColumnStyles.Add(TextCol4);

			grid.TableStyles.Add(ts1);

		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets byte(DataSet) with remote accounts.
		/// </summary>
		public byte[] wp_RemoteAccounts
		{
			get{
				if(ds.Tables["RemotePop3Servers"].Rows.Count > 0){
					using(MemoryStream mStrm = new MemoryStream()){
						ds.WriteXml(mStrm,XmlWriteMode.IgnoreSchema);
						return mStrm.ToArray();
					}
				}
				else{
					return null;
				}
			}
		}

		#endregion
				
	}
}
