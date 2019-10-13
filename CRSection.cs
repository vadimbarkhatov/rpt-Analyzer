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
        public FastColoredTextBoxNS.Language Language;// = FastColoredTextBoxNS.Language.Custom;
        public Func<IEnumerable<XElement>, IEnumerable<XElement>> ResultFilter;

    }
}
