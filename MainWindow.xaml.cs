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
    }

    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        static readonly string rptPath = @"C:\test\Reports\*";
        static readonly string localDBPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CRPTApp.db";
        static readonly string sharedDBPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\SharedCache.db";

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
                        + x.Elements("SourceFields").Elements("Field").First().Attribute("FormulaName").Value + " ON "
                        + x.Elements("DestinationFields").Elements("Field").First().Attribute("FormulaName").Value)
                     .Combine("\r\n" + "\r\n"),
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
        public BindingList<ReportItem> SelectedReportItems { get; set; } = new BindingList<ReportItem>();
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

        private void LoadXML(IEnumerable<string> folders, string dbPath, bool CleanupOrphans)
        {
            Xroot = new XElement("Reports");

            using (var db = new LiteDatabase(dbPath))
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
                var reportItem = new ReportItem { Text = baseReport.Attribute("Name").Value, DisplayResults = ReportResults(baseReport) };

                foreach (var subReport in flattenedReport.Skip(1))
                {
                    Dictionary<CRElement, string> results = ReportResults(subReport);



                    reportItem.SubReports.Add(new ReportItem { Text = subReport.Attribute("Name").Value, DisplayResults = results });
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
            var selectedResults = "ERROR";

            if (tvReports?.SelectedItems.Count > 0)
            {
                ReportItem selectedItem = (ReportItem)tvReports.SelectedItems[0];

                selectedResults = selectedItem.DisplayResults[PreviewElement];
            }
            else return;

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
                ParseRPT(directories);
                
                //TODO: Refactor
                LoadXML(dialog.FileNames, localDBPath, true);
                SearchReports();
            }
        }

        private void ParseRPT(IEnumerable<string> paths)
        {
            string dbLoc = localDBPath;

            if (new Uri(paths.First()).Host == "") //if path is non UNC
            {
                dbLoc = localDBPath;
            }

            foreach (string path in paths)
            {
                string rptPath = path + @"\*";

                RptToXml.RptToXml.Convert(rptPath, dbLoc, false);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {

        }

        private void test(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdatePreview();
        }

        private void PreviewReport(object sender, RoutedEventArgs e)
        {
            CRViewer crViewer = new CRViewer();

            crViewer.Show();
        }
    }
}
