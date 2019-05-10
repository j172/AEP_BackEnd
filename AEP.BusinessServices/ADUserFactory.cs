using AEP.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AEP.BusinessServices
{
    public class ADUserFactory
    {
        public static string GetUserName()
        {
             string[] Logon = HttpContext.Current.User.Identity.Name.Split('\\');
             return Logon[1];
        }
        public static bool IsAdmin()
        {
            bool ret = false;
            string UserName = GetUserName();

            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.AEPAdmin.Where(x => x.UserAD == UserName)
                            select a).FirstOrDefault();
                if (query!=null)
                {
                    ret = true;
                }
            }
            return ret;

        }
        public static bool IsAEPUsers()
        {
            bool ret = false;
            string UserName = GetUserName();

            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.AEPGroup.Where(x => x.GroupName == "AEPUsers" || x.GroupName == "DQAUsers")
                             select a).ToList();
                if (query != null)
                {
                    //var query1 = query.AEPGroupUser.Where(x => x.UserAD == UserName).FirstOrDefault();
                    //if (query1 != null)
                    //{
                    //    ret = true;
                    //}
                    foreach (var grp in query)
                    {
                        //var query1 = grp.AEPGroupUser.Where(x => x.UserAD == UserName).FirstOrDefault();
                        var query1 = (from a in grp.AEPGroupUser
                                      where a.UserAD.Equals(UserName, StringComparison.OrdinalIgnoreCase)
                                      select a).FirstOrDefault();
                        if (query1 != null)
                        {
                            ret = true;
                        }
                    }
                    
                }
            }
            return ret;

        }

        public static string GetMyColor()
        {
            string UserName = GetUserName();
            string color = "";
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.MyFavorite.Where(x => x.UserAD == UserName)
                             select a).FirstOrDefault();
                if (query != null)
                {
                    color = query.MyColor;
                }
            }
            return color;
        }

        public static void SaveMyColor(string color)
        {
            string UserName = GetUserName();
            using (AEPEntities Entities = new AEPEntities())
            {
                var query = (from a in Entities.MyFavorite.Where(x => x.UserAD == UserName)
                             select a).FirstOrDefault();
                if (query != null)
                {
                    //更新
                    Entities.Database.ExecuteSqlCommand("Delete from MyFavorite Where UserAD = '" + UserName + "'");
                    Entities.SaveChanges();
                }
                
                //新增
                MyFavorite mytable = new MyFavorite();
                mytable.UserAD = UserName;
                mytable.MyColor = "#" + color;
                Entities.MyFavorite.Add(mytable);
                Entities.SaveChanges();
            }
        }
    }
}
