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
    using System.Threading.Tasks;
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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        static readonly string localSaveDir = Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CHEORptAnalyzer").FullName;
        static readonly string localDBPath = localSaveDir + "\\CRPTApp.db";
        public string Version { get; } = "0.9.2";

        #region GUI Bound Properties

        public bool SearchFields { get; set; } = true;
        public bool SearchRF { get; set; } = true;
        public bool SearchCommand { get; set; } = true;
        public bool ContainsSeach { get; set; } = true;
        public string SearchString { get; set; } = string.Empty;

        public string ReportInfo { get; set; } = string.Empty;
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
                         x => CRSection.Field.ResultFilter(x).Gate(SearchFields)
                                .Concat(CRSection.RecordFormula.ResultFilter(x).Gate(SearchRF))
                                .Concat(CRSection.Command.ResultFilter(x).Gate(SearchCommand))
                                .Where(s => TextFilter(s.Value));

            IEnumerable<XElement> foundReports = Xroot.Elements("Report").Where(x => ContainsSeach == reportFilter(x.Descendants()).Count() > 0);

            ReportItems.Clear();

            foreach (XElement report in foundReports)
            {
                IEnumerable<XElement> flattenedReport = FlattenReport(report);

                XElement baseReport = flattenedReport.First();
                var baseReportItem = new ReportItem(baseReport, null)
                {
                    XMLView = report,
                    FilePath = report.Attribute("FileName").Value,
                    Author = report.Element("Summaryinfo").Attribute("ReportAuthor").Value,
                    HasSavedData = report.Attribute("HasSavedData").Value == "True" ? true : false,
                    ReportComment = report.Element("Summaryinfo").Attribute("ReportComments").Value
                };

                foreach (XElement subReport in flattenedReport.Skip(1))
                {
                    baseReportItem.SubReports.Add(new ReportItem(subReport, baseReportItem));
                }
                ReportItems.Add(baseReportItem);
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
                var SelectedReportItem = SelectedReportItems.First();

                selectedResults = SelectedReportItem.GetSection(PreviewElement);

                lbReportInfo.Text = SelectedReportItem.GetInfo();
            }

            textBox.ClearStylesBuffer();
            textBox.Language = ReportItem.CRSections[PreviewElement].Language;
            textBox.Text = selectedResults;

            TextStyle searchStyle = new TextStyle(null, System.Drawing.Brushes.Yellow, System.Drawing.FontStyle.Regular);
            textBox.AddStyle(searchStyle);
            textBox.Range.ClearStyle(searchStyle);
            textBox.Range.SetStyle(searchStyle, Regex.Escape(SearchString.Trim()), RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (ReportItem.CRSections[PreviewElement].Language == FastColoredTextBoxNS.Language.Custom) Extensions.CrystalSyntaxHighlight(textBox);
        }

        private async void OpenFolder(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = true,
            };


            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                LoadingProgress loadingProgress = new LoadingProgress();

                loadingProgress.Owner = this;
                loadingProgress.Show();
                loadingProgress.Cursor = System.Windows.Input.Cursors.Wait;
                this.IsEnabled = false;

                loadingProgress.txtLoadingText.Text = "Searching folders...";
                var progress = new Progress<string>(s =>
                {
                    loadingProgress.txtLoadingText.Text = s;
                });

                IEnumerable<string> directories = dialog.FileNames.SelectMany(x => Directory.GetDirectories(x, "*.*", SearchOption.AllDirectories)).Concat(dialog.FileNames);

                IEnumerable<string> rptPaths = await Task.Run(() => ParseRPT(directories, progress));
                Xroot = await Task.Run(() => LoadXMLFromDb(rptPaths, localDBPath));

                SearchReports();

                loadingProgress.Close();
                this.IsEnabled = true;
                this.Focus();
            }
        }

        private static IEnumerable<string> ParseRPT(IEnumerable<string> directories, IProgress<string> progress)
        {
            string dbLoc = localDBPath;

            List<string> rptPaths = new List<string>();

            foreach (var dir in directories)
            {
                var matchingFiles = Directory.GetFiles(dir, searchPattern: "*.rpt");
                rptPaths.AddRange(matchingFiles);
            }

            RptToXml.RptToXml.Convert(rptPaths, dbLoc, progress, false);

            return rptPaths;
        }

        private static XElement LoadXMLFromDb(IEnumerable<string> rptPaths, string dbPath)
        {
            XElement root = new XElement("Reports");

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

                    root.Add(xelement);
                }
            }

            return root;
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

        private void OpenBOE(object sender, RoutedEventArgs e)
        {
            BOEExporter.RetrieveReport();
        }
    }
}
