using IAutoM8.Service.Users.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.ProjectTasks.Dto
{
    public class HomeDashboardDto
    {
        public IList<HomeListViewTaskDto> UserTasks { get; set; }
        public IList<UserFilterItemDto> UsersList { get; set; }
        public IList<UserFilterItemDto> OutsourceList { get; set; }
    }
}
