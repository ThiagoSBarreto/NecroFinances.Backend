using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NecroFinances.Application.Interfaces;
using NecroFinances.Application.Models;
using System.Security.Claims;

namespace NecroFinances.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(
            IUserService userService,
            ITokenService tokenService
            )
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDTO userDTO)
        {
            var user = await _userService.RegisterAsync(userDTO);

            if (user == null)
            {
                return Conflict("Usuário já em uso.");
            }

            return Ok(new { message = "Usuário criado com sucesso", user.Username });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDTO dto)
        {
            UserModel user = await _userService.AuthenticateAsync(dto);

            if (user == null)
            {
                return Unauthorized("Usuário ou senha inválidos.");
            }

            TokenResponse token = _tokenService.GenerateToken(user);

            return Ok(token);
        }

        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenResponse token)
        {

            return Ok(_tokenService.RefreshToken(token.RefreshToken));
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] RefreshTokenRequest request)
        {
            _tokenService.RevokeRefreshToken(request.RefreshToken);
            return NoContent();
        }

        [Authorize]
        [HttpGet("valueVisibility")]
        public IActionResult GetValueVisibility()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized("Você não tem permissão para acessar este serviço.");
            }

            var hideValues = _userService.GetHideValues(userId);
            if (!hideValues.HasValue)
            {
                return NotFound("Usuário não encontrado.");
            }

            return Ok(new { hideValues = hideValues.Value });
        }

        [Authorize]
        [HttpPut("valueVisibility")]
        public IActionResult UpdateValueVisibility([FromBody] UpdateValueVisibilityRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized("Você não tem permissão para acessar este serviço.");
            }

            if (!_userService.SetHideValues(userId, request.HideValues, request.Password))
            {
                return Unauthorized("Senha inválida.");
            }

            return Ok(new { hideValues = request.HideValues });
        }

        private bool TryGetUserId(out long userId)
        {
            return long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId);
        }
    }
}
