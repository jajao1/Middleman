using FluentValidation;
using Moq;
using Middleman.FluentValidation;
using ValidationException = FluentValidation.ValidationException;

namespace Middleman.Test.xUnit
{
    public class CreateUserRequest : IRequest<Guid>
    {
        public string UserName { get; }
        public string Email { get; }
        public CreateUserRequest(string userName, string email) { UserName = userName; Email = email; }
    }

    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().MinimumLength(3);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    // --- A CLASSE DE TESTES ---
    public class ValidationBehaviorTests
    {
        [Fact]
        public async Task Handle_WithValidRequest_ShouldCallNextDelegate()
        {
            // Arrange
            var validRequest = new CreateUserRequest("John Doe", "john.doe@example.com");
            var validator = new CreateUserRequestValidator();

            var nextMock = new Mock<RequestHandlerDelegate<Guid>>();
            nextMock.Setup(next => next(It.IsAny<CancellationToken>())).Returns(Task.FromResult(Guid.NewGuid()));

            var behavior = new ValidationBehavior<CreateUserRequest, Guid>(new[] { validator });

            // Act
            await behavior.Handle(validRequest, nextMock.Object, CancellationToken.None);

            // Assert
            nextMock.Verify(next => next(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidRequest_ShouldThrowValidationException_AndNotCallNext()
        {
            // Arrange
            var invalidRequest = new CreateUserRequest("", "not-an-email");
            var validator = new CreateUserRequestValidator();

            var nextMock = new Mock<RequestHandlerDelegate<Guid>>();

            var behavior = new ValidationBehavior<CreateUserRequest, Guid>(new[] { validator });

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                behavior.Handle(invalidRequest, nextMock.Object, CancellationToken.None)
            );

            nextMock.Verify(next => next(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNoValidators_ShouldCallNextDelegate()
        {
            // Arrange
            var request = new CreateUserRequest("Any", "any@example.com");

            var nextMock = new Mock<RequestHandlerDelegate<Guid>>();
            nextMock.Setup(next => next(It.IsAny<CancellationToken>())).Returns(Task.FromResult(Guid.NewGuid()));

            var behavior = new ValidationBehavior<CreateUserRequest, Guid>(Array.Empty<IValidator<CreateUserRequest>>());

            // Act
            await behavior.Handle(request, nextMock.Object, CancellationToken.None);

            // Assert
            nextMock.Verify(next => next(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
