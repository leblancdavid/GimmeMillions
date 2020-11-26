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

        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(_userService.GetUsers());
        }

        [HttpGet]
        [Route("api/[controller]/{username}")]
        public IActionResult GetUser(string username)
        {
            var user = _userService.GetUser(username);
            if (user.IsFailure)
                return NotFound(username);

            return Ok(user.Value.WithoutPassword());
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/[controller]/check/{username}")]
        public IActionResult UserExist(string username)
        {
            if (!_userService.UserExists(username))
                return NotFound(username);

            return Ok();
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
        public IActionResult ResetPassword([FromBody]PasswordResetDto model)
        {
            var result = _userService.UpdatePassword(model.Username, model.OldPassword, model.NewPassword);

            if (result.IsFailure)
                return BadRequest(new { message = result.Error });

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost()]
        public IActionResult AddUser([FromBody]AddUserDto model)
        {
            var superuserAuth = _userService.Authenticate(model.Superuser, model.SuperuserPassword);

            if (superuserAuth.IsFailure)
                return BadRequest(new { message = superuserAuth.Error });

            var result = _userService.AddOrUpdateUser(new User(model.FirstName, model.LastName, model.Username, model.Password, UserRole.Default));

            if (result.IsFailure)
                return BadRequest(new { message = result.Error });

            return Created($"api/user/{result.Value.Id}", result.Value.WithoutPassword());
        }

        [AllowAnonymous]
        [HttpDelete("")]
        public IActionResult DeleteUser([FromBody]DeleteUserDto model)
        {
            var superuserAuth = _userService.Authenticate(model.Superuser, model.SuperuserPassword);

            if (superuserAuth.IsFailure)
                return BadRequest(new { message = superuserAuth.Error });

            _userService.RemoveUser(model.Username);

            return Ok();
        }



    }
}