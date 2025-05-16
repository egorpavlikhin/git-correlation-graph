using System;
using System.Collections.Generic;
using System.Linq;

namespace GitCorrelationGraph.Models
{
    /// <summary>
    /// Represents the correlation graph of files in a git repository
    /// </summary>
    public class CorrelationGraph
    {
        /// <summary>
        /// Dictionary of file nodes, keyed by file path
        /// </summary>
        public Dictionary<string, FileNode> Nodes { get; set; }

        /// <summary>
        /// Information about the last processed commit
        /// </summary>
        public ProcessingState ProcessingState { get; set; }

        public CorrelationGraph()
        {
            Nodes = new Dictionary<string, FileNode>();
            ProcessingState = new ProcessingState();
        }

        /// <summary>
        /// Get or create a node for the given file path
        /// </summary>
        public FileNode GetOrCreateNode(string filePath)
        {
            if (!Nodes.TryGetValue(filePath, out var node))
            {
                node = new FileNode(filePath);
                Nodes[filePath] = node;
            }

            return node;
        }

        /// <summary>
        /// Get or create an edge between two file nodes
        /// </summary>
        public FileEdge GetOrCreateEdge(string sourceFilePath, string targetFilePath)
        {
            var sourceNode = GetOrCreateNode(sourceFilePath);
            var targetNode = GetOrCreateNode(targetFilePath);

            if (!sourceNode.Edges.TryGetValue(targetFilePath, out var edge))
            {
                edge = new FileEdge(sourceFilePath, targetFilePath)
                {
                    SourceNode = sourceNode,
                    TargetNode = targetNode
                };

                sourceNode.Edges[targetFilePath] = edge;
            }

            return edge;
        }

        /// <summary>
        /// Get the top N correlations in the graph
        /// </summary>
        public IEnumerable<FileEdge> GetTopCorrelations(int count)
        {
            return Nodes.Values
                .SelectMany(n => n.Edges.Values)
                .OrderByDescending(e => e.Correlation)
                .Take(count);
        }

        /// <summary>
        /// Remove a node and all its edges from the graph
        /// </summary>
        /// <param name="filePath">The file path of the node to remove</param>
        /// <returns>True if the node was found and removed, false otherwise</returns>
        public bool RemoveNode(string filePath)
        {
            if (!Nodes.TryGetValue(filePath, out var nodeToRemove))
            {
                return false;
            }

            // Remove all edges that point to this node from other nodes
            foreach (var node in Nodes.Values)
            {
                node.Edges.Remove(filePath);
            }

            // Remove the node itself
            Nodes.Remove(filePath);

            return true;
        }
    }
}
