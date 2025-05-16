using System;
using System.Collections.Generic;

namespace GitCorrelationGraph.Models
{
    /// <summary>
    /// Represents a file node in the correlation graph
    /// </summary>
    public class FileNode
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
        /// Dictionary of edges to other files, keyed by the other file's path
        /// </summary>
        public Dictionary<string, FileEdge> Edges { get; set; }
        
        public FileNode(string filePath)
        {
            FilePath = filePath;
            CommitCount = 0;
            Edges = new Dictionary<string, FileEdge>();
        }
    }
}
