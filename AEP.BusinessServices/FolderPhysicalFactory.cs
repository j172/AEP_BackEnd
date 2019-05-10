using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using AEP.BusinessEntities;

namespace AEP.BusinessServices
{
    public class FolderPhysicalFactory
    {
        public static string RootFolder = ConfigurationManager.AppSettings["RootFolder"];
        public static string HistoryRootFolder = ConfigurationManager.AppSettings["HistoryRootFolder"];

        public static string CreateFolder(Int64 ParentFolderID, string FolderName)
        {
            string ParentFolderPath = GetParentFolderPath(ParentFolderID);
            DirectoryInfo Dir = new System.IO.DirectoryInfo(ParentFolderPath);
            DirectoryInfo subDir = Dir.CreateSubdirectory(FolderName);
            return subDir.FullName;
        }
        public static string ReNameFolder(Int64 ParentFolderID, Int64 FolderID, string NewFolderName)
        {
            string ParentFolderPath = GetParentFolderPath(ParentFolderID);
            string FolderPath = GetFolderPath(FolderID);
            string NewFolderPath = System.IO.Path.Combine(ParentFolderPath, NewFolderName);

            if (Directory.Exists(FolderPath))
            {
                System.IO.Directory.Move(FolderPath, NewFolderPath);
            }
            return NewFolderPath;
        }
        public static void DeleteFolder(Int64 FolderID)
        {
            string FolderPath = GetFolderPath(FolderID);
            DirectoryInfo Dir = new System.IO.DirectoryInfo(FolderPath);
            Dir.Delete(true);
        }
        public static string GetFolderPath(Int64 FolderID)
        {
            string PhysicalPath = string.Empty;
            if (FolderID == 0)
            {
                PhysicalPath = RootFolder;
            }
            else
            {
                using (AEPEntities Entities = new AEPEntities())
                {
                    PhysicalPath = (from a in Entities.FolderNode
                                    where a.FolderID == FolderID
                                    select a.PhysicalPath).FirstOrDefault();

                }
            }


            if (PhysicalPath.Substring(PhysicalPath.Length - 1, 1) != "\\")
            {
                PhysicalPath = PhysicalPath + "\\";
            }
            return PhysicalPath;

        }


        public static string GetParentFolderPath(Int64 ParentFolderID)
        {
            string PhysicalPath = string.Empty;
            if (ParentFolderID == 0)
            {
                PhysicalPath = RootFolder;
            }
            else
            {
                using (AEPEntities Entities = new AEPEntities())
                {
                    PhysicalPath = (from a in Entities.FolderNode
                                    where a.FolderID == ParentFolderID
                                    select a.PhysicalPath).FirstOrDefault();

                }
            }

            return PhysicalPath;

        }
    }
}
