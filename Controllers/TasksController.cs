using Microsoft.AspNetCore.Mvc;
using FinalProject.BLL.DTOs;
using FinalProject.BLL.Interfaces;
using FinalProject.DAL;
using TaskStatus = FinalProject.DAL.TaskStatus;

namespace FinalProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetUserTasks([FromQuery] int userId, [FromQuery] TaskStatus? status = null)
        {
            var tasks = await _taskService.GetUserTasksAsync(userId, status);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id, [FromQuery] int userId)
        {
            var task = await _taskService.GetTaskByIdAsync(id, userId);
            if (task == null)
                return NotFound();

            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            try
            {
                var task = await _taskService.CreateTaskAsync(createTaskDto);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromQuery] int userId, [FromBody] UpdateTaskDto updateTaskDto)
        {
            try
            {
                var task = await _taskService.UpdateTaskAsync(id, updateTaskDto, userId);
                return Ok(task);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(int id, [FromQuery] int userId)
        {
            var result = await _taskService.DeleteTaskAsync(id, userId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/complete")]
        public async Task<ActionResult<TaskDto>> CompleteTask(int id, [FromQuery] int userId)
        {
            try
            {
                var task = await _taskService.CompleteTaskAsync(id, userId);
                return Ok(task);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByCategory([FromQuery] int userId, int categoryId)
        {
            var tasks = await _taskService.GetTasksByCategoryAsync(userId, categoryId);
            return Ok(tasks);
        }

        [HttpGet("tag/{tagId}")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByTag([FromQuery] int userId, int tagId)
        {
            var tasks = await _taskService.GetTasksByTagAsync(userId, tagId);
            return Ok(tasks);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetOverdueTasks([FromQuery] int userId)
        {
            var tasks = await _taskService.GetOverdueTasksAsync(userId);
            return Ok(tasks);
        }
    }
}
