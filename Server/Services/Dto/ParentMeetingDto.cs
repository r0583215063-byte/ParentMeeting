using System.ComponentModel.DataAnnotations;

namespace Service.Dto
{
    public class ParentMeetingDto
    {
        public int Id { get; set; }
        [Required]
        public int StudentId { get; set; }
        public string StudentName { get; set; } 

        [Required]
        public int ParentId { get; set; }
        public string ParentName { get; set; }
        // שדות חדשים שהוספנו
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }

        [Required, StringLength(5, MinimumLength = 1)]
        public string ClassName { get; set; }

        [Required] // MaxLength לא רלוונטי ל-int, השתמש ב-Required
        public int SchoolId { get; set; }

        [Required]
        public DateTime MeetingDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsPast { get; set; } = false;
    }
}