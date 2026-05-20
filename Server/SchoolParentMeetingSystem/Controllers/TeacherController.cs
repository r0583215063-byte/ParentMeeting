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
    public class TeacherController : ControllerBase
    {
        private readonly IService<TeacherDto> _service;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(IService<TeacherDto> service, ILogger<TeacherController> logger)
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
        public async Task<IActionResult> AddItem([FromBody] TeacherDto teacherDto)
        {
            try
            {
                if (teacherDto == null) return BadRequest("נתוני הבקשה ריקים.");
                if (!ModelState.IsValid) return BadRequest(ModelState);

                teacherDto.SchoolId = CurrentSchoolId;

                var result = await _service.AddItem(teacherDto);
                if (result == null) return NotFound();

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ביצירת מורה חדש");
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
                var teachers = await _service.GetBySchoolId(schoolId);
                return Ok(teachers);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת רשימת מורים");
                return BadRequest("שגיאה בקבלת הנתונים.");
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var teacher = await _service.GetById(id);
                if (teacher == null) return NotFound();

                if (teacher.SchoolId != CurrentSchoolId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(teacher);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת מורה לפי מזהה {Id}", id);
                return BadRequest("שגיאה בקבלת הנתונים.");
            }
        }

        [Authorize(Roles = "School")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var teacher = await _service.GetById(id);
                if (teacher == null) return NotFound();

                if (teacher.SchoolId != CurrentSchoolId && !User.IsInRole("Admin"))
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
                _logger.LogError(ex, "שגיאה במחיקת מורה {Id}", id);
                return BadRequest("מחיקת הרשומה נכשלה.");
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] TeacherDto teacher, int id)
        {
            try
            {
                var existing = await _service.GetById(id);
                if (existing == null) return NotFound();

                if (existing.SchoolId != CurrentSchoolId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                teacher.SchoolId = existing.SchoolId;

                var newTeacher = await _service.UpdateItem(id, teacher);
                return Ok(newTeacher);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בעדכון מורה {Id}", id);
                return BadRequest("עדכון הרשומה נכשל.");
            }
        }
    }
}