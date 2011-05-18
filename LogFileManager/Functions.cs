using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Ionic.Zip;

namespace LogFileManager
{
    class FUNCTIONS
    {
        #region Class members:
        //Sync these from main.cs
        public static string SRC = main.SRC;
        public static string DST = main.DST;
        public static int MAX_AGE = main.MAX_AGE;

        public static bool DIREC_FOUND = false; //Assume directory does not exist
        public static bool ZIP_WRITTEN = false; //Assume archive not written
        #endregion

        /// <summary>
        /// Collects all the filenames in a given path and returns those matching a given pattern
        /// </summary>
        /// <param name="path">Path to search</param>
        /// <param name="pattern">Pattern to match</param>
        public static string[] CollectFileNames(string path, string pattern)
        {
            try
            {
                //Collect all filenames in given path
                string[] allfiles = Directory.GetFiles(path);
                DIREC_FOUND = true;

                //Regular expression for specified pattern
                Regex r = new Regex(pattern);

                //List to hold all matching filenames
                List<string> files = new List<string>();

                //Check for matches and store
                foreach (string _file in allfiles)
                    if (r.IsMatch(_file))
                        files.Add(_file);

                //Return array of matches
                return files.ToArray();
            }
            catch (DirectoryNotFoundException) //In case of invalid directory
            {
                Console.WriteLine("ERROR: " + SRC + " not found! May not exist or insufficient privileges");
                return new string[0]; //Return an empty array
            }
        }

        /// <summary>
        /// Zips and deletes a collection of files into an archive.
        /// </summary>
        /// <param name="files">A list of filenames to be archived into a .zip file</param>
        public static void ZipFiles(string[] files)
        {
            int zipCount = 0, delCount = 0, fCount = 1;
            List<string> notWritten = new List<string>();

            //Create a new zip archive from the chosen files
            using (ZipFile zip = new ZipFile())
            {
                foreach (string _file in files)
                {
                    ProgressBar(fCount, files.Length);
                    fCount++;
                    //Remove extraneous data from the filename 
                    //(upper levels of the path, makes the archive cleaner)
                    int lastslash = _file.LastIndexOf('\\');
                    string localname = _file.Substring(lastslash + 1);
                    try
                    {
                        zip.AddFile(localname);
                        zipCount++;
                    }
                    catch (ZipException) //Catches an invalid filename
                    {
                        notWritten.Add(_file);
                    }
                }

                //Notify user of files not written, if any
                Console.WriteLine();
                foreach (string _file in notWritten)
                {
                    Console.WriteLine(_file + " not written to archive " + DST);
                }

                try
                {
                    Console.Write("Writing... ");
                    zip.Save(DST);
                    ZIP_WRITTEN = true;
                    Console.WriteLine("done.");
                }
                catch (DirectoryNotFoundException) //Path to DST does not exist
                {
                    Console.WriteLine("ERROR: " + DST + " not found! May not exist or insufficient privileges");
                    CleanTmps();
                }
            }

            if (ZIP_WRITTEN) //Do the following only if the files were successfully archived
            {
                //Delete the archived files
                //Catch exceptions individually so that deletion doesn't stop from one error (do as much for the user as you can)
                Console.Write("Deleting old files... ");
                foreach (string _file in files)
                    try
                    {
                        File.Delete(_file);
                        delCount++;
                    }
                    catch (ArgumentException) { } //Skip invalid strings
                    catch (IOException) //File in use
                    {
                        Console.WriteLine("ERROR: Could not delete " + _file + ", file was in use");
                    }
                    catch (UnauthorizedAccessException) //Denied Access (read-only or insufficient privileges)
                    {
                        Console.WriteLine("ERROR: Could not delete " + _file + ", access denied. File may be read-only");
                    }
                Console.WriteLine("done.");

                //Relay success to user
                Console.WriteLine(DST + " successfully created (" + zipCount + " files archived, " + delCount + " files deleted)");
            }
        }

        /// <summary>
        /// Checks src for any .tmp files and removes them. These .tmp files
        /// are normally created when an exception is encountered in ZipFiles.
        /// DotNetZip creates .tmp files as archives, then renames them later,
        /// so stragglers need to be cleaned.
        /// </summary>
        public static void CleanTmps()
        {
            //Collect all files
            string[] files = Directory.GetFiles(SRC); //Note: no need to catch a DirectoryNotFoundException here, the code only runs if it exists
            Regex r = new Regex(@"^.*\.tmp$");
            foreach (string _file in files)
            {
                //Delete matching files
                //Catch exceptions individually so that deletion doesn't stop from one error (do as much for the user as you can)
                if (r.IsMatch(_file))
                    try
                    {
                        File.Delete(_file);
                    }
                    catch (ArgumentException) { continue; } //Skip invalid strings
                    catch (IOException) //File in use
                    {
                        Console.WriteLine("ERROR: Could not delete " + _file + ", file was in use");
                    }
                    catch (UnauthorizedAccessException) //Denied Access (read-only or insufficient privileges)
                    {
                        Console.WriteLine("ERROR: Could not delete " + _file + ", access denied. File may be read-only");
                    }
            }
        }

        /// <summary>
        /// Draws a console-based progress bar.
        /// </summary>
        /// <param name="progress">Current operation number</param>
        /// <param name="total">Total number of operations expected</param>
        public static void ProgressBar(int current, int total)
        {
            //Draw an empty bar
            Console.CursorLeft = 0;
            Console.Write('[');
            Console.CursorLeft = 32;
            Console.Write(']');
            Console.CursorLeft = 1;
            float unit = 30.0f / total;

            int pos = 1;
            for (int i = 0; i <= unit * current; i++)
            {
                Console.CursorLeft = pos++;
                Console.Write("+");
            }

            for (int i = pos; i <= 31; i++)
            {
                Console.CursorLeft = pos++;
                Console.Write("-");
            }

            Console.CursorLeft = 35;
            Console.Write(current.ToString() + " of " + total.ToString() + "     ");
        }

        /// <summary>
        /// Determines the age of a given .zip file in days
        /// </summary>
        /// <param name="file">The .zip file to be age-checked</param>
        /// <returns></returns>
        public static int Age(string file)
        {
            //Truncate filename to a local name
            int slash = file.LastIndexOf('\\');
            string local = file.Substring(slash + 1);

            //Index of year, month, and day are known to be 0, 5, and 8, respectively
            //Extract month and day from filename
            int fileYear = Convert.ToInt32(local.Substring(0, 4));
            int fileMonth = Convert.ToInt32(local.Substring(5, 2));
            int fileDay = Convert.ToInt32(local.Substring(8, 2));
            DateTime fileDT = new DateTime(fileYear, fileMonth, fileDay);

            TimeSpan age = DateTime.Now - fileDT;
            return age.Days;
        }

        /// <summary>
        /// Cleans the current directory of old archives. The desired
        /// maximum age can be set in the Member Variables.
        /// </summary>
        public static void CleanOldZips()
        {
            //Collect all the .zip files in the current directory
            string[] allZips = CollectFileNames(SRC, @"^.*\.zip$");

            foreach (string _zip in allZips)
                //Compare age of _zip to the maximum allowable age
                //Delete those that exceed the maximum
                if (Age(_zip) > MAX_AGE)
                    try
                    {
                        File.Delete(_zip);
                        Console.WriteLine(_zip + " successfully cleaned: age");
                    }
                    catch (ArgumentException) { continue; } //Skip invalid strings
                    catch (IOException) //File in use
                    {
                        Console.WriteLine("ERROR: Could not delete " + _zip + ", file was in use");
                    }
                    catch (UnauthorizedAccessException) //Denied Access (read-only or insufficient privileges)
                    {
                        Console.WriteLine("ERROR: Could not delete " + _zip + ", access denied. File may be read-only");
                    }
        }

        /// <summary>
        /// Updates the class members after changes in main
        /// </summary>
        public static void UpdateVars()
        {
            SRC = main.SRC;
            DST = main.DST;
            MAX_AGE = main.MAX_AGE;
        }
    }
}
