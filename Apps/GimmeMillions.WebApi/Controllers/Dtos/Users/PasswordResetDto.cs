using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GimmeMillions.WebApi.Controllers.Dtos.Users
{
    public class PasswordResetDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
