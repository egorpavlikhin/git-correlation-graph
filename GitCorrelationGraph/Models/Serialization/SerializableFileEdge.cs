using System;

namespace GitCorrelationGraph.Models.Serialization
{
    /// <summary>
    /// Serialization-friendly version of FileEdge
    /// </summary>
    public class SerializableFileEdge
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
        
        public SerializableFileEdge()
        {
            SourceFilePath = string.Empty;
            TargetFilePath = string.Empty;
            CoCommitCount = 0;
        }
        
        /// <summary>
        /// Create a serializable edge from a FileEdge
        /// </summary>
        public static SerializableFileEdge FromFileEdge(FileEdge edge)
        {
            return new SerializableFileEdge
            {
                SourceFilePath = edge.SourceFilePath,
                TargetFilePath = edge.TargetFilePath,
                CoCommitCount = edge.CoCommitCount
            };
        }
        
        /// <summary>
        /// Convert back to a FileEdge
        /// </summary>
        public FileEdge ToFileEdge()
        {
            return new FileEdge(SourceFilePath, TargetFilePath)
            {
                CoCommitCount = CoCommitCount
                // SourceNode and TargetNode will be restored separately
            };
        }
    }
}
