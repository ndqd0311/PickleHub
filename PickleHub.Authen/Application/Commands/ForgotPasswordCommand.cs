using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Domain.Entities;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Authen.Infrastructure.Service;

namespace PickleHub.Authen.Application.Commands
{
    public record ForgotPasswordCommand(string Email) : IRequest;

    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand>
    {
        private readonly AuthenDbContext _db;
        private readonly JwtTokenService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public ForgotPasswordHandler(
            AuthenDbContext db,
            JwtTokenService jwtService,
            IEmailService emailService,
            IConfiguration config)
        {
            _db = db;
            _jwtService = jwtService;
            _emailService = emailService;
            _config = config;
        }

        public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

            if (user == null || user.IsBlocked) return;

            // Vô hiệu hoá các token reset cũ chưa dùng
            var oldTokens = await _db.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);

            foreach (var old in oldTokens)
                old.IsUsed = true;

            var tokenValue = _jwtService.GeneratePasswordResetToken();
            var expiryMinutes = 15;

            _db.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                Token = tokenValue,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
            });

            await _db.SaveChangesAsync(ct);

            var resetLink = $"{_config["App:BaseUrl"]}/reset-password?token={tokenValue}";
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink, ct);
        }
    }
}