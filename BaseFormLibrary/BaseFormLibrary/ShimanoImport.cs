using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Design;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Data.OleDb;

namespace BaseFormLibrary
{
    public partial class ShimanoImport : UserControl
    {

        public enum CollapseDirection
        {
            CSV,
            TXT,
            XLS,
            XLSX

        }

        public enum DelimiterOption
        {
            Comma,
            Tab
        }

        #region Properties

        [Category("Behavior"), DefaultValue(true)]
        [Description("Set to be true for auto tab to next tab index.")]
        public bool ShimanoAutoTabToNextIndex { get; set; }

        [Browsable(true), DefaultValue(DelimiterOption.Comma)]
        [ListBindable(true), Editor(typeof(ComboBox), typeof(UITypeEditor))]
        [Description("Delimiter for CSV and TXT only default comma")]
        private DelimiterOption _DO = DelimiterOption.Comma;
        public DelimiterOption ShimanoTextDelimiter
        {
            get { return _DO; }
            set
            {
                _DO = value;

            }
        }

        [Category("Behavior"), DefaultValue(false)]
        [Description("Set to be true for import including header or false for import exclude header")]
        public bool ShimanoImportWithHeader { get; set; }

        [Browsable(true), DefaultValue("CSV")]
        [ListBindable(true), Editor(typeof(ComboBox), typeof(UITypeEditor))]
        private CollapseDirection _direction = CollapseDirection.CSV;
        public CollapseDirection ShimanoFileType
        {
            get { return _direction; }
            set
            {
                _direction = value;

            }
        }

        #endregion



        public event EventHandler BrowseClick;

        public ShimanoImport()
        {
            InitializeComponent();
        }

        private void shimanoButton1_Click(object sender, EventArgs e)
        {
            OFD1.Filter = "Files |*." + ShimanoFileType.ToString();
            OFD1.Title = ShimanoFileType.ToString();
            OFD1.CheckPathExists = true;
            OFD1.ShowDialog();
            if (BrowseClick != null)
                BrowseClick(sender, e);
        }

        private void OFD1_FileOk(object sender, CancelEventArgs e)
        {
            string filePath = OFD1.FileName;
            //string extension = Path.GetExtension(filePath);
            shimanoTextBox1.Text = filePath;
        }

        public DataTable ImportToTable()
        {
            string delimiter = @",";
            if (this.ShimanoTextDelimiter == DelimiterOption.Comma)
                delimiter = @",";
            if (this.ShimanoTextDelimiter == DelimiterOption.Tab)
                delimiter = @"\t";

            DataTable dt = new DataTable();
            if (shimanoTextBox1.Text != string.Empty)
            {

                try
                {
                    if (ShimanoFileType.ToString() == "CSV" || ShimanoFileType.ToString() == "TXT")
                    {
                        using (TextFieldParser csvReader = new TextFieldParser(shimanoTextBox1.Text))
                        {
                            csvReader.SetDelimiters(new string[] { delimiter });
                            csvReader.HasFieldsEnclosedInQuotes = true;
                            //read column names
                            string[] colFields = csvReader.ReadFields();
                            foreach (string column in colFields)
                            {
                                DataColumn datecolumn = new DataColumn(column);
                                datecolumn.AllowDBNull = true;
                                dt.Columns.Add(datecolumn);
                            }
                            while (!csvReader.EndOfData)
                            {
                                string[] fieldData = csvReader.ReadFields();
                                //Making empty value as null
                                for (int i = 0; i < fieldData.Length; i++)
                                {
                                    if (fieldData[i] == "")
                                    {
                                        fieldData[i] = null;
                                    }
                                }
                                dt.Rows.Add(fieldData);
                            }
                        }

                        if (dt.Rows.Count > 0 && ShimanoImportWithHeader == false)
                        {
                            dt.Rows[0].Delete();
                        }

                    }
                    else if (ShimanoFileType.ToString() == "XLSX" || ShimanoFileType.ToString() == "XLS")
                    {
                        string hdr = "No";
                        if(ShimanoImportWithHeader == true)
                        {
                            hdr = "Yes";
                        }

                        string ConStr = null;
                        ConStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + shimanoTextBox1.Text + ";Extended Properties=\"Excel 12.0;HDR=" + hdr + ";IMEX=1\"";
                        // ConStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + shimanoTextBox1.Text + ";Extended Properties=\"Excel 12.0;HDR=No;IMEX=1";
                        OleDbConnection conn = new OleDbConnection(ConStr);
                        conn.Open();
                        DataTable dbSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                        if (dbSchema == null || dbSchema.Rows.Count < 1)
                        {
                            return null;
                        }
                        string firstSheetName = dbSchema.Rows[0]["TABLE_NAME"].ToString();
                        OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [" + firstSheetName + "]", conn);
                        adapter.Fill(dt);
                        conn.Close();
                        conn.Dispose();

                        //if (dt.Rows.Count > 0 && ShimanoImportWithHeader == false)
                        //{
                        //    dt.Rows[0].Delete();
                        //}
                        return dt;
                    }
                    else
                    {
                        MessageBox.Show("User Information Not Found!", "Login - " + DateTime.Now.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return dt;
                    }
                }
                catch (Exception ex)
                {
                    SysUtl.SavErrLog(ex.ToString(),ex);
                }
            }
            return dt;

        }

        public bool ShowImport(DataTable DTSource)
        {
            bool sts = false;
            _Import imp = new _Import();
            imp.DataTableSouce = DTSource;
            imp.ShowDialog();
            sts = imp.ImportStatus;
            imp.Dispose();
            return sts;
        }


    }
}
