using Middleman.Abstractions;

namespace Middleman
{
    /// <summary>
    /// Defines the contract for the central dispatcher, 
    /// responsible for routing messages to their respective handlers
    /// and publishing notifications to all registered handlers.
    /// </summary>
    public interface IMiddleman : ISender, IPublisher
    {
    }
}
