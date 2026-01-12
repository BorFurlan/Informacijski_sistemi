using Microsoft.AspNetCore.Identity.UI.Services;

namespace FinFriend.Services
{
    /// <summary>
    /// A no-op email sender implementation for development.
    /// Email confirmation is disabled, so this doesn't actually send emails.
    /// </summary>
    public class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // No-op: do nothing since email confirmation is disabled
            return Task.CompletedTask;
        }
    }
}
