using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class Added_sp_USPVENDORJOBINVITES : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE [dbo].[uspGetPendingJobInvitesForVendor] @VendorGuid [uniqueidentifier] AS 
SELECT DISTINCT
   FP.Id AS FORMULAID,
   FP.Name AS FormulaName,
   PT.Id AS TASKID,
   PT.Title AS TaskName,
   S.Id AS SKILLID,
   S.Name AS SkillName,
   T.Name AS TeamName,
   PT.StartDate as StartDate,
   DATEADD(MINUTE, PT.Duration, PT.StartDate) AS DUEDATE,
   CAST((CAST(PT.DURATION AS decimal(15, 2)) / 60) AS DECIMAL(15, 2) ) AS DurationHours,
   DATEDIFF(HOUR, CAST(FTS.Created AS DATETIME), GETDATE()) as TimeLeftHours,
   ptv.Id as ProjectTaskVendorId,
   FTS.Created as SentOn
FROM
   FormulaTaskStatistic FTS 
   INNER JOIN
      ProjectTask PT 
      ON FTS.ProjectTaskId = PT.Id 
   INNER JOIN
      ProjectTaskVendor ptv 
      on pt.Id = ptv.ProjectTaskId 
      AND ptv.VendorGuid = fts.VendorGuid 
   INNER JOIN
      Project P 
      ON PT.ProjectId = P.Id 
   INNER JOIN
      FormulaTask FT 
      ON PT.FormulaTaskId = FT.Id 
   INNER JOIN
      FormulaProject FP 
      ON FT.FormulaProjectId = FP.Id 
   LEFT JOIN
      SKILL S 
      on FT.AssignedSkillId = s.Id 
   LEFT JOIN
      TEAM T 
      ON FT.TeamId = T.Id 
WHERE
   FTS.Type = 0 
   AND FTS.Completed IS NULL 
   --AND NOT EXISTS 
   --(
   --   SELECT DISTINCT
   --      * 
   --   FROM
   --      FormulaTaskStatistic FTS2 
   --   WHERE
   --      FTS.VendorGuid = FTS2.VendorGuid 
   --      AND FTS.ProjectTaskId = FTS2.ProjectTaskId 
   --      AND FTS2.Type = 4
   --)
   AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost
   AND FTS.VendorGuid = @VendorGuid";

            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[uspGetPendingJobInvitesForVendor];");
        }
    }
}
