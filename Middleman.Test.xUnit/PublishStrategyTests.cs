using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Middleman.Test.xUnit
{
    public class PublishStrategyTests
    {
        [Fact]
        public async Task Publish_WithSequentialStrategy_ShouldRunHandlersInRegistrationOrder()
        {
            // Arrange
            var executionOrder = new List<int>();
            var middleman = BuildMiddleman(
                options => options.PublishStrategy = NotificationPublishStrategy.Sequential,
                services =>
                {
                    services.AddSingleton<IList<int>>(executionOrder);
                });

            // Act
            await middleman.Publish(new SequentialOrderTrackingNotification(), CancellationToken.None);

            // Assert
            Assert.Equal([1, 2], executionOrder);
        }

        [Fact]
        public async Task Publish_WithSequentialAndContinueOnException_ShouldExecuteAllAndThrowAggregate()
        {
            // Arrange
            var executionOrder = new List<int>();
            var middleman = BuildMiddleman(
                options =>
                {
                    options.PublishStrategy = NotificationPublishStrategy.Sequential;
                    options.ContinueOnException = true;
                },
                services =>
                {
                    services.AddSingleton<IList<int>>(executionOrder);
                });

            // Act
            var exception = await Assert.ThrowsAsync<AggregateException>(
                () => middleman.Publish(new SequentialContinueNotification(), CancellationToken.None));

            // Assert
            Assert.Single(exception.InnerExceptions);
            Assert.Equal([1, 2], executionOrder);
        }

        [Fact]
        public async Task Publish_WithParallelAndContinueOnException_ShouldAggregateAllFailures()
        {
            // Arrange
            var executionOrder = new List<int>();
            var middleman = BuildMiddleman(
                options =>
                {
                    options.PublishStrategy = NotificationPublishStrategy.Parallel;
                    options.ContinueOnException = true;
                },
                services =>
                {
                    services.AddSingleton<IList<int>>(executionOrder);
                });

            // Act
            var exception = await Assert.ThrowsAsync<AggregateException>(
                () => middleman.Publish(new ParallelContinueNotification(), CancellationToken.None));

            // Assert
            Assert.Equal(2, exception.InnerExceptions.Count);
            Assert.Equal(2, executionOrder.Count);
        }

        private static IMiddleman BuildMiddleman(Action<MiddlemanOptions> configureOptions, Action<IServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            services.AddMiddleman(Assembly.GetExecutingAssembly(), configureOptions);
            configureServices(services);

            return services.BuildServiceProvider().GetRequiredService<IMiddleman>();
        }

        public sealed class SequentialOrderTrackingNotification : INotification
        {
        }

        public sealed class SequentialContinueNotification : INotification
        {
        }

        public sealed class ParallelContinueNotification : INotification
        {
        }

        public sealed class FirstSequentialOrderHandler : INotificationHandler<SequentialOrderTrackingNotification>
        {
            private readonly IList<int> _executionOrder;

            public FirstSequentialOrderHandler(IList<int> executionOrder)
            {
                _executionOrder = executionOrder;
            }

            public Task Handle(SequentialOrderTrackingNotification notification, CancellationToken cancellationToken)
            {
                _executionOrder.Add(1);
                return Task.CompletedTask;
            }
        }

        public sealed class SecondSequentialOrderHandler : INotificationHandler<SequentialOrderTrackingNotification>
        {
            private readonly IList<int> _executionOrder;

            public SecondSequentialOrderHandler(IList<int> executionOrder)
            {
                _executionOrder = executionOrder;
            }

            public Task Handle(SequentialOrderTrackingNotification notification, CancellationToken cancellationToken)
            {
                _executionOrder.Add(2);
                return Task.CompletedTask;
            }
        }

        public sealed class ThrowingSequentialContinueHandler : INotificationHandler<SequentialContinueNotification>
        {
            private readonly IList<int> _executionOrder;

            public ThrowingSequentialContinueHandler(IList<int> executionOrder)
            {
                _executionOrder = executionOrder;
            }

            public Task Handle(SequentialContinueNotification notification, CancellationToken cancellationToken)
            {
                _executionOrder.Add(1);
                throw new InvalidOperationException("handler 1 failed");
            }
        }

        public sealed class SuccessSequentialContinueHandler : INotificationHandler<SequentialContinueNotification>
        {
            private readonly IList<int> _executionOrder;

            public SuccessSequentialContinueHandler(IList<int> executionOrder)
            {
                _executionOrder = executionOrder;
            }

            public Task Handle(SequentialContinueNotification notification, CancellationToken cancellationToken)
            {
                _executionOrder.Add(2);
                return Task.CompletedTask;
            }
        }

        public sealed class ThrowingParallelContinueHandlerOne : INotificationHandler<ParallelContinueNotification>
        {
            private readonly IList<int> _executionOrder;

            public ThrowingParallelContinueHandlerOne(IList<int> executionOrder)
            {
                _executionOrder = executionOrder;
            }

            public Task Handle(ParallelContinueNotification notification, CancellationToken cancellationToken)
            {
                _executionOrder.Add(1);
                throw new InvalidOperationException("handler 1 failed");
            }
        }

        public sealed class ThrowingParallelContinueHandlerTwo : INotificationHandler<ParallelContinueNotification>
        {
            private readonly IList<int> _executionOrder;

            public ThrowingParallelContinueHandlerTwo(IList<int> executionOrder)
            {
                _executionOrder = executionOrder;
            }

            public Task Handle(ParallelContinueNotification notification, CancellationToken cancellationToken)
            {
                _executionOrder.Add(2);
                throw new InvalidOperationException("handler 2 failed");
            }
        }
    }
}
