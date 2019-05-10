using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.DataModel
{
    public class AEPFile
    {
        public string id { set; get; }
        public string name { set; get; }
        public bool isfolder { set; get; }
        public string owner { set; get; }
        public string status { set; get; }
        public string physicalPath { set; get; }
        public DateTime createDateTime { set; get; }
        public DateTime updateDateTime { set; get; }
        public string version { set; get; }
        public string checkOutBy { set; get; }
        public string checkInBy { set; get; }
        public DateTime checkOutDateTime { set; get; }
        public string deleteBy { set; get; }
        public DateTime? deleteDateTime { set; get; }
        public string comments { set; get; }
        public bool isAlertMe { set; get; }
        public bool isDelete { set; get; }
        public Int64? folderID { set; get; }
    }
}
