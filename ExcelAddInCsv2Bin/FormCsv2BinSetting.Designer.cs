namespace ExcelAddInCsv2Bin
{
	partial class FormCsv2BinSetting
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DataGridViewManifest = new System.Windows.Forms.DataGridView();
			this.ButtonManifestSave = new System.Windows.Forms.Button();
			this.ButtonManifestLoad = new System.Windows.Forms.Button();
			this.ButtonManifestDelete = new System.Windows.Forms.Button();
			this.ButtonRowDelete = new System.Windows.Forms.Button();
			this.ButtonRowMoveUp = new System.Windows.Forms.Button();
			this.ButtonRowMoveDown = new System.Windows.Forms.Button();
			this.ButtonBinExport = new System.Windows.Forms.Button();
			this.ButtonCsExport = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewManifest)).BeginInit();
			this.SuspendLayout();
			//
			// DataGridViewManifest
			//
			this.DataGridViewManifest.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DataGridViewManifest.Location = new System.Drawing.Point(41, 40);
			this.DataGridViewManifest.Name = "DataGridViewManifest";
			this.DataGridViewManifest.RowTemplate.Height = 21;
			this.DataGridViewManifest.Size = new System.Drawing.Size(565, 297);
			this.DataGridViewManifest.TabIndex = 0;
			this.DataGridViewManifest.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewManifest_CellValidated);
			this.DataGridViewManifest.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridViewManifest_DataError);
			//
			// ButtonManifestSave
			//
			this.ButtonManifestSave.Location = new System.Drawing.Point(128, 393);
			this.ButtonManifestSave.Name = "ButtonManifestSave";
			this.ButtonManifestSave.Size = new System.Drawing.Size(94, 23);
			this.ButtonManifestSave.TabIndex = 1;
			this.ButtonManifestSave.Text = "SaveManifest";
			this.ButtonManifestSave.UseVisualStyleBackColor = true;
			this.ButtonManifestSave.Click += new System.EventHandler(this.ButtonSaveManifest_Click);
			//
			// ButtonManifestLoad
			//
			this.ButtonManifestLoad.Location = new System.Drawing.Point(295, 393);
			this.ButtonManifestLoad.Name = "ButtonManifestLoad";
			this.ButtonManifestLoad.Size = new System.Drawing.Size(99, 23);
			this.ButtonManifestLoad.TabIndex = 2;
			this.ButtonManifestLoad.Text = "LoadManifest";
			this.ButtonManifestLoad.UseVisualStyleBackColor = true;
			this.ButtonManifestLoad.Click += new System.EventHandler(this.ButtonManifestLoad_Click);
			//
			// ButtonManifestDelete
			//
			this.ButtonManifestDelete.Location = new System.Drawing.Point(443, 393);
			this.ButtonManifestDelete.Name = "ButtonManifestDelete";
			this.ButtonManifestDelete.Size = new System.Drawing.Size(98, 23);
			this.ButtonManifestDelete.TabIndex = 3;
			this.ButtonManifestDelete.Text = "DeleteManifest";
			this.ButtonManifestDelete.UseVisualStyleBackColor = true;
			this.ButtonManifestDelete.Click += new System.EventHandler(this.ButtonManifestDelete_Click);
			//
			// ButtonRowDelete
			//
			this.ButtonRowDelete.Location = new System.Drawing.Point(663, 89);
			this.ButtonRowDelete.Name = "ButtonRowDelete";
			this.ButtonRowDelete.Size = new System.Drawing.Size(95, 23);
			this.ButtonRowDelete.TabIndex = 4;
			this.ButtonRowDelete.Text = "RowDelete";
			this.ButtonRowDelete.UseVisualStyleBackColor = true;
			this.ButtonRowDelete.Click += new System.EventHandler(this.ButtonRowDelete_Click);
			//
			// ButtonRowMoveUp
			//
			this.ButtonRowMoveUp.Location = new System.Drawing.Point(663, 157);
			this.ButtonRowMoveUp.Name = "ButtonRowMoveUp";
			this.ButtonRowMoveUp.Size = new System.Drawing.Size(95, 23);
			this.ButtonRowMoveUp.TabIndex = 5;
			this.ButtonRowMoveUp.Text = "RowMoveUp";
			this.ButtonRowMoveUp.UseVisualStyleBackColor = true;
			this.ButtonRowMoveUp.Click += new System.EventHandler(this.ButtonRowMoveUp_Click);
			//
			// ButtonRowMoveDown
			//
			this.ButtonRowMoveDown.Location = new System.Drawing.Point(663, 223);
			this.ButtonRowMoveDown.Name = "ButtonRowMoveDown";
			this.ButtonRowMoveDown.Size = new System.Drawing.Size(95, 23);
			this.ButtonRowMoveDown.TabIndex = 6;
			this.ButtonRowMoveDown.Text = "RowMoveDown";
			this.ButtonRowMoveDown.UseVisualStyleBackColor = true;
			this.ButtonRowMoveDown.Click += new System.EventHandler(this.ButtonRowMoveDown_Click);
			//
			// ButtonBinExport
			//
			this.ButtonBinExport.Location = new System.Drawing.Point(679, 332);
			this.ButtonBinExport.Name = "ButtonBinExport";
			this.ButtonBinExport.Size = new System.Drawing.Size(75, 23);
			this.ButtonBinExport.TabIndex = 7;
			this.ButtonBinExport.Text = "ExportBin";
			this.ButtonBinExport.UseVisualStyleBackColor = true;
			this.ButtonBinExport.Click += new System.EventHandler(this.ButtonBinExport_Click);
			//
			// ButtonCsExport
			//
			this.ButtonCsExport.Location = new System.Drawing.Point(679, 379);
			this.ButtonCsExport.Name = "ButtonCsExport";
			this.ButtonCsExport.Size = new System.Drawing.Size(75, 23);
			this.ButtonCsExport.TabIndex = 8;
			this.ButtonCsExport.Text = "ExportCs";
			this.ButtonCsExport.UseVisualStyleBackColor = true;
			this.ButtonCsExport.Click += new System.EventHandler(this.ButtonCsExport_Click);
			//
			// FormCsv2BinSetting
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.ButtonCsExport);
			this.Controls.Add(this.ButtonBinExport);
			this.Controls.Add(this.ButtonRowMoveDown);
			this.Controls.Add(this.ButtonRowMoveUp);
			this.Controls.Add(this.ButtonRowDelete);
			this.Controls.Add(this.ButtonManifestDelete);
			this.Controls.Add(this.ButtonManifestLoad);
			this.Controls.Add(this.ButtonManifestSave);
			this.Controls.Add(this.DataGridViewManifest);
			this.Name = "FormCsv2BinSetting";
			this.Text = "FormCsv2BinSetting";
			this.Load += new System.EventHandler(this.FormCsv2BinSetting_Load);
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewManifest)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView DataGridViewManifest;
		private System.Windows.Forms.Button ButtonManifestSave;
		private System.Windows.Forms.Button ButtonManifestLoad;
		private System.Windows.Forms.Button ButtonManifestDelete;
		private System.Windows.Forms.Button ButtonRowDelete;
		private System.Windows.Forms.Button ButtonRowMoveUp;
		private System.Windows.Forms.Button ButtonRowMoveDown;
		private System.Windows.Forms.Button ButtonBinExport;
		private System.Windows.Forms.Button ButtonCsExport;
	}
}