using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using AEP.BusinessEntities;

namespace AEP.Job
{
    public class DeleteFunc
    {
        static void Main(string[] args)
        {
            DeleteFolder();
            DeleteFile();
        }

        private static void DeleteFile()
        {
            try
            {

                DateTime today = DateTime.Now;
                using (AEPEntities Entities = new AEPEntities())
                {
                    var query = from a in Entities.DocRecord.Where(x => x.DocStatus == 3)    //.Where(x => x.DeleteDate.HasValue)
                                select a;

                    foreach (var item in query)
                    {
                        DateTime delDate = item.DeleteDate.Value.AddDays(14);
                        if (delDate < today)
                        {
                            if (File.Exists(item.PhysicalPath))
                            {
                                //刪除實體檔案
                                File.Delete(item.PhysicalPath);
                            }
                            //刪除DB記錄
                            Entities.DocRecord.Remove(item);
                        }
                    }
                    Entities.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        
        }
        private static void DeleteFolder()
        {
            try
            {

                DateTime today = DateTime.Now;
                using (AEPEntities Entities = new AEPEntities())
                {
                    var query = from a in Entities.FolderNode.Where(x => x.IsDelete == true && x.DeleteDate.HasValue)
                                select a;

                    foreach (var item in query)
                    {
                        DateTime delDate = item.DeleteDate.Value.AddDays(14);
                        List<string> lstDelFolderPath = new List<string>();
                        List<string> lstDelFilePath = new List<string>();
                        List<string> lstDelDocID = new List<string>();
                        if (delDate < today)
                        {
                            Int64 folderID = item.FolderID;
                            List<Int64> lstFolderID = GetFoldersFromFolderID(folderID);
                            var qDelFolder = from a in Entities.FolderNode
                                             where lstFolderID.Contains(a.FolderID)
                                             select a;

                            foreach (var p in qDelFolder)
                            {
                                lstDelFolderPath.Add(p.PhysicalPath);
                            }
                            ///////////////////////////////////////////////
                            var qDelFile = from a in Entities.DocRecord
                                           where lstFolderID.Contains(a.FolderID.Value)
                                           select a;
                            var qDelFile_H = from a in Entities.DocRecord_H
                                             where lstFolderID.Contains(a.FolderID.Value)
                                             select a;

                            foreach (var p in qDelFile)
                            {
                                lstDelDocID.Add(p.DocID);
                                lstDelFilePath.Add(p.PhysicalPath);
                            }
                            foreach (var p in qDelFile_H)
                            {
                                lstDelFilePath.Add(p.PhysicalPath);
                            }
                            ///////////////////////////////////////////////

                            //刪除實體檔案                        
                            //foreach (var d in lstDelFilePath)
                            //{
                            //    System.IO.File.Delete(d.ToString());
                            //}
                            foreach (var d in lstDelFolderPath)
                            {
                                if (Directory.Exists(d))
                                {
                                    DirectoryInfo DIFO = new DirectoryInfo(d);
                                    DIFO.Delete(true);
                                }
                            }
                            foreach (var d in lstDelFilePath)
                            {
                                if (File.Exists(d))
                                {
                                    File.Delete(d);
                                }
                            }

                            //刪除DB資料
                            var qDelDocAlert = from a in Entities.DocAlert
                                               where lstDelDocID.Contains(a.DocID)
                                               select a;
                            foreach (var alt in qDelDocAlert)
                            {
                                Entities.DocAlert.Remove(alt);
                            }
                            foreach (var detail_H in qDelFile_H)
                            {
                                Entities.DocRecord_H.Remove(detail_H);
                            }
                            foreach (var detail in qDelFile)
                            {
                                Entities.DocRecord.Remove(detail);
                            }
                            var qDelFolderAuth = from a in Entities.FolderAuth
                                                 where lstFolderID.Contains(a.FolderID)
                                                 select a;
                            foreach (var auth in qDelFolderAuth)
                            {
                                Entities.FolderAuth.Remove(auth);
                            }
                            foreach (var folder in qDelFolder)
                            {
                                Entities.FolderNode.Remove(folder);
                            }
                        }
                    }
                    Entities.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
        public static List<Int64> GetFoldersFromFolderID(Int64 FolderID)
        {
            List<Int64> lstFolderID = new List<Int64>();
            lstFolderID.Add(FolderID);

            using (AEPEntities Entities = new AEPEntities())
            {

                List<FolderNode> lstAllFolderNode = Entities.FolderNode.ToList();
                GetFoldersFromFolderID_SubFolder(lstAllFolderNode, FolderID, lstFolderID);
            }

            return lstFolderID;
        }
        public static void GetFoldersFromFolderID_SubFolder(List<FolderNode> lstAllFolderNode, Int64 ParentFolderID, List<Int64> lstFolderID)
        {
            var lst = (from a in lstAllFolderNode
                       where a.ParentID == ParentFolderID
                       select a).ToList();
            foreach (var query in lst)
            {
                lstFolderID.Add(query.FolderID);
                GetFoldersFromFolderID_SubFolder(lstAllFolderNode, query.FolderID, lstFolderID);
            }

        }

        

    }
}
