using Microsoft.AspNetCore.Mvc;
using FinalProject.BLL.DTOs;
using FinalProject.BLL.Interfaces;
using FinalProject.DAL;

namespace FinalProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RemindersController : ControllerBase
    {
        private readonly IReminderService _reminderService;

        public RemindersController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReminderDto>>> GetUserReminders([FromQuery] int userId)
        {
            var reminders = await _reminderService.GetUserRemindersAsync(userId);
            return Ok(reminders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReminderDto>> GetReminder(int id, [FromQuery] int userId)
        {
            var reminder = await _reminderService.GetReminderByIdAsync(id, userId);
            if (reminder == null)
                return NotFound();

            return Ok(reminder);
        }

        [HttpPost]
        public async Task<ActionResult<ReminderDto>> CreateReminder([FromBody] CreateReminderDto createReminderDto, [FromQuery] int userId)
        {
            try
            {
                var reminder = await _reminderService.CreateReminderAsync(createReminderDto, userId);
                return CreatedAtAction(nameof(GetReminder), new { id = reminder.Id }, reminder);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ReminderDto>> UpdateReminder(int id, [FromQuery] int userId, [FromBody] UpdateReminderDto updateReminderDto)
        {
            try
            {
                var reminder = await _reminderService.UpdateReminderAsync(id, updateReminderDto, userId);
                return Ok(reminder);
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
        public async Task<ActionResult> DeleteReminder(int id, [FromQuery] int userId)
        {
            var result = await _reminderService.DeleteReminderAsync(id, userId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<ReminderDto>>> GetPendingReminders()
        {
            var reminders = await _reminderService.GetPendingRemindersAsync();
            return Ok(reminders);
        }
    }
}
