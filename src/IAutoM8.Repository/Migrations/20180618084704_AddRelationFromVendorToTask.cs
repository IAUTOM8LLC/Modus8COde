using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddRelationFromVendorToTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormulaTaskVendor",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: false),
                    FormulaTaskId = table.Column<int>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: true),
                    Price = table.Column<short>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    VendorGuid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulaTaskVendor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormulaTaskVendor_FormulaTask_FormulaTaskId",
                        column: x => x.FormulaTaskId,
                        principalTable: "FormulaTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormulaTaskVendor_AspNetUsers_VendorGuid",
                        column: x => x.VendorGuid,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTaskVendor",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: true),
                    Price = table.Column<short>(nullable: false),
                    ProjectTaskId = table.Column<int>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    VendorGuid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTaskVendor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTaskVendor_ProjectTask_ProjectTaskId",
                        column: x => x.ProjectTaskId,
                        principalTable: "ProjectTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTaskVendor_AspNetUsers_VendorGuid",
                        column: x => x.VendorGuid,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTaskVendor_FormulaTaskId",
                table: "FormulaTaskVendor",
                column: "FormulaTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaTaskVendor_VendorGuid",
                table: "FormulaTaskVendor",
                column: "VendorGuid");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTaskVendor_ProjectTaskId",
                table: "ProjectTaskVendor",
                column: "ProjectTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTaskVendor_VendorGuid",
                table: "ProjectTaskVendor",
                column: "VendorGuid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormulaTaskVendor");

            migrationBuilder.DropTable(
                name: "ProjectTaskVendor");
        }
    }
}
