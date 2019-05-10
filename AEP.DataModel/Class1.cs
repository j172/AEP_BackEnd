using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEP.DataModel
{
    public class node
    {
        public int id { set; get; }
        public string name { set; get; }
        public List<node> children { set; get; }
        public bool hasChildren { set; get; }
        public bool isExpanded { set; get; }
    }
    public class MyFolder
    {
        public string label { set; get; }
        public string data { set; get; }
        public string expandedIcon { set; get; }
        public string collapsedIcon { set; get; }
        public string owner { set; get; }
        public List<MyFolder> children { set; get; }
    }

    public class file
    {
        public string name { set; get; }
        public bool isfolder { set; get; }
        public string owner { set; get; }
        public string status { set; get; }
        public DateTime createDateTime { set; get; }
        public DateTime updateDateTime { set; get; }
        public string version { set; get; }
    }
}
