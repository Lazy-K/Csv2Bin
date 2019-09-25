namespace ExcelAddInCsv2Bin
{
	partial class RibbonCsv2Bin : Microsoft.Office.Tools.Ribbon.RibbonBase
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		public RibbonCsv2Bin()
			: base(Globals.Factory.GetRibbonFactory())
		{
			InitializeComponent();
		}

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region コンポーネント デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.tab1 = this.Factory.CreateRibbonTab();
			this.group1 = this.Factory.CreateRibbonGroup();
			this.settingButton = this.Factory.CreateRibbonButton();
			this.tab1.SuspendLayout();
			this.group1.SuspendLayout();
			this.SuspendLayout();
			//
			// tab1
			//
			this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
			this.tab1.Groups.Add(this.group1);
			this.tab1.Label = "TabAddIns";
			this.tab1.Name = "tab1";
			//
			// group1
			//
			this.group1.Items.Add(this.settingButton);
			this.group1.Label = "Csv2Bin";
			this.group1.Name = "group1";
			//
			// settingButton
			//
			this.settingButton.Label = "Setting";
			this.settingButton.Name = "settingButton";
			this.settingButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.settingButton_Click);
			//
			// RibbonCsv2Bin
			//
			this.Name = "RibbonCsv2Bin";
			this.RibbonType = "Microsoft.Excel.Workbook";
			this.Tabs.Add(this.tab1);
			this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.RibbonCsv2Bin_Load);
			this.tab1.ResumeLayout(false);
			this.tab1.PerformLayout();
			this.group1.ResumeLayout(false);
			this.group1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
		internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
		internal Microsoft.Office.Tools.Ribbon.RibbonButton settingButton;
	}

	partial class ThisRibbonCollection
	{
		internal RibbonCsv2Bin RibbonCsv2Bin
		{
			get { return this.GetRibbon<RibbonCsv2Bin>(); }
		}
	}
}
