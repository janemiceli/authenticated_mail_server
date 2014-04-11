using System;
using System.Windows.Forms;

namespace LumiSoft.UI.Controls
{
	public class DataGridNoActiveCellColumn : DataGridTextBoxColumn
	{		
		protected override void Edit(System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			if(this.DataGridTableStyle.DataGrid.CurrentRowIndex > -1){
				this.DataGridTableStyle.DataGrid.UnSelect(this.DataGridTableStyle.DataGrid.CurrentRowIndex);				
			}
				
			this.DataGridTableStyle.DataGrid.Select(rowNum);
		}
	}

}
