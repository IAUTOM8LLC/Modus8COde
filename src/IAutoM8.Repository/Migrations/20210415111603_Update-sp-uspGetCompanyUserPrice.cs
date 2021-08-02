using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class UpdatespuspGetCompanyUserPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/***** Object: StoredProcedure [dbo].[uspGetCompanyUserPrice] Script Date: 15-04-2021 16:44:18 *****/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/******
exec [dbo].[uspGetCompanyUserPrice] 'D6738F05-0907-4F6D-7941-08D60A39DAA9',2,52215


******/
DROP PROCEDURE IF EXISTS uspGetCompanyUserPrice
GO


CREATE PROCEDURE [dbo].[uspGetCompanyUserPrice]
@CompanyId uniqueidentifier,
--@role nvarchar(50),
@rating tinyint,
--@working tinyint,
--@responding tinyint
@FORMULATASKID INT
AS
BEGIN
SET NOCOUNT ON;
SELECT DISTINCT VENDORS.companyid as Company_Id,
VENDORS.COMPANYWORKERID AS COMPANYWORKER_ID,
VENDORS.fullname AS COMPANYWORKER_FULLNAME,
--VENDORS.email AS VENDOR_EMAIL,
--VENDORS.ROLE AS VENDOR_ROLE,
--VENDORS.teamname,
--VENDORS.skillname,
VENDORS.formulaname,
VENDORS.formulaid,
VENDORS.formulataskid,
VENDORS.FormulaTaskName,
--VENDORS.COMPANYPRICE,
COMPANYPRICE.COMPANYPRICE,
VENDORS.COMPANYWORKERPRICE,
RATING.reviews,
RATING.avgrating,
VENDORS.NoOfWorkers
--CAST(DWELL.avgdwell / 60 AS DECIMAL(15, 2)) AS VENDOR_DWELL,
--CAST(MEAN_DWELL.meandwell / 60 AS DECIMAL(15, 2)) AS
--FORMULATASK_MEAN_DWELL,
--CAST(CT.avgct / 60 AS DECIMAL(15, 2)) AS VENDOR_CT,
--CAST(MEAN_CT.meanct / 60 AS DECIMAL(15, 2)) AS FORMULATASK_MEAN_CT ,
--CAST(( DWELL.avgdwell + CT.avgct ) / 60 AS DECIMAL(15, 2)) AS Vendor_AVG_TAT,
--CAST((MEAN_DWELL.meandwell + MEAN_CT.meanct) / 60 AS DECIMAL(15, 2)) AS FORMULATASK_MEAN_TAT
FROM (SELECT DISTINCT anu_companyWorker.id AS COMPANYWORKERID,
up_companyWorker.fullname,
--anu_companyWorker.email,
--anr_companyWorker.NAME AS ROLE,
anu_company.id as companyid,
fp.id AS FormulaID,
FP.NAME AS FormulaName,
FTV.formulataskid,
FT.Title AS FormulaTaskName,
--S.NAME AS SKILLNAME,
--T.NAME AS TEAMNAME,
--FTV_COMPANY.price AS COMPANYPRICE,
FTV.price AS COMPANYWORKERPRICE,
COUNT(anu_companyWorker.id) OVER (PARTITION BY FTV.formulataskid) AS NoOfWorkers
FROM aspnetusers anu_company
INNER JOIN aspnetuserroles anur_company ON anur_company.userid = anu_company.id
--INNER JOIN aspnetroles anr ON anr.id = anur.roleid AND anr.NAME IN ('Vendor','CompanyWorker') --AND anr.NAME = @role --new role added for sprint 10b
INNER JOIN aspnetroles anr_company ON anr_company.id = anur_company.roleid AND anr_company.NAME IN ('Company') -- newly added by nik
INNER JOIN userprofile up_company ON up_company.userid = anu_company.id

--INNER JOIN aspnetusers anu_companyWorker on anu_company.Id = anu_companyWorker.OwnerId
INNER JOIN userprofile up_companyWorker ON anu_company.Id = up_companyWorker.CompanyWorkerOwnerID
INNER JOIN aspnetusers anu_companyWorker on up_companyWorker.UserId = anu_companyWorker.Id
INNER JOIN aspnetuserroles anur_companyWorker ON anur_companyWorker.userid = anu_companyWorker.id
INNER JOIN aspnetroles anr_companyWorker ON anr_companyWorker.id = anur_companyWorker.roleid AND anr_companyWorker.NAME IN ('CompanyWorker') -- newly added by nik
--INNER JOIN userprofile up_companyWorker ON up_companyWorker.userid = anu_companyWorker.id
INNER JOIN formulataskvendor FTV ON FTV.vendorguid = anu_companyWorker.id
INNER JOIN formulatask FT ON FTV.formulataskid = FT.id
INNER JOIN formulaproject FP ON FT.formulaprojectid = FP.id and ( (fp.isglobal=1 and fp.status = 5) or (fp.IsGlobal = 0) ) -- newly added by nik to just show publishyed admin formula or custom formulas
--LEFT JOIN skill S ON FT.assignedskillid = s.id
--LEFT JOIN team T ON FT.teamid = T.id
--INNER JOIN formulataskvendor FTV_COMPANY ON FTV_COMPANY.VENDORGUID = ANU_COMPANY.ID AND FTV.FORMULATASKID = FTV_COMPANY.FORMULATASKID
--INNER JOIN FORMULATASK FT_COMPANY ON FTV_COMPANY.formulataskid = FT_COMPANY.ID
WHERE anu_company.id = @CompanyId
AND FTV.formulataskid = @FORMULATASKID
) VENDORS
INNER JOIN ( SELECT distinct FTV_COMPANY.PRICE AS COMPANYPRICE ,FT_COMPANY.Id AS FormulaTaskId_COMPANY, FTV_COMPANY.VendorGuid AS COMPANYID
FROM
formulataskvendor FTV_COMPANY
INNER JOIN FORMULATASK FT_COMPANY ON FTV_COMPANY.formulataskid = FT_COMPANY.ID

) COMPANYPRICE ON VENDORS.companyid = COMPANYPRICE.COMPANYID AND VENDORS.FormulaTaskId = COMPANYPRICE.FormulaTaskId_COMPANY
LEFT JOIN (SELECT FTS.VENDORGUID AS vendorguid,
--ANU.OwnerId,
FTS.formulataskid,
Avg(FTS.value) AS AvgRATING,
Count(FTS.value) AS REVIEWS
FROM formulataskstatistic FTS
--INNER JOIN ASPNETUSERS ANU ON FTS.VENDORGUID = ANU.ID
WHERE [value] IS NOT NULL
AND [Type] = @rating
AND EXISTS (SELECT *
FROM formulataskstatistic FTS_2
WHERE ( FTS_2.vendorguid = FTS.vendorguid
AND FTS_2.formulataskid =
FTS.formulataskid )
AND FTS_2.[Type] = 1
AND FTS_2.completed IS NOT NULL)
GROUP BY vendorguid,
formulataskid--,
--ANU.OwnerId
--) RATING ON VENDORS.COMPANYID = RATING.OwnerId AND VENDORS.formulataskid = RATING.formulataskid
) RATING ON VENDORS.COMPANYWORKERID = RATING.vendorguid AND VENDORS.formulataskid = RATING.formulataskid
-- LEFT JOIN (SELECT --FTS.vendorguid,
-- ANU.OwnerId,
-- FTS.formulataskid,
-- Cast(Avg(Cast (Replace(FTS.value, '-', '') AS DECIMAL(15, 2))) AS DECIMAL(15, 2)) AS AvgDWELL
-- FROM formulataskstatistic FTS
-- INNER JOIN ASPNETUSERS ANU ON FTS.VENDORGUID = ANU.ID
-- WHERE [value] IS NOT NULL
-- AND [type] = @responding
-- AND EXISTS (SELECT *
-- FROM formulataskstatistic FTS_2
-- WHERE ( FTS_2.vendorguid = FTS.vendorguid
-- AND FTS_2.formulataskid =
-- FTS.formulataskid )
-- AND FTS_2.[Type] = @working
-- AND FTS_2.completed IS NOT NULL)
-- GROUP BY --vendorguid,
-- formulataskid,
-- ANU.OwnerId

--) DWELL ON VENDORS.COMPANYID = RATING.OwnerId AND VENDORS.formulataskid = DWELL.formulataskid
--LEFT JOIN (SELECT formulataskid,
-- Cast(Avg(Cast (vlu AS DECIMAL(15, 2))) AS
-- DECIMAL(15, 2)) AS
-- MEANDWELL
-- FROM (SELECT formulataskid,
-- vendorguid,
-- Cast(Replace(value, '-', '') AS DECIMAL(15, 2)) AS VLU
-- FROM formulataskstatistic FTS
-- WHERE [value] IS NOT NULL
-- AND [type] = @responding
-- AND EXISTS (SELECT *
-- FROM formulataskstatistic FTS_2
-- WHERE ( FTS_2.vendorguid =
-- FTS.vendorguid
-- AND FTS_2.formulataskid =
-- FTS.formulataskid )
-- AND FTS_2.[Type] = @working
-- AND FTS_2.completed IS NOT
-- NULL)) A
-- GROUP BY formulataskid
--) MEAN_DWELL ON VENDORS.formulataskid = MEAN_DWELL.formulataskid
--LEFT JOIN (SELECT --FTS.vendorguid,
-- ANU.OwnerId,
-- FTS.formulataskid,
-- Cast(Avg(Cast (Replace(FTS.value, '-', '') AS
-- DECIMAL(15, 2))) AS
-- DECIMAL(15, 2)) AS
-- AvgCT
-- FROM formulataskstatistic FTS
-- INNER JOIN ASPNETUSERS ANU ON FTS.VENDORGUID = ANU.ID
-- WHERE [value] IS NOT NULL
-- AND [Type] = @working
-- AND EXISTS (SELECT *
-- FROM formulataskstatistic FTS_2
-- WHERE ( FTS_2.vendorguid = FTS.vendorguid
-- AND FTS_2.formulataskid =
-- FTS.formulataskid )
-- AND FTS_2.[Type] = @working
-- AND FTS_2.completed IS NOT NULL)
-- GROUP BY --vendorguid,
-- formulataskid,
-- OwnerId

--) CT ON VENDORS.COMPANYID = CT.OwnerId AND VENDORS.formulataskid = CT.formulataskid
--LEFT JOIN (SELECT formulataskid,
-- Cast(Avg(Cast (vlu AS DECIMAL(15, 2))) AS
-- DECIMAL(15, 2)) AS
-- MEANCT
-- FROM (SELECT formulataskid,
-- vendorguid,
-- Cast (Replace(value, '-', '') AS DECIMAL(15, 2)
-- ) AS
-- VLU
-- FROM formulataskstatistic FTS
-- WHERE [value] IS NOT NULL
-- AND [Type] = @working
-- AND EXISTS (SELECT *
-- FROM formulataskstatistic FTS_2
-- WHERE ( FTS_2.vendorguid =
-- FTS.vendorguid
-- AND FTS_2.formulataskid =
-- FTS.formulataskid )
-- AND FTS_2.[Type] = @working
-- AND FTS_2.completed IS NOT
-- NULL))A
-- GROUP BY formulataskid
--) MEAN_CT ON VENDORS.formulataskid = MEAN_CT.formulataskid
ORDER BY VENDORS.COMPANYID,
COMPANYWORKER_FULLNAME,
formulataskid,
--skillname,
formulaid
END";


            migrationBuilder.Sql(sql);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
