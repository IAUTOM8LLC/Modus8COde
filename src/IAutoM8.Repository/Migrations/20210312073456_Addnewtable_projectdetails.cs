using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class Addnewtable_projectdetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/****** Object:  Table [dbo].[ProjectDetails]    Script Date: 3/12/2021 12:59:19 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectDetails]') AND type in (N'U'))
DROP TABLE [dbo].[ProjectDetails]
GO

/****** Object:  Table [dbo].[ProjectDetails]    Script Date: 3/12/2021 12:59:19 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ProjectDetails](
	[ProjectLevel1] [varchar](100) NULL,
	[ProjectLevel1DEscription] [varchar](500) NULL,
	[ProjectLevel2] [varchar](100) NULL,
	[ProjectLevel2DEscription] [varchar](500) NULL,
	[ProjectLevel3] [varchar](100) NULL,
	[ProjectLevel3DEscription] [varchar](500) NULL,
	[ProjectLevel1ID] [int] NULL,
	[ProjectLevel2ID] [int] NULL,
	[ProjectLevel3ID] [int] NULL,
	[IsCompleted] [bit] NULL,
	[UserGuid] [uniqueidentifier] NULL,
	[CREATEDDATE] [datetime] NULL,
	[ProjectDetailID] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
GO";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
