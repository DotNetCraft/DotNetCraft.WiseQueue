namespace DotNetCraft.WiseQueue.Core.Entities.Enums
{
    /// <summary>
    /// <c>List</c> of available task's state.
    /// </summary>
    public enum TaskStates
    {
        /// <summary>
        /// Shows that this is a new task.
        /// </summary>
        New = 1,
        /// <summary>
        /// Shows that this task has been executed in one of the servers.
        /// </summary>
        Running = 2,
        /// <summary>
        /// Shows that task has been successfully completed.
        /// </summary>
        Successed = 4,
        /// <summary>
        /// Shows that task has been failed.
        /// </summary>
        Failed = 5,
        /// <summary>
        /// Mark task for cancelling.
        /// </summary>
        Cancelling = 6,
        /// <summary>
        /// Mark task as cancelled.
        /// </summary>
        Cancelled = 7
    }
}
