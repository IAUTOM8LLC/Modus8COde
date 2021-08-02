using Microsoft.Extensions.DependencyInjection;

namespace IAutoM8.MapPositions
{
    class Program
    {
        static void Main(string[] args)
        {
            var collection = new ServiceCollection();
            Module.Configure(collection);
            var serviceProvider = collection.BuildServiceProvider();

            serviceProvider.GetService<IWorker>().Do();
        }
    }
}
