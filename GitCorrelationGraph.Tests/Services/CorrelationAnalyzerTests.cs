using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Moq;
using LibGit2Sharp;
using GitCorrelationGraph.Git;
using GitCorrelationGraph.Models;
using GitCorrelationGraph.Services;
using GitCorrelationGraph.Serialization;

namespace GitCorrelationGraph.Tests.Services
{
    public class CorrelationAnalyzerTests : IDisposable
    {
        private readonly string _tempRepoPath;
        private readonly string _tempOutputPath;
        private Repository _repository;
        
        public CorrelationAnalyzerTests()
        {
            // Create a temporary repository for testing
            _tempRepoPath = Path.Combine(Path.GetTempPath(), "git-correlation-test-" + Guid.NewGuid());
            _tempOutputPath = Path.Combine(Path.GetTempPath(), "correlation-graph-" + Guid.NewGuid() + ".json");
            
            Directory.CreateDirectory(_tempRepoPath);
            Repository.Init(_tempRepoPath);
            
            _repository = new Repository(_tempRepoPath);
            
            // Create some test commits
            CreateTestCommits();
        }
        
        [Fact]
        public async Task AnalyzeAsync_ShouldProcessCommitsAndSaveGraph()
        {
            // Arrange
            var analyzer = new CorrelationAnalyzer(_tempRepoPath, _tempOutputPath, 10);
            
            // Act
            var graph = await analyzer.AnalyzeAsync();
            
            // Assert
            graph.ShouldNotBeNull();
            graph.Nodes.Count.ShouldBeGreaterThan(0);
            graph.ProcessingState.TotalCommitsProcessed.ShouldBe(3); // We created 3 test commits
            
            // Verify that the output file was created
            File.Exists(_tempOutputPath).ShouldBeTrue();
        }
        
        [Fact]
        public async Task AnalyzeAsync_ShouldContinueFromLastProcessedCommit()
        {
            // Arrange
            var analyzer = new CorrelationAnalyzer(_tempRepoPath, _tempOutputPath, 1);
            
            // Process the first commit
            await analyzer.AnalyzeAsync();
            
            // Add a new commit
            AddNewCommit();
            
            // Act - Process the remaining commits
            var graph = await analyzer.AnalyzeAsync();
            
            // Assert
            graph.ProcessingState.TotalCommitsProcessed.ShouldBe(4); // 3 initial + 1 new
            
            // The graph should contain all files
            graph.Nodes.Count.ShouldBeGreaterThanOrEqualTo(4); // At least 4 files
        }
        
        [Fact]
        public void DisplayTopCorrelations_ShouldNotThrowException()
        {
            // Arrange
            var graph = CreateTestGraph();
            var analyzer = new CorrelationAnalyzer(_tempRepoPath, _tempOutputPath, 10);
            
            // Act & Assert
            Should.NotThrow(() => analyzer.DisplayTopCorrelations(graph, 2));
        }
        
        private void CreateTestCommits()
        {
            // Create first commit with file1.txt
            File.WriteAllText(Path.Combine(_tempRepoPath, "file1.txt"), "Test content 1");
            Commands.Stage(_repository, "file1.txt");
            
            var author = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
            _repository.Commit("Initial commit", author, author);
            
            // Create second commit with file2.txt
            File.WriteAllText(Path.Combine(_tempRepoPath, "file2.txt"), "Test content 2");
            Commands.Stage(_repository, "file2.txt");
            
            _repository.Commit("Second commit", author, author);
            
            // Create third commit with file3.txt and modify file1.txt
            File.WriteAllText(Path.Combine(_tempRepoPath, "file3.txt"), "Test content 3");
            File.AppendAllText(Path.Combine(_tempRepoPath, "file1.txt"), "\nModified content");
            Commands.Stage(_repository, "file3.txt");
            Commands.Stage(_repository, "file1.txt");
            
            _repository.Commit("Third commit", author, author);
        }
        
        private void AddNewCommit()
        {
            // Create a new commit with file4.txt
            File.WriteAllText(Path.Combine(_tempRepoPath, "file4.txt"), "Test content 4");
            Commands.Stage(_repository, "file4.txt");
            
            var author = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
            _repository.Commit("Fourth commit", author, author);
        }
        
        private CorrelationGraph CreateTestGraph()
        {
            var graph = new CorrelationGraph();
            
            // Create nodes
            var nodeA = graph.GetOrCreateNode("A.cs");
            var nodeB = graph.GetOrCreateNode("B.cs");
            
            nodeA.CommitCount = 10;
            nodeB.CommitCount = 5;
            
            // Create edge
            var edgeAB = graph.GetOrCreateEdge("A.cs", "B.cs");
            edgeAB.CoCommitCount = 4;
            
            return graph;
        }
        
        public void Dispose()
        {
            _repository.Dispose();
            
            // Clean up the temporary repository and output file
            try
            {
                Directory.Delete(_tempRepoPath, true);
                
                if (File.Exists(_tempOutputPath))
                {
                    File.Delete(_tempOutputPath);
                }
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }
    }
}
