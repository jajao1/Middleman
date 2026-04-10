namespace Middleman
{
    /// <summary>
    /// Centralized exception handling behavior for requests that return a response.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public sealed class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IExceptionHandler<TRequest, TResponse>> _handlers;

        public ExceptionHandlingBehavior(IEnumerable<IExceptionHandler<TRequest, TResponse>> handlers)
        {
            ArgumentNullException.ThrowIfNull(handlers);

            _handlers = handlers as IExceptionHandler<TRequest, TResponse>[] ?? handlers.ToArray();
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            try
            {
                return await next(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                foreach (var handler in _handlers)
                {
                    var result = await handler.Handle(request, ex, cancellationToken).ConfigureAwait(false);
                    if (result.Handled)
                    {
                        return result.Response;
                    }
                }

                throw;
            }
        }
    }

    /// <summary>
    /// Centralized exception handling behavior for requests without a response.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    public sealed class ExceptionHandlingBehavior<TRequest> : IPipelineBehavior<TRequest>
        where TRequest : IRequest
    {
        private readonly IEnumerable<IExceptionHandler<TRequest>> _handlers;

        public ExceptionHandlingBehavior(IEnumerable<IExceptionHandler<TRequest>> handlers)
        {
            ArgumentNullException.ThrowIfNull(handlers);

            _handlers = handlers as IExceptionHandler<TRequest>[] ?? handlers.ToArray();
        }

        public async Task Handle(
            TRequest request,
            RequestHandlerDelegate next,
            CancellationToken cancellationToken)
        {
            try
            {
                await next(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                foreach (var handler in _handlers)
                {
                    var result = await handler.Handle(request, ex, cancellationToken).ConfigureAwait(false);
                    if (result.Handled)
                    {
                        return;
                    }
                }

                throw;
            }
        }
    }
}
