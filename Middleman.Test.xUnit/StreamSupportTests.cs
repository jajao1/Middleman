using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Middleman.Test.xUnit
{
    public class StreamSupportTests
    {
        [Fact]
        public async Task CreateStream_WithRegisteredHandler_ShouldYieldExpectedItems()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddMiddleman(Assembly.GetExecutingAssembly());
            var middleman = services.BuildServiceProvider().GetRequiredService<IMiddleman>();

            // Act
            var results = new List<int>();
            await foreach (var item in middleman.CreateStream(new GetNumbersStreamQuery(3), CancellationToken.None))
            {
                results.Add(item);
            }

            // Assert
            Assert.Equal([0, 1, 2], results);
        }

        [Fact]
        public void CreateStream_WithoutRegisteredHandler_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(It.IsAny<Type>()))
                .Returns((object?)null);

            var middleman = new Middleman(serviceProviderMock.Object);

            // Act / Assert
            Assert.Throws<InvalidOperationException>(
                () => middleman.CreateStream(new MissingStreamQuery(), CancellationToken.None));
        }

        public sealed class GetNumbersStreamQuery : IStreamRequest<int>
        {
            public GetNumbersStreamQuery(int count)
            {
                Count = count;
            }

            public int Count { get; }
        }

        public sealed class GetNumbersStreamQueryHandler : IStreamRequestHandler<GetNumbersStreamQuery, int>
        {
            public async IAsyncEnumerable<int> Handle(
                GetNumbersStreamQuery request,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                for (var i = 0; i < request.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return i;
                    await Task.Yield();
                }
            }
        }

        public sealed class MissingStreamQuery : IStreamRequest<int>
        {
        }
    }
}
