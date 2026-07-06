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
    public record LoginCommand(string Email, string Password) : IRequest<AuthResultDto>;

    public class LoginHandler : IRequestHandler<LoginCommand, AuthResultDto>
    {
        private readonly AuthenDbContext _db;
        private readonly JwtTokenService _jwtService;
        private readonly IConfiguration _config;

        public LoginHandler(AuthenDbContext db, JwtTokenService jwtService, IConfiguration config)
        {
            _db = db;
            _jwtService = jwtService;
            _config = config;
        }

        public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
 
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Email hoặc mật khẩu không đúng.");
            }

            if (!user.IsBlocked)
            {
                throw new ForbiddenException("Tài khoản đã bị khóa.");
            }

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshTokenValue = _jwtService.GenerateRefreshTokenValue();
            var refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"]!);

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
            });

            await _db.SaveChangesAsync(ct);

            return new AuthResultDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue
            };
        }
    }
}
