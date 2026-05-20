using DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Service.Dto;
using Service.Importing;
using Service.Interfaces;

namespace SchoolParentMeetingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchoolController(
        IRegister<SchoolRegisterDto, School> registerService,
        ILogin<SchoolLoginDto> loginSevice,
        IService<SchoolDto> service,
        ExcelImportService excelImportService,
        SchoolParentMeetingSystemContext context) : ControllerBase
    {
        private readonly IRegister<SchoolRegisterDto, School> _registerService = registerService;
        private readonly ILogin<SchoolLoginDto> _loginService = loginSevice;
        private readonly IService<SchoolDto> _service = service;
        private readonly ExcelImportService _excelImportService = excelImportService;
        private readonly SchoolParentMeetingSystemContext _context = context;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] SchoolRegisterDto schoolDto)
        {
            try
            {
                var name = schoolDto.Name.Trim().ToLower();

                var exists = await _context.Schools
                    .AnyAsync(s => s.Name.ToLower() == name);

                if (exists)
                    return BadRequest("Name already exists");

                if (string.IsNullOrWhiteSpace(schoolDto.Password) || schoolDto.Password.Length < 6)
                    return BadRequest("Password must be at least 6 characters");

                schoolDto.Name = name;

                var result = await _registerService.Register(schoolDto);

                return Ok(new { Message = "Success", SchoolId = result.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] SchoolLoginDto loginDto)
        {
            try
            {
                var result = await _loginService.Login(loginDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getAllSchools")]
        public async Task<IActionResult> GetAll()
        {
            var schools = await _service.GetAll();
            return Ok(schools);
        }

        [Authorize(Roles = "Admin,School")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var school = await _service.GetById(id);
            if (school == null) return NotFound();
            return Ok(school);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteItem(id);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SchoolDto school)
        {
            var updated = await _service.UpdateItem(id, school);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [Authorize]
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("לא נבחר קובץ");

            var schoolIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(schoolIdClaim))
            {
                return Unauthorized("מזהה בית הספר לא נמצא בטוקן");
            }

            int schoolId = int.Parse(schoolIdClaim);

            using var stream = file.OpenReadStream();
            await _excelImportService.ImportFromExcel(stream, schoolId);

            return Ok("הקובץ יובא בהצלחה");
        }

        [Authorize(Roles = "Admin,School")]
        [HttpPost("setup-meeting")]
        public async Task<IActionResult> SetupMeeting([FromBody] MeetingSetupDto model)
        {
            var schoolIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim)) return Unauthorized();

            var school = await _context.Schools.FindAsync(int.Parse(schoolIdClaim));
            if (school == null) return NotFound();

            school.MeetingDate = model.Date;
            school.MeetingStartTime = model.StartTime;
            school.MeetingEndTime = model.EndTime;
            school.SlotDurationMinutes = model.Duration;

            await _context.SaveChangesAsync();
            return Ok(new { message = "הנתונים עודכנו בהצלחה" });
        }
        [Authorize(Roles = "Admin,School")]
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var schoolIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim)) return Unauthorized();
            int schoolId = int.Parse(schoolIdClaim);

            var studentCount = await _context.Students
                .CountAsync(s => s.SchoolId == schoolId);

            var hasSchedule = await _context.ParentMeetings
                .AnyAsync(a => a.SchoolId == schoolId);

            return Ok(new SchoolStatusDto
            {
                StudentCount = studentCount,
                IsScheduleGenerated = hasSchedule
            });
        }
    }
}