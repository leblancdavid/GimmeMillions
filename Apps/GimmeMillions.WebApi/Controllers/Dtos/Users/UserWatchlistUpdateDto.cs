using System.ComponentModel.DataAnnotations;

namespace GimmeMillions.WebApi.Controllers.Dtos.Users
{
    public class UserWatchlistUpdateDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string[] Symbols { get; set; }
    }
}
