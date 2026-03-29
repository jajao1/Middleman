namespace Middleman
{
    public enum NotificationPublishStrategy
    {
        Parallel = 0,
        Sequential = 1
    }

    public sealed class MiddlemanOptions
    {
        /// <summary>
        /// Defines how notification handlers are executed.
        /// </summary>
        public NotificationPublishStrategy PublishStrategy { get; set; } = NotificationPublishStrategy.Parallel;

        /// <summary>
        /// When true, all handlers are executed even if some fail.
        /// </summary>
        public bool ContinueOnException { get; set; }
    }
}
