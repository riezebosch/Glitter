using GraphX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Glitter
{
    class FileVertex : VertexBase
    {
        public GitObject GitObject { get; set; }

        public string Id { get; set; }
    }
}
