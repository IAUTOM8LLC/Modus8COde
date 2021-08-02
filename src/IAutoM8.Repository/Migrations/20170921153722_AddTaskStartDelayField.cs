using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddTaskStartDelayField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StartDelay",
                table: "ProjectTask",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StartDelay",
                table: "FormulaTask",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartDelay",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "StartDelay",
                table: "FormulaTask");
        }
    }
}
