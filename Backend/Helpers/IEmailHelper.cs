using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Helpers
{
    /// <summary>
    /// Generic Email Helper Class that has methods for sending Email to one or more Email addresses and useful functions to get Account Password Reset/Email Confirmation links.
    /// May be used with more than one third-party email provider given that the SendEmail and SendEmailToMultiple functions are adjusted.
    /// </summary>
    public interface IEmailHelper
    {
        /// <summary>
        /// Sends email to an email address
        /// </summary>
        /// <param name="Subject">Subject of the Email</param>
        /// <param name="Body">HTML Body of the Email</param>
        /// <param name="Destination">Destination of the Email</param>
        /// <returns>Result of the Sending Email Operation</returns>
        Task<bool> SendEmail(string Subject, string Body, string Destination);
        /// <summary>
        /// Sends email to multiple email addresses
        /// </summary>
        /// <param name="Subject">Subject of the Email</param>
        /// <param name="Body">Body of the Email</param>
        /// <param name="Destinations">Destinations of the Email</param>
        /// <returns>Integer Tuple with number of delivered and non-delivered emails respectively</returns>
        Task<(int, int)> SendEmailToMultiple(string Subject, string Body, List<string> Destinations);
        /// <summary>
        /// Generates Password Reset Link for an user given its user ID and Password Reset Token
        /// </summary>
        /// <param name="userId">User UUID</param>
        /// <param name="PasswordResetToken">User Password Reset Token</param>
        /// <returns></returns>
        string GetPasswordResetLink(string userId, string PasswordResetToken);
        /// <summary>
        /// Generates Email Confirmation Link for an user given its user ID and Password Reset Token
        /// </summary>
        /// <param name="UserId">User UUID</param>
        /// <param name="EmailConfirmationToken">User Email Confirmation Token</param>
        /// <returns></returns>
        string GetEmailConfirmationLink(string UserId, string EmailConfirmationToken);
    }
}
