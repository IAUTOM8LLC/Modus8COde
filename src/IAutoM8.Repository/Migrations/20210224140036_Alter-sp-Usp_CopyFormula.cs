using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AlterspUsp_CopyFormula : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/****** Object:  StoredProcedure [dbo].[Usp_CopyFormula]    Script Date: 24-02-2021 14:07:22 ******/
DROP PROCEDURE IF EXISTS [dbo].[Usp_CopyFormula]
GO
/****** Object:  StoredProcedure [dbo].[Usp_CopyFormula]    Script Date: 24-02-2021 14:07:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================   
-- exec Usp_CopyFormula 15076,'1 GMB Review_copynew','',0,'FB851BA4-AD50-4F33-B416-08D7FF8EC6D5'       
-- Updated by : Nikhil Sehgal      
-- Description: To make a Copy    of existing formula
-- Modified by Nikhil Sehgal on 2021-02-12 --> added New insert for 'Reoccuranceoptions' tbl and made changes to insert of 'Formulatask' table for reoccuranceoptionsid		       
-- Modified by Nikhil Sehgal on 2021-02-16 --> added New columns in FP and FT tbls for copyformula and made changes to insert of 'formulaproject','Formulatask' table and removed publicvaultformulaid's insertion
		--alter table formulaproject add CopyFormulaprojectid int
		--alter table formulatask add CopyFormulataskid int
-- ============================================= 


CREATE PROCEDURE [dbo].[Usp_CopyFormula] @FormulaID          INT,       
                                         @FormulaName        VARCHAR(100),       
                                         @FormulaDescription NVARCHAR(200),    
           @isGlobal bit, @ownerGuid [uniqueidentifier]    
AS       

BEGIN         
BEGIN TRY    
BEGIN TRANSACTION    
  
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
                   isstarred,
				   copyformulaprojectid)       
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
             --@FormulaID,       
			 null,
             @isGlobal,       
             isstarred,
			 @FormulaID   
      FROM   formulaproject       
      WHERE  id = @FormulaID       
      
      -- User           
      -- IsGlobal = 0           
      -- PublicVaultFormulaProjectID =null    
	 
	  DECLARE @NewFormulaID INT       
      
      SET @NewFormulaID= Scope_identity()     


	  --check if reoccuranceoptionsid is not null then create a newone

	  IF EXISTS(SELECT 1 FROM FORMULATASK WHERE RecurrenceOptionsId IS NOT NULL AND FormulaProjectId = @FormulaID)
	  BEGIN
		INSERT INTO [dbo].[RecurrenceOptions]
				   ([Cron]
				   ,[CronTab]
				   ,[EndRecurrenceDate]
				   ,[MaxOccurrences]
				   ,[RecurrenceType]
				   ,[Occurrences]
				   ,[NextOccurenceDate]
				   ,[DayDiff]
				   ,[IsAsap])
		select [Cron]
				   ,[CronTab]
				   ,[EndRecurrenceDate]
				   ,[MaxOccurrences]
				   ,[RecurrenceType]
				   ,[Occurrences]
				   ,[NextOccurenceDate]
				   ,[DayDiff]
				   ,[IsAsap]
		from	   [RecurrenceOptions] RO
		INNER JOIN FORMULATASK FT ON RO.Id = FT.RecurrenceOptionsId

	  DECLARE @NewRecurrenceOptionsId INT       
      
      SET @NewRecurrenceOptionsId= Scope_identity()  
			 
	  END

	
	  
	        
      
      
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
                   isglobal,
				   copyformulataskid)       
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
             --recurrenceoptionsid,    --commented as it was copying same values as FK
			 case when recurrenceoptionsid is not null then @NewRecurrenceOptionsId  else null end as recurrenceoptionsid ,
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
             --id,       
			 null,
             --0 ,
			 @isGlobal,
			 id
			 			     
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
      

      --INSERT INTO formulataskdependency       
      --            (childtaskid,       
      --             parenttaskid,       
      --             required)       
      --SELECT FTToChild.id  AS ChildTask,       
      --       FTToParent.id AS ParentTask,       
      --       FTD.required       
      --FROM   formulatask FTFromChild       
      --       INNER JOIN formulatask FTToChild       
      --               ON FTFromChild.id = FTToChild.publicvaultformulataskid       
      --       INNER JOIN formulatask FTFromParent       
      --               ON FTFromChild.formulaprojectid =       
      --                  FTFromParent.formulaprojectid       
      --       INNER JOIN formulatask FTToParent       
      --               ON FTToParent.publicvaultformulataskid = FTFromParent.id       
      --       INNER JOIN formulataskdependency FTD       
      --               ON FTD.childtaskid = FTFromChild.id       
      --                  AND FTD.parenttaskid = FTFromParent.id       
      --WHERE  FTFromChild.formulaprojectid = @FormulaID       
      --       AND FTToChild.formulaprojectid = @NewFormulaID       
      --       AND FTFromParent.formulaprojectid = @FormulaID       
      --       AND FTToParent.formulaprojectid = @NewFormulaID   
	  
	  
      INSERT INTO formulataskdependency       
                  (childtaskid,       
                   parenttaskid,       
                   required)       
      SELECT FTToChild.id  AS ChildTask,       
             FTToParent.id AS ParentTask,       
             FTD.required       
      FROM   formulatask FTFromChild       
             INNER JOIN formulatask FTToChild       
                     ON FTFromChild.id = FTToChild.CopyFormulataskid       
             INNER JOIN formulatask FTFromParent       
                     ON FTFromChild.formulaprojectid =       
                        FTFromParent.formulaprojectid       
             INNER JOIN formulatask FTToParent       
                     ON FTToParent.CopyFormulataskid = FTFromParent.id       
             INNER JOIN formulataskdependency FTD       
                     ON FTD.childtaskid = FTFromChild.id       
                        AND FTD.parenttaskid = FTFromParent.id       
      WHERE  FTFromChild.formulaprojectid = @FormulaID       
             AND FTToChild.formulaprojectid = @NewFormulaID       
             AND FTFromParent.formulaprojectid = @FormulaID       
             AND FTToParent.formulaprojectid = @NewFormulaID       
      
   --   SELECT 
	  --@FormulaID AS OldFormulaId,
	  --@NewFormulaID AS NewFormulaID,  
	  --PublicVaultFormulaTaskID AS OldTaskId,
	  --Id AS NewTaskId 
	  --FROM FormulaTask 
	  --WHERE FormulaProjectId=@NewFormulaID  

	   SELECT 
	  @FormulaID AS OldFormulaId,
	  @NewFormulaID AS NewFormulaID,  
	  CopyFormulataskid AS OldTaskId,
	  Id AS NewTaskId 
	  FROM FormulaTask 
	  WHERE FormulaProjectId=@NewFormulaID  

 COMMIT TRANSACTION    
END TRY    

BEGIN CATCH     
    IF @@TRANCOUNT >0    
    ROLLBACK TRANSACTION    
 PRINT 'TRANSACTION ROLLED BACK'    
 END CATCH    
        
        
END
GO";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
