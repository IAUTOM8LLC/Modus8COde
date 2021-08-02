using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class RemoveTreeDetailsAndRemoveConnectionType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTask_TreeDetail_TreeDetailId",
                table: "ProjectTask");

            migrationBuilder.DropTable(
                name: "TreeLeaf");

            migrationBuilder.DropTable(
                name: "TreeDetail");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTask_TreeDetailId",
                table: "ProjectTask");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ProjectTaskDependency");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ProjectTaskConditionOption");

            migrationBuilder.DropColumn(
                name: "TreeStatus",
                table: "ProjectTask");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Type",
                table: "ProjectTaskDependency",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Type",
                table: "ProjectTaskConditionOption",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "TreeStatus",
                table: "ProjectTask",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "TreeDetail",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TreeStatus = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreeDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TreeLeaf",
                columns: table => new
                {
                    TaskId = table.Column<int>(nullable: false),
                    TreeDetailId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreeLeaf", x => new { x.TaskId, x.TreeDetailId });
                    table.ForeignKey(
                        name: "FK_TreeLeaf_ProjectTask_TaskId",
                        column: x => x.TaskId,
                        principalTable: "ProjectTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreeLeaf_TreeDetail_TreeDetailId",
                        column: x => x.TreeDetailId,
                        principalTable: "TreeDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTask_TreeDetailId",
                table: "ProjectTask",
                column: "TreeDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_TreeLeaf_TreeDetailId",
                table: "TreeLeaf",
                column: "TreeDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTask_TreeDetail_TreeDetailId",
                table: "ProjectTask",
                column: "TreeDetailId",
                principalTable: "TreeDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
