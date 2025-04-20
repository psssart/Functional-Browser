using NUnit.Framework;
using CefSharp;
using Functional_Browser;
using Moq;
using System.Collections.Specialized;

namespace Functional_Browser.Tests
{
    public class CustomRequestHandlerTests
    {
        private CustomRequestHandler _handler;

        [SetUp]
        public void Setup()
        {
            _handler = new CustomRequestHandler();
        }

        [Test]
        public void Should_Block_Ad_Request()
        {
            var requestMock = CreateMockRequest("https://ads.example.com/script.js");

            Console.WriteLine($"Mocked URL: {requestMock.Url}");

            var result = _handler.OnBeforeResourceLoad(null, null, null, requestMock, null);

            Assert.That(result, Is.EqualTo(CefReturnValue.Cancel));
        }

        [Test]
        public void Should_Allow_Normal_Request()
        {
            var requestMock = CreateMockRequest("https://example.com/index.html");

            var result = _handler.OnBeforeResourceLoad(null, null, null, requestMock, null);

            Assert.That(result, Is.EqualTo(CefReturnValue.Continue));
        }

        private IRequest CreateMockRequest(string url)
        {
            var mock = new Mock<IRequest>();
            mock.Setup(r => r.Url).Returns(url);
            mock.SetupAllProperties();

            mock.Setup(r => r.Headers).Returns(new NameValueCollection());
            mock.Setup(r => r.ResourceType).Returns(CefSharp.ResourceType.Script);
            mock.Setup(r => r.Identifier).Returns(0UL);
            mock.Setup(r => r.Method).Returns("GET");

            return mock.Object;
        }
    }
}
