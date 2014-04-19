using System.IO;

namespace Topshelf.FileSystemWatcher
{
    /// <summary>
    /// Factory to create FileSystemEventArgsExt
    /// </summary>
    public static class FileSystemEventFactory
    {
        public static TopshelfFileSystemEventArgs CreateNormalFileSystemEvent(FileSystemEventArgs fileSystemEventArgs)
        {
            return new TopshelfFileSystemEventArgs(fileSystemEventArgs.ChangeType, fileSystemEventArgs.FullPath, fileSystemEventArgs.Name, FileSystemEventType.Normal);
        }

        public static TopshelfFileSystemEventArgs CreateCurrentStateFileSystemEvent(string directory, string name)
        {
            return new TopshelfFileSystemEventArgs(WatcherChangeTypes.All, directory, name, FileSystemEventType.CurrentState);
        }
    }
}
