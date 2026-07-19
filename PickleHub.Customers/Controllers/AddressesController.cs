using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleHub.Customers.Application.Features.Address.AddAdress;
using PickleHub.Customers.Application.Features.Address.DeleteAddress;
using PickleHub.Customers.Application.Features.Address.GetAddressById;
using PickleHub.Customers.Application.Features.Address.GetMyAddress;
using PickleHub.Customers.Application.Features.Address.SetDefaultAddress;
using PickleHub.Customers.Application.Features.Address.UpdateAddress;

namespace PickleHub.Customers.Controllers
{
    [ApiController]
    [Route("customers")]
    [Authorize]
    public class AddressesController(ISender mediator) : ControllerBase
    {
        [HttpGet("me/addresses")]
        public async Task<IActionResult> GetMyAddresses(CancellationToken ct)
        {
            var result = await mediator.Send(new GetMyAddressesQuery(), ct);
            return Ok(result);
        }

        [HttpPost("me/addresses")]
        public async Task<IActionResult> Add(
            [FromBody] AddAddressCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command, ct);
            return Created(string.Empty, result);
        }

        [HttpPut("me/addresses/{addressId:guid}")]
        public async Task<IActionResult> Update(
            Guid addressId, [FromBody] UpdateAddressCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command with { AddressId = addressId }, ct);
            return Ok(result);
        }

        [HttpDelete("me/addresses/{addressId:guid}")]
        public async Task<IActionResult> Delete(Guid addressId, CancellationToken ct)
        {
            await mediator.Send(new DeleteAddressCommand(addressId), ct);
            return NoContent();
        }

        [HttpPatch("me/addresses/{addressId:guid}/set-default")]
        public async Task<IActionResult> SetDefault(Guid addressId, CancellationToken ct)
        {
            var result = await mediator.Send(new SetDefaultAddressCommand(addressId), ct);
            return Ok(result);
        }

        // Internal — Order Service gọi để lấy địa chỉ snapshot
        [HttpGet("addresses/{addressId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAddressById(Guid addressId, CancellationToken ct)
        {
            var result = await mediator.Send(new GetAddressByIdQuery(addressId), ct);
            return Ok(result);
        }
    }
}
