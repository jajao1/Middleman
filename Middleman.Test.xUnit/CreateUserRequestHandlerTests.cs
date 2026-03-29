using Middleman;
using Middleman.Tests.Support;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Middleman.Tests
{
    public class CreateUserRequestHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly CreateUserRequestHandler _handler;

        public CreateUserRequestHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _handler = new CreateUserRequestHandler(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_Should_CallRepositoryAdd_AndReturnNewUserId()
        {
            // Arrange (Organizar)
            var request = new CreateUserRequest("John Doe");
            var expectedId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(repo => repo.Add(It.IsAny<User>()))
                .ReturnsAsync(expectedId);

            // Act (Agir)
            var resultId = await _handler.Handle(request, CancellationToken.None);

            // Assert (Verificar)
            Assert.Equal(expectedId, resultId);
            _userRepositoryMock.Verify(repo => repo.Add(It.IsAny<User>()), Times.Once);
        }
    }
}
