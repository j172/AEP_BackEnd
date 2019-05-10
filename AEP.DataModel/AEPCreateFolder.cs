using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.DataModel
{
    public class AEPCreateFolder
    {
        public string name { set; get; }
        public Int64 parentID { set; get; }
        public string parentPhysicalPath { set; get; }
    }
}
