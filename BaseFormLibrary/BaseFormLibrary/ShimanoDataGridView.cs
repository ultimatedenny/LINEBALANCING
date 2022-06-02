using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaseFormLibrary
{
    [ToolboxBitmap(typeof(DataGridView))]
    public partial class ShimanoDataGridView : DataGridView
    {
        public ShimanoDataGridView()
        {
            InitializeComponent();

            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Cyan;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DefaultCellStyle = dataGridViewCellStyle1;
            this.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.ControlLight;
            this.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.MultiSelect = false;
            this.ReadOnly = true;
            this.RowTemplate.Height = 24;
            this.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        private void ShimanoDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
           
            if (e.Button == MouseButtons.Right)
            {
                if (this.Rows.Count > 0) 
                {
                    if (this.Columns[e.ColumnIndex].CellType.ToString() == "System.Windows.Forms.DataGridViewTextBoxCell")
                    {
                        if (this.Rows.Count > 0)
                        {
                            //MessageBox.Show(this.Columns[e.ColumnIndex].CellType.ToString());
                            ContextMenu m = new ContextMenu();
                            if (this.Columns[e.ColumnIndex].SortMode != DataGridViewColumnSortMode.NotSortable)
                            {
                                m.MenuItems.Add(new MenuItem("Sort Asc", delegate (object sdr, EventArgs ee) { Sort_Click(sdr, ee, e.ColumnIndex, " Asc"); }));
                                m.MenuItems.Add(new MenuItem("Sort Des", delegate (object sdr, EventArgs ee) { Sort_Click(sdr, ee, e.ColumnIndex, " Desc"); }));
                                if (dtOrder.Rows.Count > 0)
                                {
                                    m.MenuItems.Add(new MenuItem("Clear Sort", delegate (object sdr, EventArgs ee) { Sort_Click(sdr, ee, e.ColumnIndex, "Clear"); }));
                                }
                            }
                            m.MenuItems.Add(new MenuItem("Find", delegate (object sdr, EventArgs ee) { Find_Click(sdr, ee, e.ColumnIndex); }));
                            if (this.Columns[e.ColumnIndex].Frozen == false)
                            {
                                m.MenuItems.Add(new MenuItem("Freeze Panes", delegate (object sdr, EventArgs ee) { Freeze_Click(sdr, ee, e.ColumnIndex, true); }));
                            }
                            else
                            {
                                m.MenuItems.Add(new MenuItem("Unfreeze Pane", delegate (object sdr, EventArgs ee) { Freeze_Click(sdr, ee, e.ColumnIndex, false); }));
                            }
                            int currentMouseOverRow = this.HitTest(e.X, e.Y).RowIndex;

                            if (currentMouseOverRow >= 0)
                            {
                                m.MenuItems.Add(new MenuItem(string.Format("Do something to row {0}", currentMouseOverRow.ToString())));
                            }

                            m.Show(this, this.PointToClient(Cursor.Position));
                        }
                    }
                }
            }
        }

        DataTable dtSource = new DataTable();
        DataTable dtOrder = new DataTable
        {
            Columns = { { "ColNam", typeof(string) }, { "Sort", typeof(string) } }
        };

        public bool  colNew = false;
        void Sort_Click(Object sender, EventArgs e, int ColIndex, string srt)
        {

            if (colNew == false)
            {
                foreach (DataGridViewColumn dcol in this.Columns)
                {
                    dcol.HeaderCell = new MyDataGridViewColumnHeaderCell();
                    dcol.SortMode = DataGridViewColumnSortMode.Programmatic;
                }
                colNew = true;
            }

            if(dtSource.Rows.Count == 0)
            {
                if(this.DataSource is DataView) //put vilidation dataview or datatable by bondra 20180307
                    dtSource = (this.DataSource as DataView).ToTable();
                else if(this.DataSource is DataTable) //put vilidation dataview or datatable by bondra 20180307
                    dtSource = (this.DataSource as DataTable);
            }

            if (dtSource.Rows.Count == 0)
            {
                MessageBox.Show("Error found, there is no data in datagridview.");
                return;
            }

            string colnam = "", strOrder="";
            colnam = this.Columns[ColIndex].DataPropertyName.ToString(); //change from column name to be DataPropertyName by bondra 20180307

            foreach (DataRow row in dtOrder.Rows)
            {

                if (row["ColNam"].ToString().Equals(colnam))
                {
                    row.Delete();
                    break;
                }

            }

            dtOrder.Rows.Add(colnam, srt);

            foreach (DataRow drRow in dtOrder.Rows)
            {
                strOrder += "," + drRow["ColNam"].ToString() + drRow["Sort"].ToString();
                if (srt == "Clear")
                {
                    ((MyDataGridViewColumnHeaderCell)this.Columns[drRow["ColNam"].ToString()].HeaderCell).SortOrderDirection = SortOrder.None;
                }
            }

            if (strOrder.Length > 0)
            {
                strOrder = strOrder.Substring(1, strOrder.Length - 1);
            }

            if(srt== "Clear")
            {
                dtOrder.Clear();
                strOrder = "";
            }

            DataTable dt = dtSource;
            DataView x = dt.DefaultView;
            x.Sort = strOrder;
            this.DataSource = dt;

            ((MyDataGridViewColumnHeaderCell)this.Columns[ColIndex].HeaderCell).SortOrderDirection = (srt == " Asc") ? SortOrder.Ascending : (srt == " Desc") ? SortOrder.Descending : SortOrder.None;
        }

        

        void Find_Click(Object sender, EventArgs e, int ColIndex)
        {
            _FindForm FindForm = new _FindForm();
            FindForm.dgvFind = this;
            FindForm.ColIndex = ColIndex;

            FindForm.shimanoTabControl1.TabPages[0].Text = this.Columns[ColIndex].HeaderText.ToString();
            FindForm.ShowDialog();
            //string FindStr = FindForm.txtFind.Text.ToUpper();
            //if (FindStr != "")
            //{
            //    bool valueResulet = true;
            //    foreach (DataGridViewRow row in this.Rows)
            //    {
                    
            //        if (row.Cells[ColIndex].Value.ToString().ToUpper().Contains(FindStr))
            //        {
            //            if (valueResulet == true)
            //            { 
            //                this.ClearSelection();
            //                this.Rows[row.Index].Cells[ColIndex].Selected = true;
            //                this.FirstDisplayedScrollingRowIndex = row.Index;
            //                valueResulet = false;
            //                break;
            //            }
            //        }
                    
            //    }
            //    if (valueResulet != false)
            //    {
            //        MessageBox.Show("Record is not avalable", "Not Found");
            //        return;
            //    }
            //}
            FindForm.Dispose();
        }

        void Freeze_Click(Object sender, EventArgs e, int ColIndex, bool Status)
        {
            this.Columns[ColIndex].Frozen = Status;
            if(Status == false)
            {
                this.Columns[0].Frozen = Status;
            }
        }

        //set number in left side -- not yet used
        private void ShimanoDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        private void ShimanoDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex >= 0 && e.RowIndex >=0)
                this.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.SelectionBackColor = Color.DeepSkyBlue;
        }

        private void ShimanoDataGridView_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
                this.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.SelectionBackColor = default(Color);
        }

        

        void CopyCell_Click(Object sender, EventArgs e, int ColIndex, int RowIndex)
        {
            Clipboard.SetText(this.Rows[RowIndex].Cells[ColIndex].Value.ToString());
        }

        void CopyFullRow_Click(Object sender, EventArgs e, int ColIndex, int RowIndex)
        {
            var newline = System.Environment.NewLine;
            var tab = "\t";
            var clipboard_string = new StringBuilder();
                for (int i = 0; i < this.Rows[RowIndex].Cells.Count; i++)
                {
                    if (i == (this.Rows[RowIndex].Cells.Count - 1))
                        clipboard_string.Append(this.Rows[RowIndex].Cells[i].Value + newline);
                    else
                        clipboard_string.Append(this.Rows[RowIndex].Cells[i].Value + tab);
                }

            Clipboard.SetText(clipboard_string.ToString());
        }

        private void ShimanoDataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (this.Columns[e.ColumnIndex].CellType.ToString() == "System.Windows.Forms.DataGridViewTextBoxCell")
                {
                    if(e.RowIndex.ToString() != "-1")
                    {
                        ContextMenu m = new ContextMenu();
                        m.MenuItems.Add(new MenuItem("Copy Cell", delegate (object sdr, EventArgs ee) { CopyCell_Click(sdr, ee, e.ColumnIndex, e.RowIndex); }));
                        m.MenuItems.Add(new MenuItem("Copy Full Row", delegate (object sdr, EventArgs ee) { CopyFullRow_Click(sdr, ee, e.ColumnIndex, e.RowIndex); }));
                        m.Show(this, this.PointToClient(Cursor.Position));
                    }
                }
            }
        }


    }
}

public partial class MyDataGridViewColumnHeaderCell : DataGridViewColumnHeaderCell
{
    public SortOrder SortOrderDirection { get; set; } // defaults to zero = SortOrder.None;

    protected override void Paint(System.Drawing.Graphics graphics, System.Drawing.Rectangle clipBounds, System.Drawing.Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
    {

        this.SortGlyphDirection = this.SortOrderDirection;
        base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

    }

    //public override object Clone()
    //{
    //    MyDataGridViewColumnHeaderCell result = (MyDataGridViewColumnHeaderCell)base.Clone();
    //    result.SortOrderDirection = this.SortOrderDirection;
    //    return result;
    //}

}
