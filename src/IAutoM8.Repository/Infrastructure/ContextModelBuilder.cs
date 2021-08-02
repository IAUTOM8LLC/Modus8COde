using System;
using IAutoM8.Domain.Models;
using IAutoM8.Domain.Models.Abstract.Task;
using IAutoM8.Domain.Models.Business;
using IAutoM8.Domain.Models.Client;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.Team;
using IAutoM8.Domain.Models.User;
using Microsoft.EntityFrameworkCore;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.Global.Enums;

namespace IAutoM8.Repository.Infrastructure
{
    public static class ContextModelBuilder
    {
        public static void Configure(ModelBuilder builder)
        {
            builder.Ignore(typeof(TaskConditionOption<ProjectTaskCondition, ProjectTask>));
            builder.Ignore(typeof(TaskConditionOption<FormulaTaskCondition, FormulaTask>));

            MapUsers(builder);
            MapProjects(builder);
            MapFormulas(builder);
            MapSkill(builder);
            MapClients(builder);
            MapResources(builder);
            MapNotificationSettings(builder);
            MapVendors(builder);
            MapCredits(builder);
        }

        private static void MapVendors(ModelBuilder builder)
        {
            builder.Entity<FormulaTaskVendor>(t =>
            {
                t.HasKey(x => x.Id);
                t.HasOne(x => x.FormulaTask)
                    .WithMany(x => x.FormulaTaskVendors)
                    .HasForeignKey(x => x.FormulaTaskId)
                    .OnDelete(DeleteBehavior.Cascade);
                t.HasOne(x => x.Vendor)
                    .WithMany(x => x.FormulaTaskVendors)
                    .HasForeignKey(x => x.VendorGuid)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<ProjectTaskVendor>(t =>
            {
                t.HasKey(x => x.Id);
                t.HasOne(x => x.ProjectTask)
                    .WithMany(x => x.ProjectTaskVendors)
                    .HasForeignKey(x => x.ProjectTaskId)
                    .OnDelete(DeleteBehavior.Cascade);
                t.HasOne(x => x.Vendor)
                    .WithMany(x => x.ProjectTaskVendors)
                    .HasForeignKey(x => x.VendorGuid)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<FormulaTaskStatistic>(t =>
            {
                t.HasKey(x => x.Id);
            });

            builder.Entity<VendorPaymentRequest>(t =>
            {
                t.HasKey(x => x.Id);
            });
        }

        private static void MapResources(ModelBuilder builder)
        {
            builder.Ignore(typeof(ResourceBase));
            
            builder.Entity<Resource>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(250);
                t.Property(x => x.Path)
                    .IsRequired()
                    .HasMaxLength(300)
                    .HasDefaultValue("");
                t.Property(x => x.Mime)
                    .HasMaxLength(150);
                t.Property(x => x.Size)
                    .IsRequired()
                    .HasDefaultValue(0);

            });
            builder.Entity<ResourceFormula>(t =>
            {
                t.HasKey(x => new { x.ResourceId, x.FormulaId });
                t.HasOne(x => x.Formula)
                    .WithMany(x => x.ResourceFormula)
                    .HasForeignKey(k => k.FormulaId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasOne(x => x.Resource)
                    .WithMany(x => x.ResourceFormula)
                    .HasForeignKey(k => k.ResourceId)
                    .OnDelete(DeleteBehavior.Restrict);

            });
            builder.Entity<ResourceFormulaTask>(t =>
            {
                t.HasKey(x => new { x.ResourceId, x.FormulaTaskId });
                t.HasOne(x => x.FormulaTask)
                    .WithMany(x => x.ResourceFormulaTask)
                    .HasForeignKey(k => k.FormulaTaskId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasOne(x => x.Resource)
                    .WithMany(x => x.ResourceFormulaTask)
                    .HasForeignKey(k => k.ResourceId)
                    .OnDelete(DeleteBehavior.Restrict);

            });
            builder.Entity<ResourceProject>(t =>
            {
                t.HasKey(x => new { x.ResourceId, x.ProjectId });
                t.HasOne(x => x.Project)
                    .WithMany(x => x.ResourceProject)
                    .HasForeignKey(k => k.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasOne(x => x.Resource)
                    .WithMany(x => x.ResourceProject)
                    .HasForeignKey(k => k.ResourceId)
                    .OnDelete(DeleteBehavior.Restrict);

            });
            builder.Entity<ResourceProjectTask>(t =>
            {
                t.HasKey(x => new { x.ResourceId, x.ProjectTaskId });
                t.HasOne(x => x.ProjectTask)
                    .WithMany(x => x.ResourceProjectTask)
                    .HasForeignKey(k => k.ProjectTaskId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasOne(x => x.Resource)
                    .WithMany(x => x.ResourceProjectTask)
                    .HasForeignKey(k => k.ResourceId)
                    .OnDelete(DeleteBehavior.Restrict);

            });
        }

        private static void MapSkill(ModelBuilder builder)
        {
            builder.Entity<Team>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(250);
                t.HasMany(x => x.ReviewingTasks)
                    .WithOne(x => x.ReviewingTeam)
                    .HasForeignKey(k => k.ReviewingTeamId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasMany(x => x.AssignedTasks)
                    .WithOne(x => x.AssignedTeam)
                    .HasForeignKey(k => k.AssignedTeamId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasOne(x => x.Owner)
                    .WithMany(x => x.Teams)
                    .HasForeignKey(k => k.OwnerGuid)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasMany(x => x.AssignedFormulaTasks)
                    .WithOne(x => x.AssignedFormulaTeam)
                    .HasForeignKey(k => k.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasMany(x => x.AssignedProjectTasks)
                    .WithOne(x => x.AssignedProjectTeam)
                    .HasForeignKey(k => k.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);

            });
            builder.Entity<TeamUser>(t =>
            {
                t.HasKey(x => new { x.TeamId, x.UserId });
                t.HasOne(x => x.Team)
                    .WithMany(x => x.TeamUsers)
                    .HasForeignKey(k => k.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasOne(x => x.User)
                    .WithMany(x => x.TeamUsers)
                    .HasForeignKey(k => k.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Skill>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(250);
                t.HasMany(x => x.ReviewingTasks)
                    .WithOne(x => x.ReviewingSkill)
                    .HasForeignKey(k => k.ReviewingSkillId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasMany(x => x.AssignedTasks)
                    .WithOne(x => x.AssignedSkill)
                    .HasForeignKey(k => k.AssignedSkillId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasMany(x => x.ReviewingFormulaTasks)
                    .WithOne(x => x.ReviewingSkill)
                    .HasForeignKey(k => k.ReviewingSkillId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasMany(x => x.AssignedFormulaTasks)
                    .WithOne(x => x.AssignedSkill)
                    .HasForeignKey(k => k.AssignedSkillId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasOne(x => x.Owner)
                    .WithMany(x => x.Skills)
                    .HasForeignKey(k => k.OwnerGuid)
                    .OnDelete(DeleteBehavior.Restrict);

            });
            builder.Entity<ProjectTaskUser>(t =>
            {
                t.HasKey(x => new {x.UserId, x.ProjectTaskId, x.ProjectTaskUserType});
                t.HasOne(x => x.User)
                    .WithMany(x => x.ProjectTaskUsers)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                t.HasOne(x => x.ProjectTask)
                    .WithMany(x => x.ProjectTaskUsers)
                    .HasForeignKey(x => x.ProjectTaskId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<UserSkill>(t =>
            {
                t.HasKey(x => new { x.SkillId, x.UserId });
                t.HasOne(x => x.Skill)
                    .WithMany(x => x.UserSkills)
                    .HasForeignKey(k => k.SkillId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasOne(x => x.User)
                    .WithMany(x => x.UserSkills)
                    .HasForeignKey(k => k.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<TeamSkill>(t =>
            {
                t.HasKey(x => new { x.SkillId, x.TeamId });
                t.HasOne(x => x.Skill)
                    .WithMany(x => x.TeamSkills)
                    .HasForeignKey(k => k.SkillId)
                    .OnDelete(DeleteBehavior.Cascade);
                t.HasOne(x => x.Team)
                    .WithMany(x => x.TeamSkills)
                    .HasForeignKey(k => k.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<FormulaTeam>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(250);
                t.HasMany(x => x.ReviewingTasks)
                    .WithOne(x => x.ReviewingTeam)
                    .HasForeignKey(k => k.ReviewingTeamId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasMany(x => x.AssignedTasks)
                    .WithOne(x => x.AssignedTeam)
                    .HasForeignKey(k => k.AssignedTeamId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasOne(x => x.Owner)
                    .WithMany(x => x.FormulaTeams)
                    .HasForeignKey(k => k.OwnerGuid)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void MapUsers(ModelBuilder builder)
        {
            builder.Entity<User>(t =>
            {
                t.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(250);
                t.Property(x => x.IsPayed)
                    .IsRequired()
                    .HasDefaultValue(false);
            });

            builder.Entity<UserProject>(t =>
            {
                t.HasKey(k => new { k.UserId, k.ProjectId });

                t.HasOne(x => x.User)
                    .WithMany(x => x.UserProjects)
                    .HasForeignKey(k => k.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                t.HasOne(x => x.Project)
                    .WithMany(x => x.UserProjects)
                    .HasForeignKey(k => k.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<UserProfile>(t =>
            {
                t.HasKey(x => x.UserId);
                t.HasOne(x => x.User).WithOne(x => x.Profile);
                t.Property(p => p.AffiliateCode).HasMaxLength(20);
                t.Property(p => p.AffiliatePass).HasMaxLength(10);
            });

            builder.Entity<UserRole>(t =>
            {
                t.HasOne(x => x.User)
                    .WithMany(x => x.Roles)
                    .HasForeignKey(x => x.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                t.HasOne(x => x.Role)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.RoleId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Business>(t =>
            {
                t.HasKey(x => x.UserId);
                t.Property(x => x.ToDoSummaryTime).HasDefaultValue(new DateTime(1,1,1,8,0,0));
            });

            builder.Entity<InfusionSignUp>(t =>
            {
                t.HasKey(x => x.Id);
            });
        }

        private static void MapProjects(ModelBuilder builder)
        {
            builder.Entity<Project>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Name).IsRequired().HasMaxLength(100);
                t.Property(x => x.DateCreated).IsRequired();
                t.Property(x => x.StartDate).IsRequired();

                t.HasOne(x => x.Owner)
                    .WithMany(x => x.UserCreatedProjects)
                    .HasForeignKey(x => x.OwnerGuid);

                t.HasOne(x => x.Parent)
                    .WithMany(x => x.Children)
                    .HasForeignKey(x => x.ParentProjectId);
            });

            builder.Entity<ProjectTask>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Title).HasMaxLength(100);
                t.Property(x => x.DateCreated).IsRequired();
                t.Property(x => x.IsShareResources).IsRequired().HasDefaultValue(false);
                t.Property(x => x.IsTrainingLocked).IsRequired().HasDefaultValue(false);
                t.Property(x => x.DescNotificationFlag).IsRequired().HasDefaultValue(false);

                t.HasOne(x => x.Project)
                    .WithMany(x => x.Tasks)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(x => x.RecurrenceOptions)
                    .WithOne()
                    .HasForeignKey<ProjectTask>(x => x.RecurrenceOptionsId);

                t.HasOne(x => x.Owner)
                    .WithMany(x => x.UserCreatedTasks)
                    .HasForeignKey(x => x.OwnerGuid);

                t.HasMany(x => x.AssignedConditionOptions)
                    .WithOne(x => x.AssignedTask)
                    .HasForeignKey(x => x.AssignedTaskId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                t.HasOne(x => x.Condition)
                   .WithOne(x => x.Task)
                   .HasForeignKey<ProjectTask>(x => x.TaskConditionId)
                   .IsRequired(false);

                t.HasOne(x => x.ReviewingUser)
                    .WithMany(x => x.ReviewingTasks)
                    .HasForeignKey(k => k.ReviewingUserGuid)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(x => x.ProccessingUser)
                    .WithMany(x => x.ProccessingTasks)
                    .HasForeignKey(k => k.ProccessingUserGuid)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(x => x.FormulaProject)
                    .WithMany(x => x.ProjectTasks)
                    .HasForeignKey(k => k.FormulaId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(x => x.ParentTask)
                    .WithMany(x => x.FormulaProjectTask)
                    .HasForeignKey(k => k.ParentTaskId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(x => x.FormulaTask)
                    .WithMany(x => x.ProjectTasks)
                    .HasForeignKey(k => k.FormulaTaskId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ProjectTaskDependency>(t =>
            {
                t.HasKey(x => new { x.ParentTaskId, x.ChildTaskId });

                t.HasOne(x => x.ParentTask)
                    .WithMany(x => x.ChildTasks)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(x => x.ChildTask)
                    .WithMany(x => x.ParentTasks)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ProjectTaskCondition>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Condition).IsRequired().HasMaxLength(250);

                t.HasMany(x => x.Options)
                    .WithOne()
                    .HasForeignKey(x => x.TaskConditionId);
            });

            builder.Entity<ProjectTaskConditionOption>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Option).IsRequired().HasMaxLength(250);

                t.HasOne(x => x.Condition)
                    .WithMany(x => x.Options)
                    .HasForeignKey(x => x.TaskConditionId);
            });
            builder.Entity<ProjectTaskComment>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Text).HasMaxLength(500);
                t.Property(x => x.DateCreated).IsRequired();

                t.HasOne(x => x.ProjectTask)
                    .WithMany(x => x.ProjectTaskComments);

                t.HasOne(x => x.User)
                    .WithMany(x => x.ProjectTaskComments)
                    .HasForeignKey(x => x.UserGuid)
                     .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TaskJob>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.HangfireJobId).IsRequired().HasMaxLength(50);
                t.HasOne(x => x.Task)
                     .WithMany(x => x.TaskJobs)
                     .HasForeignKey(x => x.TaskId)
                     .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TaskHistory>(t =>
            {
                t.HasKey(x => x.Id);
                t.HasIndex(x => x.HistoryTime);
                t.HasOne(x => x.Task)
                    .WithMany(x => x.TaskHistories)
                    .HasForeignKey(x => x.TaskId)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasOne(x => x.ProjectTaskConditionOption)
                    .WithMany(x => x.TaskHistories)
                    .HasForeignKey(x => x.ProjectTaskConditionOptionId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
                t.HasOne(x => x.User)
                    .WithMany(x => x.TaskHistories)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });

            builder.Entity<RecurrenceOptions>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Cron).HasMaxLength(50);
                t.Property(x => x.DayDiff).IsRequired()
                    .HasDefaultValue(0);
                t.Property(x => x.IsAsap).IsRequired()
                    .HasDefaultValue(false);
            });

            builder.Entity<ProjectTaskChecklist>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Name).IsRequired().HasMaxLength(150);
                t.Property(x => x.TodoIsChecked).IsRequired().HasDefaultValue(false).HasColumnName("IsChecked_Todo");
                t.Property(x => x.ReviewerIsChecked).IsRequired().HasDefaultValue(false).HasColumnName("IsChecked_Reviewer");
                t.Property(x => x.DateCreated).IsRequired();
            });

            builder.Entity<ProjectNote>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Text).IsRequired().HasMaxLength(400);
                t.Property(x => x.DateCreated).IsRequired();
                t.Property(x => x.IsPublished).HasDefaultValue(false);
            });
        }

        private static void MapFormulas(ModelBuilder builder)
        {
            builder.Entity<FormulaProject>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Name).IsRequired().HasMaxLength(100);
                t.Property(x => x.IsLocked).IsRequired().HasDefaultValue(false);
                t.Property(x => x.DateCreated).IsRequired();
                t.Property(x => x.IsStarred).HasDefaultValue(false);
                t.HasOne(x => x.Owner)
                    .WithMany(x => x.UserCreatedFormulaProjects)
                    .HasForeignKey(x => x.OwnerGuid);
                t.HasOne(x => x.OriginalFormulaProject)
                    .WithMany()
                    .HasForeignKey(x => x.OriginalFormulaProjectId);
                t.HasMany(x => x.ChildFormulaProjects)
                    .WithOne(x => x.OriginalFormulaProject)
                    .HasForeignKey(x => x.OriginalFormulaProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<FormulaTask>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Title).HasMaxLength(100);
                t.Property(x => x.IsShareResources).IsRequired().HasDefaultValue(false);
                t.Property(x => x.IsTrainingLocked).IsRequired().HasDefaultValue(false);
                t.Property(x => x.DescNotificationFlag).IsRequired().HasDefaultValue(false);

                t.HasOne(x => x.RecurrenceOptions)
                    .WithOne()
                    .HasForeignKey<FormulaTask>(x => x.RecurrenceOptionsId);

                t.HasOne(x => x.FormulaProject)
                    .WithMany(x => x.FormulaTasks)
                    .OnDelete(DeleteBehavior.Restrict);
                t.HasOne(x => x.InternalFormulaProject)
                    .WithMany(x => x.InternalFormulaTasks)
                    .HasForeignKey(x => x.InternalFormulaProjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(x => x.Owner)
                    .WithMany(x => x.UserCreatedFormulaTasks)
                    .HasForeignKey(x => x.OwnerGuid);

                t.HasMany(x => x.AssignedConditionOptions)
                    .WithOne(x => x.AssignedTask)
                    .HasForeignKey(x => x.AssignedTaskId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                t.HasMany(x => x.ChildFormulaTasks)
                    .WithOne(x => x.OriginalFormulaTask)
                    .HasForeignKey(x => x.OriginalFormulaTaskId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(x => x.Condition)
                    .WithOne(x => x.Task)
                    .HasForeignKey<FormulaTask>(x => x.TaskConditionId)
                    .IsRequired(false);
            });

            builder.Entity<FormulaTaskDependency>(t =>
            {
                t.HasKey(x => new { x.ParentTaskId, x.ChildTaskId });

                t.HasOne(x => x.ParentTask)
                    .WithMany(x => x.ChildTasks)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(x => x.ChildTask)
                    .WithMany(x => x.ParentTasks)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<FormulaShare>(t =>
            {
                t.HasKey(x => new { x.FormulaProjectId, x.UserId });

                t.HasOne(x => x.AccessHolder)
                    .WithMany(x => x.AccessibleFormulas)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(x => x.FormulaProject)
                    .WithMany(x => x.FormulaShares)
                    .HasForeignKey(x => x.FormulaProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<FormulaTaskCondition>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Condition).IsRequired().HasMaxLength(250);

                t.HasMany(x => x.Options)
                    .WithOne()
                    .HasForeignKey(x => x.TaskConditionId);
            });

            builder.Entity<FormulaTaskConditionOption>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Option).IsRequired().HasMaxLength(250);

                t.HasOne(x => x.Condition)
                    .WithMany(x => x.Options)
                    .HasForeignKey(x => x.TaskConditionId);
            });

            builder.Entity<FormulaTaskChecklist>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Name).IsRequired().HasMaxLength(150);
                t.Property(x => x.DateCreated).IsRequired();
            });

            builder.Entity<Category>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Name).IsRequired().HasMaxLength(100);
            });

            builder.Entity<FormulaProjectCategory>(t =>
            {
                t.HasKey(x => new { x.FormulaProjectId, x.CategoryId });
                t.HasOne(x => x.FormulaProject)
                    .WithMany(m => m.FormulaProjectCategories)
                    .HasForeignKey(k => k.FormulaProjectId);

                t.HasOne(x => x.Category)
                    .WithMany(m => m.FormulaProjectCategories)
                    .HasForeignKey(k => k.CategoryId);
            });

            builder.Entity<FormulaNote>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Text).IsRequired().HasMaxLength(400);
                t.Property(x => x.DateCreated).IsRequired();
            });

            builder.Entity<FormulaTaskDisableStatus>(t =>
            {
                t.HasKey(x => new
                {
                    x.ChildFormulaId,
                    x.ParentFormulaId,
                    x.InternalChildFormulaId,
                    x.InternalChildFormulaTaskId,
                });
                t.Property(x => x.DateCreated).IsRequired();
                t.Property(x => x.IsDisabled).IsRequired();
            });
        }

        private static void MapClients(ModelBuilder builder)
        {
            builder.Entity<Client>(c =>
            {
                c.HasKey(x => x.Id);
                c.Property(x => x.CompanyName).IsRequired().HasMaxLength(250);
                c.Property(x => x.Email).IsRequired().HasMaxLength(250);
                c.Property(x => x.FirstName).IsRequired().HasMaxLength(50);
                c.Property(x => x.LastName).IsRequired().HasMaxLength(50);
                c.Property(x => x.City).HasMaxLength(50);
                c.Property(x => x.Country).HasMaxLength(50);
                c.Property(x => x.MobilePhone).HasMaxLength(50);
                c.Property(x => x.OfficePhone).HasMaxLength(50);
                c.Property(x => x.State).HasMaxLength(50);
                c.Property(x => x.StreetAddress1).HasMaxLength(100);
                c.Property(x => x.StreetAddress2).HasMaxLength(100);
                c.Property(x => x.Zip).HasMaxLength(12);

                c.HasMany(x => x.Projects)
                    .WithOne(x => x.Client)
                    .HasForeignKey(x => x.ClientId);

                c.HasOne(x => x.BusinessOwner)
                    .WithMany(x => x.Clients)
                    .HasForeignKey(x => x.BusinessOwnerGuid);
            });
        }

        private static void MapNotificationSettings(ModelBuilder builder)
        {
            builder.Entity<NotificationSetting>(c =>
            {
                c.HasKey(x => x.Id);

                c.HasOne(x => x.Bussiness)
                    .WithMany(x => x.NotificationSettings)
                    .HasForeignKey(x => x.BussinessId)
                    .OnDelete(DeleteBehavior.Cascade);

                c.HasOne(x => x.Role)
                    .WithMany()
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                c.HasOne(x => x.User)
                    .WithMany(x => x.NotificationSettings)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                c.HasOne(x => x.Task)
                    .WithMany(x => x.NotificationSettings)
                    .HasForeignKey(x => x.TaskId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<Notification>(c =>
            {
                c.HasKey(x => x.Id);

                c.HasOne(x => x.Recipient)
                    .WithMany(x => x.RecepientNotifications)
                    .HasForeignKey(x => x.RecipientGuid)
                    .OnDelete(DeleteBehavior.Restrict);

                c.HasOne(x => x.Sender)
                    .WithMany(x => x.SenderNotifications)
                    .HasForeignKey(x => x.SenderGuid)
                    .OnDelete(DeleteBehavior.Restrict);

                c.HasOne(x => x.Task)
                    .WithMany(x => x.Notifications)
                    .HasForeignKey(x => x.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                c.Property(x => x.Message).HasMaxLength(200);
                c.Property(x => x.Url).HasMaxLength(50);
            });
        }

        private static void MapCredits(ModelBuilder builder)
        {
            builder.Entity<Credits>(t =>
            {
                t.HasKey(x => x.UserId);
                t.HasOne(x => x.User).WithOne(x => x.Credits);
            });

            builder.Entity<CreditsTax>(c =>
            {
                c.HasKey(x => x.Id);
            });

            builder.Entity<TransferRequest>(c =>
            {
                c.HasKey(x => x.Id);
                c.HasIndex(x => x.RequestTime);
                c.HasOne(x => x.Vendor)
                    .WithMany(x => x.TransferRequests)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(true);
            });

            builder.Entity<CreditLog>(c =>
            {
                c.HasKey(x => x.Id);
                c.HasIndex(x => x.HistoryTime);
                c.HasOne(x => x.Manager)
                    .WithMany(x => x.ManagerCreditLogs)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(true);

                c.HasOne(x => x.Vendor)
                    .WithMany(x => x.VendorCreditLogs)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                c.HasOne(x => x.ProjectTask)
                    .WithMany(x => x.CreditLogs)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });
        }
    }
}
