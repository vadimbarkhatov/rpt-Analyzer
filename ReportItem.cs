using System;
using System.Collections.Generic;
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
        public List<Dictionary<CRElement, string>> DisplayResults = new List<Dictionary<CRElement, string>>();


        public override string ToString()
        {
            return Text;
        }
    }
}
