using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class UpdatespuspGetVendorStatsByVendorGuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/***** Object: StoredProcedure [dbo].[uspGetVendorStatsByVendorGuid] Script Date: 15-04-2021 16:12:36 *****/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE IF EXISTS [dbo].[uspGetVendorStatsByVendorGuid]

GO

CREATE PROCEDURE [dbo].[uspGetVendorStatsByVendorGuid]
@VendorGuid [uniqueidentifier]

AS
BEGIN

SELECT VENDORS.VENDOR_ID,VENDORS.Vendor_FullName--,V_TASKS.TOT_TASKS_PER_VENDOR
,CASE WHEN TASKS_INVITES.TASKS_INVITES IS NULL THEN 0 ELSE TASKS_INVITES.TASKS_INVITES END AS INVITES
,CASE WHEN TASKS_ACTIVE.TASKS_ACTIVE IS NULL THEN 0 ELSE TASKS_ACTIVE.TASKS_ACTIVE END AS ACTIVE
,CASE WHEN TASKS_ATRISK.TASKS_ATRISK IS NULL THEN 0 ELSE TASKS_ATRISK.TASKS_ATRISK END AS ATRISK
,CASE WHEN TASKS_OVERDUE.TASKS_OVERDUE IS NULL THEN 0 ELSE TASKS_OVERDUE.TASKS_OVERDUE END AS OVERDUE
,CASE WHEN QUEUE_REVENUE.QUEUE_REVENUE IS NULL THEN 0.00 ELSE QUEUE_REVENUE.QUEUE_REVENUE END AS QUEUEREVENUE
,CASE WHEN TASKS_LOST.TASKS_LOST IS NULL THEN 0 ELSE TASKS_LOST.TASKS_LOST END AS LOST
,CASE WHEN TOT_TASKS_COMPLETED.TOT_TASKS_COMPLETED IS NULL THEN 0 ELSE TOT_TASKS_COMPLETED.TOT_TASKS_COMPLETED END AS TOTALCOMPLETED
,CASE WHEN TOTAL_REVENUE.TOTAL_REVENUE IS NULL THEN 0.00 ELSE TOTAL_REVENUE.TOTAL_REVENUE END AS TOTALREVENUE
from
(
SELECT DISTINCT anu.Id as Vendor_ID,up.FullName as Vendor_FullName--,FTV.FormulaTaskId
FROM AspNetUsers anu
INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name in ('Vendor','CompanyWorker')--and anr.Name='Vendor' --new role added for sprint 10B
INNER JOIN UserProfile up on up.UserId=anu.Id
INNER JOIN FormulaTaskVendor FTV on FTV.VendorGuid=anu.Id AND FTV.VendorGuid=@VendorGuid
--INNER JOIN ProjectTask PT ON FTV.ID = PT.FormulaTaskId
) VENDORS

LEFT JOIN
(
SELECT FTS.VendorGuid, COUNT(DISTINCT FTS.ProjectTaskId) AS TASKS_INVITES
--SELECT FTS.VendorGuid, FTS.ProjectTaskId, FTS.Type, FTS.Created, FTS.Completed--, DATEDIFF(HOUR,CAST(CREATED AS DATETIME),GETDATE()) AS T
FROM FormulaTaskStatistic FTS
LEFT JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id
WHERE FTS.Type = 0 AND FTS.Completed IS NULL
--AND DATEDIFF(HOUR,CAST(CREATED AS DATETIME),GETDATE()) <= 24 --commented to accomodate lost cases
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6
GROUP BY FTS.VendorGuid
)
TASKS_INVITES ON VENDORS.Vendor_ID = TASKS_INVITES.VendorGuid

LEFT JOIN
(
SELECT FTS.VendorGuid, COUNT(DISTINCT FTS.ProjectTaskId) AS TASKS_ACTIVE
--SELECT FTS.VendorGuid, FTS.ProjectTaskId, FTS.Type, FTS.Created, FTS.Completed
FROM FormulaTaskStatistic FTS
INNER JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id
WHERE FTS.Type = 1 AND FTS.Completed IS NULL
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6
GROUP BY FTS.VendorGuid
)
TASKS_ACTIVE ON VENDORS.Vendor_ID = TASKS_ACTIVE.VendorGuid

LEFT JOIN
(
SELECT FTS.VendorGuid, COUNT(DISTINCT FTS.ProjectTaskId) AS TASKS_OVERDUE
--SELECT FTS.VendorGuid, FTS.ProjectTaskId, FTS.Type, FTS.Created, FTS.Completed,PT.DURATION,PT.StartDate,CAST(PT.STARTDATE AS datetime),DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETDATE())
FROM FormulaTaskStatistic FTS
INNER JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id
WHERE FTS.Type = 1 AND FTS.Completed IS NULL
AND DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETDATE()) > PT.Duration
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6
GROUP BY FTS.VendorGuid
)
TASKS_OVERDUE ON VENDORS.Vendor_ID = TASKS_OVERDUE.VendorGuid

LEFT JOIN
(
SELECT FTS.VendorGuid, COUNT(DISTINCT FTS.ProjectTaskId) AS TASKS_ATRISK
--SELECT FTS.VendorGuid, FTS.ProjectTaskId, FTS.Type, FTS.Created, FTS.Completed,CAST(ROUND((PT.DURATION*0.8),0) AS INT) AS 80OF,PT.StartDate
--,CAST(PT.STARTDATE AS datetime),DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETDATE())
FROM FormulaTaskStatistic FTS
INNER JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id
WHERE FTS.Type = 1 AND FTS.Completed IS NULL
AND (( DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETDATE()) >= (CAST(ROUND((PT.DURATION*0.8),0) AS INT)) )
AND ( DATEDIFF(MINUTE,CAST(PT.StartDate AS DATETIME),GETDATE()) <= PT.Duration ))
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6
GROUP BY FTS.VendorGuid
)
TASKS_ATRISK ON VENDORS.Vendor_ID = TASKS_ATRISK.VendorGuid

LEFT JOIN
(
SELECT A.VendorGuid, COUNT(DISTINCT A.ProjectTaskId) AS COUNT_TASKS, SUM(A.FINAL_PRICE) AS QUEUE_REVENUE
FROM (
SELECT FTS.VendorGuid, FTS.ProjectTaskId, FTS.Type, FTS.Created, FTS.Completed,Duration--,PTV.Price
--,ROUND((ROUND((PT.Duration/60),2)*PTV.Price),2) AS FINAL_PRICE
--,CAST((( CAST((CAST(PT.Duration AS decimal(10,2))/60) AS decimal(15,2)) )*PTV.Price) AS decimal(15,2)) AS FINAL_PRICE
,CAST(PTV.Price AS decimal(15,2)) AS FINAL_PRICE --newly added
FROM FormulaTaskStatistic FTS
INNER JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id
INNER JOIN ProjectTaskVendor PTV ON PT.ID = PTV.ProjectTaskId and fts.VendorGuid = ptv.VendorGuid
WHERE FTS.Type = 1 AND FTS.Completed IS NULL
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6
)A
GROUP BY A.VendorGuid
)
QUEUE_REVENUE ON VENDORS.Vendor_ID = QUEUE_REVENUE.VendorGuid


--LEFT JOIN
--(
--SELECT FTS.VendorGuid, COUNT(DISTINCT FTS.ProjectTaskId) AS TASKS_LOST
----SELECT FTS.VendorGuid, FTS.ProjectTaskId, FTS.Type, FTS.Created, FTS.Completed--, DATEDIFF(HOUR,CAST(CREATED AS DATETIME),GETDATE()) AS T
--FROM FormulaTaskStatistic FTS
--LEFT JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id
--WHERE FTS.Type = 0 AND FTS.Completed IS NULL
--AND DATEDIFF(HOUR,CAST(CREATED AS DATETIME),GETDATE()) > 24
--GROUP BY FTS.VendorGuid
--)
--TASKS_LOST ON VENDORS.Vendor_ID = TASKS_LOST.VendorGuid

LEFT JOIN
(
SELECT FTS.VendorGuid, COUNT(DISTINCT FTS.ProjectTaskId) AS TASKS_LOST
FROM FormulaTaskStatistic FTS
LEFT JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id
WHERE FTS.Type = 9
GROUP BY FTS.VendorGuid
)
TASKS_LOST ON VENDORS.Vendor_ID = TASKS_LOST.VendorGuid


LEFT JOIN
(
SELECT FTS.VendorGuid, COUNT(DISTINCT FTS.ProjectTaskId) AS TOT_TASKS_COMPLETED
--SELECT FTS.VendorGuid, FTS.ProjectTaskId, FTS.Type, FTS.Created, FTS.Completed
FROM FormulaTaskStatistic FTS
INNER JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id
WHERE FTS.Type = 1 AND FTS.Completed IS NOT NULL
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6
GROUP BY FTS.VendorGuid
)
TOT_TASKS_COMPLETED ON VENDORS.Vendor_ID = TOT_TASKS_COMPLETED.VendorGuid

LEFT JOIN
(
SELECT A.VendorGuid, COUNT(DISTINCT A.ProjectTaskId) AS COUNT_TASKS, SUM(A.FINAL_PRICE) AS TOTAL_REVENUE
FROM (

SELECT FTS.VendorGuid, FTS.ProjectTaskId, FTS.Type, FTS.Created, FTS.Completed
--,CAST((( CAST((CAST(PT.Duration AS decimal(10,2))/60) AS decimal(15,2)) )*PTV.Price) AS decimal(15,2)) AS FINAL_PRICE
,CAST(PTV.Price AS decimal(15,2)) AS FINAL_PRICE --newly added
--,PTV.Price
FROM FormulaTaskStatistic FTS
INNER JOIN ProjectTask PT ON FTS.ProjectTaskId = PT.Id
INNER JOIN ProjectTaskVendor PTV ON PT.ID = PTV.ProjectTaskId and fts.VendorGuid = ptv.VendorGuid
WHERE FTS.Type = 1 AND FTS.Completed IS NOT NULL
AND NOT EXISTS (SELECT DISTINCT * FROM FormulaTaskStatistic FTS2 WHERE FTS.VendorGuid = FTS2.VendorGuid AND FTS.ProjectTaskId = FTS2.ProjectTaskId AND FTS2.Type in (6,9)) -- check added to exclude lost cases type = 9 and cancelled by vendor type = 6
)A
GROUP BY A.VendorGuid
)
TOTAL_REVENUE ON VENDORS.Vendor_ID = TOTAL_REVENUE.VendorGuid


END";


            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
