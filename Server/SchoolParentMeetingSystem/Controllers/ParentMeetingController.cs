using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service.Dto;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolParentMeetingSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ParentMeetingController : ControllerBase
    {
        private readonly IService<ParentMeetingDto> _service;
        private readonly ILogger<ParentMeetingController> _logger;

        public ParentMeetingController(IService<ParentMeetingDto> service, ILogger<ParentMeetingController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int CurrentSchoolId
        {
            get
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null || !int.TryParse(claim.Value, out int schoolId))
                {
                    throw new UnauthorizedAccessException("מזהה בית ספר אינו תקין או חסר בטוקן.");
                }
                return schoolId;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] ParentMeetingDto parentMeetingDto)
        {
            try
            {
                if (parentMeetingDto == null) return BadRequest("נתוני הבקשה ריקים.");
                if (!ModelState.IsValid) return BadRequest(ModelState);

                parentMeetingDto.SchoolId = CurrentSchoolId;

                var result = await _service.AddItem(parentMeetingDto);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ביצירת פגישה חדשה");
                return BadRequest("נכשל בביצוע הפעולה.");
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                int schoolId = CurrentSchoolId;
                var meetings = await _service.GetBySchoolId(schoolId);
                return Ok(meetings);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת רשימת פגישות");
                return BadRequest("שגיאה בקבלת הנתונים.");
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var meeting = await _service.GetById(id);
                if (meeting == null) return NotFound();

                if (meeting.SchoolId != CurrentSchoolId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(meeting);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת פגישה לפי מזהה {Id}", id);
                return BadRequest("שגיאה בקבלת הנתונים.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var meeting = await _service.GetById(id);
                if (meeting == null) return NotFound();

                if (meeting.SchoolId != CurrentSchoolId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                await _service.DeleteItem(id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה במחיקת פגישה {Id}", id);
                return BadRequest("מחיקת הרשומה נכשלה.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ParentMeetingDto parentMeeting, int id)
        {
            try
            {
                var existing = await _service.GetById(id);
                if (existing == null) return NotFound();

                if (existing.SchoolId != CurrentSchoolId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                parentMeeting.SchoolId = existing.SchoolId;

                var result = await _service.UpdateItem(id, parentMeeting);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בעדכון פגישה {Id}", id);
                return BadRequest("עדכון הרשומה נכשל.");
            }
        }
    }
}