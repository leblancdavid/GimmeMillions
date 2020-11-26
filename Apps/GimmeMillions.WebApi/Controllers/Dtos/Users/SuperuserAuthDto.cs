using System.ComponentModel.DataAnnotations;

namespace GimmeMillions.WebApi.Controllers.Dtos.Users
{
    public class SuperuserAuthDto
    {
        [Required]
        public string Superuser { get; set; }
        [Required]
        public string SuperuserPassword { get; set; }
    }
}
