using ChatApp.Api.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public class CloudinaryService :  ICloudinaryService
{
    
    private readonly Cloudinary _cloudinary;

    public CloudinaryService()
    {
        var account = new Account(
            "dlldmku9o",
            "583133179141792",
            "oZUd17YS1_K38v9a-dFwY5BzZUo");

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        try
        {
            // using var stream = file.OpenReadStream();

            // var uploadParams = new ImageUploadParams()
            // {
            //     File = new FileDescription(file.FileName, stream),
            //     Folder = "uploads"
            // };

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset to beginning before upload

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, memoryStream),
                Folder = "uploads"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString(); // this URL saved in DB
            }
            else
            {
                throw new Exception("Image upload failed");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Invalid file", ex);
        }

    }
    public async Task<string> GetCloudinaryPublicId(string imageUrl)
    {
        try
        {
            var uri = new Uri(imageUrl);
            var parts = uri.AbsolutePath.Split('/');

            // Get everything after "upload/"
            var index = Array.IndexOf(parts, "upload");
            if (index == -1 || index + 1 >= parts.Length)
                return null;

            var publicPath = string.Join("/", parts.Skip(index + 1)); // e.g., uploads/abc123.jpg
            var publicId = Path.ChangeExtension(publicPath, null); // remove extension
            return publicId;
        }
        catch
        {
            return null;
        }
    }
    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return false;

        var publicId = GetCloudinaryPublicId(imageUrl);
        if (string.IsNullOrEmpty(publicId.ToString()))
            return false;

        var deletionParams = new DeletionParams(publicId.ToString());
        var result = await _cloudinary.DestroyAsync(deletionParams);

        return result.Result == "ok" || result.Result == "not_found"; // Treat not_found as success
    }
}
