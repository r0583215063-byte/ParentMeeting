using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service.Dto;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolParentMeetingSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ParentAvailabilityController : ControllerBase
    {
        private readonly IService<ParentAvailabilityDto> _service;
        private readonly IService<ParentDto> _parentService;
        private readonly ILogger<ParentAvailabilityController> _logger; 

        public ParentAvailabilityController(
            IService<ParentAvailabilityDto> service,
            IService<ParentDto> parentService,
            ILogger<ParentAvailabilityController> logger)
        {
            _service = service;
            _parentService = parentService;
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

        [Authorize(Roles = "Admin,School")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] ParentAvailabilityDto dto)
        {
            try
            {
                if (dto == null) return BadRequest("נתוני הבקשה ריקים.");

                int schoolId = CurrentSchoolId;
                dto.SchoolId = schoolId;

                var allParents = await _parentService.GetBySchoolId(schoolId);
                var parent = allParents.FirstOrDefault(p => p.ParentIdentity == dto.ParentIdentity);

                dto.ParentId = parent?.Id; // שימוש ב-Null-conditional operator מקצר ונקי

                var result = await _service.AddItem(dto);
                return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result); // החזרה תקנית של אובייקט שנוצר
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ביצירת זמינות הורה");
                return BadRequest("נכשל בביצוע הפעולה. נא לנסות שנית מאוחר יותר.");
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                int schoolId = CurrentSchoolId;
                var list = await _service.GetBySchoolId(schoolId);
                return Ok(list);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת רשימת זמינויות");
                return BadRequest("שגיאה בקבלת הנתונים.");
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var item = await _service.GetById(id);
                if (item == null) return NotFound("הרשומה לא נמצאה.");

                if (item.SchoolId != CurrentSchoolId && !User.IsInRole("Admin"))
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
                _logger.LogError(ex, "שגיאה במחיקת רשומה {Id}", id);
                return BadRequest("מחיקת הרשומה נכשלה.");
            }
        }
    }
}