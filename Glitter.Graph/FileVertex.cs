using Glitter.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Glitter.Graph
{
    class FileVertex
    {
        public string Id { get; set; }
        public string Body { get; set; }

        public ObjectType ObjectType { get; set; }

        //public override bool Equals(object obj)
        //{
        //    var rhs = obj as FileVertex;
        //    if (rhs != null)
        //    {
        //        return false;
        //    }

        //    return Id == rhs.Id;
        //}

        //public override int GetHashCode()
        //{
        //    return Id.GetHashCode();
        //}
    }
}
