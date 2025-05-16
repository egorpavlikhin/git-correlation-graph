using System;
using System.Collections.Generic;
using System.Linq;

namespace GitCorrelationGraph.Models.Serialization
{
    /// <summary>
    /// Serialization-friendly version of CorrelationGraph
    /// </summary>
    public class SerializableCorrelationGraph
    {
        /// <summary>
        /// List of file nodes
        /// </summary>
        public List<SerializableFileNode> Nodes { get; set; }
        
        /// <summary>
        /// Information about the last processed commit
        /// </summary>
        public ProcessingState ProcessingState { get; set; }
        
        public SerializableCorrelationGraph()
        {
            Nodes = new List<SerializableFileNode>();
            ProcessingState = new ProcessingState();
        }
        
        /// <summary>
        /// Create a serializable graph from a CorrelationGraph
        /// </summary>
        public static SerializableCorrelationGraph FromCorrelationGraph(CorrelationGraph graph)
        {
            var serializableGraph = new SerializableCorrelationGraph
            {
                ProcessingState = graph.ProcessingState,
                Nodes = new List<SerializableFileNode>()
            };
            
            foreach (var node in graph.Nodes.Values)
            {
                serializableGraph.Nodes.Add(SerializableFileNode.FromFileNode(node));
            }
            
            return serializableGraph;
        }
        
        /// <summary>
        /// Convert back to a CorrelationGraph
        /// </summary>
        public CorrelationGraph ToCorrelationGraph()
        {
            var graph = new CorrelationGraph
            {
                ProcessingState = ProcessingState
            };
            
            // First, create all nodes
            foreach (var serializableNode in Nodes)
            {
                graph.Nodes[serializableNode.FilePath] = serializableNode.ToFileNode();
            }
            
            // Then, create all edges and restore references
            foreach (var serializableNode in Nodes)
            {
                var sourceNode = graph.Nodes[serializableNode.FilePath];
                
                foreach (var serializableEdge in serializableNode.Edges)
                {
                    var edge = serializableEdge.ToFileEdge();
                    edge.SourceNode = sourceNode;
                    edge.TargetNode = graph.Nodes[serializableEdge.TargetFilePath];
                    
                    sourceNode.Edges[serializableEdge.TargetFilePath] = edge;
                }
            }
            
            return graph;
        }
    }
}
