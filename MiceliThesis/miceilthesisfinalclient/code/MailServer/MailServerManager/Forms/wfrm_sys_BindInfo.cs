using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Drawing;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;

using LumiSoft.MailServer.API.UserAPI;

namespace LumiSoft.MailServer.UI
{
    /// <summary>
    /// Bind info window.
    /// </summary>
    public class wfrm_sys_BindInfo : Form
    {
        private ComboBox      m_pProtocol  = null;
        private ComboBox      m_pIP        = null;
        private NumericUpDown m_pPort      = null;
        private CheckBox      m_pSSL       = null;
        private Label         mt_CertInfo  = null;
        private Button        m_pLoadCert  = null;
        private GroupBox      m_pGroupBox1 = null;
        private Button        m_pCancel    = null;
        private Button        m_pOk        = null;

        private int    m_DefaultPort    = 10000;
        private int    m_DefaultSSLPort = 10001;
        private byte[] m_Cert           = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="server">Reference to server.</param>
        /// <param name="defaultPort">Specifies default port.</param>
        /// <param name="defaultSSLPort">Specifies default SSL port.</param>
        public wfrm_sys_BindInfo(Server server,int defaultPort,int defaultSSLPort) : this(server,false,defaultPort,defaultSSLPort)
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="server">Reference to server.</param>
        /// <param name="defaultPort">Specifies default port.</param>
        /// <param name="defaultSSLPort">Specifies default SSL port.</param>
        public wfrm_sys_BindInfo(Server server,bool allowUDP,int defaultPort,int defaultSSLPort)
        {
            m_DefaultPort    = defaultPort;
            m_DefaultSSLPort = defaultSSLPort;

            InitUI();

            m_pProtocol.Items.Add("TCP");
            if(allowUDP){
                m_pProtocol.Items.Add("UDP");
            }
            m_pProtocol.SelectedIndex = 0;

            foreach(IPAddress ip in server.IPAddresses){
                if(ip.Equals(IPAddress.Any)){
                    m_pIP.Items.Add(new WComboBoxItem("any IPv4",ip));
                }
                else if(ip.Equals(IPAddress.Loopback)){
                    m_pIP.Items.Add(new WComboBoxItem("localhost IPv4",ip));
                }
                else if(ip.Equals(IPAddress.IPv6Any)){
                    m_pIP.Items.Add(new WComboBoxItem("Any IPv6",ip));
                }
                else if(ip.Equals(IPAddress.IPv6Loopback)){
                    m_pIP.Items.Add(new WComboBoxItem("localhost IPv6",ip));
                }
                else{
                    m_pIP.Items.Add(new WComboBoxItem(ip.ToString(),ip));
                }
            }

            m_pIP.SelectedIndex = 0;
            m_pPort.Value = defaultPort;
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes window UI.
        /// </summary>
        private void InitUI()
        {
            this.ClientSize = new Size(350,173);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Add/Edit Bind info";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            m_pProtocol = new ComboBox();
            m_pProtocol.Size = new Size(60,20);
            m_pProtocol.Location = new Point(9,35);
            m_pProtocol.DropDownStyle = ComboBoxStyle.DropDownList;
            m_pProtocol.SelectedIndexChanged += new EventHandler(m_pProtocol_SelectedIndexChanged);

            m_pIP = new ComboBox();
            m_pIP.Size = new Size(195,20);
            m_pIP.Location = new Point(75,35);
            m_pIP.DropDownStyle = ComboBoxStyle.DropDownList;

            m_pPort = new NumericUpDown();
            m_pPort.Size = new Size(63,20);
            m_pPort.Location = new Point(280,35);
            m_pPort.Minimum = 1;
            m_pPort.Maximum = 99999;

            m_pSSL = new CheckBox();
            m_pSSL.Size = new Size(200,20);
            m_pSSL.Location = new Point(9,70);
            m_pSSL.Text = "Dedicated SSL connection";
            m_pSSL.CheckedChanged += new EventHandler(m_pSSL_CheckedChanged);

            mt_CertInfo = new Label();
            mt_CertInfo.Size = new Size(120,13);
            mt_CertInfo.Location = new Point(9,100);
            mt_CertInfo.Text = "Certificate Not Loaded";

            m_pLoadCert = new Button();
            m_pLoadCert.Size = new Size(145,20);
            m_pLoadCert.Location = new Point(195,100);
            m_pLoadCert.Text = "Load Certificate";
            m_pLoadCert.Click += new EventHandler(m_pLoadCert_Click);

            m_pGroupBox1 = new GroupBox();
            m_pGroupBox1.Size = new Size(340,2);
            m_pGroupBox1.Location = new Point(5,135);

            m_pCancel = new Button();
            m_pCancel.Size = new Size(70,20);
            m_pCancel.Location = new Point(195,145);
            m_pCancel.Text = "Cancel";
            m_pCancel.Click += new EventHandler(m_pCancel_Click);

            m_pOk = new Button();
            m_pOk.Size = new Size(70,20);
            m_pOk.Location = new Point(270,145);
            m_pOk.Text = "Ok";
            m_pOk.Click += new EventHandler(m_pOk_Click);

            this.Controls.Add(m_pProtocol);
            this.Controls.Add(m_pIP);
            this.Controls.Add(m_pPort);
            this.Controls.Add(m_pSSL);
            this.Controls.Add(mt_CertInfo);
            this.Controls.Add(m_pLoadCert);
            this.Controls.Add(m_pGroupBox1);
            this.Controls.Add(m_pCancel);
            this.Controls.Add(m_pOk);
        }
                                                                                
        #endregion


        #region Events Handling

        #region method m_pProtocol_SelectedIndexChanged

        private void m_pProtocol_SelectedIndexChanged(object sender,EventArgs e)
        {
            if(m_pProtocol.SelectedItem.ToString() == "TCP"){
                m_pSSL.Enabled = true;
                m_pLoadCert.Enabled = true;
            }
            else{
                m_pSSL.Enabled = false;
                m_pLoadCert.Enabled = false;
            }
        }

        #endregion

        #region method m_pSSL_CheckedChanged

        private void m_pSSL_CheckedChanged(object sender, EventArgs e)
        {
            if(m_pSSL.Checked){
                m_pPort.Value = m_DefaultSSLPort;
            }
            else{
                m_pPort.Value = m_DefaultPort;
            }
        }

        #endregion

        #region method m_pLoadCert_Click

        private void m_pLoadCert_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if(dlg.ShowDialog(this) == DialogResult.OK){
                try{
                    X509Certificate2 cert = (X509Certificate2)X509Certificate2.CreateFromCertFile(dlg.FileName);
                    if(!cert.HasPrivateKey){
                        MessageBox.Show(this,"Certificate is not server certificate, private key is missing !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                        return;
                    }

                    m_Cert = File.ReadAllBytes(dlg.FileName);
                    mt_CertInfo.Text = "Certificate Loaded";
                }
                catch{
                    MessageBox.Show(this,"Invalid or not supported certificate file !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }

        #endregion


        #region method m_pCancel_Click

        private void m_pCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region method m_pOk_Click

        private void m_pOk_Click(object sender, EventArgs e)
        {
            if(m_pSSL.Checked && m_Cert == null){
                MessageBox.Show(this,"Please load certificate !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
    
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        #endregion

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets selected protocol.
        /// </summary>
        public string Protocol
        {
            get{ return m_pProtocol.SelectedItem.ToString(); }
        }

        /// <summary>
        /// Gets selected IP address.
        /// </summary>
        public IPAddress IP
        {
            get{ return (IPAddress)((WComboBoxItem)m_pIP.SelectedItem).Tag; }
        }

        /// <summary>
        /// Gets selected port.
        /// </summary>
        public int Port
        {
            get{ return (int)m_pPort.Value; }
        }

        /// <summary>
        /// Gets if SSL selected.
        /// </summary>
        public bool SSL
        {
            get{ return m_pSSL.Checked; }
        }

        /// <summary>
        /// Gets certificate or null if no certificate loaded.
        /// </summary>
        public byte[] Certificate
        {
            get{ return m_Cert; }
        }

        #endregion

    }
}
