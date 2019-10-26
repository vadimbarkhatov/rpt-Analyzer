using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CHEORptAnalyzer
{
    public class CRSection
    {
        public FastColoredTextBoxNS.Language Language;
        public Func<IEnumerable<XElement>, IEnumerable<XElement>> ResultFilter;
        public Func<IEnumerable<XElement>, string> ResultFormat 
            = s => s.Select(x => x.Value).Combine(Environment.NewLine);
    }
}
