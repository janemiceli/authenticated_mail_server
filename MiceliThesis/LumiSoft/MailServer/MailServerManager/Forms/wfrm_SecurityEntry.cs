using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using LumiSoft.MailServer;
using LumiSoft.UI.Controls;

namespace MailServerManager.Forms
{
	internal enum SecAction
	{
		Deny_SMTP,
		Deny_POP3,
		Allow_SMTP,
		Allow_POP3,
		Allow_Relay,
		Deny_Relay,
	}

	/// <summary>
	/// Summary description for wfrm_SecurityEntry.
	/// </summary>
	public class wfrm_SecurityEntry : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WButton m_pCancel;
		private LumiSoft.UI.Controls.WButton m_pOk;
		private LumiSoft.UI.Controls.WEditBox m_pDomain;
		private LumiSoft.UI.Controls.WEditBox m_pEndIP;
		private LumiSoft.UI.Controls.WEditBox m_pStartIP;
		private LumiSoft.UI.Controls.WComboBox m_pAction;
		private LumiSoft.UI.Controls.WComboBox m_pType;
		private LumiSoft.UI.Controls.WComboBox m_pProtocol;
		private LumiSoft.UI.Controls.WEditBox m_pDescription;
		private LumiSoft.UI.Controls.WLabel mt_domain;
		private LumiSoft.UI.Controls.WLabel mt_ipaddress;
		private LumiSoft.UI.Controls.WLabel mt_action;
		private LumiSoft.UI.Controls.WLabel mt_type;
		private LumiSoft.UI.Controls.WLabel mt_protocol;
		private LumiSoft.UI.Controls.WLabel mt_description;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private string  m_Description = "";
		private string  m_Protocol    = "";
		private string  m_Type        = "";
		private string  m_Action      = "";
		private string  m_Content     = "";
		private long    m_StartIP     = 0;
		private long    m_EndIP       = 0;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="ds"></param>
		public wfrm_SecurityEntry()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			FillCombos();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		internal wfrm_SecurityEntry(SecAction action)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
						
			if(action == SecAction.Allow_SMTP || action == SecAction.Deny_SMTP || action == SecAction.Allow_Relay || action == SecAction.Deny_Relay){
				m_pProtocol.Items.Add("SMTP");			
			}
			else{
				m_pProtocol.Items.Add("POP3");
			}
			m_pProtocol.Enabled = false;
			m_pProtocol.VisibleItems = 2;
			m_pProtocol.SelectedIndex = 0;
						
			m_pType.Items.Add("IP");
			m_pType.Items.Add("IP Range");
			m_pType.VisibleItems = 2;
			
			m_pAction.Items.Clear();
			if(action == SecAction.Allow_SMTP || action == SecAction.Allow_POP3){
				m_pAction.Items.Add("Allow");
			}
			if(action == SecAction.Deny_SMTP || action == SecAction.Deny_POP3){
				m_pAction.Items.Add("Deny");
			}
			if(action == SecAction.Allow_Relay){
				m_pAction.Items.Add("Allow Relay");	
			}
			if(action == SecAction.Deny_Relay){
				m_pAction.Items.Add("Deny Relay");
			}				
			m_pAction.Enabled = false;
			m_pAction.VisibleItems = 4;	
					
			m_pType.SelectedIndex     = 0;
			m_pAction.SelectedIndex   = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ds"></param>
		/// <param name="dr"></param>
		public wfrm_SecurityEntry(DataRow dr)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
						
			FillCombos();

			try
			{
				m_pDescription.Text = dr["Description"].ToString();
				m_pProtocol.Text    = dr["Protocol"].ToString();
				m_pType.Text        = dr["Type"].ToString();
				m_pAction.Text      = dr["Action"].ToString();
			
				if(m_pType.Text == "IP" || m_pType.Text == "Domain"){
					m_pStartIP.Text = dr["Content"].ToString();
				}

				if(m_pType.Text == "IP Range"){
					string content = dr["Content"].ToString();
					string[] iprange =  content.Split(new Char[]{'-'});

					m_pStartIP.Text = iprange[0];
					m_pEndIP.Text   = iprange[1];

					m_pEndIP.Visible = true;
				}
			}
			catch(Exception x)
			{
				MessageBox.Show("error:" + x.Message);
			}			
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
			this.m_pCancel = new LumiSoft.UI.Controls.WButton();
			this.m_pOk = new LumiSoft.UI.Controls.WButton();
			this.m_pDomain = new LumiSoft.UI.Controls.WEditBox();
			this.m_pEndIP = new LumiSoft.UI.Controls.WEditBox();
			this.m_pStartIP = new LumiSoft.UI.Controls.WEditBox();
			this.m_pAction = new LumiSoft.UI.Controls.WComboBox();
			this.m_pType = new LumiSoft.UI.Controls.WComboBox();
			this.m_pProtocol = new LumiSoft.UI.Controls.WComboBox();
			this.m_pDescription = new LumiSoft.UI.Controls.WEditBox();
			this.mt_domain = new LumiSoft.UI.Controls.WLabel();
			this.mt_ipaddress = new LumiSoft.UI.Controls.WLabel();
			this.mt_action = new LumiSoft.UI.Controls.WLabel();
			this.mt_type = new LumiSoft.UI.Controls.WLabel();
			this.mt_protocol = new LumiSoft.UI.Controls.WLabel();
			this.mt_description = new LumiSoft.UI.Controls.WLabel();
			this.SuspendLayout();
			// 
			// m_pCancel
			// 
			this.m_pCancel.Location = new System.Drawing.Point(248, 150);
			this.m_pCancel.Name = "m_pCancel";
			this.m_pCancel.Size = new System.Drawing.Size(72, 24);
			this.m_pCancel.TabIndex = 15;
			this.m_pCancel.Text = "Cancel";
			this.m_pCancel.UseStaticViewStyle = true;
			// 
			// m_pCancel.ViewStyle
			// 
			this.m_pCancel.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pCancel_ButtonPressed);
			// 
			// m_pOk
			// 
			this.m_pOk.Location = new System.Drawing.Point(168, 150);
			this.m_pOk.Name = "m_pOk";
			this.m_pOk.Size = new System.Drawing.Size(72, 24);
			this.m_pOk.TabIndex = 23;
			this.m_pOk.Text = "OK";
			this.m_pOk.UseStaticViewStyle = true;
			// 
			// m_pOk.ViewStyle
			// 
			this.m_pOk.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pOk_Click);
			// 
			// m_pDomain
			// 
			this.m_pDomain.Location = new System.Drawing.Point(8, 150);
			this.m_pDomain.Name = "m_pDomain";
			this.m_pDomain.TabIndex = 22;
			this.m_pDomain.UseStaticViewStyle = true;
			// 
			// m_pDomain.ViewStyle
			// 
			this.m_pDomain.Visible = false;
			// 
			// m_pEndIP
			// 
			this.m_pEndIP.Location = new System.Drawing.Point(120, 110);
			this.m_pEndIP.Name = "m_pEndIP";
			this.m_pEndIP.TabIndex = 21;
			this.m_pEndIP.UseStaticViewStyle = true;
			// 
			// m_pEndIP.ViewStyle
			// 
			this.m_pEndIP.Visible = false;
			// 
			// m_pStartIP
			// 
			this.m_pStartIP.Location = new System.Drawing.Point(8, 110);
			this.m_pStartIP.Name = "m_pStartIP";
			this.m_pStartIP.TabIndex = 20;
			this.m_pStartIP.UseStaticViewStyle = true;
			// 
			// m_pStartIP.ViewStyle
			// 
			// 
			// m_pAction
			// 
			this.m_pAction.DropDownWidth = 96;
			this.m_pAction.Location = new System.Drawing.Point(232, 70);
			this.m_pAction.Name = "m_pAction";
			this.m_pAction.SelectedIndex = -1;
			this.m_pAction.Size = new System.Drawing.Size(96, 20);
			this.m_pAction.TabIndex = 19;
			this.m_pAction.UseStaticViewStyle = true;
			// 
			// m_pAction.ViewStyle
			// 
			this.m_pAction.VisibleItems = 5;
			// 
			// m_pType
			// 
			this.m_pType.DropDownWidth = 96;
			this.m_pType.Location = new System.Drawing.Point(120, 70);
			this.m_pType.Name = "m_pType";
			this.m_pType.SelectedIndex = -1;
			this.m_pType.Size = new System.Drawing.Size(96, 20);
			this.m_pType.TabIndex = 18;
			this.m_pType.UseStaticViewStyle = true;
			// 
			// m_pType.ViewStyle
			// 
			this.m_pType.VisibleItems = 5;
			this.m_pType.SelectedIndexChanged += new System.EventHandler(this.m_pType_SelectedIndexChanged);
			// 
			// m_pProtocol
			// 
			this.m_pProtocol.DropDownWidth = 96;
			this.m_pProtocol.Location = new System.Drawing.Point(8, 70);
			this.m_pProtocol.Name = "m_pProtocol";
			this.m_pProtocol.SelectedIndex = -1;
			this.m_pProtocol.Size = new System.Drawing.Size(96, 20);
			this.m_pProtocol.TabIndex = 17;
			this.m_pProtocol.UseStaticViewStyle = true;
			// 
			// m_pProtocol.ViewStyle
			// 
			this.m_pProtocol.VisibleItems = 5;
			this.m_pProtocol.SelectedIndexChanged += new System.EventHandler(this.m_pProtocol_SelectedIndexChanged);
			// 
			// m_pDescription
			// 
			this.m_pDescription.Location = new System.Drawing.Point(8, 22);
			this.m_pDescription.Name = "m_pDescription";
			this.m_pDescription.Size = new System.Drawing.Size(320, 20);
			this.m_pDescription.TabIndex = 16;
			this.m_pDescription.UseStaticViewStyle = true;
			// 
			// m_pDescription.ViewStyle
			// 
			// 
			// mt_domain
			// 
			this.mt_domain.Location = new System.Drawing.Point(8, 134);
			this.mt_domain.Name = "mt_domain";
			this.mt_domain.Size = new System.Drawing.Size(72, 16);
			this.mt_domain.TabIndex = 29;
			this.mt_domain.Text = "Domain";
			this.mt_domain.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_domain.ViewStyle
			// 
			this.mt_domain.Visible = false;
			// 
			// mt_ipaddress
			// 
			this.mt_ipaddress.Location = new System.Drawing.Point(8, 94);
			this.mt_ipaddress.Name = "mt_ipaddress";
			this.mt_ipaddress.Size = new System.Drawing.Size(72, 16);
			this.mt_ipaddress.TabIndex = 28;
			this.mt_ipaddress.Text = "IP Address";
			this.mt_ipaddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_ipaddress.ViewStyle
			// 
			// 
			// mt_action
			// 
			this.mt_action.Location = new System.Drawing.Point(232, 54);
			this.mt_action.Name = "mt_action";
			this.mt_action.Size = new System.Drawing.Size(72, 16);
			this.mt_action.TabIndex = 27;
			this.mt_action.Text = "Action";
			this.mt_action.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_action.ViewStyle
			// 
			// 
			// mt_type
			// 
			this.mt_type.Location = new System.Drawing.Point(120, 54);
			this.mt_type.Name = "mt_type";
			this.mt_type.Size = new System.Drawing.Size(72, 16);
			this.mt_type.TabIndex = 26;
			this.mt_type.Text = "Type";
			this.mt_type.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_type.ViewStyle
			// 
			// 
			// mt_protocol
			// 
			this.mt_protocol.Location = new System.Drawing.Point(8, 54);
			this.mt_protocol.Name = "mt_protocol";
			this.mt_protocol.Size = new System.Drawing.Size(72, 16);
			this.mt_protocol.TabIndex = 25;
			this.mt_protocol.Text = "Protocol";
			this.mt_protocol.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_protocol.ViewStyle
			// 
			// 
			// mt_description
			// 
			this.mt_description.Location = new System.Drawing.Point(8, 6);
			this.mt_description.Name = "mt_description";
			this.mt_description.Size = new System.Drawing.Size(72, 16);
			this.mt_description.TabIndex = 24;
			this.mt_description.Text = "Description";
			this.mt_description.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_description.ViewStyle
			// 
			// 
			// wfrm_SecurityEntry
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(336, 181);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_pCancel,
																		  this.m_pOk,
																		  this.m_pDomain,
																		  this.m_pEndIP,
																		  this.m_pStartIP,
																		  this.m_pAction,
																		  this.m_pType,
																		  this.m_pProtocol,
																		  this.m_pDescription,
																		  this.mt_domain,
																		  this.mt_ipaddress,
																		  this.mt_action,
																		  this.mt_type,
																		  this.mt_protocol,
																		  this.mt_description});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "wfrm_SecurityEntry";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Security Entry";
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		#region function m_pProtocol_SelectedIndexChanged

		private void m_pProtocol_SelectedIndexChanged(object sender, System.EventArgs e)
		{

			if(m_pProtocol.Text == "SMTP"){
				m_pAction.Items.Clear();

				m_pAction.Items.Add("Allow");
				m_pAction.Items.Add("Allow Relay");
				m_pAction.Items.Add("Deny");
				m_pAction.Items.Add("Deny Realy");
			}

			if(m_pProtocol.Text == "POP3"){
				m_pAction.Items.Clear();

				m_pAction.Items.Add("Allow");				
				m_pAction.Items.Add("Deny");				
			}

			m_pAction.SelectedIndex = 0;
		}

		#endregion

		#region function m_pType_SelectedIndexChanged

		private void m_pType_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(m_pType.Text == "IP"){
				m_pStartIP.Visible = true;
				mt_ipaddress.Visible      = true;

				m_pEndIP.Visible   = false;
				m_pDomain.Visible  = false;
				mt_domain.Visible  = false;
			}

			if(m_pType.Text == "IP Range"){
				m_pStartIP.Visible = true;
				m_pEndIP.Visible   = true;
				mt_ipaddress.Visible      = true;

				m_pDomain.Visible  = false;
				mt_domain.Visible  = false;
			}

			if(m_pType.Text == "Domain"){
				m_pDomain.Visible  = true;
				mt_domain.Visible  = true;

				m_pStartIP.Visible = false;
				m_pEndIP.Visible   = false;
				mt_ipaddress.Visible      = false;
			}
		}

		#endregion

		#region function m_pOk_Click

		private void m_pOk_Click(object sender, System.EventArgs e)
		{
			try
			{										
				string content = "";
				long   StartIP = 0;
				long   EndIP   = 0;

				if(m_pType.Text == "IP"){
					content = m_pStartIP.Text;

					if(!IsIpValid(content)){
						MessageBox.Show("IP address isn't valid!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
						return;
					}

					StartIP = IP_to_long(m_pStartIP.Text);
					EndIP   = IP_to_long(m_pStartIP.Text);
				}

				if(m_pType.Text == "IP Range"){
					content = m_pStartIP.Text + "-" + m_pEndIP.Text;

					if(!IsIpValid(m_pStartIP.Text)){
						MessageBox.Show("Start IP address isn't valid!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
						return;
					}
					if(!IsIpValid(m_pEndIP.Text)){
						MessageBox.Show("End IP address isn't valid!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
						return;
					}

					StartIP = IP_to_long(m_pStartIP.Text);
					EndIP   = IP_to_long(m_pEndIP.Text);
				}

				if(m_pType.Text == "Domain"){
					content = m_pDomain.Text;
				}

				m_Description = m_pDescription.Text;
				m_Protocol    = m_pProtocol.Text;
				m_Type        = m_pType.Text;
				m_Action      = m_pAction.Text;
				m_Content     = content;
				m_StartIP     = StartIP;
				m_EndIP       = EndIP;
			}
			catch(Exception x)
			{
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		#endregion

		#region function m_pCancel_ButtonPressed

		private void m_pCancel_ButtonPressed(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		#endregion

		#endregion


		#region function FillCombos

		private void FillCombos()
		{
			m_pProtocol.Items.Add("SMTP");
			m_pProtocol.Items.Add("POP3");
			m_pProtocol.VisibleItems = 2;
			
			m_pType.Items.Add("IP");
			m_pType.Items.Add("IP Range");
			m_pType.VisibleItems = 2;
			
			m_pAction.Items.Add("Allow");
			m_pAction.Items.Add("Allow Relay");
			m_pAction.Items.Add("Deny");
			m_pAction.Items.Add("Deny Relay");
			m_pAction.VisibleItems = 4;
			
			m_pProtocol.SelectedIndex = 0;
			m_pType.SelectedIndex     = 0;
			m_pAction.SelectedIndex   = 0;
		}

		#endregion


		#region function IP_to_long

		/// <summary>
		/// Removes points from ip and fill all blocks eg.(10.0.0.1 = 10 000 000 001).
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		private long IP_to_long(string ip)
		{
			string retVal = "";

			string[] str = ip.Split(new char[]{'.'});

			// loop through all ip blocks.
			foreach(string ipBlock in str){
				string buff = ipBlock;
				// If block size is smaller than 3, append '0' at the beginning of string.
				if(ipBlock.Length < 3){
					for(int i=0;i<3;i++){
						if(buff.Length >= 3){
							break;
						}
						buff = "0" + buff;
					}
				}

				retVal += buff;
			}

			return Convert.ToInt64(retVal);
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
			catch{ //(Exception x){
				return false;
			}
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// 
		/// </summary>
		public string wp_Description
		{
			get{ return m_Description; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string wp_Protocol
		{
			get{ return m_Protocol; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string wp_Type
		{
			get{ return m_Type; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string wp_Action
		{
			get{ return m_Action; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string wp_Content
		{
			get{ return m_Content; }
		}

		/// <summary>
		/// 
		/// </summary>
		public long wp_StartIP
		{
			get{ return m_StartIP; }
		}

		/// <summary>
		/// 
		/// </summary>
		public long wp_EndIP
		{
			get{ return m_EndIP; }
		}

		#endregion
	}
}
