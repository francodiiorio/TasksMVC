using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Models;
using TareasMVC.Services;

namespace TareasMVC.Controllers
{
    [Route("api/steps")]
    public class StepsController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IUsersService usersService;

        public StepsController(ApplicationDbContext context, IUsersService usersService) 
        {
            this.context = context;
            this.usersService = usersService;
        }

        [HttpPost("{taskId:int}")]
        public async Task<ActionResult<Step>> Post(int taskId, [FromBody]PasoCreateDTO pasoCreateDTO)
        {
            var userId = usersService.GetUserId();
            var task = await context.Tareas.FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            if (task.UserCreateId != userId)
            {
                return Forbid();
            }

            var stepsExist = await context.Steps.AnyAsync(p => p.TaskId == taskId);
            var mayorOrder = 0;

            if (stepsExist)
            {
                mayorOrder = await context.Steps.Where(p => p.TaskId == taskId).Select(p => p.Order).MaxAsync();
            }

            var step = new Step();
            step.TaskId = taskId;
            step.Order = mayorOrder;
            step.Description = pasoCreateDTO.Description;
            step.Complete = pasoCreateDTO.Complete;

            context.Add(step);
            await context.SaveChangesAsync();

            return step;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] PasoCreateDTO stepCreateDTO)
        {
            var userId = usersService.GetUserId();
            var step = await context.Steps.Include(s => s.Task).FirstOrDefaultAsync(s => s.Id == id);

            if(step == null) 
            { 
                return NotFound();            
            }

            if (step.Task.UserCreateId != userId)
            {
                return Forbid();
            }

            step.Description = stepCreateDTO.Description;
            step.Complete = stepCreateDTO.Complete;

            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var userId = usersService.GetUserId();
            var step = await context.Steps.Include(s => s.Task).FirstOrDefaultAsync(t => t.Id == id);

            if (step == null)
            {
                return NotFound();
            }

            if(step.Task.UserCreateId != userId) { return Forbid(); }

            context.Remove(step);
            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("order/{taskId:int}")]
        public async Task<IActionResult> Order(int taskId, [FromBody] Guid[] ids)
        {
            var userId = usersService.GetUserId();
            var task = await context.Tareas.FirstOrDefaultAsync(t => t.Id == taskId && t.UserCreateId == userId);
            
            if(task == null)
            {
                return NotFound();
            }

            var steps = await context.Steps.Where(x => x.TaskId == taskId).ToListAsync();
            var stepsId = steps.Select(x => x.Id);
            var stepsOutOfTask = ids.Except(stepsId).ToList();

            if (stepsOutOfTask.Any()) 
            {
                return BadRequest("No todos los pasos estan presentes");
            }

            var stepsDictionary = steps.ToDictionary(s => s.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var stepId = ids[i];
                var step = stepsDictionary[stepId];
                step.Order = i + 1;
            }

            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
