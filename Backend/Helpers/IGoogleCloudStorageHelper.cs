using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace BackendAPI.Helpers
{
    public interface IGoogleCloudStorageHelper
    {
        Task<String> Upload(IFormFile File, string DestinationFileName);
        Task Delete(string DestinationFileName);
    }
}
