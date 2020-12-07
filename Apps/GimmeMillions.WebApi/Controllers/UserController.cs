using GimmeMillions.Domain.Authentication;
using GimmeMillions.WebApi.Controllers.Dtos.Users;
using GimmeMillions.WebApi.Services;
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

        [HttpGet("{username}")]
        public IActionResult GetUser(string username)
        {
            var user = _userService.GetUser(username);
            if (user.IsFailure)
                return NotFound(username);

            return Ok(user.Value.WithoutPassword());
        }

        [HttpGet("{username}/check")]
        [AllowAnonymous]
        public IActionResult UserExist(string username)
        {
            return Ok(_userService.UserExists(username));
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateDto model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user.IsFailure)
                return Unauthorized(new { message = user.Error });

            return Ok(user.Value.WithoutPassword());
        }

        [HttpPut("reset")]
        public IActionResult ResetPassword([FromBody]PasswordResetDto model)
        {
            var result = _userService.UpdatePassword(model.Username, model.OldPassword, model.NewPassword);

            if (result.IsFailure)
                return BadRequest(new { message = result.Error });

            return Ok();
        }

        [HttpPut("watchlist/add")]
        public IActionResult AddToWatchlist([FromBody]UserWatchlistUpdateDto model)
        {
            var result = _userService.AddToWatchlist(model.Username, model.Symbols);
            if (result.IsFailure)
                return NotFound(model.Username);

            return Ok();
        }

        [HttpPut("watchlist/remove")]
        public IActionResult RemoveFromWatchlist([FromBody]UserWatchlistUpdateDto model)
        {
            _userService.RemoveFromWatchlist(model.Username, model.Symbols);
            return Ok();
        }

        [SuperuserOnlyAuth]
        [HttpPost()]
        public IActionResult AddUser([FromBody]AddUserDto model)
        {
            var result = _userService.AddOrUpdateUser(new User(model.FirstName, model.LastName, model.Username, model.Password, UserRole.Default));

            if (result.IsFailure)
                return BadRequest(new { message = result.Error });

            return Created($"api/user/{result.Value.Id}", result.Value.WithoutPassword());
        }

        [SuperuserOnlyAuth]
        [HttpDelete("{username}")]
        public IActionResult DeleteUser(string username)
        {
            _userService.RemoveUser(username);

            return Ok();
        }



    }
}