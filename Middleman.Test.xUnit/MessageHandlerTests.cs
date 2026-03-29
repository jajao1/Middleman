using Middleman;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Middleman.Tests.Support
{
    // --- Entidade de Domínio ---
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // --- Abstração do Repositório (a dependência que vamos mockar) ---
    public interface IUserRepository
    {
        Task<Guid> Add(User user);
    }

    // --- O Request e seu Handler (o que vamos testar) ---
    public class CreateUserRequest : IRequest<Guid>
    {
        public string Name { get; }
        public CreateUserRequest(string name) => Name = name;
    }

    public class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, Guid>
    {
        private readonly IUserRepository _userRepository;

        public CreateUserRequestHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Guid> Handle(CreateUserRequest message, CancellationToken cancellationToken)
        {
            var user = new User { Name = message.Name };
            var newId = await _userRepository.Add(user);
            return newId;
        }
    }
}
