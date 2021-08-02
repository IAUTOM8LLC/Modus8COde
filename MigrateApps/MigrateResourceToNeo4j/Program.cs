using Microsoft.Extensions.DependencyInjection;
using System;

namespace MigrateResourceToNeo4j
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
