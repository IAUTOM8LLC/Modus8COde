using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddedtabletypeProjectDetailType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"/****** Object:  UserDefinedTableType [dbo].[ProjectDetailType]    Script Date: 3/12/2021 1:20:33 PM ******/
DROP TYPE if exists  [dbo].[ProjectDetailType]
GO

/****** Object:  UserDefinedTableType [dbo].[ProjectDetailType]    Script Date: 3/12/2021 1:20:33 PM ******/
CREATE TYPE [dbo].[ProjectDetailType] AS TABLE(
	[ProjectLevel1] [varchar](100) NULL,
	[ProjectLevel1DEscription] [varchar](500) NULL,
	[ProjectLevel2] [varchar](100) NULL,
	[ProjectLevel2DEscription] [varchar](500) NULL,
	[ProjectLevel3] [varchar](100) NULL,
	[ProjectLevel3DEscription] [varchar](500) NULL
)
GO";

            migrationBuilder.Sql(sql);
        }

    protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
