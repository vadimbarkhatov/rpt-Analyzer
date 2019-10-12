using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CHEORptAnalyzer.MainWindow;

namespace CHEORptAnalyzer
{
    public class XElementWrap
    {
        public string Text;
        public Dictionary<CRElement, string> SearchResults = new Dictionary<CRElement, string>();

        public override string ToString()
        {
            return Text;
        }
    }
}
