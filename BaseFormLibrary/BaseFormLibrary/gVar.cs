using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections;
using System.Windows.Forms;
using System.Reflection;
public static class gVar {
	
//**** Sample 
    public static Dictionary<String, Object> properties = new Dictionary<String, Object>();
    public static Dictionary<String, String> ProTyp = new Dictionary<String, String>();
 
    public static void inivar(String comcod) 
    {
        DataTable tSysset = SysUtl.ExecuteDTQuery("select * from sysset", null);
        DataTable tCom = SysUtl.ExecuteDTQuery("select * from Com where comcod ='"+comcod+"'", null);
        Object loVal ;
        String FldNam = "";
        if (tSysset.Rows.Count > 0)
        {
            for (int nMain = 0; nMain < tSysset.Rows.Count; nMain++)
            {
                loVal = tSysset.Rows[nMain]["Sysvar"];
                if (tSysset.Rows[nMain]["MapField"].ToString().Trim()!=String.Empty)
                {
                    FldNam = tSysset.Rows[nMain]["MapField"].ToString();
                    if (FldNam.Contains("."))
                    {
                       FldNam= FldNam.Substring(FldNam.IndexOf(".")+1);
                    }
                    loVal = tCom.Rows[0][FldNam].ToString();
                }
                gVar.AddProperty(tSysset.Rows[nMain]["Syscod"].ToString(), loVal, tSysset.Rows[nMain]["VarTyp"].ToString());
            }
        }
        tSysset.Dispose();
    }
   
    public static void AddProperty(String key, Object value,String ValTyp) 
    {
        if (properties.ContainsKey(key.ToUpper())==false)
        {
            properties.Add(key.ToUpper(), value);
        }
        if (ProTyp.ContainsKey(key.ToUpper()) == false)
        {
            ProTyp.Add(key.ToUpper(), ValTyp);
        }      
    }

    public static void DestroyAllProperty()
    {
        if (properties.Count > 0)
        {
            properties.Clear();
        }
    }

    public static bool DestroyProperty(String key)
    {
        if (properties.Remove(key.ToUpper()))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public static dynamic getProperty(String key)
    {
        switch (ProTyp[key.ToUpper()])
        {
            case "C":
                return properties[key.ToUpper()].ToString();
            case "N":
                //return (double)properties[key];
                return Convert.ToDecimal(properties[key.ToUpper()]);
            case "D":
                //return (DateTime)properties[key];
                return Convert.ToDateTime (properties[key.ToUpper()]);
            case "L":
                //return (Boolean)properties[key];
                return Convert.ToBoolean(properties[key.ToUpper()]);
            default:
                return properties[key.ToUpper()];
        }
    }

    public static void setProperty(String key,Object value)
    {
        properties[key.ToUpper()] = value;
    }


}

