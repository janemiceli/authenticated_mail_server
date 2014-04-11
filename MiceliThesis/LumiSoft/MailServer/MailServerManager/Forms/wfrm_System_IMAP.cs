using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using LumiSoft.MailServer;

namespace MailServerManager.Forms
{
	/// <summary>
	/// Summary description for wfrm_System_IMAP.
	/// </summary>
	public class wfrm_System_IMAP : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WComboBox m_pIMAPIPAddresses;
		private LumiSoft.UI.Controls.WLabel wLabel7;
		private LumiSoft.UI.Controls.WSpinEdit m_pIMAP_Threads;
		private LumiSoft.UI.Controls.WSpinEdit m_pIMAP;
		private LumiSoft.UI.Controls.WLabel mt_pop3max;
		private LumiSoft.UI.Controls.WLabel mt_pop3;
		private LumiSoft.UI.Controls.WLabel wLabel10;
		private LumiSoft.UI.Controls.WSpinEdit m_pIMAPSessionIdle;
		private LumiSoft.UI.Controls.WLabel wLabel8;
		private LumiSoft.UI.Controls.WLabel wLabel11;
		private LumiSoft.UI.Controls.WSpinEdit m_pIMAPCommandIdle;
		private LumiSoft.UI.Controls.WLabel wLabel9;
		private LumiSoft.UI.Controls.WSpinEdit m_pMaxIMAPbadCmds;
		private LumiSoft.UI.Controls.WLabel wLabel13;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pEnabled;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private DataSet dsVal = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="ds"></param>
		public wfrm_System_IMAP(DataSet ds)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			dsVal = ds;

			m_pIMAPIPAddresses.Items.Add("(All Unassigned)");

			IPHostEntry hostInfo = Dns.GetHostByName(Dns.GetHostName());			
			foreach(IPAddress ip in hostInfo.AddressList){
				string ipStr = ip.ToString();
				m_pIMAPIPAddresses.Items.Add(ipStr);
			}

			LoadData(ds);
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
			this.m_pIMAPIPAddresses = new LumiSoft.UI.Controls.WComboBox();
			this.wLabel7 = new LumiSoft.UI.Controls.WLabel();
			this.m_pIMAP_Threads = new LumiSoft.UI.Controls.WSpinEdit();
			this.m_pIMAP = new LumiSoft.UI.Controls.WSpinEdit();
			this.mt_pop3max = new LumiSoft.UI.Controls.WLabel();
			this.mt_pop3 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel10 = new LumiSoft.UI.Controls.WLabel();
			this.m_pIMAPSessionIdle = new LumiSoft.UI.Controls.WSpinEdit();
			this.wLabel8 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel11 = new LumiSoft.UI.Controls.WLabel();
			this.m_pIMAPCommandIdle = new LumiSoft.UI.Controls.WSpinEdit();
			this.wLabel9 = new LumiSoft.UI.Controls.WLabel();
			this.m_pMaxIMAPbadCmds = new LumiSoft.UI.Controls.WSpinEdit();
			this.wLabel13 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.m_pEnabled = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.SuspendLayout();
			// 
			// m_pIMAPIPAddresses
			// 
			this.m_pIMAPIPAddresses.DrawBorder = true;
			this.m_pIMAPIPAddresses.DropDownWidth = 128;
			this.m_pIMAPIPAddresses.EditStyle = LumiSoft.UI.Controls.EditStyle.Selectable;
			this.m_pIMAPIPAddresses.Location = new System.Drawing.Point(176, 144);
			this.m_pIMAPIPAddresses.Name = "m_pIMAPIPAddresses";
			this.m_pIMAPIPAddresses.SelectedIndex = -1;
			this.m_pIMAPIPAddresses.SelectionLength = 0;
			this.m_pIMAPIPAddresses.SelectionStart = 0;
			this.m_pIMAPIPAddresses.Size = new System.Drawing.Size(128, 20);
			this.m_pIMAPIPAddresses.TabIndex = 42;
			this.m_pIMAPIPAddresses.UseStaticViewStyle = true;
			// 
			// m_pIMAPIPAddresses.ViewStyle
			// 
			this.m_pIMAPIPAddresses.VisibleItems = 5;
			// 
			// wLabel7
			// 
			this.wLabel7.Location = new System.Drawing.Point(8, 144);
			this.wLabel7.Name = "wLabel7";
			this.wLabel7.Size = new System.Drawing.Size(166, 24);
			this.wLabel7.TabIndex = 41;
			this.wLabel7.Text = "IMAP IP Address";
			this.wLabel7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel7.ViewStyle
			// 
			// 
			// m_pIMAP_Threads
			// 
			this.m_pIMAP_Threads.BackColor = System.Drawing.Color.White;
			this.m_pIMAP_Threads.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pIMAP_Threads.DecimalPlaces = 0;
			this.m_pIMAP_Threads.DecMaxValue = new System.Decimal(new int[] {
																				999,
																				0,
																				0,
																				0});
			this.m_pIMAP_Threads.DecMinValue = new System.Decimal(new int[] {
																				1,
																				0,
																				0,
																				0});
			this.m_pIMAP_Threads.DecValue = new System.Decimal(new int[] {
																			 100,
																			 0,
																			 0,
																			 0});
			this.m_pIMAP_Threads.DrawBorder = true;
			this.m_pIMAP_Threads.Location = new System.Drawing.Point(176, 192);
			this.m_pIMAP_Threads.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pIMAP_Threads.MaxLength = 32767;
			this.m_pIMAP_Threads.Name = "m_pIMAP_Threads";
			this.m_pIMAP_Threads.ReadOnly = false;
			this.m_pIMAP_Threads.Size = new System.Drawing.Size(64, 20);
			this.m_pIMAP_Threads.TabIndex = 39;
			this.m_pIMAP_Threads.Text = "100";
			this.m_pIMAP_Threads.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pIMAP_Threads.UseStaticViewStyle = true;
			// 
			// m_pIMAP_Threads.ViewStyle
			// 
			// 
			// m_pIMAP
			// 
			this.m_pIMAP.BackColor = System.Drawing.Color.White;
			this.m_pIMAP.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pIMAP.DecimalPlaces = 0;
			this.m_pIMAP.DecMaxValue = new System.Decimal(new int[] {
																		999999999,
																		0,
																		0,
																		0});
			this.m_pIMAP.DecMinValue = new System.Decimal(new int[] {
																		0,
																		0,
																		0,
																		0});
			this.m_pIMAP.DecValue = new System.Decimal(new int[] {
																	 143,
																	 0,
																	 0,
																	 0});
			this.m_pIMAP.DrawBorder = true;
			this.m_pIMAP.Location = new System.Drawing.Point(176, 168);
			this.m_pIMAP.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pIMAP.MaxLength = 32767;
			this.m_pIMAP.Name = "m_pIMAP";
			this.m_pIMAP.ReadOnly = false;
			this.m_pIMAP.Size = new System.Drawing.Size(64, 20);
			this.m_pIMAP.TabIndex = 37;
			this.m_pIMAP.Text = "143";
			this.m_pIMAP.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pIMAP.UseStaticViewStyle = true;
			// 
			// m_pIMAP.ViewStyle
			// 
			// 
			// mt_pop3max
			// 
			this.mt_pop3max.Location = new System.Drawing.Point(8, 192);
			this.mt_pop3max.Name = "mt_pop3max";
			this.mt_pop3max.Size = new System.Drawing.Size(166, 24);
			this.mt_pop3max.TabIndex = 36;
			this.mt_pop3max.Text = "Maximum IMAP Threads";
			this.mt_pop3max.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_pop3max.ViewStyle
			// 
			// 
			// mt_pop3
			// 
			this.mt_pop3.Location = new System.Drawing.Point(8, 168);
			this.mt_pop3.Name = "mt_pop3";
			this.mt_pop3.Size = new System.Drawing.Size(166, 24);
			this.mt_pop3.TabIndex = 35;
			this.mt_pop3.Text = "IMAP Port on IP Address Above";
			this.mt_pop3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_pop3.ViewStyle
			// 
			// 
			// wLabel10
			// 
			this.wLabel10.Location = new System.Drawing.Point(8, 24);
			this.wLabel10.Name = "wLabel10";
			this.wLabel10.Size = new System.Drawing.Size(166, 24);
			this.wLabel10.TabIndex = 29;
			this.wLabel10.Text = "Session Idle Timeout";
			this.wLabel10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel10.ViewStyle
			// 
			// 
			// m_pIMAPSessionIdle
			// 
			this.m_pIMAPSessionIdle.BackColor = System.Drawing.Color.White;
			this.m_pIMAPSessionIdle.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pIMAPSessionIdle.DecimalPlaces = 0;
			this.m_pIMAPSessionIdle.DecMaxValue = new System.Decimal(new int[] {
																				   999999999,
																				   0,
																				   0,
																				   0});
			this.m_pIMAPSessionIdle.DecMinValue = new System.Decimal(new int[] {
																				   20,
																				   0,
																				   0,
																				   0});
			this.m_pIMAPSessionIdle.DecValue = new System.Decimal(new int[] {
																				60,
																				0,
																				0,
																				0});
			this.m_pIMAPSessionIdle.DrawBorder = true;
			this.m_pIMAPSessionIdle.Location = new System.Drawing.Point(176, 24);
			this.m_pIMAPSessionIdle.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pIMAPSessionIdle.MaxLength = 32767;
			this.m_pIMAPSessionIdle.Name = "m_pIMAPSessionIdle";
			this.m_pIMAPSessionIdle.ReadOnly = false;
			this.m_pIMAPSessionIdle.Size = new System.Drawing.Size(64, 20);
			this.m_pIMAPSessionIdle.TabIndex = 31;
			this.m_pIMAPSessionIdle.Text = "60";
			this.m_pIMAPSessionIdle.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pIMAPSessionIdle.UseStaticViewStyle = true;
			// 
			// m_pIMAPSessionIdle.ViewStyle
			// 
			// 
			// wLabel8
			// 
			this.wLabel8.Location = new System.Drawing.Point(240, 24);
			this.wLabel8.Name = "wLabel8";
			this.wLabel8.Size = new System.Drawing.Size(48, 24);
			this.wLabel8.TabIndex = 33;
			this.wLabel8.Text = "sec.";
			this.wLabel8.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel8.ViewStyle
			// 
			// 
			// wLabel11
			// 
			this.wLabel11.Location = new System.Drawing.Point(240, 48);
			this.wLabel11.Name = "wLabel11";
			this.wLabel11.Size = new System.Drawing.Size(48, 24);
			this.wLabel11.TabIndex = 34;
			this.wLabel11.Text = "sec.";
			this.wLabel11.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel11.ViewStyle
			// 
			// 
			// m_pIMAPCommandIdle
			// 
			this.m_pIMAPCommandIdle.BackColor = System.Drawing.Color.White;
			this.m_pIMAPCommandIdle.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pIMAPCommandIdle.DecimalPlaces = 0;
			this.m_pIMAPCommandIdle.DecMaxValue = new System.Decimal(new int[] {
																				   999999999,
																				   0,
																				   0,
																				   0});
			this.m_pIMAPCommandIdle.DecMinValue = new System.Decimal(new int[] {
																				   10,
																				   0,
																				   0,
																				   0});
			this.m_pIMAPCommandIdle.DecValue = new System.Decimal(new int[] {
																				60,
																				0,
																				0,
																				0});
			this.m_pIMAPCommandIdle.DrawBorder = true;
			this.m_pIMAPCommandIdle.Location = new System.Drawing.Point(176, 48);
			this.m_pIMAPCommandIdle.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pIMAPCommandIdle.MaxLength = 32767;
			this.m_pIMAPCommandIdle.Name = "m_pIMAPCommandIdle";
			this.m_pIMAPCommandIdle.ReadOnly = false;
			this.m_pIMAPCommandIdle.Size = new System.Drawing.Size(64, 20);
			this.m_pIMAPCommandIdle.TabIndex = 32;
			this.m_pIMAPCommandIdle.Text = "60";
			this.m_pIMAPCommandIdle.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pIMAPCommandIdle.UseStaticViewStyle = true;
			// 
			// m_pIMAPCommandIdle.ViewStyle
			// 
			// 
			// wLabel9
			// 
			this.wLabel9.Location = new System.Drawing.Point(8, 48);
			this.wLabel9.Name = "wLabel9";
			this.wLabel9.Size = new System.Drawing.Size(166, 24);
			this.wLabel9.TabIndex = 30;
			this.wLabel9.Text = "Command Idle Timeout";
			this.wLabel9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel9.ViewStyle
			// 
			// 
			// m_pMaxIMAPbadCmds
			// 
			this.m_pMaxIMAPbadCmds.BackColor = System.Drawing.Color.White;
			this.m_pMaxIMAPbadCmds.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pMaxIMAPbadCmds.DecimalPlaces = 0;
			this.m_pMaxIMAPbadCmds.DecMaxValue = new System.Decimal(new int[] {
																				  999999999,
																				  0,
																				  0,
																				  0});
			this.m_pMaxIMAPbadCmds.DecMinValue = new System.Decimal(new int[] {
																				  1,
																				  0,
																				  0,
																				  0});
			this.m_pMaxIMAPbadCmds.DecValue = new System.Decimal(new int[] {
																			   8,
																			   0,
																			   0,
																			   0});
			this.m_pMaxIMAPbadCmds.DrawBorder = true;
			this.m_pMaxIMAPbadCmds.Location = new System.Drawing.Point(176, 96);
			this.m_pMaxIMAPbadCmds.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pMaxIMAPbadCmds.MaxLength = 32767;
			this.m_pMaxIMAPbadCmds.Name = "m_pMaxIMAPbadCmds";
			this.m_pMaxIMAPbadCmds.ReadOnly = false;
			this.m_pMaxIMAPbadCmds.Size = new System.Drawing.Size(64, 20);
			this.m_pMaxIMAPbadCmds.TabIndex = 38;
			this.m_pMaxIMAPbadCmds.Text = "8";
			this.m_pMaxIMAPbadCmds.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pMaxIMAPbadCmds.UseStaticViewStyle = true;
			// 
			// m_pMaxIMAPbadCmds.ViewStyle
			// 
			// 
			// wLabel13
			// 
			this.wLabel13.Location = new System.Drawing.Point(8, 96);
			this.wLabel13.Name = "wLabel13";
			this.wLabel13.Size = new System.Drawing.Size(166, 24);
			this.wLabel13.TabIndex = 40;
			this.wLabel13.Text = "Max. bad commands";
			this.wLabel13.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel13.ViewStyle
			// 
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(8, 232);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(166, 24);
			this.wLabel1.TabIndex = 43;
			this.wLabel1.Text = "Enabled";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// m_pEnabled
			// 
			this.m_pEnabled.Checked = false;
			this.m_pEnabled.DrawBorder = true;
			this.m_pEnabled.Location = new System.Drawing.Point(176, 232);
			this.m_pEnabled.Name = "m_pEnabled";
			this.m_pEnabled.ReadOnly = false;
			this.m_pEnabled.Size = new System.Drawing.Size(16, 22);
			this.m_pEnabled.TabIndex = 44;
			this.m_pEnabled.UseStaticViewStyle = true;
			// 
			// m_pEnabled.ViewStyle
			// 
			// 
			// wfrm_System_IMAP
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 357);
			this.Controls.Add(this.m_pEnabled);
			this.Controls.Add(this.wLabel1);
			this.Controls.Add(this.m_pIMAPIPAddresses);
			this.Controls.Add(this.wLabel7);
			this.Controls.Add(this.m_pIMAP_Threads);
			this.Controls.Add(this.m_pIMAP);
			this.Controls.Add(this.mt_pop3max);
			this.Controls.Add(this.mt_pop3);
			this.Controls.Add(this.wLabel10);
			this.Controls.Add(this.m_pIMAPSessionIdle);
			this.Controls.Add(this.wLabel8);
			this.Controls.Add(this.wLabel11);
			this.Controls.Add(this.m_pIMAPCommandIdle);
			this.Controls.Add(this.wLabel9);
			this.Controls.Add(this.m_pMaxIMAPbadCmds);
			this.Controls.Add(this.wLabel13);
			this.Name = "wfrm_System_IMAP";
			this.Text = "wfrm_System_IMAP";
			this.ResumeLayout(false);

		}
		#endregion


		#region function LoadData

		private void LoadData(DataSet ds)
		{
			try
			{
				DataRow dr = ds.Tables["Settings"].Rows[0];
				m_pIMAPIPAddresses.Text = dr["IMAP_IPAddress"].ToString();
				m_pIMAP.Text            = dr["IMAP_Port"].ToString();
				m_pIMAP_Threads.Text    = dr["IMAP_Threads"].ToString();
				m_pIMAPSessionIdle.Text = dr["IMAP_SessionIdleTimeOut"].ToString();
				m_pIMAPCommandIdle.Text = dr["IMAP_CommandIdleTimeOut"].ToString();
				m_pMaxIMAPbadCmds.Text  = dr["IMAP_MaxBadCommands"].ToString();
				m_pEnabled.Checked      = Convert.ToBoolean(dr["IMAP_Enabled"]);

			}
			catch(Exception x)
			{
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}
		}

		#endregion

		#region function SaveData

		internal void SaveData()
		{
			DataRow dr = dsVal.Tables["Settings"].Rows[0];
			if(dr["IMAP_IPAddress"].ToString()		    != m_pIMAPIPAddresses.Text) { dr["IMAP_IPAddress"]          =  m_pIMAPIPAddresses.Text; }
			if(dr["IMAP_Port"].ToString()				!= m_pIMAP.Text)            { dr["IMAP_Port"]               =  m_pIMAP.Text; }
			if(dr["IMAP_Threads"].ToString()			!= m_pIMAP_Threads.Text)    { dr["IMAP_Threads"]            =  m_pIMAP_Threads.Text; }
			if(dr["IMAP_SessionIdleTimeOut"].ToString()	!= m_pIMAPSessionIdle.Text) { dr["IMAP_SessionIdleTimeOut"] =  m_pIMAPSessionIdle.Text; }
			if(dr["IMAP_CommandIdleTimeOut"].ToString()	!= m_pIMAPCommandIdle.Text) { dr["IMAP_CommandIdleTimeOut"] =  m_pIMAPCommandIdle.Text; }
			if(dr["IMAP_MaxBadCommands"].ToString()		!= m_pMaxIMAPbadCmds.Text)  { dr["IMAP_MaxBadCommands"]     =  m_pMaxIMAPbadCmds.Text; }
			if(Convert.ToBoolean(dr["IMAP_Enabled"])    != m_pEnabled.Checked)      { dr["IMAP_Enabled"]            =  m_pEnabled.Checked; }
		}

		#endregion
	}
}
