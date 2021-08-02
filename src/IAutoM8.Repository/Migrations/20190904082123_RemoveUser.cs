using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class RemoveUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTaskUser_AspNetUsers_UserId",
                table: "ProjectTaskUser");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistory_AspNetUsers_UserId",
                table: "TaskHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamUser_AspNetUsers_UserId",
                table: "TeamUser");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSkill_AspNetUsers_UserId",
                table: "UserSkill");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTaskUser_AspNetUsers_UserId",
                table: "ProjectTaskUser",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistory_AspNetUsers_UserId",
                table: "TaskHistory",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamUser_AspNetUsers_UserId",
                table: "TeamUser",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSkill_AspNetUsers_UserId",
                table: "UserSkill",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTaskUser_AspNetUsers_UserId",
                table: "ProjectTaskUser");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistory_AspNetUsers_UserId",
                table: "TaskHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamUser_AspNetUsers_UserId",
                table: "TeamUser");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSkill_AspNetUsers_UserId",
                table: "UserSkill");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTaskUser_AspNetUsers_UserId",
                table: "ProjectTaskUser",
                column: "UserId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_TeamUser_AspNetUsers_UserId",
                table: "TeamUser",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSkill_AspNetUsers_UserId",
                table: "UserSkill",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
