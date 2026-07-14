using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using PickleHub.Common.Interfaces;

namespace PickleHub.Catalog.Infrastructure.Service
{
    public class CloudinaryStorageService : IStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryStorageService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"]
                ?? throw new InvalidOperationException("Cloudinary:CloudName chưa cấu hình.");

            var apiKey = configuration["Cloudinary:ApiKey"]
                ?? throw new InvalidOperationException("Cloudinary:ApiKey chưa cấu hình.");

            var apiSecret = configuration["Cloudinary:ApiSecret"]
                ?? throw new InvalidOperationException("Cloudinary:ApiSecret chưa cấu hình.");

            _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret))
            {
                Api = { Secure = true }
            };
        }

        public async Task<FileUploadResult> UploadAsync(
            Stream fileStream,
            string fileName,
            string folder,
            string resourceType = "image",
            CancellationToken ct = default)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = $"picklehub/{folder}",
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto"), // tự convert sang WebP
                UseFilename = true,
                UniqueFilename = true
            };

            var result = await _cloudinary.UploadAsync(uploadParams, ct);

            if (result.Error != null)
                throw new InvalidOperationException($"Cloudinary lỗi: {result.Error.Message}");

            return new FileUploadResult(
                PublicId: result.PublicId,
                SecureUrl: result.SecureUrl.ToString(),
                ResourceType: "image",
                Width: result.Width,
                Height: result.Height,
                SizeBytes: result.Bytes
            );
        }

        public async Task DeleteAsync(string publicId, string resourceType = "image")
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);

            // "ok"      : xóa thành công
            // "not found": file đã không tồn tại trên Cloudinary
            //              (có thể đã bị xóa thủ công hoặc xóa trước đó)
            //              => vẫn coi là thành công.
            if (result.Result == "ok" || result.Result == "not found")
            {
                return;
            }

            throw new InvalidOperationException(
                $"Cloudinary xoá thất bại: {result.Result}"
            );
        }
        //private string ExtractPublicId(string url)
        //{
        //    try
        //    {
        //        var uri = new Uri(url);
        //        var segments = uri.AbsolutePath.Split('/');
        //        var uploadIndex = Array.IndexOf(segments, "upload");
        //        if (uploadIndex < 0) return string.Empty;

        //        var afterUpload = segments.Skip(uploadIndex + 1).ToArray();

        //        if (afterUpload.Length > 0
        //            && afterUpload[0].StartsWith("v")
        //            && long.TryParse(afterUpload[0][1..], out _))
        //        {
        //            afterUpload = afterUpload.Skip(1).ToArray();
        //        }

        //        // Bỏ extension ở segment cuối
        //        afterUpload[^1] = Path.GetFileNameWithoutExtension(afterUpload[^1]);

        //        return string.Join("/", afterUpload);
        //    }
        //    catch
        //    {
        //        return string.Empty;
        //    }
        //}
    }
}
