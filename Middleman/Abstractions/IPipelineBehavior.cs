namespace Middleman
{
    // Delegate for the request pipeline with response
    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

    // Delegate for the request pipeline WITHOUT response
    public delegate Task RequestHandlerDelegate();

    // Delegate for the streaming request pipeline
    public delegate IAsyncEnumerable<TResponse> StreamHandlerDelegate<TResponse>();

    // Interface for behaviors of requests WITH response
    public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
    }

    // Interface for behaviors of requests WITHOUT response
    public interface IPipelineBehavior<in TRequest> where TRequest : IRequest
    {
        Task Handle(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken);
    }

    // Interface for behaviors of streaming requests
    public interface IStreamPipelineBehavior<in TRequest, TResponse> where TRequest : IStreamRequest<TResponse>
    {
        IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
    }
}
