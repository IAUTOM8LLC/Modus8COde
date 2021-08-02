using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class FormulaTeams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormulaTeam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    OwnerGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulaTeam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormulaTeam_AspNetUsers_OwnerGuid",
                        column: x => x.OwnerGuid,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTask_AssignedTeamId",
                table: "FormulaTask",
                column: "AssignedTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTask_ReviewingTeamId",
                table: "FormulaTask",
                column: "ReviewingTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTeam_OwnerGuid",
                table: "FormulaTeam",
                column: "OwnerGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaTask_FormulaTeam_AssignedTeamId",
                table: "FormulaTask",
                column: "AssignedTeamId",
                principalTable: "FormulaTeam",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaTask_FormulaTeam_ReviewingTeamId",
                table: "FormulaTask",
                column: "ReviewingTeamId",
                principalTable: "FormulaTeam",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormulaTask_FormulaTeam_AssignedTeamId",
                table: "FormulaTask");

            migrationBuilder.DropForeignKey(
                name: "FK_FormulaTask_FormulaTeam_ReviewingTeamId",
                table: "FormulaTask");

            migrationBuilder.DropTable(
                name: "FormulaTeam");

            migrationBuilder.DropIndex(
                name: "IX_FormulaTask_AssignedTeamId",
                table: "FormulaTask");

            migrationBuilder.DropIndex(
                name: "IX_FormulaTask_ReviewingTeamId",
                table: "FormulaTask");
        }
    }
}
