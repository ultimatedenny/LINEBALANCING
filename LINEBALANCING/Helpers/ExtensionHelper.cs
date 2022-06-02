using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Web;

namespace LineBalancing.Helpers
{
    public static class ExtensionHelper
    {
        public static bool ValidateExcel(HttpPostedFileBase file)
        {
            bool isValid = false;

            if (Path.GetExtension(file.FileName).ToLower() == ".xls" || Path.GetExtension(file.FileName).ToLower() == ".xlsx")
            {
                isValid = true;
            }

            return isValid;
        }

        public static string GetExcelFilePath(HttpPostedFileBase file)
        {
            string filename = file.FileName;

            string connectionString = string.Empty;
            string excelFilePath = string.Empty;

            try
            {
                string targetpath = HttpContext.Current.Server.MapPath("~/UploadFile/");
                excelFilePath = targetpath + filename;

                // Save file
                file.SaveAs(excelFilePath);

                if (filename.EndsWith(".xls"))
                {
                    connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", excelFilePath);
                }
                else if (filename.EndsWith(".xlsx"))
                {
                    connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", excelFilePath);
                }

                OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
                DataSet dataSet = new DataSet();

                adapter.Fill(dataSet, "ExcelTable");

                DataTable dataTable = dataSet.Tables["ExcelTable"];
            }
            catch (Exception exception)
            {
                throw exception;
            }

            return excelFilePath;
        }
    }
}