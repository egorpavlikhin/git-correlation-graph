using System;
using Xunit;
using Shouldly;
using GitCorrelationGraph.Models;

namespace GitCorrelationGraph.Tests.Models
{
    public class FileNodeTests
    {
        [Fact]
        public void FileNode_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var filePath = "test/file.cs";
            var node = new FileNode(filePath);
            
            // Assert
            node.FilePath.ShouldBe(filePath);
            node.CommitCount.ShouldBe(0);
            node.Edges.ShouldNotBeNull();
            node.Edges.Count.ShouldBe(0);
        }
        
        [Fact]
        public void FileNode_ShouldTrackCommitCount()
        {
            // Arrange
            var node = new FileNode("test/file.cs");
            
            // Act
            node.CommitCount = 5;
            
            // Assert
            node.CommitCount.ShouldBe(5);
        }
        
        [Fact]
        public void FileNode_ShouldAddEdges()
        {
            // Arrange
            var sourceNode = new FileNode("source.cs");
            var targetNode = new FileNode("target.cs");
            
            // Act
            var edge = new FileEdge("source.cs", "target.cs")
            {
                SourceNode = sourceNode,
                TargetNode = targetNode,
                CoCommitCount = 3
            };
            
            sourceNode.Edges["target.cs"] = edge;
            
            // Assert
            sourceNode.Edges.Count.ShouldBe(1);
            sourceNode.Edges["target.cs"].ShouldBe(edge);
            sourceNode.Edges["target.cs"].CoCommitCount.ShouldBe(3);
        }
    }
}
