using System;

namespace IAutoM8.Domain.Models.Project.Task
{
    public class ProjectTaskComment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime DateCreated { get; set; }


        #region Foreign Keys
        public int ProjectTaskId { get; set; }
        public Guid UserGuid { get; set; }

        #endregion

        #region Navigation Properties

        public virtual ProjectTask ProjectTask { get; set; }
        public virtual User.User User { get; set; }

        #endregion
    }
}
