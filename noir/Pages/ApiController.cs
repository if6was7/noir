using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace noir.Controllers  // или noir.Pages, если в Pages создаёшь
{
	[ApiController]
	public class ApiController : ControllerBase
	{
		// Загрузка аватарки (поддерживает GIF и видео до 3 сек)
		[HttpPost("/api/upload-avatar")]
		public async Task<IActionResult> UploadAvatar(IFormFile file, string username)
		{
			if (file == null || file.Length == 0)
				return Ok(new { success = false, error = "No file selected" });

			var isVideo = file.ContentType.StartsWith("video/");
			var isGif = file.ContentType == "image/gif";
			var isImage = file.ContentType.StartsWith("image/");

			if (!isVideo && !isGif && !isImage)
				return Ok(new { success = false, error = "Invalid file type. Use JPG, PNG, GIF, WEBP, MP4" });

			if (file.Length > 10 * 1024 * 1024)
				return Ok(new { success = false, error = "File too large. Max 10MB" });

			// Сохраняем файл
			var extension = isVideo ? ".mp4" : (isGif ? ".gif" : ".jpg");
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatar", username + extension);

			var dir = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			return Ok(new { success = true, fileType = isVideo ? "video" : "image" });
		}

		// Загрузка баннера (только изображения)
		[HttpPost("/api/upload-banner")]
		public async Task<IActionResult> UploadBanner(IFormFile file, string username)
		{
			if (file == null || file.Length == 0)
				return Ok(new { success = false, error = "No file selected" });

			if (!file.ContentType.StartsWith("image/"))
				return Ok(new { success = false, error = "Only images allowed (JPG, PNG, WEBP)" });

			if (file.Length > 8 * 1024 * 1024)
				return Ok(new { success = false, error = "File too large. Max 8MB" });

			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatar", username + "_banner.jpg");

			var dir = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			return Ok(new { success = true });
		}
	}
}