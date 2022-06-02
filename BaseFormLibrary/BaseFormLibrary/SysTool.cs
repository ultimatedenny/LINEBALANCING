using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using Ionic.Zip;

namespace BaseFormLibrary
{
    public class SysTool
    {

        public static SqlConnection SqlConHd = new SqlConnection();
        public static bool SQLCon(string ConStr)
        {
            SqlConHd.ConnectionString = ConStr;
            try
            {
                SqlConHd.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SQLCloseCon()
        {
            SqlConnection.ClearPool(SysTool.SqlConHd);
            SysTool.SqlConHd.Dispose();
            SysTool.SqlConHd.Close();
        }

        //***************************************************************************
        //                                  ExecuteDTQuery
        //***************************************************************************
        public static DataTable ExecuteDTQuery(String StrCon, String SqlCmd, SqlTransaction Tranx)
        {
            try
            {

                SQLCon(StrCon);

                SqlCommand Selectcommand = new SqlCommand(SqlCmd, SqlConHd);
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
            catch
            {
                return null;
            }
            finally
            {
                SQLCloseCon();
            }
        }

        //***************************************************************************
        //                                  ExecuteDSQuery
        //***************************************************************************
        public static DataSet ExecuteDSQuery(String StrCon, String SqlCmd, SqlTransaction Tranx)
        {
            try
            {

                SQLCon(StrCon);

                SqlCommand Selectcommand = new SqlCommand(SqlCmd, SqlConHd);
                DataSet ds = new DataSet();
                SqlDataAdapter DataAd = new SqlDataAdapter();
                DataAd.SelectCommand = Selectcommand;
                if (Tranx != null)
                {
                    DataAd.SelectCommand.Transaction = Tranx;
                }
                DataAd.Fill(ds);
                if ((ds != null) && (ds.Tables.Count > 0))
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                SQLCloseCon();
            }
        }

        //********************************************************************************
        //                              ExecuteNonQuery
        //********************************************************************************
        public static Int64 ExecuteNonQuery(String StrCon, String SqlCmd, SqlTransaction Tranx)
        {
            try
            {
                SQLCon(StrCon);
                if (SysTool.SqlConHd.State == ConnectionState.Closed)
                {
                    return -1;
                }
                SqlCommand SqlCom = new SqlCommand(SqlCmd, SysTool.SqlConHd);
                SqlCom.Transaction = Tranx;

                Int64 num = new Int64();

                num = SqlCom.ExecuteNonQuery();

                return num;
            }
            catch (Exception Exp)
            {
                Exception MyExp = Exp;
                return -1;
            }
            finally
            {
                SQLCloseCon();
            }
        }

        /// <param name="StrCon">Connection String</param>
        /// <param name="SqlCmd">SQL Command</param>
        /// <param name="Tranx">Tranx</param>
        public static string ExecuteScalar(String StrCon, String SqlCmd, SqlTransaction Tranx)
        {
            try
            {
                SQLCon(StrCon);
                if (SysTool.SqlConHd.State == ConnectionState.Closed)
                {
                    return "-1";
                }
                string returnValue;

                SqlCommand SqlCom = new SqlCommand(SqlCmd, SysTool.SqlConHd);
                SqlCom.Transaction = Tranx;
                returnValue = SqlCom.ExecuteScalar().ToString();

                return returnValue;
            }
            catch
            {
                return "-1";
            }
            finally
            {
                SQLCloseCon();
            }
        }

        /// <param name="sourcePath">Example C:\EXPANA</param>
        /// <param name="destinationPath">Example C:\EXPANA</param>
        /// <param name="OutputFileName">Example ZipOutput</param>
        public static void FolderToZipFile(string sourcePath, string destinationPath,string OutputFileName) 
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(@sourcePath);
                zip.Save(destinationPath + @"\" + OutputFileName + ".zip");
            }
        }

        public static void FileToZipFile(List<string> ListsourcePath, string destinationPath, string OutputFileName)
        {
            using (ZipFile zip = new ZipFile())
            {
                foreach (string sourcePath in ListsourcePath)
                {
                    zip.AddFile(sourcePath);
                }
                zip.Save(destinationPath + @"\" + OutputFileName + ".zip");
            }
        }

    }
}
