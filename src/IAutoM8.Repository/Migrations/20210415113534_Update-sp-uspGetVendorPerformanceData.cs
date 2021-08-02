using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class UpdatespuspGetVendorPerformanceData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/***** Object: StoredProcedure [dbo].[uspGetVendorPerformanceData] Script Date: 15-04-2021 17:03:08 *****/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE IF EXISTS [dbo].[uspGetVendorPerformanceData]
GO

CREATE PROCEDURE [dbo].[uspGetVendorPerformanceData]
@userId uniqueidentifier,
--@role nvarchar(50),
@rating tinyint,
@working tinyint,
@responding tinyint
AS
BEGIN
SET NOCOUNT ON;

SELECT DISTINCT VENDORS.id AS VENDOR_ID,
VENDORS.fullname AS VENDOR_FULLNAME,
VENDORS.teamname,
VENDORS.skillname,
VENDORS.formulaname,
VENDORS.formulaid,
VENDORS.formulataskid,
--VENDORS.FormulaTaskName,
cast(VENDORS.formulataskid as varchar) +' - ' + VENDORS.FormulaTaskName as FormulaTaskName , --Prefixed formulataskid on 2021-02-12
VENDORS.price,
RATING.reviews,
RATING.avgrating,
CAST(DWELL.avgdwell / 60 AS DECIMAL(15, 2)) AS VENDOR_DWELL,
CAST(MEAN_DWELL.meandwell / 60 AS DECIMAL(15, 2)) AS
FORMULATASK_MEAN_DWELL,
CAST(CT.avgct / 60 AS DECIMAL(15, 2)) AS VENDOR_CT,
CAST(MEAN_CT.meanct / 60 AS DECIMAL(15, 2)) AS FORMULATASK_MEAN_CT
,
CAST(( DWELL.avgdwell + CT.avgct ) / 60 AS DECIMAL(15, 2)) AS Vendor_AVG_TAT,
CAST((MEAN_DWELL.meandwell + MEAN_CT.meanct) / 60 AS DECIMAL(15, 2)) AS FORMULATASK_MEAN_TAT
FROM (SELECT DISTINCT anu.id,
up.fullname,
fp.id AS FormulaID,
FP.NAME AS FormulaName,
FTV.formulataskid,
FT.Title AS FormulaTaskName,
S.NAME AS SKILLNAME,
T.NAME AS TEAMNAME,
FTV.price
FROM aspnetusers anu
INNER JOIN aspnetuserroles anur ON anur.userid = anu.id
INNER JOIN aspnetroles anr ON anr.id = anur.roleid AND anr.NAME IN ('Vendor','CompanyWorker') --AND anr.NAME = @role --new role added for sprint 10b
INNER JOIN userprofile up ON up.userid = anu.id
INNER JOIN formulataskvendor FTV ON FTV.vendorguid = anu.id
INNER JOIN formulatask FT ON FTV.formulataskid = FT.id
INNER JOIN formulaproject FP ON FT.formulaprojectid = FP.id and ( (fp.isglobal=1 and fp.status = 5) or (fp.IsGlobal = 0) ) -- newly added by nik to just show publishyed admin formula or custom formulas
LEFT JOIN skill S ON FT.assignedskillid = s.id
LEFT JOIN team T ON FT.teamid = T.id
WHERE anu.id = @userId
--AND EXISTS (SELECT *
-- FROM formulataskstatistic FTS_2
-- WHERE ( FTS_2.vendorguid = anu.id
-- AND FTS_2.formulataskid =
-- FTV.formulataskid )
-- FTS_2.[Type] = @working
-- AND FTS_2.completed IS NOT NULL)
) VENDORS

LEFT JOIN (SELECT vendorguid,
formulataskid,
Avg(value) AS AvgRATING,
Count(value) AS REVIEWS
FROM formulataskstatistic FTS
WHERE [value] IS NOT NULL
AND [Type] = @rating
AND EXISTS (SELECT *
FROM formulataskstatistic FTS_2
WHERE ( FTS_2.vendorguid = FTS.vendorguid
AND FTS_2.formulataskid =
FTS.formulataskid )
AND FTS_2.[Type] = @working
AND FTS_2.completed IS NOT NULL)
GROUP BY vendorguid,
formulataskid
) RATING ON VENDORS.id = RATING.vendorguid AND VENDORS.formulataskid = RATING.formulataskid

LEFT JOIN (SELECT vendorguid,
formulataskid,
Cast(Avg(Cast (Replace(value, '-', '') AS
DECIMAL(15, 2))) AS
DECIMAL(15, 2)) AS
AvgDWELL
FROM formulataskstatistic FTS
WHERE [value] IS NOT NULL
AND [type] = @responding
AND EXISTS (SELECT *
FROM formulataskstatistic FTS_2
WHERE ( FTS_2.vendorguid = FTS.vendorguid
AND FTS_2.formulataskid =
FTS.formulataskid )
AND FTS_2.[Type] = @working
AND FTS_2.completed IS NOT NULL)
GROUP BY vendorguid,
formulataskid

) DWELL ON VENDORS.id = DWELL.vendorguid AND VENDORS.formulataskid = DWELL.formulataskid

LEFT JOIN (SELECT formulataskid,
Cast(Avg(Cast (vlu AS DECIMAL(15, 2))) AS
DECIMAL(15, 2)) AS
MEANDWELL
FROM (SELECT formulataskid,
vendorguid,
Cast(Replace(value, '-', '') AS DECIMAL(15, 2))
AS VLU
FROM formulataskstatistic FTS
WHERE [value] IS NOT NULL
AND [type] = @responding
AND EXISTS (SELECT *
FROM formulataskstatistic FTS_2
WHERE ( FTS_2.vendorguid =
FTS.vendorguid
AND FTS_2.formulataskid =
FTS.formulataskid )
AND FTS_2.[Type] = @working
AND FTS_2.completed IS NOT
NULL)) A
GROUP BY formulataskid
) MEAN_DWELL ON VENDORS.formulataskid = MEAN_DWELL.formulataskid

LEFT JOIN (SELECT vendorguid,
formulataskid,
Cast(Avg(Cast (Replace(value, '-', '') AS
DECIMAL(15, 2))) AS
DECIMAL(15, 2)) AS
AvgCT
FROM formulataskstatistic FTS
WHERE [value] IS NOT NULL
AND [Type] = @working
AND EXISTS (SELECT *
FROM formulataskstatistic FTS_2
WHERE ( FTS_2.vendorguid = FTS.vendorguid
AND FTS_2.formulataskid =
FTS.formulataskid )
AND FTS_2.[Type] = @working
AND FTS_2.completed IS NOT NULL)
GROUP BY vendorguid,
formulataskid

) CT ON VENDORS.id = CT.vendorguid AND VENDORS.formulataskid = CT.formulataskid

LEFT JOIN (SELECT formulataskid,
Cast(Avg(Cast (vlu AS DECIMAL(15, 2))) AS
DECIMAL(15, 2)) AS
MEANCT
FROM (SELECT formulataskid,
vendorguid,
Cast (Replace(value, '-', '') AS DECIMAL(15, 2)
) AS
VLU
FROM formulataskstatistic FTS
WHERE [value] IS NOT NULL
AND [Type] = @working
AND EXISTS (SELECT *
FROM formulataskstatistic FTS_2
WHERE ( FTS_2.vendorguid =
FTS.vendorguid
AND FTS_2.formulataskid =
FTS.formulataskid )
AND FTS_2.[Type] = @working
AND FTS_2.completed IS NOT
NULL))A
GROUP BY formulataskid
) MEAN_CT ON VENDORS.formulataskid = MEAN_CT.formulataskid

ORDER BY vendor_id,
skillname,
formulaid
END";


            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
