using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class FormulaResharing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResharingAllowed",
                table: "FormulaProject",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OriginalFormulaProjectId",
                table: "FormulaProject",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormulaProject_OriginalFormulaProjectId",
                table: "FormulaProject",
                column: "OriginalFormulaProjectId");

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

            migrationBuilder.DropIndex(
                name: "IX_FormulaProject_OriginalFormulaProjectId",
                table: "FormulaProject");

            migrationBuilder.DropColumn(
                name: "IsResharingAllowed",
                table: "FormulaProject");

            migrationBuilder.DropColumn(
                name: "OriginalFormulaProjectId",
                table: "FormulaProject");
        }
    }
}
