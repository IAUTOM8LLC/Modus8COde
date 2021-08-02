using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class ModifySPGetAllTasksForUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sqlQuery = @"
/****** Object:  StoredProcedure [dbo].[uspGetAllTasksForUsers]    Script Date: 21-10-2020 17:50:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================              
-- Author:  Nikhil Sehgal    
-- Create date: 10/08/2020              
-- Description: To get INFOR about specific OWNER/MANAGER wrt PROJECTS AND TASKS       
-- EXEC [dbo].[uspGetAllTasksForUsers] '44EF029C-60B8-466A-0A26-08D538EEAC75'    

-- =============================================             

ALTER PROCEDURE [dbo].[uspGetAllTasksForUsers]
@INPUT_PRIMARYUSERID [uniqueidentifier]

AS
BEGIN


SELECT DISTINCT A.*,CASE WHEN N.IsRead IS NOT NULL THEN N.IsRead ELSE 1 END AS IS_READ 
, CASE WHEN A.Status = 'OVERDUE' THEN 1
	WHEN A.STATUS = 'ATRISK' THEN 2
	WHEN A.STATUS = 'TODO' THEN 3
	WHEN A.STATUS = 'REVIEW' THEN 4
	WHEN A.STATUS = 'COMPLETED' THEN 5
	WHEN A.STATUS = 'UPCOMING' THEN 6

	END AS STATUS_ENUM
 FROM (
SELECT DISTINCT 
 ROLES.Id AS PRIMARYUSERID, ROLES.FullName AS PRIMARYUSER_FULLNAME, ROLES.ROLE AS PRIMARYUSER_ROLE , ROLES.OwnerId AS PRIMARYOWNERID
,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.PROJECTTASKID
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.PROJECTTASKID
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.PROJECTTASKID
		ELSE NULL END AS PROJECTTASKID
		
,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.PROJECTTASKTITLE
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.PROJECTTASKTITLE
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.PROJECTTASKTITLE
		ELSE NULL END AS PROJECTTASK

,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.Status
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.Status
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.Status
		ELSE NULL END AS Status
,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.FormulaName
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.FormulaName
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.FormulaName
		ELSE NULL END AS FORMULA
,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.PROJECTID
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.PROJECTID
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.PROJECTID
		ELSE NULL END AS PROJECTID
,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.PROJECTNAME
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.PROJECTNAME
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.PROJECTNAME
		ELSE NULL END AS PROJECT
,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.Id
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.Id
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.Id
		ELSE NULL END AS ASSIGNED
,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.FullName
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.FullName
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.FullName
		ELSE NULL END AS ASSIGNED_NAME
,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.ROLE
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.ROLE
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.ROLE
		ELSE NULL END AS ASSIGNED_ROLE
, 0 AS AVGTAT
,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.StartDate
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.StartDate
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.StartDate
		ELSE NULL END AS START
,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.ETA
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.ETA
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.ETA
		ELSE NULL END AS ETA

--,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.ProjectTaskUserType
--		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.ProjectTaskUserType
--		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.ProjectTaskUserType
--		ELSE NULL END AS ProjectTaskUserType
,CASE	WHEN ROLES.ROLE = 'Owner' then OWNERTASKS.UserProfilePicPath
		WHEN ROLES.ROLE = 'Manager' then MANAGERTASKS.UserProfilePicPath
		WHEN ROLES.ROLE = 'Worker' then WORKERTASKS.UserProfilePicPath
		ELSE NULL END AS UserProfilePicPath

FROM
 (
SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,anu.OwnerId
	FROM AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	--WHERE ANU.Id =  '44EF029C-60B8-466A-0A26-08D538EEAC75' --FOR TESTING
	WHERE ANU.Id =  @INPUT_PRIMARYUSERID
) ROLES

LEFT JOIN 
( 
--USERS UNDER OWNER ASSGINED TASKS
	SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,anu.OwnerId
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
         WHEN (pt.[Status] = 1 and ( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) > pt.Duration )) THEN 'OVERDUE' 
         WHEN (pt.[Status] = 1 and  ((CAST( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) AS INT) >= ( CAST( ROUND(( pt.Duration * 0.8 ), 0) AS INT) ) ) AND ( CAST(DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE() ) AS INT) <= CAST(pt.Duration AS int)) )) THEN 'ATRISK' 
         WHEN pt.[Status] = 1 THEN 'TODO'
		 WHEN pt.[Status] = 2 THEN 'REVIEW'
		 WHEN pt.[Status] = 3 THEN 'COMPLETED'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 0
	INNER JOIN ProjectTask PT ON PTU.PROJECTTASKID = PT.ID AND pt.Status in  (1,3) AND PTU.UserId = PT.ProccessingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId

	UNION --SELF OWNER ASSIGNED TASKS

	SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,anu.Id AS OwnerId--USERID USED INSTEAD OF OWNERID FOR SELF OWNER
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
         WHEN (pt.[Status] = 1 and ( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) > pt.Duration )) THEN 'OVERDUE' 
         WHEN (pt.[Status] = 1 and  ((CAST( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) AS INT) >= ( CAST( ROUND(( pt.Duration * 0.8 ), 0) AS INT) ) ) AND ( CAST(DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE() ) AS INT) <= CAST(pt.Duration AS int)) )) THEN 'ATRISK' 
         WHEN pt.[Status] = 1 THEN 'TODO'
		 WHEN pt.[Status] = 2 THEN 'REVIEW'
		 WHEN pt.[Status] = 3 THEN 'COMPLETED'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 0
	INNER JOIN ProjectTask PT ON PTU.PROJECTTASKID = PT.ID AND pt.Status in  (1,3) AND PTU.UserId = PT.ProccessingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	WHERE ANU.OwnerId IS NULL AND anr.Name = 'Owner' 
	--AND ANU.ID = '44EF029C-60B8-466A-0A26-08D538EEAC75'   
	 


	UNION --TO BE REVIEWED TASKS FOR USERS UNDER OWNER

	SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,anu.OwnerId
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
         WHEN (pt.[Status] = 1 and ( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) > pt.Duration )) THEN 'OVERDUE' 
         WHEN (pt.[Status] = 1 and  ((CAST( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) AS INT) >= ( CAST( ROUND(( pt.Duration * 0.8 ), 0) AS INT) ) ) AND ( CAST(DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE() ) AS INT) <= CAST(pt.Duration AS int)) )) THEN 'ATRISK' 
         WHEN pt.[Status] = 1 THEN 'TODO'
		 WHEN pt.[Status] = 2 THEN 'REVIEW'
		 WHEN pt.[Status] = 3 THEN 'COMPLETED'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 1
	INNER JOIN ProjectTask PT ON PTU.PROJECTTASKID = PT.ID AND pt.Status = 2 AND PTU.UserId = PT.ReviewingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId

	UNION --TO BE REVIEWED TASKS FOR SELF OWNER

	SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,anu.Id AS OwnerId
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
         WHEN (pt.[Status] = 1 and ( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) > pt.Duration )) THEN 'OVERDUE' 
         WHEN (pt.[Status] = 1 and  ((CAST( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) AS INT) >= ( CAST( ROUND(( pt.Duration * 0.8 ), 0) AS INT) ) ) AND ( CAST(DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE() ) AS INT) <= CAST(pt.Duration AS int)) )) THEN 'ATRISK' 
         WHEN pt.[Status] = 1 THEN 'TODO'
		 WHEN pt.[Status] = 2 THEN 'REVIEW'
		 WHEN pt.[Status] = 3 THEN 'COMPLETED'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 1
	INNER JOIN ProjectTask PT ON PTU.PROJECTTASKID = PT.ID AND pt.Status = 2 AND PTU.UserId = PT.ReviewingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	WHERE ANU.OwnerId IS NULL AND anr.Name = 'Owner' 


	
	UNION --TO GET VENDOR PROJECTS BASED ON PT.OWNERID

	--ACTIVE STATUS VENDOR PROJECTS

	SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,PT.OwnerGuid AS OwnerId --PROJECTTASKOWNER INCLUDED HERE
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	,CASE WHEN (DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETDATE()) > PT.Duration) THEN 'OVERDUE'
	 WHEN (	(DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETDATE()) >= (CAST(ROUND((PT.DURATION*0.8),0) AS INT)) )	AND 	(DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETDATE()) <= PT.Duration)	) THEN 'ATRISK'
	ELSE 'ACTIVE' END AS STATUS
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	--INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 1
	ProjectTask PT
	INNER JOIN PROJECTTASKVENDOR PTV ON PT.Id = PTV.ProjectTaskId AND PTV.Status = 5
	INNER JOIN FormulaTaskStatistic FTS ON PTV.ProjectTaskId=FTS.ProjectTaskId AND PTV.VendorGuid = FTS.VendorGuid  
	INNER JOIN Project p ON pt.ProjectId = p.Id
	INNER JOIN AspNetUsers anu ON FTS.VendorGuid = ANU.Id --BASED ON VENDORGUID OF FTS
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name='Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	WHERE FTS.Type = 1 AND FTS.Completed IS NULL
	AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6


	UNION 

	--COMPLETED STATUS VENDOR PROEJCTS

	SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,PT.OwnerGuid AS OwnerId --PROJECTTASKOWNER INCLUDED HERE
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	,'COMPLETED' AS STATUS
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	--INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 1
	ProjectTask PT
	INNER JOIN PROJECTTASKVENDOR PTV ON PT.Id = PTV.ProjectTaskId AND PTV.Status = 5
	INNER JOIN FormulaTaskStatistic FTS ON PTV.ProjectTaskId=FTS.ProjectTaskId AND PTV.VendorGuid = FTS.VendorGuid  
	INNER JOIN Project p ON pt.ProjectId = p.Id
	INNER JOIN AspNetUsers anu ON FTS.VendorGuid = ANU.Id --BASED ON VENDORGUID OF FTS
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name='Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	WHERE FTS.Type = 1 AND FTS.Completed IS NOT NULL
	AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6

	UNION 

	--IN-HOUSE USERS UNDER OWNER or self owner ASSIGNED TASKS FOR UPCOMING TASKS
	SELECT DISTINCT 
	NULL AS ID, NULL AS FullName, NULL AS ROLE
	--anu.Id,up.FullName, anr.Name AS ROLE
	,CASE WHEN anu.OwnerId IS NOT NULL THEN anu.OwnerId WHEN anu.OwnerId IS NULL THEN ANU.ID END AS OwnerId --NEW CASE ADDED TO HANDLE SELF OWNER OR TEAM UNDER OWNER
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
		 WHEN pt.[Status] = 0 THEN 'UPCOMING'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 0
	INNER JOIN ProjectTask PT ON PTU.PROJECTTASKID = PT.ID AND pt.Status in  (0) --AND PTU.UserId = PT.ProccessingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	WHERE P.DateCreated >= DATEADD(DAY,-7,GETUTCDATE())

	UNION

	--VENDOR ASSIGNED UNDER OWNER ASSIGNED TASKS FOR UPCOMING TASKS
	SELECT DISTINCT 
	NULL AS ID, NULL AS FullName, NULL AS ROLE
	--anu.Id,up.FullName, anr.Name AS ROLE
	--,CASE WHEN anu.OwnerId IS NOT NULL THEN anu.OwnerId WHEN anu.OwnerId IS NULL THEN ANU.ID END AS OwnerId --NEW CASE ADDED TO HANDLE SELF OWNER OR TEAM UNDER OWNER
	,PT.OWNERGUID AS OWNERID --CHANGED SINCE IT IS FOR VENDOR
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
		 WHEN pt.[Status] = 0 THEN 'UPCOMING'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name='Vendor' --CHANGED SINCE IT IS ONLY FOR VENDORS
	INNER JOIN UserProfile up on up.UserId=anu.Id
	--INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 0
	INNER JOIN ProjectTaskVendor PTV ON ANU.Id = PTV.VendorGuid
	INNER JOIN ProjectTask PT ON PTV.PROJECTTASKID = PT.ID AND pt.Status in  (0) --AND PTU.UserId = PT.ProccessingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	WHERE P.DateCreated >= DATEADD(DAY,-7,GETUTCDATE())

	
) OWNERTASKS
ON ROLES.Id = OWNERTASKS.OwnerId AND ROLES.ROLE = 'Owner'



LEFT JOIN 
(
	SELECT DISTINCT  
	 anu.Id
	,up.FullName, anr.Name AS ROLE,anu.OwnerId
	,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
         WHEN (pt.[Status] = 1 and ( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) > pt.Duration )) THEN 'OVERDUE' 
         WHEN (pt.[Status] = 1 and  ((CAST( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) AS INT) >= ( CAST( ROUND(( pt.Duration * 0.8 ), 0) AS INT) ) ) AND ( CAST(DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE() ) AS INT) <= CAST(pt.Duration AS int)) )) THEN 'ATRISK' 
         WHEN pt.[Status] = 1 THEN 'TODO'
		 WHEN pt.[Status] = 2 THEN 'REVIEW'
		 WHEN pt.[Status] = 3 THEN 'COMPLETED'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 0
	INNER JOIN ProjectTask PT ON PTU.PROJECTTASKID = PT.ID AND pt.Status in  (1,3) AND PTU.UserId = PT.ProccessingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId

	UNION

	SELECT DISTINCT  
	 anu.Id
	,up.FullName, anr.Name AS ROLE,anu.OwnerId
	,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
         WHEN (pt.[Status] = 1 and ( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) > pt.Duration )) THEN 'OVERDUE' 
         WHEN (pt.[Status] = 1 and  ((CAST( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) AS INT) >= ( CAST( ROUND(( pt.Duration * 0.8 ), 0) AS INT) ) ) AND ( CAST(DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE() ) AS INT) <= CAST(pt.Duration AS int)) )) THEN 'ATRISK' 
         WHEN pt.[Status] = 1 THEN 'TODO'
		 WHEN pt.[Status] = 2 THEN 'REVIEW'
		 WHEN pt.[Status] = 3 THEN 'COMPLETED'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 1
	INNER JOIN ProjectTask PT ON PTU.PROJECTTASKID = PT.ID AND pt.Status = 2 AND PTU.UserId = PT.ReviewingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId

	--TO GET VENDOR PROJECTS BASED ON PT.OWNERID
	UNION 

	--ACTIVE STATUS VENDOR PROJECTS
	SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,PT.OwnerGuid AS OwnerId --PROJECTTASKOWNER INCLUDED HERE
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	,CASE WHEN (DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETDATE()) > PT.Duration) THEN 'OVERDUE'
	 WHEN (	(DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETDATE()) >= (CAST(ROUND((PT.DURATION*0.8),0) AS INT)) )	AND 	(DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETDATE()) <= PT.Duration)	) THEN 'ATRISK'
	ELSE 'ACTIVE' END AS STATUS
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	--INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 1
	ProjectTask PT
	INNER JOIN PROJECTTASKVENDOR PTV ON PT.Id = PTV.ProjectTaskId AND PTV.Status = 5
	INNER JOIN FormulaTaskStatistic FTS ON PTV.ProjectTaskId=FTS.ProjectTaskId AND PTV.VendorGuid = FTS.VendorGuid  
	INNER JOIN Project p ON pt.ProjectId = p.Id
	INNER JOIN AspNetUsers anu ON FTS.VendorGuid = ANU.Id --BASED ON VENDORGUID OF FTS
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name='Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	WHERE FTS.Type = 1 AND FTS.Completed IS NULL
	AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6

	UNION 

	--COMPLETED STATUS VENDOR PROEJCTS

	SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,PT.OwnerGuid AS OwnerId --PROJECTTASKOWNER INCLUDED HERE
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	,'COMPLETED' AS STATUS
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	--INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 1
	ProjectTask PT
	INNER JOIN PROJECTTASKVENDOR PTV ON PT.Id = PTV.ProjectTaskId AND PTV.Status = 5
	INNER JOIN FormulaTaskStatistic FTS ON PTV.ProjectTaskId=FTS.ProjectTaskId AND PTV.VendorGuid = FTS.VendorGuid  
	INNER JOIN Project p ON pt.ProjectId = p.Id
	INNER JOIN AspNetUsers anu ON FTS.VendorGuid = ANU.Id --BASED ON VENDORGUID OF FTS
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name='Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	WHERE FTS.Type = 1 AND FTS.Completed IS NOT NULL
	AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6

		UNION 

	--IN-HOUSE USERS UNDER OWNER or self owner ASSIGNED TASKS FOR UPCOMING TASKS
	SELECT DISTINCT 
	NULL AS ID, NULL AS FullName, NULL AS ROLE
	--anu.Id,up.FullName, anr.Name AS ROLE
	,CASE WHEN anu.OwnerId IS NOT NULL THEN anu.OwnerId WHEN anu.OwnerId IS NULL THEN ANU.ID END AS OwnerId --NEW CASE ADDED TO HANDLE SELF OWNER OR TEAM UNDER OWNER
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
		 WHEN pt.[Status] = 0 THEN 'UPCOMING'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 0
	INNER JOIN ProjectTask PT ON PTU.PROJECTTASKID = PT.ID AND pt.Status in  (0) --AND PTU.UserId = PT.ProccessingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	WHERE P.DateCreated >= DATEADD(DAY,-7,GETUTCDATE())

	UNION

	--VENDOR ASSIGNED UNDER OWNER ASSIGNED TASKS FOR UPCOMING TASKS
	SELECT DISTINCT 
	NULL AS ID, NULL AS FullName, NULL AS ROLE
	--anu.Id,up.FullName, anr.Name AS ROLE
	--,CASE WHEN anu.OwnerId IS NOT NULL THEN anu.OwnerId WHEN anu.OwnerId IS NULL THEN ANU.ID END AS OwnerId --NEW CASE ADDED TO HANDLE SELF OWNER OR TEAM UNDER OWNER
	,PT.OWNERGUID AS OWNERID --CHANGED SINCE IT IS FOR VENDOR
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
		 WHEN pt.[Status] = 0 THEN 'UPCOMING'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name='Vendor' --CHANGED SINCE IT IS ONLY FOR VENDORS
	INNER JOIN UserProfile up on up.UserId=anu.Id
	--INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 0
	INNER JOIN ProjectTaskVendor PTV ON ANU.Id = PTV.VendorGuid
	INNER JOIN ProjectTask PT ON PTV.PROJECTTASKID = PT.ID AND pt.Status in  (0) --AND PTU.UserId = PT.ProccessingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	WHERE P.DateCreated >= DATEADD(DAY,-7,GETUTCDATE())

		
) MANAGERTASKS
ON ROLES.OwnerId = MANAGERTASKS.OwnerId AND ROLES.ROLE = 'Manager'


LEFT JOIN 
(	--assigned users data
	SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,anu.OwnerId
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
         WHEN (pt.[Status] = 1 and ( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) > pt.Duration )) THEN 'OVERDUE' 
         WHEN (pt.[Status] = 1 and  ((CAST( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) AS INT) >= ( CAST( ROUND(( pt.Duration * 0.8 ), 0) AS INT) ) ) AND ( CAST(DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE() ) AS INT) <= CAST(pt.Duration AS int)) )) THEN 'ATRISK' 
         WHEN pt.[Status] = 1 THEN 'TODO'
		 WHEN pt.[Status] = 2 THEN 'REVIEW'
		 WHEN pt.[Status] = 3 THEN 'COMPLETED'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 0
	INNER JOIN ProjectTask PT ON PTU.PROJECTTASKID = PT.ID AND pt.Status in  (1,3) AND PTU.UserId = PT.ProccessingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId

	UNION

	--reviewing users data
	SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,anu.OwnerId
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
         WHEN (pt.[Status] = 1 and ( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) > pt.Duration )) THEN 'OVERDUE' 
         WHEN (pt.[Status] = 1 and  ((CAST( DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE()) AS INT) >= ( CAST( ROUND(( pt.Duration * 0.8 ), 0) AS INT) ) ) AND ( CAST(DATEDIFF(MINUTE, CAST(pt.StartDate AS DATETIME), GETDATE() ) AS INT) <= CAST(pt.Duration AS int)) )) THEN 'ATRISK' 
         WHEN pt.[Status] = 1 THEN 'TODO'
		 WHEN pt.[Status] = 2 THEN 'REVIEW'
		 WHEN pt.[Status] = 3 THEN 'COMPLETED'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 1
	INNER JOIN ProjectTask PT ON PTU.PROJECTTASKID = PT.ID AND pt.Status = 2 AND PTU.UserId = PT.ReviewingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	
	UNION

	--assigned IN-HOUSE users UPCOMING TASKS data
	SELECT DISTINCT anu.Id,up.FullName, anr.Name AS ROLE,anu.OwnerId
	 ,p.[Name] AS PROJECTNAME
	, P.Id AS PROJECTID
	, pt.Id AS PROJECTTASKID
	, pt.Title AS PROJECTTASKTITLE
	, CASE
		 WHEN pt.[Status] = 0 THEN 'UPCOMING'
       END AS [Status]
	, fp.[Name] AS FormulaName
	, pt.FormulaTaskId
	, pt.StartDate
	, pt.DueDate
	,DATEADD(MINUTE,PT.Duration,PT.StartDate) AS ETA
	,PT.Duration
	--,PTU.ProjectTaskUserType
	,up.Path AS UserProfilePicPath
	FROM 
	AspNetUsers anu
	INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
	INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name<>'Vendor'
	INNER JOIN UserProfile up on up.UserId=anu.Id
	INNER JOIN ProjectTaskUser PTU ON ANU.Id = PTU.UserId AND PTU.ProjectTaskUserType = 0
	INNER JOIN ProjectTask PT ON PTU.PROJECTTASKID = PT.ID AND pt.Status in  (0) --AND PTU.UserId = PT.ProccessingUserGuid
	INNER JOIN Project p ON pt.ProjectId = p.Id
	LEFT JOIN FormulaTask ft ON ft.Id = pt.FormulaTaskId
	LEFT JOIN FormulaProject fp ON fp.Id = ft.FormulaProjectId
	WHERE P.DateCreated >= DATEADD(DAY,-7,GETUTCDATE())
	
) WORKERTASKS
ON ROLES.Id = WORKERTASKS.ID AND ROLES.ROLE = 'Worker'

)A
LEFT JOIN NOTIFICATION N ON A.ASSIGNED = N.RecipientGuid AND A.PROJECTTASKID = N.TaskId AND  N.NotificationType = 8 AND N.IsRead = 0
ORDER BY STATUS_ENUM

END
";
            migrationBuilder.Sql(sqlQuery);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[uspGetFormulaMeanTat];");
        }
    }
}
