using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddExplicitRelationFormulaTaskToOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormulaTask_AspNetUsers_UserId",
                table: "FormulaTask");

            migrationBuilder.DropIndex(
                name: "IX_FormulaTask_UserId",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FormulaTask");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "FormulaTask",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "FormulaTask",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerGuid",
                table: "FormulaTask",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTask_OwnerGuid",
                table: "FormulaTask",
                column: "OwnerGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaTask_AspNetUsers_OwnerGuid",
                table: "FormulaTask",
                column: "OwnerGuid",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormulaTask_AspNetUsers_OwnerGuid",
                table: "FormulaTask");

            migrationBuilder.DropIndex(
                name: "IX_FormulaTask_OwnerGuid",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "OwnerGuid",
                table: "FormulaTask");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "FormulaTask",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTask_UserId",
                table: "FormulaTask",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaTask_AspNetUsers_UserId",
                table: "FormulaTask",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
