using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddSPAssignSortListOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sqlQuery = @"
CREATE PROCEDURE [dbo].[uspAssignSkillSortOrder]
@formulaid int
AS
BEGIN

;with cte_1 as
(
select ParentTaskId, cast(ParentTaskId as varchar(10) ) as rnk  from formulataskdependency
where ChildTaskId in (select ft.id
					from formulaproject fp
					inner join formulatask ft on fp.id = ft.formulaprojectid
					where fp.id = @formulaid)

union all

select ftd.ChildTaskId,  cast(cte_1.rnk +'-'+ cast(ROW_NUMBER() over (partition by ftd.parenttaskid order by ftd.childtaskid) as varchar(10) )  as varchar(10)) 
 from FormulaTaskDependency ftd
inner join cte_1 on cte_1.ParentTaskId = ftd.ParentTaskId

)
select ParentTaskId as Taskid,	rnk,	rnk2
 from 
(select  *,row_number() over (partition by parenttaskid order by rnk) as rnk2 
from cte_1
) a
where rnk2 = 1
order by 2

end";
            migrationBuilder.Sql(sqlQuery);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[uspAssignSkillSortOrder];");
        }
    }
}
