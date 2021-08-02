using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class addtriggerNEW_QA_SKILL_INSERT_MAPING_WITH_EXISTINGOWNERS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"/****** Object:  Trigger [dbo].[NEW_QA_SKILL_INSERT_MAPING_WITH_EXISTINGOWNERS]    Script Date: 07-01-2021 14:06:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE TRIGGER [dbo].[NEW_QA_SKILL_INSERT_MAPING_WITH_EXISTINGOWNERS] ON [dbo].[Skill] --CHANGE THE TBL NAME LATER ON
AFTER INSERT 
AS
BEGIN

INSERT INTO [dbo].[USERSKILL]
           ([SkillId]
           ,[UserId])
SELECT InsertedPublicSkills.ID AS SkillId, ExistingOwners.Vendor_ID AS UserId  FROM

 
 (
		 SELECT DISTINCT I.ID
		 FROM inserted I
		 WHERE I.ISGLOBAL=1 AND I.STATUS = 5
		 AND I.NAME LIKE 'QA%'
		 UNION
		 SELECT DISTINCT I.ID
		 FROM inserted I
		 WHERE I.ISGLOBAL=1 AND I.STATUS = 5
		 AND I.NAME LIKE 'PREP%'
 ) InsertedPublicSkills

  CROSS JOIN
 (
		SELECT DISTINCT anu.Id as Vendor_ID
		--,anr.Name,up.FullName as Vendor_FullName  
		 FROM AspNetUsers anu  	
		 INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id    
		 INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId  
		 INNER JOIN UserProfile up on up.UserId=anu.Id    
		 WHERE anr.Name='OWNER'
) ExistingOwners

END

GO

ALTER TABLE [dbo].[Skill] ENABLE TRIGGER [NEW_QA_SKILL_INSERT_MAPING_WITH_EXISTINGOWNERS]
GO


";

            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
