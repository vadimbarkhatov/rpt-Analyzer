using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public string FilePath { get; set; } = "";
        public string Author { get; set; } = "";
        public DateTime LastSaved { get; set; } = new DateTime();
        public bool HasSavedData { get; set; } = false;
        public string SummaryInfo { get; set; } = "";

        public BindingList<ReportItem> SubReports { get; set; }

        public ReportItem()
        {
            this.SubReports = new BindingList<ReportItem>();
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
