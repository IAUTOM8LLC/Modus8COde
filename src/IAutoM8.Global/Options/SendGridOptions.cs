using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Global.Options
{
    public class SendGridOptions
    {
        public string Server { get; set; }
        public string SenderEmail { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string ApiKey { get; set; }
    }
}
