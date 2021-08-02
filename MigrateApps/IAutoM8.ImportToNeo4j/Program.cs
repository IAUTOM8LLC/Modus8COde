using Microsoft.Extensions.DependencyInjection;

namespace IAutoM8.ImportToNeo4j
{
    class Program
    {
        static void Main(string[] args)
        {
            var collection = new ServiceCollection();
            Module.Configure(collection);
            var serviceProvider = collection.BuildServiceProvider();

            serviceProvider.GetService<IWorker>().Do().Wait();
        }
    }
}
