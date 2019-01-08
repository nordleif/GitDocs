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
    public class GitFileInfo : IFileInfo
    {
        #region Static Members

        public static GitFileInfo Create(DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            var fileInfo = new GitFileInfo();
            fileInfo.IsDirectory = true;
            fileInfo.LastModified = directory.LastWriteTime;
            fileInfo.Length = 0;
            fileInfo.Name = directory.Name.Replace(".git", "");
            fileInfo.PhysicalPath = null;

            return fileInfo;
        }

        public static GitFileInfo Create(FileInfo file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var fileInfo = new GitFileInfo();
            fileInfo.IsDirectory = false;
            fileInfo.LastModified = file.LastWriteTime;
            fileInfo.Length = fileInfo.m_buffer.Length;
            fileInfo.Name = file.Name;
            fileInfo.PhysicalPath = "";
            fileInfo.m_buffer = File.ReadAllBytes(file.FullName);

            return fileInfo;
        }

        public static GitFileInfo Create(TreeEntry entry, DateTime lastModified, byte[] buffer = null)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            
            var fileInfo = new GitFileInfo();
            fileInfo.GitEntryPath = entry.Path;
            fileInfo.IsDirectory = true;
            fileInfo.LastModified = lastModified;
            fileInfo.Length = 0;
            fileInfo.Name = entry.Name;
            fileInfo.PhysicalPath = "";
            
            if (entry.Target is Blob blob)
            {
                fileInfo.IsDirectory = false;
                fileInfo.Length = blob.Size;
            }
        
            if (buffer != null)
            {
                fileInfo.m_buffer = buffer;
                fileInfo.Length = buffer.Length;
            }

            //if (fileInfo.Name == "index.html")
            //    System.Diagnostics.Debugger.Break();
            
            return fileInfo;
        }

        #endregion

        private byte[] m_buffer;

        public GitFileInfo()
        {

        }

        public string GitEntryPath { get; set; }

        public override string ToString()
        {
            return Name;
        }

        #region IFileInfo Members

        public long Length { get; set; }

        public string PhysicalPath { get; set; }

        public string Name { get; set; }

        public DateTime LastModified { get; set; }

        public bool IsDirectory { get; set; }

        public Stream CreateReadStream()
        {
            if (m_buffer != null)
                return new MemoryStream(m_buffer);
            else
                return null;
        }

        #endregion
    }
}

