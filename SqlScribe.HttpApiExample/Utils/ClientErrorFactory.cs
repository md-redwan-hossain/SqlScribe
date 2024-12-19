using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;

namespace SqlScribe.HttpApiExample.Utils;

public class ClientErrorFactory : IClientErrorFactory
{
    public IActionResult GetClientError(ActionContext actionContext, IClientErrorActionResult clientError)
    {
        var code = clientError.StatusCode ?? StatusCodes.Status400BadRequest;

        var result = new ApiResponse
        {
            Success = false,
            Code = code,
            Message = ReasonPhrases.GetReasonPhrase(code)
        };

        return new ObjectResult(result) { StatusCode = code };
    }
}