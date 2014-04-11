using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Data;
using LumiSoft.MailServer;

namespace MailServerManager.Forms
{
	/// <summary>
	/// Summary description for wfrm_System_POP3.
	/// </summary>
	public class wfrm_System_POP3 : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WLabel wLabel10;
		private LumiSoft.UI.Controls.WLabel wLabel9;
		private LumiSoft.UI.Controls.WLabel wLabel11;
		private LumiSoft.UI.Controls.WLabel wLabel8;
		private LumiSoft.UI.Controls.WSpinEdit m_pPOP3SessionIdle;
		private LumiSoft.UI.Controls.WSpinEdit m_pPOP3CommandIdle;
		private LumiSoft.UI.Controls.WLabel wLabel13;
		private LumiSoft.UI.Controls.WSpinEdit m_pMaxPOP3badCmds;
		private LumiSoft.UI.Controls.WComboBox m_pPOP3IPAddresses;
		private LumiSoft.UI.Controls.WLabel wLabel7;
		private LumiSoft.UI.Controls.WSpinEdit m_pPOP3_Threads;
		private LumiSoft.UI.Controls.WSpinEdit m_pPOP3;
		private LumiSoft.UI.Controls.WLabel mt_pop3max;
		private LumiSoft.UI.Controls.WLabel mt_pop3;
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pEnabled;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private DataSet dsVal = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="ds"></param>
		public wfrm_System_POP3(DataSet ds)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			dsVal = ds;

			m_pPOP3IPAddresses.Items.Add("(All Unassigned)");

			IPHostEntry hostInfo = Dns.GetHostByName(Dns.GetHostName());			
			foreach(IPAddress ip in hostInfo.AddressList){
				string ipStr = ip.ToString();
				m_pPOP3IPAddresses.Items.Add(ipStr);
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
			this.wLabel10 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel9 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel11 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel8 = new LumiSoft.UI.Controls.WLabel();
			this.m_pPOP3SessionIdle = new LumiSoft.UI.Controls.WSpinEdit();
			this.m_pPOP3CommandIdle = new LumiSoft.UI.Controls.WSpinEdit();
			this.wLabel13 = new LumiSoft.UI.Controls.WLabel();
			this.m_pMaxPOP3badCmds = new LumiSoft.UI.Controls.WSpinEdit();
			this.m_pPOP3IPAddresses = new LumiSoft.UI.Controls.WComboBox();
			this.wLabel7 = new LumiSoft.UI.Controls.WLabel();
			this.m_pPOP3_Threads = new LumiSoft.UI.Controls.WSpinEdit();
			this.m_pPOP3 = new LumiSoft.UI.Controls.WSpinEdit();
			this.mt_pop3max = new LumiSoft.UI.Controls.WLabel();
			this.mt_pop3 = new LumiSoft.UI.Controls.WLabel();
			this.m_pEnabled = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.SuspendLayout();
			// 
			// wLabel10
			// 
			this.wLabel10.Location = new System.Drawing.Point(8, 24);
			this.wLabel10.Name = "wLabel10";
			this.wLabel10.Size = new System.Drawing.Size(166, 24);
			this.wLabel10.TabIndex = 17;
			this.wLabel10.Text = "Session Idle Timeout";
			this.wLabel10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel10.ViewStyle
			// 
			// 
			// wLabel9
			// 
			this.wLabel9.Location = new System.Drawing.Point(8, 48);
			this.wLabel9.Name = "wLabel9";
			this.wLabel9.Size = new System.Drawing.Size(166, 24);
			this.wLabel9.TabIndex = 18;
			this.wLabel9.Text = "Command Idle Timeout";
			this.wLabel9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel9.ViewStyle
			// 
			// 
			// wLabel11
			// 
			this.wLabel11.Location = new System.Drawing.Point(240, 48);
			this.wLabel11.Name = "wLabel11";
			this.wLabel11.Size = new System.Drawing.Size(48, 24);
			this.wLabel11.TabIndex = 22;
			this.wLabel11.Text = "sec.";
			this.wLabel11.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel11.ViewStyle
			// 
			// 
			// wLabel8
			// 
			this.wLabel8.Location = new System.Drawing.Point(240, 24);
			this.wLabel8.Name = "wLabel8";
			this.wLabel8.Size = new System.Drawing.Size(48, 24);
			this.wLabel8.TabIndex = 21;
			this.wLabel8.Text = "sec.";
			this.wLabel8.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel8.ViewStyle
			// 
			// 
			// m_pPOP3SessionIdle
			// 
			this.m_pPOP3SessionIdle.BackColor = System.Drawing.Color.White;
			this.m_pPOP3SessionIdle.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pPOP3SessionIdle.DecimalPlaces = 0;
			this.m_pPOP3SessionIdle.DecMaxValue = new System.Decimal(new int[] {
																				   999999999,
																				   0,
																				   0,
																				   0});
			this.m_pPOP3SessionIdle.DecMinValue = new System.Decimal(new int[] {
																				   20,
																				   0,
																				   0,
																				   0});
			this.m_pPOP3SessionIdle.DecValue = new System.Decimal(new int[] {
																				60,
																				0,
																				0,
																				0});
			this.m_pPOP3SessionIdle.DrawBorder = true;
			this.m_pPOP3SessionIdle.Location = new System.Drawing.Point(176, 24);
			this.m_pPOP3SessionIdle.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pPOP3SessionIdle.MaxLength = 32767;
			this.m_pPOP3SessionIdle.Name = "m_pPOP3SessionIdle";
			this.m_pPOP3SessionIdle.ReadOnly = false;
			this.m_pPOP3SessionIdle.Size = new System.Drawing.Size(64, 20);
			this.m_pPOP3SessionIdle.TabIndex = 19;
			this.m_pPOP3SessionIdle.Text = "60";
			this.m_pPOP3SessionIdle.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pPOP3SessionIdle.UseStaticViewStyle = true;
			// 
			// m_pPOP3SessionIdle.ViewStyle
			// 
			// 
			// m_pPOP3CommandIdle
			// 
			this.m_pPOP3CommandIdle.BackColor = System.Drawing.Color.White;
			this.m_pPOP3CommandIdle.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pPOP3CommandIdle.DecimalPlaces = 0;
			this.m_pPOP3CommandIdle.DecMaxValue = new System.Decimal(new int[] {
																				   999999999,
																				   0,
																				   0,
																				   0});
			this.m_pPOP3CommandIdle.DecMinValue = new System.Decimal(new int[] {
																				   10,
																				   0,
																				   0,
																				   0});
			this.m_pPOP3CommandIdle.DecValue = new System.Decimal(new int[] {
																				60,
																				0,
																				0,
																				0});
			this.m_pPOP3CommandIdle.DrawBorder = true;
			this.m_pPOP3CommandIdle.Location = new System.Drawing.Point(176, 48);
			this.m_pPOP3CommandIdle.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pPOP3CommandIdle.MaxLength = 32767;
			this.m_pPOP3CommandIdle.Name = "m_pPOP3CommandIdle";
			this.m_pPOP3CommandIdle.ReadOnly = false;
			this.m_pPOP3CommandIdle.Size = new System.Drawing.Size(64, 20);
			this.m_pPOP3CommandIdle.TabIndex = 20;
			this.m_pPOP3CommandIdle.Text = "60";
			this.m_pPOP3CommandIdle.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pPOP3CommandIdle.UseStaticViewStyle = true;
			// 
			// m_pPOP3CommandIdle.ViewStyle
			// 
			// 
			// wLabel13
			// 
			this.wLabel13.Location = new System.Drawing.Point(8, 96);
			this.wLabel13.Name = "wLabel13";
			this.wLabel13.Size = new System.Drawing.Size(166, 24);
			this.wLabel13.TabIndex = 27;
			this.wLabel13.Text = "Max. bad commands";
			this.wLabel13.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel13.ViewStyle
			// 
			// 
			// m_pMaxPOP3badCmds
			// 
			this.m_pMaxPOP3badCmds.BackColor = System.Drawing.Color.White;
			this.m_pMaxPOP3badCmds.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pMaxPOP3badCmds.DecimalPlaces = 0;
			this.m_pMaxPOP3badCmds.DecMaxValue = new System.Decimal(new int[] {
																				  999999999,
																				  0,
																				  0,
																				  0});
			this.m_pMaxPOP3badCmds.DecMinValue = new System.Decimal(new int[] {
																				  1,
																				  0,
																				  0,
																				  0});
			this.m_pMaxPOP3badCmds.DecValue = new System.Decimal(new int[] {
																			   8,
																			   0,
																			   0,
																			   0});
			this.m_pMaxPOP3badCmds.DrawBorder = true;
			this.m_pMaxPOP3badCmds.Location = new System.Drawing.Point(176, 96);
			this.m_pMaxPOP3badCmds.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pMaxPOP3badCmds.MaxLength = 32767;
			this.m_pMaxPOP3badCmds.Name = "m_pMaxPOP3badCmds";
			this.m_pMaxPOP3badCmds.ReadOnly = false;
			this.m_pMaxPOP3badCmds.Size = new System.Drawing.Size(64, 20);
			this.m_pMaxPOP3badCmds.TabIndex = 26;
			this.m_pMaxPOP3badCmds.Text = "8";
			this.m_pMaxPOP3badCmds.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pMaxPOP3badCmds.UseStaticViewStyle = true;
			// 
			// m_pMaxPOP3badCmds.ViewStyle
			// 
			// 
			// m_pPOP3IPAddresses
			// 
			this.m_pPOP3IPAddresses.DrawBorder = true;
			this.m_pPOP3IPAddresses.DropDownWidth = 128;
			this.m_pPOP3IPAddresses.EditStyle = LumiSoft.UI.Controls.EditStyle.Selectable;
			this.m_pPOP3IPAddresses.Location = new System.Drawing.Point(176, 144);
			this.m_pPOP3IPAddresses.Name = "m_pPOP3IPAddresses";
			this.m_pPOP3IPAddresses.SelectedIndex = -1;
			this.m_pPOP3IPAddresses.SelectionLength = 0;
			this.m_pPOP3IPAddresses.SelectionStart = 0;
			this.m_pPOP3IPAddresses.Size = new System.Drawing.Size(128, 20);
			this.m_pPOP3IPAddresses.TabIndex = 28;
			this.m_pPOP3IPAddresses.UseStaticViewStyle = true;
			// 
			// m_pPOP3IPAddresses.ViewStyle
			// 
			this.m_pPOP3IPAddresses.VisibleItems = 5;
			// 
			// wLabel7
			// 
			this.wLabel7.Location = new System.Drawing.Point(8, 144);
			this.wLabel7.Name = "wLabel7";
			this.wLabel7.Size = new System.Drawing.Size(166, 24);
			this.wLabel7.TabIndex = 27;
			this.wLabel7.Text = "POP3 IP Address";
			this.wLabel7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel7.ViewStyle
			// 
			// 
			// m_pPOP3_Threads
			// 
			this.m_pPOP3_Threads.BackColor = System.Drawing.Color.White;
			this.m_pPOP3_Threads.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pPOP3_Threads.DecimalPlaces = 0;
			this.m_pPOP3_Threads.DecMaxValue = new System.Decimal(new int[] {
																				999,
																				0,
																				0,
																				0});
			this.m_pPOP3_Threads.DecMinValue = new System.Decimal(new int[] {
																				1,
																				0,
																				0,
																				0});
			this.m_pPOP3_Threads.DecValue = new System.Decimal(new int[] {
																			 10,
																			 0,
																			 0,
																			 0});
			this.m_pPOP3_Threads.DrawBorder = true;
			this.m_pPOP3_Threads.Location = new System.Drawing.Point(176, 192);
			this.m_pPOP3_Threads.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pPOP3_Threads.MaxLength = 32767;
			this.m_pPOP3_Threads.Name = "m_pPOP3_Threads";
			this.m_pPOP3_Threads.ReadOnly = false;
			this.m_pPOP3_Threads.Size = new System.Drawing.Size(64, 20);
			this.m_pPOP3_Threads.TabIndex = 26;
			this.m_pPOP3_Threads.Text = "10";
			this.m_pPOP3_Threads.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pPOP3_Threads.UseStaticViewStyle = true;
			// 
			// m_pPOP3_Threads.ViewStyle
			// 
			// 
			// m_pPOP3
			// 
			this.m_pPOP3.BackColor = System.Drawing.Color.White;
			this.m_pPOP3.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pPOP3.DecimalPlaces = 0;
			this.m_pPOP3.DecMaxValue = new System.Decimal(new int[] {
																		999999999,
																		0,
																		0,
																		0});
			this.m_pPOP3.DecMinValue = new System.Decimal(new int[] {
																		0,
																		0,
																		0,
																		0});
			this.m_pPOP3.DecValue = new System.Decimal(new int[] {
																	 110,
																	 0,
																	 0,
																	 0});
			this.m_pPOP3.DrawBorder = true;
			this.m_pPOP3.Location = new System.Drawing.Point(176, 168);
			this.m_pPOP3.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pPOP3.MaxLength = 32767;
			this.m_pPOP3.Name = "m_pPOP3";
			this.m_pPOP3.ReadOnly = false;
			this.m_pPOP3.Size = new System.Drawing.Size(64, 20);
			this.m_pPOP3.TabIndex = 25;
			this.m_pPOP3.Text = "110";
			this.m_pPOP3.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pPOP3.UseStaticViewStyle = true;
			// 
			// m_pPOP3.ViewStyle
			// 
			// 
			// mt_pop3max
			// 
			this.mt_pop3max.Location = new System.Drawing.Point(8, 192);
			this.mt_pop3max.Name = "mt_pop3max";
			this.mt_pop3max.Size = new System.Drawing.Size(166, 24);
			this.mt_pop3max.TabIndex = 24;
			this.mt_pop3max.Text = "Maximum POP3 Threads";
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
			this.mt_pop3.TabIndex = 23;
			this.mt_pop3.Text = "POP3 Port on IP Address Above";
			this.mt_pop3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_pop3.ViewStyle
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
			this.m_pEnabled.TabIndex = 46;
			this.m_pEnabled.UseStaticViewStyle = true;
			// 
			// m_pEnabled.ViewStyle
			// 
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(8, 232);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(166, 24);
			this.wLabel1.TabIndex = 45;
			this.wLabel1.Text = "Enabled";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// wfrm_System_POP3
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 357);
			this.Controls.Add(this.m_pEnabled);
			this.Controls.Add(this.wLabel1);
			this.Controls.Add(this.m_pPOP3IPAddresses);
			this.Controls.Add(this.wLabel7);
			this.Controls.Add(this.m_pPOP3_Threads);
			this.Controls.Add(this.m_pPOP3);
			this.Controls.Add(this.mt_pop3max);
			this.Controls.Add(this.mt_pop3);
			this.Controls.Add(this.wLabel10);
			this.Controls.Add(this.m_pPOP3SessionIdle);
			this.Controls.Add(this.wLabel8);
			this.Controls.Add(this.wLabel11);
			this.Controls.Add(this.m_pPOP3CommandIdle);
			this.Controls.Add(this.wLabel9);
			this.Controls.Add(this.m_pMaxPOP3badCmds);
			this.Controls.Add(this.wLabel13);
			this.Name = "wfrm_System_POP3";
			this.Text = "wfrm_System_POP3";
			this.ResumeLayout(false);

		}
		#endregion


		#region function LoadData

		private void LoadData(DataSet ds)
		{
			try
			{
				DataRow dr = ds.Tables["Settings"].Rows[0];
				m_pPOP3IPAddresses.Text = dr["POP3_IPAddress"].ToString();
				m_pPOP3.Text            = dr["POP3_Port"].ToString();
				m_pPOP3_Threads.Text    = dr["POP3_Threads"].ToString();
				m_pPOP3SessionIdle.Text = dr["POP3_SessionIdleTimeOut"].ToString();
				m_pPOP3CommandIdle.Text = dr["POP3_CommandIdleTimeOut"].ToString();
				m_pMaxPOP3badCmds.Text  = dr["POP3_MaxBadCommands"].ToString();
				m_pEnabled.Checked      = Convert.ToBoolean(dr["POP3_Enabled"]);
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
			if(dr["POP3_IPAddress"].ToString()		    != m_pPOP3IPAddresses.Text) { dr["POP3_IPAddress"]          =  m_pPOP3IPAddresses.Text; }
			if(dr["POP3_Port"].ToString()				!= m_pPOP3.Text)            { dr["POP3_Port"]               =  m_pPOP3.Text; }
			if(dr["POP3_Threads"].ToString()			!= m_pPOP3_Threads.Text)    { dr["POP3_Threads"]            =  m_pPOP3_Threads.Text; }
			if(dr["POP3_SessionIdleTimeOut"].ToString()	!= m_pPOP3SessionIdle.Text) { dr["POP3_SessionIdleTimeOut"] =  m_pPOP3SessionIdle.Text; }
			if(dr["POP3_CommandIdleTimeOut"].ToString()	!= m_pPOP3CommandIdle.Text) { dr["POP3_CommandIdleTimeOut"] =  m_pPOP3CommandIdle.Text; }
			if(dr["POP3_MaxBadCommands"].ToString()		!= m_pMaxPOP3badCmds.Text)  { dr["POP3_MaxBadCommands"]     =  m_pMaxPOP3badCmds.Text; }
			if(Convert.ToBoolean(dr["POP3_Enabled"])    != m_pEnabled.Checked)      { dr["POP3_Enabled"]            =  m_pEnabled.Checked; }
		}

		#endregion

	}
}
