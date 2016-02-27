using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using Logger;

namespace PlowTruck
{
    public class PlowTruckCore
    {
        #region Variables
        // PlowTruck() - These variables will more or less apply globally
        public string xPath
        {
            get { return xmlPath; }
        }
        private string xmlPath;
        private LogWriter plowLog;
        public bool VerboseLogging = true;
        // LoadExtensions()
        private XmlDocument xmlPlowExtensions;
        private string[] xmlExtensions;
        private string[] xmlFolderNames;
        private string[] xmlActions;
        // Scan()
        public string ScanDirectory { get; set; }
        public XmlDocument ScanResults { get { return scan_matched; } }
        //public XmlDocument UnmatchedResults { get { return scan_unmatched; } }
        private XmlDocument scan_matched;
        //private XmlDocument scan_unmatched; // Since we're returning standardized results there's no reason to have this
        // Plow()
        public string PlowDirectory { get; set; }
        #endregion

        #region Constructors
        public PlowTruckCore(string XMLPath)
        {
            if (!(File.Exists(XMLPath)))
                throw new FileNotFoundException();
            xmlPath = XMLPath;

            plowLog = new LogWriter(Environment.CurrentDirectory, "PlowTruck");
            // Initialize the XML document internally
            LoadExtensions();
        }
        #endregion

        #region Methods
        public void Scan()
        {
            //====================================
            // Validation/Checks and Balances
            //====================================
            // Check if the directory exists
            if (!(Directory.Exists(ScanDirectory)))
                throw new DirectoryNotFoundException();
            // Check that an XML file has been loaded, if not then load it
            if (xmlPlowExtensions == null)
                LoadExtensions();
            // Initialize the XML results doc
            scan_matched = new XmlDocument();
            //scan_unmatched = new XmlDocument();

            // Get all of the files in the specified directory
            var di = new DirectoryInfo(ScanDirectory);

            // Search through files finding ones that match the extension criteria
            foreach (FileInfo file in di.GetFiles())
            {
                bool match = false;
                int x=0;
                do {
                    if (x >= xmlExtensions.Length)
                    {
                        break;
                    }
                    else if (file.Extension == xmlExtensions[x])
                    {
                        match = true;
                        // Validate action, if validated then add to matches otherwise log an error
                        if (ValidateAction(xmlActions[x]))
                            AddResult(scan_matched, xmlActions[x], xmlFolderNames[x], xmlExtensions[x], file.FullName);
                        else
                            plowLog.WriteLog(LogWriter.LOG_TYPE.ERROR, String.Format("Action {0} for {1} did not validate!", xmlActions[x], file.FullName),
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    }
                    else { x++; }
                } 
                while (!match);
                // If the file did not match, add the unmatched tag
                if (!(match))
                {
                    //AddResult(scan_unmatched, "unmatched", "", "", file.FullName);
                    AddResult(scan_matched, "unmatched", "", "", file.FullName);
                }
            }

        }
        public void Plow()
        {
            // Validate result
            if (scan_matched == null)
            {
                plowLog.WriteLog(LogWriter.LOG_TYPE.ERROR, "There are no results in the XML, Plow failed.",
                    this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
            }

            // Read through the XML results and process each one
            XmlElement results_root = scan_matched.DocumentElement;
            XmlNodeList results_nodes = results_root.SelectNodes("Result");
            foreach (XmlNode childNode in results_nodes)
            {
                switch (childNode.Attributes["action"].Value)
                {
                    case "Move":
                        if (VerboseLogging)
                            plowLog.WriteLog(LogWriter.LOG_TYPE.VERBOSE, String.Format("Moving: {0} to {1}", childNode.InnerText, childNode.Attributes["foldername"].Value),
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        // DO move commands
                        MoveFile(childNode.InnerText, childNode.Attributes["foldername"].Value, childNode.Attributes["extension"].Value);
                        break;
                    case "Delete":
                        if (VerboseLogging)
                            plowLog.WriteLog(LogWriter.LOG_TYPE.VERBOSE, String.Format("Deleting: {0}", childNode.InnerText),
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        // Do delete commands
                        File.Delete(childNode.InnerText); // Since we're just deleting, there's no need to separate into an individual method
                        break;
                    case "Archive":
                        if (VerboseLogging)
                            plowLog.WriteLog(LogWriter.LOG_TYPE.VERBOSE, String.Format("Archiving: {0} to {1}", childNode.InnerText, childNode.Attributes["foldername"].Value),
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        // Do archive commands - In the XML, the file name must be specified and the FolderName will be used for the Archive name
                        ArchiveFile(childNode.InnerText, childNode.Attributes["foldername"].Value);
                        break;
                    case "Exclude": case "Unmatched":
                        plowLog.WriteLog(LogWriter.LOG_TYPE.INFO, String.Format("Excluding: {0} Reason: {1}", childNode.InnerText, childNode.Attributes["action"].Value),
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        // Do exclude commands
                        break;
                    case "MoveAndArchive":
                        // Do move and archive commands

                        break;
                    default:
                        // No action found
                        if (VerboseLogging) 
                        {
                            plowLog.WriteLog(LogWriter.LOG_TYPE.INFO, "No equivalent action found for: " + childNode.Attributes["action"].Value + " [File]::" + childNode.InnerText,
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        }
                        else
                        {
                            plowLog.WriteLog(LogWriter.LOG_TYPE.INFO, "No equivalent action found for: " + childNode.Attributes["action"].Value,
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        }
                        break;
                }
            }
        }
        private void MoveFile(string filePath, string folderPath, string fileExt)
        {
            if (!(Directory.Exists(PlowDirectory)))
                throw new DirectoryNotFoundException(); //  This may not be necessary, however, it adds redundancy error handling

            string newFolderPath = PlowDirectory + "\\" + folderPath;
            string newFolderFilePath = PlowDirectory + "\\" + folderPath + "\\" + Path.GetFileName(filePath);
            int rptFile = 1;

            if (!(Directory.Exists(newFolderPath)))
            {// If the directory doesn't exist, create it and move the file
                Directory.CreateDirectory(newFolderPath);
                File.Move(filePath, newFolderFilePath);
            }
            else // If the directory didn't exist we created it, but what if the file already exists in that directory
            {
                if (!(File.Exists(newFolderFilePath))) // If the file does not exist in the folder
                { File.Move(filePath, newFolderFilePath); } // then just move it
                else
                {// Otherwise rename it until we have a filename that doesn't exist
                    do { rptFile++; }
                    while (File.Exists(newFolderPath + "\\" + Path.GetFileNameWithoutExtension(filePath) + " (" + rptFile + ")" + fileExt));
                    // Then move the file
                    File.Move(filePath, newFolderPath + "\\" + Path.GetFileNameWithoutExtension(filePath) + " (" + rptFile + ")" + fileExt);
                }
            }
        }
        private void ArchiveFile(string filePath, string zipPath)
        {
            /*
            zipPath = ScanDirectory + "\\" + zipPath;
            string temp_dir = Environment.CurrentDirectory + "\\tempdir";
            Directory.CreateDirectory(temp_dir);

            File.Move(filePath, temp_dir);
            ZipFile.CreateFromDirectory(temp_dir, zipPath);

            Directory.Delete(temp_dir,true);*/
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = @"C:\Program Files\7-Zip\7z.exe";
            startInfo.Arguments = "a " + zipPath + " " + filePath;
            try
            {
                using (Process proc_7z = Process.Start(startInfo))
                {
                    proc_7z.WaitForExit();
                }
            }
            catch (Exception err)
            {
                plowLog.WriteLog(LogWriter.LOG_TYPE.ERROR, "Error in archive step: " + err.Message,
                    this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
            }
        }
        private void AddResult(XmlDocument xDoc, string Action, string FolderName, string Extension, string FilePath)
        {
            /*(Matched)
             * <Results>
             *   <Result action="move" foldername="Text Documents" extension="txt">C:\Downloads\File.txt</Result>
             * </Results>
             * (Unmatched)
             * <Results>
             *   <Result action="unmatched" foldername="" extension="">C:\Downloads\File.123</Result>
             *   <Result action="excluded" ... </Result>
             * </Results>
             * (Delete)
             * <Result action="delete" foldername="" extension="txt">C:\Downloads\File.txt</Result>
             * (Archive)
             * <Result action="archive" foldername="C:\Temp\ArchiveFile.zip" extension="txt">C:\Downloads\File.txt</Result>
             */
            XmlElement result = xDoc.CreateElement("Result");
            if (xDoc.DocumentElement != null)
            {
                result.SetAttribute("action", Action);
                result.SetAttribute("foldername", FolderName);
                result.SetAttribute("extension", Extension);
                result.InnerText = FilePath;
                xDoc.DocumentElement.AppendChild(result);
            }
            else
            {
                XmlElement root = xDoc.CreateElement("Results");
                //XmlElement result = xDoc.CreateElement("Result");
                result.SetAttribute("action", Action);
                result.SetAttribute("foldername", FolderName);
                result.SetAttribute("extension", Extension);
                result.InnerText = FilePath;
                root.AppendChild(result);
                xDoc.AppendChild(root);
            }
        }
        private void LoadExtensions()
        {
            xmlPlowExtensions = new XmlDocument();
            if (VerboseLogging)
                plowLog.WriteLog(LogWriter.LOG_TYPE.VERBOSE, "Loading XML file: " + xPath, this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
            // Load the XML document
            try
            {
                xmlPlowExtensions.Load(xPath);
            }
            catch (XmlException xmlerr)
            {// This allows the code to continue running, keep an eye on this as it may allow failures during Scan() parsing of the XML file
                plowLog.WriteLog(LogWriter.LOG_TYPE.ERROR, "Loading XML file failed. " + xmlerr.Message,
                    this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
            }
            var ext = xmlPlowExtensions.SelectNodes("/Extensions/Extension/Name");
            var folder = xmlPlowExtensions.SelectNodes("/Extensions/Extension/FolderName");
            var action = xmlPlowExtensions.SelectNodes("/Extensions/Extension/Action");

            xmlExtensions = new string[ext.Count];
            xmlFolderNames = new string[folder.Count];
            xmlActions = new string[action.Count];

            for (int i = 0; i < ext.Count; i++)
            {
                if (VerboseLogging)
                    plowLog.WriteLog(LogWriter.LOG_TYPE.VERBOSE, "Loaded XML entry for: " + ext[i].InnerText, 
                        this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);

                xmlExtensions[i] = ext[i].InnerText;
                xmlFolderNames[i] = folder[i].InnerText;
                xmlActions[i] = action[i].InnerText;
            }
        }
        private bool ValidateAction(string Action)
        {
            bool valid = false;
            foreach (PlowActions act in Enum.GetValues(typeof(PlowActions)))
            {
                if (Action == act.ToString())
                {
                    valid = true;
                    return true;
                }
                else
                    valid = false;
            }
            if (valid) return true;
            else return false;
        }

        #endregion Methods

        #region Enumerations
        public enum PlowActions
        {/*
         * enum_Actions
         *  0-exclude (skip)
         *  1-move
         *  2-delete
         *  3-archive
         *  4-move and archive 
         */
            Exclude = 0,
            Move = 1,
            Delete = 2,
            Archive = 3,
            MoveAndArchive = 4
        }
        #endregion Enumerations
    }
}