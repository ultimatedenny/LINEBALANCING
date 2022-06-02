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
    [ToolboxBitmap(typeof(ComboBox))]
    public partial class ShimanoComboBox : ComboBox
    {

        #region Properties

        [Category("Behavior"), DefaultValue(true)]
        [Description("Set to true when you allow user to input other items")]
        public bool ShimanoAllowManual { get; set; }


        #endregion
        public ShimanoComboBox()
        {
            InitializeComponent();
            ShimanoAllowManual = true;
            this.FlatStyle = FlatStyle.Flat;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }


        private void ShimanoComboBox_Leave(object sender, EventArgs e)
        {
            this.BackColor = default(Color);
            if (this.ShimanoAllowManual == false)
            {
                if (this.Items.Count > 0 && this.Text != string.Empty)
                {
                    int index = -1;
                    index = this.FindString(this.Text);
                    if (index > -1)
                    {
                        this.SelectedIndex = index;
                    }
                    if (this.SelectedItem == null)
                    {
                        MessageBox.Show("No selected item", "Combobox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Text = string.Empty;
                    }
                }
                else
                {
                    MessageBox.Show("No selected item", "Combobox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Text = string.Empty;
                }
            }
        }

        private void ShimanoComboBox_Enter(object sender, EventArgs e)
        {
            this.BackColor = Color.Cyan;
        }
    }
}
