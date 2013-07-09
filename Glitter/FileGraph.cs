using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;

namespace Glitter
{
    class FileGraph : BidirectionalGraph<FileVertex, FileEdge>
    {
        public FileGraph()
        {
            
        }
        public FileGraph(bool allowParallelEdges)
            : base(allowParallelEdges)
        {
            
        }
    }
}
