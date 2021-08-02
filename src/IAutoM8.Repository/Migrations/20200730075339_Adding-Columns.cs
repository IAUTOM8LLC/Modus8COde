using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddingColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PublicVaultTeamID",
                table: "Team",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublicVaultSkillID",
                table: "Skill",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublicVaultFormulaTaskID",
                table: "FormulaTask",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublicVaultFormulaProjectID",
                table: "FormulaProject",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "FormulaProject",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicVaultTeamID",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "PublicVaultSkillID",
                table: "Skill");

            migrationBuilder.DropColumn(
                name: "PublicVaultFormulaTaskID",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "PublicVaultFormulaProjectID",
                table: "FormulaProject");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FormulaProject");
        }
    }
}
