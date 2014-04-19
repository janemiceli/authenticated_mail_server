using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

using LumiSoft.MailServer.API.UserAPI;

namespace LumiSoft.MailServer.UI
{
    /// <summary>
    /// System Services IMAP settings window.
    /// </summary>
    public class wfrm_System_Services_IMAP : Form
    {
        //--- Common UI ------------------------------
        private TabControl    m_pTab            = null;
        private Button        m_pApply          = null;
        //--- Tabpage General UI ---------------------
        private CheckBox      m_pEnabled         = null;
        private Label         mt_HostName        = null;
        private TextBox       m_pHostName        = null;
        private Label         mt_GreetingText    = null;
        private TextBox       m_pGreetingText    = null;
        private Label         mt_SessionTimeout  = null;
        private NumericUpDown m_pSessionTimeout  = null;
        private Label         mt_SessTimeoutSec  = null;
        private Label         mt_MaxConnections  = null;
        private NumericUpDown m_pMaxConnections  = null;
        private Label         mt_MaxConnsPerIP   = null;
        private NumericUpDown m_pMaxConnsPerIP   = null;
        private Label         mt_MaxConnsPerIP0  = null;
        private Label         mt_MaxBadCommands  = null;
        private NumericUpDown m_pMaxBadCommands  = null;
        private Label         mt_Bindings        = null;
        private ListView      m_pBindings        = null;
        private Button        m_pBindings_Add    = null;
        private Button        m_pBindings_Delete = null;
        //---------------------------------------------

        private VirtualServer m_pVirtualServer = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="virtualServer">Virtual server.</param>
        public wfrm_System_Services_IMAP(VirtualServer virtualServer)
        {
            m_pVirtualServer = virtualServer;

            InitUI();

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
            m_pEnabled = new CheckBox();
            m_pEnabled.Size = new Size(70,20);
            m_pEnabled.Location = new Point(170,10);
            m_pEnabled.Text = "Enabled";

            mt_HostName = new Label();
            mt_HostName.Size = new Size(155,20);
            mt_HostName.Location = new Point(10,40);
            mt_HostName.TextAlign = ContentAlignment.MiddleRight;
            mt_HostName.Text = "Host Name:";

            m_pHostName = new TextBox();
            m_pHostName.Size = new Size(250,20);
            m_pHostName.Location = new Point(170,40);

            mt_GreetingText = new Label();
            mt_GreetingText.Size = new Size(155,20);
            mt_GreetingText.Location = new Point(10,65);
            mt_GreetingText.TextAlign = ContentAlignment.MiddleRight;
            mt_GreetingText.Text = "Greeting Text:";

            m_pGreetingText = new TextBox();
            m_pGreetingText.Size = new Size(250,20);
            m_pGreetingText.Location = new Point(170,65);

            mt_SessionTimeout = new Label();
            mt_SessionTimeout.Size = new Size(155,20);
            mt_SessionTimeout.Location = new Point(10,130);
            mt_SessionTimeout.TextAlign = ContentAlignment.MiddleRight;
            mt_SessionTimeout.Text = "Session Idle Timeout:";

            m_pSessionTimeout = new NumericUpDown();
            m_pSessionTimeout.Size = new Size(70,20);
            m_pSessionTimeout.Location = new Point(170,130);
            m_pSessionTimeout.Minimum = 10;
            m_pSessionTimeout.Maximum = 99999;

            mt_SessTimeoutSec = new Label();
            mt_SessTimeoutSec.Size = new Size(25,20);
            mt_SessTimeoutSec.Location = new Point(245,130);
            mt_SessTimeoutSec.TextAlign = ContentAlignment.MiddleLeft;
            mt_SessTimeoutSec.Text = "sec.";

            mt_MaxConnections = new Label();
            mt_MaxConnections.Size = new Size(155,20);
            mt_MaxConnections.Location = new Point(10,170);
            mt_MaxConnections.TextAlign = ContentAlignment.MiddleRight;
            mt_MaxConnections.Text = "Maximum Connections:";

            m_pMaxConnections = new NumericUpDown();
            m_pMaxConnections.Size = new Size(70,20);
            m_pMaxConnections.Location = new Point(170,170);
            m_pMaxConnections.Minimum = 1;
            m_pMaxConnections.Maximum = 99999;

            mt_MaxConnsPerIP = new Label();
            mt_MaxConnsPerIP.Size = new Size(164,20);
            mt_MaxConnsPerIP.Location = new Point(1,195);
            mt_MaxConnsPerIP.TextAlign = ContentAlignment.MiddleRight;
            mt_MaxConnsPerIP.Text = "Maximum Connections per IP:";

            m_pMaxConnsPerIP = new NumericUpDown();
            m_pMaxConnsPerIP.Size = new Size(70,20);
            m_pMaxConnsPerIP.Location = new Point(170,195);
            m_pMaxConnsPerIP.Minimum = 0;
            m_pMaxConnsPerIP.Maximum = 99999;

            mt_MaxConnsPerIP0 = new Label();
            mt_MaxConnsPerIP0.Size = new Size(164,20);
            mt_MaxConnsPerIP0.Location = new Point(245,195);
            mt_MaxConnsPerIP0.TextAlign = ContentAlignment.MiddleLeft;
            mt_MaxConnsPerIP0.Text = "(0 for unlimited)";
                        
            mt_MaxBadCommands = new Label();
            mt_MaxBadCommands.Size = new Size(155,20);
            mt_MaxBadCommands.Location = new Point(10,220);
            mt_MaxBadCommands.TextAlign = ContentAlignment.MiddleRight;
            mt_MaxBadCommands.Text = "Maximum Bad Commands:";

            m_pMaxBadCommands = new NumericUpDown();
            m_pMaxBadCommands.Size = new Size(70,20);
            m_pMaxBadCommands.Location = new Point(170,220);
            m_pMaxBadCommands.Minimum = 1;
            m_pMaxBadCommands.Maximum = 99999;

            mt_Bindings = new Label();
            mt_Bindings.Size = new Size(70,20);
            mt_Bindings.Location = new Point(10,325);
            mt_Bindings.Text = "IP Bindings:";

            m_pBindings = new ListView();
            m_pBindings.Size = new Size(400,100);
            m_pBindings.Location = new Point(10,350);
            m_pBindings.View = View.Details;
            m_pBindings.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            m_pBindings.HideSelection = false;
            m_pBindings.FullRowSelect = true;
            m_pBindings.SelectedIndexChanged += new EventHandler(m_pBindings_SelectedIndexChanged);
            m_pBindings.Columns.Add("IP",150,HorizontalAlignment.Left);            
            m_pBindings.Columns.Add("Port",100,HorizontalAlignment.Left);
            m_pBindings.Columns.Add("SSL",60,HorizontalAlignment.Left);
            m_pBindings.Columns.Add("Certificate",60,HorizontalAlignment.Left);
            
            m_pBindings_Add = new Button();
            m_pBindings_Add.Size = new Size(70,20);
            m_pBindings_Add.Location = new Point(420,350);
            m_pBindings_Add.Text = "Add";
            m_pBindings_Add.Click += new EventHandler(m_pBindings_Add_Click);

            m_pBindings_Delete = new Button();
            m_pBindings_Delete.Size = new Size(70,20);
            m_pBindings_Delete.Location = new Point(420,375);
            m_pBindings_Delete.Text = "Delete";
            m_pBindings_Delete.Click += new EventHandler(m_pBindings_Delete_Click);
                        
            // Tabpage General UI
            m_pTab.TabPages[0].Controls.Add(mt_HostName);
            m_pTab.TabPages[0].Controls.Add(m_pHostName);
            m_pTab.TabPages[0].Controls.Add(mt_GreetingText);
            m_pTab.TabPages[0].Controls.Add(m_pGreetingText);
            m_pTab.TabPages[0].Controls.Add(mt_SessionTimeout);
            m_pTab.TabPages[0].Controls.Add(m_pSessionTimeout);
            m_pTab.TabPages[0].Controls.Add(mt_SessTimeoutSec);
            m_pTab.TabPages[0].Controls.Add(mt_MaxConnections);
            m_pTab.TabPages[0].Controls.Add(m_pMaxConnections);
            m_pTab.TabPages[0].Controls.Add(mt_MaxConnsPerIP);
            m_pTab.TabPages[0].Controls.Add(m_pMaxConnsPerIP);
            m_pTab.TabPages[0].Controls.Add(mt_MaxConnsPerIP0);
            m_pTab.TabPages[0].Controls.Add(mt_MaxBadCommands);
            m_pTab.TabPages[0].Controls.Add(m_pMaxBadCommands);
            m_pTab.TabPages[0].Controls.Add(mt_Bindings);
            m_pTab.TabPages[0].Controls.Add(m_pBindings);
            m_pTab.TabPages[0].Controls.Add(m_pBindings_Add);
            m_pTab.TabPages[0].Controls.Add(m_pBindings_Delete);
            m_pTab.TabPages[0].Controls.Add(m_pEnabled);
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


        #region method m_pBindings_SelectedIndexChanged

        private void m_pBindings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(m_pBindings.SelectedItems.Count == 0){
                m_pBindings_Delete.Enabled = false;
            }
            else{
                m_pBindings_Delete.Enabled = true;
            }
        }

        #endregion

        #region method m_pBindings_Add_Click

        private void m_pBindings_Add_Click(object sender, EventArgs e)
        {
            wfrm_sys_BindInfo frm = new wfrm_sys_BindInfo(m_pVirtualServer.Server,143,993);
            if(frm.ShowDialog(this) == DialogResult.OK){
                ListViewItem it = new ListViewItem();
                if(frm.IP.ToString() == "0.0.0.0"){
                    it.Text = "Any IPv4";
                }
                else if(frm.IP.ToString() == "0:0:0:0:0:0:0:0"){
                    it.Text = "Any IPv6";
                }
                else{
                    it.Text = frm.IP.ToString();
                }
                it.SubItems.Add(frm.Port.ToString());
                it.SubItems.Add(frm.SSL.ToString());
                it.SubItems.Add(Convert.ToString(frm.Certificate != null));
                it.Tag = new BindInfo("TCP",frm.IP,frm.Port,frm.SSL,frm.Certificate);
                it.Selected = true;
                m_pBindings.Items.Add(it);
            }
        }

        #endregion

        #region method m_pBindings_Delete_Click

        private void m_pBindings_Delete_Click(object sender, EventArgs e)
        {
            if(m_pBindings.SelectedItems.Count > 0){
                if(MessageBox.Show(this,"Are you sure you want to delete binding '" + m_pBindings.SelectedItems[0].SubItems[0].Text + ":" + m_pBindings.SelectedItems[0].SubItems[1].Text + "' ?","Confirm:",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes){                    
                    m_pBindings.SelectedItems[0].Remove();
                }
            }
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
                IMAP_Settings settings = m_pVirtualServer.SystemSettings.IMAP;
 
				m_pEnabled.Checked      = settings.Enabled;
				m_pHostName.Text        = settings.HostName;
                m_pGreetingText.Text    = settings.GreetingText;
				m_pSessionTimeout.Value = settings.SessionIdleTimeOut;
				m_pMaxConnections.Value = settings.MaximumConnections;
                m_pMaxConnsPerIP.Value  = settings.MaximumConnectionsPerIP;
				m_pMaxBadCommands.Value = settings.MaximumBadCommands;
                
                foreach(BindInfo binding in settings.Binds){
                    ListViewItem it = new ListViewItem();
                    if(binding.IP.ToString() == "0.0.0.0"){
                        it.Text = "Any IPv4";
                    }
                    else if(binding.IP.ToString() == "0:0:0:0:0:0:0:0"){
                        it.Text = "Any IPv6";
                    }
                    else{
                        it.Text = binding.IP.ToString();
                    }
                    it.SubItems.Add(binding.Port.ToString());
                    it.SubItems.Add(binding.SSL.ToString());
                    it.SubItems.Add(Convert.ToString(binding.Certificate != null));
                    it.Tag = binding;
                    m_pBindings.Items.Add(it);
                }

                m_pBindings_SelectedIndexChanged(this,new EventArgs());
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
                IMAP_Settings settings = m_pVirtualServer.SystemSettings.IMAP;
 
				settings.Enabled                 = m_pEnabled.Checked;
				settings.HostName                = m_pHostName.Text;
                settings.GreetingText            = m_pGreetingText.Text;
				settings.SessionIdleTimeOut      = (int)m_pSessionTimeout.Value ;
				settings.MaximumConnections      = (int)m_pMaxConnections.Value;
                settings.MaximumConnectionsPerIP = (int)m_pMaxConnsPerIP.Value;
				settings.MaximumBadCommands      = (int)m_pMaxBadCommands.Value;// IP binds
                List<BindInfo> binds = new List<BindInfo>();
                foreach(ListViewItem it in m_pBindings.Items){
                    binds.Add((BindInfo)it.Tag);
                }
                settings.Binds = binds.ToArray();
                
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
