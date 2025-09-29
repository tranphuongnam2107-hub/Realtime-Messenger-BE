using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Models.Model;

namespace Services.Interface
{
    public interface IUploadService
    {
        Task<FileMetadata> UploadAsync(IFormFile file, string folderName);
        Task<bool> DeleteAsync(string publicId, string resourceType);
    }
}
