using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Teams.Dto
{
    public class UpdateTeamDetailDto
    {
        public int Id { get; set; }
        public string TeamName { get; set; }
        public bool IsGlobal { get; set; }
        public List<int> TeamSkills { get; set; }
    }
}
