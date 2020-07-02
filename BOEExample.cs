using System;

public class Class1
{
    public Class1()
    {
        try
        {
            string query = String.Format("SELECT * FROM ci_infoobjects WHERE si_id={0}", ObjectId);
            InfoObject infoObject = Helper.GetInfoObjects(session, query)[1];

            // get the first file (for testing)
            CrystalDecisions.Enterprise.File attachment = infoObject.Files[1];

            // get the extension; remove the '.'
            string ext = Path.GetExtension(attachment.Name).Remove(0);
            // create path (e.g. C:\Users\USERNAME\Desktop\ReportName.rpt)
            string filePath = String.Format("{0}\\{1}.{2}", path, infoObject.Title, ext);

            // create file (SUCCESS)

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {

                // open FRS file
                using (attachment.StreamingFile)
                {
                    // get file's content chunk by chunk
                    byte[] buffer;

                    while ((buffer = (byte[])attachment.StreamingFile.NextChunk) != null) // <-- exception generated here
                    {
                        // write to destination
                        fileStream.Write(buffer, 0, buffer.Length);
                    }

                }

            }

        } // try
        catch (Exception e)
        {
            throw e;
        }

        void SaveFiles(EnterpriseSession session, int ObjectId, string path)
        {
            try
            {
                // compose query
                string query = String.Format("SELECT * FROM ci_infoobjects WHERE si_id={0}", ObjectId);

                // retrieve InfoObject from repository
                InfoObject infoObject = Helper.GetInfoObjects(session, query)[1];

                foreach (CrystalDecisions.Enterprise.File file in infoObject.Files)
                {
                    // get the extension; remove the '.'
                    string ext = Path.GetExtension(file.Name).Remove(0, 1);

                    string filePath = String.Format("{0}\\{1}.{2}", path, infoObject.Title, ext);

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        // create a byte array sized to hold the file; cast as an object
                        Object bufferObject = (Object)new Byte[file.Size];
                        // copy the file to the buffer object
                        file.CopyTo(ref bufferObject);

                        // cast the object to a byte array
                        Byte[] buffer = (Byte[])bufferObject;

                        // save the byte array as a file
                        fileStream.Write(buffer, 0, buffer.Length);
                    }
                }
            } // try
            catch (Exception e)
            {
                throw e;
            }

        } // getfile

        string GetFRSPath(string repoName)
        {
            string frsRoot = string.Empty;
            string qry = "Select SI_ID, SI_NAME, SI_HOSTED_SERVICES from CI_SYSTEMOBJECTS where SI_NAME like '%." + repoName + "FileRepository'";
            string defName = string.Format("Default%sFRSDir", repoName);

            using (InfoObjects iobjs = _BOEInfoStore.Query(qry))
            {
                if (iobjs.Count > 0)
                {
                    Server svr = (Server)iobjs[1];
                    ConfiguredService csvc = (ConfiguredService)svr.HostedServices.get_Item(0);
                    ServiceConfigProperties cprops = csvc.ConfigProps;
                    frsRoot = cprops.GetItem("RootDirectory").ToString();
                }

            }

            return frsRoot;

        }
    }
}
