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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs events)
        {

            

            //@"C:\test\Aged Trial Balance Summary for GL account 14241140.xml"

            //XPathDocument doc = new XPathDocument(@"C:\test\Aged Trial Balance Summary for GL account 14241140.xml");
            //doc.Load(@"C:\test\Aged Trial Balance Summary for GL account 14241140.xml");
            //XmlNode root = doc.DocumentElement;
            //XPathNavigator nav = doc.CreateNavigator();

            //nav.Select()

            //XmlNode node = root.SelectSingleNode("/Report/Database/Tables/Table[@ClassName='CrystalReports.CommandTable']/Command");
            //Console.WriteLine(node.InnerXml);
            //Console.ReadKey();
            //txtOutput.Text = node.InnerXml;
            //doc.

            //Thread.Sleep(1000);

            string[] files =
                Directory.GetFiles(@"C:\test\Reports\", "*.xml");

            XElement xroot = new XElement("Reports");

            foreach (string reportDefPath in files)
            {
                XElement xelement = XElement.Load(reportDefPath);
                //txtOutput.Text +=  reportDefPath;
                xroot.Add(xelement);
                //Trace.WriteLine(xroot.Nodes().Count());
            }



            //xelement.Elements().Attribute("CrystalReports.CommandTable");

            //xelement.Parent.

            //XElement.Load()

            //.Elements("Database").Elements("Tables").Elements("Table")
            //.Elements("Database").Elements("Tables").Elements("Table").Elements("Fields").Elements("Field")


            //ClassName = "CrystalReports.CommandTable"

            IEnumerable<XElement> childList =
                from el in xroot.Elements("Report").Elements("Database").Elements("Tables").Elements("Table")
                //from el in xroot.Elements()
                //where (string)el.Attribute("ClassName") == "CrystalReports.CommandTable"
                select el;

            //IEnumerable<XElement> anc = childList.First().Ancestors("Report");

            foreach (XElement e in childList)
            {
                //txtOutput.Text += e.Attribute("LongName").Value;
                //txtOutput.Text += "\n";

                foreach (XElement f in e.Ancestors("Report"))
                {
                    lbReports.Items.Add(f.Attribute("FileName"));
                    //txtOutput.Text += f.Attribute("FileName");
                    //txtOutput.Text += "\n";
                }
                //e.
            }

                //Console.WriteLine(e2);
            //xelement.
        }


        private void ParseRPT(object sender, RoutedEventArgs e)
        {
            //string[] paths = new string[1];

            //Report/Database/Tables/Table[@ClassName='CrystalReports.CommandTable']/Command

            string[] rptFiles = new string[1];// = Directory.GetFiles(@"C:\test\Reports\", "*.rpt");
            //string[] rptFiles = Directory.GetFiles(@"C:\test\", "*.rpt");

            rptFiles[0] = @"C:\test\Reports\*";
            //rptFiles[1] = @"C:\test\Reports\Ambcare Clinic Visits with No Shows & Cancellations by Dept and Fiscal Year (Projected Visits).rpt";

            //paths[0] = @"C:\test\Aged Trial Balance Summary for GL account 14241140.rpt";

            RptToXml.RptToXml.Main2(rptFiles);
        }
    }
}
