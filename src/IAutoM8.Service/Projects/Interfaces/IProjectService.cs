using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Repository;
using IAutoM8.Service.Projects.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAutoM8.Service.Projects.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectDto>> GetOwnProjects();
        Task<ProjectDto> GetProject(int projectId);
        Task<List<ProjectDto>> GetChildProjets(int parentProjectId);
        Task<ProjectDto> AddProject(AddProjectDto project);
        Task<ProjectDto> AddChildProject(AddChildProjectDto project);
        Task DeleteProject(int projectId);
        Task DeleteChildProject(int projectId);
        Task<ProjectDto> UpdateProject(ProjectDto project);
        Task<ProjectDto> UpdateChildProject(AddChildProjectDto project);
        Task<Project> CreateProjectFromFormula(ITransactionScope trx, FormulaProject formula, DateTime startDate, string projectName, int? parentProjectId);
        Task CopyFormulaResourcesToProject(Project project, FormulaProject formula);
        Task AssignToProject(AssignUsersToProjectDto model);
        Task<IEnumerable<AssignedUserDto>> GetAssignedUsers(int? projectId);
        Task<int> GetMostRecentId();
        Task<IEnumerable<int>> GetOwnProjectsIds(Guid? userId = null);
        void BulkImportProject(string path);
    }
}
