namespace Middleman
{
    /// <summary>
    /// Represents a request/query that yields a stream of responses.
    /// </summary>
    /// <typeparam name="TResponse">Type of each streamed item.</typeparam>
    public interface IStreamRequest<out TResponse>
    {
    }
}
