using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Topshelf.FileSystemWatcher
{
    public class FileSystemWatcherConfigurator
    {
        private ICollection<Action<DirectoryConfiguration>> _configurations;
        public ICollection<Action<DirectoryConfiguration>> DirectoryConfigurationAction => _configurations ?? (_configurations = new Collection<Action<DirectoryConfiguration>>());

        public FileSystemWatcherConfigurator AddDirectory(Action<DirectoryConfiguration> directoryConfiguration)
        {
            DirectoryConfigurationAction.Add(directoryConfiguration);
            return this;
        }

        public class DirectoryConfiguration
        {
            public string Path { get; set; }
            public bool IncludeSubDirectories { get; set; }
            public bool CreateDir { get; set; }
            public string FileFilter { get; set; }
            public NotifyFilters NotifyFilters { get; set; }
            public bool GetInitialStateEvent { get; set; }
            public int InternalBufferSize { get; set; }
        }
    }
}