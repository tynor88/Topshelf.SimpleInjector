using System;
using System.IO;

namespace Topshelf.FileSystemWatcher.Sample
{
    internal class Program
    {
        private static readonly string _testDir = Directory.GetCurrentDirectory() + @"\test\";
        private static readonly bool _includeSubDirectories = true;
        private static readonly bool _excludeDuplicateEvents = true;

        private static void Main(string[] args)
        {
            HostFactory.Run(config =>
            {
                config.Service<Program>(s =>
                {
                    s.ConstructUsing(() => new Program());
                    s.BeforeStartingService((hostStart) =>
                    {
                        if (!Directory.Exists(_testDir))
                            Directory.CreateDirectory(_testDir);
                    });
                    s.WhenStarted((service, host) => true);
                    s.WhenStopped((service, host) => true);
                    s.WhenFileSystemCreated(ConfigureDirectoryWorkCreated, FileSystemCreated);
                    s.WhenFileSystemChanged(ConfigureDirectoryWorkChanged, FileSystemCreated);
                    s.WhenFileSystemRenamed(ConfigureDirectoryWorkRenamedFile, FileSystemRenamedFile);
                    s.WhenFileSystemRenamed(ConfigureDirectoryWorkRenamedDirectory, FileSystemRenamedDirectory);
                    s.WhenFileSystemDeleted(ConfigureDirectoryWorkDeleted, FileSystemCreated);
                });
            });
            Console.ReadKey();
        }

        private static void ConfigureDirectoryWorkCreated(FileSystemWatcherConfigurator fswConfig)
        {
            fswConfig.AddDirectory(dir =>
            {
                dir.Path = _testDir;
                dir.IncludeSubDirectories = _includeSubDirectories;
                dir.NotifyFilters = NotifyFilters.DirectoryName | NotifyFilters.FileName;
                dir.ExcludeDuplicateEvents = _excludeDuplicateEvents;
            });
        }
        private static void ConfigureDirectoryWorkChanged(FileSystemWatcherConfigurator fswConfig)
        {
            fswConfig.AddDirectory(dir =>
            {
                dir.Path = _testDir;
                dir.IncludeSubDirectories = _includeSubDirectories;
                dir.NotifyFilters = NotifyFilters.LastWrite;
                dir.ExcludeDuplicateEvents = _excludeDuplicateEvents;
            });
        }
        private static void ConfigureDirectoryWorkRenamedFile(FileSystemWatcherConfigurator fswConfig)
        {
            fswConfig.AddDirectory(dir =>
            {
                dir.Path = _testDir;
                dir.IncludeSubDirectories = _includeSubDirectories;
                dir.NotifyFilters = NotifyFilters.FileName;
                dir.ExcludeDuplicateEvents = _excludeDuplicateEvents;
            });
        }
        private static void ConfigureDirectoryWorkRenamedDirectory(FileSystemWatcherConfigurator fswConfig)
        {
            fswConfig.AddDirectory(dir =>
            {
                dir.Path = _testDir;
                dir.IncludeSubDirectories = _includeSubDirectories;
                dir.NotifyFilters = NotifyFilters.DirectoryName;
                dir.ExcludeDuplicateEvents = _excludeDuplicateEvents;
            });
        }
        private static void ConfigureDirectoryWorkDeleted(FileSystemWatcherConfigurator fswConfig)
        {
            fswConfig.AddDirectory(dir =>
            {
                dir.Path = _testDir;
                dir.IncludeSubDirectories = _includeSubDirectories;
                dir.NotifyFilters = NotifyFilters.DirectoryName | NotifyFilters.FileName;
                dir.ExcludeDuplicateEvents = _excludeDuplicateEvents;
            });
        }

        private static void FileSystemCreated(TopshelfFileSystemEventArgs topshelfFileSystemEventArgs)
        {
            Console.WriteLine("*********************");
            Console.WriteLine("ChangeType = {0}", topshelfFileSystemEventArgs.ChangeType);
            Console.WriteLine("FullPath = {0}", topshelfFileSystemEventArgs.FullPath);
            Console.WriteLine("Name = {0}", topshelfFileSystemEventArgs.Name);
            Console.WriteLine("FileSystemEventType {0}", topshelfFileSystemEventArgs.FileSystemEventType);
            Console.WriteLine("*********************");
        }

        private static void FileSystemRenamedFile(TopshelfFileSystemEventArgs topshelfFileSystemEventArgs)
        {
            Console.WriteLine("*********************");
            Console.WriteLine("Rename File");
            Console.WriteLine("ChangeType = {0}", topshelfFileSystemEventArgs.ChangeType);
            Console.WriteLine("FullPath = {0}", topshelfFileSystemEventArgs.FullPath);
            Console.WriteLine("Name = {0}", topshelfFileSystemEventArgs.Name);
            Console.WriteLine("FileSystemEventType {0}", topshelfFileSystemEventArgs.FileSystemEventType);
            Console.WriteLine("*********************");
        }

        private static void FileSystemRenamedDirectory(TopshelfFileSystemEventArgs topshelfFileSystemEventArgs)
        {
            Console.WriteLine("*********************");
            Console.WriteLine("Rename Dir");
            Console.WriteLine("ChangeType = {0}", topshelfFileSystemEventArgs.ChangeType);
            Console.WriteLine("FullPath = {0}", topshelfFileSystemEventArgs.FullPath);
            Console.WriteLine("Name = {0}", topshelfFileSystemEventArgs.Name);
            Console.WriteLine("FileSystemEventType {0}", topshelfFileSystemEventArgs.FileSystemEventType);
            Console.WriteLine("*********************");
        }
    }
}