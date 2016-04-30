using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Logger;

namespace PlowTruck
{
    /// <summary>
    /// This class will handle the configuration of the PlowTruck process.
    /// </summary>
    class Configuration
    {
        #region Variable Declaration
        private LogWriter ConfigLogger = new LogWriter(Environment.CurrentDirectory, "PlowTruck");

        private string _plowPath;
        private string _destPath;
        private string _extXMLFile;
        private bool Verbose;
        private Dictionary<string, string> _extLookup;
        private Dictionary<string, string> _extAction;

        public string PlowPath
        {
            get { return _plowPath; }
            set {
                if (Directory.Exists(value))
                    _plowPath = value;
                else
                {
                    if (Verbose)
                        ConfigLogger.WriteLog(LogWriter.LOG_TYPE.ERROR, (String.Format("Directory '{0}' was not found!", value)), MethodBase.GetCurrentMethod().Name);
                    throw new DirectoryNotFoundException();
                }
            }
        }
        public string DestinationPath
        {
            get { return _destPath; }
            set {
                if (Directory.Exists(value))
                    _destPath = value;
                else
                {
                    if (Verbose)
                        ConfigLogger.WriteLog(LogWriter.LOG_TYPE.ERROR, (String.Format("Directory '{0}' was not found!", value)), MethodBase.GetCurrentMethod().Name);
                    throw new DirectoryNotFoundException();
                }
            }
        }
        public string ExtensionDefinitionPath
        {
            get { return _extXMLFile; }
            set {
                if (File.Exists(value))
                    _extXMLFile = value;
                else
                {
                    if (Verbose)
                        ConfigLogger.WriteLog(LogWriter.LOG_TYPE.ERROR, (String.Format("Could not find '{0}' file specified.", value)), MethodBase.GetCurrentMethod().Name);
                    throw new FileNotFoundException();
                }
            }
        }
        public Dictionary<string, string> ExtensionLookup
        {
            get { return _extLookup; }
        }
        public Dictionary<string, string> ExtensionAction
        {
            get { return _extAction; }
        }
        #endregion

        #region Constructors
        // Provide different methods for creating the Configuration class
        
        public Configuration()
        {
            Verbose = false;
        }
        public Configuration(bool verbose)
        {
            Verbose = verbose;
            if(Verbose)
                ConfigLogger.WriteLog(LogWriter.LOG_TYPE.VERBOSE, "Created config object", this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
        }
        // Populate the variables with the values passed to the constructor...nothing special going on here
        public Configuration(string xmlConfigFile, string plowPath, string destinationPlowPath = "", bool verbose=false)
        {
            Verbose = verbose;
            if(Verbose)
                ConfigLogger.WriteLog(LogWriter.LOG_TYPE.VERBOSE, "Created config object", this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);

            ExtensionDefinitionPath = xmlConfigFile;
            PlowPath = plowPath;
            DestinationPath = destinationPlowPath;
        }
        #endregion

        #region Methods
        // Thinking this method will run all of the methods necessary to read the XML file
        // and populate the internal variables with the information needed
        public void LoadConfig()
        {
            // Create a new XML document and verbose log it
            XmlDocument xmlPlowExtensions = new XmlDocument();
            if (Verbose)
                ConfigLogger.WriteLog(LogWriter.LOG_TYPE.VERBOSE, String.Format("Loading XML - File: {0}", _extXMLFile), this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
            // Load the XML document
            try
            {
                xmlPlowExtensions.Load(_extXMLFile);
            }
            catch (XmlException xmlerr)
            {// This allows the code to continue running, keep an eye on this as it may allow failures during parsing of the XML file
                ConfigLogger.WriteLog(LogWriter.LOG_TYPE.ERROR, String.Format("Loading the XML file failed: {0}", xmlerr.Message), this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
            }

            // Create variables to hold the data loaded from the XML file
            var extension = xmlPlowExtensions.SelectNodes("/Extensions/Extension/Name");
            var folder = xmlPlowExtensions.SelectNodes("/Extensions/Extension/FolderName");
            var action = xmlPlowExtensions.SelectNodes("/Extensions/Extension/Action");
            
            for (int i = 0; i < extension.Count; i++)
            {
                // Log for verbose logging
                if (Verbose)
                    ConfigLogger.WriteLog(LogWriter.LOG_TYPE.VERBOSE, String.Format("Loaded XML entry for: {0}", extension[i].InnerText), this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                // Add the extension/folder and extension/action to the associated dictionaries
                _extLookup.Add(extension[i].InnerText, folder[i].InnerText);
                _extAction.Add(extension[i].InnerText, action[i].InnerText);
            }
            if (Verbose)
                ConfigLogger.WriteLog(LogWriter.LOG_TYPE.VERBOSE, String.Format("Completed XML loading - File: {0}", _extXMLFile), this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
        }
        #endregion
    }
}
