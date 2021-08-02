using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddNewFieldsForFormulaTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormulaId",
                table: "ProjectTask",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentTaskId",
                table: "ProjectTask",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTask_FormulaId",
                table: "ProjectTask",
                column: "FormulaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTask_ParentTaskId",
                table: "ProjectTask",
                column: "ParentTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTask_FormulaProject_FormulaId",
                table: "ProjectTask",
                column: "FormulaId",
                principalTable: "FormulaProject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTask_ProjectTask_ParentTaskId",
                table: "ProjectTask",
                column: "ParentTaskId",
                principalTable: "ProjectTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTask_FormulaProject_FormulaId",
                table: "ProjectTask");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTask_ProjectTask_ParentTaskId",
                table: "ProjectTask");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTask_FormulaId",
                table: "ProjectTask");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTask_ParentTaskId",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "FormulaId",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "ParentTaskId",
                table: "ProjectTask");
        }
    }
}
