using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PlowTruck
{
    /* Plow Truck by Russell Hoyer
    * Version 0.x (alpha[Full Feature Project])
    * (see latest change for version #)
    * 
    * This class is intended to take the files in a certain folder (i.e. Downloads) and move all the files into sub-folders
    * based on the type of file (extension) they have. EXEs in the Executable folder, ZIP in the archives folder. Etc.
    * 
    * It is to be used with an XML file of extensions, that acts as a filter, which contains the extension name (without the .)
    * and the folder name to which it belongs. The schema for the XML file is as follows:
    *  <Extensions>
    *      <Extension>
    *          <Name>exe</Name>
    *          <FolderName>Executables</FolderName>
    *          <Action>Move</Action>
    *      </Extension>
    *  </Extensions>
    *  
    * [Experimental]
    *  <Type>Date,DateRange,Extension,Filename</Type>
    *  <Value>docx,desktop.ini,mmddyy(and variations on that)</Value>
    * 
    * Changes v0.11 - (Alpha 2)
    *   -[Direction] Re-writing the Scan and Plow methods to be more dynamic and accept more operations
    * 
    * 
    *  [Planned]
    *   -Re-write the Plow method to have the following process (for the addition of action items):
    *      -> Call Load method, this returns an array of collections (Exclusions, Extensions, Actions)
    *      -> If Exclusion is found -> Skip to the next file
    *      -> If Extension is found -> Iterate through Action, if none default to Move
    *      -> Next
    *      
    *   -
    *   -Add a configuration file for Console/GUI
    *      -In the Console this config file will allow user to specify if they want full paths displayed vs filenames only
    *      -Output view=Full,Condensed,Verbose
    *   -Allow for an exclusion list, so that files like "desktop.ini", or other user specified files and extensions don't get moved
    *      -Allow for this exclusion list to also include dates/date ranges
    *   -Create GUI that will allow user to visually specify the folder structure (XML based)
    *   -Create a project that will allow the user to run Plow Truck as a service that will watch the folder and move files as they appear
    *   -Optional output to results to an XML file with the following schema
    *      <PlowResults>
    *          <Found>
    *              <Filename>testing.docx</Filename>
    *              <Reason>Included</Reason>
    *          </Found>
    *          <Found>
    *              <Filename>testing.blah</Filename>
    *              <Reason>No Match</Reason>
    *          </Found>
    *          <Found>
    *              <Filename>desktop.ini</Filename>
    *              <Reason>Excluded</Reason>
    *          </Found>
    *      </PlowResults>
    *   -MoveModes(2): 
    *      Mode 1 will allow the user to move files into the same folder but different sub-folders (think organizing into bins)
    *      Mode 2 will allow the user to move files to a completely different path (think reorganizing to another room)
    *   -[Created-Implemented, not yet refined] PlowTruckFleet Class
    *      This will allow the user to create and use multiple plow truck services to watch multiple folders
    *   -[Complete] Restore default XML tree; this will rebuild the default XML file from hard code
    *   -Multiple user support
    *   -Allow for different actions to be specified in the XML for each extension (i.e. move, delete, archive)
    *   -Create actions to be taken for files; move by default, delete, archive
    *   -Watch folder for new file arrivals; allow for archival (or move to a separate path for archival on a NAS drive) or deletion after a certain date (7-Zip?)
    */
    public class PlowTruckCore
    {
        #region Variables
        // PlowTruck() - These variables will more or less apply globally
        public string xPath
        {
            get { return xmlPath; }
        }
        private string xmlPath;
        private Log plowLog;
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

            plowLog = new Log(Environment.CurrentDirectory, "PlowTruck");
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
                            plowLog.WriteLog(Log.LOG_TYPE.ERROR, String.Format("Action {0} for {1} did not validate!", xmlActions[x], file.FullName),
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
                plowLog.WriteLog(Log.LOG_TYPE.ERROR, "There are no results in the XML, Plow failed.",
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
                            plowLog.WriteLog(Log.LOG_TYPE.VERBOSE, String.Format("Moving: {0} to {1}", childNode.InnerText, childNode.Attributes["foldername"].Value),
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        // DO move commands
                        MoveFile(childNode.InnerText, childNode.Attributes["foldername"].Value, childNode.Attributes["extension"].Value);
                        break;
                    case "Delete":
                        if (VerboseLogging)
                            plowLog.WriteLog(Log.LOG_TYPE.VERBOSE, String.Format("Deleting: {0}", childNode.InnerText),
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        // Do delete commands
                        break;
                    case "Archive":
                        if (VerboseLogging)
                            plowLog.WriteLog(Log.LOG_TYPE.VERBOSE, String.Format("Archiving: {0} to {1}", childNode.InnerText, childNode.Attributes["foldername"].Value),
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        // Do archive commands
                        break;
                    case "Exclude": case "Unmatched":
                            plowLog.WriteLog(Log.LOG_TYPE.INFO, String.Format("Excluding: {0}", childNode.InnerText),
                                this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        // Do exclude commands
                        break;
                    case "MoveAndArchive":
                        // Do move and archive commands
                        break;
                    default:
                        // No action found
                        plowLog.WriteLog(Log.LOG_TYPE.INFO, "No equivalent action found for: " + childNode.Attributes["action"].Value,
                            this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
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
        private void AddResult(XmlDocument xDoc, string Action, string FolderName, string Extension, string FilePath)
        {
            /*(Matched)
             * <Results>
             *   <Result action="move" foldername="Text Documents" extension="txt">C:\Downloads\File.txt</Result>
             * </Results>
             * (Unmatched)
             * <Results>
             *   <Result action="unmatched" foldername="" extension="">C:\Downloads\File.123</Result>
             * </Results>
             * (Delete)
             * <Result action="delete" foldername="" extension="txt">C:\Downloads\File.txt</Result>
             * (Archive)
             * <Result action="archive" foldername="C:\Temp\ArchiveFile.zip" extension="txt">C:\Downloads\File.txt</Result>
             */
            XmlElement result = xDoc.CreateElement("Result");
            if (xDoc.DocumentElement != null)
            {
                result.SetAttribute("action", Action.ToString());
                result.SetAttribute("foldername", FolderName);
                result.SetAttribute("extension", Extension);
                result.InnerText = FilePath;
                xDoc.DocumentElement.AppendChild(result);
            }
            else
            {
                XmlElement root = xDoc.CreateElement("Results");
                //XmlElement result = xDoc.CreateElement("Result");
                result.SetAttribute("action", Action.ToString());
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
                plowLog.WriteLog(Log.LOG_TYPE.VERBOSE, "Loading XML file: " + xPath, this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
            // Load the XML document
            try
            {
                xmlPlowExtensions.Load(xPath);
            }
            catch (XmlException xmlerr)
            {// This allows the code to continue running, keep an eye on this as it may allow failures during Scan() parsing of the XML file
                plowLog.WriteLog(Log.LOG_TYPE.ERROR, "Loading XML file failed. " + xmlerr.Message,
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
                    plowLog.WriteLog(Log.LOG_TYPE.VERBOSE, "Loading XML entry for: " + ext[i].InnerText, 
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
    internal class Log
    {
        #region Variables
        public string Path
        {
            get { return _path; }
        }
        public string Name
        {
            get { return _name; }
        }
        private string _path;
        private string _name;
        public string ErrorMessage { get { return errormsg; } }
        private string errormsg;
        #endregion
        #region Enumerations
        public enum LOG_TYPE
        {
            WARNING,
            VERBOSE,
            DEBUG,
            ERROR,
            INFO,
            CONFIG
        }
        #endregion
        #region Constructor
        public Log(string sPath, string fName)
        {
            // Validation occurs in the setters, no need to do double the work.
            if (!(Directory.Exists(sPath) || string.IsNullOrEmpty(sPath)))
                throw new DirectoryNotFoundException();
            _path = sPath;
            if (!(string.IsNullOrEmpty(fName)))
                _name = fName + ".log";

        }
        #endregion
        #region Methods
        public bool WriteLog(LOG_TYPE logtype, string message, string code_local)
        {
            string full_path = _path + "\\" + _name;
            try
            {
                StreamWriter log_writer = new StreamWriter(full_path, true);
                //[LOG_TYPE]    Date    Message (Code location)
                log_writer.WriteLine("{0}\t[{1}]{2}\t({3})", System.DateTime.Now, logtype, message, code_local);
                log_writer.Close();

                return true;
            }
            catch(Exception err)
            {
                errormsg = "[Log>>WriteLog]: " + err.Message;
                return false;
            }
        }
        #endregion Methods
    }
}