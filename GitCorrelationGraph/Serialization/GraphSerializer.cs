using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GitCorrelationGraph.Models;
using GitCorrelationGraph.Models.Serialization;

namespace GitCorrelationGraph.Serialization
{
    /// <summary>
    /// Handles serialization and deserialization of the correlation graph
    /// </summary>
    public class GraphSerializer
    {
        private readonly string _filePath;

        public GraphSerializer(string filePath)
        {
            _filePath = filePath;
        }

        /// <summary>
        /// Save the graph to a JSON file
        /// </summary>
        public async Task SaveGraphAsync(CorrelationGraph graph)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            // Convert to serializable format
            var serializableGraph = SerializableCorrelationGraph.FromCorrelationGraph(graph);

            var json = JsonConvert.SerializeObject(serializableGraph, settings);

            await File.WriteAllTextAsync(_filePath, json);
        }

        /// <summary>
        /// Load the graph from a JSON file
        /// </summary>
        public async Task<CorrelationGraph> LoadGraphAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new CorrelationGraph();
            }

            var json = await File.ReadAllTextAsync(_filePath);

            var serializableGraph = JsonConvert.DeserializeObject<SerializableCorrelationGraph>(json);

            if (serializableGraph != null)
            {
                return serializableGraph.ToCorrelationGraph();
            }

            return new CorrelationGraph();
        }
    }
}
