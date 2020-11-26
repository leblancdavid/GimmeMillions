using System.ComponentModel.DataAnnotations;

namespace GimmeMillions.WebApi.Controllers.Dtos.Users
{
    public class AuthenticateDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
