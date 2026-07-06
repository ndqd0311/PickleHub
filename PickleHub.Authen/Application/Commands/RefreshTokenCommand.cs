using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Application.DTOs;
using PickleHub.Authen.Domain.Entities;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Authen.Infrastructure.Service;
using PickleHub.Common.Exceptions;

namespace PickleHub.Authen.Application.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDto>;

    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
    {
        private readonly AuthenDbContext _db;
        private readonly JwtTokenService _jwtService;
        private readonly IConfiguration _config;

        public RefreshTokenHandler(AuthenDbContext db, JwtTokenService jwtService, IConfiguration config)
        {
            _db = db;
            _jwtService = jwtService;
            _config = config;
        }

        public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken ct)
        {
            var existingToken = await _db.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, ct);

            if (existingToken is null || !existingToken.IsActive || existingToken.User is null)
            {
                throw new UnauthorizedException( "Refresh token không hợp lệ hoặc đã hết hạn.");
            }

            existingToken.RevokedAt = DateTime.UtcNow;

            var newAccessToken = _jwtService.GenerateAccessToken(existingToken.User);
            var newRefreshTokenValue = _jwtService.GenerateRefreshTokenValue();
            var refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"]!);

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = existingToken.UserId,
                Token = newRefreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
            });

            await _db.SaveChangesAsync(ct);

            return new AuthResultDto
            {
                UserId = existingToken.User.Id,
                Email = existingToken.User.Email,
                Role = existingToken.User.Role.ToString(),
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue
            };
        }
    }
}
