using GimmeMillions.Domain.Authentication;
using GimmeMillions.WebApi.Controllers.Dtos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GimmeMillions.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateDto model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user.IsFailure)
                return BadRequest(new { message = user.Error });

            return Ok(user.Value.WithoutPassword());
        }

        [AllowAnonymous]
        [HttpPut("reset")]
        public IActionResult Reset([FromBody]PasswordResetDto model)
        {
            var result = _userService.UpdatePassword(model.Username, model.OldPassword, model.NewPassword);

            if (result.IsFailure)
                return BadRequest(new { message = result.Error });

            return Ok();
        }


    }
}