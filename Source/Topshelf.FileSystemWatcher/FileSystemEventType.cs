namespace Topshelf.FileSystemWatcher
{
    /// <summary>
    /// Determines which type of Event is thrown. E.g. CurrentState
    /// </summary>
    public enum FileSystemEventType
    {
        /// <summary>
        /// A normal FileSystemChange event
        /// </summary>
        Normal,

        /// <summary>
        /// A renamed FileSystemChange event
        /// </summary>
        Renamed,

        /// <summary>
        /// A CurrentState event (e.g. if service was restarted)
        /// </summary>
        CurrentState,
    }
}
