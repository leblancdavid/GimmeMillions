using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GimmeMillions.Domain.Authentication;
using GimmeMillions.WebApi.Controllers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GimmeMillions.WebApi.Controllers
{
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
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user.IsFailure)
                return BadRequest(new { message = user.Error });

            return Ok(user.Value.WithoutPassword());
        }
    }
}