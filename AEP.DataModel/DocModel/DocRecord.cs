using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.DataModel
{
    public class DocRecord
    {
        public string DocID { set; get; }
        public string DocName { set; get; }
        public int? DocVersion { set; get; }
        public string PhysicalPath { set; get; }
        public long FolderID { set; get; }
        public int? DocStatus { set; get; }
        public string CheckInBy { set; get; }
        public DateTime? CheckInDate { set; get; }
        public string CheckOutBy { set; get; }
        public DateTime? CheckOutDate { set; get; }
        public string CreateBy { set; get; }
        public DateTime? CreateDate { set; get; }
        public string DeleteBy { set; get; }
        public DateTime? DeleteDate { set; get; }
    }
}
