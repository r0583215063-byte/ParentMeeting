using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.Entities;
using Service.Dto;
using Service.Importing;
using Service.Interfaces;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolParentMeetingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchoolController : ControllerBase
    {
        private readonly IRegister<SchoolRegisterDto, School> _registerService;
        private readonly ILogin<SchoolLoginDto> _loginService;
        private readonly IService<SchoolDto> _service;
        private readonly ExcelImportService _excelImportService;
        private readonly ILogger<SchoolController> _logger;

        public SchoolController(
            IRegister<SchoolRegisterDto, School> registerService,
            ILogin<SchoolLoginDto> loginSevice,
            IService<SchoolDto> service,
            ExcelImportService excelImportService,
            ILogger<SchoolController> logger)
        {
            _registerService = registerService;
            _loginService = loginSevice;
            _service = service;
            _excelImportService = excelImportService;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] SchoolRegisterDto schoolDto)
        {
            try
            {
                if (schoolDto == null) return BadRequest("נתוני הבקשה ריקים.");

                var result = await _registerService.Register(schoolDto);
                return Ok(new { Message = "Success", SchoolId = result.Id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ברישום בית ספר חדש");
                return BadRequest("רישום בית הספר נכשל.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] SchoolLoginDto loginDto)
        {
            try
            {
                if (loginDto == null) return BadRequest("נתוני הבקשה ריקים.");
                var result = await _loginService.Login(loginDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בתהליך התחברות בית ספר");
                return BadRequest("ההתחברות נכשלה.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getAllSchools")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var schools = await _service.GetAll();
                return Ok(schools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת כל בתי הספר");
                return BadRequest("שגיאה בקבלת הנתונים.");
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                if (id != CurrentSchoolId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var school = await _service.GetById(id);
                if (school == null) return NotFound();
                return Ok(school);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת נתוני בית ספר {Id}", id);
                return BadRequest("שגיאה בקבלת הנתונים.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var school = await _service.GetById(id);
                if (school == null) return NotFound();

                await _service.DeleteItem(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה במחיקת בית ספר {Id}", id);
                return BadRequest("מחיקת הרשומה נכשלה.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SchoolDto school)
        {
            try
            {
                var updated = await _service.UpdateItem(id, school);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בעדכון בית ספר {Id}", id);
                return BadRequest("עדכון הרשומה נכשל.");
            }
        }


        [Authorize]
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel()
        {
            try
            {
                if (!Request.HasFormContentType)
                {
                    return BadRequest("בקשה לא תקינה, צפוי פורמט Content-Type של Form.");
                }

                var form = await Request.ReadFormAsync();
                var file = form.Files.GetFile("file");

                if (file == null || file.Length == 0)
                {
                    return BadRequest("לא נבחר קובץ תקני או שהקובץ ריק.");
                }

                int schoolId = CurrentSchoolId;
                using var stream = file.OpenReadStream();

                await _excelImportService.ImportFromExcel(stream, schoolId);

                return Ok(new { message = "הקובץ יובא בהצלחה" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בייבוא קובץ אקסל עבור בית ספר");
                return BadRequest($"כישלון בעיבוד הקובץ: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin,School")]
        [HttpPost("setup-meeting")]
        public async Task<IActionResult> SetupMeeting([FromBody] MeetingSetupDto model)
        {
            try
            {
                if (model == null) return BadRequest("נתוני ההגדרה ריקים.");
                int schoolId = CurrentSchoolId;

                if (_service is SchoolService schoolService)
                {
                    await schoolService.SetupMeeting(schoolId, model);
                }
                else
                {
                    return BadRequest("שירות עדכון בית הספר אינו זמין.");
                }

                return Ok(new { message = "הנתונים עודכנו בהצלחה" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בעדכון הגדרות יום הורים");
                return BadRequest("עדכון ההגדרות נכשל.");
            }
        }
        [Authorize(Roles = "Admin,School")]
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                int schoolId = CurrentSchoolId;

                var schools = await _service.GetBySchoolId(schoolId);
                if (schools == null || schools.Count == 0) return NotFound("בית הספר לא נמצא.");

                if (_service is SchoolService schoolService)
                {
                    var statusInfo = await schoolService.GetSchoolStatusAsync(schoolId);

                    return Ok(new
                    {
                        studentCount = statusInfo.StudentCount,
                        isScheduleGenerated = statusInfo.IsScheduleGenerated
                    });
                }

                return BadRequest("שירות עדכון סטטוס בית הספר אינו זמין.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליפת סטטוס בית ספר עבור מזהה {SchoolId}", CurrentSchoolId);
                return BadRequest("שגיאה בקבלת הנתונים מהשרת.");
            }
        }
    }
}