using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BackendAPI.Helpers
{
    public class GoogleCloudStorageHelper : IGoogleCloudStorageHelper
    {
        private readonly GoogleCredential _googleCredential;
        private readonly StorageClient _storageClient;
        private readonly string bucketName;
        private readonly IConfiguration _configuration;

        public GoogleCloudStorageHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            Dictionary<string, object> settings = _configuration.GetSection("Google:ServiceKey").Get<Dictionary<string, object>>();
            string ServiceAccountConfiguration = JsonConvert.SerializeObject(settings);
            this.bucketName = _configuration["Google:BucketName"];
            _googleCredential = GoogleCredential.FromJson(ServiceAccountConfiguration);
            _storageClient = StorageClient.Create(_googleCredential);
        }

        public async Task Delete(string DestinationFileName)
        {
            await _storageClient.DeleteObjectAsync(bucketName, DestinationFileName);
        }

        public async Task<string> Upload(IFormFile File, string DestinationFileName)
        {
            Google.Apis.Storage.v1.Data.Object dataObject = await _storageClient.UploadObjectAsync(bucketName, DestinationFileName, File.ContentType, File.OpenReadStream());
            return dataObject.MediaLink;
        }
    }
}
