using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class DropSinglePersonTeam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSingelPersonTeam",
                table: "Team");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSingelPersonTeam",
                table: "Team",
                nullable: false,
                defaultValue: false);
        }
    }
}
