using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AEP.BusinessEntities;
using System.Data.Entity.Core.Objects;
using System.IO;
//using Excel = Microsoft.Office.Interop.Excel;
using System.Data.Entity.Validation;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;


namespace AEP.DataMigration
{
    class Program
    {
        static System.Collections.Specialized.StringCollection log = new System.Collections.Specialized.StringCollection();

        //public static string RootFolder = ConfigurationManager.AppSettings["RootFolder"];
        //public static string HistoryRootFolder = ConfigurationManager.AppSettings["HistoryRootFolder"];


        static void Main(string[] args)
        {
            DataArchive();
            //UpdateOwnerTime();
        }

        static void DataArchive()
        {
            // Start with drives if you have to search the entire computer.
            string[] drives = System.Environment.GetLogicalDrives();

            foreach (string dr in drives)
            {
                if (dr != "D:\\")
                    continue;
                System.IO.DriveInfo di = new System.IO.DriveInfo(dr);

                // Here we skip the drive if it is not ready to be read. This
                // is not necessarily the appropriate action in all scenarios.
                if (!di.IsReady)
                {
                    Console.WriteLine("The drive {0} could not be read", di.Name);
                    continue;
                }
                System.IO.DirectoryInfo rootDir = di.RootDirectory;
                WalkDirectoryTree(rootDir, 0); //root下
            }

            // Write out all the files that could not be processed.
            Console.WriteLine("Files with restricted access:");
            foreach (string s in log)
            {
                Console.WriteLine(s);
            }
            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key");
            Console.ReadKey();
        }

        static void WalkDirectoryTree(System.IO.DirectoryInfo root, Int64 ParentID)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                log.Add(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            //if (files != null)
            //{
                #region foreach files in loop
                foreach (System.IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    Console.WriteLine(fi.FullName);

                    if (fi.Name.Contains(".lnk") || fi.Name.Contains(".sys") || (fi.Attributes == FileAttributes.Hidden))
                        continue;

                    // Create DocRecord 
                    string MaxDocID = string.Empty;
                    string UserName = "AEPAdmin";//HttpContext.Current.User.Identity.Name;
                    using (AEPEntities Entities = new AEPEntities())
                    {
                        ObjectParameter DocID = new ObjectParameter("DocID", typeof(String));
                        Entities.GetDocID(UserName, DocID);
                        MaxDocID = DocID.Value.ToString();

                        var myDocRecord = (from a in Entities.DocRecord.Where(x => x.DocID == MaxDocID)
                                           select a).FirstOrDefault();

                        if (myDocRecord != null)
                        {
                            myDocRecord.DocID = MaxDocID;
                            myDocRecord.DocName = fi.Name.Replace("\n", "");
                            myDocRecord.DocVersion = 1;
                            myDocRecord.PhysicalPath = fi.FullName.Replace("\n", "");
                            myDocRecord.FolderID = ParentID;
                            myDocRecord.DocStatus = 1;
                            myDocRecord.CheckInBy = UserName;
                            myDocRecord.CheckInDate = DateTime.Now;
                            myDocRecord.CreateBy = UserName;
                            myDocRecord.CreateDate = DateTime.Now;
                        }
                        Entities.SaveChanges();

                        SyncFileToHistory(MaxDocID, fi.FullName.ToString());
                    }
                }
                #endregion

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                #region foreach subDirs in loop (遞迴跑WalkDirectoryTree)
                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    if (dirInfo.ToString().Contains('$') || dirInfo.FullName.ToString().Contains("Forms") || (dirInfo.Attributes == FileAttributes.Hidden) ||
                        dirInfo.FullName.ToString().Contains("_History") || dirInfo.FullName.ToString().Contains("System Volume Information") ||
                        dirInfo.FullName.ToString().Equals("AEPJob") || dirInfo.FullName.ToString().Equals("AEPWeb") || dirInfo.FullName.ToString().Equals("AEP.Portal") || 
                        dirInfo.FullName.ToString().Equals("CLASS") || dirInfo.FullName.ToString().Equals("tools"))
                        continue;

                    if (dirInfo.ToString().Equals("ROOT"))
                    {
                        WalkDirectoryTree(dirInfo, 0);
                    }
                    else
                    {
                        Int64 FolderID = 0;
                        // Create Folder Node 
                        using (AEPEntities Entities = new AEPEntities())
                        {
                            FolderNode myFolderNode = new FolderNode();
                            myFolderNode.FolderName = dirInfo.ToString().Replace("\n", "");
                            myFolderNode.ParentID = ParentID;
                            myFolderNode.PhysicalPath = dirInfo.FullName.Replace("\n", "");
                            myFolderNode.IsAuthInherit = false;     //add by Amy, 2019/5/8
                            myFolderNode.CreateDate = DateTime.Now;
                            Entities.FolderNode.Add(myFolderNode);

                            Entities.SaveChanges();
                            Entities.Entry(myFolderNode).Reload();
                            FolderID = myFolderNode.FolderID;
                        }
                        
                        // Create History Folder
                        CreateHistoryFolder(dirInfo.FullName.Replace("\n", ""));

                        // Resursive call for each subdirectory.
                        WalkDirectoryTree(dirInfo, FolderID);

                    }
                }
                #endregion
            //}

        }

        static void CreateHistoryFolder(string RootFolder)
        {
            string HistoryRootFullName = RootFolder.Replace("D:\\ROOT\\", "D:\\_History\\");
            string[] lstHistory = HistoryRootFullName.Split('\\');
            string HistoryRootFolder = string.Empty;
            if (lstHistory.Length > 1)
            {
                for (int i = 0; i < lstHistory.Length; i++)
                {
                    HistoryRootFolder += lstHistory[i].ToString() + "\\";
                }
            }
            string newFolder = HistoryRootFolder.Replace("\n", "");
            DirectoryInfo di = new DirectoryInfo(newFolder);
            if (!di.Exists)
            {
                di.Create();
            }
        }

        static void SyncFileToHistory(string DocID, string RootFolder)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                string HistoryRootFullName = RootFolder.Replace("D:\\ROOT\\", "D:\\_History\\");
                string[] lstHistory = HistoryRootFullName.Split('\\');
                string HistoryRootFolder = string.Empty;
                if (lstHistory.Length > 1)
                {
                    for (int i = 0; i < (lstHistory.Length - 1); i++)
                    {
                        HistoryRootFolder += lstHistory[i].ToString() + "\\";
                    }
                }

                var query = (from a in Entities.DocRecord
                             where a.DocID == DocID
                             select a).FirstOrDefault();
                if (query != null)
                {
                    int DocVersion = query.DocVersion.Value;
                    FileInfo fi = new FileInfo(query.PhysicalPath);
                    string Name = fi.Name.Replace("\n", "");
                    string Extension = fi.Extension;
                    string Name_NoExt = Name.Replace(Extension, "");
                    string NewName = string.Format("{0}_V{1}{2}", Name_NoExt, DocVersion, Extension);
                    string folder = fi.DirectoryName.Replace("\n", "");
                    string newFolder = HistoryRootFolder.Replace("\n", ""); //folder.Replace(RootFolder, HistoryRootFolder);
                    string NewFileName = Path.Combine(newFolder, NewName);

                    DirectoryInfo di = new DirectoryInfo(newFolder);
                    if (!di.Exists)
                    {
                        di.Create();
                    }
                    try
                    {
                        fi.CopyTo(NewFileName);
                    }
                    catch(Exception exx)
                    {
                        Console.WriteLine("ERROR ==> NewFileName = " + NewFileName + "; \n " + exx.Message);
                    }

                    DocRecord_H myDocRecord_H = new DocRecord_H();
                    myDocRecord_H.DocID = query.DocID;
                    myDocRecord_H.DocName = NewName;// query.DocName;
                    myDocRecord_H.DocVersion = query.DocVersion;
                    myDocRecord_H.PhysicalPath = NewFileName.Replace("\n", "");
                    myDocRecord_H.FolderID = query.FolderID;
                    myDocRecord_H.DocStatus = query.DocStatus;
                    myDocRecord_H.CheckInBy = query.CheckInBy;
                    myDocRecord_H.CheckInDate = query.CheckInDate;
                    myDocRecord_H.CheckOutBy = query.CheckOutBy;
                    myDocRecord_H.CheckOutDate = query.CheckOutDate;
                    myDocRecord_H.CreateBy = query.CreateBy;
                    myDocRecord_H.CreateDate = query.CreateDate;
                    myDocRecord_H.DeleteBy = query.DeleteBy;
                    myDocRecord_H.DeleteDate = query.DeleteDate;
                    Entities.DocRecord_H.Add(myDocRecord_H);

                }
                Entities.SaveChanges();

            }

        }


        static void UpdateOwnerTime()
        {
            //連接EntityFramework DB
            //EntityDB db = new EntityDB();

            //根路徑
            string FilePathRoot = @"C:\_AEP\CLASS\AEP\ImportData\";
            Console.WriteLine("Files Update Start:" + FilePathRoot);

            //事件名稱
            string Event = null;

            //跑迴圈抓根路徑底下資料夾的路徑
            foreach (string EventsPath in Directory.GetFiles(FilePathRoot))
            {
                //擷取資料夾名稱，分辨檔案類別
                Event = EventsPath.Split('.').Last();

                if (Event == "xlsx" || Event == "xls")
                {
                    ImportExcel(EventsPath);
                    Console.WriteLine("Files Path:" + EventsPath);
                }
            }
        }

        static void ImportExcel(string EventsPath)
        {
            string G_sheetName = string.Empty;
            int G_sheetRow = 0;

            try
            {
                SharePointData mySPData = new SharePointData();
                XSSFWorkbook workbook = null;
                workbook = new XSSFWorkbook(EventsPath); ////(fuExcel.FileContent); //只能讀取 System.IO.Stream 
                int sheetCount = workbook.NumberOfSheets; //獲取所有SheetName

                for (int i = 0; i < sheetCount; i++)
                {
                    XSSFSheet sheet = null;
                    sheet = (XSSFSheet)workbook.GetSheetAt(i);   //0表示：第一個 worksheet工作表
                    G_sheetName = sheet.SheetName;
                    G_sheetRow = 0;
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = 0;
                    if (firstRow != null)
                        cellCount = firstRow.LastCellNum; //一行最後一個cell的編號 即總的列數
                    else
                        continue;

                    if (sheet.LastRowNum > 0)
                    {
                        int rowCount = sheet.LastRowNum;
                        for (int r = 1; r <= rowCount; r++)
                        {
                            G_sheetRow = r;
                            IRow row = sheet.GetRow(r);
                            if (row == null) continue; //沒有數據的行默認是null　　　　　　　

                            SharePointDetail myDetail = new SharePointDetail();
                            myDetail.fileName = row.GetCell(0).ToString();
                            myDetail.modifiedTime = Convert.ToDateTime(row.GetCell(1).DateCellValue);
                            myDetail.modifiedBy = row.GetCell(2).ToString();
                            myDetail.itemType = row.GetCell(3).ToString();
                            myDetail.filePath = row.GetCell(4).ToString();
                            myDetail.allPath = row.GetCell(5).StringCellValue.ToString();
                            myDetail.rootPath = row.GetCell(6).StringCellValue.ToString();

                            mySPData.lstSharePointData.Add(myDetail);
                        }
                    }
                }
                UpdateLinqToDB(mySPData);
            }
            catch (Exception exp)
            {
                string Err = "Sheet Name:" + G_sheetName + "; Row:" + G_sheetRow;                
                Console.WriteLine("ERROR:" + Err + "; \n" + exp);
                throw (exp);
            }
            
                
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #region //* marked 使用Excel應用程式 *//
            /*

            //連接Entity Framework, ex: EntityDB db = new EntityDB();
            //AEPEntities Entities = new AEPEntities();

            //開啟Excel應用程式
            Excel.Application xlApp = new Excel.Application
            {
                DisplayAlerts = false,	//關閉警告
                Visible = false,	//背景執行
            };

            //讀取資料夾檔案，但排除開啟時產生的"~$"開頭的隱藏暫存檔
            //foreach(string MemberPath in Directory.GetFiles(EventsPath, @"*.*").Where(x => !x.Contains("~")))
            //{}

            if (!EventsPath.Contains('~') && !EventsPath.Contains('$'))
            {

                //開啟路徑檔案
                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(EventsPath);

                //初始宣告
                string ErrorMessage = null;
                bool IsImportOK = true;
                string FileName = xlWorkbook.Name;
                string SheetName = null;

                try
                {
                    //將每個sheet資料分別匯入
                    //只有一個sheet時，直接Excel.Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                    //註：Excel的Sheet編號是從1開始，不是0
                    foreach (Excel.Worksheet xlWorksheet in xlWorkbook.Sheets)
                    {
                        switch (SheetName = xlWorksheet.Name)
                        {
                            case "1. 外部文件管制專區":
                                UpdateExcelToDB(xlWorksheet);
                                break;
                            case "2. APQP":
                                UpdateExcelToDB(xlWorksheet);
                                break;
                            case "5. AEP Keyparts database":
                                UpdateExcelToDB(xlWorksheet);
                                break;
                            case "6. APQP 專案執行":
                                UpdateExcelToDB(xlWorksheet);
                                break;
                        } //-- Switch END.
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = "[" + SheetName + "] " + ex.Message;
                    IsImportOK = false;
                }
                finally
                {
                    ////寫Log紀錄
                    //db.資料匯入紀錄.Add(new 資料匯入紀錄
                    //{
                    //    員工編號 = FileName,
                    //    員工姓名 = emp.員工姓名,				
                    //    資料類型 = Event,
                    //    匯入時間 = DateTime.Now,
                    //    是否成功 = 是否成功匯入,
                    //    失敗原因 = ErrorMessage
                    //});

                    ////最後一次執行儲存指令，刷新DB資料
                    //db.SaveChanges();

                    ////關閉工作簿
                    //xlWorkbook.Close();

                    ////讀完檔案後處理
                    //移動檔案(MemberPath, Event, 是否成功匯入)
                }

                //所有檔案讀完後，關閉Excel應用程式
                xlApp.Quit();
            }
             * */
            #endregion

        }


        //static void UpdateExcelToDB(Excel.Worksheet xlWorksheet)
        //{
        //    int lastUsedRow = 0;
        //    int lastUsedColumn = 0;
        //    #region APQP 專案執行 (ROOT\APQP1)

        //    lastUsedRow = xlWorksheet.Cells.Find("*", System.Reflection.Missing.Value,
        //                  System.Reflection.Missing.Value, System.Reflection.Missing.Value,
        //                  Excel.XlSearchOrder.xlByRows, Excel.XlSearchDirection.xlPrevious,
        //                  false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;

        //    lastUsedColumn = xlWorksheet.Cells.Find("*", System.Reflection.Missing.Value,
        //                     System.Reflection.Missing.Value, System.Reflection.Missing.Value,
        //                     Excel.XlSearchOrder.xlByColumns, Excel.XlSearchDirection.xlPrevious,
        //                     false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Column;


        //    for (int row = 2; row <= lastUsedRow; row++)
        //    {
        //        string Name = string.Empty; ;
        //        DateTime CreateTime = DateTime.Now;
        //        string OriModifyBy = string.Empty;
        //        string ModifyBy = string.Empty;
        //        string RootPath = string.Empty;
        //        Int64 FolderID = 0;
        //        string DocID = string.Empty;

        //        if (xlWorksheet.Cells[row, 4].Text.Equals("資料夾"))
        //        {
        //            RootPath = xlWorksheet.Cells[row, 7].Text;
        //            RootPath = RootPath.Replace('/', '\\');
        //            Name = xlWorksheet.Cells[row, 1].Text.Replace("\n", "");
        //            CreateTime = Convert.ToDateTime(xlWorksheet.Cells[row, 2].Text);
        //            OriModifyBy = xlWorksheet.Cells[row, 3].Text.Replace(" (TPE)", "");
        //            string[] ADName = OriModifyBy.Split('.');
        //            string samaccountname = ADName[1].Trim() + "_" + ADName[0].Trim();
        //            ModifyBy = samaccountname;

        //            FolderID = GetFolderID(RootPath.Replace("\n", ""));

        //            // Update DB -- FolderNode & FolderOwner
        //            using (AEPEntities Entities = new AEPEntities())
        //            {
        //                var qFolder = (from a in Entities.FolderNode
        //                               where a.FolderID == FolderID
        //                               select a).FirstOrDefault();
        //                if (qFolder != null)
        //                {
        //                    qFolder.CreateBy = ModifyBy;
        //                    qFolder.CreateDate = CreateTime;
        //                    qFolder.FolderOwner = ModifyBy;
        //                }

        //                //var qOwner = (from a in Entities.FolderOwner
        //                //              where a.FolderID == FolderID
        //                //              select a).FirstOrDefault();
        //                //if (qOwner != null)
        //                //{
        //                //    qOwner.UserAD = ModifyBy;
        //                //    qOwner.CreateBy = ModifyBy;
        //                //    qOwner.CreateDate = CreateTime;
        //                //}
        //                Entities.SaveChanges();
        //            }
        //        }
        //        else if (xlWorksheet.Cells[row, 4].Text.Equals("項目"))
        //        {
        //            RootPath = xlWorksheet.Cells[row, 7].Text;
        //            RootPath = RootPath.Replace('/', '\\');
        //            Name = xlWorksheet.Cells[row, 1].Text.Replace("\n", "");
        //            CreateTime = Convert.ToDateTime(xlWorksheet.Cells[row, 2].Text);
        //            OriModifyBy = xlWorksheet.Cells[row, 3].Text.Replace(" (TPE)", "");
        //            string[] ADName = OriModifyBy.Split('.');
        //            string samaccountname = ADName[1].Trim() + "_" + ADName[0].Trim();
        //            ModifyBy = samaccountname;
        //            DocID = GetDocID(RootPath.Replace("\n", ""));

        //            // Update DB -- FolderNode & FolderOwner
        //            using (AEPEntities Entities = new AEPEntities())
        //            {
        //                var qDoc = (from a in Entities.DocRecord
        //                            where a.DocID == DocID
        //                            select a).FirstOrDefault();
        //                if (qDoc != null)
        //                {
        //                    qDoc.CheckInBy = ModifyBy;
        //                    qDoc.CheckInDate = CreateTime;
        //                }
        //                Entities.SaveChanges();
        //            }
        //        }
        //    } //-- For Loop END
        //    #endregion

        //}
        static void UpdateLinqToDB(SharePointData mySPData)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                try
                {
                    foreach (var p in mySPData.lstSharePointData)
                    {
                        string str_itemType = p.itemType;
                        string Name = string.Empty; ;
                        DateTime CreateTime = DateTime.Now;
                        string OriModifyBy = string.Empty;
                        string ModifyBy = string.Empty;
                        string RootPath = string.Empty;
                        Int64 FolderID = 0;
                        string DocID = string.Empty;

                        if (str_itemType.Equals("資料夾"))
                        {
                            RootPath = p.rootPath;
                            RootPath = RootPath.Replace('/', '\\');
                            Name = p.fileName.Replace("\n", "");
                            CreateTime = p.modifiedTime;
                            OriModifyBy = p.modifiedBy.Replace(" (TPE)", "");
                            string[] ADName = OriModifyBy.Split('.');
                            string samaccountname = ADName[1].Trim() + "_" + ADName[0].Trim();
                            ModifyBy = samaccountname;
                            FolderID = GetFolderID(RootPath.Replace("\n", ""));

                            ////////////////////
                            var qFolder = (from a in Entities.FolderNode
                                           where a.FolderID == FolderID
                                           select a).FirstOrDefault();
                            if (qFolder != null)
                            {
                                qFolder.CreateBy = ModifyBy;
                                qFolder.CreateDate = CreateTime;
                                qFolder.FolderOwner = ModifyBy;
                            }
                            Console.WriteLine("FolderID:" + FolderID + "; Folder ModifyBy: " + ModifyBy);
                        }
                        else if (str_itemType.Equals("項目"))
                        {
                            RootPath = p.rootPath;
                            RootPath = RootPath.Replace('/', '\\');
                            Name = p.fileName.Replace("\n", "");
                            CreateTime = p.modifiedTime;
                            OriModifyBy =p.modifiedBy.Replace(" (TPE)", "");
                            string[] ADName = OriModifyBy.Split('.');
                            string samaccountname = ADName[1].Trim() + "_" + ADName[0].Trim();
                            ModifyBy = samaccountname;
                            DocID = GetDocID(RootPath.Replace("\n", ""));

                            /////////////////////////
                            var qDoc = (from a in Entities.DocRecord
                                        where a.DocID == DocID
                                        select a).FirstOrDefault();
                            if (qDoc != null)
                            {
                                qDoc.CheckInBy = ModifyBy;
                                qDoc.CheckInDate = CreateTime;
                            }
                            Console.WriteLine("DocID:" + DocID + "; Doc ModifyBy: " + ModifyBy);
                        }
                    }
                    Entities.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    //Session["SaveFlag"] = ex;
                    Console.WriteLine(ex);
                    // Retrieve the error messages as a list of strings.
                    var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    var fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    // Throw a new DbEntityValidationException with the improved exception message.
                    throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
                }
                catch (Exception ex2)
                {
                    var errorMessage = ((System.Exception)((ex2.InnerException).InnerException)).Message;
                    if (errorMessage.Contains("違反 PRIMARY KEY"))
                    {
                        //Session["SaveFlag"] = "違反 PRIMARY KEY";
                        //Response.Write("<script>alert('Upload Data duplicate, please check!');</script>");
                        Console.WriteLine(ex2 + "違反 PRIMARY KEY");                        
                    }
                    else
                    {
                        //Session["SaveFlag"] = ex2;
                        //Response.Write("<script>alert('UPLOAD ERROR! Please check your upload data. \\n' " + ex2 + ");</script>");
                        Console.WriteLine(ex2);
                    }
                    //throw (ex2);
                }
            }
        }


        static Int64 GetFolderID(string path)
        {
            Int64 FolderID = 0;
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.FolderNode.Where(x => x.PhysicalPath.EndsWith(path))
                            select a).ToList();
                if (query != null)
                {
                    foreach (var item in query)
                    {
                        FolderID = item.FolderID;
                    }
                }
            }
            return FolderID;
        }
        static string GetDocID(string path)
        {
            string DocID = string.Empty;
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.DocRecord.Where(x => x.PhysicalPath.EndsWith(path))
                             select a).ToList();
                if (query != null)
                {
                    foreach (var item in query)
                    {
                        DocID = item.DocID;
                    }
                }
            }
            return DocID;
        }
    }

    public class SharePointData
    {
        public List<SharePointDetail> lstSharePointData = new List<SharePointDetail>();
    }
    public class SharePointDetail
    {
        public string fileName { set; get; }
        public DateTime modifiedTime { set; get; }
        public string modifiedBy { set; get; }
        public string itemType { set; get; }
        public string filePath { set; get; }
        public string allPath { set; get; }
        public string rootPath { set; get; }
    }
}
