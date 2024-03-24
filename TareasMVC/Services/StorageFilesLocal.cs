using TareasMVC.Models;

namespace TareasMVC.Services
{
    public class StorageFilesLocal : IFilesStorage
    {
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor httpContextAccessor;

        public StorageFilesLocal(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            this.env = env;
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task Delete(string path, string container)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Task.CompletedTask;
            }

            var fileName = Path.GetFileName(path);
            var directoryFile = Path.Combine(env.WebRootPath, container, fileName);

            if (File.Exists(directoryFile))
            {
                File.Delete(directoryFile);
            }
            return Task.CompletedTask;
        }

        public async Task<StorageFileResult[]> Storage(string container, IEnumerable<IFormFile> files)
        {
            var tasks = files.Select(async file =>
            {
                var fileOriginalName = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                string folder = Path.Combine(env.WebRootPath, container);

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string path = Path.Combine(folder, fileName);
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    var content = ms.ToArray();
                    await File.WriteAllBytesAsync(path, content);
                }

                var url = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
                var urlFile = Path.Combine(url, container, fileName).Replace('\\', '/');
                return new StorageFileResult
                {
                    Url = urlFile,
                    Title = fileOriginalName
                };
            });

            var result = await Task.WhenAll(tasks);
            return result;
        }
    }
}
