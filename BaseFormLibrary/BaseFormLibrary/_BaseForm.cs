using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaseFormLibrary
{
    public partial class _BaseForm : Form
    {
        public string FrmNo = "";
        public _BaseForm()  
        {
            InitializeComponent();
            
            if (ShimanoMainForm == false)
            {
                    try
                    {
                        this.BackColor = Color.FromName(gVar.getProperty("DefaultFormColor"));
                    }
                    catch
                    {
                        //default color
                    }
            }
            
        }
        public void ModSet(Form ThisForm)
        {

            string Sql = @" select a.ObjNam,a.ObjProMet,a.ObjVal,a.ValTyp from modObjSet a
                            left outer join ModObjSetLevel b on a.comcod=b.comcod and a.Whloc=b.Whloc and a.FrmNam=b.FrmNam and a.ObjNam=b.ObjNam and b.UseLev='" + gVar.getProperty("UseLev") + @"'
                            where a.FrmNam ='" + ThisForm.Name + "' and a.comcod ='" + Convert.ToString(gVar.getProperty("Comcod")) + @"'
                            and not isnull(a.ObjProMet,'')='' and a.ObjVal is not null
                            and b.ObjNam is null
                            union
                            select ObjNam,ObjProMet,ObjVal,ValTyp from ModObjSetLevel
                            where FrmNam ='" + ThisForm.Name + "' and comcod ='" + Convert.ToString(gVar.getProperty("Comcod")) + @"'
                            and not isnull(ObjProMet,'')='' and ObjVal is not null
                            and UseLev='" + gVar.getProperty("UseLev") + "'";
            //"select * from modObjSet where FrmNam ='" + ThisForm.Name + "' and comcod ='" + Convert.ToString(gVar.getProperty("Comcod")) + "' and not isnull(ObjProMet,'')='' and ObjVal is not null";
            DataTable tModSet = SysUtl.ExecuteDTQuery(Sql, null);
            if(tModSet == null)
            {
                return;
            }

            for (int i = 0; i < tModSet.Rows.Count; i++)
            {
                Control[] MyTxt = ThisForm.Controls.Find(tModSet.Rows[i]["ObjNam"].ToString(), true);
                if (MyTxt.Length > 0)
                {
                    Control myCtr = MyTxt[0];
                    PropertyInfo loPro = myCtr.GetType().GetProperty(tModSet.Rows[i]["ObjProMet"].ToString().Trim(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    switch (tModSet.Rows[i]["ValTyp"].ToString())
                    {
                        case "C":
                            loPro.SetValue(myCtr, tModSet.Rows[i]["ObjVal"].ToString().Trim(),null);
                            break;
                        case "L":
                            if (tModSet.Rows[i]["ObjVal"].ToString().ToUpper().Trim() == ".T." || tModSet.Rows[i]["ObjVal"].ToString().ToUpper().Trim() == "TRUE")
                            {
                                loPro.SetValue(myCtr, true,null);
                            }
                            if (tModSet.Rows[i]["ObjVal"].ToString().ToUpper().Trim() == ".F." || tModSet.Rows[i]["ObjVal"].ToString().ToUpper().Trim() == "FALSE")
                            {
                                loPro.SetValue(myCtr, false,null);
                            }
                            break;
                        case "N":
                            loPro.SetValue(myCtr, Convert.ToDecimal(tModSet.Rows[i]["ObjVal"].ToString()),null);
                            break;
                        case "D":
                            loPro.SetValue(myCtr, Convert.ToDateTime(tModSet.Rows[i]["ObjVal"].ToString()),null);
                            break;
                        default:
                            break;
                    }

                }
            }
        }

        public void ActionDo(object sender, EventArgs e)
        { 

        }

        private void _BaseForm_Load(object sender, EventArgs e)
        {

            try
            {
                SysUtl.ExecuteNonQuery("exec SP_ActLog @PrgNam='" + this.Text + "'", null);
            }
            catch
            {
                //default
            }

            if (ShimanoMainForm == true)
            {
                MaximizeBox = true;
                MinimizeBox = true;
            }
        }

        private void _BaseForm_FormClosed(object sender, FormClosedEventArgs e)
        {

            string Sql = @"insert into LogHisAct(RunDat,PCNam,PrgNam,UseID,EndDat,Mesg)
                            select a.AccDat,b.PCNam,a.PrgNam,b.UseID,getdate(),a.PrgNam from ActLog a
                            inner join 
                            (SELECT session_id, host_name as PCNam,SubString(login_name,12,15) as UseID,GETDATE()  as AccDat
                            FROM sys.dm_exec_sessions  where session_id = @@SPID) b on a.PID=b.session_id 
                            where a.PrgNam='" + this.Text + @"'
                            
                            delete a from ActLog a
                            inner join 
                            (SELECT session_id, host_name as PCNam,SubString(login_name,12,15) as UseID,GETDATE()  as AccDat
                            FROM sys.dm_exec_sessions  where session_id = @@SPID) b on a.PID=b.session_id 
                            where a.PrgNam='" + this.Text + @"'";
            try
            {
                SysUtl.ExecuteNonQuery(Sql, null);
            }
            catch(Exception ex)
            {
                SysUtl.SavErrLog(ex.ToString(),ex);
            }
        }

        private void shimanoButton5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
    
}
