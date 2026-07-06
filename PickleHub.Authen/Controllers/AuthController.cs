using MediatR;
using Microsoft.AspNetCore.Mvc;
using PickleHub.Authen.Application.Commands;
using PickleHub.Common.Exceptions;

namespace PickleHub.Authen.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController(ISender mediator) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command, ct);
            return Created(string.Empty, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken ct)
        {
            await mediator.Send(command, ct);
            return NoContent();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
        {
            await mediator.Send(command, ct);  
            return Ok(new { message = "Nếu email tồn tại, link đặt lại mật khẩu đã được gửi." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
        {
            await mediator.Send(command, ct);
            return Ok(new { message = "Đặt lại mật khẩu thành công." });
        }

        // Admin only — gọi từ Gateway sau khi đã validate JWT + role
        [HttpPatch("users/{userId:guid}/block")]
        public async Task<IActionResult> BlockUser(Guid userId, [FromBody] BlockUserCommand command, CancellationToken ct)
        {
            await mediator.Send(new BlockUserCommand(userId, command.IsBlocked), ct);
            return NoContent();
        }

        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword(
                    [FromBody] ChangePasswordRequest command, CancellationToken ct)
        {
            // Gateway đã validate JWT và forward X-User-Id
            var userIdHeader = HttpContext.Request.Headers["X-User-Id"].FirstOrDefault();

            if (string.IsNullOrEmpty(userIdHeader) || !Guid.TryParse(userIdHeader, out var userId))
                throw new UnauthorizedException("Không xác định được người dùng.");

            await mediator.Send(new ChangePasswordCommand(userId, command.OldPassword, command.NewPassword), ct);
            return NoContent();
        }

        public record ChangePasswordRequest(string OldPassword, string NewPassword);

    }

}

