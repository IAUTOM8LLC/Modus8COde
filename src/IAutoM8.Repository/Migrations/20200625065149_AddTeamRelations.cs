using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddTeamRelations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "ProjectTask",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "FormulaTask",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTask_TeamId",
                table: "ProjectTask",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTask_TeamId",
                table: "FormulaTask",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaTask_Team_TeamId",
                table: "FormulaTask",
                column: "TeamId",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTask_Team_TeamId",
                table: "ProjectTask",
                column: "TeamId",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormulaTask_Team_TeamId",
                table: "FormulaTask");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTask_Team_TeamId",
                table: "ProjectTask");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTask_TeamId",
                table: "ProjectTask");

            migrationBuilder.DropIndex(
                name: "IX_FormulaTask_TeamId",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "FormulaTask");
        }
    }
}
