using System;
using System.Linq;
using Xunit;
using Shouldly;
using GitCorrelationGraph.Models;

namespace GitCorrelationGraph.Tests.Models
{
    public class CorrelationGraphTests
    {
        [Fact]
        public void CorrelationGraph_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var graph = new CorrelationGraph();

            // Assert
            graph.Nodes.ShouldNotBeNull();
            graph.Nodes.Count.ShouldBe(0);
            graph.ProcessingState.ShouldNotBeNull();
            graph.ProcessingState.LastProcessedCommitHash.ShouldBe(string.Empty);
            graph.ProcessingState.TotalCommitsProcessed.ShouldBe(0);
        }

        [Fact]
        public void GetOrCreateNode_ShouldCreateNewNode_WhenNodeDoesNotExist()
        {
            // Arrange
            var graph = new CorrelationGraph();
            var filePath = "test/file.cs";

            // Act
            var node = graph.GetOrCreateNode(filePath);

            // Assert
            node.ShouldNotBeNull();
            node.FilePath.ShouldBe(filePath);
            graph.Nodes.Count.ShouldBe(1);
            graph.Nodes[filePath].ShouldBe(node);
        }

        [Fact]
        public void GetOrCreateNode_ShouldReturnExistingNode_WhenNodeExists()
        {
            // Arrange
            var graph = new CorrelationGraph();
            var filePath = "test/file.cs";
            var existingNode = graph.GetOrCreateNode(filePath);
            existingNode.CommitCount = 5; // Modify the node

            // Act
            var node = graph.GetOrCreateNode(filePath);

            // Assert
            node.ShouldBe(existingNode);
            node.CommitCount.ShouldBe(5);
            graph.Nodes.Count.ShouldBe(1);
        }

        [Fact]
        public void GetOrCreateEdge_ShouldCreateNodesAndEdge_WhenNodesDoNotExist()
        {
            // Arrange
            var graph = new CorrelationGraph();
            var sourceFilePath = "source.cs";
            var targetFilePath = "target.cs";

            // Act
            var edge = graph.GetOrCreateEdge(sourceFilePath, targetFilePath);

            // Assert
            edge.ShouldNotBeNull();
            edge.SourceFilePath.ShouldBe(sourceFilePath);
            edge.TargetFilePath.ShouldBe(targetFilePath);

            graph.Nodes.Count.ShouldBe(2);
            graph.Nodes.ContainsKey(sourceFilePath).ShouldBeTrue();
            graph.Nodes.ContainsKey(targetFilePath).ShouldBeTrue();

            edge.SourceNode.ShouldBe(graph.Nodes[sourceFilePath]);
            edge.TargetNode.ShouldBe(graph.Nodes[targetFilePath]);

            graph.Nodes[sourceFilePath].Edges.Count.ShouldBe(1);
            graph.Nodes[sourceFilePath].Edges[targetFilePath].ShouldBe(edge);
        }

        [Fact]
        public void GetOrCreateEdge_ShouldReturnExistingEdge_WhenEdgeExists()
        {
            // Arrange
            var graph = new CorrelationGraph();
            var sourceFilePath = "source.cs";
            var targetFilePath = "target.cs";

            var existingEdge = graph.GetOrCreateEdge(sourceFilePath, targetFilePath);
            existingEdge.CoCommitCount = 3; // Modify the edge

            // Act
            var edge = graph.GetOrCreateEdge(sourceFilePath, targetFilePath);

            // Assert
            edge.ShouldBe(existingEdge);
            edge.CoCommitCount.ShouldBe(3);
        }

        [Fact]
        public void GetTopCorrelations_ShouldReturnOrderedCorrelations()
        {
            // Arrange
            var graph = new CorrelationGraph();

            // Create nodes and edges with different correlations
            var nodeA = graph.GetOrCreateNode("A.cs");
            var nodeB = graph.GetOrCreateNode("B.cs");
            var nodeC = graph.GetOrCreateNode("C.cs");

            nodeA.CommitCount = 10;
            nodeB.CommitCount = 5;
            nodeC.CommitCount = 8;

            var edgeAB = graph.GetOrCreateEdge("A.cs", "B.cs");
            edgeAB.CoCommitCount = 4; // Correlation: 4/5 = 0.8

            var edgeAC = graph.GetOrCreateEdge("A.cs", "C.cs");
            edgeAC.CoCommitCount = 7; // Correlation: 7/8 = 0.875

            var edgeBC = graph.GetOrCreateEdge("B.cs", "C.cs");
            edgeBC.CoCommitCount = 3; // Correlation: 3/5 = 0.6

            // Act
            var topCorrelations = graph.GetTopCorrelations(3).ToList();

            // Assert
            topCorrelations.Count.ShouldBe(3);
            topCorrelations[0].ShouldBe(edgeAC); // Highest correlation
            topCorrelations[1].ShouldBe(edgeAB); // Second highest
            topCorrelations[2].ShouldBe(edgeBC); // Lowest correlation

            topCorrelations[0].Correlation.ShouldBeGreaterThan(topCorrelations[1].Correlation);
            topCorrelations[1].Correlation.ShouldBeGreaterThan(topCorrelations[2].Correlation);
        }

        [Fact]
        public void RemoveNode_ShouldRemoveNodeAndItsEdges()
        {
            // Arrange
            var graph = new CorrelationGraph();

            // Create a graph with 3 nodes and edges between them
            var nodeA = graph.GetOrCreateNode("A.cs");
            var nodeB = graph.GetOrCreateNode("B.cs");
            var nodeC = graph.GetOrCreateNode("C.cs");

            var edgeAB = graph.GetOrCreateEdge("A.cs", "B.cs");
            var edgeAC = graph.GetOrCreateEdge("A.cs", "C.cs");
            var edgeBC = graph.GetOrCreateEdge("B.cs", "C.cs");

            // Act
            var result = graph.RemoveNode("B.cs");

            // Assert
            result.ShouldBeTrue();
            graph.Nodes.Count.ShouldBe(2);
            graph.Nodes.ContainsKey("B.cs").ShouldBeFalse();

            // Check that edges to B.cs were removed
            graph.Nodes["A.cs"].Edges.ContainsKey("B.cs").ShouldBeFalse();
            graph.Nodes["C.cs"].Edges.ContainsKey("B.cs").ShouldBeFalse();

            // Check that other edges still exist
            graph.Nodes["A.cs"].Edges.ContainsKey("C.cs").ShouldBeTrue();
        }

        [Fact]
        public void RemoveNode_ShouldReturnFalse_WhenNodeDoesNotExist()
        {
            // Arrange
            var graph = new CorrelationGraph();
            graph.GetOrCreateNode("A.cs");

            // Act
            var result = graph.RemoveNode("NonExistentFile.cs");

            // Assert
            result.ShouldBeFalse();
            graph.Nodes.Count.ShouldBe(1);
        }
    }
}
