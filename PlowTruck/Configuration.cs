using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Logger;

namespace PlowTruck
{
    class Configuration
    {
        private LogWriter ConfigLogger = new LogWriter(Environment.CurrentDirectory, "PlowTruck");

        private string _plowPath;
        private string _destPath;
        private Dictionary<string, string> _extLookup;

        public string PlowPath
        {
            get { return _plowPath; }
            set {
                if (Directory.Exists(value))
                    _plowPath = value;
                else
                    throw new DirectoryNotFoundException();
            }
        }
        public string DestinationPath
        {
            get { return _destPath; }
            set {
                if (Directory.Exists(value))
                    _destPath = value;
                else
                    throw new DirectoryNotFoundException();
            }
        }
        public Dictionary<string, string> ExtensionLookup
        {
            get { return _extLookup; }
        }

        public Configuration()
        {
            ConfigLogger.WriteLog(LogWriter.LOG_TYPE.VERBOSE, "", "");
        }

        // Thinking this method will run all of the methods necessary to read the XML file
        // and populate the internal variables with the information needed
        public void LoadConfig()
        {
        }
    }
}
