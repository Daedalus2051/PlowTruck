﻿using System;
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
    *          <Include>true</Include>
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
    public class PlowTruck
    {
        #region Variables
        // PlowTruck() - These variables will more or less apply globally
        public string xPath
        {
            get { return xPath; }
            set
            {
                if (!(File.Exists(value)))
                    throw new FileNotFoundException();
                xPath = value;
            }
        }
        private Log plowLog;
        public bool VerboseLogging = false;
        // LoadExtensions()
        private XmlDocument xmlPlowExtensions;
        private string[] xmlExtensions;
        private string[] xmlFolderNames;
        private string[] xmlActions;
        // Scan()
        public string ScanDirectory { get; set; }
        public XmlDocument ScanResults { get { return scan_matched; } }
        public XmlDocument UnmatchedResults { get { return scan_unmatched; } }
        private XmlDocument scan_matched;
        private XmlDocument scan_unmatched;
        #endregion

        #region Constructors
        public PlowTruck(string XMLPath)
        {
            xPath = XMLPath;
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
            scan_matched.CreateElement("Results");

            // Get all of the files in the specified directory
            var di = new DirectoryInfo(ScanDirectory);

            // Search through files finding ones that match the extension criteria
            foreach (FileInfo file in di.GetFiles())
            {
                bool match = false;
            }

        }

        private void AddMatch(XmlDocument xDoc, PlowActions Action, string FolderName, string Extension, string FilePath)
        {
            /*
             * <Results>
             *   <Result action="1" foldername="Text Documents" extension="txt">C:\Downloads\File.txt</Result>
             * </Results>
             */
            XmlElement root = xDoc.CreateElement("Results");
            XmlElement result = xDoc.CreateElement("Result");
            result.SetAttribute("action", Action.ToString());
            result.SetAttribute("foldername", FolderName);
            result.SetAttribute("extension", Extension);
            result.InnerText = FilePath;
            root.AppendChild(result);
            xDoc.AppendChild(root);
        }

        private void LoadExtensions()
        {
            xmlPlowExtensions = new XmlDocument();
            if (VerboseLogging)
                plowLog.WriteLog(Log.LOG_TYPE.VERBOSE, "Loading XML file: " + xPath, this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
            // Load the XML document
            xmlPlowExtensions.Load(xPath);
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

        //With FileInfo this is obsolete...deprecated
        /// <summary>
        /// Find the extension or filename from a full path to a file
        /// </summary>
        /// <param name="path">The fully qualified path (i.e. C:\Folder\SubFolder\File.ext)</param>
        /// <param name="withoutExt">(Optional) Return the filename without the extension</param>
        /// <returns>(string) File extenion or filename without extension</returns>
        private string FindExtension(string path, bool withoutExt = false)
        {
            string[] tempPath;
            string[] tempExt;
            //Split the pathing apart
            tempPath = path.Split('\\');
            //Split the filename and ext apart
            tempExt = tempPath[(tempPath.Length - 1)].Split('.');
            //Return the last extension on the file name (this allows for files with multiple periods)
            if (withoutExt)
                if (tempExt.Length > 2)
                {//if the file has more than one period in the name
                    string tempResult = tempExt[0];
                    for (int i = 1; i < (tempExt.Length - 1); i++)
                    {
                        tempResult = tempResult + "." + tempExt[i];
                    }
                    return tempResult;
                }
                else
                {
                    return tempExt[0];
                }
            else
            {// return normally
                return tempExt[tempExt.Length - 1];
            }
        }
        /// <summary>
        /// Find the filename with extension from a full path
        /// </summary>
        /// <param name="path">Full path to the file</param>
        /// <returns>(string) Filename with extension</returns>
        private string FindFilename(string path)
        {
            string[] tempPath;
            //Split the pathing apart
            tempPath = path.Split('\\');
            return tempPath[(tempPath.Length - 1)];
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
            get { return Path; }
            set
            {
                if (!(Directory.Exists(value)))
                    throw new DirectoryNotFoundException();
                Path = value;
            }
        }
        public string Name
        {
            get { return Name; }
            set
            {
                if (!(string.IsNullOrEmpty(value)))
                    Name = value + ".log";
            }
        }
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
            // Validation occurs in the setters, no need to do double the work
            Path = sPath;
            Name = fName;
        }
        #endregion
        #region Methods
        public bool WriteLog(LOG_TYPE logtype, string message, string code_local)
        {
            string full_path = Path + "\\" + Name;
            try
            {
                StreamWriter log_writer = new StreamWriter(full_path, true);
                //[LOG_TYPE]    Date    Message (Code location)
                log_writer.WriteLine("[{0}]\t{1}\t{2}\t({3})", logtype, System.DateTime.Now, message, code_local);
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