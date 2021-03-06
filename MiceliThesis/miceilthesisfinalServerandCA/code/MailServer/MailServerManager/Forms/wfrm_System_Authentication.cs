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
    /// System Authentication settings window.
    /// </summary>
    public class wfrm_System_Authentication : Form
    {
        //--- Common UI ------------------------------
        private TabControl    m_pTab           = null;
        private Button        m_pApply         = null;
        //--- Tabpage General UI ---------------------
        private Label    mt_AuthenticationType = null;
        private ComboBox m_pAuthenticationType = null;
        private Label    mt_DomainName         = null;
        private TextBox  m_pDomainName         = null;                
        //--------------------------------------------

        private VirtualServer m_pVirtualServer = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="virtualServer">Virtual server.</param>
        public wfrm_System_Authentication(VirtualServer virtualServer)
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
            mt_AuthenticationType = new Label();
            mt_AuthenticationType.Size = new Size(150,20);
            mt_AuthenticationType.Location = new Point(10,30);
            mt_AuthenticationType.Text = "Authentication Type:";

            m_pAuthenticationType = new ComboBox();
            m_pAuthenticationType.Size = new Size(150,20);
            m_pAuthenticationType.Location = new Point(10,50);
            m_pAuthenticationType.DropDownStyle = ComboBoxStyle.DropDownList;
            m_pAuthenticationType.SelectedIndexChanged += new EventHandler(m_pAuthenticationType_SelectedIndexChanged);
            m_pAuthenticationType.Items.Add("Integrated");
            m_pAuthenticationType.Items.Add("Windows");
            //m_pAuthenticationType.Items.Add("LDAP");

            mt_DomainName = new Label();
            mt_DomainName.Size = new Size(150,20);
            mt_DomainName.Location = new Point(10,80);
            mt_DomainName.Text = "Domain:";
            mt_DomainName.Visible = false;

            m_pDomainName = new TextBox();
            m_pDomainName.Size = new Size(150,20);
            m_pDomainName.Location = new Point(10,100);
            m_pDomainName.Visible = false;

            // Tabpage General UI
            m_pTab.TabPages[0].Controls.Add(mt_AuthenticationType);
            m_pTab.TabPages[0].Controls.Add(m_pAuthenticationType);
            m_pTab.TabPages[0].Controls.Add(mt_DomainName);
            m_pTab.TabPages[0].Controls.Add(m_pDomainName);
            //---------------------------------------------------//

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


        #region method m_pAuthenticationType_SelectedIndexChanged

        private void m_pAuthenticationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            mt_DomainName.Visible = false;
            m_pDomainName.Visible = false;

            if(m_pAuthenticationType.Text == "Windows"){
                mt_DomainName.Visible = true;
                m_pDomainName.Visible = true;
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
                Auth_Settings settings = m_pVirtualServer.SystemSettings.Authentication;
                				
				m_pAuthenticationType.SelectedIndex = Convert.ToInt32(settings.AuthenticationType) - 1; 
				m_pDomainName.Text                  = settings.WinDomain;
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
                Auth_Settings settings = m_pVirtualServer.SystemSettings.Authentication;
                				
				settings.AuthenticationType = (ServerAuthenticationType_enum)(m_pAuthenticationType.SelectedIndex + 1); 
				settings.WinDomain          = m_pDomainName.Text;
                
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
