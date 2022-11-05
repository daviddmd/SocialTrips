using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Mailjet.Client.TransactionalEmails.Response;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendAPI.Helpers
{
    public class EmailHelper : IEmailHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IMailjetClient _mailjetClient;
        private readonly string _email;
        private readonly string _bulkEmail;
        private readonly string _footer;
        private readonly string _domain;
        public EmailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _mailjetClient = new MailjetClient(_configuration["EmailSettings:ApiKey"], _configuration["EmailSettings:ApiSecret"]);
            _email = _configuration["EmailSettings:SendingEmail"];
            _bulkEmail = _configuration["EmailSettings:BulkSendingEmail"];
            _domain = configuration["Domain"];
            _footer = $"<h5>2022 <a href='{_domain}'>Viagens Sociais</a></h5>";

        }
        public string GetPasswordResetLink(string userId, string PasswordResetToken)
        {
            string PasswordConfirmationTokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(PasswordResetToken));
            return $"{_domain}/reset-password/{userId}/{PasswordConfirmationTokenEncoded}";
        }
        public string GetEmailConfirmationLink(string UserId, string EmailConfirmationToken)
        {
            string EmailConfirmationTokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(EmailConfirmationToken));
            return $"{_domain}/confirm-email/{UserId}/{EmailConfirmationTokenEncoded}";
        }
        public async Task<bool> SendEmail(string Subject, string Body,string Destination)
        {
            Body += _footer;
            TransactionalEmail email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(_email,"Viagens Sociais"))
                .WithSubject(Subject)
                .WithHtmlPart(Body)
                .WithTo(new SendContact(Destination))
                .Build();
            TransactionalEmailResponse response = await _mailjetClient.SendTransactionalEmailAsync(email);
            //Uma resposta para cada e-mail enviado
            if (response.Messages.Length == 1 && response.Messages[0].Status == "success")
            {
                return true;
            }
            return false;
        }
        //retorna um tuplo com o nº de mensagens entregues e não entregues
        public async Task<(int,int)> SendEmailToMultiple(string Subject, string Body,List<string> Destinations)
        {
            Body += _footer;
            int NumberMessagesDelivered = 0;
            int NumberMessagesNonDelivered = 0;
            IEnumerable<SendContact> SendContacts = from contact in Destinations select new SendContact(contact);
            TransactionalEmail email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(_bulkEmail,"Viagens Sociais"))
                .WithSubject(Subject)
                .WithHtmlPart(Body)
                .WithTo(SendContacts)
                .Build();
            TransactionalEmailResponse response = await _mailjetClient.SendTransactionalEmailAsync(email);
            foreach (MessageResult message in response.Messages)
            {
                if (message.Status != "success")
                {
                    NumberMessagesNonDelivered += 1;
                }
                else
                {
                    NumberMessagesDelivered += 1;
                }
            }
            return (NumberMessagesDelivered, NumberMessagesNonDelivered);
        }
    }
}
