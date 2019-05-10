using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.DataModel
{
    public class GroupModel
    {
        public long GroupID { set; get; }
        public string GroupName { set; get; }
        public bool IsSystem { set; get; }
        public List<GroupUserModel> lstGroupUser = new List<GroupUserModel>();
    }


    public class GroupUserModel
    {
        public long GroupID { set; get; }
        public string UserAD {set; get;}
    }

    //public class UserModel
    //{
    //    public string UserAD { set; get; }
    //}
}
