using Microsoft.Extensions.DependencyInjection;
using System;

namespace IAutoM8.ShareSubFormulas
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
