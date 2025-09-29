using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Models.Model;
using Services.Config.CloudinaryConfig;
using Services.Interface;
using ResourceType = CloudinaryDotNet.Actions.ResourceType;

namespace Services.Implement
{
    public class UploadService : IUploadService
    {
        private readonly Cloudinary _cloudinary;

        public UploadService(CloudinaryConnection cloudinary)
        {
            _cloudinary = cloudinary.GetClient();
        }

        public async Task<bool> DeleteAsync(string publicId, string resourceType)
        {
            if (string.IsNullOrEmpty(publicId))
                throw new ArgumentException("PublicId không hợp lệ.");

            ResourceType type = resourceType.ToLower() switch
            {
                "image" => ResourceType.Image,
                "video" => ResourceType.Video,
                "raw" => ResourceType.Raw,
                _ => throw new ArgumentException("Loại tài nguyên không hợp lệ.")
            };

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = type
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
        }

        public async Task<FileMetadata> UploadAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is invalid.");

            await using var stream = file.OpenReadStream();

            if (file.ContentType.StartsWith("image/"))
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folderName,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    return new FileMetadata
                    {
                        FileName = file.FileName,
                        FileUrl = uploadResult.SecureUrl.ToString(),
                        Size = file.Length,
                        ContentType = file.ContentType
                    };

                throw new Exception("Upload fail: " + uploadResult.Error?.Message);
            }
            else
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folderName
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    return new FileMetadata
                    {
                        FileName = file.FileName,
                        FileUrl = uploadResult.SecureUrl.ToString(),
                        Size = file.Length,
                        ContentType = file.ContentType ?? "application/octet-stream"
                    };

                throw new Exception("Upload file fail: " + uploadResult.Error?.Message);
            }
        }

        
    }
}
