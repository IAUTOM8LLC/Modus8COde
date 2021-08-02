using AutoMapper;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Formula;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Formula.Interfaces;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Resources.Dto;
using IAutoM8.Tests.Common;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IAutoM8.Service.ProjectTasks.Dto;
using Xunit;

namespace IAutoM8.Service.Tests.CommonService
{
    public class ProjectTaskEntityImportServiceTests
    {
        private Mock<IDateTimeService> _dateTimeServiceMock;
        private Mock<IStorageService> _storageServiceMock;
        private Mock<ITaskNeo4jRepository> _taskNeo4JRepositoryMock;
        private Mock<IFormulaTaskNeo4jRepository> _formulaTaskNeo4JRepositoryMock;
        private Mock<IMapper> _mapperMock;
        private Mock<ClaimsPrincipal> _principalMock;
        private Mock<ITransactionScope> _transactionScopeMock;
        private Mock<IRepo> _repoMock;
        private Mock<Func<ITransactionScope, Project, IEnumerable<FormulaTask>, DateTime,
                IEnumerable<int>, IEnumerable<SkillMapDto>, int?, (int x, int y), Task<Dictionary<int, int>>>> _funcMock;
        private readonly IProjectTaskEntityImportService _service;

        public ProjectTaskEntityImportServiceTests()
        {
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _storageServiceMock = new Mock<IStorageService>();
            _taskNeo4JRepositoryMock = new Mock<ITaskNeo4jRepository>();
            _repoMock = new Mock<IRepo>();
            _transactionScopeMock = new Mock<ITransactionScope>();
            SetupMapperMock();
            SetupFormulaTaskNeo4JRepositoryMock();
            SetupPrincipalMock();
            _funcMock = new Mock<Func<ITransactionScope, Project, IEnumerable<FormulaTask>,
                DateTime, IEnumerable<int>, IEnumerable<SkillMapDto>, int?, (int x, int y), Task<Dictionary<int, int>>>>();
            _funcMock.Setup(s => s(It.IsAny<ITransactionScope>(),
                It.IsAny<Project>(), It.IsAny<IEnumerable<FormulaTask>>(),
                It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<SkillMapDto>>(),
                It.IsAny<int?>(), It.IsAny<(int x, int y)>()))
                .Returns(Task.FromResult(new Dictionary<int, int> { [1001] = 1002 }));
            _service = new ProjectTaskEntityImportService(
                 _dateTimeServiceMock.Object,
                 _storageServiceMock.Object,
                 _taskNeo4JRepositoryMock.Object,
                 _formulaTaskNeo4JRepositoryMock.Object,
                 _mapperMock.Object,
                 _principalMock.Object);
        }

        private void SetupFormulaTaskNeo4JRepositoryMock()
        {
            _formulaTaskNeo4JRepositoryMock = new Mock<IFormulaTaskNeo4jRepository>();
            _formulaTaskNeo4JRepositoryMock.Setup(s => s.GetTaskResourcesAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new List<TaskResourceNeo4jDto>()));
        }

        private void SetupPrincipalMock()
        {
            _principalMock = new Mock<ClaimsPrincipal>();
            _principalMock.Setup(s => s.Claims)
                .Returns(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                });
        }

        private void SetupMapperMock()
        {
            _mapperMock = new Mock<IMapper>();
            _mapperMock.Setup(s => s.Map<ProjectTask>(It.IsAny<object>(), It.IsAny<Action<IMappingOperationOptions>>()))
                .Returns((FormulaTask formula, Action<IMappingOperationOptions> opts) =>
                {
                    var paramMock = new Mock<IMappingOperationOptions>();
                    var items = new Dictionary<string, object>();
                    paramMock.Setup(s => s.Items).Returns(items);
                    opts.Invoke(paramMock.Object);
                    var postOffset = (ValueTuple<int, int>)items["PositionOffset"];
                    return new ProjectTask
                    {
                        Title = $"{formula.Title}-map",
                        OwnerGuid = (Guid)items["OwnerGuid"],
                        ParentTaskId = (int?)items["ParentTaskId"],
                        DateCreated = (DateTime)items["NowUtc"],
                        PosX = postOffset.Item1 + formula.PosX,
                        PosY = postOffset.Item2 + formula.PosY
                    };
                });
            _mapperMock.Setup(s => s.Map<TaskResourceNeo4jDto>(It.IsAny<object>(), It.IsAny<Action<IMappingOperationOptions>>()))
                .Returns((TaskResourceNeo4jDto res, Action<IMappingOperationOptions> opts) => {
                    var paramMock = new Mock<IMappingOperationOptions>();
                    var items = new Dictionary<string, object>();
                    paramMock.Setup(s => s.Items).Returns(items);
                    opts.Invoke(paramMock.Object);
                    return new TaskResourceNeo4jDto
                    {
                        Path = (string)items["path"]
                    };
                });
        }

        private List<ProjectTask> GetTransactionStorageForAdding()
        {
            var storage = new List<ProjectTask>();
            _transactionScopeMock.Setup(s => s.AddAsync(It.IsAny<ProjectTask>()))
                .Callback((ProjectTask task) => { task.Id = 1; storage.Add(task); })
                .Returns(Task.CompletedTask);
            return storage;
        }
        #region MapFormulaTaskAsync
        [Fact]
        public async Task MapFormulaTaskAsync_WithoutParentId_ReturnsCorrectData()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            _repoMock.SetupSingleValueRepoMock(new FormulaTask
                {
                    Id = 11,
                    FormulaProjectId = 21
                }, _transactionScopeMock);
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                InternalFormulaProjectId = 21,
                InternalFormulaProject = new FormulaProject { Name = "name" },
                PosX = 100,
                PosY = 101
            };

            var result = await _service.MapFormulaTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, _funcMock.Object);

            AssertEx.EqualSerialization(new Dictionary<int, int>{ [1] = 2, [1001] = 1002 }, result);
        }

        [Fact]
        public async Task MapFormulaTaskAsync_WithoutParentId_SaveCorrectData()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            _repoMock.SetupSingleValueRepoMock(new FormulaTask
            {
                Id = 11,
                FormulaProjectId = 21
            }, _transactionScopeMock);
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                InternalFormulaProjectId = 21,
                InternalFormulaProject = new FormulaProject { Name = "name" },
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            var result = await _service.MapFormulaTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, _funcMock.Object);

            AssertEx.EqualSerialization(
                new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = 1,
                        Title = "name",
                        FormulaId = 21,
                        PosX = 100,
                        PosY = 101,
                        ProjectId = 3,
                        OwnerGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                        FormulaTaskId = 2
                    }
                }, projectTaskStorage);
        }

        [Fact]
        public async Task MapFormulaTaskAsync_WithoutParentId_SaveChangesAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            _repoMock.SetupSingleValueRepoMock(new FormulaTask
            {
                Id = 11,
                FormulaProjectId = 21
            }, _transactionScopeMock);
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                InternalFormulaProjectId = 21,
                InternalFormulaProject = new FormulaProject { Name = "name" },
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            var result = await _service.MapFormulaTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, _funcMock.Object);

            _transactionScopeMock.Verify(v => v.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task MapFormulaTaskAsync_WithoutParentId_AddTaskAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            _repoMock.SetupSingleValueRepoMock(new FormulaTask
            {
                Id = 11,
                FormulaProjectId = 21
            }, _transactionScopeMock);
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                InternalFormulaProjectId = 21,
                InternalFormulaProject = new FormulaProject { Name = "name" },
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            var result = await _service.MapFormulaTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, _funcMock.Object);

            _taskNeo4JRepositoryMock.Verify(v => v.AddTaskAsync(1,3), Times.Once);
        }

        [Fact]
        public async Task MapFormulaTaskAsync_WithoutParentId_FuncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            _repoMock.SetupSingleValueRepoMock(new FormulaTask
            {
                Id = 11,
                FormulaProjectId = 21
            }, _transactionScopeMock);
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                InternalFormulaProjectId = 21,
                InternalFormulaProject = new FormulaProject { Name = "name" },
                PosX = 100,
                PosY = 101
            };
            var outsources = new List<int>();
            var skillMaps = new List<SkillMapDto>();
            var startDate = new DateTime(2010, 10, 5);
            var result = await _service.MapFormulaTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, startDate, outsources, new List<SkillMapDto>(), null, _funcMock.Object);

            _funcMock.Verify(v => v(_transactionScopeMock.Object, project,
                It.Is<IEnumerable<FormulaTask>>(l => l.Count() == 1 && l.ElementAt(0).Id == 11),
                startDate, outsources, skillMaps, 1, default((int, int))), Times.Once);
        }



        [Fact]
        public async Task MapFormulaTaskAsync_WithParentId_ReturnsCorrectData()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            _repoMock.SetupSingleValueRepoMock(new FormulaTask
            {
                Id = 11,
                FormulaProjectId = 21
            }, _transactionScopeMock);
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                InternalFormulaProjectId = 21,
                InternalFormulaProject = new FormulaProject { Name = "name" },
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            var result = await _service.MapFormulaTaskAsync(_transactionScopeMock.Object,
                project, formulaTask,new DateTime(), new List<int>(), new List<SkillMapDto>(), 5, _funcMock.Object);

            AssertEx.EqualSerialization(new Dictionary<int, int> { [1] = 2, [1001] = 1002 }, result);
        }

        [Fact]
        public async Task MapFormulaTaskAsync_WithParentId_SaveCorrectData()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            _repoMock.SetupSingleValueRepoMock(new FormulaTask
            {
                Id = 11,
                FormulaProjectId = 21
            }, _transactionScopeMock);
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                InternalFormulaProjectId = 21,
                InternalFormulaProject = new FormulaProject { Name = "name" },
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            var result = await _service.MapFormulaTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, new DateTime(), new List<int>(), new List<SkillMapDto>(), 5, _funcMock.Object);

            AssertEx.EqualSerialization(
                new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = 1,
                        Title = "name",
                        FormulaId = 21,
                        PosX = 100,
                        PosY = 101,
                        ProjectId = 3,
                        ParentTaskId = 5,
                        OwnerGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                        FormulaTaskId = 2
                    }
                }, projectTaskStorage);
        }

        [Fact]
        public async Task MapFormulaTaskAsync_WithParentId_SaveChangesAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            _repoMock.SetupSingleValueRepoMock(new FormulaTask
            {
                Id = 11,
                FormulaProjectId = 21
            }, _transactionScopeMock);
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                InternalFormulaProjectId = 21,
                InternalFormulaProject = new FormulaProject { Name = "name" },
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            var result = await _service.MapFormulaTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, new DateTime(), new List<int>(), new List<SkillMapDto>(), 5, _funcMock.Object);

            _transactionScopeMock.Verify(v => v.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task MapFormulaTaskAsync_WithParentId_AddFormulaTaskAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            _repoMock.SetupSingleValueRepoMock(new FormulaTask
            {
                Id = 11,
                FormulaProjectId = 21
            }, _transactionScopeMock);
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                InternalFormulaProjectId = 21,
                InternalFormulaProject = new FormulaProject { Name = "name" },
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            var result = await _service.MapFormulaTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, new DateTime(), new List<int>(), new List<SkillMapDto>(), 5, _funcMock.Object);

            _taskNeo4JRepositoryMock.Verify(v => v.AddFormulaTaskAsync(1, 3, 5), Times.Once);
        }

        [Fact]
        public async Task MapFormulaTaskAsync_WithParentId_FuncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            _repoMock.SetupSingleValueRepoMock(new FormulaTask
            {
                Id = 11,
                FormulaProjectId = 21
            }, _transactionScopeMock);
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                InternalFormulaProjectId = 21,
                InternalFormulaProject = new FormulaProject { Name = "name" },
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();
            var startTime = new DateTime(2010, 1, 2);
            var outsources= new List<int>();
            var skillMaps = new List<SkillMapDto>();
            var result = await _service.MapFormulaTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, startTime, outsources, skillMaps, null, _funcMock.Object);

            _funcMock.Verify(v => v(_transactionScopeMock.Object, project,
                It.Is<IEnumerable<FormulaTask>>(l => l.Count() == 1 && l.ElementAt(0).Id == 11),
                startTime, outsources, skillMaps, 1, default((int, int))), Times.Once);
        }
        #endregion
        #region MapTaskAsync
        [Fact]
        public async Task MapTaskAsync_WithoutParentId_ReturnsCorrectData()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };
            var posOffset = (20, 30);

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, null, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            AssertEx.EqualSerialization((2, 0), result);
        }

        [Fact]
        public async Task MapTaskAsync_WithoutParentId_SaveCorrectData()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };

            var posOffset = (20, 30);

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, null, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            AssertEx.EqualSerialization(
                new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Title = "title-map",
                        PosX = 120,
                        PosY = 131,
                        OwnerGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                    }
                }, project.Tasks);
        }

        [Fact]
        public async Task MapTaskAsync_WithoutParentId_SaveChangesAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();
            var posOffset = (20, 30);

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, null, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            _transactionScopeMock.Verify(v => v.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task MapTaskAsync_WithoutParentId_AddTaskAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();
            var posOffset = (20, 30);

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, null, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            _taskNeo4JRepositoryMock.Verify(v => v.AddTaskAsync(0, 3), Times.Once);
        }

        [Fact]
        public async Task MapTaskAsync_WithoutParentId_LinkResource_AddResourceToTaskAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };

            var posOffset = (20, 30);

            _formulaTaskNeo4JRepositoryMock.Setup(s => s.GetTaskResourcesAsync(2))
                .Returns(Task.FromResult(new List<TaskResourceNeo4jDto>
                {
                    new TaskResourceNeo4jDto
                    {
                        Name = "name",
                        Type = (byte)ResourceType.Link
                    }
                }));

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, null, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            _taskNeo4JRepositoryMock.Verify(v => v.AddResourceToTaskAsync(0,
                It.Is<TaskResourceNeo4jDto>(t => t.Path == "name")), Times.Once);
            _storageServiceMock.Verify(v => v.CopyFileAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<StorageType>(), It.IsAny<StorageType>()), Times.Never);
        }

        [Fact]
        public async Task MapTaskAsync_WithoutParentId_FileResource_AddResourceToTaskAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };

            var posOffset = (20, 30);

            _formulaTaskNeo4JRepositoryMock.Setup(s => s.GetTaskResourcesAsync(2))
                .Returns(Task.FromResult(new List<TaskResourceNeo4jDto>
                {
                    new TaskResourceNeo4jDto
                    {
                        Path = "path",
                        Name = "name",
                        Type = (byte)ResourceType.File
                    }
                }));

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, null, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            _taskNeo4JRepositoryMock.Verify(v => v.AddResourceToTaskAsync(0,
                It.Is<TaskResourceNeo4jDto>(t => t.Path == "0/name")), Times.Once);
            _storageServiceMock.Verify(v => v.CopyFileAsync("path", "0/name", StorageType.FormulaTask, StorageType.ProjectTask), Times.Once);
        }


        [Fact]
        public async Task MapTaskAsync_WithParentId_ReturnsCorrectData()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };

            var posOffset = (20, 30);

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, 5, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            AssertEx.EqualSerialization((2, 0), result);
        }

        [Fact]
        public async Task MapTaskAsync_WithParentId_SaveCorrectData()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };

            var posOffset = (20, 30);

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, 5, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            AssertEx.EqualSerialization(
                new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Title = "title-map",
                        PosX = 120,
                        PosY = 131,
                        OwnerGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                        ParentTaskId = 5
                    }
                }, project.Tasks);
        }

        [Fact]
        public async Task MapTaskAsync_WithParentId_SaveChangesAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };

            var posOffset = (20, 30);

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, 5, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            _transactionScopeMock.Verify(v => v.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task MapTaskAsync_WithParentId_AddFormulaTaskAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };

            var posOffset = (20, 30);

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, 5, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            _taskNeo4JRepositoryMock.Verify(v => v.AddFormulaTaskAsync(0, 3, 5), Times.Once);
        }

        [Fact]
        public async Task MapTaskAsync_WithParentId_LinkResource_AddResourceToTaskAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();
            var posOffset = (20, 30);

            _formulaTaskNeo4JRepositoryMock.Setup(s => s.GetTaskResourcesAsync(2))
                .Returns(Task.FromResult(new List<TaskResourceNeo4jDto>
                {
                    new TaskResourceNeo4jDto
                    {
                        Name = "name",
                        Type = (byte)ResourceType.Link
                    }
                }));

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, 5, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            _taskNeo4JRepositoryMock.Verify(v => v.AddResourceToTaskAsync(0,
                It.Is<TaskResourceNeo4jDto>(t => t.Path == "name")), Times.Once);
            _storageServiceMock.Verify(v => v.CopyFileAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<StorageType>(), It.IsAny<StorageType>()), Times.Never);
        }

        [Fact]
        public async Task MapTaskAsync_WithParentId_FileResource_AddResourceToTaskAsyncCorrectCalling()
        {
            var projectTaskStorage = GetTransactionStorageForAdding();
            var project = new Project { Id = 3 };
            var formulaTask = new FormulaTask
            {
                Id = 2,
                Title = "title",
                PosX = 100,
                PosY = 101
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();
            var posOffset = (20, 30);

            _formulaTaskNeo4JRepositoryMock.Setup(s => s.GetTaskResourcesAsync(2))
                .Returns(Task.FromResult(new List<TaskResourceNeo4jDto>
                {
                    new TaskResourceNeo4jDto
                    {
                        Path = "path",
                        Name = "name",
                        Type = (byte)ResourceType.File
                    }
                }));

            var result = await _service.MapTaskAsync(_transactionScopeMock.Object,
                project, formulaTask, 5, posOffset, new DateTime(), new List<int>(), new List<SkillMapDto>());

            _taskNeo4JRepositoryMock.Verify(v => v.AddResourceToTaskAsync(0,
                It.Is<TaskResourceNeo4jDto>(t => t.Path == "0/name")), Times.Once);
            _storageServiceMock.Verify(v => v.CopyFileAsync("path", "0/name", StorageType.FormulaTask, StorageType.ProjectTask), Times.Once);
        }
        #endregion
    }
}
