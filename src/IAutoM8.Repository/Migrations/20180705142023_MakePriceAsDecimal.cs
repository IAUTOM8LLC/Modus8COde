using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class MakePriceAsDecimal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "ProjectTaskVendor",
                nullable: false,
                oldClrType: typeof(short));

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "FormulaTaskVendor",
                nullable: false,
                oldClrType: typeof(short));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "Price",
                table: "ProjectTaskVendor",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<short>(
                name: "Price",
                table: "FormulaTaskVendor",
                nullable: false,
                oldClrType: typeof(decimal));
        }
    }
}
