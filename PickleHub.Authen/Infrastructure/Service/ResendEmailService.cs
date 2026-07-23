using Resend;
using System.Net.Mail;

namespace PickleHub.Authen.Infrastructure.Service
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _config;

        public ResendEmailService(IResend resend, IConfiguration config)
        {
            _resend = resend;
            _config = config;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken ct)
        {
            var message = new EmailMessage
            {
                From = _config["Resend:FromEmail"]!,
                To = { toEmail },
                Subject = "Đặt lại mật khẩu PickleHub",
                HtmlBody = $"""
                    <h2>Đặt lại mật khẩu</h2>
                    <p>Bấm vào link bên dưới để đặt lại mật khẩu. Link có hiệu lực trong <strong>15 phút</strong>.</p>
                    <p><a href="{resetLink}">{resetLink}</a></p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.</p>
                """
            };

            await _resend.EmailSendAsync(message, ct);
        }
    }
}