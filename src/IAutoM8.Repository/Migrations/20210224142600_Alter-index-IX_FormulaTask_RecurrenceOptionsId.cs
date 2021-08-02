using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class AlterindexIX_FormulaTask_RecurrenceOptionsId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
/****** Object:  Index [IX_FormulaTask_RecurrenceOptionsId]    Script Date: 24-02-2021 14:13:34 ******/
DROP INDEX if exists [IX_FormulaTask_RecurrenceOptionsId] ON [dbo].[FormulaTask]
GO

/****** Object:  Index [IX_FormulaTask_RecurrenceOptionsId]    Script Date: 24-02-2021 14:13:34 ******/
CREATE NONCLUSTERED INDEX [IX_FormulaTask_RecurrenceOptionsId] ON [dbo].[FormulaTask]
(
	[RecurrenceOptionsId] ASC
)
WHERE ([RecurrenceOptionsId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
