using AutoMapper;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Domain.Models.User;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Resources.Dto;
using IAutoM8.Service.Resources.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace IAutoM8.Service.Resources
{
    public class ResourceService : IResourceService
    {
        private readonly IStorageService _storageService;
        private readonly IRepo _repo;
        private readonly ClaimsPrincipal _principal;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;
        private readonly IFormulaTaskNeo4jRepository _formulaTaskNeo4JRepository;
        private readonly IResourceNeo4jRepository _resourceNeo4JRepository;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ResourceService(IRepo repo, IStorageService storageService, ClaimsPrincipal principal,
            ITaskNeo4jRepository taskNeo4JRepository,
            IFormulaTaskNeo4jRepository formulaTaskNeo4JRepository,
            IResourceNeo4jRepository resourceNeo4JRepository,
            IMapper mapper,
            IHostingEnvironment hostingEnvironment)
        {
            _repo = repo;
            _storageService = storageService;
            _principal = principal;
            _taskNeo4JRepository = taskNeo4JRepository;
            _formulaTaskNeo4JRepository = formulaTaskNeo4JRepository;
            _resourceNeo4JRepository = resourceNeo4JRepository;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task UpdateFormulaResources(UpdateResourceDto resource)
        {
            var resources = await _repo.Track<ResourceFormula>()
                .Include(i => i.Resource)
                .Where(w => w.FormulaId == resource.Id)
                .ToListAsync();

            await MakeChanges(resource, resources,
                (id, name, mime, size, type) => new ResourceFormula
                {
                    FormulaId = id,
                    Resource = new Resource
                    {
                        Name = name,
                        Type = type,
                        Mime = mime,
                        Size = size,
                        Path = GetPath(type, id, name)
                    }
                }, StorageType.Formula);
        }

        public async Task UpdateFormulaTaskResources(UpdateTaskResourceDto resource)
        {
            await UpdateResourceForTask(_formulaTaskNeo4JRepository.GetTaskResourcesAsync,
                _formulaTaskNeo4JRepository.BeginTransaction, _formulaTaskNeo4JRepository.AddResourceToTaskAsync,
                resource, StorageType.FormulaTask);
        }

        public async Task UpdateProjectResources(UpdateResourceDto resource)
        {
            var resources = await _repo.Track<ResourceProject>()
                .Include(i => i.Resource)
                .Where(w => w.ProjectId == resource.Id)
                .ToListAsync();

            await MakeChanges(resource, resources,
                (id, name, mime, size, type) => new ResourceProject
                {
                    ProjectId = id,
                    Resource = new Resource
                    {
                        Name = name,
                        Type = type,
                        Mime = mime,
                        Size = size,
                        Path = GetPath(type, id, name)
                    }
                }, StorageType.Project);
        }

        public async Task UpdateProjectTaskResources(UpdateTaskResourceDto resource)
        {
            await UpdateResourceForTask(_taskNeo4JRepository.GetTaskResourcesAsync,
                _taskNeo4JRepository.BeginTransaction, _taskNeo4JRepository.AddResourceToTaskAsync,
                resource, StorageType.ProjectTask);
        }

        public async Task<string> UploadTempFile(IFormFile file)
        {
            //UploadTempExcelFile

            string fileUrl;
            using (var content = file.OpenReadStream())
            {
                fileUrl = await _storageService.UploadFileAsync(content, $"{_principal.GetUserId()}/{file.FileName}", StorageType.Temp);
            }

            return JsonConvert.SerializeObject(new
            {
                success = true,
                fileInfo = new
                {
                    url = fileUrl,
                    timestamp = DateTime.UtcNow
                }
            });
        }

        public async Task<string> UploadTempExcelFile(IFormFile file)
        {
            string fileName;
            string fileUrl;
            string fileDate;
            using (var content = file.OpenReadStream())
            {

                string uploads = Path.Combine(_hostingEnvironment.WebRootPath, "BulkExcelProjectImport");
                string extension = Path.GetExtension(file.FileName);
                if (file.Length > 0)
                {

                    //fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    //extension = Path.GetExtension(file.FileName);
                    //fileDate = fileName + extension;

                    fileUrl = Path.Combine(uploads,file.FileName);
                    using (var fileStream = new FileStream(fileUrl, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                }

                string[] filePaths = Directory.GetFiles(Path.Combine(_hostingEnvironment.WebRootPath, "BulkExcelProjectImport/"));
                fileUrl = filePaths[0];
                //fileUrl = await _storageService.UploadFileAsync(content, $"{_principal.GetUserId()}/{file.FileName}", StorageType.Temp);

                return fileUrl;
            }
        }

        public async Task<string> GetProfileImage(Guid userId)
        {
            var userProfile = await _repo.Read<UserProfile>()
                    .Where(w => w.UserId == userId)
                    .FirstOrDefaultAsync();

            if (userProfile == null)
            {
                throw new ValidationException("User profile doesn't exit.");
            }

            if (String.IsNullOrWhiteSpace(userProfile.Path))
            {
                throw new ValidationException("Profile image doesn't exist");
            }

            return _storageService.GetFileUri($"{userId}/{userProfile.Path}", StorageType.ProfileImage);
        }

        public async Task<string> UploadProfileImage(IFormFile file)
        {
            string fileUrl;
            var userId = _principal.GetUserId();

            var userProfile = await _repo.Track<UserProfile>()
                    .Where(w => w.UserId == userId)
                    .FirstOrDefaultAsync();

            if (userProfile != null)
            {
                using (var content = file.OpenReadStream())
                {
                    fileUrl = await _storageService.UploadFileAsync(content, $"{_principal.GetUserId()}/{file.FileName}", StorageType.ProfileImage);
                }

                userProfile.Path = file.FileName;

                await _repo.SaveChangesAsync();

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    fileInfo = new
                    {
                        url = fileUrl
                    }
                });
            }

            return JsonConvert.SerializeObject(new
            {
                success = false,
                fileInfo = new
                {
                    url = String.Empty
                }
            });
        }
        public async Task<string> UploadDescriptionFile(IFormFile file)
        {
            using (var content = file.OpenReadStream())
            {
                return await _storageService.UploadFileAsync(content, file.FileName, StorageType.Description);
            }
        }

        private async Task MakeChanges<T>(UpdateResourceDto resource, List<T> resources,
            Func<int, string, string, int, ResourceType, T> newEntity, StorageType storage) where T : ResourceBase
        {
            var userId = _principal.GetUserId();
            foreach (var toDelete in resource.ToDeleteList)
            {
                var entity = resources.First(w => w.ResourceId == toDelete);

                if (entity.Resource.Type == ResourceType.File)
                {
                    var canDelete = _repo.Read<Resource>(
                                    t => t.ResourceFormula,
                                    t => t.ResourceFormulaTask,
                                    t => t.ResourceProject,
                                    t => t.ResourceProjectTask)
                                .Count(w =>
                                    w.Id == toDelete
                                    && (
                                        w.ResourceProjectTask.Any()
                                        || w.ResourceProject.Any()
                                        || w.ResourceFormulaTask.Any()
                                        || w.ResourceFormula.Any()
                                    )
                                ) <= 1;

                    if (canDelete)
                    {
                        await _storageService.DeleteFileAsync(entity.Resource.Path, storage);
                    }
                }

                _repo.Remove(entity);
            }

            foreach (var url in resource.ToAddUrlList)
            {
                await _repo.AddAsync(newEntity(resource.Id, url, null, 0, ResourceType.Link));
            }

            foreach (var file in resource.ToAddFileList)
            {
                await _storageService.CopyFileAsync($"{userId}/{file.Name}", $"{resource.Id}/{file.Name}", StorageType.Temp, storage);
                await _repo.AddAsync(newEntity(resource.Id, file.Name, file.Mime, file.Size, ResourceType.File));
            }

            await _repo.SaveChangesAsync();
            await _storageService.DeleteFolderAsync(userId.ToString(), StorageType.Temp);
        }

        private static string GetPath(ResourceType type, int id, string name)
        {
            return type == ResourceType.File
                ? $"{id}/{name}"
                : name;
        }

        private async Task UpdateResourceForTask(Func<int, Task<List<TaskResourceNeo4jDto>>> recieveResources,
            Func<ITransaction> beginTransaction, Func<int, TaskResourceNeo4jDto, Task> addResource,
            UpdateTaskResourceDto resource, StorageType storage)
        {
            var resources = await recieveResources(resource.Id);
            var userId = _principal.GetUserId();
            using (var transaction = beginTransaction())
            {
                foreach (var toDelete in resource.ToDeleteList)
                {
                    var entity = resources.First(w => w.Id == toDelete);
                    await _storageService.DeleteFileAsync($"{resource.Id}/{entity.Name}", storage);
                    await _resourceNeo4JRepository.DeleteResourceAsync(toDelete);
                }

                foreach (var dto in _mapper.Map<List<TaskResourceNeo4jDto>>(resource.ToAddUrlList))
                {
                    await addResource(resource.Id, dto);
                }

                var taskResourceNeo4s = _mapper.Map<List<TaskResourceNeo4jDto>>(resource.ToAddFileList,
                   opts: opts =>
                   {
                       opts.Items.Add("pathBuilder", (Func<string, string>)(name => $"{resource.Id}/{name}"));
                   });


                foreach (var dto in taskResourceNeo4s)
                {
                    await _storageService.CopyFileAsync($"{userId}/{dto.Name}",
                        $"{resource.Id}/{dto.Name}", StorageType.Temp, storage);
                    await addResource(resource.Id, dto);
                }


                foreach (var dto in _mapper.Map<List<UpdateTaskResourceNeo4jDto>>(resource.UpdateResourceList))
                {
                    await _resourceNeo4JRepository.UpdateTaskResourceAsync(dto);
                }

                await _storageService.DeleteFolderAsync(userId.ToString(), StorageType.Temp);
                transaction.Commit();
            }
        }
    }
}
