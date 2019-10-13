using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace CHEORptAnalyzer
{
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
        public bool SearchFields { get; set; } = true;
        public bool SearchRF { get; set; } = true;
        public bool SearchCommand { get; set; } = true;
        public string SearchString { get; set; } = "";
        public bool ContainsSeach { get; set; } = true;
        public CRElement PreviewElement { get; set; } = CRElement.Field;
        FastColoredTextBox textBox = new FastColoredTextBox();
        Func<string, bool> textFilter;

        private const string rptPath = @"C:\test\Reports\*";
        private string cacheFolder = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\ReportCache\";
        XElement xroot = new XElement("null");
        static readonly Dictionary<CRElement, CRSection> CRSections = new Dictionary<CRElement, CRSection>
        {
            [CRElement.Field] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.Custom,
                ResultFilter = x => x.Descendants("Tables").Descendants("Field")
            },
            [CRElement.Command] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.SQL,
                ResultFilter = x => x.Descendants("Command")
            },
            [CRElement.Formula] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.SQL,
                ResultFilter = x => x.Descendants("RecordSelectionFormula")
            },
            [CRElement.FormulaField] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.CSharp,
                ResultFilter = x => x.Descendants("FormulaFieldDefinition"),
                ResultFormat = s =>
                    s.Select(x => x.Attribute("FormulaName").Value + Environment.NewLine + "{" + Environment.NewLine + x.Value.AppendToNewLine("\t") + Environment.NewLine + "}")
                     .Combine(Environment.NewLine + Environment.NewLine)
                    
            }
        };


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            windowsFormsHost.Child = textBox;
            textBox.Language = FastColoredTextBoxNS.Language.HTML;
            textBox.ReadOnly = true;

            textFilter = s => s.IndexOf(SearchString.Trim(), StringComparison.OrdinalIgnoreCase) >= 0; //Case insensitive contains

        }

        private void LoadXML(IEnumerable<string> folders)
        {
            xroot = new XElement("Reports");

            foreach (string folder in folders)
            {
                string[] files;

                try
                {
                    files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);
                }
                catch (Exception ex)
                {
                    files = new string[0];
                    Trace.WriteLine(ex);
                    System.Windows.Forms.MessageBox.Show("Error", ex.Message, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }

                
                foreach (string reportDefPath in files)
                {
                    XElement xelement;

                    try { xelement = XElement.Load(reportDefPath); }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                        System.Windows.Forms.MessageBox.Show("Error", ex.Message, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        continue;
                    }

                    xroot.Add(xelement);
                }
            }
        }

        public static string AppendToNewLine(string left, string text)
            => text + left.Split(new string[] { "\r\n", "\r" }, StringSplitOptions.None).Combine("\r\n" + text);

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
                                .Where(s => textFilter(s.Value));

            IEnumerable<XElement> foundReports = xroot.Elements("Report").Where(x => ContainsSeach == reportFilter(x.Descendants()).Count() > 0);

            var currItem = lbReports.SelectedItem as XElementWrap;
            lbReports.Items.Clear();

            foreach (XElement report in foundReports)
            {
                var results = new Dictionary<CRElement, string>();

                foreach (CRElement f in CRSections.Keys)
                {
                    
                    Func<IEnumerable<XElement>, IEnumerable<XElement>> filterFunc = CRSections[f].ResultFilter;
                    var SearchResult = report.Descendants().Apply(filterFunc);

                    results[f] = CRSections[f].ResultFormat(report.Descendants().Apply(filterFunc));
                }

                var newItem = new XElementWrap() { Text = report.Attribute("FileName").Value, SearchResults = results };

                lbReports.Items.Add(newItem);

                if (newItem.Text == currItem?.Text)
                {
                    lbReports.SelectedItem = lbReports.Items[lbReports.Items.Count - 1];
                }
            }
        }

        private void ParseRPT(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                string outputFolder = GetOutputFolderPath(path);
                Directory.CreateDirectory(outputFolder);

                string rptPath = path + @"\*";
                string[] rptFiles = new string[2];
                rptFiles[0] = rptPath;
                rptFiles[1] = outputFolder;

                RptToXml.RptToXml.Convert(rptFiles);
            }
        }

        private string GetOutputFolderPath(string path)
        {
            string outputPath;

            if (new Uri(path).Host == "") //if path is non UNC
            {
                outputPath = Extensions.LocalToUNC(path) ?? "";
                if (outputPath == "")
                {
                    outputPath = System.Environment.MachineName + "\\" + path.Replace(":", "$");
                }

            }
            else outputPath = path.Remove(0, 2); //removes the first two slashes that all UNC paths have

            return cacheFolder + outputPath + "\\";
        }

        private void LbReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        TextStyle SearchStyle = new TextStyle(null, System.Drawing.Brushes.Yellow, System.Drawing.FontStyle.Regular);

        private void UpdatePreview()
        {
            var selectedResults = (lbReports?.SelectedItem as XElementWrap)?.SearchResults[PreviewElement] ?? "";

            textBox.Language = CRSections[PreviewElement].Language;
            textBox.Text = selectedResults;

            textBox.AddStyle(SearchStyle);
            textBox.Range.ClearStyle(SearchStyle);
            textBox.Range.SetStyle(SearchStyle, Regex.Escape(SearchString.Trim()), RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) { UpdatePreview(); }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                InitialDirectory = @"C:\test\",
                IsFolderPicker = true,
                Multiselect = true
            };

            IEnumerable<string> directories;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                directories = dialog.FileNames.SelectMany(x => Directory.GetDirectories(x, "*.*", SearchOption.AllDirectories)).Concat(dialog.FileNames);
                foreach(var dir in directories)
                {
                    ParseRPT(directories);
                }

                var searchFolders = dialog.FileNames.Select(x => GetOutputFolderPath(x));
                LoadXML(searchFolders);
                SearchReports();
            }
        }
    }
 
}
