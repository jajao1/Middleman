namespace Middleman.Abstractions
{
    public interface ISender
    {
        /// <summary>
        /// Sends a request and returns a single response.
        /// </summary>
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a request without response.
        /// </summary>
        Task Send(IRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a stream request and returns an async stream response.
        /// </summary>
        IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
