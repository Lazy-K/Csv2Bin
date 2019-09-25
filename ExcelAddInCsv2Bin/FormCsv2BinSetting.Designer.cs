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
			//
			// FormCsv2BinSetting
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.manifestDataGridView);
			this.Name = "FormCsv2BinSetting";
			this.Text = "FormCsv2BinSetting";
			this.Load += new System.EventHandler(this.FormCsv2BinSetting_Load);
			((System.ComponentModel.ISupportInitialize)(this.manifestDataGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView manifestDataGridView;
	}
}