using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Authen.Application.Commands
{
    public record LogoutCommand(string RefreshToken) : IRequest;

    public class LogoutHandler : IRequestHandler<LogoutCommand>
    {
        private readonly AuthenDbContext _db;

        public LogoutHandler(AuthenDbContext db)
        {
            _db = db;
        }

        public async Task Handle(LogoutCommand request, CancellationToken ct)
        {
            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, ct);

            if (token is null || !token.IsActive)
                throw new UnauthorizedException("Refresh token không hợp lệ.");

            token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}