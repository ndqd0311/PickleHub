using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleHub.Catalog.Application.Features.Products.CreateVariant;

namespace PickleHub.Catalog.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ProductVariantsController( ISender mediator) : ControllerBase
    {
        [HttpPost("products/{productId:guid}/variants")]
        public async Task<IActionResult> Create(Guid productId, [FromBody] CreateProductVariantCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command with { ProductId = productId}, ct);
            return Created(string.Empty, result);
        }
    }
}
