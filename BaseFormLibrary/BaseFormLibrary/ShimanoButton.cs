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
    public partial class ShimanoButton : Button
    {


        public void ShimanoCheckDBAuthorize(string FrmNo)
        {
            this.Enabled = false;
            string SQL = string.Format(@"select [group],BusFunc,MnuCod,isnull(BtnFuncAdd,0) as BtnFuncAdd,isnull(BtnFuncEdit,0) as BtnFuncEdit,isnull(BtnFuncDelete,0) as BtnFuncDelete from levelmenu where [group]='{0}' 
                                            and BusFunc='{1}' and MnuCod='{2}'", gVar.getProperty("SysNam"), gVar.getProperty("sys_BusFunc"), FrmNo);
            DataTable dt = SysUtl.ExecuteDTQuery(SQL, null);
            foreach (DataRow dr in dt.Rows)
            {
                this.Enabled = Convert.ToBoolean(dr["BtnFuncAdd"].ToString());
            }

        }

        public event EventHandler ButtonClick;
        public ShimanoButton()
        {
            InitializeComponent();
            this.FlatStyle = FlatStyle.Flat;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

       
        private void ShimanoButton_Click(object sender, EventArgs e)
        {
            if (ButtonClick != null)
                ButtonClick(sender, e);
        }
    }
}
