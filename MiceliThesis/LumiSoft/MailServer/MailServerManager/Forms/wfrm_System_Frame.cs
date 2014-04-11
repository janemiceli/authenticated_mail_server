using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Net;
using LumiSoft.MailServer;

namespace MailServerManager.Forms
{
	/// <summary>
	/// Summary description for wfrm_System_Frame.
	/// </summary>
	public class wfrm_System_Frame : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WTabs.WTab wTab1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private ServerAPI            m_ServerAPI = null;
		private DataSet              dsSettings  = null;
		private wfrm_System_General  m_pGene     = null;
		private wfrm_System_SMTP     m_pSMTP     = null;
		private wfrm_System_POP3     m_pPOP3     = null;
		private wfrm_System_IMAP     m_pIMAP     = null;
		private wfrm_System_Delivery m_pDely     = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="serverAPI"></param>
		public wfrm_System_Frame(ServerAPI serverAPI)
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

				dsSettings = m_ServerAPI.GetSettings();
				dsSettings.AcceptChanges();
			}
			catch(Exception x){
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}

			InitTab();
		}

		#region function Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			try
			{
				m_pGene.SaveData();
				m_pSMTP.SaveData(); 
				m_pPOP3.SaveData();
				m_pIMAP.SaveData();
				m_pDely.SaveData();

				if(dsSettings.HasChanges()){
					if(MessageBox.Show(null,"Do you want to save settings?","Save",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes){
						m_ServerAPI.UpdateSettings(dsSettings);
						m_ServerAPI.DatabaseTypeChanged();
					}
				}
			}
			catch(Exception x){
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}

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
			this.wTab1 = new LumiSoft.UI.Controls.WTabs.WTab();
			this.SuspendLayout();
			// 
			// wTab1
			// 
			this.wTab1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wTab1.Name = "wTab1";
			this.wTab1.SelectedTab = null;
			this.wTab1.Size = new System.Drawing.Size(292, 273);
			this.wTab1.TabIndex = 0;
			// 
			// wfrm_System_Frame
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.wTab1});
			this.Name = "wfrm_System_Frame";
			this.Text = "wfrm_System_Frame";
			this.ResumeLayout(false);

		}
		#endregion


		#region function InitTab

		/// <summary>
		/// Add tabs to tab control.
		/// </summary>
		private void InitTab()
		{			
			m_pGene = new wfrm_System_General(dsSettings);
			m_pSMTP = new wfrm_System_SMTP(dsSettings);
			m_pPOP3 = new wfrm_System_POP3(dsSettings);
			m_pIMAP = new wfrm_System_IMAP(dsSettings);
			m_pDely = new wfrm_System_Delivery(dsSettings);

			wTab1.AddTab(m_pGene,"General");
			wTab1.AddTab(m_pSMTP,"SMTP");
			wTab1.AddTab(m_pPOP3,"POP3");
			wTab1.AddTab(m_pIMAP,"IMAP");
			wTab1.AddTab(m_pDely,"Delivery");

			wTab1.SelectFirstTab();
		}

		#endregion

	}
}
