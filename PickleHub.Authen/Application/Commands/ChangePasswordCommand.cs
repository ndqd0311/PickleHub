using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Authen.Application.Commands
{
    public record ChangePasswordCommand(
        Guid UserId,     
        string OldPassword,
        string NewPassword
    ) : IRequest;

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("Vui lòng nhập mật khẩu hiện tại.");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8).WithMessage("Mật khẩu mới phải có ít nhất 8 ký tự.")
                .NotEqual(x => x.OldPassword).WithMessage("Mật khẩu mới không được trùng mật khẩu cũ.");
        }
    }

    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand>
    {
        private readonly AuthenDbContext _db;

        public ChangePasswordHandler(AuthenDbContext db)
        {
            _db = db;
        }

        public async Task Handle(ChangePasswordCommand request, CancellationToken ct)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng.");

            if (user.IsBlocked)
                throw new ForbiddenException("Tài khoản đã bị khóa.");

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                throw new UnauthorizedException("Mật khẩu hiện tại không đúng.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Thu hồi toàn bộ refresh token → buộc đăng nhập lại
            var activeTokens = await _db.RefreshTokens
                .Where(r => r.UserId == user.Id && r.RevokedAt == null)
                .ToListAsync(ct);

            foreach (var t in activeTokens)
                t.RevokedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }
    }
}