using System.IO;

namespace Topshelf.FileSystemWatcher
{
    public class TopshelfFileSystemEventArgs : RenamedEventArgs
    {
        /// <summary>
        /// Determine whether this event was raised upon initial state check
        /// (e.g. if Service was restarted or machine was shutdown)
        /// </summary>
        public FileSystemEventType FileSystemEventType { get; private set; }

        /// <summary>
        /// Instantiate a new FileSystemEventArgsExt
        /// </summary>
        /// <param name="changeType"></param>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <param name="oldName"></param>
        /// <param name="fileSystemEventType"></param>
        public TopshelfFileSystemEventArgs(WatcherChangeTypes changeType, string directory, string name, string oldName, FileSystemEventType fileSystemEventType)
            : base(changeType, directory, name, oldName)
        {
            FileSystemEventType = fileSystemEventType;
        }
    }
}
