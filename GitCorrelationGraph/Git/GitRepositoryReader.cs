using LibGit2Sharp;

namespace GitCorrelationGraph.Git
{
    /// <summary>
    /// Provides access to git repository data
    /// </summary>
    public class GitRepositoryReader : IDisposable
    {
        private readonly Repository _repository;
        private readonly string _repositoryPath;
        private readonly FileFilter _fileFilter;

        /// <summary>
        /// Creates a new GitRepositoryReader with the default file filter
        /// </summary>
        /// <param name="repositoryPath">Path to the git repository</param>
        public GitRepositoryReader(string repositoryPath)
            : this(repositoryPath, FileFilter.CreateDefault())
        {
        }

        /// <summary>
        /// Creates a new GitRepositoryReader with a custom file filter
        /// </summary>
        /// <param name="repositoryPath">Path to the git repository</param>
        /// <param name="fileFilter">File filter to use</param>
        public GitRepositoryReader(string repositoryPath, FileFilter fileFilter)
        {
            _repositoryPath = repositoryPath;
            _repository = new Repository(repositoryPath);
            _fileFilter = fileFilter;
        }

        /// <summary>
        /// Get a batch of commits from the repository
        /// </summary>
        /// <param name="startCommitHash">The commit hash to start from (exclusive), or empty for the first commit</param>
        /// <param name="batchSize">The maximum number of commits to retrieve</param>
        public IEnumerable<Commit> GetCommitBatch(string startCommitHash, int batchSize)
        {
            var filter = new CommitFilter();

            // If we have a starting commit, filter from that point
            if (!string.IsNullOrEmpty(startCommitHash))
            {
                var commit = _repository.Lookup<Commit>(startCommitHash);
                if (commit != null)
                {
                    filter.ExcludeReachableFrom = commit;
                }
            }

            return _repository.Commits
                .QueryBy(filter)
                .Take(batchSize);
        }

        /// <summary>
        /// Get the files changed in a commit
        /// </summary>
        public IEnumerable<string> GetFilesInCommit(Commit commit)
        {
            IEnumerable<string> files;

            if (commit.Parents.Any())
            {
                var parent = commit.Parents.First();
                var changes = _repository.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);

                files = changes
                    .Select(change => change.Path)
                    .ToList();
            }
            else
            {
                // For the initial commit, get all files
                files = commit.Tree
                    .Select(entry => entry.Path)
                    .ToList();
            }

            // Apply file filtering
            return _fileFilter.FilterFiles(files);
        }

        public void Dispose()
        {
            _repository.Dispose();
        }
    }
}
