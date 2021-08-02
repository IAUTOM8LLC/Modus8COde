using IAutoM8.Global.Enums;
using System;

namespace IAutoM8.Service.CommonDto
{
    public class RecurrenceOptionsDto
    {
        public string Cron { get; set; }
        public CronTab CronTab { get; set; }
        public int MaxOccurrences { get; set; }
        public FormulaTaskRecurrenceType RecurrenceType { get; set; }
        public DateTime? EndRecurrenceDate { get; set; }
        public byte DayDiff { get; set; }
        public bool IsAsap { get; set; }
    }
}
