using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.DataModel
{
    public class AEPReturnUploadFile
    {
        public string FileName { set; get; }
        public bool IsOK { set; get; }
        public string DocID { set; get; }
        public object file { set; get; }
        public string message { set; get; }
    }
}
