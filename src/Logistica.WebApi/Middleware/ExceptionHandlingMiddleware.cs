using System.Text.Json;
using Joseco.DDD.Core.Results;

namespace Logistica.WebApi.Middleware;

// §11.9 DESIGN.md, §12.1 del maestro: unico lugar donde una DomainException se traduce a
// {codigo, mensaje}. Como unica excepcion documentada, ArgumentException tambien se traduce a
// 400 (INC-S2, §19.5 DESIGN.md): Joseco.DDD.Core.Abstractions.Entity(Guid id) la lanza con
// Guid.Empty, y sin esta rama un id vacio saldria como 500 sin cuerpo. Cualquier otra excepcion
// cae al manejo por defecto de ASP.NET Core: no hay catch-all.
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "DomainException {Codigo}: {Mensaje}", ex.Error.Code, ex.Error.Description);
            await EscribirRespuestaAsync(context, ex.Error.Code, ex.Error.Description, MapearHttpStatus(ex.Error.Type));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "ArgumentException traducida a 400 (INC-S2)");
            await EscribirRespuestaAsync(context, "ARGUMENTO_INVALIDO", ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    private static int MapearHttpStatus(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Failure => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Problem => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status400BadRequest
    };

    private static async Task EscribirRespuestaAsync(HttpContext context, string codigo, string mensaje, int statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var cuerpo = new { codigo, mensaje };
        await context.Response.WriteAsync(JsonSerializer.Serialize(cuerpo, SerializerOptions));
    }
}
