namespace PickleHub.Authen.Infrastructure.Service
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken ct);
    }
}