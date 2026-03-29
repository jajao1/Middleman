using Middleman;
using System.Threading;
using System.Threading.Tasks;

namespace Middleman.Tests.Support
{
    // --- Suporte para testes de Message com Retorno ---
    public class GetNumberQuery : IRequest<int> { }

    public class GetNumberQueryHandler : IRequestHandler<GetNumberQuery, int>
    {
        public Task<int> Handle(GetNumberQuery message, CancellationToken cancellationToken)
        {
            return Task.FromResult(42);
        }
    }

    // --- Suporte para testes de Notification ---
    public class MyTestNotification : INotification { }

    // Request SEM retorno
    public class DeleteUserRequest : IRequest
    {
        public Guid UserId { get; }
        public DeleteUserRequest(Guid userId) { UserId = userId; }
    }
}
