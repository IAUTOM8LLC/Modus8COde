using IAutoM8.Domain.Models;
using IAutoM8.Service.CommonDto;
using System;

namespace IAutoM8.Service.CommonService.Interfaces
{
    public interface IDateTimeService
    {
        DateTime NowUtc { get; }
        DateTime TodayUtc { get; }
        DateTime GetNextOccurence(string cron, DateTime dateTime);
        DateTime GetNextOccurence(RecurrenceAsapDto recurrenceOptions);
        DateTime MaxDate(DateTime date1, DateTime date2, DateTime date3);
        DateTime MaxDate(DateTime date1, DateTime date2);
        RecurrenceAsapDto ParseRecurrenceAsap(RecurrenceOptions recurrenceOptions, DateTime now);
    }
}
