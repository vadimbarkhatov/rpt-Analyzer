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
        XElement xroot;

        public bool SearchFields { get; set; } = true;
        public bool SearchRF { get; set; } = true;
        public bool SearchCommand { get; set; } = true;
        public string SearchString { get; set; } = "";
        public string SearchMod { get; set; } = "";

        public List<string> Item
        {
            get { return Items; }
            set { Items = value; }
        }

        public List<string> Items { get; set; } = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            tbPreview.Document.PageWidth = 1000;

            Item.Add("Contains");
            Item.Add("Does Not Contain");


            LoadXML();

            fieldFilter = node => node.Descendants("Field");
            cmdFilter = node => node.Descendants("Command");
            rcdFilter = node => node.Descendants("RecordSelectionFormula");

            textFilterMod = x => x;

            textFilter = s => textFilterMod(s.IndexOf(SearchString.Trim(), StringComparison.OrdinalIgnoreCase) >= 0); //Case insensitive contains
        }

        private void LoadXML()
        {
            string[] files;

            try { files = Directory.GetFiles(@"C:\test\Reports", "*.xml"); }
            catch { files = new string[0]; }

            xroot = new XElement("Reports");

            foreach (string reportDefPath in files)
            {
                XElement xelement = XElement.Load(reportDefPath);
                xroot.Add(xelement);
            }
        }

        readonly Func<XElement, IEnumerable<XElement>> fieldFilter;
        readonly Func<XElement, IEnumerable<XElement>> cmdFilter;
        readonly Func<XElement, IEnumerable<XElement>> rcdFilter;

        readonly Func<string, bool> textFilter;
        Func<bool, bool> textFilterMod;

        private void Button_Click(object sender, RoutedEventArgs events)
        {
            Func<XElement, IEnumerable<XElement>> nodeFilter = x => Enumerable.Empty<XElement>();

            //Func<XElement, IEnumerable<XElement>> fieldFilter = node => node.Descendants("Field").Where(x => x.Attribute("FormulaName") != null && textFilter(x.Attribute("FormulaName").Value));

            nodeFilter = x => fieldFilter(x).Concat(cmdFilter(x)).Concat(rcdFilter(x)).Where(y => textFilter(y.Value));
            
            //attrFilter = node => attrFilter(node).Union(fieldFilter(node)).Union(cmdFilter(node)).Union(rcdFilter(node));

            IEnumerable<XElement> foundReports = xroot.Elements("Report").Where(x => nodeFilter(x).Count() > 0);


            lbReports.Items.Clear();
            foreach (XElement e in foundReports)
            {
                var results = nodeFilter(e).Select(x => x.Value).ToList();
                results.RemoveAll(x => x == "");

                lbReports.Items.Add(new ListTuple<XElement>() { Text = e.Attribute("FileName").Value, Obj = e, SearchResults = results });
            }
        }


        private void ParseRPT(object sender, RoutedEventArgs e)
        {
            string[] rptFiles = new string[1];
            rptFiles[0] = @"C:\test\Reports\*";


            RptToXml.RptToXml.Main2(rptFiles);
        }


        private void LbReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            tbPreview.Document.Blocks.Clear();

            if (lbReports.SelectedItem == null) return;

            var searchResults = (lbReports.SelectedItem as ListTuple<XElement>).SearchResults;

            var text = "";

            foreach (string f in searchResults ?? Enumerable.Empty<string>())
            {
                text += f + "\r";

            }

            tbPreview.AppendText(text);

            Highlighter(SearchString.Trim(), tbPreview);
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //RadioButton rb = (RadioButton)sender;
            var test = 3;
        }

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
