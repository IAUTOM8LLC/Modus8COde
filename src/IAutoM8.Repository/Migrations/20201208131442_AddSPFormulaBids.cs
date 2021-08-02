using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddSPFormulaBids : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var query = @"/****** Object:  StoredProcedure [dbo].[USP_GetFormulaBidsForVendors]    Script Date: 01-12-2020 17:06:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================          
-- Author:  Nikhil Sehgal    
-- Create date: 12-01-2020          
-- Description: To get the formula bid certifications for vendor    
-- Exec [dbo].[USP_GetFormulaBidsForVendors] 'D6738F05-0907-4F6D-7941-08D60A39DAA9'
-- ============================================= 
        

create procedure USP_GetFormulaBidsForVendors
@inputvendorguid [uniqueidentifier]
AS

BEGIN

SELECT distinct FP.id as f_id, fp.name as f_name, ft.id as ft_id, ft.Title as ft_name
FROM FORMULATASKVENDOR FTV
INNER JOIN FORMULATASK FT ON FTV.FORMULATASKID = FT.ID
INNER JOIN FORMULAPROJECT FP ON FT.FormulaProjectId = FP.Id
WHERE FTV.VENDORGUID = @INPUTVENDORGUID
AND FTV.Status = 2
order by ft.id desc,fp.id desc

END";

            migrationBuilder.Sql(query);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[USP_GetFormulaBidsForVendors];");
        }
    }
}
