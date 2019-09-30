using CrystalDecisions.Enterprise;
using CrystalDecisions.ReportAppServer.ClientDoc;
using CrystalDecisions.ReportAppServer.ReportDefModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHEORptAnalyzer
{
    class BOEExporter
    {
        public static void RetrieveReport()
        {
            EnterpriseSession boEnterpriseSession;
            InfoObject boInfoObject;
            ReportClientDocument boReportClientDocument;

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
            boEnterpriseSession = boSessionMgr.Logon("vbarkhatov", "", "boeappprd", "Enterprise");
            //boEnterpriseSession = boSessionMgr.
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
    }
}
