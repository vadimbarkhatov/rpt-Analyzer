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
        public string Text;
        public string Test = "test";
        public Dictionary<CRElement, string> DisplayResults = new Dictionary<CRElement, string>();

        public override string ToString()
        {
            return Text;
        }
    }
}
