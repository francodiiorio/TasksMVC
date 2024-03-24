using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Models;
using TareasMVC.Services;

namespace TareasMVC.Controllers
{
    [Route("api/files")]
    public class FilesController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IFilesStorage filesStorage;
        private readonly IUsersService usersService;
        private readonly string container = "filesattachments";

        public FilesController(ApplicationDbContext context,
            IFilesStorage filesStorage,
            IUsersService usersService) 
        {
            this.context = context;
            this.filesStorage = filesStorage;
            this.usersService = usersService;
        }

        [HttpPost("{taskId:int}")]
        public async Task<ActionResult<IEnumerable<FileAttachment>>> Post(int taskId, [FromForm] IEnumerable<IFormFile> files)
        {
            var userId = usersService.GetUserId();
            var task = await context.Tareas.FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            if(task.UserCreateId != userId)
            {
                return Forbid();
            }

            var fileAttachmentExist = await context.FileAttachments.AnyAsync(f => f.TaskId == taskId);
            var mayorOrder = 0;

            if (fileAttachmentExist)
            {
                mayorOrder = await context.FileAttachments.Where(f => f.TaskId == taskId).Select(f => f.Order).MaxAsync();
            }

            var results = await filesStorage.Storage(container, files);
            var filesAttachments = results.Select((result, index) => new FileAttachment
            {
                TaskId = taskId,
                Date = DateTime.UtcNow,
                Url = result.Url,
                Title = result.Title,
                Order = mayorOrder + index + 1,
            }).ToList();

            context.AddRange(filesAttachments);
            await context.SaveChangesAsync();

            return filesAttachments.ToList();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] string title)
        {
            var userId = usersService.GetUserId();
            var file = await context.FileAttachments.Include(f => f.Task).FirstOrDefaultAsync(f => f.Id == id);

            if (file == null)
            {
                return NotFound();
            }

            if (file.Task.UserCreateId != userId)
            {
                return Forbid();
            }

            file.Title = title;
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = usersService.GetUserId();
            var file = await context.FileAttachments.Include(t => t.Task).FirstOrDefaultAsync(f => f.Id == id);

            if (file == null)
            {
                return NotFound();
            }

            if (file.Task.UserCreateId != userId) 
            {
                return Forbid();
            }

            context.Remove(file);
            await context.SaveChangesAsync();
            await filesStorage.Delete(file.Url, container);
            return Ok();
        }

        [HttpPost("order/{taskId:int}")]
        public async Task<IActionResult> Order(int taskId, [FromBody] Guid[] ids)
        {
            var userId = usersService.GetUserId();
            var task = await context.Tareas.FirstOrDefaultAsync(t => t.Id == taskId && t.UserCreateId == userId);

            if (task == null)
            {
                return NotFound();
            }

            var files = await context.FileAttachments.Where(x => x.TaskId == taskId).ToListAsync();
            var filesId = files.Select(x => x.Id);
            var filesOutOfTask = ids.Except(filesId).ToList();

            if (filesOutOfTask.Any())
            {
                return BadRequest("No todos los archivos estan presentes");
            }

            var filesDictionary = files.ToDictionary(s => s.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var fileId = ids[i];
                var file = filesDictionary[fileId];
                file.Order = i + 1;
            }

            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
