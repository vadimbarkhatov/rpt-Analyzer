using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CHEORptAnalyzer.MainWindow;

namespace CHEORptAnalyzer
{
    public class ReportItem
    {
        public string Text
        {
            get => _text + (SubReports.Count > 0 ? " [" + SubReports.Count + "]" : "");
            set => _text = value;
        }
        public Dictionary<CRElement, string> DisplayResults = new Dictionary<CRElement, string>();
        private string _text;

        public XElement XMLView { get; set; }
        


        public string FilePath { get; set; } = "";
        public string Author { get; set; } = "";
        public DateTime LastSaved { get; set; } = new DateTime();
        public bool HasSavedData { get; set; } = false;
        public string ReportComment { get; set; } = "";

        public ReportItem BaseReport { get; set; }
        public BindingList<ReportItem> SubReports { get; set; }

        public ReportItem()
        {
            this.SubReports = new BindingList<ReportItem>();
        }

        public ReportItem GetBaseReport()
        {
            if(BaseReport != null)
            {
                return BaseReport;
            }

            return this;
        }

        public string GetInfo()
        {
            if (BaseReport == null)
            {
                return "Path: " + Directory.GetParent(FilePath) + "\r\n" +
                        "Author: " + Author + "\r\n" +
                        //"Last Saved:"
                        "Has Saved Data: " + HasSavedData + "\r\n" +
                        "Comments: " + ReportComment + "\r\n";
            }
            else
            {
                return BaseReport.GetInfo();
            } 
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
