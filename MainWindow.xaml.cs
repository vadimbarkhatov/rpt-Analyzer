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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private const string rptPath = @"C:\test\Reports\*";
        private string cacheFolder = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\ReportCache\";
        XElement xroot = new XElement("null");

        public bool SearchFields { get; set; } = true;
        public bool SearchRF { get; set; } = true;
        public bool SearchCommand { get; set; } = true;
        public string SearchString { get; set; } = "";
        public bool ContainsSeach { get; set; } = true;

        FastColoredTextBox textBox = new FastColoredTextBox();
        

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            windowsFormsHost.Child = textBox;
            textBox.Language = FastColoredTextBoxNS.Language.HTML;
            textBox.ReadOnly = true;

            //LoadXML(cacheFolder);

            textFilter = s => s.IndexOf(SearchString.Trim(), StringComparison.OrdinalIgnoreCase) >= 0; //Case insensitive contains

            resultFilterFuncs = new Dictionary<string, Func<IEnumerable<XElement>, IEnumerable<XElement>>>
            {
                { "Field", x => x.Descendants("Tables").Descendants("Field") },
                { "Command", x => x.Descendants("Command") },
                { "RecordSelectionFormula", x => x.Descendants("RecordSelectionFormula") }
            };

            //SearchReports();

            

            
        }

        private void LoadXML(string folder)
        {
            string[] files;

            try { files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories); }
            catch { files = new string[0]; }

            xroot = new XElement("Reports");

            foreach (string reportDefPath in files)
            {
                XElement xelement;

                try { xelement = XElement.Load(reportDefPath); }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    continue;
                }

                xroot.Add(xelement);
            }

        }


        Func<string, bool> textFilter;
        Dictionary<string, Func<IEnumerable<XElement>, IEnumerable<XElement>>> resultFilterFuncs;


        private void BtnSearch_Click(object sender, RoutedEventArgs events)
        {
            SearchReports();
        }

        private void SearchReports()
        {
            Func<IEnumerable<XElement>, IEnumerable<XElement>> reportFilter =
                         x => x.Concat(resultFilterFuncs["Field"](x)).Gate(SearchFields)
                               .Concat(resultFilterFuncs["RecordSelectionFormula"](x)).Gate(SearchRF)
                               .Concat(resultFilterFuncs["Command"](x)).Gate(SearchCommand)
                               .Where(s => textFilter(s.Value));

            IEnumerable <XElement> foundReports = xroot.Elements("Report").Where(x => ContainsSeach == reportFilter(x.Descendants()).Count() > 0);

            var currItem = lbReports.SelectedItem as XElementWrap;
            lbReports.Items.Clear();

            foreach (XElement report in foundReports)
            {
                var results = new Dictionary<string, string>();

                foreach (string f in resultFilterFuncs.Keys)
                {
                    Func<IEnumerable<XElement>, IEnumerable<XElement>> filterFunc = resultFilterFuncs[f];

                    results[f] = string.Join("\r", report.Descendants().Apply(filterFunc).Select(x => x.Value));
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
                string rptPath = path + @"\*";

                string[] rptFiles = new string[2];
                rptFiles[0] = rptPath;


                string outputFolder = cacheFolder + path.Remove(0, 2) + "\\";

                Directory.CreateDirectory(outputFolder);
                

                rptFiles[1] = outputFolder;

                RptToXml.RptToXml.Convert(rptFiles);

                LoadXML(outputFolder);
                SearchReports();
            }
        }

        private void LbReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        TextStyle SearchStyle = new TextStyle(null, System.Drawing.Brushes.Yellow, System.Drawing.FontStyle.Regular);

        private void UpdatePreview()
        {
            var selectedResults = (lbReports?.SelectedItem as XElementWrap)?.SearchResults[previewMode] ?? "";

            textBox.Text = selectedResults;

            textBox.AddStyle(SearchStyle);
            textBox.Range.ClearStyle(SearchStyle);
            textBox.Range.SetStyle(SearchStyle, Regex.Escape(SearchString.Trim()), RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        string previewMode = "Field";

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;

            //TODO:refactor
            switch (rb.Content)
            {
                case "Columns":
                    previewMode = "Field";
                    textBox.Language = FastColoredTextBoxNS.Language.HTML;
                    break;
                case "Formula":
                    previewMode = "RecordSelectionFormula";
                    textBox.Language = FastColoredTextBoxNS.Language.CSharp;
                    break;
                case "Command":
                    previewMode = "Command";
                    textBox.Language = FastColoredTextBoxNS.Language.SQL;
                    break;
            }

            UpdatePreview();
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                InitialDirectory = @"\\localhost\c$\",
                IsFolderPicker = true,
                Multiselect = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ParseRPT(dialog.FileNames);
            }
        }
    }
}
