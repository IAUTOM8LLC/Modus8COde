using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class Addedspusp_ImportProjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @" go drop procedure if exists [dbo].[usp_ImportProjects]
go 
/****** Object:  StoredProcedure [dbo].[usp_ImportProjects]    Script Date: 3/12/2021 1:27:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author: Dharminder Jasuja
-- Create date: 01-29-2021
-- Description: To Insert multiple Projects Imported from Excel
-- =============================================
--[usp_ImportProjects] '44EF029C-60B8-466A-0A26-08D538EEAC75'
create PROCEDURE [dbo].[usp_ImportProjects]
@ProjectDetails ProjectDetailType READONLY,
@UserGuid UNIQUEIDENTIFIER
AS
BEGIN
BEGIN TRY
BEGIN TRANSACTION

DECLARE @CurrentDate DateTime
SET @CurrentDate=GETDATE();


---- Insert the Data in Temporary Table
INSERT INTO ProjectDetails(ProjectLevel1,ProjectLevel1Description
,ProjectLevel2,ProjectLevel2Description
,ProjectLevel3,ProjectLevel3Description
,UserGuid
,CreatedDate
,IsCompleted)

SELECT ProjectLevel1,ProjectLevel1Description
,ProjectLevel2,ProjectLevel2Description
,ProjectLevel3,ProjectLevel3Description
,@UserGuid
,@CurrentDate
,0 As IsCompleted
FROM @ProjectDetails
WHERE ISNULL(ProjectLevel1,'')<>''


DECLARE @IdentityOutput TABLE ( ProjectID int, ProjectDetailID INT )


---- INSERT Level 1 Project
INSERT INTO Project(DateCreated,Name,Description,OwnerGuid,Details,IsActive,IsDeleted,StartDate)
OUTPUT inserted.Id,CAST(inserted.Details AS INT) INTO @IdentityOutput
SELECT CreatedDate,ProjectLevel1,ProjectLevel1Description,UserGuid,ProjectDetailID,1,0,@CurrentDate
FROM ProjectDetails
WHERE Userguid=@UserGuid AND CreatedDate=@CurrentDate
AND ISNULL(ProjectLevel1,'')<>''


UPDATE PD
SET PD.ProjectLevel1ID=I.ProjectID
FROM
ProjectDetails PD
INNER JOIN @IdentityOutput I
ON PD.ProjectDetailID=I.ProjectDetailID

Delete @IdentityOutput

--INSERT Level 2 Project
INSERT INTO Project(DateCreated,Name,Description,OwnerGuid,Details,IsActive,IsDeleted,StartDate)
OUTPUT inserted.Id,CAST(inserted.Details As INT) INTO @IdentityOutput
SELECT CreatedDate,ProjectLevel2,ProjectLevel2DEscription,UserGuid,ProjectDetailID,1,0,@CurrentDate
FROM ProjectDetails
WHERE Userguid=@UserGuid AND CreatedDate=@CurrentDate
AND ISNULL(ProjectLevel2,'')<>''

UPDATE PD
SET PD.ProjectLevel2ID=I.ProjectID
FROM
ProjectDetails PD
INNER JOIN @IdentityOutput I
ON PD.ProjectDetailID=I.ProjectDetailID

sELECT * FROM @IdentityOutput
Delete @IdentityOutput


--INSERT Level 3 Project
INSERT INTO Project(DateCreated,Name,Description,OwnerGuid,Details,IsActive,IsDeleted,StartDate)
OUTPUT inserted.Id,CAST(inserted.Details As INT) INTO @IdentityOutput
SELECT CreatedDate,ProjectLevel3,ProjectLevel3DEscription,UserGuid,ProjectDetailID,1,0,@CurrentDate
FROM ProjectDetails
WHERE Userguid=@UserGuid AND CreatedDate=@CurrentDate
AND ISNULL(ProjectLevel3,'')<>''

UPDATE PD
SET PD.ProjectLevel3ID=I.ProjectID
FROM
ProjectDetails PD
INNER JOIN @IdentityOutput I
ON PD.ProjectDetailID=I.ProjectDetailID

sELECT * FROM @IdentityOutput

Delete @IdentityOutput

-- Make the relations
UPDATE P
SET P.Details=null
FROM Project P
INNER JOIN ProjectDetails PD
ON P.Id=pd.ProjectLevel1ID
WHERE pd.Userguid=@UserGuid AND PD.CreatedDate=@CurrentDate

UPDATE P
SET P.ParentProjectId=PD.ProjectLevel1ID
,P.Details=null
FROM Project P
INNER JOIN ProjectDetails PD
ON P.Id=pd.ProjectLevel2ID
WHERE pd.Userguid=@UserGuid AND PD.CreatedDate=@CurrentDate

UPDATE P
SET P.ParentProjectId=PD.ProjectLevel2ID
,P.Details=null
FROM Project P
INNER JOIN ProjectDetails PD
ON P.Id=pd.ProjectLevel3ID
WHERE pd.Userguid=@UserGuid AND PD.CreatedDate=@CurrentDate


DELETE From ProjectDetails WHERE UserGuid=@UserGuid AND CREATEDDATE=@CurrentDate

COMMIT TRANSACTION
END TRY
BEGIN CATCH
IF @@TRANCOUNT >0
ROLLBACK TRANSACTION
PRINT 'TRANSACTION ROLLED BACK'
END CATCH


END";
            migrationBuilder.Sql(sql);
        }

    protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
