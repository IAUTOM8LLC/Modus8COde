namespace IAutoM8.Global.Enums
{
    public enum ActivityType : short
    {
        New = 0,
        InProgress = 1,
        NeedsReview = 2,
        Completed = 3,
        AcceptReview = 4,
        DeclineReview = 5,
        Processing = 6,
        Reviewing = 7,
        Overdue = 8,
        Deadline = 9,
        AssignWorkingSkill = 10,
        AssignReviewingSkill = 11,
        UpdateProcessingUser = 12
    }
}
