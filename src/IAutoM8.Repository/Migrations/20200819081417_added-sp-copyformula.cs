using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class addedspcopyformula : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE [dbo].[Usp_CopyFormula] @FormulaID          INT,       
                                         @FormulaName        VARCHAR(100),       
                                         @FormulaDescription NVARCHAR(200),    
           @isGlobal bit, @ownerGuid [uniqueidentifier]    
AS       
  BEGIN       
      DECLARE @CurrentDate AS DATETIME       
      
      SET @CurrentDate=Getutcdate ()       
      
      INSERT INTO formulaproject       
                  (datecreated,       
                   description,       
                   isdeleted,       
                   lastupdated,       
                   NAME,       
                   ownerguid,       
                   sharetype,       
                   isresharingallowed,       
                   originalformulaprojectid,       
                   islocked,       
                   status,       
                   publicvaultformulaprojectid,       
                   isglobal,       
                   isstarred)       
      SELECT @CurrentDate,       
             @FormulaDescription,       
             isdeleted,       
             lastupdated,       
             @FormulaName,       
             @ownerGuid,       
             sharetype,       
             isresharingallowed,       
             originalformulaprojectid,       
             islocked,       
             1,       
             @FormulaID,       
             @isGlobal,       
             isstarred       
      FROM   formulaproject       
      WHERE  id = @FormulaID       
      
      -- User           
      -- IsGlobal = 0           
      -- PublicVaultFormulaProjectID =null          
      DECLARE @NewFormulaID INT       
      
      SET @NewFormulaID= Scope_identity()       
      
      INSERT INTO formulatask       
                  (assignedteamid,       
                   description,       
                   duration,       
                   formulaprojectid,       
                   posx,       
                   posy,       
                   reviewingteamid,       
                   taskconditionid,       
                   title,       
                   datecreated,       
                   lastupdated,       
                   ownerguid,       
                   isautomated,       
                   recurrenceoptionsid,       
                   startdelay,       
                   isinterval,       
                   isshareresources,       
                   internalformulaprojectid,       
                   assignedskillid,       
                   reviewingskillid,       
                   originalformulataskid,       
                   descnotificationflag,       
                   reviewertraining,       
                   teamid,       
                   publicvaultformulataskid,       
                   isglobal)       
      SELECT assignedteamid,       
             description,       
             duration,       
             @NewFormulaID,       
             posx,       
             posy,       
             reviewingteamid,       
             taskconditionid,       
             title,       
             @CurrentDate,       
             lastupdated,       
             @ownerGuid,       
             isautomated,       
             recurrenceoptionsid,       
             startdelay,       
             isinterval,       
             isshareresources,       
             internalformulaprojectid,       
             assignedskillid,       
             reviewingskillid,       
             originalformulataskid,       
             descnotificationflag,       
             reviewertraining,       
             teamid,       
             id,       
             0       
      FROM   formulatask       
      WHERE  formulaprojectid = @FormulaID       
      ORDER  BY id       
      
      --select * from FormulaTaskChecklist          
      INSERT INTO formulataskchecklist       
                  (datecreated,       
                   formulataskid,       
                   NAME,       
                   type)       
      SELECT DISTINCT @CurrentDate,       
                      OOA.id,       
                      ftc.NAME,       
                      ftc.type       
      FROM   formulataskchecklist ftc       
             LEFT JOIN formulatask ft       
      ON ftc.formulataskid = ft.id       
             OUTER apply (SELECT id       
                          FROM   formulatask       
                          WHERE  formulatask.title = ft.title       
                                 AND formulatask.formulaprojectid =       
                         @NewFormulaID)       
                         AS       
                         OOA       
      WHERE  ft.formulaprojectid = @FormulaID       
      
      --select * from FormulaNote          
      INSERT INTO formulanote       
                  (datecreated,       
                   formulaid,       
                   text)       
      SELECT DISTINCT @CurrentDate,       
                      @NewFormulaID,       
                      FN.text       
      FROM   formulanote FN       
             LEFT JOIN formulaproject FP       
                    ON FN.formulaid = FP.id       
      WHERE  FP.id = @FormulaID       
      
      INSERT INTO formulataskdependency       
                  (childtaskid,       
                   parenttaskid,       
                   required)       
      SELECT FTToChild.id  AS ChildTask,       
             FTToParent.id AS ParentTask,       
             FTD.required       
      FROM   formulatask FTFromChild       
             INNER JOIN formulatask FTToChild       
                     ON FTFromChild.id = FTToChild.publicvaultformulataskid       
             INNER JOIN formulatask FTFromParent       
                     ON FTFromChild.formulaprojectid =       
                        FTFromParent.formulaprojectid       
             INNER JOIN formulatask FTToParent       
                     ON FTToParent.publicvaultformulataskid = FTFromParent.id       
             INNER JOIN formulataskdependency FTD       
                     ON FTD.childtaskid = FTFromChild.id       
                        AND FTD.parenttaskid = FTFromParent.id       
      WHERE  FTFromChild.formulaprojectid = @FormulaID       
             AND FTToChild.formulaprojectid = @NewFormulaID       
             AND FTFromParent.formulaprojectid = @FormulaID       
             AND FTToParent.formulaprojectid = @NewFormulaID       
      
      SELECT 
	  @FormulaID AS OldFormulaId,
	  @NewFormulaID AS NewFormulaID,  
	  PublicVaultFormulaTaskID AS OldTaskId,
	  Id AS NewTaskId 
	  FROM FormulaTask 
	  WHERE FormulaProjectId=@NewFormulaID  
  END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
