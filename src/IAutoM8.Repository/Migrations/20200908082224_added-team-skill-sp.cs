using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IAutoM8.Repository.Migrations
{
    public partial class addedteamskillsp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE [dbo].[usp_GetTeamSkillFormulaList] @ownerID [uniqueidentifier] , @isAdmin bit AS     
BEGIN    
   IF @isAdmin = 1     
     BEGIN    
     -- Global ( Admin query & Global Owner)    
     SELECT DISTINCT    
     t.Id AS Id,    
     t.Name AS TeamName,    
     t.IsGlobal AS TeamIsGlobal,    
     t.Status AS TeamStatus,    
     s.Id AS SkillId,    
     s.Name AS SkillName,    
     s.IsGlobal AS SkillIsGlobal,    
     s.Status AS SkillStatus,    
     FormulaID,    
     FormulaName,    
     FormulaIsGlobal,    
     FormulaCreatedDate,    
     FormulaUpdatedDate,    
     FormulaStatus,    
     ft.Title as TaskName,    
     ft.Id as TaskID,    
     ft.IsGlobal as TaskIsGlobal,    
     STUFF((     
     select    
     ',' + up2.FullName     
     from    
     FormulaTaskVendor FTV2     
     LEFT JOIN    
        UserProfile up2     
        ON FTV2.VendorGuid = up2.UserId     
     where    
     FTV2.FormulaTaskId = FTV.FormulaTaskId     
     order by    
     up2.FullName for xml path('') ), 1, 1, '') as OutsourcerName     
     FROM    
     Team t     
     LEFT JOIN    
        teamskill ts     
        ON t.Id = ts.teamid     
     LEFT JOIN    
        Skill s     
        ON s.Id = ts.skillid  AND s.IsGlobal = 1  AND s.status = 5 
     LEFT JOIN    
        (    
        SELECT    
        AssignedSkillId,    
        ft.IsGlobal,    
        FT.TITLE,    
        FT.ID,    
        fp.id as FormulaID,    
        fp.Name AS FormulaName,    
        fp.IsGlobal AS FormulaIsGlobal,    
        fp.DateCreated AS FormulaCreatedDate,    
        fp.LastUpdated as FormulaUpdatedDate,    
        fp.Status as FormulaStatus     
        FROM    
        FormulaTask ft     
        join    
        FormulaProject fp     
        on ft.FormulaProjectId = fp.Id     
        and fp.IsGlobal = 1     
        AND fp.Status in ( 1, 2, 3, 4 )    
        )    
        ft     
        on ft.AssignedSkillId = s.id     
        and ft.IsGlobal = 1     
     LEFT JOIN    
        FormulaTaskVendor FTV     
        on ft.Id = ftv.FormulaTaskId     
        and ftv.Status = 3      -- status 3 means certified vendor    
     LEFT JOIN    
        UserProfile up     
        ON ftv.VendorGuid = up.UserId     
     WHERE    
     t.ISGLOBAL = 1     
     AND t.STATUS = 5     
     --and s.IsGlobal = 1     
     --and s.status = 5
	  ORDER BY    
        t.IsGlobal desc,  
		t.Name,  
        S.IsGlobal DESC,    
		s.Name,  
        t.Id  
     END    
   ELSE   
       
     SELECT DISTINCT    
     t.Id AS Id,    
     t.Name AS TeamName,    
     t.IsGlobal AS TeamIsGlobal,    
     t.Status AS TeamStatus,    
     s.Id AS SkillId,    
     s.Name AS SkillName,    
     s.IsGlobal AS SkillIsGlobal,    
     s.Status AS SkillStatus,    
     FormulaID,    
     FormulaName,    
     FormulaIsGlobal,    
     FormulaCreatedDate,    
     FormulaUpdatedDate,    
     FormulaStatus,    
     ft.Title as TaskName,    
     ft.Id as TaskID,    
     ft.IsGlobal as TaskIsGlobal,    
     STUFF((     
     select    
     ',' + up2.FullName     
     from    
     FormulaTaskVendor FTV2     
     LEFT JOIN    
        UserProfile up2     
        ON FTV2.VendorGuid = up2.UserId     
     where    
     FTV2.FormulaTaskId = FTV.FormulaTaskId     
     order by    
     up2.FullName for xml path('') ), 1, 1, '') as OutsourcerName     
     FROM    
     Team t     
     LEFT JOIN    
        teamskill ts     
        ON t.Id = ts.teamid     
     LEFT JOIN    
        Skill s     
        ON s.Id = ts.skillid     
     LEFT JOIN    
        (    
        SELECT    
        AssignedSkillId,    
        ft.IsGlobal,    
        FT.TITLE,    
        FT.ID,    
        fp.id as FormulaID,    
        fp.Name AS FormulaName,    
        fp.IsGlobal AS FormulaIsGlobal,    
        fp.DateCreated AS FormulaCreatedDate,    
        fp.LastUpdated as FormulaUpdatedDate,    
        fp.Status as FormulaStatus     
        FROM    
        FormulaTask ft     
        join    
        FormulaProject fp     
        on ft.FormulaProjectId = fp.Id     
        and fp.IsGlobal = 1     
        AND fp.Status =5    
        )    
       ft     
        on ft.AssignedSkillId = s.id     
        and ft.IsGlobal = 1     
     LEFT JOIN    
        FormulaTaskVendor FTV     
        on ft.Id = ftv.FormulaTaskId     
        and ftv.Status = 3      -- status 3 means certified vendor    
     LEFT JOIN    
        UserProfile up     
       ON ftv.VendorGuid = up.UserId     
     WHERE    
     t.ISGLOBAL = 1     
     AND t.STATUS = 5     
     and s.IsGlobal = 1     
     and s.status = 5     
    
    
     UNION ALL    
     -- Only Owner query: Global Team,Skills used to create custom Formulas.    
     SELECT DISTINCT    
     t.Id AS Id,    
     t.Name AS TeamName,    
     t.IsGlobal AS TeamIsGlobal,    
     t.Status AS TeamStatus,    
     s.Id AS SkillId,    
     s.Name AS SkillName,    
     s.IsGlobal AS SkillIsGlobal,    
     s.Status AS SkillStatus,    
     fp.id as FormulaID,    
     fp.Name AS FormulaName,    
     fp.IsGlobal AS FormulaIsGlobal,    
     fp.DateCreated AS FormulaCreatedDate,    
     fp.LastUpdated as FormulaUpdatedDate,    
     fp.Status as FormulaStatus,    
     ft.Title as TaskName,    
     ft.Id as TaskID,    
     ft.IsGlobal as TaskIsGlobal,    
     --up.UserId as OutsourcerId,     
     --up.FullName AS OutsourcerName     
     stuff((     
     select    
        ',' + up2.FullName     
     from    
        FormulaTaskVendor FTV2     
        LEFT JOIN    
        UserProfile up2     
        ON FTV2.VendorGuid = up2.UserId     
     where    
        FTV2.FormulaTaskId = FTV.FormulaTaskId     
     order by    
        up2.FullName for xml path('') ), 1, 1, '') as OutsourcerName     
     FROM    
        TEAMSKILL TS     
        JOIN    
        TEAM T     
        ON TS.TeamId = T.ID     
        JOIN    
        SKILL S     
        ON TS.SkillId = S.ID     
        and s.IsGlobal = 1     
        and s.status = 5     
        JOIN    
        FormulaTask ft     
        on ft.AssignedSkillId = s.id     
        and ft.ownerguid = @ownerID     
        join    
        FormulaProject fp     
        on fp.id = ft.FormulaProjectId     
        and fp.ownerguid = @ownerID     
        LEFT JOIN    
        FormulaTaskVendor FTV     
        on ft.Id = ftv.FormulaTaskId     
        and ftv.Status = 3     
        LEFT JOIN    
        UserProfile up     
        ON ftv.VendorGuid = up.UserId      
    
    
     UNION ALL    
    
    
     -- Only Owner query: Custom Team or Custome skills used to create custom formulas    
     SELECT DISTINCT    
        t.Id AS Id,    
        t.Name AS TeamName,    
        t.IsGlobal AS TeamIsGlobal,    
        t.Status AS TeamStatus,    
        s.Id AS SkillId,    
        s.Name AS SkillName,    
        s.IsGlobal AS SkillIsGlobal,    
        s.Status AS SkillStatus,    
        fp.id as FormulaID,    
        fp.Name AS FormulaName,    
        fp.IsGlobal AS FormulaIsGlobal,    
        fp.DateCreated AS FormulaCreatedDate,    
        fp.LastUpdated as FormulaUpdatedDate,    
        fp.Status as FormulaStatus,    
        ft.Title as TaskName,    
        ft.Id as TaskID,    
        ft.IsGlobal as TaskIsGlobal,    
        --up.UserId as OutsourcerId,     
        --up.FullName AS OutsourcerName     
        stuff((     
        select    
        ',' + up2.FullName     
        from    
        FormulaTaskVendor FTV2     
        LEFT JOIN    
        UserProfile up2     
        ON FTV2.VendorGuid = up2.UserId     
        where    
        FTV2.FormulaTaskId = FTV.FormulaTaskId     
        order by    
        up2.FullName for xml path('') ), 1, 1, '') as OutsourcerName     
        FROM    
        Team t     
        LEFT JOIN    
        teamskill ts     
        ON t.Id = ts.teamid     
        LEFT JOIN    
        Skill s     
        ON s.Id = ts.skillid     
        LEFT JOIN    
        FormulaTask ft     
        on ft.AssignedSkillId = s.id     
        and ft.ownerguid = @ownerID     
        LEFT join    
        FormulaProject fp     
        on fp.id = ft.FormulaProjectId     
        and fp.ownerguid = @ownerID     
        LEFT JOIN    
        FormulaTaskVendor FTV     
        on ft.Id = ftv.FormulaTaskId     
        and ftv.Status = 3     
        LEFT JOIN    
        UserProfile up     
        ON ftv.VendorGuid = up.UserId     
        where    
       --t.ownerguid = @ownerID     
	   (s.ownerguid = @ownerID  OR t.ownerguid = @ownerID)
        ORDER BY    
        t.IsGlobal desc,  
		t.Name,  
        S.IsGlobal DESC,    
		s.Name,  
        t.Id     
END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
