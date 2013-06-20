using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glitter.Graph
{
    class FileGraph : QuickGraph.BidirectionalGraph<FileVertex, FileEdge>
    {
        public FileGraph()
        {
            
        }
        public FileGraph(bool allowParallelEdges)
            : base(allowParallelEdges)
        {
            
        }
        public FileGraph(bool allowParallelEdges, int vertexCapacity)
            : base(allowParallelEdges, vertexCapacity)
        {
            
        }
        public FileGraph(bool allowParallelEdges, int vertexCapacity, int edgeCapacity)
            : base(allowParallelEdges, vertexCapacity, edgeCapacity)
        {
            
        }
        public FileGraph(bool allowParallelEdges, int vertexCapacity, int edgeCapacity, IEqualityComparer<FileVertex> vertexComparer)
            : base(allowParallelEdges, vertexCapacity, edgeCapacity, vertexComparer)
        {
            
        }
        public FileGraph(bool allowParallelEdges, int capacity, Func<int, QuickGraph.Collections.IVertexEdgeDictionary<FileVertex, FileEdge>> vertexEdgesDictionaryFactory)
            : base(allowParallelEdges, capacity, vertexEdgesDictionaryFactory)
        {
            
        }
    }
}
