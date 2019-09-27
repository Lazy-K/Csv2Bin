using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Csv2Bin;
using Microsoft.Office.Tools.Ribbon;

namespace ExcelAddInCsv2Bin
{
	public partial class RibbonCsv2Bin
	{
		private void RibbonCsv2Bin_Load(object sender, RibbonUIEventArgs e)
		{

		}

		private void ButtonSetting_Click(object sender, RibbonControlEventArgs e)
		{
			var form = new FormCsv2BinSetting();
			form.ShowDialog();
		}

		private void ButtonBinExport_Click(object sender, RibbonControlEventArgs e)
		{
			var header = new ManifestHeader();
			var contents = new List<ManifestContent>();
			if (!FormCsv2BinSetting.LoadManifestFile(FormCsv2BinSetting.GetManifestFilePath(), ref header, ref contents))
			{
				MessageBox.Show("Manifest Load Failed", "Export Bin", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			FormCsv2BinSetting.ExportBin(header, contents);
		}
	}
}
