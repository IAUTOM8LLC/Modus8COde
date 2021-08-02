using AutoMapper;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Global.Enums;
using IAutoM8.Global.Options;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Formula;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.Formula.Interfaces;
using IAutoM8.Service.FormulaTasks.Interfaces;
using IAutoM8.Service.Projects.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Resources.Dto;
using IAutoM8.Tests.Common;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IAutoM8.Service.Tests.Formula
{
    public class FormulaServiceTests
    {
        private Mock<IRepo> _repoMock;
        private Mock<ITaskImportService> _taskImportServiceMock;
        private Mock<ClaimsPrincipal> _principalMock;
        private Mock<ITaskStartDateHelperService> _startDateHelperServiceMock;
        private Mock<IStorageService> _storageServiceMock;
        private Mock<ITaskNeo4jRepository> _taskNeo4JRepositoryMock;
        private Mock<IFormulaNeo4jRepository> _formulaNeo4JRepositoryMock;
        private Mock<IDateTimeService> _dateTimeServiceMock;
        private Mock<IMapper> _mapperMock;
        private Mock<ITransactionScope> _transactionScopeMock;
        private Mock<ITransaction> _transactionMock;
        private Mock<IFormulaTaskService> _formulaTaskService;
        private Mock<IEntityFrameworkPlus> _entityFrameworkPlusMock;
        private Mock<IProjectService> _projectServiceMock;
        private Mock<IProjectTaskOutsourcesService> _projectTaskOutsourcesServiceMock;
        private Mock<IFormulaTaskNeo4jRepository> _formulaTaskNeo4jRepository;
        private Mock<IOptions<PredefinedFormulaSeller>> _sellerOptionMock;
        private readonly IFormulaService _formulaService;

        public FormulaServiceTests()
        {
            _transactionMock = new Mock<ITransaction>();
            _taskImportServiceMock = new Mock<ITaskImportService>();
            _repoMock = new Mock<IRepo>();
            _transactionScopeMock = new Mock<ITransactionScope>();
            _startDateHelperServiceMock = new Mock<ITaskStartDateHelperService>();
            _storageServiceMock = new Mock<IStorageService>();
            _taskNeo4JRepositoryMock = new Mock<ITaskNeo4jRepository>();
            _formulaNeo4JRepositoryMock = new Mock<IFormulaNeo4jRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _formulaTaskService = new Mock<IFormulaTaskService>();
            _entityFrameworkPlusMock = new Mock<IEntityFrameworkPlus>();
            _projectServiceMock = new Mock<IProjectService>();
            _sellerOptionMock = new Mock<IOptions<PredefinedFormulaSeller>>();
            _projectTaskOutsourcesServiceMock = new Mock<IProjectTaskOutsourcesService>();
            _formulaTaskNeo4jRepository = new Mock<IFormulaTaskNeo4jRepository>();
            SetupMapperMock();
            SetupPrincipalMock();
            _formulaService = new FormulaService(
                _taskImportServiceMock.Object,
                _repoMock.Object,
                _principalMock.Object,
                _startDateHelperServiceMock.Object,
                _storageServiceMock.Object,
                _taskNeo4JRepositoryMock.Object,
                _formulaNeo4JRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _formulaTaskService.Object,
                _entityFrameworkPlusMock.Object,
                _mapperMock.Object,
                _projectServiceMock.Object,
                _sellerOptionMock.Object,
                _projectTaskOutsourcesServiceMock.Object,
                _formulaTaskNeo4jRepository.Object);
        }

        private void SetupPrincipalMock()
        {
            _principalMock = new Mock<ClaimsPrincipal>();
            _principalMock.Setup(s => s.Claims)
                .Returns(new List<Claim>
                {
                    new Claim(ClaimTypes.PrimarySid, "22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                    new Claim(ClaimTypes.NameIdentifier, "22345200-abe8-4f60-90c8-0d43c5f6c0f5")
                });
        }

        private void SetupMapperMock()
        {
            _mapperMock = new Mock<IMapper>();
            _mapperMock.Setup(s => s.Map<FormulaDto>(It.IsAny<object>()))
                .Returns((FormulaProject formula) => new FormulaDto
                {
                    Name = $"{formula.Name}-map",
                    Resources = _mapperMock.Object.Map<List<ResourceDto>>(formula.ResourceFormula)
                });

            _mapperMock.Setup(s => s.Map<FormulaDto>(It.IsAny<object>(), It.IsAny<Action<IMappingOperationOptions>>()))
                .Returns((FormulaProject formula, Action<IMappingOperationOptions> action) => new FormulaDto
                {
                    Name = $"{formula.Name}-map",
                    Resources = _mapperMock.Object.Map<List<ResourceDto>>(formula.ResourceFormula)
                });


            _mapperMock.Setup(s => s.Map(It.IsAny<FormulaDto>(), It.IsAny<FormulaProject>()))
                .Callback((FormulaDto dto, FormulaProject formula) =>
                {
                    formula.Name = dto.Name;
                });

            _mapperMock.Setup(s => s.Map<List<ResourceDto>>(It.IsAny<object>()))
                .Returns((ICollection<ResourceFormula> formula) =>
                {
                    var list = new List<ResourceDto>();
                    foreach (var res in formula)
                    {
                        list.Add(new ResourceDto
                        {
                            Type = res.Resource.Type,
                            Url = $"{res.Resource.Path}-url"
                        });
                    }
                    return list;
                });

            _mapperMock.Setup(s => s.Map<FormulaProject>(It.IsAny<object>()))
                .Returns((AddFormulaDto formula) => new FormulaProject
                {
                    Name = $"{formula.Name}-mapback"
                });
        }

        #region GetFormulas tests
        class GetFormulasTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                    new List<FormulaProject>
                    {
                        new FormulaProject
                        {
                            OwnerGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                            IsDeleted = true
                        }
                    },
                    new List<FormulaDto>()
                };//deleted own formula
                yield return new object[] {
                    new List<FormulaProject>
                    {
                        new FormulaProject
                        {
                            Owner = new IAutoM8.Domain.Models.User.User
                            {
                                OwnerId = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                            },
                            IsDeleted = true
                        }
                    },
                    new List<FormulaDto>()
                };//deleted manager formula
                yield return new object[] {
                    new List<FormulaProject>
                    {
                        new FormulaProject
                        {
                            OwnerGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f4"),
                            Owner = new IAutoM8.Domain.Models.User.User()
                        }
                    },
                    new List<FormulaDto>()
                };//formula of other owner
                yield return new object[] {
                    new List<FormulaProject>
                    {
                        new FormulaProject
                        {
                            Owner = new IAutoM8.Domain.Models.User.User
                            {
                                OwnerId = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f4")
                            },
                        }
                    },
                    new List<FormulaDto>()
                };//formula of other manager
                yield return new object[] {
                    new List<FormulaProject>
                    {
                        new FormulaProject
                        {
                            Name = "name",
                            OwnerGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                            Owner = new IAutoM8.Domain.Models.User.User()
                        }
                    },
                    new List<FormulaDto>
                    {
                        new FormulaDto
                        {
                            Name = "name-map"
                        }
                    }
                };//formula of owner
                yield return new object[] {
                    new List<FormulaProject>
                    {
                        new FormulaProject
                        {
                            Name = "name",
                            Owner = new IAutoM8.Domain.Models.User.User
                            {
                                OwnerId = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                            }
                        }
                    },
                    new List<FormulaDto>
                    {
                        new FormulaDto
                        {
                            Name = "name-map"
                        }
                    }
                };//formula of manager
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        //[Theory]
        //[ClassData(typeof(GetFormulasTestData))]
        //public async Task GetFormulas_ReturnCorrectData(List<FormulaProject> repoData, List<FormulaDto> expectedResult)
        //{
        //    _repoMock.SetupRepoMock(repoData);

        //    var actual = await _formulaService.GetFormulas();

        //    AssertEx.EqualSerialization(expectedResult, actual);
        //}

        #endregion

        #region GetFormula tests

        class GetFormulaTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                    new List<FormulaProject>
                    {
                        new FormulaProject
                        {
                            Id = 1,
                            Name = "name",
                            ResourceFormula = new List<ResourceFormula>
                            {
                                new ResourceFormula
                                {
                                    Resource = new Resource
                                    {
                                        Type = IAutoM8.Global.Enums.ResourceType.File,
                                        Path = "path"
                                    }
                                }
                            }
                        }
                    },
                    1,
                    new FormulaDto
                    {
                        Name = "name-map",
                        Resources = new List<ResourceDto>
                        {
                            new ResourceDto
                            {
                                Type = IAutoM8.Global.Enums.ResourceType.File,
                                Url = "path-url"
                            }
                        }
                    }
                };
                yield return new object[] {
                    new List<FormulaProject>
                    {
                        new FormulaProject
                        {
                            Id = 2,
                            Name = "name",
                            ResourceFormula = new List<ResourceFormula>
                            {
                                new ResourceFormula
                                {
                                    Resource = new Resource
                                    {
                                        Type = IAutoM8.Global.Enums.ResourceType.Link,
                                        Path = "path"
                                    }
                                }
                            }
                        }
                    },
                    2,
                    new FormulaDto
                    {
                        Name = "name-map",
                        Resources = new List<ResourceDto>
                        {
                            new ResourceDto
                            {
                                Type = IAutoM8.Global.Enums.ResourceType.Link,
                                Url = "path-url"
                            }
                        }
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(GetFormulaTestData))]
        public async Task GetFormula_ExistedFormulaId_ReturnCorrectData(List<FormulaProject> repoData, int id, FormulaDto expectedResult)
        {
            _repoMock.SetupRepoMock(repoData);
            var actual = await _formulaService.GetFormula(id);

            AssertEx.EqualSerialization(expectedResult, actual);
        }

        [Fact]
        public async Task GetFormula_NotExistedFormulaId_ThrowException()
        {
            _repoMock.SetupRepoMock(new List<FormulaProject>());

            await Assert.ThrowsAsync<ValidationException>(() => _formulaService.GetFormula(1));
        }
        #endregion

        #region AddFormula tests
        private List<FormulaProject> GetTransactionStorageForAdding()
        {
            var storage = new List<FormulaProject>();
            _transactionScopeMock.Setup(s => s.AddAsync(It.IsAny<FormulaProject>()))
                .Callback((FormulaProject formula) => { formula.Id = 1; storage.Add(formula); })
                .Returns(Task.CompletedTask);
            return storage;
        }

        [Fact]
        public async Task AddFormula_ValidData_ReturnCorrectData()
        {
            var storage = GetTransactionStorageForAdding();
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);

            var actual = await _formulaService.AddFormula(new AddFormulaDto
            {
                Name = "name"
            });

            AssertEx.EqualSerialization(new FormulaDto
            {
                Name = "name-mapback-map"
            }, actual);
        }

        [Fact]
        public async Task AddFormula_ValidData_SaveCorrectData()
        {
            var storage = GetTransactionStorageForAdding();
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);

            _dateTimeServiceMock.Setup(s => s.NowUtc).Returns(new DateTime(2001, 1, 2));
            await _formulaService.AddFormula(new AddFormulaDto
            {
                Name = "name"
            });

            AssertEx.EqualSerialization(new List<FormulaProject>
            {
                new FormulaProject{
                    Name = "name-mapback",
                    OwnerGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f5"),
                    DateCreated = new DateTime(2001, 1, 2),
                    Id = 1
                }
            }, storage);
        }

        [Fact]
        public async Task AddFormula_ValidData_AddFormulaAsyncCorrectCalling()
        {
            var storage = GetTransactionStorageForAdding();
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);

            await _formulaService.AddFormula(new AddFormulaDto
            {
                Name = "name"
            });


            _formulaNeo4JRepositoryMock.Verify(s => s.AddFormulaAsync(1), Times.Once);
        }

        [Fact]
        public async Task AddFormula_ValidData_SaveAndCommitAsyncCorrectCalling()
        {
            var storage = GetTransactionStorageForAdding();
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);

            await _formulaService.AddFormula(new AddFormulaDto
            {
                Name = "name"
            });


            _transactionScopeMock.Verify(s => s.SaveAndCommitAsync(CancellationToken.None), Times.Once);
        }
        #endregion

        #region UpdateFormula tests
        private List<FormulaProject> GetTransactionStorageForUpdating()
        {
            return new List<FormulaProject>{
                new FormulaProject
                {
                    Id = 1,
                    Name = "old name"
                }
            };
        }

        [Fact]
        public async Task UpdateFormula_ExistedFormulaId_ReturnCorrectData()
        {
            var storage = GetTransactionStorageForUpdating();

            var formulaTasks = new List<FormulaTask>
            {
                new FormulaTask
                {
                    Id = 1,
                    InternalFormulaProjectId = 1
                }
            };

            _repoMock.SetupRepoMock(storage, _transactionScopeMock);
            _repoMock.SetupRepoMock(formulaTasks, _transactionScopeMock);

            var actual = await _formulaService.UpdateFormula(new FormulaDto
            {
                Id = 1,
                Name = "new name"
            });

            AssertEx.EqualSerialization(new FormulaDto
            {
                Name = "new name-map"
            }, actual);
        }

        [Fact]
        public async Task UpdateFormula_ExistedFormulaId_SaveCorrectData()
        {
            var storage = GetTransactionStorageForUpdating();
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);
            _dateTimeServiceMock.Setup(s => s.NowUtc).Returns(new DateTime(2001, 1, 2));
            await _formulaService.UpdateFormula(new FormulaDto
            {
                Id = 1,
                Name = "new name"
            });

            AssertEx.EqualSerialization(new List<FormulaProject> {
                new FormulaProject
                {
                    Id = 1,
                    Name = "new name",
                    LastUpdated = new DateTime(2001, 1, 2)
                }
            }, storage);
        }

        [Fact]
        public async Task UpdateFormula_CategoriesId_SaveCorrectData()
        {
            var storage = GetTransactionStorageForUpdating();
            storage[0].FormulaProjectCategories = new List<FormulaProjectCategory>
                    {
                        new FormulaProjectCategory{CategoryId=2},
                        new FormulaProjectCategory{CategoryId=3}
                    };
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);
            _dateTimeServiceMock.Setup(s => s.NowUtc).Returns(new DateTime(2001, 1, 2));
            await _formulaService.UpdateFormula(new FormulaDto
            {
                Id = 1,
                Name = "new name",
                CategoryIds = new List<int> { 2, 4 }
            });

            AssertEx.EqualSerialization(new List<FormulaProject> {
                new FormulaProject
                {
                    Id = 1,
                    Name = "new name",
                    LastUpdated = new DateTime(2001, 1, 2),
                    FormulaProjectCategories = new List<FormulaProjectCategory>
                    {
                        new FormulaProjectCategory{CategoryId=2},
                        new FormulaProjectCategory{CategoryId=4}
                    }
                }
            }, storage);
        }

        [Fact]
        public async Task UpdateFormula_ExistedFormulaId_SaveAndCommitAsyncCorrectCalling()
        {
            var storage = GetTransactionStorageForUpdating();
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);
            _dateTimeServiceMock.Setup(s => s.NowUtc).Returns(new DateTime(2001, 1, 2));
            await _formulaService.UpdateFormula(new FormulaDto
            {
                Id = 1,
                Name = "new name"
            });

            _transactionScopeMock.Verify(v => v.SaveAndCommitAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task UpdateFormula_NotExistedFormulaId_ThrowsException()
        {
            var storage = GetTransactionStorageForUpdating();
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);


            await Assert.ThrowsAsync<ValidationException>(() => _formulaService.UpdateFormula(new FormulaDto
            {
                Id = 12
            }));
        }
        #endregion

        #region DeleteFormula tests
        private List<FormulaProject> GetTransactionStorageForDeleting()
        {
            return new List<FormulaProject>{
                new FormulaProject
                {
                    Id = 1,
                    IsDeleted = false,
                    ChildFormulaProjects = new List<FormulaProject>
                    {
                        new FormulaProject
                        {
                            Id  = 5
                        }
                    }
                }
            };
        }

        [Fact]
        public async Task DeleteFormula_ExistedFormulaId_SaveCorrectData()
        {
            var storage = GetTransactionStorageForDeleting();
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);
            _dateTimeServiceMock.Setup(s => s.NowUtc).Returns(new DateTime(2001, 1, 2));
            await _formulaService.DeleteFormula(1);

            AssertEx.EqualSerialization(new List<FormulaProject> {
                new FormulaProject
                {
                    Id = 1,
                    IsDeleted = true,
                    ChildFormulaProjects = new List<FormulaProject>
                    {
                        new FormulaProject
                        {
                            Id  = 5
                        }
                    }
                }
            }, storage);
        }

        [Fact]
        public async Task DeleteFormula_ExistedFormulaId_SaveAndCommitAsyncCorrectCalling()
        {
            var storage = GetTransactionStorageForDeleting();
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);
            await _formulaService.DeleteFormula(1);

            _transactionScopeMock.Verify(v => v.SaveAndCommitAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task DeleteFormula_NotExistedFormulaId_ThrowsException()
        {
            var storage = GetTransactionStorageForDeleting();
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);


            await Assert.ThrowsAsync<ValidationException>(() => _formulaService.DeleteFormula(2));
        }
        #endregion

        #region ImportTasksIntoProject tests
        private List<Project> GetTransactionProjectStorageForImportTasks()
        {
            return new List<Project>{
                new Project
                {
                    Id = 1,
                    StartDate = new DateTime(2010, 1, 2)
                }
            };
        }

        private List<FormulaProject> GetTransactionFormulaStorageForImportTasks()
        {
            return new List<FormulaProject>{
                new FormulaProject
                {
                    Id = 2,
                    FormulaTasks = new List<FormulaTask>
                    {
                        new FormulaTask
                        {
                            Id = 3
                        }
                    }
                }
            };
        }
        private void PrepareMocksForImportTasksIntoProject(
            Dictionary<int, int> importTasksIntoProjectResult,
            IEnumerable<int> getProjectRootTaskIdsResult,
            InitStartDateResultDto initTasksStartDateResult)
        {
            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
            var projectStorage = GetTransactionProjectStorageForImportTasks();
            var formulaStorage = GetTransactionFormulaStorageForImportTasks();
            _repoMock.SetupRepoMock(projectStorage, _transactionScopeMock);
            _repoMock.SetupRepoMock(formulaStorage, _transactionScopeMock);

            _taskImportServiceMock.Setup(s => s.ImportTasksIntoProjectAsync(It.IsAny<ITransactionScope>(),
                It.IsAny<Project>(), It.IsAny<IEnumerable<FormulaTask>>(),
                It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<SkillMapDto>>(),
                It.IsAny<int?>(), It.IsAny<(int x, int y)>())).ReturnsAsync(importTasksIntoProjectResult);

            _taskNeo4JRepositoryMock.Setup(s => s.GetProjectRootTaskIdsAsync(It.IsAny<int>()))
                .ReturnsAsync(getProjectRootTaskIdsResult);

            _startDateHelperServiceMock.Setup(s => s.InitTasksStartDate(It.IsAny<ITransactionScope>(),
                It.IsAny<int>(), It.IsAny<ProjectStartDatesDto>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(initTasksStartDateResult);

        }

        [Fact]
        public async Task ImportTasksIntoProject_ExistedProjectExistedFormula_CommitCorrectCalls()
        {
            PrepareMocksForImportTasksIntoProject(new Dictionary<int, int>(),
                new List<int>(), new InitStartDateResultDto());

            await _formulaService.ImportTasksIntoProject(1, new ImportTasksDto
            {
                FormulaId = 2,
                ProjectStartDates = new ProjectStartDatesDto(),
                SkillMappings = new List<SkillMapDto>()
            });

            _transactionMock.Verify(v => v.Commit(), Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProject_ExistedProjectExistedFormula_SaveAndCommitAsyncCorrectCalls()
        {
            PrepareMocksForImportTasksIntoProject(new Dictionary<int, int>(),
                new List<int>(), new InitStartDateResultDto());

            await _formulaService.ImportTasksIntoProject(1, new ImportTasksDto
            {
                FormulaId = 2,
                ProjectStartDates = new ProjectStartDatesDto(),
                SkillMappings = new List<SkillMapDto>()
            });

            _transactionScopeMock.Verify(v => v.SaveAndCommitAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProject_ExistedProjectExistedFormula_CopyFormulaResourcesToProjectCorrectCalls()
        {
            PrepareMocksForImportTasksIntoProject(new Dictionary<int, int>(),
                new List<int>(), new InitStartDateResultDto());

            var model = new ImportTasksDto
            {
                FormulaId = 2,
                ProjectStartDates = new ProjectStartDatesDto(),
                SkillMappings = new List<SkillMapDto>()
            };
            await _formulaService.ImportTasksIntoProject(1, model);

            _projectServiceMock.Verify(v => v.CopyFormulaResourcesToProject(It.Is<Project>(p => p.Id == 1),
                It.Is<FormulaProject>(l => l.Id == 2)), Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProject_ExistedProjectExistedFormula_ImportTasksIntoProjectAsyncCorrectCalls()
        {
            PrepareMocksForImportTasksIntoProject(new Dictionary<int, int>(),
                new List<int>(), new InitStartDateResultDto());

            var skillMappings = new List<SkillMapDto>
            {
                new SkillMapDto {SkillId = 3, IsOutsorced = true},
                new SkillMapDto {SkillId = 4}
            };

            var model = new ImportTasksDto
            {
                FormulaId = 2,
                ProjectStartDates = new ProjectStartDatesDto(),
                SkillMappings = skillMappings,
            };
            await _formulaService.ImportTasksIntoProject(1, model);

            _taskImportServiceMock.Verify(v => v.ImportTasksIntoProjectAsync(_transactionScopeMock.Object,
                It.Is<Project>(p => p.Id == 1),
                It.Is<IEnumerable<FormulaTask>>(l => l.Count() == 1 && l.ElementAt(0).Id == 3), new DateTime(2010, 1, 2),
                It.Is<IEnumerable<int>>(l => l.Count() == 1 && l.ElementAt(0) == 3), skillMappings, null, It.IsAny<(int x, int y)>()
                ), Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProject_ExistedProjectExistedFormula_GetProjectRootTaskIdsAsyncCorrectCalls()
        {
            PrepareMocksForImportTasksIntoProject(new Dictionary<int, int>(),
                new List<int>(), new InitStartDateResultDto());

            var model = new ImportTasksDto
            {
                FormulaId = 2,
                ProjectStartDates = new ProjectStartDatesDto(),
                SkillMappings = new List<SkillMapDto>()
            };
            await _formulaService.ImportTasksIntoProject(1, model);

            _taskNeo4JRepositoryMock.Verify(v => v.GetProjectRootTaskIdsAsync(1), Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProject_ExistedProjectExistedFormula_ScheduleJobsAsyncCorrectCalls()
        {
            var initTasksStartDateResult = new InitStartDateResultDto
            {
                RootTasks = new List<ProjectTask>()
            };
            PrepareMocksForImportTasksIntoProject(new Dictionary<int, int>(),
                new List<int>(), initTasksStartDateResult);

            var model = new ImportTasksDto
            {
                FormulaId = 2,
                ProjectStartDates = new ProjectStartDatesDto(),
                SkillMappings = new List<SkillMapDto>()
            };

            await _formulaService.ImportTasksIntoProject(1, model);
            _taskImportServiceMock.Verify(
                v => v.ScheduleJobsAsync(_transactionScopeMock.Object, initTasksStartDateResult.RootTasks),
                Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProject_ExistedProjectExistedFormula_InitTasksStartDateCorrectCalls()
        {
            var initTasksStartDateResult = new InitStartDateResultDto
            {
                RootTasks = new List<ProjectTask>()
            };
            PrepareMocksForImportTasksIntoProject(new Dictionary<int, int> { { 500, 50 } },
                new List<int> { 500 }, initTasksStartDateResult);

            var model = new ImportTasksDto
            {
                FormulaId = 2,
                ProjectStartDates = new ProjectStartDatesDto
                {
                    ProjectStartDateTime = new DateTime(2015, 1, 1),
                    RootStartDateTime = new Dictionary<int, DateTime?> { { 50, new DateTime(2000, 1, 1) } }
                },
                SkillMappings = new List<SkillMapDto>()
            };

            await _formulaService.ImportTasksIntoProject(1, model);

            _startDateHelperServiceMock.Verify(
                v => v.InitTasksStartDate(_transactionScopeMock.Object, 1,
                It.Is<ProjectStartDatesDto>(dto => dto.RootStartDateTime.Count == 1 &&
                dto.RootStartDateTime[500] == new DateTime(2000, 1, 1) &&
                dto.ProjectStartDateTime.Value == new DateTime(2015, 1, 1)),
                It.Is<IEnumerable<int>>(l => l.Count() == 1 && l.ElementAt(0) == 500)),
                Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProject_ExistedProjectExistedFormulaNoSetProjectTart_InitTasksStartDateCorrectCalls()
        {
            var initTasksStartDateResult = new InitStartDateResultDto
            {
                RootTasks = new List<ProjectTask>()
            };
            PrepareMocksForImportTasksIntoProject(new Dictionary<int, int> { { 500, 50 } },
                new List<int> { 500 }, initTasksStartDateResult);

            var model = new ImportTasksDto
            {
                FormulaId = 2,
                ProjectStartDates = new ProjectStartDatesDto
                {
                    RootStartDateTime = new Dictionary<int, DateTime?> { { 50, new DateTime(2000, 1, 1) } }
                },
                SkillMappings = new List<SkillMapDto>()
            };
            await _formulaService.ImportTasksIntoProject(1, model);

            _startDateHelperServiceMock.Verify(
                v => v.InitTasksStartDate(_transactionScopeMock.Object, 1,
                It.Is<ProjectStartDatesDto>(dto => dto.RootStartDateTime.Count == 1 &&
                dto.RootStartDateTime[500] == new DateTime(2000, 1, 1) &&
                dto.ProjectStartDateTime.Value == new DateTime(2010, 1, 2)),
                It.Is<IEnumerable<int>>(l => l.Count() == 1 && l.ElementAt(0) == 500)),
                Times.Once);
        }
        #endregion
    }
}
