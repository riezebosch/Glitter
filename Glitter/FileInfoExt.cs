using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glitter
{
    static class FileInfoExt
    {
        public static bool Ignore(this FileInfo fi, string root)
        {
            return fi.FullName.StartsWith(Path.Combine(root, "logs"), StringComparison.OrdinalIgnoreCase) ||
                !fi.FullName.StartsWith(Path.Combine(root, "objects"), StringComparison.OrdinalIgnoreCase) &&
                !fi.FullName.StartsWith(Path.Combine(root, "refs"), StringComparison.OrdinalIgnoreCase) &&
                fi.Name != "index" &&
                fi.Name != "HEAD";
        }
    }
}
