using System;
using System.Collections.Generic;

namespace GitCorrelationGraph.Models.Serialization
{
    /// <summary>
    /// Serialization-friendly version of FileNode
    /// </summary>
    public class SerializableFileNode
    {
        /// <summary>
        /// Path of the file relative to the repository root
        /// </summary>
        public string FilePath { get; set; }
        
        /// <summary>
        /// Number of commits this file appears in
        /// </summary>
        public int CommitCount { get; set; }
        
        /// <summary>
        /// List of edges to other files
        /// </summary>
        public List<SerializableFileEdge> Edges { get; set; }
        
        public SerializableFileNode()
        {
            FilePath = string.Empty;
            CommitCount = 0;
            Edges = new List<SerializableFileEdge>();
        }
        
        /// <summary>
        /// Create a serializable node from a FileNode
        /// </summary>
        public static SerializableFileNode FromFileNode(FileNode node)
        {
            var serializableNode = new SerializableFileNode
            {
                FilePath = node.FilePath,
                CommitCount = node.CommitCount,
                Edges = new List<SerializableFileEdge>()
            };
            
            foreach (var edge in node.Edges.Values)
            {
                serializableNode.Edges.Add(SerializableFileEdge.FromFileEdge(edge));
            }
            
            return serializableNode;
        }
        
        /// <summary>
        /// Convert back to a FileNode
        /// </summary>
        public FileNode ToFileNode()
        {
            return new FileNode(FilePath)
            {
                CommitCount = CommitCount
                // Edges will be restored separately
            };
        }
    }
}
