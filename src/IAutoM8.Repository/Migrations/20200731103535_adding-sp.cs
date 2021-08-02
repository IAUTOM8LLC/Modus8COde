using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class addingsp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE [dbo].[Usp_publishteam] @TeamID INT 
AS 
  BEGIN 
      UPDATE team 
      SET    status = 3 
      WHERE  id = @TeamID 

      SELECT 1 AS Result 
  END ";          

            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
