using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.ProjectTasks;
using IAutoM8.Service.ProjectTasks.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using IAutoM8.Tests.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAutoM8.Service.ProjectTasks.Dto;
using Xunit;

namespace IAutoM8.Service.Tests.ProjectTasks
{
    public class TaskImportServiceTests
    {

        private Mock<IScheduleService> _scheduleServiceMock;
        private Mock<IDateTimeService> _dateTimeServiceMock;
        private Mock<IProjectTaskEntityImportService> _projectTaskEntityImportServiceMock;
        private Mock<ITransactionScope> _transactionScopeMock;
        private Mock<IRepo> _repoMock;
        private ITaskImportService _taskImportService;

        public TaskImportServiceTests()
        {
            _scheduleServiceMock = new Mock<IScheduleService>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _transactionScopeMock = new Mock<ITransactionScope>();
            _repoMock = new Mock<IRepo>();
            SetupProjectTaskEntityImportServiceMock();
            _taskImportService = new TaskImportService(_scheduleServiceMock.Object,
                _dateTimeServiceMock.Object,
                _repoMock.Object,
                _projectTaskEntityImportServiceMock.Object);
        }

        private void SetupProjectTaskEntityImportServiceMock()
        {
            _projectTaskEntityImportServiceMock = new Mock<IProjectTaskEntityImportService>();
            _projectTaskEntityImportServiceMock.Setup(s => s.MapFormulaTaskAsync(It.IsAny<ITransactionScope>(),
                It.IsAny<Project>(), It.IsAny<FormulaTask>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<SkillMapDto>>(), It.IsAny<int?>(),
                It.IsAny<Func<ITransactionScope, Project, IEnumerable<FormulaTask>, DateTime, IEnumerable<int>,
                IEnumerable<SkillMapDto>, int?, (int x, int y), Task<Dictionary<int, int>>>>()))
                .Returns(Task.FromResult(new Dictionary<int, int>()));

            _projectTaskEntityImportServiceMock.Setup(s => s.MapTaskAsync(It.IsAny<ITransactionScope>(),
                It.IsAny<Project>(), It.IsAny<FormulaTask>(), It.IsAny<int?>(),
                It.IsAny<ValueTuple<int, int>>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<SkillMapDto>>()))
                .Returns(Task.FromResult(new ValueTuple<int, int>()));
        }

        #region ScheduleJobsAsync tests
        [Fact]
        public async Task ScheduleJobsAsync_CreateAutomatedJobCorrectCalling()
        {
            var task = new ProjectTask { IsAutomated = true };
            await _taskImportService.ScheduleJobsAsync(_transactionScopeMock.Object,
                new List<ProjectTask>
                {
                    task
                });
            _scheduleServiceMock.Verify(v => v.CreateAutomatedJob(_transactionScopeMock.Object, task,false), Times.Once);
        }

        [Fact]
        public async Task ScheduleJobsAsync_CreateJobBeginCorrectCalling()
        {
            var task = new ProjectTask();
            await _taskImportService.ScheduleJobsAsync(_transactionScopeMock.Object,
                new List<ProjectTask>
                {
                    task
                });
            _scheduleServiceMock.Verify(v => v.CreateJobBegin(_transactionScopeMock.Object, task, false), Times.Once);
        }
        #endregion

        #region ScheduleJobsAsync tests
        [Fact]
        public async Task ImportTasksIntoProjectAsync_FormulaTask_MapFormulaTaskAsyncCorrectCalling()
        {
            var project = new Project();
            var task = new FormulaTask
            {
                InternalFormulaProjectId = 4
            };
            var tasks = new List<FormulaTask>
            {
                task
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            var startFrom = new DateTime(2014, 4, 13);
            var outsources = new List<int>();
            await _taskImportService.ImportTasksIntoProjectAsync(
                _transactionScopeMock.Object, project, tasks, startFrom, outsources, new List<SkillMapDto>(), null, (2, 3));

            _projectTaskEntityImportServiceMock.Verify(s => s.MapFormulaTaskAsync(_transactionScopeMock.Object,
                project, task, startFrom, outsources, new List<SkillMapDto>(), null, _taskImportService.ImportTasksIntoProjectAsync), Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProjectAsync_FormulaTask_ReturnCorrectData()
        {
            var project = new Project();
            var task = new FormulaTask
            {
                InternalFormulaProjectId = 4
            };
            var tasks = new List<FormulaTask>
            {
                task
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            _projectTaskEntityImportServiceMock.Setup(s => s.MapFormulaTaskAsync(It.IsAny<ITransactionScope>(),
                It.IsAny<Project>(), It.IsAny<FormulaTask>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>(),
                    It.IsAny<IEnumerable<SkillMapDto>>(), It.IsAny<int?>(),
                It.IsAny<Func<ITransactionScope, Project, IEnumerable<FormulaTask>, DateTime, IEnumerable<int>, IEnumerable<SkillMapDto>,
                int?, (int x, int y), Task<Dictionary<int, int>>>>()))
                .Returns(Task.FromResult(new Dictionary<int, int> { [11] = 12 }));

            var result = await _taskImportService.ImportTasksIntoProjectAsync(
                _transactionScopeMock.Object, project, tasks, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, (2, 3));

            AssertEx.EqualSerialization(new Dictionary<int, int> { [11] = 12 }, result);
        }

        [Fact]
        public async Task ImportTasksIntoProjectAsync_FormulaTask_SaveChangesAsyncCorrectCalling()
        {
            var project = new Project();
            var task = new FormulaTask
            {
                InternalFormulaProjectId = 4
            };
            var tasks = new List<FormulaTask>
            {
                task
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            var result = await _taskImportService.ImportTasksIntoProjectAsync(
                _transactionScopeMock.Object, project, tasks, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, (2, 3));

            _transactionScopeMock.Verify(v => v.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProjectAsync_FormulaTask_ProjectCorrectData()
        {
            var project = new Project();
            var task = new FormulaTask
            {
                InternalFormulaProjectId = 4
            };
            var tasks = new List<FormulaTask>
            {
                task
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();
            _dateTimeServiceMock.Setup(s => s.NowUtc).Returns(new DateTime(2000, 1, 2));
            var result = await _taskImportService.ImportTasksIntoProjectAsync(
                _transactionScopeMock.Object, project, tasks, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, (2, 3));

            AssertEx.EqualSerialization(new Project
            {
                LastUpdated = new DateTime(2000, 1, 2)
            }, project);
        }

        [Fact]
        public async Task ImportTasksIntoProjectAsync_FormulaTask_MapDependencyAsyncCorrectCalling()
        {
            var project = new Project();
            var task = new FormulaTask
            {
                Id = 11,
                InternalFormulaProjectId = 4
            };
            var tasks = new List<FormulaTask>
            {
                task
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            _projectTaskEntityImportServiceMock.Setup(s => s.MapFormulaTaskAsync(It.IsAny<ITransactionScope>(),
                It.IsAny<Project>(), It.IsAny<FormulaTask>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<SkillMapDto>>(), It.IsAny<int?>(),
                It.IsAny<Func<ITransactionScope, Project, IEnumerable<FormulaTask>, DateTime, IEnumerable<int>, IEnumerable<SkillMapDto>,
                int?, (int x, int y), Task<Dictionary<int, int>>>>()))
                .Returns(Task.FromResult(new Dictionary<int, int> { [12] = 11 }));

            var result = await _taskImportService.ImportTasksIntoProjectAsync(
                _transactionScopeMock.Object, project, tasks, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, (2, 3));

            _projectTaskEntityImportServiceMock.Verify(v => v.MapDependencyAsync(_transactionScopeMock.Object,
                project, task,
                It.Is<Dictionary<int, int>>(d => d.Count == 1 && d[12] == 11)), Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProjectAsync_Task_MapTaskAsyncCorrectCalling()
        {
            var project = new Project();
            var task = new FormulaTask();
            var tasks = new List<FormulaTask>
            {
                task
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();
            var startFrom = new DateTime(2014, 4, 13);
            var outsources = new List<int>();
            var skillMappings = new List<SkillMapDto>();
            await _taskImportService.ImportTasksIntoProjectAsync(
                _transactionScopeMock.Object, project, tasks, startFrom, outsources, skillMappings, null, (2, 3));

            _projectTaskEntityImportServiceMock.Verify(s => s.MapTaskAsync(_transactionScopeMock.Object,
                project, task, null, It.Is<ValueTuple<int, int>>(t => t.Item1 == 2 && t.Item2 == 3),
                startFrom, outsources, skillMappings), Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProjectAsync_Task_ReturnCorrectData()
        {
            var project = new Project();
            var task = new FormulaTask();
            var tasks = new List<FormulaTask>
            {
                task
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            _projectTaskEntityImportServiceMock.Setup(s => s.MapTaskAsync(It.IsAny<ITransactionScope>(),
                It.IsAny<Project>(), It.IsAny<FormulaTask>(), It.IsAny<int?>(),
                It.IsAny<ValueTuple<int, int>>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<SkillMapDto>>()))
                .Returns(Task.FromResult(new ValueTuple<int, int>(11, 12)));

            var result = await _taskImportService.ImportTasksIntoProjectAsync(
                _transactionScopeMock.Object, project, tasks, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, (2, 3));

            AssertEx.EqualSerialization(new Dictionary<int, int> { [12] = 11 }, result);
        }

        [Fact]
        public async Task ImportTasksIntoProjectAsync_Task_SaveChangesAsyncCorrectCalling()
        {
            var project = new Project();
            var task = new FormulaTask();
            var tasks = new List<FormulaTask>
            {
                task
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            var result = await _taskImportService.ImportTasksIntoProjectAsync(
                _transactionScopeMock.Object, project, tasks, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, (2, 3));

            _transactionScopeMock.Verify(v => v.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProjectAsync_Task_ProjectCorrectData()
        {
            var project = new Project();
            var task = new FormulaTask();
            var tasks = new List<FormulaTask>
            {
                task
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();
            _dateTimeServiceMock.Setup(s => s.NowUtc).Returns(new DateTime(2000, 1, 2));
            var result = await _taskImportService.ImportTasksIntoProjectAsync(
                _transactionScopeMock.Object, project, tasks, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, (2, 3));

            AssertEx.EqualSerialization(new Project
            {
                LastUpdated = new DateTime(2000, 1, 2)
            }, project);
        }

        [Fact]
        public async Task ImportTasksIntoProjectAsync_Task_MapDependencyAsyncCorrectCalling()
        {
            var project = new Project();
            var task = new FormulaTask
            {
                Id = 11
            };
            var tasks = new List<FormulaTask>
            {
                task
            };
            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            _projectTaskEntityImportServiceMock.Setup(s => s.MapTaskAsync(It.IsAny<ITransactionScope>(),
                It.IsAny<Project>(), It.IsAny<FormulaTask>(),  It.IsAny<int?>(),
                It.IsAny<ValueTuple<int, int>>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<SkillMapDto>>()))
                .Returns(Task.FromResult(new ValueTuple<int, int>(11, 12)));

            var result = await _taskImportService.ImportTasksIntoProjectAsync(
                _transactionScopeMock.Object, project, tasks, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, (2, 3));

            _projectTaskEntityImportServiceMock.Verify(v => v.MapDependencyAsync(_transactionScopeMock.Object,
                project, task,
                It.Is<Dictionary<int, int>>(d => d.Count == 1 && d[12] == 11)), Times.Once);
        }

        [Fact]
        public async Task ImportTasksIntoProjectAsync_ConditionalTask_MapConditionAsyncCorrectCalling()
        {
            var project = new Project();
            var task = new FormulaTask
            {
                Id = 11,
                TaskConditionId = 100
            };
            var tasks = new List<FormulaTask>
            {
                task
            };

            var teamMaps = new Dictionary<int, IEnumerable<Guid>>();

            _projectTaskEntityImportServiceMock.Setup(s => s.MapTaskAsync(It.IsAny<ITransactionScope>(),
                It.IsAny<Project>(), It.IsAny<FormulaTask>(), It.IsAny<int?>(),
                It.IsAny<ValueTuple<int, int>>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<SkillMapDto>>()))
                .Returns(Task.FromResult(new ValueTuple<int, int>(11, 12)));

            var result = await _taskImportService.ImportTasksIntoProjectAsync(
                _transactionScopeMock.Object, project, tasks, new DateTime(), new List<int>(), new List<SkillMapDto>(), null, (2, 3));

            _projectTaskEntityImportServiceMock.Verify(v => v.MapConditionAsync(_transactionScopeMock.Object,
                project, task, It.Is<KeyValuePair<int, int>>(p => p.Key == 12 && p.Value == 11),
                It.Is<Dictionary<int, int>>(d => d.Count == 1 && d[12] == 11)), Times.Once);
        }
        #endregion
    }
}
