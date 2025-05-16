using Shouldly;
using GitCorrelationGraph.Git;

namespace GitCorrelationGraph.Tests.Git
{
    public class FileFilterTests
    {
        [Fact]
        public void ShouldExclude_ShouldReturnTrue_ForRootFiles_WhenExcludeRootFilesIsTrue()
        {
            // Arrange
            var filter = new FileFilter(
                excludedExtensions: new string[0],
                excludedFileNames: new string[0],
                excludeRootFiles: true);

            // Act & Assert
            filter.ShouldExclude("README.md").ShouldBeTrue();
            filter.ShouldExclude("LICENSE").ShouldBeTrue();
            filter.ShouldExclude("file.txt").ShouldBeTrue();
        }

        [Fact]
        public void ShouldExclude_ShouldReturnFalse_ForFilesInSubfolders_WhenExcludeRootFilesIsTrue()
        {
            // Arrange
            var filter = new FileFilter(
                excludedExtensions: new string[0],
                excludedFileNames: new string[0],
                excludeRootFiles: true);

            // Act & Assert
            filter.ShouldExclude("src/file.txt").ShouldBeFalse();
            filter.ShouldExclude("folder/README.md").ShouldBeFalse();
            filter.ShouldExclude("dir1/dir2/file.cs").ShouldBeFalse();
        }

        [Fact]
        public void ShouldExclude_ShouldReturnTrue_ForExcludedExtensions()
        {
            // Arrange
            var filter = new FileFilter(
                excludedExtensions: new[] { ".csproj", ".sln" },
                excludedFileNames: new string[0],
                excludeRootFiles: false);

            // Act & Assert
            filter.ShouldExclude("project.csproj").ShouldBeTrue();
            filter.ShouldExclude("solution.sln").ShouldBeTrue();
            filter.ShouldExclude("src/project.csproj").ShouldBeTrue();
            filter.ShouldExclude("src/solution.sln").ShouldBeTrue();
        }

        [Fact]
        public void ShouldExclude_ShouldReturnFalse_ForNonExcludedExtensions()
        {
            // Arrange
            var filter = new FileFilter(
                excludedExtensions: new[] { ".csproj", ".sln" },
                excludedFileNames: new string[0],
                excludeRootFiles: false);

            // Act & Assert
            filter.ShouldExclude("file.cs").ShouldBeFalse();
            filter.ShouldExclude("file.txt").ShouldBeFalse();
            filter.ShouldExclude("src/file.cs").ShouldBeFalse();
        }

        [Fact]
        public void ShouldExclude_ShouldReturnTrue_ForExcludedFileNames()
        {
            // Arrange
            var filter = new FileFilter(
                excludedExtensions: new string[0],
                excludedFileNames: new[] { "Program.cs", "package.json", "tsconfig.json" },
                excludeRootFiles: false);

            // Act & Assert
            filter.ShouldExclude("Program.cs").ShouldBeTrue();
            filter.ShouldExclude("package.json").ShouldBeTrue();
            filter.ShouldExclude("tsconfig.json").ShouldBeTrue();
            filter.ShouldExclude("src/Program.cs").ShouldBeTrue();
            filter.ShouldExclude("src/package.json").ShouldBeTrue();
        }

        [Fact]
        public void ShouldExclude_ShouldReturnFalse_ForNonExcludedFileNames()
        {
            // Arrange
            var filter = new FileFilter(
                excludedExtensions: new string[0],
                excludedFileNames: new[] { "Program.cs", "package.json", "tsconfig.json" },
                excludeRootFiles: false);

            // Act & Assert
            filter.ShouldExclude("Main.cs").ShouldBeFalse();
            filter.ShouldExclude("config.json").ShouldBeFalse();
            filter.ShouldExclude("src/Main.cs").ShouldBeFalse();
        }

        [Fact]
        public void FilterFiles_ShouldRemoveExcludedFiles()
        {
            // Arrange
            var filter = FileFilter.CreateDefault(); // Uses all exclusion rules
            var files = new[]
            {
                "README.md", // Root file
                "LICENSE", // Root file
                "Program.cs", // Excluded filename
                "package.json", // Excluded filename
                "tsconfig.json", // Excluded filename
                "project.csproj", // Excluded extension
                "solution.sln", // Excluded extension
                "src/file.cs", // Not excluded
                "src/Program.cs", // Excluded filename
                "src/project.csproj", // Excluded extension
                "src/file.txt" // Not excluded
            };

            // Act
            var filteredFiles = filter.FilterFiles(files).ToList();

            // Assert
            filteredFiles.Count.ShouldBe(2);
            filteredFiles.ShouldContain("src/file.cs");
            filteredFiles.ShouldContain("src/file.txt");
        }
    }
}
