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
	/// Summary description for wfrm_System_SMTP.
	/// </summary>
	public class wfrm_System_SMTP : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WLabel wLabel1;
		private LumiSoft.UI.Controls.WSpinEdit m_pMaxSMTPbadCmds;
		private LumiSoft.UI.Controls.WSpinEdit m_pMaxRecipients;
		private LumiSoft.UI.Controls.WLabel mt_maxrecipients;
		private LumiSoft.UI.Controls.WLabel wLabel6;
		private LumiSoft.UI.Controls.WLabel wLabel4;
		private LumiSoft.UI.Controls.WSpinEdit m_pMessageSize;
		private LumiSoft.UI.Controls.WSpinEdit m_pCommandIdle;
		private LumiSoft.UI.Controls.WSpinEdit m_pSessionIdle;
		private LumiSoft.UI.Controls.WLabel mt_msgsize;
		private LumiSoft.UI.Controls.WLabel mt_commandidle;
		private LumiSoft.UI.Controls.WLabel mt_sessionidle;
		private LumiSoft.UI.Controls.WLabel wLabel5;
		private LumiSoft.UI.Controls.WSpinEdit m_pSMTP_Threads;
		private LumiSoft.UI.Controls.WSpinEdit m_pSMTP;
		private LumiSoft.UI.Controls.WComboBox m_pSMTPIPAddresses;
		private LumiSoft.UI.Controls.WLabel mt_smtpmax;
		private LumiSoft.UI.Controls.WLabel mt_smtp;
		private LumiSoft.UI.Controls.WLabel mt_ipaddress;
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pEnabled;
		private LumiSoft.UI.Controls.WLabel wLabel7;
		private LumiSoft.UI.Controls.WEditBox m_pDefaultDomain;
		private LumiSoft.UI.Controls.WLabel wLabel8;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private DataSet dsVal = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="ds"></param>
		public wfrm_System_SMTP(DataSet ds)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			dsVal = ds;

			m_pSMTPIPAddresses.Items.Add("(All Unassigned)");

			IPHostEntry hostInfo = Dns.GetHostByName(Dns.GetHostName());			
			foreach(IPAddress ip in hostInfo.AddressList){
				string ipStr = ip.ToString();
				m_pSMTPIPAddresses.Items.Add(ipStr);
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
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.m_pMaxSMTPbadCmds = new LumiSoft.UI.Controls.WSpinEdit();
			this.m_pMaxRecipients = new LumiSoft.UI.Controls.WSpinEdit();
			this.mt_maxrecipients = new LumiSoft.UI.Controls.WLabel();
			this.wLabel6 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel4 = new LumiSoft.UI.Controls.WLabel();
			this.m_pMessageSize = new LumiSoft.UI.Controls.WSpinEdit();
			this.m_pCommandIdle = new LumiSoft.UI.Controls.WSpinEdit();
			this.m_pSessionIdle = new LumiSoft.UI.Controls.WSpinEdit();
			this.mt_msgsize = new LumiSoft.UI.Controls.WLabel();
			this.mt_commandidle = new LumiSoft.UI.Controls.WLabel();
			this.mt_sessionidle = new LumiSoft.UI.Controls.WLabel();
			this.wLabel5 = new LumiSoft.UI.Controls.WLabel();
			this.m_pSMTP_Threads = new LumiSoft.UI.Controls.WSpinEdit();
			this.m_pSMTP = new LumiSoft.UI.Controls.WSpinEdit();
			this.m_pSMTPIPAddresses = new LumiSoft.UI.Controls.WComboBox();
			this.mt_smtpmax = new LumiSoft.UI.Controls.WLabel();
			this.mt_smtp = new LumiSoft.UI.Controls.WLabel();
			this.mt_ipaddress = new LumiSoft.UI.Controls.WLabel();
			this.m_pEnabled = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.wLabel7 = new LumiSoft.UI.Controls.WLabel();
			this.m_pDefaultDomain = new LumiSoft.UI.Controls.WEditBox();
			this.wLabel8 = new LumiSoft.UI.Controls.WLabel();
			this.SuspendLayout();
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(8, 192);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(166, 24);
			this.wLabel1.TabIndex = 16;
			this.wLabel1.Text = "Max. bad commands";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// m_pMaxSMTPbadCmds
			// 
			this.m_pMaxSMTPbadCmds.BackColor = System.Drawing.Color.White;
			this.m_pMaxSMTPbadCmds.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pMaxSMTPbadCmds.DecimalPlaces = 0;
			this.m_pMaxSMTPbadCmds.DecMaxValue = new System.Decimal(new int[] {
																				  999999999,
																				  0,
																				  0,
																				  0});
			this.m_pMaxSMTPbadCmds.DecMinValue = new System.Decimal(new int[] {
																				  1,
																				  0,
																				  0,
																				  0});
			this.m_pMaxSMTPbadCmds.DecValue = new System.Decimal(new int[] {
																			   8,
																			   0,
																			   0,
																			   0});
			this.m_pMaxSMTPbadCmds.DrawBorder = true;
			this.m_pMaxSMTPbadCmds.Location = new System.Drawing.Point(176, 192);
			this.m_pMaxSMTPbadCmds.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pMaxSMTPbadCmds.MaxLength = 32767;
			this.m_pMaxSMTPbadCmds.Name = "m_pMaxSMTPbadCmds";
			this.m_pMaxSMTPbadCmds.ReadOnly = false;
			this.m_pMaxSMTPbadCmds.Size = new System.Drawing.Size(64, 20);
			this.m_pMaxSMTPbadCmds.TabIndex = 6;
			this.m_pMaxSMTPbadCmds.Text = "8";
			this.m_pMaxSMTPbadCmds.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pMaxSMTPbadCmds.UseStaticViewStyle = true;
			// 
			// m_pMaxSMTPbadCmds.ViewStyle
			// 
			// 
			// m_pMaxRecipients
			// 
			this.m_pMaxRecipients.BackColor = System.Drawing.Color.White;
			this.m_pMaxRecipients.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pMaxRecipients.DecimalPlaces = 0;
			this.m_pMaxRecipients.DecMaxValue = new System.Decimal(new int[] {
																				 999999999,
																				 0,
																				 0,
																				 0});
			this.m_pMaxRecipients.DecMinValue = new System.Decimal(new int[] {
																				 1,
																				 0,
																				 0,
																				 0});
			this.m_pMaxRecipients.DecValue = new System.Decimal(new int[] {
																			  100,
																			  0,
																			  0,
																			  0});
			this.m_pMaxRecipients.DrawBorder = true;
			this.m_pMaxRecipients.Location = new System.Drawing.Point(176, 168);
			this.m_pMaxRecipients.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pMaxRecipients.MaxLength = 32767;
			this.m_pMaxRecipients.Name = "m_pMaxRecipients";
			this.m_pMaxRecipients.ReadOnly = false;
			this.m_pMaxRecipients.Size = new System.Drawing.Size(64, 20);
			this.m_pMaxRecipients.TabIndex = 5;
			this.m_pMaxRecipients.Text = "100";
			this.m_pMaxRecipients.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pMaxRecipients.UseStaticViewStyle = true;
			// 
			// m_pMaxRecipients.ViewStyle
			// 
			// 
			// mt_maxrecipients
			// 
			this.mt_maxrecipients.Location = new System.Drawing.Point(8, 168);
			this.mt_maxrecipients.Name = "mt_maxrecipients";
			this.mt_maxrecipients.Size = new System.Drawing.Size(166, 24);
			this.mt_maxrecipients.TabIndex = 15;
			this.mt_maxrecipients.Text = "Max. Recipients per Message";
			this.mt_maxrecipients.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_maxrecipients.ViewStyle
			// 
			// 
			// wLabel6
			// 
			this.wLabel6.Location = new System.Drawing.Point(240, 144);
			this.wLabel6.Name = "wLabel6";
			this.wLabel6.Size = new System.Drawing.Size(48, 24);
			this.wLabel6.TabIndex = 22;
			this.wLabel6.Text = "MB";
			this.wLabel6.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel6.ViewStyle
			// 
			// 
			// wLabel4
			// 
			this.wLabel4.Location = new System.Drawing.Point(240, 24);
			this.wLabel4.Name = "wLabel4";
			this.wLabel4.Size = new System.Drawing.Size(48, 24);
			this.wLabel4.TabIndex = 20;
			this.wLabel4.Text = "sec.";
			this.wLabel4.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel4.ViewStyle
			// 
			// 
			// m_pMessageSize
			// 
			this.m_pMessageSize.BackColor = System.Drawing.Color.White;
			this.m_pMessageSize.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pMessageSize.DecimalPlaces = 0;
			this.m_pMessageSize.DecMaxValue = new System.Decimal(new int[] {
																			   999999999,
																			   0,
																			   0,
																			   0});
			this.m_pMessageSize.DecMinValue = new System.Decimal(new int[] {
																			   1,
																			   0,
																			   0,
																			   0});
			this.m_pMessageSize.DecValue = new System.Decimal(new int[] {
																			10,
																			0,
																			0,
																			0});
			this.m_pMessageSize.DrawBorder = true;
			this.m_pMessageSize.Location = new System.Drawing.Point(176, 144);
			this.m_pMessageSize.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pMessageSize.MaxLength = 32767;
			this.m_pMessageSize.Name = "m_pMessageSize";
			this.m_pMessageSize.ReadOnly = false;
			this.m_pMessageSize.Size = new System.Drawing.Size(64, 20);
			this.m_pMessageSize.TabIndex = 4;
			this.m_pMessageSize.Text = "10";
			this.m_pMessageSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pMessageSize.UseStaticViewStyle = true;
			// 
			// m_pMessageSize.ViewStyle
			// 
			// 
			// m_pCommandIdle
			// 
			this.m_pCommandIdle.BackColor = System.Drawing.Color.White;
			this.m_pCommandIdle.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pCommandIdle.DecimalPlaces = 0;
			this.m_pCommandIdle.DecMaxValue = new System.Decimal(new int[] {
																			   999999999,
																			   0,
																			   0,
																			   0});
			this.m_pCommandIdle.DecMinValue = new System.Decimal(new int[] {
																			   10,
																			   0,
																			   0,
																			   0});
			this.m_pCommandIdle.DecValue = new System.Decimal(new int[] {
																			60,
																			0,
																			0,
																			0});
			this.m_pCommandIdle.DrawBorder = true;
			this.m_pCommandIdle.Location = new System.Drawing.Point(176, 48);
			this.m_pCommandIdle.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pCommandIdle.MaxLength = 32767;
			this.m_pCommandIdle.Name = "m_pCommandIdle";
			this.m_pCommandIdle.ReadOnly = false;
			this.m_pCommandIdle.Size = new System.Drawing.Size(64, 20);
			this.m_pCommandIdle.TabIndex = 1;
			this.m_pCommandIdle.Text = "60";
			this.m_pCommandIdle.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pCommandIdle.UseStaticViewStyle = true;
			// 
			// m_pCommandIdle.ViewStyle
			// 
			// 
			// m_pSessionIdle
			// 
			this.m_pSessionIdle.BackColor = System.Drawing.Color.White;
			this.m_pSessionIdle.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pSessionIdle.DecimalPlaces = 0;
			this.m_pSessionIdle.DecMaxValue = new System.Decimal(new int[] {
																			   999999999,
																			   0,
																			   0,
																			   0});
			this.m_pSessionIdle.DecMinValue = new System.Decimal(new int[] {
																			   20,
																			   0,
																			   0,
																			   0});
			this.m_pSessionIdle.DecValue = new System.Decimal(new int[] {
																			60,
																			0,
																			0,
																			0});
			this.m_pSessionIdle.DrawBorder = true;
			this.m_pSessionIdle.Location = new System.Drawing.Point(176, 24);
			this.m_pSessionIdle.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pSessionIdle.MaxLength = 32767;
			this.m_pSessionIdle.Name = "m_pSessionIdle";
			this.m_pSessionIdle.ReadOnly = false;
			this.m_pSessionIdle.Size = new System.Drawing.Size(64, 20);
			this.m_pSessionIdle.TabIndex = 0;
			this.m_pSessionIdle.Text = "60";
			this.m_pSessionIdle.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pSessionIdle.UseStaticViewStyle = true;
			// 
			// m_pSessionIdle.ViewStyle
			// 
			// 
			// mt_msgsize
			// 
			this.mt_msgsize.Location = new System.Drawing.Point(8, 144);
			this.mt_msgsize.Name = "mt_msgsize";
			this.mt_msgsize.Size = new System.Drawing.Size(166, 24);
			this.mt_msgsize.TabIndex = 14;
			this.mt_msgsize.Text = "Max. Message Size";
			this.mt_msgsize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_msgsize.ViewStyle
			// 
			// 
			// mt_commandidle
			// 
			this.mt_commandidle.Location = new System.Drawing.Point(8, 48);
			this.mt_commandidle.Name = "mt_commandidle";
			this.mt_commandidle.Size = new System.Drawing.Size(166, 24);
			this.mt_commandidle.TabIndex = 11;
			this.mt_commandidle.Text = "Command Idle Timeout";
			this.mt_commandidle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_commandidle.ViewStyle
			// 
			// 
			// mt_sessionidle
			// 
			this.mt_sessionidle.Location = new System.Drawing.Point(8, 24);
			this.mt_sessionidle.Name = "mt_sessionidle";
			this.mt_sessionidle.Size = new System.Drawing.Size(166, 24);
			this.mt_sessionidle.TabIndex = 10;
			this.mt_sessionidle.Text = "Session Idle Timeout";
			this.mt_sessionidle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_sessionidle.ViewStyle
			// 
			// 
			// wLabel5
			// 
			this.wLabel5.Location = new System.Drawing.Point(240, 48);
			this.wLabel5.Name = "wLabel5";
			this.wLabel5.Size = new System.Drawing.Size(48, 24);
			this.wLabel5.TabIndex = 21;
			this.wLabel5.Text = "sec.";
			this.wLabel5.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel5.ViewStyle
			// 
			// 
			// m_pSMTP_Threads
			// 
			this.m_pSMTP_Threads.BackColor = System.Drawing.Color.White;
			this.m_pSMTP_Threads.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pSMTP_Threads.DecimalPlaces = 0;
			this.m_pSMTP_Threads.DecMaxValue = new System.Decimal(new int[] {
																				999,
																				0,
																				0,
																				0});
			this.m_pSMTP_Threads.DecMinValue = new System.Decimal(new int[] {
																				1,
																				0,
																				0,
																				0});
			this.m_pSMTP_Threads.DecValue = new System.Decimal(new int[] {
																			 10,
																			 0,
																			 0,
																			 0});
			this.m_pSMTP_Threads.DrawBorder = true;
			this.m_pSMTP_Threads.Location = new System.Drawing.Point(176, 288);
			this.m_pSMTP_Threads.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pSMTP_Threads.MaxLength = 32767;
			this.m_pSMTP_Threads.Name = "m_pSMTP_Threads";
			this.m_pSMTP_Threads.ReadOnly = false;
			this.m_pSMTP_Threads.Size = new System.Drawing.Size(64, 20);
			this.m_pSMTP_Threads.TabIndex = 9;
			this.m_pSMTP_Threads.Text = "10";
			this.m_pSMTP_Threads.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pSMTP_Threads.UseStaticViewStyle = true;
			// 
			// m_pSMTP_Threads.ViewStyle
			// 
			// 
			// m_pSMTP
			// 
			this.m_pSMTP.BackColor = System.Drawing.Color.White;
			this.m_pSMTP.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pSMTP.DecimalPlaces = 0;
			this.m_pSMTP.DecMaxValue = new System.Decimal(new int[] {
																		999999999,
																		0,
																		0,
																		0});
			this.m_pSMTP.DecMinValue = new System.Decimal(new int[] {
																		0,
																		0,
																		0,
																		0});
			this.m_pSMTP.DecValue = new System.Decimal(new int[] {
																	 25,
																	 0,
																	 0,
																	 0});
			this.m_pSMTP.DrawBorder = true;
			this.m_pSMTP.Location = new System.Drawing.Point(176, 264);
			this.m_pSMTP.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pSMTP.MaxLength = 32767;
			this.m_pSMTP.Name = "m_pSMTP";
			this.m_pSMTP.ReadOnly = false;
			this.m_pSMTP.Size = new System.Drawing.Size(64, 20);
			this.m_pSMTP.TabIndex = 8;
			this.m_pSMTP.Text = "25";
			this.m_pSMTP.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pSMTP.UseStaticViewStyle = true;
			// 
			// m_pSMTP.ViewStyle
			// 
			// 
			// m_pSMTPIPAddresses
			// 
			this.m_pSMTPIPAddresses.DrawBorder = true;
			this.m_pSMTPIPAddresses.DropDownWidth = 128;
			this.m_pSMTPIPAddresses.EditStyle = LumiSoft.UI.Controls.EditStyle.Selectable;
			this.m_pSMTPIPAddresses.Location = new System.Drawing.Point(176, 240);
			this.m_pSMTPIPAddresses.Name = "m_pSMTPIPAddresses";
			this.m_pSMTPIPAddresses.SelectedIndex = -1;
			this.m_pSMTPIPAddresses.SelectionLength = 0;
			this.m_pSMTPIPAddresses.SelectionStart = 0;
			this.m_pSMTPIPAddresses.Size = new System.Drawing.Size(128, 20);
			this.m_pSMTPIPAddresses.TabIndex = 7;
			this.m_pSMTPIPAddresses.UseStaticViewStyle = true;
			// 
			// m_pSMTPIPAddresses.ViewStyle
			// 
			this.m_pSMTPIPAddresses.VisibleItems = 5;
			// 
			// mt_smtpmax
			// 
			this.mt_smtpmax.Location = new System.Drawing.Point(8, 288);
			this.mt_smtpmax.Name = "mt_smtpmax";
			this.mt_smtpmax.Size = new System.Drawing.Size(166, 24);
			this.mt_smtpmax.TabIndex = 19;
			this.mt_smtpmax.Text = "Maximum SMTP Threads";
			this.mt_smtpmax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_smtpmax.ViewStyle
			// 
			// 
			// mt_smtp
			// 
			this.mt_smtp.Location = new System.Drawing.Point(8, 264);
			this.mt_smtp.Name = "mt_smtp";
			this.mt_smtp.Size = new System.Drawing.Size(166, 24);
			this.mt_smtp.TabIndex = 18;
			this.mt_smtp.Text = "SMTP Port on IP Address Above";
			this.mt_smtp.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_smtp.ViewStyle
			// 
			// 
			// mt_ipaddress
			// 
			this.mt_ipaddress.Location = new System.Drawing.Point(8, 240);
			this.mt_ipaddress.Name = "mt_ipaddress";
			this.mt_ipaddress.Size = new System.Drawing.Size(166, 24);
			this.mt_ipaddress.TabIndex = 17;
			this.mt_ipaddress.Text = "SMTP IP Address";
			this.mt_ipaddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_ipaddress.ViewStyle
			// 
			// 
			// m_pEnabled
			// 
			this.m_pEnabled.Checked = false;
			this.m_pEnabled.DrawBorder = true;
			this.m_pEnabled.Location = new System.Drawing.Point(176, 320);
			this.m_pEnabled.Name = "m_pEnabled";
			this.m_pEnabled.ReadOnly = false;
			this.m_pEnabled.Size = new System.Drawing.Size(16, 22);
			this.m_pEnabled.TabIndex = 46;
			this.m_pEnabled.UseStaticViewStyle = true;
			// 
			// m_pEnabled.ViewStyle
			// 
			// 
			// wLabel7
			// 
			this.wLabel7.Location = new System.Drawing.Point(8, 320);
			this.wLabel7.Name = "wLabel7";
			this.wLabel7.Size = new System.Drawing.Size(166, 24);
			this.wLabel7.TabIndex = 45;
			this.wLabel7.Text = "Enabled";
			this.wLabel7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel7.ViewStyle
			// 
			// 
			// m_pDefaultDomain
			// 
			this.m_pDefaultDomain.DrawBorder = true;
			this.m_pDefaultDomain.Location = new System.Drawing.Point(176, 80);
			this.m_pDefaultDomain.Name = "m_pDefaultDomain";
			this.m_pDefaultDomain.Size = new System.Drawing.Size(272, 20);
			this.m_pDefaultDomain.TabIndex = 47;
			this.m_pDefaultDomain.UseStaticViewStyle = true;
			// 
			// m_pDefaultDomain.ViewStyle
			// 
			// 
			// wLabel8
			// 
			this.wLabel8.Location = new System.Drawing.Point(8, 80);
			this.wLabel8.Name = "wLabel8";
			this.wLabel8.Size = new System.Drawing.Size(166, 24);
			this.wLabel8.TabIndex = 48;
			this.wLabel8.Text = "Defult domain";
			this.wLabel8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel8.ViewStyle
			// 
			// 
			// wfrm_System_SMTP
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 357);
			this.Controls.Add(this.m_pDefaultDomain);
			this.Controls.Add(this.wLabel8);
			this.Controls.Add(this.m_pEnabled);
			this.Controls.Add(this.wLabel7);
			this.Controls.Add(this.m_pSMTP_Threads);
			this.Controls.Add(this.m_pSMTP);
			this.Controls.Add(this.m_pSMTPIPAddresses);
			this.Controls.Add(this.mt_smtpmax);
			this.Controls.Add(this.mt_smtp);
			this.Controls.Add(this.mt_ipaddress);
			this.Controls.Add(this.wLabel4);
			this.Controls.Add(this.mt_sessionidle);
			this.Controls.Add(this.mt_commandidle);
			this.Controls.Add(this.m_pSessionIdle);
			this.Controls.Add(this.wLabel5);
			this.Controls.Add(this.m_pCommandIdle);
			this.Controls.Add(this.wLabel6);
			this.Controls.Add(this.m_pMaxRecipients);
			this.Controls.Add(this.wLabel1);
			this.Controls.Add(this.m_pMessageSize);
			this.Controls.Add(this.mt_msgsize);
			this.Controls.Add(this.mt_maxrecipients);
			this.Controls.Add(this.m_pMaxSMTPbadCmds);
			this.Name = "wfrm_System_SMTP";
			this.Text = "wfrm_System_SMTP";
			this.ResumeLayout(false);

		}
		#endregion


		#region function LoadData

		private void LoadData(DataSet ds)
		{
			try
			{
				DataRow dr = ds.Tables["Settings"].Rows[0];
				m_pSMTPIPAddresses.Text = dr["SMTP_IPAddress"].ToString();
				m_pSMTP.Text            = dr["SMTP_Port"].ToString();
				m_pSMTP_Threads.Text    = dr["SMTP_Threads"].ToString();
				m_pSessionIdle.Text     = dr["SMTP_SessionIdleTimeOut"].ToString();
				m_pCommandIdle.Text     = dr["SMTP_CommandIdleTimeOut"].ToString();
				m_pMaxSMTPbadCmds.Text  = dr["SMTP_MaxBadCommands"].ToString();
				m_pMessageSize.Text     = dr["MaxMessageSize"].ToString();
				m_pMaxRecipients.Text   = dr["MaxRecipients"].ToString();
				m_pDefaultDomain.Text   = dr["SMTP_DefaultDomain"].ToString();
				m_pEnabled.Checked      = Convert.ToBoolean(dr["SMTP_Enabled"]);
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
			if(dr["SMTP_IPAddress"].ToString()		    != m_pSMTPIPAddresses.Text) { dr["SMTP_IPAddress"]          =  m_pSMTPIPAddresses.Text; }
			if(dr["SMTP_Port"].ToString()				!= m_pSMTP.Text)            { dr["SMTP_Port"]               =  m_pSMTP.Text; }
			if(dr["SMTP_Threads"].ToString()			!= m_pSMTP_Threads.Text)    { dr["SMTP_Threads"]            =  m_pSMTP_Threads.Text; }
			if(dr["SMTP_SessionIdleTimeOut"].ToString()	!= m_pSessionIdle.Text)     { dr["SMTP_SessionIdleTimeOut"] =  m_pSessionIdle.Text; }
			if(dr["SMTP_CommandIdleTimeOut"].ToString()	!= m_pCommandIdle.Text)     { dr["SMTP_CommandIdleTimeOut"] =  m_pCommandIdle.Text; }
			if(dr["SMTP_MaxBadCommands"].ToString()		!= m_pMaxSMTPbadCmds.Text)  { dr["SMTP_MaxBadCommands"]     =  m_pMaxSMTPbadCmds.Text; }
			if(dr["MaxMessageSize"].ToString()			!= m_pMessageSize.Text)     { dr["MaxMessageSize"]          =  m_pMessageSize.Text; }
			if(dr["MaxRecipients"].ToString()			!= m_pMaxRecipients.Text)   { dr["MaxRecipients"]           =  m_pMaxRecipients.Text; }
			if(dr["SMTP_DefaultDomain"].ToString()		!= m_pDefaultDomain.Text)   { dr["SMTP_DefaultDomain"]      =  m_pDefaultDomain.Text; }
			if(Convert.ToBoolean(dr["SMTP_Enabled"])    != m_pEnabled.Checked)      { dr["SMTP_Enabled"]            =  m_pEnabled.Checked; }
		}

		#endregion

	}
}
