using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddTableFormulaTaskDisabledStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormulaTaskDisableStatus",
                columns: table => new
                {
                    ChildFormulaId = table.Column<int>(nullable: false),
                    ParentFormulaId = table.Column<int>(nullable: false),
                    InternalChildFormulaId = table.Column<int>(nullable: false),
                    InternalChildFormulaTaskId = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    IsDisabled = table.Column<bool>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulaTaskDisableStatus", x => new { x.ChildFormulaId, x.ParentFormulaId, x.InternalChildFormulaId, x.InternalChildFormulaTaskId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormulaTaskDisableStatus");
        }
    }
}
