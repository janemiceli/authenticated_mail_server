using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace LumiSoft.UI.Controls
{
	/// <summary>
	/// Summary description for WDataGrid.
	/// </summary>
	public class WDataGrid : System.Windows.Forms.DataGrid
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public WDataGrid()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitForm call

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion


		#region override ProcessDialogKey
		
		protected override bool ProcessDialogKey(Keys keyData)
		{
			bool retVal = false;
			int rowIndex = this.CurrentRowIndex;

			if(keyData == Keys.Up && rowIndex == 0){				
				retVal = true;
			}

			if(this.DataSource is DataView){
				DataView dv = (DataView)this.DataSource;
				if(keyData == Keys.Down && rowIndex > dv.Count-2){					
					retVal = true;
				}				
			}

			if(keyData == Keys.Escape){
				retVal = true;
			}

			if(keyData == Keys.Enter){
				retVal = true;
			}		
			
			return retVal;
		}

		#endregion	

		#region override OnDoubleClick

		protected override void OnDoubleClick(System.EventArgs e)
		{
			base.OnDoubleClick(e);

			System.Windows.Forms.DataGrid.HitTestInfo hTest = this.HitTest(this.PointToClient(Control.MousePosition));
			if(hTest.Row > -1){
				this.UnSelect(this.CurrentRowIndex);
				this.SelectRow(hTest.Row);
			}
			else{
				this.SelectRow(this.CurrentRowIndex);
			}			
		}

		#endregion

		#region override OnMouseDown

		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			base.OnMouseDown(e);

			System.Windows.Forms.DataGrid.HitTestInfo hTest = this.HitTest(this.PointToClient(Control.MousePosition));
			if(hTest.Row > -1){
				this.UnSelectRow(this.CurrentRowIndex);
				this.SelectRow(hTest.Row);
			}
			else{
				this.SelectRow(this.CurrentRowIndex);
			}
		}

		#endregion

		#region override OnMouseUp

		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			base.OnMouseUp(e);

			System.Windows.Forms.DataGrid.HitTestInfo hTest = this.HitTest(this.PointToClient(Control.MousePosition));
			if(hTest.Row == -1){
				this.SelectRow(this.CurrentRowIndex);
			}
		}

		#endregion


		#region function SelectRow

		private void SelectRow(int rowIndex)
		{
			if(rowIndex > -1){
				this.Select(rowIndex);
			}
		}

		#endregion

		#region function UnSelectRow

		private void UnSelectRow(int rowIndex)
		{
			if(rowIndex > -1){
				this.UnSelect(rowIndex);
			}
		}

		#endregion

	}
}
