using System.Threading.Tasks;

namespace MigrateResourceToNeo4j
{
    public interface IWorker
    {
        Task Do();
    }
}
