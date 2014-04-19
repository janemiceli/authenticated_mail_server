using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Net;

using LumiSoft.MailServer.API.UserAPI;
using LumiSoft.Net.Dns.Client;

namespace LumiSoft.MailServer.UI
{
    /// <summary>
    /// System Services Relay settings window.
    /// </summary>
    public class wfrm_System_Services_Relay : Form
    {
        //--- Common UI -------------------------------------
        private TabControl    m_pTab                  = null;
        private Button        m_pApply                = null;
        //--- Tabpage General UI ----------------------------
        private RadioButton   m_pSendSmartHost        = null;
        private Label         mt_SmartHost            = null;
        private TextBox       m_pSmartHost            = null;
        private NumericUpDown m_pSmartHostPort        = null;
        private CheckBox      m_pUseSSL               = null;
        private Label         mt_SmartHostUser        = null;
        private TextBox       m_pSmartHostUser        = null;
        private Label         mt_SmartHostPassword    = null;
        private TextBox       m_pSmartHostPassword    = null;
        private RadioButton   m_pSendDns              = null;
        private Label         mt_Dns1                 = null;
        private TextBox       m_pDns1                 = null;
        private Label         mt_Dns2                 = null;
        private TextBox       m_pDns2                 = null;
        private Button        m_pTestDns              = null;
        private Label         mt_HostName             = null;
        private TextBox       m_pHostName             = null;
        private Label         mt_SessionTimeout       = null;
        private NumericUpDown m_pSessionTimeout       = null;
        private Label         mt_SessTimeoutSec       = null;
        private Label         mt_MaxConnections       = null;
        private NumericUpDown m_pMaxConnections       = null;
        private Label         mt_MaxConnsPerIP        = null;
        private NumericUpDown m_pMaxConnsPerIP        = null;
        private Label         mt_MaxConnsPerIP0       = null;
        private Label         mt_RelayInterval        = null;
        private NumericUpDown m_pRelayInterval        = null;
        private Label         mt_RelayIntervalSeconds = null;
        private Label         mt_RelayRetryInterval   = null;
        private NumericUpDown m_pRelayRetryInterval   = null;
        private Label         mt_RelayRetryIntervSec  = null;
        private Label         mt_SendUndelWarning     = null;
        private NumericUpDown m_pSendUndelWarning     = null;
        private Label         mt_SendUndelWarnMinutes = null;
        private Label         mt_SendUndelivered      = null;
        private NumericUpDown m_pSendUndelivered      = null;
        private Label         mt_SendUndeliveredHours = null;
        private Label         mt_SendingIP            = null;
        private ComboBox      m_pSendingIP            = null;
        private CheckBox      m_pStoreUndeliveredMsgs = null;
        //--------------------------------------------------

        private VirtualServer m_pVirtualServer = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="virtualServer">Virtual server.</param>
        public wfrm_System_Services_Relay(VirtualServer virtualServer)
        {
            m_pVirtualServer = virtualServer;

            InitUI();

            foreach(IPAddress ip in virtualServer.Server.IPAddresses){
                if(ip.Equals(IPAddress.Any)){
                    m_pSendingIP.Items.Add(new WComboBoxItem("any IPv4",ip));
                }
                else if(ip.Equals(IPAddress.Loopback)){
                    m_pSendingIP.Items.Add(new WComboBoxItem("localhost IPv4",ip));
                }
                else if(ip.Equals(IPAddress.IPv6Any)){
                    m_pSendingIP.Items.Add(new WComboBoxItem("Any IPv6",ip));
                }
                else if(ip.Equals(IPAddress.IPv6Loopback)){
                    m_pSendingIP.Items.Add(new WComboBoxItem("localhost IPv6",ip));
                }
                else{
                    m_pSendingIP.Items.Add(new WComboBoxItem(ip.ToString(),ip));
                }
            }
            m_pSendingIP.SelectedIndex = 0;

            LoadData();
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes window UI.
        /// </summary>
        private void InitUI()
        {
            //--- Common UI -------------------------------------//
            m_pTab = new TabControl();
            m_pTab.Size = new Size(515,490);
            m_pTab.Location = new Point(5,0);
            m_pTab.TabPages.Add(new TabPage("General"));

            m_pApply = new Button();
            m_pApply.Size = new Size(70,20);
            m_pApply.Location = new Point(450,500);
            m_pApply.Text = "Apply";
            m_pApply.Click += new EventHandler(m_pApply_Click);
            //---------------------------------------------------//

            //--- Tabpage General UI ----------------------------//
            m_pSendSmartHost = new RadioButton();
            m_pSendSmartHost.Size = new Size(250,20);
            m_pSendSmartHost.Location = new Point(10,15);
            m_pSendSmartHost.Text = "Send mails through SmartHost";
            m_pSendSmartHost.CheckedChanged += new EventHandler(m_pSendSmartHost_CheckedChanged);

            mt_SmartHost = new Label();
            mt_SmartHost.Size = new Size(100,20);
            mt_SmartHost.Location = new Point(10,35);
            mt_SmartHost.TextAlign = ContentAlignment.MiddleRight;
            mt_SmartHost.Text = "Smart Host:";

            m_pSmartHost = new TextBox();
            m_pSmartHost.Size = new Size(170,20);
            m_pSmartHost.Location = new Point(115,35);

            m_pSmartHostPort = new NumericUpDown();
            m_pSmartHostPort.Size = new Size(70,20);
            m_pSmartHostPort.Location = new Point(290,35);
            m_pSmartHostPort.Minimum = 1;
            m_pSmartHostPort.Maximum = 99999;

            m_pUseSSL = new CheckBox();
            m_pUseSSL.Size = new Size(70,20);
            m_pUseSSL.Location = new Point(365,35);
            m_pUseSSL.Text = "Use SSL";
            m_pUseSSL.CheckedChanged += new EventHandler(m_pUseSSL_CheckedChanged);

            mt_SmartHostUser = new Label();
            mt_SmartHostUser.Size = new Size(100,20);
            mt_SmartHostUser.Location = new Point(10,60);
            mt_SmartHostUser.TextAlign = ContentAlignment.MiddleRight;
            mt_SmartHostUser.Text = "User Name:";

            m_pSmartHostUser = new TextBox();
            m_pSmartHostUser.Size = new Size(170,20);
            m_pSmartHostUser.Location = new Point(115,60);

            mt_SmartHostPassword = new Label();
            mt_SmartHostPassword.Size = new Size(100,20);
            mt_SmartHostPassword.Location = new Point(10,80);
            mt_SmartHostPassword.TextAlign = ContentAlignment.MiddleRight;
            mt_SmartHostPassword.Text = "Password:";

            m_pSmartHostPassword = new TextBox();
            m_pSmartHostPassword.Size = new Size(170,20);
            m_pSmartHostPassword.Location = new Point(115,80);

            m_pSendDns = new RadioButton();
            m_pSendDns.Size = new Size(250,20);
            m_pSendDns.Location = new Point(10,110);
            m_pSendDns.Text = "Send mails directly using DNS";
            m_pSendDns.CheckedChanged += new EventHandler(m_pSendSmartHost_CheckedChanged);

            mt_Dns1 = new Label();
            mt_Dns1.Size = new Size(100,20);
            mt_Dns1.Location = new Point(10,130);
            mt_Dns1.TextAlign = ContentAlignment.MiddleRight;
            mt_Dns1.Text = "DNS 1:";

            m_pDns1 = new TextBox();
            m_pDns1.Size = new Size(170,20);
            m_pDns1.Location = new Point(115,130);

            mt_Dns2 = new Label();
            mt_Dns2.Size = new Size(100,20);
            mt_Dns2.Location = new Point(10,150);
            mt_Dns2.TextAlign = ContentAlignment.MiddleRight;
            mt_Dns2.Text = "DNS 2:";

            m_pDns2 = new TextBox();
            m_pDns2.Size = new Size(170,20);
            m_pDns2.Location = new Point(115,150);

            m_pTestDns = new Button();
            m_pTestDns.Size = new Size(70,20);
            m_pTestDns.Location = new Point(290,130);
            m_pTestDns.Text = "Test Dns";
            m_pTestDns.Click += new EventHandler(m_pTestDns_Click);

            mt_HostName = new Label();
            mt_HostName.Size = new Size(200,20);
            mt_HostName.Location = new Point(10,180);
            mt_HostName.TextAlign = ContentAlignment.MiddleRight;
            mt_HostName.Text = "Host Name:";

            m_pHostName = new TextBox();
            m_pHostName.Size = new Size(285,20);
            m_pHostName.Location = new Point(215,180);
            
            mt_SessionTimeout = new Label();
            mt_SessionTimeout.Size = new Size(200,20);
            mt_SessionTimeout.Location = new Point(10,210);
            mt_SessionTimeout.TextAlign = ContentAlignment.MiddleRight;
            mt_SessionTimeout.Text = "Session Idle Timeout:";

            m_pSessionTimeout = new NumericUpDown();
            m_pSessionTimeout.Size = new Size(70,20);
            m_pSessionTimeout.Location = new Point(215,210);
            m_pSessionTimeout.Minimum = 10;
            m_pSessionTimeout.Maximum = 99999;

            mt_SessTimeoutSec = new Label();
            mt_SessTimeoutSec.Size = new Size(70,20);
            mt_SessTimeoutSec.Location = new Point(290,210);
            mt_SessTimeoutSec.TextAlign = ContentAlignment.MiddleLeft;
            mt_SessTimeoutSec.Text = "seconds";

            mt_MaxConnections = new Label();
            mt_MaxConnections.Size = new Size(200,20);
            mt_MaxConnections.Location = new Point(10,240);
            mt_MaxConnections.TextAlign = ContentAlignment.MiddleRight;
            mt_MaxConnections.Text = "Maximum Connections:";
                        
            m_pMaxConnections = new NumericUpDown();
            m_pMaxConnections.Size = new Size(70,20);
            m_pMaxConnections.Location = new Point(215,240);
            m_pMaxConnections.Minimum = 1;
            m_pMaxConnections.Maximum = 99999;

            mt_MaxConnsPerIP = new Label();
            mt_MaxConnsPerIP.Size = new Size(200,20);
            mt_MaxConnsPerIP.Location = new Point(1,265);
            mt_MaxConnsPerIP.TextAlign = ContentAlignment.MiddleRight;
            mt_MaxConnsPerIP.Text = "Maximum Connections per IP:";

            m_pMaxConnsPerIP = new NumericUpDown();
            m_pMaxConnsPerIP.Size = new Size(70,20);
            m_pMaxConnsPerIP.Location = new Point(215,265);
            m_pMaxConnsPerIP.Minimum = 0;
            m_pMaxConnsPerIP.Maximum = 99999;

            mt_MaxConnsPerIP0 = new Label();
            mt_MaxConnsPerIP0.Size = new Size(164,20);
            mt_MaxConnsPerIP0.Location = new Point(290,265);
            mt_MaxConnsPerIP0.TextAlign = ContentAlignment.MiddleLeft;
            mt_MaxConnsPerIP0.Text = "(0 for unlimited)";

            mt_RelayInterval = new Label();
            mt_RelayInterval.Size = new Size(200,20);
            mt_RelayInterval.Location = new Point(10,290);
            mt_RelayInterval.TextAlign = ContentAlignment.MiddleRight;
            mt_RelayInterval.Text = "Relay Interval:";

            m_pRelayInterval = new NumericUpDown();
            m_pRelayInterval.Size = new Size(70,20);
            m_pRelayInterval.Location = new Point(215,290);
            m_pRelayInterval.Minimum = 1;
            m_pRelayInterval.Maximum = 9999;

            mt_RelayIntervalSeconds = new Label();
            mt_RelayIntervalSeconds.Size = new Size(50,20);
            mt_RelayIntervalSeconds.Location = new Point(290,290);
            mt_RelayIntervalSeconds.TextAlign = ContentAlignment.MiddleLeft;
            mt_RelayIntervalSeconds.Text = "seconds";

            mt_RelayRetryInterval = new Label();
            mt_RelayRetryInterval.Size = new Size(200,20);
            mt_RelayRetryInterval.Location = new Point(10,315);
            mt_RelayRetryInterval.TextAlign = ContentAlignment.MiddleRight;
            mt_RelayRetryInterval.Text = "Relay Retry Interval:";

            m_pRelayRetryInterval = new NumericUpDown();
            m_pRelayRetryInterval.Size = new Size(70,20);
            m_pRelayRetryInterval.Location = new Point(215,315);
            m_pRelayRetryInterval.Minimum = 1;
            m_pRelayRetryInterval.Maximum = 9999;

            mt_RelayRetryIntervSec = new Label();
            mt_RelayRetryIntervSec.Size = new Size(50,20);
            mt_RelayRetryIntervSec.Location = new Point(290,315);
            mt_RelayRetryIntervSec.TextAlign = ContentAlignment.MiddleLeft;
            mt_RelayRetryIntervSec.Text = "seconds";

            mt_SendUndelWarning = new Label();
            mt_SendUndelWarning.Size = new Size(200,20);
            mt_SendUndelWarning.Location = new Point(10,345);
            mt_SendUndelWarning.TextAlign = ContentAlignment.MiddleRight;
            mt_SendUndelWarning.Text = "Send undelivered warning after:";

            m_pSendUndelWarning = new NumericUpDown();
            m_pSendUndelWarning.Size = new Size(70,20);
            m_pSendUndelWarning.Location = new Point(215,345);
            m_pSendUndelWarning.Minimum = 1;
            m_pSendUndelWarning.Maximum = 9999;

            mt_SendUndelWarnMinutes = new Label();
            mt_SendUndelWarnMinutes.Size = new Size(50,20);
            mt_SendUndelWarnMinutes.Location = new Point(290,345);
            mt_SendUndelWarnMinutes.TextAlign = ContentAlignment.MiddleLeft;
            mt_SendUndelWarnMinutes.Text = "minutes";

            mt_SendUndelivered = new Label();
            mt_SendUndelivered.Size = new Size(200,20);
            mt_SendUndelivered.Location = new Point(10,370);
            mt_SendUndelivered.TextAlign = ContentAlignment.MiddleRight;
            mt_SendUndelivered.Text = "Send undelivered after:";

            m_pSendUndelivered = new NumericUpDown();
            m_pSendUndelivered.Size = new Size(70,20);
            m_pSendUndelivered.Location = new Point(215,370);
            m_pSendUndelivered.Minimum = 1;
            m_pSendUndelivered.Maximum = 999;

            mt_SendUndeliveredHours = new Label();
            mt_SendUndeliveredHours.Size = new Size(50,20);
            mt_SendUndeliveredHours.Location = new Point(290,370);
            mt_SendUndeliveredHours.TextAlign = ContentAlignment.MiddleLeft;
            mt_SendUndeliveredHours.Text = "hours";
                        
            mt_SendingIP = new Label();
            mt_SendingIP.Size = new Size(100,20);
            mt_SendingIP.Location = new Point(10,415);
            mt_SendingIP.Text = "Sending IP:";

            m_pSendingIP = new ComboBox();
            m_pSendingIP.Size = new Size(150,20);
            m_pSendingIP.Location = new Point(10,435);
            m_pSendingIP.DropDownStyle = ComboBoxStyle.DropDownList;

            m_pStoreUndeliveredMsgs = new CheckBox();
            m_pStoreUndeliveredMsgs.Size = new Size(250,20);
            m_pStoreUndeliveredMsgs.Location = new Point(180,435);
            m_pStoreUndeliveredMsgs.Text = "Store undelivered messages";
            
            m_pTab.TabPages[0].Controls.Add(m_pSendSmartHost);
            m_pTab.TabPages[0].Controls.Add(mt_SmartHost);
            m_pTab.TabPages[0].Controls.Add(m_pSmartHost);
            m_pTab.TabPages[0].Controls.Add(m_pSmartHostPort);
            m_pTab.TabPages[0].Controls.Add(m_pUseSSL);
            m_pTab.TabPages[0].Controls.Add(mt_SmartHostUser);
            m_pTab.TabPages[0].Controls.Add(m_pSmartHostUser);
            m_pTab.TabPages[0].Controls.Add(mt_SmartHostPassword);
            m_pTab.TabPages[0].Controls.Add(m_pSmartHostPassword);
            m_pTab.TabPages[0].Controls.Add(m_pSendDns);
            m_pTab.TabPages[0].Controls.Add(mt_Dns1);
            m_pTab.TabPages[0].Controls.Add(m_pDns1);
            m_pTab.TabPages[0].Controls.Add(mt_Dns2);
            m_pTab.TabPages[0].Controls.Add(m_pDns2);
            m_pTab.TabPages[0].Controls.Add(m_pTestDns);
            m_pTab.TabPages[0].Controls.Add(mt_HostName);
            m_pTab.TabPages[0].Controls.Add(mt_SessionTimeout);
            m_pTab.TabPages[0].Controls.Add(m_pSessionTimeout);
            m_pTab.TabPages[0].Controls.Add(mt_SessTimeoutSec);
            m_pTab.TabPages[0].Controls.Add(m_pHostName);
            m_pTab.TabPages[0].Controls.Add(mt_MaxConnections);
            m_pTab.TabPages[0].Controls.Add(m_pMaxConnections);
            m_pTab.TabPages[0].Controls.Add(mt_MaxConnsPerIP);
            m_pTab.TabPages[0].Controls.Add(m_pMaxConnsPerIP);
            m_pTab.TabPages[0].Controls.Add(mt_MaxConnsPerIP0);
            m_pTab.TabPages[0].Controls.Add(mt_RelayInterval);
            m_pTab.TabPages[0].Controls.Add(m_pRelayInterval);
            m_pTab.TabPages[0].Controls.Add(mt_RelayIntervalSeconds);
            m_pTab.TabPages[0].Controls.Add(mt_RelayRetryInterval);
            m_pTab.TabPages[0].Controls.Add(m_pRelayRetryInterval);
            m_pTab.TabPages[0].Controls.Add(mt_RelayRetryIntervSec);
            m_pTab.TabPages[0].Controls.Add(mt_SendUndelWarning);
            m_pTab.TabPages[0].Controls.Add(m_pSendUndelWarning);
            m_pTab.TabPages[0].Controls.Add(mt_SendUndelWarnMinutes);
            m_pTab.TabPages[0].Controls.Add(mt_SendUndelivered);
            m_pTab.TabPages[0].Controls.Add(m_pSendUndelivered);
            m_pTab.TabPages[0].Controls.Add(mt_SendUndeliveredHours);
            m_pTab.TabPages[0].Controls.Add(mt_SendingIP);
            m_pTab.TabPages[0].Controls.Add(m_pSendingIP);
            m_pTab.TabPages[0].Controls.Add(m_pStoreUndeliveredMsgs);
            //-------------------------------------------------//


            // Common UI
            this.Controls.Add(m_pTab);
            this.Controls.Add(m_pApply);
        }
                                                                                
        #endregion


        #region Events Handling

        #region method OnVisibleChanged

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if(!this.Visible){
                SaveData(true); 
            }
        }

        #endregion


        #region method m_pSendSmartHost_CheckedChanged

        private void m_pSendSmartHost_CheckedChanged(object sender, EventArgs e)
        {
            if(m_pSendSmartHost.Checked){
				m_pDns1.Enabled              = false;
				m_pDns2.Enabled              = false;
				m_pSmartHost.Enabled         = true;
				m_pSmartHostPort.Enabled     = true;
                m_pUseSSL.Enabled            = true;
                m_pSmartHostUser.Enabled     = true;
                m_pSmartHostPassword.Enabled = true;
				m_pTestDns.Enabled           = false;
			}
			else{
                m_pDns1.Enabled              = true;
				m_pDns2.Enabled              = true;
				m_pSmartHost.Enabled         = false;
				m_pSmartHostPort.Enabled     = false;
                m_pUseSSL.Enabled            = false;
                m_pSmartHostUser.Enabled     = false;
                m_pSmartHostPassword.Enabled = false;
				m_pTestDns.Enabled           = true;				
			}
        }

        #endregion

        #region method m_pUseSSL_CheckedChanged

        private void m_pUseSSL_CheckedChanged(object sender, EventArgs e)
        {
            if(m_pUseSSL.Checked){
                m_pSmartHostPort.Value = 465;
            }
            else{
                m_pSmartHostPort.Value = 25;
            }
        }

        #endregion

        #region method m_pTestDns_Click

        private void m_pTestDns_Click(object sender, EventArgs e)
        {
            Dns_Client.DnsServers  = new string[]{m_pDns1.Text};
			Dns_Client.UseDnsCache = false;
			Dns_Client dns = new Dns_Client();

			DnsServerResponse response = dns.Query("lumisoft.ee",QTYPE.MX);
			if(!response.ConnectionOk || response.ResponseCode != RCODE.NO_ERROR){
				MessageBox.Show(this,"Invalid dns server(" + m_pDns1.Text + "), can't resolve lumisoft.ee","Info",MessageBoxButtons.OK,MessageBoxIcon.Warning);
				return;
			}

			Dns_Client.DnsServers  = new string[]{m_pDns2.Text};
			Dns_Client.UseDnsCache = false;
			Dns_Client dns2 = new Dns_Client();

			response = dns2.Query("lumisoft.ee",QTYPE.MX);
			if(!response.ConnectionOk || response.ResponseCode != RCODE.NO_ERROR){
				MessageBox.Show(this,"Invalid dns server(" + m_pDns2.Text + "), can't resolve lumisoft.ee","Info",MessageBoxButtons.OK,MessageBoxIcon.Warning);
				return;
			}

			MessageBox.Show(this,"Ok.","Info",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        #endregion


        #region method m_pApply_Click

        private void m_pApply_Click(object sender, EventArgs e)
        {
            SaveData(false); 
        }

        #endregion

        #endregion

        
        #region method LoadData

        /// <summary>
        /// Loads data to UI.
        /// </summary>
        private void LoadData()
        {            
            try{
                Relay_Settings settings =  m_pVirtualServer.SystemSettings.Relay;
 		
				if(settings.UseSmartHost){
					m_pSendSmartHost.Checked = true;
				}
				else{
					m_pSendDns.Checked = true;
				}
				m_pSmartHost.Text               = settings.SmartHost;
				m_pSmartHostPort.Value          = settings.SmartHostPort;
                m_pUseSSL.Checked               = settings.SmartHostSsl;
				m_pSmartHostUser.Text           = settings.SmartHostUserName;
				m_pSmartHostPassword.Text       = settings.SmartHostPassword;
				m_pDns1.Text                    = settings.Dns1;
				m_pDns2.Text                    = settings.Dns2;
                m_pHostName.Text                = settings.HostName;
                m_pSessionTimeout.Value         = settings.SessionIdleTimeOut;
				m_pMaxConnections.Value         = settings.MaximumConnections;
                m_pMaxConnsPerIP.Value          = settings.MaximumConnectionsPerIP;
				m_pRelayInterval.Value          = settings.RelayInterval;
				m_pRelayRetryInterval.Value     = settings.RelayRetryInterval;
				m_pSendUndelWarning.Value       = settings.SendUndeliveredWarningAfter;
				m_pSendUndelivered.Value        = settings.SendUndeliveredAfter;
				m_pStoreUndeliveredMsgs.Checked = settings.StoreUndeliveredMessages;
				m_pSendingIP.Text               = settings.BindIP.ToString();
			}
			catch(Exception x){
				wfrm_sys_Error frm = new wfrm_sys_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}
        }

        #endregion

        #region method SaveData

        /// <summary>
        /// Saves data.
        /// </summary>
        /// <param name="confirmSave">Specifies is save confirmation UI is showed.</param>
        private void SaveData(bool confirmSave)
        {
            try{
                Relay_Settings settings =  m_pVirtualServer.SystemSettings.Relay;
 		
				settings.UseSmartHost                = m_pSendSmartHost.Checked;
				settings.SmartHost                   = m_pSmartHost.Text;
				settings.SmartHostPort               = (int)m_pSmartHostPort.Value;
                settings.SmartHostSsl                = m_pUseSSL.Checked;
				settings.SmartHostUserName           = m_pSmartHostUser.Text;
				settings.SmartHostPassword           = m_pSmartHostPassword.Text;
				settings.Dns1                        = m_pDns1.Text;
				settings.Dns2                        = m_pDns2.Text;
                settings.HostName                    = m_pHostName.Text;
                settings.SessionIdleTimeOut          = (int)m_pSessionTimeout.Value;
				settings.MaximumConnections          = (int)m_pMaxConnections.Value;
                settings.MaximumConnectionsPerIP     = (int)m_pMaxConnsPerIP.Value;
				settings.RelayInterval               = (int)m_pRelayInterval.Value;
				settings.RelayRetryInterval          = (int)m_pRelayRetryInterval.Value;
				settings.SendUndeliveredWarningAfter = (int)m_pSendUndelWarning.Value;
				settings.SendUndeliveredAfter        = (int)m_pSendUndelivered.Value;
				settings.StoreUndeliveredMessages    = m_pStoreUndeliveredMsgs.Checked;
				settings.BindIP                      = (IPAddress)((WComboBoxItem)m_pSendingIP.SelectedItem).Tag;
                               
                if(m_pVirtualServer.SystemSettings.HasChanges){
                    if(!confirmSave || MessageBox.Show(this,"You have changes settings, do you want to save them ?","Confirm:",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes){
                        m_pVirtualServer.SystemSettings.Commit();
                    }
                }
            }
			catch(Exception x){
				wfrm_sys_Error frm = new wfrm_sys_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}
        }

        #endregion

    }
}
