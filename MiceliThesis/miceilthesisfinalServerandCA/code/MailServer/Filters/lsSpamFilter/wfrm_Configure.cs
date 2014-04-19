using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace lsSpamFilter
{
	/// <summary>
	/// Summary description for wfrm_Configure.
	/// </summary>
	public class wfrm_Configure : System.Windows.Forms.Form
	{
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.DataGrid dataGrid2;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		private LumiSoft.UI.Controls.WLabel wLabel2;
		private LumiSoft.UI.Controls.WButton m_pSave;
		private System.Windows.Forms.Label label1;
		private LumiSoft.UI.Controls.WButton m_pCalculate;
		private LumiSoft.UI.Controls.WEditBox m_pMd5Value;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private DataSet ds = null;

		public wfrm_Configure()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			ds = new DataSet();
			DataTable dt = ds.Tables.Add("KewWords");
			dt.Columns.Add("Cost",typeof(int));
			dt.Columns.Add("KeyWord");
			
			dt = ds.Tables.Add("ContentMd5");
			dt.Columns.Add("Description");
			dt.Columns.Add("EntryMd5Value");
			ds.ReadXml(Application.StartupPath + "\\lsSpam_db.xml");

			InitGrid();

			dataGrid1.DataSource = ds.Tables["KewWords"];
			dataGrid2.DataSource = ds.Tables["ContentMd5"];
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
			this.dataGrid1 = new System.Windows.Forms.DataGrid();
			this.dataGrid2 = new System.Windows.Forms.DataGrid();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel2 = new LumiSoft.UI.Controls.WLabel();
			this.m_pSave = new LumiSoft.UI.Controls.WButton();
			this.label1 = new System.Windows.Forms.Label();
			this.m_pCalculate = new LumiSoft.UI.Controls.WButton();
			this.m_pMd5Value = new LumiSoft.UI.Controls.WEditBox();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid2)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGrid1
			// 
			this.dataGrid1.CaptionVisible = false;
			this.dataGrid1.DataMember = "";
			this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid1.Location = new System.Drawing.Point(16, 24);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.Size = new System.Drawing.Size(448, 120);
			this.dataGrid1.TabIndex = 0;
			// 
			// dataGrid2
			// 
			this.dataGrid2.CaptionVisible = false;
			this.dataGrid2.DataMember = "";
			this.dataGrid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid2.Location = new System.Drawing.Point(16, 200);
			this.dataGrid2.Name = "dataGrid2";
			this.dataGrid2.Size = new System.Drawing.Size(448, 104);
			this.dataGrid2.TabIndex = 1;
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(16, 184);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(168, 16);
			this.wLabel1.TabIndex = 2;
			this.wLabel1.Text = "Attachments blocking";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// wLabel2
			// 
			this.wLabel2.Location = new System.Drawing.Point(16, 8);
			this.wLabel2.Name = "wLabel2";
			this.wLabel2.Size = new System.Drawing.Size(168, 16);
			this.wLabel2.TabIndex = 3;
			this.wLabel2.Text = "Keywords blocking";
			this.wLabel2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel2.ViewStyle
			// 
			// 
			// m_pSave
			// 
			this.m_pSave.DrawBorder = true;
			this.m_pSave.Location = new System.Drawing.Point(376, 328);
			this.m_pSave.Name = "m_pSave";
			this.m_pSave.Size = new System.Drawing.Size(88, 24);
			this.m_pSave.TabIndex = 4;
			this.m_pSave.Text = "Save";
			this.m_pSave.UseStaticViewStyle = true;
			// 
			// m_pSave.ViewStyle
			// 
			this.m_pSave.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pSave_ButtonPressed);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 328);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(112, 16);
			this.label1.TabIndex = 5;
			this.label1.Text = "Calculate entry md5 value";
			// 
			// m_pCalculate
			// 
			this.m_pCalculate.DrawBorder = true;
			this.m_pCalculate.Location = new System.Drawing.Point(128, 325);
			this.m_pCalculate.Name = "m_pCalculate";
			this.m_pCalculate.Size = new System.Drawing.Size(48, 24);
			this.m_pCalculate.TabIndex = 6;
			this.m_pCalculate.Text = "Calc";
			this.m_pCalculate.UseStaticViewStyle = true;
			// 
			// m_pCalculate.ViewStyle
			// 
			this.m_pCalculate.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pCalculate_ButtonPressed);
			// 
			// m_pMd5Value
			// 
			this.m_pMd5Value.AutoSize = true;
			this.m_pMd5Value.DrawBorder = true;
			this.m_pMd5Value.Location = new System.Drawing.Point(184, 328);
			this.m_pMd5Value.Name = "m_pMd5Value";
			this.m_pMd5Value.Size = new System.Drawing.Size(184, 20);
			this.m_pMd5Value.TabIndex = 7;
			this.m_pMd5Value.UseStaticViewStyle = true;
			// 
			// m_pMd5Value.ViewStyle
			// 
			// 
			// wfrm_Configure
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(480, 357);
			this.Controls.Add(this.m_pMd5Value);
			this.Controls.Add(this.m_pCalculate);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.m_pSave);
			this.Controls.Add(this.wLabel2);
			this.Controls.Add(this.wLabel1);
			this.Controls.Add(this.dataGrid2);
			this.Controls.Add(this.dataGrid1);
			this.Name = "wfrm_Configure";
			this.Text = "Configure";
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid2)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		private void m_pSave_ButtonPressed(object sender, System.EventArgs e)
		{
			foreach(DataTable dt in ds.Tables){
				foreach(DataRow dr in dt.Rows){
					foreach(DataColumn dc in dt.Columns){
						if(dr.IsNull(dc)){
							MessageBox.Show(this,"Table '" + dt.TableName + "' Can't contain null values !!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
							return;
						}
					}
				}
			}

			ds.WriteXml(Application.StartupPath + "\\lsSpam_db.xml");
		}

		private void m_pCalculate_ButtonPressed(object sender, System.EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.RestoreDirectory = true;
			if(dlg.ShowDialog() == DialogResult.OK){
				using(FileStream fs = File.OpenRead(dlg.FileName)){
					byte[] data = new byte[fs.Length];
					fs.Read(data,0,data.Length);

					System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
					m_pMd5Value.Text = Convert.ToBase64String(md5.ComputeHash(data));
				}
			}
		}

		#endregion


		#region method InitGrid

		private void InitGrid()
		{
			DataGridTableStyle ts1 = new DataGridTableStyle();
			ts1.MappingName = "KewWords";

			DataGridTextBoxColumn TextCol2 = new DataGridTextBoxColumn();
			TextCol2.MappingName = "Cost";
			TextCol2.HeaderText = "Cost";
			TextCol2.Width = 40;
			ts1.GridColumnStyles.Add(TextCol2);

			DataGridTextBoxColumn TextCol3 = new DataGridTextBoxColumn();
			TextCol3.MappingName = "KeyWord";
			TextCol3.HeaderText = "KeyWord";
			TextCol3.Width = 350;
			ts1.GridColumnStyles.Add(TextCol3);

			dataGrid1.TableStyles.Add(ts1);


			DataGridTableStyle ts2 = new DataGridTableStyle();
			ts2.MappingName = "ContentMd5";

			DataGridTextBoxColumn TextCol4 = new DataGridTextBoxColumn();
			TextCol4.MappingName = "Description";
			TextCol4.HeaderText = "Description";
			TextCol4.Width = 190;
			ts2.GridColumnStyles.Add(TextCol4);

			DataGridTextBoxColumn TextCol5 = new DataGridTextBoxColumn();
			TextCol5.MappingName = "EntryMd5Value";
			TextCol5.HeaderText = "EntryMd5Value";
			TextCol5.Width = 200;
			ts2.GridColumnStyles.Add(TextCol5);

			dataGrid2.TableStyles.Add(ts2);
		}

		#endregion
		
	}
}
