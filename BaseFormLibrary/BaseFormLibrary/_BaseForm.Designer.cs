using System.ComponentModel;
namespace BaseFormLibrary
{
    partial class _BaseForm
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
            this.SuspendLayout();
            // 
            // _BaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(736, 573);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "_BaseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "myBaseFrm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this._BaseForm_FormClosed);
            this.Load += new System.EventHandler(this._BaseForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        #region Properties

        [Category("Behavior"), DefaultValue(false)]
        [Description("Set to be true for Main Form")]
        public bool ShimanoMainForm { get; set; }

        #endregion

    }
}

