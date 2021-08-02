using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class FixRaskHistoryTaskConditionMap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskHistory_ProjectTaskConditionOptionId",
                table: "TaskHistory");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistory_ProjectTaskConditionOptionId",
                table: "TaskHistory",
                column: "ProjectTaskConditionOptionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskHistory_ProjectTaskConditionOptionId",
                table: "TaskHistory");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistory_ProjectTaskConditionOptionId",
                table: "TaskHistory",
                column: "ProjectTaskConditionOptionId",
                unique: true,
                filter: "[ProjectTaskConditionOptionId] IS NOT NULL");
        }
    }
}
