using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Global;
using IAutoM8.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.MapPositions
{
    class Worker : IWorker
    {
        private readonly IRepo _repo;

        public Worker(
            IRepo repo)
        {
            _repo = repo;
        }
        public void Do()
        {
            using (var trx = _repo.Transaction())
            {
                foreach (var group in trx.Track<ProjectTask>().Where(w => w.ParentTaskId.HasValue)
                    .GroupBy(g => g.ParentTaskId.Value)
                    .ToList())
                {
                    var minX = group.Min(m => m.PosX);
                    var minY = group.Min(m => m.PosY);
                    foreach(var task in group)
                    {
                        task.PosY = task.PosY - minY + PositionMapConstants.PaddingTop;
                        task.PosX = task.PosX - minX + PositionMapConstants.PaddingLeft;
                    }
                }
                trx.SaveAndCommit();
            }
        }
    }
}
