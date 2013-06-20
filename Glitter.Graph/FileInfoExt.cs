using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glitter.Graph
{
    static class FileInfoExt
    {
        public static bool Ignore(this FileInfo fi, string parent)
        {
            return fi.FullName.StartsWith(Path.Combine(parent, "logs")) ||
                !fi.FullName.StartsWith(Path.Combine(parent, "objects")) &&
                !fi.FullName.StartsWith(Path.Combine(parent, "refs")) &&
                fi.Name != "index" &&
                fi.Name != "HEAD";
        }
    }
}
