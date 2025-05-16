using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Shouldly;
using LibGit2Sharp;
using GitCorrelationGraph.Git;
using GitCorrelationGraph.Models;
using Moq;

namespace GitCorrelationGraph.Tests.Git
{
    public class CommitProcessorTests : IDisposable
    {
        private readonly string _tempRepoPath;
        private Repository _repository;

        public CommitProcessorTests()
        {
            // Create a temporary repository for testing
            _tempRepoPath = Path.Combine(Path.GetTempPath(), "git-correlation-test-" + Guid.NewGuid());
            Directory.CreateDirectory(_tempRepoPath);
            Repository.Init(_tempRepoPath);

            _repository = new Repository(_tempRepoPath);

            // Create some test commits
            CreateTestCommits();
        }

        [Fact]
        public void ProcessCommit_ShouldUpdateGraph_WithFilesFromCommit()
        {
            // Arrange
            using var reader = new GitRepositoryReader(_tempRepoPath, FileFilter.CreateTestFilter());
            var processor = new CommitProcessor(reader);
            var graph = new CorrelationGraph();

            var commit = reader.GetCommitBatch(string.Empty, 1).First();

            // Act
            processor.ProcessCommit(commit, graph);

            // Assert
            graph.Nodes.Count.ShouldBeGreaterThan(0);
            graph.ProcessingState.LastProcessedCommitHash.ShouldBe(commit.Sha);
            graph.ProcessingState.TotalCommitsProcessed.ShouldBe(1);
        }

        [Fact]
        public void ProcessCommit_ShouldIncrementCommitCount_ForEachFile()
        {
            // Arrange
            using var reader = new GitRepositoryReader(_tempRepoPath, FileFilter.CreateTestFilter());
            var processor = new CommitProcessor(reader);
            var graph = new CorrelationGraph();

            var commit = reader.GetCommitBatch(string.Empty, 1).First();

            // Act
            processor.ProcessCommit(commit, graph);

            // Assert
            foreach (var node in graph.Nodes.Values)
            {
                node.CommitCount.ShouldBe(1);
            }
        }

        [Fact]
        public void ProcessCommit_ShouldCreateEdges_BetweenFilesInSameCommit()
        {
            // Arrange
            using var reader = new GitRepositoryReader(_tempRepoPath, FileFilter.CreateTestFilter());
            var processor = new CommitProcessor(reader);
            var graph = new CorrelationGraph();

            // Get the third commit which modifies file1.txt and adds file3.txt
            var commit = reader.GetCommitBatch(string.Empty, 10)
                .First(c => c.Message == "Third commit");

            // Act
            processor.ProcessCommit(commit, graph);

            // Assert
            graph.Nodes.Count.ShouldBe(2); // file1.txt and file3.txt

            // Check that there's an edge between file1.txt and file3.txt
            var file1Node = graph.Nodes.Values.FirstOrDefault(n => n.FilePath == "file1.txt");
            file1Node.ShouldNotBeNull();
            file1Node.Edges.Count.ShouldBe(1);

            var edge = file1Node.Edges.Values.First();
            edge.SourceFilePath.ShouldBe("file1.txt");
            edge.TargetFilePath.ShouldBe("file3.txt");
            edge.CoCommitCount.ShouldBe(1);
        }

        [Fact]
        public void ProcessCommitBatch_ShouldProcessMultipleCommits()
        {
            // Arrange
            using var reader = new GitRepositoryReader(_tempRepoPath, FileFilter.CreateTestFilter());
            var processor = new CommitProcessor(reader);
            var graph = new CorrelationGraph();

            // Act
            var processedCount = processor.ProcessCommitBatch(string.Empty, 10, graph);

            // Assert
            processedCount.ShouldBe(3); // We created 3 test commits
            graph.ProcessingState.TotalCommitsProcessed.ShouldBe(3);
            graph.Nodes.Count.ShouldBeGreaterThanOrEqualTo(3); // At least 3 files
        }

        [Fact]
        public void ProcessCommitBatch_ShouldRespectStartCommit()
        {
            // Arrange
            using var reader = new GitRepositoryReader(_tempRepoPath);
            var processor = new CommitProcessor(reader);
            var graph = new CorrelationGraph();

            // Process the first commit
            var firstCommit = reader.GetCommitBatch(string.Empty, 1).First();
            processor.ProcessCommit(firstCommit, graph);

            // Act - Process the remaining commits
            var processedCount = processor.ProcessCommitBatch(firstCommit.Sha, 10, graph);

            // Assert
            processedCount.ShouldBe(2); // 2 remaining commits after the first one
            graph.ProcessingState.TotalCommitsProcessed.ShouldBe(3); // 1 + 2 = 3
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

        public void Dispose()
        {
            _repository.Dispose();

            // Clean up the temporary repository
            try
            {
                Directory.Delete(_tempRepoPath, true);
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }
    }
}
