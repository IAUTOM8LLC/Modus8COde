namespace IAutoM8.Global.Enums
{
    public enum FormulaRequestStatus : byte
    {
        Declined,
        DeclinedByOwner,
        None,
        Accepted,
        WaitingForCompanyApproval,
        AcceptedByCompanyAndSentToWorker
    }
}
