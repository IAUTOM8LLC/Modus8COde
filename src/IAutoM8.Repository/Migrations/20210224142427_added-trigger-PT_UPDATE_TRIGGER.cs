using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class addedtriggerPT_UPDATE_TRIGGER : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
/****** Object:  Trigger [PT_UPDATE_TRIGGER]    Script Date: 24-02-2021 14:11:28 ******/
DROP TRIGGER if exists [dbo].[PT_UPDATE_TRIGGER]
GO

/****** Object:  Trigger [dbo].[PT_UPDATE_TRIGGER]    Script Date: 24-02-2021 14:11:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- JOB INVITE ENQUEUE UI
CREATE TRIGGER [dbo].[PT_UPDATE_TRIGGER] ON [dbo].[ProjectTask]
AFTER UPDATE 
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[FormulaTaskStatistic]
			   ([Completed]
			   ,[Created]
			   ,[FormulaTaskId]
			   ,[FormulaTaskStatisticId]
			   ,[ProjectTaskId]
			   ,[Type]
			   ,[Value]
			   ,[VendorGuid])
	SELECT 
			   NULL AS Completed
			   ,GETUTCDATE() AS Created
			   ,PT.FormulaTaskId AS FormulaTaskId
			   ,NULL AS FormulaTaskStatisticId
			   ,PTV.ProjectTaskId AS ProjectTaskId
			   ,0 AS Type
			   ,NULL AS Value
			   ,PTV.VENDORGUID AS VendorGuid

	FROM	
	--FormulaTaskStatistic FTS
	inserted I  
	INNER JOIN ProjectTaskDependency PTD ON I.ID = PTD.ParentTaskId
	INNER JOIN PROJECTTASKVENDOR PTV ON PTD.CHILDTASKID = PTV.ProjectTaskId AND PTV.STATUS = 7 --NEW ENUM FOR AWATING FOR PRIMARY TASK (JOB INVITE ENQUEUE)
	INNER JOIN ProjectTask PT ON PTV.ProjectTaskId = PT.Id 
	WHERE  I.Status IN (2,3) 

	INSERT INTO [dbo].[Notification]
           ([CreateDate]
           ,[IsRead]
           ,[Message]
           ,[NotificationType]
           ,[RecipientGuid]
           ,[SenderGuid]
           ,[TaskId]
           ,[Url])
     SELECT 
           GETUTCDATE()   as [CreateDate]
           ,0 AS IsRead
           ,PT.title + ' has been sent to you for acceptance' AS [Message]
           ,10 AS NotificationType
           ,PTV.VENDORGUID AS RecipientGuid
           ,PT.OWNERGUID AS SenderGuid
           ,NULL AS TaskId
           ,NULL AS Url
	 FROM	
	 --FormulaTaskStatistic FTS
	 --INNER JOIN 
	 inserted I  --ON FTS.Id= I.ID 
	 INNER JOIN ProjectTaskDependency PTD ON I.ID = PTD.ParentTaskId
	 INNER JOIN PROJECTTASKVENDOR PTV ON PTD.CHILDTASKID = PTV.ProjectTaskId AND PTV.STATUS = 7 --NEW ENUM FOR AWATING FOR PRIMARY TASK (JOB INVITE ENQUEUE)
	 INNER JOIN ProjectTask PT ON PTV.ProjectTaskId = PT.Id 
	 --WHERE  I.TYPE = 1 AND I.COMPLETED IS NOT NULL
	 WHERE  I.Status IN (2,3) 


	 UPDATE PTV
	 SET ptv.status = 4
	 from 
	 --FormulaTaskStatistic FTS
	 --INNER JOIN 
	 inserted I  --ON FTS.Id= I.ID 
	 INNER JOIN ProjectTaskDependency PTD ON I.ID = PTD.ParentTaskId
	 INNER JOIN PROJECTTASKVENDOR PTV ON PTD.CHILDTASKID = PTV.ProjectTaskId AND PTV.STATUS = 7 --NEW ENUM FOR AWATING FOR PRIMARY TASK (JOB INVITE ENQUEUE)
	 INNER JOIN ProjectTask PT ON PTV.ProjectTaskId = PT.Id 
	 --WHERE  I.TYPE = 1 AND I.COMPLETED IS NOT NULL
	 WHERE  I.Status IN (2,3) 

END
GO

ALTER TABLE [dbo].[ProjectTask] ENABLE TRIGGER [PT_UPDATE_TRIGGER]
GO";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
