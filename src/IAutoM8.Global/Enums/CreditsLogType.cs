using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Global.Enums
{
    public enum CreditsLogType
    {
        Charge,
        VendorAcceptRequest,
        CompleteTask,
        ConfirmPayment,
        StopOutsource
    }

    public enum TransferRequestEnum
    {
        Requested,
        InProcess,
        Processed,
        Declined,
    }
}
