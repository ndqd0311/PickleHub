using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update;
using PickleHub.Catalog.Application.Features.Products.CreateImage;

namespace PickleHub.Catalog.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ProductImagesController(ISender mediator) : ControllerBase
    {
        [HttpPost("products/{productId:guid}/images")]
        public async Task<IActionResult> Create(Guid productId, [FromBody] CreateProductImageCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command with { ProductId = productId}, ct);
            return StatusCode(201, result);
        }
    }
}
