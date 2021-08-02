using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Global.Options
{
    public class EmailTemplates
    {
        public string SignIn { get; set; }
        public string SignUp { get; set; }
        public string EmailNotification { get; set; }
        public string EmailNotificationExtended { get; set; }
        public string DailyMorningSummary { get; set; }
        public string DailyEveningSummary { get; set; }
        public string DailyOwnerEveningSummary { get; set; }
        public string VendorInvitationToBid { get; set; }
        public string VendorInvitationToWork { get; set; }
        public string VendorStartToWork { get; set; }
        public string TimeToInviteVendor { get; set; }
        public string TransferRequest { get; set; }       
        public string PublishNotification { get; set; }
        public string CompanyWorkerInvitation { get; set; }//Added On client request 19-04-2021 for sending company worker invitation email

    }
}
