using IAutoM8.Domain.Models;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using NCrontab.Advanced;
using NCrontab.Advanced.Enumerations;
using System;

namespace IAutoM8.Service.CommonService
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime NowUtc
            => DateTime.Now.ToUniversalTime();

        public DateTime TodayUtc
            => DateTime.Today.ToUniversalTime();

        public DateTime MaxDate(DateTime date1, DateTime date2, DateTime date3)
            => MaxDate(date3, MaxDate(date1, date2));

        public DateTime MaxDate(DateTime date1, DateTime date2)
            => date1 > date2 ? date1 : date2;

        public DateTime GetNextOccurence(string cron, DateTime dateTime)
            => CrontabSchedule.Parse(cron, CronStringFormat.WithYears).GetNextOccurrence(dateTime);

        public DateTime GetNextOccurence(RecurrenceAsapDto recurrenceOptions)
            => CrontabSchedule.Parse(recurrenceOptions.Cron, CronStringFormat.WithYears).GetNextOccurrence(recurrenceOptions.StartFrom);

        public RecurrenceAsapDto ParseRecurrenceAsap(RecurrenceOptions recurrenceOptions, DateTime now)
        {
            var cron = recurrenceOptions.Cron;
            var sartTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            
            if (recurrenceOptions.IsAsap)
            {
                var cronParts = recurrenceOptions.Cron.Split(" ");
                cronParts[0] = now.Minute.ToString();
                cronParts[1] = now.Hour.ToString();
                cron = string.Join(" ", cronParts);
                sartTime = sartTime.AddSeconds(-1);
            }
            return new RecurrenceAsapDto
            {
                Cron = cron,
                StartFrom = sartTime
            };
        }
    }
}
