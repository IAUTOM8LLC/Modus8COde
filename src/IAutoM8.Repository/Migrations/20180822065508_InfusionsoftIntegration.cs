using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class InfusionsoftIntegration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AffiliateCode",
                table: "UserProfile",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AffiliateId",
                table: "UserProfile",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AffiliatePass",
                table: "UserProfile",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "UserProfile",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPayed",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AffiliateCode",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "AffiliateId",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "AffiliatePass",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "IsPayed",
                table: "AspNetUsers");
        }
    }
}
