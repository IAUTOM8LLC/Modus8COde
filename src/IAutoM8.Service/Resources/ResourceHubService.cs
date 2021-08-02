using AutoMapper;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Resources.Dto;
using IAutoM8.Service.Resources.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.Service.Resources
{
    public class ResourceHubService : IResourceHubService
    {
        private readonly IRepo _repo;
        private readonly ClaimsPrincipal _principal;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;
        public ResourceHubService(IRepo repo,
            ClaimsPrincipal principal,
            ITaskNeo4jRepository taskNeo4JRepository,
            IMapper mapper,
            IStorageService storageService)
        {
            _repo = repo;
            _principal = principal;
            _taskNeo4JRepository = taskNeo4JRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<IEnumerable<ResourceInfoDto>> GetProjectResorcesAsync(int projectId)
        {
            var userId = _principal.GetUserId();
            IEnumerable<TaskResourceInfoNeo4jDto> taskResources = null;
            if (_principal.IsWorker())
            {
                var taskIds = await _repo.Read<ProjectTask>()
                   .Where(c => c.ProjectId == projectId).Join(_repo.Read<ProjectTaskUser>()
                        .Where(w => w.UserId == userId && w.ProjectTaskUserType == ProjectTaskUserType.Assigned)
                        .Select(s => s.ProjectTaskId),
                        inner => inner.Id, outer => outer,
                        (task, userProject) => task)
                        .Select(s => s.Id).ToListAsync();
                taskResources = await _taskNeo4JRepository.GetProjectResourcesAsync(projectId, taskIds);
            }
            else if (_principal.IsVendor())
            {

                var taskIds = await _repo.Read<ProjectTaskVendor>()
                        .Include(i => i.ProjectTask)
                        .Where(w => w.VendorGuid == userId && w.ProjectTask.ProjectId == projectId &&
                        ((w.ProjectTask.Status == TaskStatusType.New && w.Status == ProjectRequestStatus.None) ||
                        (w.ProjectTask.Status != TaskStatusType.New && w.Status == ProjectRequestStatus.Accepted)))
                        .GroupBy(g => g.ProjectTaskId)
                        .Select(s => s.Key).ToListAsync();
                taskResources = await _taskNeo4JRepository.GetProjectResourcesAsync(projectId, taskIds);
            }
            else
            {
                taskResources = await _taskNeo4JRepository.GetProjectResourcesAsync(projectId);
            }

            var projectResources = await _repo.Read<ResourceProject>()
                .Include(c => c.Resource)
                .Where(c => c.ProjectId == projectId)
                .Select(s => s.Resource)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ResourceInfoDto>>(taskResources,
               opts =>
               {
                   opts.Items.Add("urlBuilder",
                       (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.ProjectTask)));
               })
               .Concat(_mapper.Map<IEnumerable<ResourceInfoDto>>(projectResources,
                    opts =>
                    {
                        opts.Items.Add("urlBuilder",
                            (Func<string, string>)((path) => _storageService.GetFileUri(path, StorageType.Project)));
                    }));
        }
    }
}
