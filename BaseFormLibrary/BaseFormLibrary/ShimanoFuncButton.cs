using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaseFormLibrary
{
    public partial class ShimanoFuncButton : UserControl
    {
        private bool cBtnAdd = false;
        private bool cBtnEdit = false;
        private bool cBtnDelete = false;

        public event EventHandler AddClick;
        public event EventHandler EditClick;
        public event EventHandler DeleteClick;
        public event EventHandler SaveClick;
        public event EventHandler CancelClick;

        [Category("Behavior"), DefaultValue(true)]
        [Description("Set to be true to check authorize from table lavelmenu")]
        public bool ShimanoCheckDBAuthorize { get; set; }


        public ShimanoFuncButton()
        {
            InitializeComponent();
            ShimanoCheckDBAuthorize = true;
            
        }



        public void BtnCtl(Boolean Flag)
        {
            this.BtnAdd.Enabled = (cBtnAdd == false) ? false : Flag;
            this.BtnEdit.Enabled = (cBtnEdit == false) ? false : Flag;
            this.BtnDelete.Enabled = (cBtnDelete == false) ? false : Flag;
            this.BtnSave.Enabled = !Flag;
            this.BtnCancel.Enabled = !Flag;
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            //this.BtnCtl(false);
            if (AddClick != null)
                AddClick(sender, e);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            //this.BtnCtl(false);
            if (EditClick != null)
                EditClick(sender, e);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            //this.BtnCtl(false);
            if (DeleteClick != null)
                DeleteClick(sender, e);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            //this.BtnCtl(true);
            if (SaveClick != null)
                SaveClick(sender, e);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            //this.BtnCtl(true);
            if (CancelClick != null)
                CancelClick(sender, e);
        }

        
        private void ShimanoFuncButton_Load(object sender, EventArgs e)
        {
            if (ShimanoCheckDBAuthorize)
            {
                try
                {
                    string SQL = string.Format(@"select [group],BusFunc,MnuCod,isnull(BtnFuncAdd,0) as BtnFuncAdd,isnull(BtnFuncEdit,0) as BtnFuncEdit,isnull(BtnFuncDelete,0) as BtnFuncDelete from levelmenu where [group]='{0}' 
                                            and BusFunc='{1}' and MnuCod='{2}'", gVar.getProperty("SysNam"), gVar.getProperty("sys_BusFunc"), (this.ParentForm as _BaseForm).FrmNo.ToString());
                    DataTable dt = SysUtl.ExecuteDTQuery(SQL, null);
                    foreach (DataRow dr in dt.Rows)
                    {
                        cBtnAdd = Convert.ToBoolean(dr["BtnFuncAdd"].ToString());
                        cBtnEdit = Convert.ToBoolean(dr["BtnFuncEdit"].ToString());
                        cBtnDelete = Convert.ToBoolean(dr["BtnFuncDelete"].ToString());
                    }

                    BtnCtl(true);
                }
                catch
                {

                }

            }

        }
    }
}
