using AEP.BusinessEntities;
using AEP.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Configuration;
using AEP.MailServer;

namespace AEP.BusinessServices
{
    public class FileFactory
    {
        public static string RootFolder = ConfigurationManager.AppSettings["RootFolder"];
        public static string HistoryRootFolder = ConfigurationManager.AppSettings["HistoryRootFolder"];

        private static string CreateDocID()
        {
            string MaxDocID = string.Empty;
             using (AEPEntities Entities = new AEPEntities())
            {

                ObjectParameter DocID = new ObjectParameter("DocID", typeof(String));
                Entities.GetDocID(ADUserFactory.GetUserName(), DocID);
                MaxDocID = DocID.Value.ToString();
            }
            return MaxDocID;
        }
        public static string CreateNewFile(string FileName, string FullFileName, AEPFolderAttr MyFolderAttr, string comment, string strChecked)
        {
             string DocID = CreateDocID();
             using (AEPEntities Entities = new AEPEntities())
            {
                 var query=(from a in Entities.DocRecord
                           where a.DocID==DocID
                           select a).FirstOrDefault();
                 if(query!=null)
                 {
                      
                        query.DocName =FileName;
                        query.DocVersion=1;
                        query.PhysicalPath=FullFileName;
                        query.FolderID = MyFolderAttr.id;
                        query.DocStatus = 1;
                        query.Comments = comment;
                        query.CheckInDate = DateTime.Now;
                        query.CheckInBy = ADUserFactory.GetUserName();
                 }
                 Entities.SaveChanges();
            }
             SyncFileToHistory(DocID);


             bool Checked = false;
             Boolean.TryParse(strChecked, out Checked);
             if (Checked)
             {
                 NoticeMail(DocID);
             }
             return DocID;
        }

        public static void CheckInFile(string DocID,string Comments,string strChecked)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.DocRecord
                             where a.DocID == DocID
                             select a).FirstOrDefault();
                if (query != null)
                {
                    int DocVersion = 1;
                    if(query.DocVersion.HasValue){
                        DocVersion = query.DocVersion.Value + 1;
                    }
                    query.DocVersion = DocVersion;
                    query.DocStatus = 1;
                    query.CheckInDate = DateTime.Now;
                    query.CheckInBy = ADUserFactory.GetUserName();
                    query.Comments = Comments;
                }
                Entities.SaveChanges();
            }
            SyncFileToHistory(DocID);
            AlertMeSendMail(DocID);


            bool Checked=false;
            Boolean.TryParse(strChecked,out Checked);
            if(Checked){
                NoticeMail(DocID);
            }

        }
        public static void CheckOutFile(string DocID)
        {
             using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.DocRecord
                             where a.DocID == DocID
                             select a).FirstOrDefault();
                if (query != null)
                {
                    
                    query.DocStatus = 2;
                    query.CheckOutDate = DateTime.Now;
                    query.CheckOutBy = ADUserFactory.GetUserName();
                }
                Entities.SaveChanges();
            }
        }
        public static string CheckOutStatus(string DocID)
        {
            int DocStatus = 0;
            string CheckOutUser = string.Empty;
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.DocRecord
                             where a.DocID == DocID
                             select a).FirstOrDefault();
                if (query != null)
                {
                    DocStatus = (query.DocStatus.HasValue) ? query.DocStatus.Value : 1; //1: Check-In ; 2: Check-Out ; 3: Delete
                    CheckOutUser = query.CheckOutBy;
                }
            }
            return DocStatus + ";" + CheckOutUser;
        }

        public static void DeleteFile(string DocID)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.DocRecord
                             where a.DocID == DocID
                             select a).FirstOrDefault();
                if (query != null)
                {

                    query.DocStatus = 3;
                    query.DeleteBy = ADUserFactory.GetUserName(); ;
                    query.DeleteDate =DateTime.Now;
                }
                Entities.SaveChanges();
            }
        }
        public static void DisCheckOutFile(string DocID)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.DocRecord
                             where a.DocID == DocID
                             select a).FirstOrDefault();
                if (query != null)
                {

                    query.DocStatus = 1;
                    query.CheckOutDate = null;
                    query.CheckOutBy =string.Empty;
                }
                Entities.SaveChanges();
            }
        }
        
        public static List<AEPFile> GetFiles(Int64 FolderID)
        {


            List<AEPFile> lstNode = new List<AEPFile>();

             using (AEPEntities Entities = new AEPEntities())
             {
                 var lstFolder = (from a in Entities.FolderNode
                              where a.ParentID == FolderID
                              select a).ToList();
                 var lstFile = (from a in Entities.DocRecord.Include("DocAlert")
                              where a.FolderID == FolderID
                              select a).ToList();

                 AEPFile myBottomfile = new AEPFile();
                 int count = 1;
                 bool flag = false;
                 foreach (var p in lstFolder)
                 {
                     bool isDelete = p.IsDelete == null ? false : p.IsDelete.Value;

                     //******* 系統操作說明 always置底******** //add by Amy, 2019/2/21
                     if (p.FolderName.Equals("系統操作說明"))
                     {
                         myBottomfile.id = p.FolderID.ToString();
                         myBottomfile.name = p.FolderName;
                         myBottomfile.owner = p.FolderOwner;
                         myBottomfile.physicalPath = p.PhysicalPath;
                         myBottomfile.updateDateTime = p.CreateDate.Value;
                         myBottomfile.isfolder = true;
                         myBottomfile.isDelete = isDelete;
                         flag = true;

                         if (count != lstFolder.Count())
                             continue;
                     }                     
                     ////////////////////////////////////////////////////////////////
                                          
                     AEPFile myfile = new AEPFile();
                     myfile.id = p.FolderID.ToString();
                     myfile.name = p.FolderName;
                     myfile.owner = p.FolderOwner;
                     myfile.physicalPath = p.PhysicalPath;
                     myfile.updateDateTime = p.CreateDate.Value;
                     myfile.isfolder = true;
                     myfile.isDelete = isDelete;
                     lstNode.Add(myfile);

                     count++;
                     if (count == lstFolder.Count() && flag == true)
                     {
                         lstNode.Add(myBottomfile); // 系統操作說明 always置底
                     }
                 }     
                 foreach (var p in lstFile)
                 {

                     bool isAlertMe = false;
                     var query = p.DocAlert.Where(x => x.AlertUserAD == ADUserFactory.GetUserName()).FirstOrDefault();
                     if (query != null)
                     {
                         isAlertMe = true;
                     }
                     bool isDelete = !string.IsNullOrEmpty(p.DeleteBy)  ;

                     AEPFile myfile = new AEPFile();
                     myfile.id = p.DocID;
                     myfile.name = p.DocName;
                     myfile.version = p.DocVersion.Value.ToString();
                     myfile.status = p.DocStatus.Value.ToString();
                     myfile.physicalPath = p.PhysicalPath;
                     myfile.updateDateTime = p.CheckInDate.HasValue ? p.CheckInDate.Value : p.CreateDate.Value;
                     myfile.isfolder = false;
                     myfile.checkInBy = p.CheckInBy;
                     myfile.checkOutBy = p.CheckOutBy;
                     myfile.isAlertMe = isAlertMe;
                     myfile.isDelete = isDelete;


                     lstNode.Add(myfile);
                 }

             }





             return lstNode;

        }

        public static List<AEPFile> GetFilesIncludeSubFolder(Int64 FolderID)
        {


            List<AEPFile> lstNode = new List<AEPFile>();

            using (AEPEntities Entities = new AEPEntities())
            {
                var lstFolder = (from a in Entities.FolderNode
                                 where a.ParentID == FolderID
                                 select a).ToList();
                var lstFile = (from a in Entities.DocRecord.Include("DocAlert")
                               where a.FolderID == FolderID
                               select a).ToList();

                foreach (var p in lstFolder)
                {
                    bool isDelete = p.IsDelete == null ? false : p.IsDelete.Value;
                    AEPFile myfile = new AEPFile();
                    myfile.id = p.FolderID.ToString();
                    myfile.name = p.FolderName;
                    myfile.owner = p.FolderOwner;
                    myfile.physicalPath = p.PhysicalPath;
                    myfile.updateDateTime = p.CreateDate.Value;
                    myfile.isfolder = true;
                    myfile.isDelete = isDelete;
                    lstNode.Add(myfile);
                }
                foreach (var p in lstFile)
                {

                    bool isAlertMe = false;
                    var query = p.DocAlert.Where(x => x.AlertUserAD == ADUserFactory.GetUserName()).FirstOrDefault();
                    if (query != null)
                    {
                        isAlertMe = true;
                    }
                    bool isDelete = !string.IsNullOrEmpty(p.DeleteBy);

                    AEPFile myfile = new AEPFile();
                    myfile.id = p.DocID;
                    myfile.name = p.DocName;
                    myfile.version = p.DocVersion.Value.ToString();
                    myfile.status = p.DocStatus.Value.ToString();
                    myfile.physicalPath = p.PhysicalPath;
                    myfile.updateDateTime = p.CheckInDate.HasValue ? p.CheckInDate.Value : p.CreateDate.Value;
                    myfile.isfolder = false;
                    myfile.checkInBy = p.CheckInBy;
                    myfile.checkOutBy = p.CheckOutBy;
                    myfile.isAlertMe = isAlertMe;
                    myfile.isDelete = isDelete;


                    lstNode.Add(myfile);
                }

            }





            return lstNode;

        }


        public static byte[] ReadFile(string DocID)
        {

            byte[] dataBytes = null;
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.DocRecord
                             where a.DocID == DocID
                             select a).FirstOrDefault();
                if (query != null)
                {
                    int DocVersion = query.DocVersion.Value;
                     dataBytes = File.ReadAllBytes(query.PhysicalPath);


                }
            }
            return dataBytes;
        }
        public static byte[] ReadFileHistory(Int64 HistoryID)
        {

            byte[] dataBytes = null;
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.DocRecord_H
                             where a.HistoryID == HistoryID
                             select a).FirstOrDefault();
                if (query != null)
                {
                    int DocVersion = query.DocVersion.Value;
                    dataBytes = File.ReadAllBytes(query.PhysicalPath);


                }
            }
            return dataBytes;
        }

        public static void SyncFileToHistory(string DocID)
        {
             using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.DocRecord
                             where a.DocID == DocID
                             select a).FirstOrDefault();
                if (query != null)
                {
                    int DocVersion = query.DocVersion.Value;
                    FileInfo fi = new FileInfo(query.PhysicalPath);
                    string Name = fi.Name;
                    string Extension = fi.Extension;
                    string Name_NoExt = Name.Replace(Extension, "");
                    string NewName = string.Format("{0}.V{1}{2}", Name_NoExt, DocVersion, Extension);
                    string folder = fi.DirectoryName;
                    string newFolder = folder.Replace(RootFolder, HistoryRootFolder);
                    string NewFileName=Path.Combine(newFolder,NewName);

                    DirectoryInfo di = new DirectoryInfo(newFolder);
                    if (!di.Exists)
                    {
                        di.Create();
                    }
                    fi.CopyTo(NewFileName);

                    DocRecord_H myDocRecord_H = new DocRecord_H();
                    myDocRecord_H.DocID =query.DocID;
                    myDocRecord_H.DocName = NewName;
	                myDocRecord_H.DocVersion =query.DocVersion;
                    myDocRecord_H.PhysicalPath = NewFileName;
	                myDocRecord_H.FolderID =query.FolderID;
	                myDocRecord_H.DocStatus =query.DocStatus;
	                myDocRecord_H.CheckInBy =query.CheckInBy;
	                myDocRecord_H.CheckInDate =query.CheckInDate;
	                myDocRecord_H.CheckOutBy =query.CheckOutBy;
	                myDocRecord_H.CheckOutDate =query.CheckOutDate;
	                myDocRecord_H.CreateBy =query.CreateBy;
	                myDocRecord_H.CreateDate =query.CreateDate;
	                myDocRecord_H.DeleteBy =query.DeleteBy;
	                myDocRecord_H.DeleteDate =query.DeleteDate;
                    myDocRecord_H.Comments = query.Comments;
                    


                    Entities.DocRecord_H.Add(myDocRecord_H);

                 }
                Entities.SaveChanges();

            }

        }

        public static AEPFile GetFileInfo(string DocID)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                var lstFile = (from a in Entities.DocRecord
                               where a.DocID == DocID
                               select a).ToList();

                AEPFile myfile = new AEPFile();

                foreach (var p in lstFile)
                {                    
                    myfile.id = p.DocID;
                    myfile.name = p.DocName;
                    myfile.version = p.DocVersion.Value.ToString();
                    myfile.status = p.DocStatus.Value.ToString();
                    myfile.physicalPath = p.PhysicalPath;
                    myfile.updateDateTime = p.CheckInDate.HasValue ? p.CheckInDate.Value : p.CreateDate.Value;
                    myfile.isfolder = false;
                    myfile.comments = p.Comments;
                    myfile.checkInBy = p.CheckInBy;
                    myfile.checkOutBy = p.CheckOutBy;
                    myfile.deleteBy = p.DeleteBy;
                    myfile.deleteDateTime = p.DeleteDate.HasValue ? p.DeleteDate.Value : (DateTime?)null;
                }

                return myfile;
            }
        }
        public static List<AEPFile> GetFileHistory(string DocID)
        {
            List<AEPFile> lstFileHistory = new List<AEPFile>();
            using (AEPEntities Entities = new AEPEntities())
            {
                //var lstFile = (from a in Entities.DocRecord                               
                //               where a.DocID == DocID
                //               select a).ToList();
                var lstFile_H = (from b in Entities.DocRecord_H
                                 where b.DocID == DocID
                                 select b).ToList();

                //foreach (var p in lstFile)
                //{
                //    AEPFile myfile = new AEPFile();

                //    myfile.id = p.DocID;
                //    myfile.name = p.DocName;
                //    myfile.version = p.DocVersion.Value.ToString();
                //    myfile.status = p.DocStatus.Value.ToString();
                //    myfile.physicalPath = p.PhysicalPath;
                //    myfile.updateDateTime = p.CheckInDate.HasValue ? p.CheckInDate.Value : p.CreateDate.Value;
                //    myfile.isfolder = false;
                //    myfile.checkInBy = p.CheckInBy;
                //    myfile.checkOutBy = p.CheckOutBy;
                //    myfile.deleteBy = p.DeleteBy;
                //    myfile.deleteDateTime = p.DeleteDate.HasValue ? p.DeleteDate.Value : (DateTime?)null;

                //    lstFileHistory.Add(myfile);
                //}

                foreach (var p in lstFile_H)
                {
                    AEPFile myfile_H = new AEPFile();

                    myfile_H.id = p.HistoryID.ToString();
                    myfile_H.name = p.DocName;
                    myfile_H.version = p.DocVersion.HasValue ? p.DocVersion.Value.ToString() : "1";
                    myfile_H.status = p.DocStatus.HasValue ? p.DocStatus.Value.ToString() : "0";
                    myfile_H.physicalPath = p.PhysicalPath;
                    myfile_H.updateDateTime = p.CheckInDate.HasValue ? p.CheckInDate.Value : p.CreateDate.Value;
                    myfile_H.isfolder = false;
                    myfile_H.checkInBy = p.CheckInBy;
                    myfile_H.checkOutBy = p.CheckOutBy;
                    myfile_H.deleteBy = p.DeleteBy;
                    myfile_H.deleteDateTime = p.DeleteDate.HasValue ? p.DeleteDate.Value : (DateTime?)null;

                    lstFileHistory.Add(myfile_H);
                }
            }
            return lstFileHistory;
        }

        public static Int64 GetFileFolderID(string DocID)
        {
            Int64 FolderID = 0;
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.DocRecord.Where(x => x.DocID == DocID)
                             select a).ToList();

                foreach (var item in query)
                {
                    FolderID = item.FolderID.Value;
                }
            }
            return FolderID;
        }

        public static bool SetAlertMe(string DocID)
        {
            bool result = false;
            try
            {
                using (AEPEntities Entities = new AEPEntities())
                {
                    AEP.BusinessEntities.DocAlert newAlert = new AEP.BusinessEntities.DocAlert()
                    {
                        DocID = DocID,
                        AlertUserAD = ADUserFactory.GetUserName()
                    };
                    Entities.DocAlert.Add(newAlert);
                    Entities.SaveChanges();
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
        public static bool CancelAlertMe(string DocID)
        {
            bool result = false;
            try
            {
                string AlertUserAD = ADUserFactory.GetUserName();
                using (AEPEntities Entities = new AEPEntities())
                {
                    var query = (from a in Entities.DocAlert
                                 where a.DocID == DocID && a.AlertUserAD == AlertUserAD
                                 select a).FirstOrDefault();
                    if (query != null)
                    {
                        Entities.DocAlert.Remove(query);
                    }

                    Entities.SaveChanges();
                    
                    
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public static void AlertMeSendMail(string DocID)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = from a in Entities.DocAlert.Where(x => x.DocID == DocID)
                            select a;

                AEPFile myFile = GetFileInfo(DocID);

                string mailTitle = "AEP檔案管理系統 - File has been changed";
                StringBuilder alertMailContent = new StringBuilder();
                alertMailContent.Append("<b>File Name: </b>").Append(myFile.name).Append(" has been changed.").Append("<br/>");
                alertMailContent.Append("<b>Changed by: </b>").Append(myFile.checkInBy).Append(" , <b>Changed time: </b>").Append(myFile.updateDateTime).Append("<br/>");
                alertMailContent.Append("<br/>");
                alertMailContent.Append("本郵件由系統自動發佈,請勿直接回覆此郵件, 相關問題請洽Jessie Yu / Norman Lee");

                string sendTo = "";
                foreach (var item in query)
                {
                    string ADName = item.AlertUserAD;
                    if (item.AlertUserAD.Contains("\\"))
                    {
                        ADName = item.AlertUserAD.Split('\\')[1];

                    }
                    
                    sendTo += ADName + "@compal.com;";
                }
                
                bool result =   AEPSendMail.SendMail("",sendTo, "AEPAdmin@compal.com", mailTitle, alertMailContent.ToString());
            }
        }
        public static void NoticeMail(string DocID)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                Int64 FolderID = GetFileFolderID(DocID);
                var query = from a in Entities.FolderAuth.Where(x => x.FolderID == FolderID)
                            select a;

                AEPFile myFile = GetFileInfo(DocID);

                string mailTitle = "AEP檔案管理系統 - File has been changed";
                StringBuilder alertMailContent = new StringBuilder();
                alertMailContent.Append("<b>File Name: </b>").Append(myFile.name).Append(" has been changed.").Append("<br/>");
                alertMailContent.Append("<b>Changed by: </b>").Append(myFile.checkInBy).Append(" , <b>Changed time: </b>").Append(myFile.updateDateTime).Append("<br/>");
                alertMailContent.Append("<br/>");
                alertMailContent.Append("本郵件由系統自動發佈,請勿直接回覆此郵件, 相關問題請洽Jessie Yu / Norman Lee");

                string sendTo = "";


                AEPFolder myAEPFolder = FolderFactory.GetMyFolder(FolderID);

                string OwnerADName = myAEPFolder.data.owner;
                if (OwnerADName.Contains("\\"))
                {
                    OwnerADName = OwnerADName.Split('\\')[1];

                }
                sendTo += OwnerADName + "@compal.com;";

                foreach (var item in query)
                {
                    if (!string.IsNullOrEmpty(item.UserAD))
                    {
                        //string ADName = item.UserAD.Split('\\')[1];
                        string ADName = item.UserAD;
                        if (item.UserAD.Contains("\\"))
                        {
                            ADName = item.UserAD.Split('\\')[1];

                        }
                        sendTo += ADName + "@compal.com;";
                    }
                    else
                    {
                        List<string> lstADName = new List<string>();
                        lstADName = GetNoticeUsers(item.GroupID.Value);
                        foreach (var row in lstADName)
                        {
                            //string name = row.Split('\\')[1];
                            string name = row;
                            if (row.Contains("\\"))
                            {
                                name = row.Split('\\')[1];

                            }

                            sendTo += name + "@compal.com;";
                        }
                    }
                }

                bool result = AEPSendMail.SendMail(sendTo, "", "AEPAdmin@compal.com", mailTitle, alertMailContent.ToString());
            }
        }
        private static List<string> GetNoticeUsers(Int64 GroupID)
        {
            List<string> lstADName = new List<string>();
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = from a in Entities.AEPGroupUser.Where(x => x.GroupID == GroupID)
                            select a;
                foreach (var item in query)
                {
                    lstADName.Add(item.UserAD);
                }
            }
            return lstADName;
        }
        public static void SetDownloadLog(string DocID)
        {
             
            try
            {
                using (AEPEntities Entities = new AEPEntities())
                {
                    var query = (from a in Entities.DocRecord.Where(x => x.DocID == DocID)
                                 select a).FirstOrDefault();

                    if(query!=null)
                    {
                        DownloadLog newAlert = new DownloadLog()
                        {
                            DocID = DocID,
                            DocName=query.DocName,
                            DocVersion=query.DocVersion,
                            UserAD = ADUserFactory.GetUserName(),
                            DownloadTime = DateTime.Now
                        };
                        Entities.DownloadLog.Add(newAlert);
                        Entities.SaveChanges(); 
                    }

                   
                }
                 
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            
        }
        public static void SetDocHDownloadLog(Int64 HistoryID)
        {

            try
            {
                using (AEPEntities Entities = new AEPEntities())
                {
                    var query = (from a in Entities.DocRecord_H.Where(x => x.HistoryID == HistoryID)
                                 select a).FirstOrDefault();

                    if (query != null)
                    {
                        DocHDownloadLog newAlert = new DocHDownloadLog()
                        {
                            HistoryID = HistoryID,
                            UserAD = ADUserFactory.GetUserName(),
                            CreateDate = DateTime.Now
                        };
                        Entities.DocHDownloadLog.Add(newAlert);
                        Entities.SaveChanges();
                    }


                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


        public static List<DownloadLogModel> GetDownloadLog()
        {
            List<DownloadLogModel> lstLog = new List<DownloadLogModel>();
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = from a in Entities.DownloadLog
                            select a;
                var queryH = from b in Entities.DocHDownloadLog
                             join c in Entities.DocRecord_H
                             on new { b.HistoryID } equals new { c.HistoryID }
                             select new 
                             {
                                 UserAD = b.UserAD,
                                 DocID = c.DocID,
                                 DocName = c.DocName,
                                 DocVersion = c.DocVersion,
                                 DownloadTime = b.CreateDate
                             };
                             
                foreach(var p in query)
                {
                    DownloadLogModel myLog = new DownloadLogModel();
                    myLog.UserAD = p.UserAD;
                    myLog.DocID = p.DocID;
                    myLog.DocName = p.DocName;
                    myLog.DocVersion = (p.DocVersion.HasValue)? p.DocVersion.Value : 1;
                    myLog.DownloadTime = p.DownloadTime.Value;

                    lstLog.Add(myLog);
                }

                foreach(var p in queryH)
                {
                    DownloadLogModel myLog = new DownloadLogModel();
                    myLog.UserAD = p.UserAD;
                    myLog.DocID = p.DocID;
                    myLog.DocName = p.DocName;
                    myLog.DocVersion = (p.DocVersion.HasValue) ? p.DocVersion.Value : 1;
                    myLog.DownloadTime = p.DownloadTime.Value;

                    lstLog.Add(myLog);
                }
            }
            return lstLog;
        }

        public static List<AEPFile> GetMyCheckOutList()
        {
            List<AEPFile> lstMyCheckOutList = new List<AEPFile>();
            using (AEPEntities Entities = new AEPEntities())
            {
                string UserName = ADUserFactory.GetUserName();
                var query = from a in Entities.DocRecord.Where(x => x.DocStatus == 2 && x.CheckOutBy == UserName)
                            select a;
                foreach (var p in query)
                {
                    AEPFile myCheckOutFile = new AEPFile();
                    myCheckOutFile.id = p.DocID;
                    myCheckOutFile.name = p.DocName;
                    myCheckOutFile.physicalPath = p.PhysicalPath;
                    myCheckOutFile.checkOutBy = p.CheckOutBy;
                    myCheckOutFile.checkOutDateTime = p.CheckOutDate.Value;
                    myCheckOutFile.folderID = p.FolderID;

                    lstMyCheckOutList.Add(myCheckOutFile);
                }                            
            }
            return lstMyCheckOutList;
        }

        public static List<AEPFile> GetMyAlertMeList()
        {
            List<AEPFile> lstMyAlertMeList = new List<AEPFile>();
            using (AEPEntities Entities = new AEPEntities())
            {
                string UserName = ADUserFactory.GetUserName();
                var query = from a in Entities.DocAlert.Where(x => x.AlertUserAD == UserName)
                            join b in Entities.DocRecord
                            on a.DocID equals b.DocID
                            select b;
                foreach (var p in query)
                {
                    AEPFile myCheckOutFile = new AEPFile();
                    myCheckOutFile.id = p.DocID;
                    myCheckOutFile.name = p.DocName;
                    myCheckOutFile.physicalPath = p.PhysicalPath;
                    myCheckOutFile.folderID = p.FolderID;
                    
                    lstMyAlertMeList.Add(myCheckOutFile);
                }
            }
            return lstMyAlertMeList;
        }

        public static List<AEPFile> GetKeywordSearch(string keyword)
        {
            List<AEPFile> lstMySearch = new List<AEPFile>();
            using (AEPEntities Entities = new AEPEntities())
            {
                var qFolder = from a in Entities.FolderNode.Where(x => x.IsDelete != true && x.FolderName.Contains(keyword))
                              select a;
                var qFile = from a in Entities.DocRecord.Where(x => x.DocStatus.Value < 3 && x.DocName.Contains(keyword))
                            select a;

                foreach (var p in qFolder)
                {
                    AEPFile myFolder = new AEPFile();
                    myFolder.id = p.FolderID.ToString();
                    myFolder.name = p.FolderName;
                    myFolder.isfolder = true;
                    myFolder.owner = p.FolderOwner;
                    myFolder.physicalPath = p.PhysicalPath;
                    lstMySearch.Add(myFolder);
                }
                foreach (var pp in qFile)
                {
                    AEPFile myFile = new AEPFile();
                    myFile.id = pp.DocID;
                    myFile.name = pp.DocName;
                    myFile.isfolder = false;
                    myFile.folderID = pp.FolderID;
                    myFile.physicalPath = pp.PhysicalPath;
                    lstMySearch.Add(myFile);
                }
            }
            return lstMySearch;
        }
    }
}
