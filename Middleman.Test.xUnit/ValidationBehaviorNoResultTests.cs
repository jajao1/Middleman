using FluentValidation;
using Moq;
using Middleman.FluentValidation;
using Middleman.Tests.Support;

namespace Middleman.Test.xUnit
{
    public class ValidationBehaviorNoResultTests
    {
        [Fact]
        public async Task Handle_WithValidRequest_ShouldCallNextDelegate()
        {
            // Arrange
            var validRequest = new DeleteUserRequest(Guid.NewGuid());
            var validator = new DeleteUserRequestValidator();

            var nextMock = new Mock<RequestHandlerDelegate>();
            nextMock.Setup(next => next(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var behavior = new ValidationBehaviorNoResult<DeleteUserRequest>(new[] { validator });

            // Act
            await behavior.Handle(validRequest, nextMock.Object, CancellationToken.None);

            // Assert
            nextMock.Verify(next => next(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidRequest_ShouldThrowValidationException_AndNotCallNext()
        {
            // Arrange
            var invalidRequest = new DeleteUserRequest(Guid.Empty);
            var validator = new DeleteUserRequestValidator();

            var nextMock = new Mock<RequestHandlerDelegate>();

            var behavior = new ValidationBehaviorNoResult<DeleteUserRequest>(new[] { validator });

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
            var request = new DeleteUserRequest(Guid.NewGuid());

            var nextMock = new Mock<RequestHandlerDelegate>();
            nextMock.Setup(next => next(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var behavior = new ValidationBehaviorNoResult<DeleteUserRequest>(Array.Empty<IValidator<DeleteUserRequest>>());

            // Act
            await behavior.Handle(request, nextMock.Object, CancellationToken.None);

            // Assert
            nextMock.Verify(next => next(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
