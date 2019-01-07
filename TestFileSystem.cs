using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;

namespace GitDocs
{
    public class TestFileSystem : IFileSystem
    {
        private string m_path;

        public TestFileSystem(string path)
        {
            m_path = path;
        }

        private string GetFullPath(string path)
        {
            if (path.StartsWith("/", StringComparison.Ordinal))
                path = path.Substring(1);

            path = Path.GetFullPath(Path.Combine(m_path, path));
            if (!path.StartsWith(m_path, StringComparison.OrdinalIgnoreCase))
                return null;
            else
                return path;
        }

        #region IFileSystem Members

        public bool TryGetDirectoryContents(string subpath, out IEnumerable<IFileInfo> contents)
        {
            contents = null;

            var path = GetFullPath(Path.Combine(m_path, subpath));
            var directory = new DirectoryInfo(path);
            if (!directory.Exists)
                return false;

            var result = new List<GitFileInfo>();
            var items = directory.GetDirectories();
            foreach (var item in items)
                result.Add(GitFileInfo.Create(item));

            contents = result;
            return true;
        }

        public bool TryGetFileInfo(string subpath, out IFileInfo fileInfo)
        {
            fileInfo = null;

            var path = GetFullPath(Path.Combine(m_path, subpath));
            var file = new FileInfo(path);
            if (!file.Exists)
                return false;

            fileInfo = GitFileInfo.Create(file);
            return true;
        }

        #endregion
    }
}
