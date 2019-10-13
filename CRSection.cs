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
        readonly FastColoredTextBoxNS.Language lang;// = FastColoredTextBoxNS.Language.Custom;
        readonly Func<IEnumerable<XElement>, IEnumerable<XElement>> resultFilter;

    }
}
