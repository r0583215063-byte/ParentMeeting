using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; 

namespace Service.Dto
{
    public class SchoolLoginDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^.{6,20}$")]
        public string Password { get; set; } = string.Empty;

        public string? Token { get; set; }
    }
}