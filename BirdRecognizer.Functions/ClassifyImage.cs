using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using QvcImageTagger.Common;
using QvcImageTagger.Common.Services;

namespace QvcImageTagger.Functions
{
    public static class ClassifyImage
    {
        [FunctionName("ClassifyImage")]
        public static async Task Run([BlobTrigger("clothes/{name}", 
                                     Connection = "AzureWebJobsStorage")]Stream blob, 
                                     string name)
        {
            var storageService = InstantiateStorageService();
            if (await AlreadyProcessed(storageService, name))
            {
                return;
            }

            var response = await GetPredictionResponse(blob);
            await ApplyPredictionToBlob(storageService, name, response);
        }

        private static AzureBlobStorageService InstantiateStorageService()
        {
            var storageService = new AzureBlobStorageService(
                new AzureBlobStorageServiceOptions
                    {
                    //ConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage"),
                    //ContainerName = Environment.GetEnvironmentVariable("ContainerName")
                    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=imagetaggingstorage;AccountKey=anknBcg55WJ2N3+EbR5NVXiYwCYQRET/y05XelX5i1vIhZEupJvZa7BURZ0XRPtv3dpF4b8GmPbrt0NmqHOM5w==;EndpointSuffix=core.windows.net",
                    ContainerName = "clothes"
                        
                });
            return storageService;
        }

        private static async Task<bool> AlreadyProcessed(IStorageService storageService, string name)
        {
            var metadata = await storageService.GetMetadataFromFileAsync(name);
            return metadata[Constants.ImageMetadataKeys.ClassificationStatus] 
                != ImageClassificationStatus.Pending.ToString();
        }


        private static async Task<PredictionResponse> GetPredictionResponse(Stream blob)
        {
            var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("Prediction-Key", 
            //    Environment.GetEnvironmentVariable("PredictionKey"));

            client.DefaultRequestHeaders.Add("Prediction-Key",
                "af5a9a9e832e4ec4b964bea4bfc70a25");


            var url = "https://eastus.api.cognitive.microsoft.com/customvision/v3.0/Prediction/83e95659-edce-42ee-879e-8d2659bb870d/classify/iterations/Iteration1/image";
            //Environment.GetEnvironmentVariable("PredictionUrl");

            var byteData = GetImageAsByteArray(blob);
            //byte[] byteData = GetImageAsByteArrayLocal(@"D:\Hackathon\images\test\hello.jpg");


            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");
                //content.Headers.ContentType =
                //    new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PredictionResponse>(responseString);
            }
        }

        private static byte[] GetImageAsByteArray(Stream blob)
        {
            var binaryReader = new BinaryReader(blob);
            return binaryReader.ReadBytes((int)blob.Length);
        }

        private static byte[] GetImageAsByteArrayLocal(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }
        private static async Task ApplyPredictionToBlob(IStorageService storageService, 
                                                        string name, 
                                                        PredictionResponse response)
        {
            var metadata = CreateMetadata(response);
            await WriteMetadataToFile(storageService, name, metadata);
        }

        private static IDictionary<string, string> CreateMetadata(PredictionResponse response)
        {
            return new Dictionary<string, string>
                {
                    {
                        Constants.ImageMetadataKeys.ClassificationStatus,
                        ImageClassificationStatus.Completed.ToString()
                    },
                    {
                        Constants.ImageMetadataKeys.PredictionDetail,
                        GetMetaDataFromPrediction(response)
                    }
                };
        }

        private static string GetMetaDataFromPrediction(PredictionResponse response)
        {
            var predictionDetail = response.Predictions
                .Select(x => new { tag = x.TagName, probability = Math.Round(x.Probability, 4) })
                .ToArray();
            return JsonConvert.SerializeObject(predictionDetail);
        }

        private static async Task WriteMetadataToFile(IStorageService storageService, string name, IDictionary<string, string> metadata)
        {
            await storageService.SetMetadataOnFileAsync(name, metadata);
        }
    }
}
