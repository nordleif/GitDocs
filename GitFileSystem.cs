using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;
using LibGit2Sharp;

namespace GitDocs
{
    public class GitFileSystem : IFileSystem
    {
        private string m_branchName;
        private string m_directoryName;

        public GitFileSystem(string directoryName, string branchName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
                throw new ArgumentNullException(nameof(directoryName));

            if (string.IsNullOrWhiteSpace(branchName))
                throw new ArgumentNullException(nameof(branchName));

            m_directoryName = directoryName;
            m_branchName = branchName;
        }

        private string GetFullPath(string path)
        {
            if (path.StartsWith("/", StringComparison.Ordinal))
                path = path.Substring(1);

            path = Path.GetFullPath(Path.Combine(m_directoryName, path));
            if (!path.StartsWith(m_directoryName, StringComparison.OrdinalIgnoreCase))
                return null;
            else
                return path;
        }

        private IEnumerable<GitFileInfo> ReadGitTree(Tree tree, DateTime lastModified)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));

            foreach (var entry in tree)
            {
                if (entry.Target is Tree subTree)
                {
                    yield return GitFileInfo.Create(entry, lastModified);
                    var items = ReadGitTree(subTree, lastModified);
                    foreach (var item in items)
                        yield return item;
                }
                else if (entry.Target is Blob blob)
                {
                    yield return GitFileInfo.Create(entry, lastModified);
                }
            }
        }

        #region IFileSystem Members

        public bool TryGetDirectoryContents(string subpath, out IEnumerable<IFileInfo> contents)
        {
            contents = null;

            var segments = new Uri(new Uri("http://foo.bar"), subpath).Segments;
            var organizationName = segments.Length > 1 ? segments[1] : string.Empty;
            var repositoryName = segments.Length > 2 ? segments[2].TrimEnd('/') : string.Empty;
            subpath = string.Join("", segments.Skip(3));

            if (string.IsNullOrWhiteSpace(repositoryName))
            {
                var path = GetFullPath(Path.Combine(m_directoryName, organizationName));
                var directory = new DirectoryInfo(path);
                if (!directory.Exists)
                    return false;

                var items = new List<IFileInfo>();
                var directories = directory.GetDirectories();
                foreach (var item in directories)
                    items.Add(GitFileInfo.Create(item));

                //var files = directory.GetFiles();
                //foreach (var item in files)
                //    items.Add(GitFileInfo.Create(item));

                contents = items;
                return true;
            }
            else
            {
                var path = GetFullPath(Path.Combine(m_directoryName, organizationName, $"{repositoryName}.git"));
                var directory = new DirectoryInfo(path);
                if (!directory.Exists)
                    return false;

                using (var repository = new Repository(path, new RepositoryOptions { }))
                {
                    repository.Config.Set("core.autocrlf", false);
                    var branch = repository.Branches[m_branchName];
                    if (branch == null)
                        branch = repository.Branches["master"];
                    if (branch == null)
                        return false;

                    var lastCommit = branch.Tip;
                    var tree = lastCommit.Tree;
                    if (!string.IsNullOrWhiteSpace(subpath))
                    {
                        var entry = lastCommit.Tree[subpath];
                        if (entry == null)
                            return false;

                        tree = entry.Target as Tree;
                        if (tree == null)
                            return false;
                    }
                    
                    var lastModified = lastCommit.Committer.When.DateTime;
                    var items = ReadGitTree(tree, lastModified).Where(i => i.GitEntryPath.Equals(Path.Combine(subpath, i.Name))).ToArray();
                    if (!items.Any())
                        return false;
                    
                    contents = items;
                }

                return true;
            }
        }

        public bool TryGetFileInfo(string subpath, out IFileInfo fileInfo)
        {
            fileInfo = null;

            var segments = new Uri(new Uri("http://foo.bar"), subpath).Segments;
            var organizationName = segments.Length > 1 ? segments[1] : string.Empty;
            var repositoryName = segments.Length > 2 ? segments[2].TrimEnd('/') : string.Empty;
            subpath = string.Join("", segments.Skip(3));

            if (string.IsNullOrWhiteSpace(repositoryName) || string.IsNullOrWhiteSpace(subpath))
                return false;
            
            var path = GetFullPath(Path.Combine(m_directoryName, organizationName, $"{repositoryName}.git"));
            var directory = new DirectoryInfo(path);
            if (!directory.Exists)
                return false;

            using (var repository = new Repository(path, new RepositoryOptions { }))
            {
                repository.Config.Set("core.autocrlf", false);
                var branch = repository.Branches[m_branchName];
                if (branch == null)
                    branch = repository.Branches["master"];
                if (branch == null)
                    return false;

                var lastCommit = branch.Tip;
                var tree = lastCommit.Tree;
                var lastModified = lastCommit.Committer.When.DateTime;

                var entry = tree[subpath];
                if (entry == null)
                    return false;

                var blob = entry.Target as Blob;
                if (blob == null)
                    return false;

                byte[] buffer;
                using (var stream = blob.GetContentStream())
                using (var reader = new BinaryReader(stream))
                    buffer = reader.ReadBytes((int)stream.Length);

                fileInfo = GitFileInfo.Create(entry, lastModified, buffer);

                return true;
            }
        }

        #endregion
    }
}
