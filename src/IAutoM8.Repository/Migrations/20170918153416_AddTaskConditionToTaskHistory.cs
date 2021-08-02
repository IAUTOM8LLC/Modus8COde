using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddTaskConditionToTaskHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectTaskConditionOptionId",
                table: "TaskHistory",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistory_ProjectTaskConditionOptionId",
                table: "TaskHistory",
                column: "ProjectTaskConditionOptionId",
                unique: true,
                filter: "[ProjectTaskConditionOptionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistory_ProjectTaskConditionOption_ProjectTaskConditionOptionId",
                table: "TaskHistory",
                column: "ProjectTaskConditionOptionId",
                principalTable: "ProjectTaskConditionOption",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistory_ProjectTaskConditionOption_ProjectTaskConditionOptionId",
                table: "TaskHistory");

            migrationBuilder.DropIndex(
                name: "IX_TaskHistory_ProjectTaskConditionOptionId",
                table: "TaskHistory");

            migrationBuilder.DropColumn(
                name: "ProjectTaskConditionOptionId",
                table: "TaskHistory");
        }
    }
}
