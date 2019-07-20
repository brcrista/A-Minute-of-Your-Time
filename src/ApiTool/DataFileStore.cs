using System.IO;

namespace ApiTool
{
    /// <summary>
    /// Persists content received from an external service.
    /// </summary>
    class DataFileStore
    {
        private readonly string outputDirectory;

        public DataFileStore(string outputDirectory)
        {
            // Ensure the output directory exists
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            this.outputDirectory = outputDirectory;
        }

        /// <summary>
        /// Writes content to the store.
        /// </summary>
        /// <param name="filename">Relative path to the file from the output directory.</param>
        /// <param name="content">Content to write to the file.</param>
        public void WriteFile(string filename, string content)
        {
            var targetFile = Path.Combine(outputDirectory, filename);

            Directory.CreateDirectory(
                path: Path.GetDirectoryName(targetFile));

            File.WriteAllText(
                path: targetFile,
                contents: content);
        }

        /// <summary>
        /// Indicates whether a file with the given name exists in the store.
        /// </summary>
        public bool Contains(string filename)
        {
            return File.Exists(
                path: Path.Combine(outputDirectory, filename));
        }
    }
}