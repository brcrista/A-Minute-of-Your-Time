using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FetchPullRequestData
{
    static class FileHelper
    {
        /// <summary>
        /// Asynchronously writes a new file, creating all intermediate paths as necessary.
        /// If the file already exists, it is overwritten.
        /// </summary>
        public static async Task WriteFileAsync(string path, string contents)
        {
            Directory.CreateDirectory(
                path: Path.GetDirectoryName(path));

            var bytes = new UTF8Encoding().GetBytes(contents);
            using (var fileStream = File.Open(path, FileMode.OpenOrCreate))
            {
                await fileStream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
    }
}