using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class Updatespusp_ShareNotesDownstream : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/***** Object: StoredProcedure [dbo].[usp_ShareNotesDownstream] Script Date: 15-04-2021 16:24:06 *****/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/***** Object: StoredProcedure [dbo].[usp_ShareNotesDownstream] Script Date: 26-11-2020 17:14:02 *****/

-- =============================================
-- Author: Nikhil Sehgal
-- Create date: 11-26-2020
-- Description: To share notes downstream within a project upon buton click
-- exec usp_ShareNotesDownstream 64787
--exec usp_ShareNotesDownstream 66174
-- =============================================

DROP PROCEDURE IF EXISTS [dbo].[usp_ShareNotesDownstream]
GO

CREATE PROCEDURE [dbo].[usp_ShareNotesDownstream]
@inputprojecttaskid int
AS

BEGIN
----set @inputprojecttaskid = 64787 --64787


--select ptd.ChildTaskId as PROJECTTASKID
--from ProjectTaskDependency ptd
--where ptd.ParentTaskId in (
-- select pt.Id from projecttask pt
-- where Id = @inputprojecttaskid
-- )

--union

--select ptd.ParentTaskId as PROJECTTASKID
--from ProjectTaskDependency ptd
--where ptd.childtaskid in (
-- select pt.Id from projecttask pt
-- where Id = @inputprojecttaskid
-- )

--union

--select pt.Id as PROJECTTASKID from ProjectTask pt
--where ParentTaskId in (
-- select ptd.ChildTaskId from ProjectTaskDependency ptd
-- where ptd.ParentTaskId in (
-- select pt.id from projecttask pt
-- where id in (
-- select pt.ParentTaskId
-- from ProjectTask pt
-- where id = @inputprojecttaskid
-- )
-- )
-- )

--union

--select pt.Id as PROJECTTASKID from ProjectTask pt
--where ParentTaskId in (
-- select ptd.ParentTaskId from ProjectTaskDependency ptd
-- where ptd.ChildTaskId in (
-- select pt.id from projecttask pt
-- where id in (
-- select pt.ParentTaskId
-- from ProjectTask pt
-- where id = @inputprojecttaskid
-- )
-- )
-- )


--union select @inputprojecttaskid


;WITH CTE
AS
(
select @inputprojecttaskid as PROJECTTASKID


UNION ALL

select ptd.ChildTaskId as PROJECTTASKID
from ProjectTaskDependency ptd
INNER JOIN CTE ON PTD.ParentTaskId = CTE.PROJECTTASKID

)
SELECT DISTINCT * FROM CTE

END";


            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
