using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddedspuspGetAssignskillFormulaTat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
-- =============================================          
-- Author:  Nikhil Sehgal       
-- Create date: 12-29-2020          
-- Description: To Publish/Re-publish admin vault Formula to client's account      
-- EXEC [dbo].[uspGetAssignskillFormulaTat]  '3'
-- ============================================= 
CREATE PROCEDURE [dbo].[uspGetAssignskillFormulaTat] 
@formulaId  INT

AS
BEGIN


SELECT DISTINCT OWNERGUID,FORMULAID,ISGLOBAL,OUTSOURCER_TAT,TOTAL_TAT
from (

SELECT TASKS.OwnerGuid AS OWNERGUID, TASKS.FORMULAID 
, 0 AS ISGLOBAL
, SUM(ISNULL((cast((isnull(MEAN_DWELL.MEANDWELL,0) + isnull(MEAN_CT.MEANCT,0) + isnull(MEAN_INVITE_DWELL.MEANINVITEDWELL,0)) as int) ), 0)) AS OUTSOURCER_TAT
, SUM(ISNULL((cast((isnull(MEAN_DWELL.MEANDWELL,0) + isnull(MEAN_CT.MEANCT,0) + isnull(MEAN_INVITE_DWELL.MEANINVITEDWELL,0)) as int) ), 0)) AS TOTAL_TAT


FROM

(
select FP.ID AS FORMULAID,FP.OwnerGuid, FT.ID AS FormulaTaskId
from FORMULAPROJECT FP
INNER JOIN FORMULATASK FT ON FP.ID = FT.FORMULAPROJECTID
WHERE FP.ID = @FORMULAID

--select FP.ID AS FORMULAID,FP.OwnerGuid, FT.ID AS FormulaTaskId
--from FORMULAPROJECT FP
--INNER JOIN FORMULATASK FT ON FP.ID = FT.FORMULAPROJECTID
--WHERE FP.OwnerGuid = @LOGINGUID

) TASKS

LEFT JOIN --MEAN INVITE DWELL
 (
 SELECT FormulaTaskId, AVG(CAST((VLU) AS INT)) as MEANINVITEDWELL
 FROM(
 select FormulaTaskId, VendorGuid, REPLACE(Value, '-', '')  AS VLU
 from FormulaTaskStatistic FTS
 where
 [Value] is not null and
 [Type] = 0
 AND EXISTS(SELECT * FROM FormulaTaskStatistic FTS_2 WHERE(FTS_2.VendorGuid = FTS.VendorGuid AND FTS_2.FormulaTaskId = FTS.FormulaTaskId) AND FTS_2.TYPE = 1 AND FTS_2.Completed IS NOT NULL)
 ) A
 group by FormulaTaskId
) MEAN_INVITE_DWELL on   TASKS.FormulaTaskId = MEAN_INVITE_DWELL.FormulaTaskId


LEFT JOIN --MEAN DWELL (ACCEPTED BUT NOT YET STARTED)
 (
 SELECT FormulaTaskId, AVG(CAST((VLU) AS INT)) as MEANDWELL
 FROM(
 select FormulaTaskId, VendorGuid,REPLACE(Value, '-', '') AS VLU
 from FormulaTaskStatistic FTS
 where
 [Value] is not null and
 [Type] = 7
 AND EXISTS(SELECT * FROM FormulaTaskStatistic FTS_2 WHERE(FTS_2.VendorGuid = FTS.VendorGuid AND FTS_2.FormulaTaskId = FTS.FormulaTaskId) AND FTS_2.TYPE = 1 AND FTS_2.Completed IS NOT NULL)
 ) A
 group by FormulaTaskId
) MEAN_DWELL on   TASKS.FormulaTaskId = MEAN_DWELL.FormulaTaskId


LEFT JOIN --COMPLETION TIME (CT)
 (
 SELECT FormulaTaskId, AVG(CAST((VLU) AS INT)) as MEANCT
 FROM(
 select FormulaTaskId, VendorGuid, REPLACE(Value, '-', '') AS VLU
 from FormulaTaskStatistic FTS
 where
 [Value] is not null and
 [Type] = 1
 AND EXISTS(SELECT * FROM FormulaTaskStatistic FTS_2 WHERE(FTS_2.VendorGuid = FTS.VendorGuid AND FTS_2.FormulaTaskId = FTS.FormulaTaskId) AND FTS_2.TYPE = 1 AND FTS_2.Completed IS NOT NULL)
 )A
 group by FormulaTaskId
) MEAN_CT on   TASKS.FormulaTaskId = MEAN_CT.FormulaTaskId

GROUP BY TASKS.FORMULAID,TASKS.OwnerGuid
--,ISGLOBAL
)A
END";

            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[uspGetAssignskillFormulaTat];");
        }
    }
}
