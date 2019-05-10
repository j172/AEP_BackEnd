using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AEP.BusinessEntities;
using System.Web;

namespace AEP.BusinessServices
{
    public class GroupFactory2
    {
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
        public static void GroupJoinMember(Int64 GroupID , List<string> lstADName)
        {
            
            using (AEPEntities Entities = new AEPEntities())
            {
                Entities.Database.ExecuteSqlCommand("delete from AEPGroupUser where GroupID=" + GroupID);

                foreach (var ADName in lstADName)
                {
                    AEPGroupUser myGroupUser = new AEPGroupUser();
                    myGroupUser.GroupID = GroupID;
                    myGroupUser.UserAD = ADName;
                    myGroupUser.CreateBy = ADUserFactory.GetUserName();
                    myGroupUser.CreateDate = DateTime.Now;
                    Entities.AEPGroupUser.Add(myGroupUser);
                }
                Entities.SaveChanges();
            }
           
        }
    }
}
