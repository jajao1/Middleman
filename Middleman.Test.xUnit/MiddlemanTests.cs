using Middleman;
using Middleman.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Middleman.Tests
{
    public class MiddlemanTests
    {
        [Fact]
        public async Task Send_Should_FindHandlerAndReturnResult()
        {
            // Arrange (Organizar)
            var query = new GetNumberQuery();
            var expectedResult = 42;
            var handler = new GetNumberQueryHandler();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IRequestHandler<GetNumberQuery, int>)))
                .Returns(handler);

            var middleman = new Middleman(serviceProviderMock.Object);

            // Act (Agir)
            var result = await middleman.Send(query, CancellationToken.None);

            // Assert (Verificar)
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task Send_Should_ThrowInvalidOperationException_WhenHandlerIsNotFound()
        {
            // Arrange
            var query = new GetNumberQuery();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(It.IsAny<Type>()))
                .Returns((object?)null);

            var middleman = new Middleman(serviceProviderMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => middleman.Send(query, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Publish_Should_FindAndExecuteAllRegisteredHandlers()
        {
            // Arrange (Organizar)
            var testNotification = new MyTestNotification();
            var handler1Mock = new Mock<INotificationHandler<MyTestNotification>>();
            var handler2Mock = new Mock<INotificationHandler<MyTestNotification>>();

            var handlers = new[] { handler1Mock.Object, handler2Mock.Object };

            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<MyTestNotification>>)))
                .Returns(handlers);

            var middleman = new Middleman(serviceProviderMock.Object);

            // Act (Agir)
            await middleman.Publish(testNotification, CancellationToken.None);

            // Assert (Verificar)
            handler1Mock.Verify(h => h.Handle(testNotification, It.IsAny<CancellationToken>()), Times.Once);
            handler2Mock.Verify(h => h.Handle(testNotification, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Publish_Should_NotThrow_WhenNoHandlersAreFound()
        {
            // Arrange
            var testNotification = new MyTestNotification();

            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var middleman = new Middleman(serviceProvider);

            // Act
            var exception = await Record.ExceptionAsync(() => middleman.Publish(testNotification, CancellationToken.None));

            // Assert
            Assert.Null(exception);
        }
    }
}
