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
        public BindingList<XElementWrap> ReportItems { get; set; } = new BindingList<XElementWrap>();
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

        private void LoadXML(IEnumerable<string> folders)
        {
            Xroot = new XElement("Reports");

            using (var db = new LiteDatabase(@"C:\test\MyData.db"))
            {
                foreach (string folder in folders)
                {
                    //IEnumerable<LiteFileInfo> files = Enumerable.Empty<LiteFileInfo>();

                    //try
                    //{
                        //files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);
                        string folderId = Extensions.ToLiteDBID(folder) + "/";
                        IEnumerable<LiteFileInfo> files = db.FileStorage.Find(folderId);
                    //}
                    //catch (Exception ex)
                    //{
                        //files = new string[0];
                        //files = Enumerable.Empty<LiteFileInfo>();
                        //Trace.WriteLine(ex);
                        //System.Windows.Forms.MessageBox.Show("Error", ex.Message, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    //}


                    foreach (LiteFileInfo reportDefPath in files)
                    {
                        XElement xelement;

                        try { xelement = XElement.Load(reportDefPath.OpenRead()); }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                            System.Windows.Forms.MessageBox.Show("Error", ex.Message, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
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
                var results = new Dictionary<CRElement, string>();

                foreach (CRElement crElement in CRSections.Keys)
                {
                    Func<IEnumerable<XElement>, IEnumerable<XElement>> filterFunc = CRSections[crElement].ResultFilter;
                    Func<IEnumerable<XElement>, string> resultFormatter = CRSections[crElement].ResultFormat;

                    results[crElement] = report.Descendants().Apply(filterFunc).Apply(resultFormatter);
                }

                ReportItems.Add(new XElementWrap() { Text = Path.GetFileName(report.Attribute("FileName").Value), SearchResults = results });
            }
        }

        private void LbReports_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePreview();

        private void RadioButton_Checked(object sender, RoutedEventArgs e) => UpdatePreview();

        private void UpdatePreview()
        {
            var selectedResults = (lbReports?.SelectedItem as XElementWrap)?.SearchResults[PreviewElement] ?? "";

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
                

                var searchFolders = dialog.FileNames.Select(x => GetOutputFolderPath(x));
                LoadXML(directories);
                SearchReports();
            }
        }

        private void ParseRPT(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                string rptPath = path + @"\*";

                RptToXml.RptToXml.Convert(rptPath, @"C:\test\MyData.db");
            }
        }

        private string GetOutputFolderPath(string path)
        {
            string outputPath;

            if (new Uri(path).Host == string.Empty) // if path is non UNC
            {
                outputPath = Extensions.LocalToUNC(path) ?? string.Empty;
                if (outputPath == string.Empty)
                {
                    outputPath = System.Environment.MachineName + "\\" + path.Replace(":", "$");
                }
            }
            else
            {
                outputPath = path.Remove(0, 2); // removes the first two slashes that all UNC paths have
            }

            return CacheFolder + outputPath + "\\";
        }
    }
}
