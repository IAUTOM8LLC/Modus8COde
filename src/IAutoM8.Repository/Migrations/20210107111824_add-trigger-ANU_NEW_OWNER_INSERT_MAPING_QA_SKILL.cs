using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class addtriggerANU_NEW_OWNER_INSERT_MAPING_QA_SKILL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"/****** Object:  Trigger [dbo].[ANU_NEW_OWNER_INSERT_MAPING_QA_SKILL]    Script Date: 07-01-2021 14:04:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TRIGGER [dbo].[ANU_NEW_OWNER_INSERT_MAPING_QA_SKILL] ON [dbo].[AspNetUsers] --CHANGE THE TBL NAME LATER
AFTER INSERT 
AS
BEGIN

INSERT INTO [dbo].[UserSkill]
           ([SkillId]
           ,[UserId])
SELECT PUBLICSKILL.ID AS SkillId, INSERTEDUSER.Vendor_ID AS UserId  FROM

(
		SELECT DISTINCT I.Id as Vendor_ID
		--,anr.Name,up.FullName as Vendor_FullName  
		 --FROM AspNetUsers anu    
		 FROM INSERTED I    
		 INNER JOIN AspNetUserRoles anur on anur.UserId=I.Id    
		 INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId  
		 INNER JOIN UserProfile up on up.UserId=I.Id    
		 WHERE anr.Name='OWNER'
) INSERTEDUSER
 CROSS JOIN 
 (
		 SELECT DISTINCT *
		 FROM SKILL S
		 WHERE S.ISGLOBAL=1 AND S.STATUS = 5
		 AND NAME LIKE 'QA%'
		 UNION
		 SELECT DISTINCT *
		 FROM SKILL S
		 WHERE S.ISGLOBAL=1 AND S.STATUS = 5
		 AND NAME LIKE 'PREP%'
 ) PUBLICSKILL

END
GO

ALTER TABLE [dbo].[AspNetUsers] ENABLE TRIGGER [ANU_NEW_OWNER_INSERT_MAPING_QA_SKILL]
GO


";

            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
