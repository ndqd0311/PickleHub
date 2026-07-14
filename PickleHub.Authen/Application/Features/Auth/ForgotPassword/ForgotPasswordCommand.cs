using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Domain.Entities;
using PickleHub.Authen.Domain.Interfaces.Repositories;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Authen.Infrastructure.Service;

namespace PickleHub.Authen.Application.Features.Auth.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : IRequest;
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public ForgotPasswordHandler(
            IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IUnitOfWork unitOfWork,
            JwtTokenService jwtService,
            IEmailService emailService,
            IConfiguration config)
        {
            _userRepository = userRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _emailService = emailService;
            _config = config;
        }

        public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, ct);

            if (user == null || user.IsBlocked) return;

            // Vô hiệu hóa các token cũ chưa dùng
            var oldTokens = await _passwordResetTokenRepository
                .GetActiveByUserIdAsync(user.Id, ct);

            foreach (var old in oldTokens)
                old.MarkAsUsed();

            var tokenValue = _jwtService.GeneratePasswordResetToken();
            var resetToken = PasswordResetToken.Create(user.Id, tokenValue);

            _passwordResetTokenRepository.Add(resetToken);
            await _unitOfWork.SaveChangesAsync(ct);

            var resetLink = $"{_config["App:BaseUrl"]}/reset-password?token={tokenValue}";
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink, ct);
        }
    }
}