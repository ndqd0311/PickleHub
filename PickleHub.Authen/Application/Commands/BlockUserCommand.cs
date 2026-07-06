using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Authen.Application.Commands
{
    public record BlockUserCommand(Guid UserId, bool IsBlocked) : IRequest;

    public class BlockUserHandler : IRequestHandler<BlockUserCommand>
    {
        private readonly AuthenDbContext _db;

        public BlockUserHandler(AuthenDbContext db)
        {
            _db = db;
        }

        public async Task Handle(BlockUserCommand request, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user is null)
                throw new NotFoundException("Không tìm thấy người dùng.");

            if (user.Role == Domain.Enums.UserRole.Admin)
                throw new ForbiddenException("Không thể block tài khoản Admin.");

            user.IsBlocked = request.IsBlocked;

            // Nếu block → thu hồi toàn bộ refresh token ngay lập tức
            if (request.IsBlocked)
            {
                var activeTokens = await _db.RefreshTokens
                    .Where(r => r.UserId == user.Id && r.RevokedAt == null)
                    .ToListAsync(ct);

                foreach (var t in activeTokens)
                    t.RevokedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(ct);
        }
    }
}