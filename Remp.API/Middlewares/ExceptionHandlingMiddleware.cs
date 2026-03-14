using System.Net;
using System.Text.Json;
using FluentValidation;
using Remp.Common.Exceptions;
using Remp.Common.Helpers;

namespace Remp.API.Middlewares;

public class ExceptionHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionHandlingMiddleware> _logger;
  private readonly IHostEnvironment _env;

  public ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
  {
    _next = next;
    _logger = logger;
    _env = env;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
      await HandleExceptionAsync(context, ex);
    }
  }

  private async Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    context.Response.ContentType = "application/json";

    int statusCode;
    string message;
    List<string> errors = [];

    switch (exception)
    {
      case ValidationException validationEx:
        statusCode = (int)HttpStatusCode.BadRequest;
        message = "Validation failed.";
        errors = validationEx.Errors
          .Select(e => e.ErrorMessage)
          .ToList();
        break;

      case UnauthorizedAccessException:
        statusCode = (int)HttpStatusCode.Unauthorized;
        message = "Authentication required.";
        break;

      case ForbiddenException:
        statusCode = (int)HttpStatusCode.Forbidden;
        message = exception.Message;
        break;

      case NotFoundException:
        statusCode = (int)HttpStatusCode.NotFound;
        message = exception.Message;
        break;
      
      case InvalidOperationException:
        statusCode = (int)HttpStatusCode.Conflict;
        message = exception.Message;
        break;

      default:
        statusCode = (int)HttpStatusCode.InternalServerError;
        message = _env.IsDevelopment()
          ? exception.Message
          : "An unexpected error occurred.";
        break;
    }

    if(_env.IsDevelopment() && statusCode == (int)HttpStatusCode.InternalServerError)
    {
      errors.Add(exception.StackTrace ?? string.Empty);
    }

    context.Response.StatusCode = statusCode;

    var response = ApiResponse<object>.Fail(statusCode, message, errors);
    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    await context.Response.WriteAsync(json);
  }

}
