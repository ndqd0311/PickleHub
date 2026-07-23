using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Domain.Interfaces.Repositories;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;
using System.Web;

namespace PickleHub.Authen.Application.Features.Auth.ResetPassword
{
    public record ResetPasswordCommand(string Token, string NewPassword) : IRequest;

    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
    {
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordHandler(
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork)
        {
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ResetPasswordCommand request, CancellationToken ct)
        {
            // Decode URL-encoded token (in case it's copied from email link with %2F, %2B, %3D etc)
            var decodedToken = HttpUtility.UrlDecode(request.Token);
            var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(decodedToken, ct);

            if (resetToken is null || !resetToken.IsValid)
                throw new UnauthorizedException("Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");

            resetToken.User!.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
            resetToken.MarkAsUsed();

            // Thu hồi toàn bộ refresh token — buộc đăng nhập lại
            var activeTokens = await _refreshTokenRepository
                .GetActiveByUserIdAsync(resetToken.UserId, ct);

            foreach (var t in activeTokens)
                t.Revoke();

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}