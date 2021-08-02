using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class CreatespUSP_GetFormulaBidsForCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/****** Object:  StoredProcedure [dbo].[USP_GetFormulaBidsForCompany]    Script Date: 24-02-2021 14:07:22 ******/
DROP PROCEDURE IF EXISTS [dbo].[USP_GetFormulaBidsForCompany]
GO
/****** Object:  StoredProcedure [dbo].[USP_GetFormulaBidsForCompany]    Script Date: 24-02-2021 14:07:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[USP_GetFormulaBidsForCompany]
@inputvendorguid [uniqueidentifier]
AS

BEGIN


SELECT distinct FP.id as f_id, fp.name as f_name, ft.id as ft_id, ft.Title as ft_name, ftv.id as ftv_id,FTV.CREATED AS FTV_CREATED, S.Name AS S_NAME, T.Name AS T_NAME,
COMPANYWORKERDETAILS.COMPANYWORKER_FULLNAME AS COMPANYWORKER_FULLNAME, COMPANYWORKERDETAILS.COMPANYWORKER_ID AS COMPANYWORKER_ID
FROM FORMULATASKVENDOR FTV
INNER JOIN ASPNETUSERS ANU ON FTV.VENDORGUID = ANU.ID
INNER JOIN USERPROFILE UP ON ANU.ID = UP.USERID
INNER JOIN ASPNETUSERROLES ANUR ON ANU.ID = ANUR.UserId
INNER JOIN AspNetRoles ANR ON ANUR.RoleId = ANR.Id
INNER JOIN FORMULATASK FT ON FTV.FORMULATASKID = FT.ID
INNER JOIN FORMULAPROJECT FP ON FT.FormulaProjectId = FP.Id
LEFT JOIN SKILL S ON FT.AssignedSkillId = S.Id
LEFT JOIN Team T ON FT.TeamId = T.Id
INNER JOIN (SELECT UP.FULLNAME AS COMPANYWORKER_FULLNAME, ANU.Id AS COMPANYWORKER_ID FROM ASPNETUSERS ANU
INNER JOIN USERPROFILE UP ON ANU.ID = UP.USERID
INNER JOIN ASPNETUSERROLES ANUR ON ANU.ID = ANUR.UserId
INNER JOIN AspNetRoles ANR ON ANUR.RoleId = ANR.Id
)COMPANYWORKERDETAILS ON FTV.ChildCompanyWorkerID = COMPANYWORKERDETAILS.COMPANYWORKER_ID

WHERE FTV.VENDORGUID = @INPUTVENDORGUID
AND FTV.Status = 2
AND ANR.Name = 'Company'
order by ft.id desc,fp.id desc

END
GO";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
