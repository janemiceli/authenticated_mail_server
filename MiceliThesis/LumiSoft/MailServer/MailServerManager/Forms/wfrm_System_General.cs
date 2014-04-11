using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using LumiSoft.MailServer;

namespace MailServerManager.Forms
{
	/// <summary>
	/// Summary description for wfrm_System_General.
	/// </summary>
	public class wfrm_System_General : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private LumiSoft.UI.Controls.WLabel wLabel12;
		private LumiSoft.UI.Controls.WEditBox m_pMailStorePath;
		private LumiSoft.UI.Controls.WEditBox m_pConStr;
		private LumiSoft.UI.Controls.WComboBox m_pDbType;
		private LumiSoft.UI.Controls.WEditBox m_pServerLogPath;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		private LumiSoft.UI.Controls.WLabel wLabel2;
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pLogServerEvents;
		private LumiSoft.UI.Controls.WEditBox m_pPOP3path;
		private LumiSoft.UI.Controls.WEditBox m_pSMTPpath;
		private LumiSoft.UI.Controls.WLabel mt_path2;
		private LumiSoft.UI.Controls.WLabel mt_path1;
		private LumiSoft.UI.Controls.WLabel mt_logPOP3;
		private LumiSoft.UI.Controls.WLabel mt_logSMTP;
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pLogPOP3Cmds;
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pLogSMTPCmds;
		private System.Windows.Forms.GroupBox groupBox2;
		private LumiSoft.UI.Controls.WButton m_pGetSmtpLogPath;
		private LumiSoft.UI.Controls.WButton m_pGetPop3LogPath;
		private LumiSoft.UI.Controls.WButton m_pGetServerLogPath;
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pLogIMAPCmds;
		private LumiSoft.UI.Controls.WLabel wLabel3;
		private LumiSoft.UI.Controls.WLabel wLabel4;
		private LumiSoft.UI.Controls.WEditBox m_pIMAPpath;
		private LumiSoft.UI.Controls.WButton m_pGetIMAPLogPath;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private DataSet dsVal = null;

		/// <summary>
		/// Default 
		/// </summary>
		/// <param name="ds"></param>
		public wfrm_System_General(DataSet ds)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			dsVal       = ds;

			m_pDbType.Items.Add("XML","XML");
			m_pDbType.Items.Add("MSSQL","MSSQL");
			m_pDbType.SelectedIndex = 0;

			LoadData(ds);
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.wLabel12 = new LumiSoft.UI.Controls.WLabel();
			this.m_pMailStorePath = new LumiSoft.UI.Controls.WEditBox();
			this.m_pConStr = new LumiSoft.UI.Controls.WEditBox();
			this.m_pDbType = new LumiSoft.UI.Controls.WComboBox();
			this.m_pServerLogPath = new LumiSoft.UI.Controls.WEditBox();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel2 = new LumiSoft.UI.Controls.WLabel();
			this.m_pLogServerEvents = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.m_pPOP3path = new LumiSoft.UI.Controls.WEditBox();
			this.m_pSMTPpath = new LumiSoft.UI.Controls.WEditBox();
			this.mt_path2 = new LumiSoft.UI.Controls.WLabel();
			this.mt_path1 = new LumiSoft.UI.Controls.WLabel();
			this.mt_logPOP3 = new LumiSoft.UI.Controls.WLabel();
			this.mt_logSMTP = new LumiSoft.UI.Controls.WLabel();
			this.m_pLogPOP3Cmds = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.m_pLogSMTPCmds = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.m_pLogIMAPCmds = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.wLabel3 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel4 = new LumiSoft.UI.Controls.WLabel();
			this.m_pIMAPpath = new LumiSoft.UI.Controls.WEditBox();
			this.m_pGetServerLogPath = new LumiSoft.UI.Controls.WButton();
			this.m_pGetPop3LogPath = new LumiSoft.UI.Controls.WButton();
			this.m_pGetSmtpLogPath = new LumiSoft.UI.Controls.WButton();
			this.m_pGetIMAPLogPath = new LumiSoft.UI.Controls.WButton();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.wLabel12);
			this.groupBox1.Controls.Add(this.m_pMailStorePath);
			this.groupBox1.Controls.Add(this.m_pConStr);
			this.groupBox1.Controls.Add(this.m_pDbType);
			this.groupBox1.Location = new System.Drawing.Point(8, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(456, 88);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Storage";
			// 
			// wLabel12
			// 
			this.wLabel12.Location = new System.Drawing.Point(16, 48);
			this.wLabel12.Name = "wLabel12";
			this.wLabel12.Size = new System.Drawing.Size(88, 24);
			this.wLabel12.TabIndex = 3;
			this.wLabel12.Text = "MailStore Path";
			this.wLabel12.UseStaticViewStyle = false;
			// 
			// wLabel12.ViewStyle
			// 
			// 
			// m_pMailStorePath
			// 
			this.m_pMailStorePath.DrawBorder = true;
			this.m_pMailStorePath.Location = new System.Drawing.Point(112, 48);
			this.m_pMailStorePath.Name = "m_pMailStorePath";
			this.m_pMailStorePath.Size = new System.Drawing.Size(328, 20);
			this.m_pMailStorePath.TabIndex = 2;
			this.m_pMailStorePath.UseStaticViewStyle = true;
			// 
			// m_pMailStorePath.ViewStyle
			// 
			// 
			// m_pConStr
			// 
			this.m_pConStr.DrawBorder = true;
			this.m_pConStr.Location = new System.Drawing.Point(112, 24);
			this.m_pConStr.Name = "m_pConStr";
			this.m_pConStr.Size = new System.Drawing.Size(328, 20);
			this.m_pConStr.TabIndex = 1;
			this.m_pConStr.UseStaticViewStyle = true;
			// 
			// m_pConStr.ViewStyle
			// 
			// 
			// m_pDbType
			// 
			this.m_pDbType.DrawBorder = true;
			this.m_pDbType.DropDownWidth = 88;
			this.m_pDbType.EditStyle = LumiSoft.UI.Controls.EditStyle.Selectable;
			this.m_pDbType.Location = new System.Drawing.Point(16, 24);
			this.m_pDbType.Name = "m_pDbType";
			this.m_pDbType.SelectedIndex = -1;
			this.m_pDbType.SelectionLength = 0;
			this.m_pDbType.SelectionStart = 0;
			this.m_pDbType.Size = new System.Drawing.Size(88, 20);
			this.m_pDbType.TabIndex = 0;
			this.m_pDbType.UseStaticViewStyle = true;
			// 
			// m_pDbType.ViewStyle
			// 
			this.m_pDbType.VisibleItems = 10;
			// 
			// m_pServerLogPath
			// 
			this.m_pServerLogPath.DrawBorder = true;
			this.m_pServerLogPath.Location = new System.Drawing.Point(80, 200);
			this.m_pServerLogPath.Name = "m_pServerLogPath";
			this.m_pServerLogPath.ReadOnly = true;
			this.m_pServerLogPath.Size = new System.Drawing.Size(344, 20);
			this.m_pServerLogPath.TabIndex = 5;
			this.m_pServerLogPath.UseStaticViewStyle = true;
			// 
			// m_pServerLogPath.ViewStyle
			// 
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(16, 200);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(62, 24);
			this.wLabel1.TabIndex = 11;
			this.wLabel1.Text = "Path:";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// wLabel2
			// 
			this.wLabel2.Location = new System.Drawing.Point(32, 176);
			this.wLabel2.Name = "wLabel2";
			this.wLabel2.TabIndex = 10;
			this.wLabel2.Text = "Log server events";
			this.wLabel2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel2.ViewStyle
			// 
			// 
			// m_pLogServerEvents
			// 
			this.m_pLogServerEvents.Checked = true;
			this.m_pLogServerEvents.DrawBorder = true;
			this.m_pLogServerEvents.Location = new System.Drawing.Point(16, 176);
			this.m_pLogServerEvents.Name = "m_pLogServerEvents";
			this.m_pLogServerEvents.ReadOnly = true;
			this.m_pLogServerEvents.Size = new System.Drawing.Size(16, 22);
			this.m_pLogServerEvents.TabIndex = 4;
			this.m_pLogServerEvents.UseStaticViewStyle = true;
			// 
			// m_pLogServerEvents.ViewStyle
			// 
			// 
			// m_pPOP3path
			// 
			this.m_pPOP3path.DrawBorder = true;
			this.m_pPOP3path.Location = new System.Drawing.Point(80, 144);
			this.m_pPOP3path.Name = "m_pPOP3path";
			this.m_pPOP3path.ReadOnly = true;
			this.m_pPOP3path.Size = new System.Drawing.Size(344, 20);
			this.m_pPOP3path.TabIndex = 3;
			this.m_pPOP3path.UseStaticViewStyle = true;
			// 
			// m_pPOP3path.ViewStyle
			// 
			// 
			// m_pSMTPpath
			// 
			this.m_pSMTPpath.DrawBorder = true;
			this.m_pSMTPpath.Location = new System.Drawing.Point(80, 48);
			this.m_pSMTPpath.Name = "m_pSMTPpath";
			this.m_pSMTPpath.ReadOnly = true;
			this.m_pSMTPpath.Size = new System.Drawing.Size(344, 20);
			this.m_pSMTPpath.TabIndex = 1;
			this.m_pSMTPpath.UseStaticViewStyle = true;
			// 
			// m_pSMTPpath.ViewStyle
			// 
			// 
			// mt_path2
			// 
			this.mt_path2.Location = new System.Drawing.Point(16, 144);
			this.mt_path2.Name = "mt_path2";
			this.mt_path2.Size = new System.Drawing.Size(62, 24);
			this.mt_path2.TabIndex = 9;
			this.mt_path2.Text = "Path:";
			this.mt_path2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_path2.ViewStyle
			// 
			// 
			// mt_path1
			// 
			this.mt_path1.Location = new System.Drawing.Point(16, 48);
			this.mt_path1.Name = "mt_path1";
			this.mt_path1.Size = new System.Drawing.Size(62, 24);
			this.mt_path1.TabIndex = 7;
			this.mt_path1.Text = "Path:";
			this.mt_path1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_path1.ViewStyle
			// 
			// 
			// mt_logPOP3
			// 
			this.mt_logPOP3.Location = new System.Drawing.Point(32, 120);
			this.mt_logPOP3.Name = "mt_logPOP3";
			this.mt_logPOP3.TabIndex = 8;
			this.mt_logPOP3.Text = "Log POP3 commands";
			this.mt_logPOP3.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_logPOP3.ViewStyle
			// 
			// 
			// mt_logSMTP
			// 
			this.mt_logSMTP.Location = new System.Drawing.Point(32, 24);
			this.mt_logSMTP.Name = "mt_logSMTP";
			this.mt_logSMTP.TabIndex = 6;
			this.mt_logSMTP.Text = "Log SMTP commands";
			this.mt_logSMTP.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_logSMTP.ViewStyle
			// 
			// 
			// m_pLogPOP3Cmds
			// 
			this.m_pLogPOP3Cmds.Checked = false;
			this.m_pLogPOP3Cmds.DrawBorder = true;
			this.m_pLogPOP3Cmds.Location = new System.Drawing.Point(16, 120);
			this.m_pLogPOP3Cmds.Name = "m_pLogPOP3Cmds";
			this.m_pLogPOP3Cmds.ReadOnly = false;
			this.m_pLogPOP3Cmds.Size = new System.Drawing.Size(16, 22);
			this.m_pLogPOP3Cmds.TabIndex = 2;
			this.m_pLogPOP3Cmds.UseStaticViewStyle = true;
			// 
			// m_pLogPOP3Cmds.ViewStyle
			// 
			// 
			// m_pLogSMTPCmds
			// 
			this.m_pLogSMTPCmds.Checked = false;
			this.m_pLogSMTPCmds.DrawBorder = true;
			this.m_pLogSMTPCmds.Location = new System.Drawing.Point(16, 24);
			this.m_pLogSMTPCmds.Name = "m_pLogSMTPCmds";
			this.m_pLogSMTPCmds.ReadOnly = false;
			this.m_pLogSMTPCmds.Size = new System.Drawing.Size(16, 22);
			this.m_pLogSMTPCmds.TabIndex = 0;
			this.m_pLogSMTPCmds.UseStaticViewStyle = true;
			// 
			// m_pLogSMTPCmds.ViewStyle
			// 
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.m_pGetIMAPLogPath);
			this.groupBox2.Controls.Add(this.m_pLogIMAPCmds);
			this.groupBox2.Controls.Add(this.wLabel3);
			this.groupBox2.Controls.Add(this.wLabel4);
			this.groupBox2.Controls.Add(this.m_pIMAPpath);
			this.groupBox2.Controls.Add(this.m_pGetServerLogPath);
			this.groupBox2.Controls.Add(this.m_pGetPop3LogPath);
			this.groupBox2.Controls.Add(this.m_pGetSmtpLogPath);
			this.groupBox2.Controls.Add(this.m_pLogSMTPCmds);
			this.groupBox2.Controls.Add(this.mt_logSMTP);
			this.groupBox2.Controls.Add(this.mt_path1);
			this.groupBox2.Controls.Add(this.m_pSMTPpath);
			this.groupBox2.Controls.Add(this.m_pLogPOP3Cmds);
			this.groupBox2.Controls.Add(this.mt_logPOP3);
			this.groupBox2.Controls.Add(this.m_pPOP3path);
			this.groupBox2.Controls.Add(this.mt_path2);
			this.groupBox2.Controls.Add(this.wLabel2);
			this.groupBox2.Controls.Add(this.wLabel1);
			this.groupBox2.Controls.Add(this.m_pLogServerEvents);
			this.groupBox2.Controls.Add(this.m_pServerLogPath);
			this.groupBox2.Location = new System.Drawing.Point(8, 112);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(456, 232);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Logs";
			// 
			// m_pLogIMAPCmds
			// 
			this.m_pLogIMAPCmds.Checked = false;
			this.m_pLogIMAPCmds.DrawBorder = true;
			this.m_pLogIMAPCmds.Location = new System.Drawing.Point(16, 72);
			this.m_pLogIMAPCmds.Name = "m_pLogIMAPCmds";
			this.m_pLogIMAPCmds.ReadOnly = false;
			this.m_pLogIMAPCmds.Size = new System.Drawing.Size(16, 22);
			this.m_pLogIMAPCmds.TabIndex = 15;
			this.m_pLogIMAPCmds.UseStaticViewStyle = true;
			// 
			// m_pLogIMAPCmds.ViewStyle
			// 
			// 
			// wLabel3
			// 
			this.wLabel3.Location = new System.Drawing.Point(32, 72);
			this.wLabel3.Name = "wLabel3";
			this.wLabel3.TabIndex = 17;
			this.wLabel3.Text = "Log IMAP commands";
			this.wLabel3.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel3.ViewStyle
			// 
			// 
			// wLabel4
			// 
			this.wLabel4.Location = new System.Drawing.Point(16, 96);
			this.wLabel4.Name = "wLabel4";
			this.wLabel4.Size = new System.Drawing.Size(62, 24);
			this.wLabel4.TabIndex = 18;
			this.wLabel4.Text = "Path:";
			this.wLabel4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel4.ViewStyle
			// 
			// 
			// m_pIMAPpath
			// 
			this.m_pIMAPpath.DrawBorder = true;
			this.m_pIMAPpath.Location = new System.Drawing.Point(80, 96);
			this.m_pIMAPpath.Name = "m_pIMAPpath";
			this.m_pIMAPpath.ReadOnly = true;
			this.m_pIMAPpath.Size = new System.Drawing.Size(344, 20);
			this.m_pIMAPpath.TabIndex = 16;
			this.m_pIMAPpath.UseStaticViewStyle = true;
			// 
			// m_pIMAPpath.ViewStyle
			// 
			// 
			// m_pGetServerLogPath
			// 
			this.m_pGetServerLogPath.DrawBorder = true;
			this.m_pGetServerLogPath.Location = new System.Drawing.Point(425, 200);
			this.m_pGetServerLogPath.Name = "m_pGetServerLogPath";
			this.m_pGetServerLogPath.Size = new System.Drawing.Size(24, 20);
			this.m_pGetServerLogPath.TabIndex = 14;
			this.m_pGetServerLogPath.Text = "...";
			this.m_pGetServerLogPath.UseStaticViewStyle = true;
			// 
			// m_pGetServerLogPath.ViewStyle
			// 
			this.m_pGetServerLogPath.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pGetServerLogPath_ButtonPressed);
			// 
			// m_pGetPop3LogPath
			// 
			this.m_pGetPop3LogPath.DrawBorder = true;
			this.m_pGetPop3LogPath.Location = new System.Drawing.Point(425, 144);
			this.m_pGetPop3LogPath.Name = "m_pGetPop3LogPath";
			this.m_pGetPop3LogPath.Size = new System.Drawing.Size(24, 20);
			this.m_pGetPop3LogPath.TabIndex = 13;
			this.m_pGetPop3LogPath.Text = "...";
			this.m_pGetPop3LogPath.UseStaticViewStyle = true;
			// 
			// m_pGetPop3LogPath.ViewStyle
			// 
			this.m_pGetPop3LogPath.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pGetPop3LogPath_ButtonPressed);
			// 
			// m_pGetSmtpLogPath
			// 
			this.m_pGetSmtpLogPath.DrawBorder = true;
			this.m_pGetSmtpLogPath.Location = new System.Drawing.Point(425, 48);
			this.m_pGetSmtpLogPath.Name = "m_pGetSmtpLogPath";
			this.m_pGetSmtpLogPath.Size = new System.Drawing.Size(24, 20);
			this.m_pGetSmtpLogPath.TabIndex = 12;
			this.m_pGetSmtpLogPath.Text = "...";
			this.m_pGetSmtpLogPath.UseStaticViewStyle = true;
			// 
			// m_pGetSmtpLogPath.ViewStyle
			// 
			this.m_pGetSmtpLogPath.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pGetSmtpLogPath_ButtonPressed);
			// 
			// m_pGetIMAPLogPath
			// 
			this.m_pGetIMAPLogPath.DrawBorder = true;
			this.m_pGetIMAPLogPath.Location = new System.Drawing.Point(424, 96);
			this.m_pGetIMAPLogPath.Name = "m_pGetIMAPLogPath";
			this.m_pGetIMAPLogPath.Size = new System.Drawing.Size(24, 20);
			this.m_pGetIMAPLogPath.TabIndex = 19;
			this.m_pGetIMAPLogPath.Text = "...";
			this.m_pGetIMAPLogPath.UseStaticViewStyle = true;
			// 
			// m_pGetIMAPLogPath.ViewStyle
			// 
			this.m_pGetIMAPLogPath.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pGetImapLogPath_ButtonPressed);
			// 
			// wfrm_System_General
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 357);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "wfrm_System_General";
			this.Text = "wfrm_System_General";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		private void m_pGetSmtpLogPath_ButtonPressed(object sender, System.EventArgs e)
		{
			using(FolderBrowserDialog dlg = new FolderBrowserDialog()){
				dlg.SelectedPath = m_pSMTPpath.Text;
				if(dlg.ShowDialog() == DialogResult.OK){
					m_pSMTPpath.Text = dlg.SelectedPath;
					if(m_pSMTPpath.Text.ToLower() == (Application.StartupPath + "\\Logs\\Smtp").ToLower()){
						m_pSMTPpath.Text = "";
					}
				}
			}
		}

		private void m_pGetPop3LogPath_ButtonPressed(object sender, System.EventArgs e)
		{
			using(FolderBrowserDialog dlg = new FolderBrowserDialog()){
				dlg.SelectedPath = m_pPOP3path.Text;
				if(dlg.ShowDialog() == DialogResult.OK){
					m_pPOP3path.Text = dlg.SelectedPath;
					if(m_pPOP3path.Text.ToLower() == (Application.StartupPath + "\\Logs\\Pop3").ToLower()){
						m_pPOP3path.Text = "";
					}
				}
			}
		}

		private void m_pGetImapLogPath_ButtonPressed(object sender, System.EventArgs e)
		{
			using(FolderBrowserDialog dlg = new FolderBrowserDialog()){
				dlg.SelectedPath = m_pIMAPpath.Text;
				if(dlg.ShowDialog() == DialogResult.OK){
					m_pIMAPpath.Text = dlg.SelectedPath;
					if(m_pIMAPpath.Text.ToLower() == (Application.StartupPath + "\\Logs\\IMAP").ToLower()){
						m_pIMAPpath.Text = "";
					}
				}
			}
		}

		private void m_pGetServerLogPath_ButtonPressed(object sender, System.EventArgs e)
		{
			using(FolderBrowserDialog dlg = new FolderBrowserDialog()){
				dlg.SelectedPath = m_pServerLogPath.Text;
				if(dlg.ShowDialog() == DialogResult.OK){
					m_pServerLogPath.Text = dlg.SelectedPath;
					if(m_pServerLogPath.Text.ToLower() == (Application.StartupPath + "\\Logs\\Server").ToLower()){
						m_pServerLogPath.Text = "";
					}
				}
			}
		}

		#endregion


		#region function LoadData

		private void LoadData(DataSet ds)
		{
			try
			{
				DataRow dr = ds.Tables["Settings"].Rows[0];
				m_pMailStorePath.Text  = dr["MailRoot"].ToString();
				m_pLogSMTPCmds.Checked = Convert.ToBoolean(dr["LogSMTPCmds"]);
				m_pLogPOP3Cmds.Checked = Convert.ToBoolean(dr["LogPOP3Cmds"]);
				m_pLogIMAPCmds.Checked = Convert.ToBoolean(dr["LogIMAPCmds"]);
				m_pSMTPpath.Text       = dr["SMTP_LogPath"].ToString();
				m_pPOP3path.Text       = dr["POP3_LogPath"].ToString();
				m_pIMAPpath.Text       = dr["IMAP_LogPath"].ToString();
				m_pServerLogPath.Text  = dr["Server_LogPath"].ToString();
				m_pConStr.Text         = dr["ConnectionString"].ToString();

				m_pDbType.SelectItemByTag(dr["DataBaseType"].ToString());

				if(m_pDbType.Text == "MSSQL"){					
					m_pConStr.Enabled = true;
//					m_pTest.Enabled = true;
				}	
			}
			catch(Exception x)
			{
				wfrm_Error frm = new wfrm_Error(x,new System.Diagnostics.StackTrace());
				frm.ShowDialog(this);
			}
		}

		#endregion

		#region function SaveData

		internal void SaveData()
		{
			DataRow dr = dsVal.Tables["Settings"].Rows[0];
			if(dr["MailRoot"].ToString()	        != m_pMailStorePath.Text)  { dr["MailRoot"]         =  m_pMailStorePath.Text; }
			if(Convert.ToBoolean(dr["LogSMTPCmds"]) != m_pLogSMTPCmds.Checked) { dr["LogSMTPCmds"]      =  m_pLogSMTPCmds.Checked; }
			if(Convert.ToBoolean(dr["LogPOP3Cmds"])	!= m_pLogPOP3Cmds.Checked) { dr["LogPOP3Cmds"]      =  m_pLogPOP3Cmds.Checked; }
			if(Convert.ToBoolean(dr["LogIMAPCmds"])	!= m_pLogIMAPCmds.Checked) { dr["LogIMAPCmds"]      =  m_pLogIMAPCmds.Checked; }
			if(dr["ConnectionString"].ToString()    != m_pConStr.Text)         { dr["ConnectionString"] =  m_pConStr.Text; }
			if(dr["DataBaseType"].ToString()	    != m_pDbType.Text)         { dr["DataBaseType"]     =  m_pDbType.Text; }
			if(dr["SMTP_LogPath"].ToString()	    != m_pSMTPpath.Text)       { dr["SMTP_LogPath"]     =  m_pSMTPpath.Text; }
			if(dr["POP3_LogPath"].ToString()	    != m_pPOP3path.Text)       { dr["POP3_LogPath"]     =  m_pPOP3path.Text; }
			if(dr["IMAP_LogPath"].ToString()	    != m_pIMAPpath.Text)       { dr["IMAP_LogPath"]     =  m_pIMAPpath.Text; }
			if(dr["Server_LogPath"].ToString()	    != m_pServerLogPath.Text)  { dr["Server_LogPath"]   =  m_pServerLogPath.Text; }					
		}

		#endregion
		
	}
}
