using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glitter.WPF
{
    public class GitObject
    {
        public ObjectHeader Header { get; set; }
        public string Body { get; set; }
    }
}
