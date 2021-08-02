using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddLockedPropertyToFormula : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormulaProject_FormulaProject_OriginalFormulaProjectId",
                table: "FormulaProject");

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "FormulaProject",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaProject_FormulaProject_OriginalFormulaProjectId",
                table: "FormulaProject",
                column: "OriginalFormulaProjectId",
                principalTable: "FormulaProject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormulaProject_FormulaProject_OriginalFormulaProjectId",
                table: "FormulaProject");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "FormulaProject");

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaProject_FormulaProject_OriginalFormulaProjectId",
                table: "FormulaProject",
                column: "OriginalFormulaProjectId",
                principalTable: "FormulaProject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
