using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.DataModel
{
    public class FolderAuthMainModel
    {
        public Int64 FolderID { set; get; }
        public bool? IsAuthInherit { set; get; } //是否繼承父階權限 (預設為True)，Add by Amy, 2018/11/22
        public List<FolderAuthModel> lstFolderAuth = new List<FolderAuthModel>();
    }

    public class FolderAuthModel
    {
        public Int64 AuthID { set; get; }
        public Int64 FolderID { set; get; }
        public string FolderName { set; get; }
        public Int64? GroupID { set; get; }
        public string GroupName { set; get; }
        public string UserAD { set; get; }
        public int FolderAuth { set; get; }
        public int AuthType { set; get; }
        //public bool IsAuthInherit { set; get; } //是否繼承父階權限 (預設為True)，Add by Amy, 2018/11/22
    }
}
