using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RptToXml
{
	public class RptToXml
	{
		public static void Convert(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: RptToXml.exe <RPT filename | wildcard> [outputfilename]");
				Console.WriteLine("       outputfilename argument is valid only with single filename in first argument");
				return;
			}

			string rptPathArg = args[0];
			bool wildCard = rptPathArg.Contains("*");
			if (!wildCard && !ReportFilenameValid(rptPathArg))
				return;


			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

			var rptPaths = new List<string>();
			if (!wildCard)
			{
				rptPaths.Add(rptPathArg);
			}
			else
			{
				var directory = Path.GetDirectoryName(rptPathArg);
                if (string.IsNullOrEmpty(directory))
                {
                    directory = ".";
                }
                var matchingFiles = Directory.GetFiles(directory, Path.GetFileName(rptPathArg), SearchOption.AllDirectories);
                rptPaths.AddRange(matchingFiles.Where(ReportFilenameValid));
			}

			if (rptPaths.Count == 0)
			{
				Trace.WriteLine("No reports matched the wildcard.");
			}

            //var xmlHolder = System.Xml.XmlWriter.Create(;

			foreach (string rptPath in rptPaths)
			{
				Trace.WriteLine("Dumping " + rptPath);
                string xmlPath;

                if (args.Length > 1)
                {
                    xmlPath = args[1] + "[" + CalculateMD5Hash(rptPath) + "]" + Path.GetFileName(rptPath);
                }
                else
                {
                    xmlPath = rptPath;
                }


                //xmlPath = Path.ChangeExtension(xmlPath, "xml");
                MemoryStream test = new MemoryStream();



                try
                {
                    using (var writer = new RptDefinitionWriter(rptPath))
                    {
                        writer.WriteToXml(test);
                        
                        //writer.WriteToXml()
                    }
                }
                catch (Exception ex)
                {
                    Trace.Write(ex.Message);

                }

			}
		}

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        static bool ReportFilenameValid(string rptPath)
		{
			string extension = Path.GetExtension(rptPath);
			if (String.IsNullOrEmpty(extension) || !extension.Equals(".rpt", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Input filename [" + rptPath + "] does not end in .RPT");
				return false;
			}

			if (!File.Exists(rptPath))
			{
				Console.WriteLine("Report file [" + rptPath + "] does not exist.");
				return false;
			}

			return true;
		}
	}
}
