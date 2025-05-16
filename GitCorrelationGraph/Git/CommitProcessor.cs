using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using GitCorrelationGraph.Models;

namespace GitCorrelationGraph.Git
{
    /// <summary>
    /// Processes commits and updates the correlation graph
    /// </summary>
    public class CommitProcessor
    {
        private readonly GitRepositoryReader _repositoryReader;

        public CommitProcessor(GitRepositoryReader repositoryReader)
        {
            _repositoryReader = repositoryReader;
        }

        /// <summary>
        /// Process a single commit and update the graph
        /// </summary>
        public void ProcessCommit(Commit commit, CorrelationGraph graph)
        {
            // Get files in the commit, excluding deleted files
            var files = _repositoryReader.GetFilesInCommit(commit, true).ToList();

            // Process deleted files if this is not the initial commit
            if (commit.Parents.Any())
            {
                var parent = commit.Parents.First();
                var changes = _repositoryReader.Repository.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);

                // Find deleted files and remove them from the graph
                foreach (var change in changes)
                {
                    if (change.Status == ChangeKind.Deleted)
                    {
                        graph.RemoveNode(change.Path);
                    }
                }
            }

            // Update commit count for each file
            foreach (var file in files)
            {
                var node = graph.GetOrCreateNode(file);
                node.CommitCount++;
            }

            // Update co-commit count for each pair of files
            for (int i = 0; i < files.Count; i++)
            {
                for (int j = i + 1; j < files.Count; j++)
                {
                    var edge = graph.GetOrCreateEdge(files[i], files[j]);
                    edge.CoCommitCount++;
                }
            }

            // Update processing state
            graph.ProcessingState.LastProcessedCommitHash = commit.Sha;
            graph.ProcessingState.LastProcessedCommitDate = commit.Author.When.DateTime;
            graph.ProcessingState.TotalCommitsProcessed++;
        }

        /// <summary>
        /// Process a batch of commits and update the graph
        /// </summary>
        public int ProcessCommitBatch(
            string startCommitHash,
            int batchSize,
            CorrelationGraph graph)
        {
            var commits = _repositoryReader.GetCommitBatch(startCommitHash, batchSize).ToList();

            foreach (var commit in commits)
            {
                ProcessCommit(commit, graph);
            }

            return commits.Count;
        }
    }
}
