using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleHub.Customers.Application.Features.Customers.BlockCustomer;
using PickleHub.Customers.Application.Features.Customers.GetCustomerDetail;
using PickleHub.Customers.Application.Features.Customers.GetCustomers;
using PickleHub.Customers.Application.Features.Customers.GetDashboardSummary;
using PickleHub.Customers.Application.Features.Customers.GetMe;
using PickleHub.Customers.Application.Features.Customers.UpdateMe;

namespace PickleHub.Customers.Controllers
{
    [ApiController]
    [Route("customers")]
    public class CustomersController(ISender mediator) : ControllerBase
    {
        // Customer tự xem/sửa thông tin

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe(CancellationToken ct)
        {
            var result = await mediator.Send(new GetMeQuery(), ct);
            return Ok(result);
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMe(
            [FromBody] UpdateMeCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command, ct);
            return Ok(result);
        }

        // Admin quản lý 

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] GetCustomersQuery query, CancellationToken ct)
        {
            var result = await mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet("{customerId:guid}/detail")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDetail(Guid customerId, CancellationToken ct)
        {
            var result = await mediator.Send(new GetCustomerDetailQuery(customerId), ct);
            return Ok(result);
        }

        [HttpPatch("{customerId:guid}/block")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Block(
            Guid customerId, [FromBody] BlockCustomerRequest body, CancellationToken ct)
        {
            await mediator.Send(new BlockCustomerCommand(customerId, body.IsBlocked), ct);
            return NoContent();
        }

        [HttpGet("dashboard/summary")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DashboardSummary(CancellationToken ct)
        {
            var result = await mediator.Send(new GetDashboardSummaryQuery(), ct);
            return Ok(result);
        }

        // Internal — dùng cho service khác call sang

        [HttpGet("{customerId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid customerId, CancellationToken ct)
        {
            var result = await mediator.Send(new GetCustomerDetailQuery(customerId), ct);
            return Ok(result);
        }
    }

    public record BlockCustomerRequest(bool IsBlocked);
}
