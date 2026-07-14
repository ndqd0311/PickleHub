using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Application.DTOs;
using PickleHub.Authen.Domain.Entities;
using PickleHub.Authen.Domain.Interfaces.Repositories;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Authen.Infrastructure.Service;
using PickleHub.Common.Exceptions;

namespace PickleHub.Authen.Application.Features.Auth.Register
{
    public record RegisterCommand(string Email, string Password) : IRequest<AuthResultDto>;

    public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenService _jwtService;
        private readonly IConfiguration _config;

        public RegisterHandler(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            JwtTokenService jwtService,
            IConfiguration config)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _config = config;
        }

        public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken ct)
        {
            var existed = await _userRepository.ExistsByEmailAsync(request.Email, ct);
            if (existed)
                throw new ConflictException("Email này đã được đăng ký.");

            var user = User.Create(
                request.Email,
                BCrypt.Net.BCrypt.HashPassword(request.Password));

            _userRepository.Add(user);

            var refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"]!);
            var refreshTokenValue = _jwtService.GenerateRefreshTokenValue();
            var refreshToken = RefreshToken.Create(user.Id, refreshTokenValue, refreshDays);

            _refreshTokenRepository.Add(refreshToken);
            await _unitOfWork.SaveChangesAsync(ct);

            return new AuthResultDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = _jwtService.GenerateAccessToken(user),
                RefreshToken = refreshTokenValue
            };
        }
    }
}
