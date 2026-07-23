using PickleHub.Common.DTOs;
using PickleHub.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace PickleHub.Authen.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (statusCode, message, errors) = ex switch
                {
                    NotFoundException nf => (HttpStatusCode.NotFound, nf.Message, (Dictionary<string, string[]>?)null),
                    ConflictException cf => (HttpStatusCode.Conflict, cf.Message, (Dictionary<string, string[]>?)null),
                    ForbiddenException fb => (HttpStatusCode.Forbidden, fb.Message, (Dictionary<string, string[]>?)null),
                    UnauthorizedException ua => (HttpStatusCode.Unauthorized, ua.Message, (Dictionary<string, string[]>?)null),
                    DomainException de => (HttpStatusCode.BadRequest, de.Message, (Dictionary<string, string[]>?)null),
                    //ValidationException vl => (HttpStatusCode.BadRequest, vl.Message,
                    //    vl.Errors.Count > 0 ? (object?)vl.Errors : null),
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

                //context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;

                await context.Response.WriteAsJsonAsync(new ErrorResponseDto
                {
                    Error = new ErrorDetail
                    {
                        Message = message,
                        Errors = errors
                    }
                });

                //var payload = JsonSerializer.Serialize(new
                //{
                //    error = new { code, message }
                //});

                //await context.Response.WriteAsync(payload);
            }
        }
    }
}
