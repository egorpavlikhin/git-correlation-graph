using System;
using Xunit;
using Shouldly;
using GitCorrelationGraph.Models;

namespace GitCorrelationGraph.Tests.Models
{
    public class FileEdgeTests
    {
        [Fact]
        public void FileEdge_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var sourceFilePath = "source.cs";
            var targetFilePath = "target.cs";
            var edge = new FileEdge(sourceFilePath, targetFilePath);
            
            // Assert
            edge.SourceFilePath.ShouldBe(sourceFilePath);
            edge.TargetFilePath.ShouldBe(targetFilePath);
            edge.CoCommitCount.ShouldBe(0);
            edge.SourceNode.ShouldBeNull();
            edge.TargetNode.ShouldBeNull();
        }
        
        [Fact]
        public void FileEdge_ShouldCalculateCorrelation_WhenNodesAreSet()
        {
            // Arrange
            var sourceNode = new FileNode("source.cs") { CommitCount = 10 };
            var targetNode = new FileNode("target.cs") { CommitCount = 5 };
            
            var edge = new FileEdge("source.cs", "target.cs")
            {
                SourceNode = sourceNode,
                TargetNode = targetNode,
                CoCommitCount = 4
            };
            
            // Act
            var correlation = edge.Correlation;
            
            // Assert
            correlation.ShouldBe(0.8); // 4 / 5 = 0.8 (80%)
        }
        
        [Fact]
        public void FileEdge_ShouldCalculateCorrelation_WhenSourceNodeHasFewerCommits()
        {
            // Arrange
            var sourceNode = new FileNode("source.cs") { CommitCount = 5 };
            var targetNode = new FileNode("target.cs") { CommitCount = 10 };
            
            var edge = new FileEdge("source.cs", "target.cs")
            {
                SourceNode = sourceNode,
                TargetNode = targetNode,
                CoCommitCount = 4
            };
            
            // Act
            var correlation = edge.Correlation;
            
            // Assert
            correlation.ShouldBe(0.8); // 4 / 5 = 0.8 (80%)
        }
        
        [Fact]
        public void FileEdge_ShouldReturnZeroCorrelation_WhenNodesAreNull()
        {
            // Arrange
            var edge = new FileEdge("source.cs", "target.cs");
            
            // Act
            var correlation = edge.Correlation;
            
            // Assert
            correlation.ShouldBe(0);
        }
        
        [Fact]
        public void FileEdge_ShouldReturnZeroCorrelation_WhenCommitCountIsZero()
        {
            // Arrange
            var sourceNode = new FileNode("source.cs") { CommitCount = 0 };
            var targetNode = new FileNode("target.cs") { CommitCount = 5 };
            
            var edge = new FileEdge("source.cs", "target.cs")
            {
                SourceNode = sourceNode,
                TargetNode = targetNode,
                CoCommitCount = 0
            };
            
            // Act
            var correlation = edge.Correlation;
            
            // Assert
            correlation.ShouldBe(0);
        }
    }
}
