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
        public static readonly CRSection Field = new CRSection
        {
            Language = FastColoredTextBoxNS.Language.Custom,
            ResultFilter = x => x.Descendants("Tables").Descendants("Field"),
        };
        public static readonly CRSection Command = new CRSection
        {
            Language = FastColoredTextBoxNS.Language.SQL,
            ResultFilter = x => x.Descendants("Command"),
        };
        public static readonly CRSection RecordFormula = new CRSection
        {
            Language = FastColoredTextBoxNS.Language.Custom,
            ResultFilter = x => x.Where(y => y.Name == "DataDefinition").Elements("RecordSelectionFormula"),
        };
        public static readonly CRSection FormulaFields = new CRSection
        {
            Language = FastColoredTextBoxNS.Language.Custom,
            ResultFilter = x => x.Where(y => y.Name == "DataDefinition").Elements("FormulaFieldDefinitions").Elements("FormulaFieldDefinition"),
            ResultFormat = s =>
                s.Select(x =>
                    x.Attribute("FormulaName").Value + " : " + x.Attribute("ValueType").Value.Replace("Field", "") +
                    "\r\n" + "{" +
                    "\r\n" + x.Value.AppendToNewLine("\t") +
                    "\r\n" + "}")
                 .Combine("\r\n" + "\r\n"),
        };
        public static readonly CRSection GroupFormula = new CRSection
        {
            Language = FastColoredTextBoxNS.Language.Custom,
            ResultFilter = x => x.Where(y => y.Name == "DataDefinition").Elements("GroupSelectionFormula"),
        };
        public static readonly CRSection TableLinks = new CRSection
        {
            Language = FastColoredTextBoxNS.Language.Custom,
            ResultFilter = x => x.Descendants("TableLinks").Elements("TableLink"),
            ResultFormat = s =>
                s.Select(x =>
                    x.Attribute("JoinType").Value + " "
                    + x.Elements("SourceFields").Elements("Field").First().Attribute("FormulaName").Value + " On "
                    + x.Elements("DestinationFields").Elements("Field").First().Attribute("FormulaName").Value)
                 .Combine("\r\n" + "\r\n"),
        };
        public static readonly CRSection Parameters = new CRSection
        {
            Language = FastColoredTextBoxNS.Language.Custom,
            ResultFilter = x => x.Where(y => y.Name == "DataDefinition").Elements("ParameterFieldDefinitions").Elements("ParameterFieldDefinition"),

            ResultFormat = s =>
                s.Select(x => //Parameters that are linked to a subreport have a different schema and need to be handled seperately
                    x.Attribute("IsLinkedToSubreport") != null ?
                    "{" + x.Attribute("Name").Value + "} -> \"" + x.Attribute("ReportName").Value + "\"" :
                    (x.Attribute("ParameterFieldUsage").Value == "NotInUse" ? "//" : "") + x.Attribute("FormulaName").Value + " : " + x.Attribute("ValueType").Value.Replace("Field", "")
                ).Combine("\r\n"),
        };


        public FastColoredTextBoxNS.Language Language;
        public Func<IEnumerable<XElement>, IEnumerable<XElement>> ResultFilter;
        public Func<IEnumerable<XElement>, string> ResultFormat 
            = s => s.Select(x => x.Value).Combine(Environment.NewLine);


    }
}
