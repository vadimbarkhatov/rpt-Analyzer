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
		public static void Convert(IEnumerable<string> rptPaths, string liteDBPath, bool forceRefresh = false)
		{
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            using (var db = new LiteDatabase(liteDBPath))
            {
                foreach (string rptPath in rptPaths)
                {
                    string id = CHEORptAnalyzer.Extensions.CalculateMD5Hash(rptPath);

                    Trace.WriteLine("Dumping " + rptPath);
                    FileInfo rptFile = new FileInfo(rptPath);
                    LiteFileInfo xmlFile = db.FileStorage.FindById(id);

                    if(rptFile.Exists && xmlFile != null)
                    {
                        if (!forceRefresh && xmlFile.UploadDate > rptFile.LastWriteTime) continue;
                    }

                    Stream stream = new MemoryStream();

                    try
                    {
                        using (var writer = new RptDefinitionWriter(rptPath))
                        {
                            writer.WriteToXml(stream);
                            stream.Position = 0;
                            db.FileStorage.Upload(id, "empty", stream);

                            var filePathMeta = new BsonDocument();
                            filePathMeta["fullPath"] = rptPath;
                            db.FileStorage.SetMetadata(id, filePathMeta);
                        }
                    }
                    catch (Exception ex)
                    {
                        //Logs.Instance.log.Error(ex.Message, ex);
                        System.Windows.Forms.MessageBox.Show("Exception with report: " + "\r\n" + rptPath + "\r\n"  + ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                }
            }
		}
	}
}
