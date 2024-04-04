namespace Bookify.Web.Helpers.Services
{
    public interface IImageService
    {
        public Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string path, string imageName,bool hasThumb);
        public void Delete(string imageUrl);
    }
}
