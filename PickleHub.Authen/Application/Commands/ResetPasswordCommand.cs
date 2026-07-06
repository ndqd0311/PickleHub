using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Authen.Application.Commands
{
    public record ResetPasswordCommand(string Token, string NewPassword) : IRequest;

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.");
        }
    }

    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
    {
        private readonly AuthenDbContext _db;

        public ResetPasswordHandler(AuthenDbContext db)
        {
            _db = db;
        }

        public async Task Handle(ResetPasswordCommand request, CancellationToken ct)
        {
            var resetToken = await _db.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == request.Token, ct);

            if (resetToken == null || !resetToken.IsValid || resetToken.User is null)
                throw new UnauthorizedException("Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");

            resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            resetToken.IsUsed = true;

            // Thu hồi toàn bộ refresh token — buộc đăng nhập lại sau reset password
            var activeTokens = await _db.RefreshTokens
                .Where(r => r.UserId == resetToken.UserId && r.RevokedAt == null)
                .ToListAsync(ct);

            foreach (var t in activeTokens)
                t.RevokedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }
    }
}