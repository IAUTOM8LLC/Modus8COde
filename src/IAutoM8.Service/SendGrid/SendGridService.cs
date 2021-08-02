using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IAutoM8.Global.Options;
using IAutoM8.Service.SendGrid.Interfaces;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace IAutoM8.Service.SendGrid
{
    public class SendGridService : ISendGridService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly SendGridOptions _sendGridOptions;
        private readonly AccountConfirmationSetting _accountConfirmationSetting;

        public SendGridService(
            ISendGridClient sendGridClient,
            IOptions<SendGridOptions> sendGridOptions,
            IOptions<AccountConfirmationSetting> accountConfirmationSetting)
        {
            _sendGridClient = sendGridClient;
            _sendGridOptions = sendGridOptions.Value;
            _accountConfirmationSetting = accountConfirmationSetting.Value;
        }

        public async Task SendMessage(string toEmail, string templateId, string subject, Dictionary<string, string> substitutions)
        {
            await SendMessage(new List<string> { toEmail }, templateId, subject, substitutions);
        }

        public async Task SendMessage(List<string> toEmails, string templateId, string subject, Dictionary<string, string> substitutions)
        {
            var sendGridMessage = new SendGridMessage();
            sendGridMessage.From = new EmailAddress(_sendGridOptions.SenderEmail);
            sendGridMessage.AddTos(toEmails.Select(t => new EmailAddress(t)).ToList());
            sendGridMessage.TemplateId = templateId;
            sendGridMessage.AddSubstitutions(substitutions);
            sendGridMessage.AddSubstitution("{{SiteUrl}}", _accountConfirmationSetting.SiteUrl);
            sendGridMessage.Subject = subject;

            await _sendGridClient.SendEmailAsync(sendGridMessage);
        }

        public async Task SendMessage(string toEmail, string body, string subject)
        {
            var sendGridMessage = new SendGridMessage();
            sendGridMessage.From = new EmailAddress(_sendGridOptions.SenderEmail);
            sendGridMessage.AddTo(toEmail);
            sendGridMessage.HtmlContent = body;
            sendGridMessage.Subject = subject;

            await _sendGridClient.SendEmailAsync(sendGridMessage);
        }
    }
}
