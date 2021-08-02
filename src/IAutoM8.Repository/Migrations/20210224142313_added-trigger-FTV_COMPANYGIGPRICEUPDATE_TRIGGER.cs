using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class addedtriggerFTV_COMPANYGIGPRICEUPDATE_TRIGGER : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"

/****** Object:  Trigger [FTV_COMPANYGIGPRICEUPDATE_TRIGGER]    Script Date: 24-02-2021 14:10:16 ******/
DROP TRIGGER if exists [dbo].[FTV_COMPANYGIGPRICEUPDATE_TRIGGER]
GO

/****** Object:  Trigger [dbo].[FTV_COMPANYGIGPRICEUPDATE_TRIGGER]    Script Date: 24-02-2021 14:10:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--CREATED BY - NIKHIL SEHGAL
--CREATED DATE - 16-02-2021
--TO DYNAMICALLY UPDATE COMPANY GIG PRICE FOR ALL EXISTING ROWS FOR THAT FORMULATASKID WHEN PRICE IS UPDATED FROM UI or DB

CREATE TRIGGER [dbo].[FTV_COMPANYGIGPRICEUPDATE_TRIGGER] ON [dbo].[FormulaTaskVendor]
AFTER UPDATE 
AS
BEGIN
SET NOCOUNT ON;

IF (UPDATE (PRICE))
BEGIN
IF TRIGGER_NESTLEVEL() > 1
     RETURN

	update FTV_TARGET
	set FTV_TARGET.price = i.price
	--SELECT I.*,FTV_TARGET.* 
	FROM 
	INSERTED I 
	INNER JOIN ASPNETUSERS ANU ON I.VENDORGUID = ANU.ID
	INNER JOIN AspNetUserRoles ANUR ON ANU.Id = ANUR.UserId
	INNER JOIN AspNetRoles ANR ON ANUR.RoleId = ANR.Id AND ANR.Name = 'Company'

	
	INNER JOIN FORMULATASKVENDOR FTV_TARGET ON I.VENDORGUID = FTV_TARGET.VendorGuid AND I.FormulaTaskId = FTV_TARGET.FormulaTaskId
	
	--INNER JOIN FormulaTaskVendor FTV_CLIENT ON I.FORMULATASKID = FTV_CLIENT.FORMULATASKID AND I.VENDORGUID = FTV_CLIENT.VendorGuid 
	--INNER JOIN FORMULATASK FT_CLIENT  ON FTV_CLIENT.FORMULATASKID = FT_CLIENT.Id AND FT_CLIENT.PublicVaultFormulaTaskID IS NOT NULL
	--INNER JOIN FORMULAPROJECT FP_CLIENT ON FT_CLIENT.FormulaProjectId = FP_CLIENT.ID AND FP_CLIENT.PUBLICVAULTFORMULAPROJECTID IS NOT NULL AND FP_CLIENT.ISGLOBAL = 1
	--INNER JOIN FormulaTask FT_ADMIN ON FT_ADMIN.Id = FT_CLIENT.PublicVaultFormulaTaskID AND FT_ADMIN.PublicVaultFormulaTaskID IS NULL
	--INNER JOIN FORMULATASKVENDOR FTV_ADMIN ON FTV_ADMIN.FormulaTaskId = FT_ADMIN.Id AND FTV_ADMIN.VendorGuid = I.VendorGuid
	--INNER JOIN FORMULAPROJECT FP_ADMIN ON FT_ADMIN.FormulaProjectId = FP_ADMIN.Id AND FP_ADMIN.IsGlobal = 1 AND FP_ADMIN.PublicVaultFormulaProjectID IS NULL

	--print 'inside'
END

--print 'outside'

END
GO

ALTER TABLE [dbo].[FormulaTaskVendor] ENABLE TRIGGER [FTV_COMPANYGIGPRICEUPDATE_TRIGGER]
GO";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
