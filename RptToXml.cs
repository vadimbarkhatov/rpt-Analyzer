using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LiteDB;

namespace RptToXml
{
	public class RptToXml
	{
		public static void Convert(string rptLoc, string liteDBPath, bool forceRefresh = false)
		{
			//if (args.Length < 1)
			//{
				//Console.WriteLine("Usage: RptToXml.exe <RPT filename | wildcard> [outputfilename]");
				//Console.WriteLine("       outputfilename argument is valid only with single filename in first argument");
				//return;
			//}

			string rptPathArg = rptLoc;
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

            using (var db = new LiteDatabase(liteDBPath))
            {
                foreach (string rptPath in rptPaths)
                {
                    Trace.WriteLine("Dumping " + rptPath);
                    FileInfo rptFile = new FileInfo(rptPath);

                    //if (!forceRefresh && xmlFile.LastWriteTime > rptFile.LastWriteTime) continue;

                    string id = CHEORptAnalyzer.Extensions.ToLiteDBID(rptPath);

                    Stream stream = new MemoryStream();

                    try
                    {
                        using (var writer = new RptDefinitionWriter(rptPath))
                        {
                            writer.WriteToXml(stream);
                            db.FileStorage.Upload(id, "empty", stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.Write(ex);
                        System.Windows.Forms.MessageBox.Show(ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
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
