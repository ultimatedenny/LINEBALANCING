namespace BaseFormLibrary
{
    partial class _ExportData
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
            this.shimanoLabel1 = new BaseFormLibrary.ShimanoLabel();
            this.cbExportFormat = new BaseFormLibrary.ShimanoComboBox();
            this.BtnExport = new BaseFormLibrary.ShimanoButton();
            this.dsExport = new System.Data.DataSet();
            this.SFD = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.dsExport)).BeginInit();
            this.SuspendLayout();
            // 
            // shimanoLabel1
            // 
            this.shimanoLabel1.AutoSize = true;
            this.shimanoLabel1.Location = new System.Drawing.Point(31, 20);
            this.shimanoLabel1.Name = "shimanoLabel1";
            this.shimanoLabel1.Size = new System.Drawing.Size(60, 17);
            this.shimanoLabel1.TabIndex = 0;
            this.shimanoLabel1.Text = "Format :";
            // 
            // cbExportFormat
            // 
            this.cbExportFormat.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbExportFormat.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbExportFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.cbExportFormat.FormattingEnabled = true;
            this.cbExportFormat.Items.AddRange(new object[] {
            "Excel Workbook | *.xlsx",
            "Excel 97-2003 Workbook | *.xls",
            "CSV (Comma delimited) | *.csv",
            "Unicode Text | *.txt",
            "PDF | *.pdf",
            "DataBase File | *.dbf",
            "Word Document | *.doc",
            "MS Access | *.mdb",
            "HTML | *.html"});
            this.cbExportFormat.Location = new System.Drawing.Point(34, 40);
            this.cbExportFormat.Name = "cbExportFormat";
            this.cbExportFormat.Size = new System.Drawing.Size(211, 24);
            this.cbExportFormat.TabIndex = 1;
            // 
            // BtnExport
            // 
            this.BtnExport.Location = new System.Drawing.Point(34, 83);
            this.BtnExport.Name = "BtnExport";
            this.BtnExport.Size = new System.Drawing.Size(211, 28);
            this.BtnExport.TabIndex = 2;
            this.BtnExport.Text = "Export Data";
            this.BtnExport.UseVisualStyleBackColor = true;
            this.BtnExport.Click += new System.EventHandler(this.BtnExport_Click);
            // 
            // dsExport
            // 
            this.dsExport.DataSetName = "NewDataSet";
            // 
            // _ExportData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(289, 136);
            this.Controls.Add(this.BtnExport);
            this.Controls.Add(this.cbExportFormat);
            this.Controls.Add(this.shimanoLabel1);
            this.Name = "_ExportData";
            this.ShowIcon = false;
            this.Text = "Shimano - Export";
            ((System.ComponentModel.ISupportInitialize)(this.dsExport)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ShimanoLabel shimanoLabel1;
        private ShimanoComboBox cbExportFormat;
        private ShimanoButton BtnExport;
        public System.Data.DataSet dsExport;
        private System.Windows.Forms.SaveFileDialog SFD;
    }
}