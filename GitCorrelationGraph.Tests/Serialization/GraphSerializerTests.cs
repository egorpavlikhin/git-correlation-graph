using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using GitCorrelationGraph.Models;
using GitCorrelationGraph.Serialization;

namespace GitCorrelationGraph.Tests.Serialization
{
    public class GraphSerializerTests : IDisposable
    {
        private readonly string _tempFilePath;
        
        public GraphSerializerTests()
        {
            _tempFilePath = Path.Combine(Path.GetTempPath(), "graph-test-" + Guid.NewGuid() + ".json");
        }
        
        [Fact]
        public async Task SaveAndLoadGraph_ShouldPreserveGraphStructure()
        {
            // Arrange
            var graph = CreateTestGraph();
            var serializer = new GraphSerializer(_tempFilePath);
            
            // Act
            await serializer.SaveGraphAsync(graph);
            var loadedGraph = await serializer.LoadGraphAsync();
            
            // Assert
            loadedGraph.ShouldNotBeNull();
            loadedGraph.Nodes.Count.ShouldBe(graph.Nodes.Count);
            loadedGraph.ProcessingState.LastProcessedCommitHash.ShouldBe(graph.ProcessingState.LastProcessedCommitHash);
            loadedGraph.ProcessingState.LastProcessedCommitDate.ShouldBe(graph.ProcessingState.LastProcessedCommitDate);
            loadedGraph.ProcessingState.TotalCommitsProcessed.ShouldBe(graph.ProcessingState.TotalCommitsProcessed);
            
            // Check that nodes were preserved
            foreach (var nodePath in graph.Nodes.Keys)
            {
                loadedGraph.Nodes.ContainsKey(nodePath).ShouldBeTrue();
                loadedGraph.Nodes[nodePath].CommitCount.ShouldBe(graph.Nodes[nodePath].CommitCount);
                loadedGraph.Nodes[nodePath].Edges.Count.ShouldBe(graph.Nodes[nodePath].Edges.Count);
            }
            
            // Check that edges were preserved
            var sourceNode = graph.Nodes["A.cs"];
            var loadedSourceNode = loadedGraph.Nodes["A.cs"];
            
            foreach (var edgePath in sourceNode.Edges.Keys)
            {
                loadedSourceNode.Edges.ContainsKey(edgePath).ShouldBeTrue();
                loadedSourceNode.Edges[edgePath].CoCommitCount.ShouldBe(sourceNode.Edges[edgePath].CoCommitCount);
                
                // Check that references were restored
                loadedSourceNode.Edges[edgePath].SourceNode.ShouldBe(loadedSourceNode);
                loadedSourceNode.Edges[edgePath].TargetNode.ShouldBe(loadedGraph.Nodes[edgePath]);
            }
        }
        
        [Fact]
        public async Task LoadGraph_ShouldReturnNewGraph_WhenFileDoesNotExist()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "non-existent-" + Guid.NewGuid() + ".json");
            var serializer = new GraphSerializer(nonExistentPath);
            
            // Act
            var graph = await serializer.LoadGraphAsync();
            
            // Assert
            graph.ShouldNotBeNull();
            graph.Nodes.Count.ShouldBe(0);
            graph.ProcessingState.LastProcessedCommitHash.ShouldBe(string.Empty);
            graph.ProcessingState.TotalCommitsProcessed.ShouldBe(0);
        }
        
        private CorrelationGraph CreateTestGraph()
        {
            var graph = new CorrelationGraph();
            
            // Create nodes
            var nodeA = graph.GetOrCreateNode("A.cs");
            var nodeB = graph.GetOrCreateNode("B.cs");
            var nodeC = graph.GetOrCreateNode("C.cs");
            
            nodeA.CommitCount = 10;
            nodeB.CommitCount = 5;
            nodeC.CommitCount = 8;
            
            // Create edges
            var edgeAB = graph.GetOrCreateEdge("A.cs", "B.cs");
            edgeAB.CoCommitCount = 4;
            
            var edgeAC = graph.GetOrCreateEdge("A.cs", "C.cs");
            edgeAC.CoCommitCount = 7;
            
            var edgeBC = graph.GetOrCreateEdge("B.cs", "C.cs");
            edgeBC.CoCommitCount = 3;
            
            // Set processing state
            graph.ProcessingState.LastProcessedCommitHash = "abc123";
            graph.ProcessingState.LastProcessedCommitDate = new DateTime(2023, 1, 1);
            graph.ProcessingState.TotalCommitsProcessed = 15;
            
            return graph;
        }
        
        public void Dispose()
        {
            // Clean up the temporary file
            try
            {
                if (File.Exists(_tempFilePath))
                {
                    File.Delete(_tempFilePath);
                }
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }
    }
}
