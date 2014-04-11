using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MailServerManager.Forms
{
	/// <summary>
	/// Summary description for wfrm_EditUndeliveredText.
	/// </summary>
	public class wfrm_EditUndeliveredText : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WButton m_pOk;
		private LumiSoft.UI.Controls.WEditBox m_pText;
		private LumiSoft.UI.Controls.WButton m_pCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="template">Template text.</param>
		public wfrm_EditUndeliveredText(string template)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			m_pText.Text = template;
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
			this.m_pText = new LumiSoft.UI.Controls.WEditBox();
			this.m_pOk = new LumiSoft.UI.Controls.WButton();
			this.m_pCancel = new LumiSoft.UI.Controls.WButton();
			this.SuspendLayout();
			// 
			// m_pText
			// 
			this.m_pText.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_pText.DecimalPlaces = 2;
			this.m_pText.DecMaxValue = new System.Decimal(new int[] {
																		999999999,
																		0,
																		0,
																		0});
			this.m_pText.DecMinValue = new System.Decimal(new int[] {
																		999999999,
																		0,
																		0,
																		-2147483648});
			this.m_pText.Lines = new string[0];
			this.m_pText.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Text;
			this.m_pText.MaxLength = 32767;
			this.m_pText.Multiline = true;
			this.m_pText.Name = "m_pText";
			this.m_pText.PasswordChar = '\0';
			this.m_pText.ReadOnly = false;
			this.m_pText.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.m_pText.Size = new System.Drawing.Size(450, 240);
			this.m_pText.TabIndex = 0;
			this.m_pText.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pText.UseStaticViewStyle = true;
			// 
			// m_pOk
			// 
			this.m_pOk.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.m_pOk.Location = new System.Drawing.Point(290, 250);
			this.m_pOk.Name = "m_pOk";
			this.m_pOk.Size = new System.Drawing.Size(72, 24);
			this.m_pOk.TabIndex = 1;
			this.m_pOk.Text = "OK";
			this.m_pOk.UseStaticViewStyle = true;
			this.m_pOk.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pOk_ButtonPressed);
			// 
			// m_pCancel
			// 
			this.m_pCancel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.m_pCancel.Location = new System.Drawing.Point(370, 250);
			this.m_pCancel.Name = "m_pCancel";
			this.m_pCancel.Size = new System.Drawing.Size(72, 24);
			this.m_pCancel.TabIndex = 2;
			this.m_pCancel.Text = "Cancel";
			this.m_pCancel.UseStaticViewStyle = true;
			this.m_pCancel.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pCancel_ButtonPressed);
			// 
			// wfrm_EditUndeliveredText
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(450, 279);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_pCancel,
																		  this.m_pOk,
																		  this.m_pText});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "wfrm_EditUndeliveredText";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit template";
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		private void m_pOk_ButtonPressed(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void m_pCancel_ButtonPressed(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// 
		/// </summary>
		public string wp_Template
		{
			get{ return m_pText.Text; }
		}

		#endregion

	}
}
