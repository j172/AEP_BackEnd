using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.DataModel
{
    public class AEPFolder
    {
        public string label { set; get; }
        public AEPFolderAttr data { set; get; }
        public string expandedIcon { set; get; }
        public string collapsedIcon { set; get; }
        public List<AEPFolder> children { set; get; }
    }
}
