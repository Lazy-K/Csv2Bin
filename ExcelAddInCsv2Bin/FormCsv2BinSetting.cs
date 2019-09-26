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
		//private List<ManifestContent> _manifestContents = new List<ManifestContent>();

		public FormCsv2BinSetting()
		{
			InitializeComponent();
		}

		private bool LoadManifestFile(string filePath, ref List<ManifestContent> contents)
		{
			var logFilePath = "_csv2bin_log.txt";
			try
			{
				if (!File.Exists(filePath))
				{
					return false;
				}

				if (File.Exists(logFilePath))
				{
					File.Delete(logFilePath);
				}

				var logFile = File.CreateText(logFilePath);
				Console.SetOut(logFile);

				var result = Manifest.Read(filePath, ref _manifestHeader, ref contents);

				logFile.Dispose();
				var standardOutput = new StreamWriter(Console.OpenStandardOutput());
				standardOutput.AutoFlush = true;
				Console.SetOut(standardOutput);

				if (!result)
				{
					MessageBox.Show(File.ReadAllText(logFilePath), "Load Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
					goto Failed;
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString(), "Load Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				goto Failed;
			}
			finally
			{
				try
				{
					if (File.Exists(logFilePath))
					{
						File.Delete(logFilePath);
					}
				}
				catch (Exception)
				{
				}
			}
			return true;
			Failed:
			return false;
		}

		private bool SaveManifestFile(string filePath, in List<ManifestContent> contents)
		{
			var logFilePath = "_csv2bin_log.txt";
			try
			{
				if (File.Exists(logFilePath))
				{
					File.Delete(logFilePath);
				}

				var logFile = File.CreateText(logFilePath);
				Console.SetOut(logFile);

				var result = Manifest.Write(filePath, _manifestHeader, contents);

				logFile.Dispose();
				var standardOutput = new StreamWriter(Console.OpenStandardOutput());
				standardOutput.AutoFlush = true;
				Console.SetOut(standardOutput);

				if (!result)
				{
					MessageBox.Show(File.ReadAllText(logFilePath), "Save Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
					goto Failed;
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString(), "Save Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				goto Failed;
			}
			finally
			{
				try
				{
					if (File.Exists(logFilePath))
					{
						File.Delete(logFilePath);
					}
				}
				catch (Exception)
				{
				}
			}
			return true;
			Failed:
			return false;
		}

		private string GetManifestFilePath()
		{
			var activeSheet = (Microsoft.Office.Interop.Excel.Worksheet)Globals.ThisAddIn.Application.ActiveSheet;
			return activeSheet.Name + "_csv2bin_manifest.xml";
		}

		private void SetupDefaultManifest(ref List<ManifestContent> contents)
		{
			var activeSheet = (Microsoft.Office.Interop.Excel.Worksheet)Globals.ThisAddIn.Application.ActiveSheet;
			_manifestHeader.version = 1.0f;
			_manifestHeader.structName = activeSheet.Name;

			contents.Clear();
		}

		private void manifestDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs anError)
		{
			//MessageBox.Show("Error happened " + anError.Context.ToString());

			if (anError.Context.HasFlag(DataGridViewDataErrorContexts.Commit))
			{
				//MessageBox.Show("Commit error");
			}
			if (anError.Context.HasFlag(DataGridViewDataErrorContexts.CurrentCellChange))
			{
				//MessageBox.Show("Cell change");
			}
			if (anError.Context.HasFlag(DataGridViewDataErrorContexts.Parsing))
			{
				//MessageBox.Show("parsing error");
			}
			if (anError.Context.HasFlag(DataGridViewDataErrorContexts.LeaveControl))
			{
				//MessageBox.Show("leave control error");
			}

			if ((anError.Exception) is ConstraintException)
			{
				//DataGridView view = (DataGridView)sender;
				//view.Rows[anError.RowIndex].ErrorText = "an error";
				//view.Rows[anError.RowIndex].Cells[anError.ColumnIndex].ErrorText = "an error";

				//anError.ThrowException = false;
			}

			var dgv = (DataGridView)sender;
			if (anError.Context.HasFlag(DataGridViewDataErrorContexts.Parsing))
			{
				//dgv.Rows[anError.RowIndex].ErrorText = "format error1";
				//dgv.Rows[anError.RowIndex].Cells[anError.ColumnIndex].ErrorText = "Error";
			}
		}

		private bool Int32ToValueTypeString(Int32 valueType, out string dest)
		{
			dest = string.Empty;
			if (0 > valueType || (int)Csv2Bin.ValueType.Length <= valueType) return false;
			dest = ((Csv2Bin.ValueType)valueType).ToString();
			return true;
		}

		private bool StringToValueType(string valueType, out Csv2Bin.ValueType dest)
		{
			dest = Csv2Bin.ValueType.s8;
			if (null == valueType) return false;
			var valueTypeCount = (int)Csv2Bin.ValueType.Length;
			for (var i = 0; i < valueTypeCount; ++i)
			{
				if (((Csv2Bin.ValueType)i).ToString() == valueType)
				{
					dest = (Csv2Bin.ValueType)i;
					return true;
				}
			}
			return false;
		}

		private bool IsValidInputManifestDataGridView()
		{
			var dgv = (DataGridView)manifestDataGridView;
			var rowsCount = dgv.Rows.Count;
			for (var i = 0; i < rowsCount; ++i)
			{
				if (string.Empty != dgv.Rows[i].ErrorText) return false;
			}
			return true;
		}

		private void RefreshManifestDataGridViewErrorText(int rowIndex)
		{
			var dgv = (DataGridView)manifestDataGridView;

			dgv.Rows[rowIndex].ErrorText = string.Empty;
			dgv.Rows[rowIndex].Cells["valueName"].ErrorText = string.Empty;
			dgv.Rows[rowIndex].Cells["valueType"].ErrorText = string.Empty;
			dgv.Rows[rowIndex].Cells["length"].ErrorText = string.Empty;
			dgv.Rows[rowIndex].Cells["structFieldName"].ErrorText = string.Empty;
			dgv.Rows[rowIndex].Cells["structBitsName"].ErrorText = string.Empty;

			if (dgv.AllowUserToAddRows && dgv.Rows.Count - 1 <= rowIndex)
			{
				return;
			}

			var isValidValueType = false;
			Csv2Bin.ValueType valueType = Csv2Bin.ValueType.s8;
			{
				var isInvalid = false;
				if (null == dgv.Rows[rowIndex].Cells["valueType"].Value)
				{
					isInvalid = true;
				}
				else
				{
					if (!StringToValueType((string)dgv.Rows[rowIndex].Cells["valueType"].Value, out valueType))
					{
						isInvalid = true;
					}
					else
					{
						isValidValueType = true;
					}
				}

				if (isInvalid)
				{
					dgv.Rows[rowIndex].Cells["valueType"].ErrorText = "Error";
					dgv.Rows[rowIndex].ErrorText = "Error";
				}
			}
			{
				var isInvalid = false;
				if (null == dgv.Rows[rowIndex].Cells["length"].Value)
				{
					if (Csv2Bin.ValueType.bits32 == valueType || Csv2Bin.ValueType.utf16 == valueType)
					{
						dgv.Rows[rowIndex].Cells["length"].ErrorText = "Error";
						dgv.Rows[rowIndex].ErrorText = "Error";
					}
				}
				else
				{
					var length = (int)dgv.Rows[rowIndex].Cells["length"].Value;
					if (isValidValueType && Csv2Bin.ValueType.bits32 == valueType)
					{
						if (0 > length/*0はビットフィールド強制スプリットで0は許可*/ || 32 < length)
						{
							isInvalid = true;
						}
					}
					else if (isValidValueType && Csv2Bin.ValueType.utf16 == valueType)
					{
						if (0 >= length)
						{
							isInvalid = true;
						}
					}
					else
					{
						if (0 != length)
						{
							isInvalid = true;
						}
					}

					if (isInvalid)
					{
						dgv.Rows[rowIndex].Cells["length"].ErrorText = "Error";
						dgv.Rows[rowIndex].ErrorText = "Error";
					}
				}
			}
			{
				if (isValidValueType && Csv2Bin.ValueType.bits32 != valueType)
				{
					if (null != dgv.Rows[rowIndex].Cells["structFieldName"].Value &&
						(string)dgv.Rows[rowIndex].Cells["structFieldName"].Value != string.Empty)
					{
						dgv.Rows[rowIndex].Cells["structFieldName"].ErrorText = "Error";
						dgv.Rows[rowIndex].ErrorText = "Error";
					}
					if (null != dgv.Rows[rowIndex].Cells["structBitsName"].Value &&
						(string)dgv.Rows[rowIndex].Cells["structBitsName"].Value != string.Empty)
					{
						dgv.Rows[rowIndex].Cells["structBitsName"].ErrorText = "Error";
						dgv.Rows[rowIndex].ErrorText = "Error";
					}
				}
			}
		}

		private void manifestDataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
		{
			RefreshManifestDataGridViewErrorText(e.RowIndex);
			saveManifestButton.Enabled = IsValidInputManifestDataGridView();
		}


		private void FormCsv2BinSetting_Load(object sender, EventArgs e)
		{
			var contents = new List<ManifestContent>();
			if (!LoadManifestFile(GetManifestFilePath(), ref contents))
			{
				SetupDefaultManifest(ref contents);
			}

			//var dt = new DataTable();
			ref var dgv = ref manifestDataGridView;
			{
				{
					var column = new DataGridViewColumn();
					column.CellTemplate = new DataGridViewTextBoxCell();
					column.Name = column.HeaderText = "valueName";
					column.ValueType = Type.GetType("System.String");
					dgv.Columns.Add(column);
				}
				{
					var column = new DataGridViewComboBoxColumn();
					column.Name = column.HeaderText = "valueType";
					column.ValueType = Type.GetType("System.String");
					{
						var bc = new BindingSource();
						var valueTypeCount = (int)Csv2Bin.ValueType.Length;
						for (var i = 0; i < valueTypeCount; ++i)
						{
							bc.Add(((Csv2Bin.ValueType)i).ToString());
						}
						column.DataSource = bc;
					}
					dgv.Columns.Add(column);
				}
				{
					var column = new DataGridViewColumn();
					column.CellTemplate = new DataGridViewTextBoxCell();
					column.Name = column.HeaderText = "length";
					column.ValueType = Type.GetType("System.Int32");
					dgv.Columns.Add(column);
				}
				{
					var column = new DataGridViewColumn();
					column.CellTemplate = new DataGridViewTextBoxCell();
					column.Name = column.HeaderText = "structFieldName";
					column.ValueType = Type.GetType("System.String");
					dgv.Columns.Add(column);
				}
				{
					var column = new DataGridViewColumn();
					column.CellTemplate = new DataGridViewTextBoxCell();
					column.Name = column.HeaderText = "structBitsName";
					column.ValueType = Type.GetType("System.String");
					dgv.Columns.Add(column);
				}
			}
			//manifestDataGridView.DataSource = dt;

			RefreshManifestView(contents);
		}

		private void RefreshManifestView(in List<ManifestContent> contents)
		{
			ref var dgv = ref manifestDataGridView;
			dgv.Rows.Clear();
			var contentCount = contents.Count;
			for (var i = 0; i < contentCount; ++i)
			{
				dgv.Rows.Add(
					contents[i].valueName,
					contents[i].valueType.ToString(),
					contents[i].length,
					contents[i].structFieldName,
					contents[i].structBitsName);
			}
		}

		private void GetManifestContentsFromView(ref List<ManifestContent> contents)
		{
			contents.Clear();
			ref var dgv = ref manifestDataGridView;
			var rowsCount = dgv.AllowUserToAddRows ? dgv.Rows.Count - 1 : dgv.Rows.Count;
			for (var i = 0; i < rowsCount; ++i)
			{
				var content = new ManifestContent();
				{
					var value = dgv.Rows[i].Cells["valueName"].Value;
					if (null != value) content.valueName = (System.String)value;
				}
				{
					var value = (System.String)dgv.Rows[i].Cells["valueType"].Value;
					if (!StringToValueType(value, out content.valueType))
					{
						content.valueType = Csv2Bin.ValueType.s8;
					}
				}
				{
					var value = dgv.Rows[i].Cells["length"].Value;
					if (null != value) content.length = (System.Int32)value;
				}
				{
					var value = dgv.Rows[i].Cells["structFieldName"].Value;
					if (null != value) content.structFieldName = (System.String)value;
				}
				{
					var value = dgv.Rows[i].Cells["structBitsName"].Value;
					if (null != value) content.structBitsName = (System.String)value;
				}
				contents.Add(content);
			}
		}

		private void saveManifestButton_Click(object sender, EventArgs e)
		{
			var contents = new List<ManifestContent>();
			GetManifestContentsFromView(ref contents);
			if (SaveManifestFile(GetManifestFilePath(), contents))
			{
				MessageBox.Show("Succeed", "Save Manifest File", MessageBoxButtons.OK);
			}
		}

		private void manifestLoadButton_Click(object sender, EventArgs e)
		{
			var contents = new List<ManifestContent>();
			if (!LoadManifestFile(GetManifestFilePath(), ref contents))
			{
				MessageBox.Show("Failed", "Load Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			MessageBox.Show("Succeed", "Load Manifest File", MessageBoxButtons.OK);
			RefreshManifestView(contents);
		}

		private void manifestDeleteButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (File.Exists(GetManifestFilePath()))
				{
					File.Delete(GetManifestFilePath());
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString(), "Delete Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				goto Failed;
			}
			MessageBox.Show("Succeed", "Delete Manifest File", MessageBoxButtons.OK);
			return;
			Failed:
			return;
		}

#if false

		private void buttonDelete_Click(object sender, EventArgs e)
		{
#if false
			var rows = dataGridView1.SelectedRows;
			var count = rows.Count;
			for (var i = 0; i < count; ++i)
			{
				dataGridView1.Rows.Remove(rows[i]);
			}
#elif true
			if (dataGridView1.CurrentRow.IsNewRow) return;
			dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
#endif
		}

		private void buttonUp_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow.IsNewRow) return;
			var index = dataGridView1.CurrentRow.Index;
			if (0 >= index) return;

			var columnCount = dataGridView1.ColumnCount;
			var o1 = new object[columnCount];
			var o2 = new object[columnCount];
			for (var i = 0; i < columnCount; ++i)
			{
				o1[i] = dataGridView1.Rows[index].Cells[i].Value;
				o2[i] = dataGridView1.Rows[index - 1].Cells[i].Value;
			}

			dataGridView1.Rows.RemoveAt(index);
			dataGridView1.Rows.RemoveAt(index - 1);

			dataGridView1.Rows.Insert(index - 1, o2);
			dataGridView1.Rows.Insert(index - 1, o1);

			dataGridView1.CurrentCell = dataGridView1.Rows[index - 1].Cells[0];
		}

		private void buttonDown_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow.IsNewRow) return;
			var rowCount = dataGridView1.Rows.Count;
			if (rowCount - 1 <= dataGridView1.CurrentRow.Index) return;
			var index = dataGridView1.CurrentRow.Index;
			dataGridView1.Rows.InsertCopies(index, index + 1, 1);
		}
#endif
	}
}
