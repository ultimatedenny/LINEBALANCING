namespace BaseFormLibrary
{
    partial class ShimanoImport
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OFD1 = new System.Windows.Forms.OpenFileDialog();
            this.shimanoButton1 = new BaseFormLibrary.ShimanoButton();
            this.shimanoTextBox1 = new BaseFormLibrary.ShimanoTextBox();
            this.SuspendLayout();
            // 
            // OFD1
            // 
            this.OFD1.FileName = "OFD1";
            this.OFD1.FileOk += new System.ComponentModel.CancelEventHandler(this.OFD1_FileOk);
            // 
            // shimanoButton1
            // 
            this.shimanoButton1.AutoSize = true;
            this.shimanoButton1.Dock = System.Windows.Forms.DockStyle.Right;
            this.shimanoButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.shimanoButton1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.shimanoButton1.Location = new System.Drawing.Point(228, 0);
            this.shimanoButton1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.shimanoButton1.Name = "shimanoButton1";
            this.shimanoButton1.Size = new System.Drawing.Size(80, 37);
            this.shimanoButton1.TabIndex = 1;
            this.shimanoButton1.Text = "Browse";
            this.shimanoButton1.UseVisualStyleBackColor = true;
            this.shimanoButton1.Click += new System.EventHandler(this.shimanoButton1_Click);
            // 
            // shimanoTextBox1
            // 
            this.shimanoTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.shimanoTextBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.shimanoTextBox1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.shimanoTextBox1.Location = new System.Drawing.Point(0, 0);
            this.shimanoTextBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.shimanoTextBox1.Name = "shimanoTextBox1";
            this.shimanoTextBox1.ReadOnly = true;
            this.shimanoTextBox1.ShimanoDataType = BaseFormLibrary.ShimanoTextBox.CollapseDirection.String;
            this.shimanoTextBox1.Size = new System.Drawing.Size(230, 29);
            this.shimanoTextBox1.TabIndex = 0;
            // 
            // ShimanoImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.shimanoButton1);
            this.Controls.Add(this.shimanoTextBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ShimanoImport";
            this.Size = new System.Drawing.Size(308, 37);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog OFD1;
        public ShimanoTextBox shimanoTextBox1;
        public ShimanoButton shimanoButton1;
    }
}
