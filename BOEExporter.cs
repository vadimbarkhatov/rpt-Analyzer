using CrystalDecisions.Enterprise;
using CrystalDecisions.ReportAppServer.ClientDoc;
using CrystalDecisions.ReportAppServer.ReportDefModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrystalDecisions.ReportAppServer.Utilities;
using System.IO;
using RptToXml;

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
            boEnterpriseSession = boSessionMgr.Logon("vbarkhatov", "Friday67", "boeappprd", "Enterprise");
            //boEnterpriseSession = boSessionMgr.
            //Session.Add("boEnterpriseSession", boEnterpriseSession);
            boEnterpriseService = boEnterpriseSession.GetService("", "InfoStore");
            boInfoStore = new InfoStore(boEnterpriseService);

            //boReportName = "Daily Non Violent Crisis Intervention Skill (NVC3).rpt";

            //Retrieve the report object from the InfoStore, only need the SI_ID for RAS
            //boQuery = "Select SI_FILES From CI_INFOOBJECTS Where SI_NAME = '" + boReportName +
            //"' AND SI_Instance=0";
            //boInfoObjects = boInfoStore.Query(boQuery);
            //boInfoObject = boInfoObjects[1];

            SaveFiles(boInfoStore);

            boEnterpriseService = null;

            //boReportClientDocument.ReportDocument

            /**
             * This exports the report to a byte() that we will stream out using the default options for the enum
             * 
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
            //CrystalDecisions.ReportAppServer.CommonObjectModel.ByteArray boByteArray = boReportClientDocument.PrintOutputController.Export(CrReportExportFormatEnum.crReportExportFormatCrystalReports, 1);


            //CrystalDecisions.ReportAppServer.Utilities.Conversion boConversion = new CrystalDecisions.ReportAppServer.Utilities.Conversion();

            //boReportClientDocument.ReportDocument.


            //ExportOptions exportOptions = new ExportOptions();
            //exportOptions.

            //var test = boReportClientDocument.PrintOutputController.ExportEx()
            //Save the ByteArray to disk, overwriting any existing file with the same name.
            //boByteArray.Save(@"c:\test\myExport.rpt", true);

            //Response.Write(@"File successfully saved to C:\Windows\Temp\myExport.pdf");

            //boReportClientDocument.Close();
            boEnterpriseSession.Logoff();
        }

        static void SaveFiles(InfoStore boInfoStore)
        {
            //try
            //{
                // compose query
                //string query = String.Format("SELECT * FROM ci_infoobjects WHERE si_id={0}", ObjectId);
                string boReportName = "Daily Non Violent Crisis Intervention Skill (NVC3).rpt";
                //Retrieve the report object from the InfoStore, only need the SI_ID for RAS
                string boQuery = "Select SI_FILES From CI_INFOOBJECTS Where SI_NAME = '" + boReportName +
                    "' AND SI_Instance=0";

                // retrieve InfoObject from repository
                InfoObject infoObject = boInfoStore.Query(boQuery)[1];

                CrystalDecisions.Enterprise.File file = infoObject.Files[1];


                Object bufferObject = (Object)new Byte[file.Size];
                // copy the file to the buffer object
                file.CopyTo(ref bufferObject);

                // cast the object to a byte array
                Byte[] buffer = (Byte[])bufferObject;

                MemoryStream stream = new MemoryStream(buffer);

                //using (var writer = new RptDefinitionWriter(stream))
                //{
                //    writer.WriteToXml(stream);
                //    stream.Position = 0;
                //    db.FileStorage.Upload(id, "empty", stream);

                //    var filePathMeta = new BsonDocument();
                //    filePathMeta["fullPath"] = rptPath;
                //    db.FileStorage.SetMetadata(id, filePathMeta);
                //}
            //}
            //catch (Exception e)
            //{
            //throw e;
            //}

        } // getfile
    }
}
