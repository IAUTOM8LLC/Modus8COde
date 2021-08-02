using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class spusp_PublishFormulaForNewOnwer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
/****** Object:  StoredProcedure [dbo].[usp_PublishFormulaForNewOnwer]    Script Date: 31-07-2020 15:20:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author: Dharminder Jasuja
-- Create date: 07-27-2020
-- Description: To Publish all admin vault Formulas to New client's account
-- =============================================
--Select * from Formul
--usp_PublishFormulaForNewOnwer '08567D93-244C-45BD-1893-08D835342E5B', '2A3B99E6-0A9F-42DD-15B5-08D7A66504B7'
CREATE PROCEDURE [dbo].[usp_PublishFormulaForNewOnwer]
@UserGuid UNIQUEIDENTIFIER,
@AdminGuid UNIQUEIDENTIFIER
AS
BEGIN


DECLARE @CurrentDate As DATETIME
SET @CurrentDate=GETDATE()


INSERT INTO FormulaProject(DateCreated,IsDeleted,Description,Name,OwnerGuid,ShareType, IsGlobal,PublicVaultFormulaProjectID)
SELECT DISTINCT @CurrentDate,0,FP.Description,FP.Name,@UserGuid,0,1,FP.ID
FROM FormulaProject FP
WHERE FP.OwnerGuid=@AdminGuid
AND FP.IsGlobal=1
AND Status in (2,4)


INSERT INTO Team(Name,OwnerGuid,DateCreated,IsGlobal,PublicVaultTeamID)
SELECT DISTINCT TAdmin.Name,@UserGuid,@CurrentDate,1,TAdmin.Id
FROM Team TAdmin
INNER JOIN FormulaTask FT
ON FT.TeamId=TAdmin.ID
INNER JOIN FormulaProject FP
ON FP.Id=FT.FormulaProjectId
WHERE FP.OwnerGuid=@AdminGuid
AND FP.IsGlobal=1
AND FP.Status in (2,4)

INSERT INTO Skill(Name,OwnerGuid,DateCreated,IsGlobal,PublicVaultSkillID)
SELECT DISTINCT SAdmin.Name,@UserGuid,@CurrentDate,1,SAdmin.Id
FROM Skill SAdmin
INNER JOIN FormulaTask FT
ON FT.AssignedSkillId=SAdmin.ID
INNER JOIN FormulaProject FP
ON FP.Id=FT.FormulaProjectId
WHERE FP.OwnerGuid=@AdminGuid




-- Insert Skill entries for all the owner Users if it does not exist already
INSERT INTO FormulaTask(Description,Duration,FormulaProjectId,PosX,PosY,TaskConditionId,Title,DateCreated,OwnerGuid,IsAutomated,RecurrenceOptionsId
,StartDelay,IsInterval,IsShareResources,AssignedSkillId,ReviewingSkillId,DescNotificationFlag,TeamId,IsGlobal,PublicVaultFormulaTaskID,ReviewerTraining)
SELECT DISTINCT FTAdmin.Description
,FTAdmin.Duration
,FPClient.Id
,FTAdmin.PosX
,FTAdmin.PosY
,FTAdmin.TaskConditionId
,FTAdmin.Title
,@CurrentDate
,TClient.OwnerGuid
,FTAdmin.IsAutomated
,FTAdmin.RecurrenceOptionsId
,FTAdmin.StartDelay
,FTAdmin.IsInterval
,FTAdmin.IsShareResources
,SClient.Id
,SReviewClient.Id
,FTAdmin.DescNotificationFlag
,TClient.ID
,1
,FTAdmin.ID
,FTAdmin.ReviewerTraining
FROM FormulaTask FTAdmin
INNER JOIN Team TClient
ON FTAdmin.TeamId=TClient.PublicVaultTeamID
INNER JOIN Skill SClient
ON FTAdmin.AssignedSkillId=SClient.PublicVaultSkillID
AND SClient.OwnerGuid=TClient.OwnerGuid
LEFT OUTER JOIN Skill SReviewClient
ON FTAdmin.ReviewingSkillId=SReviewClient.PublicVaultSkillID
AND SReviewClient.OwnerGuid=TClient.OwnerGuid
INNER JOIN FormulaProject FPAdmin
ON FPAdmin.id=FTAdmin.FormulaProjectId
AND SReviewClient.OwnerGuid=TClient.OwnerGuid
INNER JOIN FormulaProject FPClient
ON FPClient.PublicVaultFormulaProjectID=FPAdmin.ID
AND FPClient.OwnerGuid=TClient.OwnerGuid
WHERE FTAdmin.OwnerGuid=@AdminGuid
AND TClient.OwnerGuid=@UserGuid


---Select * from FormulaTask Where TeamID=690
-- Select * from FormulaTask

INSERT INTO TeamSkill(TeamId,SkillId)
SELECT DISTINCT FTClient.TeamID,FTClient.AssignedSkillId
FROM FormulaTask FTClient
LEFT OUTER JOIN TeamSkill TS
ON TS.TeamId=FTClient.TeamID
AND TS.SkillId=FTClient.AssignedSkillId
WHERE FTClient.OwnerGuid=@UserGuid
AND TS.TeamId IS NULL
AND FTClient.TeamId IS NOT NULL AND FTClient.AssignedSkillId IS NOT NULL

INSERT INTO TeamSkill(TeamId,SkillId)
SELECT DISTINCT FTClient.TeamID,FTClient.ReviewingSkillId
FROM FormulaTask FTClient
INNER JOIN FormulaTask FTAdmin
ON FTAdmin.ID=FTClient.PublicVaultFormulaTaskID
LEFT OUTER JOIN TeamSkill TS
ON TS.TeamId=FTClient.TeamID
AND TS.SkillId=FTClient.ReviewingSkillId
WHERE FTClient.OwnerGuid=@UserGuid
AND TS.TeamId IS NULL
AND FTClient.TeamId IS NOT NULL AND FTClient.ReviewingSkillId IS NOT NULL


INSERT INTO FormulaTaskChecklist(DateCreated,FormulaTaskId,Name,Type)
SELECT DISTINCT @CurrentDate,FTClient.ID,FTCAdmin.Name,FTCAdmin.Type
FROM FormulaTaskChecklist FTCAdmin
INNER JOIN FormulaTask FTClient
ON FTCAdmin.FormulaTaskId=FTClient.ID
INNER JOIN FormulaTask FTAdmin
ON FTAdmin.ID=FTClient.PublicVaultFormulaTaskID
WHERE FTAdmin.OwnerGuid=@AdminGuid
AND FTClient.OwnerGuid=@UserGuid


INSERT INTO FormulaNote(DateCreated,FormulaId,Text)
SELECT DISTINCT @CurrentDate,FPClient.Id,FNAdmin.Text
FROM FormulaNote FNAdmin
INNER JOIN FormulaProject FPAdmin
ON FNAdmin.FormulaID=FPAdmin.ID
INNER JOIN FormulaProject FPClient
ON FPClient.PublicVaultFormulaProjectID=FPAdmin.ID
WHERE FPAdmin.OwnerGuid=@AdminGuid
AND FPClient.OwnerGuid=@UserGuid

--1) Formula notes
--2) checklist
--3) Reviewer training

SELECT 1 AS Result

END

GO";
            migrationBuilder.Sql(sp);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
