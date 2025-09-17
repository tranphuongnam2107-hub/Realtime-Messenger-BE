using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Config.CloudinaryConfig
{
    public class CloudinaryHelper
    {
        public string ExtractPublicId(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return string.Empty;

            var uri = new Uri(fileUrl);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Tìm vị trí của "upload"
            int indexOfUpload = Array.IndexOf(segments, "upload");
            if (indexOfUpload == -1 || indexOfUpload + 1 >= segments.Length)
                return string.Empty;

            // Bỏ phần "upload" và "v{version}"
            var parts = segments.Skip(indexOfUpload + 1).ToList();

            // Nếu phần tử đầu có dạng v123456789 => bỏ đi
            if (parts[0].StartsWith("v") && long.TryParse(parts[0].Substring(1), out _))
            {
                parts.RemoveAt(0);
            }

            // Bỏ đuôi file (.jpg, .png, .pdf, ...)
            var fileName = Path.GetFileNameWithoutExtension(parts.Last());
            parts[parts.Count - 1] = fileName;

            return string.Join("/", parts);
        }
    }
}
