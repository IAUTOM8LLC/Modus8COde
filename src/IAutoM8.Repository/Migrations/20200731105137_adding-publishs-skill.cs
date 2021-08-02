using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class addingpublishsskill : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"-- =============================================     
-- Author:  Dharminder Jasuja     
-- Create date: 07-27-2020     
-- Description: To Publish/Re-publish admin vault Skill to client's account     
-- =============================================     
--Select * from Formul     
--usp_PublishFormula 15007     
CREATE PROCEDURE [dbo].[Usp_publishskill] @SkillID INT   
AS   
  BEGIN   
      --Update Skill table if already exist      
      UPDATE SClient   
      SET    SClient.NAME = SAdmin.NAME   
      FROM   skill SClient   
             INNER JOIN skill SAdmin   
                     ON SClient.publicvaultskillid = SAdmin.id   
      WHERE  SAdmin.id = @SkillID   
  
      -- Insert Skill entries for all the owner Users if it does not exist already      
      INSERT INTO skill   
                  (NAME,   
                   ownerguid,   
                   datecreated,   
                   isglobal,   
                   publicvaultskillid)   
      SELECT SAdmin.NAME,   
             Users.ownerguid,   
             Getdate(),   
             1,   
             SAdmin.id   
      FROM   skill SAdmin   
             CROSS JOIN (SELECT aspnetusers.id AS OwnerGuid   
                         FROM   aspnetusers   
                                INNER JOIN aspnetuserroles   
                                        ON aspnetuserroles.userid =   
                                           aspnetusers.id   
                                INNER JOIN aspnetroles   
                                        ON aspnetroles.id =   
                                           aspnetuserroles.roleid   
                         WHERE  aspnetroles.[name] = 'Owner') Users   
             LEFT OUTER JOIN skill SClient   
                          ON SClient.publicvaultskillid = SAdmin.id   
                             AND SClient.ownerguid = Users.ownerguid   
      WHERE  SAdmin.id = @SkillID   
             AND SClient.id IS NULL   
  
      UPDATE skill   
      SET    status = 3   
      WHERE  id = @SkillID   
  
      SELECT 1 AS Result   
  END ";

            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
