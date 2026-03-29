namespace Middleman
{
    /// <summary>
    /// Represents an event/notification that has occurred in the system.
    /// Notifications are published via the IMiddleman and handled by one or more INotificationHandler instances.
    /// This is a marker interface; it does not contain any members.
    /// </summary>
    public interface INotification
    {
        // This interface is intentionally left empty.
        // It serves as a constraint for notification classes that can be published.
    }
}
