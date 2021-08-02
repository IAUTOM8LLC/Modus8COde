using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Domain.Models.Project
{
    public class ProjectNote
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsPublished { get; set; }
        public int? FormulaId { get; set; }
        public int? ProjectId { get; set; }
        public int? ProjectTaskId { get; set; }
        public DateTime DateCreated { get; set; }

        public int? ShareNoteParentID { get; set; }
    }
}
