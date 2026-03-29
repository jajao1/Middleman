namespace Middleman
{
    /// <summary>
    /// Defines a handler for a specific notification type.
    /// A notification can have multiple notification handlers.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to be handled.</typeparam>
    public interface INotificationHandler<in TNotification> where TNotification : INotification
    {
        /// <summary>
        /// Handles the notification. This method is called by the Middleman when a notification is published.
        /// </summary>
        /// <param name="notification">The notification object.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the completion of the handling process.</returns>
        Task Handle(TNotification notification, CancellationToken cancellationToken);
    }
}
