using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Data.Entity;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

using AEP.BusinessEntities;
using AEP.DataModel;
using System.Web;


namespace AEP.BusinessServices
{
    public class GroupFactory
    {
        public static List<GroupModel> GetGroupList ()
        {
            List<GroupModel> lstGroup = new List<GroupModel>();
            List<GroupUserModel> lstGroupUser = new List<GroupUserModel>();

            using (AEPEntities Entities = new AEPEntities())
            {
                var group = from a in Entities.AEPGroup
                            select new GroupModel
                            {
                                GroupID = a.GroupID,
                                GroupName = a.GroupName,
                                IsSystem = a.IsSystem.HasValue?a.IsSystem.Value:false
                            };
                //lstGroup.AddRange(group);

                foreach (var item in group)
                {
                    GroupModel myGroup = new GroupModel();
                    myGroup.GroupID = item.GroupID;
                    myGroup.GroupName = item.GroupName;
                    myGroup.IsSystem = item.IsSystem;
                    var groupUser = from a in Entities.AEPGroupUser.Where(x => x.GroupID == item.GroupID)
                                    select new GroupUserModel
                                    {
                                        GroupID = a.GroupID,
                                        UserAD = a.UserAD                                        
                                    };
                    //lstGroupUser.AddRange(groupUser);
                    myGroup.lstGroupUser.AddRange(groupUser);
                    lstGroup.Add(myGroup);
                }

            }

            return lstGroup;
        }
        public static List<GroupUserModel> GetGroupMember(Int64 GroupID)
        {
            List<GroupUserModel> lstGroupUser = new List<GroupUserModel>();
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.AEPGroupUser.Where(x => x.GroupID == GroupID)
                             select a).ToList();
                foreach (var user in query)
                {
                    GroupUserModel gUser = new GroupUserModel();
                    gUser.GroupID = GroupID;
                    gUser.UserAD = user.UserAD;
                    lstGroupUser.Add(gUser);
                }
            }
            return lstGroupUser;
        }
        public static List<GroupModel> GetGroupList(string GroupName)
        {
            List<GroupModel> lstGroup = new List<GroupModel>();
            List<GroupUserModel> lstGroupUser = new List<GroupUserModel>();

            using (AEPEntities Entities = new AEPEntities())
            {
                var group = from a in Entities.AEPGroup                            
                            select new GroupModel
                            {
                                GroupID = a.GroupID,
                                GroupName = a.GroupName,
                                IsSystem = a.IsSystem.HasValue ? a.IsSystem.Value : false
                            };
                group = group.Where(x => x.GroupName.Contains(GroupName));

                foreach (var item in group)
                {
                    GroupModel myGroup = new GroupModel();
                    myGroup.GroupID = item.GroupID;
                    myGroup.GroupName = item.GroupName;

                    var groupUser = from a in Entities.AEPGroupUser.Where(x => x.GroupID == item.GroupID)
                                    select new GroupUserModel
                                    {
                                        GroupID = a.GroupID,
                                        UserAD = a.UserAD
                                    };
                    //lstGroupUser.AddRange(groupUser);
                    myGroup.lstGroupUser.AddRange(groupUser);
                    lstGroup.Add(myGroup);
                }

            }

            return lstGroup;
        }
        public static Int64 CreateGroup(string GroupName)
        {
            Int64 GroupID = 0;
            using (AEPEntities Entities = new AEPEntities())
            {

                AEPGroup myGroup = new AEPGroup();
                myGroup.GroupName = GroupName;
                myGroup.CreateBy = ADUserFactory.GetUserName();
                myGroup.CreateDate = DateTime.Now;
                myGroup.IsSystem = false;
                Entities.AEPGroup.Add(myGroup);
                Entities.SaveChanges();
                Entities.Entry(myGroup).Reload();
                GroupID = myGroup.GroupID;
            }
            return GroupID;
        }
        public static void GroupJoinMember(Int64 GroupID, List<GroupUserModel> lstADName)
        {

            using (AEPEntities Entities = new AEPEntities())
            {
                Entities.Database.ExecuteSqlCommand("delete from AEPGroupUser where GroupID=" + GroupID);

                foreach (var item in lstADName)
                {
                    AEPGroupUser myGroupUser = new AEPGroupUser();
                    myGroupUser.GroupID = GroupID;
                    myGroupUser.UserAD = item.UserAD;
                    myGroupUser.CreateBy = ADUserFactory.GetUserName();
                    myGroupUser.CreateDate = DateTime.Now;
                    Entities.AEPGroupUser.Add(myGroupUser);
                }
                Entities.SaveChanges();
            }

        }
        public static bool IsOccupiedGroupName(string GroupName)
        {
            try
            {
                if (GroupName == "null")
                    return false;
                using (AEPEntities Entities = new AEPEntities())
                {
                    var query = (from a in Entities.AEPGroup.Where(x => x.GroupName == GroupName)
                                 select a).Take(1).ToList();
                    if (query != null && query.Count() > 0)
                    {                       
                        return false;   //GroupName已存在
                    }
                    else
                    {
                        return true;    //GroupName可用
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public static string AddGroupUser(long GroupID, string UserAD)
        {
            try
            {
                using (AEPEntities Entities = new AEPEntities())
                {
                    AEPGroupUser newUser = new AEPGroupUser()
                    {
                        GroupID = GroupID,
                        UserAD = UserAD,
                        CreateBy = ADUserFactory.GetUserName(),
                        CreateDate = System.DateTime.Now
                    };
                    Entities.AEPGroupUser.Add(newUser);
                    Entities.SaveChanges();
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return "[GroupFactory] AddGroupUser Error! " + ex.Message;
            }
        }
        public static string DelGroupUser(long GroupID, string UserAD)
        {
            try
            {                
                using (AEPEntities Entities = new AEPEntities())
                {
                    Entities.Database.ExecuteSqlCommand("Delete From AEPGroupUser Where GroupID=" + GroupID + " and UserAD=" + UserAD);
                    Entities.SaveChanges();
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static void SaveGroup(GroupModel myGroup)
        {
            try
            {
                long GroupID = myGroup.GroupID;
                List<GroupUserModel> lstADName = myGroup.lstGroupUser;
                using (AEPEntities Entities = new AEPEntities())
                {
                    Entities.Database.ExecuteSqlCommand("delete from AEPGroupUser where GroupID=" + GroupID);                    
                    foreach (var item in lstADName)
                    {
                        AEPGroupUser myGroupUser = new AEPGroupUser();
                        myGroupUser.GroupID = GroupID;
                        myGroupUser.UserAD = item.UserAD;
                        myGroupUser.CreateBy = ADUserFactory.GetUserName();
                        myGroupUser.CreateDate = DateTime.Now;
                        Entities.AEPGroupUser.Add(myGroupUser);

                    }
                    Entities.SaveChanges();
                    //Entities.Entry(myEntity).Reload();
                    //myModel.Iden = myEntity.Iden;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string UploadMemberList(List<GroupUserModel> lstGroupUser) //(GroupModel myGroup)
        {
            try
            {
                using (AEPEntities Entities = new AEPEntities())
                {
                    //long GroupID = myGroup.GroupID;
                    //List<GroupUserModel> lstGroupUser = myGroup.lstGroupUser;
                    //Entities.Database.ExecuteSqlCommand("Delete From AEPGroupUser where GroupID=" + GroupID);
                    int i = 0;
                    foreach (var item in lstGroupUser)
                    {
                        long GroupID = item.GroupID;
                        i++;
                        if (i == 1)
                        {
                            Entities.Database.ExecuteSqlCommand("Delete From AEPGroupUser where GroupID=" + GroupID);
                        }

                        AEPGroupUser myGroupUser = new AEPGroupUser();
                        myGroupUser.GroupID = GroupID;
                        myGroupUser.UserAD = item.UserAD;
                        myGroupUser.CreateBy = ADUserFactory.GetUserName();
                        myGroupUser.CreateDate = DateTime.Now;
                        Entities.AEPGroupUser.Add(myGroupUser);
                    }
                    Entities.SaveChanges();
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return "[GroupFactory] Upload Member List Error! "+ ex.Message;
            }
        }
        public static void DeleteGroup(long GroupID)
        {
            try
            {
                using (AEPEntities Entities = new AEPEntities())
                {
                    Entities.Database.ExecuteSqlCommand("Delete From AEPGroupUser Where GroupID=" + GroupID);
                    Entities.Database.ExecuteSqlCommand("Delete From AEPGroup Where GroupID=" + GroupID);
                    Entities.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string GetGroupName(Int64 GroupID)
        {
            string GroupName = string.Empty;
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.AEPGroup.Where(x => x.GroupID == GroupID)
                             select a).Take(1).ToList();
                foreach (var p in query)
                {
                    GroupName = p.GroupName;
                }
            }
            return GroupName;
        }
        public static List<string> CheckADListIsExist(List<string> lstUser)
        {
            List<string> myValidUser = new List<string>();
            try
            {
                foreach (var name in lstUser)
                {
                    List<UserADInfo> myUser = UserADManager.GetAccounts(name);//UserADManager.GetUserADInfo_NoSession(name);
                    if (myUser.Count == 0)
                    {
                        myValidUser.Add(name + " is not exist!");
                    }
                    else
                    {
                        myValidUser.Add(name);
                    }
                    //if (myUser != null && !string.IsNullOrEmpty(myUser.samaccountname))
                    //{
                    //    myValidUser.Add(myUser.samaccountname);
                    //}
                    //else
                    //{
                    //    myValidUser.Add(name + " 不存在!");
                    //}
                }
            }
            catch(Exception ex)
            {
                throw (ex);
            }
            return myValidUser;
        }
    }
}
