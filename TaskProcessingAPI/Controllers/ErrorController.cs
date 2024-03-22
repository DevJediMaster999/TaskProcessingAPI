using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskProcessingAPI.Configurations;
using TaskProcessingAPI.Domain.Exceptions;

namespace TaskProcessingAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult HandleError([FromServices] IOptions<DevToolsConfig> devToolsConfig)
        {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandlerFeature == null)
                return Problem();

            var statusCode = exceptionHandlerFeature.Error switch
            {
                UserNotFoundException => StatusCodes.Status404NotFound,
                UserNameAlreadyExistsException => StatusCodes.Status400BadRequest,
                UserNameEmptyException => StatusCodes.Status400BadRequest,
                NotImplementedException => StatusCodes.Status404NotFound,
                TaskNotFoundException => StatusCodes.Status404NotFound,
                TaskEmptyDescriptionException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            if (devToolsConfig.Value.Enabled)
            {
                var errorMessage = exceptionHandlerFeature.Error switch
                {
                    UserNotFoundException => "User Not Found.",
                    UserNameAlreadyExistsException => $"The username {((UserNameAlreadyExistsException)exceptionHandlerFeature.Error).Name} is already taken. Please choose another username.",
                    TaskNotFoundException => "Task Not Found.",
                    TaskEmptyDescriptionException => "Description is required.",
                    UserNameEmptyException => "The username is required.",
                    _ => null
                };

                return Problem(
                    statusCode: statusCode,
                    detail: exceptionHandlerFeature.Error.StackTrace,
                    title: errorMessage ?? exceptionHandlerFeature.Error.Message);
            }
            return Problem(statusCode: statusCode);
        }
    }
}
