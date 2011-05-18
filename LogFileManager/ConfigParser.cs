using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LogFileManager
{
    /*
     * This is a parser for configuration files. There are a few standards for
     * the config files if you expect this parser to work. Namely, keys and
     * values in the file should be = separated. The parser itself has no regard
     * for separation in the keys themselves (i.e. PARENT.child.grandchild style
     * properties), so keep that in mind when using it. The functionality can
     * certainly be implemented, if you wish.
     */
    class ConfigParser
    {
        #region Class members and constants:
        private static string DEFAULT_PATH = Directory.GetCurrentDirectory() + "\\LogFileConfig.txt";
        private string CONFIG_FILE;
        private Dictionary<string, string> TABLE;
        #endregion

        #region Constructors:
        /// <summary>
        /// Default constructor. Creates an empty table and the config file path
        /// defaults to "config.txt"
        /// </summary>
        public ConfigParser()
        {
            try
            {
                //If file does not exist, create it
                FileStream fs = File.OpenWrite(DEFAULT_PATH);
                fs.Close(); fs = null;

                CONFIG_FILE = DEFAULT_PATH;
                TABLE = new Dictionary<string, string>();
                ParseConfig();
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("ERROR: Directory for config file does not exist");
            }
        }

        /// <summary>
        /// Instance constructor
        /// </summary>
        /// <param name="config">Path to config file</param>
        public ConfigParser(string config)
        {
            try
            {
                //If file does not exist, create it
                FileStream fs = File.OpenWrite(DEFAULT_PATH);
                fs.Close(); fs = null;

                CONFIG_FILE = config;
                TABLE = new Dictionary<string, string>();
                ParseConfig();
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("ERROR: Directory for config file does not exist");
            }
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public ConfigParser(ConfigParser cp)
        {
            CONFIG_FILE = cp.CONFIG_FILE;
            TABLE = cp.TABLE;
        }
        #endregion

        #region Access methods:
        //Class member access
        public string GetConfig() { return CONFIG_FILE; }
        public Dictionary<string, string> GetTable() { return TABLE; }

        //Table access
        public string GetValue(string key) { return TABLE[key]; }
        public void SetValue(string key, string value) { TABLE[key] = value; }
        public string this[string key] 
        { 
            get 
            { 
                return TABLE[key]; 
            }
            set
            {
                TABLE[key] = value;
            }

        }

        //Setting class members
        public void SetConfig(string path) 
        {
            if (File.Exists(path))
            {
                CONFIG_FILE = path;
                ParseConfig(); //Re-parse
            }
            else
                Console.Write("ERROR: Specified file does not exist");
        }
        public void SetTable(Dictionary<string, string> table) { TABLE = table; }
        #endregion

        #region Parsing:
        /// <summary>
        /// Parses a line into some relevant information, separated according
        /// to an internal standard
        /// </summary>
        /// <param name="line">The line to parse</param>
        public void ParseLine(string line)
        {
            if (line.Equals(""))
                return;

            //Find the equals sign
            int eql = line.IndexOf('=');
            //Separate data (blah=blah)
            TABLE.Add(line.Substring(0, eql), line.Substring(eql + 1));
        }

        /// <summary>
        /// Parses a specific line in the config file
        /// </summary>
        /// <param name="line">The line to parse</param>
        public void ParseLine(int line)
        {
            //Have to count the lines one by one, unfortunately
            using (var sr = new StreamReader(CONFIG_FILE))
            {
                //Reads lines up to the desired line
                for (int i = 1; i < line; i++)
                    sr.ReadLine();
                //Parse the desired line
                ParseLine(sr.ReadLine());
            }
        }

        /// <summary>
        /// Clears all data from TABLE and parses the config file
        /// </summary>
        public void ParseConfig()
        {
            TABLE.Clear(); //Clear the table
            using (var sr = new StreamReader(CONFIG_FILE))
            {
                //Get the first line
                string line = sr.ReadLine();
                while (line != null)
                {
                    //Parse and get the next line
                    ParseLine(line);
                    line = sr.ReadLine();
                }
                sr.Close();
            }
        }

        /// <summary>
        /// Rewrites the config file to hold the data in TABLE.
        /// Essentially the inverse of ParseConfig
        /// </summary>
        public void RewriteConfig()
        {
            //Delete the old config file
            File.Delete(CONFIG_FILE);

            //Write data to a new config file
            TextWriter config = new StreamWriter(CONFIG_FILE);
            foreach (KeyValuePair<string, string> entry in TABLE)
                config.WriteLine(entry.Key + "=" + entry.Value);

            //Close the file
            config.Close();
        }

        /// <summary>
        /// Clears the entire config file
        /// </summary>
        public void ClearConfig()
        {
            File.Delete(CONFIG_FILE);
            FileStream fs = File.Open(CONFIG_FILE, FileMode.OpenOrCreate);
            fs.Close();
        }
        #endregion
    }
}
