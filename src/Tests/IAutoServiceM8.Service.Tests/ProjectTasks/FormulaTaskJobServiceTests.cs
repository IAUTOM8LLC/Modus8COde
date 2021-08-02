using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IAutoM8.Domain.Models;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Formula.Dto;
using IAutoM8.Service.ProjectTasks;
using IAutoM8.Service.ProjectTasks.Dto;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using IAutoM8.Tests.Common;
using Moq;
using Xunit;

namespace IAutoM8.Service.Tests.ProjectTasks
{
    public class FormulaTaskJobServiceTests
    {
        private IFormulaTaskJobService _unit;
        private Mock<IScheduleService> _scheduleServiceMock;
        private Mock<ITaskNeo4jRepository> _taskNeo4JRepositoryMock;
        private Mock<ITaskScheduleService> _taskScheduleServiceMock;
        private Mock<ITaskStartDateHelperService> _startDateHelperServiceMock;
        private Mock<IRepo> _repoMock;
        private Mock<ITransactionScope> _transactionScopeMock;

        public FormulaTaskJobServiceTests()
        {
            _scheduleServiceMock = new Mock<IScheduleService>();
            _taskNeo4JRepositoryMock = new Mock<ITaskNeo4jRepository>();
            _taskScheduleServiceMock = new Mock<ITaskScheduleService>();
            _taskScheduleServiceMock.Setup(s => s.GetNextOccurence(It.IsAny<ITransactionScope>(), It.IsAny<int>(), It.IsAny<RecurrenceOptions>()))
                .ReturnsAsync(new RecurrenceAsapDto());
            _startDateHelperServiceMock = new Mock<ITaskStartDateHelperService>();
            _transactionScopeMock = new Mock<ITransactionScope>();
            _repoMock = new Mock<IRepo>();

            _unit = new FormulaTaskJobService(
                _scheduleServiceMock.Object,
                _taskNeo4JRepositoryMock.Object,
                _taskScheduleServiceMock.Object,
                _startDateHelperServiceMock.Object
                );
        }

        #region RemoveFormulaTaskJobs tests

        [Fact]
        public async Task RemoveFormulaTaskJobs_ChildTaskInNewStatus_RemoveJobCalled()
        {
            await _unit.RemoveFormulaTaskJobs(_transactionScopeMock.Object,
                new ProjectTask
                {
                    Id = 5,
                    Status = TaskStatusType.New
                }, false);

            _scheduleServiceMock.Verify(t => t.RemoveJob(_transactionScopeMock.Object, 5), Times.Once);
        }

        [Fact]
        public async Task RemoveFormulaTaskJobs_ChildTaskInNewStatus_GetFormulaRootTaskIdsAsyncCalled()
        {
            var projectTasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2
                }
            };

            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);
            await _unit.RemoveFormulaTaskJobs(_transactionScopeMock.Object,
                new ProjectTask
                {
                    Id = 5,
                    Status = TaskStatusType.New
                }, true);

            _taskNeo4JRepositoryMock.Verify(t => t.GetFormulaRootTaskIdsAsync(5), Times.Once);
        }

        [Fact]
        public async Task RemoveFormulaTaskJobs_ChildTaskInNewStatusWithRootTasks_RemoveFormulaTaskJobsCalled()
        {
            var projectTasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    Status = TaskStatusType.New
                },
                new ProjectTask
                {
                    Id = 2,
                    FormulaId = 4,
                    Status = TaskStatusType.New
                },
                new ProjectTask
                {
                    Id = 3,
                    FormulaId = 4,
                    Status = TaskStatusType.New
                },
                new ProjectTask
                {
                    Id = 4
                }
            };

            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);
            _taskNeo4JRepositoryMock.Setup(t => t.GetFormulaRootTaskIdsAsync(1)).ReturnsAsync(new List<int>
            {
                2, 3
            });

            await _unit.RemoveFormulaTaskJobs(_transactionScopeMock.Object,
                new ProjectTask
                {
                    Id = 1,
                    Status = TaskStatusType.New
                }, true);

            var taskIds = new List<int>
            {
                1, 2, 3
            };
            _scheduleServiceMock.Verify(t => t.RemoveJob(_transactionScopeMock.Object, It.Is<int>(taskId => taskIds.Contains(taskId))));
        }

        #endregion

        #region AddFormulaTaskJobs tests

        [Fact]
        public async Task AddFormulaTaskJobs_TaskWihoutFormulaRootIds_GetFormulaRootTaskIdsAsyncCalled()
        {
            _repoMock.SetupRepoMock(new List<ProjectTask>(), _transactionScopeMock);
            await _unit.AddFormulaTaskJobs(_transactionScopeMock.Object, new ProjectTask
            {
                Id = 3
            });

            _taskNeo4JRepositoryMock.Verify(t => t.GetFormulaRootTaskIdsAsync(3), Times.Once);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public async Task AddFormulaTaskJobs_TaskWihFormulaRootIds_GetFormulaRootTaskIdsAsyncCalled(int formulaTaskId)
        {
            var projectTasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2
                },
                new ProjectTask
                {
                    Id = 3
                }
            };

            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);
            _taskNeo4JRepositoryMock.Setup(t => t.GetFormulaRootTaskIdsAsync(1)).ReturnsAsync(new List<int>
            {
                2, 3
            });
            await _unit.AddFormulaTaskJobs(_transactionScopeMock.Object, new ProjectTask
            {
                Id = 1
            });

            _taskScheduleServiceMock.Verify(t => t.ScheduleNewTask(_transactionScopeMock.Object, It.Is<ProjectTask>(pt => pt.Id == formulaTaskId), false), Times.Once);
        }

        [Fact]
        public async Task AddFormulaTaskJobs_TaskWihFormulaRootIds_RecuriveCallOccured()
        {
            var projectTasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1
                },
                new ProjectTask
                {
                    Id = 2
                },
                new ProjectTask
                {
                    Id = 3,
                    FormulaId = 4
                }
            };

            _repoMock.SetupRepoMock(projectTasks, _transactionScopeMock);
            _taskNeo4JRepositoryMock.Setup(t => t.GetFormulaRootTaskIdsAsync(1)).ReturnsAsync(new List<int>
            {
                2, 3
            });
            await _unit.AddFormulaTaskJobs(_transactionScopeMock.Object, new ProjectTask
            {
                Id = 1
            });

            _taskNeo4JRepositoryMock.Verify(t => t.GetFormulaRootTaskIdsAsync(3), Times.Once);
        }

        #endregion

        #region UpdateFormulaTaskTime tests

        [Fact]
        public async Task UpdateFormulaTaskTime_TaskWithFormulaId_GetParentTaskIdsAsyncCalled()
        {
            SetupMocksForUpdateFormulaTaskTime();
            await _unit.UpdateFormulaTaskTime(_transactionScopeMock.Object, new ProjectTask
            {
                Id = 1,
                FormulaId = 3,
                ProjectId = 5
            });

            _taskNeo4JRepositoryMock.Verify(t => t.GetParentTaskIdsAsync(1), Times.Once);
        }

        [Fact]
        public async Task UpdateFormulaTaskTime_TaskWithFormulaId_GetFormulaRootTaskIdsAsyncCalled()
        {
            SetupMocksForUpdateFormulaTaskTime();
            await _unit.UpdateFormulaTaskTime(_transactionScopeMock.Object, new ProjectTask
            {
                Id = 1,
                FormulaId = 3,
                ProjectId = 5
            });

            _taskNeo4JRepositoryMock.Verify(t => t.GetFormulaRootTaskIdsAsync(1), Times.Once);
        }

        [Fact]
        public async Task UpdateFormulaTaskTime_TaskWithFormulaId_InitTasksStartDateCalled()
        {
            SetupMocksForUpdateFormulaTaskTime();
            await _unit.UpdateFormulaTaskTime(_transactionScopeMock.Object, new ProjectTask
            {
                Id = 1,
                FormulaId = 3,
                ProjectId = 5
            });

            _startDateHelperServiceMock.Verify(t => t.InitTasksStartDate(_transactionScopeMock.Object,
                5,
                It.IsAny<ProjectStartDatesDto>(),
                It.IsAny<IEnumerable<int>>()), Times.Once);
        }

        [Fact]
        public async Task UpdateFormulaTaskTime_TaskWithFormulaId_SaveChangesAsyncCalled()
        {
            SetupMocksForUpdateFormulaTaskTime();
            await _unit.UpdateFormulaTaskTime(_transactionScopeMock.Object, new ProjectTask
            {
                Id = 1,
                FormulaId = 3,
                ProjectId = 5
            });

            _transactionScopeMock.Verify(t => t.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        private void SetupMocksForUpdateFormulaTaskTime()
        {
            _repoMock.SetupRepoMock(new List<ProjectTask>(), _transactionScopeMock);
            _startDateHelperServiceMock.Setup(t => t.InitTasksStartDate(_transactionScopeMock.Object, 5,
                It.IsAny<ProjectStartDatesDto>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(new InitStartDateResultDto());
        }

        #endregion

        #region TryResetProjectTaskTreeStatuses

        [Fact]
        public async Task ScheduleTaskJobs_ExistedDependency_IsRootAsyncCorrectCalling()
        {
            await _unit.ScheduleTaskJobs(new ProjectTask
            {
                Id = 1,
                Status = TaskStatusType.New
            }, _transactionScopeMock.Object);

            _taskNeo4JRepositoryMock.Verify(t => t.IsRootAsync(1), Times.Once);
        }

        [Fact]
        public async Task ScheduleTaskJobs_ExistedDependency_AddFormulaTaskJobsCorrectCalling()
        {
            _taskNeo4JRepositoryMock.Setup(t => t.IsRootAsync(5)).ReturnsAsync(true);
            _repoMock.SetupRepoMock(new List<ProjectTask>(), _transactionScopeMock);
            await _unit.ScheduleTaskJobs(new ProjectTask
            {
                Id = 5,
                Status = TaskStatusType.New,
                FormulaId = 21
            }, _transactionScopeMock.Object);

            _taskNeo4JRepositoryMock.Verify(t => t.GetFormulaRootTaskIdsAsync(5), Times.Once);
        }

        [Fact]
        public async Task ScheduleTaskJobs_ExistedDependency_GetNextOccurenceCorrectCalling()
        {
            _taskNeo4JRepositoryMock.Setup(t => t.IsRootAsync(5)).ReturnsAsync(true);
            await _unit.ScheduleTaskJobs(new ProjectTask
            {
                Id = 5,
                Status = TaskStatusType.New,
                RecurrenceOptionsId = 15,
                RecurrenceOptions = new RecurrenceOptions()
            }, _transactionScopeMock.Object);

            _taskScheduleServiceMock.Verify(t => t.GetNextOccurence(_transactionScopeMock.Object, 5, It.IsAny<RecurrenceOptions>()), Times.Once);
        }

        [Fact]
        public async Task ScheduleTaskJobs_ExistedDependency_ScheduleNewTaskCorrectCalling()
        {
            _taskNeo4JRepositoryMock.Setup(t => t.IsRootAsync(5)).ReturnsAsync(true);
            await _unit.ScheduleTaskJobs(new ProjectTask
            {
                Id = 5,
                Status = TaskStatusType.New
            }, _transactionScopeMock.Object);

            _taskScheduleServiceMock.Verify(t => t.ScheduleNewTask(_transactionScopeMock.Object, It.Is<ProjectTask>(pt => pt.Id == 5), false), Times.Once);
        }

        #endregion

        #region TryResetProjectTaskTreeStatuses

        [Fact]
        public async Task TryResetProjectTaskTreeStatuses_ExistedDependency_HasRelationsAsyncCorrectCalling()
        {
            await _unit.TryResetProjectTaskTreeStatuses(TaskStatusType.Completed, 2, 3);

            _taskNeo4JRepositoryMock.Verify(t => t.HasRelationsAsync(2, 3), Times.Once);
        }

        [Fact]
        public async Task TryResetProjectTaskTreeStatuses_ExistedDependency_IsGraphCompletedCorrectCalling()
        {
            await _unit.TryResetProjectTaskTreeStatuses(TaskStatusType.Completed, 2, 3);

            _taskNeo4JRepositoryMock.Verify(t => t.IsGraphCompleted(2), Times.Once);
        }

        [Fact]
        public async Task TryResetProjectTaskTreeStatuses_ExistedDependency_ResetProjectTaskTreeStatusesCorrectCalling()
        {
            _taskNeo4JRepositoryMock.Setup(t => t.IsGraphCompleted(2)).ReturnsAsync(true);
            await _unit.TryResetProjectTaskTreeStatuses(TaskStatusType.Completed, 2, 3);

            _scheduleServiceMock.Verify(t => t.ResetProjectTaskTreeStatuses(2), Times.Once);
        }

        #endregion


    }
}
