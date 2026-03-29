namespace Middleman
{
    /// <summary>
    /// Defines a handler for stream requests.
    /// </summary>
    /// <typeparam name="TRequest">Stream request type.</typeparam>
    /// <typeparam name="TResponse">Type of each streamed item.</typeparam>
    public interface IStreamRequestHandler<in TRequest, TResponse>
        where TRequest : IStreamRequest<TResponse>
    {
        /// <summary>
        /// Handles a stream request and returns an async stream of results.
        /// </summary>
        /// <param name="request">The stream request object.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>An async stream of response items.</returns>
        IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
