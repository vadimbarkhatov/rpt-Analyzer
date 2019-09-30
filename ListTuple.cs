using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CHEORptAnalyzer
{
    public class XElementWrap
    {
        public string Text;
        public XElement XEle;
        public Dictionary<string, string> SearchResults = new Dictionary<string, string>();

        public override string ToString()
        {
            return Text;
        }
    }
}
