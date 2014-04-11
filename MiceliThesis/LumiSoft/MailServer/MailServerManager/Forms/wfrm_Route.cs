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
	/// <summary>
	/// Summary description for wfrm_Route.
	/// </summary>
	public class wfrm_Route : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WComboBox m_pRouteType;
		private LumiSoft.UI.Controls.WButton m_pCancel;
		private LumiSoft.UI.Controls.WComboBox m_pDomains;
		private LumiSoft.UI.Controls.WLabel mt_symbolAT;
		private LumiSoft.UI.Controls.WComboBox m_pRouteTo;
		private LumiSoft.UI.Controls.WButton m_pOk;
		private LumiSoft.UI.Controls.WEditBox m_pDescription;
		private LumiSoft.UI.Controls.WLabel wLabel3;
		private LumiSoft.UI.Controls.WEditBox m_pPattern;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private ServerAPI m_ServerAPI   = null;
		private string    m_Pattern     = "";
		private string    m_MailBox     = "";
		private string    m_Description = "";
		private string    m_DomainID    = "";
		private string    m_DomainName  = "";

		/// <summary>
		/// Add new constructor.
		/// </summary>
		/// <param name="serverAPI"></param>
		public wfrm_Route(ServerAPI serverAPI) : this(serverAPI,null)
		{
		}

		/// <summary>
		/// Edit constructor.
		/// </summary>
		/// <param name="serverAPI"></param>
		/// <param name="dr"></param>
		public wfrm_Route(ServerAPI serverAPI,DataRow dr)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			m_ServerAPI = serverAPI;

			m_pRouteType.Items.Add("Route to mailbox","mailbox");
			m_pRouteType.Items.Add("Route to remote address","remoteaddress");
			m_pRouteType.SelectedIndex = 0;

			foreach(DataRowView drV in serverAPI.GetDomainList()){
				m_pDomains.Items.Add(drV["DomainName"].ToString(),drV["DomainID"].ToString());
			}
			
			if(dr != null){
				string mailbox = dr["Mailbox"].ToString();
				if(mailbox.ToUpper().StartsWith("REMOTE:")){
					mailbox = mailbox.Substring(7);
					m_pRouteType.SelectItemByTag("remoteaddress");
				}

				m_pDomains.SelectItemByTag(dr["DomainID"].ToString());
				m_pPattern.Text     = dr["Pattern"].ToString();
				m_pRouteTo.Text     = mailbox;
				m_pDescription.Text = dr["Description"].ToString();				
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
			this.m_pRouteType = new LumiSoft.UI.Controls.WComboBox();
			this.m_pCancel = new LumiSoft.UI.Controls.WButton();
			this.m_pDomains = new LumiSoft.UI.Controls.WComboBox();
			this.mt_symbolAT = new LumiSoft.UI.Controls.WLabel();
			this.m_pRouteTo = new LumiSoft.UI.Controls.WComboBox();
			this.m_pOk = new LumiSoft.UI.Controls.WButton();
			this.m_pDescription = new LumiSoft.UI.Controls.WEditBox();
			this.wLabel3 = new LumiSoft.UI.Controls.WLabel();
			this.m_pPattern = new LumiSoft.UI.Controls.WEditBox();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.SuspendLayout();
			// 
			// m_pRouteType
			// 
			this.m_pRouteType.DropDownWidth = 200;
			this.m_pRouteType.Location = new System.Drawing.Point(8, 54);
			this.m_pRouteType.Name = "m_pRouteType";
			this.m_pRouteType.EditStyle = LumiSoft.UI.Controls.EditStyle.Selectable;
			this.m_pRouteType.SelectedIndex = -1;
			this.m_pRouteType.Size = new System.Drawing.Size(200, 20);
			this.m_pRouteType.TabIndex = 20;
			this.m_pRouteType.UseStaticViewStyle = true;
			// 
			// m_pRouteType.ViewStyle
			// 
			this.m_pRouteType.VisibleItems = 10;
			this.m_pRouteType.SelectedIndexChanged += new System.EventHandler(this.m_pRouteType_SelectedIndexChanged);
			// 
			// m_pCancel
			// 
			this.m_pCancel.Location = new System.Drawing.Point(240, 150);
			this.m_pCancel.Name = "m_pCancel";
			this.m_pCancel.Size = new System.Drawing.Size(72, 24);
			this.m_pCancel.TabIndex = 11;
			this.m_pCancel.Text = "Cancel";
			this.m_pCancel.UseStaticViewStyle = true;
			// 
			// m_pCancel.ViewStyle
			// 
			this.m_pCancel.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pCancel_ButtonPressed);
			// 
			// m_pDomains
			// 
			this.m_pDomains.DropDownWidth = 144;
			this.m_pDomains.Location = new System.Drawing.Point(176, 22);
			this.m_pDomains.Name = "m_pDomains";
			this.m_pDomains.EditStyle = LumiSoft.UI.Controls.EditStyle.Selectable;
			this.m_pDomains.SelectedIndex = -1;
			this.m_pDomains.Size = new System.Drawing.Size(144, 20);
			this.m_pDomains.TabIndex = 13;
			this.m_pDomains.UseStaticViewStyle = true;
			// 
			// m_pDomains.ViewStyle
			// 
			this.m_pDomains.VisibleItems = 5;
			this.m_pDomains.SelectedIndexChanged += new System.EventHandler(this.m_pDomains_SelectedIndexChanged);
			// 
			// mt_symbolAT
			// 
			this.mt_symbolAT.Location = new System.Drawing.Point(160, 22);
			this.mt_symbolAT.Name = "mt_symbolAT";
			this.mt_symbolAT.Size = new System.Drawing.Size(16, 24);
			this.mt_symbolAT.TabIndex = 19;
			this.mt_symbolAT.Text = "@";
			// 
			// mt_symbolAT.ViewStyle
			// 
			// 
			// m_pRouteTo
			// 
			this.m_pRouteTo.DropDownWidth = 200;
			this.m_pRouteTo.Location = new System.Drawing.Point(8, 78);
			this.m_pRouteTo.Name = "m_pRouteTo";
			this.m_pRouteTo.EditStyle = LumiSoft.UI.Controls.EditStyle.Selectable;
			this.m_pRouteTo.SelectedIndex = -1;
			this.m_pRouteTo.Size = new System.Drawing.Size(200, 20);
			this.m_pRouteTo.TabIndex = 14;
			this.m_pRouteTo.UseStaticViewStyle = true;
			// 
			// m_pRouteTo.ViewStyle
			// 
			this.m_pRouteTo.VisibleItems = 10;
			// 
			// m_pOk
			// 
			this.m_pOk.Location = new System.Drawing.Point(160, 150);
			this.m_pOk.Name = "m_pOk";
			this.m_pOk.Size = new System.Drawing.Size(72, 24);
			this.m_pOk.TabIndex = 16;
			this.m_pOk.Text = "OK";
			this.m_pOk.UseStaticViewStyle = true;
			// 
			// m_pOk.ViewStyle
			// 
			this.m_pOk.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pOk_ButtonPressed);
			// 
			// m_pDescription
			// 
			this.m_pDescription.Location = new System.Drawing.Point(8, 118);
			this.m_pDescription.Name = "m_pDescription";
			this.m_pDescription.Size = new System.Drawing.Size(312, 20);
			this.m_pDescription.TabIndex = 15;
			this.m_pDescription.UseStaticViewStyle = true;
			// 
			// m_pDescription.ViewStyle
			// 
			// 
			// wLabel3
			// 
			this.wLabel3.Location = new System.Drawing.Point(8, 102);
			this.wLabel3.Name = "wLabel3";
			this.wLabel3.Size = new System.Drawing.Size(150, 16);
			this.wLabel3.TabIndex = 18;
			this.wLabel3.Text = "Description";
			this.wLabel3.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel3.ViewStyle
			// 
			// 
			// m_pPattern
			// 
			this.m_pPattern.Location = new System.Drawing.Point(8, 22);
			this.m_pPattern.Name = "m_pPattern";
			this.m_pPattern.Size = new System.Drawing.Size(152, 20);
			this.m_pPattern.TabIndex = 12;
			this.m_pPattern.UseStaticViewStyle = true;
			// 
			// m_pPattern.ViewStyle
			// 
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(8, 6);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(150, 16);
			this.wLabel1.TabIndex = 17;
			this.wLabel1.Text = "Match Pattern";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// wfrm_Route
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(328, 181);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_pRouteType,
																		  this.m_pCancel,
																		  this.m_pDomains,
																		  this.mt_symbolAT,
																		  this.m_pRouteTo,
																		  this.m_pOk,
																		  this.m_pDescription,
																		  this.wLabel3,
																		  this.m_pPattern,
																		  this.wLabel1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "wfrm_Route";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Route";
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		#region function m_pOk_ButtonPressed

		private void m_pOk_ButtonPressed(object sender, System.EventArgs e)
		{
			if(m_pDomains.SelectedItem == null){
				m_pDomains.FlashControl();
				return;
			}

			if(m_pPattern.Text.Length < 1){
				m_pPattern.FlashControl();
				return;
			}

			if(m_pRouteTo.Text.Length < 2){
				m_pRouteTo.FlashControl();
				return;
			}

			m_Pattern     = m_pPattern.Text;

			if(m_pRouteType.SelectedItem.Tag.ToString() == "mailbox"){
				m_MailBox = m_pRouteTo.Text;
			}
			else{
				m_MailBox = "REMOTE:" + m_pRouteTo.Text;
			}

			m_Description = m_pDescription.Text;
			m_DomainID    = m_pDomains.SelectedItem.Tag.ToString();
			m_DomainName  = m_pDomains.SelectedItem.Text;

			this.DialogResult = DialogResult.OK;			
		}

		#endregion

		#region function m_pCancel_ButtonPressed

		private void m_pCancel_ButtonPressed(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		#endregion

		#region function m_pDomains_SelectedIndexChanged

		private void m_pDomains_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			FillMailboxList();
		}

		#endregion

		#region function m_pRouteType_SelectedIndexChanged

		private void m_pRouteType_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(m_pRouteType.SelectedItem != null){
				if(m_pRouteType.SelectedItem.Tag.ToString() == "mailbox"){
					FillMailboxList();
					m_pRouteTo.EditStyle = LumiSoft.UI.Controls.EditStyle.Selectable;
					return;
				}

				if(m_pRouteType.SelectedItem.Tag.ToString() == "remoteaddress"){
					m_pRouteTo.EditStyle = LumiSoft.UI.Controls.EditStyle.Editable;
					m_pRouteTo.Items.Clear();
					return;
				}
			}
		}

		#endregion

		#endregion


		#region function FillMailboxList

		private void FillMailboxList()
		{
			if(m_pDomains.SelectedItem != null){
				m_pRouteTo.Items.Clear();
				foreach(DataRowView drV in m_ServerAPI.GetUserList(m_pDomains.SelectedItem.Tag.ToString())){
					m_pRouteTo.Items.Add(drV["UserName"].ToString());
				}
			}
		}

		#endregion

		
		#region Properties Implementation

		/// <summary>
		/// 
		/// </summary>
		public string Pattern
		{
			get{ return m_Pattern; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string MailBox
		{
			get{ return m_MailBox; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Description
		{
			get{ return m_Description; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string DomainID
		{
			get{ return m_DomainID; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string DomainName
		{
			get{ return m_DomainName; }
		}

		#endregion
	}
}
