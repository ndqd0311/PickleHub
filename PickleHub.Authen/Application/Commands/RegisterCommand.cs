using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Application.DTOs;
using PickleHub.Authen.Domain.Entities;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Authen.Infrastructure.Service;
using PickleHub.Common.Exceptions;

namespace PickleHub.Authen.Application.Commands
{
    public record RegisterCommand(string Email, string Password) : IRequest<AuthResultDto>;

    public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResultDto>
    {
        private readonly AuthenDbContext _db;
        private readonly JwtTokenService _jwtService;
        private readonly IConfiguration _config;

        public RegisterHandler(AuthenDbContext db, JwtTokenService jwtService, IConfiguration config)
        {
            _db = db;
            _jwtService = jwtService;
            _config = config;
        }

        public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken ct)
        {
            var existed = await _db.Users.AnyAsync(u => u.Email == request.Email, ct);

            if (existed)
            {
                throw new ConflictException( "Email này đã được đăng ký.");
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _db.Users.Add(user);

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshTokenValue();
            var refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"]!);

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
            });

            await _db.SaveChangesAsync(ct);

            return new AuthResultDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
