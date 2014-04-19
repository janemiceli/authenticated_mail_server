using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using LumiSoft.MailServer.API.UserAPI;
using LumiSoft.MailServer.UI.Resources;

namespace LumiSoft.MailServer.UI
{
    /// <summary>
    /// System Services SIP settings window.
    /// </summary>
    public class wfrm_System_Services_SIP : Form
    {
        //--- Common UI ------------------------------
        private TabControl    m_pTab      = null;
        private Button        m_pApply    = null;
        //--- Tabpage General UI ---------------------
        private CheckBox      m_pEnabled         = null;
        private Label         mt_HostName        = null;
        private TextBox       m_pHostName        = null;
        private Label         mt_MinExpires      = null;
        private NumericUpDown m_pMinExpires      = null;
        private Label         mt_Bindings        = null;
        private ToolStrip     m_pBindingsToolbar = null;
        private ListView      m_pBindings        = null;
        //---------------------------------------------

        private VirtualServer m_pVirtualServer = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public wfrm_System_Services_SIP(VirtualServer virtualServer)
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
                        
            this.Controls.Add(m_pTab);
            this.Controls.Add(m_pApply);
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

            mt_MinExpires = new Label();
            mt_MinExpires.Size = new Size(155,20);
            mt_MinExpires.Location = new Point(10,65);
            mt_MinExpires.TextAlign = ContentAlignment.MiddleRight;
            mt_MinExpires.Text = "Minimum Expires:";

            m_pMinExpires = new NumericUpDown();
            m_pMinExpires.Size = new Size(70,20);
            m_pMinExpires.Location = new Point(170,65);
            m_pMinExpires.Minimum = 60;
            m_pMinExpires.Maximum = 9999;

            mt_Bindings = new Label();
            mt_Bindings.Size = new Size(70,20);
            mt_Bindings.Location = new Point(10,325);
            mt_Bindings.Text = "IP Bindings:";

            m_pBindingsToolbar = new ToolStrip();
            m_pBindingsToolbar.Size = new Size(60,25);
            m_pBindingsToolbar.Location = new Point(450,325);
            m_pBindingsToolbar.Dock = DockStyle.None;
            m_pBindingsToolbar.GripStyle = ToolStripGripStyle.Hidden;
            m_pBindingsToolbar.BackColor = this.BackColor;
            m_pBindingsToolbar.Renderer = new ToolBarRendererEx();
            m_pBindingsToolbar.ItemClicked += new ToolStripItemClickedEventHandler(m_pBindingsToolbar_ItemClicked);
            // Add button
            ToolStripButton button_Add = new ToolStripButton();
            button_Add.Image = ResManager.GetIcon("add.ico").ToBitmap();
            button_Add.Tag = "add";
            button_Add.ToolTipText = "Add";
            m_pBindingsToolbar.Items.Add(button_Add);
            // Delete button
            ToolStripButton button_Delete = new ToolStripButton();
            button_Delete.Enabled = false;
            button_Delete.Image = ResManager.GetIcon("delete.ico").ToBitmap();
            button_Delete.Tag = "delete";
            button_Delete.ToolTipText = "Delete";
            m_pBindingsToolbar.Items.Add(button_Delete);

            m_pBindings = new ListView();
            m_pBindings.Size = new Size(485,100);
            m_pBindings.Location = new Point(10,350);
            m_pBindings.View = View.Details;
            m_pBindings.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            m_pBindings.HideSelection = false;
            m_pBindings.FullRowSelect = true;
            m_pBindings.MultiSelect = false;
            m_pBindings.SelectedIndexChanged += new EventHandler(m_pBindings_SelectedIndexChanged);
            m_pBindings.Columns.Add("IP",150,HorizontalAlignment.Left);
            m_pBindings.Columns.Add("Protocol",60,HorizontalAlignment.Left);
            m_pBindings.Columns.Add("Port",100,HorizontalAlignment.Left);
            m_pBindings.Columns.Add("SSL",60,HorizontalAlignment.Left);
            m_pBindings.Columns.Add("Certificate",60,HorizontalAlignment.Left);
                                    
            // Tabpage General UI
            m_pTab.TabPages[0].Controls.Add(m_pEnabled);
            m_pTab.TabPages[0].Controls.Add(mt_HostName);
            m_pTab.TabPages[0].Controls.Add(m_pHostName);
            m_pTab.TabPages[0].Controls.Add(mt_MinExpires);
            m_pTab.TabPages[0].Controls.Add(m_pMinExpires);
            m_pTab.TabPages[0].Controls.Add(mt_Bindings);
            m_pTab.TabPages[0].Controls.Add(m_pBindingsToolbar);
            m_pTab.TabPages[0].Controls.Add(m_pBindings);          
            //-------------------------------------------------//            
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


        #region method m_pBindingsToolbar_ItemClicked

        private void m_pBindingsToolbar_ItemClicked(object sender,ToolStripItemClickedEventArgs e)
        {
            if(e.ClickedItem.Tag == null){
                return;
            }

            if(e.ClickedItem.Tag.ToString() == "add"){
                wfrm_sys_BindInfo frm = new wfrm_sys_BindInfo(m_pVirtualServer.Server,true,5060,5061);
                if(frm.ShowDialog(this) == DialogResult.OK){
                    ListViewItem it = new ListViewItem();
                    it.Text = frm.Protocol;
                    if(frm.IP.ToString() == "0.0.0.0"){
                        it.SubItems.Add("Any IPv4");
                    }
                    else if(frm.IP.ToString() == "0:0:0:0:0:0:0:0"){
                        it.SubItems.Add("Any IPv6");
                    }
                    else{
                        it.SubItems.Add(frm.IP.ToString());
                    }
                    it.SubItems.Add(frm.Port.ToString());
                    it.SubItems.Add(frm.SSL.ToString());
                    it.SubItems.Add(Convert.ToString(frm.Certificate != null));
                    it.Tag = new BindInfo(frm.Protocol,frm.IP,frm.Port,frm.SSL,frm.Certificate);
                    it.Selected = true;
                    m_pBindings.Items.Add(it);
                }
            }
            else if(e.ClickedItem.Tag.ToString() == "delete"){
                if(m_pBindings.SelectedItems.Count > 0){
                    if(MessageBox.Show(this,"Are you sure you want to delete binding '" + m_pBindings.SelectedItems[0].SubItems[0].Text + ":" + m_pBindings.SelectedItems[0].SubItems[1].Text + "' ?","Confirm:",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes){                    
                        m_pBindings.SelectedItems[0].Remove();
                    }
                }
            }
        }

        #endregion

        #region method m_pBindings_SelectedIndexChanged

        private void m_pBindings_SelectedIndexChanged(object sender,EventArgs e)
        {
            if(m_pBindings.SelectedItems.Count > 0){
                m_pBindingsToolbar.Items[1].Enabled = true;
            }
            else{
                m_pBindingsToolbar.Items[1].Enabled = false;
            }
        }

        #endregion


        #region method m_pApply_Click

        private void m_pApply_Click(object sender,EventArgs e)
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
				SIP_Settings settings = m_pVirtualServer.SystemSettings.SIP;

				m_pEnabled.Checked  = settings.Enabled;
                m_pHostName.Text    = settings.HostName;
                m_pMinExpires.Value = Convert.ToDecimal(settings.MinimumExpires);

                foreach(BindInfo binding in settings.Binds){
                    ListViewItem it = new ListViewItem();
                    it.Text = binding.Protocol;
                    if(binding.IP.ToString() == "0.0.0.0"){
                        it.SubItems.Add("Any IPv4");
                    }
                    else if(binding.IP.ToString() == "0:0:0:0:0:0:0:0"){
                        it.SubItems.Add("Any IPv6");
                    }
                    else{
                        it.SubItems.Add(binding.IP.ToString());
                    }
                    it.SubItems.Add(binding.Port.ToString());
                    it.SubItems.Add(binding.SSL.ToString());
                    it.SubItems.Add(Convert.ToString(binding.Certificate != null));
                    it.Tag = binding;
                    m_pBindings.Items.Add(it);
                }
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
                SIP_Settings settings = m_pVirtualServer.SystemSettings.SIP;

                settings.Enabled        = m_pEnabled.Checked;
                settings.HostName       = m_pHostName.Text;
                settings.MinimumExpires = (int)m_pMinExpires.Value;
                // IP binds
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
