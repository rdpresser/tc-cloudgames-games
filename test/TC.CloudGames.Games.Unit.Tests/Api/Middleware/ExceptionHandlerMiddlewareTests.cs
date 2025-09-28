namespace TC.CloudGames.Games.Unit.Tests.Api.Middleware
{
    public class ExceptionHandlerMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WithException_ReturnsProblemDetails()
        {
            var context = new DefaultHttpContext();
            var logger = A.Fake<ILogger<ExceptionHandlerMiddleware>>();
            var middleware = new ExceptionHandlerMiddleware(_ => throw new Exception("Test exception"), logger);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task InvokeAsync_WithValidationException_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var logger = A.Fake<ILogger<ExceptionHandlerMiddleware>>();
            var validationException = new Games.Api.Exceptions.ValidationException([new() { ErrorMessage = "Validation failed" }]);
            var middleware = new ExceptionHandlerMiddleware(_ => throw validationException, logger);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task InvokeAsync_WithArgumentNullException_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var logger = A.Fake<ILogger<ExceptionHandlerMiddleware>>();
            var argumentException = new ArgumentNullException("parameter", "Parameter cannot be null");
            var middleware = new ExceptionHandlerMiddleware(_ => throw argumentException, logger);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidOperationException_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var logger = A.Fake<ILogger<ExceptionHandlerMiddleware>>();
            var invalidOperationException = new InvalidOperationException("Invalid operation performed");
            var middleware = new ExceptionHandlerMiddleware(_ => throw invalidOperationException, logger);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task InvokeAsync_WithUnauthorizedAccessException_ReturnsUnauthorized()
        {
            var context = new DefaultHttpContext();
            var logger = A.Fake<ILogger<ExceptionHandlerMiddleware>>();
            var unauthorizedException = new UnauthorizedAccessException("Access denied");
            var middleware = new ExceptionHandlerMiddleware(_ => throw unauthorizedException, logger);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task InvokeAsync_WithNoException_ContinuesToNextMiddleware()
        {
            var context = new DefaultHttpContext();
            var logger = A.Fake<ILogger<ExceptionHandlerMiddleware>>();
            var nextCalled = false;
            var middleware = new ExceptionHandlerMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, logger);

            await middleware.InvokeAsync(context);

            nextCalled.ShouldBeTrue();
            context.Response.StatusCode.ShouldBe(200); // Default status code
        }

        [Fact]
        public async Task InvokeAsync_WithCustomException_ReturnsInternalServerError()
        {
            var context = new DefaultHttpContext();
            var logger = A.Fake<ILogger<ExceptionHandlerMiddleware>>();
            var customException = new ApplicationException("Custom application error");
            var middleware = new ExceptionHandlerMiddleware(_ => throw customException, logger);

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        }
    }
}

