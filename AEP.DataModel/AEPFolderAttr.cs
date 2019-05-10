using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.DataModel
{
    public class AEPFolderAttr
    {
        public Int64 id { set; get; }
        public string physicalPath { set; get; }
        public string owner { set; get; }
        public Int64 parentId { set; get; }
        public bool isOwner { set; get; }
        public int auth { set; get; }
        public bool isDelete { set; get; }
        public bool IsAuthInherit {set; get;}
    }
}
