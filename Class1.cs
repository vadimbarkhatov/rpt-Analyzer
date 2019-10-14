using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FastColoredTextBoxNS;

namespace CHEORptAnalyzer
{
    //public class FCTextBox : SyntaxHighlighter
    //{
    //    public virtual void HighlightSyntax(string XMLdescriptionFile, Range range)
    //    {
    //        SyntaxDescriptor desc = null;
    //        if (!descByXMLfileNames.TryGetValue(XMLdescriptionFile, out desc))
    //        {
    //            var doc = new XmlDocument();
    //            string file = XMLdescriptionFile;
    //            if (!File.Exists(file))
    //                file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(file));

    //            doc.LoadXml(File.ReadAllText(file));
    //            desc = ParseXmlDescription(doc);
    //            descByXMLfileNames[XMLdescriptionFile] = desc;
    //        }

    //        HighlightSyntax(desc, range);
    //    }
    //}
}
