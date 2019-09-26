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
			this.manifestDataGridView = new System.Windows.Forms.DataGridView();
			this.saveManifestButton = new System.Windows.Forms.Button();
			this.manifestLoadButton = new System.Windows.Forms.Button();
			this.manifestDeleteButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.manifestDataGridView)).BeginInit();
			this.SuspendLayout();
			//
			// manifestDataGridView
			//
			this.manifestDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.manifestDataGridView.Location = new System.Drawing.Point(41, 40);
			this.manifestDataGridView.Name = "manifestDataGridView";
			this.manifestDataGridView.RowTemplate.Height = 21;
			this.manifestDataGridView.Size = new System.Drawing.Size(565, 297);
			this.manifestDataGridView.TabIndex = 0;
			this.manifestDataGridView.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.manifestDataGridView_CellValidated);
			this.manifestDataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.manifestDataGridView_DataError);
			//
			// saveManifestButton
			//
			this.saveManifestButton.Location = new System.Drawing.Point(147, 393);
			this.saveManifestButton.Name = "saveManifestButton";
			this.saveManifestButton.Size = new System.Drawing.Size(75, 23);
			this.saveManifestButton.TabIndex = 1;
			this.saveManifestButton.Text = "Save";
			this.saveManifestButton.UseVisualStyleBackColor = true;
			this.saveManifestButton.Click += new System.EventHandler(this.saveManifestButton_Click);
			//
			// manifestLoadButton
			//
			this.manifestLoadButton.Location = new System.Drawing.Point(315, 393);
			this.manifestLoadButton.Name = "manifestLoadButton";
			this.manifestLoadButton.Size = new System.Drawing.Size(75, 23);
			this.manifestLoadButton.TabIndex = 2;
			this.manifestLoadButton.Text = "Load";
			this.manifestLoadButton.UseVisualStyleBackColor = true;
			this.manifestLoadButton.Click += new System.EventHandler(this.manifestLoadButton_Click);
			//
			// manifestDeleteButton
			//
			this.manifestDeleteButton.Location = new System.Drawing.Point(457, 393);
			this.manifestDeleteButton.Name = "manifestDeleteButton";
			this.manifestDeleteButton.Size = new System.Drawing.Size(75, 23);
			this.manifestDeleteButton.TabIndex = 3;
			this.manifestDeleteButton.Text = "Delete";
			this.manifestDeleteButton.UseVisualStyleBackColor = true;
			this.manifestDeleteButton.Click += new System.EventHandler(this.manifestDeleteButton_Click);
			//
			// FormCsv2BinSetting
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.manifestDeleteButton);
			this.Controls.Add(this.manifestLoadButton);
			this.Controls.Add(this.saveManifestButton);
			this.Controls.Add(this.manifestDataGridView);
			this.Name = "FormCsv2BinSetting";
			this.Text = "FormCsv2BinSetting";
			this.Load += new System.EventHandler(this.FormCsv2BinSetting_Load);
			((System.ComponentModel.ISupportInitialize)(this.manifestDataGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView manifestDataGridView;
		private System.Windows.Forms.Button saveManifestButton;
		private System.Windows.Forms.Button manifestLoadButton;
		private System.Windows.Forms.Button manifestDeleteButton;
	}
}