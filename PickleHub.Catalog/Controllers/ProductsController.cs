using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleHub.Catalog.Application.Features.Products.CreateProduct;
using PickleHub.Catalog.Application.Features.Products.DeleteProduct;
using PickleHub.Catalog.Application.Features.Products.GetProducts;
using PickleHub.Catalog.Application.Features.Products.PublishProduct;
using PickleHub.Catalog.Application.Features.Products.RestoreProduct;
using PickleHub.Catalog.Application.Features.Products.UpdateProduct;

namespace PickleHub.Catalog.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController(ISender mediator) : ControllerBase
    {
        //public
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] GetProductsQuery query, CancellationToken ct)
        {
            var result = await mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet("{value}")]
        public async Task<IActionResult> GetBySlugOrId(string value, CancellationToken ct)
        {
            if (Guid.TryParse(value, out var id))
                return Ok(await mediator.Send(new GetProductByIdQuery(id), ct));

            return Ok(await mediator.Send(new GetProductBySlugQuery(value), ct));
        }

        //admin
        [HttpGet("~/admin/products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Search([FromQuery] GetAdminProductsQuery query, CancellationToken ct)
        {
            var result = await mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command, ct);
            return CreatedAtAction(nameof(GetBySlugOrId), new { value = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command with { Id = id }, ct);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await mediator.Send(new DeleteProductCommand(id), ct);
            return NoContent();
        }

        [HttpPatch("{id:guid}/publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
        {
            await mediator.Send(new PublishProductCommand(id), ct);
            return NoContent();
        }

        [HttpPatch("{id:guid}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
        {
            await mediator.Send(new RestoreProductCommand(id), ct);
            return NoContent();
        }
    }
}
