using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class Addednewcolumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShareNoteParentID",
                table: "ProjectNote",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChildCompanyWorkerID",
                table: "FormulaTaskVendor",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShareNoteParentID",
                table: "ProjectNote");

            migrationBuilder.DropColumn(
                name: "ChildCompanyWorkerID",
                table: "FormulaTaskVendor");
        }
    }
}
