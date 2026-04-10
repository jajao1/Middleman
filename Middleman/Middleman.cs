using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Middleman
{
    /// <summary>
    /// The concrete implementation of IMiddleman. It uses an IServiceProvider
    /// to resolve and dispatch messages and notifications to their respective handlers.
    /// </summary>
    public class Middleman : IMiddleman
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MiddlemanOptions _options;

        public Middleman(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            _serviceProvider = serviceProvider;
            _options = serviceProvider.GetService<MiddlemanOptions>() ?? new MiddlemanOptions();
        }

        public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            var notificationType = notification.GetType();
            var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

            var handlers = _serviceProvider.GetServices(handlerType)
                .Where(handler => handler is not null)
                .Cast<object>()
                .ToList();

            if (handlers.Count == 0)
            {
                return;
            }

            var handleMethod = handlerType.GetMethod("Handle", [notificationType, typeof(CancellationToken)]);

            if (handleMethod is null)
            {
                throw new InvalidOperationException($"Method 'Handle' not found on handler for notification '{notificationType.Name}'");
            }

            var handlerDelegates = handlers
                .Select(handler => (Func<Task>)(() => InvokeNotificationHandler(handleMethod, handler, notification, cancellationToken)))
                .ToList();

            switch (_options.PublishStrategy)
            {
                case NotificationPublishStrategy.Sequential:
                    await ExecuteSequential(handlerDelegates).ConfigureAwait(false);
                    break;
                default:
                    await ExecuteParallel(handlerDelegates).ConfigureAwait(false);
                    break;
            }
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var requestType = request.GetType();

            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType)
                ?? throw new InvalidOperationException($"No handler found for request type {requestType.Name}");

            RequestHandlerDelegate<TResponse> handlerDelegate = token =>
            {
                var handleMethod = handler.GetType().GetMethod("Handle", [requestType, typeof(CancellationToken)]);
                if (handleMethod is null)
                {
                    throw new InvalidOperationException($"Method 'Handle' not found on handler '{handler.GetType().Name}'");
                }

                var task = handleMethod.Invoke(handler, [request, token]) as Task<TResponse>;
                if (task is null)
                {
                    throw new InvalidOperationException($"Handler '{handler.GetType().Name}' returned an invalid task.");
                }

                return task;
            };

            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
            var enumerableBehaviorType = typeof(IEnumerable<>).MakeGenericType(behaviorType);

            var behaviors = (_serviceProvider.GetService(enumerableBehaviorType) as IEnumerable<object> ?? Enumerable.Empty<object>())
                .Reverse();

            var pipeline = behaviors.Aggregate(
                handlerDelegate,
                (next, behavior) => token =>
                {
                    var handleMethod = behaviorType.GetMethod(
                        "Handle",
                        [requestType, typeof(RequestHandlerDelegate<TResponse>), typeof(CancellationToken)]);

                    if (handleMethod is null)
                    {
                        throw new InvalidOperationException(
                            $"Method 'Handle' not found on behavior '{behavior.GetType().Name}'.");
                    }

                    var task = handleMethod.Invoke(behavior, [request, next, token]) as Task<TResponse>;
                    if (task is null)
                    {
                        throw new InvalidOperationException(
                            $"Behavior '{behavior.GetType().Name}' returned an invalid task.");
                    }

                    return task;
                });

            return pipeline(cancellationToken);
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var requestType = request.GetType();

            var handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType)
                ?? throw new InvalidOperationException($"No stream handler found for request type {requestType.Name}");

            StreamHandlerDelegate<TResponse> handlerDelegate = token =>
            {
                var handleMethod = handler.GetType().GetMethod("Handle", [requestType, typeof(CancellationToken)]);
                if (handleMethod is null)
                {
                    throw new InvalidOperationException($"Method 'Handle' not found on stream handler '{handler.GetType().Name}'");
                }

                var stream = handleMethod.Invoke(handler, [request, token]) as IAsyncEnumerable<TResponse>;
                if (stream is null)
                {
                    throw new InvalidOperationException($"Stream handler '{handler.GetType().Name}' returned an invalid stream.");
                }

                return stream;
            };

            var behaviorType = typeof(IStreamPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
            var enumerableBehaviorType = typeof(IEnumerable<>).MakeGenericType(behaviorType);

            var behaviors = (_serviceProvider.GetService(enumerableBehaviorType) as IEnumerable<object> ?? Enumerable.Empty<object>())
                .Reverse();

            var pipeline = behaviors.Aggregate(
                handlerDelegate,
                (next, behavior) => token =>
                {
                    var handleMethod = behaviorType.GetMethod(
                        "Handle",
                        [requestType, typeof(StreamHandlerDelegate<TResponse>), typeof(CancellationToken)]);

                    if (handleMethod is null)
                    {
                        throw new InvalidOperationException(
                            $"Method 'Handle' not found on stream behavior '{behavior.GetType().Name}'.");
                    }

                    var stream = handleMethod.Invoke(behavior, [request, next, token]) as IAsyncEnumerable<TResponse>;
                    if (stream is null)
                    {
                        throw new InvalidOperationException(
                            $"Stream behavior '{behavior.GetType().Name}' returned an invalid stream.");
                    }

                    return stream;
                });

            return pipeline(cancellationToken);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var requestType = request.GetType();

            var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);
            var handler = _serviceProvider.GetService(handlerType)
                ?? throw new InvalidOperationException($"No handler found for request type {requestType.Name}");

            RequestHandlerDelegate handlerDelegate = token =>
            {
                var handleMethod = handler.GetType().GetMethod("Handle", [requestType, typeof(CancellationToken)]);
                if (handleMethod is null)
                {
                    throw new InvalidOperationException($"Method 'Handle' not found on handler '{handler.GetType().Name}'");
                }

                var task = handleMethod.Invoke(handler, [request, token]) as Task;
                if (task is null)
                {
                    throw new InvalidOperationException($"Handler '{handler.GetType().Name}' returned an invalid task.");
                }

                return task;
            };

            var behaviorType = typeof(IPipelineBehavior<>).MakeGenericType(requestType);
            var enumerableBehaviorType = typeof(IEnumerable<>).MakeGenericType(behaviorType);

            var behaviors = (_serviceProvider.GetService(enumerableBehaviorType) as IEnumerable<object> ?? Enumerable.Empty<object>())
                .Reverse();

            var pipeline = behaviors.Aggregate(
                handlerDelegate,
                (next, behavior) => token =>
                {
                    var handleMethod = behaviorType.GetMethod(
                        "Handle",
                        [requestType, typeof(RequestHandlerDelegate), typeof(CancellationToken)]);

                    if (handleMethod is null)
                    {
                        throw new InvalidOperationException(
                            $"Method 'Handle' not found on behavior '{behavior.GetType().Name}'.");
                    }

                    var task = handleMethod.Invoke(behavior, [request, next, token]) as Task;
                    if (task is null)
                    {
                        throw new InvalidOperationException(
                            $"Behavior '{behavior.GetType().Name}' returned an invalid task.");
                    }

                    return task;
                });

            return pipeline(cancellationToken);
        }

        private Task InvokeNotificationHandler(MethodInfo handleMethod, object handler, INotification notification, CancellationToken cancellationToken)
        {
            try
            {
                var task = handleMethod.Invoke(handler, [notification, cancellationToken]) as Task;
                if (task is null)
                {
                    throw new InvalidOperationException(
                        $"Handler '{handler.GetType().Name}' returned an invalid task for notification '{notification.GetType().Name}'.");
                }

                return task;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                return Task.FromException(ex.InnerException);
            }
        }

        private async Task ExecuteParallel(IReadOnlyCollection<Func<Task>> handlerDelegates)
        {
            if (!_options.ContinueOnException)
            {
                await Task.WhenAll(handlerDelegates.Select(handler => handler())).ConfigureAwait(false);
                return;
            }

            var tasks = handlerDelegates
                .Select(async handler =>
                {
                    try
                    {
                        await handler().ConfigureAwait(false);
                        return (Exception?)null;
                    }
                    catch (Exception ex)
                    {
                        return ex;
                    }
                })
                .ToList();

            var exceptions = (await Task.WhenAll(tasks).ConfigureAwait(false))
                .Where(exception => exception is not null)
                .Cast<Exception>()
                .ToList();

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        private async Task ExecuteSequential(IReadOnlyCollection<Func<Task>> handlerDelegates)
        {
            if (!_options.ContinueOnException)
            {
                foreach (var handler in handlerDelegates)
                {
                    await handler().ConfigureAwait(false);
                }

                return;
            }

            var exceptions = new List<Exception>();

            foreach (var handler in handlerDelegates)
            {
                try
                {
                    await handler().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
