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
