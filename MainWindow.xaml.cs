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
using System.Text;
using System.Xml;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
        public BindingList<XElementWrap> ReportItems { get; set; } = new BindingList<XElementWrap>();

        const string rptPath = @"C:\test\Reports\*";
        static readonly string cacheFolder = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\ReportCache\";
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
                Language = FastColoredTextBoxNS.Language.Custom,
                ResultFilter = x => x.Descendants("RecordSelectionFormula")
            },
            [CRElement.FormulaField] = new CRSection
            {
                Language = FastColoredTextBoxNS.Language.Custom,
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
        }

        private bool TextFilter(string text)
        {
            return text.IndexOf(SearchString.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
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

            IEnumerable<XElement> foundReports = xroot.Elements("Report").Where(x => ContainsSeach == reportFilter(x.Descendants()).Count() > 0);

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

        private void LbReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }


        private void UpdatePreview()
        {
            var selectedResults = (lbReports?.SelectedItem as XElementWrap)?.SearchResults[PreviewElement] ?? "";

            textBox.ClearStylesBuffer();
            textBox.Language = CRSections[PreviewElement].Language;
            textBox.Text = selectedResults;

            TextStyle SearchStyle = new TextStyle(null, System.Drawing.Brushes.Yellow, System.Drawing.FontStyle.Regular);
            textBox.AddStyle(SearchStyle);
            textBox.Range.ClearStyle(SearchStyle);
            textBox.Range.SetStyle(SearchStyle, Regex.Escape(SearchString.Trim()), RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (CRSections[PreviewElement].Language == FastColoredTextBoxNS.Language.Custom) Extensions.CrystalSyntaxHighlight(textBox);
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
                foreach (var dir in directories)
                {
                    ParseRPT(directories);
                }

                var searchFolders = dialog.FileNames.Select(x => GetOutputFolderPath(x));
                LoadXML(searchFolders);
                SearchReports();
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


    }

}
