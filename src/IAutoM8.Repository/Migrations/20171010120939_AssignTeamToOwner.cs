using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AssignTeamToOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerGuid",
                table: "Team",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Team_OwnerGuid",
                table: "Team",
                column: "OwnerGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Team_AspNetUsers_OwnerGuid",
                table: "Team",
                column: "OwnerGuid",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Team_AspNetUsers_OwnerGuid",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Team_OwnerGuid",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "OwnerGuid",
                table: "Team");
        }
    }
}
