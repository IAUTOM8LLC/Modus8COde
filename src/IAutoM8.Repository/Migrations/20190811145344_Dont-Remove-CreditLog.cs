using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class DontRemoveCreditLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditLog_ProjectTask_ProjectTaskId",
                table: "CreditLog");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditLog_ProjectTask_ProjectTaskId",
                table: "CreditLog",
                column: "ProjectTaskId",
                principalTable: "ProjectTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditLog_ProjectTask_ProjectTaskId",
                table: "CreditLog");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditLog_ProjectTask_ProjectTaskId",
                table: "CreditLog",
                column: "ProjectTaskId",
                principalTable: "ProjectTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
