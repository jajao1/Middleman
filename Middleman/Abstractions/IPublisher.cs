namespace Middleman.Abstractions
{
    public interface IPublisher
    {
        /// <summary>
        /// Publishes a notification to all registered handlers.
        /// </summary>
        Task Publish(INotification notification, CancellationToken cancellationToken = default);
    }
}
