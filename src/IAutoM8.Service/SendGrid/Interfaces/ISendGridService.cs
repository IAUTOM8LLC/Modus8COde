using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IAutoM8.Service.SendGrid.Interfaces
{
    public interface ISendGridService
    {
        Task SendMessage(string toEmail, string body, string subject);
        Task SendMessage(string toEmail, string templateId, string subject, Dictionary<string, string> substitutions);
        Task SendMessage(List<string> toEmails, string templateId, string subject, Dictionary<string, string> substitutions);
    }
}
