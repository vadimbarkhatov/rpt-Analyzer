using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CHEORptAnalyzer.MainWindow;

namespace CHEORptAnalyzer
{
    public enum CRElement
    {
        Field,
        Formula,
        Command,
        FormulaField,
        GroupFormula,
        TableLinks,
        Parameters,
    }

    public class ReportItem
    {
        public static readonly Dictionary<CRElement, CRSection> CRSections = new Dictionary<CRElement, CRSection>
        {
            [CRElement.Field] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.Custom,
                ResultFilter = x => x.Descendants("Tables").Descendants("Field"),
            },
            [CRElement.Command] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.SQL,
                ResultFilter = x => x.Descendants("Command"),
            },
            [CRElement.Formula] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.Custom,
                ResultFilter = x => x.Where(y => y.Name == "DataDefinition").Elements("RecordSelectionFormula"),
            },
            [CRElement.GroupFormula] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.Custom,
                ResultFilter = x => x.Where(y => y.Name == "DataDefinition").Elements("GroupSelectionFormula"),
            },
            [CRElement.FormulaField] = new CRSection
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
            },
            [CRElement.TableLinks] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.Custom,
                ResultFilter = x => x.Descendants("TableLinks").Elements("TableLink"),
                ResultFormat = s =>
                    s.Select(x =>
                        x.Attribute("JoinType").Value + " "
                        + x.Elements("SourceFields").Elements("Field").First().Attribute("FormulaName").Value + " On "
                        + x.Elements("DestinationFields").Elements("Field").First().Attribute("FormulaName").Value)
                     .Combine("\r\n" + "\r\n"),
            },
            [CRElement.Parameters] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.Custom,
                ResultFilter = x => x.Where(y => y.Name == "DataDefinition").Elements("ParameterFieldDefinitions").Elements("ParameterFieldDefinition"),

                ResultFormat = s =>
                    s.Select(x => //Parameters that are linked to a subreport have a different schema and need to be handled seperately
                        x.Attribute("IsLinkedToSubreport") != null ?
                        "{" + x.Attribute("Name").Value + "} -> \"" + x.Attribute("ReportName").Value + "\"" :
                        (x.Attribute("ParameterFieldUsage").Value == "NotInUse" ? "//" : "") + x.Attribute("FormulaName").Value + " : " + x.Attribute("ValueType").Value.Replace("Field", "")
                    ).Combine("\r\n"),
            },
        };

        //Parameters,

        public static readonly CRSection CRField = new CRSection
        {
            Language = FastColoredTextBoxNS.Language.Custom,
            ResultFilter = x => x.Descendants("Tables").Descendants("Field"),
        };
        public static readonly CRSection CRCommand = new CRSection
        {
            Language = FastColoredTextBoxNS.Language.SQL,
            ResultFilter = x => x.Descendants("Command"),
        };
        public static readonly CRSection CRRecordFormula = new CRSection
        {
            Language = FastColoredTextBoxNS.Language.Custom,
            ResultFilter = x => x.Where(y => y.Name == "DataDefinition").Elements("RecordSelectionFormula"),
        };
        public static readonly CRSection CRFormulaFields = new CRSection
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
        public static readonly CRSection CRGroupFormula = new CRSection
        {
            Language = FastColoredTextBoxNS.Language.Custom,
            ResultFilter = x => x.Where(y => y.Name == "DataDefinition").Elements("GroupSelectionFormula"),
        };
        public static readonly CRSection CRTableLinks = new CRSection
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
        public static readonly CRSection CRParameters = new CRSection
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


        public string Text
        {
            get => XMLData.Attribute("Name").Value + (SubReports.Count > 0 ? " [" + SubReports.Count + "]" : "");
            set => _text = value;
        }
        public Dictionary<CRElement, string> DisplayResults = new Dictionary<CRElement, string>();
        private string _text;

        public XElement XMLView { get; set; }

        private XElement XMLData { get; set; }

        public string FilePath { get; set; } = "";
        public string Author { get; set; } = "";
        public DateTime LastSaved { get; set; } = new DateTime();
        public bool HasSavedData { get; set; } = false;
        public string ReportComment { get; set; } = "";

        public ReportItem BaseReport { get; set; }
        public BindingList<ReportItem> SubReports { get; set; }


        public ReportItem(XElement XMLData, ReportItem baseReport = null)
        {

            this.XMLData = XMLData;
            this.BaseReport = baseReport;
            this.SubReports = new BindingList<ReportItem>();
        }

        public ReportItem GetBaseReport()
        {
            if(BaseReport != null)
            {
                return BaseReport;
            }

            return this;
        }

        public string GetInfo()
        {
            if (BaseReport == null)
            {
                return "Path: " + Directory.GetParent(FilePath) + "\r\n" +
                        "Author: " + Author + "\r\n" +
                        //"Last Saved:"
                        "Has Saved Data: " + HasSavedData + "\r\n";
            }
            else
            {
                return BaseReport.GetInfo();
            } 
        }

        public string GetSection(CRElement crSection)
        {
            return XMLData.Elements()
                    .Apply(CRSections[crSection].ResultFilter)
                    .Apply(CRSections[crSection].ResultFormat);
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
