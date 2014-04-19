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
    /// SIP registration contacts window.
    /// </summary>
    public class wfrm_Monitoring_SipRegistration_Contacts : Form
    {
        private PictureBox m_pIcon        = null;
        private Label      mt_Info        = null;
        private GroupBox   m_pSeparator1  = null;
        private ToolStrip  m_pToolbar     = null;
        private ListView   m_pContacts    = null;
        private GroupBox   m_pSeparator2  = null;
        private Button     m_pCancel      = null;
        private Button     m_pOk          = null;

        private SipRegistration m_pRegistration = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="registration">Registration what contacts to show.</param>
        public wfrm_Monitoring_SipRegistration_Contacts(SipRegistration registration)
        {
            m_pRegistration = registration;

            InitUI();
                        
            LoadContacts();
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes window UI.
        /// </summary>
        private void InitUI()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(500,300);
            this.Text = "Registration Contacts";
            this.Icon = ResManager.GetIcon("rule.ico");

            m_pIcon = new PictureBox();
            m_pIcon.Size = new Size(32,32);
            m_pIcon.Location = new Point(10,10);
            m_pIcon.Image = ResManager.GetIcon("rule.ico").ToBitmap();

            mt_Info = new Label();
            mt_Info.Size = new Size(450,32);
            mt_Info.Location = new Point(50,10);
            mt_Info.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            mt_Info.TextAlign = ContentAlignment.MiddleLeft;
            mt_Info.Text = "Address of recod '" + m_pRegistration.AddressOfRecord + "' registered contacts.";

            m_pSeparator1 = new GroupBox();
            m_pSeparator1.Size = new Size(485,3);
            m_pSeparator1.Location = new Point(7,50);
            m_pSeparator1.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            m_pToolbar = new ToolStrip();
            m_pToolbar.Location = new Point(430,55);
            m_pToolbar.Size = new Size(60,25);
            m_pToolbar.Dock = DockStyle.None;
            m_pToolbar.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            m_pToolbar.GripStyle = ToolStripGripStyle.Hidden;
            m_pToolbar.BackColor = this.BackColor;
            m_pToolbar.Renderer = new ToolBarRendererEx();
            m_pToolbar.ItemClicked += new ToolStripItemClickedEventHandler(m_pToolbar_ItemClicked);
            // Add button
            ToolStripButton button_Add = new ToolStripButton();
            button_Add.Image = ResManager.GetIcon("add.ico").ToBitmap();
            button_Add.Tag = "add";
            button_Add.ToolTipText = "Add";
            m_pToolbar.Items.Add(button_Add);
            // Delete button
            ToolStripButton button_Delete = new ToolStripButton();
            button_Delete.Enabled = false;
            button_Delete.Image = ResManager.GetIcon("delete.ico").ToBitmap();
            button_Delete.Tag = "delete";
            button_Delete.ToolTipText = "Delete";
            m_pToolbar.Items.Add(button_Delete);
            // Separator
            m_pToolbar.Items.Add(new ToolStripSeparator());
            // Refresh button
            ToolStripButton button_Refresh = new ToolStripButton();
            button_Refresh.Image = ResManager.GetIcon("refresh.ico").ToBitmap();
            button_Refresh.Tag = "refresh";
            button_Refresh.ToolTipText  = "Refresh";
            m_pToolbar.Items.Add(button_Refresh);

            m_pContacts = new ListView();
            m_pContacts.Size = new Size(480,160);
            m_pContacts.Location = new Point(10,85);
            m_pContacts.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            m_pContacts.View = View.Details;
            m_pContacts.FullRowSelect = true;
            m_pContacts.HideSelection = false;
            m_pContacts.Columns.Add("Contact URI",340);
            m_pContacts.Columns.Add("Expires",60);
            m_pContacts.Columns.Add("Priority",50);
            m_pContacts.SelectedIndexChanged += new EventHandler(m_pContacts_SelectedIndexChanged);

            m_pSeparator2 = new GroupBox();
            m_pSeparator2.Size = new Size(485,4);
            m_pSeparator2.Location = new Point(7,260);
            m_pSeparator2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            m_pCancel = new Button();
            m_pCancel.Size = new Size(70,20);
            m_pCancel.Location = new Point(340,270);
            m_pCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            m_pCancel.Text = "Cancel";
            m_pCancel.Click += new EventHandler(m_pCancel_Click);

            m_pOk = new Button();
            m_pOk.Size = new Size(70,20);
            m_pOk.Location = new Point(415,270);
            m_pOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            m_pOk.Text = "Ok";
            m_pOk.Click += new EventHandler(m_pOk_Click);

            this.Controls.Add(m_pIcon);
            this.Controls.Add(mt_Info);
            this.Controls.Add(m_pSeparator1);
            this.Controls.Add(m_pToolbar);
            this.Controls.Add(m_pContacts);
            this.Controls.Add(m_pSeparator2);
            //this.Controls.Add(m_pCancel);
            this.Controls.Add(m_pOk);
        }
                                                
        #endregion


        #region Events Handling

        #region method m_pToolbar_ItemClicked

        private void m_pToolbar_ItemClicked(object sender,ToolStripItemClickedEventArgs e)
        {
            if(e.ClickedItem.Tag == null){
                return;
            }
            else if(e.ClickedItem.Tag.ToString() == "add"){
                MessageBox.Show("TODO:");
            }
            else if(e.ClickedItem.Tag.ToString() == "delete"){
                MessageBox.Show("TODO:");
            }
            else if(e.ClickedItem.Tag.ToString() == "refresh"){
                LoadContacts();
            }
        }

        #endregion

        #region method m_pContacts_SelectedIndexChanged

        private void m_pContacts_SelectedIndexChanged(object sender,EventArgs e)
        {
            if(m_pContacts.SelectedItems.Count > 0){
                m_pToolbar.Items[1].Enabled = true;
            }
            else{
                m_pToolbar.Items[1].Enabled = false;
            }
        }

        #endregion

        #region method m_pCancel_Click

        private void m_pCancel_Click(object sender,EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region method m_pOk_Click

        private void m_pOk_Click(object sender,EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        #endregion

        #endregion


        #region method LoadContacts

        /// <summary>
        /// Loads contacts to UI.
        /// </summary>
        private void LoadContacts()
        {
            m_pContacts.Items.Clear();

            m_pRegistration.Refresh();
            foreach(SipRegistrationContact contact in m_pRegistration.Contacts){
                ListViewItem it = new ListViewItem(contact.ContactUri);
                it.SubItems.Add(contact.Expires.ToString());
                it.SubItems.Add(contact.Priority.ToString());
                m_pContacts.Items.Add(it);
            }
        }

        #endregion

    }
}
