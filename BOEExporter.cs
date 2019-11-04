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
using System.Threading;

namespace CHEORptAnalyzer
{
    class BOEExporter
    {
        public static void RetrieveReport()
        {
            EnterpriseSession boEnterpriseSession;

            EnterpriseService boEnterpriseService;

            //Log on to the Enterprise CMS
            using (SessionMgr boSessionMgr = new SessionMgr())
            {
                boEnterpriseSession = boSessionMgr.Logon("vbarkhatov", "Friday67", "boeappprd", "Enterprise");
                boEnterpriseService = boEnterpriseSession.GetService("", "InfoStore");

                using (InfoStore boInfoStore = new InfoStore(boEnterpriseService))
                {
                    SaveFiles(boInfoStore);
                }
                    

                boEnterpriseService = null;
                boEnterpriseSession.Logoff();
            }
        }

        static void SaveFiles(InfoStore boInfoStore)
        {
            //try
            //{
                // compose query
                //string query = String.Format("SELECT * FROM ci_infoobjects WHERE si_id={0}", ObjectId);
            //string boReportName = "Daily Non Violent Crisis Intervention Skill (NVC3).rpt";
            //Retrieve the report object from the InfoStore, only need the SI_ID for RAS
            //string boQuery = "Select SI_FILES From CI_INFOOBJECTS Where SI_NAME = '" + boReportName +"' AND SI_Instance=0";
            string boQuery = @"Select SI_FILES From CI_INFOOBJECTS Where SI_NAME LIKE '%Epic%' AND SI_Instance = 0 AND SI_Kind = 'CrystalReport'";

            InfoObjects infoObjects = boInfoStore.Query(boQuery);

            // retrieve InfoObject from repository
            for (int i = 1; i < infoObjects.Count; i++)
            {
                InfoObject infoObject = infoObjects[i];

                CrystalDecisions.Enterprise.File file = infoObject.Files[1];
                //file.

                object bufferObject = new byte[file.Size];
                // copy the file to the buffer object
                file.CopyTo(ref bufferObject);
                // cast the object to a byte array
                byte[] buffer = (byte[])bufferObject;

                MemoryStream stream = new MemoryStream(buffer);

                string tempFolder = Directory.CreateDirectory(Path.GetTempPath() + "CHEORPTAnalyzer\\").FullName;
                


                using (FileStream createdFile = System.IO.File.Create(tempFolder + infoObject.ToString()))
                {
                    stream.WriteTo(createdFile);
                }


                //To give BusinessObjects a breather, not sure if really needed
                Thread.Sleep(300);
            }
            



            

        } 
    }
}
