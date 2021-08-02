using System.Threading.Tasks;

namespace IAutoM8.ImportToNeo4j
{
    public interface IWorker
    {
        Task Do();
    }
}
