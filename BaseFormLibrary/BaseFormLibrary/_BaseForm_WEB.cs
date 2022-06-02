using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;


namespace BaseFormLibrary
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:ServerControl1 runat=server></{0}:ServerControl1>")]
    public class _BaseForm_WEB : System.Web.UI.Page
    {
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? "[" + this.ID + "]" : s);
            }

            set
            {
                ViewState["Text"] = value;
            }
        }

        public void ModSet(System.Web.UI.Page ThisForm)
        {
                if ((Session["UseID"] == null) || (Session["ConStr"] == null))
                {
                    Response.Write(@"<script>alert('Session has been terminated!')</script>");
                    Response.Write("<script>window.open('Default.aspx','_parent')</script>");
                    Server.Transfer("Default.aspx");
                    return;
                }

            string Sql = @" select a.ObjNam,a.ObjProMet,a.ObjVal,a.ValTyp from modObjSet a
                            left outer join ModObjSetLevel b on a.comcod=b.comcod and a.Whloc=b.Whloc and a.FrmNam=b.FrmNam and a.ObjNam=b.ObjNam and b.UseLev='" + Session["UseLev"] + @"'
                            where a.FrmNam ='" + ThisForm.ToString() + @"' and a.comcod ='" + Session["ComCod"] + @"'
                            and not isnull(a.ObjProMet,'')='' and a.ObjVal is not null
                            and b.ObjNam is null
                            union
                            select ObjNam,ObjProMet,ObjVal,ValTyp from ModObjSetLevel
                            where FrmNam ='" + ThisForm.ToString() + @"' and comcod ='" + Session["ComCod"] + @"'
                            and not isnull(ObjProMet,'')='' and ObjVal is not null
                            and UseLev='" + Session["UseLev"] + "'";
            DataTable tModSet = SysTool.ExecuteDTQuery(Session["ConStr"].ToString(), Sql, null);
            for (int i = 0; i < tModSet.Rows.Count; i++)
            {
                string[] strCtl = tModSet.Rows[i]["ObjNam"].ToString().Split('.');
                //Control myCtr = ThisForm.FindControl(tModSet.Rows[i]["ObjNam"].ToString());
                Control myCtr = ThisForm;
                foreach (string ComCtl in strCtl)
                {
                    try
                    {
                        myCtr = myCtr.FindControl(ComCtl);
                    }
                    catch
                    {
                        //null
                    }
                }

                if (myCtr != null)
                {
                    PropertyInfo loPro = myCtr.GetType().GetProperty(tModSet.Rows[i]["ObjProMet"].ToString().Trim(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    switch (tModSet.Rows[i]["ValTyp"].ToString())
                    {
                        case "C":
                            loPro.SetValue(myCtr, tModSet.Rows[i]["ObjVal"].ToString().Trim(), null);
                            break;
                        case "L":
                            if (tModSet.Rows[i]["ObjVal"].ToString().ToUpper().Trim() == ".T." || tModSet.Rows[i]["ObjVal"].ToString().ToUpper().Trim() == "TRUE")
                            {
                                loPro.SetValue(myCtr, true, null);
                            }
                            if (tModSet.Rows[i]["ObjVal"].ToString().ToUpper().Trim() == ".F." || tModSet.Rows[i]["ObjVal"].ToString().ToUpper().Trim() == "FALSE")
                            {
                                loPro.SetValue(myCtr, false, null);
                            }
                            break;
                        case "N":
                            loPro.SetValue(myCtr, Convert.ToDecimal(tModSet.Rows[i]["ObjVal"].ToString()), null);
                            break;
                        case "D":
                            loPro.SetValue(myCtr, Convert.ToDateTime(tModSet.Rows[i]["ObjVal"].ToString()), null);
                            break;
                        default:
                            break;
                    }
                }

            }


        }

    }
}
