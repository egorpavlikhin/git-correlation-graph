using System;
using System.Threading.Tasks;
using GitCorrelationGraph.Models;
using GitCorrelationGraph.Git;
using GitCorrelationGraph.Serialization;

namespace GitCorrelationGraph.Services
{
    /// <summary>
    /// Main service to coordinate the analysis process
    /// </summary>
    public class CorrelationAnalyzer
    {
        private readonly string _repositoryPath;
        private readonly string _outputFilePath;
        private readonly int _batchSize;

        public CorrelationAnalyzer(
            string repositoryPath,
            string outputFilePath,
            int batchSize = 100)
        {
            _repositoryPath = repositoryPath;
            _outputFilePath = outputFilePath;
            _batchSize = batchSize;
        }

        /// <summary>
        /// Run the analysis process
        /// </summary>
        public async Task<CorrelationGraph> AnalyzeAsync()
        {
            // Load existing graph if available
            var serializer = new GraphSerializer(_outputFilePath);
            var graph = await serializer.LoadGraphAsync();

            // Process commits
            using (var repositoryReader = new GitRepositoryReader(_repositoryPath))
            {
                var processor = new CommitProcessor(repositoryReader);

                var startCommitHash = graph.ProcessingState.LastProcessedCommitHash;
                var processedCount = processor.ProcessCommitBatch(startCommitHash, _batchSize, graph);

                Console.WriteLine($"Processed {processedCount} commits");
                Console.WriteLine($"Total commits processed: {graph.ProcessingState.TotalCommitsProcessed}");
                Console.WriteLine($"Total files: {graph.Nodes.Count}");

                if (processedCount > 0)
                {
                    // Save the updated graph
                    await serializer.SaveGraphAsync(graph);

                    Console.WriteLine($"Graph saved to {_outputFilePath}");
                    Console.WriteLine($"Last processed commit: {graph.ProcessingState.LastProcessedCommitHash}");
                    Console.WriteLine($"Last processed date: {graph.ProcessingState.LastProcessedCommitDate}");
                }
                else
                {
                    Console.WriteLine("No new commits to process");
                }
            }

            return graph;
        }

        /// <summary>
        /// Display the top correlations in the graph
        /// </summary>
        public void DisplayTopCorrelations(CorrelationGraph graph, int count = 10)
        {
            Console.WriteLine($"\nTop {count} file correlations:");
            Console.WriteLine("-----------------------------");

            var topCorrelations = graph.GetTopCorrelations(count);

            foreach (var edge in topCorrelations)
            {
                int sourceCommitCount = edge.SourceNode?.CommitCount ?? 0;
                int targetCommitCount = edge.TargetNode?.CommitCount ?? 0;
                int minCommitCount = Math.Min(sourceCommitCount, targetCommitCount);

                Console.WriteLine($"{edge.SourceFilePath} <-> {edge.TargetFilePath}: " +
                    $"{edge.Correlation:P2} ({edge.CoCommitCount}/{minCommitCount})");
            }
        }
    }
}
