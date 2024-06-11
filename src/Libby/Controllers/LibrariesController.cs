using Libby.Contracts;
using Libby.Data;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Libby.Controllers;

[Route("api/libraries")]
public sealed class LibrariesController : ControllerBase
{
    [HttpPost("")]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateLibrary model,
        [FromServices] LibbyDataContext dataContext,
        [FromServices] IPublishEndpoint publishEndpoint)
    {
        if (!Path.IsPathRooted(model.Path))
        {
            return BadRequest();
        }

        var library = new Data.Models.Library
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Path = model.Path
        };

        await dataContext.Libraries.AddAsync(library);

        await publishEndpoint.Publish(
            new LibraryCreated
            {
                LibraryId = library.Id,
                LibraryPath = library.Path
            });

        await dataContext.SaveChangesAsync();

        return Ok(new { id = library.Id });
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllAsync(
        [FromServices] LibbyDataContext dataContext)
    {
        var libraries = await dataContext.Libraries
            .OrderBy(l => l.Name)
            .ThenBy(l => l.Path)
            .ToListAsync();

        return Ok(
            libraries
                .Select(l => new
                {
                    id = l.Id,
                    path = l.Path,
                    name = l.Name
                })
                .ToList());
    }
}
