using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Models.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Confirmação de senha não confere")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}