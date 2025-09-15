using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Config.CloudinaryConfig;
using Services.Interface;

namespace AppAPI.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IUploadService _uploadService;
        private readonly CloudinaryHelper _cloudinaryHelper;

        public FileUploadController(IUploadService uploadService, CloudinaryHelper cloudinaryHelper)
        {
            _uploadService = uploadService;
            _cloudinaryHelper = cloudinaryHelper;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles([FromForm] List<IFormFile> files, [FromQuery] string folder = "chat")
        {
            if (files == null || !files.Any())
                return BadRequest(new { message = "Please select at least a file." });

            var uploadedUrls = new List<string>();

            try
            {
                foreach (var file in files)
                {
                    var uploadedUrl = await _uploadService.UploadAsync(file, folder);
                    uploadedUrls.Add(uploadedUrl);
                }

                return Ok(new
                {
                    message = "Upload success",
                    data = uploadedUrls
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Upload fail.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFile([FromQuery] string fileUrl, [FromQuery] string resourceType = "image")
        {
            if (string.IsNullOrEmpty(fileUrl))
                return BadRequest(new { message = "FileUrl không được để trống." });

            var publicId = _cloudinaryHelper.ExtractPublicId(fileUrl);
            if (string.IsNullOrEmpty(publicId))
                return BadRequest(new { message = "Không thể lấy public_id từ URL" });

            var result = await _uploadService.DeleteAsync(publicId, resourceType);

            return result
                ? Ok(new { message = "Xóa thành công", publicId })
                : BadRequest(new { message = "Xóa thất bại" });
        }
    }
}
