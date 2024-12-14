using Microsoft.AspNetCore.Mvc;
using SqlScribe.HttpApiExample.DataTransferObjects;
using SqlScribe.HttpApiExample.Services;

namespace SqlScribe.HttpApiExample.Controllers;

[ApiController]
[Route("api/books")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class BookController : ControllerBase
{
    private readonly IBookService _bookService;
    public BookController(IBookService bookService) => _bookService = bookService;

    [HttpGet]
    public async Task<IActionResult> GetBook()
    {
        var data = await _bookService.GetAllAsync();
        return ControllerContext.MakeResponse(StatusCodes.Status200OK, data);
    }

    [HttpGet("by-price-range")]
    public async Task<IActionResult> GetAggregatedBook([FromQuery] decimal lowerBound, [FromQuery] decimal upperBound)
    {
        var data = await _bookService.GetAllByPriceRangeAsync(lowerBound, upperBound, HttpContext.RequestAborted);
        return ControllerContext.MakeResponse(StatusCodes.Status200OK, data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBook(int id)
    {
        var result = await _bookService.GetOneAsync(id);

        return result.Match(
            entity => ControllerContext.MakeResponse(StatusCodes.Status200OK, entity),
            err => ControllerContext.MakeResponse(err)
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutBook(int id, BookRequest dto)
    {
        if (ModelState.IsValid is false)
        {
            return ControllerContext.MakeResponse(StatusCodes.Status400BadRequest);
        }

        var result = await _bookService.UpdateAsync(id, dto);

        return result.Match(
            entity => ControllerContext.MakeResponse(StatusCodes.Status200OK, entity),
            err => ControllerContext.MakeResponse(StatusCodes.Status304NotModified, err)
        );
    }

    [HttpPost]
    public async Task<IActionResult> PostBook(BookRequest dto)
    {
        if (ModelState.IsValid is false)
        {
            return ControllerContext.MakeResponse(StatusCodes.Status400BadRequest);
        }

        var result = await _bookService.CreateAsync(dto);

        return result.Match(
            entity => ControllerContext.MakeResponse(StatusCodes.Status201Created, entity),
            err => ControllerContext.MakeResponse(err)
        );
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var result = await _bookService.RemoveAsync(id);

        return result.Match(
            success => ControllerContext.MakeResponse(success),
            err => ControllerContext.MakeResponse(err)
        );
    }
}