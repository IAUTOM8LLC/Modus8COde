using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddInternalFormulaTaskDependency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InternalFormulaProjectId",
                table: "FormulaTask",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTask_InternalFormulaProjectId",
                table: "FormulaTask",
                column: "InternalFormulaProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaTask_FormulaProject_InternalFormulaProjectId",
                table: "FormulaTask",
                column: "InternalFormulaProjectId",
                principalTable: "FormulaProject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormulaTask_FormulaProject_InternalFormulaProjectId",
                table: "FormulaTask");

            migrationBuilder.DropIndex(
                name: "IX_FormulaTask_InternalFormulaProjectId",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "InternalFormulaProjectId",
                table: "FormulaTask");
        }
    }
}
