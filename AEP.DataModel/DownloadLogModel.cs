using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.DataModel
{
    public class DownloadLogModel
    {
        public string UserAD { set; get; }
        public string DocID { set; get; }
        public string DocName { set; get; }
        public int DocVersion { set; get; }
        public DateTime DownloadTime { set; get; }
    }
}
