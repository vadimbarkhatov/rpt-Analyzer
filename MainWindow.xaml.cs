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

using CrystalDecisions.Enterprise;
using CrystalDecisions.ReportAppServer.ClientDoc;
using CrystalDecisions.ReportAppServer.Controllers;
using CrystalDecisions.ReportAppServer.ReportDefModel;

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

            tbPreview.Document.PageWidth = 1000;

            fieldFilter = node => node.Descendants("Field");//.Where(x => textFilter(x.Value));
            cmdFilter = node => node.Descendants("Command");//.Where(x => textFilter(x.Value));
            rcdFilter = node => node.Descendants("RecordSelectionFormula");//.Where(x => textFilter(x.Value));

            Func<string> searchString = tbSearch.Text.Trim;

            textFilter = s => s.ToUpper().Contains(searchString().ToUpper());
        }

        Func<XElement, IEnumerable<XElement>> fieldFilter;
        Func<XElement, IEnumerable<XElement>> cmdFilter;
        Func<XElement, IEnumerable<XElement>> rcdFilter;

        //string searchString = "";

        Func<string, bool> textFilter;// = s => s.ToUpper().Contains(searchString.ToUpper());


        private void Button_Click(object sender, RoutedEventArgs events)
        {
            //string searchString = tbSearch.Text.Trim;
            Func<XElement, IEnumerable<XElement>> nodeFilter = x => Enumerable.Empty<XElement>();

            //Func<XElement, IEnumerable<XElement>> fieldFilter = node => node.Descendants("Field").Where(x => x.Attribute("FormulaName") != null && textFilter(x.Attribute("FormulaName").Value));
            

            nodeFilter = x => fieldFilter(x).Concat(cmdFilter(x)).Concat(rcdFilter(x)).Where(y => textFilter(y.Value));
            //attrFilter = x
            //attrFilter = node => attrFilter(node).Union(fieldFilter(node)).Union(cmdFilter(node)).Union(rcdFilter(node));

            IEnumerable<XElement> foundReports = xroot.Elements("Report").Where(x => nodeFilter(x).Count() > 0);

            //xroot.Descendants();
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

            Highlighter(tbSearch.Text.Trim());
        }


        private void Highlighter(string searchText)
        {
            tbPreview.SelectAll();
            tbPreview.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));
            tbPreview.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

            Regex reg = new Regex(searchText, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            TextPointer position = tbPreview.Document.ContentStart;
            List<TextRange> ranges = new List<TextRange>();

            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = position.GetTextInRun(LogicalDirection.Forward);
                    var matchs = reg.Matches(text);

                    foreach (Match match in matchs)
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
            RetrieveReport();
        }

        EnterpriseSession boEnterpriseSession;
        InfoObject boInfoObject;
        ReportClientDocument boReportClientDocument;

        private void RetrieveReport()
        {
            SessionMgr boSessionMgr;
            InfoStore boInfoStore;
            EnterpriseService boEnterpriseService;
            InfoObjects boInfoObjects;
            string boReportName;
            string boQuery;
            CrystalDecisions.ReportAppServer.ClientDoc.ReportAppFactory boReportAppFactory;

            //Log on to the Enterprise CMS
            boSessionMgr = new SessionMgr();
            //boEnterpriseSession = boSessionMgr.Logon(Request.QueryString["username"], Request.QueryString["password"], Request.QueryString["cms"], Request.QueryString["authtype"]);
            boEnterpriseSession = boSessionMgr.Logon("vbarkhatov", "wat", "boeappprd", "Enterprise");
            //Session.Add("boEnterpriseSession", boEnterpriseSession);
            boEnterpriseService = boEnterpriseSession.GetService("", "InfoStore");
            boInfoStore = new InfoStore(boEnterpriseService);

            boReportName = "Daily Non Violent Crisis Intervention Skill (NVC3).rpt";

            //Retrieve the report object from the InfoStore, only need the SI_ID for RAS
            boQuery = "Select SI_ID From CI_INFOOBJECTS Where SI_NAME = '" + boReportName +
                "' AND SI_Instance=0";
            boInfoObjects = boInfoStore.Query(boQuery);
            boInfoObject = boInfoObjects[1];

            boEnterpriseService = null;

            //Retrieve the RASReportFactory
            boEnterpriseService = boEnterpriseSession.GetService("RASReportFactory");
            boReportAppFactory = (ReportAppFactory)boEnterpriseService.Interface;
            //Open the report from Enterprise
            boReportClientDocument = boReportAppFactory.OpenDocument(boInfoObject.ID, 0);

            /**
             * This exports the report to a byte() that we will stream out using the default options for the enum
             * The available enums are:
             * CrReportExportFormatEnum.crReportExportFormatCharacterSeparatedValues
             * CrReportExportFormatEnum.crReportExportFormatCrystalReports
             * CrReportExportFormatEnum.crReportExportFormatEditableRTF
             * CrReportExportFormatEnum.crReportExportFormatHTML
             * CrReportExportFormatEnum.crReportExportFormatMSExcel
             * CrReportExportFormatEnum.crReportExportFormatMSWord
             * CrReportExportFormatEnum.crReportExportFormatPDF
             * CrReportExportFormatEnum.crReportExportFormatRecordToMSExcel
             * CrReportExportFormatEnum.crReportExportFormatRTF
             * CrReportExportFormatEnum.crReportExportFormatTabSeparatedText
             * CrReportExportFormatEnum.crReportExportFormatText
             * CrReportExportFormatEnum.crReportExportFormatXML
            */
            CrystalDecisions.ReportAppServer.CommonObjectModel.ByteArray boByteArray = boReportClientDocument.PrintOutputController.Export(CrReportExportFormatEnum.crReportExportFormatCrystalReports, 1);
            //Save the ByteArray to disk, overwriting any existing file with the same name.
            boByteArray.Save(@"c:\test\myExport.rpt", true);

            //Response.Write(@"File successfully saved to C:\Windows\Temp\myExport.pdf");


            boReportClientDocument.Close();
            boEnterpriseSession.Logoff();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;

            
        }
    }
}
