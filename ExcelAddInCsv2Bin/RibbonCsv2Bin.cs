using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Tools.Ribbon;

namespace ExcelAddInCsv2Bin
{
	public partial class RibbonCsv2Bin
	{
		private void RibbonCsv2Bin_Load(object sender, RibbonUIEventArgs e)
		{

		}

		private void settingButton_Click(object sender, RibbonControlEventArgs e)
		{
			var form = new FormCsv2BinSetting();
			form.ShowDialog();
		}
	}
}
