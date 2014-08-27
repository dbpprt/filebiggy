using System.IO;

namespace FileBiggy.Common
{
    public static class FileUtilities
    {
        public static string EnsureExists(string path)
        {
            if (!File.Exists(path))
            {
                using (File.Create(path))
                {
                }
            }

            return path;
        }
    }
}