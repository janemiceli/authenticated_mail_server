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
	/// Summary description for wfrm_Filter.
	/// </summary>
	public class wfrm_Filter : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WButton m_pGet;
		private LumiSoft.UI.Controls.WEditBox m_pAssembly;
		private LumiSoft.UI.Controls.WEditBox m_pClass;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		private LumiSoft.UI.Controls.WLabel wLabel2;
		private LumiSoft.UI.Controls.WLabel wLabel3;
		private LumiSoft.UI.Controls.WButton m_pCancel;
		private LumiSoft.UI.Controls.WButton m_pOk;
		private LumiSoft.UI.Controls.WCheckBox.WCheckBox m_pEnabled;
		private LumiSoft.UI.Controls.WLabel mt_Enabled;
		private LumiSoft.UI.Controls.WSpinEdit m_pCost;
		private LumiSoft.UI.Controls.WLabel wLabel4;
		private LumiSoft.UI.Controls.WEditBox m_pDescription;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Add new constructor.
		/// </summary>
		/// <param name="serverAPI"></param>
		public wfrm_Filter(ServerAPI serverAPI) : this(serverAPI,null)
		{
		}
        
		public wfrm_Filter(ServerAPI serverAPI,DataRow dr)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			if(dr != null){
				m_pDescription.Text = dr["Description"].ToString();
				m_pAssembly.Text    = dr["Assembly"].ToString();
				m_pClass.Text       = dr["ClassName"].ToString();
				m_pCost.DecValue    = Convert.ToDecimal(dr["Cost"]);
				m_pEnabled.Checked  = Convert.ToBoolean(dr["Enabled"]);
			}
		}

		#region method Dispose

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
			this.m_pAssembly = new LumiSoft.UI.Controls.WEditBox();
			this.m_pClass = new LumiSoft.UI.Controls.WEditBox();
			this.m_pCost = new LumiSoft.UI.Controls.WSpinEdit();
			this.m_pGet = new LumiSoft.UI.Controls.WButton();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel2 = new LumiSoft.UI.Controls.WLabel();
			this.wLabel3 = new LumiSoft.UI.Controls.WLabel();
			this.m_pCancel = new LumiSoft.UI.Controls.WButton();
			this.m_pOk = new LumiSoft.UI.Controls.WButton();
			this.m_pEnabled = new LumiSoft.UI.Controls.WCheckBox.WCheckBox();
			this.mt_Enabled = new LumiSoft.UI.Controls.WLabel();
			this.wLabel4 = new LumiSoft.UI.Controls.WLabel();
			this.m_pDescription = new LumiSoft.UI.Controls.WEditBox();
			this.SuspendLayout();
			// 
			// m_pAssembly
			// 
			this.m_pAssembly.DrawBorder = true;
			this.m_pAssembly.Location = new System.Drawing.Point(8, 56);
			this.m_pAssembly.Name = "m_pAssembly";
			this.m_pAssembly.Size = new System.Drawing.Size(328, 20);
			this.m_pAssembly.TabIndex = 0;
			this.m_pAssembly.UseStaticViewStyle = true;
			// 
			// m_pAssembly.ViewStyle
			// 
			// 
			// m_pClass
			// 
			this.m_pClass.DrawBorder = true;
			this.m_pClass.Location = new System.Drawing.Point(8, 96);
			this.m_pClass.Name = "m_pClass";
			this.m_pClass.Size = new System.Drawing.Size(328, 20);
			this.m_pClass.TabIndex = 1;
			this.m_pClass.UseStaticViewStyle = true;
			// 
			// m_pClass.ViewStyle
			// 
			// 
			// m_pCost
			// 
			this.m_pCost.BackColor = System.Drawing.Color.White;
			this.m_pCost.ButtonsAlign = LumiSoft.UI.Controls.LeftRight.Right;
			this.m_pCost.DecimalPlaces = 0;
			this.m_pCost.DecMaxValue = new System.Decimal(new int[] {
																		999999999,
																		0,
																		0,
																		0});
			this.m_pCost.DecMinValue = new System.Decimal(new int[] {
																		0,
																		0,
																		0,
																		0});
			this.m_pCost.DecValue = new System.Decimal(new int[] {
																	 0,
																	 0,
																	 0,
																	 0});
			this.m_pCost.DrawBorder = true;
			this.m_pCost.Location = new System.Drawing.Point(64, 120);
			this.m_pCost.Mask = LumiSoft.UI.Controls.WEditBox_Mask.Numeric;
			this.m_pCost.MaxLength = 32767;
			this.m_pCost.Name = "m_pCost";
			this.m_pCost.ReadOnly = false;
			this.m_pCost.Size = new System.Drawing.Size(64, 20);
			this.m_pCost.TabIndex = 2;
			this.m_pCost.Text = "0";
			this.m_pCost.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.m_pCost.UseStaticViewStyle = true;
			// 
			// m_pCost.ViewStyle
			// 
			// 
			// m_pGet
			// 
			this.m_pGet.DrawBorder = true;
			this.m_pGet.Location = new System.Drawing.Point(344, 56);
			this.m_pGet.Name = "m_pGet";
			this.m_pGet.Size = new System.Drawing.Size(32, 20);
			this.m_pGet.TabIndex = 3;
			this.m_pGet.Text = "...";
			this.m_pGet.UseStaticViewStyle = true;
			// 
			// m_pGet.ViewStyle
			// 
			this.m_pGet.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pGet_ButtonPressed);
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(8, 40);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(144, 16);
			this.wLabel1.TabIndex = 4;
			this.wLabel1.Text = "Assembly";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// wLabel2
			// 
			this.wLabel2.Location = new System.Drawing.Point(8, 80);
			this.wLabel2.Name = "wLabel2";
			this.wLabel2.Size = new System.Drawing.Size(144, 16);
			this.wLabel2.TabIndex = 5;
			this.wLabel2.Text = "Class";
			this.wLabel2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel2.ViewStyle
			// 
			// 
			// wLabel3
			// 
			this.wLabel3.Location = new System.Drawing.Point(8, 120);
			this.wLabel3.Name = "wLabel3";
			this.wLabel3.Size = new System.Drawing.Size(56, 24);
			this.wLabel3.TabIndex = 6;
			this.wLabel3.Text = "Cost";
			this.wLabel3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// wLabel3.ViewStyle
			// 
			// 
			// m_pCancel
			// 
			this.m_pCancel.DrawBorder = true;
			this.m_pCancel.Location = new System.Drawing.Point(304, 120);
			this.m_pCancel.Name = "m_pCancel";
			this.m_pCancel.Size = new System.Drawing.Size(72, 24);
			this.m_pCancel.TabIndex = 17;
			this.m_pCancel.Text = "Cancel";
			this.m_pCancel.UseStaticViewStyle = true;
			// 
			// m_pCancel.ViewStyle
			// 
			this.m_pCancel.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pCancel_ButtonPressed);
			// 
			// m_pOk
			// 
			this.m_pOk.DrawBorder = true;
			this.m_pOk.Location = new System.Drawing.Point(216, 120);
			this.m_pOk.Name = "m_pOk";
			this.m_pOk.Size = new System.Drawing.Size(72, 24);
			this.m_pOk.TabIndex = 18;
			this.m_pOk.Text = "OK";
			this.m_pOk.UseStaticViewStyle = true;
			// 
			// m_pOk.ViewStyle
			// 
			this.m_pOk.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pOk_ButtonPressed);
			// 
			// m_pEnabled
			// 
			this.m_pEnabled.Checked = true;
			this.m_pEnabled.DrawBorder = true;
			this.m_pEnabled.Location = new System.Drawing.Point(192, 124);
			this.m_pEnabled.Name = "m_pEnabled";
			this.m_pEnabled.ReadOnly = false;
			this.m_pEnabled.Size = new System.Drawing.Size(16, 16);
			this.m_pEnabled.TabIndex = 19;
			this.m_pEnabled.UseStaticViewStyle = true;
			// 
			// m_pEnabled.ViewStyle
			// 
			// 
			// mt_Enabled
			// 
			this.mt_Enabled.Location = new System.Drawing.Point(136, 120);
			this.mt_Enabled.Name = "mt_Enabled";
			this.mt_Enabled.Size = new System.Drawing.Size(56, 24);
			this.mt_Enabled.TabIndex = 20;
			this.mt_Enabled.Text = "Enabled";
			this.mt_Enabled.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// mt_Enabled.ViewStyle
			// 
			// 
			// wLabel4
			// 
			this.wLabel4.Location = new System.Drawing.Point(8, 0);
			this.wLabel4.Name = "wLabel4";
			this.wLabel4.Size = new System.Drawing.Size(144, 16);
			this.wLabel4.TabIndex = 22;
			this.wLabel4.Text = "Description";
			this.wLabel4.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// wLabel4.ViewStyle
			// 
			// 
			// m_pDescription
			// 
			this.m_pDescription.DrawBorder = true;
			this.m_pDescription.Location = new System.Drawing.Point(8, 16);
			this.m_pDescription.Name = "m_pDescription";
			this.m_pDescription.Size = new System.Drawing.Size(328, 20);
			this.m_pDescription.TabIndex = 21;
			this.m_pDescription.UseStaticViewStyle = true;
			// 
			// m_pDescription.ViewStyle
			// 
			// 
			// wfrm_Filter
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(386, 151);
			this.Controls.Add(this.wLabel4);
			this.Controls.Add(this.m_pDescription);
			this.Controls.Add(this.mt_Enabled);
			this.Controls.Add(this.m_pEnabled);
			this.Controls.Add(this.m_pCancel);
			this.Controls.Add(this.m_pOk);
			this.Controls.Add(this.wLabel3);
			this.Controls.Add(this.wLabel2);
			this.Controls.Add(this.wLabel1);
			this.Controls.Add(this.m_pGet);
			this.Controls.Add(this.m_pCost);
			this.Controls.Add(this.m_pClass);
			this.Controls.Add(this.m_pAssembly);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "wfrm_Filter";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Filter";
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		private void m_pGet_ButtonPressed(object sender, System.EventArgs e)
		{
			using(OpenFileDialog dlg = new OpenFileDialog()){
				dlg.InitialDirectory = Application.StartupPath + "\\Filters";
				dlg.Filter = "Assembly|*.dll|Executable|*.exe";

				if(dlg.ShowDialog() == DialogResult.OK){
					m_pAssembly.Text = dlg.FileName;
				}
			}
		}

		private void m_pOk_ButtonPressed(object sender, System.EventArgs e)
		{
			System.Reflection.Assembly ass = null;			
			try
			{
				ass = System.Reflection.Assembly.LoadFile(m_pAssembly.Text);
			}
			catch(Exception x){
				MessageBox.Show(this,"Invalid assembly location or isn't NET assembly !","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
				return;
			}

			Type tp = ass.GetType(m_pClass.Text);
			if(tp == null){
				MessageBox.Show(this,"Invalid class !","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
			}
			if(tp.GetInterface("ISmtpMessageFilter") == null){
				MessageBox.Show(this,"Invalid class, class doesn't conatin ISmtpMessageFilter !","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
				return;
			}

			this.DialogResult = DialogResult.OK;
		}

		private void m_pCancel_ButtonPressed(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets 
		/// </summary>
		public bool wp_Enabled
		{
			get{ return m_pEnabled.Checked; }
		}

		/// <summary>
		/// Gets 
		/// </summary>
		public string wp_Description
		{
			get{ return m_pDescription.Text; }
		}

		/// <summary>
		/// Gets 
		/// </summary>
		public string wp_Assembly
		{
			get{ return m_pAssembly.Text; }
		}

		/// <summary>
		/// Gets 
		/// </summary>
		public string wp_Class
		{
			get{ return m_pClass.Text; }
		}

		/// <summary>
		/// Gets 
		/// </summary>
		public int wp_Cost
		{
			get{ return (int)m_pCost.DecValue; }
		}

		#endregion

	}
}
