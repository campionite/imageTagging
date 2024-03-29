﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace QvcImageTagger.Common.Services
{
    public class AzureBlobStorageService : IStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobStorageService(AzureBlobStorageServiceOptions options)
        {
            _connectionString = "DefaultEndpointsProtocol=https;AccountName=imagetaggingstorage;AccountKey=anknBcg55WJ2N3+EbR5NVXiYwCYQRET/y05XelX5i1vIhZEupJvZa7BURZ0XRPtv3dpF4b8GmPbrt0NmqHOM5w==;EndpointSuffix=core.windows.net";
            // options.ConnectionString;
            _containerName = options.ContainerName;
        }

        public AzureBlobStorageService(IOptions<AzureBlobStorageServiceOptions> options)
        {
            _connectionString = "DefaultEndpointsProtocol=https;AccountName=imagetaggingstorage;AccountKey=anknBcg55WJ2N3+EbR5NVXiYwCYQRET/y05XelX5i1vIhZEupJvZa7BURZ0XRPtv3dpF4b8GmPbrt0NmqHOM5w==;EndpointSuffix=core.windows.net";
            //options.Value.ConnectionString;
            _containerName = options.Value.ContainerName;
        }

        public async Task UploadFileAsync(string fileName, byte[] bytes, string contentType)
        {
            var blobClient = GetClient();
            var container = await GetOrCreateContainer(blobClient);
            await CreateAndUploadBlob(fileName, bytes, contentType, container);
        }

        public async Task<IDictionary<string, string>> GetMetadataFromFileAsync(string fileName)
        {
            var blobClient = GetClient();
            var container = await GetOrCreateContainer(blobClient);
            if (!await container.ExistsAsync())
            {
                return null;
            }

            var blob = container.GetBlockBlobReference(fileName);
            if (!await blob.ExistsAsync())
            {
                return null;
            }

            await blob.FetchAttributesAsync();
            return blob.Metadata;
        }

        public async Task SetMetadataOnFileAsync(string fileName, IDictionary<string, string> metadata)
        {
            var blobClient = GetClient();
            var container = await GetOrCreateContainer(blobClient);
            if (!await container.ExistsAsync())
            {
                return;
            }

            var blob = container.GetBlockBlobReference(fileName);
            if (!await blob.ExistsAsync())
            {
                return;
            }

            await blob.FetchAttributesAsync();
            foreach (var kvp in metadata)
            {
                AddOrUpdateMetadataValue(blob, kvp);
            }

            await blob.SetMetadataAsync();
        }

        private static void AddOrUpdateMetadataValue(CloudBlob blob, KeyValuePair<string, string> kvp)
        {
            if (blob.Metadata.ContainsKey(kvp.Key))
            {
                blob.Metadata[kvp.Key] = kvp.Value;
            }
            else
            {
                blob.Metadata.Add(kvp.Key, kvp.Value);
            }
        }

        private CloudBlobClient GetClient()
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            return storageAccount.CreateCloudBlobClient();
        }

        private async Task<CloudBlobContainer> GetOrCreateContainer(CloudBlobClient blobClient)
        {
            var container = blobClient.GetContainerReference(_containerName);
            await container.CreateIfNotExistsAsync();

            var permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
            await container.SetPermissionsAsync(permissions);
            return container;
        }

        private static async Task CreateAndUploadBlob(string fileName, byte[] bytes, string contentType, CloudBlobContainer container)
        {
            var blob = container.GetBlockBlobReference(fileName);
            blob.Properties.ContentType = contentType;

            blob.Metadata.Add(
                Constants.ImageMetadataKeys.ClassificationStatus, 
                ImageClassificationStatus.Pending.ToString());

            await blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
        }
    }
}
