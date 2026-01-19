using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Task32.Infrastructure;

public class GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            throw HandleException(ex);
        }
    }

    private RpcException HandleException(Exception exception)
    {
        (StatusCode statusCode, string message) = exception switch
        {
            ArgumentException argEx => (StatusCode.InvalidArgument, argEx.Message),
            InvalidOperationException opEx => (StatusCode.FailedPrecondition, opEx.Message),
            KeyNotFoundException => (StatusCode.NotFound, "Resource not found"),
            _ => (StatusCode.Internal, "An internal error occurred"),
        };

        logger.LogError(exception, "gRPC error: {StatusCode} - {Message}", statusCode, message);

        var metadata = new Metadata
        {
            { "error-type", exception.GetType().Name },
            { "error-message", message },
        };

        return new RpcException(new Status(statusCode, message), metadata);
    }
}