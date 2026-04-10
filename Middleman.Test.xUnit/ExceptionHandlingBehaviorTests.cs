using Moq;

namespace Middleman.Test.xUnit
{
    public class ExceptionHandlingBehaviorTests
    {
        [Fact]
        public async Task Handle_WithResponse_WhenExceptionIsHandled_ShouldReturnFallbackResponse()
        {
            // Arrange
            var request = new ExceptionRequest();
            var expected = "fallback";
            var exception = new InvalidOperationException("boom");

            var exceptionHandler = new Mock<IExceptionHandler<ExceptionRequest, string>>();
            exceptionHandler
                .Setup(h => h.Handle(request, exception, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ExceptionHandlerState<string>.HandledWith(expected));

            var behavior = new ExceptionHandlingBehavior<ExceptionRequest, string>(new[] { exceptionHandler.Object });

            RequestHandlerDelegate<string> next = _ => throw exception;

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.Equal(expected, result);
            exceptionHandler.Verify(h => h.Handle(request, exception, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithResponse_WhenExceptionIsNotHandled_ShouldRethrow()
        {
            // Arrange
            var request = new ExceptionRequest();
            var exception = new InvalidOperationException("boom");

            var exceptionHandler = new Mock<IExceptionHandler<ExceptionRequest, string>>();
            exceptionHandler
                .Setup(h => h.Handle(request, exception, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ExceptionHandlerState<string>.NotHandled());

            var behavior = new ExceptionHandlingBehavior<ExceptionRequest, string>(new[] { exceptionHandler.Object });

            RequestHandlerDelegate<string> next = _ => throw exception;

            // Act / Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => behavior.Handle(request, next, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithoutResponse_WhenExceptionIsHandled_ShouldCompleteWithoutThrowing()
        {
            // Arrange
            var request = new ExceptionNoResultRequest();
            var exception = new InvalidOperationException("boom");

            var exceptionHandler = new Mock<IExceptionHandler<ExceptionNoResultRequest>>();
            exceptionHandler
                .Setup(h => h.Handle(request, exception, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ExceptionHandlerState.HandledResult());

            var behavior = new ExceptionHandlingBehavior<ExceptionNoResultRequest>(new[] { exceptionHandler.Object });

            RequestHandlerDelegate next = _ => throw exception;

            // Act
            var thrown = await Record.ExceptionAsync(
                () => behavior.Handle(request, next, CancellationToken.None));

            // Assert
            Assert.Null(thrown);
            exceptionHandler.Verify(h => h.Handle(request, exception, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutResponse_WhenExceptionIsNotHandled_ShouldRethrow()
        {
            // Arrange
            var request = new ExceptionNoResultRequest();
            var exception = new InvalidOperationException("boom");

            var exceptionHandler = new Mock<IExceptionHandler<ExceptionNoResultRequest>>();
            exceptionHandler
                .Setup(h => h.Handle(request, exception, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ExceptionHandlerState.NotHandled());

            var behavior = new ExceptionHandlingBehavior<ExceptionNoResultRequest>(new[] { exceptionHandler.Object });

            RequestHandlerDelegate next = _ => throw exception;

            // Act / Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => behavior.Handle(request, next, CancellationToken.None));
        }

        public sealed class ExceptionRequest : IRequest<string>
        {
        }

        public sealed class ExceptionNoResultRequest : IRequest
        {
        }
    }
}
