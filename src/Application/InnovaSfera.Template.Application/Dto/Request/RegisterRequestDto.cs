using System.ComponentModel.DataAnnotations;

namespace InnovaSfera.Template.Application.Dto.Request;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome de usuário é obrigatório")]
    [MinLength(3, ErrorMessage = "Nome de usuário deve ter pelo menos 3 caracteres")]
    [MaxLength(50, ErrorMessage = "Nome de usuário deve ter no máximo 50 caracteres")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(8, ErrorMessage = "Senha deve ter pelo menos 8 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "Senha deve conter pelo menos: 1 letra minúscula, 1 maiúscula, 1 número e 1 caractere especial")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare("Password", ErrorMessage = "Senha e confirmação devem ser iguais")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string? FirstName { get; set; }

    [MaxLength(100, ErrorMessage = "Sobrenome deve ter no máximo 100 caracteres")]
    public string? LastName { get; set; }
}
