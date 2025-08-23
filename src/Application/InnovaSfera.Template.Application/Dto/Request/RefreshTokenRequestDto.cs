using System.ComponentModel.DataAnnotations;

namespace InnovaSfera.Template.Application.Dto.Request;

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token é obrigatório")]
    public string RefreshToken { get; set; } = string.Empty;
}
