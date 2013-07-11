using GraphX;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glitter
{
    class FileEdge : EdgeBase<FileVertex>
    {
        public FileEdge(FileVertex source, FileVertex target)
            : base(source, target)
        {
        }

        public string Name { get; set; }
    }
}
