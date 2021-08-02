using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddTransferRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransferRequest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDone = table.Column<bool>(nullable: false),
                    RequestTime = table.Column<DateTime>(nullable: false),
                    RequestedAmount = table.Column<decimal>(nullable: false),
                    RequestedAmountWithTax = table.Column<decimal>(nullable: false),
                    VendorId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferRequest_AspNetUsers_VendorId",
                        column: x => x.VendorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequest_RequestTime",
                table: "TransferRequest",
                column: "RequestTime");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequest_VendorId",
                table: "TransferRequest",
                column: "VendorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferRequest");
        }
    }
}
