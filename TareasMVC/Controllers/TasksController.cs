using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Models;
using TareasMVC.Services;

namespace TareasMVC.Controllers
{
    [Route("/api/tasks")]
    public class TasksController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IUsersService usersService;
        private readonly IMapper mapper;

        public TasksController(ApplicationDbContext context, IUsersService usersService, IMapper mapper)
        {
            this.context = context;
            this.usersService = usersService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<TaskDTO>>> Get()
        {
            var idUser = usersService.GetUserId();
            var tasks = await context.Tareas.Where(t => t.UserCreateId == idUser).OrderBy(t => t.Order).ProjectTo<TaskDTO>(mapper.ConfigurationProvider).ToListAsync();

            return tasks;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tarea>> Get(int id)
        {
            var idUser = usersService.GetUserId();
            var task = await context.Tareas.Include(t => t.Steps.OrderBy(s => s.Order)).Include(t => t.FileAttachments.OrderBy(f => f.Order)).FirstOrDefaultAsync(t => t.Id == id && t.UserCreateId == idUser);

            if (task == null)
            {
                return NotFound();
            }

            return task;
        }

        [HttpPost]
        public async Task<ActionResult<Tarea>> Post([FromBody] string title)
        {
            var idUser = usersService.GetUserId();
            var tasksExist = await context.Tareas.AnyAsync(t => t.UserCreateId == idUser);
            var mayorOrder = 0;
            if(tasksExist)
            {
                mayorOrder = await context.Tareas.Where(t => t.UserCreateId == idUser).Select(t => t.Order).MaxAsync();
            }

            var task = new Tarea
            {
                Title = title,
                UserCreateId = idUser,
                Date = DateTime.UtcNow,
                Order = mayorOrder + 1
            };


            context.Add(task);
            await context.SaveChangesAsync();
            return task;
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> TaskEdit(int id, [FromBody] TaskEditDTO taskEditDTO)
        {
            var userId = usersService.GetUserId();
            var task = await context.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UserCreateId == userId);

            if(task == null)
            {
                return NotFound();
            }
            task.Title = taskEditDTO.Title;
            task.Description = taskEditDTO.Description;

            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var userId = usersService.GetUserId();
            var task = await context.Tareas.FirstOrDefaultAsync(t =>  t.Id == id && t.UserCreateId == userId);

            if(task == null)
            {
                return NotFound();
            }
            context.Remove(task);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("order")]
        public async Task<IActionResult> Order([FromBody] int[] ids)
        {
            var userId = usersService.GetUserId();
            var tasks = await context.Tareas.Where(t => t.UserCreateId == userId).ToListAsync();
            var TasksId = tasks.Select(t => t.Id);
            var TasksIdsOutOfUser = ids.Except(TasksId).ToList();

            if (TasksIdsOutOfUser.Any())
            {
                return Forbid();
            }

            var tasksDictionary = tasks.ToDictionary(t => t.Id);

            for ( int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var task = tasksDictionary[id];
                task.Order = i + 1;
            }

            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
