namespace IAutoM8.Global.Enums
{
    public enum StatisticType : byte
    {
        Responding,
        Working,
        Rating,
        Messaging,
        CancelAfterNudge = 6,
        Lost = 9,
        LostDueToOvertime = 10,
        JobReAssigned = 11,
        AcceptedButNotStarted = 7
    }
}
