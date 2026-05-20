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
    public class ParentController : ControllerBase
    {
        private readonly IService<ParentDto> _service;
        private readonly ILogger<ParentController> _logger;

        public ParentController(IService<ParentDto> service, ILogger<ParentController> logger)
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
        public async Task<IActionResult> AddItem([FromBody] ParentDto parentDto)
        {
            try
            {
                if (parentDto == null) return BadRequest("נתוני הבקשה ריקים.");
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var result = await _service.AddItem(parentDto);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ביצירת הורה חדש");
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
                var parents = await _service.GetBySchoolId(schoolId);
                return Ok(parents);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת רשימת הורים");
                return BadRequest("שגיאה בקבלת הנתונים.");
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var parent = await _service.GetById(id);
                if (parent == null) return NotFound();

                return Ok(parent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת הורה לפי מזהה {Id}", id);
                return BadRequest("שגיאה בקבלת הנתונים.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var parent = await _service.GetById(id);
                if (parent == null) return NotFound();

                await _service.DeleteItem(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה במחיקת הורה {Id}", id);
                return BadRequest("מחיקת הרשומה נכשלה.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ParentDto parent, int id)
        {
            try
            {
                var existingParent = await _service.GetById(id);
                if (existingParent == null) return NotFound();

                var newParent = await _service.UpdateItem(id, parent);
                return Ok(newParent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בעדכון הורה {Id}", id);
                return BadRequest("עדכון הרשומה נכשל.");
            }
        }
    }
}