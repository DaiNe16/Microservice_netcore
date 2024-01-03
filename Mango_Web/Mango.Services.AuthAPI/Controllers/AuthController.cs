using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Mango.Services.CouponAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Mango.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private ResponseDto _response;
        private readonly AppDbContext _db;
        public AuthController(IAuthService authService, AppDbContext appDbContext)
        {
            _authService = authService;
            _response = new ResponseDto();
            _db = appDbContext;
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            var loginResponse = await _authService.Login(loginRequestDto);
            if(loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Username or password is incorrect";
                return BadRequest(_response);
            }

            _response.Result = loginResponse;
            return Ok(_response);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegistrationRequestDto registrationRequestDto)
        {
            var registrationResponse = await _authService.Register(registrationRequestDto);
            if (!string.IsNullOrEmpty(registrationResponse))
            {
                _response.IsSuccess = false;
                _response.Message = registrationResponse;
                return BadRequest(_response);
            }

            _response.Result = "Account has been registered.";
            return Ok(_response);
        }

        [HttpPost("AssignRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(string email, string roleName)
        {
            var response = await _authService.AssignRole(email, roleName);
            if(!response)
            {
                _response.IsSuccess = false;
                _response.Message = "Failed to assign role.";
                return BadRequest(_response);
            }
            _response.Message = "Successfully to assign role.";
            return Ok(_response);
        }

        [HttpGet("GetMe")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            UserDto userDto = new UserDto();
            // Access claims from ClaimsPrincipal
            //var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            //var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var username = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;

            var user_DB = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == username);
            if(user_DB == null)
            {
                return NotFound(userDto);
            }
            userDto.ID = user_DB.Id;
            userDto.Name = user_DB.Name;
            userDto.Email = user_DB.Name;
            userDto.PhoneNumber = user_DB.PhoneNumber;


            // Access roles from ClaimsPrincipal
            //var roles = User.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList();

            return Ok(userDto);
        }
    }
}
