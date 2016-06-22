using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlowTruck
{
    class Operations
    {
        Configuration config = new Configuration();

        #region Constructors
        public Operations(Configuration conf)
        {
            config = conf;
        }
        #endregion

        // Scan
        public void Scan(Configuration conf)
        {

        }

        // Plow (scan first)

        // Plow (plow based on scan results

        #region Classes
        public class Results
        {
            public DataSet ResultSet { get; set; }
            public Results()
            {
                ResultSet.Tables.Add("Results");
                ResultSet.Tables[0].Columns.Add("File");
                ResultSet.Tables[0].Columns.Add("Extension");
                ResultSet.Tables[0].Columns.Add("Action");k
            }
        }
        #endregion
    }
}