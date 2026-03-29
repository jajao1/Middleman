namespace Middleman
{
    /// <summary>
    /// Marks a request as cacheable when dispatched through the pipeline.
    /// </summary>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public interface ICacheableRequest<out TResponse> : IRequest<TResponse>
    {
        /// <summary>
        /// Gets the key used to cache the request response.
        /// </summary>
        string CacheKey { get; }

        /// <summary>
        /// Gets an optional absolute expiration relative to now.
        /// </summary>
        TimeSpan? AbsoluteExpirationRelativeToNow { get; }

        /// <summary>
        /// Gets an optional sliding expiration.
        /// </summary>
        TimeSpan? SlidingExpiration { get; }
    }
}
