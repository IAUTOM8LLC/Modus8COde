using System;
using System.Collections.Generic;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using IAutoM8.WebSockets.Stores.Interfaces;
using IAutoM8.Service.Scheduler;
using Moq;
using Xunit;
using System.Threading.Tasks;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Tests.Common;
using System.Linq;
using IAutoM8.Domain.Models;
using NCrontab.Advanced;
using NCrontab.Advanced.Enumerations;
using System.Globalization;
using IAutoM8.Global.Enums;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Service.Braintree.Interfaces;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.SendGrid.Interfaces;
using IAutoM8.Global.Options;
using Microsoft.Extensions.Options;
using IAutoM8.Domain.Models.User;
using IAutoM8.Domain.Models.Project;

namespace IAutoM8.Service.Tests.JobServiceTest
{
    public class JobServiceTests
    {
        private IJobService _unit;
        private readonly Mock<IRepo> _repoMock;
        private readonly Mock<Func<IScheduleService>> _scheduleServiceFactoryMock;
        private readonly Mock<IScheduleService> _scheduleServiceMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<ITaskNeo4jRepository> _taskNeo4JRepositoryMock;
        private readonly Mock<ITaskSocketStore> _taskSocketStoreMock;
        private readonly Mock<ICreditsService> _creditsServiceMock;
        private readonly Mock<Func<ITaskService>> _taskServiceMock;
        private readonly Mock<ISendGridService> _sendGridServiceMock;
        private readonly Mock<IOptions<EmailTemplates>> _emailTemplatesMock;
        private Mock<ITransactionScope> _transactionScopeMock;
        private Mock<ITransaction> _transactionMock;

        public JobServiceTests()
        {
            _repoMock = new Mock<IRepo>();
            _scheduleServiceFactoryMock = new Mock<Func<IScheduleService>>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _taskNeo4JRepositoryMock = new Mock<ITaskNeo4jRepository>();
            _taskSocketStoreMock = new Mock<ITaskSocketStore>();
            _transactionScopeMock = new Mock<ITransactionScope>();
            _transactionMock = new Mock<ITransaction>();
            _scheduleServiceMock = new Mock<IScheduleService>();
            _creditsServiceMock = new Mock<ICreditsService>();
            _taskServiceMock = new Mock<Func<ITaskService>>();
            _sendGridServiceMock = new Mock<ISendGridService>();
            _emailTemplatesMock = new Mock<IOptions<EmailTemplates>>();

            _taskNeo4JRepositoryMock.Setup(s => s.BeginTransaction()).Returns(_transactionMock.Object);
            _scheduleServiceFactoryMock.Setup(t => t())
                .Returns(_scheduleServiceMock.Object);

            _unit = new JobService(
                _repoMock.Object,
                _scheduleServiceFactoryMock.Object,
                _dateTimeServiceMock.Object,
                _notificationServiceMock.Object,
                _taskNeo4JRepositoryMock.Object,
                _creditsServiceMock.Object,
                _emailTemplatesMock.Object,
                _taskServiceMock.Object,
                _sendGridServiceMock.Object,
                _taskSocketStoreMock.Object);
        }

        #region ForceEnd tests
        [Fact]
        public void ForceEnd_CorrectTaskId_BeginTransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 3
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ForceEnd(1);
            _taskNeo4JRepositoryMock.Verify(t => t.BeginTransaction(), Times.Once);
        }

        [Fact]
        public void ForceEnd_CorrectTaskId_RemoveRangeCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 3,

                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        }
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ForceEnd(1);
            _transactionScopeMock.Verify(t => t.RemoveRange(It.Is<IEnumerable<TaskJob>>(x => x.Count() == 2 && x.All(j => j.Type == Global.Enums.TaskJobType.End))));
        }

        [Fact]
        public void ForceEnd_CorrectTaskId_ChangeTaskStatusAsyncCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 3
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ForceEnd(1);
            _taskNeo4JRepositoryMock.Verify(t => t.ChangeTaskStatusAsync(1, Global.Enums.TaskStatusType.Completed), Times.Once);
        }

        [Fact]
        public void ForceEnd_CorrectTaskId_SaveChangesCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 3
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ForceEnd(1);
            _transactionScopeMock.Verify(t => t.SaveChanges(), Times.Once);
        }

        [Fact]
        public void ForceEnd_CorrectTaskId_IsGraphCompletedCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 3
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ForceEnd(1);
            _taskNeo4JRepositoryMock.Verify(t => t.IsGraphCompleted(1), Times.Once);
        }

        [Fact]
        public void ForceEnd_CorrectTaskId_ResetProjectTaskTreeStatusesCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 3
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _taskNeo4JRepositoryMock.Setup(t => t.IsGraphCompleted(1))
                .ReturnsAsync(true);
            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ForceEnd(1);
            _scheduleServiceMock.Verify(t => t.ResetProjectTaskTreeStatuses(1), Times.Once);
        }

        [Fact]
        public void ForceEnd_CorrectTaskId_EndTaskJobCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 3
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _taskNeo4JRepositoryMock.Setup(t => t.IsGraphCompleted(1))
                .ReturnsAsync(false);
            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ForceEnd(1);
            _scheduleServiceMock.Verify(t => t.EndTaskJob(_transactionScopeMock.Object, 1), Times.Once);
        }

        [Fact]
        public void ForceEnd_CorrectTaskId_SaveAndCommitCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 3
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _taskNeo4JRepositoryMock.Setup(t => t.IsGraphCompleted(1))
                .ReturnsAsync(false);
            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ForceEnd(1);
            _transactionScopeMock.Verify(t => t.SaveAndCommit(), Times.Once);
        }

        [Fact]
        public void ForceEnd_CorrectTaskId_CommitCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 3
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _taskNeo4JRepositoryMock.Setup(t => t.IsGraphCompleted(1))
                .ReturnsAsync(false);
            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ForceEnd(1);
            _transactionMock.Verify(t => t.Commit(), Times.Once);
        }

        [Fact]
        public void ForceEnd_CorrectTaskId_TaskStatusChangedCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 3,
                    ProjectId = 18
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _taskNeo4JRepositoryMock.Setup(t => t.IsGraphCompleted(1))
                .ReturnsAsync(false);
            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ForceEnd(1);
            _taskSocketStoreMock.Verify(t => t.TaskStatusChanged(18, 1, Global.Enums.TaskStatusType.Completed), Times.Once);
        }
        #endregion

        #region ManageBegin tests

        [Fact]
        public void ManageBegin_HasNoAcceptedVendor_SendInviteOutsourceAsyncCalled()
        {
            var task =
                new ProjectTask
                {
                    Id = 1,
                    ProjectTaskVendors = new List<ProjectTaskVendor>
                    {
                        new ProjectTaskVendor{ Status = ProjectRequestStatus.AcceptedByOther },
                        new ProjectTaskVendor{ Status = ProjectRequestStatus.Declined },
                        new ProjectTaskVendor{ Status = ProjectRequestStatus.DeclinedByOwner },
                        new ProjectTaskVendor{ Status = ProjectRequestStatus.None },
                        new ProjectTaskVendor{ Status = ProjectRequestStatus.Send }
                    },
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }
                };
            var projectTaskTestData = new List<ProjectTask>
            {
                task
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b"),
                    Credits = new Domain.Models.Credits.Credits()
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _notificationServiceMock.Verify(v => v.SendInviteOutsourceAsync(task), Times.Once);
        }

        [Fact]
        public void ManageBegin_HasNoVendor_SendInviteOutsourceAsyncNotCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ProjectTaskVendors = new List<ProjectTaskVendor>
                    {
                    },
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _notificationServiceMock.Verify(v => v.SendInviteOutsourceAsync(It.IsAny<ProjectTask>()), Times.Never);
        }

        [Fact]
        public void ManageBegin_HasAcceptedVendor_SendInviteOutsourceAsyncNotCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ProjectTaskVendors = new List<ProjectTaskVendor>
                    {
                        new ProjectTaskVendor{ Status = ProjectRequestStatus.Accepted }
                    },
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    },
                    FormulaTaskId = 2,
                    ProccessingUserGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b"),
                    Credits = new Domain.Models.Credits.Credits()
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _notificationServiceMock.Verify(v => v.SendStartProjectTaskOutsourcesAsync(_transactionScopeMock.Object, 1), Times.Once);
        }

        [Fact]
        public void ManageBegin_HasAcceptedVendor_StatisticAdded()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ProjectTaskVendors = new List<ProjectTaskVendor>
                    {
                        new ProjectTaskVendor{ Status = ProjectRequestStatus.Accepted }
                    },
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    },
                    FormulaTaskId = 2,
                    ProccessingUserGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199c")
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b"),
                    Credits = new Domain.Models.Credits.Credits()
                }
            }, _transactionScopeMock);
            _dateTimeServiceMock.Setup(t => t.NowUtc)
                .Returns(new DateTime(2018, 5, 20));
            _unit.ManageBegin(1);
            _transactionScopeMock.Verify(v => v.Add(It.Is<FormulaTaskStatistic>(it =>
                          it.Created == new DateTime(2018, 5, 20) &&
                          it.FormulaTaskId == 2 &&
                          it.ProjectTaskId == 1 &&
                          it.Type == StatisticType.Working &&
                          it.VendorGuid == new Guid("4f2a3649-9f0b-40b2-b656-1750f984199c"))), Times.Once);
        }

        [Fact]
        public void ManageBegin_IncorrectTaskId_TransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            Assert.Throws<InvalidOperationException>(() => _unit.ManageBegin(3));
            _repoMock.Verify(t => t.Transaction(), Times.Once);
        }

        [Fact]
        public void ManageBegin_CorrectTaskId_BeginTransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _taskNeo4JRepositoryMock.Verify(t => t.BeginTransaction(), Times.Once);
        }

        [Fact]
        public void ManageBegin_CorrectTaskId_TransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _repoMock.Verify(t => t.Transaction(), Times.Once);
        }

        [Fact]
        public void ManageBegin_CorrectTaskId_ChangeTaskStatusAsyncCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _taskNeo4JRepositoryMock.Verify(t => t.ChangeTaskStatusAsync(1, Global.Enums.TaskStatusType.InProgress), Times.Once);
        }

        [Fact]
        public void ManageBegin_CorrectTaskId_AddCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            var utcNow = DateTime.UtcNow;
            _dateTimeServiceMock.Setup(t => t.NowUtc)
                .Returns(utcNow);
            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _transactionScopeMock.Verify(t =>
                t.Add(It.Is<TaskHistory>(x => x.TaskId == 1
                && x.Type == Global.Enums.ActivityType.InProgress
                && x.HistoryTime == utcNow)), Times.Once);
        }

        [Fact]
        public void ManageBegin_CorrectTaskId_RemoveRangeCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Begin
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        }
                    },
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _transactionScopeMock.Verify(t => t.RemoveRange(It.Is<IEnumerable<TaskJob>>(x => x.Count() == 1 && x.FirstOrDefault().Type == Global.Enums.TaskJobType.Begin)));
        }

        [Fact]
        public void ManageBegin_CorrectTaskId_ChangeTaskStatusAsyncForParentCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 2,
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }

                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _taskNeo4JRepositoryMock.Verify(t => t.ChangeTaskStatusAsync(2, Global.Enums.TaskStatusType.InProgress));
        }

        [Fact]
        public void ManageBegin_CorrectTaskId_SendInProgressTaskAsyncCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 2,
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _notificationServiceMock.Verify(t => t.SendInProgressTaskAsync(_transactionScopeMock.Object, 1), Times.Once);
        }

        [Fact]
        public void ManageBegin_CorrectTaskId_SaveAndCommitCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 2,
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _transactionScopeMock.Verify(t => t.SaveAndCommit(), Times.Once);
        }

        [Fact]
        public void ManageBegin_CorrectTaskId_CommitCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 2,
                    Project = new Domain.Models.Project.Project
                    {
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _transactionMock.Verify(t => t.Commit(), Times.Once);
        }

        [Fact]
        public void ManageBegin_CorrectTaskId_TaskStatusChangedCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    ParentTaskId = 2,
                    ProjectId = 13,
                    Project = new Domain.Models.Project.Project
                    {
                        Id = 13,
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);
            _unit.ManageBegin(1);
            _taskSocketStoreMock.Verify(t => t.TaskStatusChanged(13, 1, Global.Enums.TaskStatusType.InProgress), Times.Once);
        }

        #endregion

        #region ManageEnd tests

        [Fact]
        public void ManageEnd_IncorrectTaskId_TransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            Assert.Throws<InvalidOperationException>(() => _unit.ManageEnd(3));
            _repoMock.Verify(t => t.Transaction(), Times.Once);
        }

        [Fact]
        public void ManageEnd_CorrectTaskId_BeginTransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageEnd(1);
            _taskNeo4JRepositoryMock.Verify(t => t.BeginTransaction(), Times.Once);
        }

        [Fact]
        public void ManageEnd_CorrectTaskId_TransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageEnd(1);
            _repoMock.Verify(t => t.Transaction(), Times.Once);
        }

        [Fact]
        public void ManageEnd_CorrectTaskId_SendOverdueTaskAsyncCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageEnd(1);
            _notificationServiceMock.Verify(t => t.SendOverdueTaskAsync(_transactionScopeMock.Object, 1), Times.Once);
        }

        [Fact]
        public void ManageEnd_CorrectTaskId_RemoveRangeCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Begin
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        }
                    },
                    ProccessingUserGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageEnd(1);
            _transactionScopeMock.Verify(t => t.RemoveRange(It.Is<IEnumerable<TaskJob>>(x => x.Count() == 2 && x.All(j => j.Type == Global.Enums.TaskJobType.End))));
        }

        [Fact]
        public void ManageEnd_CorrectTaskId_SaveChangesCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Begin
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        }
                    },
                    ProccessingUserGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageEnd(1);
            _transactionScopeMock.Verify(t => t.SaveChanges(), Times.Once);
        }

        [Fact]
        public void ManageEnd_CorrectTaskId_EndTaskJobCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Begin
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        }
                    },
                    ProccessingUserGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b"),
                    ParentTaskId = 58
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageEnd(1);
            _scheduleServiceMock.Verify(t => t.EndTaskJob(_transactionScopeMock.Object, 1), Times.Once);
        }

        [Fact]
        public void ManageEnd_CorrectTaskId_SaveAndCommitCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Begin
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        }
                    },
                    ProccessingUserGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b"),
                    ParentTaskId = 58
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageEnd(1);
            _transactionScopeMock.Verify(t => t.SaveAndCommit(), Times.Once);
        }

        [Fact]
        public void ManageEnd_CorrectTaskId_CommitCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Begin
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        }
                    },
                    ProccessingUserGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b"),
                    ParentTaskId = 58
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageEnd(1);
            _transactionMock.Verify(t => t.Commit(), Times.Once);
        }

        [Fact]
        public void ManageEnd_CorrectTaskId_TaskStatusChangedCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Begin
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        }
                    },
                    ProccessingUserGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b"),
                    ParentTaskId = 58,
                    ProjectId = 223
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageEnd(1);
            _taskSocketStoreMock.Verify(t => t.TaskStatusChanged(223, 1, Global.Enums.TaskStatusType.Completed));
        }

        #endregion

        #region ManageTaskTreeEnd tests

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_BeginTransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageTaskTreeEnd(1);
            _taskNeo4JRepositoryMock.Verify(t => t.BeginTransaction(), Times.Once);
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_TransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageTaskTreeEnd(1);
            _repoMock.Verify(t => t.Transaction(), Times.Once);
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_GetRootTaskIdsAsyncCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageTaskTreeEnd(1);
            _taskNeo4JRepositoryMock.Verify(t => t.GetRootTaskIdsAsync(1), Times.Once);
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_IsRootAsyncCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageTaskTreeEnd(1);
            _taskNeo4JRepositoryMock.Verify(t => t.IsRootAsync(1), Times.Once);
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_GetParentTaskIdsAsyncCalled()
        {
            SetDataForManageTaskTreeEnd();

            _unit.ManageTaskTreeEnd(1);
            _taskNeo4JRepositoryMock.Verify(t => t.GetParentTaskIdsAsync(1), Times.Once);
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_GetFormulaRootTaskIdsAsyncCalled()
        {
            SetDataForManageTaskTreeEnd();

            _unit.ManageTaskTreeEnd(1);
            _taskNeo4JRepositoryMock.Verify(t => t.GetFormulaRootTaskIdsAsync(1));
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_GetNextOccurenceCalled()
        {
            SetDataForManageTaskTreeEnd();

            _unit.ManageTaskTreeEnd(1);
            _dateTimeServiceMock.Verify(t => t.GetNextOccurence("0 21 1 1 ? *", new DateTime(2018, 6, 20, 3, 40, 0).ToUniversalTime()));
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_SaveChangesCalled()
        {
            SetDataForManageTaskTreeEnd();

            _unit.ManageTaskTreeEnd(1);
            _transactionScopeMock.Verify(t => t.SaveChanges());
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_GetChildTaskIdsAsyncCalled()
        {
            SetDataForManageTaskTreeEnd();

            _unit.ManageTaskTreeEnd(1);
            _taskNeo4JRepositoryMock.Verify(t => t.GetChildTaskIdsAsync(1), Times.Once);
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_TastStatusesChanged()
        {
            var projectTasks = SetDataForManageTaskTreeEnd();

            _unit.ManageTaskTreeEnd(1);
            Assert.True(projectTasks.Where(t => t.Id == 1 || t.Id == 3 || t.Id == 4).All(x => x.Status == Global.Enums.TaskStatusType.New));
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_NextOccurenceChanged()
        {
            var projectTasks = SetDataForManageTaskTreeEnd();

            _unit.ManageTaskTreeEnd(1);
            Assert.True(projectTasks.Where(t => t.Id == 3 || t.Id == 4).All(x => x.RecurrenceOptions.NextOccurenceDate == new DateTime(2019, 1, 1, 21, 0, 0)));
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_SaveAndCommitCalled()
        {
            var projectTasks = SetDataForManageTaskTreeEnd();

            _unit.ManageTaskTreeEnd(1);
            _transactionScopeMock.Verify(t => t.SaveAndCommit(), Times.Once);
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_CommitCalled()
        {
            var projectTasks = SetDataForManageTaskTreeEnd();

            _unit.ManageTaskTreeEnd(1);
            _transactionMock.Verify(t => t.Commit(), Times.Once);
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_CreateJobBeginCalled()
        {
            var projectTasks = SetDataForManageTaskTreeEnd();

            _unit.ManageTaskTreeEnd(1);
            _scheduleServiceMock.Verify(t => t.CreateJobBegin(_transactionScopeMock.Object, It.Is<ProjectTask>(x => x.Id == 3 || x.Id == 4), false));
        }

        [Fact]
        public void ManageTaskTreeEnd_CorrectTaskId_TaskStatusChangedCalled()
        {
            var projectTasks = SetDataForManageTaskTreeEnd();

            _unit.ManageTaskTreeEnd(1);
            _taskSocketStoreMock.Verify(t => t.TaskStatusChanged(12, It.Is<Dictionary<int, TaskStatusType>>(x => x.Count() == 3 && x.All(j => j.Value == TaskStatusType.New))));
        }

        private List<ProjectTask> SetDataForManageTaskTreeEnd()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    FormulaId = 32,
                    ProjectId = 12,
                    Project = new Project
                    {
                        Id = 12,
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    },
                    StartDate = new DateTime(2018, 12, 12),
                    RecurrenceOptionsId = 28,
                    RecurrenceOptions = new RecurrenceOptions
                    {
                        Cron = "0 21 1 1 ? *",
                        RecurrenceType = Global.Enums.FormulaTaskRecurrenceType.EndNever,
                        NextOccurenceDate = new DateTime(2018, 6, 20)
                    },
                    Duration = 40,
                    Status = Global.Enums.TaskStatusType.Completed
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4,
                    ProjectId = 12,
                    Project = new Project
                    {
                        Id = 12,
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    },
                    StartDate = new DateTime(2018, 12, 12),
                    RecurrenceOptionsId = 21,
                    RecurrenceOptions = new RecurrenceOptions
                    {
                        Cron = "0 21 1 1 ? *",
                        RecurrenceType = Global.Enums.FormulaTaskRecurrenceType.EndNever,
                        NextOccurenceDate = new DateTime(2018, 6, 22)
                    },
                    Duration = 50,
                    Status = Global.Enums.TaskStatusType.Completed
                },
                new ProjectTask
                {
                    Id = 3,
                    StartDate = new DateTime(2018, 12, 12),
                    RecurrenceOptionsId = 24,
                    ProjectId = 12,
                    Project = new Project
                    {
                        Id = 12,
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    },
                    RecurrenceOptions = new RecurrenceOptions
                    {
                        Cron = "0 21 1 1 ? *",
                        RecurrenceType = Global.Enums.FormulaTaskRecurrenceType.EndNever,
                        NextOccurenceDate = new DateTime(2018, 6, 20)
                    },
                    Duration = 50,
                    Status = Global.Enums.TaskStatusType.Completed
                },
                new ProjectTask
                {
                    Id = 4,
                    StartDate = new DateTime(2018, 12, 12),
                    RecurrenceOptionsId = 23,
                    ProjectId = 12,
                    Project = new Project
                    {
                        Id = 12,
                        OwnerGuid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                    },
                    RecurrenceOptions = new RecurrenceOptions
                    {
                        Cron = "0 21 1 1 ? *",
                        RecurrenceType = Global.Enums.FormulaTaskRecurrenceType.EndNever,
                        NextOccurenceDate = new DateTime(2018, 6, 20)
                    },
                    Duration = 36,
                    Status = Global.Enums.TaskStatusType.Completed
                }
            };

            _dateTimeServiceMock.Setup(t => t.MaxDate(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns((DateTime date1, DateTime date2) =>
                {
                    return date1 > date2 ? date1 : date2;
                });

            _dateTimeServiceMock.Setup(t => t.GetNextOccurence(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns((string cron, DateTime dateTime) =>
                {
                    return CrontabSchedule.Parse(cron, CronStringFormat.WithYears).GetNextOccurrence(dateTime);
                });

            _dateTimeServiceMock.Setup(t => t.NowUtc)
                .Returns(new DateTime(2018, 5, 20));

            _taskNeo4JRepositoryMock.Setup(t => t.GetParentTaskIdsAsync(1))
                .ReturnsAsync(new List<int> { 2 });

            _taskNeo4JRepositoryMock.Setup(t => t.GetParentTaskIdsAsync(It.Is<int>(x => x == 3 || x == 4)))
                .ReturnsAsync(new List<int> { 1 });

            _taskNeo4JRepositoryMock.Setup(t => t.GetRootTaskIdsAsync(1))
                .ReturnsAsync(new List<int> { 1 });

            _taskNeo4JRepositoryMock.Setup(t => t.GetFormulaRootTaskIdsAsync(1))
               .ReturnsAsync(new List<int> { 3, 4 });
            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);

            _repoMock.SetupRepoMock(new List<User> {
                new User
                {
                    Id = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b")
                }
            }, _transactionScopeMock);

            return projectTaskTestData;
        }
        #endregion

        #region ManageDelayedTaskTreeEnd tests

        [Fact]
        public void ManageDelayedTaskTreeEnd_CorrectTaskId_TransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageDelayedTaskTreeEnd(1);
            _repoMock.Verify(t => t.Transaction(), Times.Once);
        }

        #endregion

        #region ManageOverdue tests

        [Fact]
        public void ManageOverdue_CorrectTaskId_TransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageOverdue(1);
            _repoMock.Verify(t => t.Transaction(), Times.Once);
        }

        [Fact]
        public void ManageOverdue_CorrectTaskId_AddForTaskHistoryCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        }
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageOverdue(1);
            _transactionScopeMock.Verify(t => t.Add(It.Is<TaskHistory>(x => x.TaskId == 1 && x.Type == Global.Enums.ActivityType.Overdue)), Times.Once);
        }

        [Fact]
        public void ManageOverdue_CorrectTaskId_RemoveRangeCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Deadline
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Deadline
                        }
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageOverdue(1);
            _transactionScopeMock.Verify(t =>
                t.RemoveRange(It.Is<IEnumerable<TaskJob>>(x => x.All(j => j.Type == Global.Enums.TaskJobType.Overdue) && x.Count() == 1)), Times.Once);
        }

        [Fact]
        public void ManageOverdue_CorrectTaskId_SaveAndCommitCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageOverdue(1);
            _transactionScopeMock.Verify(t => t.SaveAndCommit(), Times.Once);
        }

        [Fact]
        public void ManageOverdue_CorrectTaskId_SendOverdueTaskAsyncCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageOverdue(1);
            _notificationServiceMock.Verify(t => t.SendOverdueTaskAsync(_transactionScopeMock.Object, 1), Times.Once);
        }

        #endregion

        #region ManageDeadline tests

        [Fact]
        public void ManageDeadline_CorrectTaskId_TransactionCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageDeadline(1);
            _repoMock.Verify(t => t.Transaction(), Times.Once);
        }

        [Fact]
        public void ManageDeadline_CorrectTaskId_AddForTaskHistoryCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.End
                        }
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageDeadline(1);
            _transactionScopeMock.Verify(t => t.Add(It.Is<TaskHistory>(x => x.TaskId == 1 && x.Type == Global.Enums.ActivityType.Deadline)), Times.Once);
        }

        [Fact]
        public void ManageDeadline_CorrectTaskId_RemoveRangeCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    TaskJobs = new List<TaskJob>
                    {
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Deadline
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Overdue
                        },
                        new TaskJob
                        {
                            TaskId = 1,
                            Type = Global.Enums.TaskJobType.Deadline
                        }
                    }
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageDeadline(1);
            _transactionScopeMock.Verify(t =>
                t.RemoveRange(It.Is<IEnumerable<TaskJob>>(x => x.All(j => j.Type == Global.Enums.TaskJobType.Deadline) && x.Count() == 2)), Times.Once);
        }

        [Fact]
        public void ManageDeadline_CorrectTaskId_SaveAndCommitCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageDeadline(1);
            _transactionScopeMock.Verify(t => t.SaveAndCommit(), Times.Once);
        }

        [Fact]
        public void ManageDeadline_CorrectTaskId_SendOverdueTaskAsyncCalled()
        {
            var projectTaskTestData = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    ParentTaskId = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTaskTestData, _transactionScopeMock);
            _unit.ManageDeadline(1);
            _notificationServiceMock.Verify(t => t.SendDeadlineAsync(_transactionScopeMock.Object, 1), Times.Once);
        }

        #endregion

        #region ManageDaySummary tests

        [Fact]
        public void SendSummaryAsync_CorrectId_SendSummaryAsyncCalled()
        {
            var guid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b");
            _unit.ManageDaySummary(guid);
            _notificationServiceMock.Verify(t => t.SendSummaryAsync(guid), Times.Once);
        }

        #endregion

        #region ManageDailyToDoSummary tests

        [Fact]
        public void ManageDailyToDoSummary_CorrectId_SendDailyToDoSummaryCalled()
        {
            var guid = new Guid("4f2a3649-9f0b-40b2-b656-1750f984199b");
            _unit.ManageDailyToDoSummary(guid);
            _notificationServiceMock.Verify(t => t.SendDailyToDoSummary(guid), Times.Once);
        }

        #endregion
    }
}
