namespace IAutoM8.Global.Enums
{
    public enum ProjectRequestStatus : byte
    {
        Declined,
        AcceptedByOther,
        DeclinedByOwner,
        None,
        Send,
        Accepted,
        CancelAfterNudge = 6,
        JobInviteEnqueue = 7,
        Lost = 9,
        LostDueToOvertime = 10
    }
}
