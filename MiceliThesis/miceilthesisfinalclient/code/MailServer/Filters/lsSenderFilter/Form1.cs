using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace lsVirusFilter
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Settings : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WEditBox m_pDnsServer;
		private LumiSoft.UI.Controls.WButton m_pSave;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		private LumiSoft.UI.Controls.WLabel wLabel2;
		private LumiSoft.UI.Controls.WButton m_pGetProgram;

		private DataSet ds = null;

		public Settings()
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
				ds = new DataSet();
				DataTable dt = ds.Tables.Add("Settings");
				dt.Columns.Add("DnsServer");
				ds.ReadXml(Application.StartupPath + "\\lsSenderFilter_db.xml");

				m_pDnsServer.Text = ds.Tables["Settings"].Rows[0]["DnsServer"].ToString();
			}
			catch(Exception x){
				MessageBox.Show(this,x.Message + "\r\n" + x.StackTrace,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
			}
		}

		#region method Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			this.m_pDnsServer = new LumiSoft.UI.Controls.WEditBox();
			this.m_pSave = new LumiSoft.UI.Controls.WButton();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel2 = new LumiSoft.UI.Controls.WLabel();
			this.m_pGetProgram = new LumiSoft.UI.Controls.WButton();
			this.SuspendLayout();
			// 
			// m_pDnsServer
			// 
			this.m_pDnsServer.DrawBorder = true;
			this.m_pDnsServer.Location = new System.Drawing.Point(16, 24);
			this.m_pDnsServer.Name = "m_pDnsServer";
			this.m_pDnsServer.Size = new System.Drawing.Size(352, 20);
			this.m_pDnsServer.TabIndex = 0;
			this.m_pDnsServer.UseStaticViewStyle = true;
			// 
			// m_pArguments.ViewStyle
			// 
			// 
			// m_pSave
			// 
			this.m_pSave.DrawBorder = true;
			this.m_pSave.Location = new System.Drawing.Point(280, 88);
			this.m_pSave.Name = "m_pSave";
			this.m_pSave.Size = new System.Drawing.Size(88, 24);
			this.m_pSave.TabIndex = 2;
			this.m_pSave.Text = "Save and close";
			this.m_pSave.UseStaticViewStyle = true;
			// 
			// m_pSave.ViewStyle
			// 
			this.m_pSave.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pSave_ButtonPressed);
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(16, 48);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(328, 16);
			this.wLabel1.TabIndex = 3;
			this.wLabel1.Text = "Scan arguments. NOTE: use #FileName as scan file palceholder.";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// wLabel2
			// 
			this.wLabel2.Location = new System.Drawing.Point(16, 8);
			this.wLabel2.Name = "wLabel2";
			this.wLabel2.Size = new System.Drawing.Size(328, 16);
			this.wLabel2.TabIndex = 4;
			this.wLabel2.Text = "Scan programm. For example c:\\Program Files\\NAV\\Nav.exe.";
			this.wLabel2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel2.ViewStyle
			// 
			// 
			// m_pGetProgram
			// 
			this.m_pGetProgram.DrawBorder = true;
			this.m_pGetProgram.Location = new System.Drawing.Point(376, 24);
			this.m_pGetProgram.Name = "m_pGetProgram";
			this.m_pGetProgram.Size = new System.Drawing.Size(24, 24);
			this.m_pGetProgram.TabIndex = 5;
			this.m_pGetProgram.Text = "..";
			this.m_pGetProgram.UseStaticViewStyle = true;
			// 
			// m_pGetProgram.ViewStyle
			// 
			// 
			// Settings
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(400, 117);
			this.Controls.Add(this.m_pGetProgram);
			this.Controls.Add(this.wLabel2);
			this.Controls.Add(this.wLabel1);
			this.Controls.Add(this.m_pSave);
			this.Controls.Add(this.m_pDnsServer);
			this.Name = "Settings";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Settings";
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling
		
		private void m_pSave_ButtonPressed(object sender, System.EventArgs e)
		{
			ds.Tables["Settings"].Rows[0]["DnsServer"] = m_pDnsServer.Text;

			ds.WriteXml(Application.StartupPath + "\\lsSenderFilter_db.xml");

			this.Close();
		}

		#endregion

	}
}
