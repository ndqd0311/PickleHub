using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleHub.System.Application.Features.Announcements.CreateAnnouncement;
using PickleHub.System.Application.Features.Announcements.DeleteAnnouncement;
using PickleHub.System.Application.Features.Announcements.GetActiveAnnouncements;
using PickleHub.System.Application.Features.Announcements.GetAnnouncements;
using PickleHub.System.Application.Features.Announcements.UpdateAnnouncement;

namespace PickleHub.System.Controllers
{
    [ApiController]
    [Route("system/announcements")]
    public class AnnouncementsController(ISender mediator) : ControllerBase
    {
        // Public — frontend đọc để hiển thị banner
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var result = await mediator.Send(new GetActiveAnnouncementsQuery(), ct);
            return Ok(result);
        }

        // Admin only 
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await mediator.Send(new GetAnnouncementsQuery(), ct);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromBody] CreateAnnouncementCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command, ct);
            return Created(string.Empty, result);
        }

        [HttpPut("{announcementId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            Guid announcementId,
            [FromBody] UpdateAnnouncementCommand command,
            CancellationToken ct)
        {
            var result = await mediator.Send(command with { AnnouncementId = announcementId }, ct);
            return Ok(result);
        }

        [HttpDelete("{announcementId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid announcementId, CancellationToken ct)
        {
            await mediator.Send(new DeleteAnnouncementCommand(announcementId), ct);
            return NoContent();
        }
    }
}
