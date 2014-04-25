using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Topshelf.Logging;
using Topshelf.ServiceConfigurators;

namespace Topshelf.FileSystemWatcher
{
    public static class ServiceConfiguratorExtensions
    {
        private const string _defaultFileFilter = "*.*";

        private static readonly ICollection<System.IO.FileSystemWatcher> _watchers =
            new Collection<System.IO.FileSystemWatcher>();

        public static ServiceConfigurator<T> WhenFileSystemChanged<T>(this ServiceConfigurator<T> configurator,
            Action<FileSystemWatcherConfigurator> fileSystemWatcherConfigurator,
            Action<TopshelfFileSystemEventArgs> fileSystemChanged)
            where T : class
        {
            return WhenFileSystemChanged(configurator, fileSystemWatcherConfigurator, fileSystemChanged,
                null, null, fileSystemChanged);
        }

        public static ServiceConfigurator<T> WhenFileSystemCreated<T>(this ServiceConfigurator<T> configurator,
            Action<FileSystemWatcherConfigurator> fileSystemWatcherConfigurator,
            Action<TopshelfFileSystemEventArgs> fileSystemCreated)
            where T : class
        {
            return WhenFileSystemChanged(configurator, fileSystemWatcherConfigurator, null,
                fileSystemCreated, null, fileSystemCreated);
        }

        public static ServiceConfigurator<T> WhenFileSystemDeleted<T>(this ServiceConfigurator<T> configurator,
            Action<FileSystemWatcherConfigurator> fileSystemWatcherConfigurator,
            Action<TopshelfFileSystemEventArgs> fileSystemDeleted)
            where T : class
        {
            return WhenFileSystemChanged(configurator, fileSystemWatcherConfigurator, null,
                null, fileSystemDeleted, fileSystemDeleted);
        }

        public static ServiceConfigurator<T> WhenFileSystemChanged<T>(this ServiceConfigurator<T> configurator,
            Action<FileSystemWatcherConfigurator> fileSystemWatcherConfigurator,
            Action<TopshelfFileSystemEventArgs> fileSystemChanged,
            Action<TopshelfFileSystemEventArgs> fileSystemCreated,
            Action<TopshelfFileSystemEventArgs> fileSystemDeleted,
            Action<TopshelfFileSystemEventArgs> fileSystemInitialState)
            where T : class
        {
            var log = HostLogger.Get(typeof(ServiceConfiguratorExtensions));

            var fileSystemWatcherConfig = new FileSystemWatcherConfigurator();
            fileSystemWatcherConfigurator(fileSystemWatcherConfig);

            var configs = new Collection<FileSystemWatcherConfigurator.DirectoryConfiguration>();
            foreach (Action<FileSystemWatcherConfigurator.DirectoryConfiguration> action in
                fileSystemWatcherConfig.DirectoryConfigurationAction)
            {
                var config = new FileSystemWatcherConfigurator.DirectoryConfiguration();
                action(config);
                configs.Add(config);
            }

            FileSystemEventHandler watcherOnChanged = CreateEventHandler(fileSystemChanged);
            FileSystemEventHandler watcherOnCreated = CreateEventHandler(fileSystemCreated);
            FileSystemEventHandler watcherOnDeleted = CreateEventHandler(fileSystemDeleted);

            if (configs.Any())
            {
                BeforeStartingService(configurator, configs, log, watcherOnChanged, watcherOnCreated, watcherOnDeleted);
                AfterStartingService(configurator, configs, log, fileSystemInitialState);
                BeforeStoppingService(configurator, log, watcherOnChanged);
            }

            return configurator;
        }

        private static FileSystemEventHandler CreateEventHandler(Action<TopshelfFileSystemEventArgs> fileSystemAction)
        {
            FileSystemEventHandler eventHandler = null;

            if (fileSystemAction != null)
            {
                eventHandler =
                    (sender, args) => fileSystemAction(FileSystemEventFactory.CreateNormalFileSystemEvent(args));
            }

            return eventHandler;
        }

        private static void BeforeStartingService<T>(ServiceConfigurator<T> configurator,
            IEnumerable<FileSystemWatcherConfigurator.DirectoryConfiguration> configs, LogWriter log,
            FileSystemEventHandler watcherOnChanged,
            FileSystemEventHandler watcherOnCreated,
            FileSystemEventHandler watcherOnDeleted) where T : class
        {
            configurator.BeforeStartingService(() =>
            {
                foreach (FileSystemWatcherConfigurator.DirectoryConfiguration config in configs)
                {
                    if (!Directory.Exists(config.Path))
                    {
                        if (config.CreateDir)
                        {
                            log.Debug(
                                string.Format("[Topshelf.FileSystemWatcher] Path ({0}) does not exist. Creating...",
                                    config.Path));
                            Directory.CreateDirectory(config.Path);
                        }
                        else
                        {
                            throw new DirectoryNotFoundException(config.Path +
                                                                 " does not exist. Please call CreateDir in the FileSystemWatcherConfigurator, or make sure the dirs exist in the FileSystem");
                        }
                    }

                    var fileSystemWatcher = CreateFileSystemWatcher(config.Path, config.NotifyFilters,
                        config.FileFilter, config.IncludeSubDirectories, config.InternalBufferSize);

                    if (watcherOnChanged != null)
                        fileSystemWatcher.Changed += watcherOnChanged;
                    if (watcherOnCreated != null)
                        fileSystemWatcher.Created += watcherOnCreated;
                    if (watcherOnDeleted != null)
                        fileSystemWatcher.Deleted += watcherOnDeleted;

                    _watchers.Add(fileSystemWatcher);

                    log.Info(string.Format("[Topshelf.FileSystemWatcher] configured to listen for events in {0}",
                        config.Path));

                    foreach (System.IO.FileSystemWatcher watcher in _watchers)
                    {
                        watcher.EnableRaisingEvents = true;
                    }

                    log.Info(string.Format("[Topshelf.FileSystemWatcher] listening for events"));
                }
            });
        }

        private static void AfterStartingService<T>(ServiceConfigurator<T> configurator,
            IEnumerable<FileSystemWatcherConfigurator.DirectoryConfiguration> configs, LogWriter log,
            Action<TopshelfFileSystemEventArgs> fileSystemChanged) where T : class
        {
            configurator.AfterStartingService(() =>
            {
                foreach (FileSystemWatcherConfigurator.DirectoryConfiguration config in configs)
                {
                    if (config.GetInitialStateEvent)
                    {
                        log.Info(string.Format("[Topshelf.FileSystemWatcher] Checking for InitialState Events"));

                        string[] paths;
                        if (config.FileFilter != null && !string.IsNullOrWhiteSpace(config.FileFilter))
                        {
                            paths = Directory.GetFiles(config.Path, config.FileFilter);
                        }
                        else
                        {
                            paths = Directory.GetFiles(config.Path);
                        }

                        if (paths.Any())
                        {
                            foreach (string path in paths)
                            {
                                fileSystemChanged(FileSystemEventFactory.CreateCurrentStateFileSystemEvent(path, Path.GetFileName(path)));
                            }
                        }
                    }
                }
            });
        }

        private static void BeforeStoppingService<T>(ServiceConfigurator<T> configurator, LogWriter log,
            FileSystemEventHandler watcherOnChanged) where T : class
        {
            configurator.BeforeStoppingService(() =>
            {
                if (_watchers != null && _watchers.Any())
                {
                    foreach (System.IO.FileSystemWatcher fileSystemWatcher in _watchers)
                    {
                        fileSystemWatcher.EnableRaisingEvents = false;
                        fileSystemWatcher.Changed -= watcherOnChanged;
                        fileSystemWatcher.Created -= watcherOnChanged;
                        fileSystemWatcher.Deleted -= watcherOnChanged;
                        fileSystemWatcher.Dispose();
                        log.Info(string.Format("[Topshelf.FileSystemWatcher] Unsubscribed for FileSystemChange events"));
                    }
                }
            });
        }

        private static System.IO.FileSystemWatcher CreateFileSystemWatcher(string path,
            NotifyFilters notifyFilters, string fileFilter, bool includeSubDirectories, int internalBufferSize)
        {
            System.IO.FileSystemWatcher watcher = new System.IO.FileSystemWatcher
            {
                Path = path,
                EnableRaisingEvents = true,
                IncludeSubdirectories = includeSubDirectories,
                NotifyFilter = notifyFilters,
            };

            if (fileFilter != null && !string.IsNullOrWhiteSpace(fileFilter))
            {
                watcher.Filter = fileFilter;
            }
            else
            {
                watcher.Filter = _defaultFileFilter;
            }
            if (internalBufferSize > 0)
            {
                watcher.InternalBufferSize = internalBufferSize;
            }

            return watcher;
        }
    }
}
