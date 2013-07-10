using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glitter
{
    public class GitObject
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public ObjectType ObjectType { get; set; }

        public IList<Tuple<string, string>> References { get; private set; }

        public GitObject()
        {
            References = new List<Tuple<string, string>>();
        }

        public void AddReference(string id, string name = "")
        {
            References.Add(new Tuple<string, string>(id, name));
        }
    }
}
