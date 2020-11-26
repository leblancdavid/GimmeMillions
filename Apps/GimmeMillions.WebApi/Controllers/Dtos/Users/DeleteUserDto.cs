using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GimmeMillions.WebApi.Controllers.Dtos.Users
{
    public class DeleteUserDto : SuperuserAuthDto
    {
        [Required]
        public string Username { get; set; }
    }
}
