using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SharpOutcome.Helpers.Contracts;
using SharpOutcome.Helpers.Enums;
using SqlScribe.HttpApiExample.Utils;

namespace SqlScribe.HttpApiExample.Controllers;

public static class ControllerExtensions
{
    public static IActionResult MakeResponse(this ActionContext context, IGoodOutcome success)
    {
        var code = success.Tag switch
        {
            GoodOutcomeTag.Ok => StatusCodes.Status200OK,
            GoodOutcomeTag.Created => StatusCodes.Status201Created,
            GoodOutcomeTag.Deleted => StatusCodes.Status204NoContent,
            _ => StatusCodes.Status200OK
        };

        return context.MakeResponse(code, null, success.Reason);
    }


    public static IActionResult MakeResponse(this ActionContext context, IBadOutcome error)
    {
        var code = error.Tag switch
        {
            BadOutcomeTag.Failure => StatusCodes.Status500InternalServerError,
            BadOutcomeTag.Unexpected => StatusCodes.Status500InternalServerError,
            BadOutcomeTag.BadRequest => StatusCodes.Status400BadRequest,
            BadOutcomeTag.Conflict => StatusCodes.Status409Conflict,
            BadOutcomeTag.NotFound => StatusCodes.Status404NotFound,
            BadOutcomeTag.Unauthorized => StatusCodes.Status401Unauthorized,
            BadOutcomeTag.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return context.MakeResponse(code, null, error.Reason);
    }


    public static IActionResult MakeResponse(this ActionContext context, int code, object? data = null,
        string? message = null)
    {
        if (code == StatusCodes.Status204NoContent) return new NoContentResult();

        var isSuccess = code is >= 200 and < 300;
        var res = new ApiResponse
        {
            Success = isSuccess,
            Message = message ?? ReasonPhrases.GetReasonPhrase(code),
            Code = code,
            Data = data
        };

        return new ObjectResult(res) { StatusCode = code };
    }
}