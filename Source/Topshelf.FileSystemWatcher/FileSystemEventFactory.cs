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
            return new TopshelfFileSystemEventArgs(fileSystemEventArgs.ChangeType, Path.GetDirectoryName(fileSystemEventArgs.FullPath), fileSystemEventArgs.Name, null, FileSystemEventType.Normal);
        }

        public static TopshelfFileSystemEventArgs CreateRenamedFileSystemEvent(RenamedEventArgs fileSystemEventArgs)
        {
            return new TopshelfFileSystemEventArgs(fileSystemEventArgs.ChangeType, Path.GetDirectoryName(fileSystemEventArgs.FullPath), fileSystemEventArgs.Name, fileSystemEventArgs.OldName, FileSystemEventType.Renamed);
        }

        public static TopshelfFileSystemEventArgs CreateCurrentStateFileSystemEvent(string directory, string name)
        {
            return new TopshelfFileSystemEventArgs(WatcherChangeTypes.All, directory, name, null, FileSystemEventType.CurrentState);
        }
    }
}
