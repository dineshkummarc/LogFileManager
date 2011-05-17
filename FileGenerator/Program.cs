using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FileGenerator
{
    class Program
    {
        static Random RAND = new Random((int)DateTime.Now.Ticks);

        static void Main(string[] args)
        {
            string filename;
            //Generate 1000 random .txt file names with their names as content
            for (int i = 0; i < 1000; i++)
            {
                filename = RandomString(10) + ".txt4";
                TextWriter tw = new StreamWriter(filename);
                //int limit = RAND.Next(50);
                //for(int j = 0; j < limit; j++)
                tw.WriteLine(filename);
                tw.Close();
            }
        }

        static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * RAND.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }
    }
}
