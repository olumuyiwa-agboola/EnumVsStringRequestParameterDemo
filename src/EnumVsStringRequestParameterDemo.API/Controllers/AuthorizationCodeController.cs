using System.Net;
using Microsoft.AspNetCore.Mvc;
using EnumVsStringRequestParameterDemo.API.Models;

namespace EnumVsStringRequestParameterDemo.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthorizationCodeController : ControllerBase
    {
        [HttpPost]
        [Route("Send")]
        public async Task<IActionResult> SendAuthorizationCode(SendAuthorizationCodeRequest request)
        {
            var validator = new SendAuthorizationCodeRequestValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                Dictionary<string, string> validationFailures = [];
                foreach (var failure in validationResult.Errors)
                {
                    if (validationFailures.ContainsKey(failure.PropertyName))
                        validationFailures[failure.PropertyName] += " | " + failure.ErrorMessage;
                    else
                        validationFailures[failure.PropertyName] = failure.ErrorMessage;
                }

                var validationProblemDetails = new ValidationProblemDetails()
                {
                    Instance = null,
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "One or more validations failed.",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                };

                validationFailures.ToList().ForEach(failure =>
                    validationProblemDetails.Errors.Add(failure.Key, [failure.Value])
                );

                return StatusCode((int)HttpStatusCode.BadRequest, validationProblemDetails);
            }

            return StatusCode((int)HttpStatusCode.OK, new SendAuthorizationCodeResponse()
            {
                UserIdentifier = request.UserIdentifier,
            });
        }
    }
}
