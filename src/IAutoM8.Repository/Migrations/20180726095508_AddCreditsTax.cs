using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AddCreditsTax : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditsTax",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Fee = table.Column<float>(nullable: false),
                    Percentage = table.Column<float>(nullable: false),
                    Type = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditsTax", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO CreditsTax(Percentage, Fee, Type) VALUES('0', '0', '0')"
                + "INSERT INTO CreditsTax(Percentage, Fee, Type) VALUES('15', '0', '1')"
                + "INSERT INTO CreditsTax(Percentage, Fee, Type) VALUES('5', '0.3', '2')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditsTax");
        }
    }
}
