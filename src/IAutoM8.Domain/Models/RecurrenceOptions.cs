using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Domain.Models
{
    public class RecurrenceOptions
    {
        public int Id { get; set; }
        public string Cron { get; set; }
        public CronTab CronTab { get; set; }
        public int MaxOccurrences { get; set; }
        public int Occurrences { get; set; }
        public FormulaTaskRecurrenceType RecurrenceType { get; set; }
        public DateTime? EndRecurrenceDate { get; set; }
        public DateTime? NextOccurenceDate { get; set; }
        public byte DayDiff { get; set; }
        public bool IsAsap { get; set; }
    }
}
