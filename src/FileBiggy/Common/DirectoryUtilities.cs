using System.IO;

namespace FileBiggy.Common
{
    public static class DirectoryUtilities
    {
        public static string EnsureExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }
}
