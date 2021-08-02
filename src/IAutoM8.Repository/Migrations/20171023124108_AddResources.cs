using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddResources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Resource",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResourceFormula",
                columns: table => new
                {
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    FormulaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceFormula", x => new { x.ResourceId, x.FormulaId });
                    table.ForeignKey(
                        name: "FK_ResourceFormula_FormulaProject_FormulaId",
                        column: x => x.FormulaId,
                        principalTable: "FormulaProject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResourceFormula_Resource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResourceFormulaTask",
                columns: table => new
                {
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    FormulaTaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceFormulaTask", x => new { x.ResourceId, x.FormulaTaskId });
                    table.ForeignKey(
                        name: "FK_ResourceFormulaTask_FormulaTask_FormulaTaskId",
                        column: x => x.FormulaTaskId,
                        principalTable: "FormulaTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResourceFormulaTask_Resource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResourceProject",
                columns: table => new
                {
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceProject", x => new { x.ResourceId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_ResourceProject_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResourceProject_Resource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResourceProjectTask",
                columns: table => new
                {
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    ProjectTaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceProjectTask", x => new { x.ResourceId, x.ProjectTaskId });
                    table.ForeignKey(
                        name: "FK_ResourceProjectTask_ProjectTask_ProjectTaskId",
                        column: x => x.ProjectTaskId,
                        principalTable: "ProjectTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResourceProjectTask_Resource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceFormula_FormulaId",
                table: "ResourceFormula",
                column: "FormulaId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceFormulaTask_FormulaTaskId",
                table: "ResourceFormulaTask",
                column: "FormulaTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceProject_ProjectId",
                table: "ResourceProject",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceProjectTask_ProjectTaskId",
                table: "ResourceProjectTask",
                column: "ProjectTaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceFormula");

            migrationBuilder.DropTable(
                name: "ResourceFormulaTask");

            migrationBuilder.DropTable(
                name: "ResourceProject");

            migrationBuilder.DropTable(
                name: "ResourceProjectTask");

            migrationBuilder.DropTable(
                name: "Resource");
        }
    }
}
