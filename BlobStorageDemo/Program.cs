using Azure.Storage.Blobs;
using System;

namespace BlobStorageDemo
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Azure Blob storage v12 - .NET quickstart sample\n");

            string connectionString = "DefaultEndpointsProtocol=https;AccountName=imagetaggingstorage;AccountKey=anknBcg55WJ2N3+EbR5NVXiYwCYQRET/y05XelX5i1vIhZEupJvZa7BURZ0XRPtv3dpF4b8GmPbrt0NmqHOM5w==;EndpointSuffix=core.windows.net";
            //Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            //Create a unique name for the container
            string containerName = "quickstartblobs" + Guid.NewGuid().ToString();

            // Create the container and return a container client object
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
            Console.ReadLine();

        }
    }
}
