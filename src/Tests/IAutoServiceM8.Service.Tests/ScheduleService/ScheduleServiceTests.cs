using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IAutoM8.Service.Hangfire.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using Moq;
using Xunit;
using IAutoM8.Service.Scheduler;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Neo4jRepository.Interfaces;
using System.Linq.Expressions;
using IAutoM8.Tests.Common;
using IAutoM8.Domain.Models.Project.Task;
using System.Threading;
using IAutoM8.Global.Enums;
using IAutoM8.Domain.Models;
using System.ComponentModel.DataAnnotations;
using NCrontab.Advanced.Exceptions;

namespace IAutoM8.Service.Tests.Schedule
{
    public class ScheduleServiceTests
    {
        private IScheduleService _unit;
        private Mock<IHangfireService> _hangfireServiceMock;
        private Mock<IJobService> _jobServiceMock;
        private Mock<IDateTimeService> _dateTimeServiceMock;
        private Mock<ITaskNeo4jRepository> _taskNeo4jRepositoryMock;
        private Mock<IRepo> _repoMock;
        private Mock<ITransactionScope> _transactionScopeMock;

        private const string ReccuringBussinessJobId = "{0}-bussiness";
        private const string ReccuringSummaryJobId = "{0}-dailyToDoSummary";

        public ScheduleServiceTests()
        {
            _hangfireServiceMock = new Mock<IHangfireService>();
            _repoMock = new Mock<IRepo>();
            _jobServiceMock = new Mock<IJobService>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _taskNeo4jRepositoryMock = new Mock<ITaskNeo4jRepository>();
            _transactionScopeMock = new Mock<ITransactionScope>();
            _unit = new ScheduleService(
                _repoMock.Object,
                _jobServiceMock.Object,
                _dateTimeServiceMock.Object,
                _taskNeo4jRepositoryMock.Object,
                _hangfireServiceMock.Object
                );
        }

        #region DaySummary test

        [Fact]
        public async Task DaySummary_CorrectGuid_RunDaySummaryInvoke()
        {
            var id = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6");
            await _unit.DaySummary(id);
            _hangfireServiceMock.Verify(t => t.RunDaySummary(ReccuringBussinessJobId, id,
                It.IsAny<Expression<Action>>(), "0 0 * * *", TimeZoneInfo.Utc));
        }

        #endregion

        #region DailyToDoSummary test
        [Fact]
        public async Task DailyToDoSummary_CorrectGuid_RunDailyToDoSummaryInvoke()
        {
            var id = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6");
            await _unit.DailyToDoSummary(id);
            _hangfireServiceMock.Verify(t => t.RunDailyToDoSummary(ReccuringSummaryJobId, id,
                It.IsAny<Expression<Action>>(), "0 8 * * *", TimeZoneInfo.Utc));
        }

        [Fact]
        public async Task DailyToDoSummary_NotificationTimePassed_RunDailyToDoSummaryInvokeWithCorrectCron()
        {
            var id = new Guid("22345200-abe8-4f60-90c8-0d43c5f6c0f6");
            DateTime notificationTime = new DateTime(2000, 1, 1, 3, 0, 0);
            await _unit.DailyToDoSummary(id, notificationTime);
            _hangfireServiceMock.Verify(t => t.RunDailyToDoSummary(ReccuringSummaryJobId, id,
                It.IsAny<Expression<Action>>(), "0 3 * * *", TimeZoneInfo.Utc));
        }
        #endregion

        #region RemoveJob test
        [Theory]
        [InlineData("228")]
        [InlineData("248")]
        public async Task RemoveJob_CorrectTaskId_DeleteJobCalledTwice(string hangfireJobId)
        {
            _repoMock.SetupRepoMock(new List<TaskJob> {
                new TaskJob
                {
                    Id = 1,
                    HangfireJobId = "228",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                new TaskJob
                {
                    Id = 2,
                    HangfireJobId = "248",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                new TaskJob
                {
                    Id = 3,
                    HangfireJobId = "268",
                    TaskId = 123,
                    Type = TaskJobType.Overdue
                }
            }, _transactionScopeMock);

            await _unit.RemoveJob(_transactionScopeMock.Object, 123);
            _hangfireServiceMock.Verify(t => t.DeleteJob(hangfireJobId), Times.Once);
        }

        [Fact]
        public async Task RemoveJob_CorrectTaskId_RemoveRangeCalled()
        {
            var storage = new List<TaskJob> {
                new TaskJob
                {
                    Id = 1,
                    HangfireJobId = "228",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                new TaskJob
                {
                    Id = 2,
                    HangfireJobId = "248",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                new TaskJob
                {
                    Id = 3,
                    HangfireJobId = "268",
                    TaskId = 123,
                    Type = TaskJobType.Overdue
                }
            };
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);

            await _unit.RemoveJob(_transactionScopeMock.Object, 123);
            _transactionScopeMock.Verify(t => t.RemoveRange(storage), Times.Once);
        }

        [Fact]
        public async Task RemoveJob_CorrectTaskId_SaveChangesAsyncCalled()
        {
            var storage = new List<TaskJob> {
                new TaskJob
                {
                    Id = 1,
                    HangfireJobId = "228",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                new TaskJob
                {
                    Id = 2,
                    HangfireJobId = "248",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                new TaskJob
                {
                    Id = 3,
                    HangfireJobId = "268",
                    TaskId = 123,
                    Type = TaskJobType.End
                }
            };
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);

            await _unit.RemoveJob(_transactionScopeMock.Object, 123);
            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Once);
        }
        #endregion

        #region RemoveJobEnd test

        [Fact]
        public async Task RemoveJobEnd_CorrectTaskId_DeleteJobCalledTwice()
        {
            _repoMock.SetupRepoMock(new List<TaskJob> {
                new TaskJob
                {
                    Id = 1,
                    HangfireJobId = "228",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                new TaskJob
                {
                    Id = 2,
                    HangfireJobId = "248",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                new TaskJob
                {
                    Id = 3,
                    HangfireJobId = "268",
                    TaskId = 123,
                    Type = TaskJobType.End
                }
            }, _transactionScopeMock);

            await _unit.RemoveJobEnd(_transactionScopeMock.Object, 123);
            _hangfireServiceMock.Verify(t => t.DeleteJob("268"), Times.Once);
        }

        [Fact]
        public async Task RemoveJobEnd_CorrectTaskId_RemoveRangeCalled()
        {
            var endTaskJob = new TaskJob
            {
                Id = 3,
                HangfireJobId = "268",
                TaskId = 123,
                Type = TaskJobType.End
            };
            var storage = new List<TaskJob> {
                new TaskJob
                {
                    Id = 1,
                    HangfireJobId = "228",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                new TaskJob
                {
                    Id = 2,
                    HangfireJobId = "248",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                endTaskJob
            };
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);

            await _unit.RemoveJobEnd(_transactionScopeMock.Object, 123);
            _transactionScopeMock.Verify(t => t.RemoveRange(It.Is<List<TaskJob>>(x => x.Contains(endTaskJob))), Times.Once);
        }

        [Fact]
        public async Task RemoveJobEnd_CorrectTaskId_SaveChangesAsyncCalled()
        {
            var storage = new List<TaskJob> {
                new TaskJob
                {
                    Id = 1,
                    HangfireJobId = "228",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                new TaskJob
                {
                    Id = 2,
                    HangfireJobId = "248",
                    TaskId = 123,
                    Type = TaskJobType.Begin
                },
                new TaskJob
                {
                    Id = 3,
                    HangfireJobId = "268",
                    TaskId = 123,
                    Type = TaskJobType.Overdue
                }
            };
            _repoMock.SetupRepoMock(storage, _transactionScopeMock);

            await _unit.RemoveJob(_transactionScopeMock.Object, 123);
            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        #endregion

        #region CreateJobBegin tests

        [Fact]
        public async Task CreateJobBegin_TaskWithoutCron_ExceptionThrown()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22
                }
            };
            await Assert.ThrowsAsync<ValidationException>(() => _unit.CreateJobBegin(_transactionScopeMock.Object, projectTask));
        }

        [Fact]
        public async Task CreateJobBegin_TaskWithIncorrectCron_ExceptionThrown()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "12 0 * * *"
                }
            };
            await Assert.ThrowsAsync<CrontabException>(() => _unit.CreateJobBegin(_transactionScopeMock.Object, projectTask));
        }

        [Fact]
        public async Task CreateJobBegin_CorrectTask_ScheduleJobForManageBeginCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };
            await _unit.CreateJobBegin(_transactionScopeMock.Object, projectTask);
            _hangfireServiceMock.Verify(t => t.ScheduleJobForManageBegin(1, new TimeSpan(21, 0, 0)), Times.Once);
        }

        [Fact]
        public async Task CreateJobBegin_CorrectTask_ScheduleJobForManageOverdueCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };
            await _unit.CreateJobBegin(_transactionScopeMock.Object, projectTask);
            _hangfireServiceMock.Verify(t => t.ScheduleJobForManageOverdue(1, new TimeSpan(21, 0, 0).Add(TimeSpan.FromMinutes(projectTask.Duration.Value))), Times.Once);
        }

        [Fact]
        public async Task CreateJobBegin_CorrectTask_ScheduleJobForManageDeadlineCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };
            await _unit.CreateJobBegin(_transactionScopeMock.Object, projectTask);
            var deadlineMinutes = projectTask.Duration.Value * 0.85;
            _hangfireServiceMock.Verify(t => t.ScheduleJobForManageDeadline(1, new TimeSpan(21, 0, 0).Add(TimeSpan.FromMinutes(deadlineMinutes))), Times.Once);
        }

        [Fact]
        public async Task CreateJobBegin_CorrectTask_AddAsyncForDeadlineCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };

            var deadlineId = "59228";
            var deadlineMinutes = projectTask.Duration.Value * 0.85;
            _hangfireServiceMock
                .Setup(t => t.ScheduleJobForManageDeadline(projectTask.Id, new TimeSpan(21, 0, 0).Add(TimeSpan.FromMinutes(deadlineMinutes))))
                .Returns(deadlineId);
            await _unit.CreateJobBegin(_transactionScopeMock.Object, projectTask);
            _transactionScopeMock.Verify(t => t.AddAsync(It.Is<TaskJob>(x => x.HangfireJobId == deadlineId && x.Type == TaskJobType.Deadline && x.TaskId == projectTask.Id)), Times.Once);
        }

        [Fact]
        public async Task CreateJobBegin_CorrectTask_AddAsyncForBeginCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };

            var beginId = "51111";
            _hangfireServiceMock
                .Setup(t => t.ScheduleJobForManageBegin(projectTask.Id, new TimeSpan(21, 0, 0)))
                .Returns(beginId);
            await _unit.CreateJobBegin(_transactionScopeMock.Object, projectTask);
            _transactionScopeMock.Verify(t => t.AddAsync(It.Is<TaskJob>(x => x.HangfireJobId == beginId && x.Type == TaskJobType.Begin && x.TaskId == projectTask.Id)), Times.Once);
        }

        [Fact]
        public async Task CreateJobBegin_CorrectTask_AddAsyncForOverdueCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };

            var overdueId = "51133";
            _hangfireServiceMock
                .Setup(t => t.ScheduleJobForManageOverdue(projectTask.Id, new TimeSpan(21, 0, 0).Add(TimeSpan.FromMinutes(projectTask.Duration.Value))))
                .Returns(overdueId);
            await _unit.CreateJobBegin(_transactionScopeMock.Object, projectTask);
            _transactionScopeMock.Verify(t => t.AddAsync(It.Is<TaskJob>(x => x.HangfireJobId == overdueId && x.Type == TaskJobType.Overdue && x.TaskId == projectTask.Id)), Times.Once);
        }

        [Fact]
        public async Task CreateJobBegin_CorrectTask_SaveChangesAsyncCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };

            await _unit.CreateJobBegin(_transactionScopeMock.Object, projectTask);
            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Once);
        }
        #endregion

        #region CreateJobEnd tests

        [Fact]
        public async Task CreateJobEnd_IncorrectTask_ExceptionThrown()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22
                }
            };
            await Assert.ThrowsAsync<ValidationException>(() => _unit.CreateJobEnd(_transactionScopeMock.Object, projectTask));
        }

        [Fact]
        public async Task CreateJobEnd_TaskWithIncorrectCron_ExceptionThrown()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "12 0 * * *"
                }
            };
            await Assert.ThrowsAsync<CrontabException>(() => _unit.CreateJobEnd(_transactionScopeMock.Object, projectTask));
        }

        [Fact]
        public async Task CreateJobEnd_CorrectTask_ScheduleJobForManageEndCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };
            await _unit.CreateJobEnd(_transactionScopeMock.Object, projectTask);
            _hangfireServiceMock.Verify(t => t.ScheduleJobForManageEnd(projectTask.Id, new TimeSpan(21, 0, 0).Add(TimeSpan.FromMinutes(projectTask.Duration.Value))));
        }

        [Fact]
        public async Task CreateJobEnd_CorrectTask_AddAsyncForJobEndCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };

            var jobEndId = "214447";
            _hangfireServiceMock
                .Setup(t => t.ScheduleJobForManageEnd(projectTask.Id, new TimeSpan(21, 0, 0).Add(TimeSpan.FromMinutes(projectTask.Duration.Value))))
                .Returns(jobEndId);
            await _unit.CreateJobEnd(_transactionScopeMock.Object, projectTask);
            _transactionScopeMock.Verify(t => t.AddAsync(It.Is<TaskJob>(x => x.TaskId == projectTask.Id && x.HangfireJobId == jobEndId && x.Type == TaskJobType.End)), Times.Once);
        }

        [Fact]
        public async Task CreateJobEnd_CorrectTask_SaveChangesAsyncCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };

            await _unit.CreateJobEnd(_transactionScopeMock.Object, projectTask);
            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        #endregion

        #region CreateAutomatedJob tests
        [Fact]
        public async Task CreateAutomatedJob_IncorrectTask_ExceptionThrown()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22
                }
            };
            await Assert.ThrowsAsync<ValidationException>(() => _unit.CreateAutomatedJob(_transactionScopeMock.Object, projectTask));
        }

        [Fact]
        public async Task CreateAutomatedJob_TaskWithIncorrectCron_ExceptionThrown()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "12 0 * * *"
                }
            };
            await Assert.ThrowsAsync<CrontabException>(() => _unit.CreateAutomatedJob(_transactionScopeMock.Object, projectTask));
        }

        [Fact]
        public async Task CreateAutomatedJob_CorrectTask_ScheduleJobForManageBeginCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };
            await _unit.CreateAutomatedJob(_transactionScopeMock.Object, projectTask);
            _hangfireServiceMock.Verify(t => t.ScheduleJobForManageBegin(projectTask.Id, new TimeSpan(21, 0, 0)), Times.Once);
        }

        [Fact]
        public async Task CreateAutomatedJob_CorrectTask_ScheduleJobForManageEndCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };
            await _unit.CreateAutomatedJob(_transactionScopeMock.Object, projectTask);
            _hangfireServiceMock.Verify(t => t.ScheduleJobForManageEnd(projectTask.Id, new TimeSpan(21, 0, 0).Add(TimeSpan.FromMinutes(projectTask.Duration.Value))), Times.Once);
        }

        [Fact]
        public async Task CreateAutomatedJob_CorrectTask_AddAsyncForBeginCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };

            var beginId = "51111";
            _hangfireServiceMock
                .Setup(t => t.ScheduleJobForManageBegin(projectTask.Id, new TimeSpan(21, 0, 0)))
                .Returns(beginId);
            await _unit.CreateAutomatedJob(_transactionScopeMock.Object, projectTask);
            _transactionScopeMock.Verify(t => t.AddAsync(It.Is<TaskJob>(x => x.HangfireJobId == beginId && x.Type == TaskJobType.Begin && x.TaskId == projectTask.Id)), Times.Once);
        }

        [Fact]
        public async Task CreateAutomatedJob_CorrectTask_AddAsyncForOverdueCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };

            var endId = "51133";
            _hangfireServiceMock
                .Setup(t => t.ScheduleJobForManageEnd(projectTask.Id, new TimeSpan(21, 0, 0).Add(TimeSpan.FromMinutes(projectTask.Duration.Value))))
                .Returns(endId);
            await _unit.CreateAutomatedJob(_transactionScopeMock.Object, projectTask);
            _transactionScopeMock.Verify(t => t.AddAsync(It.Is<TaskJob>(x => x.HangfireJobId == endId && x.Type == TaskJobType.End && x.TaskId == projectTask.Id)), Times.Once);
        }

        [Fact]
        public async Task CreateAutomatedJob_CorrectTask_SaveChangesAsyncCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };

            await _unit.CreateAutomatedJob(_transactionScopeMock.Object, projectTask);
            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        #endregion

        #region ResetProjectTaskTreeStatuses tests

        [Fact]
        public async Task ResetProjectTaskTreeStatuses_CorrectJobId_EnqueueJobManageTaskTreeEndCalled()
        {
            await _unit.ResetProjectTaskTreeStatuses(123);
            _hangfireServiceMock.Verify(t => t.EnqueueJobManageTaskTreeEnd(123), Times.Once);
        }

        #endregion

        #region ResetProjectTaskTreeStatuses tests

        [Fact]
        public async Task DelayedProjectTaskTreeStatuses_CorrectJobId_RunManageDelayedTaskTreeEndCalled()
        {
            await _unit.DelayedProjectTaskTreeStatuses(123, new TimeSpan(0,0, 30));
            _hangfireServiceMock.Verify(t => t.RunManageDelayedTaskTreeEnd(123, new TimeSpan(0, 0, 30)), Times.Once);
        }

        #endregion

        #region EnqueneJobEnd tests

        [Fact]
        public async Task EnqueneJobEnd_CorrectTaskPassed_EnqueueJobForceEndCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };
            await _unit.EnqueneJobEnd(_transactionScopeMock.Object, projectTask);
            _hangfireServiceMock.Verify(t => t.EnqueueJobForceEnd(projectTask.Id), Times.Once);
        }

        [Fact]
        public async Task EnqueneJobEnd_CorrectTaskPassed_AddAsyncCalled()
        {
            var projectTask = new ProjectTask
            {
                Id = 1,
                RecurrenceOptions = new RecurrenceOptions
                {
                    Id = 22,
                    Cron = "0 21 1 1 ? *"
                },
                Duration = 40
            };

            var jobId = "1111";
            _hangfireServiceMock.Setup(t => t.EnqueueJobForceEnd(projectTask.Id))
                .Returns(jobId);
            await _unit.EnqueneJobEnd(_transactionScopeMock.Object, projectTask);
            _transactionScopeMock.Verify(t => t.AddAsync(It.Is<TaskJob>(x => x.HangfireJobId == jobId && x.TaskId == projectTask.Id && x.Type == TaskJobType.End)));
        }

        #endregion
    }
}
