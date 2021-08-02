using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Users.Dto
{
    public class BalanceDto
    {
        public decimal Unpaid { get; set; }
        public decimal Expected { get; set; }
        public decimal Total { get; set; }
        public DateTime? LastTransfer { get; set; }
        public IEnumerable<TasksWithPriceDto> Tasks { get; set; }
        public int FinishedTasksCount { get; set; }
        public decimal RequestedAmount { get; set; }
        public bool PayoneerEmailAvailable { get; set; }
    }
}
