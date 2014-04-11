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
	/// Summary description for wfrm_Domain.
	/// </summary>
	public class wfrm_Domain : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WButton m_pAdd;
		private LumiSoft.UI.Controls.WButton m_pCancel;
		private LumiSoft.UI.Controls.WEditBox m_pDescription;
		private LumiSoft.UI.Controls.WEditBox m_pDomain;
		private LumiSoft.UI.Controls.WLabel mt_description;
		private LumiSoft.UI.Controls.WLabel mt_domain;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private ServerAPI m_ServerAPI   = null;
		private DataRow   m_DataRow     = null;
		private string    m_Domain      = "";
		private string    m_Description = "";

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="serverAPI"></param>
		public wfrm_Domain(ServerAPI serverAPI)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			try
			{
				m_ServerAPI = serverAPI;
			}
			catch(Exception x)
			{
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
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
			this.m_pAdd = new LumiSoft.UI.Controls.WButton();
			this.m_pCancel = new LumiSoft.UI.Controls.WButton();
			this.m_pDescription = new LumiSoft.UI.Controls.WEditBox();
			this.m_pDomain = new LumiSoft.UI.Controls.WEditBox();
			this.mt_description = new LumiSoft.UI.Controls.WLabel();
			this.mt_domain = new LumiSoft.UI.Controls.WLabel();
			this.SuspendLayout();
			// 
			// m_pAdd
			// 
			this.m_pAdd.Location = new System.Drawing.Point(128, 94);
			this.m_pAdd.Name = "m_pAdd";
			this.m_pAdd.Size = new System.Drawing.Size(72, 24);
			this.m_pAdd.TabIndex = 9;
			this.m_pAdd.Text = "OK";
			this.m_pAdd.UseStaticViewStyle = true;
			// 
			// m_pAdd.ViewStyle
			// 
			this.m_pAdd.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pAdd_Click);
			// 
			// m_pCancel
			// 
			this.m_pCancel.Location = new System.Drawing.Point(208, 94);
			this.m_pCancel.Name = "m_pCancel";
			this.m_pCancel.Size = new System.Drawing.Size(72, 24);
			this.m_pCancel.TabIndex = 6;
			this.m_pCancel.Text = "Cancel";
			this.m_pCancel.UseStaticViewStyle = true;
			// 
			// m_pCancel.ViewStyle
			// 
			this.m_pCancel.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pCancel_Click);
			// 
			// m_pDescription
			// 
			this.m_pDescription.Location = new System.Drawing.Point(8, 62);
			this.m_pDescription.Name = "m_pDescription";
			this.m_pDescription.Size = new System.Drawing.Size(280, 20);
			this.m_pDescription.TabIndex = 8;
			this.m_pDescription.UseStaticViewStyle = true;
			// 
			// m_pDescription.ViewStyle
			// 
			// 
			// m_pDomain
			// 
			this.m_pDomain.Location = new System.Drawing.Point(8, 22);
			this.m_pDomain.Name = "m_pDomain";
			this.m_pDomain.Size = new System.Drawing.Size(136, 20);
			this.m_pDomain.TabIndex = 7;
			this.m_pDomain.UseStaticViewStyle = true;
			// 
			// m_pDomain.ViewStyle
			// 
			// 
			// mt_description
			// 
			this.mt_description.Location = new System.Drawing.Point(8, 46);
			this.mt_description.Name = "mt_description";
			this.mt_description.Size = new System.Drawing.Size(104, 16);
			this.mt_description.TabIndex = 11;
			this.mt_description.Text = "Description";
			this.mt_description.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_description.ViewStyle
			// 
			// 
			// mt_domain
			// 
			this.mt_domain.Location = new System.Drawing.Point(8, 6);
			this.mt_domain.Name = "mt_domain";
			this.mt_domain.Size = new System.Drawing.Size(104, 16);
			this.mt_domain.TabIndex = 10;
			this.mt_domain.Text = "Domain";
			this.mt_domain.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_domain.ViewStyle
			// 
			// 
			// wfrm_Domain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(296, 125);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_pAdd,
																		  this.m_pCancel,
																		  this.m_pDescription,
																		  this.m_pDomain,
																		  this.mt_description,
																		  this.mt_domain});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "wfrm_Domain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Domain";
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		#region function m_pAdd_Click

		private void m_pAdd_Click(object sender, System.EventArgs e)
		{
			try
			{
				if(m_pDomain.Text.Length <= 0){
					MessageBox.Show("Domain name cannot be empty!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
					m_pDomain.FlashControl();
					return;
				}

				if(m_ServerAPI.DomainExists(m_pDomain.Text)){
					MessageBox.Show("Domain already exists!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
					return;
				}

				m_Domain      = m_pDomain.Text;
				m_Description = m_pDescription.Text;


			//	m_DataRow = m_ServerAPI.AddDomain(m_pDomain.Text,m_pDescription.Text);
			//	if(m_DataRow == null){
			//		MessageBox.Show("Error creating domain!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
			//		return;
			//	}

				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch(Exception x)
			{
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}
		}

		#endregion

		#region function m_pCancel_Click

		private void m_pCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#endregion

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets domain name.
		/// </summary>
		public string wp_Domain
		{
			get{ return m_Domain; }
		}

		/// <summary>
		/// Gets domain description.
		/// </summary>
		public string wp_Description
		{
			get{ return m_Description; }
		}

		public DataRow wp_Dr
		{
			get{ return m_DataRow; }
		}

		#endregion

	}
}
