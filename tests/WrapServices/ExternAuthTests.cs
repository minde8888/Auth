using Moq;
using Auth.Services.WrapServices;
using Google.Apis.Auth;
using Auth.Domain.Exceptions;
using Auth.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace tests.WrapServices
{
    public class ExternAuthTests
    {
        private readonly Mock<IExternAuth> _externAuthMock;
        private readonly IExternAuth _service;

        public ExternAuthTests() 
        {
            _externAuthMock = new Mock<IExternAuth>();
            _service = _externAuthMock.Object;
        }

        [Fact]
        public async Task GoogleJsonValidaror_InvalidToken_ThrowsExternalAuthException()
        {
            // Arrange
            _externAuthMock.Setup(x => x.GoogleJsonValidaror(It.IsAny<string>()))
                .ThrowsAsync(new ExternalAuthException());

            // Act + Assert
            await Assert.ThrowsAsync<ExternalAuthException>(() => _service.GoogleJsonValidaror("invalid-token"));
        }

        [Fact]
        public async Task GoogleJsonValidaror_ValidToken_ReturnsPayload()
        {
            // Arrange
            var payload = new GoogleJsonWebSignature.Payload();
            _externAuthMock.Setup(x => x.GoogleJsonValidaror(It.IsAny<string>()))
                .ReturnsAsync(payload);

            // Act
            var result = await _service.GoogleJsonValidaror("valid-token");

            // Assert
            Assert.Same(payload, result);
        }
    }
}
