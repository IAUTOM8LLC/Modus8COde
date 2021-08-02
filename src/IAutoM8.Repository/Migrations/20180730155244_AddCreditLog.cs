using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddCreditLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(nullable: false),
                    AmountWithTax = table.Column<decimal>(nullable: false),
                    HistoryTime = table.Column<DateTime>(nullable: false),
                    ManagerId = table.Column<Guid>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    VendorId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditLog_AspNetUsers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditLog_AspNetUsers_VendorId",
                        column: x => x.VendorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditLog_HistoryTime",
                table: "CreditLog",
                column: "HistoryTime");

            migrationBuilder.CreateIndex(
                name: "IX_CreditLog_ManagerId",
                table: "CreditLog",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditLog_VendorId",
                table: "CreditLog",
                column: "VendorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditLog");
        }
    }
}
