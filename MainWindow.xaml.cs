namespace CHEORptAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Xml;
    using System.Xml.Linq;
    using FastColoredTextBoxNS;
    using LiteDB;
    using Microsoft.WindowsAPICodePack.Dialogs;

    public enum CRElement
    {
        Field,
        Formula,
        Command,
        FormulaField,
        TableLinks,
        Parameters,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        static readonly string localDBPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CRPTApp.db";

        #region CR Section Definitions

        static readonly Dictionary<CRElement, CRSection> CRSections = new Dictionary<CRElement, CRSection>
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
            [CRElement.FormulaField] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.Custom,
                ResultFilter = x => x.Where(y => y.Name == "DataDefinition").Elements("FormulaFieldDefinitions").Elements("FormulaFieldDefinition"),
                ResultFormat = s =>
                    s.Select(x =>
                        x.Attribute("FormulaName").Value +
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

                ResultFormat = s => //TODO: Could definately be better...
                s.Where(x => x.Attribute("IsLinkedToSubreport") != null).Select(y => y.Attribute("Name").Value + " -> " + y.Attribute("ReportName").Value).Concat(
                s.Where(y => y.Attribute("IsLinkedToSubreport") == null && true).Select(y => y.Attribute("Name").Value)).Combine("\r\n" + "\r\n")



                //Where(x => false)
                //    .Select(x => //Parameters that are linked to a subreport have a different schema and need to be handled seperately
                //        x.Attribute("IsLinkedToSubreport") != null ? "{" + x.Attribute("Name").Value + "} : " + x.Attribute("ReportName").Value
                //        : //""
                //        x.Attribute("ReportName").Value == "" ? x.Attribute("FormulaName").Value + " : " + x.Attribute("ReportName").Value : ""
                //    )



                //ResultFormat = s =>
                //    s.Select(x =>
                //        x.Attribute("JoinType").Value + " "
                //        + x.Elements("SourceFields").Elements("Field").First().Attribute("FormulaName").Value + " ON "
                //        + x.Elements("DestinationFields").Elements("Field").First().Attribute("FormulaName").Value)
                //     .Combine("\r\n" + "\r\n"),
            },
        };

        #endregion CR Section Definitions

        #region GUI Bound Properties

        public bool SearchFields { get; set; } = true;
        public bool SearchRF { get; set; } = true;
        public bool SearchCommand { get; set; } = true;
        public bool ContainsSeach { get; set; } = true;
        public string SearchString { get; set; } = string.Empty;
        public CRElement PreviewElement { get; set; } = CRElement.Field;
        public BindingList<ReportItem> ReportItems { get; set; } = new BindingList<ReportItem>();
        public IEnumerable<ReportItem> SelectedReportItems
        {
            get { return tvReports.SelectedItems.Count > 0 ? tvReports.SelectedItems.Cast<ReportItem>() : null; }
        }
        #endregion GUI Bound Properties

        XElement Xroot = new XElement("null");

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private bool TextFilter(string text)
        {
            return text.IndexOf(SearchString.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs events) => SearchReports();

        private void SearchReports()
        {
            Func<IEnumerable<XElement>, IEnumerable<XElement>> reportFilter =
                         x => CRSections[CRElement.Field].ResultFilter(x).Gate(SearchFields)
                                .Concat(CRSections[CRElement.Formula].ResultFilter(x).Gate(SearchRF))
                                .Concat(CRSections[CRElement.Command].ResultFilter(x).Gate(SearchCommand))
                                .Where(s => TextFilter(s.Value));

            IEnumerable<XElement> foundReports = Xroot.Elements("Report").Where(x => ContainsSeach == reportFilter(x.Descendants()).Count() > 0);

            ReportItems.Clear();

            foreach (XElement report in foundReports)
            {
                IEnumerable<XElement> flattenedReport = FlattenReport(report);

                var baseReport = flattenedReport.First();
                var reportItem =
                    new ReportItem
                    {
                        Text = baseReport.Attribute("Name").Value,
                        DisplayResults = ReportResults(baseReport),
                        XMLView = report,
                        FilePath = report.Attribute("FileName").Value,
                        Author = report.Element("Summaryinfo").Attribute("ReportAuthor").Value,
                        //TODO: LastSaved 
                        HasSavedData = report.Attribute("HasSavedData").Value == "True" ? true : false,
                        ReportComment = report.Element("Summaryinfo").Attribute("ReportComments").Value,
                    };

                foreach (var subReport in flattenedReport.Skip(1))
                {
                    Dictionary<CRElement, string> results = ReportResults(subReport);

                    reportItem.SubReports.Add(new ReportItem { Text = subReport.Attribute("Name").Value, DisplayResults = results, BaseReport = reportItem });
                }

                ReportItems.Add(reportItem);
            }

        }

        private static IEnumerable<XElement> FlattenReport(XElement report)
        {
            var baseReport = new XElement("BaseReport");
            baseReport.Add(report.Elements().Where(x => x.Name != "SubReports"));
            baseReport.SetAttributeValue("Name", Path.GetFileNameWithoutExtension(report.Attribute("FileName").Value));

            var flattenedReports = new[] { baseReport }.Concat(report.Elements("SubReports").Elements("Report"));

            return flattenedReports;
        }


        private static Dictionary<CRElement, string> ReportResults(XElement report)
        {
            var results = new Dictionary<CRElement, string>();

            foreach (CRElement crElement in CRSections.Keys)
            {
                results[crElement] = report.Descendants()
                    .Apply(CRSections[crElement].ResultFilter)
                    .Apply(CRSections[crElement].ResultFormat);
            }

            return results;
        }

        private void TvReports_SelectionChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) => UpdatePreview();

        private void UpdatePreview()
        {
            var selectedResults = "";

            if (SelectedReportItems != null)
            {
                selectedResults = SelectedReportItems.First().DisplayResults[PreviewElement];
            }

            textBox.ClearStylesBuffer();
            textBox.Language = CRSections[PreviewElement].Language;
            textBox.Text = selectedResults;

            TextStyle searchStyle = new TextStyle(null, System.Drawing.Brushes.Yellow, System.Drawing.FontStyle.Regular);
            textBox.AddStyle(searchStyle);
            textBox.Range.ClearStyle(searchStyle);
            textBox.Range.SetStyle(searchStyle, Regex.Escape(SearchString.Trim()), RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (CRSections[PreviewElement].Language == FastColoredTextBoxNS.Language.Custom) Extensions.CrystalSyntaxHighlight(textBox);
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                InitialDirectory = @"C:\test\",
                IsFolderPicker = true,
                Multiselect = true,
            };


            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                IEnumerable<string> directories = dialog.FileNames.SelectMany(x => Directory.GetDirectories(x, "*.*", SearchOption.AllDirectories)).Concat(dialog.FileNames);


                IEnumerable<string> rptPaths = ParseRPT(directories);

                LoadXMLFromDb(rptPaths, localDBPath);
                SearchReports();
            }
        }

        private IEnumerable<string> ParseRPT(IEnumerable<string> directories)
        {
            string dbLoc = localDBPath;

            //if (new Uri(directories.First()).Host == "") //if path is non UNC
            //{
            //dbLoc = localDBPath;
            //}

            List<string> rptPaths = new List<string>();

            foreach (var dir in directories)
            {
                var matchingFiles = Directory.GetFiles(dir, searchPattern: "*.rpt");
                rptPaths.AddRange(matchingFiles);
            }

            RptToXml.RptToXml.Convert(rptPaths, dbLoc, false);

            return rptPaths;
        }

        private void LoadXMLFromDb(IEnumerable<string> rptPaths, string dbPath)
        {
            Xroot = new XElement("Reports");

            using (var db = new LiteDatabase(dbPath))
            {
                foreach (string rptPath in rptPaths)
                {
                    string rptPathId = Extensions.CalculateMD5Hash(rptPath);

                    XElement xelement;
                    try
                    {
                        xelement = XElement.Load(db.FileStorage.OpenRead(rptPathId));
                    }
                    catch (XmlException ex)
                    {
                        Logs.Instance.log.Error(ex.Message, ex);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logs.Instance.log.Error(ex.Message, ex);
                        continue;
                    }

                    Xroot.Add(xelement);
                }
            }
        }


        private void PreviewReport(object sender, RoutedEventArgs e)
        {
            CRViewer crViewer = new CRViewer();
            crViewer.Show();
            crViewer.LoadReport(SelectedReportItems?.First().GetBaseReport().FilePath);
        }

        private void ExportToXML(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML-File | *.xml";
            saveFileDialog.InitialDirectory = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents");

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //TODO: error handling
                SelectedReportItems?.First().GetBaseReport().XMLView?.Save(saveFileDialog.FileName);
            }
        }

        private void OpenReportDesigner(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(SelectedReportItems?.First().GetBaseReport().FilePath);
            }
            catch (Exception ex)
            {
                Logs.Instance.log.Error(ex.Message, ex);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
