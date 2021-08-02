using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class addREcurrenceOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cron",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "CronTab",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "Cron",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "EndRecurrenceDate",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "IsConditional",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "MaxOccurrences",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "RecurrenceType",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "Tab",
                table: "FormulaTask");

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceOptionsId",
                table: "ProjectTask",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceOptionsId",
                table: "FormulaTask",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecurrenceOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Cron = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CronTab = table.Column<byte>(type: "tinyint", nullable: false),
                    EndRecurrenceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxOccurrences = table.Column<int>(type: "int", nullable: false),
                    RecurrenceType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurrenceOptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTask_RecurrenceOptionsId",
                table: "ProjectTask",
                column: "RecurrenceOptionsId",
                unique: true,
                filter: "[RecurrenceOptionsId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTask_RecurrenceOptionsId",
                table: "FormulaTask",
                column: "RecurrenceOptionsId",
                unique: true,
                filter: "[RecurrenceOptionsId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaTask_RecurrenceOptions_RecurrenceOptionsId",
                table: "FormulaTask",
                column: "RecurrenceOptionsId",
                principalTable: "RecurrenceOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTask_RecurrenceOptions_RecurrenceOptionsId",
                table: "ProjectTask",
                column: "RecurrenceOptionsId",
                principalTable: "RecurrenceOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormulaTask_RecurrenceOptions_RecurrenceOptionsId",
                table: "FormulaTask");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTask_RecurrenceOptions_RecurrenceOptionsId",
                table: "ProjectTask");

            migrationBuilder.DropTable(
                name: "RecurrenceOptions");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTask_RecurrenceOptionsId",
                table: "ProjectTask");

            migrationBuilder.DropIndex(
                name: "IX_FormulaTask_RecurrenceOptionsId",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "RecurrenceOptionsId",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "RecurrenceOptionsId",
                table: "FormulaTask");

            migrationBuilder.AddColumn<string>(
                name: "Cron",
                table: "ProjectTask",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "CronTab",
                table: "ProjectTask",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "Cron",
                table: "FormulaTask",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "FormulaTask",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndRecurrenceDate",
                table: "FormulaTask",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConditional",
                table: "FormulaTask",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxOccurrences",
                table: "FormulaTask",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceType",
                table: "FormulaTask",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tab",
                table: "FormulaTask",
                nullable: true);
        }
    }
}
