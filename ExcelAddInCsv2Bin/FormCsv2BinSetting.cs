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
		private ManifestHeader _defaultHeader = new ManifestHeader();
		private List<ManifestContent> _defaultContents = new List<ManifestContent>();

		public FormCsv2BinSetting()
		{
			InitializeComponent();
		}

		private static string GetBasePath()
		{
#if DEBUG
			return "./";
#else
			var activeWorkbook = Globals.ThisAddIn.Application.ActiveWorkbook;
			return activeWorkbook.Path;
#endif
		}

		public static string GetManifestFilePath()
		{
			var activeSheet = (Microsoft.Office.Interop.Excel.Worksheet)Globals.ThisAddIn.Application.ActiveSheet;
			return GetBasePath() + "/" + activeSheet.Name + "_csv2bin_manifest.xml";
		}

		private static string GetCsFilePath(string structName)
		{
			return GetBasePath() + "/" + structName + ".cs";
		}

		private static string GetBinFilePath()
		{
			var activeSheet = (Microsoft.Office.Interop.Excel.Worksheet)Globals.ThisAddIn.Application.ActiveSheet;
			return GetBasePath() + "/" + activeSheet.Name + ".bin";
		}

		private static string GetLogFilePath()
		{
			return GetBasePath() + "/" + "_csv2bin_log.txt";
		}

		public static bool LoadManifestFile(string filePath, ref ManifestHeader header, ref List<ManifestContent> contents)
		{
			var logFilePath = GetLogFilePath();
			var standardOutput = Console.Out;
			var result = true;
			try
			{
				if (!File.Exists(filePath))
				{
					result = false;
					goto Finally;
				}

				if (File.Exists(logFilePath))
				{
					File.Delete(logFilePath);
				}

				using (var logFile = File.CreateText(logFilePath))
				{
					Console.SetOut(logFile);
					result = Manifest.Read(filePath, ref header, ref contents);
				}

				if (!result)
				{
					MessageBox.Show(File.ReadAllText(logFilePath), "Load Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
					goto Finally;
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString(), "Load Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				result = false;
				goto Finally;
			}

			Finally:
			Console.SetOut(standardOutput);

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

			return result;
		}

		private bool SaveManifestFile(string filePath, in ManifestHeader header, in List<ManifestContent> contents)
		{
			var logFilePath = GetLogFilePath();
			var standardOutput = Console.Out;
			var result = true;
			try
			{
				if (File.Exists(logFilePath))
				{
					File.Delete(logFilePath);
				}

				using (var logFile = File.CreateText(logFilePath))
				{
					Console.SetOut(logFile);
					result = Manifest.Write(filePath, header, contents);
				}

				if (!result)
				{
					MessageBox.Show(File.ReadAllText(logFilePath), "Save Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
					goto Finally;
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString(), "Save Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				result = false;
				goto Finally;
			}

			Finally:
			Console.SetOut(standardOutput);

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

			return result;
		}

		private void SetupDefaultManifest(ref ManifestHeader header, ref List<ManifestContent> contents)
		{
			var activeSheet = (Microsoft.Office.Interop.Excel.Worksheet)Globals.ThisAddIn.Application.ActiveSheet;
			header.version = 1.0f;
			{
				var s = activeSheet.Name;
				s = s.Replace(" ", "");
				s = s.Replace("\t", "");
				header.structName = s;
			}
			contents.Clear();
		}

		private void DataGridViewManifest_DataError(object sender, DataGridViewDataErrorEventArgs anError)
		{
#if false
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
#endif
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
			var dgv = (DataGridView)DataGridViewManifest;
			var rowCount = dgv.RowCount;
			for (var i = 0; i < rowCount; ++i)
			{
				if (string.Empty != dgv.Rows[i].ErrorText) return false;
			}
			return true;
		}

		private void RefreshManifestDataGridViewErrorText(int rowIndex)
		{
			var dgv = (DataGridView)DataGridViewManifest;

			dgv.Rows[rowIndex].ErrorText = string.Empty;
			dgv.Rows[rowIndex].Cells["valueName"].ErrorText = string.Empty;
			dgv.Rows[rowIndex].Cells["valueType"].ErrorText = string.Empty;
			dgv.Rows[rowIndex].Cells["length"].ErrorText = string.Empty;
			dgv.Rows[rowIndex].Cells["structFieldName"].ErrorText = string.Empty;
			dgv.Rows[rowIndex].Cells["structBitsName"].ErrorText = string.Empty;

			if (dgv.Rows.Count - 1 <= rowIndex)
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
				}
			}
			{
				var isInvalid = false;
				if (null == dgv.Rows[rowIndex].Cells["length"].Value)
				{
					if (Csv2Bin.ValueType.bits32 == valueType || Csv2Bin.ValueType.utf16 == valueType)
					{
						dgv.Rows[rowIndex].Cells["length"].ErrorText = "Error";
					}
				}
				else
				{
					var length = (int)dgv.Rows[rowIndex].Cells["length"].Value;
					if (isValidValueType && Csv2Bin.ValueType.bits32 == valueType)
					{
						if (0 > length/*0はビットフィールド強制スプリットで0は許可*/ || 15/*BitVector32のSection引数制限*/ < length)
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
					}
				}
			}
			{
				if (isValidValueType && Csv2Bin.ValueType.bits32 != valueType)
				{
					if (null != dgv.Rows[rowIndex].Cells["structBitsName"].Value &&
						(string)dgv.Rows[rowIndex].Cells["structBitsName"].Value != string.Empty)
					{
						dgv.Rows[rowIndex].Cells["structBitsName"].ErrorText = "Error";
					}
				}
			}

			{
				var columnCount = dgv.ColumnCount;
				for (var i = 0; i < columnCount; ++i)
				{
					if (string.Empty != dgv.Rows[rowIndex].Cells[i].ErrorText)
					{
						dgv.Rows[rowIndex].ErrorText = Resource.SR_DATA_GRID_VIEW_MANIFEST_ERROR_TEXT_ROW;
						break;
					}
				}
			}
		}

		private void OnManifestValidateStateChanged()
		{
			ButtonManifestSave.Enabled =
			ButtonBinExport.Enabled =
			ButtonCsExport.Enabled = IsValidInputManifestDataGridView();
		}

		private void DataGridViewManifest_CellValidated(object sender, DataGridViewCellEventArgs e)
		{
			RefreshManifestDataGridViewErrorText(e.RowIndex);
			OnManifestValidateStateChanged();
		}

		private void DataGridViewManifest_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			((DataGridView)sender).BeginEdit(true); // 1回のクリックでエディットモードにする
		}

		private void FormCsv2BinSetting_Load(object sender, EventArgs e)
		{
			ref var dgv = ref DataGridViewManifest;
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

			if (!LoadManifestFile(GetManifestFilePath(), ref _defaultHeader, ref _defaultContents))
			{
				SetupDefaultManifest(ref _defaultHeader, ref _defaultContents);
			}
			RefreshManifestView(_defaultHeader, _defaultContents);
		}

		private void RefreshManifestView(in ManifestHeader header, in List<ManifestContent> contents)
		{
			TextBoxStructName.Text = header.structName;

			ref var dgv = ref DataGridViewManifest;
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

		private void GetManifestContentsFromView(ref ManifestHeader header, ref List<ManifestContent> contents)
		{
			header.version = 1.0f;
			header.structName = TextBoxStructName.Text;

			contents.Clear();
			ref var dgv = ref DataGridViewManifest;
			var rowCount = dgv.RowCount - 1;
			for (var i = 0; i < rowCount; ++i)
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

		private void ButtonSaveManifest_Click(object sender, EventArgs e)
		{
			var header = new ManifestHeader();
			var contents = new List<ManifestContent>();
			GetManifestContentsFromView(ref header, ref contents);

			var filePath = GetManifestFilePath();
			if (!SaveManifestFile(filePath, header, contents))
			{
				MessageBox.Show(string.Format("\"{0}\"\nFailed", Path.GetFullPath(filePath)), "Save Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			{
				_defaultHeader = header;
				_defaultContents = contents;
			}
			MessageBox.Show(string.Format("\"{0}\"\nSucceed", Path.GetFullPath(filePath)), "Save Manifest File", MessageBoxButtons.OK);
		}

		private void ButtonManifestLoad_Click(object sender, EventArgs e)
		{
			var filePath = GetManifestFilePath();
			var header = new ManifestHeader();
			var contents = new List<ManifestContent>();
			if (!LoadManifestFile(filePath, ref header, ref contents))
			{
				MessageBox.Show(string.Format("\"{0}\"\nFailed", Path.GetFullPath(filePath)), "Load Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			{
				_defaultHeader = header;
				_defaultContents = contents;
			}
			RefreshManifestView(header, contents);
			MessageBox.Show(string.Format("\"{0}\"\nSucceed", Path.GetFullPath(filePath)), "Load Manifest File", MessageBoxButtons.OK);
		}

		private void ButtonManifestDelete_Click(object sender, EventArgs e)
		{
			var dialogResult = MessageBox.Show("Delete Manifest?", "Delete Manifest", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (DialogResult.No == dialogResult)
			{
				return;
			}

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
				return;
			}

			SetupDefaultManifest(ref _defaultHeader, ref _defaultContents);
			RefreshManifestView(_defaultHeader, _defaultContents);
			MessageBox.Show("Succeed", "Delete Manifest File", MessageBoxButtons.OK);
		}

		private void ButtonRowDelete_Click(object sender, EventArgs e)
		{
			ref var dgv = ref DataGridViewManifest;
			if (dgv.CurrentRow.IsNewRow) return;
			dgv.Rows.Remove(dgv.CurrentRow);

			OnManifestValidateStateChanged();
		}

		private void ButtonRowMoveUp_Click(object sender, EventArgs e)
		{
			ref var dgv = ref DataGridViewManifest;
			if (dgv.CurrentRow.IsNewRow) return;

			var index = dgv.CurrentRow.Index;
			if (0 >= index) return;

			var columnCount = dgv.ColumnCount;
			var o1 = new object[columnCount];
			var o2 = new object[columnCount];
			for (var i = 0; i < columnCount; ++i)
			{
				o1[i] = dgv.Rows[index].Cells[i].Value;
				o2[i] = dgv.Rows[index - 1].Cells[i].Value;
			}

			dgv.Rows.RemoveAt(index);
			dgv.Rows.RemoveAt(index - 1);

			dgv.Rows.Insert(index - 1, o2);
			dgv.Rows.Insert(index - 1, o1);

			dgv.CurrentCell = dgv.Rows[index - 1].Cells[0];
		}

		private void ButtonRowMoveDown_Click(object sender, EventArgs e)
		{
			ref var dgv = ref DataGridViewManifest;
			if (dgv.CurrentRow.IsNewRow) return;

			var rowCount = dgv.Rows.Count;
			var index = dgv.CurrentRow.Index;
			if (rowCount - 2 <= index) return;


			var columnCount = dgv.ColumnCount;
			var o1 = new object[columnCount];
			var o2 = new object[columnCount];
			for (var i = 0; i < columnCount; ++i)
			{
				o1[i] = dgv.Rows[index].Cells[i].Value;
				o2[i] = dgv.Rows[index + 1].Cells[i].Value;
			}

			dgv.Rows.RemoveAt(index);
			dgv.Rows.RemoveAt(index);

			dgv.Rows.Insert(index, o1);
			dgv.Rows.Insert(index, o2);

			dgv.CurrentCell = dgv.Rows[index + 1].Cells[0];
		}

		private void ButtonCsExport_Click(object sender, EventArgs e)
		{
			try
			{
				var header = new ManifestHeader();
				var contents = new List<ManifestContent>();
				GetManifestContentsFromView(ref header, ref contents);
				var code = Csv2Bin.Manifest.GenerateCode(header, contents);

				var filePath = GetCsFilePath(header.structName);
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
				File.WriteAllText(filePath, code);
				MessageBox.Show(string.Format("\"{0}\"\nSucceed", Path.GetFullPath(filePath)), "Export Cs", MessageBoxButtons.OK);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString(), "Export Cs", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public static void ExportBin(in ManifestHeader header, in List<ManifestContent> contents)
		{
			var logFilePath = GetLogFilePath();
			var standardOutput = Console.Out;
			try
			{
				UInt32 numRecords;
				var binary = new List<byte>();

				var result = false;

				if (File.Exists(logFilePath))
				{
					File.Delete(logFilePath);
				}

				var activeSheet = (Microsoft.Office.Interop.Excel.Worksheet)Globals.ThisAddIn.Application.ActiveSheet;
				var csvFilePath = activeSheet.Name + ".csv";
				{ // Save Csv File
				  /**
				   * activeSheet.SaveAs()でファイルを保存すると
				   * 実行中のエクセルが保存先のファイルに切り替わるので、
				   * 一時ワークブックを作成してactiveSheetの内容をコピーして、
				   * 一時ワークブックを使ってSaveAs()を処理させる
				   *
				   * workSheet.Copy(Type.Missing, activeSheet)は同一ブック内の
				   * シートコピーしか機能しないためCell毎にコピーする
				   */
					var rowCount = 0; // activeSheet.Cells.Rows.Countだと空行までカウントしているため独自カウント
					while (null != activeSheet.Cells[rowCount + 1, 1].Value)
					{
						++rowCount;
					}
					var columnCount = 0;// activeSheet.Cells.Column.Countだと空列までカウントしているため独自カウント
					while (null != activeSheet.Cells[1, columnCount + 1].Value)
					{
						++columnCount;
					}

					{
						var tempApp = new Microsoft.Office.Interop.Excel.Application();
						tempApp.Visible = false;
						var tempWb = tempApp.Workbooks.Add();
						var tempWs = tempWb.Sheets[1];
						//tempWs.Select(Type.Missing);
						for (var i = 0; i < rowCount; ++i)
						{
							for (var j = 0; j < columnCount; ++j)
							{
								tempWs.Cells[i + 1, j + 1].Value = activeSheet.Cells[i + 1, j + 1].Value;
							}
						}

						if (File.Exists(csvFilePath))
						{
							File.Delete(csvFilePath);
						}
						tempWb.SaveAs(csvFilePath, 62/*xlCSVUTF8*/);
						tempWb.Close(false);
					}
				}

				using (var logFile = File.CreateText(logFilePath))
				{
					Console.SetOut(logFile);
					result = Csv2Bin.Manifest.GenerateBinary(csvFilePath, contents, out binary, out numRecords);
				}
				if (!result)
				{
					MessageBox.Show(File.ReadAllText(logFilePath), "Export Bin", MessageBoxButtons.OK, MessageBoxIcon.Error);
					goto Finally;
				}

				var filePath = GetBinFilePath();
				using (var writer = new BinaryWriter(new FileStream(filePath, FileMode.Create)))
				{
					writer.Write(binary.ToArray());
					if (true)
					{ // Append summary
						UInt32 size = (UInt32)binary.Count / numRecords;
						writer.Write(size);
						writer.Write(numRecords);
					}
				}
				MessageBox.Show(string.Format("\"{0}\"\nSucceed", Path.GetFullPath(filePath)), "Export Bin", MessageBoxButtons.OK);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString(), "Export Bin", MessageBoxButtons.OK, MessageBoxIcon.Error);
				goto Finally;
			}

			Finally:
			Console.SetOut(standardOutput);

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

		private void ButtonBinExport_Click(object sender, EventArgs e)
		{
			var header = new ManifestHeader();
			var contents = new List<ManifestContent>();
			GetManifestContentsFromView(ref header, ref contents);
			ExportBin(header, contents);
		}

		private void TextBoxStructName_Validating(object sender, CancelEventArgs e)
		{
			var tb = (TextBox)sender;
			if (string.Empty == tb.Text || tb.Text.Contains(" ") || tb.Text.Contains("\t"))
			{
				MessageBox.Show("Invalid format", "StructName", MessageBoxButtons.OK, MessageBoxIcon.Error);
				e.Cancel = true;
			}
		}

		private bool IsEqualManifest(
			in ManifestHeader header1,
			in List<ManifestContent> contents1,
			in ManifestHeader header2,
			in List<ManifestContent> contents2)
		{
			if (header1.version != header2.version) return false;
			if (header1.structName != header2.structName) return false;

			if (contents1.Count != contents2.Count) return false;
			var contentCount = contents1.Count;
			for (var i = 0; i < contentCount; ++i)
			{
				if (contents1[i].valueName != contents2[i].valueName) return false;
				if (contents1[i].valueType != contents2[i].valueType) return false;
				if (contents1[i].length != contents2[i].length) return false;
				if (contents1[i].structFieldName != contents2[i].structFieldName) return false;
				if (contents1[i].structBitsName != contents2[i].structBitsName) return false;
			}
			return true;
		}

		private void FormCsv2BinSetting_FormClosing(object sender, FormClosingEventArgs e)
		{
			var header = new ManifestHeader();
			var contents = new List<ManifestContent>();
			GetManifestContentsFromView(ref header, ref contents);

			if (IsEqualManifest(header, contents, _defaultHeader, _defaultContents))
			{
				return;
			}

			var dialogResult = MessageBox.Show("Save Manifest?", "Save Manifest", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (DialogResult.No == dialogResult)
			{
				return;
			}

			var filePath = GetManifestFilePath();
			if (!SaveManifestFile(filePath, header, contents))
			{
				MessageBox.Show(string.Format("\"{0}\"\nFailed", Path.GetFullPath(filePath)), "Save Manifest File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}
	}
}
