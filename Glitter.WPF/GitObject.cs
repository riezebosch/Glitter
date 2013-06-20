using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glitter.WPF
{
    public class GitObject
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public ObjectType Type { get; set; }

        public IList<string> References { get; private set; }

        public GitObject()
        {
            References = new List<string>();
        }
    }
}
