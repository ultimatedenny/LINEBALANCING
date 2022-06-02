namespace BaseFormLibrary
{
    partial class ShimanoComboBox
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
            // ShimanoComboBox
            // 
            this.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.Size = new System.Drawing.Size(121, 24);
            this.Enter += new System.EventHandler(this.ShimanoComboBox_Enter);
            this.Leave += new System.EventHandler(this.ShimanoComboBox_Leave);
            this.ResumeLayout(false);

        }

        #endregion


    }
}
