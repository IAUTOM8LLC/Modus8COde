using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddChildFormulaTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginalFormulaTaskId",
                table: "FormulaTask",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTask_OriginalFormulaTaskId",
                table: "FormulaTask",
                column: "OriginalFormulaTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaTask_FormulaTask_OriginalFormulaTaskId",
                table: "FormulaTask",
                column: "OriginalFormulaTaskId",
                principalTable: "FormulaTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormulaTask_FormulaTask_OriginalFormulaTaskId",
                table: "FormulaTask");

            migrationBuilder.DropIndex(
                name: "IX_FormulaTask_OriginalFormulaTaskId",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "OriginalFormulaTaskId",
                table: "FormulaTask");
        }
    }
}
