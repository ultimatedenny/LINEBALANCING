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
    public partial class _Import : _BaseForm
    {
        public _Import()
        {
            InitializeComponent();
        }

        public DataTable DataTableSouce;
        public bool ImportStatus = false;

        private void _Import_Load(object sender, EventArgs e)
        {
            this.GridView1.AutoGenerateColumns = true;
            this.GridView1.DataSource = DataTableSouce;
        }

        private void shimanoButton1_Click(object sender, EventArgs e)
        {
            ImportStatus = true;
            this.Close();
        }

        private void shimanoButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
