using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddParentIdToProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentProjectId",
                table: "Project",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Project_ParentProjectId",
                table: "Project",
                column: "ParentProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Project_ParentProjectId",
                table: "Project",
                column: "ParentProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Project_ParentProjectId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_ParentProjectId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "ParentProjectId",
                table: "Project");
        }
    }
}
