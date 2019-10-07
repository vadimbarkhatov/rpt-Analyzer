using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
//using ExtensionMethods;



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
        XElement xroot;

        public bool SearchFields { get; set; } = true;
        public bool SearchRF { get; set; } = true;
        public bool SearchCommand { get; set; } = true;
        public string SearchString { get; set; } = "";
        public bool ContainsSeach { get; set; } = true;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            tbPreview.Document.PageWidth = 1000;

            LoadXML();

            textFilter = s => s.IndexOf(SearchString.Trim(), StringComparison.OrdinalIgnoreCase) >= 0; //Case insensitive contains

            SearchReports();
        }

        private void LoadXML()
        {
            string[] files;

            try { files = Directory.GetFiles(cacheFolder, "*.xml"); }
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

            textFilter = s => s.IndexOf(SearchString.Trim(), StringComparison.OrdinalIgnoreCase) >= 0; //Case insensitive contains

            resultFilterFuncs = new Dictionary<string, Func<XElement, IEnumerable<XElement>>>
            {
                { "Field", x => x.Descendants("Tables").Descendants("Field") },
                { "Command", x => x.Descendants("Command") },
                { "RecordSelectionFormula", x => x.Descendants("RecordSelectionFormula") }
            };
        }


        Func<string, bool> textFilter;
        Dictionary<string, Func<XElement, IEnumerable<XElement>>> resultFilterFuncs;


        private void BtnSearch_Click(object sender, RoutedEventArgs events)
        {
            SearchReports();
        }

        private void SearchReports()
        {
            Func<XElement, IEnumerable<XElement>> nodeFilter = x => Enumerable.Empty<XElement>();

            if (SearchFields) nodeFilter = nodeFilter.Combine<XElement>(resultFilterFuncs["Field"]);
            if (SearchRF) nodeFilter = nodeFilter.Combine<XElement>(resultFilterFuncs["RecordSelectionFormula"]);
            if (SearchCommand) nodeFilter = nodeFilter.Combine<XElement>(resultFilterFuncs["Command"]);

            nodeFilter = nodeFilter.Filter<XElement>(s => textFilter(s.Value));
            
            IEnumerable <XElement> foundReports = xroot.Elements("Report").Where(x => ContainsSeach == nodeFilter(x).Count() > 0);

            var currItem = lbReports.SelectedItem as XElementWrap;
            lbReports.Items.Clear();

            foreach (XElement e in foundReports)
            {
                var results = new Dictionary<string, string>();

                foreach (string f in resultFilterFuncs.Keys)
                {
                    Func<XElement, IEnumerable<XElement>> filterFunc = resultFilterFuncs[f];

                    results[f] = filterFunc(e).Select(x => x.Value)
                                              .DefaultIfEmpty(string.Empty)
                                              .Aggregate((x, y) => x + "\r" + y);
                }

                var newItem = new XElementWrap() { Text = e.Attribute("FileName").Value, XEle = e, SearchResults = results };

                lbReports.Items.Add(newItem);

                if (newItem.Text == currItem?.Text)
                {
                    lbReports.SelectedItem = lbReports.Items[lbReports.Items.Count - 1];
                }
            }
        }

        private void ParseRPT(object sender, RoutedEventArgs e)
        {
            string[] rptFiles = new string[2];
            rptFiles[0] = rptPath;


            if(!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
            }

            rptFiles[1] = cacheFolder;

            RptToXml.RptToXml.Convert(rptFiles);

            LoadXML();
            SearchReports();
        }

        private void LbReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (tbPreview == null) return;

            tbPreview.Document.Blocks.Clear();

            if (lbReports.SelectedItem == null) return;

            var selectedResults = (lbReports.SelectedItem as XElementWrap).SearchResults[previewMode];

            tbPreview.AppendText(selectedResults);

            Highlighter(SearchString.Trim(), tbPreview);
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
                    break;
                case "Formula":
                    previewMode = "RecordSelectionFormula";
                    break;
                case "Command":
                    previewMode = "Command";
                    break;
            }

            UpdatePreview();
        }
        //log reivewed by person

        private static void Highlighter(string searchText, RichTextBox rtb)
        {
            rtb.SelectAll();
            rtb.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));
            rtb.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

            Regex reg = new Regex(Regex.Escape(searchText), RegexOptions.Compiled | RegexOptions.IgnoreCase);

            TextPointer position = rtb.Document.ContentStart;
            List<TextRange> ranges = new List<TextRange>();

            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = position.GetTextInRun(LogicalDirection.Forward);
                    var matches = reg.Matches(text);

                    foreach (Match match in matches)
                    {
                        TextPointer start = position.GetPositionAtOffset(match.Index);
                        TextPointer end = start.GetPositionAtOffset(searchText.Length);

                        TextRange textrange = new TextRange(start, end);
                        ranges.Add(textrange);
                    }
                }
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }

            foreach (TextRange range in ranges)
            {
                range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
                range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BOEExporter.RetrieveReport();

            //System.Windows.Forms.MessageBox.Show("Test");
            //System.Windows.MessageBox.Show("Test");
        }
    }
}
