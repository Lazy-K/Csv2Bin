using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Csv2Bin;

namespace ExcelAddInCsv2Bin
{
	public partial class FormCsv2BinSetting : Form
	{
		private ManifestHeader _manifestHeader;
		private List<ManifestContent> _manifestContents;

		public FormCsv2BinSetting()
		{
			InitializeComponent();
		}

		private bool LoadManifest(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return false;
			}

			var logFilePath = "_csv2bin_log.txt";
			if (File.Exists(logFilePath))
			{
				File.Delete(logFilePath);
			}

			var logFile = File.CreateText(logFilePath);
			Console.SetOut(logFile);

			var result = Manifest.Parse(filePath, ref _manifestHeader, ref _manifestContents);

			logFile.Dispose();
			var standardOutput = new StreamWriter(Console.OpenStandardOutput());
			standardOutput.AutoFlush = true;
			Console.SetOut(standardOutput);

			if (!result)
			{
				MessageBox.Show(File.ReadAllText(logFilePath), "Load Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return result;
		}

		private void FormCsv2BinSetting_Load(object sender, EventArgs e)
		{
			var activeSheet = (Microsoft.Office.Interop.Excel.Worksheet)Globals.ThisAddIn.Application.ActiveSheet;
			if (!LoadManifest(activeSheet.Name + "_csv2bin_manifest.xml"))
			{
				_manifestHeader.version = 1.0f;
				_manifestHeader.structName = activeSheet.Name;
			}

			//var dt = new DataTable();
			ref var dt = ref manifestDataGridView;

			{
				dt.Columns.Add("valueName", "valueName");

				{
					var bc = new BindingSource();
					{
						var valueTypeCount = (int)Csv2Bin.ValueType.Length;
						for (var i = 0; i < valueTypeCount; ++i)
						{
							bc.Add(((Csv2Bin.ValueType)i).ToString());
						}
					}
					var column = new DataGridViewComboBoxColumn();
					column.Name = column.HeaderText = "valueType";
					column.DataSource = bc;
					dt.Columns.Add(column);
				}

				dt.Columns.Add("length", "length");
				dt.Columns.Add("structFieldName", "structFieldName");
				dt.Columns.Add("structBitsName", "structBitsName");
			}

			//manifestDataGridView.DataSource = dt;

		}
	}
}
