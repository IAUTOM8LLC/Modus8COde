using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class RenameTeamToSkill : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedSkillId",
                table: "ProjectTask",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewingSkillId",
                table: "ProjectTask",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedSkillId",
                table: "FormulaTask",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewingSkillId",
                table: "FormulaTask",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Skill",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    OwnerGuid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skill", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Skill_AspNetUsers_OwnerGuid",
                        column: x => x.OwnerGuid,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSkill",
                columns: table => new
                {
                    SkillId = table.Column<int>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSkill", x => new { x.SkillId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserSkill_Skill_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skill",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSkill_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTask_AssignedSkillId",
                table: "ProjectTask",
                column: "AssignedSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTask_ReviewingSkillId",
                table: "ProjectTask",
                column: "ReviewingSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTask_AssignedSkillId",
                table: "FormulaTask",
                column: "AssignedSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTask_ReviewingSkillId",
                table: "FormulaTask",
                column: "ReviewingSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Skill_OwnerGuid",
                table: "Skill",
                column: "OwnerGuid");

            migrationBuilder.CreateIndex(
                name: "IX_UserSkill_UserId",
                table: "UserSkill",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaTask_Skill_AssignedSkillId",
                table: "FormulaTask",
                column: "AssignedSkillId",
                principalTable: "Skill",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaTask_Skill_ReviewingSkillId",
                table: "FormulaTask",
                column: "ReviewingSkillId",
                principalTable: "Skill",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTask_Skill_AssignedSkillId",
                table: "ProjectTask",
                column: "AssignedSkillId",
                principalTable: "Skill",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTask_Skill_ReviewingSkillId",
                table: "ProjectTask",
                column: "ReviewingSkillId",
                principalTable: "Skill",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormulaTask_Skill_AssignedSkillId",
                table: "FormulaTask");

            migrationBuilder.DropForeignKey(
                name: "FK_FormulaTask_Skill_ReviewingSkillId",
                table: "FormulaTask");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTask_Skill_AssignedSkillId",
                table: "ProjectTask");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTask_Skill_ReviewingSkillId",
                table: "ProjectTask");

            migrationBuilder.DropTable(
                name: "UserSkill");

            migrationBuilder.DropTable(
                name: "Skill");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTask_AssignedSkillId",
                table: "ProjectTask");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTask_ReviewingSkillId",
                table: "ProjectTask");

            migrationBuilder.DropIndex(
                name: "IX_FormulaTask_AssignedSkillId",
                table: "FormulaTask");

            migrationBuilder.DropIndex(
                name: "IX_FormulaTask_ReviewingSkillId",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "AssignedSkillId",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "ReviewingSkillId",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "AssignedSkillId",
                table: "FormulaTask");

            migrationBuilder.DropColumn(
                name: "ReviewingSkillId",
                table: "FormulaTask");
        }
    }
}
