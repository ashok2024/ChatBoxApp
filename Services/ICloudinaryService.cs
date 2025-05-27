using ChatApp.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Api.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<string> GetCloudinaryPublicId(string imageUrl);
        Task<bool> DeleteImageAsync(string imageUrl);

    }
}