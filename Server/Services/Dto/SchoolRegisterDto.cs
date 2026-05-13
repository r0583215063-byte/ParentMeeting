using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Dto
{
    public class SchoolRegisterDto
    {
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;


        [Required, MinLength(6), RegularExpression(@"^.{6,20}$")]
        public string Password { get; set; } = string.Empty;


    }
}
