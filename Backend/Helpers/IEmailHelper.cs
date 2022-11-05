using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Helpers
{
    public interface IEmailHelper
    {
        Task<bool> SendEmail(string Subject, string Body, string Destination);
        Task<(int, int)> SendEmailToMultiple(string Subject, string Body, List<string> Destinations);
        string GetPasswordResetLink(string userId, string PasswordResetToken);
        string GetEmailConfirmationLink(string UserId, string EmailConfirmationToken);
    }
}
