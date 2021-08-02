using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddStoreProcShareNotesInFormula : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/****** Object:  StoredProcedure [dbo].[usp_ShareNotesInFormula]    Script Date: 27-11-2020 11:45:25 ******/
-- =============================================          
-- Author:  Nikhil Sehgal    
-- Create date: 11-26-2020          
-- Description: To share notes in tasks with same formula within a project upon buton click
-- exec [usp_ShareNotesInFormula] 64787  
-- ============================================= 

CREATE PROCEDURE [dbo].[usp_ShareNotesInFormula]
@inputprojecttaskid int
AS

BEGIN

SELECT DISTINCT
--PT.PROJECTID, FP.ID AS FORMULAID,
 PT.Id AS PROJECTTASKID 
FROM PROJECTTASK PT
INNER JOIN FORMULATASK FT ON PT.FORMULATASKID = FT.ID
INNER JOIN FormulaProject FP ON FT.FormulaProjectId = FP.Id
INNER JOIN (

SELECT PT.PROJECTID, FP.ID AS FORMULAID, PT.Id AS PROJECTTASKID FROM PROJECTTASK PT
INNER JOIN FORMULATASK FT ON PT.FORMULATASKID = FT.ID
INNER JOIN FormulaProject FP ON FT.FormulaProjectId = FP.Id
WHERE PT.ID = @inputprojecttaskid

)A ON PT.ProjectId = A.ProjectId AND FP.Id = A.FORMULAID

END";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[usp_ShareNotesInFormula];");
        }
    }
}
