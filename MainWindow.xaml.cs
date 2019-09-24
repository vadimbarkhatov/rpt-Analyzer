using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

        public bool SearchFields { get; set; }
        public bool SearchRF { get; set; }
        public bool SearchCommand { get; set; }


        public MainWindow()
        {
            InitializeComponent();

            string[] files =
                Directory.GetFiles(@"C:\test\Reports\", "*.xml");

            xroot = new XElement("Reports");

            foreach (string reportDefPath in files)
            {
                XElement xelement = XElement.Load(reportDefPath);
                //txtOutput.Text +=  reportDefPath;
                xroot.Add(xelement);
                //Trace.WriteLine(xroot.Nodes().Count());
            }

            DataContext = this;
        }



        private void Button_Click(object sender, RoutedEventArgs events)
        {

            //.Elements("Database").Elements("Tables").Elements("Table")
            //.Elements("Database").Elements("Tables").Elements("Table").Elements("Fields").Elements("Field")
            //ClassName = "CrystalReports.CommandTable"

            var searchString = tbSearch.Text;

            //Func<XElement, XAttribute> attributes

            //XAttribute x2 = new XAttribute();

            //x2.
            //IEnumerable<XAttribute> attrFilter(XElement node) => node.Descendants("Field").Attributes("FormulaName").Where(x => x.Value.ToUpper().Contains(searchString.ToUpper()));

            Func<string, bool> textFilter = s => s.ToUpper().Contains(searchString.ToUpper());

            Func<XElement, IEnumerable<XElement>> attrFilter;

            //attrFilter = node => node.Descendants("Field").Attributes("FormulaName").Where(x => textFilter(x.Value));

            Func<XElement, IEnumerable<XElement>> fieldFilter = node => node.Descendants("Field").Where(x => x.Attribute("FormulaName") != null && textFilter(x.Attribute("FormulaName").Value));
            Func<XElement, IEnumerable<XElement>> cmdFilter = node => node.Descendants("Command").Where(x => textFilter(x.Value));
            Func<XElement, IEnumerable<XElement>> rcdFilter = node => node.Descendants("RecordSelectionFormula").Where(x => textFilter(x.Value));

            attrFilter = node => fieldFilter(node).Union(cmdFilter(node)).Union(rcdFilter(node));
            


            IEnumerable<XElement> foundReports = xroot.Elements("Report").Where(x => attrFilter(x).Count() > 0);


            xroot.Descendants();
            lbReports.Items.Clear();

            foreach (XElement e in foundReports)
            {
                lbReports.Items.Add(new ListTuple<XElement>() { Text = e.Attribute("FileName").Value, Obj = e});
                Trace.WriteLine((e as IXmlLineInfo).HasLineInfo());
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
            var selReport = (lbReports.SelectedItem as ListTuple<XElement>)?.Obj;

            tbPreview.Text = selReport?.ToString();
        }

    }
}
