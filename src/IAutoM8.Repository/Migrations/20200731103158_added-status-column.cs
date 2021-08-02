using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class addedstatuscolumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Team",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Skill",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "FormulaProject",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Skill");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "FormulaProject",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
