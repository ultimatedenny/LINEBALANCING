namespace BaseFormLibrary
{
    partial class ShimanoButtonExport
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
            this.SuspendLayout();
            // 
            // ShimanoButtonExport
            // 
            this.Image = global::BaseFormLibrary.Properties.Resources.Export;
            this.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.Location = new System.Drawing.Point(14, 652);
            this.Size = new System.Drawing.Size(75, 45);
            this.Text = "Export";
            this.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.UseVisualStyleBackColor = true;
            this.Click += new System.EventHandler(this.ShimanoButtonExport_Click);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
