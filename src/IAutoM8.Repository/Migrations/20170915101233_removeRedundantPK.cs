using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class removeRedundantPK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Business_AspNetUsers_UserId1",
                table: "Business");

            migrationBuilder.DropIndex(
                name: "IX_Business_UserId1",
                table: "Business");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Business");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Business",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Business_UserId1",
                table: "Business",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Business_AspNetUsers_UserId1",
                table: "Business",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
