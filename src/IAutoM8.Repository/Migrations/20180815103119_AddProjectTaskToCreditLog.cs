using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddProjectTaskToCreditLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectTaskId",
                table: "CreditLog",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditLog_ProjectTaskId",
                table: "CreditLog",
                column: "ProjectTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditLog_ProjectTask_ProjectTaskId",
                table: "CreditLog",
                column: "ProjectTaskId",
                principalTable: "ProjectTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditLog_ProjectTask_ProjectTaskId",
                table: "CreditLog");

            migrationBuilder.DropIndex(
                name: "IX_CreditLog_ProjectTaskId",
                table: "CreditLog");

            migrationBuilder.DropColumn(
                name: "ProjectTaskId",
                table: "CreditLog");
        }
    }
}
