using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class Notification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    IsRead = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(maxLength: 200, nullable: true),
                    NotificationType = table.Column<int>(nullable: false),
                    RecipientGuid = table.Column<Guid>(nullable: false),
                    SenderGuid = table.Column<Guid>(nullable: true),
                    TaskId = table.Column<int>(nullable: true),
                    Url = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notification_AspNetUsers_RecipientGuid",
                        column: x => x.RecipientGuid,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notification_AspNetUsers_SenderGuid",
                        column: x => x.SenderGuid,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notification_ProjectTask_TaskId",
                        column: x => x.TaskId,
                        principalTable: "ProjectTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_RecipientGuid",
                table: "Notification",
                column: "RecipientGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_SenderGuid",
                table: "Notification",
                column: "SenderGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_TaskId",
                table: "Notification",
                column: "TaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notification");
        }
    }
}
