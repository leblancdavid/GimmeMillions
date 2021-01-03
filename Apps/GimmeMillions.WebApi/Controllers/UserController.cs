using GimmeMillions.Domain.Authentication;
using GimmeMillions.WebApi.Controllers.Dtos.Users;
using GimmeMillions.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GimmeMillions.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService _userService;
        private ILogger _logger;
        public UserController(IUserService userService, ILogger logger)
        {
            _userService = userService; 
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            _logger.LogDebug("GET users request");
            return Ok(_userService.GetUsers());
        }

        [HttpGet("{username}")]
        public IActionResult GetUser(string username)
        {
            _logger.LogDebug($"GET user '{username}' request");
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
            _logger.LogDebug($"Authentication request from user '{model.Username}'");
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user.IsFailure)
                return Unauthorized(new { message = user.Error });

            return Ok(user.Value.WithoutPassword());
        }

        [HttpPut("reset")]
        public IActionResult ResetPassword([FromBody]PasswordResetDto model)
        {
            _logger.LogDebug($"Password reset request from user '{model.Username}'");
            var result = _userService.UpdatePassword(model.Username, model.OldPassword, model.NewPassword);

            if (result.IsFailure)
                return BadRequest(new { message = result.Error });

            return Ok();
        }

        [HttpPut("watchlist/add")]
        public IActionResult AddToWatchlist([FromBody]UserWatchlistUpdateDto model)
        {
            _logger.LogDebug($"Adding {string.Join(',', model.Symbols)} to user '{model.Username}' watchlist");
            var result = _userService.AddToWatchlist(model.Username, model.Symbols);
            if (result.IsFailure)
                return NotFound(model.Username);

            return Ok();
        }

        [HttpPut("watchlist/remove")]
        public IActionResult RemoveFromWatchlist([FromBody]UserWatchlistUpdateDto model)
        {
            _logger.LogDebug($"Removing {string.Join(',', model.Symbols)} from user '{model.Username}' watchlist");
            _userService.RemoveFromWatchlist(model.Username, model.Symbols);
            return Ok();
        }

        [SuperuserOnlyAuth]
        [HttpPost()]
        public IActionResult AddUser([FromBody]AddUserDto model)
        {
            _logger.LogDebug($"Adding new user '{model.Username}'");
            var result = _userService.AddOrUpdateUser(new User(model.FirstName, model.LastName, model.Username, model.Password, UserRole.Default));

            if (result.IsFailure)
                return BadRequest(new { message = result.Error });

            return Created($"api/user/{result.Value.Id}", result.Value.WithoutPassword());
        }

        [SuperuserOnlyAuth]
        [HttpDelete("{username}")]
        public IActionResult DeleteUser(string username)
        {
            _logger.LogDebug($"Deleting user '{username}'");
            _userService.RemoveUser(username);

            return Ok();
        }



    }
}