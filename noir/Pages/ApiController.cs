using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace noir.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ApiController : ControllerBase
	{
		// Загрузка аватарки (GIF и видео до 3 сек)
		[HttpPost("upload-avatar")]
		public async Task<IActionResult> UploadAvatar(IFormFile file, string username)
		{
			try
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

				var extension = isVideo ? ".mp4" : (isGif ? ".gif" : ".jpg");
				var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatar");
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

				// Удаляем старые файлы пользователя
				foreach (var ext in new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4" })
				{
					var oldFile = Path.Combine(dir, username + ext);
					if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
				}

				var filePath = Path.Combine(dir, username + extension);
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				return Ok(new { success = true, fileType = isVideo ? "video" : "image", url = $"/images/avatar/{username}{extension}" });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, error = ex.Message });
			}
		}

		// Загрузка баннера (только изображения)
		[HttpPost("upload-banner")]
		public async Task<IActionResult> UploadBanner(IFormFile file, string username)
		{
			try
			{
				if (file == null || file.Length == 0)
					return Ok(new { success = false, error = "No file selected" });

				if (!file.ContentType.StartsWith("image/"))
					return Ok(new { success = false, error = "Only images allowed (JPG, PNG, WEBP)" });

				if (file.Length > 8 * 1024 * 1024)
					return Ok(new { success = false, error = "File too large. Max 8MB" });

				var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "banner");
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

				var filePath = Path.Combine(dir, username + "_banner.jpg");
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				return Ok(new { success = true, url = $"/images/banner/{username}_banner.jpg" });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, error = ex.Message });
			}
		}

		// Загрузка фото товара
		[HttpPost("upload-photo")]
		public async Task<IActionResult> UploadPhoto(IFormFile file, int listingId)
		{
			try
			{
				if (file == null || file.Length == 0)
					return Ok(new { success = false, error = "No file selected" });

				if (!file.ContentType.StartsWith("image/"))
					return Ok(new { success = false, error = "Only images allowed" });

				if (file.Length > 10 * 1024 * 1024)
					return Ok(new { success = false, error = "File too large. Max 10MB" });

				var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "photo", listingId.ToString());
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

				var fileName = Guid.NewGuid().ToString("N") + ".jpg";
				var filePath = Path.Combine(dir, fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				return Ok(new { success = true, url = $"/images/photo/{listingId}/{fileName}" });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, error = ex.Message });
			}
		}
	}
}