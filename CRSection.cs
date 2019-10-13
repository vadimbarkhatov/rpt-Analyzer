using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CHEORptAnalyzer
{
    class CRSection
    {
        public FastColoredTextBoxNS.Language Language;
        public Func<IEnumerable<XElement>, IEnumerable<XElement>> ResultFilter;
        public Func<IEnumerable<XElement>, string> ResultFormat 
            = s => s.Select(x => x.Value).Combine(Environment.NewLine);

        //public Func<IEnumerable<XElement>, string> ResultFormat2
            //= s => string.Join("\r", s.Select(x =>x.Attribute("FormulaName") + "\t" + x.Value));
    }
}
