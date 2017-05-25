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
            public DirectoryConfiguration()
            {
                FileFilter = "*.*";
                ExcludeDuplicateEventsWindowTime = TimeSpan.FromSeconds(2);
            }
            
            /// <summary>
            /// Watching directory
            /// </summary>
            public string Path { get; set; }
            /// <summary>
            /// Include subdirectories
            /// </summary>
            public bool IncludeSubDirectories { get; set; }
            /// <summary>
            /// Create directory if not exist
            /// </summary>
            public bool CreateDir { get; set; }
            /// <summary>
            /// Filter string used to determine what files are monitored in
            /// Default value: "*.*" (Watches all files)
            /// </summary>
            public string FileFilter { get; set; }
            /// <summary>
            ///  Specifies changes to watch for in a file or folder
            /// </summary>
            public NotifyFilters NotifyFilters { get; set; }
            /// <summary>
            /// Activate the return the initial state of watching directory
            /// </summary>
            public bool GetInitialStateEvent { get; set; }
            /// <summary>
            /// The size (in bytes) of the FileSystemWatcher internal buffer.
            /// </summary>
            public int InternalBufferSize { get; set; }
            /// <summary>
            /// Activate filters to exclude duplicate events
            /// </summary>
            public bool ExcludeDuplicateEvents { get; set; }
            /// <summary>
            /// Window time for excluding duplicate events
            /// Default value: 2 seconds
            /// </summary>
            public TimeSpan ExcludeDuplicateEventsWindowTime { get; set; }

        }
    }
}