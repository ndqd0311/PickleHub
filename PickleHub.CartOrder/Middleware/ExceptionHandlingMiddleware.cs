using System.Net;
using Microsoft.AspNetCore.Http;
using PickleHub.Common.DTOs;
using PickleHub.Common.Exceptions;

namespace PickleHub.CartOrder.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, message, errors) = ex switch
            {
                NotFoundException nf => (HttpStatusCode.NotFound, nf.Message, (Dictionary<string, string[]>?)null),
                ConflictException cf => (HttpStatusCode.Conflict, cf.Message, (Dictionary<string, string[]>?)null),
                ForbiddenException fb => (HttpStatusCode.Forbidden, fb.Message, (Dictionary<string, string[]>?)null),
                UnauthorizedException ua => (HttpStatusCode.Unauthorized, ua.Message, (Dictionary<string, string[]>?)null),
                FluentValidation.ValidationException vl => (
                    HttpStatusCode.BadRequest,
                    "Validation failed.",
                    vl.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        )
                ),
                _ => (HttpStatusCode.InternalServerError, "Đã có lỗi xảy ra phía hệ thống.", (Dictionary<string, string[]>?)null)
            };

            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                Error = new ErrorDetail
                {
                    Message = message,
                    Errors = errors
                }
            });
        }
    }
}
