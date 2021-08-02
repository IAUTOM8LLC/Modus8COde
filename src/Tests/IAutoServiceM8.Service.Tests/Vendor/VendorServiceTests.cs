using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Resource;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Neo4jRepository.Dto;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.Resources.Dto;
using IAutoM8.Service.Vendor;
using IAutoM8.Service.Vendor.Interfaces;
using IAutoM8.Tests.Common;
using Moq;
using Xunit;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Global.Enums;
using IAutoM8.Service.Notification.Interfaces;

namespace IAutoM8.Service.Tests.Vendor
{
    public class VendorServiceTests
    {
        private Mock<IRepo> _repoMock;
        private Mock<IFormulaTaskNeo4jRepository> _formulaTaskNeo4JRepositoryMock;
        private Mock<ClaimsPrincipal> _principalMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IStorageService> _storageServiceMock;
        private Mock<ITaskNeo4jRepository> _taskNeo4JRepositoryMock;
        private Mock<ITransactionScope> _transactionScopeMock;
        private Mock<ITaskHistoryService> _taskHistoryServiceMock;
        private Mock<IDateTimeService> _dateTimeServiceMock;
        private Mock<INotificationService> _notificationServiceMock;
        private readonly IVendorService _vendorService;

        public VendorServiceTests()
        {
            _repoMock = new Mock<IRepo>();
            _formulaTaskNeo4JRepositoryMock = new Mock<IFormulaTaskNeo4jRepository>();
            _storageServiceMock = new Mock<IStorageService>();
            _taskNeo4JRepositoryMock = new Mock<ITaskNeo4jRepository>();
            _transactionScopeMock = new Mock<ITransactionScope>();
            _mapperMock = new Mock<IMapper>();
            _principalMock = new Mock<ClaimsPrincipal>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _principalMock.Setup(s => s.Claims)
                .Returns(new List<Claim>
                {
                    new Claim(ClaimTypes.PrimarySid, "22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                    new Claim(ClaimTypes.NameIdentifier, "22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                });
            _taskHistoryServiceMock = new Mock<ITaskHistoryService>();
            _vendorService = new VendorService(
                _repoMock.Object,
                _mapperMock.Object,
                _taskNeo4JRepositoryMock.Object,
                _formulaTaskNeo4JRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _taskHistoryServiceMock.Object,
                _principalMock.Object,
                _storageServiceMock.Object,
                _notificationServiceMock.Object
            );

            _mapperMock.Setup(s => s.Map<List<ResourceDto>>(It.IsAny<IList<TaskResourceNeo4jDto>>(), It.IsAny<Action<IMappingOperationOptions>>()))
                .Returns((ICollection<TaskResourceNeo4jDto> formula, Action<IMappingOperationOptions> action) =>
                {
                    var list = new List<ResourceDto>();
                    foreach (var res in formula)
                    {
                        list.Add(new ResourceDto
                        {
                            Type = res.Type == 0 ? Global.Enums.ResourceType.File : Global.Enums.ResourceType.Link,
                            Url = $"{res.Path}-url"
                        });
                    }
                    return list;
                });

            _mapperMock.Setup(s => s.Map<List<ResourceDto>>(It.IsAny<IList<Resource>>(), It.IsAny<Action<IMappingOperationOptions>>()))
                .Returns((ICollection<Resource> formula, Action<IMappingOperationOptions> action) =>
                {
                    var list = new List<ResourceDto>();
                    foreach (var res in formula)
                    {
                        list.Add(new ResourceDto
                        {
                            Type = res.Type,
                            Url = $"{res.Path}-url"
                        });
                    }
                    return list;
                });

            _formulaTaskNeo4JRepositoryMock.Setup(t => t.GetTaskAndSharedResourcesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Neo4jRepository.Dto.TaskResourceNeo4jDto>());

            _taskNeo4JRepositoryMock.Setup(t => t.GetTaskAndSharedResourcesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Neo4jRepository.Dto.TaskResourceNeo4jDto>());
        }

        #region GetFormulaTaskVendorNotification tests

        [Fact]
        public async Task GetFormulaTaskVendorNotification_IncorrectId_ExceptionThrown()
        {
            _repoMock.SetupSingleValueRepoMock(new FormulaTaskVendor
            {
                Id = 11,
                VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),

            }, _transactionScopeMock);

            await Assert.ThrowsAsync<ValidationException>(() => _vendorService.GetFormulaTaskVendorNotification(100));
        }

        [Fact]
        public async Task GetFormulaTaskVendorNotification_IncorrectNotificationStatus_ExceptionThrown()
        {
            _repoMock.SetupSingleValueRepoMock(new FormulaTaskVendor
            {
                Id = 11,
                VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                Status = Global.Enums.FormulaRequestStatus.DeclinedByOwner
            }, _transactionScopeMock);

            await Assert.ThrowsAsync<ValidationException>(() => _vendorService.GetFormulaTaskVendorNotification(100));
        }

        [Fact]
        public async Task GetFormulaTaskVendorNotification_CorrectId_ReturnCorrectData()
        {
            _repoMock.SetupRepoMock(new List<FormulaTaskVendor> {
                new FormulaTaskVendor
                {
                    Id = 11,
                    VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                    Status = Global.Enums.FormulaRequestStatus.None,
                    FormulaTask = new FormulaTask
                    {
                        Title = "Hello",
                        Description = "Lorem ipsum",
                        Id = 22,
                        FormulaProjectId = 3,
                        Duration = 12
                    }
                }
            }, _transactionScopeMock);


            _repoMock.SetupRepoMock(new List<ResourceFormula> {
                new ResourceFormula
                {
                    Resource = new Resource
                    {
                        Path = "dasdas"
                    },
                    FormulaId = 3
                },
                new ResourceFormula
                {
                    Resource = new Resource
                    {
                        Path = "dasdas/dqewq/23"
                    },
                    FormulaId = 5
                },
                new ResourceFormula
                {
                    Resource = new Resource
                    {
                        Path = "dasdas/eqwecxz/44"
                    },
                    FormulaId = 3
                }
            }, _transactionScopeMock);

            var actual = await _vendorService.GetFormulaTaskVendorNotification(11);
            var expected = new FormulaTaskVendorDto
            {
                NotificationId = 11,
                Title = "Hello",
                Description = "Lorem ipsum",
                Duration = 12,
                Price = 0,
                Answer = Global.Enums.FormulaRequestStatus.None,
                Resources = new List<ResourceDto>
                {
                    new ResourceDto
                    {
                        Type = Global.Enums.ResourceType.File,
                        Url = "dasdas-url"
                    },
                    new ResourceDto
                    {
                        Type = Global.Enums.ResourceType.File,
                        Url = "dasdas/eqwecxz/44-url"
                    }
                }
            };
            AssertEx.EqualSerialization(expected, actual);
        }
        #endregion

        #region GetProjectTaskVendorNotification tests

        [Fact]
        public async Task GetProjectTaskVendorNotification_IncorrectId_ExceptionThrown()
        {
            _repoMock.SetupSingleValueRepoMock(new ProjectTaskVendor
            {
                Id = 11,
                VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),

            }, _transactionScopeMock);

            await Assert.ThrowsAsync<ValidationException>(() => _vendorService.GetProjectTaskVendorNotification(100));
        }

        [Fact]
        public async Task GetProjectTaskVendorNotification_IncorrectNotificationStatus_ExceptionThrown()
        {
            _repoMock.SetupSingleValueRepoMock(new ProjectTaskVendor
            {
                Id = 11,
                VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                Status = Global.Enums.ProjectRequestStatus.DeclinedByOwner
            }, _transactionScopeMock);

            await Assert.ThrowsAsync<ValidationException>(() => _vendorService.GetProjectTaskVendorNotification(100));
        }

        [Fact]
        public async Task GetProjectTaskVendorNotification_CorrectId_ReturnCorrectData()
        {
            _repoMock.SetupRepoMock(new List<ProjectTaskVendor> {
                new ProjectTaskVendor
                {
                    Id = 15,
                    VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                    Vendor = new Domain.Models.User.User
                    {
                        Id = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                    },
                    Status = Global.Enums.ProjectRequestStatus.Send,
                    Price = 200,
                    ProjectTask = new ProjectTask
                    {
                        Title = "Hello",
                        Description = "Lorem ipsum",
                        Id = 22,
                        ProjectId = 3,
                        Duration = 12
                    }
                }
            }, _transactionScopeMock);


            _repoMock.SetupRepoMock(new List<ResourceProject> {
                new ResourceProject
                {
                    Resource = new Resource
                    {
                        Path = "dasdas"
                    },
                    ProjectId = 3
                },
                new ResourceProject
                {
                    Resource = new Resource
                    {
                        Path = "dasdas/dqewq/23"
                    },
                    ProjectId = 5
                },
                new ResourceProject
                {
                    Resource = new Resource
                    {
                        Path = "dasdas/eqwecxz/44"
                    },
                    ProjectId = 3
                }
            }, _transactionScopeMock);

            var actual = await _vendorService.GetProjectTaskVendorNotification(15);
            var expected = new ProjectTaskVendorDto
            {
                NotificationId = 15,
                Title = "Hello",
                Description = "Lorem ipsum",
                Duration = 12,
                Price = 200,
                Answer = Global.Enums.ProjectRequestStatus.Send,
                Resources = new List<ResourceDto>
                {
                    new ResourceDto
                    {
                        Type = Global.Enums.ResourceType.File,
                        Url = "dasdas-url"
                    },
                    new ResourceDto
                    {
                        Type = Global.Enums.ResourceType.File,
                        Url = "dasdas/eqwecxz/44-url"
                    }
                }
            };
            AssertEx.EqualSerialization(expected, actual);
        }

        #endregion

        #region UpdateProjectTaskVendorNotificationStatus tests

        [Fact]
        public async Task UpdateProjectTaskVendorNotificationStatus_CorrectDto_StatusUpdated()
        {
            var vendorNotification = new ProjectTaskVendor
            {
                Id = 15,
                VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                Vendor = new Domain.Models.User.User
                {
                    Id = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                },
                Status = Global.Enums.ProjectRequestStatus.Send,
                Price = 200,
                ProjectTask = new ProjectTask
                {
                    Title = "Hello",
                    Description = "Lorem ipsum",
                    Id = 22,
                    ProjectId = 3,
                    Duration = 12,
                    Project = new Project
                    {
                        OwnerGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                    }
                }
            };

            _repoMock.SetupRepoMock(new List<ProjectTaskVendor> {
                vendorNotification
            }, _transactionScopeMock);

            _repoMock.SetupRepoMock(new List<FormulaTaskStatistic>(), _transactionScopeMock);

            _repoMock.SetupRepoMock(
                new List<CreditsTax>
                {
                    new CreditsTax
                    {
                        Id = 1,
                        Fee = 0,
                        Percentage = 3,
                        Type = Global.Enums.CreditsTaxType.Vendor
                    }
                }
            , _transactionScopeMock);

            await _vendorService.UpdateProjectTaskVendorNotificationStatus(new ProjectTaskVendorDto
            {
                NotificationId = 15,
                Answer = Global.Enums.ProjectRequestStatus.Accepted
            });

            Assert.True(vendorNotification.Status == Global.Enums.ProjectRequestStatus.Accepted);
        }

        [Fact]
        public async Task UpdateProjectTaskVendorNotificationStatus_CorrectDto_SaveChangesAsyncCalled()
        {
            var vendorNotification = new ProjectTaskVendor
            {
                Id = 15,
                VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                Vendor = new Domain.Models.User.User
                {
                    Id = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                },
                Status = Global.Enums.ProjectRequestStatus.Send,
                Price = 200,
                ProjectTask = new ProjectTask
                {
                    Title = "Hello",
                    Description = "Lorem ipsum",
                    Id = 22,
                    ProjectId = 3,
                    Duration = 12,
                    Project = new Project
                    {
                         OwnerGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                    }
                }
            };

            _repoMock.SetupRepoMock(new List<ProjectTaskVendor> {
                vendorNotification
            }, _transactionScopeMock);

            _repoMock.SetupRepoMock(new List<FormulaTaskStatistic> (), _transactionScopeMock);

            _repoMock.SetupRepoMock(
                new List<CreditsTax>
                {
                    new CreditsTax
                    {
                        Id = 1,
                        Fee = 0,
                        Percentage = 3,
                        Type = Global.Enums.CreditsTaxType.Vendor
                    }
                }
            , _transactionScopeMock);

            await _vendorService.UpdateProjectTaskVendorNotificationStatus(new ProjectTaskVendorDto
            {
                NotificationId = 15,
                Answer = Global.Enums.ProjectRequestStatus.Accepted
            });

            _transactionScopeMock.Verify(t => t.SaveAndCommit(), Times.Once);
        }


        [Fact]
        public async Task UpdateProjectTaskVendorNotificationStatus_CorrectDto_AddStatistic()
        {
            var vendorNotification = new ProjectTaskVendor
            {
                Id = 15,
                VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                Vendor = new Domain.Models.User.User
                {
                    Id = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                },
                Status = Global.Enums.ProjectRequestStatus.Send,
                Price = 200,
                ProjectTaskId = 22,
                ProjectTask = new ProjectTask
                {
                    Title = "Hello",
                    Description = "Lorem ipsum",
                    Id = 22,
                    ProjectId = 3,
                    Duration = 12,
                    Project = new Project
                    {
                        OwnerGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6")
                    },
                    FormulaTaskId = 50,
                    Status = TaskStatusType.InProgress
                }
            };

            _repoMock.SetupRepoMock(new List<ProjectTaskVendor> {
                vendorNotification
            }, _transactionScopeMock);

            _repoMock.SetupRepoMock(new List<FormulaTaskStatistic>(), _transactionScopeMock);

            _repoMock.SetupRepoMock(
                new List<CreditsTax>
                {
                    new CreditsTax
                    {
                        Id = 1,
                        Fee = 0,
                        Percentage = 3,
                        Type = Global.Enums.CreditsTaxType.Vendor
                    }
                }
            , _transactionScopeMock);

            await _vendorService.UpdateProjectTaskVendorNotificationStatus(new ProjectTaskVendorDto
            {
                NotificationId = 15,
                Answer = Global.Enums.ProjectRequestStatus.Accepted
            });
            _transactionScopeMock.Verify(v => v.Add(It.Is<FormulaTaskStatistic>(it =>
                            it.FormulaTaskId == 50 &&
                            it.ProjectTaskId == 22 &&
                            it.Type == StatisticType.Working &&
                            it.VendorGuid == new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"))), Times.Once);
        }

        #endregion

        #region UpdateProjectTaskVendorNotificationStatus tests

        [Fact]
        public async Task UpdateFormulaTaskVendorNotificationStatus_CorrectDto_StatusUpdated()
        {
            var vendorNotification = new FormulaTaskVendor
            {
                Id = 15,
                VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                Status = Global.Enums.FormulaRequestStatus.None,
                Price = 200,
                FormulaTask = new FormulaTask
                {
                    Title = "Hello",
                    Description = "Lorem ipsum",
                    Id = 22,
                    FormulaProjectId = 3,
                    Duration = 12
                }
            };

            _repoMock.SetupRepoMock(new List<FormulaTaskVendor> {
                vendorNotification
            }, _transactionScopeMock);

            await _vendorService.UpdateFormulaTaskVendorNotificationStatus(new FormulaTaskVendorDto
            {
                NotificationId = 15,
                Price = 300,
                Answer = Global.Enums.FormulaRequestStatus.Accepted
            });

            Assert.True(vendorNotification.Status == Global.Enums.FormulaRequestStatus.Accepted);
        }

        [Fact]
        public async Task UpdateFormulaTaskVendorNotificationStatus_CorrectDto_PriceUpdated()
        {
            var vendorNotification = new FormulaTaskVendor
            {
                Id = 15,
                VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                Status = Global.Enums.FormulaRequestStatus.None,
                Price = 200,
                FormulaTask = new FormulaTask
                {
                    Title = "Hello",
                    Description = "Lorem ipsum",
                    Id = 22,
                    FormulaProjectId = 3,
                    Duration = 12
                }
            };

            _repoMock.SetupRepoMock(new List<FormulaTaskVendor> {
                vendorNotification
            }, _transactionScopeMock);

            await _vendorService.UpdateFormulaTaskVendorNotificationStatus(new FormulaTaskVendorDto
            {
                NotificationId = 15,
                Answer = Global.Enums.FormulaRequestStatus.Accepted,
                Price = 300
            });

            Assert.True(vendorNotification.Price == 300);
        }

        [Fact]
        public async Task UpdateFormulaTaskVendorNotificationStatus_CorrectDto_SaveChangesAsyncCalled()
        {
            var vendorNotification = new FormulaTaskVendor
            {
                Id = 15,
                VendorGuid = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
                Status = Global.Enums.FormulaRequestStatus.None,
                Price = 200,
                FormulaTask = new FormulaTask
                {
                    Title = "Hello",
                    Description = "Lorem ipsum",
                    Id = 22,
                    FormulaProjectId = 3,
                    Duration = 12
                }
            };

            _repoMock.SetupRepoMock(new List<FormulaTaskVendor> {
                vendorNotification
            }, _transactionScopeMock);

            await _vendorService.UpdateFormulaTaskVendorNotificationStatus(new FormulaTaskVendorDto
            {
                NotificationId = 15,
                Answer = Global.Enums.FormulaRequestStatus.Accepted,
                Price = 300
            });

            _repoMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        #endregion
    }
}
