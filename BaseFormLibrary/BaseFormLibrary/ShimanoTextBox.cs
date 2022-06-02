using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaseFormLibrary
{
    [ToolboxBitmap(typeof(TextBox))]
    public partial class ShimanoTextBox : TextBox
    {
        public enum CollapseDirection
        {
            String,
            Integer,
            Decimal
        }

        #region Properties

        [Category("Behavior"), DefaultValue(true)]
        [Description("Set to be true for auto tab to next tab index.")]
        public bool ShimanoAutoTabToNextIndex { get; set; }

        [Browsable(true), DefaultValue("String")]
        [ListBindable(true), Editor(typeof(ComboBox), typeof(UITypeEditor))]
        private CollapseDirection _direction = CollapseDirection.String;
        public CollapseDirection ShimanoDataType
        {
            get { return _direction; }
            set
            {
                _direction = value;

            }
        }

        #endregion

        public ShimanoTextBox()
        {
            InitializeComponent();
            this.ShimanoAutoTabToNextIndex = true;
            this.BorderStyle = BorderStyle.FixedSingle;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
       
        private void ShimanoTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.ShimanoAutoTabToNextIndex == true)
            { 
                if (e.KeyCode == Keys.Enter)
                {
                    SendKeys.Send("{Tab}");
                }
            }
        }

        private void ShimanoTextBox_Leave(object sender, EventArgs e)
        {
            if ((this.Enabled == true) && (this.ReadOnly == false))
                this.BackColor = default(Color);
        }

        private void ShimanoTextBox_Enter(object sender, EventArgs e)
        {
            if((this.Enabled == true) && (this.ReadOnly == false))
                this.BackColor = Color.Cyan;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (this.ShimanoDataType.ToString() == "String")
            {
                e.Handled = false;
            }
            else if (this.ShimanoDataType.ToString() == "Integer")
            {
                if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
            else if (this.ShimanoDataType.ToString() == "Decimal")
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != '\b')
                {
                    e.Handled = true;
                }
                if (e.KeyChar == '.' && this.Text.IndexOf('.') > -1)
                {
                    e.Handled = true;
                }
                if (e.KeyChar == '.' && this.Text == string.Empty)
                {
                    e.Handled = true;
                }
            }
        }


        


    }
}
