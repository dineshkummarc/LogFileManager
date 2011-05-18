using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LogFileManager
{
    class main
    {
        #region Member variables:
        public static string SRC = Directory.GetCurrentDirectory(); //Assumes current directory, but easily changeable
        public static string DST = DateTime.Now.ToString("yyyy-MM-dd") + ".zip"; //Save with a date-stamped filename
        public static bool DIREC_FOUND = false; //Assume directory does not exist
        public static bool ZIP_WRITTEN = false; //Assume archive not written
        public static int MAX_AGE = 2; //Max age of .zip files in days
        public static string REGEX = @"^.*\.txt\d+$"; //Regular expression marking files to search for
        #endregion

        /// <summary>
        /// Main method. Executes the program.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            Console.WriteLine();

            //Parse arguments and get files
            ParseArgs(args);

            //Do stuff only if the help menu wasn't brought up
            if ((args.Length != 0) && (args[0].Equals("-h") | args[0].Equals("--help")))
                return;

            FUNCTIONS.UpdateVars();
            string[] files = FUNCTIONS.CollectFileNames(SRC, REGEX);

            //Only perform following if directory exists (if not, CollectFileNames will output appropriate error)
            if(FUNCTIONS.DIREC_FOUND)
                if (files.Length == 0)
                    Console.WriteLine("ERROR: No files to archive!");
                else
                    FUNCTIONS.ZipFiles(files);

            //Clean the current directory of old archives
            FUNCTIONS.CleanOldZips();
        }

        /// <summary>
        /// Parses arguments to the program
        /// </summary>
        static void ParseArgs(string[] args)
        {
            //Parse config file
            ConfigParser config = new ConfigParser();
            string val;

            int numArgs = args.Length;
            if (numArgs == 0) //Nothing to parse, use values from config file
            {
                if (config.GetTable().TryGetValue("SRC", out val))
                    SRC = config["SRC"];
                else
                    config.GetTable().Add("SRC", SRC);

                if (config.GetTable().TryGetValue("MAX_AGE", out val))
                    MAX_AGE = Convert.ToInt32(config["MAX_AGE"]);
                else
                    config.GetTable().Add("MAX_AGE", Convert.ToString(MAX_AGE));

                if (config.GetTable().TryGetValue("REGEX", out val))
                    REGEX = config["REGEX"];
                else
                    config.GetTable().Add("REGEX", REGEX);

                //Rewrite the config file
                config.RewriteConfig();

                //Use and write defaults if either entry does not exist
            }
            else if (numArgs == 1) //One argument: either asking for help or bad command
            {
                if (args[0].Equals("-h") || args[0].Equals("--help"))
                {
                    Console.WriteLine("LogFileManager v1.0");
                    Console.WriteLine("Archives a group of files following a common name pattern to \"yyyy-MM-dd.zip\"");
                    Console.WriteLine("Written by James Forshee (jforshee@archvision.com)\n");
                    Console.WriteLine("## Command List:");
                    Console.WriteLine(" -a or --age-max <maximum>:\t\tChange maximum archive age");
                    Console.WriteLine(" -c or --config-file <filename>:\tUse a different config-file this time");
                    Console.WriteLine(" -h or --help:\t\t\t\tBring up this menu");
                    Console.WriteLine(" -r or --regex <string>:\t\tChange filename pattern to archive");
                    Console.WriteLine(" -s or --source <directory>:\t\tChange directory of files to archive");
                }
                else //Bad args
                    Console.WriteLine("Invalid arguments");
            }
            else if (numArgs % 2 == 0) //Should be an even number of arguments 
            {
                for (int i = 0; i < numArgs; i += 2) //Check only every other arg
                {
                    if (args[i].Equals("-a") | args[i].Equals("--age-max")) //Change maximum archive age
                    {
                        //Extract the arg
                        try
                        {
                            int max = Convert.ToInt32(args[i + 1]);
                            //Catch negative numbers
                            if (max < 0)
                                Console.WriteLine("ERROR: Maximum age must be positive");
                            else //Write changes to temp
                            {
                                MAX_AGE = max;
                                if (config.GetTable().TryGetValue("MAX_AGE", out val))
                                    config["MAX_AGE"] = Convert.ToString(max);
                                else
                                    config.GetTable().Add("MAX_AGE", Convert.ToString(max));
                            }
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("ERROR: Invalid maximum age");
                        }
                        catch (OverflowException)
                        {
                            Console.WriteLine("ERROR: Maximum age is not in range");
                        }
                    }
                    else if (args[i].Equals("-c") | args[i].Equals("--config-file")) //Change config-file for this run
                        config.SetConfig(args[i + 1]); //Don't need to check existence, SetConfig already does

                    else if (args[i].Equals("-r") | args[i].Equals("--regex"))
                    {
                        REGEX = args[i + 1];
                        if (config.GetTable().TryGetValue("REGEX", out val))
                            config["REGEX"] = REGEX;
                        else
                            config.GetTable().Add("REGEX", REGEX);
                    }
                    else if (args[i].Equals("-s") | args[i].Equals("--source")) //Change source directory
                    {
                        SRC = args[i + 1];
                        DST = SRC + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip"; //Update DST
                        if (config.GetTable().TryGetValue("SRC", out val))
                            config["SRC"] = SRC;
                        else
                            config.GetTable().Add("SRC", SRC);
                    }
                    else //Invalid arg
                        if (args[i].Equals("-h") | args[i].Equals("--help"))
                            Console.WriteLine("ERROR: Please only use the help command by itself");
                        else
                            Console.WriteLine("ERROR: Invalid arg \"" + args[i] + "\"");
                }

                //Rewrite the config file
                config.RewriteConfig();
            }
            else //Invalid number of arguments
                Console.WriteLine("ERROR: Invalid number of arguments");
        }
    }
}
