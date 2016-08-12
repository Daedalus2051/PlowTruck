using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

        #region Methods
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
                string rowAction = "", extFolder ="";
                if(conf.ExtensionLookup.TryGetValue(fInfo.Extension, out extFolder))
                {
                    // We made a match, now populate the data and add it to the dataset
                    conf.ExtensionAction.TryGetValue(fInfo.Extension, out rowAction);
                    ScanResults.AddRow(fInfo.FullName, fInfo.Extension, rowAction, extFolder, true);
                }
                else
                {
                    // Add to unmatched list
                    conf.ExtensionAction.TryGetValue(fInfo.Extension, out rowAction);
                    ScanResults.AddRow(fInfo.FullName, fInfo.Extension, rowAction, extFolder, false);
                }
            }
        }

        // Plow (scan first)
        public void Plow(string PlowPath)
        {

        }

        // Plow (plow based on scan results)
        public void Plow(Results FolderScanResults)
        {

        }

        private void EmployAction(string FilePath, string Action, string ActionValue)
        {
            switch(Action.ToLower())
            {
                case "move":
                    try
                    {
                        File.Move(FilePath, ActionValue); // Might need to add logic to get the path to the file that we're plowing
                    }
                    catch(Exception MoveErr)
                    {
                        // TODO: Log error?
                    }
                    break;

                case "delete":
                    try
                    {
                        File.Delete(FilePath);
                    }
                    catch(Exception DeleteErr)
                    {
                        // TODO: Log error?
                    }
                    break;

                case "exclude":
                    Console.WriteLine($"Excluding file {FilePath}");
                    return;

                case "archive":
                    // Search for a 7-zip executable
                    if (File.Exists(@"C:\Program Files\7-Zip\7z.exe"))
                    {
                        // If we found the 7-zip executable then use it
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.CreateNoWindow = true;
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.FileName = @"C:\Program Files\7-Zip\7z.exe";
                        startInfo.Arguments = "a " + (ActionValue + ".7z") + " " + FilePath;
                        try
                        {
                            using (Process proc_7z = Process.Start(startInfo))
                            {
                                proc_7z.WaitForExit();
                            }
                        }
                        catch (Exception err)
                        {
                            // TODO: Log error?
                            /*
                            plowLog.WriteLog(LogWriter.LOG_TYPE.ERROR, "Error in archive step: " + err.Message,
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);*/
                        }
                    }
                    else
                    {
                        // TODO: Implement the .NET archive default
                        Console.WriteLine("7-zip not found in Program Files and Dot Net archive function not yet implemented.");
                    }
                    break;

                default:
                    throw new Exception($"Action {Action} is unknown or not supported.");
            }
        }
        #endregion

        #region Classes
        public class Results
        {
            public DataSet ResultSet { get; set; }
            private DataTable _table = new DataTable("Results");
            private DataColumn[] _columns = new DataColumn[5];
            public Results()
            {
                ResultSet.Tables.Add(_table);
                _columns[0] = new DataColumn("File");
                _columns[1] = new DataColumn("Extension");
                _columns[2] = new DataColumn("Action");
                _columns[3] = new DataColumn("ActionValue");
                _columns[4] = new DataColumn("Matched", typeof(bool));

                foreach(DataColumn col in _columns)
                {
                    ResultSet.Tables[0].Columns.Add(col);
                }
            }

            public bool AddRow(string FileName, string Extension, string Action, string Folder, bool Matched)
            {
                try
                {
                    DataRow row = ResultSet.Tables[0].NewRow();
                    row[0] = FileName;
                    row[1] = Extension;
                    row[2] = Action;
                    row[3] = Folder;
                    row[4] = Matched;
                    ResultSet.Tables[0].Rows.Add(row);
                    return true;
                }
                catch (Exception AddEx)
                {
                    // TODO: Log the exception
                    Console.WriteLine($"Exception adding row: {AddEx.Message}");
                    return false;
                }
            }
        }
        #endregion
    }
}