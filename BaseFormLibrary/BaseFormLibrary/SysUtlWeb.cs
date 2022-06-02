using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using System.Xml;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Windows.Forms;
using BaseFormLibrary;
using Excel = Microsoft.Office.Interop.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Spire.DataExport;
using Spire.DataExport.DBF;
using Spire.DataExport.PDF;
using Spire.DataExport.RTF;
using Spire.DataExport.Access;
using Spire.DataExport.HTML;
using QrCodeNet.Encoding;
using System.Drawing;
using System.Drawing.Imaging;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Security;
using System.Reflection;
using Spire.Barcode;
using System.Web.Configuration;

public  class SysUtlWeb
{
    // Header 
    [DllImport("advapi32.DLL", SetLastError = true)]
    public static  extern int LogonUser(
        string lpszUsername,
        string lpszDomain,
        string lpszPassword,
        int dwLogonType,
        int dwLogonProvider,
        out IntPtr phToken);

    [DllImport("advapi32.DLL")]
    public static extern int ImpersonateLoggedOnUser(IntPtr hToken); //handle to token for logged-on user

    [DllImport("advapi32.DLL")]
    public static extern bool RevertToSelf();

    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hToken);
    enum LogonProvider
    {
        LOGON32_PROVIDER_DEFAULT = 0,
        LOGON32_PROVIDER_WINNT50 = 3,
        LOGON32_PROVIDER_WINNT40 = 2,
        LOGON32_PROVIDER_WINNT35 = 1,
        LOGON32_LOGON_INTERACTIVE = 2,
        LOGON32_LOGON_NETWORK = 3,
        LOGON32_LOGON_BATCH = 4,
        LOGON32_LOGON_SERVICE = 5,
        LOGON32_LOGON_UNLOCK = 7
    }


    public  String SqlConStr = "";
    public  string DefaultSqlConStr = "";
    public  byte[] RoleCookie;
    public  bool ConnectionStatus = false;
    public  DataSet dsXMLConfig = new DataSet();
    public  String SysNam = "";// ConfigurationManager.AppSettings["SystemName"].ToString();
    public  String ErrPath = ""; //ConfigurationManager.AppSettings["ErrLogPath"].ToString()
    public  IntPtr admin_token = IntPtr.Zero;
    //public  SqlConnection SqlConHd;
    public static int TotalConStr = 10;
    //public  SqlConnection SqlConHd = new SqlConnection();
    public  SqlConnection[] PSqlConHd = new SqlConnection[TotalConStr]; // maximum 10 persistent connection

    //*****************************************************************************
    //                                    WINAUTH 
    //*****************************************************************************
    public Boolean WinAuth(String UsrNam, String PasWrd, String ConStr, bool UseRole, String RoleName, String RolePassword, bool DefaultCon = false)
    {
        String ssDomain = WebConfigurationManager.AppSettings["DOMAIN"];
        //String ssDomain = "SHIMANOACE";
        IntPtr phToken = IntPtr.Zero;
        //CloseHandle(admin_token);
        //RevertToSelf();
        //admin_token = IntPtr.Zero;

        int valid = LogonUser(
            UsrNam,
            ssDomain,
            PasWrd,
            (int)LogonProvider.LOGON32_LOGON_INTERACTIVE,
            (int)LogonProvider.LOGON32_PROVIDER_DEFAULT,
            out admin_token);

        if (valid != 0)
        {
            //int IPI = ImpersonateLoggedOnUser(admin_token);
            //if (IPI != 0)
            //{

                //ConnectionStatus = SQLCon(ConStr, UseRole, RoleName, RolePassword, DefaultCon);
                //CloseHandle(admin_token);
                //RevertToSelf();
                //admin_token = IntPtr.Zero;
                //if (ConnectionStatus == false)
                //{
                //    return false;
                //}
                //else
                //{
                //    return true;
                //}

                return true;

            //}
            //else
            //{
            //    return false;
            //}
        }
        else
        {
            return false;
        }

    }

    //*****************************************************************************
    //                                    WINAUTH 
    //*****************************************************************************
    public  Boolean WinAuth_old(String UsrNam, String PasWrd, String ConStr, bool UseRole, String RoleName, String RolePassword, bool DefaultCon = false)
    {
        String ssDomain = Environment.UserDomainName;
        IntPtr phToken = IntPtr.Zero;
        //CloseHandle(admin_token);
        //RevertToSelf();
        //admin_token = IntPtr.Zero;

        int valid = LogonUser(
            UsrNam,
            ssDomain,
            PasWrd,
            (int)LogonProvider.LOGON32_LOGON_INTERACTIVE,
            (int)LogonProvider.LOGON32_PROVIDER_DEFAULT,
            out admin_token);

        if (valid != 0)
        {
            int IPI = ImpersonateLoggedOnUser(admin_token);
            if (IPI != 0)
            {

                ConnectionStatus = SQLCon(ConStr, UseRole, RoleName, RolePassword, DefaultCon);
                CloseHandle(admin_token);
                RevertToSelf();
                admin_token = IntPtr.Zero;
                if (ConnectionStatus == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public  Boolean Domain_Authentication(string Username, string Password, string domain)
    {
        bool issuccess = false;
        if (IsValid(Username, Password, domain))
        {
            issuccess = true;
        }
        else
        {
            issuccess = false;
        }
        return issuccess;
    }

    public  Boolean IsValid(string Username, string Password, string domain)
    {
        using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domain))
        {
            // validate the credentials
            return pc.ValidateCredentials(Username, Password);
        }
    }

    public  Boolean LoadXMLtoDataSet()
    {
        String xmlfile;
        string gDirectory = Directory.GetCurrentDirectory();
        xmlfile = gDirectory + "\\SHIMANO.XML";
        FileInfo fFile = new FileInfo(xmlfile);
        MessageBox.Show(xmlfile);
        if (fFile.Exists)
        {
            dsXMLConfig.ReadXml(xmlfile);
            return true;
        }
        else
        {
            createXMLfile(xmlfile);
            return false;
        }
    }


    //*****************************************************************************
    //                                   SQLCON 
    //*****************************************************************************
    public  Boolean SQLCon(string ConStr, bool UseRole, String RoleName, String RolePassword,bool DefaultCon = false)
    {
        if(DefaultCon==true)
        {
            DefaultSqlConStr = ConStr;
        }

        SqlConnectionStringBuilder sBuild = new SqlConnectionStringBuilder(ConStr);

        for (int ConNum = 0; ConNum < TotalConStr; ConNum++)
        {
            if(PSqlConHd[ConNum] != null)
            {
                if(PSqlConHd[ConNum].DataSource.ToString() == sBuild.DataSource.ToString() && PSqlConHd[ConNum].Database.ToString() == sBuild.InitialCatalog.ToString())
                {
                    if(PSqlConHd[ConNum].State == ConnectionState.Open)
                    {
                        if (UseRole == true)
                        {
                            UnsetApprole(PSqlConHd[ConNum], RoleCookie);
                        }
                        PSqlConHd[ConNum].Close();
                    }
                    PSqlConHd[ConNum] = null;
                }
            }

            if (PSqlConHd[ConNum] == null)
            {
                PSqlConHd[ConNum] = new SqlConnection();
                PSqlConHd[ConNum].ConnectionString = ConStr;
                PSqlConHd[ConNum].Open();
                if (UseRole == true)
                {
                    RoleCookie = SetApprole(PSqlConHd[ConNum], RoleName, RolePassword);
                }
                return true;
            }
        }
        return false;
    }

    public  SqlConnection SqlConHdDirect = new SqlConnection();
    public  bool DirectSQLCon(string ConStr)
    {
        SqlConHdDirect.ConnectionString = ConStr;
        try
        {
            SqlConHdDirect.Open();
            return true;
        }
        catch (Exception Exp)
        {
            SavErrLog(Exp.ToString(), Exp);
            return false;
        }
    }

    public  bool DisconnectDirectSQLCon()
    {
        try
        {
            SqlConHdDirect.Dispose();
            SqlConHdDirect.Close();
            return true;
        }
        catch(Exception Exp)
        {
            SavErrLog(Exp.ToString(), Exp);
            return false;
        }


    }

    public  void SQLCloseCon(string ConStr = "DefaultConStr")
    {
        if (ConStr == "DefaultConStr")
        {
            ConStr = DefaultSqlConStr;
        }

        for (int ConNum = 0; ConNum < TotalConStr; ConNum++)
        {
            if (PSqlConHd[ConNum] != null)
            {
                if (PSqlConHd[ConNum].ConnectionString == ConStr)
                {
                    SqlConnection.ClearPool(PSqlConHd[ConNum]);
                    PSqlConHd[ConNum].Dispose();
                    PSqlConHd[ConNum].Close();
                    PSqlConHd[ConNum] = null;
                    break;
                }
            }

        }
    }

    //*****************************************************************************
    //                                   createXMLfile
    //*****************************************************************************
    public  void createXMLfile(string XMLFilePath)
    {
        try
        {
            XmlDocument document = new XmlDocument();
            XmlDeclaration newChild = document.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement element3 = document.CreateElement("Configuration");
            document.InsertBefore(newChild, document.DocumentElement);
            document.AppendChild(element3);
            XmlElement element2 = document.CreateElement("AppSetting");
            document.DocumentElement.PrependChild(element2);
            XmlElement element = document.CreateElement("Server");
            element.SetAttribute("KeyName", "SQLCON");
            element.SetAttribute("Value", SysUtl.SqlConStr);
            element2.AppendChild(element);
            document.Save(XMLFilePath);
        }
        catch(Exception Exp)
        {
            SavErrLog(Exp.ToString(), Exp);
        }
    }
    //******************************************************************************
    //                                  LoadXMLConfiguration
    //******************************************************************************
    public  string LoadXMLConfiguration(DataSet dsXML, string TableTab, string RowFilter)
    {

        try
        {

            DataView defaultView = dsXML.Tables[TableTab].DefaultView;
            defaultView.RowFilter = RowFilter;
            if (defaultView.Count > 0)
            {
                return (defaultView[0]["Value"].ToString());
            }
            else
            {
                return (null);
            }

        }
        catch(Exception Exp)
        {
            SavErrLog(Exp.ToString(), Exp);
            return (null);
        }

    }
    //*******************************************************************************
    //                              UpdateConfigSetting
    //*******************************************************************************
    public  void UpdateConfigSetting(string XMLFilePath)
    {
        DataSet set;
        try
        {
            set = new DataSet();
            if (File.Exists(XMLFilePath))
            {
                set.ReadXml(XMLFilePath);
                DataView defaultView = set.Tables["Server"].DefaultView;
                defaultView.RowFilter = "keyName='SQLCON'";
                if (defaultView.Count > 0)
                {
                    defaultView[0]["Value"] = SysUtl.SqlConStr;
                }
                set.AcceptChanges();
                set.WriteXml(XMLFilePath);

            }
            else
            {
                SysUtl.createXMLfile(XMLFilePath);
            }

        }
        catch(Exception Exp)
        {
            SavErrLog(Exp.ToString(), Exp);
        }
    }
    //***************************************************************************
    //                                  ExecuteDTQuery
    //***************************************************************************
    public  DataTable ExecuteDTQuery(String pSqlCmd, SqlTransaction Tranx,string ConStr="DefaultConStr", bool DefaultCon = false)
    {
        SqlConnection SelCon = new SqlConnection();
        try
        {
            if(DefaultCon == true)
            {
                ConStr = DefaultSqlConStr;
            }
            
            SqlConnectionStringBuilder sBuild = new SqlConnectionStringBuilder(ConStr);

            for (int ConNum = 0; ConNum < TotalConStr; ConNum++)
            {
                if (PSqlConHd[ConNum] != null)
                {
                    if (PSqlConHd[ConNum].ConnectionString == sBuild.ConnectionString.ToString() || PSqlConHd[ConNum].Database.ToString() == sBuild.InitialCatalog.ToString())
                    {
                        SelCon = PSqlConHd[ConNum];
                        break;
                    }
                }

            }
            if (SelCon == null)
            {
                return null;
            }

            if (SelCon.State == ConnectionState.Closed)
            {
                return null;
            }

            SqlCommand Selectcommand = new SqlCommand(pSqlCmd, SelCon);
            DataSet ds = new DataSet();
            SqlDataAdapter DataAd = new SqlDataAdapter();
            DataTable Dt = new DataTable();
            DataAd.SelectCommand = Selectcommand;
            if (Tranx != null)
            {
                DataAd.SelectCommand.Transaction = Tranx;
            }
            DataAd.Fill(ds);
            if ((ds != null) && (ds.Tables.Count > 0))
            {
                return ds.Tables[0];
            }
            else
            {
                return null;
            }
        }
        catch (Exception Exp)
        {
            if (Tranx != null)
            {
                Tranx.Rollback();
            }
            if (SelCon.State == ConnectionState.Open)
            {
                ExecuteNonQuery(@"IF @@TRANCOUNT > 0 
                                    ROLLBACK TRAN", null);
            }
            SavErrLog(Exp.ToString(),Exp);
            return null;
        }
    }



    //********************************************************************************
    //                              ExecuteNonQuery
    //********************************************************************************
    public  Int64 ExecuteNonQuery(string command, SqlTransaction Tranx, string ConStr = "DefaultConStr", bool DefaultCon = false)
    {
        SqlConnection SelCon = new SqlConnection();
        try
        {
            if (DefaultCon == true)
            {
                ConStr = DefaultSqlConStr;
            }
            
            SqlConnectionStringBuilder sBuild = new SqlConnectionStringBuilder(ConStr);

            for (int ConNum = 0; ConNum < TotalConStr; ConNum++)
            {
                if (PSqlConHd[ConNum] != null)
                {
                    if (PSqlConHd[ConNum].DataSource.ToString() == sBuild.DataSource.ToString() && PSqlConHd[ConNum].Database.ToString() == sBuild.InitialCatalog.ToString())
                    {
                        SelCon = PSqlConHd[ConNum];
                        break;

                    }
                }

            }
            if (SelCon == null)
            {
                return -1;
            }

            if (SelCon.State == ConnectionState.Closed)
            {
                return -1;
            }

          
            SqlCommand SqlCom = new SqlCommand(command, SelCon);
            SqlCom.Transaction = Tranx;

            Int64 num = new Int64();

            num = SqlCom.ExecuteNonQuery();

            return num;
        }
        catch (Exception Exp)
        {
            if (Tranx != null)
            {
                Tranx.Rollback();
            }
            if (SelCon.State == ConnectionState.Open)
            {
                ExecuteNonQuery(@"IF @@TRANCOUNT > 0 
                                    ROLLBACK TRAN", null);
            }
            SavErrLog(Exp.ToString(), Exp);
            return -1;
        }
    }

    //********************************************************************************
    //                               ExecuteScalar
    //********************************************************************************
    public  string ExecuteScalar(string command, SqlTransaction Tranx, string ConStr = "DefaultConStr")
    {
        SqlConnection SelCon = new SqlConnection();
        try
        {
            if (ConStr == "DefaultConStr")
            {
                ConStr = DefaultSqlConStr;
            }

            SqlConnectionStringBuilder sBuild = new SqlConnectionStringBuilder(ConStr);

            for (int ConNum = 0; ConNum < TotalConStr; ConNum++)
            {
                if (PSqlConHd[ConNum] != null)
                {
                    if (PSqlConHd[ConNum].DataSource.ToString() == sBuild.DataSource.ToString() || PSqlConHd[ConNum].Database.ToString() == sBuild.InitialCatalog.ToString())
                    {
                        SelCon = PSqlConHd[ConNum];
                        break;
                    }
                }

            }
            if (SelCon == null)
            {
                return "-1";
            }

            if (SelCon.State == ConnectionState.Closed)
            {
                return "-1";
            }


            string returnValue;

            SqlCommand SqlCom = new SqlCommand(command, SelCon);
            SqlCom.Transaction = Tranx;

            returnValue = SqlCom.ExecuteScalar().ToString();

            return returnValue;
        }
        catch (Exception Exp)
        {
            if (Tranx != null)
            {
                Tranx.Rollback();
            }
            if (SelCon.State == ConnectionState.Open)
            {
                ExecuteNonQuery(@"IF @@TRANCOUNT > 0 
                                    ROLLBACK TRAN", null);
            }
            SavErrLog(Exp.ToString(),Exp);
            return "-1";
        }
    }

    public  byte[] SetApprole(SqlConnection connection, string approle, string approlePassword)
    {
        StringBuilder sqlText = new StringBuilder();

        sqlText.Append("DECLARE @cookie varbinary(8000);");
        sqlText.Append("exec sp_setapprole @rolename = '" + approle + "', @password = '" + approlePassword + "'");
        sqlText.Append(",@fCreateCookie = true, @cookie = @cookie OUTPUT;");
        sqlText.Append(" SELECT @cookie");

        if (connection.State.Equals(ConnectionState.Closed))
            connection.Open();

        using (SqlCommand cmd = connection.CreateCommand())
        {
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sqlText.ToString();
            return (byte[])cmd.ExecuteScalar();
        }
    }

    public  void UnsetApprole(SqlConnection connection, byte[] approleCookie)
    {
        //string sqlText = "exec sp_unsetapprole @cookie=@approleCookie";

        if (connection.State.Equals(ConnectionState.Closed))
            connection.Open();

        using (SqlCommand cmd = connection.CreateCommand())
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_unsetapprole";
            cmd.Parameters.AddWithValue("@cookie", approleCookie);
            cmd.ExecuteNonQuery();
        }
    }


    //********************************************************************************
    //                              ExecuteDSQuery
    //********************************************************************************
    public  DataSet ExecuteDSQuery(string Command, SqlTransaction Tranx, string ConStr = "DefaultConStr")
    {
        SqlConnection SelCon = new SqlConnection();
        try
        {
            if (ConStr == "DefaultConStr")
            {
                ConStr = DefaultSqlConStr;
            }
            
            SqlConnectionStringBuilder sBuild = new SqlConnectionStringBuilder(ConStr);

            for (int ConNum = 0; ConNum < TotalConStr; ConNum++)
            {
                if (PSqlConHd[ConNum] != null)
                {
                    if (PSqlConHd[ConNum].DataSource.ToString() == sBuild.DataSource.ToString() || PSqlConHd[ConNum].Database.ToString() == sBuild.InitialCatalog.ToString())
                    {
                        SelCon = PSqlConHd[ConNum];
                        break;
                    }
                }

            }
            if (SelCon == null)
            {
                return null;
            }

            if (SelCon.State == ConnectionState.Closed)
            {
                return null;
            }

            SqlCommand Selectcommand = new SqlCommand(Command, SelCon);
            DataSet Ds = new DataSet();
            SqlDataAdapter DataAd = new SqlDataAdapter();
            Selectcommand.Transaction = Tranx;
            DataAd.SelectCommand = Selectcommand;
            DataAd.Fill(Ds);
            if ((Ds == null) && (Ds.Tables.Count > 0))
            {
                return null;
            }
            else
            {
                return Ds;
            }
        }
        catch(Exception Exp)
        {
            if (Tranx != null)
            {
                Tranx.Rollback();
            }
            if (SelCon.State == ConnectionState.Open)
            {
                ExecuteNonQuery(@"IF @@TRANCOUNT > 0 
                                    ROLLBACK TRAN", null);
            }
            SavErrLog(Exp.ToString(),Exp);
            return null;
        }
        
    }

    //********************************************************************************
    //                               Sql String with BeginTran
    //********************************************************************************
    public  StringBuilder SqlQueryWithBeginTran(string SqlQuery, string RollBackMsg, string CommitMsg)
    {
        StringBuilder StrBui = new StringBuilder();
        StrBui.Append("BEGIN TRANSACTION ");
        StrBui.Append(SqlQuery);
        StrBui.Append(@" if @@Error>0 
                            Begin 
                                Rollback Tran
                                Select '" + RollBackMsg + @"'
                            end
                            else
                            Begin 
                                commit tran
                                select '" + CommitMsg + @"'
                            end");

        return StrBui;

    }


    //********************************************************************************
    //                               Export Data
    //********************************************************************************
    public  void ExportData(DataSet dsExport,string FileName="")
    {
        _ExportData expForm = new _ExportData();
        expForm.FileName = FileName;
        expForm.dsExport = dsExport;
        expForm.ShowDialog();
        expForm.Dispose();
    }

    public enum CollapseDirection
    {
        PromtInformation,
        AskOpenOrNot,
        None
    }

    public enum GridID
    {
        Name,
        HeaderText
    }


    //********************************************************************************
    //                               Export Data DataSet to Excel
    //********************************************************************************
    public  void DataSetToExcel(DataSet dsExport, string FilePath, CollapseDirection _direction = CollapseDirection.AskOpenOrNot)
    {
        Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
        excelApp.Application.Workbooks.Add(Type.Missing);

        for(int i = excelApp.Sheets.Count;i>1;i--) //remove and leave 1 sheet
        {
            excelApp.Sheets[i].Delete();
        }

        int shtNo = 1;

        foreach (DataTable table in dsExport.Tables)
        {
            Excel.Worksheet excelWorkSheet;
            //if (excelApp.Sheets[shtNo] != null)
            //{
            //    excelWorkSheet = excelApp.Sheets[shtNo];
            //}
            //else
            //{
            //    excelWorkSheet = excelApp.Sheets.Add();
            //}
            try
            {
                excelWorkSheet = excelApp.Sheets[shtNo]; //if sheet already exists
            }
            catch
            {
                excelWorkSheet = excelApp.Sheets.Add(); //if sheet not exists
            }
            excelWorkSheet.Name = table.TableName;
            shtNo = shtNo + 1;

            for (int i = 1; i < table.Columns.Count + 1; i++)
            {
                excelWorkSheet.Cells[1, i] = table.Columns[i - 1].ColumnName;
            }

            for (int j = 0; j < table.Rows.Count; j++)
            {
                for (int k = 0; k < table.Columns.Count; k++)
                {
                    excelWorkSheet.Cells[j + 2, k + 1] = table.Rows[j].ItemArray[k].ToString();
                }
            }
        }
        //excelWorkBook.Save();
        //excelWorkBook.Close();
        excelApp.ActiveWorkbook.SaveCopyAs(FilePath);
        excelApp.ActiveWorkbook.Saved = true;
        excelApp.ActiveWorkbook.Close();
        excelApp.Quit();
        System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);

        if(_direction == CollapseDirection.None)
        {
            return;
        }
        else if(_direction == CollapseDirection.AskOpenOrNot)
        {
            if (DialogResult.Yes == MessageBox.Show("Your excel file exported successfully at " + FilePath + Environment.NewLine + "Do you wont to open file?", "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.YesNo))
            {
                if (System.IO.File.Exists(FilePath))
                {
                    System.Diagnostics.Process.Start(FilePath);
                    return;
                }
            }
        }
        else
        {
            MessageBox.Show("Your excel file exported successfully at " + FilePath + Environment.NewLine, "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.OK);
        }
        

    }



    //********************************************************************************
    //                               Export Data DataTable to Text
    //********************************************************************************
    public  void DataTableToTXT(DataTable dtExport, string FilePath, CollapseDirection _direction = CollapseDirection.AskOpenOrNot)
    {

        StringBuilder fileContent = new StringBuilder();

        foreach (var col in dtExport.Columns)
        {
            fileContent.Append(col.ToString() + ",");
        }

        fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);



        foreach (DataRow dr in dtExport.Rows)
        {

            foreach (var column in dr.ItemArray)
            {
                fileContent.Append(column.ToString() + ",");
            }

            fileContent.Replace(",", "\r\n", fileContent.Length - 1, 1);
        }

        System.IO.File.WriteAllText(FilePath, fileContent.ToString());

        if (_direction == CollapseDirection.None)
        {
            return;
        }
        else if (_direction == CollapseDirection.AskOpenOrNot)
        {
            if (DialogResult.Yes == MessageBox.Show("Your text file exported successfully at " + FilePath + Environment.NewLine + "Do you wont to open file?", "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.YesNo))
            {
                if (System.IO.File.Exists(FilePath))
                {
                    System.Diagnostics.Process.Start(FilePath);
                    return;
                }
            }
        }
        else
        {
            MessageBox.Show("Your excel file exported successfully at " + FilePath + Environment.NewLine, "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.OK);
        }
    }

    //********************************************************************************
    //                               Export Data Datatable to DBF
    //********************************************************************************
    public  void DataTableToDBF(DataTable dataFromSQL, string FilePath, CollapseDirection _direction = CollapseDirection.PromtInformation)
    {
        DBFExport DBFExport = new DBFExport();
        DBFExport.DataSource = Spire.DataExport.Common.ExportSource.DataTable;
        DBFExport.DataTable = dataFromSQL;
        DBFExport.ActionAfterExport = Spire.DataExport.Common.ActionType.None;
        DBFExport.FileName = FilePath;
        DBFExport.SaveToFile();
        if (_direction == CollapseDirection.None)
        {
            return;
        }
        else if (_direction == CollapseDirection.AskOpenOrNot)
        {
            if (DialogResult.Yes == MessageBox.Show("Your text file exported successfully at " + FilePath + Environment.NewLine + "Do you wont to open file?", "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.YesNo))
            {
                if (System.IO.File.Exists(FilePath))
                {
                    System.Diagnostics.Process.Start(FilePath);
                    return;
                }
            }
        }
        else
        {
            MessageBox.Show("Your excel file exported successfully at " + FilePath + Environment.NewLine, "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.OK);
        }

    }
    //********************************************************************************
    //                               Export Data Datatable to PDF
    //********************************************************************************
    public  void DataTableToPDF(DataTable dtExport, string FilePath, bool Landscape, CollapseDirection _direction = CollapseDirection.AskOpenOrNot)
    {
        PDFExport DBFExport = new PDFExport();
        DBFExport.DataSource = Spire.DataExport.Common.ExportSource.DataTable;
        DBFExport.DataTable = dtExport;
        if (Landscape == true)
        {
            DBFExport.PDFOptions.PageOptions.Orientation = Spire.DataExport.Common.PageOrientation.Landscape;
        }
        DBFExport.ActionAfterExport = Spire.DataExport.Common.ActionType.None;
        DBFExport.FileName = FilePath;
        DBFExport.SaveToFile();
        if (_direction == CollapseDirection.None)
        {
            return;
        }
        else if (_direction == CollapseDirection.AskOpenOrNot)
        {
            if (DialogResult.Yes == MessageBox.Show("Your text file exported successfully at " + FilePath + Environment.NewLine + "Do you wont to open file?", "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.YesNo))
            {
                if (System.IO.File.Exists(FilePath))
                {
                    System.Diagnostics.Process.Start(FilePath);
                    return;
                }
            }
        }
        else
        {
            MessageBox.Show("Your excel file exported successfully at " + FilePath + Environment.NewLine, "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.OK);
        }
    }

    //********************************************************************************
    //                               Export Data Datatable to Word
    //********************************************************************************
    public  void DataTableToMsWord(DataTable dtExport, string FilePath, CollapseDirection _direction = CollapseDirection.AskOpenOrNot)
    {
        RTFExport RTFExport = new RTFExport();
        RTFExport.DataSource = Spire.DataExport.Common.ExportSource.DataTable;
        RTFExport.DataTable = dtExport;
        RTFExport.ActionAfterExport = Spire.DataExport.Common.ActionType.None;
        RTFExport.FileName = FilePath;
        RTFExport.SaveToFile();
        if (_direction == CollapseDirection.None)
        {
            return;
        }
        else if (_direction == CollapseDirection.AskOpenOrNot)
        {
            if (DialogResult.Yes == MessageBox.Show("Your text file exported successfully at " + FilePath + Environment.NewLine + "Do you wont to open file?", "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.YesNo))
            {
                if (System.IO.File.Exists(FilePath))
                {
                    System.Diagnostics.Process.Start(FilePath);
                    return;
                }
            }
        }
        else
        {
            MessageBox.Show("Your excel file exported successfully at " + FilePath + Environment.NewLine, "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.OK);
        }
    }

    //********************************************************************************
    //                               Export Data Datatable to Access
    //********************************************************************************
    public  void DataTableToMsAccess(DataTable dtExport, string FilePath)
    {
        AccessExport accessExport = new AccessExport();
        accessExport.DataSource = Spire.DataExport.Common.ExportSource.DataTable;
        accessExport.DataTable = dtExport;
        accessExport.DatabaseName = FilePath;
        accessExport.SaveToFile();
    }

    //********************************************************************************
    //                               Export Data Datatable to HTML
    //********************************************************************************
    public  void DataTableToHTML(DataTable dtExport, string FilePath, CollapseDirection _direction = CollapseDirection.AskOpenOrNot)
    {
        HTMLExport HtmlExport = new HTMLExport();
        HtmlExport.DataSource = Spire.DataExport.Common.ExportSource.DataTable;
        HtmlExport.DataTable = dtExport;
        HtmlExport.ActionAfterExport = Spire.DataExport.Common.ActionType.None;
        HtmlExport.FileName = FilePath;
        HtmlExport.SaveToFile();
        if (_direction == CollapseDirection.None)
        {
            return;
        }
        else if (_direction == CollapseDirection.AskOpenOrNot)
        {
            if (DialogResult.Yes == MessageBox.Show("Your text file exported successfully at " + FilePath + Environment.NewLine + "Do you wont to open file?", "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.YesNo))
            {
                if (System.IO.File.Exists(FilePath))
                {
                    System.Diagnostics.Process.Start(FilePath);
                    return;
                }
            }
        }
        else
        {
            MessageBox.Show("Your excel file exported successfully at " + FilePath + Environment.NewLine, "Export Data-" + DateTime.Now.ToString(), MessageBoxButtons.OK);
        }
    }


    public  string GetCtlNum(string lcComCod, string lcPeriod, string lcCtlFld)
    {
        DataTable tCtl = new DataTable();
        tCtl = ExecuteDTQuery(@"Select " + lcCtlFld.Trim() + " as CurNum from Ctl with (ROWLOCK,UPDLOCK) Where ComCod='" + lcComCod.Trim() + "' and Period='" + lcPeriod.Trim() + "'", null);
        if (tCtl.Rows.Count < 0)
        {
            MessageBox.Show("Failed to Lock CTL table!", "GetCtlNum Failed!-" + DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return "";
        }
        else
        {
            Int64 nResult = ExecuteNonQuery("Update Ctl Set " + lcCtlFld.Trim() + "=" + lcCtlFld.Trim() + "+1 Where ComCod='" + lcComCod.Trim() + "' and Period='" + lcPeriod.Trim() + "'", null);
            if (nResult <= 0)
            {
                MessageBox.Show("Unable to update CTL table!", "GetCtlNum Failed!-" + DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
            else
            {
                string YeaCod, MonCod;
                if (Convert.ToInt64(lcPeriod.Trim().Substring(2, 2)) > 9)
                {
                    YeaCod = Convert.ToChar(Convert.ToInt64(lcPeriod.Trim().Substring(2, 2)) + 55).ToString();
                }
                else
                {
                    YeaCod = lcPeriod.Trim().Substring(3, 1).ToString();
                }
                MonCod = Convert.ToChar(Convert.ToInt64(lcPeriod.Trim().Substring(4, 2)) + 64).ToString();

                // Moved these out from CASE by CANICE 20150803
                string p = lcComCod.ToString();
                string Y = YeaCod;
                string m = MonCod;
                string c = gVar.getProperty("Comcod");
                // End Moved CANICE

                switch (lcCtlFld.Trim())
                {
                    case "SINUM":
                        return "P" + YeaCod + MonCod + tCtl.Rows[0]["CurNum"].ToString().PadLeft(4, '0');
                    //break;
                    case "SHPNUM":
                        return gVar.getProperty("sys_ShpPre") + YeaCod + MonCod + tCtl.Rows[0]["CurNum"].ToString().PadLeft(4, '0');
                    //break;
                    default:
                        break;
                }
            }
        }

        return "";
    }

    public  bool SavErrLog(string log,Exception ex = null)
    {
        if(ex == null)
        {
            ex = new Exception(log);
        }
        //string ErrPth = ErrPath + "\\ERR_" + DateTime.Now.ToString("ddMMyy_hhmmss") + ".txt";
        string info = DateTime.Now + Environment.NewLine + System.Environment.MachineName + Environment.NewLine
            + Environment.NewLine + ex.Message;
        MessageBox.Show(info, "Error Occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
        bool val = false;
        Int64 nrslt;

        string err = ex.ToString();
        err = err.Replace("\'", "\"");
        try
        {
            nrslt = ExecuteNonQuery(@"INSERT INTO ErrLog (ComCod,Description, Station,userid,DateOccured) VALUES
                              (" + gVar.getProperty("Comcod") + ",'" + err + "','" + System.Environment.MachineName + "','" + gVar.getProperty("sys_usr") + "',getdate())", null);
            if (nrslt <= 0)
            {
                MessageBox.Show("Unable to insert into ErrLog table!", "ErrLog Failed!-" + DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                val = true;
                //MessageBox.Show("Error found! For the detail please check error log", "SYSTEM-" + DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        catch (Exception ex2)
        {
            MessageBox.Show(ex2.ToString(), "Error Found-" + DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return val;
    }

    public  bool StringToText(string str,string FileName, string Path = @"C:\Expana\")
    {
        try
        {
            System.IO.File.WriteAllText(Path + FileName + ".TXT", str);
            return true;
        }
        catch { return false; }
        
    }

    //public  bool SavErrLog(string log)
    //{
    //    log = log.Replace("'", "''");
    //    string ErrPth = ErrPath + "\\ERR_" + DateTime.Now.ToString("ddMMyy_hhmmss") + ".txt";
    //    string info = DateTime.Now + Environment.NewLine + System.Environment.MachineName + Environment.NewLine + Environment.NewLine + log;
    //    try
    //    {
    //        System.IO.File.WriteAllText(ErrPth, info);
    //        //MessageBox.Show(info, "Error Detail - " + DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show(ex.ToString() + Environment.NewLine + Environment.NewLine + info, "ErrLog Failed (txt)!-" + DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    }
    //    bool val = false;
    //    Int64 nrslt;
    //    try
    //    {
    //        nrslt = ExecuteNonQuery(@"INSERT INTO ErrLog (ComCod,Description,UserId,DateOccured, Station) VALUES
    //                          (" + gVar.getProperty("Comcod") + ",'" + log + "',SUSER_NAME(),GETDATE(),'" + System.Environment.MachineName + "')", null);
    //        if (nrslt <= 0)
    //        {
    //            MessageBox.Show("Unable to insert into ErrLog table!", "ErrLog Failed!-" + DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
    //        }
    //        else
    //        {
    //            val = true;
    //            MessageBox.Show("Error found! For the detail please check error log", "SYSTEM-" + DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
    //        }

    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show(ex.ToString(), "Error Found-" + DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    }
    //    return val;
    //}

    public  string CharIncreament(string Str)
    {
        string Rsl = "";
        if (Str == "")
        {
            Rsl = "A";
        }
        else if (Str.Length > 1)
        {
            Rsl = Str.Substring(0, Str.Length - 1) + Convert.ToChar(Convert.ToUInt16(Convert.ToChar(Str.Substring(Str.Length - 1, 1))) + 1).ToString();
        }
        else
        {
            Rsl = Convert.ToChar(Convert.ToUInt16(Convert.ToChar(Str)) + 1).ToString();
        }

        return Rsl;

    }

    public  DataTable TableToQRCode(DataTable dtQR, List<string> ColNam)
    {
        DataTable dtTmp = new DataTable();
        dtTmp = dtQR.Copy();

        foreach (DataColumn dc in dtQR.Columns)
        {
            if (ColNam.Contains(dc.ColumnName.ToString()))
            {
                dtTmp.Columns.Add(dc.ColumnName.ToString() + "_imgQR", typeof(System.Byte[]));
                foreach (DataRow dr in dtTmp.Rows)
                {
                    dr[dc.ColumnName.ToString() + "_imgQR"] = StringToQRtoByte(dr[dc.ColumnName.ToString()].ToString());
                }
                dtTmp.Columns.Remove(dc.ColumnName.ToString());
                dtTmp.Columns[dc.ColumnName.ToString() + "_imgQR"].ColumnName = dc.ColumnName.ToString();
            }
        }

        return dtTmp;
    }

    //public  DataTable TableToEAN(DataTable dtEAN, List<string> CC, List<string> MC, List<string> PC, List<string> CD)
    public  DataTable TableToEAN(DataTable dtEAN, List<string> ColNam, float EANscale, int flipDegree)
    {
        DataTable dtTmp = new DataTable();
        dtTmp = dtEAN.Copy();

            foreach (DataColumn dc in dtEAN.Columns)
            {
                if (ColNam.Contains(dc.ColumnName.ToString()))
                {
                    dtTmp.Columns.Add(dc.ColumnName.ToString() + "_imgEAN", typeof(System.Byte[]));
                    foreach (DataRow dr in dtTmp.Rows)
                    {
                        dr[dc.ColumnName.ToString() + "_imgEAN"] = SysUtl.StringToEANtoByte(dr[dc.ColumnName.ToString()].ToString(), EANscale, flipDegree);
                        //byte[] imgbyte = new byte[166518];
                        //dr[dc.ColumnName.ToString() + "_imgEAN"] = imgbyte;

                    }
                    dtTmp.Columns.Remove(dc.ColumnName.ToString());
                    dtTmp.Columns[dc.ColumnName.ToString() + "_imgEAN"].ColumnName = dc.ColumnName.ToString();
                }
            }

        return dtTmp;
    }

    public  byte[] StringToQRtoByte(string str)
    {
        QrCodeControl QRC = new QrCodeControl();
        QRC.AutoSize = true;
        QRC.Text = str;

        Bitmap bitmap = new Bitmap(QRC.Size.Width, QRC.Size.Height);
        QRC.DrawToBitmap(bitmap, new System.Drawing.Rectangle(new Point(0, 0), bitmap.Size));

        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        bitmap.Save(ms, ImageFormat.Bmp);
        byte[] imgbyte = ms.ToArray();
        //Gaby 20180416: To Fix Out of Memory Issue
        ms.Flush();
        ms.Close();
        //Gaby 20180416: To Fix Out of Memory Issue
        return imgbyte;
    }

    public  byte[] StringToEANtoByte(string EAN, float EANscale, int flipDegree)
    {
        //gaby
        string CountryCode, ManufacturerCode, ProductCode, ChecksumDigit;
        CountryCode = EAN.ToString().Substring(0,2);
        ManufacturerCode = EAN.ToString().Substring(2,5);
        ProductCode = EAN.ToString().Substring(7,5);
        ChecksumDigit = EAN.ToString().Substring(12,1);

        //CountryCode = "49";
        //ManufacturerCode = "69363";
        //ProductCode = "33726";
        //ChecksumDigit = "9";

        Ean13 ean13 = new Ean13();
        ean13.CountryCode = CountryCode;
        ean13.ManufacturerCode = ManufacturerCode;
        ean13.ProductCode = ProductCode;
        if (ChecksumDigit.Length > 0)
            ean13.ChecksumDigit = ChecksumDigit;
        ean13.Scale = (float)Convert.ToDecimal(EANscale);

        System.Drawing.Bitmap bmp = ean13.CreateBitmap();
        if (flipDegree == 90)
        {
            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }
        else if (flipDegree == 180)
        {
            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
        }
        else if (flipDegree == 270)
        {
            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
        }
        //bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
        int a = bmp.Width;
        int b = bmp.Height;
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        bmp.Save(ms, ImageFormat.Bmp);
        byte[] imgbyte = ms.ToArray();
        // Gaby 20180416 : To Fix Out of Memory Issue -- Start
        ms.Flush(); 
        ms.Close();
        // Gaby 20180416 : To Fix Out of Memory Issue -- End
        return imgbyte;
    }

    



    public  Int64 DataTableToSQLTable(DataTable dt, string SqlTableDestination, bool ClearSqlDestinationTable,string ConStr = "DefaultConStr")
    {
        try
        {

            if (ConStr == "DefaultConStr")
            {
                ConStr = DefaultSqlConStr;
            }
            SqlConnection SelCon = new SqlConnection();
            for (int ConNum = 0; ConNum < TotalConStr; ConNum++)
            {
                if (PSqlConHd[ConNum] != null)
                {
                    if (PSqlConHd[ConNum].ConnectionString == ConStr)
                    {
                        SelCon = PSqlConHd[ConNum];
                        break;
                    }
                }

            }
            if (SelCon == null)
            {
                return -1;
            }

            if (SelCon.State == ConnectionState.Closed)
            {
                return -1;
            }

            if (ClearSqlDestinationTable == true)
            {
                Int64 n = ExecuteNonQuery("DELETE FROM " + SqlTableDestination, null);
                if (n < 0)
                {
                    return -1;
                }
            }

            SqlBulkCopy sbc = new SqlBulkCopy(SelCon);
            sbc.DestinationTableName = SqlTableDestination;
            sbc.WriteToServer(dt);
            return 1;
        }
        catch (Exception ex)
        {
            SavErrLog(ex.ToString(),ex);
            return -1;
        }
    }

    public  string BaseFormVersion()
    {
        string Ver = "";
        Assembly currentAssem = typeof(SysUtl).Assembly;
        object[] attribs = currentAssem.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
        if (attribs.Length > 0)
        {
            Ver = ((AssemblyFileVersionAttribute)attribs[0]).Version.ToString();
        }
        return Ver;
    }


    //********************************************************************************
    //              Convert Datagridview to datatable (not datasource)
    //********************************************************************************
    public  DataTable DatagridviewToDataTable(DataGridView dgv,GridID DataGridviewID = GridID.Name)
    {
        DataTable dt = new DataTable();
        foreach (DataGridViewColumn col in dgv.Columns)
        {
            if(DataGridviewID== GridID.Name)
                dt.Columns.Add(col.Name);
            else
                dt.Columns.Add(col.HeaderText.Trim());
        }

        foreach (DataGridViewRow row in dgv.Rows)
        {
            DataRow dRow = dt.NewRow();
            foreach (DataGridViewCell cell in row.Cells)
            {
                dRow[cell.ColumnIndex] = cell.Value;
            }
            dt.Rows.Add(dRow);
        }

        return dt;
    }

    public  System.Drawing.Image StringToEAN13(string CountryCode,string ManufacturerCode,string ProductCode,string ChecksumDigit)
    {
        Ean13 ean13 = new Ean13();
        ean13.CountryCode = CountryCode;
        ean13.ManufacturerCode = ManufacturerCode;
        ean13.ProductCode = ProductCode;
        if (ChecksumDigit.Length > 0)
            ean13.ChecksumDigit = ChecksumDigit;
        ean13.Scale = (float)Convert.ToDecimal(1);

        System.Drawing.Bitmap bmp = ean13.CreateBitmap();
        return bmp;


    }

}

