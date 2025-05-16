using System;
using System.IO;
using System.Threading.Tasks;
using GitCorrelationGraph.Services;

namespace GitCorrelationGraph
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Git Correlation Graph Analyzer");
            Console.WriteLine("------------------------------");

            string repositoryPath = GetRepositoryPath(args);
            int batchSize = GetBatchSize(args);
            string outputFilePath = Path.Combine(repositoryPath, "correlation-graph.json");

            Console.WriteLine($"Repository path: {repositoryPath}");
            Console.WriteLine($"Batch size: {batchSize}");
            Console.WriteLine($"Output file: {outputFilePath}");
            Console.WriteLine();

            try
            {
                var analyzer = new CorrelationAnalyzer(repositoryPath, outputFilePath, batchSize);

                var graph = await analyzer.AnalyzeAsync();

                analyzer.DisplayTopCorrelations(graph);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Get the repository path from command-line arguments or use the current directory
        /// </summary>
        private static string GetRepositoryPath(string[] args)
        {
            if (args.Length > 0)
            {
                return args[0];
            }

            return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Get the batch size from command-line arguments or use the default
        /// </summary>
        private static int GetBatchSize(string[] args)
        {
            if (args.Length > 1 && int.TryParse(args[1], out int batchSize))
            {
                return batchSize;
            }

            return 100; // Default batch size
        }
    }
}
