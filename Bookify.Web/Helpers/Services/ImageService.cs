
using Bookify.Web.Core.Consts;
using Bookify.Web.Core.Models;

namespace Bookify.Web.Helpers.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IList<string> _allowedExtensions;
        private const long _allowedSize = 2_097_152;
        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _allowedExtensions = new List<string>() { ".jpg", ".png", ".jpeg" };
        }

        public async Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string path, string imageName, bool hasThumb)
        {
            var imageExtension = System.IO.Path.GetExtension(image.FileName);

            if (!_allowedExtensions.Contains(imageExtension.ToLower()))
                return (isUploaded: false, errorMessage: Errors.NotAllowedExtension);
            if (image.Length > _allowedSize)
                return (isUploaded: false, errorMessage: Errors.SizeMustBeLessThan2MB);

            var imagePath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, path, imageName);

            using var fileStream = System.IO.File.Create(imagePath);
            await image.CopyToAsync(fileStream);
            
            if(hasThumb)
            {
                using var imageThumb = Image.Load(image.OpenReadStream());
                var height = (float)imageThumb.Height * 200 / imageThumb.Width;
                imageThumb.Mutate(x => x.Resize(width: 200, height: (int)height));

                var imageThumbPath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, path, "thumb", imageName);
                imageThumb.Save(imageThumbPath);
            }

            return (isUploaded: true, errorMessage: null);
        }

        public void Delete(string imageUrl)
        {
            var oldImagePath = $"{_webHostEnvironment.WebRootPath}{imageUrl}";

            if (System.IO.File.Exists(oldImagePath))
                System.IO.File.Delete(oldImagePath);
        }
    }
}