using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class Addednewcolumnsrelatedtosprint10bcompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyWorkerOwnerID",
                table: "UserProfile",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BussinessEntityID",
                table: "FormulaTaskChecklist",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CopyFormulaTaskID",
                table: "FormulaTask",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CopyFormulaProjectID",
                table: "FormulaProject",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyWorkerOwnerID",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "BussinessEntityID",
                table: "FormulaTaskChecklist");

            migrationBuilder.DropColumn(
                name: "CopyFormulaTaskID",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "CopyFormulaProjectID",
                table: "FormulaProject");
        }
    }
}
