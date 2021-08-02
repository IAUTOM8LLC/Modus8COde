using IAutoM8.Global.Enums;
using IAutoM8.Neo4jRepository.Interfaces;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MigrateResourceToNeo4j
{
    class Worker : IWorker
    {
        private readonly CloudBlobClient _blobClient;
        private readonly IFormulaTaskNeo4jRepository _formulaTaskNeo4jRepository;
        private readonly ITaskNeo4jRepository _taskNeo4JRepository;

        public Worker(CloudBlobClient blobClient,
            IFormulaTaskNeo4jRepository formulaTaskNeo4jRepository,
            ITaskNeo4jRepository taskNeo4JRepository)
        {
            _blobClient = blobClient;
            _formulaTaskNeo4jRepository = formulaTaskNeo4jRepository;
            _taskNeo4JRepository = taskNeo4JRepository;
        }
        public async Task Do()
        {
            var blobContinuationToken = new BlobContinuationToken();
            var result = await _blobClient.GetContainerReference("formula-task")
                .ListBlobsSegmentedAsync(blobContinuationToken);
            foreach (var folder in result.Results)
            {
                var directory = (folder as CloudBlobDirectory);
                var files = await _blobClient.GetContainerReference("formula-task")
                   .ListBlobsSegmentedAsync(directory.Prefix, new BlobContinuationToken());
                var taskId = int.Parse(directory.Prefix.Replace("/", ""));
                var resources = await _formulaTaskNeo4jRepository.GetTaskResourcesAsync(taskId);
                foreach (var file in files.Results)
                {
                    var block = file as CloudBlockBlob;
                    if (resources.Any(w => w.Path == block.Name))
                    {
                        Console.WriteLine($"Resource is exist {block.Name}");
                    }
                    else
                    {
                        await _formulaTaskNeo4jRepository.AddResourceToTaskAsync(taskId,
                            new IAutoM8.Neo4jRepository.Dto.TaskResourceNeo4jDto
                            {
                                Mime = block.Properties.ContentType,
                                Name = block.Name.Replace(directory.Prefix, ""),
                                Size = (int)block.Properties.Length,
                                Path = block.Name,
                                Type = (byte)ResourceType.File
                            });
                        Console.WriteLine($"Resource added {block.Name}");
                    }
                }
            }
            blobContinuationToken = new BlobContinuationToken();
            result = await _blobClient.GetContainerReference("project-task")
                .ListBlobsSegmentedAsync(blobContinuationToken);
            foreach (var folder in result.Results)
            {
                var directory = (folder as CloudBlobDirectory);
                var files = await _blobClient.GetContainerReference("project-task")
                   .ListBlobsSegmentedAsync(directory.Prefix, new BlobContinuationToken());
                var taskId = int.Parse(directory.Prefix.Replace("/", ""));
                var resources = await _taskNeo4JRepository.GetTaskResourcesAsync(taskId);
                foreach (var file in files.Results)
                {
                    var block = file as CloudBlockBlob;
                    if (resources.Any(w => w.Path == block.Name))
                    {
                        Console.WriteLine($"Resource is exist {block.Name}");
                    }
                    else
                    {
                        await _taskNeo4JRepository.AddResourceToTaskAsync(taskId,
                            new IAutoM8.Neo4jRepository.Dto.TaskResourceNeo4jDto
                            {
                                Mime = block.Properties.ContentType,
                                Name = block.Name.Replace(directory.Prefix, ""),
                                Size = (int)block.Properties.Length,
                                Path = block.Name,
                                Type = (byte)ResourceType.File
                            });
                        Console.WriteLine($"Resource added {block.Name}");
                    }
                }
            }
        }
    }
}
