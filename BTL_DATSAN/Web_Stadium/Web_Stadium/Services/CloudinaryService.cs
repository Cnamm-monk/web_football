using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Web_Stadium.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var acc = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(acc);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string?> UploadAnhAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return null;

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"pitchhub/{folder}",
                Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                UseFilename = false,
                UniqueFilename = true,
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl?.ToString();
        }

        public async Task<bool> XoaAnhAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }

        public string? LayPublicId(string? url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath;
                var uploadIndex = path.IndexOf("/upload/");
                if (uploadIndex < 0) return null;
                var afterUpload = path[(uploadIndex + 8)..];
                // Bỏ version nếu có (v1234567/)
                if (afterUpload.StartsWith("v") && afterUpload.Contains("/"))
                    afterUpload = afterUpload[(afterUpload.IndexOf('/') + 1)..];
                // Bỏ extension
                var dotIndex = afterUpload.LastIndexOf('.');
                return dotIndex > 0 ? afterUpload[..dotIndex] : afterUpload;
            }
            catch
            {
                return null;
            }
        }
    }
}
