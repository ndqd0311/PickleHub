using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleHub.Catalog.Application;
using PickleHub.Catalog.Application.Features.ProductFeature.GetProducts;
using PickleHub.Catalog.Application.Features.Products.CreateProduct;
using PickleHub.Catalog.Application.Features.Products.DeleteProduct;
using PickleHub.Catalog.Application.Features.Products.UpdateProduct;

namespace PickleHub.Catalog.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController(ISender mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] GetProductsQuery query, CancellationToken ct = default)
        {
            var result = await mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await mediator.Send(new GetProductByIdQuery(id),ct);      
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command with { Id=id}, ct);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await mediator.Send(new DeleteProductCommand(id), ct);
            return NoContent();
        }
    }
}
