using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Scheduling;
using System.Threading.Tasks;

namespace SchoolParentMeetingSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulingController : ControllerBase
    {
        private readonly ISchedulingService _schedulingService;

        public SchedulingController(ISchedulingService schedulingService)
        {
            _schedulingService = schedulingService;
        }

        private int GetCurrentSchoolId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        [Authorize(Roles = "Admin,School")]
        [HttpPost("generate")]
        public async Task<IActionResult> Generate()
        {
            try
            {
                var schoolId = GetCurrentSchoolId();

                var result = await _schedulingService.ProcessSchedulingAsync(schoolId);

                return Ok(new
                {
                    Message = "השיבוץ הסתיים בהצלחה",
                    TotalMeetings = result.Count,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה ביצירת שיבוץ: {ex.Message}");
            }
        }
    }
}