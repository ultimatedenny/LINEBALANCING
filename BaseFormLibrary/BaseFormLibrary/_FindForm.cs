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
    public partial class _FindForm : _BaseForm
    {
        public DataGridView dgvFind { get; set; }
        public int ColIndex { get; set; }


        public _FindForm()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            txtFind.Clear();
            this.Close();
        }

        string oldFind = "";
        int i = 0;
        private void BtnFind_Click(object sender, EventArgs e)
        {
            
            string FindStr = txtFind.Text.ToUpper();

            if (FindStr != "")
            {
                bool valueResulet = false;
                
                int j = 0;
                if ((oldFind == FindStr) && (i< dgvFind.Rows.Count))
                {
                    j = i +1;
                }
                oldFind = FindStr;

                for (i= j; i<dgvFind.Rows.Count; i++)
                {

                    if (dgvFind.Rows[i].Cells[ColIndex].Value.ToString().ToUpper().Contains(FindStr))
                    {
                        if (valueResulet == false)
                        {
                            dgvFind.ClearSelection();
                            dgvFind.Rows[dgvFind.Rows[i].Index].Cells[ColIndex].Selected = true;
                            dgvFind.FirstDisplayedScrollingRowIndex = dgvFind.Rows[i].Index;
                            valueResulet = true;
                            break;
                        }
                    }
                }
                if (valueResulet == false)
                {
                    MessageBox.Show("Record is not avalable", "Not Found");
                    return;
                }
            }


            //this.Close();
        }

        private void _FindForm_Load(object sender, EventArgs e)
        {
            this.Size = new System.Drawing.Size(446, 137);
            this.ActiveControl = txtFind;
            txtFind.Focus();
        }

        private void shimanoButton1_Click(object sender, EventArgs e)
        {
            FindAllSetProperties(true);
            if (dgvFind.DataSource == null)
            {
                MessageBox.Show("Record is not avalable", "Not Found");
                return;
            }

            DataTable dt = new DataTable();

            dt.Columns.Add("RowNumber");
            dt.Columns.Add("Value");



            string FindStr = txtFind.Text.ToUpper();
            int valueResulet = 0;
            if (FindStr != "")
            {
                

                int j = 0;

                DataRow drFind;

                for (i = j; i < dgvFind.Rows.Count; i++)
                {
                    if (dgvFind.Rows[i].Cells[ColIndex].Value.ToString().ToUpper().Contains(FindStr))
                    {
                        dgvFind.ClearSelection();
                        drFind = dt.NewRow();
                        drFind[0] = i + 1;
                        drFind[1] = dgvFind.Rows[i].Cells[ColIndex].Value.ToString();
                        dt.Rows.Add(drFind);
                        valueResulet++;
                    }
                }
                if (valueResulet ==0)
                {
                    MessageBox.Show("Record is not avalable", "Not Found");
                    return;
                }
                else
                {
                    grdFindAll.AutoGenerateColumns = false;
                    grdFindAll.DataSource = dt;

                }
            }


            lblTtlRow.Text = valueResulet.ToString() + " cell(s) found";


        }

        void FindAllSetProperties(bool val)
        {
            grdFindAll.Visible = val;
            lblTtlRow.Visible = val;
            this.Size = new System.Drawing.Size(this.Size.Width, 300);
        }

        private void grdFindAll_SelectionChanged(object sender, EventArgs e)
        {
            if (grdFindAll.Focused)
            {
                dgvFind.Rows[Convert.ToInt16(grdFindAll.SelectedRows[0].Cells[0].Value.ToString())-1].Selected = true;
                try
                {
                    dgvFind.FirstDisplayedScrollingRowIndex = dgvFind.SelectedRows[0].Index - 1;
                }
                catch
                {
                    dgvFind.FirstDisplayedScrollingRowIndex = dgvFind.SelectedRows[0].Index;
                }
                
            }
        }
    }
}
