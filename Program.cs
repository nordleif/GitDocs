using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;

namespace GitDocs
{
    class Program
    {
        static void Main(string[] args)
        {
            var branchName = ConfigurationManager.AppSettings["BranchName"];
            var directoryName = ConfigurationManager.AppSettings["DirectoryName"];
            var httpUrl = ConfigurationManager.AppSettings["HttpUrl"];
            
            var fileServerOptions = new FileServerOptions();
            fileServerOptions.EnableDirectoryBrowsing = true;
            fileServerOptions.EnableDirectoryBrowsing = true;
            fileServerOptions.FileSystem = new PhysicalFileSystem(@"D:\Source\");
            fileServerOptions.FileSystem = new TestFileSystem(@"D:\Source\");
            fileServerOptions.FileSystem = new GitFileSystem(directoryName, branchName);
            
            WebApp.Start(new StartOptions(httpUrl), (application) => {
                application.Use<ConsoleMiddleware>();
                application.UseFileServer(fileServerOptions);
            });

            Console.ReadLine();
        }
    }
}
