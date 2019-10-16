namespace CHEORptAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
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
        FormulaField
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        const string rptPath = @"C:\test\Reports\*";
        static readonly string CacheFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\ReportCache\";
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
                ResultFilter = x => x.Descendants("RecordSelectionFormula"),
            },
            [CRElement.FormulaField] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.Custom,
                ResultFilter = x => x.Descendants("FormulaFieldDefinition"),
                ResultFormat = s =>
                    s.Select(x =>
                        x.Attribute("FormulaName").Value +
                        "\r\n" + "{" +
                        "\r\n" + x.Value.AppendToNewLine("\t") +
                        "\r\n" + "}")
                     .Combine("\r\n" + "\r\n"),
            },
        };

        public bool SearchFields { get; set; } = true;
        public bool SearchRF { get; set; } = true;
        public bool SearchCommand { get; set; } = true;
        public bool ContainsSeach { get; set; } = true;
        public string SearchString { get; set; } = string.Empty;
        public CRElement PreviewElement { get; set; } = CRElement.Field;
        public BindingList<ReportItem> ReportItems { get; set; } = new BindingList<ReportItem>();
        public BindingList<Dictionary<CRElement, string>> SubReports { get; set; }
            = new BindingList<Dictionary<CRElement, string>>
            { new Dictionary<CRElement, string> { [CRElement.Field] = "test"}};
            
        // = new BindingList<Dictionary<CRElement, string>>();

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

        private void LoadXML(IEnumerable<string> folders, bool CleanupOrphans)
        {
            Xroot = new XElement("Reports");

            using (var db = new LiteDatabase(@"C:\test\MyData.db"))
            {
                foreach (string folder in folders)
                {
                    string folderId = Extensions.ToLiteDBID(folder) + "/";
                    IEnumerable<LiteFileInfo> reportIds = db.FileStorage.Find(folderId);

                    foreach (LiteFileInfo reportId in reportIds)
                    {
                        string fullPath = reportId.Metadata["fullPath"];

                        if (CleanupOrphans && fullPath != null && !File.Exists(fullPath))
                        {
                            db.FileStorage.Delete(reportId.Id);
                            continue;
                        }

                        XElement xelement;
                        try
                        {
                            xelement = XElement.Load(db.FileStorage.OpenRead(reportId.Id));
                        }
                        catch (XmlException ex)
                        {
                            Logs.Instance.log.Error(ex.Message, ex);
                            continue;
                        }

                        Xroot.Add(xelement);
                    }
                }
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs events)
        {
            SearchReports();
        }

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
                ReportItem reportItem = new ReportItem();
                reportItem.Text = report.Attribute("FileName").Value;
                IEnumerable<XElement> flattenedReport = FlattenReport(report);

                foreach (var subReport in flattenedReport)
                {
                    Dictionary<CRElement, string> results = ReportResults(subReport.Descendants());

                    reportItem.DisplayResults.Add(results);
                }

                ReportItems.Add(reportItem);
            }
        }

        private static IEnumerable<XElement> FlattenReport(XElement report)
        {
            var mainReport = new XElement("BaseReport");
            mainReport.Add(report.Elements().Where(x => x.Name != "SubReports"));
            mainReport.SetAttributeValue("Name", mainReport.Element("Summaryinfo").Attribute("ReportTitle").Value);
            var flattenedReports = new[] { mainReport }.Concat(report.Elements("SubReports").Elements("Report"));
            return flattenedReports;
        }


        private static Dictionary<CRElement, string> ReportResults(IEnumerable<XElement> report)
        {
            var results = new Dictionary<CRElement, string>();

            foreach (CRElement crElement in CRSections.Keys)
            {
                results[crElement] = report
                    .Apply(CRSections[crElement].ResultFilter)
                    .Apply(CRSections[crElement].ResultFormat);
            }

            return results;
        }

        private void LbReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SubReports = lbReports?.SelectedItem as BindingList<Dictionary<CRElement, string>>;
            UpdatePreview();

        }
           // => UpdatePreview();

        private void RadioButton_Checked(object sender, RoutedEventArgs e) => UpdatePreview();

        private void UpdatePreview()
        {
            var selectedResults = (lbReports?.SelectedItem as ReportItem)?.DisplayResults[0][PreviewElement] ?? "";

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

            IEnumerable<string> directories;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                directories = dialog.FileNames.SelectMany(x => Directory.GetDirectories(x, "*.*", SearchOption.AllDirectories)).Concat(dialog.FileNames);
                ParseRPT(directories);
                
                LoadXML(dialog.FileNames, true);
                SearchReports();
            }
        }

        private void ParseRPT(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                string rptPath = path + @"\*";

                RptToXml.RptToXml.Convert(rptPath, @"C:\test\MyData.db", false);
            }
        }
    }
}
