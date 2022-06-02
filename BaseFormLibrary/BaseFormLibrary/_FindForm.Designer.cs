namespace BaseFormLibrary
{
    partial class _FindForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shimanoTabControl1 = new BaseFormLibrary.ShimanoTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtFind = new BaseFormLibrary.ShimanoTextBox();
            this.shimanoLabel1 = new BaseFormLibrary.ShimanoLabel();
            this.shimanoButton1 = new BaseFormLibrary.ShimanoButton();
            this.btnFind = new BaseFormLibrary.ShimanoButton();
            this.BtnClose = new BaseFormLibrary.ShimanoButton();
            this.grdFindAll = new System.Windows.Forms.DataGridView();
            this.lblTtlRow = new BaseFormLibrary.ShimanoLabel();
            this.RowNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shimanoTabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdFindAll)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "RowNumber";
            this.dataGridViewTextBoxColumn1.HeaderText = "Row Number";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Width = 232;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Value";
            this.dataGridViewTextBoxColumn2.HeaderText = "Value";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Width = 231;
            // 
            // shimanoTabControl1
            // 
            this.shimanoTabControl1.Controls.Add(this.tabPage1);
            this.shimanoTabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.shimanoTabControl1.ItemSize = new System.Drawing.Size(428, 0);
            this.shimanoTabControl1.Location = new System.Drawing.Point(0, 0);
            this.shimanoTabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.shimanoTabControl1.Name = "shimanoTabControl1";
            this.shimanoTabControl1.SelectedIndex = 0;
            this.shimanoTabControl1.Size = new System.Drawing.Size(430, 94);
            this.shimanoTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.shimanoTabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtFind);
            this.tabPage1.Controls.Add(this.shimanoLabel1);
            this.tabPage1.Controls.Add(this.shimanoButton1);
            this.tabPage1.Controls.Add(this.btnFind);
            this.tabPage1.Controls.Add(this.BtnClose);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(422, 68);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Find";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // txtFind
            // 
            this.txtFind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFind.Location = new System.Drawing.Point(76, 11);
            this.txtFind.Margin = new System.Windows.Forms.Padding(2);
            this.txtFind.Name = "txtFind";
            this.txtFind.ShimanoDataType = BaseFormLibrary.ShimanoTextBox.CollapseDirection.String;
            this.txtFind.Size = new System.Drawing.Size(342, 20);
            this.txtFind.TabIndex = 1;
            // 
            // shimanoLabel1
            // 
            this.shimanoLabel1.AutoSize = true;
            this.shimanoLabel1.Location = new System.Drawing.Point(14, 14);
            this.shimanoLabel1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.shimanoLabel1.Name = "shimanoLabel1";
            this.shimanoLabel1.Size = new System.Drawing.Size(59, 13);
            this.shimanoLabel1.TabIndex = 0;
            this.shimanoLabel1.Text = "Find what :";
            // 
            // shimanoButton1
            // 
            this.shimanoButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.shimanoButton1.Location = new System.Drawing.Point(182, 35);
            this.shimanoButton1.Margin = new System.Windows.Forms.Padding(2);
            this.shimanoButton1.Name = "shimanoButton1";
            this.shimanoButton1.Size = new System.Drawing.Size(76, 24);
            this.shimanoButton1.TabIndex = 4;
            this.shimanoButton1.Text = "Find All";
            this.shimanoButton1.UseVisualStyleBackColor = true;
            this.shimanoButton1.Click += new System.EventHandler(this.shimanoButton1_Click);
            // 
            // btnFind
            // 
            this.btnFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFind.Location = new System.Drawing.Point(262, 35);
            this.btnFind.Margin = new System.Windows.Forms.Padding(2);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(76, 24);
            this.btnFind.TabIndex = 2;
            this.btnFind.Text = "Find Next";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.BtnFind_Click);
            // 
            // BtnClose
            // 
            this.BtnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnClose.Location = new System.Drawing.Point(342, 35);
            this.BtnClose.Margin = new System.Windows.Forms.Padding(2);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(76, 24);
            this.BtnClose.TabIndex = 3;
            this.BtnClose.Text = "Close";
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // grdFindAll
            // 
            this.grdFindAll.AllowUserToAddRows = false;
            this.grdFindAll.AllowUserToDeleteRows = false;
            this.grdFindAll.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grdFindAll.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.grdFindAll.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdFindAll.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RowNumber,
            this.Value});
            this.grdFindAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdFindAll.Location = new System.Drawing.Point(0, 94);
            this.grdFindAll.Name = "grdFindAll";
            this.grdFindAll.ReadOnly = true;
            this.grdFindAll.RowHeadersVisible = false;
            this.grdFindAll.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grdFindAll.Size = new System.Drawing.Size(430, 155);
            this.grdFindAll.TabIndex = 19;
            this.grdFindAll.Visible = false;
            this.grdFindAll.SelectionChanged += new System.EventHandler(this.grdFindAll_SelectionChanged);
            // 
            // lblTtlRow
            // 
            this.lblTtlRow.AutoSize = true;
            this.lblTtlRow.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblTtlRow.Location = new System.Drawing.Point(0, 249);
            this.lblTtlRow.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTtlRow.Name = "lblTtlRow";
            this.lblTtlRow.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblTtlRow.Size = new System.Drawing.Size(73, 13);
            this.lblTtlRow.TabIndex = 18;
            this.lblTtlRow.Text = "0 cell(s) found";
            this.lblTtlRow.Visible = false;
            // 
            // RowNumber
            // 
            this.RowNumber.DataPropertyName = "RowNumber";
            this.RowNumber.FillWeight = 30F;
            this.RowNumber.HeaderText = "Row Number";
            this.RowNumber.Name = "RowNumber";
            this.RowNumber.ReadOnly = true;
            // 
            // Value
            // 
            this.Value.DataPropertyName = "Value";
            this.Value.FillWeight = 70F;
            this.Value.HeaderText = "Value";
            this.Value.Name = "Value";
            this.Value.ReadOnly = true;
            // 
            // _FindForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 262);
            this.Controls.Add(this.grdFindAll);
            this.Controls.Add(this.lblTtlRow);
            this.Controls.Add(this.shimanoTabControl1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "_FindForm";
            this.ShowIcon = false;
            this.Text = "Shimano - Find";
            this.Load += new System.EventHandler(this._FindForm_Load);
            this.shimanoTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdFindAll)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        public ShimanoTabControl shimanoTabControl1;
        public System.Windows.Forms.TabPage tabPage1;
        public ShimanoTextBox txtFind;
        private ShimanoLabel shimanoLabel1;
        private ShimanoButton shimanoButton1;
        private ShimanoButton btnFind;
        private ShimanoButton BtnClose;
        public System.Windows.Forms.DataGridView grdFindAll;
        public ShimanoLabel lblTtlRow;
        private System.Windows.Forms.DataGridViewTextBoxColumn RowNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
    }
}