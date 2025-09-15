using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Services.Interface
{
    public interface IUploadService
    {
        Task<string> UploadAsync(IFormFile file, string folderName);
        Task<bool> DeleteAsync(string publicId, string resourceType);
    }
}
