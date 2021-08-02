using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddStoreProcReAssignVendorTasksforAllVendors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"--EXEC [dbo].[uspReAssignVendorTasksforAllVendors] ---------'95C001BF-07F3-4A61-49DF-08D818944FBA'   '850452EA-2129-461E-8688-08D8172C1608'

/****** Object:  StoredProcedure  [dbo].[uspReAssignVendorTasksforAllVendors]    Script Date: 12-11-2020 11:28:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*---------------------------------------------------------------------------

Created by  - Nikhil Sehgal
Creation date - 10-11-2020

Description -
1) Send invites to top rated vendors
2) set the processinguserid as null untill someone accepts
3) Reset the start date to getdate
4) ETA would be based on same funtionality which is currently being used
--update FTS WITH ENUM AS 10 FOR CURRENLTY WORKING VENDOR having enum  = 1 --done
--THINK OF SOMETHING FOR PROJECTTASKVENDOR TABLE AS WELL FOR NEW ENUM -10
--UPDATE FTS ENTRIES FOR NEW VENDORS WITH TYPE = 9 TO TYPE =11 --DONE
--UPDATE FTS ENTRY FOR NEW VENDOR HAVINF TYPE = 0 , SET CREATED AS GETUTCDATE() ALL ON BASIS OF PROJECTTASK ID--DONE

-----------------------------------------------------------------------------*/

CREATE PROCEDURE [dbo].[uspReAssignVendorTasksforAllVendors]     
--@VendorGuid [uniqueidentifier]    
    
AS    
BEGIN    

BEGIN TRY
BEGIN TRANSACTION
    
    
SELECT DISTINCT VENDORGUID    
,VENDORFULLNAME    
,STATUS    
,PROJECTID    
,PROJECTNAME    
,TASKID    
,TASKNAME    
,FORMULAID    
,FORMULANAME    
,Formulataskid
,SKILLID    
,SKILLNAME    
,TEAMNAME     
,StartDate    
,DUEDATE    
,DURATION    
,IsVendor    
--,ETA --newly added, mean-tat as eta    
,ETA_10 --newly added, mean-tat + 10% as ETA_10    
,target_date_10    
,IS_PAST80_ETA_10    
--,CASE WHEN StartDate >GETUTCDATE() THEN '00:00:00'    
-- WHEN ETA_10 IS NULL THEN '00:00:00'    
-- ELSE    
--replace(convert(varchar(18),DateDiff(s,(GETUTCDATE()), target_date_10)/3600)+':'    
--+convert(varchar(5),DateDiff(s, (GETUTCDATE()), target_date_10)%3600/60)+':'    
--+convert(varchar(5),(DateDiff(s, (GETUTCDATE()), target_date_10)%60)),':-',':' ) END as [ETA_10_REMAINING_inrealtime] --newly added by nik to get realtime remaining time BUT NOT USED SINCE CODE REQUIRES VALUE IN SECONDS ONLY    
    
,CASE WHEN StartDate >GETUTCDATE() THEN 0 --newly added by nik to get DIFF IN SECONDS - CURRENTLY BEING USED    
 WHEN ETA_10 IS NULL THEN 0    
 ELSE DATEDIFF(SECOND,(GETUTCDATE()),target_date_10 )    
 END AS [ETA_10_REMAINING_SECONDS]    
  
  INTO #TEMP1
 
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
,TASKS.Formulataskid 
,TASKS.SKILLID    
,TASKS.SKILLNAME    
,TASKS.TEAMNAME     
,TASKS.StartDate    
,TASKS.DUEDATE    
,TASKS.DURATION    
,CAST(1 AS bit) AS IsVendor    
--,PROJECTTASKPRICE.Price AS Price    
--,((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT ) * 60) AS ETA --newly added, mean-tat as eta    
,((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT ) * 60)+ (((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)*0.1)  AS ETA_10 --newly added, mean-tat as eta    
, dateadd(SECOND,( ((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT ) * 60)+ (((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)*0.1) ),TASKS.StartDate)  as target_date_10    
--,CASE WHEN (TASKS.StartDate <= GETUTCDATE() ) and ((  DATEDIFF(HOUR,TASKS.StartDate,GETUTCDATE()) ) >= ((((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)+(((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)*0.1))*(0.8)) )THEN 1 ELSE 0 END AS IS_PAST80_ETA_10 --newly added    
,CASE WHEN (TASKS.StartDate <= GETUTCDATE() ) and ((  DATEDIFF(SECOND,TASKS.StartDate,GETUTCDATE()) ) >= ((((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)+(((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT )*60)*0.1))*(0.8)) )THEN 1 ELSE 0 END AS IS_PAST80_ETA_10 --newly added    
    
from     
(      
 SELECT DISTINCT anu.Id as Vendor_ID,up.FullName as Vendor_FullName--,FTV.FormulaTaskId    
 FROM AspNetUsers anu    
 INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id    
 INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name='Vendor'    
 INNER JOIN UserProfile up on up.UserId=anu.Id    
 INNER JOIN FormulaTaskVendor FTV on FTV.VendorGuid=anu.Id --AND FTV.VendorGuid=@VendorGuid --Original for sp    
     
) VENDORS    
    
INNER JOIN     
(    
SELECT DISTINCT     
CASE WHEN (DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETUTCDATE()) > PT.Duration) THEN 'TASKSOVERDUE'    
  WHEN ( (DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETUTCDATE()) >= (CAST(ROUND((PT.DURATION*0.8),0) AS INT)) ) AND  (DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETUTCDATE()) <= PT.Duration) ) THEN 'ATRISK'    
ELSE 'ACTIVE' END AS STATUS,     
FTS.VendorGuid,P.Id AS PROJECTID, P.Name AS PROJECTNAME, PT.Id AS TASKID, PT.Title AS TASKNAME    
,FP.Id AS FORMULAID, FP.Name AS FORMULANAME    
,S.Id AS SKILLID,S.Name AS SKILLNAME, T.Name AS TEAMNAME     
--,S2.Name AS SKILLNAME2,S2.Id AS SKILL_ID2, T2.Name AS TEAMNAME2     
,PT.StartDate as StartDate    
,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS DUEDATE    
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
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9,10)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6  and type =10 for lost due to overtime

)     
TASKS ON VENDORS.Vendor_ID = TASKS.VendorGuid    
      
INNER JOIN     
 (    
 SELECT FormulaTaskId, CAST( ((CAST(AVG(cast (VLU as decimal(15,2))) AS DECIMAL(15,2)))) AS DECIMAL(15,2))as MEANDWELL     
 FROM (    
 select FormulaTaskId, VendorGuid,CAST(REPLACE(Value,'-','') as decimal(15,2)) AS VLU    
 from FormulaTaskStatistic FTS    
 where     
 [Value] is not null and     
 [Type]=0    
 AND EXISTS (SELECT * FROM FormulaTaskStatistic FTS_2 WHERE (FTS_2.VendorGuid = FTS.VendorGuid AND FTS_2.FormulaTaskId = FTS.FormulaTaskId) AND FTS_2.TYPE = 1 AND FTS_2.Completed IS NOT NULL)    
 ) A     
 group by FormulaTaskId    
) MEAN_DWELL  on   TASKS.FormulaTaskId = MEAN_DWELL.FormulaTaskId     
    
    
INNER JOIN     
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
    
--ORDER BY VENDORS.VENDOR_ID,STATUS,PROJECTID,TASKID    
) FINAL    
--ORDER BY VENDORGUID,STATUS,PROJECTID,TASKID    
    
	--SELECT * FROM #TEMP1


SELECT DISTINCT T1.VENDORGUID AS T1_VENDORGUID	,T1.VENDORFULLNAME	AS T1_VENDORFULLNAME,T1.STATUS	AS T1_STATUS,T1.PROJECTID AS T1_PROJECTID	,T1.PROJECTNAME AS T1_PROJECTNAME	,T1.TASKID	 AS T1_TASKID
,T1.TASKNAME AS T1_TASKNAME	,T1.FORMULAID	AS T1_FORMULAID
,T1.FORMULANAME AS T1_FORMULANAME	,T1.Formulataskid	AS T1_Formulataskid ,T1.SKILLID AS T1_SKILLID	,T1.SKILLNAME AS T1_SKILLNAME	,T1.TEAMNAME AS T1_TEAMNAME	,T1.StartDate AS T1_StartDate
	,T1.DUEDATE AS T1_DUEDATE	,T1.DURATION AS T1_DURATION	,T1.IsVendor AS T1_IsVendor	
,T1.ETA_10	AS T1_ETA_10 ,T1.target_date_10 AS T1_target_date_10	,T1.IS_PAST80_ETA_10 AS T1_IS_PAST80_ETA_10	,T1.ETA_10_REMAINING_SECONDS AS T1_ETA_10_REMAINING_SECONDS	

--,FTS.Id AS FTS_Id	,FTS.Completed AS FTS_Completed	,FTS.Created AS FTS_Created	,FTS.FormulaTaskId	AS FTS_FormulaTaskId ,FTS.FormulaTaskStatisticId AS FTS_FormulaTaskStatisticId	,FTS.ProjectTaskId AS FTS_ProjectTaskId
--,FTS.Type AS FTS_Type	,FTS.Value AS FTS_VALUE	,FTS.VendorGuid AS FTS_VendorGuid

,PTV.Id AS PTV_ID
,PTV.Created AS PTV_Created
,PTV.LastModified as PTV_Lastmodified
,PTV.Price as PTV_Price
,PTV.ProjectTaskId as PTV_Projecttaskid
,PTV.Status as PTV_Status
,PTV.VendorGuid as PTV_VendorGuid


--T1.*  ,FTS.* 
INTO #TEMP2
FROM #TEMP1 T1
INNER JOIN 
(
	SELECT * 
	FROM
		(
		SELECT row_number() OVER (PARTITION BY FORMULAID,FT_ID ORDER BY OUTSOURCER_RATING DESC, OUTSOURCER_GUID) AS RNK
		,count(OUTSOURCER_GUID) OVER (PARTITION BY FORMULAID,FT_ID ) AS COUNT_OUTSOURCERS
		,A.*
		FROM 
			(SELECT DISTINCT FP.Id AS FORMULAID
			,FT.Id AS FT_ID
			,FTV.VENDORGUID AS OUTSOURCER_GUID
			,AVG(FTS.Value) AS OUTSOURCER_RATING
			,null AS OUTSOURCER_PRICE
			 FROM FORMULAPROJECT FP
			INNER JOIN FormulaTask FT ON FP.Id = FT.FormulaProjectId
			INNER JOIN FORMULATASKVENDOR FTV ON FT.ID = FTV.FormulaTaskId AND FTV.Status = 3  -- status 3 means certified vendor   
			left JOIN FORMULATASKSTATISTIC FTS ON FT.Id = FTS.FormulaTaskId AND FTV.VendorGuid = FTS.VendorGuid AND FTS.TYPE = 2
			--WHERE not exists (select * from #temp1 where )
			GROUP BY FP.Id,FT.Id, FTV.VENDORGUID
			)A
		)B
		WHERE (COUNT_OUTSOURCERS <=4 ) OR (COUNT_OUTSOURCERS >=5 AND RNK <= ROUND((COUNT_OUTSOURCERS*0.5),0) )
	
) RATING ON T1.FORMULAID = RATING.FORMULAID AND T1.FORMULATASKID = RATING.FT_ID

--INNER JOIN FormulaTaskStatistic FTS ON T1.TASKID = FTS.ProjectTaskId AND T1.Formulataskid = FTS.FormulaTaskId AND RATING.OUTSOURCER_GUID = FTS.VendorGuid AND T1.VENDORGUID <> FTS.VendorGuid
INNER JOIN PROJECTTASKVENDOR PTV ON T1.TASKID = PTV.ProjectTaskId AND RATING.OUTSOURCER_GUID = PTV.VendorGuid AND T1.VENDORGUID <> PTV.VendorGuid
WHERE [ETA_10_REMAINING_SECONDS] <=0 --UNCOMMENT FOR FINAL
--AND NOT EXISTS (select * from FormulaTaskStatistic fts2 where fts.VendorGuid = fts2.VendorGuid and fts.projecttaskid = fts2.projecttaskid and fts2.type=10)
AND NOT EXISTS (select * from FormulaTaskStatistic fts2 where PTV.VendorGuid = fts2.VendorGuid and PTV.projecttaskid = fts2.projecttaskid and fts2.type=10)

--SELECT * FROM #TEMP2


----update FTS WITH TYPE AS 10 FOR CURRENLTY WORKING VENDOR having TYPE = 1
update fts
set fts.completed = getutcdate(),fts.type = 10
FROM #TEMP2 t2
INNER JOIN  FormulaTaskStatistic FTS ON  t2.t1_taskid = FTS.ProjectTaskId and t2.t1_vendorguid = fts.vendorguid
WHERE FTS.Type = 1 and fts.Completed is  null
PRINT '1'

--UPDATE THE STATUS OF CURRENT VENDOR TO REFLECT JOB LOST DUE TO OVERTIME IN PTV
update PTV
set PTV.status = 10
from #temp2 t2 
INNER JOIN  FormulaTaskStatistic FTS ON  t2.t1_taskid = FTS.ProjectTaskId and t2.t1_vendorguid = fts.vendorguid AND FTS.TYPE =10
INNER JOIN ProjectTaskVendor PTV ON FTS.ProjectTaskId = PTV.ProjectTaskId AND fts.vendorguid = PTV.VendorGuid
WHERE PTV.STATUS = 5
PRINT '2'

--UPDATE THE PROJECTTASK TBL TO REFLECT NO ONE IS CURRRENTLY WORKING AND TO RESET THE STARTDATE AND DUE DATE ACCORDINGLY FOR THE CURRENT VENDOR
UPDATE PT
SET PT.STATUS = 1, PT.ProccessingUserGuid = NULL, PT.STARTDATE = GETUTCDATE(), PT.DUEDATE = DATEADD(MINUTE,PT.Duration,GETUTCDATE()), PT.LastUpdated = GETUTCDATE()
FROM #TEMP2 T2
INNER JOIN  FormulaTaskStatistic FTS ON  t2.t1_taskid = FTS.ProjectTaskId and t2.t1_vendorguid = fts.vendorguid AND FTS.TYPE =10
INNER JOIN PROJECTTASK PT ON FTS.ProjectTaskId = PT.Id 
PRINT '3'


--UPDATE THE STATUS OF NEW VENDORS TO REFLECT INVITE SENT IN PTV
update PTV
set PTV.status = 4
from #temp2 t2 
INNER JOIN ProjectTaskVendor PTV ON T2.T1_TASKID = PTV.ProjectTaskId AND T2.PTV_VENDORGUID = PTV.VendorGuid
WHERE PTV.STATUS in ( 1,3) --may need to change this
PRINT '4'



	--UPDATE FTS ENTRY FOR NEW VENDOR (PREVIOUS CERITIFED VENDORS) HAVING TYPE = 0 , SET CREATED AS GETUTCDATE() ALL ON BASIS OF PROJECTTASK ID
	update fts
	set fts.created = getutcdate()
	FROM #TEMP2 t2
	INNER JOIN  FormulaTaskStatistic FTS ON  t2.t1_taskid = FTS.ProjectTaskId and t2.PTV_vendorguid = fts.vendorguid
	WHERE FTS.Type = 0 and fts.completed is null
	AND NOT EXISTS (SELECT * FROM FORMULATASKSTATISTIC FTS3 WHERE FTS.PROJECTTASKID = FTS3.PROJECTTASKID AND FTS.VendorGuid = FTS3.VendorGuid AND FTS3.Type = 10)
	PRINT '5'

	-- UPDATE FTS ENTRIES FOR NEW VENDORS (PREVIOUS CERITIFED VENDORS)  WITH TYPE = 9 TO TYPE =11
	update fts
	set fts.completed = getutcdate(),fts.type = 11
	FROM #TEMP2 t2
	INNER JOIN  FormulaTaskStatistic FTS ON  t2.t1_taskid = FTS.ProjectTaskId and t2.PTV_vendorguid = fts.vendorguid
	WHERE FTS.Type = 9
	and exists (select * from formulataskstatistic fts2 where fts.ProjectTaskId = fts2.projecttaskid and fts.VendorGuid = fts2.VendorGuid and fts2.Type = 0 and fts2.completed is null )
	AND NOT EXISTS (SELECT * FROM FORMULATASKSTATISTIC FTS3 WHERE FTS.PROJECTTASKID = FTS3.PROJECTTASKID AND FTS.VendorGuid = FTS3.VendorGuid AND FTS3.Type = 10)
	PRINT '6'

--IF NOT EXISTS (SELECT * FROM FormulaTaskStatistic FTS INNER JOIN #TEMP2 T2 ON FTS.PROJECTTASKID = T2.T1_TASKID AND FTS.VendorGuid = T2.PTV_VendorGuid  )
IF  EXISTS (select t2.ptv_vendorguid from #temp2 t2 
			left join formulataskstatistic fts on t2.PTV_Projecttaskid = fts.ProjectTaskId and t2.ptv_vendorguid = fts.vendorguid
			where  fts.vendorguid is null)

	BEGIN	
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

		SELECT DISTINCT null AS COMPLETED
		,getutcdate() as Created
		,t2.T1_Formulataskid as  FormulaTaskId
		,null as FormulaTaskStatisticId
		,t2.PTV_Projecttaskid as ProjectTaskId
		,0 AS TYPE
		--,DATEDIFF(MINUTE,CAST(FTS.Created AS DATETIME),GETUTCDATE()) AS VALUE
		,NULL AS VALUE
		,t2.PTV_VendorGuid as VendorGuid
		FROM #TEMP2 t2	
		left join formulataskstatistic fts on t2.PTV_Projecttaskid = fts.ProjectTaskId and t2.ptv_vendorguid = fts.vendorguid
		where  fts.vendorguid is null 

		print 'new record inserted'
		
	END


COMMIT TRANSACTION
END TRY

BEGIN CATCH
 IF @@TRANCOUNT >0
	ROLLBACK TRANSACTION

 PRINT 'TRANSACTION ROLLED BACK'
END CATCH

END";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[uspReAssignVendorTasksforAllVendors];");
        }
    }
}
