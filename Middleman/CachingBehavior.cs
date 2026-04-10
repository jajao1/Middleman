using Microsoft.Extensions.Caching.Memory;

namespace Middleman
{
    /// <summary>
    /// Caches request responses when the request implements <see cref="ICacheableRequest{TResponse}"/>.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IMemoryCache _cache;

        public CachingBehavior(IMemoryCache cache)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            _cache = cache;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is not ICacheableRequest<TResponse> cacheableRequest)
            {
                return await next(cancellationToken).ConfigureAwait(false);
            }

            if (_cache.TryGetValue(cacheableRequest.CacheKey, out TResponse? cachedResponse))
            {
                return cachedResponse!;
            }

            var response = await next(cancellationToken).ConfigureAwait(false);
            var entryOptions = BuildEntryOptions(cacheableRequest);

            if (entryOptions is null)
            {
                _cache.Set(cacheableRequest.CacheKey, response);
            }
            else
            {
                _cache.Set(cacheableRequest.CacheKey, response, entryOptions);
            }

            return response;
        }

        private static MemoryCacheEntryOptions? BuildEntryOptions(ICacheableRequest<TResponse> request)
        {
            if (!request.AbsoluteExpirationRelativeToNow.HasValue && !request.SlidingExpiration.HasValue)
            {
                return null;
            }

            var options = new MemoryCacheEntryOptions();

            if (request.AbsoluteExpirationRelativeToNow.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = request.AbsoluteExpirationRelativeToNow;
            }

            if (request.SlidingExpiration.HasValue)
            {
                options.SlidingExpiration = request.SlidingExpiration;
            }

            return options;
        }
    }
}
