using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using LumiSoft.MailServer;
using LumiSoft.Net.Dns;

namespace MailServerManager.Forms
{
	/// <summary>
	/// Summary description for wfrm_System_Delivery.
	/// </summary>
	public class wfrm_System_Delivery : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WButton m_pUndeliveredText;
		private LumiSoft.UI.Controls.WButton m_UndelWarningText;
		private LumiSoft.UI.Controls.WLabel wLabel6;
		private LumiSoft.UI.Controls.WLabel wLabel5;
		private LumiSoft.UI.Controls.WLabel wLabel4;
		private LumiSoft.UI.Controls.WLabel wLabel3;
		private LumiSoft.UI.Controls.WSpinEdit m_pUndeliveredWarning;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		private LumiSoft.UI.Controls.WSpinEdit m_pUndelivered;
		private LumiSoft.UI.Controls.WLabel wLabel2;
		private LumiSoft.UI.Controls.WSpinEdit m_pRelayRetryInterval;
		private LumiSoft.UI.Controls.WLabel mt_RelayRetryInterval;
		private LumiSoft.UI.Controls.WSpinEdit m_pRelayInterval;
		private LumiSoft.UI.Controls.WLabel mt_RelayInterval;
		private LumiSoft.UI.Controls.WSpinEdit m_pMaxThreads;
		private LumiSoft.UI.Controls.WLabel mt_maxthreads;
		private System.Windows.Forms.RadioButton m_pUseSmartHost;
		private System.Windows.Forms.RadioButton m_pDNS;
		private LumiSoft.UI.Controls.WLabel mt_smarthost;
		private LumiSoft.UI.Controls.WLabel mt_pridns;
		private LumiSoft.UI.Controls.WEditBox m_pSmartHost;
		private LumiSoft.UI.Controls.WEditBox m_pSecDNS;
		private LumiSoft.UI.Controls.WLabel mt_secdns;
		private LumiSoft.UI.Controls.WEditBox m_pPriDNS;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pStoreUndeliveredMessages;
		private LumiSoft.UI.Controls.WButton m_pTestDns;
		private LumiSoft.UI.Controls.WLabel wLabel7;

		private DataSet dsVal = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="ds"></param>
		public wfrm_System_Delivery(DataSet ds)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			LoadData(ds);

			dsVal = ds;
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
			this.m_pUndeliveredText = new LumiSoft.UI.Controls.WButton();
			this.m_UndelWarningText = new LumiSoft.UI.Controls.WButton();
			this.wLabel6 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel5 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel4 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel3 = new LumiSoft.UI.Controls.WLabel();
			this.m_pUndeliveredWarning = new LumiSoft.UI.Controls.WSpinEdit();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.m_pUndelivered = new LumiSoft.UI.Controls.WSpinEdit();
			this.wLabel2 = new LumiSoft.UI.Controls.WLabel();
			this.m_pRelayRetryInterval = new LumiSoft.UI.Controls.WSpinEdit();
			this.mt_RelayRetryInterval = new LumiSoft.UI.Controls.WLabel();
			this.m_pRelayInterval = new LumiSoft.UI.Controls.WSpinEdit();
			this.mt_RelayInterval = new LumiSoft.UI.Controls.WLabel();
			this.m_pMaxThreads = new LumiSoft.UI.Controls.WSpinEdit();
			this.mt_maxthreads = new LumiSoft.UI.Controls.WLabel();
			this.m_pUseSmartHost = new System.Windows.Forms.RadioButton();
			this.m_pDNS = new System.Windows.Forms.RadioButton();
			this.mt_smarthost = new LumiSoft.UI.Controls.WLabel();
			this.mt_pridns = new LumiSoft.UI.Controls.WLabel();
			this.m_pSmartHost = new LumiSoft.UI.Controls.WEditBox();
			this.m_pSecDNS = new LumiSoft.UI.Controls.WEditBox();
			this.mt_secdns = new LumiSoft.UI.Controls.WLabel();
			this.m_pPriDNS = new LumiSoft.UI.Controls.WEditBox();
			this.m_pStoreUndeliveredMessages = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.wLabel7 = new LumiSoft.UI.Controls.WLabel();
			this.m_pTestDns = new LumiSoft.UI.Controls.WButton();
			this.SuspendLayout();
			// 
			// m_pUndeliveredText
			// 
			this.m_pUndeliveredText.DrawBorder = true;
			this.m_pUndeliveredText.Location = new System.Drawing.Point(296, 290);
			this.m_pUndeliveredText.Name = "m_pUndeliveredText";
			this.m_pUndeliveredText.Size = new System.Drawing.Size(168, 24);
			this.m_pUndeliveredText.TabIndex = 57;
			this.m_pUndeliveredText.Text = "Edit undelivered text";
			this.m_pUndeliveredText.UseStaticViewStyle = true;
			// 
			// m_pUndeliveredText.ViewStyle
			// 
			this.m_pUndeliveredText.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pUndeliveredText_ButtonPressed);
			// 
			// m_UndelWarningText
			// 
			this.m_UndelWarningText.DrawBorder = true;
			this.m_UndelWarningText.Location = new System.Drawing.Point(296, 258);
			this.m_UndelWarningText.Name = "m_UndelWarningText";
			this.m_UndelWarningText.Size = new System.Drawing.Size(168, 24);
			this.m_UndelWarningText.TabIndex = 56;
			this.m_UndelWarningText.Text = "Edit undelivered warning text";
			this.m_UndelWarningText.UseStaticViewStyle = true;
			// 
			// m_UndelWarningText.ViewStyle
			// 
			this.m_UndelWarningText.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_UndelWarningText_ButtonPressed);
			// 
			// wLabel6
			// 
			this.wLabel6.Location = new System.Drawing.Point(232, 200);
			this.wLabel6.Name = "wLabel6";
			this.wLabel6.Size = new System.Drawing.Size(48, 24);
			this.wLabel6.TabIndex = 55;
			this.wLabel6.Text = "seconds";
			this.wLabel6.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel6.ViewStyle
			// 
			// 
			// wLabel5
			// 
			this.wLabel5.Location = new System.Drawing.Point(232, 176);
			this.wLabel5.Name = "wLabel5";
			this.wLabel5.Size = new System.Drawing.Size(48, 24);
			this.wLabel5.TabIndex = 54;
			this.wLabel5.Text = "seconds";
			this.wLabel5.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel5.ViewStyle
			// 
			// 
			// wLabel4
			// 
			this.wLabel4.Location = new System.Drawing.Point(232, 288);
			this.wLabel4.Name = "wLabel4";
			this.wLabel4.Size = new System.Drawing.Size(48, 24);
			this.wLabel4.TabIndex = 53;
			this.wLabel4.Text = "hours";
			this.wLabel4.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel4.ViewStyle
			// 
			// 
			// wLabel3
			// 
			this.wLabel3.Location = new System.Drawing.Point(232, 264);
			this.wLabel3.Name = "wLabel3";
			this.wLabel3.Size = new System.Drawing.Size(48, 24);
			this.wLabel3.TabIndex = 52;
			this.wLabel3.Text = "minutes";
			this.wLabel3.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel3.ViewStyle
			// 
			// 
			// m_pUndeliveredWarning
			// 
			this.m_pUndeliveredWarning.BackColor = System.Drawing.Color.White;
			this.m_pUndeliveredWarning.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pUndeliveredWarning.DecimalPlaces = 0;
			this.m_pUndeliveredWarning.DecMaxValue = new System.Decimal(new int[] {
																					  999999999,
																					  0,
																					  0,
																					  0});
			this.m_pUndeliveredWarning.DecMinValue = new System.Decimal(new int[] {
																					  999999999,
																					  0,
																					  0,
																					  -2147483648});
			this.m_pUndeliveredWarning.DecValue = new System.Decimal(new int[] {
																				   0,
																				   0,
																				   0,
																				   0});
			this.m_pUndeliveredWarning.DrawBorder = true;
			this.m_pUndeliveredWarning.Location = new System.Drawing.Point(168, 264);
			this.m_pUndeliveredWarning.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pUndeliveredWarning.MaxLength = 32767;
			this.m_pUndeliveredWarning.Name = "m_pUndeliveredWarning";
			this.m_pUndeliveredWarning.ReadOnly = false;
			this.m_pUndeliveredWarning.Size = new System.Drawing.Size(64, 20);
			this.m_pUndeliveredWarning.TabIndex = 51;
			this.m_pUndeliveredWarning.Text = "0";
			this.m_pUndeliveredWarning.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pUndeliveredWarning.UseStaticViewStyle = false;
			// 
			// m_pUndeliveredWarning.ViewStyle
			// 
			this.m_pUndeliveredWarning.ViewStyle.EditReadOnlyColor = System.Drawing.Color.White;
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(8, 288);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(160, 24);
			this.wLabel1.TabIndex = 50;
			this.wLabel1.Text = "Send undelivered after";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// m_pUndelivered
			// 
			this.m_pUndelivered.BackColor = System.Drawing.Color.White;
			this.m_pUndelivered.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pUndelivered.DecimalPlaces = 0;
			this.m_pUndelivered.DecMaxValue = new System.Decimal(new int[] {
																			   999999999,
																			   0,
																			   0,
																			   0});
			this.m_pUndelivered.DecMinValue = new System.Decimal(new int[] {
																			   999999999,
																			   0,
																			   0,
																			   -2147483648});
			this.m_pUndelivered.DecValue = new System.Decimal(new int[] {
																			0,
																			0,
																			0,
																			0});
			this.m_pUndelivered.DrawBorder = true;
			this.m_pUndelivered.Location = new System.Drawing.Point(168, 288);
			this.m_pUndelivered.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pUndelivered.MaxLength = 32767;
			this.m_pUndelivered.Name = "m_pUndelivered";
			this.m_pUndelivered.ReadOnly = false;
			this.m_pUndelivered.Size = new System.Drawing.Size(64, 20);
			this.m_pUndelivered.TabIndex = 49;
			this.m_pUndelivered.Text = "0";
			this.m_pUndelivered.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pUndelivered.UseStaticViewStyle = false;
			// 
			// m_pUndelivered.ViewStyle
			// 
			this.m_pUndelivered.ViewStyle.EditReadOnlyColor = System.Drawing.Color.White;
			// 
			// wLabel2
			// 
			this.wLabel2.Location = new System.Drawing.Point(8, 264);
			this.wLabel2.Name = "wLabel2";
			this.wLabel2.Size = new System.Drawing.Size(160, 24);
			this.wLabel2.TabIndex = 48;
			this.wLabel2.Text = "Send undelivered warning after";
			this.wLabel2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel2.ViewStyle
			// 
			// 
			// m_pRelayRetryInterval
			// 
			this.m_pRelayRetryInterval.BackColor = System.Drawing.Color.White;
			this.m_pRelayRetryInterval.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pRelayRetryInterval.DecimalPlaces = 0;
			this.m_pRelayRetryInterval.DecMaxValue = new System.Decimal(new int[] {
																					  999999999,
																					  0,
																					  0,
																					  0});
			this.m_pRelayRetryInterval.DecMinValue = new System.Decimal(new int[] {
																					  999999999,
																					  0,
																					  0,
																					  -2147483648});
			this.m_pRelayRetryInterval.DecValue = new System.Decimal(new int[] {
																				   0,
																				   0,
																				   0,
																				   0});
			this.m_pRelayRetryInterval.DrawBorder = true;
			this.m_pRelayRetryInterval.Location = new System.Drawing.Point(168, 200);
			this.m_pRelayRetryInterval.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pRelayRetryInterval.MaxLength = 32767;
			this.m_pRelayRetryInterval.Name = "m_pRelayRetryInterval";
			this.m_pRelayRetryInterval.ReadOnly = false;
			this.m_pRelayRetryInterval.Size = new System.Drawing.Size(64, 20);
			this.m_pRelayRetryInterval.TabIndex = 47;
			this.m_pRelayRetryInterval.Text = "0";
			this.m_pRelayRetryInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pRelayRetryInterval.UseStaticViewStyle = false;
			// 
			// m_pRelayRetryInterval.ViewStyle
			// 
			this.m_pRelayRetryInterval.ViewStyle.EditReadOnlyColor = System.Drawing.Color.White;
			// 
			// mt_RelayRetryInterval
			// 
			this.mt_RelayRetryInterval.Location = new System.Drawing.Point(16, 200);
			this.mt_RelayRetryInterval.Name = "mt_RelayRetryInterval";
			this.mt_RelayRetryInterval.TabIndex = 46;
			this.mt_RelayRetryInterval.Text = "Relay retry interval";
			this.mt_RelayRetryInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_RelayRetryInterval.ViewStyle
			// 
			// 
			// m_pRelayInterval
			// 
			this.m_pRelayInterval.BackColor = System.Drawing.Color.White;
			this.m_pRelayInterval.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pRelayInterval.DecimalPlaces = 0;
			this.m_pRelayInterval.DecMaxValue = new System.Decimal(new int[] {
																				 999999999,
																				 0,
																				 0,
																				 0});
			this.m_pRelayInterval.DecMinValue = new System.Decimal(new int[] {
																				 999999999,
																				 0,
																				 0,
																				 -2147483648});
			this.m_pRelayInterval.DecValue = new System.Decimal(new int[] {
																			  0,
																			  0,
																			  0,
																			  0});
			this.m_pRelayInterval.DrawBorder = true;
			this.m_pRelayInterval.Location = new System.Drawing.Point(168, 176);
			this.m_pRelayInterval.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pRelayInterval.MaxLength = 32767;
			this.m_pRelayInterval.Name = "m_pRelayInterval";
			this.m_pRelayInterval.ReadOnly = false;
			this.m_pRelayInterval.Size = new System.Drawing.Size(64, 20);
			this.m_pRelayInterval.TabIndex = 45;
			this.m_pRelayInterval.Text = "0";
			this.m_pRelayInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pRelayInterval.UseStaticViewStyle = false;
			// 
			// m_pRelayInterval.ViewStyle
			// 
			this.m_pRelayInterval.ViewStyle.EditReadOnlyColor = System.Drawing.Color.White;
			// 
			// mt_RelayInterval
			// 
			this.mt_RelayInterval.Location = new System.Drawing.Point(16, 176);
			this.mt_RelayInterval.Name = "mt_RelayInterval";
			this.mt_RelayInterval.TabIndex = 44;
			this.mt_RelayInterval.Text = "Relay interval";
			this.mt_RelayInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_RelayInterval.ViewStyle
			// 
			// 
			// m_pMaxThreads
			// 
			this.m_pMaxThreads.BackColor = System.Drawing.Color.White;
			this.m_pMaxThreads.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pMaxThreads.DecimalPlaces = 0;
			this.m_pMaxThreads.DecMaxValue = new System.Decimal(new int[] {
																			  999999999,
																			  0,
																			  0,
																			  0});
			this.m_pMaxThreads.DecMinValue = new System.Decimal(new int[] {
																			  999999999,
																			  0,
																			  0,
																			  -2147483648});
			this.m_pMaxThreads.DecValue = new System.Decimal(new int[] {
																		   0,
																		   0,
																		   0,
																		   0});
			this.m_pMaxThreads.DrawBorder = true;
			this.m_pMaxThreads.Location = new System.Drawing.Point(168, 128);
			this.m_pMaxThreads.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pMaxThreads.MaxLength = 32767;
			this.m_pMaxThreads.Name = "m_pMaxThreads";
			this.m_pMaxThreads.ReadOnly = false;
			this.m_pMaxThreads.Size = new System.Drawing.Size(64, 20);
			this.m_pMaxThreads.TabIndex = 42;
			this.m_pMaxThreads.Text = "0";
			this.m_pMaxThreads.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pMaxThreads.UseStaticViewStyle = false;
			// 
			// m_pMaxThreads.ViewStyle
			// 
			this.m_pMaxThreads.ViewStyle.EditReadOnlyColor = System.Drawing.Color.White;
			// 
			// mt_maxthreads
			// 
			this.mt_maxthreads.Location = new System.Drawing.Point(16, 128);
			this.mt_maxthreads.Name = "mt_maxthreads";
			this.mt_maxthreads.TabIndex = 41;
			this.mt_maxthreads.Text = "Max. Delivery Threads";
			this.mt_maxthreads.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_maxthreads.ViewStyle
			// 
			// 
			// m_pUseSmartHost
			// 
			this.m_pUseSmartHost.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.m_pUseSmartHost.Location = new System.Drawing.Point(40, 32);
			this.m_pUseSmartHost.Name = "m_pUseSmartHost";
			this.m_pUseSmartHost.Size = new System.Drawing.Size(176, 24);
			this.m_pUseSmartHost.TabIndex = 33;
			this.m_pUseSmartHost.Text = "Send mails through SmartHost";
			this.m_pUseSmartHost.CheckedChanged += new System.EventHandler(this.m_pDNS_CheckedChanged);
			// 
			// m_pDNS
			// 
			this.m_pDNS.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.m_pDNS.Location = new System.Drawing.Point(40, 56);
			this.m_pDNS.Name = "m_pDNS";
			this.m_pDNS.Size = new System.Drawing.Size(176, 24);
			this.m_pDNS.TabIndex = 34;
			this.m_pDNS.Text = "Send mails directly using DNS";
			this.m_pDNS.CheckedChanged += new System.EventHandler(this.m_pDNS_CheckedChanged);
			// 
			// mt_smarthost
			// 
			this.mt_smarthost.Location = new System.Drawing.Point(224, 32);
			this.mt_smarthost.Name = "mt_smarthost";
			this.mt_smarthost.Size = new System.Drawing.Size(86, 24);
			this.mt_smarthost.TabIndex = 35;
			this.mt_smarthost.Text = "SmartHost";
			this.mt_smarthost.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_smarthost.ViewStyle
			// 
			// 
			// mt_pridns
			// 
			this.mt_pridns.Location = new System.Drawing.Point(224, 56);
			this.mt_pridns.Name = "mt_pridns";
			this.mt_pridns.Size = new System.Drawing.Size(86, 24);
			this.mt_pridns.TabIndex = 36;
			this.mt_pridns.Text = "Primary DNS";
			this.mt_pridns.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_pridns.ViewStyle
			// 
			// 
			// m_pSmartHost
			// 
			this.m_pSmartHost.DrawBorder = true;
			this.m_pSmartHost.Location = new System.Drawing.Point(312, 32);
			this.m_pSmartHost.Name = "m_pSmartHost";
			this.m_pSmartHost.TabIndex = 38;
			this.m_pSmartHost.UseStaticViewStyle = true;
			// 
			// m_pSmartHost.ViewStyle
			// 
			// 
			// m_pSecDNS
			// 
			this.m_pSecDNS.DrawBorder = true;
			this.m_pSecDNS.Location = new System.Drawing.Point(312, 80);
			this.m_pSecDNS.Name = "m_pSecDNS";
			this.m_pSecDNS.TabIndex = 40;
			this.m_pSecDNS.UseStaticViewStyle = true;
			// 
			// m_pSecDNS.ViewStyle
			// 
			// 
			// mt_secdns
			// 
			this.mt_secdns.Location = new System.Drawing.Point(216, 80);
			this.mt_secdns.Name = "mt_secdns";
			this.mt_secdns.Size = new System.Drawing.Size(94, 24);
			this.mt_secdns.TabIndex = 37;
			this.mt_secdns.Text = "Secondary DNS";
			this.mt_secdns.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_secdns.ViewStyle
			// 
			// 
			// m_pPriDNS
			// 
			this.m_pPriDNS.DrawBorder = true;
			this.m_pPriDNS.Location = new System.Drawing.Point(312, 56);
			this.m_pPriDNS.Name = "m_pPriDNS";
			this.m_pPriDNS.TabIndex = 39;
			this.m_pPriDNS.UseStaticViewStyle = true;
			// 
			// m_pPriDNS.ViewStyle
			// 
			// 
			// m_pStoreUndeliveredMessages
			// 
			this.m_pStoreUndeliveredMessages.Checked = false;
			this.m_pStoreUndeliveredMessages.DrawBorder = true;
			this.m_pStoreUndeliveredMessages.Location = new System.Drawing.Point(8, 320);
			this.m_pStoreUndeliveredMessages.Name = "m_pStoreUndeliveredMessages";
			this.m_pStoreUndeliveredMessages.ReadOnly = false;
			this.m_pStoreUndeliveredMessages.Size = new System.Drawing.Size(16, 22);
			this.m_pStoreUndeliveredMessages.TabIndex = 58;
			this.m_pStoreUndeliveredMessages.UseStaticViewStyle = false;
			// 
			// m_pStoreUndeliveredMessages.ViewStyle
			// 
			// 
			// wLabel7
			// 
			this.wLabel7.Location = new System.Drawing.Point(24, 320);
			this.wLabel7.Name = "wLabel7";
			this.wLabel7.TabIndex = 59;
			this.wLabel7.Text = "Store undelivered messages";
			this.wLabel7.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel7.ViewStyle
			// 
			// 
			// m_pTestDns
			// 
			this.m_pTestDns.DrawBorder = true;
			this.m_pTestDns.Location = new System.Drawing.Point(120, 80);
			this.m_pTestDns.Name = "m_pTestDns";
			this.m_pTestDns.Size = new System.Drawing.Size(96, 24);
			this.m_pTestDns.TabIndex = 60;
			this.m_pTestDns.Text = "Test Dns";
			this.m_pTestDns.UseStaticViewStyle = true;
			// 
			// m_pTestDns.ViewStyle
			// 
			this.m_pTestDns.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pTestDns_ButtonPressed);
			// 
			// wfrm_System_Delivery
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 357);
			this.Controls.Add(this.m_pTestDns);
			this.Controls.Add(this.wLabel7);
			this.Controls.Add(this.m_pStoreUndeliveredMessages);
			this.Controls.Add(this.m_pUndeliveredText);
			this.Controls.Add(this.m_UndelWarningText);
			this.Controls.Add(this.wLabel6);
			this.Controls.Add(this.wLabel5);
			this.Controls.Add(this.wLabel4);
			this.Controls.Add(this.wLabel3);
			this.Controls.Add(this.m_pUndeliveredWarning);
			this.Controls.Add(this.wLabel1);
			this.Controls.Add(this.m_pUndelivered);
			this.Controls.Add(this.wLabel2);
			this.Controls.Add(this.m_pRelayRetryInterval);
			this.Controls.Add(this.mt_RelayRetryInterval);
			this.Controls.Add(this.m_pRelayInterval);
			this.Controls.Add(this.mt_RelayInterval);
			this.Controls.Add(this.m_pMaxThreads);
			this.Controls.Add(this.mt_maxthreads);
			this.Controls.Add(this.m_pUseSmartHost);
			this.Controls.Add(this.m_pDNS);
			this.Controls.Add(this.mt_smarthost);
			this.Controls.Add(this.mt_pridns);
			this.Controls.Add(this.m_pSmartHost);
			this.Controls.Add(this.m_pSecDNS);
			this.Controls.Add(this.mt_secdns);
			this.Controls.Add(this.m_pPriDNS);
			this.Name = "wfrm_System_Delivery";
			this.Text = "wfrm_System_Delivery";
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		#region function m_pDNS_CheckedChanged

		private void m_pDNS_CheckedChanged(object sender, System.EventArgs e)
		{
			RefreshRadioBtns();
		}

		#endregion

		#region function m_pTestDns_ButtonPressed

		private void m_pTestDns_ButtonPressed(object sender, System.EventArgs e)
		{
			DnsEx.DnsServers  = new string[]{m_pPriDNS.Text};
			DnsEx.UseDnsCache = false;
			DnsEx dns = new DnsEx();

			MX_Record[] mx = null;
			DnsReplyCode reply = dns.GetMXRecords("lumisoft.ee",out mx);
			if(reply != DnsReplyCode.Ok){
				MessageBox.Show(this,"Invalid dns server(" + m_pPriDNS.Text + "), can't resolve lumisoft.ee","Info",MessageBoxButtons.OK,MessageBoxIcon.Warning);
				return;
			}

			DnsEx.DnsServers  = new string[]{m_pSecDNS.Text};
			DnsEx.UseDnsCache = false;
			DnsEx dns2 = new DnsEx();

			MX_Record[] mx2 = null;
			DnsReplyCode reply2 = dns2.GetMXRecords("lumisoft.ee",out mx2);
			if(reply2 != DnsReplyCode.Ok){
				MessageBox.Show(this,"Invalid dns server(" + m_pSecDNS.Text + "), can't resolve lumisoft.ee","Info",MessageBoxButtons.OK,MessageBoxIcon.Warning);
				return;
			}

			MessageBox.Show(this,"Ok.","Info",MessageBoxButtons.OK,MessageBoxIcon.Information);
		}

		#endregion


		#region function m_UndelWarningText_ButtonPressed

		private void m_UndelWarningText_ButtonPressed(object sender, System.EventArgs e)
		{
			DataRow dr = dsVal.Tables["Settings"].Rows[0];

			wfrm_EditUndeliveredText frm = new wfrm_EditUndeliveredText(dr["UndeliveredWarningTemplate"].ToString());
			if(frm.ShowDialog(this) == DialogResult.OK){
				dr["UndeliveredWarningTemplate"] = frm.wp_Template;
			}
		}

		#endregion

		#region function m_pUndeliveredText_ButtonPressed

		private void m_pUndeliveredText_ButtonPressed(object sender, System.EventArgs e)
		{
			DataRow dr = dsVal.Tables["Settings"].Rows[0];

			wfrm_EditUndeliveredText frm = new wfrm_EditUndeliveredText(dr["UndeliveredTemplate"].ToString());
			if(frm.ShowDialog(this) == DialogResult.OK){
				dr["UndeliveredTemplate"] = frm.wp_Template;
			}
		}

		#endregion

		#endregion


		#region function LoadData

		private void LoadData(DataSet ds)
		{
			try
			{
				DataRow dr = ds.Tables["Settings"].Rows[0];
				m_pSmartHost.Text          = dr["SmartHost"].ToString();
				m_pPriDNS.Text             = dr["Dns1"].ToString();
				m_pSecDNS.Text             = dr["Dns2"].ToString();
				m_pMaxThreads.Text         = dr["MaxRelayThreads"].ToString();
				m_pRelayInterval.Text      = dr["RelayInterval"].ToString();
				m_pRelayRetryInterval.Text = dr["RelayRetryInterval"].ToString();
				m_pUndelivered.Text        = dr["RelayUndelivered"].ToString();
				m_pUndeliveredWarning.Text = dr["RelayUndeliveredWarning"].ToString();
				m_pStoreUndeliveredMessages.Checked = Convert.ToBoolean(dr["StoreUndeliveredMessages"]);

				bool UseSmartHost = Convert.ToBoolean(dr["UseSmartHost"]);
				if(UseSmartHost){
					m_pUseSmartHost.Checked = true;
				}
				else{
					m_pDNS.Checked = true;
				}

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
			if(dr["SmartHost"].ToString()		        != m_pSmartHost.Text)          { dr["SmartHost"]               =  m_pSmartHost.Text; }
			if(dr["Dns1"].ToString()				    != m_pPriDNS.Text)             { dr["Dns1"]                    =  m_pPriDNS.Text; }
			if(dr["Dns2"].ToString()			        != m_pSecDNS.Text)             { dr["Dns2"]                    =  m_pSecDNS.Text; }
			if(dr["MaxRelayThreads"].ToString()      	!= m_pMaxThreads.Text)         { dr["MaxRelayThreads"]         =  m_pMaxThreads.Text; }
			if(dr["RelayInterval"].ToString()	        != m_pRelayInterval.Text)      { dr["RelayInterval"]           =  m_pRelayInterval.Text; }
			if(dr["RelayRetryInterval"].ToString()		!= m_pRelayRetryInterval.Text) { dr["RelayRetryInterval"]      =  m_pRelayRetryInterval.Text; }
			if(dr["RelayUndelivered"].ToString()		!= m_pUndelivered.Text)        { dr["RelayUndelivered"]        =  m_pUndelivered.Text; }
			if(dr["RelayUndeliveredWarning"].ToString()	!= m_pUndeliveredWarning.Text) { dr["RelayUndeliveredWarning"] =  m_pUndeliveredWarning.Text; }
			if(Convert.ToBoolean(dr["UseSmartHost"])    != m_pUseSmartHost.Checked)    { dr["UseSmartHost"]            =  m_pUseSmartHost.Checked; }
			if(Convert.ToBoolean(dr["StoreUndeliveredMessages"]) != m_pStoreUndeliveredMessages.Checked) { dr["StoreUndeliveredMessages"] =  m_pStoreUndeliveredMessages.Checked; }
		}

		#endregion


		#region function RefreshRadioBtns

		private void RefreshRadioBtns()
		{
			if(m_pDNS.Checked){
				m_pPriDNS.Enabled    = true;
				m_pSecDNS.Enabled    = true;
				m_pSmartHost.Enabled = false;
				m_pTestDns.Enabled   = true;
			}
			else{
				m_pPriDNS.Enabled    = false;
				m_pSecDNS.Enabled    = false;
				m_pSmartHost.Enabled = true;
				m_pTestDns.Enabled   = false;
			}
		}

		#endregion

		#region function IsIpValid

		private bool IsIpValid(string IP)
		{
			try
			{
				string[] ipBlocks = IP.Split(new char[]{'.'});
				if(ipBlocks.Length != 4){
					return false;
				}

				System.Net.IPAddress ip = System.Net.IPAddress.Parse(IP);
				return true;
			}
			catch{//(Exception x){
				return false;
			}
		}

		#endregion
		
	}
}
