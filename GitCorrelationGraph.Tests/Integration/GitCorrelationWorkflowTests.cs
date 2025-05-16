using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using LibGit2Sharp;
using GitCorrelationGraph.Git;
using GitCorrelationGraph.Models;
using GitCorrelationGraph.Services;
using GitCorrelationGraph.Serialization;

namespace GitCorrelationGraph.Tests.Integration
{
    public class GitCorrelationWorkflowTests : IDisposable
    {
        private readonly string _tempRepoPath;
        private readonly string _tempOutputPath;
        private Repository _repository;
        
        public GitCorrelationWorkflowTests()
        {
            // Create a temporary repository for testing
            _tempRepoPath = Path.Combine(Path.GetTempPath(), "git-correlation-test-" + Guid.NewGuid());
            _tempOutputPath = Path.Combine(Path.GetTempPath(), "correlation-graph-" + Guid.NewGuid() + ".json");
            
            Directory.CreateDirectory(_tempRepoPath);
            Repository.Init(_tempRepoPath);
            
            _repository = new Repository(_tempRepoPath);
            
            // Create test commits with specific patterns to test correlation
            CreateTestCommits();
        }
        
        [Fact]
        public async Task CompleteWorkflow_ShouldCalculateCorrectCorrelations()
        {
            // Arrange
            var analyzer = new CorrelationAnalyzer(_tempRepoPath, _tempOutputPath, 10);
            
            // Act
            var graph = await analyzer.AnalyzeAsync();
            
            // Assert
            graph.ShouldNotBeNull();
            graph.Nodes.Count.ShouldBe(4); // We created 4 files
            
            // Check that file1.txt and file2.txt have high correlation (3/3 = 100%)
            var file1Node = graph.Nodes["file1.txt"];
            var file2Node = graph.Nodes["file2.txt"];
            
            file1Node.ShouldNotBeNull();
            file2Node.ShouldNotBeNull();
            
            file1Node.CommitCount.ShouldBe(3); // Appears in 3 commits
            file2Node.CommitCount.ShouldBe(3); // Appears in 3 commits
            
            var edge = file1Node.Edges["file2.txt"];
            edge.ShouldNotBeNull();
            edge.CoCommitCount.ShouldBe(3); // They appear together in 3 commits
            edge.Correlation.ShouldBe(1.0); // 3/3 = 100% correlation
            
            // Check that file3.txt and file4.txt have lower correlation (1/2 = 50%)
            var file3Node = graph.Nodes["file3.txt"];
            var file4Node = graph.Nodes["file4.txt"];
            
            file3Node.ShouldNotBeNull();
            file4Node.ShouldNotBeNull();
            
            file3Node.CommitCount.ShouldBe(2); // Appears in 2 commits
            file4Node.CommitCount.ShouldBe(2); // Appears in 2 commits
            
            var edge34 = file3Node.Edges["file4.txt"];
            edge34.ShouldNotBeNull();
            edge34.CoCommitCount.ShouldBe(1); // They appear together in 1 commit
            edge34.Correlation.ShouldBe(0.5); // 1/2 = 50% correlation
            
            // Verify that the output file was created
            File.Exists(_tempOutputPath).ShouldBeTrue();
        }
        
        [Fact]
        public async Task ContinuingProcess_ShouldUpdateCorrelations()
        {
            // Arrange
            var analyzer = new CorrelationAnalyzer(_tempRepoPath, _tempOutputPath, 2);
            
            // Process the first 2 commits
            await analyzer.AnalyzeAsync();
            
            // Act - Process the remaining commits
            var graph = await analyzer.AnalyzeAsync();
            
            // Assert
            graph.ProcessingState.TotalCommitsProcessed.ShouldBe(4); // All 4 commits processed
            
            // Check that correlations are calculated correctly
            var file1Node = graph.Nodes["file1.txt"];
            var file2Node = graph.Nodes["file2.txt"];
            
            var edge = file1Node.Edges["file2.txt"];
            edge.CoCommitCount.ShouldBe(3); // They appear together in 3 commits
            edge.Correlation.ShouldBe(1.0); // 3/3 = 100% correlation
        }
        
        private void CreateTestCommits()
        {
            var author = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
            
            // Create first commit with file1.txt and file2.txt
            File.WriteAllText(Path.Combine(_tempRepoPath, "file1.txt"), "Test content 1");
            File.WriteAllText(Path.Combine(_tempRepoPath, "file2.txt"), "Test content 2");
            Commands.Stage(_repository, "file1.txt");
            Commands.Stage(_repository, "file2.txt");
            _repository.Commit("First commit", author, author);
            
            // Create second commit modifying file1.txt and file2.txt
            File.AppendAllText(Path.Combine(_tempRepoPath, "file1.txt"), "\nModified in commit 2");
            File.AppendAllText(Path.Combine(_tempRepoPath, "file2.txt"), "\nModified in commit 2");
            Commands.Stage(_repository, "file1.txt");
            Commands.Stage(_repository, "file2.txt");
            _repository.Commit("Second commit", author, author);
            
            // Create third commit with file3.txt and modifying file1.txt and file2.txt
            File.WriteAllText(Path.Combine(_tempRepoPath, "file3.txt"), "Test content 3");
            File.AppendAllText(Path.Combine(_tempRepoPath, "file1.txt"), "\nModified in commit 3");
            File.AppendAllText(Path.Combine(_tempRepoPath, "file2.txt"), "\nModified in commit 3");
            Commands.Stage(_repository, "file3.txt");
            Commands.Stage(_repository, "file1.txt");
            Commands.Stage(_repository, "file2.txt");
            _repository.Commit("Third commit", author, author);
            
            // Create fourth commit with file4.txt and modifying file3.txt
            File.WriteAllText(Path.Combine(_tempRepoPath, "file4.txt"), "Test content 4");
            File.AppendAllText(Path.Combine(_tempRepoPath, "file3.txt"), "\nModified in commit 4");
            Commands.Stage(_repository, "file4.txt");
            Commands.Stage(_repository, "file3.txt");
            _repository.Commit("Fourth commit", author, author);
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
