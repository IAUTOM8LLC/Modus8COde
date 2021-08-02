using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class refactorBusinessEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Business_BusinessId",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Business",
                table: "Business");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BusinessId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Business");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Business",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Business",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Business",
                table: "Business",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Business_UserId1",
                table: "Business",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Business_AspNetUsers_UserId",
                table: "Business",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Business_AspNetUsers_UserId1",
                table: "Business",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Business_AspNetUsers_UserId",
                table: "Business");

            migrationBuilder.DropForeignKey(
                name: "FK_Business_AspNetUsers_UserId1",
                table: "Business");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Business",
                table: "Business");

            migrationBuilder.DropIndex(
                name: "IX_Business_UserId1",
                table: "Business");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Business");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Business");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Business",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Business",
                table: "Business",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BusinessId",
                table: "AspNetUsers",
                column: "BusinessId",
                unique: true,
                filter: "[BusinessId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Business_BusinessId",
                table: "AspNetUsers",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
