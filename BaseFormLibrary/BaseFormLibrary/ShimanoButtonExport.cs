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
    [ToolboxBitmap(typeof(Button))]
    public partial class ShimanoButtonExport : Button
    {
        public event EventHandler ButtonClick;
        public ShimanoButtonExport()
        {
            InitializeComponent();
            this.Text = "Export";
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        private void ShimanoButtonExport_Click(object sender, EventArgs e)
        {
            if (ButtonClick != null)
                ButtonClick(sender, e);
        }
    }
}
