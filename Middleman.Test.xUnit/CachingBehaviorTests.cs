using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading;

namespace Middleman.Test.xUnit
{
    public class CachingBehaviorTests
    {
        [Fact]
        public async Task Send_WithCacheableRequest_AndSameKey_ShouldExecuteHandlerOnlyOnce()
        {
            // Arrange
            CacheableCounterQueryHandler.Reset();
            var middleman = BuildMiddleman();
            var query = new CacheableCounterQuery("users:all");

            // Act
            var first = await middleman.Send(query, CancellationToken.None);
            var second = await middleman.Send(query, CancellationToken.None);

            // Assert
            Assert.Equal(first, second);
            Assert.Equal(1, CacheableCounterQueryHandler.ExecutionCount);
        }

        [Fact]
        public async Task Send_WithCacheableRequest_AndDifferentKeys_ShouldNotShareCache()
        {
            // Arrange
            CacheableCounterQueryHandler.Reset();
            var middleman = BuildMiddleman();

            // Act
            var first = await middleman.Send(new CacheableCounterQuery("users:active"), CancellationToken.None);
            var second = await middleman.Send(new CacheableCounterQuery("users:inactive"), CancellationToken.None);

            // Assert
            Assert.NotEqual(first, second);
            Assert.Equal(2, CacheableCounterQueryHandler.ExecutionCount);
        }

        [Fact]
        public async Task Send_WithNonCacheableRequest_ShouldExecuteHandlerEveryTime()
        {
            // Arrange
            NonCacheableCounterQueryHandler.Reset();
            var middleman = BuildMiddleman();
            var query = new NonCacheableCounterQuery();

            // Act
            var first = await middleman.Send(query, CancellationToken.None);
            var second = await middleman.Send(query, CancellationToken.None);

            // Assert
            Assert.NotEqual(first, second);
            Assert.Equal(2, NonCacheableCounterQueryHandler.ExecutionCount);
        }

        private static IMiddleman BuildMiddleman()
        {
            var services = new ServiceCollection();
            services.AddMiddleman(Assembly.GetExecutingAssembly());
            services.AddMiddlemanCaching();

            return services.BuildServiceProvider().GetRequiredService<IMiddleman>();
        }

        public sealed class CacheableCounterQuery : ICacheableRequest<int>
        {
            public CacheableCounterQuery(string cacheKey)
            {
                CacheKey = cacheKey;
            }

            public string CacheKey { get; }

            public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);

            public TimeSpan? SlidingExpiration => null;
        }

        public sealed class CacheableCounterQueryHandler : IRequestHandler<CacheableCounterQuery, int>
        {
            private static int _executionCount;

            public static int ExecutionCount => _executionCount;

            public static void Reset()
            {
                _executionCount = 0;
            }

            public Task<int> Handle(CacheableCounterQuery message, CancellationToken cancellationToken)
            {
                return Task.FromResult(Interlocked.Increment(ref _executionCount));
            }
        }

        public sealed class NonCacheableCounterQuery : IRequest<int>
        {
        }

        public sealed class NonCacheableCounterQueryHandler : IRequestHandler<NonCacheableCounterQuery, int>
        {
            private static int _executionCount;

            public static int ExecutionCount => _executionCount;

            public static void Reset()
            {
                _executionCount = 0;
            }

            public Task<int> Handle(NonCacheableCounterQuery message, CancellationToken cancellationToken)
            {
                return Task.FromResult(Interlocked.Increment(ref _executionCount));
            }
        }
    }
}
