using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class BindFormulaTaskWithProjectTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormulaTaskId",
                table: "ProjectTask",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTask_FormulaTaskId",
                table: "ProjectTask",
                column: "FormulaTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTask_FormulaTask_FormulaTaskId",
                table: "ProjectTask",
                column: "FormulaTaskId",
                principalTable: "FormulaTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTask_FormulaTask_FormulaTaskId",
                table: "ProjectTask");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTask_FormulaTaskId",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "FormulaTaskId",
                table: "ProjectTask");
        }
    }
}
