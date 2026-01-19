using Grpc.Core;
using System.Text.Json;

namespace Task33.Middleware;

public class GrpcExceptionMiddleware(RequestDelegate next, ILogger<GrpcExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (RpcException exception)
        {
            await HandleRpcExceptionAsync(context, exception);
        }
    }

    private async Task HandleRpcExceptionAsync(HttpContext context, RpcException exception)
    {
        logger.LogError(
            exception,
            "gRPC error occurred: {StatusCode} - {Message}",
            exception.StatusCode,
            exception.Status.Detail);

        int statusCode = exception.StatusCode switch
        {
            StatusCode.InvalidArgument => StatusCodes.Status400BadRequest,
            StatusCode.NotFound => StatusCodes.Status404NotFound,
            StatusCode.FailedPrecondition => StatusCodes.Status412PreconditionFailed,
            StatusCode.Unauthenticated => StatusCodes.Status401Unauthorized,
            StatusCode.PermissionDenied => StatusCodes.Status403Forbidden,
            StatusCode.Unavailable => StatusCodes.Status503ServiceUnavailable,
            StatusCode.Internal => StatusCodes.Status500InternalServerError,
            StatusCode.Cancelled => 499,
            StatusCode.Unknown => StatusCodes.Status500InternalServerError,
            StatusCode.DeadlineExceeded => StatusCodes.Status504GatewayTimeout,
            StatusCode.AlreadyExists => StatusCodes.Status409Conflict,
            StatusCode.ResourceExhausted => StatusCodes.Status429TooManyRequests,
            StatusCode.Aborted => StatusCodes.Status409Conflict,
            StatusCode.OutOfRange => StatusCodes.Status400BadRequest,
            StatusCode.Unimplemented => StatusCodes.Status501NotImplemented,
            StatusCode.DataLoss => StatusCodes.Status500InternalServerError,
            StatusCode.OK => StatusCodes.Status200OK,
            _ => StatusCodes.Status500InternalServerError,
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var errorResponse = new
        {
            error = exception.Status.Detail,
            statusCode = exception.StatusCode.ToString(),
            metadata = exception.Trailers.ToDictionary(
                entry => entry.Key,
                entry => entry.Value),
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
