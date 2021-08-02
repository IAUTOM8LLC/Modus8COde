using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class DecoupleTaskAndSkill : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectTaskUser",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    ProjectTaskId = table.Column<int>(nullable: false),
                    ProjectTaskUserType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTaskUser", x => new { x.UserId, x.ProjectTaskId, x.ProjectTaskUserType });
                    table.ForeignKey(
                        name: "FK_ProjectTaskUser_ProjectTask_ProjectTaskId",
                        column: x => x.ProjectTaskId,
                        principalTable: "ProjectTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectTaskUser_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTaskUser_ProjectTaskId",
                table: "ProjectTaskUser",
                column: "ProjectTaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectTaskUser");
        }
    }
}
