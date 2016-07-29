using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlowTruck
{
    class Operations
    {
        Configuration config = new Configuration();
        Results ScanResults;

        #region Constructors
        public Operations(Configuration conf)
        {
            config = conf;
            ScanResults = new Results();
        }
        #endregion

        // Scan
        public void Scan(Configuration conf)
        {
            string[] dirFiles = Directory.GetFiles(conf.PlowPath);
            List<FileInfo> fInfoCol = new List<FileInfo>();
            
            foreach (string fPath in dirFiles)
            {
                fInfoCol.Add(new FileInfo(fPath));
            }

            foreach(FileInfo fInfo in fInfoCol)
            {
                string extFolder="";
                if(conf.ExtensionLookup.TryGetValue(fInfo.Extension, out extFolder))
                {
                    // We made a match, now populate the data and add it to the dataset
                    DataRow row; string rowAction = "";
                    row = ScanResults.ResultSet.Tables[0].NewRow();
                    row[0] = fInfo.FullName;
                    row[1] = fInfo.Extension;
                    conf.ExtensionAction.TryGetValue(fInfo.Extension, out rowAction);
                    row[2] = rowAction;
                    row[3] = extFolder;

                    ScanResults.ResultSet.Tables[0].Rows.Add(row);
                }
                else
                {
                    // Add to unmatched list?
                }
            }
        }

        // Plow (scan first)

        // Plow (plow based on scan results

        #region Classes
        public class Results
        {
            public DataSet ResultSet { get; set; }
            private DataTable _table = new DataTable("Results");
            private DataColumn[] _columns = new DataColumn[4];
            public Results()
            {
                ResultSet.Tables.Add(_table);
                _columns[0] = new DataColumn("File");
                _columns[1] = new DataColumn("Extension");
                _columns[2] = new DataColumn("Action");
                _columns[3] = new DataColumn("ActionValue");

                foreach(DataColumn col in _columns)
                {
                    ResultSet.Tables[0].Columns.Add(col);
                }
            }
        }
        #endregion
    }
}