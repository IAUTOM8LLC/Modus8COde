using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class ImplementNewTaskStatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserGuid",
                table: "TaskHistory",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "TaskHistory",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProccessingUserGuid",
                table: "ProjectTask",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewingUserGuid",
                table: "ProjectTask",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistory_UserId",
                table: "TaskHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTask_ProccessingUserGuid",
                table: "ProjectTask",
                column: "ProccessingUserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTask_ReviewingUserGuid",
                table: "ProjectTask",
                column: "ReviewingUserGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTask_AspNetUsers_ProccessingUserGuid",
                table: "ProjectTask",
                column: "ProccessingUserGuid",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTask_AspNetUsers_ReviewingUserGuid",
                table: "ProjectTask",
                column: "ReviewingUserGuid",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistory_AspNetUsers_UserId",
                table: "TaskHistory",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTask_AspNetUsers_ProccessingUserGuid",
                table: "ProjectTask");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTask_AspNetUsers_ReviewingUserGuid",
                table: "ProjectTask");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistory_AspNetUsers_UserId",
                table: "TaskHistory");

            migrationBuilder.DropIndex(
                name: "IX_TaskHistory_UserId",
                table: "TaskHistory");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTask_ProccessingUserGuid",
                table: "ProjectTask");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTask_ReviewingUserGuid",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "UserGuid",
                table: "TaskHistory");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TaskHistory");

            migrationBuilder.DropColumn(
                name: "ProccessingUserGuid",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "ReviewingUserGuid",
                table: "ProjectTask");
        }
    }
}
