using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddRating : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormulaTaskStatistic",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Completed = table.Column<DateTime>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    FormulaTaskId = table.Column<int>(nullable: false),
                    FormulaTaskStatisticId = table.Column<int>(nullable: true),
                    ProjectTaskId = table.Column<int>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    Value = table.Column<short>(nullable: true),
                    VendorGuid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulaTaskStatistic", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormulaTaskStatistic");
        }
    }
}
