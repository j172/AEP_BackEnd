using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.MailServer
{
    public class MailAttachment
    {
        public string FileName { set; get; }
        public byte[] Attachment { set; get; }
    }
}
