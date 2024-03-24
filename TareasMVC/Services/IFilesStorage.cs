using TareasMVC.Models;

namespace TareasMVC.Services
{
    public interface IFilesStorage
    {
        Task Delete(string path, string container);
        Task<StorageFileResult[]> Storage(string container, IEnumerable<IFormFile> files);
    }
}
