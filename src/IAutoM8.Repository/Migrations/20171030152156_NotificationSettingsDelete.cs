using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class NotificationSettingsDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationSetting_AspNetRoles_RoleId",
                table: "NotificationSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationSetting_ProjectTask_TaskId",
                table: "NotificationSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationSetting_AspNetUsers_UserId",
                table: "NotificationSetting");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationSetting_AspNetRoles_RoleId",
                table: "NotificationSetting",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationSetting_ProjectTask_TaskId",
                table: "NotificationSetting",
                column: "TaskId",
                principalTable: "ProjectTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationSetting_AspNetUsers_UserId",
                table: "NotificationSetting",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationSetting_AspNetRoles_RoleId",
                table: "NotificationSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationSetting_ProjectTask_TaskId",
                table: "NotificationSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationSetting_AspNetUsers_UserId",
                table: "NotificationSetting");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationSetting_AspNetRoles_RoleId",
                table: "NotificationSetting",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationSetting_ProjectTask_TaskId",
                table: "NotificationSetting",
                column: "TaskId",
                principalTable: "ProjectTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationSetting_AspNetUsers_UserId",
                table: "NotificationSetting",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
