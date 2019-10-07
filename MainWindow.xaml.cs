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



namespace CHEORptAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private const string rptPath = @"C:\test\Reports\*";
        XElement xroot;

        public bool SearchFields { get; set; } = true;
        public bool SearchRF { get; set; } = true;
        public bool SearchCommand { get; set; } = true;
        public string SearchString { get; set; } = "";
        public string SearchMod { get; set; } = "";

        IEnumerable<string> filters = new List<string> { "Field", "Command", "RecordSelectionFormula" };

        public List<string> Items { get; set; } = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            tbPreview.Document.PageWidth = 1000;

            LoadXML();

            textFilterMod = x => x;
            textFilter = s => s.IndexOf(SearchString.Trim(), StringComparison.OrdinalIgnoreCase) >= 0; //Case insensitive contains

            SearchReports();
        }

        private void LoadXML()
        {
            string[] files;

            try { files = Directory.GetFiles(@"C:\test\Reports", "*.xml"); }
            catch { files = new string[0]; }

            xroot = new XElement("Reports");

            foreach (string reportDefPath in files)
            {
                XElement xelement;

                try { xelement = XElement.Load(reportDefPath); }
                catch { continue; }

                xroot.Add(xelement);
            }
        }

        readonly Func<string, bool> textFilter;
        Func<bool, bool> textFilterMod;

        private void Button_Click(object sender, RoutedEventArgs events)
        {
            SearchReports();
        }

        private void SearchReports()
        {
            Func<XElement, IEnumerable<XElement>> nodeFilter = x => Enumerable.Empty<XElement>();

            var filterFuncs = new Dictionary<string, Func<XElement, IEnumerable<XElement>>>();


            filterFuncs.Add("Field", x => x.Descendants("Tables").Descendants("Field"));
            filterFuncs.Add("Command", x => x.Descendants("Command"));
            filterFuncs.Add("RecordSelectionFormula", x => x.Descendants("RecordSelectionFormula"));

            nodeFilter = x => x.Descendants()
                               .Where(y => (y.Name.LocalName == "Field" && y.Parent.Name.LocalName == "Fields" && SearchFields) || (y.Name.LocalName == "Command" && SearchCommand) || (y.Name.LocalName == "RecordSelectionFormula" && SearchRF))
                               .Where(s => textFilter(s.Value));


            IEnumerable<XElement> foundReports = xroot.Elements("Report").Where(x => textFilterMod(nodeFilter(x).Count() > 0));

            lbReports.Items.Clear();
            foreach (XElement e in foundReports)
            {
                var results = new Dictionary<string, string>();

                foreach (var f in filters)
                {
                    results[f] = filterFuncs[f](e).Select(x => x.Value).Aggregate(string.Empty, (x, y) => x + "\r" + y);
                }
                //results.RemoveAll(x => x == "");

                var newItem = new XElementWrap() { Text = e.Attribute("FileName").Value, XEle = e, SearchResults = results };
                lbReports.Items.Add(newItem);
            }
        }

        private void ParseRPT(object sender, RoutedEventArgs e)
        {
            string[] rptFiles = new string[1];
            rptFiles[0] = rptPath;

            RptToXml.RptToXml.Convert(rptFiles);
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

            //var text = searchResults.Aggregate((x, y) => x + "\r" + y);

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

        private void CbSearchMod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mod = (cbSearchMod.SelectedItem as ComboBoxItem).Name;

            //TODO: refactor...
            switch (mod)
            {
                case "Contain":
                    textFilterMod = x => x;
                    break;
                case "DNContain":
                    textFilterMod = x => !x;
                    break;
            }

        }
    }
}
