using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RptToXml
{
	public class RptToXml
	{
		public static void Convert(string[] args, bool forceRefresh = false)
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
                if (String.IsNullOrEmpty(directory))
                {
                    directory = ".";
                }
                var matchingFiles = Directory.GetFiles(directory, searchPattern: Path.GetFileName(rptPathArg));
                rptPaths.AddRange(matchingFiles.Where(ReportFilenameValid));
			}

			if (rptPaths.Count == 0)
			{
				Trace.WriteLine("No reports matched the wildcard.");
			}

			foreach (string rptPath in rptPaths)
			{
				Trace.WriteLine("Dumping " + rptPath);
                string xmlPath;

                if (args.Length > 1)
                {
                    xmlPath = args[1] + Path.GetFileName(rptPath);
                }
                else
                {
                    xmlPath = rptPath;
                }

                xmlPath = Path.ChangeExtension(xmlPath, "xml");

                FileInfo xmlFile = new FileInfo(xmlPath);
                FileInfo rptFile = new FileInfo(rptPath);

                if (!forceRefresh && xmlFile.LastWriteTime > rptFile.LastWriteTime) continue;

                try
                {
                    using (var writer = new RptDefinitionWriter(rptPath))
                    {
                        writer.WriteToXml(xmlPath);
                    }
                }
                catch (Exception ex)
                {
                    //Trace.Write(ex.Message);
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
			}
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
