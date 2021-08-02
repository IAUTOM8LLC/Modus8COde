using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddStoreProcGetFormulaMeanTat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sqlQuery = @"CREATE PROCEDURE dbo.uspGetFormulaMeanTat
@formulaId  int
AS
BEGIN

select FORMULAID,  RIGHT('0'+CAST(MEAN_TAT_HRS/60 AS VARCHAR),3) + ':' + RIGHT('00'+CAST(MEAN_TAT_HRS%60 AS VARCHAR),2) as MEAN_TAT_HRS
from (

SELECT TASKS.FORMULAID
, SUM(ISNULL((cast((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT)as int) ), 0)) AS MEAN_TAT_HRS
--,RIGHT('0'+CAST((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT)/60 AS VARCHAR),3) + ':' + RIGHT('00'+CAST((MEAN_DWELL.MEANDWELL + MEAN_CT.MEANCT)%60 AS VARCHAR),2) AS MEAN_TAT_HRS

FROM

(
select FP.ID AS FORMULAID, FT.ID AS FormulaTaskId
from FORMULAPROJECT FP
INNER JOIN FORMULATASK FT ON FP.ID = FT.FORMULAPROJECTID
WHERE FP.ID = @FORMULAID
) TASKS


LEFT JOIN
 (
 SELECT FormulaTaskId, CAST(((CAST(AVG(cast(VLU as decimal(15, 2))) AS DECIMAL(15, 2)))) AS DECIMAL(15, 2)) as MEANDWELL
 FROM(
 select FormulaTaskId, VendorGuid, CAST(REPLACE(Value, '-', '') as decimal(15, 2)) AS VLU
 from FormulaTaskStatistic FTS
 where
 [Value] is not null and
 [Type] = 0
 AND EXISTS(SELECT * FROM FormulaTaskStatistic FTS_2 WHERE(FTS_2.VendorGuid = FTS.VendorGuid AND FTS_2.FormulaTaskId = FTS.FormulaTaskId) AND FTS_2.TYPE = 1 AND FTS_2.Completed IS NOT NULL)
 ) A
 group by FormulaTaskId
) MEAN_DWELL on   TASKS.FormulaTaskId = MEAN_DWELL.FormulaTaskId



LEFT JOIN
 (
 SELECT FormulaTaskId, CAST(((CAST(AVG(cast (VLU as decimal(15, 2))) AS DECIMAL(15,2)))) AS DECIMAL(15,2)) as MEANCT
 FROM(
 select FormulaTaskId, VendorGuid, cast(REPLACE(Value, '-', '') as decimal(15, 2)) AS VLU
 from FormulaTaskStatistic FTS
 where
 [Value] is not null and
 [Type] = 1
 AND EXISTS(SELECT * FROM FormulaTaskStatistic FTS_2 WHERE(FTS_2.VendorGuid = FTS.VendorGuid AND FTS_2.FormulaTaskId = FTS.FormulaTaskId) AND FTS_2.TYPE = 1 AND FTS_2.Completed IS NOT NULL)
 )A
 group by FormulaTaskId
) MEAN_CT on   TASKS.FormulaTaskId = MEAN_CT.FormulaTaskId

GROUP BY TASKS.FORMULAID
)A
END";

            migrationBuilder.Sql(sqlQuery);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[uspGetFormulaMeanTat];");
        }
    }
}
