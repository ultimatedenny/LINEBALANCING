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
    public partial class _ExportData : _BaseForm
    {
        public _ExportData()
        {
            InitializeComponent();
        }
        public string FileName = "";

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (cbExportFormat.Text != string.Empty)
            {
                SFD.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                SFD.Filter = cbExportFormat.Text;

                SFD.FileName = FileName;

                if (SFD.ShowDialog() == DialogResult.OK)
                {
                    if (SFD.FileName != "")
                    {
                        if (cbExportFormat.Text.Contains(".xlsx") || cbExportFormat.Text.Contains(".xls") || cbExportFormat.Text.Contains(".csv"))
                        {
                            SysUtl.DataSetToExcel(dsExport, SFD.FileName);
                        }
                        else if (cbExportFormat.Text.Contains(".pdf"))
                        {
                            SysUtl.DataTableToPDF(dsExport.Tables[0], SFD.FileName, false);
                        }
                        else if (cbExportFormat.Text.Contains(".txt"))
                        {
                            SysUtl.DataTableToTXT(dsExport.Tables[0], SFD.FileName);
                        }
                        else if (cbExportFormat.Text.Contains(".dbf"))
                        {
                            SysUtl.DataTableToDBF(dsExport.Tables[0], SFD.FileName);
                        }
                        else if (cbExportFormat.Text.Contains(".doc"))
                        {
                            SysUtl.DataTableToMsWord(dsExport.Tables[0], SFD.FileName);
                        }
                        else if (cbExportFormat.Text.Contains(".mdb"))
                        {
                            SysUtl.DataTableToMsAccess(dsExport.Tables[0], SFD.FileName);
                        }
                        else if (cbExportFormat.Text.Contains(".html"))
                        {
                            SysUtl.DataTableToHTML(dsExport.Tables[0], SFD.FileName);
                        }
                        else
                        {
                            MessageBox.Show("Export failed, file extantion is not register", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Export failed, file name can't empty!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }
    }
}
