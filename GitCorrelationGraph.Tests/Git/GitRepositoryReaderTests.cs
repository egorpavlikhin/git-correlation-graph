using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Shouldly;
using LibGit2Sharp;
using GitCorrelationGraph.Git;

namespace GitCorrelationGraph.Tests.Git
{
    public class GitRepositoryReaderTests : IDisposable
    {
        private readonly string _tempRepoPath;
        private Repository _repository;

        public GitRepositoryReaderTests()
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
        public void GetCommitBatch_ShouldReturnCommits_WhenRepositoryHasCommits()
        {
            // Arrange
            using var reader = new GitRepositoryReader(_tempRepoPath);

            // Act
            var commits = reader.GetCommitBatch(string.Empty, 10).ToList();

            // Assert
            commits.ShouldNotBeNull();
            commits.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void GetCommitBatch_ShouldRespectBatchSize()
        {
            // Arrange
            using var reader = new GitRepositoryReader(_tempRepoPath);

            // Act
            var commits = reader.GetCommitBatch(string.Empty, 2).ToList();

            // Assert
            commits.Count.ShouldBe(2);
        }

        [Fact]
        public void GetFilesInCommit_ShouldReturnFiles_ForInitialCommit()
        {
            // Arrange
            using var reader = new GitRepositoryReader(_tempRepoPath, FileFilter.CreateTestFilter());
            var firstCommit = reader.GetCommitBatch(string.Empty, 10).Last(); // Get the first commit (oldest)

            // Act
            var files = reader.GetFilesInCommit(firstCommit).ToList();

            // Assert
            files.ShouldNotBeNull();
            files.Count.ShouldBeGreaterThan(0);
            files.ShouldContain("file1.txt");
        }

        [Fact]
        public void GetFilesInCommit_ShouldReturnChangedFiles_ForNonInitialCommit()
        {
            // Arrange
            using var reader = new GitRepositoryReader(_tempRepoPath, FileFilter.CreateTestFilter());
            var commits = reader.GetCommitBatch(string.Empty, 10).ToList();
            var secondCommit = commits[commits.Count - 2]; // Get the second commit

            // Act
            var files = reader.GetFilesInCommit(secondCommit).ToList();

            // Assert
            files.ShouldNotBeNull();
            files.Count.ShouldBeGreaterThan(0);
            files.ShouldContain("file2.txt");
        }

        [Fact]
        public void GetFilesInCommit_ShouldApplyFileFilter()
        {
            // Arrange
            // Create a custom file filter that excludes files with .txt extension
            var customFilter = new FileFilter(
                excludedExtensions: new[] { ".txt" },
                excludedFileNames: new string[0],
                excludeRootFiles: false);

            using var reader = new GitRepositoryReader(_tempRepoPath, customFilter);
            var commits = reader.GetCommitBatch(string.Empty, 10).ToList();

            // Create a file with non-excluded extension
            File.WriteAllText(Path.Combine(_tempRepoPath, "test.cs"), "Test content");
            Commands.Stage(_repository, "test.cs");

            var author = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
            var commit = _repository.Commit("Test commit with mixed files", author, author);

            // Act
            var files = reader.GetFilesInCommit(commit).ToList();

            // Assert
            files.ShouldNotBeNull();
            files.Count.ShouldBe(1); // Only the .cs file should be included
            files.ShouldContain("test.cs");
            files.ShouldNotContain(f => f.EndsWith(".txt")); // No .txt files should be included
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
