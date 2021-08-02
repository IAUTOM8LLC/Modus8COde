using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AlterspuspGetVendorTasksByVendorGuidcompanyworker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/****** Object:  StoredProcedure [dbo].[uspGetVendorTasksByVendorGuid]    Script Date: 24-02-2021 14:07:22 ******/
DROP PROCEDURE IF EXISTS [dbo].[uspGetVendorTasksByVendorGuid]
GO
/****** Object:  StoredProcedure [dbo].[uspGetVendorTasksByVendorGuid]    Script Date: 24-02-2021 14:07:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================          
-- Author:  Nikhil Sehgal     
-- Create date: 24-12-2020          
-- Description: To get vendor home dashboard tasks by vendor guid          
-- ============================================= 

CREATE PROCEDURE [dbo].[uspGetVendorTasksByVendorGuid]     
@VendorGuid [uniqueidentifier]    
    
AS    
BEGIN    
    
    
SELECT DISTINCT VENDORGUID    
,VENDORFULLNAME    
,STATUS    
,PROJECTID    
,PROJECTNAME    
,TASKID    
,TASKNAME    
,FORMULAID    
,FORMULANAME    
,SKILLID    
,SKILLNAME    
,TEAMNAME     
,StartDate    
,DUEDATE    
,DURATION    
,CT    
,TAT    
,IsCancel    
,IsVendor    
,Price    
--,ETA --newly added, mean-tat as eta    
--,ETA_10 --newly added, mean-tat + 10% as ETA_10    
--,target_date_10    
--,IS_PAST80_ETA_10    
--,CASE WHEN StartDate >GETUTCDATE() THEN '00:00:00'    
-- WHEN ETA_10 IS NULL THEN '00:00:00'    
-- ELSE    
--replace(convert(varchar(18),DateDiff(s,(GETUTCDATE()), target_date_10)/3600)+':'    
--+convert(varchar(5),DateDiff(s, (GETUTCDATE()), target_date_10)%3600/60)+':'    
--+convert(varchar(5),(DateDiff(s, (GETUTCDATE()), target_date_10)%60)),':-',':' ) END as [ETA_10_REMAINING_inrealtime] --newly added by nik to get realtime remaining time BUT NOT USED SINCE CODE REQUIRES VALUE IN SECONDS ONLY    
    
--,CASE WHEN StartDate >GETUTCDATE() THEN 0 --newly added by nik to get DIFF IN SECONDS - CURRENTLY BEING USED    
-- WHEN ETA_10 IS NULL THEN 0    
-- ELSE DATEDIFF(SECOND,(GETUTCDATE()),target_date_10 )    
-- END AS [ETA_10_REMAINING_SECONDS]    

,DEADLINE --newly added
,TARGET_DATE
,IS_PAST80_DEADLINE 
, CASE WHEN DEADLINE IS NULL THEN 0
  ELSE DATEDIFF(SECOND,GETUTCDATE(),TARGET_DATE ) 
  END AS [DEADLINE_REMAINING_SECONDS] 
  ,NUDGECOUNT--NEWLY ADDED
 FROM  (    
    
SELECT DISTINCT VENDORS.VENDOR_ID AS VENDORGUID    
,VENDORS.Vendor_FullName AS VENDORFULLNAME    
,TASKS.STATUS    
,TASKS.PROJECTID    
,TASKS.PROJECTNAME    
,TASKS.TASKID    
,TASKS.TASKNAME    
,TASKS.FORMULAID    
,TASKS.FORMULANAME    
,TASKS.SKILLID    
,TASKS.SKILLNAME    
,TASKS.TEAMNAME     
,TASKS.StartDate    
,TASKS.DUEDATE    
,TASKS.DURATION    
,0 AS CT    
,0 AS TAT    
,CASE WHEN N.IsCancel  IS NULL THEN 0 ELSE N.IsCancel END AS IsCancel    
,CAST(1 AS bit) AS IsVendor    
,PROJECTTASKPRICE.Price AS Price    
--,((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT ) * 60) AS ETA --newly added, mean-tat as eta    
--,((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT ) * 60)+ (((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)*0.1)  AS ETA_10 --newly added, mean-tat as eta    
--, dateadd(SECOND,( ((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT ) * 60)+ (((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)*0.1) ),TASKS.StartDate)  as target_date_10    
----,CASE WHEN (TASKS.StartDate <= GETUTCDATE() ) and ((  DATEDIFF(HOUR,TASKS.StartDate,GETUTCDATE()) ) >= ((((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)+(((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)*0.1))*(0.8)) )THEN 1 ELSE 0 END AS IS_PAST80_ETA_10 --newly added    
--,CASE WHEN (TASKS.StartDate <= GETUTCDATE() ) and ((  DATEDIFF(SECOND,TASKS.StartDate,GETUTCDATE()) ) >= ((((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)+(((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)*0.1))*(0.8)) )THEN 1 ELSE 0 END AS IS_PAST80_ETA_10 --newly added    
, CASE WHEN TASKS.STATUS = 'NOTSTARTED'							THEN (MEAN_DWELL.MEANDWELL * 60)
	   WHEN TASKS.STATUS IN ('TASKSOVERDUE','ACTIVE')			THEN ((MEAN_CT.MEANCT*60) + ((MEAN_CT.MEANCT*60)*0.2)) --20% ADDED IN CT
	   END AS DEADLINE
, CASE WHEN TASKS.STATUS = 'NOTSTARTED'							THEN DATEADD(SECOND,(MEAN_DWELL.MEANDWELL * 60),TASKS.acceptdate)
	   WHEN TASKS.STATUS IN ('TASKSOVERDUE','ACTIVE')			THEN DATEADD(SECOND,((MEAN_CT.MEANCT*60) + ((MEAN_CT.MEANCT*60)*0.2)),TASKS.StartDate) --20% ADDED IN CT
	   END AS TARGET_DATE

, CASE WHEN  TASKS.STATUS IN ('TASKSOVERDUE','ACTIVE')	and (	(  DATEDIFF(SECOND,TASKS.StartDate,GETUTCDATE()) ) >=	(( (MEAN_CT.MEANCT*60) + ((MEAN_CT.MEANCT*60)*0.2) )*0.8)	)	THEN  1 
	   WHEN TASKS.STATUS IN ('NOTSTARTED')	and ((  DATEDIFF(SECOND,TASKS.acceptdate,GETUTCDATE()) ) >=	(( (MEAN_DWELL.MEANDWELL * 60) )*0.8) )	THEN  1 
ELSE 0 
END AS IS_PAST80_DEADLINE 
,NUDGECOUNT.NUDGECOUNT--NEWLY ADDED
from     
(      
 SELECT DISTINCT anu.Id as Vendor_ID,up.FullName as Vendor_FullName--,FTV.FormulaTaskId    
 FROM AspNetUsers anu    
 INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id    
 INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name in ('Vendor', 'CompanyWorker') --and anr.Name='Vendor'    --new role added for sprint 10B
 INNER JOIN UserProfile up on up.UserId=anu.Id    
 INNER JOIN FormulaTaskVendor FTV on FTV.VendorGuid=anu.Id AND FTV.VendorGuid=@VendorGuid --Original for sp    
 --INNER JOIN FormulaTaskVendor FTV on FTV.VendorGuid=anu.Id AND FTV.VendorGuid= '550B9B4A-358A-4F37-2800-08D782284CA5'    
    
) VENDORS    
    
INNER JOIN     
(    
SELECT DISTINCT     
CASE WHEN (DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETUTCDATE()) > PT.Duration) THEN 'TASKSOVERDUE'    
  --WHEN ( (DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETUTCDATE()) >= (CAST(ROUND((PT.DURATION*0.8),0) AS INT)) ) AND  (DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETUTCDATE()) <= PT.Duration) ) THEN 'ATRISK'    --COMMENTED ON 24DEC20 ON CLIENT REQ
ELSE 'ACTIVE' END AS STATUS,     
FTS.VendorGuid,P.Id AS PROJECTID, P.Name AS PROJECTNAME, PT.Id AS TASKID, PT.Title AS TASKNAME    
,FP.Id AS FORMULAID, FP.Name AS FORMULANAME    
,S.Id AS SKILLID,S.Name AS SKILLNAME, T.Name AS TEAMNAME     
--,S2.Name AS SKILLNAME2,S2.Id AS SKILL_ID2, T2.Name AS TEAMNAME2     
--,PT.StartDate as StartDate  recently commented
,fts.created as startdate  --newly added
,null as acceptdate
--,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS DUEDATE    
,DATEADD(MINUTE,PT.Duration,fts.created) AS DUEDATE    
,CAST((CAST(PT.DURATION AS decimal(15,2))/60) AS DECIMAL(15,2) ) AS DURATION    
--, FTS.Type, FTS.Created, FTS.Completed    
,ft.id as Formulataskid    
FROM FormulaTaskStatistic FTS    
INNER JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id    
INNER JOIN Project P ON PT.ProjectId = P.Id    
INNER JOIN  FormulaTask FT ON PT.FormulaTaskId = FT.Id    
INNER JOIN FormulaProject FP ON FT.FormulaProjectId = FP.Id    
LEFT JOIN SKILL S on FT.AssignedSkillId = s.Id    
LEFT JOIN TEAM T ON FT.TeamId = T.Id    
--LEFT JOIN SKILL S2 on PT.AssignedSkillId = s.Id    
--LEFT JOIN TEAM T2 ON PT.TeamId = T.Id    
WHERE FTS.Type = 1 AND FTS.Completed IS NULL    
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9,10)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6 AND LOST DUE TO OVERTIME CASES BY VENDOR TYPE = 10   

UNION 

SELECT DISTINCT     --newly added on 24dec20 for 
--CASE WHEN (DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETUTCDATE()) > PT.Duration) THEN 'TASKSOVERDUE'    
--  WHEN ( (DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETUTCDATE()) >= (CAST(ROUND((PT.DURATION*0.8),0) AS INT)) ) AND  (DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETUTCDATE()) <= PT.Duration) ) THEN 'ATRISK'    
--ELSE 'ACTIVE' END AS STATUS, 
'NOTSTARTED' AS STATUS,    
FTS.VendorGuid,P.Id AS PROJECTID, P.Name AS PROJECTNAME, PT.Id AS TASKID, PT.Title AS TASKNAME    
,FP.Id AS FORMULAID, FP.Name AS FORMULANAME    
,S.Id AS SKILLID,S.Name AS SKILLNAME, T.Name AS TEAMNAME     
--,S2.Name AS SKILLNAME2,S2.Id AS SKILL_ID2, T2.Name AS TEAMNAME2     
--,PT.StartDate as StartDate
,null as startdate  --newly added  
,fts.created as acceptdate  --newly added  
--,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS DUEDATE    
,null AS DUEDATE  --NEWLY ADDED
,CAST((CAST(PT.DURATION AS decimal(15,2))/60) AS DECIMAL(15,2) ) AS DURATION    
--, FTS.Type, FTS.Created, FTS.Completed    
,ft.id as Formulataskid    
FROM FormulaTaskStatistic FTS    
INNER JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id    
INNER JOIN Project P ON PT.ProjectId = P.Id    
INNER JOIN  FormulaTask FT ON PT.FormulaTaskId = FT.Id    
INNER JOIN FormulaProject FP ON FT.FormulaProjectId = FP.Id    
LEFT JOIN SKILL S on FT.AssignedSkillId = s.Id    
LEFT JOIN TEAM T ON FT.TeamId = T.Id    
--LEFT JOIN SKILL S2 on PT.AssignedSkillId = s.Id    
--LEFT JOIN TEAM T2 ON PT.TeamId = T.Id    
WHERE FTS.Type = 7 AND FTS.Completed IS NULL    
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9,10)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6 AND VENDOR CASES LOST DUE TO OVERTIME CASES BY  TYPE = 10    
  
UNION     
    
SELECT DISTINCT     
'COMPLETED' AS STATUS,     
FTS.VendorGuid,P.Id AS PROJECTID, P.Name AS PROJECTNAME, PT.Id AS TASKID, PT.Title AS TASKNAME    
,FP.Id AS FORMULAID, FP.Name AS FORMULANAME    
,S.Id AS SKILLID,S.Name AS SKILLNAME, T.Name AS TEAMNAME     
--,S2.Name AS SKILLNAME2,S2.Id AS SKILL_ID2, T2.Name AS TEAMNAME2     
--,PT.StartDate as StartDate    
,fts.Created as StartDate   
,DATEADD(MINUTE,PT.Duration,fts.Created) AS DUEDATE   
, null as acceptdate 
,CAST((CAST(PT.DURATION AS decimal(15,2))/60) AS DECIMAL(15,2) ) AS DURATION    
--, FTS.Type, FTS.Created, FTS.Completed    
,ft.id as Formulataskid    
FROM FormulaTaskStatistic FTS    
INNER JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id    
INNER JOIN Project P ON PT.ProjectId = P.Id    
INNER JOIN  FormulaTask FT ON PT.FormulaTaskId = FT.Id    
INNER JOIN FormulaProject FP ON FT.FormulaProjectId = FP.Id    
LEFT JOIN SKILL S on FT.AssignedSkillId = s.Id    
LEFT JOIN TEAM T ON FT.TeamId = T.Id    
--LEFT JOIN SKILL S2 on PT.AssignedSkillId = s.Id    
--LEFT JOIN TEAM T2 ON PT.TeamId = T.Id    
WHERE FTS.Type = 1 AND FTS.Completed IS NOT NULL    
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9,10)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6 AND VENDOR CASES LOST DUE TO OVERTIME CASES BY  TYPE = 10    
)     
TASKS ON VENDORS.Vendor_ID = TASKS.VendorGuid    
    
LEFT JOIN     --to chk if the task is eligible for cancel button
(    
SELECT * FROM     
(    
SELECT TaskId AS PROJECTTASKID, SenderGuid AS VENDORGUID, Message, CreateDate , ROW_NUMBER() OVER (PARTITION BY TaskId,SenderGuid ORDER BY CreateDate ) AS RNUM    
, CASE WHEN DATEDIFF(MINUTE,N1.CreateDate,GETUTCDATE()) >= 60 then  1 else 0 --original 1 hr check    
--, CASE WHEN DATEDIFF(MINUTE,N1.CreateDate,GETUTCDATE()) >= 2 then  1 else 0 --for testing    
 END AS IsCancel    
FROM Notification N1    
WHERE NotificationType = 17 --ORIGINAL    
--WHERE NotificationType = 13 --FOR TESTING    
AND EXISTS(    
   SELECT TaskId AS PROJECTTASKID, SenderGuid AS VENDORGUID, COUNT(Message) AS CNT    
   FROM Notification N2    
   WHERE NotificationType = 17 --ORIGINAL    
   --WHERE NotificationType = 13 --FOR TESTING    
   AND N2.TaskId = N1.TaskId AND N2.SenderGuid = N1.SenderGuid    
   GROUP BY TaskId,SenderGuid    
   HAVING COUNT(Message) >=3    
   )    
) a    
where RNUM = 3    
) N ON TASKS.VendorGuid = N.VENDORGUID AND TASKS.TASKID = N.PROJECTTASKID    


INNER JOIN    
(    
SELECT PTV.VendorGuid, PTV.ProjectTaskId, PTV.Price     
FROM ProjectTaskVendor PTV    
) PROJECTTASKPRICE    
ON TASKS.VendorGuid = PROJECTTASKPRICE.VendorGuid AND TASKS.TASKID = PROJECTTASKPRICE.ProjectTaskId    
    
    
    
--LEFT JOIN     
-- (    
-- SELECT FormulaTaskId, CAST( ((CAST(AVG(cast (VLU as decimal(15,2))) AS DECIMAL(15,2)))) AS DECIMAL(15,2))as MEANDWELL     
-- FROM (    
-- select FormulaTaskId, VendorGuid,CAST(REPLACE(Value,'-','') as decimal(15,2)) AS VLU    
-- from FormulaTaskStatistic FTS    
-- where     
-- [Value] is not null and     
--[Type]=0    
-- AND EXISTS (SELECT * FROM FormulaTaskStatistic FTS_2 WHERE (FTS_2.VendorGuid = FTS.VendorGuid AND FTS_2.FormulaTaskId = FTS.FormulaTaskId) AND FTS_2.TYPE = 1 AND FTS_2.Completed IS NOT NULL)    
-- ) A     
-- group by FormulaTaskId    
--) MEAN_DWELL  on   TASKS.FormulaTaskId = MEAN_DWELL.FormulaTaskId     
    
    
--LEFT JOIN     
-- (    
-- SELECT  FormulaTaskId, CAST( ((CAST(AVG(cast (VLU as decimal(15,2))) AS DECIMAL(15,2)))) AS DECIMAL(15,2)) as MEANCT     
-- FROM (    
-- select FormulaTaskId,VendorGuid,cast (REPLACE(Value,'-','') as decimal(15,2)) AS VLU    
-- from FormulaTaskStatistic FTS    
-- where     
-- [Value] is not null and     
-- [Type]=1    
-- AND EXISTS (SELECT * FROM FormulaTaskStatistic FTS_2 WHERE (FTS_2.VendorGuid = FTS.VendorGuid AND FTS_2.FormulaTaskId = FTS.FormulaTaskId) AND FTS_2.TYPE = 1 AND FTS_2.Completed IS NOT NULL)    
-- )A    
-- group by FormulaTaskId    
--) MEAN_CT  on   TASKS.FormulaTaskId = MEAN_CT.FormulaTaskId    
    
	 
LEFT JOIN     --newly added
 (    
 SELECT FormulaTaskId, CAST( ((CAST(AVG(cast (VLU as decimal(15,2))) AS DECIMAL(15,2)))) AS DECIMAL(15,2))as MEANDWELL     
 FROM (    
 select FormulaTaskId, VendorGuid,CAST(REPLACE(Value,'-','') as decimal(15,2)) AS VLU    
 from FormulaTaskStatistic FTS    
 where     
 [Value] is not null and     
 --[Type]=0    
 [Type]= 7
 AND EXISTS (SELECT * FROM FormulaTaskStatistic FTS_2 WHERE (FTS_2.VendorGuid = FTS.VendorGuid AND FTS_2.FormulaTaskId = FTS.FormulaTaskId) AND FTS_2.TYPE = 1 AND FTS_2.Completed IS NOT NULL)    
 ) A     
 group by FormulaTaskId    
) MEAN_DWELL  on   TASKS.FormulaTaskId = MEAN_DWELL.FormulaTaskId     
    
    
LEFT JOIN     --newly added
 (    
 SELECT  FormulaTaskId, CAST( ((CAST(AVG(cast (VLU as decimal(15,2))) AS DECIMAL(15,2)))) AS DECIMAL(15,2)) as MEANCT     
 FROM (    
 select FormulaTaskId,VendorGuid,cast (REPLACE(Value,'-','') as decimal(15,2)) AS VLU    
 from FormulaTaskStatistic FTS    
 where     
 [Value] is not null and     
 [Type]=1    
 AND EXISTS (SELECT * FROM FormulaTaskStatistic FTS_2 WHERE (FTS_2.VendorGuid = FTS.VendorGuid AND FTS_2.FormulaTaskId = FTS.FormulaTaskId) AND FTS_2.TYPE = 1 AND FTS_2.Completed IS NOT NULL)    
 )A    
 group by FormulaTaskId    
) MEAN_CT  on   TASKS.FormulaTaskId = MEAN_CT.FormulaTaskId  


LEFT JOIN --TO GET COUNT FOR NUDGE BUTTON --NEWLY ADDED FOR NEW NUDGE BUTTON WITH OVERHEAD COUNTER
(
SELECT TaskId AS PROJECTTASKID, SenderGuid AS VENDORGUID, COUNT(Message) AS NUDGECOUNT    
   FROM Notification N2    
   WHERE NotificationType = 17 --ORIGINAL    
   --WHERE NotificationType = 13 --FOR TESTING    
   --AND N2.TaskId = N1.TaskId AND N2.SenderGuid = N1.SenderGuid    
   GROUP BY TaskId,SenderGuid    
   --HAVING COUNT(Message) >=3    
) NUDGECOUNT ON TASKS.VendorGuid = NUDGECOUNT.VENDORGUID AND TASKS.TASKID = NUDGECOUNT.PROJECTTASKID

--ORDER BY VENDORS.VENDOR_ID,STATUS,PROJECTID,TASKID    
) FINAL    
ORDER BY VENDORGUID,STATUS,PROJECTID,TASKID    
    
END
GO";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
