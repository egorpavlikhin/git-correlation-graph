using System;

namespace GitCorrelationGraph.Models
{
    /// <summary>
    /// Represents an edge between two file nodes in the correlation graph
    /// </summary>
    public class FileEdge
    {
        /// <summary>
        /// Path of the source file
        /// </summary>
        public string SourceFilePath { get; set; }

        /// <summary>
        /// Path of the target file
        /// </summary>
        public string TargetFilePath { get; set; }

        /// <summary>
        /// Number of times these files appear together in commits
        /// </summary>
        public int CoCommitCount { get; set; }

        /// <summary>
        /// Calculated correlation between the two files
        /// </summary>
        public double Correlation => CalculateCorrelation();

        /// <summary>
        /// Reference to the source node
        /// </summary>
        public FileNode? SourceNode { get; set; }

        /// <summary>
        /// Reference to the target node
        /// </summary>
        public FileNode? TargetNode { get; set; }

        public FileEdge(string sourceFilePath, string targetFilePath)
        {
            SourceFilePath = sourceFilePath;
            TargetFilePath = targetFilePath;
            CoCommitCount = 0;
        }

        /// <summary>
        /// Calculate the correlation between the two files
        /// Correlation = (times appear in commits together) / (times appears in commits - lowest number between two files)
        /// </summary>
        private double CalculateCorrelation()
        {
            if (SourceNode == null || TargetNode == null)
                return 0;

            int minCommitCount = Math.Min(SourceNode.CommitCount, TargetNode.CommitCount);

            if (minCommitCount == 0)
                return 0;

            return (double)CoCommitCount / minCommitCount;
        }
    }
}
