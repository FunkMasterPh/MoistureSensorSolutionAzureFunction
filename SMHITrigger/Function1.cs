using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using System.Net.Http;
using System.Text;
using Azure.Storage.Blobs.Models;

namespace SMHITrigger
{
    public static class SMHITrigger
    {
        private const string url = "https://opendata-download-metobs.smhi.se/api/version/1.0/parameter/17/station/98210/period/latest-months/data.json";
        private static HttpClient client = new HttpClient();
        [FunctionName("SMHITrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");
            string smhiObjects = "";
            try
            {
                var response = await client.GetAsync(url);
                smhiObjects = await response.Content.ReadAsStringAsync();  
            }
            catch
            {
                return new BadRequestObjectResult("Failed to access SMHI API.");
            }
            try
            {
                MemoryStream myBlob = new MemoryStream(Encoding.UTF8.GetBytes(smhiObjects));
                var blobClient = new BlobContainerClient(Connection, containerName);
                BlobClient blob = blobClient.GetBlobClient("SMHIData");
                blob.DeleteIfExists();
                await blob.UploadAsync(myBlob);
                return new OkObjectResult("Blob uploaded successfully.");
            }
            catch
            {
                return new BadRequestObjectResult("Couldn´t access Blob Storage.");
            }
        }
    }
}
