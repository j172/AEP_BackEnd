using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AEP.BusinessEntities;
using System.Web;
using AEP.DataModel;
using System.Configuration;

namespace AEP.BusinessServices
{
    public class FolderFactory
    {
        public static string RootFolder = ConfigurationManager.AppSettings["RootFolder"];


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


        public static List<AEPFolder> LoadHierarchyFolder()
        {
            AEPFolder RoorNode = new AEPFolder();



            RoorNode.label = "Documents";   //RootFolder;   //modify by Amy, 2019/2/21, "D:\ROOT" 改成 "Documents"
            RoorNode.data = new AEPFolderAttr
            {
                id = 0,
                parentId=0,
                physicalPath = RootFolder,
                auth = 1,           //ROOT 預設為 read，2019/1/24
                //owner = root.FolderOwner.Select(x=>x.UserAD).ToList()
            };
            RoorNode.expandedIcon = "fa fa-folder-open";
            RoorNode.collapsedIcon = "fa fa-folder";




            using (AEPEntities Entities = new AEPEntities())
            {

                List<AEPGroupUser> lstAllAEPGroupUser = Entities.AEPGroupUser.ToList();
                List<FolderNode> lstAllFolderNode = Entities.FolderNode.Include("FolderAuth").ToList();
                LoadHierarchySubFolder(lstAllFolderNode, RoorNode, lstAllAEPGroupUser);
            }


            List<AEPFolder> lstRetNode = new List<AEPFolder>();
            lstRetNode.Add(RoorNode);
            
            //******* 系統操作說明 always置底******** //add by Amy, 2019/2/21
            int totalNode = 0;
            int changeNodeNum = 0;
            AEPFolder bottomFolder = new AEPFolder();
            foreach (var fNode in lstRetNode)
            {
                totalNode = fNode.children.Count;   
                for (int i = 0; i < totalNode; i++)
                {   
                    if (fNode.children[i].label.Equals("系統操作說明"))
                    {
                        changeNodeNum = i;                        
                        bottomFolder = fNode.children[i];                        
                    }                     
                }
            }            
            lstRetNode[0].children.RemoveAt(changeNodeNum);
            lstRetNode[0].children.Add(bottomFolder);
            //////////////////////////////////////////

            return lstRetNode;
        }

        //Simon ADD
        public static AEPFolder LoadHierarchyFolderByID(Int64 FolderID)
        {
            AEPFolder RoorNode = GetMyFolder(FolderID);

            using (AEPEntities Entities = new AEPEntities())
            {
                List<AEPGroupUser> lstAllAEPGroupUser = Entities.AEPGroupUser.ToList();
                List<FolderNode> lstAllFolderNode = Entities.FolderNode.Include("FolderAuth").ToList();
                LoadHierarchySubFolder(lstAllFolderNode, RoorNode, lstAllAEPGroupUser);
            }
            return RoorNode;
        }

        public static void LoadHierarchySubFolder(List<FolderNode> lstAllFolderNode, AEPFolder MyNode, List<AEPGroupUser> lstAllAEPGroupUser)
        {
            var lst = (from a in lstAllFolderNode
                            where a.ParentID == MyNode.data.id
                            select a).ToList();
                foreach (var query in lst)
                {    
                    if (MyNode.children == null)
                    {
                        MyNode.children = new List<AEPFolder>();
                    }
                    AEPFolder MySubNode = new AEPFolder();
                    MyNode.children.Add(MySubNode);
                    MySubNode.label = query.FolderName;
                    bool isDelete = query.IsDelete == null ? false : query.IsDelete.Value;
                    MySubNode.data = new AEPFolderAttr
                    {
                        id = query.FolderID,
                        parentId = query.ParentID,
                        physicalPath = query.PhysicalPath,
                        owner = query.FolderOwner,
                        isOwner = query.FolderOwner == null ? false : query.FolderOwner.ToUpper() == ADUserFactory.GetUserName().ToUpper(),
                        auth = GetAuth(query, MySubNode, lstAllAEPGroupUser, lstAllFolderNode),
                        isDelete = isDelete
                    };


                    if (isDelete)
                    {
                        MySubNode.expandedIcon = "fa fa-folder-open delete  fa-lg";
                        MySubNode.collapsedIcon = "fa fa-folder-minus  fa-lg";
                    }
                    else
                    {
                        MySubNode.expandedIcon = "fa fa-folder-open  fa-lg";
                        MySubNode.collapsedIcon = "fa fa-folder  fa-lg";
                    }


                    LoadHierarchySubFolder(lstAllFolderNode, MySubNode, lstAllAEPGroupUser);
                }
            
        }


        private static int GetAuth(FolderNode entity, AEPFolder MyNode, List<AEPGroupUser> lstAllAEPGroupUser, List<FolderNode> lstAllFolderNode)
        {
            //find user auth


            if (entity.IsAuthInherit == true)
            {
                var query = (from a in lstAllFolderNode
                             where a.FolderID == entity.ParentID
                             select a).FirstOrDefault();

                if (query != null)
                {
                    return GetAuth(query, MyNode, lstAllAEPGroupUser, lstAllFolderNode);
                }
                else
                {
                    return 0;
                }
            }
            else
            {

                int intAuth = 0;
                List<int> lstAuth = new List<int>();
                var query1 = entity.FolderAuth.Where(x => x.UserAD == ADUserFactory.GetUserName()).Select(x => x.FolderAuth1).FirstOrDefault();

                if (query1 != null && query1.HasValue)
                {
                    lstAuth.Add(query1.Value);
                }
                //find group auth
                //List<Int64> lstGroup = lstAllAEPGroupUser.Where(x => x.UserAD == ADUserFactory.GetUserName()).Select(x => x.GroupID).ToList();
                List<Int64> lstGroup = (from a in lstAllAEPGroupUser
                                        where a.UserAD.Equals(ADUserFactory.GetUserName(), StringComparison.OrdinalIgnoreCase)
                                        select a.GroupID).ToList();

                var query2 = (from a in entity.FolderAuth.Where(x => x.GroupID.HasValue)
                              where lstGroup.Contains(a.GroupID.Value)
                              select a.FolderAuth1.Value).ToList();

                lstAuth.AddRange(query2);

                if (lstAuth.Count > 0)
                {
                    intAuth = lstAuth.Max();
                }


                return intAuth;


            }


        }




        public static AEPFolder GetMyFolder(Int64 FolderID)
        {
            AEPFolder myMyFolder = new AEPFolder();
            using (AEPEntities Entities = new AEPEntities())
            {
                List<AEPGroupUser> lstAllAEPGroupUser = Entities.AEPGroupUser.ToList();
                List<FolderNode> lstAllFolderNode = Entities.FolderNode.Include("FolderAuth").ToList();
                var query = (from a in Entities.FolderNode.Include("FolderAuth")
                             where a.FolderID == FolderID
                             select a).FirstOrDefault();
                if (query != null)
                {
                    myMyFolder.label = query.FolderName;
                    bool isDelete = query.IsDelete == null ? false : query.IsDelete.Value;
                    myMyFolder.expandedIcon="fa fa-folder-open";
                    myMyFolder.collapsedIcon="fa fa-folder";
                    myMyFolder.data = new AEPFolderAttr
                    {

                        id = query.FolderID,
                        parentId = query.ParentID,
                        physicalPath = query.PhysicalPath,
                        owner = query.FolderOwner,
                        isOwner = query.FolderOwner == null ? false : query.FolderOwner.ToUpper() == ADUserFactory.GetUserName().ToUpper(),
                        auth = GetAuth(query, myMyFolder, lstAllAEPGroupUser, lstAllFolderNode),
                        isDelete = isDelete
                    };

                    //simon add
                    if (isDelete)
                    {
                        myMyFolder.expandedIcon = "fa fa-folder-open delete  fa-lg";
                        myMyFolder.collapsedIcon = "fa fa-folder-minus  fa-lg";
                    }
                    else
                    {
                        myMyFolder.expandedIcon = "fa fa-folder-open  fa-lg";
                        myMyFolder.collapsedIcon = "fa fa-folder  fa-lg";
                    }

                }

            }
            return myMyFolder;
        }
        
        
        public static Int64 CreateFolder(string FolderName, Int64 ParentFolderID)
        {
            Int64 FolderID = 0;
            using (AEPEntities Entities = new AEPEntities())
            {
                string PhysicalPath = string.Empty;
                PhysicalPath = FolderPhysicalFactory.CreateFolder(ParentFolderID, FolderName);
                FolderNode myFolderNode = new FolderNode();
                myFolderNode.FolderName = FolderName;
                myFolderNode.ParentID = ParentFolderID;
                myFolderNode.PhysicalPath = PhysicalPath;
                myFolderNode.FolderOwner = ADUserFactory.GetUserName();
                myFolderNode.CreateBy = ADUserFactory.GetUserName();
                myFolderNode.CreateDate = DateTime.Now;
                Entities.FolderNode.Add(myFolderNode);
                Entities.SaveChanges();
                Entities.Entry(myFolderNode).Reload();
                FolderID = myFolderNode.FolderID;
            }
            //Copy FolderAuth
            using (AEPEntities Entities = new AEPEntities())
            {

                var query = from a in Entities.FolderAuth.Where(x => x.FolderID == ParentFolderID)
                            select a;
                foreach (var p in query)
                {
                    FolderAuth myFolderAuth = new FolderAuth();
                    myFolderAuth.CreateBy = ADUserFactory.GetUserName();
                    myFolderAuth.CreateDate = DateTime.Now;
                    myFolderAuth.FolderAuth1 = p.FolderAuth1;
                    myFolderAuth.FolderID = FolderID;
                    myFolderAuth.GroupID = p.GroupID;
                    myFolderAuth.UserAD = p.UserAD;
                    myFolderAuth.UpdateBy = ADUserFactory.GetUserName();
                    myFolderAuth.UpdateDate = DateTime.Now;
                    Entities.FolderAuth.Add(myFolderAuth);
                }
                Entities.SaveChanges();

            }
            return FolderID;
        }

        // Simon Add
        public static Int64 RenameFolder(Int64 FolderID, string NewName)
        {

            string PhysicalPath_source = string.Empty;
            string PhysicalPath_target = string.Empty;
            using (AEPEntities Entities = new AEPEntities())
            {

                var query = (from a in Entities.FolderNode
                             where a.FolderID == FolderID
                             select a).FirstOrDefault();

                if (query != null)
                {
                    PhysicalPath_source = query.PhysicalPath;
                    string PhysicalPath = FolderPhysicalFactory.ReNameFolder(query.ParentID, query.FolderID, NewName);
                    query.FolderName = NewName;
                    query.PhysicalPath = PhysicalPath;
                    PhysicalPath_target = PhysicalPath;
                    query.UpdateBy = ADUserFactory.GetUserName();
                    query.UpdateDate = DateTime.Now;
                }
                Entities.SaveChanges();
            }

            List<Int64> lstSubFolderID = GetFoldersFromFolderID(FolderID);
            using (AEPEntities Entities = new AEPEntities())
            {

                var query = (from a in Entities.FolderNode
                             where lstSubFolderID.Contains(a.FolderID)
                             select a);
                foreach (var p in query)
                {
                    string newPhysicalPath = p.PhysicalPath;
                    newPhysicalPath = newPhysicalPath.Replace(PhysicalPath_source, PhysicalPath_target);
                    p.PhysicalPath = newPhysicalPath;
                }
                Entities.SaveChanges();
            }

            return FolderID;
        }
        public static void DeleteFolder(Int64 FolderID)
        {

            List<Int64> lstFolderID = GetFoldersFromFolderID(FolderID);
             using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.FolderNode
                             where lstFolderID.Contains(a.FolderID)
                             select a);
                foreach (var p in query)
                {
                    p.IsDelete = true;
                    p.DeleteBy = ADUserFactory.GetUserName();
                    p.DeleteDate = DateTime.Now;
                }
                var query1 = (from a in Entities.DocRecord
                             where lstFolderID.Contains(a.FolderID.Value)
                             select a);
                foreach (var p in query1)
                {
                    //p.IsDelete = true;
                    p.DocStatus = 3;
                    p.DeleteBy = ADUserFactory.GetUserName();
                    p.DeleteDate = DateTime.Now;
                }
                Entities.SaveChanges();
             }
         }
        public static void DeleteChildrenFolder(AEPEntities Entities , Int64 FolderID)
        {
            List<FolderNode> lstChildren =  GetChildren(Entities,  FolderID);
            if (lstChildren.Count > 0)
            {
                foreach (var p in lstChildren)
                {
                    p.IsDelete = true;
                    p.DeleteBy = ADUserFactory.GetUserName();
                    p.DeleteDate = DateTime.Now;
                    DeleteChildrenFolder(Entities, p.FolderID);
                }

            }
        }
        public static List<FolderNode> GetChildren(AEPEntities Entities, Int64 FolderID)
        {
            List<FolderNode> lstChildren = new List<FolderNode>();
            lstChildren = (from a in Entities.FolderNode
                               where a.ParentID == FolderID
                               select a).ToList();
            return lstChildren;
        }

        public static List<FolderNode> GetChildren(Int64 FolderID)
        {
            List<FolderNode> lstChildren = new List<FolderNode>();
            using (AEPEntities Entities = new AEPEntities())
            {
                lstChildren = (from a in Entities.FolderNode
                             where a.ParentID == FolderID
                             select a).ToList();
                
            }
            return lstChildren;
        }



        //public static void AssignOwnerToFolder(Int64 FolderID, List<string> lstADName)
        //{
            
        //    using (AEPEntities Entities = new AEPEntities())
        //    {
        //        foreach (var ADName in lstADName)
        //        {
        //            FolderOwner myFolderOwner = new FolderOwner();
        //            myFolderOwner.FolderID = FolderID;
        //            myFolderOwner.UserAD = ADName;
        //            myFolderOwner.CreateBy = HttpContext.Current.User.Identity.Name;
        //            myFolderOwner.CreateDate = DateTime.Now;
        //            Entities.FolderOwner.Add(myFolderOwner);
        //        }
        //        Entities.SaveChanges();
        //    }
        //}
        public static void AssignUserToFolder(Int64 FolderID, List<string> lstADName)
        {

            using (AEPEntities Entities = new AEPEntities())
            {
                foreach (var ADName in lstADName)
                {
                    FolderAuth myFolderAuth = new FolderAuth();
                    myFolderAuth.FolderID = FolderID;
                    myFolderAuth.UserAD = ADName;
                    myFolderAuth.CreateBy = ADUserFactory.GetUserName();
                    myFolderAuth.CreateDate = DateTime.Now;
                    Entities.FolderAuth.Add(myFolderAuth);
                }
                Entities.SaveChanges();
            }
        }
        public static void AssignGroupToFolder(Int64 FolderID, List<Int64> lstGroupID)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                foreach (var GroupID in lstGroupID)
                {
                    FolderAuth myFolderAuth = new FolderAuth();
                    myFolderAuth.FolderID = FolderID;
                    myFolderAuth.GroupID = GroupID;
                    myFolderAuth.CreateBy = ADUserFactory.GetUserName();
                    myFolderAuth.CreateDate = DateTime.Now;
                    Entities.FolderAuth.Add(myFolderAuth);
                }
                Entities.SaveChanges();
            }
        }
        public static void AssignGroupToFolder(Int64 FolderID, List<Int64> lstGroupID, List<string> lstADName)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                Entities.Database.ExecuteSqlCommand("delete from FolderAuth where FolderID=" + FolderID);
            }

            AssignUserToFolder(FolderID, lstADName);
            AssignGroupToFolder(FolderID, lstGroupID);
        }

        public static FolderAuthMainModel GetFolderAuthMain(Int64 FolderID, Int64 OriFolderID)
        {
            FolderAuthMainModel myFolderAuthMain = new FolderAuthMainModel();
            myFolderAuthMain.lstFolderAuth = GetFolderAuth(FolderID, OriFolderID);
            myFolderAuthMain.FolderID = OriFolderID;

            using (AEPEntities Entities = new AEPEntities())
            {
                myFolderAuthMain.IsAuthInherit = (from a in Entities.FolderNode.Where(x => x.FolderID == FolderID)
                          select a.IsAuthInherit
                          ).FirstOrDefault();


            }

            return myFolderAuthMain;
            

        }

        public static List<FolderAuthModel> GetFolderAuth(Int64 FolderID, Int64 OriFolderID)
        {
            //bool checkInherit = true;
            List<FolderAuthModel> mylstFolderAuth = new List<FolderAuthModel>();

            using (AEPEntities Entities = new AEPEntities())
            {
                var qq = (from a in Entities.FolderNode.Where(x => x.FolderID == FolderID)
                          select new 
                          {
                              a.ParentID,
                              a.IsAuthInherit
                          }).FirstOrDefault();

                if (qq != null && qq.IsAuthInherit == true)  //找父階權限
                {
                    return GetFolderAuth(qq.ParentID, OriFolderID);
                }
                else
                {
                    //checkInherit = false;
                    var query = (from a in Entities.FolderAuth.Where(x => x.FolderID == FolderID)
                                 select a).ToList();

                    foreach (var p in query)
                    {
                        FolderAuthModel myAuth = new FolderAuthModel();
                        myAuth.AuthID = p.AuthID;
                        myAuth.FolderID = OriFolderID;
                        myAuth.GroupID = p.GroupID;
                        myAuth.GroupName = (p.GroupID.HasValue) ? GroupFactory.GetGroupName(p.GroupID.Value) : string.Empty;
                        myAuth.UserAD = p.UserAD;
                        myAuth.FolderAuth = p.FolderAuth1.HasValue ? p.FolderAuth1.Value : 0;
                        myAuth.AuthType = string.IsNullOrEmpty(p.UserAD) ? 2 : 1;
                        //myAuth.IsAuthInherit = checkInherit;
                        mylstFolderAuth.Add(myAuth);
                    }
                }
            }

            if (mylstFolderAuth.Count == 0)
            {
                foreach(AEPGroup mydefault in GetAEPUserID())
                {
                    FolderAuthModel myAuth = new FolderAuthModel();
                    myAuth.FolderID = OriFolderID;
                    myAuth.GroupID = mydefault.GroupID;
                    myAuth.GroupName = mydefault.GroupName;
                    myAuth.FolderAuth = 1;  //1: Read; 2: Read/Write; 3: Read/Write/Delete
                    myAuth.AuthType = 2;   //2: Group授權; 1: User授權
                    //myAuth.IsAuthInherit = true;
                    mylstFolderAuth.Add(myAuth);
                }
                 
                
            }


            return mylstFolderAuth;
        }
        public static void SaveFolderAuth(FolderAuthMainModel MyFolderAuthMain)
        {
            List<FolderAuthModel> lstFolderAuth = MyFolderAuthMain.lstFolderAuth;
            try
            {
                string UserName = ADUserFactory.GetUserName();
                using (AEPEntities Entities = new AEPEntities())
                {
                    long FolderID = MyFolderAuthMain.FolderID;  //lstFolderAuth.ToArray()[0].FolderID;
                    Entities.Database.ExecuteSqlCommand("delete from FolderAuth where FolderID=" + FolderID);

                    FolderNode myFolder = (from a in Entities.FolderNode
                                           where a.FolderID == FolderID
                                           select a).FirstOrDefault();

                    //// Get ChildrenFolder, add by Amy, 2018/10/26
                    List<FolderNode> myChildrenFolder = (from a in Entities.FolderNode
                                                         where a.ParentID == FolderID
                                                         select a).ToList();
                    ///////////// Change Folder Name /////////////
                    //myFolder.FolderName = lstFolderAuth.ToArray()[0].FolderName;
                    myFolder.IsAuthInherit = MyFolderAuthMain.IsAuthInherit;

                    foreach (var p in lstFolderAuth)
                    {
                        FolderAuth myFolderAuth = new FolderAuth();
                        myFolderAuth.FolderID = p.FolderID;
                        if (p.GroupID.HasValue)
                        {
                            myFolderAuth.GroupID = p.GroupID;
                        }
                        if (!string.IsNullOrEmpty(p.UserAD))
                        {
                            myFolderAuth.UserAD = p.UserAD;
                        }
                        myFolderAuth.FolderAuth1 = p.FolderAuth;
                        myFolderAuth.CreateBy = UserName;
                        myFolderAuth.CreateDate = DateTime.Now;
                        
                        Entities.FolderAuth.Add(myFolderAuth);


                        //// Set Children Folder Auth, add by Amy, 2018/10/26
                        //foreach (var child in myChildrenFolder)
                        //{
                        //    FolderAuth childFolderAuth = new FolderAuth();
                        //    childFolderAuth.FolderID = child.FolderID;
                        //    if (p.GroupID.HasValue)
                        //    {
                        //        childFolderAuth.GroupID = p.GroupID;
                        //    }
                        //    if (!string.IsNullOrEmpty(p.UserAD))
                        //    {
                        //        childFolderAuth.UserAD = p.UserAD;
                        //    }
                        //    childFolderAuth.FolderAuth1 = p.FolderAuth;
                        //    childFolderAuth.CreateBy = UserName;
                        //    childFolderAuth.CreateDate = DateTime.Now;
                        //    Entities.FolderAuth.Add(childFolderAuth);
                        //}
                    }
                    Entities.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }

        public static void ChangeFolderOwner(Int64 FolderID, string OwnerAD)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                FolderNode qFolder = (from a in Entities.FolderNode
                            where a.FolderID == FolderID
                            select a).FirstOrDefault();
                if (qFolder != null)
                {
                    qFolder.FolderOwner = OwnerAD;
                    qFolder.UpdateBy = ADUserFactory.GetUserName();
                    qFolder.UpdateDate = DateTime.Now;
                }
                Entities.SaveChanges();
            }
        }
        public static void ChangeFolderAuth(List<FolderAuthModel> lstFolderAuth)
        {
            string UserName = ADUserFactory.GetUserName();
            using (AEPEntities Entities = new AEPEntities())
            {
                long FolderID = lstFolderAuth.FirstOrDefault().FolderID;
                Entities.Database.ExecuteSqlCommand("delete from FolderAuth where FolderID=" + FolderID);
                foreach (var p in lstFolderAuth)
                {
                    FolderAuth myFolderAuth = new FolderAuth();
                    myFolderAuth.FolderID = p.FolderID;
                    if (p.GroupID.HasValue)
                    {
                        myFolderAuth.GroupID = p.GroupID;
                    }
                    if (!string.IsNullOrEmpty(p.UserAD))
                    {
                        myFolderAuth.UserAD = p.UserAD;
                    }
                    myFolderAuth.FolderAuth1 = p.FolderAuth;
                    myFolderAuth.CreateBy = UserName;
                    myFolderAuth.CreateDate = DateTime.Now;
                    Entities.FolderAuth.Add(myFolderAuth);
                }
                Entities.SaveChanges();
            }
        }

        public static void ModifyFolderName(Int64 FolderID, string FolderName)
        {
            using (AEPEntities Entities = new AEPEntities())
            {
                FolderNode qFolder = (from a in Entities.FolderNode
                                      where a.FolderID == FolderID
                                      select a).FirstOrDefault();
                if (qFolder != null)
                {
                    qFolder.FolderName = FolderName;
                    qFolder.UpdateBy = ADUserFactory.GetUserName();
                    qFolder.UpdateDate = DateTime.Now;
                }
                Entities.SaveChanges();
            }
        }

        public static List<AEPGroup> GetAEPUserID()
        {
            using(AEPEntities Entity = new AEPEntities())
            {
                var query = (from a in Entity.AEPGroup.Where(x => x.GroupName == "AEPUsers" || x.GroupName == "DQAUsers")
                             select a).ToList();
                return query;
            }
        }
    }
}
