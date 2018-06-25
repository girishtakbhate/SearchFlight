using System;
using Plossum.CommandLine;
using Plossum;
using System.Data;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace SearchFlight
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Options options = new Options();
                CommandLineParser parser = new CommandLineParser(options);
                parser.Parse();

                if (parser.HasErrors)
                {
                    Console.WriteLine(parser.UsageInfo.GetErrorsAsString(78));

                }

                Trace.WriteLine(DateTime.Now + " -" + "Request received for origin:" + options.o + " destination:" + options.d);

                DataSet result = new DataSet();

                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                string[] items = System.Configuration.ConfigurationManager.AppSettings["filepath"].TrimEnd().Split(',');

                foreach (string item in items)
                {
                    string[] keyValue = item.Split('#');
                    dictionary.Add(keyValue[0], keyValue[1]);//Dictionary with Filepath as key and delimeter as value.
                }

                foreach (KeyValuePair<string, string> file in dictionary)
                {
                    string delimeter = char.ConvertFromUtf32(Convert.ToInt32(file.Value));//Converting ASCII value of delimeter to character.
                    ConvertfilesToDataSet(file.Key, "Combined_Table", delimeter, ref result);
                }

                DataRow[] filghtsfound;
                filghtsfound = result.Tables[0].Select("Origin = '" + options.o + "' and Destination = '" + options.d + "'", "Price, Departure Time ASC");

                if (filghtsfound.Length > 1)
                {
                    Trace.WriteLine(DateTime.Now +" -" +"No. of flights found for -o " + options.o + " -d " + options.d + " ::" + filghtsfound.Length);
                    Console.WriteLine(" {Origin} --> {Destination} ({Departure Time} --> {Destination Time}) - {Price}");
                    for (int f = 0; f < filghtsfound.Length; f++)
                    {
                        Console.WriteLine(filghtsfound[f][0] + " --> " + filghtsfound[f][2] + " " + filghtsfound[f][1] + " --> " + filghtsfound[f][3] + " - " + filghtsfound[f][4]);
                    }
                }
                else
                {
                    Console.WriteLine("No Flights Found for {0} --> {1}", options.o, options.d);
                }

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(DateTime.Now + " -" + "Exception in Main : " + ex.Message);
            }
        }

        public static DataSet ConvertfilesToDataSet(string File, string TableName, string delimiter, ref DataSet result)
        {
            try
            {
                StreamReader s = new StreamReader(File);

                string[] columns = s.ReadLine().Split(delimiter.ToCharArray());

                if (!result.Tables.Contains(TableName))

                    result.Tables.Add(TableName); //if we need different table for each file we can pass different TableName.

                foreach (string col in columns)
                {
                    string columnname = col;

                    if (!result.Tables[TableName].Columns.Contains(columnname))
                    {

                        result.Tables[TableName].Columns.Add(columnname);

                    }
                }

                string fulldata = s.ReadToEnd();

                string[] rows = fulldata.Split("\r\n".ToCharArray());

                //Add each row to the DataSet        
                foreach (string r in rows)
                {

                    string[] values = r.Split(delimiter.ToCharArray());

                    result.Tables[TableName].Rows.Add(values);
                }
               
            }
            catch(Exception ex)
            {
                Trace.WriteLine(DateTime.Now + " -" + "Exception in ConvertfilesToDataSet :" + ex.Message);
            }
            return result;
        }

        [CommandLineManager(ApplicationName = "SearchFilght")]
        class Options
        {
            [CommandLineOption(Description = "Specifies Origin", MinOccurs = 1)]
            public string o
            {
                get { return mo; }
                set
                {
                    if (String.IsNullOrEmpty(value))
                        throw new InvalidOptionValueException("The origin must not be empty", false);
                    mo = value;
                }
            }

            private string mo;

            [CommandLineOption(Description = "Specifies destination.", MinOccurs = 1)]
            public string d
            {
                get { return md; }
                set
                {
                    if (String.IsNullOrEmpty(value))
                        throw new InvalidOptionValueException("The destination must not be empty", false);
                    md = value;
                }
            }

            private string md;
        }

    }
}
