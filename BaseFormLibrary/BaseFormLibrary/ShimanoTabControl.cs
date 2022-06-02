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
    [ToolboxBitmap(typeof(TabControl))]
    public partial class ShimanoTabControl : TabControl
    {
        public ShimanoTabControl()
        {
            InitializeComponent();   
        }

      

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            
        }

        private void ShimanoTabControl_BindingContextChanged(object sender, EventArgs e)
        {
                this.SizeMode = TabSizeMode.Fixed;
                this.ItemSize = new Size((this.Width / ((this.TabCount == 0) ? 1 : this.TabCount) ) - 2, 0);
            
        }


    }
}
