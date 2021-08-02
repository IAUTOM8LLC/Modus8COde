using System.Collections.Generic;

namespace IAutoM8.Domain.Models.Abstract.Task
{
    public abstract class TaskCondition<T, TOption>
    {
        public int Id { get; set; }
        public string Condition { get; set; }
        public virtual T Task { get; set; }
        public virtual ICollection<TOption> Options { get; set; } = new List<TOption>();
    }
}
