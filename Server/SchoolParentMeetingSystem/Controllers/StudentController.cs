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
    public class StudentController : ControllerBase
    {
        private readonly IService<StudentDto> _service;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IService<StudentDto> service, ILogger<StudentController> logger)
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
        public async Task<IActionResult> AddItem([FromBody] StudentDto studentDto)
        {
            try
            {
                if (studentDto == null) return BadRequest("נתוני הבקשה ריקים.");
                if (!ModelState.IsValid) return BadRequest(ModelState);

                studentDto.SchoolId = CurrentSchoolId;

                var result = await _service.AddItem(studentDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ביצירת תלמיד חדש");
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
                var students = await _service.GetBySchoolId(schoolId);
                return Ok(students);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת רשימת תלמידים");
                return BadRequest("שגיאה בקבלת הנתונים.");
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var student = await _service.GetById(id);
                if (student == null) return NotFound();

                if (student.SchoolId != CurrentSchoolId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(student);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת תלמיד לפי מזהה {Id}", id);
                return BadRequest("שגיאה בקבלת הנתונים.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var student = await _service.GetById(id);
                if (student == null) return NotFound();

                if (student.SchoolId != CurrentSchoolId && !User.IsInRole("Admin"))
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
                _logger.LogError(ex, "שגיאה במחיקת תלמיד {Id}", id);
                return BadRequest("מחיקת הרשומה נכשלה.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] StudentDto student, int id)
        {
            try
            {
                var existing = await _service.GetById(id);
                if (existing == null) return NotFound();

                if (existing.SchoolId != CurrentSchoolId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                student.SchoolId = existing.SchoolId;

                var result = await _service.UpdateItem(id, student);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בעדכון תלמיד {Id}", id);
                return BadRequest("עדכון הרשומה נכשל.");
            }
        }
    }
}