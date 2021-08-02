using AutoMapper;
using Hangfire;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.User;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global;
using IAutoM8.Global.Enums;
using IAutoM8.Global.Exceptions;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.Projects.Dto;
using IAutoM8.Service.Projects.Interfaces;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Data.SqlClient;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace IAutoM8.Service.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly IRepo _repo;
        private readonly UserManager<User> _userManager;
        private readonly ITaskService _taskService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ClaimsPrincipal _principal;
        private readonly IStorageService _storageService;
        private readonly INotificationService _notificationService;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly IJobService _jobService;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ProjectService(
            IRepo repo,
            UserManager<User> userManager,
            ITaskService taskService,
            IDateTimeService dateTimeService,
            ClaimsPrincipal principal,
            IStorageService storageService,
            INotificationService notificationService,
            ITaskNeo4jRepository taskNeo4JRepository,
            IMapper mapper,
            IJobService jobService,
            
            IHostingEnvironment hostingEnvironment)
        {
            _repo = repo;
            _userManager = userManager;
            _taskService = taskService;
            _dateTimeService = dateTimeService;
            _principal = principal;
            _storageService = storageService;
            _notificationService = notificationService;
            _taskNeo4JRepository = taskNeo4JRepository;
            _mapper = mapper;
            _jobService = jobService;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<List<ProjectDto>> GetOwnProjects()
        {
            IQueryable<Project> query = _repo.Read<Project>()
                .Include(p => p.Owner)
                .Include(p => p.Client)
                .Include(p => p.UserProjects)
                    .ThenInclude(p => p.User)
                    .ThenInclude(p => p.Profile)
                .Include(p => p.Parent)
                .Include(p => p.Children);

            var userId = _principal.GetUserId();
            if (_principal.IsOwner())
            {
                query = query.Where(x => x.OwnerGuid == userId);
            }
            else if (_principal.IsManager())
            {
                //TODO need to refactor this for now EF Core doesnt support IQueryable Union #246
                var projectIds = await GetProjectIds(userId);
                query = query.Where(w => projectIds.Contains(w.Id));
            }
            else if (_principal.IsVendor())
            {
                query = query.Join(_repo.Read<ProjectTaskVendor>()
                        .Include(i => i.ProjectTask)
                        .Where(w => w.VendorGuid == userId && w.Status == ProjectRequestStatus.Accepted)
                        .Select(s => s.ProjectTask.ProjectId)
                        .GroupBy(g => g),
                    outer => outer.Id,
                    inner => inner.Key,
                    (project, userProject) => project
                );
            }
            else
            {
                query = query.Join(_repo.Read<ProjectTaskUser>()
                        .Include(i => i.ProjectTask)
                        .Where(w => w.UserId == userId)
                        .Select(s => s.ProjectTask.ProjectId)
                        .GroupBy(g => g),
                    outer => outer.Id,
                    inner => inner.Key,
                    (project, userProject) => project
                );
            }
            var projects = await query.ToListAsync();
            return projects.Select(Mapper.Map<ProjectDto>).ToList();
        }

        public async Task<List<ProjectDto>> GetChildProjets(int parentProjectId)
        {
            var childProjects = await _repo.Read<Project>()
                .Include(i => i.Children)
                .Where(x => x.Id == parentProjectId)
                .SelectMany(s => s.Children)
                .ToListAsync();

            return childProjects.Select(Mapper.Map<ProjectDto>).ToList();
        }

        public async Task<IEnumerable<int>> GetOwnProjectsIds(Guid? userIdToLoad = null)
        {
            IQueryable<Project> query = _repo.Read<Project>();

            Guid userId = userIdToLoad ?? _principal.GetUserId();
            var user = _repo.Read<User>().FirstOrDefault(t => t.Id == userId);

            if (await _userManager.IsInRoleAsync(user, UserRoles.Owner))
            {
                query = query.Where(x => x.OwnerGuid == userId);
            }
            else if (await _userManager.IsInRoleAsync(user, UserRoles.Manager))
            {
                //TODO need to refactor this for now EF Core doesnt support IQueryable Union #246
                var projectIds = await GetProjectIds(userId);
                query = query.Where(w => projectIds.Contains(w.Id));
            }
            //else if (await _userManager.IsInRoleAsync(user, UserRoles.Vendor))
            //{
            //    query = query.Join(_repo.Read<ProjectTaskVendor>()
            //            .Include(i => i.ProjectTask)
            //            .Where(w => w.VendorGuid == userId && w.Status == ProjectRequestStatus.Accepted)
            //            .Select(s => s.ProjectTask.ProjectId)
            //            .GroupBy(g => g),
            //        outer => outer.Id,
            //        inner => inner.Key,
            //        (project, userProject) => project
            //    );
            //}
            //else if (await _userManager.IsInRoleAsync(user, UserRoles.Vendor) || await _userManager.IsInRoleAsync(user, UserRoles.CompanyWorker)) // added logic for new role CompanyWorker WRT Sprint 10B
            //{
            //    query = query.Join(_repo.Read<ProjectTaskVendor>()
            //            .Include(i => i.ProjectTask)
            //            .Where(w => w.VendorGuid == userId && w.Status == ProjectRequestStatus.Accepted)
            //            .Select(s => s.ProjectTask.ProjectId)
            //            .GroupBy(g => g),
            //        outer => outer.Id,
            //        inner => inner.Key,
            //        (project, userProject) => project
            //    );
            //}
            else if (await _userManager.IsInRoleAsync(user, UserRoles.Vendor) || await _userManager.IsInRoleAsync(user, UserRoles.CompanyWorker) || await _userManager.IsInRoleAsync(user, UserRoles.Company)) // added logic for new role Company WRT Sprint 10B
            {
                query = query.Join(_repo.Read<ProjectTaskVendor>()
                        .Include(i => i.ProjectTask)
                        .Where(w => w.VendorGuid == userId && w.Status == ProjectRequestStatus.Accepted)
                        .Select(s => s.ProjectTask.ProjectId)
                        .GroupBy(g => g),
                    outer => outer.Id,
                    inner => inner.Key,
                    (project, userProject) => project
                );
            }
            else
            {
                query = query.Join(_repo.Read<ProjectTaskUser>()
                        .Include(i => i.ProjectTask)
                        .Where(w => w.UserId == userId && w.ProjectTaskUserType == ProjectTaskUserType.Assigned)
                        .Select(s => s.ProjectTask.ProjectId)
                        .GroupBy(g => g),
                    outer => outer.Id,
                    inner => inner.Key,
                    (project, userProject) => project
                );
            }

            return await query.Select(t => t.Id).ToListAsync();
        }

        private async Task<IEnumerable<int>> GetProjectIds(Guid userId)
        {
            return (await _repo.Read<ProjectTaskUser>()
                .Include(i => i.ProjectTask)
                .Where(w => w.UserId == userId)
                .Select(s => s.ProjectTask.ProjectId)
                .ToListAsync()).Concat(await _repo.Read<UserProject>()
                .Where(w => w.UserId == userId)
                .Select(s => s.ProjectId)
                .ToListAsync()).GroupBy(g => g).Select(s => s.Key);
        }

        public async Task<ProjectDto> GetProject(int projectId)
        {
            var userId = _principal.GetUserId();
            var ownerId = _principal.GetOwnerId();

            var query = _repo.Read<Project>()
                .Include(p => p.UserProjects)
                    .ThenInclude(p => p.User)
                    .ThenInclude(p => p.Profile)
                .Include(p => p.ResourceProject)
                    .ThenInclude(p => p.Resource)
                .Include(p => p.Parent)
                .Include(p => p.Children)
                .Where(w => w.Id == projectId && w.OwnerGuid == ownerId);

            if (_principal.IsManager())
            {
                //TODO need to refactor this for now EF Core doesnt support IQueryable Union #246
                var projectIds = await GetProjectIds(userId);
                query = query.Where(w => projectIds.Contains(w.Id));
            }
            else if (_principal.IsWorker())
            {
                query = _repo.Read<ProjectTaskUser>()
                    .Include(i => i.ProjectTask)
                    .ThenInclude(t => t.Project)
                    .Where(w => w.UserId == userId)
                    .Select(s => s.ProjectTask.Project);
            }

            var project = await query.FirstOrDefaultAsync();
            if (project == null)
                throw new ValidationException("Project is not found or access is denied");

            return Mapper.Map<ProjectDto>(project,
                opts =>
                {
                    opts.Items.Add("urlBuilder",
                        (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.Project)));
                });
        }

        public async Task<ProjectDto> AddProject(AddProjectDto data)
        {
            var userId = _principal.GetUserId();
            using (var trx = _repo.Transaction())
            {
                var project = Mapper.Map<Project>(data);
                project.DateCreated = DateTime.UtcNow;
                project.StartDate = DateTime.UtcNow;

                if (_principal.IsOwner())
                {
                    project.OwnerGuid = userId;
                }
                else if (_principal.IsManager())
                {
                    project.OwnerGuid = _principal.GetOwnerId();
                    project.UserProjects.Add(new UserProject { UserId = userId });
                }
                else
                {
                    throw new ForbiddenException();
                }

                await trx.AddAsync(project);
                await trx.SaveAndCommitAsync(CancellationToken.None);

                var projectId = project.Id;

                if (data.Managers?.Count > 0)
                {
                    await AssignManagers(_principal, projectId, data.Managers);
                }

                project = await _repo.Read<Project>()
                    .Include(p => p.Owner)
                    .Include(p => p.UserProjects)
                        .ThenInclude(up => up.User)
                    .ThenInclude(p => p.Profile)
                    .Include(p => p.Client)
                    .Include(p => p.Parent)
                    .Include(p => p.Children)
                    .FirstOrDefaultAsync(w => w.Id == projectId);

                return Mapper.Map<ProjectDto>(project);
            }
        }

        public async Task<ProjectDto> AddChildProject(AddChildProjectDto data)
        {
            // The UserProjects based on the ParentProjectId
            // These get passed to the child projects as well.
            var userProjects = await _repo.Read<UserProject>()
                .Include(p => p.User)
                    .ThenInclude(u => u.Profile)
                .Where(p => p.ProjectId == data.ParentProjectId)
                .Select(p => Mapper.Map<ProjectUserDto>(p))
                .ToListAsync();

            // Get the Parent Project details
            var project = await _repo.Read<Project>()
                .Select(p => Mapper.Map<ProjectDto>(p))
                .SingleOrDefaultAsync(p => p.Id == data.ParentProjectId);

            if (project == null)
                throw new ValidationException("Project is not found. Or your access is denied.");

            AddProjectDto model = new AddProjectDto()
            {
                Name = data.Name,
                ParentProjectId = data.ParentProjectId,
                Managers = userProjects,
                ClientId = project?.ClientId,
                Description = project?.Description
            };

            return await AddProject(model);
        }

        public async Task DeleteProject(int projectId)
        {
            //this transaction used in DeleteTasks method
            using (var transaction = _taskNeo4JRepository.BeginTransaction())
            using (var trx = _repo.Transaction())
            {
                if (_principal.CanBeOnlyWorker())
                    throw new ValidationException("Project access denied.");

                var userId = _principal.GetOwnerId();
                var project = await trx.Track<Project>()
                    .Include(t => t.UserProjects)
                    .Include(t => t.ResourceProject)
                    .ThenInclude(t => t.Resource)
                    .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerGuid == userId);

                if (project == null)
                    throw new ValidationException("Project is not found.");

                var resources = project.ResourceProject.Select(s => s.Resource).ToList();
                trx.RemoveRange(project.ResourceProject);
                await _storageService.DeleteFileAsync(projectId.ToString(), StorageType.Project);
                await trx.SaveChangesAsync();
                await _taskService.DeleteProjectTasksAsync(trx, project.Id);
                trx.RemoveRange(resources);
                trx.RemoveRange(project.UserProjects);
                trx.Remove(project);
                await trx.SaveAndCommitAsync(CancellationToken.None);
                transaction.Commit();
            }
        }

        public async Task DeleteChildProject(int projectId)
        {
            using (var trx = _repo.Transaction())
            {
                // Get the Child Project Details
                var project = await trx.Track<Project>()
                    .SingleOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                    throw new ValidationException("Project is not found. Or your access is denied.");

                trx.Remove(project);

                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task<ProjectDto> UpdateProject(ProjectDto data)
        {
            var userId = _principal.GetUserId();
            using (var trx = _repo.Transaction())
            {
                var query = trx.Track<Project>()
                    .Include(p => p.Owner)
                    .Include(p => p.UserProjects)
                    .ThenInclude(p => p.User)
                    .Where(p => p.Id == data.Id);

                if (_principal.IsOwner())
                {
                    //Ensure user is owner
                    query = query.Where(x => x.OwnerGuid == userId);
                }
                else if (_principal.IsManager())
                {
                    //TODO need to refactor this for now EF Core doesnt support IQueryable Union #246
                    var projectIds = await GetProjectIds(userId);
                    query = query.Where(w => projectIds.Contains(w.Id));
                }

                var project = await query.FirstOrDefaultAsync();
                if (project == null)
                    throw new ValidationException("Project is not found. Or your access is denied.");

                Mapper.Map(data, project);
                project.LastUpdated = _dateTimeService.NowUtc;

                await AssignManagers(_principal, data.Id, data.Managers);
                await trx.SaveAndCommitAsync();

                return Mapper.Map<ProjectDto>(trx
                    .Read<Project>()
                    .Include(p => p.Owner)
                    .Include(p => p.UserProjects)
                        .ThenInclude(p => p.User)
                            .ThenInclude(p => p.Profile)
                    .Include(p => p.Client)
                    .Include(p => p.Parent)
                    .Include(p => p.Children)
                    .FirstOrDefault(p => p.Id == data.Id));
            }
        }

        public async Task<ProjectDto> UpdateChildProject(AddChildProjectDto data)
        {
            using (var trx = _repo.Transaction())
            {
                // Get the Child Project Details
                var project = await trx.Track<Project>()
                    .SingleOrDefaultAsync(p => p.Id == data.ChildProjectId);

                if (project == null)
                    throw new ValidationException("Project is not found. Or your access is denied.");

                project.LastUpdated = _dateTimeService.NowUtc;
                project.ParentProjectId = data.ParentProjectId;

                await trx.SaveAndCommitAsync();

                return Mapper.Map<ProjectDto>(trx
                    .Read<Project>()
                    .Include(p => p.Owner)
                    .Include(p => p.UserProjects)
                        .ThenInclude(p => p.User)
                            .ThenInclude(p => p.Profile)
                    .Include(p => p.Client)
                    .Include(p => p.Parent)
                    .Include(p => p.Children)
                    .FirstOrDefault(p => p.Id == data.ChildProjectId));
            }
        }

        public async Task<Project> CreateProjectFromFormula(ITransactionScope trx, FormulaProject formula, DateTime startDate, string projectName, int? parentProjectId = null)
        {
            var userId = _principal.GetUserId();
            var project = new Project
            {
                Name = projectName,
                Description = formula.Description,
                DateCreated = _dateTimeService.NowUtc,
                StartDate = startDate.ToUniversalTime(),
                ParentProjectId = parentProjectId
            };

            if (_principal.IsOwner())
            {
                project.OwnerGuid = userId;
            }
            else
            {
                project.OwnerGuid = _principal.GetOwnerId();
                project.UserProjects.Add(new UserProject { UserId = userId });
            }
            await trx.AddAsync(project);
            await trx.SaveChangesAsync();
            await CopyFormulaResourcesToProject(project, formula);
            await trx.SaveChangesAsync();
            return project;
        }

        public async Task AssignToProject(AssignUsersToProjectDto model)
        {
            var query = _repo.Read<User>();
            query = model.Emails == null
                ? query.Where(w => model.Ids.Contains(w.Id))
                : query.Where(w => model.Emails.Contains(w.Email));

            var userIds = await query.Select(s => new { s.Id, s.Email }).ToListAsync();

            using (var trx = _repo.Transaction())
            {
                var ownerGuid = _principal.GetOwnerId();
                var project = await _repo.Read<Project>()
                    .Include(i => i.UserProjects)
                    .Where(w => w.Id == model.ProjectId)
                    .FirstAsync();

                var toDeleteList = project.UserProjects.Where(w => userIds.All(a => a.Id != w.UserId));
                trx.RemoveRange(toDeleteList);
                foreach (var user in userIds.Where(w => project.UserProjects.All(a => a.UserId != w.Id)))
                {
                    await trx.AddAsync(new UserProject
                    {
                        UserId = user.Id,
                        ProjectId = model.ProjectId
                    });
                    await _notificationService.SendAssignToProjectAsync(ownerGuid, project.Name, user.Email);
                }
                await trx.SaveAndCommitAsync(CancellationToken.None);
            }
        }

        public async Task<IEnumerable<AssignedUserDto>> GetAssignedUsers(int? projectId)
        {
            var userId = _principal.GetUserId();

            var users = await _repo.Read<User>()
                .Include(i => i.Profile)
                .Include(i => i.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(w => w.OwnerId == userId && w.Roles.Any(a => a.Role.Name == UserRoles.Manager))
                .Select(s => new AssignedUserDto
                {
                    Email = s.Email,
                    Id = s.Id,
                    FullName = s.Profile.FullName
                })
                .ToListAsync();

            return users;
        }

        public async Task<int> GetMostRecentId()
        {
            var projectId = await _repo.Read<TaskHistory>()
                .Include(i => i.Task)
                    .ThenInclude(i => i.Project)
                        .ThenInclude(t => t.Tasks)
                            .ThenInclude(t => t.ProjectTaskUsers)
                .Where(w => w.Task.OwnerGuid == _principal.GetOwnerId() &&
                        w.HistoryTime > _dateTimeService.NowUtc.AddDays(-1))
                .Where(w => w.Task.Project.UserProjects.Any(up => up.UserId == _principal.GetUserId())
                            || w.Task.Project.Tasks.Any(t =>
                                t.ProjectTaskUsers.Any(tu => tu.UserId == _principal.GetUserId())))
                .GroupBy(h => h.Task.ProjectId)
                .OrderByDescending(w => w.Count())
                .Select(s => s.Key)
                .FirstOrDefaultAsync();

            if (projectId == 0)
            {
                projectId = await _repo.Read<Project>()
                    .Include(i => i.Tasks)
                        .ThenInclude(i => i.ProjectTaskUsers)
                    .Where(w => w.OwnerGuid == _principal.GetOwnerId())
                    .Where(w => w.UserProjects.Any(up => up.UserId == _principal.GetUserId())
                                || w.Tasks.Any(t =>
                                    t.ProjectTaskUsers.Any(tu => tu.UserId == _principal.GetUserId())))
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();
            }

            return projectId;
        }

        private async Task AssignManagers(IPrincipal user, int projectId, List<ProjectUserDto> managers)
        {
            managers = managers ?? new List<ProjectUserDto>();

            if (user.IsInRole(UserRoles.Owner))
            {
                await AssignToProject(new AssignUsersToProjectDto
                {
                    Ids = managers.Select(s => s.UserId),
                    ProjectId = projectId
                });
            }
        }

        public async Task CopyFormulaResourcesToProject(Project project, FormulaProject formula)
        {
            foreach (var resource in formula.ResourceFormula)
            {
                var newResource = Mapper.Map<Domain.Models.Resource.Resource>(resource.Resource);
                project.ResourceProject.Add(
                    new Domain.Models.Resource.ResourceProject
                    {
                        Resource = newResource
                    });
                if (resource.Resource.Type == ResourceType.File)
                {
                    var path = project.Id + "/" + resource.Resource.Name;
                    newResource.Path = path;
                    await _storageService.CopyFileAsync(resource.Resource.Path, path, StorageType.Formula, StorageType.Project);
                }
            }
        }

        public void BulkImportProject(string path)
        {
            //string path = @"D:\ImportExcel\projectImport.xlsx";
         string url =   Path.Combine(_hostingEnvironment.WebRootPath, "BulkExcelProjectImport");
            var userId = _principal.GetUserId();
            var excel = ConvertExcelToDataTable(url+"/"+path);
            try
            {
                _repo.ExecuteSqlCommand(
                                "usp_ImportProjects @PROJECTDETAILS, @USERGUID",
                    new List<SqlParameter>
                        {
                    new SqlParameter { TypeName = "dbo.ProjectDetailType", ParameterName = "@PROJECTDETAILS", Value = excel },
                    new SqlParameter { ParameterName = "@USERGUID", SqlDbType = SqlDbType.UniqueIdentifier, Value = userId }
                        });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static DataTable ConvertExcelToDataTable(string FileName)
        {
            DataTable dtResult = null;
            try
            {
                int totalSheet = 0; //No of sheets on excel file  
                using (OleDbConnection objConn = new OleDbConnection(
                    @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties='Excel 12.0;';"))
                {
                    objConn.Open();
                    OleDbCommand cmd = new OleDbCommand();
                    OleDbDataAdapter oleda = new OleDbDataAdapter();
                    DataSet ds = new DataSet();
                    DataTable dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    string sheetName = string.Empty;
                    if (dt != null)
                    {
                        var tempDataTable = (from dataRow in dt.AsEnumerable()
                                             where !dataRow["TABLE_NAME"].ToString().Contains("FilterDatabase")
                                             select dataRow).CopyToDataTable();
                        dt = tempDataTable;
                        totalSheet = dt.Rows.Count;
                        sheetName = dt.Rows[0]["TABLE_NAME"].ToString();
                    }
                    cmd.Connection = objConn;
                    cmd.CommandType = CommandType.Text;
                    //cmd.CommandText = "SELECT * FROM [" + sheetName + "]";
                    cmd.CommandText = "SELECT ProjectLevel1, ProjectLevel1DEscription, ProjectLevel2, ProjectLevel2DEscription, ProjectLevel3, ProjectLevel3DEscription FROM [" + sheetName + "]";
                    oleda = new OleDbDataAdapter(cmd);
                    oleda.Fill(ds, "excelData");
                    dtResult = ds.Tables["excelData"];
                    objConn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ValidationException(ex.Message);
            }
            return dtResult; //Returning Dattable
        }
    }
}
