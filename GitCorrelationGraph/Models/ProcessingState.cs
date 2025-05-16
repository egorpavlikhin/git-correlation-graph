using System;

namespace GitCorrelationGraph.Models
{
    /// <summary>
    /// Represents the state of the processing, including the last processed commit
    /// </summary>
    public class ProcessingState
    {
        /// <summary>
        /// The hash of the last processed commit
        /// </summary>
        public string LastProcessedCommitHash { get; set; }
        
        /// <summary>
        /// The date of the last processed commit
        /// </summary>
        public DateTime LastProcessedCommitDate { get; set; }
        
        /// <summary>
        /// Total number of commits processed
        /// </summary>
        public int TotalCommitsProcessed { get; set; }
        
        public ProcessingState()
        {
            LastProcessedCommitHash = string.Empty;
            LastProcessedCommitDate = DateTime.MinValue;
            TotalCommitsProcessed = 0;
        }
    }
}
