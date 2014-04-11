using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace LumiSoft.MailServer
{
	/// <summary>
	/// Error report form.
	/// </summary>
	public class wfrm_Error : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WPictureBox m_pImage;
		private LumiSoft.UI.Controls.WButton m_pClose;
		private LumiSoft.UI.Controls.WButton m_pExtend;
		private LumiSoft.UI.Controls.WEditBox m_pErrorText;
		private LumiSoft.UI.Controls.WEditBox m_pMessage;
		private System.Windows.Forms.ImageList imgList;
		private LumiSoft.UI.Controls.WLabel mt_error;
		private System.Windows.Forms.ImageList imageList1;
		private LumiSoft.UI.Controls.WEditBox wEditBox1;
		private LumiSoft.UI.Controls.WLabel wLabel1;
		private System.ComponentModel.IContainer components;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public wfrm_Error(Exception x,System.Diagnostics.StackTrace stack)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			this.Height = 210;
			m_pImage.Image = imageList1.Images[0];

			m_pMessage.Text = x.Message;
			wEditBox1.Text = stack.GetFrame(0).GetMethod().DeclaringType.FullName + "." + stack.GetFrame(0).GetMethod().Name + "()";

			m_pErrorText.Text = x.StackTrace;
		}

		#region fucntion Dispose

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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(wfrm_Error));
			this.m_pImage = new LumiSoft.UI.Controls.WPictureBox();
			this.m_pClose = new LumiSoft.UI.Controls.WButton();
			this.m_pExtend = new LumiSoft.UI.Controls.WButton();
			this.m_pErrorText = new LumiSoft.UI.Controls.WEditBox();
			this.m_pMessage = new LumiSoft.UI.Controls.WEditBox();
			this.imgList = new System.Windows.Forms.ImageList(this.components);
			this.mt_error = new LumiSoft.UI.Controls.WLabel();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.wEditBox1 = new LumiSoft.UI.Controls.WEditBox();
			this.wLabel1 = new LumiSoft.UI.Controls.WLabel();
			this.SuspendLayout();
			// 
			// m_pImage
			// 
			this.m_pImage.DrawBorder = true;
			this.m_pImage.Image = null;
			this.m_pImage.Location = new System.Drawing.Point(232, 16);
			this.m_pImage.Name = "m_pImage";
			this.m_pImage.Size = new System.Drawing.Size(200, 112);
			this.m_pImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
			this.m_pImage.TabIndex = 28;
			this.m_pImage.UseStaticViewStyle = true;
			// 
			// m_pImage.ViewStyle
			// 
			// 
			// m_pClose
			// 
			this.m_pClose.Location = new System.Drawing.Point(360, 144);
			this.m_pClose.Name = "m_pClose";
			this.m_pClose.Size = new System.Drawing.Size(72, 24);
			this.m_pClose.TabIndex = 15;
			this.m_pClose.Text = "Close";
			this.m_pClose.UseStaticViewStyle = true;
			// 
			// m_pClose.ViewStyle
			// 
			this.m_pClose.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pClose_ButtonPressed);
			// 
			// m_pExtend
			// 
			this.m_pExtend.Location = new System.Drawing.Point(280, 144);
			this.m_pExtend.Name = "m_pExtend";
			this.m_pExtend.Size = new System.Drawing.Size(72, 24);
			this.m_pExtend.TabIndex = 17;
			this.m_pExtend.Text = "More";
			this.m_pExtend.UseStaticViewStyle = true;
			// 
			// m_pExtend.ViewStyle
			// 
			this.m_pExtend.ButtonPressed += new LumiSoft.UI.Controls.ButtonPressedEventHandler(this.m_pExtend_ButtonPressed);
			// 
			// m_pErrorText
			// 
			this.m_pErrorText.Location = new System.Drawing.Point(8, 200);
			this.m_pErrorText.Multiline = true;
			this.m_pErrorText.Name = "m_pErrorText";
			this.m_pErrorText.ReadOnly = true;
			this.m_pErrorText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_pErrorText.Size = new System.Drawing.Size(432, 184);
			this.m_pErrorText.TabIndex = 24;
			this.m_pErrorText.UseStaticViewStyle = true;
			// 
			// m_pErrorText.ViewStyle
			// 
			// 
			// m_pMessage
			// 
			this.m_pMessage.Location = new System.Drawing.Point(8, 16);
			this.m_pMessage.Multiline = true;
			this.m_pMessage.Name = "m_pMessage";
			this.m_pMessage.ReadOnly = true;
			this.m_pMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_pMessage.Size = new System.Drawing.Size(208, 112);
			this.m_pMessage.TabIndex = 19;
			this.m_pMessage.UseStaticViewStyle = true;
			// 
			// m_pMessage.ViewStyle
			// 
			// 
			// imgList
			// 
			this.imgList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imgList.ImageSize = new System.Drawing.Size(200, 100);
			this.imgList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList.ImageStream")));
			this.imgList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// mt_error
			// 
			this.mt_error.Location = new System.Drawing.Point(8, 184);
			this.mt_error.Name = "mt_error";
			this.mt_error.Size = new System.Drawing.Size(150, 16);
			this.mt_error.TabIndex = 27;
			this.mt_error.Text = "Error report:";
			this.mt_error.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// mt_error.ViewStyle
			// 
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(200, 120);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// wEditBox1
			// 
			this.wEditBox1.Location = new System.Drawing.Point(8, 149);
			this.wEditBox1.Name = "wEditBox1";
			this.wEditBox1.Size = new System.Drawing.Size(264, 20);
			this.wEditBox1.TabIndex = 29;
			// 
			// wEditBox1.ViewStyle
			// 
			this.wEditBox1.ViewStyle.EditReadOnlyColor = System.Drawing.Color.White;
			// 
			// wLabel1
			// 
			this.wLabel1.Location = new System.Drawing.Point(8, 130);
			this.wLabel1.Name = "wLabel1";
			this.wLabel1.Size = new System.Drawing.Size(176, 16);
			this.wLabel1.TabIndex = 30;
			this.wLabel1.Text = "Function:";
			this.wLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.wLabel1.UseStaticViewStyle = false;
			// 
			// wLabel1.ViewStyle
			// 
			// 
			// wfrm_Error
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(448, 397);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.wLabel1,
																		  this.wEditBox1,
																		  this.m_pImage,
																		  this.m_pClose,
																		  this.m_pExtend,
																		  this.m_pErrorText,
																		  this.m_pMessage,
																		  this.mt_error});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "wfrm_Error";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Error Info";
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		#region function  m_pClose_ButtonPressed

		private void m_pClose_ButtonPressed(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#endregion

		#region function m_pExtend_ButtonPressed

		private void m_pExtend_ButtonPressed(object sender, System.EventArgs e)
		{
			this.Height = 422;
		}

		#endregion

		#endregion
		
	}
}
