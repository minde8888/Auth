using Auth.Api.Middlewares;
using Auth.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Net;
namespace tests.Middlewares
{
    public class ExceptionMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_Returns_InternalServerError_When_Exception_Not_Handled()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var middleware = new ExceptionMiddleware(innerHttpContext =>
            {
                throw new Exception("Unhandled exception occurred.");
            });

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var result = context.Response;
            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns_NotFound_When_UserExistException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var middleware = new ExceptionMiddleware(innerHttpContext =>
            {
                throw new UserExistException();
            });

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns_NotFound_When_RoleNotExistException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var middleware = new ExceptionMiddleware(innerHttpContext =>
            {
                throw new RoleNotExistException();
            });

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns_NotFound_When_UserNotFoundException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var middleware = new ExceptionMiddleware(innerHttpContext =>
            {
                throw new UserNotFoundException();
            });

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Returns_NotFound_When_ExternalAuthException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var middleware = new ExceptionMiddleware(innerHttpContext =>
            {
                throw new ExternalAuthException();
            });

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        }
    }
}
