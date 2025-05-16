using System.IO;

namespace GitCorrelationGraph.Git
{
    /// <summary>
    /// Handles filtering of files for the correlation graph
    /// </summary>
    public class FileFilter
    {
        private readonly HashSet<string> _excludedExtensions;
        private readonly HashSet<string> _excludedFileNames;
        private readonly bool _excludeRootFiles;

        /// <summary>
        /// Creates a new file filter with the specified exclusion rules
        /// </summary>
        /// <param name="excludedExtensions">File extensions to exclude (e.g., ".csproj", ".sln")</param>
        /// <param name="excludedFileNames">Specific file names to exclude (e.g., "Program.cs", "package.json")</param>
        /// <param name="excludeRootFiles">Whether to exclude files at the repository root</param>
        public FileFilter(
            IEnumerable<string> excludedExtensions,
            IEnumerable<string> excludedFileNames,
            bool excludeRootFiles)
        {
            _excludedExtensions = new HashSet<string>(excludedExtensions, StringComparer.OrdinalIgnoreCase);
            _excludedFileNames = new HashSet<string>(excludedFileNames, StringComparer.OrdinalIgnoreCase);
            _excludeRootFiles = excludeRootFiles;
        }

        /// <summary>
        /// Creates a default file filter with common exclusions
        /// </summary>
        public static FileFilter CreateDefault()
        {
            return new FileFilter(
                excludedExtensions: new[] { ".csproj", ".sln" },
                excludedFileNames: new[] { "Program.cs", "package.json", "tsconfig.json" },
                excludeRootFiles: true);
        }

        /// <summary>
        /// Creates a file filter for testing that doesn't exclude any files
        /// </summary>
        public static FileFilter CreateTestFilter()
        {
            return new FileFilter(
                excludedExtensions: Array.Empty<string>(),
                excludedFileNames: Array.Empty<string>(),
                excludeRootFiles: false);
        }

        /// <summary>
        /// Determines if a file should be excluded based on the filter rules
        /// </summary>
        /// <param name="filePath">The file path relative to the repository root</param>
        /// <returns>True if the file should be excluded, false otherwise</returns>
        public bool ShouldExclude(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return true;

            // Check if file is in the root directory (no directory separators)
            if (_excludeRootFiles && !filePath.Contains('/') && !filePath.Contains('\\'))
                return true;

            // Check if the file has an excluded extension
            string extension = Path.GetExtension(filePath);
            if (!string.IsNullOrEmpty(extension) && _excludedExtensions.Contains(extension))
                return true;

            // Check if the file has an excluded name
            string fileName = Path.GetFileName(filePath);
            if (_excludedFileNames.Contains(fileName))
                return true;

            return false;
        }

        /// <summary>
        /// Filters a collection of file paths, removing those that should be excluded
        /// </summary>
        /// <param name="filePaths">The collection of file paths to filter</param>
        /// <returns>A filtered collection of file paths</returns>
        public IEnumerable<string> FilterFiles(IEnumerable<string> filePaths)
        {
            return filePaths.Where(filePath => !ShouldExclude(filePath));
        }
    }
}
