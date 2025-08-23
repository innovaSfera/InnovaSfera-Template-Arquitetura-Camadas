using InnovaSfera.Template.Application.Dto.Request;
using InnovaSfera.Template.Application.Dto.Response;
using InnovaSfera.Template.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InnovaSfera.Template.Presentation.Api.Controllers;

/// <summary>
/// Controller responsável pela autenticação e autorização de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authAppService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthAppService authAppService, ILogger<AuthController> logger)
    {
        _authAppService = authAppService;
        _logger = logger;
    }

    /// <summary>
    /// Realiza login do usuário
    /// </summary>
    /// <param name="request">Dados de login</param>
    /// <returns>Token de acesso e informações do usuário</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = GetIpAddress();
            var result = await _authAppService.LoginAsync(request, ipAddress);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            // Definir refresh token como cookie httpOnly (opcional, para maior segurança)
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenCookie(result.RefreshToken);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new AuthResponseDto 
            { 
                Success = false, 
                Message = "Erro interno do servidor" 
            });
        }
    }

    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    /// <param name="request">Dados de registro</param>
    /// <returns>Informações do usuário criado</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authAppService.RegisterAsync(request);

            if (!result.Success)
            {
                return Conflict(result);
            }

            return CreatedAtAction(nameof(GetProfile), new { }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new AuthResponseDto 
            { 
                Success = false, 
                Message = "Erro interno do servidor" 
            });
        }
    }

    /// <summary>
    /// Atualiza o token de acesso usando refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Novo token de acesso</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto? request = null)
    {
        try
        {
            // Tentar obter refresh token do body ou do cookie
            var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new AuthResponseDto 
                { 
                    Success = false, 
                    Message = "Refresh token não fornecido" 
                });
            }

            var ipAddress = GetIpAddress();
            var refreshRequest = new RefreshTokenRequestDto { RefreshToken = refreshToken };
            var result = await _authAppService.RefreshTokenAsync(refreshRequest, ipAddress);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            // Atualizar refresh token no cookie
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenCookie(result.RefreshToken);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new AuthResponseDto 
            { 
                Success = false, 
                Message = "Erro interno do servidor" 
            });
        }
    }

    /// <summary>
    /// Revoga o refresh token (logout)
    /// </summary>
    /// <param name="request">Refresh token para revogar</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("revoke-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> RevokeToken([FromBody] RefreshTokenRequestDto? request = null)
    {
        try
        {
            // Tentar obter refresh token do body ou do cookie
            var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new AuthResponseDto 
                { 
                    Success = false, 
                    Message = "Refresh token não fornecido" 
                });
            }

            var ipAddress = GetIpAddress();
            var revokeRequest = new RefreshTokenRequestDto { RefreshToken = refreshToken };
            var result = await _authAppService.RevokeTokenAsync(revokeRequest, ipAddress);

            // Remover refresh token do cookie
            Response.Cookies.Delete("refreshToken");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token revocation");
            return StatusCode(500, new AuthResponseDto 
            { 
                Success = false, 
                Message = "Erro interno do servidor" 
            });
        }
    }

    /// <summary>
    /// Obtém o perfil do usuário autenticado
    /// </summary>
    /// <returns>Informações do usuário</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetProfile()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = await _authAppService.GetUserProfileAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Endpoint protegido para teste de autorização
    /// </summary>
    /// <returns>Mensagem de sucesso</returns>
    [HttpGet("protected")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<object> Protected()
    {
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            Message = "Você está autenticado!",
            UserName = userName,
            Email = email,
            Roles = roles,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Endpoint protegido apenas para administradores
    /// </summary>
    /// <returns>Mensagem de sucesso</returns>
    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<object> AdminOnly()
    {
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        
        return Ok(new
        {
            Message = "Você é um administrador!",
            UserName = userName,
            Timestamp = DateTime.UtcNow
        });
    }

    #region Private Methods

    private string? GetIpAddress()
    {
        return Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    #endregion
}
