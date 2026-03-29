namespace Middleman
{
    /// <summary>
    /// Handles exceptions thrown while processing requests with a return value.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public interface IExceptionHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handles an exception thrown during request processing.
        /// </summary>
        /// <param name="request">The request being processed.</param>
        /// <param name="exception">The thrown exception.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A result indicating if the exception was handled, and an optional fallback response.
        /// </returns>
        Task<ExceptionHandlerState<TResponse>> Handle(
            TRequest request,
            Exception exception,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Handles exceptions thrown while processing requests without a return value.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    public interface IExceptionHandler<in TRequest> where TRequest : IRequest
    {
        /// <summary>
        /// Handles an exception thrown during request processing.
        /// </summary>
        /// <param name="request">The request being processed.</param>
        /// <param name="exception">The thrown exception.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A result indicating if the exception was handled.</returns>
        Task<ExceptionHandlerState> Handle(
            TRequest request,
            Exception exception,
            CancellationToken cancellationToken);
    }
}
