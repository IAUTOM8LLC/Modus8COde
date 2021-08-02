using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddedIsGlobalFlagv3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamType",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "SkillType",
                table: "Skill");

            migrationBuilder.AddColumn<bool>(
                name: "IsGlobal",
                table: "Team",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGlobal",
                table: "Skill",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGlobal",
                table: "FormulaTask",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGlobal",
                table: "FormulaProject",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGlobal",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "IsGlobal",
                table: "Skill");

            migrationBuilder.DropColumn(
                name: "IsGlobal",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "IsGlobal",
                table: "FormulaProject");

            migrationBuilder.AddColumn<byte>(
                name: "TeamType",
                table: "Team",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "SkillType",
                table: "Skill",
                nullable: false,
                defaultValue: (byte)1);
        }
    }
}
