using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class Updatespusp_PublishFormula : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/****** Object:  StoredProcedure [dbo].[usp_PublishFormula]    Script Date: 15-04-2021 16:51:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================          
-- Author:  Dharminder Jasuja    
-- Updated by : Nikhil Sehgal      
-- Create date: 07-27-2020          
-- Description: To Publish/Re-publish admin vault Formula to client's account   
-- Modified by Nikhil Sehgal on 2021-02-12 made changes to insert of formulataskchecklist table and businessentityid col added		       
-- ============================================= 
DROP PROCEDURE IF EXISTS [dbo].[usp_PublishFormula] 
GO
        
CREATE PROCEDURE [dbo].[usp_PublishFormula]           
 @FormulaID INT          
 AS          
BEGIN         
BEGIN TRY    
BEGIN TRANSACTION    
          
  DECLARE @CurrentDate As DATETIME        
  SET @CurrentDate=GETDATE()        
        
 IF EXISTS ( SELECT 1 from FormulaProject Where PublicVaultFormulaProjectID=@FormulaID)          
 BEGIN          
    UPDATE FPClient          
    SET FPClient.Description=FPAdmin.Description          
    ,FPClient.LastUpdated=@CurrentDate          
    ,FPClient.Name=FPAdmin.Name          
    ,FPClient.IsGlobal=1       
    ,FPCLIENT.Status = 5 --NEWLY ADDED    
    FROM FormulaProject FPClient          
    INNER JOIN FormulaProject FPAdmin          
    ON FPClient.PublicVaultFormulaProjectID=FPAdmin.Id          
    WHERE FPAdmin.ID=@FormulaID      
    print '1'    
          
    --select 'update'          
 END          
 ELSE           
 BEGIN          
          
 INSERT INTO FormulaProject(DateCreated,IsDeleted,Description,Name,OwnerGuid,ShareType, IsGlobal,PublicVaultFormulaProjectID,Status)          
 SELECT DISTINCT @CurrentDate,0,FP.Description,FP.Name,FP.OwnerGuid,0,1,FP.ID , 5          
 FROM FormulaProject FP    
 --Cross Join           
 --(SELECT AspNetUsers.Id AS OwnerGuid          
 --from AspNetUsers          
 --inner join AspNetUserRoles on AspNetUserRoles.UserId = AspNetUsers.Id          
 --inner join AspNetRoles on AspNetRoles.Id = AspNetUserRoles.RoleId          
 --WHERE AspNetRoles.[Name]='Owner')  Users          
 WHERE  FP.id=@FormulaID          
       print '2'    
 --select 'insert'          
 END          
          
 --Update Team table if already exist          
           
 --UPDATE TClient          
 --SET TClient.Name=TAdmin.Name      
 --,TClient.LastUpdated=@CurrentDate     
 --,tclient.Status = 5 --newly added    
 --FROM Team TClient          
 --INNER JOIN Team TAdmin          
 --ON TClient.PublicVaultTeamID=TAdmin.ID       
 --AND TClient.OwnerGuid = TAdmin.OwnerGuid --NEWLY ADDED BY NIK       
 --INNER JOIN FormulaTask FT          
 --ON FT.TeamId=TAdmin.ID          
 --INNER JOIN FormulaProject FP          
 --ON FP.Id=FT.FormulaProjectId          
 --WHERE FP.ID=@FormulaID      AND TClient.Name!=TAdmin.Name    
 --AND tclient.Status = 5 --NEWLY ADDED BY NIK    
 --     print '3'    
   
          
 -- Insert Team entries for all the owner Users if it does not exist already          
           
 --INSERT INTO Team(Name,OwnerGuid,DateCreated,IsGlobal,PublicVaultTeamID,Status)          
 --SELECT DISTINCT TAdmin.Name,TAdmin.OwnerGuid,@CurrentDate,1,TAdmin.Id  ,5         
 --FROM Team TAdmin          
 --INNER JOIN FormulaTask FT          
 --ON FT.TeamId=TAdmin.ID          
 --INNER JOIN FormulaProject FP          
 --ON FP.Id=FT.FormulaProjectId          
 ----Cross Join           
 ----(SELECT AspNetUsers.Id AS OwnerGuid          
 ----from AspNetUsers          
 ----inner join AspNetUserRoles on AspNetUserRoles.UserId = AspNetUsers.Id          
 ----inner join AspNetRoles on AspNetRoles.Id = AspNetUserRoles.RoleId          
 ----where AspNetRoles.[Name]='Owner')  Users          
 --LEFT OUTER Join Team TClient          
 --ON TClient.publicVaultTeamID=TAdmin.ID          
 --AND TAdmin.OwnerGuid=TClient.OwnerGuid    --NEWLY ADDED BY NIK      
 --WHERE FP.ID=@FormulaID          
 --AND TClient.Id IS NULL       
 --print '4'    
          
 --Delete the Team entries which exist in Client account but deleted in Admin Record          
          
 --Select * from Skill          
          
 --Update Skill table if already exist          
           
 --UPDATE SClient          
 --SET SClient.Name=SAdmin.Name       
 --, SClient.LastUpdated=@CurrentDate     
 --, SCLIENT.Status = 5 --NEWLY ADDED    
 --FROM Skill SClient          
 --INNER JOIN Skill SAdmin          
 --ON SClient.PublicVaultSkillID=SAdmin.ID       
 --AND SAdmin.OwnerGuid = SClient.OwnerGuid --NEWLY ADDED  BY NIK      
 --INNER JOIN FormulaTask FT          
 --ON FT.AssignedSkillId=SAdmin.ID          
 --INNER JOIN FormulaProject FP          
 --ON FP.Id=FT.FormulaProjectId          
 --WHERE FP.ID=@FormulaID  AND SClient.Name!=SAdmin.Name     
 --AND SCLIENT.Status = 5--NEWLY ADDED BY NIK    
 -- print '5'    
          
          
 ---- Insert Skill entries for all the owner Users if it does not exist already          
 --INSERT INTO Skill(Name,OwnerGuid,DateCreated,IsGlobal,PublicVaultSkillID,Status)          
 --SELECT DISTINCT SAdmin.Name,SAdmin.OwnerGuid,@CurrentDate,1,SAdmin.Id ,5          
 --FROM Skill SAdmin          
 --INNER JOIN FormulaTask FT          
 --ON FT.AssignedSkillId=SAdmin.ID          
 --INNER JOIN FormulaProject FP         
 --ON FP.Id=FT.FormulaProjectId          
 ----Cross Join           
 ----(SELECT AspNetUsers.Id AS OwnerGuid          
 ----from AspNetUsers          
 ----inner join AspNetUserRoles on AspNetUserRoles.UserId = AspNetUsers.Id          
 ----inner join AspNetRoles on AspNetRoles.Id = AspNetUserRoles.RoleId          
 ----where AspNetRoles.[Name]='Owner')  Users          
 --LEFT OUTER Join Skill SClient          
 --ON SClient.publicVaultSkillID=SAdmin.ID          
 --AND SAdmin.OwnerGuid=SClient.OwnerGuid  --NEWLY ADDED BY NIK     
 --WHERE FP.ID=@FormulaID          
 --AND SClient.Id IS NULL     
 -- print '6'    
          
          
 ----Update Review Skill table if already exist          
           
 --UPDATE SClient          
 --SET SClient.Name=SAdmin.Name          
 --, SClient.LastUpdated=@CurrentDate     
 --, SCLIENT.Status = 5 --NEWLY ADDED    
 --FROM Skill SClient          
 --INNER JOIN Skill SAdmin          
 --ON SClient.PublicVaultSkillID=SAdmin.ID         
 -- AND SAdmin.OwnerGuid = SClient.OwnerGuid --NEWLY ADDED  BY NIK       
 --INNER JOIN FormulaTask FT          
 --ON FT.ReviewingSkillId=SAdmin.ID          
 --INNER JOIN FormulaProject FP          
 --ON FP.Id=FT.FormulaProjectId          
 --WHERE FP.ID=@FormulaID  AND SClient.Name!=SAdmin.Name     
 --AND SCLIENT.Status = 5--NEWLY ADDED BY NIK    
 -- print '7'    
          
          
 ---- Insert Review Skill entries for all the owner Users if it does not exist already          
 --INSERT INTO Skill(Name,OwnerGuid,DateCreated,IsGlobal,PublicVaultSkillID,Status)          
 --SELECT DISTINCT SAdmin.Name,SAdmin.OwnerGuid,@CurrentDate,1,SAdmin.Id ,5          
 --FROM Skill SAdmin          
 --LEFT OUTER JOIN FormulaTask FT          
 --ON FT.ReviewingSkillId=SAdmin.ID          
 --INNER JOIN FormulaProject FP          
 --ON FP.Id=FT.FormulaProjectId          
 ----Cross Join           
 ----(SELECT AspNetUsers.Id AS OwnerGuid          
 ----from AspNetUsers          
 ----inner join AspNetUserRoles on AspNetUserRoles.UserId = AspNetUsers.Id          
 ----inner join AspNetRoles on AspNetRoles.Id = AspNetUserRoles.RoleId          
 ----where AspNetRoles.[Name]='Owner')  Users          
 --LEFT OUTER Join Skill SClient          
 --ON SClient.publicVaultSkillID=SAdmin.ID          
 --AND SAdmin.OwnerGuid=SClient.OwnerGuid  --NEWLY ADDED BY NIK          
 --WHERE FP.ID=@FormulaID          
 --AND SClient.Id IS NULL        
 -- print '8'    
          
        
        
 --Update formulatask table if already exist          
           
UPDATE FTClient          
 SET FTClient.Description=FTAdmin.Description          
 ,FTClient.Duration=FTAdmin.Duration          
 ,FTClient.PosX=FTAdmin.posX          
 ,FTClient.PosY=FTAdmin.PosY          
 ,FTClient.TaskConditionId=FTAdmin.TaskConditionId          
 ,FTClient.Title=FTAdmin.Title          
 ,FTClient.IsAutomated=FTAdmin.IsAutomated          
 ,FTClient.RecurrenceOptionsId=FTAdmin.RecurrenceOptionsId          
 ,FTClient.StartDelay=FTAdmin.StartDelay          
 ,FTClient.IsInterval=FTAdmin.IsInterval          
 ,FTClient.IsShareResources=FTAdmin.IsShareResources          
 ,FTClient.AssignedSkillId=FTADMIN.AssignedSkillId         
 ,FTClient.ReviewingSkillId=FTADMIN.ReviewingSkillId          
 ,FTClient.TeamId=FTADMIN.TeamId         
 ,FTClient.DescNotificationFlag=FTAdmin.DescNotificationFlag    
 ,FTClient.ReviewerTraining=FTAdmin.ReviewerTraining      
 ,FTCLIENT.InternalFormulaProjectId = FTADMIN.InternalFormulaProjectId --NEWLY ADDED
 ,FTCLIENT.IsTrainingLocked = FTADMIN.IsTrainingLocked --NEWLLY ADDED
 ,FTCLIENT.OriginalFormulaTaskId = FTADMIN.OriginalFormulaTaskId --NEWLY ADDED
  FROM FormulaTask FTADMIN    --complete new join set added by nik
 LEFT JOIN FormulaTask FTCLIENT ON FTADMIN.Id = FTCLIENT.PublicVaultFormulaTaskID    
 --INNER JOIN TEAM TADMIN ON FTADMIN.TeamId = TADMIN.Id    
 --INNER JOIN TEAM TCLIENT ON TCLIENT.PublicVaultTeamID = TADMIN.Id AND TADMIN.OwnerGuid = TCLIENT.OwnerGuid AND TCLIENT.Status = 5     
 --INNER JOIN SKILL SADMIN ON FTADMIN.AssignedSkillId  = SADMIN.Id    
 --INNER JOIN SKILL SCLIENT ON SCLIENT.PublicVaultSkillID = SADMIN.Id AND SADMIN.OwnerGuid = SCLIENT.OwnerGuid AND SCLIENT.Status = 5    
 --LEFT JOIN Skill SREVIEWADMIN ON FTADMIN.ReviewingSkillId = SREVIEWADMIN.Id    
 --LEFT JOIN SKILL SREVIEWCLIENT ON SREVIEWCLIENT.PublicVaultSkillID = SREVIEWADMIN.Id AND SREVIEWADMIN.OwnerGuid = SREVIEWCLIENT.OwnerGuid AND SREVIEWCLIENT.Status = 5    
 WHERE FTAdmin.FormulaProjectId=@FormulaID      
     
     
       print '9'    
  --Select * from Formulatask         
          
 -- Insert Skill entries for all the owner Users if it does not exist already          
 INSERT INTO FormulaTask(Description,Duration,FormulaProjectId,PosX,PosY,TaskConditionId,Title,DateCreated,OwnerGuid,IsAutomated,RecurrenceOptionsId          
 ,StartDelay,IsInterval,IsShareResources,AssignedSkillId,ReviewingSkillId,DescNotificationFlag,TeamId,IsGlobal,PublicVaultFormulaTaskID,ReviewerTraining
 ,InternalFormulaProjectId,OriginalFormulaTaskId,IsTrainingLocked)       --NEWLY ADDED    
 SELECT DISTINCT FTAdmin.Description          
 ,FTAdmin.Duration          
 ,FPClient.Id          
 ,FTAdmin.PosX          
 ,FTAdmin.PosY          
 ,FTAdmin.TaskConditionId          
 ,FTAdmin.Title          
 ,@CurrentDate          
 --,TClient.OwnerGuid  --PREVIOUS    
 ,FTAdmin.OwnerGuid --NEWLY ADDED    
 ,FTAdmin.IsAutomated          
 ,FTAdmin.RecurrenceOptionsId          
 ,FTAdmin.StartDelay          
 ,FTAdmin.IsInterval          
 ,FTAdmin.IsShareResources          
 ,FTAdmin.AssignedSkillId        
 ,FTAdmin.ReviewingSkillId        
 ,FTAdmin.DescNotificationFlag          
 ,FTAdmin.TeamId         
 ,1          
 ,FTAdmin.ID          
 ,FTAdmin.ReviewerTraining        
 ,FTAdmin.InternalFormulaProjectId --NEWLY ADDED
 ,FTADMIN.OriginalFormulaTaskId --NEWLY ADDED
 ,FTADMIN.IsTrainingLocked --NEWLY ADDED
  FROM  FormulaTask FTAdmin   --COMPLETE NEW SET ADDED BY NIK       
 --INNER JOIN TEAM TADMIN ON FTAdmin.TeamId = TADMIN.Id    
 --INNER JOIN Team TClient  ON TADMIN.Id=TClient.PublicVaultTeamID  AND TADMIN.OwnerGuid = TClient.OwnerGuid  AND TClient.Status = 5--NEWLY ADDED BY NIK        
 --INNER JOIN SKILL SADMIN ON FTAdmin.AssignedSkillId = SADMIN.Id    
 --INNER JOIN Skill SClient   ON SADMIN.Id=SClient.PublicVaultSkillID  AND SADMIN.OwnerGuid = SClient.OwnerGuid AND SClient.Status = 5      
 --LEFT JOIN Skill SReviewADMIN ON FTAdmin.ReviewingSkillId = SReviewADMIN.Id    
 --LEFT OUTER JOIN Skill SReviewClient ON SReviewADMIN.Id=SReviewClient.PublicVaultSkillID   AND SReviewADMIN.OwnerGuid = SReviewClient.OwnerGuid AND SReviewClient.Status = 5      
 INNER JOIN FormulaProject FPAdmin   ON FPAdmin.id=FTAdmin.FormulaProjectId          
 INNER JOIN FormulaProject FPClient  ON FPClient.PublicVaultFormulaProjectID=FPAdmin.ID       
 LEFT JOIN FormulaTask FTCLIENT ON FTAdmin.Id = FTCLIENT.PublicVaultFormulaTaskID        
 --AND FPClient.OwnerGuid=TClient.OwnerGuid          
 WHERE FTAdmin.FormulaProjectId=@FormulaID     
 AND FTCLIENT.Id IS NULL    
      
  print '10'    

-- --DELETEE FROM FORMULA TASK VENDOR BEFORE DELETING FROM FORMULATASSK TABLE  --ORIGINAL  
--DELETE FROM FORMULATASKVENDOR      
--WHERE FormulaTaskId IN (    
--    SELECT DISTINCT FTCLIENT.Id    
--    FROM FormulaTask FTCLIENT    
--    INNER JOIN FormulaProject FPCLIENT ON FTCLIENT.FormulaProjectId = FPCLIENT.ID    
--    LEFT JOIN FORMULATASK FTADMIN ON FTCLIENT.PublicVaultFormulaTaskID = FTADMIN.Id    
--    WHERE FPCLIENT.PublicVaultFormulaProjectID =  @FormulaID    
--    AND FTADMIN.Id IS NULL    
--)    
-- print '11'    
 

  --DELETEE FROM FORMULA TASK VENDOR BEFORE DELETING FROM FORMULATASSK TABLE    

DELETE FTVCLIENT
 FROM FORMULATASKVENDOR FTVCLIENT
	INNER JOIN
	(
	 SELECT FTCLIENT.ID AS FORMULATASKID,FTV.VendorGuid FROM FormulaProject FPCLIENT    
	 INNER JOIN FormulaTask FTCLIENT ON FPCLIENT.Id=FTCLIENT.FormulaProjectId    
	 INNER JOIN FormulaTask FTADMIN ON FTCLIENT.PublicVaultFormulaTaskID = FTADMIN.Id    
	 INNER JOIN FormulaTaskVendor FTV ON FTADMIN.Id = FTV.FormulaTaskId     
	 WHERE FPCLIENT.PublicVaultFormulaProjectID =  @FORMULAID  
	 ) FTVCLIENTOLD ON FTVCLIENT.FORMULATASKID = FTVCLIENTOLD.FORMULATASKID AND FTVCLIENT.VendorGuid = FTVCLIENTOLD.VendorGuid 
 print '11'    

 --delete from FormulaTaskDependency(ParentTaskId,ChildTaskId,Required) 
 
 
delete ftd
 from FormulaTaskDependency ftd 
 left join FormulaTask ftclientparent on ftd.parenttaskid = ftclientparent.id
 left join FormulaTask ftclientchild on ftd.ChildTaskId = ftclientchild.id
 left join FormulaTask ftadminparent on ftadminparent.Id = ftclientparent.PublicVaultFormulaTaskID
 left join FormulaTask ftadminchild on ftadminchild.Id = ftclientchild.PublicVaultFormulaTaskID
 where 
 (ftadminparent.FormulaProjectId = @FormulaID or ftadminchild.formulaprojectid = @FormulaID)
 --and (ftadminparent.id is null or ftadminchild.id is null) --commented as it was restricting data wrt sprint 10b





--DELETE ENTRIES FROM CLIENT FORMULATASK TABLE WHICH ARE ALREADY DELETED IN ADMIN    
--SELECT DISTINCT FTCLIENT.Id,FTCLIENT.PublicVaultFormulaTaskID --CHANGE TO DELETE    
DELETE FTCLIENT    
FROM FormulaTask FTCLIENT    
INNER JOIN FormulaProject FPCLIENT ON FTCLIENT.FormulaProjectId = FPCLIENT.ID    
LEFT JOIN FORMULATASK FTADMIN ON FTCLIENT.PublicVaultFormulaTaskID = FTADMIN.Id    
WHERE FPCLIENT.PublicVaultFormulaProjectID =  @FormulaID    
AND FTADMIN.Id IS NULL    
 print '12'    
 

--update formula task vendor if entries already exists

--IF EXISTS 
--(
--	SELECT FTVCLIENT.* FROM FORMULATASKVENDOR FTVCLIENT
--	INNER JOIN
--	(
--	  SELECT FTCLIENT.ID AS FORMULATASKID,FTV.VendorGuid FROM FormulaProject FPCLIENT    
--	 INNER JOIN FormulaTask FTCLIENT ON FPCLIENT.Id=FTCLIENT.FormulaProjectId    
--	 INNER JOIN FormulaTask FTADMIN ON FTCLIENT.PublicVaultFormulaTaskID = FTADMIN.Id    
--	 INNER JOIN FormulaTaskVendor FTV ON FTADMIN.Id = FTV.FormulaTaskId     
--	 WHERE FPCLIENT.PublicVaultFormulaProjectID = @FORMULAID  
--	 ) FTVCLIENTOLD ON FTVCLIENT.FORMULATASKID = FTVCLIENTOLD.FORMULATASKID AND FTVCLIENT.VendorGuid = FTVCLIENTOLD.VendorGuid
--  )

--BEGIN

--END

--ELSE

--BEGIN
--INSERTT INTO FORMULA TASK VENDOR    
INSERT INTO [dbo].[FormulaTaskVendor] --no public vault id column present in the table    
           ([Created]    
           ,[FormulaTaskId]    
           ,[LastModified]    
           ,[Price]    
           ,[Status]    
           ,[VendorGuid])    
 SELECT @CurrentDate as created    
 ,FTCLIENT.ID AS [FormulaTaskId]    
 ,FTV.LastModified    
 ,FTV.Price    
 ,FTV.Status    
 ,FTV.VendorGuid    
 FROM FormulaProject FPCLIENT    
 INNER JOIN FormulaTask FTCLIENT ON FPCLIENT.Id=FTCLIENT.FormulaProjectId    
 INNER JOIN FormulaTask FTADMIN ON FTCLIENT.PublicVaultFormulaTaskID = FTADMIN.Id    
 INNER JOIN FormulaTaskVendor FTV ON FTADMIN.Id = FTV.FormulaTaskId    
 WHERE FPCLIENT.PublicVaultFormulaProjectID = @FORMULAID    

--END

         print '13'      
 ---Select * from FormulaTask Where TeamID=690         
-- Select * from FormulaTask        
        
--INSERT INTO TeamSkill(TeamId,SkillId)        
--SELECT DISTINCT FTClient.TeamID,FTClient.AssignedSkillId        
--FROM FormulaTask FTClient        
--INNER JOIN FormulaTask FTAdmin        
--ON FTAdmin.ID=FTClient.PublicVaultFormulaTaskID        
--LEFT OUTER JOIN TeamSkill TS        
--ON TS.TeamId=FTClient.TeamID        
--AND TS.SkillId=FTClient.AssignedSkillId        
--WHERE FTAdmin.FormulaProjectId=@FormulaID          
--AND TS.TeamId IS NULL        
--AND FTClient.TeamId IS NOT NULL AND FTClient.AssignedSkillId  IS NOT NULL        
--   print '14'      
--INSERT INTO TeamSkill(TeamId,SkillId)        
--SELECT DISTINCT FTClient.TeamID,FTClient.ReviewingSkillId        
--FROM FormulaTask FTClient        
--INNER JOIN FormulaTask FTAdmin        
--ON FTAdmin.ID=FTClient.PublicVaultFormulaTaskID        
--LEFT OUTER JOIN TeamSkill TS        
--ON TS.TeamId=FTClient.TeamID        
--AND TS.SkillId=FTClient.ReviewingSkillId        
--WHERE FTAdmin.FormulaProjectId=@FormulaID          
--AND TS.TeamId IS NULL        
--AND FTClient.TeamId IS NOT NULL AND FTClient.ReviewingSkillId  IS NOT NULL        
--     print '15'    
        
        
DELETE FTC        
FROM FormulaTaskChecklist  FTC        
INNER JOIN FormulaTask FTClient        
ON FTC.FormulaTaskId=FTClient.ID        
INNER JOIN FormulaTask FTAdmin        
ON FTAdmin.ID=FTClient.PublicVaultFormulaTaskID        
WHERE FTAdmin.FormulaProjectId=@FormulaID        
     print '16'    
        
        
--INSERT INTO FormulaTaskChecklist(DateCreated,FormulaTaskId,Name,Type)        
--SELECT DISTINCT @CurrentDate,FTClient.ID,FTCAdmin.Name,FTCAdmin.Type        
--FROM FormulaTaskChecklist FTCAdmin        
--INNER JOIN FormulaTask FTAdmin        
--ON FTCAdmin.FormulaTaskId=FTAdmin.ID        
--INNER JOIN FormulaTask FTClient        
--ON FTAdmin.ID=FTClient.PublicVaultFormulaTaskID        
--WHERE FTAdmin.FormulaProjectId=@FormulaID        
--     print '17'   


INSERT INTO FormulaTaskChecklist(DateCreated,bussinessentityid,FormulaTaskId,Name,Type)  --newly added col bussinesentityid WRT  sprint10b bugs     
SELECT DISTINCT @CurrentDate,
FTCAdmin.id,
FTClient.ID,
FTCAdmin.Name,FTCAdmin.Type        
FROM FormulaTaskChecklist FTCAdmin        
INNER JOIN FormulaTask FTAdmin        
ON FTCAdmin.FormulaTaskId=FTAdmin.ID        
INNER JOIN FormulaTask FTClient        
ON FTAdmin.ID=FTClient.PublicVaultFormulaTaskID        
WHERE FTAdmin.FormulaProjectId=@FormulaID        
     print '17' 
	  
DELETE FNClient        
FROM FormulaNote FNClient        
INNER JOIN FormulaProject FPClient        
ON FNClient.FormulaId=FPClient.ID        
INNER JOIN FormulaProject FPAdmin        
ON FPAdmin.ID=FPClient.PublicVaultFormulaProjectID        
WHERE FPAdmin.ID=@FormulaID        
     print '18'   
	  
INSERT INTO FormulaNote(DateCreated,FormulaId,Text)        
SELECT DISTINCT @CurrentDate,FPClient.Id,FNAdmin.Text         
FROM FormulaNote FNAdmin        
INNER JOIN FormulaProject FPAdmin        
ON FNAdmin.FormulaID=FPAdmin.ID        
INNER JOIN FormulaProject FPClient        
ON FPClient.PublicVaultFormulaProjectID=FPAdmin.ID        
WHERE FPAdmin.ID=@FormulaID        
     print '19'    
        
INSERT INTO FormulaTaskDependency(ParentTaskId,ChildTaskId,Required)        
SELECT FTParentClient.id,FTChildClient.ID,FTD.Required        
FROM FormulaTaskDependency FTD        
INNER JOIN FormulaTask FTParentAdmin         
ON FTD.ParentTaskID=FTParentAdmin.ID        
INNER JOIN FormulaTask FTChildAdmin        
ON FTD.ChildTaskId=FTChildAdmin.ID        
AND FTChildAdmin.FormulaProjectId=FTParentAdmin.FormulaProjectId        
INNER JOIN FormulaTask FTParentClient        
ON FTParentClient.PublicVaultFormulaTaskID=FTParentAdmin.ID        
INNER JOIN FormulaTask FTChildClient        
ON FTChildClient.PublicVaultFormulaTaskID=FTChildAdmin.ID        
AND FTChildClient.FormulaProjectId=FTParentClient.FormulaProjectId        
WHERE FTParentAdmin.FormulaProjectId=@FormulaID        
AND FTChildAdmin.FormulaProjectId=@FormulaID        
     print '20'    
        
--1) Formula notes        
--2) checklist        
--3) Reviewer training        

--FOR INSERT INTO FT_DISABLE TBL OF PUBLISHED SET --NEWLY ADDED FOR FT ENABLE/DISABLE
INSERT INTO [dbo].[FormulaTaskDisableStatus](
	[ParentFormulaId] ,
	[ChildFormulaId] ,
	[InternalChildFormulaId] ,
	[InternalChildFormulaTaskId] ,
	[DateCreated] ,
	[IsDisabled] ,
	[LastUpdated])

SELECT DISTINCT fp_client.id as [ParentFormulaId] 
,ft_client.id as [ChildFormulaId] --confirm this
,ftds_admin.InternalChildFormulaid as [InternalChildFormulaId]
,ftds_admin.InternalChildformulataskid as [InternalChildFormulaTaskId]
,getutcdate() as [DateCreated]
,1 AS [IsDisabled]
,null as [LastUpdated]
from 
FormulaTaskDisableStatus ftds_admin
inner join formulatask ft_admin on ftds_admin.childformulaid = ft_admin.Id
inner join formulaproject fp_client on ftds_admin.parentformulaid = fp_client.publicvaultformulaprojectid
inner join formulatask ft_client on fp_client.Id = ft_client.FormulaProjectId and ft_admin.Id = ft_client.PublicVaultFormulaTaskID
where ftds_admin.[ParentFormulaId] = @formulaid  


-- To get the client set which does not have a corresponding admin set present in the disable table ----TO delete PUBLISHED SET when admin task is re-enabled
DELETE ftds_client from 
(
	select distinct ftds_client.[ParentFormulaId],
		ftds_client.[ChildFormulaId] ,
		ftds_client.[InternalChildFormulaId] ,
		ftds_client.[InternalChildFormulaTaskId] ,
		fp_client.PublicVaultFormulaProjectID,
		ft_client.PublicVaultFormulaTaskID
	from
	formulataskdisablestatus ftds_client
	inner join formulaproject fp_client on ftds_client.parentformulaid = fp_client.Id
	inner join formulatask ft_client on ftds_client.childformulaid = ft_client.id
	where 
	fp_client.PublicVaultFormulaProjectID is not null
	and ft_client.PublicVaultFormulaTaskID is not null
	and fp_client.PublicVaultFormulaProjectID = @Formulaid
) clientset
inner join formulataskdisablestatus ftds_client on clientset.[ParentFormulaId] = ftds_client.[ParentFormulaId] and clientset.[ChildFormulaId] = ftds_client.[ChildFormulaId] and clientset.[InternalChildFormulaId] = ftds_client.[InternalChildFormulaId] and clientset.[InternalChildFormulaTaskId] = ftds_client.[InternalChildFormulaTaskId]
left join formulataskdisablestatus ftds_admin on  clientset.PublicVaultFormulaProjectID = ftds_admin.[ParentFormulaId] and clientset.PublicVaultFormulaTaskID = ftds_admin.[ChildFormulaId] and clientset.[InternalChildFormulaId] = ftds_admin.[InternalChildFormulaId] and clientset.[InternalChildFormulaTaskId] = ftds_admin.[InternalChildFormulaTaskId]
where ftds_admin.[ParentFormulaId] is null
and ftds_admin.[ChildFormulaId] is null
and ftds_admin.[InternalChildFormulaId] is null
and ftds_admin.[InternalChildFormulaTaskId] is null     


        
UPDATE FormulaProject        
SET Status=3        
WHERE ID=@FormulaID        
 print '21'    

 --[dbo].[ResourceFormula]   ([ResourceId] ,[FormulaId])  

DELETE RF 
FROM
RESOURCEFORMULA RF
INNER JOIN FormulaProject FPCLIENT ON FPCLIENT.Id = RF.FormulaId
INNER JOIN FormulaProject FPADMIN ON FPCLIENT.PublicVaultFormulaProjectID = FPADMIN.Id
LEFT JOIN ResourceFormula RFADMIN ON RFADMIN.FormulaId = FPADMIN.ID AND RFADMIN.RESOURCEID = RF.RESOURCEID
WHERE FPADMIN.ID = @FORMULAID
AND RFADMIN.FormulaId IS NULL 
 

       
INSERT INTO [dbo].[ResourceFormula]   ([ResourceId] ,[FormulaId])    
SELECT RF.resourceid, FP.id     
from ResourceFormula RF    
INNER JOIN FormulaProject FP on RF.FormulaId = FP.PublicVaultFormulaProjectID    
where RF.FormulaId = @FormulaID    
 print '22'    
--FORMULA TASK VENDOR    
--SELECT * FROM FormulaTaskVendor 

   
--TO SEND A JIST NOTIFICATION ABOUT WHAT ALL HAPPENED    
Select 
'Formula' AS Type,NAME     
--, CASE WHEN ISNULL(LastUpdated,'1900-01-01')=@CurrentDate THEN 'UPDATED'     
--WHEN ISNULL(DateCreated,'1900-01-01')=@CurrentDate THEN 'ADDED'     
--ELSE null END AS STATUS       
,'ADDED\UPDATED' AS STATUS ,  
FP.Id as NewFormulaId, 
FT.Id as NewFormulaTaskId  ,
FT.PublicVaultFormulaTaskID as OldFormulaTaskId  
from FormulaProject FP 
inner join FormulaTask FT on FP.Id=FT.FormulaProjectId
Where FP.PublicVaultFormulaProjectID =@FormulaID      
--ORDER BY  FP.LastUpdated DESC, FP.DateCreated DESC    
--UNION ALL    
      
--Select DISTINCT 'Team' As Type, T.Name,    'ADDED\UPDATED' AS STATUS,    
--0 as NewFormulaId, 
--0 as NewFormulaTaskId  ,
--0 as OldFormulaTaskId    
----CASE WHEN ISNULL(T.LastUpdated,'1900-01-01')= @CurrentDate THEN 'UPDATED'     
----WHEN ISNULL(t.DateCreated,'1900-01-01')=@CurrentDate THEN 'ADDED'  END AS STATUS       
--from Team T      
--INNER JOIN Team TA      
--ON T.PublicVaultTeamID=TA.ID      
--INNER JOIN FormulaTask FT      
--ON FT.TeamId=TA.ID      
--WHERE FT.FormulaProjectId=@FormulaID      
      
--UNION ALL    
--Select DISTINCT 'Skill' As Type, T.Name, 'ADDED\UPDATED' AS STATUS, 
--0 as NewFormulaId, 
--0 as NewFormulaTaskId  ,
--0 as OldFormulaTaskId
---- CASE WHEN ISNULL(T.LastUpdated,'1900-01-01')= @CurrentDate THEN 'UPDATED'     
----WHEN ISNULL(t.DateCreated,'1900-01-01')=@CurrentDate THEN 'ADDED'     
----ELSE null END AS STATUS       
      
--from Skill T      
--INNER JOIN Skill TA      
--ON T.PublicVaultSkillID=TA.ID      
--INNER JOIN FormulaTask FT      
--ON FT.AssignedSkillId=TA.ID      
--WHERE FT.FormulaProjectId=@FormulaID       
 print '23'    

--Select 1 as Result    
    
COMMIT TRANSACTION    
END TRY    

BEGIN CATCH     
    IF @@TRANCOUNT >0    
    ROLLBACK TRANSACTION    
 PRINT 'TRANSACTION ROLLED BACK'    
 END CATCH    
        
        
END
";


            migrationBuilder.Sql(sql);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
