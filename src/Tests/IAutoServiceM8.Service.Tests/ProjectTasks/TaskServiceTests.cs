//using AutoMapper;
//using IAutoM8.Domain.Models;
//using IAutoM8.Domain.Models.Project;
//using IAutoM8.Domain.Models.Project.Task;
//using IAutoM8.Domain.Models.Skill;
//using IAutoM8.Domain.Models.Resource;
//using IAutoM8.Domain.Models.User;
//using IAutoM8.Global;
//using IAutoM8.Global.Enums;
//using IAutoM8.Global.Exceptions;
//using IAutoM8.Neo4jRepository.Dto;
//using IAutoM8.Neo4jRepository.Interfaces;
//using IAutoM8.Repository;
//using IAutoM8.Service.CommonDto;
//using IAutoM8.Service.CommonService.Interfaces;
//using IAutoM8.Service.Notification.Interfaces;
//using IAutoM8.Service.ProjectTasks;
//using IAutoM8.Service.ProjectTasks.Dto;
//using IAutoM8.Service.ProjectTasks.Interfaces;
//using IAutoM8.Service.Resources.Dto;
//using IAutoM8.Service.Scheduler.Interfaces;
//using IAutoM8.Tests.Common;
//using IAutoM8.WebSockets.Stores.Interfaces;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading;
//using System.Threading.Tasks;
//using Xunit;
//using IAutoM8.Domain.Models.Vendor;
//using IAutoM8.Service.Users.Dto;
//using Microsoft.AspNetCore.Identity;

//namespace IAutoM8.Service.Tests.ProjectTasks
//{
//    public class TaskServiceTests
//    {
//        private ITaskService _unit;
//        private Mock<IRepo> _repoMock;
//        private Mock<IScheduleService> _scheduleServiceMock;
//        private Mock<ITaskScheduleService> _taskScheduleServiceMock;
//        private Mock<ClaimsPrincipal> _principalMock;
//        private Mock<UserManager<User>> _userManagerMock;
//        private Mock<IDateTimeService> _dateTimeServiceMock;
//        private Mock<INotificationService> _notificationServiceMock;
//        private Mock<IStorageService> _storageServiceMock;
//        private Mock<ITaskNeo4jRepository> _taskNeo4JRepositoryMock;
//        private Mock<ITaskHistoryService> _taskHistoryServiceMock;
//        private Mock<ITaskStartDateHelperService> _startDateHelperServiceMock;
//        private Mock<ITaskSocketStore> _taskSocketStoreMock;
//        private Mock<ITransactionScope> _transactionScopeMock;
//        private Mock<ITransaction> _transactionMock;
//        private Mock<IFormulaTaskJobService> _formulaTaskJobServiceMock;
//        private Mock<IMapper> _mapperMock;
//        private Mock<IEntityFrameworkPlus> _entityFrameworkPlusMock;

//        private List<ProjectTask> TestTaskData;
//        static Guid UserId = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f5");

//        public TaskServiceTests()
//        {
//            _repoMock = new Mock<IRepo>();
//            _scheduleServiceMock = new Mock<IScheduleService>();
//            _taskScheduleServiceMock = new Mock<ITaskScheduleService>();
//            _taskScheduleServiceMock.Setup(s => s.GetNextOccurence(It.IsAny<ITransactionScope>(), It.IsAny<int>(), It.IsAny<RecurrenceOptions>()))
//                .ReturnsAsync(new RecurrenceAsapDto());
//            _dateTimeServiceMock = new Mock<IDateTimeService>();
//            _dateTimeServiceMock.Setup(s => s.ParseRecurrenceAsap(It.IsAny<RecurrenceOptions>(), It.IsAny<DateTime>()))
//                .Returns(new RecurrenceAsapDto());
//            _notificationServiceMock = new Mock<INotificationService>();
//            _storageServiceMock = new Mock<IStorageService>();
//            _startDateHelperServiceMock = new Mock<ITaskStartDateHelperService>();
//            _taskNeo4JRepositoryMock = new Mock<ITaskNeo4jRepository>();
//            _taskSocketStoreMock = new Mock<ITaskSocketStore>();
//            _taskHistoryServiceMock = new Mock<ITaskHistoryService>();
//            _principalMock = new Mock<ClaimsPrincipal>();
//            _principalMock.Setup(s => s.Claims).Returns(new List<Claim>
//             {
//                    new Claim(ClaimTypes.PrimarySid, "22345200-abe8-4f60-90c8-0d43c5f6c0f6"),
//                    new Claim(ClaimTypes.NameIdentifier, UserId.ToString())
//             });
//            _formulaTaskJobServiceMock = new Mock<IFormulaTaskJobService>();
//            _transactionScopeMock = new Mock<ITransactionScope>();
//            _transactionMock = new Mock<ITransaction>();
//            var userStore = new Mock<IUserStore<User>>();
            
//            var userStoreMock = new Mock<IUserStore<User>>();
//            _userManagerMock = new Mock<UserManager<User>>(
//                userStoreMock.Object, null, null, null, null, null, null, null, null);
//            ;
//            TestTaskData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ProjectId = 1,
//                    Title = "test",
//                    ProjectTaskUsers = new List<ProjectTaskUser>
//                    {
//                        new ProjectTaskUser
//                        {
//                            UserId = UserId,
//                            ProjectTaskId = 1,
//                            ProjectTaskUserType =  ProjectTaskUserType.Assigned
//                        },
//                        new ProjectTaskUser
//                        {
//                            UserId = UserId,
//                            ProjectTaskId = 1,
//                            ProjectTaskUserType =  ProjectTaskUserType.Reviewing
//                        }
//                    }
//                }
//            };

//            SetupMapperMock();
//            _entityFrameworkPlusMock = new Mock<IEntityFrameworkPlus>();
//            _unit = new TaskService(
//                _repoMock.Object,
//                _scheduleServiceMock.Object,
//                _taskScheduleServiceMock.Object,
//                _principalMock.Object,
//                _userManagerMock.Object,
//                _dateTimeServiceMock.Object,
//                _notificationServiceMock.Object,
//                _storageServiceMock.Object,
//                _taskNeo4JRepositoryMock.Object,
//                _taskHistoryServiceMock.Object,
//                _startDateHelperServiceMock.Object,
//                _taskSocketStoreMock.Object,
//                _mapperMock.Object,
//                _formulaTaskJobServiceMock.Object,
//                _entityFrameworkPlusMock.Object
//            );
//        }

//        private void SetupMapperMock()
//        {
//            _mapperMock = new Mock<IMapper>();

//            _mapperMock.Setup(t => t.Map<List<ResourceDto>>(It.IsAny<List<TaskResourceNeo4jDto>>(),
//                It.IsAny<Action<IMappingOperationOptions>>())).Returns(
//                (List<TaskResourceNeo4jDto> recouces, Action<IMappingOperationOptions> opts) =>
//                {
//                    var paramMock = new Mock<IMappingOperationOptions>();
//                    var items = new Dictionary<string, object>();
//                    paramMock.Setup(s => s.Items).Returns(items);
//                    opts.Invoke(paramMock.Object);
//                    var urlBuilder = (Func<string, string>)items["urlBuilder"];
//                    return recouces?.Select(t => new ResourceDto
//                    {
//                        Type = t.Type == 0 ? ResourceType.File : ResourceType.Link,
//                        Url = t.Type == 0 ? urlBuilder(t.Path) : t.Path
//                    }).ToList() ?? new List<ResourceDto>();
//                });
//            _mapperMock.Setup(t => t.Map<List<ResourceDto>>(It.IsAny<List<Resource>>(),
//                It.IsAny<Action<IMappingOperationOptions>>())).Returns(
//                (List<Resource> recouces, Action<IMappingOperationOptions> opts) =>
//                {
//                    var paramMock = new Mock<IMappingOperationOptions>();
//                    var items = new Dictionary<string, object>();
//                    paramMock.Setup(s => s.Items).Returns(items);
//                    opts.Invoke(paramMock.Object);
//                    var urlBuilder = (Func<string, string>)items["urlBuilder"];
//                    var isShared = items.ContainsKey("isShared") && (bool)items["isShared"];
//                    return recouces?.Select(t => new ResourceDto
//                    {
//                        Type = t.Type == 0 ? ResourceType.File : ResourceType.Link,
//                        Url = t.Type == 0 ? urlBuilder(t.Path) : t.Path,
//                        IsSharedFromParent = isShared
//                    }).ToList() ?? new List<ResourceDto>();
//                });

//            _mapperMock.Setup(t => t.Map<TaskDto>(It.IsAny<ProjectTask>()))
//                .Returns((ProjectTask projectTask) =>
//                {
//                    var result = new TaskDto
//                    {
//                        ParentTasks = new List<int>(),
//                        ChildTasks = new List<int>(),
//                        ConditionalParentTasks = new List<int>(),
//                        Resources = new List<ResourceDto>()
//                    };

//                    if (projectTask != null)
//                    {
//                        result.Status = projectTask.Status.ToString();
//                        result.Id = projectTask.Id;
//                        result.ProjectId = projectTask.ProjectId;
//                        result.Title = "test";
//                    }

//                    return result;
//                });

//            //_mapperMock.Setup(t => t.Map(It.IsAny<UpdateTaskDto>(), It.IsAny<ProjectTask>()))
//            //    .Callback(
//            //        (UpdateTaskDto updateTaskDto, ProjectTask projectTask) =>
//            //        {
//            //            projectTask.AssignedSkillId = updateTaskDto.AssignedSkillId;
//            //        });

//            _mapperMock.Setup(t => t.Map<ProjectTask>(It.IsAny<UpdateTaskDto>()))
//                .Returns((UpdateTaskDto updateTaskDto) => new ProjectTask
//                {
//                    Id = updateTaskDto.Id,
//                    ProjectId = updateTaskDto.ProjectId,
//                    RecurrenceOptions = new RecurrenceOptions(),
//                    Condition = new ProjectTaskCondition
//                    {
//                        Options = updateTaskDto.Condition.Options.Select(t => new ProjectTaskConditionOption
//                        {
//                            AssignedTaskId = t.AssignedTaskId,
//                            Id = t.Id
//                        }).ToList()
//                    }
//                });
//        }

//        #region GetTaskAsync (Task<List<TaskDto>> overload)

//        [Fact]
//        public async Task GetTaskAsync_UserWithoutAcces_ExceptionThrown()
//        {
//            var projects = new List<Project>
//            {
//                new Project
//                {
//                    Id = 1
//                },
//                new Project
//                {
//                    Id = 2
//                }
//            };

//            _repoMock.SetupRepoMock(projects, _transactionScopeMock);
//            _repoMock.SetupRepoMock(new List<User> {
//                new User
//                {
//                    Id = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f5")
//                }
//            }, _transactionScopeMock);
//            await Assert.ThrowsAsync<ForbiddenException>(() => _unit.GetTasksAsync(new List<int> { 2 }));
//        }

//        [Fact]
//        public async Task GetTaskAsync_Vendor_UserWithoutAcces_ExceptionThrown()
//        {
//            _principalMock.Setup(t => t.IsInRole(UserRoles.Vendor)).Returns(true);
//            var projects = new List<Project>
//            {
//                new Project
//                {
//                    Id = 1
//                },
//                new Project
//                {
//                    Id = 2
//                }
//            };

//            _repoMock.SetupRepoMock(projects, _transactionScopeMock);
//            _repoMock.SetupRepoMock<ProjectTaskVendor>(_transactionScopeMock);
//            _repoMock.SetupRepoMock(new List<User> {
//                new User
//                {
//                    Id = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f5")
//                }
//            }, _transactionScopeMock);
//            await Assert.ThrowsAsync<ForbiddenException>(() => _unit.GetTasksAsync(new List<int> { 2 }));
//        }

//        [Fact(Skip = "Need to add ProjectTo mocking")]
//        public async Task GetTaskAsync_UserAsOwner_CorrectTaskCanBeProccessed()
//        {
//            SetupMocksAndTestDataForGetTasksAsync();
//            _principalMock.Setup(t => t.IsInRole(UserRoles.Owner)).Returns(true);
//            var taskIdsCanBeProccessed = new[] { 1, 2 };
//            var tasks = await _unit.GetTasksAsync(new List<int> { 1 });
//            Assert.True(tasks.Where(t => taskIdsCanBeProccessed.Contains(t.Id)).All(t => t.CanBeProccessed)
//                        && tasks.Where(t => !taskIdsCanBeProccessed.Contains(t.Id)).All(t => !t.CanBeProccessed));
//        }

//        [Fact(Skip = "Need to add ProjectTo mocking")]
//        public async Task GetTaskAsync_UserAsOwner_CorrectTaskCanBeReviewed()
//        {
//            SetupMocksAndTestDataForGetTasksAsync();
//            _principalMock.Setup(t => t.IsInRole(UserRoles.Owner)).Returns(true);
//            var tasks = await _unit.GetTasksAsync(new List<int> { 1 });
//            Assert.True(tasks.Where(t => t.Id == 3).All(t => t.CanBeReviewed)
//                        && tasks.Where(t => t.Id != 3).All(t => !t.CanBeReviewed));
//        }

//        [Fact(Skip = "Need to add ProjectTo mocking")]
//        public async Task GetTaskAsync_UserAsOwner_CorrectProccessingFullNameReturned()
//        {
//            SetupMocksAndTestDataForGetTasksAsync();
//            _principalMock.Setup(t => t.IsInRole(UserRoles.Owner)).Returns(true);
//            var tasks = await _unit.GetTasksAsync(new List<int> { 1 });
//            Assert.True(tasks.FirstOrDefault(t => t.Id == 8)?.ProccessingUserName == "Ivan Ivanov"
//                        && tasks.Where(t => t.Id != 8).All(t => t.ProccessingUserName == null));
//        }

//        [Fact(Skip = "Need to add ProjectTo mocking")]
//        public async Task GetTaskAsync_UserAsOwner_CorrectReviewingUserNameReturned()
//        {
//            SetupMocksAndTestDataForGetTasksAsync();
//            _principalMock.Setup(t => t.IsInRole(UserRoles.Owner)).Returns(true);
//            var tasks = await _unit.GetTasksAsync(new List<int> { 1 });
//            Assert.True(tasks.FirstOrDefault(t => t.Id == 9)?.ReviewingUserName == "Neil deGrasse"
//                        && tasks.Where(t => t.Id != 9).All(t => t.ReviewingUserName == null));
//        }

//        [Fact(Skip = "Need to add ProjectTo mocking")]
//        public async Task GetTaskAsync_UserAsOwner_CorrectDataReturned()
//        {
//            SetupMocksAndTestDataForGetTasksAsync();
//            _principalMock.Setup(t => t.IsInRole(UserRoles.Owner)).Returns(true);
//            var actual = await _unit.GetTasksAsync(new List<int> { 1 });

//            var expected = new List<TaskDto>
//            {
//                new TaskDto
//                {
//                    ProjectId = 1,
//                    Id = 1,
//                    Status = TaskStatusType.InProgress.ToString(),
//                    Title = "test",
//                    CanBeProccessed = true,
//                    ParentTasks = new List<int>(),
//                    ChildTasks = new List<int>(),
//                    ConditionalParentTasks = new List<int>(),
//                    Resources = new List<ResourceDto>()
//                },
//                new TaskDto
//                {
//                    ProjectId = 1,
//                    Id = 2,
//                    Status = TaskStatusType.InProgress.ToString(),
//                    Title = "test",
//                    CanBeProccessed = true,
//                    ParentTasks = new List<int>(),
//                    ChildTasks = new List<int>(),
//                    ConditionalParentTasks = new List<int>(),
//                    Resources = new List<ResourceDto>()
//                },
//                new TaskDto
//                {
//                    ProjectId = 1,
//                    Id = 3,
//                    Status = TaskStatusType.NeedsReview.ToString(),
//                    Title = "test",
//                    CanBeReviewed = true,
//                    ParentTasks = new List<int>(),
//                    ChildTasks = new List<int>(),
//                    ConditionalParentTasks = new List<int>(),
//                    Resources = new List<ResourceDto>()
//                },
//                new TaskDto
//                {
//                    ProjectId = 1,
//                    Id = 4,
//                    Status = TaskStatusType.New.ToString(),
//                    Title = "test",
//                    ParentTasks = new List<int>(),
//                    ChildTasks = new List<int>(),
//                    ConditionalParentTasks = new List<int>(),
//                    Resources = new List<ResourceDto>()
//                },
//                new TaskDto
//                {
//                    ProjectId = 1,
//                    Id = 5,
//                    Status = TaskStatusType.Completed.ToString(),
//                    Title = "test",
//                    ParentTasks = new List<int>(),
//                    ChildTasks = new List<int>(),
//                    ConditionalParentTasks = new List<int>(),
//                    Resources = new List<ResourceDto>()
//                },
//                new TaskDto
//                {
//                    ProjectId = 1,
//                    Id = 8,
//                    Status = TaskStatusType.InProgress.ToString(),
//                    Title = "test",
//                    ProccessingUserName = "Ivan Ivanov",
//                    ProccessingUserId = new Guid(),
//                    ParentTasks = new List<int>(),
//                    ChildTasks = new List<int>(),
//                    ConditionalParentTasks = new List<int>(),
//                    Resources = new List<ResourceDto>()
//                },
//                new TaskDto
//                {
//                    ProjectId = 1,
//                    Id = 9,
//                    Status = TaskStatusType.NeedsReview.ToString(),
//                    Title = "test",
//                    ReviewingUserName = "Neil deGrasse",
//                    ReviewingUserId = new Guid(),
//                    ParentTasks = new List<int>(),
//                    ChildTasks = new List<int>(),
//                    ConditionalParentTasks = new List<int>(),
//                    Resources = new List<ResourceDto>()
//                }
//            };

//            AssertEx.EqualSerialization(expected, actual);
//        }

//        private void SetupMocksAndTestDataForGetTasksAsync()
//        {
//            var projects = new List<Project>
//            {
//                new Project
//                {
//                    Id = 1,
//                    OwnerGuid = UserId
//                },
//                new Project
//                {
//                    Id = 2
//                }
//            };

//            var UserSkills = new List<UserSkill>
//            {
//                new UserSkill
//                {
//                    UserId = UserId,
//                    Skill = new Skill
//                    {
//                        AssignedTasks = new List<ProjectTask>
//                        {
//                            new ProjectTask
//                            {
//                                ProjectId = 1,
//                                Id = 1,
//                                Status = TaskStatusType.InProgress
//                            },
//                            new ProjectTask
//                            {
//                                ProjectId = 1,
//                                Id = 2,
//                                Status = TaskStatusType.InProgress
//                            }
//                        }
//                    }
//                },
//                new UserSkill
//                {
//                    UserId = UserId,
//                    Skill = new Skill
//                    {
//                        ReviewingTasks = new List<ProjectTask>
//                        {
//                            new ProjectTask
//                            {
//                                ProjectId = 1,
//                                Id = 3,
//                                Status = TaskStatusType.NeedsReview
//                            }
//                        }
//                    }
//                },
//                new UserSkill
//                {
//                    UserId = UserId,
//                    Skill = new Skill
//                    {
//                        AssignedTasks = new List<ProjectTask>
//                        {
//                            new ProjectTask
//                            {
//                                ProjectId = 1,
//                                Id = 5,
//                                Status = TaskStatusType.Completed
//                            }
//                        }
//                    }
//                }
//            };

//            var projectTasks = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 1,
//                    Status = TaskStatusType.InProgress
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 2,
//                    Status = TaskStatusType.InProgress
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 3,
//                    Status = TaskStatusType.NeedsReview
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 4,
//                    Status = TaskStatusType.New
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 5,
//                    Status = TaskStatusType.Completed
//                },
//                new ProjectTask
//                {
//                    ProjectId = 2,
//                    Id = 6,
//                    Status = TaskStatusType.Completed
//                },
//                new ProjectTask
//                {
//                    ProjectId = 3,
//                    Id = 7,
//                    Status = TaskStatusType.New
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 8,
//                    Status = TaskStatusType.InProgress,
//                    ProccessingUserGuid = UserId,
//                    ProccessingUser = new User
//                    {
//                        Profile = new UserProfile
//                        {
//                            FullName = "Ivan Ivanov"
//                        }
//                    }
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 9,
//                    Status = TaskStatusType.NeedsReview,
//                    ReviewingUserGuid = UserId,
//                    ReviewingUser = new User
//                    {
//                        Profile = new UserProfile
//                        {
//                            FullName = "Neil deGrasse"
//                        }
//                    }
//                }
//            };

//            _repoMock.SetupRepoMock(projects, _transactionScopeMock);
//            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);
//            _repoMock.SetupRepoMock(UserSkills, _transactionScopeMock);
//        }
//        private void SetupMocksAndTestDataFoVendorGetTasksAsync()
//        {
//            var projects = new List<Project>
//            {
//                new Project
//                {
//                    Id = 1
//                },
//                new Project
//                {
//                    Id = 2
//                }
//            };

//            var vendors = new List<ProjectTaskVendor>
//            {
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 1,
//                    Status = ProjectRequestStatus.Accepted,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.Completed },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 2,
//                    Status = ProjectRequestStatus.Accepted,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.InProgress },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 3,
//                    Status = ProjectRequestStatus.Accepted,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.NeedsReview },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 4,
//                    Status = ProjectRequestStatus.Accepted,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.New },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 5,
//                    Status = ProjectRequestStatus.AcceptedByOther,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.Completed },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 6,
//                    Status = ProjectRequestStatus.AcceptedByOther,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.InProgress },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 7,
//                    Status = ProjectRequestStatus.AcceptedByOther,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.NeedsReview },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 8,
//                    Status = ProjectRequestStatus.AcceptedByOther,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.New },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 9,
//                    Status = ProjectRequestStatus.Declined,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.Completed },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 10,
//                    Status = ProjectRequestStatus.Declined,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.InProgress },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 11,
//                    Status = ProjectRequestStatus.Declined,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.NeedsReview },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 12,
//                    Status = ProjectRequestStatus.Declined,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.New },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 13,
//                    Status = ProjectRequestStatus.DeclinedByOwner,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.Completed },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 14,
//                    Status = ProjectRequestStatus.DeclinedByOwner,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.InProgress },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 15,
//                    Status = ProjectRequestStatus.DeclinedByOwner,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.NeedsReview },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 16,
//                    Status = ProjectRequestStatus.DeclinedByOwner,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.New },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 17,
//                    Status = ProjectRequestStatus.None,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.Completed },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 18,
//                    Status = ProjectRequestStatus.None,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.InProgress },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 19,
//                    Status = ProjectRequestStatus.None,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.NeedsReview },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 20,
//                    Status = ProjectRequestStatus.None,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.New },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 21,
//                    Status = ProjectRequestStatus.Send,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.Completed },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 22,
//                    Status = ProjectRequestStatus.Send,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.InProgress },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 23,
//                    Status = ProjectRequestStatus.Send,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.NeedsReview },
//                    VendorGuid = UserId
//                },
//                new ProjectTaskVendor
//                {
//                    ProjectTaskId = 24,
//                    Status = ProjectRequestStatus.Send,
//                    ProjectTask = new ProjectTask{ ProjectId = 1, Status = TaskStatusType.New },
//                    VendorGuid = UserId
//                }
//            };

//            var projectTasks = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 1,
//                    Status = TaskStatusType.Completed
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 2,
//                    Status = TaskStatusType.InProgress
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 3,
//                    Status = TaskStatusType.NeedsReview
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 4,
//                    Status = TaskStatusType.New
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 5,
//                    Status = TaskStatusType.Completed
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 6,
//                    Status = TaskStatusType.InProgress
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 7,
//                    Status = TaskStatusType.NeedsReview
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 8,
//                    Status = TaskStatusType.New
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 9,
//                    Status = TaskStatusType.Completed
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 10,
//                    Status = TaskStatusType.InProgress
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 11,
//                    Status = TaskStatusType.NeedsReview
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 12,
//                    Status = TaskStatusType.New
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 13,
//                    Status = TaskStatusType.Completed
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 14,
//                    Status = TaskStatusType.InProgress
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 15,
//                    Status = TaskStatusType.NeedsReview
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 16,
//                    Status = TaskStatusType.New
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 17,
//                    Status = TaskStatusType.Completed
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 18,
//                    Status = TaskStatusType.InProgress
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 19,
//                    Status = TaskStatusType.NeedsReview
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 20,
//                    Status = TaskStatusType.New
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 21,
//                    Status = TaskStatusType.Completed
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 22,
//                    Status = TaskStatusType.InProgress
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 23,
//                    Status = TaskStatusType.NeedsReview
//                },
//                new ProjectTask
//                {
//                    ProjectId = 1,
//                    Id = 24,
//                    Status = TaskStatusType.New
//                }
//            };

//            _repoMock.SetupRepoMock(projects, _transactionScopeMock);
//            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);
//            _repoMock.SetupRepoMock(vendors, _transactionScopeMock);
//            _repoMock.SetupRepoMock<UserSkill>(_transactionScopeMock);
//        }


//        [Fact(Skip = "Need to add ProjectTo mocking")]
//        public async Task GetTaskAsync_UserAsVendor_CorrectDataReturned()
//        {
//            SetupMocksAndTestDataFoVendorGetTasksAsync();
//            _principalMock.Setup(t => t.IsInRole(UserRoles.Vendor)).Returns(true);
//            var actual = await _unit.GetTasksAsync(new List<int> { 1 });

//            var expected = new List<TaskDto>
//            {
//                new TaskDto
//                {
//                    ProjectId = 1,
//                    Id = 1,
//                    Status = TaskStatusType.Completed.ToString(),
//                    Title = "test",
//                    ParentTasks = new List<int>(),
//                    ChildTasks = new List<int>(),
//                    ConditionalParentTasks = new List<int>(),
//                    Resources = new List<ResourceDto>()
//                },
//                new TaskDto
//                {
//                    ProjectId = 1,
//                    Id = 2,
//                    Status = TaskStatusType.InProgress.ToString(),
//                    Title = "test",
//                    ParentTasks = new List<int>(),
//                    ChildTasks = new List<int>(),
//                    ConditionalParentTasks = new List<int>(),
//                    Resources = new List<ResourceDto>()
//                },
//                new TaskDto
//                {
//                    ProjectId = 1,
//                    Id = 3,
//                    Status = TaskStatusType.NeedsReview.ToString(),
//                    Title = "test",
//                    ParentTasks = new List<int>(),
//                    ChildTasks = new List<int>(),
//                    ConditionalParentTasks = new List<int>(),
//                    Resources = new List<ResourceDto>()
//                },
//                new TaskDto
//                {
//                    ProjectId = 1,
//                    Id = 20,
//                    Status = TaskStatusType.New.ToString(),
//                    Title = "test",
//                    ParentTasks = new List<int>(),
//                    ChildTasks = new List<int>(),
//                    ConditionalParentTasks = new List<int>(),
//                    Resources = new List<ResourceDto>()
//                }
//            };

//            AssertEx.EqualSerialization(expected, actual);
//        }

//        #endregion

//        #region GetTaskAsync (Task<TaskDto> overload) test
//        [Fact]
//        public async Task GetTaskAsync_ShouldReturnCorrectGeneralData()
//        {
//            _repoMock.SetupRepoMock<ResourceProject>();
//            _repoMock.SetupRepoMock(TestTaskData);
//            _repoMock.SetupSingleValueRepoMock(new UserSkill
//            {
//                UserId = UserId,
//                Skill = new Skill
//                {
//                    Id = 1,
//                    Name = "test Skill",
//                    AssignedTasks = new List<ProjectTask>()
//                }
//            });
//            _repoMock.SetupRepoMock<ResourceProject>();
//            _taskNeo4JRepositoryMock
//                .Setup(s => s.GetTaskAndSharedResourcesAsync(1, 1))
//                .ReturnsAsync(new List<TaskResourceNeo4jDto>());

//            var actual = await _unit.GetTaskAsync(1);

//            var expected = new TaskDto
//            {
//                Id = 1,
//                ProjectId = 1,
//                Title = "test",
//                Status = TaskStatusType.New.ToString(),
//                CanBeReviewed = true,
//                CanBeProccessed = true,
//                ParentTasks = new List<int>(),
//                ChildTasks = new List<int>(),
//                ConditionalParentTasks = new List<int>(),
//                Resources = new List<ResourceDto>()
//            };

//            AssertEx.EqualSerialization(expected, actual);
//        }

//        [Fact]
//        public async Task GetTaskAsync_CanBeProccessed_ShouldBeTrue()
//        {
//            _repoMock.SetupRepoMock<ResourceProject>();
//            _repoMock.SetupRepoMock(TestTaskData);
//            _repoMock.SetupSingleValueRepoMock(new ProjectTaskUser
//            {
//                UserId = UserId,
//                ProjectTaskUserType = ProjectTaskUserType.Assigned,
//                ProjectTaskId = 1
//            });

//            _taskNeo4JRepositoryMock
//                .Setup(s => s.GetTaskAndSharedResourcesAsync(1, 1))
//                .ReturnsAsync(new List<TaskResourceNeo4jDto>());

//            var actual = await _unit.GetTaskAsync(1);

//            var expected = new TaskDto
//            {
//                Id = 1,
//                ProjectId = 1,
//                Title = "test",
//                CanBeProccessed = true,
//                CanBeReviewed = true,
//                Status = TaskStatusType.New.ToString(),
//                ParentTasks = new List<int>(),
//                ChildTasks = new List<int>(),
//                ConditionalParentTasks = new List<int>(),
//                Resources = new List<ResourceDto>()
//            };

//            AssertEx.EqualSerialization(expected, actual);
//        }

//        [Fact]
//        public async Task GetTaskAsync_CanBeReviewed_ShouldBeTrue()
//        {
//            _repoMock.SetupRepoMock<ResourceProject>();
//            _repoMock.SetupRepoMock(TestTaskData);

//            _taskNeo4JRepositoryMock
//                .Setup(s => s.GetTaskAndSharedResourcesAsync(1, 1))
//                .ReturnsAsync(new List<TaskResourceNeo4jDto>());

//            var actual = await _unit.GetTaskAsync(1);

//            var expected = new TaskDto
//            {
//                Id = 1,
//                ProjectId = 1,
//                Title = "test",
//                CanBeProccessed = true,
//                CanBeReviewed = true,
//                Status = TaskStatusType.New.ToString(),
//                ParentTasks = new List<int>(),
//                ChildTasks = new List<int>(),
//                ConditionalParentTasks = new List<int>(),
//                Resources = new List<ResourceDto>()
//            };

//            AssertEx.EqualSerialization(expected, actual);
//        }

//        [Fact]
//        public async Task GetTaskAsync_ShouldMapResources()
//        {
//            _repoMock.SetupRepoMock<ResourceProject>();
//            _repoMock.SetupRepoMock<UserSkill>();
//            _repoMock.SetupRepoMock(TestTaskData);

//            _storageServiceMock
//                .Setup(s => s.GetFileUri(It.IsAny<string>(), StorageType.ProjectTask))
//                .Returns("kabachok");

//            _taskNeo4JRepositoryMock
//                .Setup(s => s.GetTaskAndSharedResourcesAsync(1, 1))
//                .ReturnsAsync(new List<TaskResourceNeo4jDto>
//                {
//                        new TaskResourceNeo4jDto { Type = (byte) ResourceType.File },
//                        new TaskResourceNeo4jDto { Type = (byte) ResourceType.Link },
//                        new TaskResourceNeo4jDto { Type = (byte) ResourceType.File }
//                });

//            var actual = await _unit.GetTaskAsync(1);

//            var expected = new TaskDto
//            {
//                Id = 1,
//                ProjectId = 1,
//                Title = "test",
//                Status = TaskStatusType.New.ToString(),
//                CanBeReviewed = true,
//                CanBeProccessed = true,
//                Resources = new List<ResourceDto>
//                    {
//                        new ResourceDto
//                        {
//                            Type = ResourceType.File,
//                            Url = "kabachok"
//                        },
//                        new ResourceDto
//                        {
//                            Type = ResourceType.Link
//                        },
//                        new ResourceDto
//                        {
//                            Type = ResourceType.File,
//                            Url = "kabachok"
//                        }
//                    },
//                ParentTasks = new List<int>(),
//                ChildTasks = new List<int>(),
//                ConditionalParentTasks = new List<int>()
//            };

//            AssertEx.EqualSerialization(expected, actual);
//        }

//        [Fact]
//        public async Task GetTaskAsync_ShouldMapOnlyFileResources()
//        {
//            _repoMock.SetupRepoMock<ResourceProject>();
//            _repoMock.SetupRepoMock<UserSkill>();
//            _repoMock.SetupRepoMock(TestTaskData);

//            _taskNeo4JRepositoryMock
//                .Setup(s => s.GetTaskAndSharedResourcesAsync(1, 1))
//                .ReturnsAsync(new List<TaskResourceNeo4jDto>
//                {
//                        new TaskResourceNeo4jDto { Type = (byte) ResourceType.File },
//                        new TaskResourceNeo4jDto { Type = (byte) ResourceType.Link },
//                        new TaskResourceNeo4jDto { Type = (byte) ResourceType.File }
//                });

//            var _ = await _unit.GetTaskAsync(1);
//            _storageServiceMock.Verify(s => s.GetFileUri(It.IsAny<string>(), StorageType.ProjectTask), Times.Exactly(2));
//        }

//        [Fact]
//        public async Task GetTaskAsync_ShouldMapProjectResources()
//        {
//            _repoMock.SetupRepoMock(new List<ResourceProject> {
//                new ResourceProject
//                {
//                    ProjectId = 1,
//                    Resource = new Resource
//                    {
//                        Type = ResourceType.File,
//                        Path = "path"
//                    }
//                },
//                new ResourceProject
//                {
//                    ProjectId = 1,
//                    Resource = new Resource
//                    {
//                        Type = ResourceType.Link
//                    }
//                }});
//            _repoMock.SetupRepoMock<UserSkill>();
//            _repoMock.SetupRepoMock(TestTaskData);

//            _storageServiceMock
//                .Setup(s => s.GetFileUri("path", StorageType.Project))
//                .Returns("kabachok");

//            _taskNeo4JRepositoryMock
//                .Setup(s => s.GetTaskAndSharedResourcesAsync(1, 1))
//                .ReturnsAsync(new List<TaskResourceNeo4jDto>());

//            var actual = await _unit.GetTaskAsync(1);

//            var expected = new TaskDto
//            {
//                Id = 1,
//                ProjectId = 1,
//                Title = "test",
//                Status = TaskStatusType.New.ToString(),
//                CanBeReviewed = true,
//                CanBeProccessed = true,
//                Resources = new List<ResourceDto>
//                    {
//                        new ResourceDto
//                        {
//                            Type = ResourceType.File,
//                            Url = "kabachok",
//                            IsSharedFromParent = true
//                        },
//                        new ResourceDto
//                        {
//                            Type = ResourceType.Link,
//                            IsSharedFromParent = true
//                        }
//                    },
//                ParentTasks = new List<int>(),
//                ChildTasks = new List<int>(),
//                ConditionalParentTasks = new List<int>()
//            };

//            AssertEx.EqualSerialization(expected, actual);
//        }

//        [Fact]
//        public async Task GetTaskAsync_ShouldMapOnlyProjectFileResources()
//        {
//            _repoMock.SetupRepoMock(new List<ResourceProject> {
//                new ResourceProject
//                {
//                    ProjectId = 1,
//                    Resource = new Resource
//                    {
//                        Type = ResourceType.File,
//                        Path = "path"
//                    }
//                },
//                new ResourceProject
//                {
//                    ProjectId = 1,
//                    Resource = new Resource
//                    {
//                        Type = ResourceType.Link
//                    }
//                }});
//            _repoMock.SetupRepoMock<UserSkill>();
//            _repoMock.SetupRepoMock(TestTaskData);

//            _taskNeo4JRepositoryMock
//                .Setup(s => s.GetTaskAndSharedResourcesAsync(1, 1))
//                .ReturnsAsync(new List<TaskResourceNeo4jDto>());

//            var _ = await _unit.GetTaskAsync(1);
//            _storageServiceMock.Verify(s => s.GetFileUri("path", StorageType.Project), Times.Once);
//        }
//        #endregion

//        #region DoTask test
//        [Fact]
//        public async Task DoTask_ShouldThrow_WhenNoTaskFound()
//        {
//            _repoMock.SetupRepoMock<UserSkill>();
//            _repoMock.SetupRepoMock(TestTaskData);

//            await Assert.ThrowsAsync<ForbiddenException>(() => _unit.DoTask(10));
//        }

//        [Theory]
//        [InlineData(TaskStatusType.NeedsReview)]
//        [InlineData(TaskStatusType.New)]
//        [InlineData(TaskStatusType.Completed)]
//        public async Task DoTask_ShouldThrow_WhenTaskNotInProgress(TaskStatusType status)
//        {
//            _repoMock.SetupSingleValueRepoMock(new ProjectTask
//            {
//                Id = 1,
//                ProjectId = 1,
//                Title = "test",
//                Status = status,
//                ProjectTaskUsers = new List<ProjectTaskUser>
//                {
//                    new ProjectTaskUser
//                    {
//                        UserId = UserId,
//                        ProjectTaskUserType = ProjectTaskUserType.Assigned
//                    }
//                }
//            });

//            await Assert.ThrowsAsync<ValidationException>(() => _unit.DoTask(1));
//        }

//        [Fact]
//        public async Task DoTask_ShouldSetProcessingUser_WhenDataCorrect()
//        {
//            var task = new ProjectTask
//            {
//                Id = 1,
//                ProjectId = 1,
//                Title = "test",
//                Status = TaskStatusType.InProgress,
//                ProjectTaskUsers = new List<ProjectTaskUser>
//                {
//                    new ProjectTaskUser
//                    {
//                        UserId = UserId,
//                        ProjectTaskUserType = ProjectTaskUserType.Assigned
//                    }
//                }
//            };

//            _repoMock.SetupRepoMock<ResourceProject>();
//            _repoMock.SetupSingleValueRepoMock(task);
//            _repoMock.SetupSingleValueRepoMock(new User
//            {
//                Id = UserId,
//                Profile = new UserProfile
//                {
//                    FullName = "tset"
//                }
//            });

//            _taskNeo4JRepositoryMock
//                .Setup(s => s.GetTaskAndSharedResourcesAsync(1, 1))
//                .ReturnsAsync(new List<TaskResourceNeo4jDto>());

//            var actual = await _unit.DoTask(1);

//            Assert.Equal("tset", actual.ProccessingUserName);
//        }

//        [Fact]
//        public async Task DoTask_ShouldWriteHistory_WhenDataCorrect()
//        {
//            var task = new ProjectTask
//            {
//                Id = 1,
//                ProjectId = 1,
//                Title = "test",
//                Status = TaskStatusType.InProgress,
//                ProjectTaskUsers = new List<ProjectTaskUser>
//                {
//                    new ProjectTaskUser
//                    {
//                        UserId = UserId,
//                        ProjectTaskUserType = ProjectTaskUserType.Assigned
//                    }
//                }
//            };

//            _repoMock.SetupRepoMock<ResourceProject>();
//            _repoMock.SetupSingleValueRepoMock(task);
//            _repoMock.SetupSingleValueRepoMock(new User
//            {
//                Id = UserId,
//                Profile = new UserProfile()
//            });

//            _taskNeo4JRepositoryMock
//                .Setup(s => s.GetTaskAndSharedResourcesAsync(1, 1))
//                .ReturnsAsync(new List<TaskResourceNeo4jDto>());

//            await _unit.DoTask(1);

//            _taskHistoryServiceMock.Verify(s => s.Write(1, ActivityType.Processing, null, null, true), Times.Once);
//        }
//        #endregion

//        #region ReviewTask test
//        [Fact]
//        public async Task ReviewTask_NotExistingIdAsParam_ExeptionThrown()
//        {
//            _repoMock.SetupRepoMock<UserSkill>();
//            _repoMock.SetupRepoMock(TestTaskData);

//            await Assert.ThrowsAsync<ForbiddenException>(() => _unit.ReviewTask(10));
//        }

//        [Theory]
//        [InlineData(TaskStatusType.InProgress)]
//        [InlineData(TaskStatusType.New)]
//        [InlineData(TaskStatusType.Completed)]
//        public async Task ReviewTask_TaskNotInStatusNeedsReview_ExeptionThrown(TaskStatusType status)
//        {
//            _repoMock.SetupSingleValueRepoMock(new ProjectTask
//            {
//                Id = 1,
//                ProjectId = 1,
//                Title = "test",
//                Status = status
//            });

//            _repoMock.SetupRepoMock<ResourceProject>();

//            await Assert.ThrowsAsync<ValidationException>(() => _unit.ReviewTask(1));
//        }

//        [Fact]
//        public async Task ReviewTask_CorrectData_WriteHistory()
//        {
//            var newTask = new ProjectTask
//            {
//                Id = 1,
//                ProjectId = 1,
//                Title = "test",
//                Status = TaskStatusType.NeedsReview,
//                ProjectTaskUsers = new List<ProjectTaskUser>
//                {
//                    new ProjectTaskUser
//                    {
//                        UserId = UserId,
//                        ProjectTaskUserType = ProjectTaskUserType.Reviewing
//                    }
//                }
//            };

//            _repoMock.SetupRepoMock<ResourceProject>();
//            _repoMock.SetupSingleValueRepoMock(newTask);
//            _repoMock.SetupSingleValueRepoMock(new UserSkill
//            {
//                UserId = UserId,
//                Skill = new Skill
//                {
//                    ReviewingTasks = new List<ProjectTask> { newTask }
//                }
//            });
//            _repoMock.SetupSingleValueRepoMock(new User
//            {
//                Id = UserId,
//                Profile = new UserProfile()
//            });

//            _taskNeo4JRepositoryMock
//                .Setup(s => s.GetTaskAndSharedResourcesAsync(1, 1))
//                .ReturnsAsync(new List<TaskResourceNeo4jDto>());

//            await _unit.ReviewTask(1);

//            _taskHistoryServiceMock.Verify(s => s.Write(1, ActivityType.Reviewing, null, null, true), Times.Once);
//        }
//        #endregion

//        #region UpdateTaskStatus test
//        [Fact]
//        public async Task UpdateTaskStatusAsync_IncorrectIdAsParam_ExceptionThrown()
//        {
//            _repoMock.SetupRepoMock(TestTaskData, _transactionScopeMock);
//            await Assert.ThrowsAsync<ValidationException>(() => _unit.UpdateTaskStatusAsync(2, TaskStatusType.Completed));
//        }

//        [Fact]
//        public async Task UpdateTaskStatusAsync_ExistingTask_ChangeTaskStatusCorrectCall()
//        {
//            _repoMock.SetupRepoMock(TestTaskData, _transactionScopeMock);

//            await _unit.UpdateTaskStatusAsync(1, TaskStatusType.Completed);
//            _taskScheduleServiceMock.Verify(t => t.ChangeTaskStatus(_transactionScopeMock.Object,
//                It.Is<ProjectTask>(pt => pt.ProjectId == 1), TaskStatusType.Completed, null), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateTaskStatusAsync_ExistingTask_TaskStatusChangedCorrectCall()
//        {
//            _repoMock.SetupRepoMock(TestTaskData, _transactionScopeMock);

//            var status = TaskStatusType.Completed;
//            await _unit.UpdateTaskStatusAsync(1, status);
//            _taskSocketStoreMock.Verify(t => t.TaskStatusChanged(1, 1, status), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateTaskStatusAsync_ExistingTask_SaveAndCommitAsyncCorrectCalling()
//        {
//            _repoMock.SetupRepoMock(TestTaskData, _transactionScopeMock);

//            await _unit.UpdateTaskStatusAsync(1, TaskStatusType.Completed);
//            _transactionScopeMock.Verify(t => t.SaveAndCommitAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateTaskStatusAsync_ExistingTask_ReturnCorrectData()
//        {
//            _repoMock.SetupRepoMock(TestTaskData, _transactionScopeMock);

//            _taskScheduleServiceMock.Setup(t => t.ChangeTaskStatus(_transactionScopeMock.Object,
//                It.Is<ProjectTask>(pt => pt.Id == 1), TaskStatusType.Completed, null))
//                .Callback((ITransactionScope transactionScope, ProjectTask projectTask, TaskStatusType taskStatusType, int? selectedConditionOptionId) =>
//                    projectTask.Status = taskStatusType)
//                .Returns(Task.CompletedTask);

//            var actual = await _unit.UpdateTaskStatusAsync(1, TaskStatusType.Completed);
//            var expected = new TaskDto
//            {
//                Title = "test",
//                Id = 1,
//                ProjectId = 1,
//                Status = TaskStatusType.Completed.ToString(),
//                ParentTasks = new List<int>(),
//                ChildTasks = new List<int>(),
//                ConditionalParentTasks = new List<int>(),
//                Resources = new List<ResourceDto>()
//            };
//            AssertEx.EqualSerialization(expected, actual);
//        }
//        #endregion

//        #region UpdateTaskPositionAsync test
//        [Fact]
//        public async Task UpdateTaskPositionAsync_EmptyListAsParam_ExceptionThrown()
//        {
//            await Assert.ThrowsAsync<ValidationException>(() => _unit.UpdateTasksPositionAsync(new List<TaskPositionDto>()));
//        }

//        [Fact]
//        public async Task UpdateTaskPositionAsync_NullAsParam_ExceptionThrown()
//        {
//            await Assert.ThrowsAsync<ValidationException>(() => _unit.UpdateTasksPositionAsync(null));
//        }

//        [Fact]
//        public async Task UpdateTaskPositionAsync_ListWithIncrorrectIdsAsParam_SaveAndCommitAsyncNotCalled()
//        {
//            SetupRepoForUpdateTaskPosition();

//            var tasksToUpdade = new List<TaskPositionDto>
//            {
//                new TaskPositionDto
//                {
//                    Id = 5
//                },
//                new TaskPositionDto
//                {
//                    Id = 6
//                },
//                new TaskPositionDto
//                {
//                    Id = 7
//                }
//            };

//            await _unit.UpdateTasksPositionAsync(tasksToUpdade);
//            _transactionScopeMock.Verify(t => t.SaveAndCommitAsync(CancellationToken.None), Times.Never);
//        }

//        [Fact]
//        public async Task UpdateTaskPositionAsync_ListWithCorrectIdsAsParam_SaveAndCommitAsyncCorrectCalling()
//        {
//            SetupRepoForUpdateTaskPosition();

//            var tasksToUpdade = new List<TaskPositionDto>
//            {
//                new TaskPositionDto
//                {
//                    Id = 1
//                },
//                new TaskPositionDto
//                {
//                    Id = 2
//                },
//                new TaskPositionDto
//                {
//                    Id = 3
//                }
//            };

//            await _unit.UpdateTasksPositionAsync(tasksToUpdade);
//            _transactionScopeMock.Verify(t => t.SaveAndCommitAsync(CancellationToken.None), Times.Once);
//        }

//        private void SetupRepoForUpdateTaskPosition()
//        {
//            _repoMock.SetupRepoMock(new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ProjectId = 1,
//                    Title = "test",
//                    Status = TaskStatusType.Completed
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ProjectId = 1,
//                    Title = "test1",
//                    Status = TaskStatusType.NeedsReview
//                },
//                new ProjectTask
//                {
//                    Id = 3,
//                    ProjectId = 1,
//                    Title = "test2",
//                    Status = TaskStatusType.New
//                }
//            }, _transactionScopeMock);
//        }
//        #endregion

//        #region AddTaskDependency test
//        [Fact]
//        public async Task AddTaskDependency_TasksWithDifferentParentTask_ExceptionThrown()
//        {
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ProjectId = 1,
//                    Title = "test",
//                    Status = TaskStatusType.Completed,
//                    ParentTaskId = 4
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ProjectId = 1,
//                    Title = "test1",
//                    Status = TaskStatusType.NeedsReview,
//                    ParentTaskId = 5
//                }
//            }, _transactionScopeMock);

//            var taskDependencyDto = new TaskDependencyDto
//            {
//                ChildTaskId = 1,
//                ParentTaskId = 2
//            };

//            await Assert.ThrowsAsync<ValidationException>(() => _unit.AddTaskDependency(taskDependencyDto));
//        }

//        [Fact]
//        public async Task AddTaskDependency_CorrectData_SaveChangesAsyncCalling()
//        {
//            SetupTestDataForAddTaskDependency();

//            await _unit.AddTaskDependency(new TaskDependencyDto
//            {
//                ChildTaskId = 1,
//                ParentTaskId = 2
//            });

//            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Exactly(2));
//        }

//        [Fact]
//        public async Task AddTaskDependency_CorrectData_AddTaskDependencyAsyncCorrectCalling()
//        {
//            SetupTestDataForAddTaskDependency();

//            await _unit.AddTaskDependency(new TaskDependencyDto
//            {
//                ChildTaskId = 1,
//                ParentTaskId = 2
//            });

//            _taskNeo4JRepositoryMock.Verify(t => t.AddTaskDependencyAsync(2, 1), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskDependency_CorrectData_IsRootAsyncCorrectCalling()
//        {
//            SetupTestDataForAddTaskDependency();

//            await _unit.AddTaskDependency(new TaskDependencyDto
//            {
//                ChildTaskId = 1,
//                ParentTaskId = 2
//            });

//            _taskNeo4JRepositoryMock.Verify(t => t.IsRootAsync(1), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskDependency_CorrectData_UpdateFormulaTaskTimeCorrectCalling()
//        {
//            SetupTestDataForAddTaskDependency();

//            await _unit.AddTaskDependency(new TaskDependencyDto
//            {
//                ChildTaskId = 1,
//                ParentTaskId = 2
//            });

//            _formulaTaskJobServiceMock.Verify(t => t.UpdateFormulaTaskTime(_transactionScopeMock.Object, It.Is<ProjectTask>(pt => pt.Id == 1)), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskDependency_CorrectData_UpdateStartDatesForTreeIfNeededCorrectCalling()
//        {
//            SetupTestDataForAddTaskDependency();

//            await _unit.AddTaskDependency(new TaskDependencyDto
//            {
//                ChildTaskId = 1,
//                ParentTaskId = 2
//            });

//            _startDateHelperServiceMock.Verify(t => t.UpdateStartDatesForTreeIfNeeded(_transactionScopeMock.Object,
//                It.Is<ProjectTask>(pt => pt.Id == 2),
//                It.Is<ProjectTask>(pt => pt.Id == 1)), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskDependency_CorrectData_RemoveFormulaTaskJobsCorrectCalling()
//        {
//            SetupTestDataForAddTaskDependency();

//            await _unit.AddTaskDependency(new TaskDependencyDto
//            {
//                ChildTaskId = 1,
//                ParentTaskId = 2
//            });

//            _formulaTaskJobServiceMock.Verify(t => t.RemoveFormulaTaskJobs(_transactionScopeMock.Object, It.Is<ProjectTask>(pt => pt.Id == 1), false), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskDependency_CorrectData_RemoveJobCorrectCalling()
//        {
//            SetupTestDataForAddTaskDependency();

//            await _unit.AddTaskDependency(new TaskDependencyDto
//            {
//                ChildTaskId = 1,
//                ParentTaskId = 2
//            });

//            _scheduleServiceMock.Verify(t => t.RemoveJob(_transactionScopeMock.Object, 1), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskDependency_CorrectData_SaveAndCommitAsyncCorrectCalling()
//        {
//            SetupTestDataForAddTaskDependency();

//            await _unit.AddTaskDependency(new TaskDependencyDto
//            {
//                ChildTaskId = 1,
//                ParentTaskId = 2
//            });

//            _transactionScopeMock.Verify(t => t.SaveAndCommitAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskDependency_CorrectData_CommitCorrectCalling()
//        {
//            SetupTestDataForAddTaskDependency();

//            await _unit.AddTaskDependency(new TaskDependencyDto
//            {
//                ChildTaskId = 1,
//                ParentTaskId = 2
//            });

//            _transactionMock.Verify(t => t.Commit(), Times.Once);
//        }

//        private List<ProjectTaskDependency> GetTransactionStorageForAdding()
//        {
//            var storage = new List<ProjectTaskDependency>();
//            _transactionScopeMock.Setup(s => s.AddAsync(It.IsAny<ProjectTaskDependency>()))
//                .Callback((ProjectTaskDependency dependency) => { storage.Add(dependency); })
//                .Returns(Task.CompletedTask);
//            return storage;
//        }

//        private void SetupTestDataForAddTaskDependency()
//        {
//            var testData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ProjectId = 1,
//                    Title = "test",
//                    ParentTaskId = 3,
//                    FormulaId = 5
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ProjectId = 1,
//                    Title = "test2",
//                    ParentTaskId = 3
//                }
//            };

//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(testData, _transactionScopeMock);
//            _repoMock.SetupRepoMock(GetTransactionStorageForAdding(), _transactionScopeMock);
//        }
//        #endregion

//        #region CompleteConditionalTask test
//        [Fact]
//        public async Task CompleteConditionalTask_NotExistingIdAsParam_ExcepthionThrown()
//        {
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(TestTaskData, _transactionScopeMock);

//            await Assert.ThrowsAsync<ValidationException>(() => _unit.CompleteConditionalTask(3, new ConditionOptionDto()));
//        }

//        [Fact]
//        public async Task CompleteConditionTask_ValidData_SetTaskConditionSelectedAsyncCalled()
//        {
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(GetDataForCompleteConditionTaskTest(), _transactionScopeMock);

//            await _unit.CompleteConditionalTask(1, new ConditionOptionDto
//            {
//                Status = TaskStatusType.Completed,
//                ConditionOptionId = 1
//            });

//            _taskNeo4JRepositoryMock.Verify(t => t.SetTaskConditionSelectedAsync(1, 2), Times.Once);
//            _taskNeo4JRepositoryMock.Verify(t => t.DeselectTaskConditionsAsync(1), Times.Once);
//        }

//        [Fact]
//        public async Task CompleteConditionTask_ValidData_ChangeTaskStatusCalled()
//        {
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(GetDataForCompleteConditionTaskTest(), _transactionScopeMock);

//            await _unit.CompleteConditionalTask(1, new ConditionOptionDto
//            {
//                Status = TaskStatusType.Completed,
//                ConditionOptionId = 1
//            });

//            _taskScheduleServiceMock.Verify(t => t.ChangeTaskStatus(_transactionScopeMock.Object,
//                It.Is<ProjectTask>(pt => pt.Id == 1), It.IsAny<TaskStatusType>(), 1), Times.Once);
//        }

//        [Fact]
//        public async Task CompleteConditionTask_ValidData_SaveAndCommitAsyncCalled()
//        {
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(GetDataForCompleteConditionTaskTest(), _transactionScopeMock);

//            await _unit.CompleteConditionalTask(1, new ConditionOptionDto
//            {
//                Status = TaskStatusType.Completed,
//                ConditionOptionId = 1
//            });

//            _transactionScopeMock.Verify(t => t.SaveAndCommitAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task CompleteConditionTask_ValidData_CommitCalled()
//        {
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(GetDataForCompleteConditionTaskTest(), _transactionScopeMock);

//            await _unit.CompleteConditionalTask(1, new ConditionOptionDto
//            {
//                Status = TaskStatusType.Completed,
//                ConditionOptionId = 1
//            });

//            _transactionMock.Verify(t => t.Commit(), Times.Once);
//        }

//        private static List<ProjectTask> GetDataForCompleteConditionTaskTest()
//        {
//            return new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ProjectId = 1,
//                    Title = "condition",
//                    Status = TaskStatusType.InProgress,
//                    Condition = new ProjectTaskCondition
//                    {
//                        Id = 1,
//                        Condition = "condition",
//                        Options = new List<ProjectTaskConditionOption>
//                        {
//                            new ProjectTaskConditionOption
//                            {
//                                Id = 1,
//                                AssignedTaskId = 2,
//                                IsSelected = true
//                            },
//                            new ProjectTaskConditionOption
//                            {
//                                Id = 2,
//                                IsSelected = false
//                            }
//                        }
//                    }
//                }
//            };
//        }
//        #endregion

//        #region RemoveTaskDependency test
//        [Fact]
//        public async Task RemoveTaskDependency_NotExistDependencyAsParam_ExceptionThrown()
//        {
//            var testData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ProjectId = 1,
//                    Title = "test"
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ProjectId = 1,
//                    Title = "test2"
//                }
//            };

//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(testData, _transactionScopeMock);
//            _repoMock.SetupRepoMock(new List<ProjectTaskDependency>
//            {
//                new ProjectTaskDependency
//                {
//                    ParentTaskId = 1,
//                    ChildTaskId = 2
//                }
//            }, _transactionScopeMock);

//            await Assert.ThrowsAsync<ValidationException>(() => _unit.RemoveTaskDependency(new TaskDependencyDto
//            {
//                ChildTaskId = 3,
//                ParentTaskId = 4
//            }));
//        }

//        [Fact]
//        public async Task RemoveTaskDependency_ExistedDependency_SaveChangesAsyncCorrectCalling()
//        {
//            PrepareMocksAndTestDataForRemoveTaskDependency(false, false, false);

//            await _unit.RemoveTaskDependency(new TaskDependencyDto
//            {
//                ParentTaskId = 1,
//                ChildTaskId = 2
//            });

//            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task RemoveTaskDependency_ExistedDependency_SaveAndCommitAsyncCorrectCalling()
//        {
//            PrepareMocksAndTestDataForRemoveTaskDependency(false, false, false);

//            await _unit.RemoveTaskDependency(new TaskDependencyDto
//            {
//                ParentTaskId = 1,
//                ChildTaskId = 2
//            });

//            _transactionScopeMock.Verify(t => t.SaveAndCommitAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task RemoveTaskDependency_ExistedDependency_CommitCorrectCalling()
//        {
//            PrepareMocksAndTestDataForRemoveTaskDependency(false, false, false);

//            await _unit.RemoveTaskDependency(new TaskDependencyDto
//            {
//                ParentTaskId = 1,
//                ChildTaskId = 2
//            });

//            _transactionMock.Verify(t => t.Commit(), Times.Once);
//        }

//        [Fact]
//        public async Task RemoveTaskDependency_ExistedDependency_RemoveTaskDependencyAsyncCorrectCalling()
//        {
//            PrepareMocksAndTestDataForRemoveTaskDependency(false, false, false);

//            await _unit.RemoveTaskDependency(new TaskDependencyDto
//            {
//                ParentTaskId = 1,
//                ChildTaskId = 2
//            });

//            _taskNeo4JRepositoryMock.Verify(t => t.RemoveTaskDependencyAsync(1, 2), Times.Once);
//        }


//        private void PrepareMocksAndTestDataForRemoveTaskDependency(bool hasRelationReturnValue, bool isGraphCompletedReturnValue, bool isRootAsyncReturnValue)
//        {
//            var testData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ProjectId = 1,
//                    Title = "test"
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ProjectId = 1,
//                    Title = "test2"
//                }
//            };

//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _taskNeo4JRepositoryMock.Setup(s => s.HasRelationsAsync(It.IsAny<int>(), It.IsAny<int>()))
//                .ReturnsAsync(hasRelationReturnValue);
//            _taskNeo4JRepositoryMock.Setup(s => s.IsGraphCompleted(It.IsAny<int>()))
//                .ReturnsAsync(isGraphCompletedReturnValue);
//            _taskNeo4JRepositoryMock.Setup(s => s.IsRootAsync(It.IsAny<int>())).ReturnsAsync(isRootAsyncReturnValue);
//            _repoMock.SetupRepoMock(testData, _transactionScopeMock);
//            _repoMock.SetupRepoMock(new List<ProjectTaskDependency>
//            {
//                new ProjectTaskDependency
//                {
//                    ParentTask = new ProjectTask
//                    {
//                        Id = 3,
//                        Status = TaskStatusType.Completed
//                    },
//                    ChildTask = new ProjectTask
//                    {
//                        Id = 4,
//                        Status = TaskStatusType.New,
//                        FormulaId = 5
//                    },
//                    ParentTaskId = 1,
//                    ChildTaskId = 2
//                }
//            }, _transactionScopeMock);
//        }
//        #endregion

//        #region AssignTaskToConditionOption test
//        [Fact]
//        public async Task AssignTaskToConditionOption_NotExistConditionOptionIdAsParam_ExceptionThrown()
//        {
//            var testData = new List<ProjectTaskConditionOption>
//            {
//                new ProjectTaskConditionOption
//                {
//                    Id = 1
//                }
//            };
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(testData, _transactionScopeMock);

//            await Assert.ThrowsAsync<ValidationException>(() => _unit.AssignTaskToConditionOption(2, null));
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionIdAsParam_SaveChangesAsyncCorrectCalling()
//        {
//            var testData = new List<ProjectTaskConditionOption>
//            {
//                new ProjectTaskConditionOption
//                {
//                    Id = 1,
//                    AssignedTask = new ProjectTask
//                    {
//                        Id = 1
//                    },
//                    AssignedTaskId = 1,
//                    Condition = new ProjectTaskCondition
//                    {
//                        Task = new ProjectTask
//                        {
//                            Id = 2,
//                            Status = TaskStatusType.NeedsReview
//                        }
//                    }
//                }
//            };
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(testData, _transactionScopeMock);

//            await _unit.AssignTaskToConditionOption(1, null);
//            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionIdAndTasksWithDifferentParent_ExceptionThrown()
//        {
//            var conditionOptionsTestData = new List<ProjectTaskConditionOption>
//            {
//                new ProjectTaskConditionOption
//                {
//                    Id = 1,
//                    AssignedTask = new ProjectTask
//                    {
//                        Id = 2
//                    },
//                    AssignedTaskId = 1,
//                    Condition = new ProjectTaskCondition
//                    {
//                        Task = new ProjectTask
//                        {
//                            Id = 1,
//                            Status = TaskStatusType.NeedsReview
//                        }
//                    }
//                }
//            };

//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ParentTaskId = 3
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ParentTaskId = 4
//                }
//            };

//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(conditionOptionsTestData, _transactionScopeMock);
//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);

//            await Assert.ThrowsAsync<ValidationException>(() => _unit.AssignTaskToConditionOption(1, 2));
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionId_IsRootAsyncCorrectCalling()
//        {
//            SetupTestDataForAssignTaskToConditionOption();

//            await _unit.AssignTaskToConditionOption(1, 2);
//            _taskNeo4JRepositoryMock.Verify(t => t.IsRootAsync(2), Times.Once);
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionId_AddTaskConditionAsyncCorrectCalling()
//        {
//            SetupTestDataForAssignTaskToConditionOption();

//            await _unit.AssignTaskToConditionOption(1, 2);
//            _taskNeo4JRepositoryMock.Verify(t => t.AddTaskConditionAsync(1, 1, 2), Times.Once);
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionId_UpdateFormulaTaskTimeCorrectCalling()
//        {
//            SetupTestDataForAssignTaskToConditionOption();

//            await _unit.AssignTaskToConditionOption(1, 2);
//            _formulaTaskJobServiceMock.Verify(t => t.UpdateFormulaTaskTime(_transactionScopeMock.Object, It.Is<ProjectTask>(pt => pt.Id == 2)), Times.Once);
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionId_UpdateStartDatesForTreeIfNeededCorrectCalling()
//        {
//            SetupTestDataForAssignTaskToConditionOption();

//            await _unit.AssignTaskToConditionOption(1, 2);
//            _startDateHelperServiceMock.Verify(t => t.UpdateStartDatesForTreeIfNeeded(_transactionScopeMock.Object,
//                It.Is<ProjectTask>(pt => pt.Id == 1),
//                It.Is<ProjectTask>(pt => pt.Id == 2)), Times.Once);
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionId_RemoveFormulaTaskJobsCorrectCalling()
//        {
//            SetupTestDataForAssignTaskToConditionOption();

//            await _unit.AssignTaskToConditionOption(1, 2);
//            _formulaTaskJobServiceMock.Verify(t => t.RemoveFormulaTaskJobs(_transactionScopeMock.Object,
//                It.Is<ProjectTask>(pt => pt.Id == 2),
//                false), Times.Once);
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionId_RemoveTaskConditionAsyncCorrectCalling()
//        {
//            SetupTestDataForAssignTaskToConditionOption();

//            await _unit.AssignTaskToConditionOption(1, null);
//            _taskNeo4JRepositoryMock.Verify(t => t.RemoveTaskConditionAsync(1, 2), Times.Once);
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionId_TryResetProjectTaskTreeStatusesCorrectCalling()
//        {
//            SetupTestDataForAssignTaskToConditionOption();

//            await _unit.AssignTaskToConditionOption(1, null);
//            _formulaTaskJobServiceMock.Verify(t => t.TryResetProjectTaskTreeStatuses(TaskStatusType.NeedsReview, 1, 2), Times.Once);
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionId_ScheduleTaskJobsCorrectCalling()
//        {
//            SetupTestDataForAssignTaskToConditionOption();

//            await _unit.AssignTaskToConditionOption(1, null);
//            _formulaTaskJobServiceMock.Verify(t => t.ScheduleTaskJobs(It.Is<ProjectTask>(pt => pt.Id == 2), _transactionScopeMock.Object), Times.Once);
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionId_SaveAndCommitAsyncCorrectCalling()
//        {
//            SetupTestDataForAssignTaskToConditionOption();

//            await _unit.AssignTaskToConditionOption(1, null);
//            _transactionScopeMock.Verify(t => t.SaveAndCommitAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task AssignTaskToConditionOption_ExistedConditionOptionId_CommitCorrectCalling()
//        {
//            SetupTestDataForAssignTaskToConditionOption();

//            await _unit.AssignTaskToConditionOption(1, null);
//            _transactionMock.Verify(t => t.Commit(), Times.Once);
//        }

//        private void SetupTestDataForAssignTaskToConditionOption()
//        {
//            var conditionOptionsTestData = new List<ProjectTaskConditionOption>
//            {
//                new ProjectTaskConditionOption
//                {
//                    Id = 1,
//                    AssignedTask = new ProjectTask
//                    {
//                        Id = 2
//                    },
//                    AssignedTaskId = 2,
//                    Condition = new ProjectTaskCondition
//                    {
//                        Task = new ProjectTask
//                        {
//                            Id = 1,
//                            Status = TaskStatusType.NeedsReview
//                        }
//                    }
//                }
//            };

//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ParentTaskId = 3
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ParentTaskId = 3,
//                    FormulaId = 4
//                }
//            };

//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(conditionOptionsTestData, _transactionScopeMock);
//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//        }
//        #endregion

//        #region DeleteTasks test

//        [Fact]
//        public async Task DeleteTasks_IncorrectTaskIds_ExceptionThrown()
//        {
//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ParentTaskId = 3
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ParentTaskId = 3,
//                    FormulaId = 4
//                }
//            };

//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//            await Assert.ThrowsAsync<ValidationException>(() => _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 4, 5, 6 }));
//        }

//        [Fact]
//        public async Task DeleteTasks_ValidTaskIds_RemoveRangeForResourceProjectTaskCalled()
//        {
//            var resourceProjectTasks = SetupTestDataForDeleteTasks();

//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });
//            _transactionScopeMock.Verify(t => t.RemoveRange(resourceProjectTasks), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTasks_ValidTaskIds_SaveChangesAsyncCalled()
//        {
//            SetupTestDataForDeleteTasks();

//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });
//            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Exactly(2));
//        }

//        [Fact]
//        public async Task DeleteTasks_ValidTaskIds_RemoveRangeForResourceCalled()
//        {
//            var resourceProjectTasks = SetupTestDataForDeleteTasks();

//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });

//            var resources = resourceProjectTasks.Select(t => t.Resource);
//            _transactionScopeMock.Verify(t => t.RemoveRange(resources), Times.Once);
//        }

//        [Theory]
//        [InlineData(1)]
//        [InlineData(2)]
//        public async Task DeleteTasks_ValidTaskIds_DeleteTaskAsyncCalled(int taskId)
//        {
//            SetupTestDataForDeleteTasks();

//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });
//            _taskNeo4JRepositoryMock.Verify(t => t.DeleteTaskAsync(taskId), Times.Once);
//        }

//        [Theory]
//        [InlineData(1)]
//        [InlineData(2)]
//        public async Task DeleteTasks_ValidTaskIds_DeleteFileAsyncCalled(int taskId)
//        {
//            SetupTestDataForDeleteTasks();

//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });
//            _storageServiceMock.Verify(t => t.DeleteFileAsync(taskId.ToString(), StorageType.ProjectTask), Times.Once);
//        }

//        [Theory]
//        [InlineData(1)]
//        [InlineData(2)]
//        public async Task DeleteTasks_ValidTaskIds_RemoveJobCalled(int taskId)
//        {
//            SetupTestDataForDeleteTasks();

//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });
//            _scheduleServiceMock.Verify(t => t.RemoveJob(_transactionScopeMock.Object, taskId), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTasks_ValidTaskIds_RemoveRangeForNotificationSettingsCalled()
//        {
//            var notificationSettings = new List<NotificationSetting>
//            {
//                new NotificationSetting
//                {
//                    Id = 1
//                },
//                new NotificationSetting
//                {
//                    Id = 2
//                }
//            };

//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ParentTaskId = 3,
//                    NotificationSettings = notificationSettings
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ParentTaskId = 3
//                }
//            };
//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });

//            _transactionScopeMock.Verify(t => t.RemoveRange(notificationSettings), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTasks_ValidTaskIds_RemoveRangeForChildTasksCalled()
//        {

//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ChildTasks = new List<ProjectTaskDependency>
//                    {
//                        new ProjectTaskDependency
//                        {
//                            ChildTaskId = 2,
//                            ParentTaskId = 3
//                        },
//                        new ProjectTaskDependency
//                        {
//                            ChildTaskId = 4,
//                            ParentTaskId = 1
//                        }
//                    }
//                },
//                new ProjectTask
//                {
//                    Id = 2
//                },
//                new ProjectTask
//                {
//                    Id = 3,
//                    ChildTasks = new List<ProjectTaskDependency>
//                    {
//                        new ProjectTaskDependency
//                        {
//                            ChildTaskId = 1,
//                            ParentTaskId = 2
//                        },
//                        new ProjectTaskDependency
//                        {
//                            ChildTaskId = 3,
//                            ParentTaskId = 4
//                        }
//                    }
//                },
//                new ProjectTask
//                {
//                    Id = 4
//                }
//            };
//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2, 3, 4 });

//            var childTasks = projectTaskTestData.SelectMany(t => t.ChildTasks);
//            _transactionScopeMock.Verify(t => t.RemoveRange(childTasks), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTasks_ValidTaskIds_RemoveRangeForParentTasksCalled()
//        {

//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ParentTasks = new List<ProjectTaskDependency>
//                    {
//                        new ProjectTaskDependency
//                        {
//                            ChildTaskId = 2,
//                            ParentTaskId = 3
//                        },
//                        new ProjectTaskDependency
//                        {
//                            ChildTaskId = 4,
//                            ParentTaskId = 1
//                        }
//                    }
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ParentTasks = new List<ProjectTaskDependency>
//                    {
//                        new ProjectTaskDependency
//                        {
//                            ChildTaskId = 1,
//                            ParentTaskId = 2
//                        },
//                        new ProjectTaskDependency
//                        {
//                            ChildTaskId = 3,
//                            ParentTaskId = 4
//                        }
//                    }
//                }
//            };
//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });

//            var parentTasks = projectTaskTestData.SelectMany(t => t.ParentTasks);
//            _transactionScopeMock.Verify(t => t.RemoveRange(parentTasks), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTasks_ValidTaskIds_RemoveRangeForTaskHistoriesCalled()
//        {

//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    TaskHistories = new List<TaskHistory>
//                    {
//                        new TaskHistory
//                        {
//                            Id = 1
//                        },
//                        new TaskHistory
//                        {
//                            Id = 2
//                        }
//                    }
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    TaskHistories = new List<TaskHistory>
//                    {
//                        new TaskHistory
//                        {
//                            Id = 3
//                        },
//                        new TaskHistory
//                        {
//                            Id = 4
//                        }
//                    }
//                }
//            };
//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });

//            var taskHistories = projectTaskTestData.SelectMany(t => t.TaskHistories);
//            _transactionScopeMock.Verify(t => t.RemoveRange(taskHistories), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTasks_ValidTaskIds_RemoveRangeForAssignedConditionOptionsCalled()
//        {

//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    AssignedConditionOptions = new List<ProjectTaskConditionOption>
//                    {
//                        new ProjectTaskConditionOption
//                        {
//                            Id = 1
//                        },
//                        new ProjectTaskConditionOption
//                        {
//                            Id = 2
//                        }
//                    }
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    AssignedConditionOptions = new List<ProjectTaskConditionOption>
//                    {
//                        new ProjectTaskConditionOption
//                        {
//                            Id = 3
//                        },
//                        new ProjectTaskConditionOption
//                        {
//                            Id = 4
//                        }
//                    }
//                }
//            };
//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });

//            var assignedConditionOptions = projectTaskTestData.SelectMany(t => t.AssignedConditionOptions);
//            _transactionScopeMock.Verify(t => t.RemoveRange(assignedConditionOptions), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTasks_ValidTaskIds_RemoveRangeForAssignedRecurrenceOptionsCalled()
//        {

//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    RecurrenceOptionsId = 12,
//                    RecurrenceOptions = new RecurrenceOptions
//                    {
//                        Id = 1
//                    }
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    RecurrenceOptionsId = 13,
//                    RecurrenceOptions = new RecurrenceOptions
//                    {
//                        Id = 2
//                    }
//                }
//            };
//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });

//            var recurrenceOptions = projectTaskTestData.Select(t => t.RecurrenceOptions);
//            _transactionScopeMock.Verify(t => t.RemoveRange(recurrenceOptions), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTasks_ValidTaskIds_RemoveRangeForConditionsCalled()
//        {

//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    TaskConditionId = 12,
//                    Condition = new ProjectTaskCondition
//                    {
//                        Id = 1
//                    }
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    TaskConditionId = 13,
//                    Condition = new ProjectTaskCondition
//                    {
//                        Id = 2
//                    }
//                }
//            };
//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//            await _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 1, 2 });

//            var conditions = projectTaskTestData.Select(t => t.Condition);
//            _transactionScopeMock.Verify(t => t.RemoveRange(conditions), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTasks_ValidTaskIds_DeleteTasksCalled()
//        {
//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    FormulaId = 1
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    FormulaId = 2,
//                },
//                new ProjectTask
//                {
//                    Id = 3
//                },
//                new ProjectTask
//                {
//                    Id = 4,
//                    ParentTaskId = 2
//                }
//            };
//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);

//            await Assert.ThrowsAsync<ValidationException>(() => _unit.DeleteTasks(_transactionScopeMock.Object, new List<int> { 5 }));
//        }

//        private List<ResourceProjectTask> SetupTestDataForDeleteTasks()
//        {
//            var resourceProjectTasks = new List<ResourceProjectTask>
//            {
//                new ResourceProjectTask
//                {
//                    ProjectTaskId = 1,
//                    Resource = new Resource
//                    {
//                        Id = 1
//                    }
//                },
//                new ResourceProjectTask
//                {
//                    ProjectTaskId = 1,
//                    Resource = new Resource
//                    {
//                        Id = 2
//                    }
//                },
//                new ResourceProjectTask
//                {
//                    ProjectTaskId = 1,
//                    Resource = new Resource
//                    {
//                        Id = 3
//                    }
//                }
//            };

//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ParentTaskId = 3,
//                    ResourceProjectTask = resourceProjectTasks
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ParentTaskId = 3
//                }
//            };
//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//            return resourceProjectTasks;
//        }
//        #endregion

//        #region DeleteTask test
//        [Fact]
//        public async Task DeleteTask_IncorrectIdAsParam_ExceptionThrown()
//        {
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ProjectId = 1,
//                    Title = "test",
//                    Status = TaskStatusType.Completed,
//                    ParentTaskId = 4
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ProjectId = 1,
//                    Title = "test1",
//                    Status = TaskStatusType.NeedsReview,
//                    ParentTaskId = 5
//                }
//            }, _transactionScopeMock);

//            await Assert.ThrowsAsync<ValidationException>(() => _unit.DeleteTask(3));
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_GetParentTaskIdsAsyncCalled()
//        {
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ProjectId = 1,
//                    Title = "test",
//                    Status = TaskStatusType.Completed,
//                    ParentTaskId = 4
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ProjectId = 1,
//                    Title = "test1",
//                    Status = TaskStatusType.NeedsReview,
//                    ParentTaskId = 5
//                }
//            }, _transactionScopeMock);

//            await _unit.DeleteTask(1);
//            _taskNeo4JRepositoryMock.Verify(t => t.GetParentTaskIdsAsync(1), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_GetChildTaskIdsAsyncCalled()
//        {
//            SetupDataForDeleteTask();

//            await _unit.DeleteTask(1);
//            _taskNeo4JRepositoryMock.Verify(t => t.GetChildTaskIdsAsync(1), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_DeleteTaskAsyncCalled()
//        {
//            SetupDataForDeleteTask();

//            await _unit.DeleteTask(1);
//            _taskNeo4JRepositoryMock.Verify(t => t.DeleteTaskAsync(1), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_SaveChangesAsyncCalled()
//        {
//            SetupDataForDeleteTask();

//            await _unit.DeleteTask(1);
//            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_RemoveRangeForResourceProjectTaskCalled()
//        {
//            var recourceProjectTasks = new List<ResourceProjectTask>
//            {
//                new ResourceProjectTask
//                {
//                    ProjectTaskId = 1
//                },
//                new ResourceProjectTask
//                {
//                    ProjectTaskId = 2
//                }
//            };
//            var projectTask = new ProjectTask
//            {
//                Id = 3,
//                ProjectId = 2,
//                Title = "test2",
//                Status = TaskStatusType.New,
//                ParentTaskId = 4,
//                ResourceProjectTask = recourceProjectTasks
//            };
//            SetupDataForDeleteTask(projectTask);

//            await _unit.DeleteTask(3);
//            _transactionScopeMock.Verify(t => t.RemoveRange(recourceProjectTasks), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_RemoveRangeForResourcesCalled()
//        {
//            var recourceProjectTasks = new List<ResourceProjectTask>
//            {
//                new ResourceProjectTask
//                {
//                    ProjectTaskId = 1,
//                    Resource = new Resource
//                    {
//                        Id = 1
//                    }
//                },
//                new ResourceProjectTask
//                {
//                    ProjectTaskId = 2,
//                    Resource = new Resource
//                    {
//                        Id = 1
//                    }
//                }
//            };

//            var projectTask = new ProjectTask
//            {
//                Id = 3,
//                ProjectId = 2,
//                Title = "test2",
//                Status = TaskStatusType.New,
//                ParentTaskId = 4,
//                ResourceProjectTask = recourceProjectTasks
//            };
//            SetupDataForDeleteTask(projectTask);

//            var resources = recourceProjectTasks.Select(s => s.Resource);
//            await _unit.DeleteTask(3);
//            _transactionScopeMock.Verify(t => t.RemoveRange(resources), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_DeleteFileAsyncCalled()
//        {
//            SetupDataForDeleteTask();

//            await _unit.DeleteTask(1);
//            _storageServiceMock.Verify(t => t.DeleteFileAsync("1", StorageType.ProjectTask), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_RemoveRangeForNotificationSettingsCalled()
//        {
//            var notificationSettings = new List<NotificationSetting>
//            {
//                new NotificationSetting
//                {
//                    Id = 1
//                },
//                new NotificationSetting
//                {
//                    Id = 2
//                }
//            };

//            var projectTask = new ProjectTask
//            {
//                Id = 3,
//                ProjectId = 2,
//                Title = "test2",
//                Status = TaskStatusType.New,
//                ParentTaskId = 4,
//                NotificationSettings = notificationSettings
//            };
//            SetupDataForDeleteTask(projectTask);

//            await _unit.DeleteTask(3);
//            _transactionScopeMock.Verify(t => t.RemoveRange(notificationSettings), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_RemoveJobCalled()
//        {
//            SetupDataForDeleteTask();

//            await _unit.DeleteTask(1);
//            _scheduleServiceMock.Verify(t => t.RemoveJob(_transactionScopeMock.Object, 1), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_RemoveRangeForChildTasksCalled()
//        {
//            var projectTaskDependencies = new List<ProjectTaskDependency>
//            {
//                new ProjectTaskDependency
//                {
//                    ParentTaskId = 2
//                },
//                new ProjectTaskDependency
//                {
//                    ParentTaskId = 3
//                }
//            };

//            var projectTask = new ProjectTask
//            {
//                Id = 3,
//                ProjectId = 2,
//                Title = "test2",
//                Status = TaskStatusType.New,
//                ParentTaskId = 4,
//                ChildTasks = projectTaskDependencies
//            };
//            SetupDataForDeleteTask(projectTask);

//            await _unit.DeleteTask(3);
//            _transactionScopeMock.Verify(t => t.RemoveRange(projectTaskDependencies), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_RemoveRangeForParentTasksCalled()
//        {
//            var projectTaskDependencies = new List<ProjectTaskDependency>
//            {
//                new ProjectTaskDependency
//                {
//                    ParentTaskId = 2
//                },
//                new ProjectTaskDependency
//                {
//                    ParentTaskId = 3
//                }
//            };

//            var projectTask = new ProjectTask
//            {
//                Id = 3,
//                ProjectId = 2,
//                Title = "test2",
//                Status = TaskStatusType.New,
//                ParentTaskId = 4,
//                ParentTasks = projectTaskDependencies
//            };
//            SetupDataForDeleteTask(projectTask);

//            await _unit.DeleteTask(3);
//            _transactionScopeMock.Verify(t => t.RemoveRange(projectTaskDependencies), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_RemoveForRecurrenceOptionsCalled()
//        {
//            var recurrenceOptions = new RecurrenceOptions
//            {
//                Id = 32
//            };

//            var projectTask = new ProjectTask
//            {
//                Id = 3,
//                ProjectId = 2,
//                Title = "test2",
//                Status = TaskStatusType.New,
//                ParentTaskId = 4,
//                RecurrenceOptions = recurrenceOptions
//            };
//            SetupDataForDeleteTask(projectTask);

//            await _unit.DeleteTask(3);
//            _transactionScopeMock.Verify(t => t.Remove(recurrenceOptions), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParam_RemoveForConditionCalled()
//        {
//            var condition = new ProjectTaskCondition
//            {
//                Id = 21
//            };

//            var projectTask = new ProjectTask
//            {
//                Id = 3,
//                ProjectId = 2,
//                Title = "test2",
//                Status = TaskStatusType.New,
//                ParentTaskId = 4,
//                Condition = condition
//            };
//            SetupDataForDeleteTask(projectTask);

//            await _unit.DeleteTask(3);
//            _transactionScopeMock.Verify(t => t.Remove(condition), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParamTaskWithParents_HasRelationsAsyncCalled()
//        {
//            var condition = new ProjectTaskCondition
//            {
//                Id = 21
//            };

//            var projectTask = new ProjectTask
//            {
//                Id = 3,
//                ProjectId = 2,
//                Title = "test2",
//                Status = TaskStatusType.New,
//                ParentTaskId = 4,
//                Condition = condition
//            };
//            SetupDataForDeleteTask(projectTask);
//            var parentTaskIds = new List<int> { 6, 7, 8 };
//            _taskNeo4JRepositoryMock.Setup(t => t.GetParentTaskIdsAsync(3)).ReturnsAsync(parentTaskIds);

//            await _unit.DeleteTask(3);
//            _taskNeo4JRepositoryMock.Verify(t => t.HasRelationsAsync(6, It.Is<int>(id => parentTaskIds.Contains(id))), Times.AtLeastOnce);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParamTaskWithParents_SaveAndCommitAsyncCalled()
//        {
//            var condition = new ProjectTaskCondition
//            {
//                Id = 21
//            };

//            var projectTask = new ProjectTask
//            {
//                Id = 3,
//                ProjectId = 2,
//                Title = "test2",
//                Status = TaskStatusType.New,
//                ParentTaskId = 4,
//                Condition = condition
//            };
//            SetupDataForDeleteTask(projectTask);

//            await _unit.DeleteTask(3);
//            _transactionScopeMock.Verify(t => t.SaveAndCommitAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteTask_ExistedIdAsParamTaskWithParents_CommitCalled()
//        {
//            var condition = new ProjectTaskCondition
//            {
//                Id = 21
//            };

//            var projectTask = new ProjectTask
//            {
//                Id = 3,
//                ProjectId = 2,
//                Title = "test2",
//                Status = TaskStatusType.New,
//                ParentTaskId = 4,
//                Condition = condition
//            };
//            SetupDataForDeleteTask(projectTask);

//            await _unit.DeleteTask(3);
//            _transactionMock.Verify(t => t.Commit(), Times.Once);
//        }

//        private void SetupDataForDeleteTask(ProjectTask projectTask = null)
//        {
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            var projectTasks = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ProjectId = 1,
//                    Title = "test",
//                    Status = TaskStatusType.Completed,
//                    ParentTaskId = 4
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ProjectId = 1,
//                    Title = "test1",
//                    Status = TaskStatusType.NeedsReview,
//                    ParentTaskId = 5
//                }
//            };
//            if (projectTask != null)
//            {
//                projectTasks.Add(projectTask);
//            }
//            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);
//        }
//        #endregion

//        #region UpdateTaskAsync test
//        [Fact]
//        public async Task UpdateTaskAsync_NotExistedIdAsParam_ExceptionThrown()
//        {
//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ParentTaskId = 3
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ParentTaskId = 3,
//                    FormulaId = 4
//                }
//            };

//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);

//            await Assert.ThrowsAsync<ValidationException>(() => _unit.UpdateTaskAsync(8, new UpdateTaskDto()));
//        }

//        [Fact]
//        public async Task UpdateTaskAsync_ExistedTaskCorrectSkill_GetNextOccurenceCalled()
//        {
//            SetupMocksAndInitialDataForUpdateTaskAsync();

//            await _unit.UpdateTaskAsync(1, new UpdateTaskDto
//            {
//                AssignedUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0aa")
//                },
//                ReviewingUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0aa")
//                },
//                IsRecurrent = true
//            });
//            _taskScheduleServiceMock.Verify(t => t.GetNextOccurence(_transactionScopeMock.Object, 1,
//                It.IsAny<RecurrenceOptions>()), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateTaskAsync_ExistedTaskCorrectSkill_SaveChangesAsyncCalled()
//        {
//            SetupMocksAndInitialDataForUpdateTaskAsync();

//            await _unit.UpdateTaskAsync(1, new UpdateTaskDto
//            {
//                AssignedUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0aa")
//                },
//                ReviewingUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0aa")
//                },
//                IsRecurrent = true
//            });
//            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateTaskAsync_ExistedTaskCorrectSkill_SaveAndCommitAsyncCalled()
//        {
//            SetupMocksAndInitialDataForUpdateTaskAsync();

//            await _unit.UpdateTaskAsync(1, new UpdateTaskDto
//            {
//                AssignedUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0aa")
//                },
//                ReviewingUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0aa")
//                },
//                IsRecurrent = true
//            });
//            _transactionScopeMock.Verify(t => t.SaveAndCommitAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateTaskAsync_ExistedTaskCorrectSkill_CommitCalled()
//        {
//            SetupMocksAndInitialDataForUpdateTaskAsync();

//            await _unit.UpdateTaskAsync(1, new UpdateTaskDto
//            {
//                AssignedUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0aa")
//                },
//                ReviewingUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0aa")
//                },
//                IsRecurrent = true
//            });
//            _transactionMock.Verify(t => t.Commit(), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateTaskAsync_ExistedTaskCorrectSkill_UpdateTaskTreeCalled()
//        {
//            SetupMocksAndInitialDataForUpdateTaskAsync();

//            var updateModel = new UpdateTaskDto
//            {
//                AssignedUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0aa")
//                },
//                ReviewingUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0aa")
//                },
//                IsRecurrent = true
//            };
//            await _unit.UpdateTaskAsync(1, updateModel);
//            _taskScheduleServiceMock.Verify(t => t.UpdateTaskTree(_transactionScopeMock.Object,
//                It.Is<ProjectTask>(pt => pt.Id == 1),
//                It.Is<ProjectTask>(pt => pt.Id == 1),
//                updateModel,
//                new List<int>()), Times.Once);
//        }

//        private void SetupMocksAndInitialDataForUpdateTaskAsync()
//        {
//            var UserSkills = new List<UserSkill>
//            {
//                new UserSkill
//                {
//                    SkillId = 1,
//                    User = new User
//                    {
//                        Roles = new List<UserRole>
//                        {
//                            new UserRole
//                            {
//                                Role = new Role
//                                {
//                                    Name = UserRoles.Manager
//                                }
//                            }
//                        }
//                    }
//                },
//                new UserSkill
//                {
//                    SkillId = 2
//                }
//            };

//            var projectTaskTestData = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 1,
//                    ParentTaskId = 3,
//                    ReviewingSkillId = 1,
//                    Status = TaskStatusType.New,
//                    RecurrenceOptions = new RecurrenceOptions
//                    {
//                        Cron = "cron"
//                    },
//                    ProjectId = 1,
//                    AssignedSkillId = 12,
//                    AssignedConditionOptions = new List<ProjectTaskConditionOption>()
//                },
//                new ProjectTask
//                {
//                    Id = 2,
//                    ParentTaskId = 3,
//                    FormulaId = 4
//                }
//            };

//            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
//            _repoMock.SetupRepoMock(UserSkills, _transactionScopeMock);
//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock<ResourceProject>();
//        }
//        #endregion

//        #region AddTaskAsync test

//        [Fact]
//        public async Task AddTaskAsync_NullAsParam_ExceptionThrown()
//        {
//            await Assert.ThrowsAsync<ArgumentException>(() => _unit.AddTaskAsync(null));
//        }

//        [Fact]
//        public async Task AddTaskAsync_CorrectDataPassed_GetNextOccurenceCalled()
//        {
//            SetupMockAndTesdDataForAddTaskAsync();
//            var dateTime = new DateTime(2007, 12, 11);
//            _dateTimeServiceMock.Setup(t => t.NowUtc).Returns(dateTime);
//            var recurrenceDetails = new RecurrenceAsapDto();
//            _dateTimeServiceMock.Setup(s => s.ParseRecurrenceAsap(It.IsAny<RecurrenceOptions>(), It.IsAny<DateTime>()))
//                .Returns(recurrenceDetails);
//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());
//            _dateTimeServiceMock.Verify(t => t.GetNextOccurence(recurrenceDetails), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskAsync_CorrectDataPassed_ParseRecurrenceAsapCalled()
//        {
//            SetupMockAndTesdDataForAddTaskAsync();
//            var dateTime = new DateTime(2007, 12, 11);
//            _dateTimeServiceMock.Setup(t => t.NowUtc).Returns(dateTime);
//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());
//            _dateTimeServiceMock.Verify(s => s.ParseRecurrenceAsap(It.IsAny<RecurrenceOptions>(), dateTime), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskAsync_CorrectDataPassed_SaveChangesAsyncCalled()
//        {
//            SetupMockAndTesdDataForAddTaskAsync();

//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());
//            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Exactly(2));
//        }

//        [Fact]
//        public async Task AddTaskAsync_CorrectDataPassed_AddTaskAsyncCalled()
//        {
//            SetupMockAndTesdDataForAddTaskAsync();

//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());
//            _taskNeo4JRepositoryMock.Verify(t => t.AddTaskAsync(It.IsAny<int>(), 2), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskAsync_ConditionAndTaskWithDifferentParent_ExceptionCalled()
//        {
//            SetupMockAndTesdDataForAddTaskAsync();

//            await Assert.ThrowsAsync<ValidationException>(() => _unit.AddTaskAsync(new UpdateTaskDto
//            {
//                IsRecurrent = true,
//                ProjectId = 2,
//                RecurrenceOptions = new RecurrenceOptionsDto
//                {
//                    Cron = "cron"
//                },
//                IsConditional = true,
//                Condition = new TaskConditionDto
//                {
//                    Options = new List<TaskConditionOptionDto>
//                    {
//                        new TaskConditionOptionDto
//                        {
//                            AssignedTaskId = 3
//                        }
//                    }
//                }
//            }));
//        }

//        [Fact]
//        public async Task AddTaskAsync_ConditionTaskWithAssignedTasks_RemoveJobCalled()
//        {
//            SetupMockAndTesdDataForAddTaskAsync();
//            var projectTasks = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 4
//                },
//                new ProjectTask
//                {
//                    Id = 5
//                }
//            };
//            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);

//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());

//            _scheduleServiceMock.Verify(t => t.RemoveJob(_transactionScopeMock.Object, 4), Times.Once);
//        }

//        [Theory]
//        [InlineData(15)]
//        [InlineData(16)]
//        public async Task AddTaskAsync_ConditionTaskWithAssignedTasks_RemoveJobForFormulaRootTaskCalled(int taskId)
//        {
//            SetupMockAndTesdDataForAddTaskAsync();
//            var projectTasks = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 4,
//                    FormulaId = 6
//                },
//                new ProjectTask
//                {
//                    Id = 5,
//                    FormulaId = 7
//                }
//            };
//            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);
//            _taskNeo4JRepositoryMock.Setup(t => t.IsRootAsync(It.IsAny<int>())).ReturnsAsync(true);
//            _taskNeo4JRepositoryMock.Setup(t => t.GetFormulaRootTaskIdsAsync(4)).ReturnsAsync(new List<int> { taskId });

//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());

//            _scheduleServiceMock.Verify(t => t.RemoveJob(_transactionScopeMock.Object, taskId), Times.Once);
//        }

//        [Theory]
//        [InlineData(22, 4)]
//        [InlineData(23, 5)]
//        public async Task AddTaskAsync_ConditionTaskWithAssignedTasks_AddTaskConditionAsyncCalled(int taskConditionId, int projectTaskId)
//        {
//            SetupMockAndTesdDataForAddTaskAsync();
//            var projectTasks = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 4
//                },
//                new ProjectTask
//                {
//                    Id = 5
//                }
//            };
//            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);

//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());

//            _taskNeo4JRepositoryMock.Verify(t => t.AddTaskConditionAsync(taskConditionId, It.IsAny<int>(), projectTaskId), Times.Once);
//        }

//        [Theory]
//        [InlineData(4)]
//        [InlineData(5)]
//        public async Task AddTaskAsync_ConditionTaskWithAssignedTasks_UpdateFormulaTaskTimeCalled(int projectTaskId)
//        {
//            SetupMockAndTesdDataForAddTaskAsync();
//            var projectTasks = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 4
//                },
//                new ProjectTask
//                {
//                    Id = 5
//                }
//            };
//            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);

//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());

//            _formulaTaskJobServiceMock.Verify(t => t.UpdateFormulaTaskTime(_transactionScopeMock.Object,
//                It.Is<ProjectTask>(pt => pt.Id == projectTaskId)), Times.Once);
//        }

//        [Theory]
//        [InlineData(4)]
//        [InlineData(5)]
//        public async Task AddTaskAsync_ConditionTaskWithAssignedTasks_UpdateStartDatesForTreeIfNeededCalled(int projectTaskId)
//        {
//            SetupMockAndTesdDataForAddTaskAsync();
//            var projectTasks = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 4
//                },
//                new ProjectTask
//                {
//                    Id = 5
//                }
//            };
//            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);

//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());

//            _startDateHelperServiceMock.Verify(t => t.UpdateStartDatesForTreeIfNeeded(_transactionScopeMock.Object,
//                It.Is<ProjectTask>(pt => pt.OwnerGuid == UserId),
//                It.Is<ProjectTask>(pt => pt.Id == projectTaskId)), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskAsync_CorrectDataPassed_ScheduleNewTaskCalled()
//        {
//            SetupMockAndTesdDataForAddTaskAsync();

//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());
//            _taskScheduleServiceMock.Verify(t => t.ScheduleNewTask(_transactionScopeMock.Object, It.Is<ProjectTask>(pt => pt.OwnerGuid == UserId), false), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskAsync_CorrectDataPassed_SaveAndCommitAsyncCalled()
//        {
//            SetupMockAndTesdDataForAddTaskAsync();

//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());
//            _transactionScopeMock.Verify(t => t.SaveAndCommitAsync(CancellationToken.None), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskAsync_CorrectDataPassed_CommitCalled()
//        {
//            SetupMockAndTesdDataForAddTaskAsync();

//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());
//            _transactionMock.Verify(t => t.Commit(), Times.Once);
//        }

//        [Fact]
//        public async Task AddTaskAsync_CorrectDataPassed_SendAssignToTaskAsyncCalled()
//        {
//            SetupMockAndTesdDataForAddTaskAsync();

//            await _unit.AddTaskAsync(GetUpdateTaskDtoModel());
//            _notificationServiceMock.Verify(t => t.SendAssignToTaskAsync(_transactionScopeMock.Object, It.IsAny<int>()));
//        }

//        [Fact]
//        public async Task AddTaskAsync_CorrectDataPassed_ReturnCorrectData()
//        {
//            SetupMockAndTesdDataForAddTaskAsync();

//            var expected = new TaskDto
//            {
//                Title = "test",
//                Id = 100,
//                ProjectId = 2,
//                Status = TaskStatusType.New.ToString(),
//                ParentTasks = new List<int>(),
//                ChildTasks = new List<int>(),
//                ConditionalParentTasks = new List<int>(),
//                Resources = new List<ResourceDto>()
//            };

//            var actual = await _unit.AddTaskAsync(GetUpdateTaskDtoModel());
//            AssertEx.EqualSerialization(expected, actual);
//        }

//        private UpdateTaskDto GetUpdateTaskDtoModel()
//        {
//            return new UpdateTaskDto
//            {
//                AssignedUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0bb")
//                },
//                ReviewingUserIds = new List<Guid>
//                {
//                    new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0aa")
//                },
//                IsRecurrent = true,
//                ProjectId = 2,
//                RecurrenceOptions = new RecurrenceOptionsDto
//                {
//                    Cron = "cron"
//                },
//                IsConditional = true,
//                Condition = new TaskConditionDto
//                {
//                    Options = new List<TaskConditionOptionDto>
//                    {
//                        new TaskConditionOptionDto
//                        {
//                            Id = 22,
//                            AssignedTaskId = 4
//                        },
//                        new TaskConditionOptionDto
//                        {
//                            Id = 23,
//                            AssignedTaskId = 5
//                        }
//                    }
//                }
//            };
//        }

//        private void SetupMockAndTesdDataForAddTaskAsync()
//        {
//            var UserSkills = new List<UserSkill>
//            {
//                new UserSkill
//                {
//                    SkillId = 1,
//                    User = new User
//                    {
//                        Roles = new List<UserRole>
//                        {
//                            new UserRole
//                            {
//                                Role = new Role
//                                {
//                                    Name = UserRoles.Worker
//                                }
//                            }
//                        }
//                    }
//                },
//                new UserSkill
//                {
//                    SkillId = 2,
//                    User = new User
//                    {
//                        Roles = new List<UserRole>
//                        {
//                            new UserRole
//                            {
//                                Role = new Role
//                                {
//                                    Name = UserRoles.Manager
//                                }
//                            }
//                        }
//                    }
//                }
//            };

//            var projectTasks = new List<ProjectTask>
//            {
//                new ProjectTask
//                {
//                    Id = 3,
//                    ParentTaskId = 5
//                }
//            };

//            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
//            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);
//            _repoMock.SetupRepoMock(UserSkills, _transactionScopeMock);
//            _transactionScopeMock.Setup(s => s.AddAsync(It.IsAny<ProjectTask>()))
//                .Callback((ProjectTask projectTask) => { projectTask.Id = 100; projectTasks.Add(projectTask); })
//                .Returns(Task.CompletedTask);
//        }
//        #endregion
//    }
//}
