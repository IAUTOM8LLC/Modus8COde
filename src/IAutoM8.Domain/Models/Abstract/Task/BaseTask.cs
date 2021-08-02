using System;

namespace IAutoM8.Domain.Models.Abstract.Task
{
    public abstract class BaseTask
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public bool IsAutomated { get; set; }
        public bool IsInterval { get; set; }
        public int? Duration { get; set; }
        [Obsolete("Remove after all resource data is imported to neo4j")]
        public bool IsShareResources { get; set; }

        public int PosX { get; set; }
        public int PosY { get; set; }
        public int StartDelay { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
       
    }
}
