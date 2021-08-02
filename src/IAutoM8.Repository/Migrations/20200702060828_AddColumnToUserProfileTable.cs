using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddColumnToUserProfileTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyAvailability",
                table: "UserProfile",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "UserProfile",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "UserProfile",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyAvailability",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "UserProfile");
        }
    }
}
