using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class addedStatusDescriptioncolumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TransferRequest",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TransferRequest",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "TransferRequest");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TransferRequest");
        }
    }
}
