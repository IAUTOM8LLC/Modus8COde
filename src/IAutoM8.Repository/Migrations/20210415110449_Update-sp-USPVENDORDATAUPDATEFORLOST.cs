using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class UpdatespUSPVENDORDATAUPDATEFORLOST : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/***** Object: StoredProcedure [dbo].[USPVENDORDATAUPDATEFORLOST] Script Date: 15-04-2021 16:30:40 *****/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE IF EXISTS [dbo].[USPVENDORDATAUPDATEFORLOST]
GO

CREATE PROCEDURE [dbo].[USPVENDORDATAUPDATEFORLOST]
@INPUT_VENDORID [uniqueidentifier]

AS

BEGIN
BEGIN TRY

BEGIN TRANSACTION

DROP TABLE IF EXISTS #TEMP1

SELECT DISTINCT
FTS.VendorGuid AS VENDORGUID
,PT.OwnerGuid
,'PENDING' AS STATUS
,P.Id AS PROJECTID, P.Name AS PROJECTNAME, PT.Id AS TASKID, PT.Title AS TASKNAME
,FP.Id AS FORMULAID, FP.Name AS FORMULANAME
,S.Id AS SKILLID,S.Name AS SKILLNAME, T.Name AS TEAMNAME
--,S2.Name AS SKILLNAME2,S2.Id AS SKILL_ID2, T2.Name AS TEAMNAME2
,PT.StartDate as StartDate
,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS DUEDATE
,CAST((CAST(PT.DURATION AS decimal(15,2))/60) AS DECIMAL(15,2) ) AS DURATION
,DATEDIFF(HOUR,CAST(FTS.Created AS DATETIME),GETUTCDATE()) as TimeLeft
--, FTS.Type, FTS.Created, FTS.Completed

INTO #TEMP1
FROM FormulaTaskStatistic FTS
INNER JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id
INNER JOIN ProjectTaskVendor ptv on pt.Id = ptv.ProjectTaskId AND ptv.VendorGuid=fts.VendorGuid --added after
INNER JOIN Project P ON PT.ProjectId = P.Id
INNER JOIN FormulaTask FT ON PT.FormulaTaskId = FT.Id
INNER JOIN FormulaProject FP ON FT.FormulaProjectId = FP.Id
LEFT JOIN SKILL S on FT.AssignedSkillId = s.Id
LEFT JOIN TEAM T ON FT.TeamId = T.Id
WHERE FTS.Type = 0 AND FTS.Completed IS NULL
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type = 9) --new check added to exclude lost cases type = 4
--AND DATEDIFF(HOUR,CAST(FTS.Created AS DATETIME),GETUTCDATE()) > 24
AND DATEDIFF(MINUTE,CAST(FTS.Created AS DATETIME),GETUTCDATE()) > 1439
AND FTS.VendorGuid = @INPUT_VENDORID
--AND FTS.VendorGuid = '0B52D3DA-A7DD-4CF2-4F93-08D60200F888'

--SELECT * FROM #TEMP1


--INSERTION PART FOR FTS

INSERT INTO [dbo].[FormulaTaskStatistic]
([Completed]
,[Created]
,[FormulaTaskId]
,[FormulaTaskStatisticId]
,[ProjectTaskId]
,[Type]
,[Value]
,[VendorGuid])

SELECT DISTINCT GETUTCDATE() AS COMPLETED
,FTS.Created
,FTS.FormulaTaskId
,FTS.FormulaTaskStatisticId
,FTS.ProjectTaskId
,9 AS TYPE
--,DATEDIFF(MINUTE,CAST(FTS.Created AS DATETIME),GETUTCDATE()) AS VALUE
,NULL AS VALUE
,FTS.VendorGuid
FROM #TEMP1
INNER JOIN FormulaTaskStatistic FTS ON #TEMP1.TASKID = FTS.ProjectTaskId
WHERE FTS.Type = 0


--UPDATE PTV TABLE FOR LOST ENTRY --NEWLY ADDED
UPDATE PTV
SET STATUS = 9
FROM #TEMP1 T1
INNER JOIN FORMULATASKSTATISTIC FTS ON T1.TASKID = FTS.ProjectTaskId
INNER JOIN PROJECTTASKVENDOR PTV ON PTV.ProjectTaskId = T1.TASKID AND FTS.TYPE = 9 AND FTS.VENDORGUID = PTV.VENDORGUID --AND PTV.VendorGuid = T1.VENDORGUID


--INSERTION PART FOR NOTIFICATION TABLE

INSERT INTO [dbo].[Notification]
([CreateDate]
,[IsRead]
,[Message]
,[NotificationType]
,[RecipientGuid]
,[SenderGuid]
,[TaskId]
,[Url])
SELECT DISTINCT GETUTCDATE() AS CREATEDATE
,0 AS ISREAD
,'No Vendor accepted the task within 24HRs' as Message
,18 as NotificationType
,#TEMP1.OwnerGuid as RecipientGuid
--,#TEMP1.VENDORGUID as SenderGuid
,NULL AS SenderGuid
,TASKID as taskid
,null as url
from #TEMP1
INNER JOIN FormulaTaskStatistic FTS ON #TEMP1.TASKID = FTS.ProjectTaskId
INNER JOIN ProjectTaskVendor PTV ON FTS.ProjectTaskId = PTV.ProjectTaskId AND FTS.VendorGuid = PTV.VendorGuid
WHERE EXISTS (SELECT ID FROM FORMULATASKSTATISTIC FTS2 WHERE FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS.VendorGuid = FTS2.VendorGuid AND FTS2.Type = 0 AND FTS2.Completed IS NULL AND DATEDIFF(DAY,FTS2.Created,GETUTCDATE()) < 15) --NEWLY ADDED CHECK TO SEND NOTIFICATIONS ONLY WHEN THE TASK IS WITHIN 15 DAYS


INSERT INTO [dbo].[Notification]
([CreateDate]
,[IsRead]
,[Message]
,[NotificationType]
,[RecipientGuid]
,[SenderGuid]
,[TaskId]
,[Url])
SELECT DISTINCT GETUTCDATE() AS CREATEDATE
,0 AS ISREAD
,'you lost the job worth $' + CAST(PTV.Price AS VARCHAR(10)) as Message
,19 as NotificationType
--,#TEMP1.VENDORGUID as RecipientGuid
,FTS.VENDORGUID as RecipientGuid
,#TEMP1.OwnerGuid as SenderGuid
,TASKID as taskid
,null as url
FROM #TEMP1
INNER JOIN FormulaTaskStatistic FTS ON #TEMP1.TASKID = FTS.ProjectTaskId
INNER JOIN ProjectTaskVendor PTV ON FTS.ProjectTaskId = PTV.ProjectTaskId AND FTS.VendorGuid = PTV.VendorGuid
WHERE EXISTS (SELECT ID FROM FORMULATASKSTATISTIC FTS2 WHERE FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS.VendorGuid = FTS2.VendorGuid AND FTS2.Type = 0 AND FTS2.Completed IS NULL AND DATEDIFF(DAY,FTS2.Created,GETUTCDATE()) < 15) --NEWLY ADDED CHECK TO SEND NOTIFICATIONS ONLY WHEN THE TASK IS WITHIN 15 DAYS



COMMIT TRANSACTION
END TRY

BEGIN CATCH
IF @@TRANCOUNT>0
ROLLBACK TRANSACTION

PRINT 'TRANSACTION ROLLED BACK'
END CATCH

END";


            migrationBuilder.Sql(sql);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
